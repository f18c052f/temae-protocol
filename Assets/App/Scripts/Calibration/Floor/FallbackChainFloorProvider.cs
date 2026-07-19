using UnityEngine;

namespace TemaeTrainer.Calibration.Floor
{
    // Tries the primary provider (typically MRUK-backed) first, falling back to a secondary
    // provider that always succeeds. This is what lets real-tatami and virtual-tatami modes
    // share one code path (SPEC §3): callers never branch on which mode is active.
    public class FallbackChainFloorProvider : IFloorProvider
    {
        private readonly IFloorProvider _primary;
        private readonly IFloorProvider _fallback;

        public FallbackChainFloorProvider(IFloorProvider primary, IFloorProvider fallback)
        {
            _primary = primary;
            _fallback = fallback;
        }

        public bool TryProjectToFloor(Vector3 approxWorldPoint, out Vector3 floorWorldPoint) =>
            _primary.TryProjectToFloor(approxWorldPoint, out floorWorldPoint) ||
            _fallback.TryProjectToFloor(approxWorldPoint, out floorWorldPoint);

        public bool TryGetFloorY(out float floorY) =>
            _primary.TryGetFloorY(out floorY) ||
            _fallback.TryGetFloorY(out floorY);
    }
}
