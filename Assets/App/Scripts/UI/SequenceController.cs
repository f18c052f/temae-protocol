using TemaeTrainer.Sequence;
using UnityEngine;

namespace TemaeTrainer.UI
{
    // Thin MonoBehaviour: translates GazeDwellButton.Fired / ISequenceInputSource triggers into
    // TemaeSequenceHost calls (F1-5). Mirrors CalibrationController's role for the calibration
    // flow. "最初から" (restart) is not wired to dwell/pinch here -- SPEC §7.1 places it behind a
    // menu action, out of scope until Phase 2's menu exists; TemaeSequenceHost.RestartFromBeginning
    // stays public for that future wiring.
    public class SequenceController : MonoBehaviour
    {
        [SerializeField] private TemaeSequenceHost sequenceHost;
        [SerializeField] private GazeDwellButton nextDwellButton;
        [SerializeField] private GazeDwellButton previousDwellButton;
        [SerializeField] private MonoBehaviour pinchInputSourceBehaviour;

        private ISequenceInputSource PinchInputSource => pinchInputSourceBehaviour as ISequenceInputSource;

        private void Awake()
        {
            if (nextDwellButton != null) nextDwellButton.Fired += () => sequenceHost?.Next();
            if (previousDwellButton != null) previousDwellButton.Fired += () => sequenceHost?.Previous();
        }

        private void Update()
        {
            var pinch = PinchInputSource;
            if (pinch == null) return;

            if (pinch.ConsumeNextTrigger()) sequenceHost?.Next();
            if (pinch.ConsumePreviousTrigger()) sequenceHost?.Previous();
        }
    }
}
