using UnityEngine;
using UnityEngine.UI;
public class CarController : MonoBehaviour
{
    [Header("Car Setup")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;
    [SerializeField]
    public Transform centerOfMassObject;

    [Header("Power Distribution")]
    [Range(0f, 1f)]
    public float frontPowerBias = 0.5f; // 0 = RWD, 0.5 = AWD, 1 = FWD
    public Slider powerDistributionSlider;
    public Text powerDistributionText;

    [Header("Engine Selection")]
    public BaseEngine selectedEngine;

    [Header("Engine Settings")]
    [SerializeField] private AnimationCurve engineTorqueCurve;
    [SerializeField] private float maxEngineRPM;
    [SerializeField] private float idleRPM;
    [SerializeField] private float engineBraking;
    [SerializeField] private float clutchStrength;
    [SerializeField] private float rpmDropOnShift;
    [SerializeField] private float rpmDropDuration;
    [SerializeField] private float rpmRecoveryDuration;

    [Header("Transmission")]
    [SerializeField] private float[] gearRatios;
    [SerializeField] private float finalDriveRatio;
    [SerializeField] private float shiftUpRPM;
    [SerializeField] private float shiftDownRPM;
    [SerializeField] private float gearShiftTime;

    [Header("Vehicle Physics")]
    public float maxSteerAngle = 30f;

    [Header("Braking")]
    public float brakeForce = 1500f;
    public float handbrakeForce = 5000f;
    public float absThreshold = 1f; // Wheel slip threshold for ABS activation
    public float absPulseRate = 1f; // How fast ABS pulses
    public float absPulseStrength = 0.5f; // How much ABS reduces brake force (0.5 = 50% reduction)
    public bool absEnabled = true;
    [Range(0f, 0.5f)] public float absTargetSlip = 0.15f; // target slip ratio under braking
    public float absKp = 2000f; // proportional gain for torque cut
    public float absKi = 3000f; // integral gain for torque cut
    public float minABSActiveSpeed = 2f; // m/s below which ABS fades out
    [Range(0.3f, 0.8f)] public float frontBrakeBias = 0.6f; // portion of total brake on front axle
    [Range(0f, 0.7f)] public float steerBrakeBiasReduction = 0.4f; // extra reduction on fronts at max steer
    [Header("Braking Advanced")]
    public float brakePowerMultiplier = 1.25f; // scales base brakeForce for more overall bite
    [Range(0f, 0.15f)] public float straightRearBiasBoost = 0.06f; // shifts some brake to rear when steering small
    [Range(0f, 0.5f)] public float frontAbsTargetSlip = 0.15f;
    [Range(0f, 0.5f)] public float rearAbsTargetSlip = 0.12f;
    public float frontAbsKp = 2600f;
    public float frontAbsKi = 3600f;
    public float rearAbsKp = 2000f;
    public float rearAbsKi = 2800f;
    [Range(0.1f, 1f)] public float reverseBrakeScale = 0.7f; // scale braking while reversing (0.7 = 30% reduction)

    [Header("Input")]
    public KeyCode handbrakeKey = KeyCode.Space;
    public float reverseEngageSpeedKmh = 5f; // below this speed S engages reverse instead of braking
    public float reverseHysteresisKmh = 2f;  // hysteresis band around engage speed to avoid bouncing

    [Header("HUD")]
    public Text speedText;
    public Text rpmText;
    public Text gearText;
    public Text psiText;
    public Text horsepowerText; 
    public Text torqueText; // New text component for horsepower
    public Slider rpmGauge;
    public Text frontLeftWheelSpeedText;
    public Text frontRightWheelSpeedText;
    public Text rearLeftWheelSpeedText;
    public Text rearRightWheelSpeedText;

    [Header("RPM Gauge")]
    public Transform rpmNeedle; // Reference to the needle transform
    public float minNeedleAngle = -90f; // Minimum angle for the needle
    public float maxNeedleAngle = 90f;  // imum angle for the needle

    [Header("RPM Smoothing")]
    public float rpmSmoothing = 8f; // Higher = smoother

    [Header("Tire Marks and Sounds")]
	public AudioSource tireSkidSound;
	public FMODUnity.EventReference fmodTireSkidEvent;
	private FMOD.Studio.EventInstance fmodTireSkidInstance;
	private bool fmodSkidCreated = false;

    public float minSkidSpeed = 5f;        // Minimum speed to start skidding
    public float minSkidAngle = 10f;       // Minimum angle to start skidding
    public float skidVolumeMultiplier = 0.5f;
    public float skidPitchMultiplier = 0.8f;
    public float wheelSpinThreshold = 0.3f; // Threshold for wheel spin detection
    public float slipThreshold = 0.4f;     // Threshold for slip detection
    
    [Header("Tire Mark Decals")]
    public Material tireMarkMaterial;      // Material for tire mark decals
    public float decalWidth = 0.3f;        // Width of tire mark decals
    public float decalLength = 1.0f;       // Length of tire mark decals
    public float decalLifetime = 10f;      // How long decals stay visible
    public float decalFadeStart = 8f;      // When decals start fading
    public float minDecalDistance = 0.1f;  // Minimum distance between decals
    public LayerMask groundLayer;          // Layer mask for ground detection

    [Header("Lights")]
    public Light[] brakeLights; // Assign rear brake Light components here
    [Range(0f, 1f)] public float brakeLightThreshold = 0.05f; // how much brake input to light up

    // Private variables
    private Rigidbody rb;
    private WheelCollider[] wheelColliders;
    private float motorInput;
    private float steerInput;
    private float brakeInput;
    private bool handbrakeInput;
    
    // Engine and transmission
    private float engineRPM;
    private float targetRPM;
    private int currentGear = 1; // Start in first gear (index 2 in array)
    private bool isShifting = false;
    private float shiftTimer = 0f;
    private float rpmDropStartValue = 0f;
    private float rpmDropTargetValue = 0f;
    private float rpmDropProgress = 0f;
    private bool isDroppingRPM = false;
    private bool isRecoveringRPM = false;

    // Performance metrics
    private float currentSpeed;
    private float wheelRPM;
    private float[] smoothedWheelRPM = new float[4];
    private float[] targetWheelRPM = new float[4];
    private bool[] isWheelSkidding = new bool[4];
    private float engineRPMSmoothVelocity = 0f; // velocity term for SmoothDamp
    private float[] absIntegral = new float[4];
    private bool reverseSDriveLatch = false; // latch for S behavior while in reverse

    // New variables for steering
    private float currentSteerAngle = 0f;
    public float steerTurnSpeed = 400f;   // deg/sec when turning with input
    public float steerReturnSpeed = 800f; // deg/sec when returning to center (no input)

    private float clutchTimer = 0f;
    private float clutchDuration = 0.4f; // Duration of clutch slip in seconds
    private float preShiftRPM = 0f;
    private bool wasThrottling = false;

    private TireMarkDecal[] tireMarkDecals;

    private float[] wheelSlip = new float[4];
    private float[] absTimer = new float[4];
    private bool[] absActive = new bool[4];
    private bool brakeLightsAreOn = false;


    void Awake()
    {
        if (selectedEngine != null)
        {
            ApplyEngineSettings();
        }
    }

    void ApplyEngineSettings()
    {
        // Apply engine configuration
        engineTorqueCurve = selectedEngine.engineTorqueCurve;
        maxEngineRPM = selectedEngine.maxEngineRPM;
        idleRPM = selectedEngine.idleRPM;
        engineBraking = selectedEngine.engineBraking;
        clutchStrength = selectedEngine.clutchStrength;
        rpmDropOnShift = selectedEngine.rpmDropOnShift;
        rpmDropDuration = selectedEngine.rpmDropDuration;
        rpmRecoveryDuration = selectedEngine.rpmRecoveryDuration;

        // Apply transmission settings
        gearRatios = selectedEngine.gearRatios;
        finalDriveRatio = selectedEngine.finalDriveRatio;
        shiftUpRPM = selectedEngine.shiftUpRPM;
        shiftDownRPM = selectedEngine.shiftDownRPM;
        gearShiftTime = selectedEngine.gearShiftTime;
    }

    void Start()
    {
        Time.fixedDeltaTime = 0.01f;
        Physics.defaultSolverIterations = 12;
        Physics.defaultSolverVelocityIterations = 12;
        GetComponent<Rigidbody>().maxAngularVelocity = 100f;
        GetComponent<Rigidbody>().sleepThreshold = 0f;

        rb = GetComponent<Rigidbody>();
        
        // Set center of mass from GameObject if assigned
        if (centerOfMassObject != null)
            rb.centerOfMass = transform.InverseTransformPoint(centerOfMassObject.position);

        // Setup wheel colliders array
        SetupWheelColliders();
        
        // Initialize HUD
        if (rpmGauge != null)
        {
            rpmGauge.minValue = 0;
            rpmGauge.maxValue = maxEngineRPM;
        }

        // Initialize power distribution slider
        if (powerDistributionSlider != null)
        {
            powerDistributionSlider.minValue = 0f;
            powerDistributionSlider.maxValue = 1f;
            powerDistributionSlider.value = frontPowerBias;
            powerDistributionSlider.onValueChanged.AddListener(OnPowerDistributionChanged);
            UpdatePowerDistributionText();
        }

        // Initialize smoothed RPM values
        for (int i = 0; i < 4; i++)
        {
            smoothedWheelRPM[i] = 0f;
            targetWheelRPM[i] = 0f;
            isWheelSkidding[i] = false;
        }

        // Setup wheel speed HUD
        SetupHUD();

        // Initialize tire mark decals
        tireMarkDecals = new TireMarkDecal[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject decalObj = new GameObject($"TireMarkDecal_{i}");
            decalObj.transform.SetParent(transform);
            tireMarkDecals[i] = decalObj.AddComponent<TireMarkDecal>();
            tireMarkDecals[i].decalMaterial = tireMarkMaterial;
            tireMarkDecals[i].decalWidth = decalWidth;
            tireMarkDecals[i].decalLength = decalLength;
            tireMarkDecals[i].decalLifetime = decalLifetime;
            tireMarkDecals[i].fadeStartTime = decalFadeStart;
            tireMarkDecals[i].minDistanceBetweenDecals = minDecalDistance;
            tireMarkDecals[i].groundLayer = groundLayer;
        }

		// Configure tire skid sound (Unity or FMOD)
		if (tireSkidSound != null)
		{
			tireSkidSound.loop = true;
			tireSkidSound.playOnAwake = false;
			tireSkidSound.spatialBlend = 1f; // 3D sound
			tireSkidSound.minDistance = 5f;
			tireSkidSound.maxDistance = 50f;
			tireSkidSound.rolloffMode = AudioRolloffMode.Linear;
			tireSkidSound.dopplerLevel = 0f;
		}
		if (selectedEngine != null && selectedEngine.useFMOD && fmodTireSkidEvent.IsNull == false)
		{
			try
			{
				fmodTireSkidInstance = FMODUnity.RuntimeManager.CreateInstance(fmodTireSkidEvent);
				FMODUnity.RuntimeManager.AttachInstanceToGameObject(fmodTireSkidInstance, transform, GetComponent<Rigidbody>());
				fmodTireSkidInstance.start();
				fmodSkidCreated = true;
			}
			catch { fmodSkidCreated = false; }
		}


        // Before activating the new engine, deactivate all engine sounds
        foreach (var engine in FindObjectsOfType<BaseEngine>())
        {
            engine.DeactivateEngineSounds();
        }

        // Activate only the selected engine's sounds
        if (selectedEngine != null)
        {
            ApplyEngineSettings();
            selectedEngine.ActivateEngineSounds();
        }

        // Apply turbo settings based on tuning
        ApplyTurboSettings();
    }

    void SetupWheelColliders()
    {
        // Use your existing wheel colliders
        wheelColliders = new WheelCollider[4];
        wheelColliders[0] = frontLeftWheelCollider;
        wheelColliders[1] = frontRightWheelCollider;
        wheelColliders[2] = rearLeftWheelCollider;
        wheelColliders[3] = rearRightWheelCollider;
    }

    void Update()
    {
        // Get input
        GetInput();
        
        // Handle automatic transmission
        HandleAutomaticTransmission();
        
        // Update engine
        UpdateEngine();
        
        // Update HUD
        UpdateHUD();

        // Drive FMOD parameters (if enabled on engine)
		if (selectedEngine != null)
		{
			selectedEngine.UpdateFMODParameters(engineRPM, motorInput, currentGear, IsAtLimiter());
			var turbo = GetComponent<TurboSystem>();
			if (turbo != null)
			{
				selectedEngine.UpdateFMODBoost(turbo.GetCurrentPSI(), turbo.maxPSI);
			}
		}

        HandleEngineSound();
        HandleTireMarksAndSounds();
        UpdateBrakeLights();
        if (selectedEngine != null)
        {
            // Domyślnie: efekty wg gazu
            bool forceDeaccel = false;
            bool forceAccel = false;

            // Sprawdź czy trwa zmiana biegu
            if (isShifting)
            {
                // Rozpoznaj kierunek zmiany biegu
                // Jeśli poprzedni bieg < aktualny => upshift
                // Jeśli poprzedni bieg > aktualny => downshift
                // Musimy zapamiętać poprzedni bieg
                if (!hasPrevGear)
                {
                    prevGear = currentGear;
                    hasPrevGear = true;
                }
                if (currentGear > prevGear)
                {
                    // Upshift: efekty ON
                    forceDeaccel = true;
                    forceAccel = false;
                }
                else if (currentGear < prevGear)
                {
                    // Downshift: efekty OFF
                    forceDeaccel = false;
                    forceAccel = true;
                }
            }
            else
            {
                hasPrevGear = false;
            }
            selectedEngine.UpdateDeaccelEQ(motorInput, forceDeaccel, forceAccel);
        }
    }
    // --- Do obsługi logiki zmiany biegu (upshift/downshift) dla efektów dźwięku ---
    private int prevGear = 1;
    private bool hasPrevGear = false;

    void FixedUpdate()
    {
        // Apply physics
        ApplySteering();
        ApplyMotor();
        ApplyBraking();
        
        // Update wheel visuals
        UpdateWheelVisuals();
        
        // Calculate current speed
        currentSpeed = rb.linearVelocity.magnitude * 3.6f; // Convert to km/h
    }

    void GetInput()
    {
        float vertical = Input.GetAxis("Vertical");
        motorInput = vertical;
        steerInput = Input.GetAxis("Horizontal");
        
        float brakeAxis = 0f;
        try { brakeAxis = Mathf.Clamp01(Input.GetAxis("Brake")); } catch { brakeAxis = 0f; }
        
        bool sPressed = Input.GetKey(KeyCode.S);
        bool nearlyStopped = currentSpeed <= reverseEngageSpeedKmh + 0.01f;
        float lowThresh = Mathf.Max(0f, reverseEngageSpeedKmh - reverseHysteresisKmh);
        float highThresh = reverseEngageSpeedKmh + reverseHysteresisKmh;
        
        if (sPressed)
        {
            if (currentGear == -1)
            {
                // In reverse gear: S is always reverse throttle; braking in reverse is mapped to W
                motorInput = -1f;
                brakeInput = 0f;
            }
            else
            {
                if (nearlyStopped)
                {
                    // Use S as reverse throttle when nearly stopped (preparing to engage R)
                    motorInput = -1f;
                    brakeInput = 0f;
                }
                else
                {
                    // Use S as brake when moving forward
                    brakeInput = 1f;
                }
            }
        }
        else
        {
            // Reset latch when S released
            reverseSDriveLatch = false;
            // No S pressed: use mapped Brake axis only
            brakeInput = brakeAxis;
        }
        
        // When reversing and moving, make W act as brake instead of forward throttle
        if (currentGear == -1)
        {
            bool wPressed = Input.GetKey(KeyCode.W);
            if (wPressed && !nearlyStopped)
            {
                brakeInput = 1f;
                // prevent forward drive while commanding brake in reverse
                if (motorInput > 0f) motorInput = 0f;
            }
        }
        
        handbrakeInput = Input.GetKey(handbrakeKey);
    }

    void HandleAutomaticTransmission()
    {
        if (isShifting)
        {
            shiftTimer += Time.deltaTime;
            if (shiftTimer >= gearShiftTime)
            {
                isShifting = false;
                shiftTimer = 0f;
                if (currentGear > 0)
                {
                    float gearRatio = Mathf.Abs(gearRatios[GetGearRatioIndex(currentGear)]);
                    float newEngineRPM = wheelRPM * gearRatio * finalDriveRatio;
                    engineRPM = Mathf.Max(newEngineRPM, idleRPM);
                }
            }
            return;
        }

        // Reverse and neutral logic
        if (motorInput < -0.3f && currentSpeed < 5f && currentGear > -1)
        {
            currentGear = -1;
            isShifting = false;
            shiftTimer = 0f;
        }
        else if (motorInput > 0.3f && currentGear == -1 && currentSpeed < 5f)
        {
            currentGear = 1;
            isShifting = false;
            shiftTimer = 0f;
        }
        else if (currentSpeed < 2f && Mathf.Abs(motorInput) < 0.1f && currentGear != 0)
        {
            currentGear = 0;
        }
        else if (motorInput > 0.1f && currentGear == 0)
        {
            currentGear = 1;
        }

        // Upshift logic - now only based on RPM
        int maxForwardGear = (gearRatios != null ? gearRatios.Length - 2 : 6); // -1: reverse, 0: neutral, rest: forward
        if (currentGear > 0 && currentGear < maxForwardGear && motorInput > 0.5f)
        {
            // Only shift up when we reach the shift up RPM
            if (engineRPM >= shiftUpRPM)
            {
                currentGear++;
                isShifting = true;
                shiftTimer = 0f;
                clutchTimer = 0f;
                preShiftRPM = engineRPM;
            }
        }

        // Downshift logic - tylko jeśli po redukcji RPM < shiftUpRPM - 500
        if (currentGear > 1)
        {
            bool isBraking = brakeInput > 0.2f || handbrakeInput;
            float downshiftRPM = shiftDownRPM;
            if (isBraking)
                downshiftRPM *= 1.15f;

            if (currentSpeed > 5f)
            {
                // Oblicz przewidywane RPM po redukcji
                int lowerGear = currentGear - 1;
                float lowerGearRatio = Mathf.Abs(gearRatios[GetGearRatioIndex(lowerGear)]);
                float currentGearRatio = Mathf.Abs(gearRatios[GetGearRatioIndex(currentGear)]);
                float predictedRPM = engineRPM * (lowerGearRatio / currentGearRatio);
                if (predictedRPM < (shiftUpRPM - 500))
                {
                    currentGear--;
                    isShifting = true;
                    shiftTimer = 0f;
                    clutchTimer = 0f;
                    preShiftRPM = engineRPM;
                }
            }
        }
    }

    void ShiftUp()
    {
        if (currentGear < 6)
        {
            currentGear++;
            isShifting = true;
            shiftTimer = 0f;
        }
    }

    void ShiftDown()
    {
        if (currentGear > -1)
        {
            currentGear--;
            isShifting = true;
            shiftTimer = 0f;
        }
    }

    void UpdateEngine()
    {
        if (gearRatios == null || gearRatios.Length == 0)
        {
            Debug.LogError($"gearRatios array is not set or empty on {gameObject.name}. Please check the engine or car setup.");
            return;
        }
        // Calculate wheel RPM from all wheels (AWD) with improved smoothing
        float rawWheelRPM = Mathf.Abs((wheelColliders[0].rpm + wheelColliders[1].rpm + 
                             wheelColliders[2].rpm + wheelColliders[3].rpm) / 4f);
        
        // Apply adaptive smoothing to wheel RPM (slower response when coasting to reduce jitter)
        float wheelRpmSmoothingRate = (motorInput < 0.1f && !isShifting) ? 2f : 5f;
        wheelRPM = Mathf.Lerp(wheelRPM, rawWheelRPM, Time.deltaTime * wheelRpmSmoothingRate);
        
        float targetRPM = engineRPM;
        if (currentGear != 0 && !isShifting) // Not in neutral and not shifting
        {
            float gearRatio = Mathf.Abs(gearRatios[GetGearRatioIndex(currentGear)]);
            float wheelBasedRPM = wheelRPM * gearRatio * finalDriveRatio;
            wheelBasedRPM = Mathf.Max(wheelBasedRPM, idleRPM);
            if (motorInput > 0.1f)
            {
                // More linear RPM progression
                targetRPM = wheelBasedRPM + (motorInput * (maxEngineRPM - wheelBasedRPM) * 0.3f);
            }
            else
            {
                targetRPM = wheelBasedRPM;
            }
        }
        else if (isShifting)
        {
            clutchTimer += Time.deltaTime;
            float minClutchDuration = 0.35f; // Minimum clutch slip for 1→2
            float actualClutchDuration = (currentGear == 2) ? Mathf.Max(clutchDuration, minClutchDuration) : clutchDuration;
            float t = Mathf.Clamp01(clutchTimer / actualClutchDuration);
            float gearRatio = Mathf.Abs(gearRatios[GetGearRatioIndex(currentGear)]);
            float wheelBasedRPM = wheelRPM * gearRatio * finalDriveRatio;
            targetRPM = Mathf.Lerp(preShiftRPM, wheelBasedRPM, t);
        }
        else if (motorInput > 0.1f)
        {
            targetRPM = idleRPM + (motorInput * (maxEngineRPM - idleRPM) * 0.5f);
        }
        else
        {
            targetRPM = idleRPM;
        }
        // Clamp target RPM
        targetRPM = Mathf.Clamp(targetRPM, idleRPM, maxEngineRPM);
        // Apply adaptive damping: longer damping time when coasting to avoid oscillations
        float dampingTime = (motorInput < 0.1f && !isShifting) ? 0.3f : 0.15f;
        engineRPM = Mathf.SmoothDamp(engineRPM, targetRPM, ref engineRPMSmoothVelocity, dampingTime);
    }

    void ApplySteering()
    {
        // Calculate target steer angle
        float targetSteerAngle = steerInput * maxSteerAngle;
        
        // Use faster return-to-center when input is small
        float absInput = Mathf.Abs(steerInput);
        float steerDegPerSec = Mathf.Lerp(steerReturnSpeed, steerTurnSpeed, absInput);
        currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, targetSteerAngle, steerDegPerSec * Time.deltaTime);

        // Apply steering to front wheels
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;

        // Reduce steering angle when braking hard (simulating weight transfer)
        if (brakeInput > 0.5f)
        {
            float brakeSteerReduction = 0.7f; // Reduce steering by 30% when braking hard
            frontLeftWheelCollider.steerAngle *= brakeSteerReduction;
            frontRightWheelCollider.steerAngle *= brakeSteerReduction;
        }
    }

