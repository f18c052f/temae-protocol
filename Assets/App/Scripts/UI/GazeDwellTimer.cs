using System;
using UnityEngine;

namespace TemaeTrainer.UI
{
    // Plain C# dwell-progress accumulator, kept separate from GazeDwellButton so the fire/reset
    // logic is unit-testable without a Camera or Collider (mirrors CalibrationSession's split
    // from CalibrationController). Fires once per continuous gaze; must look away and back to
    // fire again.
    public class GazeDwellTimer
    {
        private readonly float _dwellSeconds;
        private float _elapsed;
        private bool _hasFiredForCurrentGaze;

        public GazeDwellTimer(float dwellSeconds)
        {
            if (dwellSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(dwellSeconds));
            _dwellSeconds = dwellSeconds;
        }

        public float Progress { get; private set; }

        // Returns true on the single frame the dwell threshold is crossed.
        public bool Tick(bool isGazing, float deltaTime)
        {
            if (!isGazing)
            {
                _elapsed = 0f;
                Progress = 0f;
                _hasFiredForCurrentGaze = false;
                return false;
            }

            _elapsed += deltaTime;
            Progress = Mathf.Clamp01(_elapsed / _dwellSeconds);

            if (_hasFiredForCurrentGaze || _elapsed < _dwellSeconds) return false;

            _hasFiredForCurrentGaze = true;
            return true;
        }
    }
}
