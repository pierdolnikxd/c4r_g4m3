using UnityEngine;

public class Engine_4G63 : BaseEngine
{
    [SerializeField] private void Reset()
    {
        // Parametry silnika 4G63 (Evo IX)
        maxEngineRPM = 7500f;
        idleRPM = 850f;
        engineBraking = 60f;
        clutchStrength = 110f;
        rpmDropOnShift = 0.28f;
        rpmDropDuration = 0.35f;
        rpmRecoveryDuration = 0.18f;

        // Krzywa momentu obrotowego (przybliżona, Nm)
        engineTorqueCurve = new AnimationCurve(
            new Keyframe(1000f, 180f),
            new Keyframe(2000f, 240f),
            new Keyframe(3000f, 300f),
            new Keyframe(3500f, 340f), // Peak torque (ok. 343Nm @ 3500rpm)
            new Keyframe(5000f, 320f),
            new Keyframe(6500f, 280f),
            new Keyframe(7500f, 220f)
        );

        // Przełożenia skrzyni biegów (5-biegowa, Evo IX)
        gearRatios = new float[] {
            -3.416f, // Reverse
            0f,      // Neutral
            3.827f,  // 1st
            2.360f,  // 2nd
            1.685f,  // 3rd
            1.297f,  // 4th
            1.000f   // 5th
        };
        finalDriveRatio = 4.529f;
        shiftUpRPM = 7200f;
        shiftDownRPM = 2200f;
        gearShiftTime = 0.28f;
    }
}