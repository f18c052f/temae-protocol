using UnityEngine;

namespace TemaeTrainer.Calibration.Floor
{
    // Abstraction over "where is the floor" so calibration input works identically whether
    // MRUK scene data is available (real tatami mode) or not (virtual tatami mode, or a
    // Simulator session with no synthetic environment loaded yet). See CLAUDE.md Phase1 plan.
    public interface IFloorProvider
    {
        bool TryProjectToFloor(Vector3 approxWorldPoint, out Vector3 floorWorldPoint);
        bool TryGetFloorY(out float floorY);
    }
}
