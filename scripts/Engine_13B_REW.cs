using UnityEngine;

public class Engine_13B_REW : BaseEngine
{
    [SerializeField] private void Reset()
    {
        // Initialize 13B_REW specific settings
        maxEngineRPM = 9000f;
        idleRPM = 750f;
        engineBraking = 50f;
        clutchStrength = 100f;
        rpmDropOnShift = 0.3f;
        rpmDropDuration = 0.4f;
        rpmRecoveryDuration = 0.2f;

        // Initialize torque curve
        engineTorqueCurve = new AnimationCurve(
            new Keyframe(1000f, 120f),    // Low RPM torque
        new Keyframe(2000f, 180f),    // Mid-low RPM
        new Keyframe(3000f, 220f),    // Mid RPM
        new Keyframe(4000f, 250f),    // Mid-high RPM
        new Keyframe(5000f, 270f),    // Peak torque
        new Keyframe(6000f, 260f),    // High RPM
        new Keyframe(7000f, 240f),     // Redline
        new Keyframe(8000f, 220f),      // Redline
        new Keyframe(9000f, 200f)      // Redline
        );

        // Initialize gear ratios
        gearRatios = new float[] { 
            -3.545f,  // Reverse
            0f,       // Neutral
            3.483f,   // 1st
            2.015f,   // 2nd
            1.391f,   // 3rd
            1.000f,   // 4th
            0.782f    // 5th
        };
        finalDriveRatio = 4.100f;
        shiftUpRPM = 8900f;
        shiftDownRPM = 4000f;
        gearShiftTime = 0.3f;
    }
} 