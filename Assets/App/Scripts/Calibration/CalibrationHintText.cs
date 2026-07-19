using TMPro;
using UnityEngine;

namespace TemaeTrainer.Calibration
{
    // Minimal instructional text for the calibration procedure itself (which corner to pick,
    // what to do with the current preview). Not JSON-driven: this describes the calibration
    // process, not temae content, so it stays as plain UI copy per CLAUDE.md's data-driven rule.
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CalibrationHintText : MonoBehaviour
    {
        [SerializeField] private CalibrationController controller;

        private TextMeshProUGUI _text;

        private void Awake() => _text = GetComponent<TextMeshProUGUI>();

        private void Update()
        {
            if (controller == null || controller.Session == null)
            {
                _text.text = "キャリブレーション準備中...";
                return;
            }

            var session = controller.Session;
            _text.text = session.Stage switch
            {
                CalibrationStage.PickingBaseCorner =>
                    $"点前畳の角をピンチで指定してください({session.BaseCornersPicked + 1}/4)\n茶道口に近い下座角から時計回りに",
                CalibrationStage.PreviewingApproach =>
                    $"「{session.CurrentLayoutEntry?.Id}」のプレビューです\nずれていれば中指ピンチで縁を補正、向きが違えば左手ピンチで90度回転\n問題なければ右手ピンチで確定",
                CalibrationStage.Complete => "キャリブレーション完了",
                _ => string.Empty
            };
        }
    }
}
