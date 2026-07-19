namespace TemaeTrainer.UI
{
    // SPEC §7.1 operation-system ②(ピンチ・副). Mirrors Calibration/ICalibrationInputSource so
    // SequenceController can work with hand tracking now and a controller fallback later.
    public interface ISequenceInputSource
    {
        // Edge-triggered (true only on the frame the pinch starts).
        bool ConsumeNextTrigger();
        bool ConsumePreviousTrigger();
    }
}
