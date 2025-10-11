using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class BaseEngine : MonoBehaviour
{
    [Header("FMOD (Optional)")]
    public bool useFMOD = false;
#if FMOD_PRESENT
#endif
    public EventReference fmodEngineEvent;
    public string fmodRPMParameter = "RPM";
	public string fmodThrottleParameter = "Load"; // 0..1, reflects accel/deaccel blend
	public string fmodBoostParameter = "Boost"; // 0..1 normalized boost
	public string fmodPSIParameter = "PSI"; // absolute PSI if desired
    public float fmodRPMScale = 1f; // multiply engine RPM before sending to FMOD
    private EventInstance fmodEngineInstance;
    private bool fmodInstanceCreated = false;
	private float fmodLoadParam = 0f;

    [Header("Engine Configuration")]
    public float engineRPM { get; protected set; }
    public float maxEngineRPM = 8000f;
    public float idleRPM = 800f;
    public float engineBraking = 50f;
    public float clutchStrength = 100f;
    public float rpmDropOnShift = 0.3f;
    public float rpmDropDuration = 0.4f;
    public float rpmRecoveryDuration = 0.2f;

    [Header("Power Curve")]
    public AnimationCurve baseEngineTorqueCurve;
    public AnimationCurve engineTorqueCurve; // aktualnie używana krzywa

    [Header("Transmission")]
    public float[] gearRatios;
    public float finalDriveRatio;
    public float shiftUpRPM = 6500f;
    public float shiftDownRPM = 2000f;
    public float gearShiftTime = 0.3f;

    void Awake()
    {
        // Przy starcie kopiujemy bazową krzywą do aktualnej krzywej
        if (baseEngineTorqueCurve != null)
        {
            engineTorqueCurve = new AnimationCurve(baseEngineTorqueCurve.keys);
        }
        else
        {
            Debug.LogWarning($"{name} baseEngineTorqueCurve is null!");
        }

        // FMOD setup (optional)
        if (useFMOD && fmodEngineEvent.IsNull == false)
        {
            try
            {
                fmodEngineInstance = RuntimeManager.CreateInstance(fmodEngineEvent);
                RuntimeManager.AttachInstanceToGameObject(fmodEngineInstance, transform, GetComponent<Rigidbody>());
                fmodEngineInstance.start();
                fmodInstanceCreated = true;
            }
            catch { fmodInstanceCreated = false; }
        }
        {
        if (baseEngineTorqueCurve != null)
            engineTorqueCurve = new AnimationCurve(baseEngineTorqueCurve.keys);
        }
    }

	public void UpdateDeaccelEQ(float motorInput, bool forceDeaccel = false, bool forceAccel = false)
    {
		float t;
		if (forceDeaccel)
			t = 1f;
		else if (forceAccel)
			t = 0f;
		else
			t = Mathf.Clamp01(1f - (motorInput * 2f)); // t=1 dla puszczonego gazu, t=0 dla pełnego gazu

		// If FMOD is enabled, drive Load parameter (1- t) and return
		if (useFMOD && fmodInstanceCreated)
		{
			fmodLoadParam = Mathf.Clamp01(1f - t);
			if (!string.IsNullOrEmpty(fmodThrottleParameter))
			{
				try { fmodEngineInstance.setParameterByName(fmodThrottleParameter, fmodLoadParam, false); } catch { }
			}
			return;
		}
    }

    // Call from CarController each frame to drive FMOD parameters
	public void UpdateFMODParameters(float currentRPM, float throttle, int gear, bool atLimiter)
    {
        engineRPM = currentRPM;
        if (!useFMOD || !fmodInstanceCreated) return;
        try
        {
            if (!string.IsNullOrEmpty(fmodRPMParameter))
                fmodEngineInstance.setParameterByName(fmodRPMParameter, currentRPM * fmodRPMScale, false);
			if (!string.IsNullOrEmpty(fmodThrottleParameter))
				fmodEngineInstance.setParameterByName(fmodThrottleParameter, fmodLoadParam, false);
        }
        catch { }
    }

	// Optional: drive boost/PSI parameters for FMOD engine event
	public void UpdateFMODBoost(float psi, float maxPsi)
	{
		if (!useFMOD || !fmodInstanceCreated) return;
		try
		{
			float norm = (maxPsi > 0f) ? Mathf.Clamp01(psi / maxPsi) : 0f;
			if (!string.IsNullOrEmpty(fmodBoostParameter))
				fmodEngineInstance.setParameterByName(fmodBoostParameter, norm, false);
			if (!string.IsNullOrEmpty(fmodPSIParameter))
				fmodEngineInstance.setParameterByName(fmodPSIParameter, psi, false);
		}
		catch { }
	}

    public void ActivateEngineSounds()
    {
        // If FMOD is enabled, skip Unity AudioSources
        if (useFMOD)
        {
            // Ensure FMOD instance is running when engine is activated
            if (fmodEngineEvent.IsNull == false && !fmodInstanceCreated)
            {
                try
                {
                    fmodEngineInstance = RuntimeManager.CreateInstance(fmodEngineEvent);
                    RuntimeManager.AttachInstanceToGameObject(fmodEngineInstance, transform, GetComponent<Rigidbody>());
                    fmodEngineInstance.start();
                    fmodInstanceCreated = true;
                }
                catch { fmodInstanceCreated = false; }
            }
            return;
        }
    }

        // Wywołaj to przy każdej zmianie biegu w CarController
    public void ResetReleaseSoundOnShift()
    {
        // ...existing code...
    }

    public void DeactivateEngineSounds()
    {
        // Stop FMOD instance if active
        if (fmodInstanceCreated)
        {
            try
            {
                fmodEngineInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                fmodEngineInstance.release();
            }
            catch { }
            fmodInstanceCreated = false;
        }

        // If FMOD is enabled, skip Unity AudioSources
        if (useFMOD) return;
    }

    public void ResetEngineSounds()
    {
        // Restart FMOD engine instance
        if (useFMOD && fmodEngineEvent.IsNull == false)
        {
            if (fmodInstanceCreated)
            {
                try
                {
                    fmodEngineInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    fmodEngineInstance.release();
                }
                catch { }
                fmodInstanceCreated = false;
            }
            try
            {
                fmodEngineInstance = RuntimeManager.CreateInstance(fmodEngineEvent);
                RuntimeManager.AttachInstanceToGameObject(fmodEngineInstance, transform, GetComponent<Rigidbody>());
                fmodEngineInstance.start();
                fmodInstanceCreated = true;
            }
            catch { fmodInstanceCreated = false; }
            return;
        }
    }

    public void HandleReleaseThrottleSound(float motorInput, float engineRPM)
    {
        // ...existing code...
    }

    public void TestSetRPM(float rpm)
{
    engineRPM = Mathf.Clamp(rpm, idleRPM, maxEngineRPM);
    // Jeśli masz własną logikę aktualizacji dźwięków na podstawie engineRPM, wywołaj ją tutaj.
}

    void OnDisable()
    {
        if (fmodInstanceCreated)
        {
            try
            {
                fmodEngineInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                fmodEngineInstance.release();
            }
            catch { }
            fmodInstanceCreated = false;
        }
    }
}