using UnityEngine;

public class Engine_RB26DETT : BaseEngine
{
    [SerializeField] private void Start()
    {
        maxEngineRPM = 8000f;
        idleRPM = 800f;
        engineBraking = 50f;
        clutchStrength = 100f;
        rpmDropOnShift = 0.3f;
        rpmDropDuration = 0.4f;
        rpmRecoveryDuration = 0.2f;
        baseEngineTorqueCurve = new AnimationCurve(
            new Keyframe(1000f, 120f),
            new Keyframe(2000f, 180f),
            new Keyframe(3500f, 225f),
            new Keyframe(4800f, 245f),
            new Keyframe(6000f, 230f),
            new Keyframe(6800f, 215f),
            new Keyframe(7500f, 190f),
            new Keyframe(8000f, 160f)
        );
        gearRatios = new float[] { 
            -3.545f,  // Reverse
            0f,       // Neutral
            3.827f,   // 1st
            2.360f,   // 2nd
            1.685f,   // 3rd
            1.312f,   // 4th
            1.000f,   // 5th
            0.793f    // 6th
        };
        finalDriveRatio = 3.545f;
        shiftUpRPM = 7800f;
        shiftDownRPM = 2000f;
        gearShiftTime = 0.3f;
    }
} 