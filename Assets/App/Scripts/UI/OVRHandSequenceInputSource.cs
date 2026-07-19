using System.Collections.Generic;
using UnityEngine;

namespace TemaeTrainer.UI
{
    // Concrete ISequenceInputSource backed by Meta hand tracking: right-hand index pinch = next,
    // left-hand index pinch = previous (SPEC §7.1 ②). Hand assignment is provisional -- see
    // Docs/DEVICE_CHECKLIST.md, needs real-device tuning for seated ergonomics while holding
    // tools.
    public class OVRHandSequenceInputSource : MonoBehaviour, ISequenceInputSource
    {
        [SerializeField] private OVRHand nextHand;
        [SerializeField] private OVRHand previousHand;

        private readonly Dictionary<OVRHand, bool> _wasPinching = new();

        public bool ConsumeNextTrigger() => ConsumePinch(nextHand);
        public bool ConsumePreviousTrigger() => ConsumePinch(previousHand);

        private bool ConsumePinch(OVRHand hand)
        {
            if (hand == null || !hand.IsTracked) return false;

            var pinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            _wasPinching.TryGetValue(hand, out var wasPinching);
            _wasPinching[hand] = pinching;
            return pinching && !wasPinching;
        }
    }
}
