using System.Collections.Generic;
using TemaeTrainer.Calibration.Floor;
using UnityEngine;

namespace TemaeTrainer.Calibration
{
    // Concrete ICalibrationInputSource backed by Meta hand tracking. Aiming uses the index
    // fingertip (projected to the floor via IFloorProvider) rather than OVRHand.PointerPose,
    // since the user is pointing at a nearby floor corner while seated, not aiming a long ray.
    //
    // Finger-to-action mapping (confirm=pointingHand index, correct=pointingHand middle,
    // toggle=toggleHand index) is provisional -- see Docs/DEVICE_CHECKLIST.md, this needs
    // real-device tuning for seated ergonomics.
    public class OVRHandCalibrationInputSource : MonoBehaviour, ICalibrationInputSource
    {
        [SerializeField] private OVRHand pointingHand;
        [SerializeField] private OVRSkeleton pointingSkeleton;
        [SerializeField] private OVRHand toggleHand;
        [SerializeField] private MonoBehaviour floorProviderBehaviour;

        private readonly Dictionary<(OVRHand hand, OVRHand.HandFinger finger), bool> _wasPinching = new();

        private IFloorProvider FloorProvider => floorProviderBehaviour as IFloorProvider;

        public bool TryGetCandidatePoint(out Vector3 worldPoint)
        {
            worldPoint = default;
            if (pointingSkeleton == null || !pointingSkeleton.IsDataValid || FloorProvider == null) return false;

            foreach (var bone in pointingSkeleton.Bones)
            {
                if (bone.Id != OVRSkeleton.BoneId.Hand_IndexTip && bone.Id != OVRSkeleton.BoneId.XRHand_IndexTip)
                    continue;
                return FloorProvider.TryProjectToFloor(bone.Transform.position, out worldPoint);
            }

            return false;
        }

        public bool ConsumeConfirmTrigger() => ConsumePinch(pointingHand, OVRHand.HandFinger.Index);
        public bool ConsumeCorrectionTrigger() => ConsumePinch(pointingHand, OVRHand.HandFinger.Middle);
        public bool ConsumeToggleTrigger() => ConsumePinch(toggleHand, OVRHand.HandFinger.Index);

        private bool ConsumePinch(OVRHand hand, OVRHand.HandFinger finger)
        {
            if (hand == null || !hand.IsTracked) return false;

            var key = (hand, finger);
            var pinching = hand.GetFingerIsPinching(finger);
            _wasPinching.TryGetValue(key, out var wasPinching);
            _wasPinching[key] = pinching;
            return pinching && !wasPinching;
        }
    }
}
