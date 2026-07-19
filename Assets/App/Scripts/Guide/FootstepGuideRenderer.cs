using System.Collections.Generic;
using System.Linq;
using TemaeTrainer.Calibration;
using TemaeTrainer.Sequence;
using TemaeTrainer.Sequence.Data;
using TMPro;
using UnityEngine;

namespace TemaeTrainer.Guide
{
    // F1-4: draws the current step's footsteps as flat markers on the calibrated tatami
    // (L=blue, R=red, per SPEC §7.1 color+shape distinction), with an order number and a
    // pulse on whichever marker(s) share the lowest "order" (the "次に踏む足" the spec calls
    // for). Quest has no foot tracking, so there is no notion of "already stepped on" within a
    // step -- the whole step's footsteps are shown at once and "next" means "first in this
    // step's order", not "next unconfirmed step globally". Subscribes to
    // TemaeSequenceHost/CalibrationController, never mutates their state (one-way data flow).
    public class FootstepGuideRenderer : MonoBehaviour
    {
        [SerializeField] private TemaeSequenceHost sequenceHost;
        [SerializeField] private CalibrationController calibration;
        [SerializeField] private Color leftColor = new(0.2f, 0.4f, 1f, 0.85f);
        [SerializeField] private Color rightColor = new(1f, 0.25f, 0.2f, 0.85f);
        [SerializeField] private float markerSize = 0.12f;
        [SerializeField] private float labelHeight = 0.15f;
        [SerializeField] private float pulseSpeed = 4f;
        [SerializeField] private float pulseAmount = 0.15f;

        private readonly Dictionary<string, TatamiFrame> _frames = new();
        private readonly List<GameObject> _activeMarkers = new();
        private readonly List<Transform> _nextMarkers = new();
        private readonly List<Transform> _labels = new();
        private StepData _currentStep;
        private float _pulsePhase;

        private void Awake()
        {
            if (sequenceHost != null) sequenceHost.StepChanged += OnStepChanged;
            if (calibration != null) calibration.TatamiFrameReady += OnTatamiFrameReady;
        }

        private void OnDestroy()
        {
            if (sequenceHost != null) sequenceHost.StepChanged -= OnStepChanged;
            if (calibration != null) calibration.TatamiFrameReady -= OnTatamiFrameReady;
        }

        private void Update()
        {
            if (_nextMarkers.Count == 0) return;

            _pulsePhase += Time.deltaTime * pulseSpeed;
            var scale = 1f + Mathf.Sin(_pulsePhase) * pulseAmount;
            foreach (var marker in _nextMarkers) marker.localScale = new Vector3(markerSize, markerSize, 1f) * scale;

            var camera = Camera.main;
            if (camera == null) return;
            foreach (var label in _labels)
                label.rotation = Quaternion.LookRotation(label.position - camera.transform.position, Vector3.up);
        }

        private void OnStepChanged(StepChangedEventArgs args)
        {
            _currentStep = args.Step;
            Rebuild();
        }

        private void OnTatamiFrameReady(string role, TatamiFrame frame)
        {
            _frames[role] = frame;
            Rebuild();
        }

        private void Rebuild()
        {
            foreach (var marker in _activeMarkers) Destroy(marker);
            _activeMarkers.Clear();
            _nextMarkers.Clear();
            _labels.Clear();
            _pulsePhase = 0f;

            var footsteps = _currentStep?.Footsteps;
            if (footsteps == null || footsteps.Count == 0) return;

            var minOrder = footsteps.Min(f => f.Order);
            foreach (var footstep in footsteps)
            {
                if (!_frames.TryGetValue(footstep.Tatami, out var frame)) continue;
                CreateMarker(frame, footstep, footstep.Order == minOrder);
            }
        }

        private void CreateMarker(TatamiFrame frame, FootstepData footstep, bool isNext)
        {
            var worldPos = frame.GetWorldPoint(footstep.U, footstep.V) + Vector3.up * 0.001f;
            var color = footstep.Foot == "L" ? leftColor : rightColor;

            var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.name = $"Footstep_{footstep.Foot}{footstep.Order}";
            Destroy(marker.GetComponent<Collider>());
            marker.transform.SetParent(transform, false);
            marker.transform.position = worldPos;
            marker.transform.rotation = Quaternion.Euler(90f, frame.YawDeg + footstep.YawDeg, 0f);
            marker.transform.localScale = new Vector3(markerSize, markerSize, 1f);

            var renderer = marker.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { color = color };

            _activeMarkers.Add(marker);
            if (isNext) _nextMarkers.Add(marker.transform);

            // Parented to this renderer, not the marker: the marker's localScale pulses (isNext
            // markers), and a child would inherit that non-uniform scale onto its own text mesh.
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.position = worldPos + Vector3.up * labelHeight;
            var label = labelGo.AddComponent<TextMeshPro>();
            label.text = footstep.Order.ToString();
            label.fontSize = 4f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;

            _activeMarkers.Add(labelGo);
            _labels.Add(labelGo.transform);
        }
    }
}
