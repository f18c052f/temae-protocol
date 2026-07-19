using System;
using System.Collections.Generic;
using TemaeTrainer.Sequence.Data;

namespace TemaeTrainer.Sequence
{
    public readonly struct StepChangedEventArgs
    {
        public SectionData Section { get; }
        public StepData Step { get; }
        public int StepIndex { get; }
        public int StepCount { get; }
        public bool IsAtStart { get; }
        public bool IsAtEnd { get; }

        public StepChangedEventArgs(SectionData section, StepData step, int stepIndex, int stepCount, bool isAtStart, bool isAtEnd)
        {
            Section = section;
            Step = step;
            StepIndex = stepIndex;
            StepCount = stepCount;
            IsAtStart = isAtStart;
            IsAtEnd = isAtEnd;
        }
    }

    // Sole holder of "current step" state (CLAUDE.md one-way data flow). Guide/UI subscribe to
    // StepChanged; they must never mutate this class's state directly, only call Next/Previous/
    // RestartFromBeginning. Section jumping (F2-4) is out of scope for Phase 1.
    public class TemaeSequence
    {
        private readonly List<(SectionData Section, StepData Step)> _flattened = new();
        private int _index;

        public TemaeSequence(TemaeDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            foreach (var section in document.Sections ?? new List<SectionData>())
            foreach (var step in section.Steps ?? new List<StepData>())
                _flattened.Add((section, step));

            if (_flattened.Count == 0)
                throw new ArgumentException("document must contain at least one step", nameof(document));

            _index = 0;
        }

        public event Action<StepChangedEventArgs> StepChanged;

        public SectionData CurrentSection => _flattened[_index].Section;
        public StepData CurrentStep => _flattened[_index].Step;
        public int StepIndex => _index;
        public int StepCount => _flattened.Count;
        public bool IsAtStart => _index == 0;
        public bool IsAtEnd => _index == _flattened.Count - 1;

        public void Next()
        {
            if (IsAtEnd) return;
            _index++;
            RaiseStepChanged();
        }

        public void Previous()
        {
            if (IsAtStart) return;
            _index--;
            RaiseStepChanged();
        }

        public void RestartFromBeginning()
        {
            _index = 0;
            RaiseStepChanged();
        }

        private void RaiseStepChanged()
        {
            StepChanged?.Invoke(new StepChangedEventArgs(CurrentSection, CurrentStep, _index, _flattened.Count, IsAtStart, IsAtEnd));
        }
    }
}
