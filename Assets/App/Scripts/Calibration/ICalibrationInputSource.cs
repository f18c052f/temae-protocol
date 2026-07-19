using UnityEngine;

namespace TemaeTrainer.Calibration
{
    // Abstracts "how the user points at and confirms a point in space" so CalibrationController
    // can work with hand tracking now and a controller-based fallback later (SPEC §2: hand
    // tracking primary, controller as fallback) without CalibrationSession knowing about either.
    public interface ICalibrationInputSource
    {
        bool TryGetCandidatePoint(out Vector3 worldPoint);

        // Edge-triggered (true only on the frame the gesture starts), mirroring OVRHand.IsPressed().
        bool ConsumeConfirmTrigger();
        bool ConsumeCorrectionTrigger();
        bool ConsumeToggleTrigger();
    }
}
