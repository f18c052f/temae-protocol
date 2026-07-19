using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace TemaeTrainer.Calibration.Floor
{
    // Scene-Capture-backed floor detection. Registering the callback in Awake is safe even if
    // the scene model is already loaded: MRUK.RegisterSceneLoadedCallback fires immediately in
    // that case. If no room is ever found (e.g. Simulator without a synthetic environment that
    // carries scene data), TryProjectToFloor/TryGetFloorY simply keep returning false, and
    // FallbackChainFloorProvider is expected to take over.
    public class MrukFloorProvider : MonoBehaviour, IFloorProvider
    {
        private MRUKRoom _room;

        private void Awake()
        {
            MRUK.Instance?.RegisterSceneLoadedCallback(OnSceneLoaded);
        }

        private void OnSceneLoaded()
        {
            _room = MRUK.Instance.GetCurrentRoom();
        }

        public bool TryProjectToFloor(Vector3 approxWorldPoint, out Vector3 floorWorldPoint)
        {
            floorWorldPoint = approxWorldPoint;
            if (_room == null || _room.FloorAnchors.Count == 0) return false;

            var ray = new Ray(approxWorldPoint + Vector3.up * 0.5f, Vector3.down);
            var filter = new LabelFilter(MRUKAnchor.SceneLabels.FLOOR);
            if (!_room.Raycast(ray, 2f, filter, out var hit, out _)) return false;

            floorWorldPoint = hit.point;
            return true;
        }

        public bool TryGetFloorY(out float floorY)
        {
            floorY = 0f;
            if (_room == null || _room.FloorAnchors.Count == 0) return false;

            floorY = _room.FloorAnchors[0].transform.position.y;
            return true;
        }
    }
}
