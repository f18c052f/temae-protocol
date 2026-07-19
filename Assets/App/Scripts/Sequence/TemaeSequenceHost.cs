using System;
using TemaeTrainer.Sequence.Data;
using UnityEngine;

namespace TemaeTrainer.Sequence
{
    // Thin MonoBehaviour wrapper so Guide/UI components can [SerializeField] reference the
    // sequence from the Inspector. All state and step-advance logic lives in TemaeSequence.
    public class TemaeSequenceHost : MonoBehaviour
    {
        [SerializeField] private string dataFileName = "usucha_hiradate.json";

        public TemaeDocument Document { get; private set; }
        public TemaeSequence Sequence { get; private set; }

        public event Action<StepChangedEventArgs> StepChanged;

        // Kicked off from Start() (not Awake()) so that other components' Awake() calls have
        // already run and subscribed to StepChanged before this fires its first event.
        private async void Start()
        {
            try
            {
                var loader = new TemaeJsonLoader();
                Document = await loader.LoadFromStreamingAssetsAsync(dataFileName);
                Sequence = new TemaeSequence(Document);
                Sequence.StepChanged += OnInnerStepChanged;
                OnInnerStepChanged(new StepChangedEventArgs(
                    Sequence.CurrentSection, Sequence.CurrentStep, Sequence.StepIndex, Sequence.StepCount, Sequence.IsAtStart, Sequence.IsAtEnd));
            }
            catch (Exception ex)
            {
                Debug.LogError($"TemaeSequenceHost failed to load '{dataFileName}': {ex}");
            }
        }

        public void Next() => Sequence?.Next();
        public void Previous() => Sequence?.Previous();
        public void RestartFromBeginning() => Sequence?.RestartFromBeginning();

        private void OnInnerStepChanged(StepChangedEventArgs args) => StepChanged?.Invoke(args);
    }
}
