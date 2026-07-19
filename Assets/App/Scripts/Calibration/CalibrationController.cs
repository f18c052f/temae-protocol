using System;
using TemaeTrainer.Sequence;
using UnityEngine;

namespace TemaeTrainer.Calibration
{
    // Thin MonoBehaviour: creates CalibrationSession once the temae document's calibration
    // config is available, and translates ICalibrationInputSource triggers into session method
    // calls. All calibration state/logic lives in CalibrationSession itself.
    public class CalibrationController : MonoBehaviour
    {
        [SerializeField] private TemaeSequenceHost sequenceHost;
        [SerializeField] private MonoBehaviour inputSourceBehaviour;

        public CalibrationSession Session { get; private set; }

        public event Action<string, TatamiFrame> TatamiFrameReady;
        public event Action CalibrationComplete;

        private ICalibrationInputSource InputSource => inputSourceBehaviour as ICalibrationInputSource;

        private void Update()
        {
            if (Session == null)
            {
                TryCreateSession();
                return;
            }

            if (Session.Stage == CalibrationStage.Complete) return;

            var input = InputSource;
            if (input == null) return;

            switch (Session.Stage)
            {
                case CalibrationStage.PickingBaseCorner:
                    if (input.ConsumeConfirmTrigger() && input.TryGetCandidatePoint(out var corner))
                        Session.SubmitBaseCornerPoint(corner);
                    break;

                case CalibrationStage.PreviewingApproach:
                    if (input.ConsumeToggleTrigger())
                        Session.ToggleApproachOrientation();
                    else if (input.ConsumeCorrectionTrigger() && input.TryGetCandidatePoint(out var edgePoint))
                        Session.CorrectApproachByEdgePoint(edgePoint);
                    else if (input.ConsumeConfirmTrigger())
                        Session.ConfirmApproachPreview();
                    break;
            }
        }

        private void TryCreateSession()
        {
            if (sequenceHost == null || sequenceHost.Document?.Calibration == null) return;

            Session = new CalibrationSession(sequenceHost.Document.Calibration);
            Session.TatamiFrameReady += (role, frame) => TatamiFrameReady?.Invoke(role, frame);
            Session.CalibrationComplete += () => CalibrationComplete?.Invoke();
        }
    }
}
