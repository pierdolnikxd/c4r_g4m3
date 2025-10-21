using UnityEngine;
using System.Collections;
using FMODUnity;
using FMOD.Studio;

public class TurboSystem : MonoBehaviour
{
    [Header("Turbo Settings")]
    public float maxPSI = 20f;
    public float spoolRateLow = 1.5f;
    public float spoolRateMid = 4f;
    public float spoolRateHigh = 8f;
    public float dropRate = 15f;
    public float minBlowOffPSI = 2f;
    public float shiftPSIRetention = 0.7f;

    [Header("Backfire Settings")]
    public float backfireChance = 0.7f;
    public float backfireMinPops = 1;
    public float backfireMaxPops = 4;
    public float backfireMinDelay = 0.05f;
    public float backfireMaxDelay = 0.2f;
    public float backfireCooldown = 0.1f;

    [Header("FMOD Events")]
    public EventReference turboSpoolEvent;
    public EventReference blowOffEvent;
    public EventReference backfireEvent;

    [Header("FMOD Parameters")]
    public string rpmParameter = "RPM";
    public string boostParameter = "Boost";
    public string psiParameter = "PSI";
    public string loadParameter = "Load";

    [Header("Exhaust Effects")]
    public ParticleSystem[] exhaustParticles; // 🔥 efekt strzałów z wydechu

    [Header("FMOD LOL XD")]
    private EventInstance turboSpoolInstance;
    private bool turboSpoolActive = false;

    // Private vars
    private float currentPSI = 0f;
    private float targetPSI = 0f;
    private float lastThrottle = 0f;
    private bool wasThrottleReleased = true;
    private int prevGear = 1;
    private bool isDroppingPSI = false;
    private float psiDropOnShift = 0f;
    private float psiDropTimer = 0f;
    private float psiDropDuration = 0.5f;
    private float lastBackfireTime = 0f;

    void Start()
    {
        // Debug FMOD native log (opcjonalne, ale pomocne)
        FMOD.Debug.Initialize(FMOD.DEBUG_FLAGS.LOG, FMOD.DEBUG_MODE.TTY);

        if (turboSpoolEvent.IsNull)
        {
            turboSpoolActive = false;
            Debug.LogWarning("[TurboSystem] turboSpoolEvent IS NULL — sprawdź przypisanie w Inspectorze.");
            return;
        }

        try
        {
            turboSpoolInstance = RuntimeManager.CreateInstance(turboSpoolEvent);
            RuntimeManager.AttachInstanceToGameObject(turboSpoolInstance, transform, GetComponent<Rigidbody>());

            // Nie startujemy od razu - startujemy kiedy gracz da gaz.
            turboSpoolActive = true;
            Debug.Log("[TurboSystem] Spool instance created. Will start on throttle.");
        }
        catch (System.Exception ex)
        {
            turboSpoolActive = false;
            Debug.LogError($"[TurboSystem] Błąd tworzenia spool instance: {ex}");
        }
    }

    void Update()
    {
        var car = GetComponent<CarController>();
        if (car == null) return;

        float throttle = car.GetThrottleInput();
        int currentGear = car.GetCurrentGear();
        bool isAtLimiter = car.IsAtLimiter();
        float rpm = car.GetEngineRPM();

        // --- PSI logic ---
        if (isDroppingPSI)
        {
            psiDropTimer += Time.deltaTime;
            float t = Mathf.Clamp01(psiDropTimer / psiDropDuration);
            float retained = psiDropOnShift * shiftPSIRetention;
            currentPSI = Mathf.Lerp(psiDropOnShift, retained, t);
            if (t >= 1f) isDroppingPSI = false;
        }
        else
        {
            targetPSI = maxPSI * throttle;
            float buildRate = (currentPSI < 3f) ? spoolRateLow :
                             (currentPSI < 10f) ? spoolRateMid : spoolRateHigh;

            if (currentPSI < targetPSI)
                currentPSI = Mathf.MoveTowards(currentPSI, targetPSI, buildRate * Time.deltaTime);
            else if (currentPSI > targetPSI)
                currentPSI = Mathf.MoveTowards(currentPSI, targetPSI, dropRate * Time.deltaTime);

            currentPSI = Mathf.Clamp(currentPSI, 0f, maxPSI);
        }

        // --- Blow-off on throttle release ---
        float fullThrottle = 0.99f;
        if (lastThrottle >= fullThrottle && throttle < fullThrottle && !wasThrottleReleased && currentPSI > minBlowOffPSI)
        {
            PlayBlowOff(currentPSI);
            if (Random.value < backfireChance)
                StartCoroutine(PlayBackfireSequence(currentPSI));
            wasThrottleReleased = true;
        }
        else if (throttle >= fullThrottle)
        {
            wasThrottleReleased = false;
        }

        // --- Upshift handling ---
        if (currentGear > prevGear)
        {
            if (currentPSI > 1f)
            {
                PlayBlowOff(currentPSI);
                if (Random.value < backfireChance)
                    StartCoroutine(PlayBackfireSequence(currentPSI));
            }
            psiDropOnShift = currentPSI;
            psiDropTimer = 0f;
            isDroppingPSI = true;
        }
        prevGear = currentGear;

        // --- Limiter backfire ---
        if (isAtLimiter && rpm > 7800f && Random.value < backfireChance * 0.5f)
            StartCoroutine(PlayBackfireSequence(currentPSI));

        if (turboSpoolActive)
            UpdateSpoolFMOD(rpm, throttle, currentPSI);

        lastThrottle = throttle;
    }

