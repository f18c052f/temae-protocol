using System;
using UnityEngine;

namespace TemaeTrainer.UI
{
    // Thin MonoBehaviour: "is the head-gaze ray currently hitting this button's collider",
    // fed into GazeDwellTimer each frame. SPEC §7.1 operation-system ①(注視ドウェル・主).
    public class GazeDwellButton : MonoBehaviour
    {
        [SerializeField] private float dwellSeconds = 1f;
        [SerializeField] private float maxDistance = 5f;
        [SerializeField] private Collider targetCollider;

        private GazeDwellTimer _timer;

        public event Action Fired;
        public float Progress => _timer?.Progress ?? 0f;

        private void Awake()
        {
            _timer = new GazeDwellTimer(dwellSeconds);
            if (targetCollider == null) targetCollider = GetComponent<Collider>();
        }

        private void Update()
        {
            var camera = Camera.main;
            var isGazing = camera != null && targetCollider != null
                && Physics.Raycast(camera.transform.position, camera.transform.forward, out var hit, maxDistance)
                && hit.collider == targetCollider;

            if (_timer.Tick(isGazing, Time.deltaTime)) Fired?.Invoke();
        }
    }
}
