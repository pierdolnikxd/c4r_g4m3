using UnityEngine;

public class SimpleEngineSound : MonoBehaviour
{
    public CarController car; // Przypisz w Inspectorze
    public AudioSource[] engineAudioSources; // Każdy AudioSource = inny zakres RPM
    public float rpmStepBetweenClips = 1000f; // Odstęp RPM między warstwami
    public float engineIdleRPM = 800f;        // RPM biegu jałowego
    [Range(0f, 5f)] public float blendSharpness = 2.5f; // Im wyżej, tym ostrzejszy blend
    [Range(0f, 1f)] public float minVolumeLastLayer = 0.15f; // Minimalna głośność ostatniej warstwy

    void Update()
    {
        float currentRPM = car.GetEngineRPM();

        for (int i = 0; i < engineAudioSources.Length; i++)
        {
            float preferredRPM = i * rpmStepBetweenClips + engineIdleRPM;
            float rpmDifference = preferredRPM - currentRPM;
            float blendRange = rpmStepBetweenClips * 1.2f;
            bool isLast = (i == engineAudioSources.Length - 1);

            if (isLast && currentRPM >= preferredRPM)
            {
                // Ostatnia warstwa: powyżej swojego zakresu zawsze max volume
                engineAudioSources[i].volume = Mathf.Lerp(engineAudioSources[i].volume, 1f, Time.deltaTime * 10f);
                float pitch = currentRPM / preferredRPM;
                engineAudioSources[i].pitch = Mathf.Lerp(engineAudioSources[i].pitch, pitch, Time.deltaTime * 10f);
                if (!engineAudioSources[i].isPlaying)
                    engineAudioSources[i].Play();
            }
            else if (rpmDifference < blendRange && rpmDifference > -blendRange)
            {
                float norm = Mathf.Abs(rpmDifference) / blendRange;
                float volume = Mathf.Pow(1f - norm, blendSharpness);
                if (isLast)
                    volume = Mathf.Lerp(minVolumeLastLayer, 1f, volume); // minVolume do 1
                engineAudioSources[i].volume = Mathf.Lerp(engineAudioSources[i].volume, volume, Time.deltaTime * 10f);

                float pitch = currentRPM / preferredRPM;
                engineAudioSources[i].pitch = Mathf.Lerp(engineAudioSources[i].pitch, pitch, Time.deltaTime * 10f);

                if (!engineAudioSources[i].isPlaying)
                    engineAudioSources[i].Play();
            }
            else
            {
                float targetVol = (isLast ? minVolumeLastLayer : 0f);
                engineAudioSources[i].volume = Mathf.Lerp(engineAudioSources[i].volume, targetVol, Time.deltaTime * 10f);
                if (engineAudioSources[i].volume < 0.01f && !isLast && engineAudioSources[i].isPlaying)
                    engineAudioSources[i].Stop();
            }
        }
    }
}