    void HandleEngineSound()
    {
        if (selectedEngine == null) return;

        // If using FMOD engine event, skip legacy layered audio to avoid doubling
        if (selectedEngine.useFMOD) return;
    }

    void ApplyMotor()
    {
        if (gearRatios == null || gearRatios.Length == 0)
        {
            Debug.LogError($"gearRatios array is not set or empty on {gameObject.name}. Please check the engine or car setup.");
            return;
        }
        // --- TURBO ---
    float turboBoost = 1f;
    TurboSystem turbo = GetComponent<TurboSystem>();
    if (turbo != null)
    {
        // tu boost = 1.0f przy 0 PSI, rośnie liniowo z ciśnieniem
        turboBoost = 1f + (turbo.GetCurrentPSI() * 0.06f); // ~6% mocy za każde 1 PSI
    }

        // Reverse gear logic takes precedence so braking input cannot cancel reverse drive
        if (currentGear == -1)
        {
            float reverseTorque = engineTorqueCurve.Evaluate(engineRPM) * Mathf.Abs(motorInput) * turboBoost;
            float gearRatio = Mathf.Abs(gearRatios[GetGearRatioIndex(currentGear)]); // Reverse gear ratio
            float totalTorque = reverseTorque * gearRatio * finalDriveRatio;
            
            // Apply reverse torque to all wheels
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].motorTorque = -totalTorque / 4f; // Negative torque for reverse
            }
            return;
        }

        // If braking in forward/neutral, don't apply positive motor torque (let brakes work)
        if (brakeInput > 0.05f)
        {
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].motorTorque = 0f;
            }
        }
        else
        {
            // float engineTorque = engineTorqueCurve.Evaluate(engineRPM) * motorInput * turboBoost; stare to
            float baseTorque = engineTorqueCurve.Evaluate(engineRPM) * motorInput;   // bez turbo
            float boostedTorque = baseTorque * turboBoost;                     // z turbo
            if (currentGear != 0 && !isShifting) // Not in neutral and not shifting
            {
                float gearRatio = Mathf.Abs(gearRatios[GetGearRatioIndex(currentGear)]);
                float totalTorque = boostedTorque * gearRatio * finalDriveRatio;
                
                // Calculate front and rear torque based on power bias
                float frontTorque = totalTorque * frontPowerBias;
                float rearTorque = totalTorque * (1f - frontPowerBias);
                
                // Apply torque to front wheels
                wheelColliders[0].motorTorque = frontTorque / 2f;
                wheelColliders[1].motorTorque = frontTorque / 2f;
                
                // Apply torque to rear wheels
                wheelColliders[2].motorTorque = rearTorque / 2f;
                wheelColliders[3].motorTorque = rearTorque / 2f;
            }
            else
            {
                // No torque when in neutral or shifting
                for (int i = 0; i < 4; i++)
                {
                    wheelColliders[i].motorTorque = 0f;
                }
            }
        }

        // Apply engine braking when not accelerating and in gear (skip if braking)
        if (brakeInput <= 0.05f && motorInput < 0.1f && currentGear > 0 && !isShifting)
        {
            float engineBrakeTorque = engineBraking;
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].motorTorque = -engineBrakeTorque / 4f;
            }
        }
    }

    void ApplyBraking()
    {
        // Calculate per-wheel slip ratio and grounded state
        bool[] grounded = new bool[4];
        float[] slipRatio = new float[4];
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            WheelCollider wc = wheelColliders[i];
            WheelHit hit;
            grounded[i] = wc.GetGroundHit(out hit);

            Vector3 wheelForward = wc.transform.forward;
            Vector3 pointVel = rb.GetPointVelocity(wc.transform.position);
            float longVel = Vector3.Dot(pointVel, wheelForward);

            float wheelAngular = wc.rpm * 2f * Mathf.PI / 60f;
            float wheelLinear = wheelAngular * wc.radius;

            float absLongVel = Mathf.Abs(longVel);
            float sr = 0f;
            if (absLongVel > 0.5f)
                sr = Mathf.Clamp01((absLongVel - Mathf.Abs(wheelLinear)) / absLongVel);

            if (grounded[i])
                sr = Mathf.Max(sr, Mathf.Abs(hit.forwardSlip));

            slipRatio[i] = sr;
            wheelSlip[i] = sr;
        }

        // Distribute base brake by axle bias
        float totalBrake = Mathf.Max(0f, brakeForce * brakePowerMultiplier * brakeInput);
        if (currentGear == -1)
        {
            totalBrake *= reverseBrakeScale;
        }
        // Dynamic axle bias: when steering is small, shift a bit of bias to the rear to help stability
        float steerAbs = Mathf.Clamp01(Mathf.Abs(steerInput));
        float rearBiasBoost = Mathf.Lerp(straightRearBiasBoost, 0f, steerAbs);
        float effectiveFrontBias = Mathf.Clamp01(frontBrakeBias - rearBiasBoost);
        float frontTotal = totalBrake * effectiveFrontBias;
        float rearTotal = totalBrake * (1f - effectiveFrontBias);
        float perFront = frontTotal * 0.5f;
        float perRear = rearTotal * 0.5f;

        // Reduce front brake while steering (cornering stability)
        float steerFactor = Mathf.Lerp(1f, 1f - steerBrakeBiasReduction, Mathf.Clamp01(Mathf.Abs(steerInput)));
        float frontScaled = perFront * steerFactor;

        // Apply ABS modulation per wheel (PI control towards target slip)
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            bool isFront = (i == 0 || i == 1);
            float baseTorque = isFront ? frontScaled : perRear;

            // Skip ABS for handbrake on rears
            bool canABS = absEnabled && !handbrakeInput && grounded[i] && currentSpeed > minABSActiveSpeed;

            if (canABS)
            {
                float targetSlip = isFront ? frontAbsTargetSlip : rearAbsTargetSlip;
                float error = slipRatio[i] - targetSlip;
                // Only integrate when above target (tendency to lock)
                float integrate = Mathf.Max(0f, error);
                absIntegral[i] += integrate * Time.fixedDeltaTime;
                absIntegral[i] = Mathf.Clamp(absIntegral[i], 0f, 1f);

                float kp = isFront ? frontAbsKp : rearAbsKp;
                float ki = isFront ? frontAbsKi : rearAbsKi;
                float cutTorque = Mathf.Max(0f, kp * error + ki * absIntegral[i]);
                float scale = 1f - (cutTorque / (baseTorque + 1e-3f));
                baseTorque *= Mathf.Clamp01(scale);
            }
            else
            {
                // Decay integral when ABS inactive
                absIntegral[i] = Mathf.MoveTowards(absIntegral[i], 0f, Time.fixedDeltaTime * 0.5f);
            }

            // Fade ABS near standstill to allow full stop
            if (currentSpeed <= minABSActiveSpeed)
            {
                float fade = Mathf.InverseLerp(0.2f, minABSActiveSpeed, currentSpeed);
                baseTorque *= Mathf.Clamp01(fade);
            }

            wheelColliders[i].brakeTorque = baseTorque;
        }

        // Handbrake to rears (overrides)
        if (handbrakeInput)
        {
            wheelColliders[2].brakeTorque = handbrakeForce;
            wheelColliders[3].brakeTorque = handbrakeForce;
        }
    }


    void UpdateWheelVisuals()
    {
        UpdateWheelVisual(frontLeftWheelCollider, frontLeftWheel);
        UpdateWheelVisual(frontRightWheelCollider, frontRightWheel);
        UpdateWheelVisual(rearLeftWheelCollider, rearLeftWheel);
        UpdateWheelVisual(rearRightWheelCollider, rearRightWheel);
    }

    void UpdateWheelVisual(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    void UpdateHUD()
    {
        if (speedText != null)
            speedText.text = "Speed: " + currentSpeed.ToString("F0") + " km/h";

        if (rpmText != null)
            rpmText.text = "RPM: " + engineRPM.ToString("F0");

        if (gearText != null)
        {
            string gearDisplay = currentGear == 0 ? "N" : 
                               currentGear == -1 ? "R" : 
                               currentGear.ToString();
            gearText.text = "Gear: " + gearDisplay;
        }

        // Calculate and display horsepower
        if (horsepowerText != null || torqueText != null)
{
    float turboBoost = 1f;
    TurboSystem turbo = GetComponent<TurboSystem>();
    psiText.text = "PSI: " + turbo.GetCurrentPSI().ToString("F0");
    if (turbo != null)
        turboBoost = 1f + (turbo.GetCurrentPSI() * 0.06f);

    float baseTorque = engineTorqueCurve.Evaluate(engineRPM) * motorInput; // Nm bez turbo
    float boostedTorque = baseTorque * turboBoost;                        // Nm po turbo

    float baseHP = (baseTorque * engineRPM) / 7127f;
    float boostedHP = (boostedTorque * engineRPM) / 7127f;

    // Wyświetlanie HP
    if (horsepowerText != null)
    {
        if (turboBoost > 1.01f)
            horsepowerText.text = $"Power: {boostedHP:F0} HP (Boost +{(boostedHP - baseHP):F0})";
        else
            horsepowerText.text = $"Power: {baseHP:F0} HP";
    }

    // Wyświetlanie momentu obrotowego (Nm)
    if (torqueText != null)
    {
        if (turboBoost > 1.01f)
            torqueText.text = $"Torque: {boostedTorque:F0} Nm (Boost +{(boostedTorque - baseTorque):F0})";
        else
            torqueText.text = $"Torque: {baseTorque:F0} Nm";
    }
}


        // Update RPM gauge needle
        if (rpmNeedle != null)
        {
            float rpmRatio = engineRPM / maxEngineRPM;
            float targetAngle = Mathf.Lerp(minNeedleAngle, maxNeedleAngle, rpmRatio);
            rpmNeedle.localRotation = Quaternion.Euler(0, 0, targetAngle);
        }

        // Update wheel speed displays with actual wheel RPM values
        UpdateWheelSpeedText(frontLeftWheelSpeedText, wheelColliders[0].rpm);
        UpdateWheelSpeedText(frontRightWheelSpeedText, wheelColliders[1].rpm);
        UpdateWheelSpeedText(rearLeftWheelSpeedText, wheelColliders[2].rpm);
        UpdateWheelSpeedText(rearRightWheelSpeedText, wheelColliders[3].rpm);
    }

    void UpdateWheelSpeedText(Text textComponent, float wheelRPM)
    {
        if (textComponent != null)
        {
            // Format the RPM with one decimal place for smoother display
            textComponent.text = textComponent.name + ": " + wheelRPM.ToString("F1") + " RPM";
            
            // Smoother color transition based on wheel speed
            float normalizedRPM = Mathf.Clamp01(Mathf.Abs(wheelRPM) / 1000f); // Adjust 1000f to your needs
            textComponent.color = Color.Lerp(Color.white, Color.red, normalizedRPM);
        }
    }

    void SetupHUD()
    {
        // Create Canvas if it doesn't exist
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create Panel for wheel speeds
        GameObject panelObj = new GameObject("WheelSpeedsPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0, 0);
        panelRect.pivot = new Vector2(0, 0);
        panelRect.anchoredPosition = new Vector2(10, 10);
        panelRect.sizeDelta = new Vector2(200, 150); // Increased height for power distribution

        // Create power distribution slider
        GameObject sliderObj = new GameObject("PowerDistributionSlider");
        sliderObj.transform.SetParent(panelRect, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0);
        sliderRect.anchorMax = new Vector2(1, 0);
        sliderRect.pivot = new Vector2(0, 0);
        sliderRect.anchoredPosition = new Vector2(0, 130);
        sliderRect.sizeDelta = new Vector2(0, 20);

        powerDistributionSlider = sliderObj.AddComponent<Slider>();
        powerDistributionSlider.minValue = 0f;
        powerDistributionSlider.maxValue = 1f;
        powerDistributionSlider.value = frontPowerBias;

        // Create power distribution text
        GameObject textObj = new GameObject("PowerDistributionText");
        textObj.transform.SetParent(panelRect, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 0);
        textRect.pivot = new Vector2(0, 0);
        textRect.anchoredPosition = new Vector2(0, 110);
        textRect.sizeDelta = new Vector2(0, 20);

        powerDistributionText = textObj.AddComponent<Text>();
        powerDistributionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        powerDistributionText.fontSize = 14;
        powerDistributionText.alignment = TextAnchor.MiddleLeft;
        UpdatePowerDistributionText();

        // Create Text elements for each wheel
        CreateWheelSpeedText(panelRect, "FrontLeft", new Vector2(0, 95), ref frontLeftWheelSpeedText);
        CreateWheelSpeedText(panelRect, "FrontRight", new Vector2(0, 70), ref frontRightWheelSpeedText);
        CreateWheelSpeedText(panelRect, "RearLeft", new Vector2(0, 45), ref rearLeftWheelSpeedText);
        CreateWheelSpeedText(panelRect, "RearRight", new Vector2(0, 20), ref rearRightWheelSpeedText);

        // Create horsepower text
        CreateWheelSpeedText(panelRect, "Power", new Vector2(0, 0), ref horsepowerText);
    }

    void CreateWheelSpeedText(RectTransform parent, string name, Vector2 position, ref Text textComponent)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 0);
        textRect.pivot = new Vector2(0, 0);
        textRect.anchoredPosition = position;
        textRect.sizeDelta = new Vector2(0, 20);
        
        textComponent = textObj.AddComponent<Text>();
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = 14;
        textComponent.alignment = TextAnchor.MiddleLeft;
        textComponent.text = name + ": 0 RPM";
    }

    // Add these methods to expose values to TurboSystem
    public float GetThrottleInput()
    {
        return motorInput;
    }

    public float GetEngineRPM()
    {
        return engineRPM;
    }

    // Add this method to expose current gear to TurboSystem
    public int GetCurrentGear()
    {
        return currentGear;
    }

    // Add this method to expose shifting state to TurboSystem
    public bool IsShifting()
    {
        return isShifting;
    }

    private void HandleTireMarksAndSounds()
    {
        bool anyWheelSkidding = false;
        float maxSkidIntensity = 0f;

        for (int i = 0; i < wheelColliders.Length; i++)
        {
            WheelHit hit;
            if (wheelColliders[i].GetGroundHit(out hit))
            {
                // Calculate slip and spin
                float forwardSlip = Mathf.Abs(hit.forwardSlip);
                float sideSlip = Mathf.Abs(hit.sidewaysSlip);
                float slip = Mathf.Max(forwardSlip, sideSlip);
                
                // Check for wheel spin (acceleration from standstill or hard acceleration)
                bool isWheelSpinning = currentSpeed < 10f && motorInput > 0.8f && forwardSlip > wheelSpinThreshold;
                
                // Check for loss of traction (sliding)
                bool isLosingTraction = currentSpeed > minSkidSpeed && 
                                      ((slip > slipThreshold && motorInput > 0.1f) || // Only slide when accelerating
                                       (Mathf.Abs(steerInput) > 0.8f && sideSlip > slipThreshold * 1.5f)); // Or during hard steering
                
                // Combine conditions for skidding - must have significant input and slip
                bool isSkidding = (isWheelSpinning || isLosingTraction) && 
                                 (Mathf.Abs(motorInput) > 0.3f || Mathf.Abs(steerInput) > 0.8f) &&
                                 slip > slipThreshold * 0.8f;
                
                isWheelSkidding[i] = isSkidding;
                anyWheelSkidding |= isSkidding;

                // Create ground decals when skidding
                if (isSkidding && tireMarkDecals != null && i < tireMarkDecals.Length)
                {
                    // Get the wheel's forward direction projected onto the ground plane
                    Vector3 wheelForward = wheelColliders[i].transform.forward;
                    Vector3 groundForward = Vector3.ProjectOnPlane(wheelForward, hit.normal).normalized;
                    
                    // Create decal slightly above ground to prevent z-fighting
                    Vector3 decalPosition = hit.point + (hit.normal * 0.01f);
                    tireMarkDecals[i].CreateDecal(decalPosition, hit.normal, groundForward);
                }

                maxSkidIntensity = Mathf.Max(maxSkidIntensity, slip);
            }
        }

		// Handle tire skid sound (Unity or FMOD)
		if (selectedEngine != null && selectedEngine.useFMOD && fmodSkidCreated)
		{
			float speedFactor = Mathf.Clamp01(currentSpeed / 50f);
			float slipFactor = Mathf.Clamp01(maxSkidIntensity);
			float volume = (anyWheelSkidding ? 1f : 0f) * speedFactor * slipFactor * skidVolumeMultiplier;
			float pitch = 0.8f + (speedFactor * 0.4f) * skidPitchMultiplier;
			try
			{
				fmodTireSkidInstance.setParameterByName("Volume", volume, false);
				fmodTireSkidInstance.setParameterByName("Pitch", pitch, false);
				fmodTireSkidInstance.setParameterByName("Active", anyWheelSkidding ? 1f : 0f, false);
			}
			catch { }
		}
		else if (tireSkidSound != null)
		{
			if (anyWheelSkidding && !tireSkidSound.isPlaying)
			{
				tireSkidSound.Play();
			}
			else if (!anyWheelSkidding && tireSkidSound.isPlaying)
			{
				tireSkidSound.Stop();
			}
			if (tireSkidSound.isPlaying)
			{
				float speedFactor = Mathf.Clamp01(currentSpeed / 50f);
				float slipFactor = Mathf.Clamp01(maxSkidIntensity);
				float volume = speedFactor * slipFactor * skidVolumeMultiplier;
				float pitch = 0.8f + (speedFactor * 0.4f) * skidPitchMultiplier;
				tireSkidSound.volume = volume;
				tireSkidSound.pitch = pitch;
			}
		}
    }

    public bool IsAtLimiter()
    {
        return engineRPM >= maxEngineRPM - 100f;
    }

    void OnPowerDistributionChanged(float value)
    {
        frontPowerBias = value;
        UpdatePowerDistributionText();
    }

    void UpdatePowerDistributionText()
    {
        if (powerDistributionText != null)
        {
            string driveType;
            if (frontPowerBias < 0.1f)
                driveType = "RWD";
            else if (frontPowerBias > 0.9f)
                driveType = "FWD";
            else
                driveType = "AWD";

            powerDistributionText.text = $"Drive: {driveType} ({frontPowerBias:P0})";
        }
    }

    // Helper to get a safe gear ratio index
    private int GetGearRatioIndex(int gear)
    {
        // gear: -1 (reverse), 0 (neutral), 1...N (forward)
        int idx = gear + 1;
        if (gearRatios == null || gearRatios.Length == 0)
            return 1; // fallback to neutral
        if (idx < 0) return 0; // reverse
        if (idx >= gearRatios.Length) return gearRatios.Length - 1; // highest forward gear
        return idx;
    }

    // Public method to reapply engine settings when switching cars
    public void ReapplyEngineSettings()
    {
        if (selectedEngine != null)
        {
            // First, deactivate all engine sounds from all engines to prevent conflicts
            foreach (var engine in FindObjectsOfType<BaseEngine>())
            {
                engine.DeactivateEngineSounds();
            }
            
            // Reset the selected engine's sounds to initial state
            selectedEngine.ResetEngineSounds();
            
            // Apply engine settings
            ApplyEngineSettings();
            
            // Activate the selected engine's sounds
            selectedEngine.ActivateEngineSounds();
            
            // Reset engine RPM to idle
            engineRPM = idleRPM;
            targetRPM = idleRPM;
            
            // Debug log to confirm engine settings are applied
            Debug.Log($"Engine settings reapplied for {gameObject.name} with engine: {selectedEngine.name}");
        }
        else
        {
            Debug.LogWarning($"No engine assigned to {gameObject.name}");
        }
    }

    // Apply turbo settings based on tuning preferences
    private void ApplyTurboSettings()
    {
        bool turboEnabled = PlayerPrefs.GetInt("TurboEnabled", 1) == 1; // Default to enabled
        TurboSystem turboSystem = GetComponent<TurboSystem>();
        
        if (turboSystem != null)
        {
            turboSystem.enabled = turboEnabled;
            Debug.Log($"Turbo system {(turboEnabled ? "enabled" : "disabled")} for {gameObject.name}");
        }
    }

    void UpdateBrakeLights()
    {
        bool shouldBeOn = brakeInput > brakeLightThreshold;
        if (shouldBeOn == brakeLightsAreOn) return;
        brakeLightsAreOn = shouldBeOn;
        if (brakeLights == null) return;
        for (int i = 0; i < brakeLights.Length; i++)
        {
            if (brakeLights[i] != null)
            {
                brakeLights[i].enabled = brakeLightsAreOn;
            }
        }
    }

    // ...existing code...
}