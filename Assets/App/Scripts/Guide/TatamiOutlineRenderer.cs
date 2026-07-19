using System.Collections.Generic;
using TemaeTrainer.Calibration;
using UnityEngine;

namespace TemaeTrainer.Guide
{
    // F1-3: draws each calibrated tatami's outline as a thin closed-loop line. Subscribes to
    // CalibrationController and never mutates calibration state (one-way data flow).
    public class TatamiOutlineRenderer : MonoBehaviour
    {
        [SerializeField] private CalibrationController calibration;
        [SerializeField] private Color lineColor = new(1f, 1f, 1f, 0.6f);
        [SerializeField] private float lineWidth = 0.01f;
        [SerializeField] private bool visible = true;

        private readonly Dictionary<string, LineRenderer> _outlines = new();

        private void Awake()
        {
            if (calibration != null) calibration.TatamiFrameReady += OnTatamiFrameReady;
        }

        private void OnDestroy()
        {
            if (calibration != null) calibration.TatamiFrameReady -= OnTatamiFrameReady;
        }

        public void SetVisible(bool value)
        {
            visible = value;
            foreach (var line in _outlines.Values) line.enabled = visible;
        }

        private void OnTatamiFrameReady(string role, TatamiFrame frame)
        {
            if (!_outlines.TryGetValue(role, out var line))
            {
                line = CreateLineRenderer(role);
                _outlines[role] = line;
            }

            var corners = frame.GetCornerPointsWorld();
            line.positionCount = 5;
            for (var i = 0; i < 4; i++) line.SetPosition(i, corners[i]);
            line.SetPosition(4, corners[0]);
            line.enabled = visible;
        }

        private LineRenderer CreateLineRenderer(string role)
        {
            var go = new GameObject($"TatamiOutline_{role}");
            go.transform.SetParent(transform, false);

            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.widthMultiplier = lineWidth;
            line.numCapVertices = 0;
            line.numCornerVertices = 0;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { color = lineColor };
            line.material = material;

            return line;
        }
    }
}
