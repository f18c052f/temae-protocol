using System;
using System.Collections.Generic;
using TemaeTrainer.Sequence.Data;
using UnityEngine;

namespace TemaeTrainer.Calibration
{
    public enum CalibrationStage
    {
        PickingBaseCorner,
        PreviewingApproach,
        Complete
    }

    // Sole holder of calibration state (mirrors TemaeSequence's role for step state): drives
    // the F1-2 flow of picking the temae tatami's 4 corners, then previewing/correcting each
    // adjacent tatami in calibration.layout order. Guide/UI subscribe to TatamiFrameReady /
    // CalibrationComplete; only CalibrationController (fed by real hand input) calls the
    // mutating methods below.
    public class CalibrationSession
    {
        private readonly CalibrationConfig _config;
        private readonly List<Vector3> _baseCorners = new();
        private readonly Dictionary<string, TatamiFrame> _frames = new();
        private Queue<TatamiLayoutEntry> _pendingLayout;
        private TatamiLayoutEntry _currentLayoutEntry;

        public CalibrationSession(CalibrationConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _pendingLayout = new Queue<TatamiLayoutEntry>(_config.Layout ?? new List<TatamiLayoutEntry>());
            Stage = CalibrationStage.PickingBaseCorner;
        }

        public CalibrationStage Stage { get; private set; }
        public int BaseCornersPicked => _baseCorners.Count;
        public TatamiLayoutEntry CurrentLayoutEntry => _currentLayoutEntry;
        public IReadOnlyDictionary<string, TatamiFrame> Frames => _frames;

        public event Action<CalibrationStage> StageChanged;
        public event Action<string, TatamiFrame> TatamiFrameReady;
        public event Action CalibrationComplete;

        public void SubmitBaseCornerPoint(Vector3 worldPoint)
        {
            if (Stage != CalibrationStage.PickingBaseCorner) return;

            _baseCorners.Add(worldPoint);
            if (_baseCorners.Count < 4) return;

            var frame = TatamiGridMath.FitRectangle("temae", _baseCorners.ToArray(), _config.TatamiSize.Length, _config.TatamiSize.Width);
            _frames["temae"] = frame;
            TatamiFrameReady?.Invoke("temae", frame);
            AdvanceToNextLayoutEntryOrComplete();
        }

        public void ConfirmApproachPreview()
        {
            if (Stage != CalibrationStage.PreviewingApproach) return;
            AdvanceToNextLayoutEntryOrComplete();
        }

        public void CorrectApproachByEdgePoint(Vector3 worldPoint)
        {
            if (Stage != CalibrationStage.PreviewingApproach || _currentLayoutEntry == null) return;

            var corrected = TatamiGridMath.CorrectByEdgePoint(_frames[_currentLayoutEntry.Id], worldPoint);
            _frames[_currentLayoutEntry.Id] = corrected;
            TatamiFrameReady?.Invoke(_currentLayoutEntry.Id, corrected);
        }

        public void ToggleApproachOrientation()
        {
            if (Stage != CalibrationStage.PreviewingApproach || _currentLayoutEntry == null) return;

            var toggled = TatamiGridMath.ToggleOrientation(_frames[_currentLayoutEntry.Id]);
            _frames[_currentLayoutEntry.Id] = toggled;
            TatamiFrameReady?.Invoke(_currentLayoutEntry.Id, toggled);
        }

        public void Restart()
        {
            _baseCorners.Clear();
            _frames.Clear();
            _pendingLayout = new Queue<TatamiLayoutEntry>(_config.Layout ?? new List<TatamiLayoutEntry>());
            _currentLayoutEntry = null;
            SetStage(CalibrationStage.PickingBaseCorner);
        }

        private void AdvanceToNextLayoutEntryOrComplete()
        {
            if (_pendingLayout.Count == 0)
            {
                _currentLayoutEntry = null;
                SetStage(CalibrationStage.Complete);
                CalibrationComplete?.Invoke();
                return;
            }

            _currentLayoutEntry = _pendingLayout.Dequeue();
            var reference = _frames[_currentLayoutEntry.AdjacentTo];
            var preview = TatamiGridMath.ExtrapolateAdjacent(reference, _currentLayoutEntry, _config.TatamiSize.Length, _config.TatamiSize.Width);
            _frames[_currentLayoutEntry.Id] = preview;
            TatamiFrameReady?.Invoke(_currentLayoutEntry.Id, preview);
            SetStage(CalibrationStage.PreviewingApproach);
        }

        private void SetStage(CalibrationStage stage)
        {
            Stage = stage;
            StageChanged?.Invoke(stage);
        }
    }
}
