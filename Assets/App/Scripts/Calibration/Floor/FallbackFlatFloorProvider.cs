using UnityEngine;

namespace TemaeTrainer.Calibration.Floor
{
    // Always succeeds: treats the Y of the first point it is ever given as "the floor".
    // This is what makes virtual-tatami mode (and MRUK-less Simulator sessions) work without
    // any special-casing elsewhere in the calibration flow.
    public class FallbackFlatFloorProvider : IFloorProvider
    {
        private float? _assumedFloorY;

        public bool TryProjectToFloor(Vector3 approxWorldPoint, out Vector3 floorWorldPoint)
        {
            _assumedFloorY ??= approxWorldPoint.y;
            floorWorldPoint = new Vector3(approxWorldPoint.x, _assumedFloorY.Value, approxWorldPoint.z);
            return true;
        }

        public bool TryGetFloorY(out float floorY)
        {
            floorY = _assumedFloorY ?? 0f;
            return _assumedFloorY.HasValue;
        }
    }
}
