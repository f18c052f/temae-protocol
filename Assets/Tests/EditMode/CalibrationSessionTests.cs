using System.Collections.Generic;
using NUnit.Framework;
using TemaeTrainer.Calibration;
using TemaeTrainer.Sequence.Data;
using UnityEngine;

namespace TemaeTrainer.Tests
{
    public class CalibrationSessionTests
    {
        private static CalibrationConfig BuildConfig()
        {
            return new CalibrationConfig
            {
                TatamiSize = new TatamiSizeData { Length = 1.76f, Width = 0.88f },
                RequiredTatami = new List<string> { "temae", "approach1" },
                Layout = new List<TatamiLayoutEntry>
                {
                    new() { Id = "approach1", AdjacentTo = "temae", Edge = TatamiGridMath.EdgeU1, Rotated90 = false }
                }
            };
        }

        private static readonly Vector3[] BaseCorners =
        {
            new(0f, 0f, 0f),
            new(1.76f, 0f, 0f),
            new(1.76f, 0f, 0.88f),
            new(0f, 0f, 0.88f)
        };

        [Test]
        public void InitialStage_IsPickingBaseCorner()
        {
            var session = new CalibrationSession(BuildConfig());
            Assert.AreEqual(CalibrationStage.PickingBaseCorner, session.Stage);
            Assert.AreEqual(0, session.BaseCornersPicked);
        }

        [Test]
        public void SubmitBaseCornerPoint_FirstThreeCorners_StaysInPickingStage()
        {
            var session = new CalibrationSession(BuildConfig());
            for (var i = 0; i < 3; i++) session.SubmitBaseCornerPoint(BaseCorners[i]);

            Assert.AreEqual(CalibrationStage.PickingBaseCorner, session.Stage);
            Assert.AreEqual(3, session.BaseCornersPicked);
            Assert.IsFalse(session.Frames.ContainsKey("temae"));
        }

        [Test]
        public void SubmitBaseCornerPoint_FourthCorner_ProducesTemaeFrameAndAdvancesToPreview()
        {
            var session = new CalibrationSession(BuildConfig());
            string readyRole = null;
            session.TatamiFrameReady += (role, _) => readyRole = role;

            foreach (var corner in BaseCorners) session.SubmitBaseCornerPoint(corner);

            Assert.AreEqual(CalibrationStage.PreviewingApproach, session.Stage);
            Assert.IsTrue(session.Frames.ContainsKey("temae"));
            Assert.AreEqual("approach1", session.CurrentLayoutEntry.Id);
            Assert.AreEqual("approach1", readyRole); // last frame-ready call was for the preview
            Assert.IsTrue(session.Frames.ContainsKey("approach1"));
        }

        [Test]
        public void CorrectApproachByEdgePoint_UpdatesApproachFrameOnly()
        {
            var session = new CalibrationSession(BuildConfig());
            foreach (var corner in BaseCorners) session.SubmitBaseCornerPoint(corner);
            var temaeBefore = session.Frames["temae"];
            var approachBefore = session.Frames["approach1"];

            // approach1's u=0 edge sits at temae's u=1 edge (x=1.76); nudge it 0.05m further out.
            session.CorrectApproachByEdgePoint(new Vector3(1.81f, 0f, 0.44f));

            Assert.AreEqual(CalibrationStage.PreviewingApproach, session.Stage);
            Assert.AreSame(temaeBefore, session.Frames["temae"]);
            Assert.AreNotSame(approachBefore, session.Frames["approach1"]);
        }

        [Test]
        public void ToggleApproachOrientation_ChangesApproachFrameOnly()
        {
            var session = new CalibrationSession(BuildConfig());
            foreach (var corner in BaseCorners) session.SubmitBaseCornerPoint(corner);
            var yawBefore = session.Frames["approach1"].YawDeg;

            session.ToggleApproachOrientation();

            Assert.AreEqual(yawBefore + 90f, session.Frames["approach1"].YawDeg, 0.01f);
        }

        [Test]
        public void ConfirmApproachPreview_LastLayoutEntry_CompletesCalibration()
        {
            var session = new CalibrationSession(BuildConfig());
            var completed = false;
            session.CalibrationComplete += () => completed = true;
            foreach (var corner in BaseCorners) session.SubmitBaseCornerPoint(corner);

            session.ConfirmApproachPreview();

            Assert.AreEqual(CalibrationStage.Complete, session.Stage);
            Assert.IsTrue(completed);
            Assert.IsNull(session.CurrentLayoutEntry);
        }

        [Test]
        public void Restart_ClearsFramesAndReturnsToPickingStage()
        {
            var session = new CalibrationSession(BuildConfig());
            foreach (var corner in BaseCorners) session.SubmitBaseCornerPoint(corner);
            session.ConfirmApproachPreview();
            Assert.AreEqual(CalibrationStage.Complete, session.Stage);

            session.Restart();

            Assert.AreEqual(CalibrationStage.PickingBaseCorner, session.Stage);
            Assert.AreEqual(0, session.BaseCornersPicked);
            Assert.AreEqual(0, session.Frames.Count);
        }

        [Test]
        public void SubmitBaseCornerPoint_AfterAlreadyInPreviewStage_IsIgnored()
        {
            var session = new CalibrationSession(BuildConfig());
            foreach (var corner in BaseCorners) session.SubmitBaseCornerPoint(corner);
            Assert.AreEqual(CalibrationStage.PreviewingApproach, session.Stage);

            session.SubmitBaseCornerPoint(new Vector3(5f, 0f, 5f));

            Assert.AreEqual(CalibrationStage.PreviewingApproach, session.Stage);
        }
    }
}