    private void UpdateSpoolFMOD(float rpm, float throttle, float psi)
    {
        // guard
        if (!turboSpoolActive) return;
        if (turboSpoolInstance.Equals(null)) return; // dodatkowa ochrona

        float boostNorm = Mathf.Clamp01(psi / maxPSI);
        float throttleThreshold = 0.05f;

        try
        {
            // pobierz stan
            PLAYBACK_STATE state;
            turboSpoolInstance.getPlaybackState(out state);

            if (throttle > throttleThreshold)
            {
                // startuj jeśli nie gra
                if (state != PLAYBACK_STATE.PLAYING)
                {
                    turboSpoolInstance.start();
                }

                // ustaw parametr Boost (0..1)
                FMOD.RESULT res = turboSpoolInstance.setParameterByName(boostParameter, boostNorm);
            }
            else
            {
                // wycisz lub zatrzymaj
                FMOD.RESULT res = turboSpoolInstance.setParameterByName(boostParameter, 0f);
                if (res != FMOD.RESULT.OK)

                if (state == PLAYBACK_STATE.PLAYING)
                {
                    turboSpoolInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TurboSystem] Błąd UpdateSpoolFMOD: {ex}");
        }
    }




    private void PlayBlowOff(float psi)
    {
        try
        {
            var inst = RuntimeManager.CreateInstance(blowOffEvent);
            RuntimeManager.AttachInstanceToGameObject(inst, transform, GetComponent<Rigidbody>());
            inst.setParameterByName(boostParameter, Mathf.Clamp01(psi / maxPSI));
            inst.start();
            inst.release();
        }
        catch { }
    }

    private IEnumerator PlayBackfireSequence(float psi)
    {
        if (Time.time - lastBackfireTime < backfireCooldown)
            yield break;
        lastBackfireTime = Time.time;

        int pops = Mathf.RoundToInt(Random.Range(backfireMinPops, backfireMaxPops));
        for (int i = 0; i < pops; i++)
        {
            PlayBackfireEvent(psi);
            PlayExhaustParticles();
            yield return new WaitForSeconds(Random.Range(backfireMinDelay, backfireMaxDelay));
        }
    }

    private void PlayBackfireEvent(float psi)
    {
        try
        {
            var inst = RuntimeManager.CreateInstance(backfireEvent);
            RuntimeManager.AttachInstanceToGameObject(inst, transform, GetComponent<Rigidbody>());
            inst.setParameterByName(psiParameter, psi);
            inst.setParameterByName(boostParameter, Mathf.Clamp01(psi / maxPSI));
            inst.start();
            inst.release();
        }
        catch { }
    }

    private void PlayExhaustParticles()
    {
        if (exhaustParticles == null || exhaustParticles.Length == 0)
            return;

        ParticleSystem ps = exhaustParticles[Random.Range(0, exhaustParticles.Length)];
        if (ps != null)
        {
            ps.Clear();
            ps.Play();
        }
    }

    void OnDisable()
    {
        try
        {
            turboSpoolInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            turboSpoolInstance.release();
            turboSpoolActive = false;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TurboSystem] OnDisable error: {ex}");
        }
    }

    public float GetCurrentPSI() => currentPSI;
    public float GetBoostLevel() => currentPSI / maxPSI;
}
