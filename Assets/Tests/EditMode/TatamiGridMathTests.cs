using NUnit.Framework;
using TemaeTrainer.Calibration;
using TemaeTrainer.Sequence.Data;
using UnityEngine;

namespace TemaeTrainer.Tests
{
    public class TatamiGridMathTests
    {
        private const float Tolerance = 0.01f;

        private static void AssertVectorApprox(Vector3 expected, Vector3 actual, float tolerance = Tolerance)
        {
            Assert.AreEqual(expected.x, actual.x, tolerance, $"x mismatch: expected {expected}, got {actual}");
            Assert.AreEqual(expected.y, actual.y, tolerance, $"y mismatch: expected {expected}, got {actual}");
            Assert.AreEqual(expected.z, actual.z, tolerance, $"z mismatch: expected {expected}, got {actual}");
        }

        [Test]
        public void FitRectangle_AxisAlignedCorners_RecoversDimensionsAndOrigin()
        {
            // C00=(0,0,0), C10=(1.76,0,0), C11=(1.76,0,0.88), C01=(0,0,0.88)
            var corners = new[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(1.76f, 0f, 0f),
                new Vector3(1.76f, 0f, 0.88f),
                new Vector3(0f, 0f, 0.88f)
            };

            var frame = TatamiGridMath.FitRectangle("temae", corners, 1.76f, 0.88f);

            Assert.AreEqual(1.76f, frame.Length, Tolerance);
            Assert.AreEqual(0.88f, frame.Width, Tolerance);
            AssertVectorApprox(corners[0], frame.OriginPosition);
            AssertVectorApprox(corners[1], frame.GetWorldPoint(1f, 0f));
            AssertVectorApprox(corners[2], frame.GetWorldPoint(1f, 1f));
            AssertVectorApprox(corners[3], frame.GetWorldPoint(0f, 1f));
        }

        [Test]
        public void FitRectangle_RotatedCorners_RecoversRotatedFrame()
        {
            var rotation = Quaternion.Euler(0f, 37f, 0f);
            var origin = new Vector3(2f, 0.85f, 3f);
            const float length = 1.76f;
            const float width = 0.88f;
            var uAxis = rotation * Vector3.right;
            var vAxis = rotation * Vector3.forward;
            var corners = new[]
            {
                origin,
                origin + uAxis * length,
                origin + uAxis * length + vAxis * width,
                origin + vAxis * width
            };

            var frame = TatamiGridMath.FitRectangle("temae", corners, length, width);

            Assert.AreEqual(length, frame.Length, Tolerance);
            Assert.AreEqual(width, frame.Width, Tolerance);
            for (var i = 0; i < 4; i++)
                AssertVectorApprox(corners[i], frame.GetCornerPointsWorld()[i], 0.02f);
        }

        [Test]
        public void FitRectangle_NoisyCorners_StaysWithinToleranceOfIdealRectangle()
        {
            var noise = new[]
            {
                new Vector3(0.01f, 0f, -0.01f),
                new Vector3(-0.01f, 0f, 0.01f),
                new Vector3(0.01f, 0f, 0.01f),
                new Vector3(-0.01f, 0f, -0.01f)
            };
            var ideal = new[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(1.76f, 0f, 0f),
                new Vector3(1.76f, 0f, 0.88f),
                new Vector3(0f, 0f, 0.88f)
            };
            var noisy = new Vector3[4];
            for (var i = 0; i < 4; i++) noisy[i] = ideal[i] + noise[i];

            var frame = TatamiGridMath.FitRectangle("temae", noisy, 1.76f, 0.88f);

            Assert.AreEqual(1.76f, frame.Length, 0.05f);
            Assert.AreEqual(0.88f, frame.Width, 0.05f);
        }

        private static TatamiFrame ReferenceFrame() => new("temae", new Vector3(1f, 0.9f, 1f), 20f, 1.76f, 0.88f);

        [Test]
        public void ExtrapolateAdjacent_EdgeU1_SharesReferencesU1Edge()
        {
            var reference = ReferenceFrame();
            var layout = new TatamiLayoutEntry { Id = "approach1", AdjacentTo = "temae", Edge = TatamiGridMath.EdgeU1 };

            var adjacent = TatamiGridMath.ExtrapolateAdjacent(reference, layout, 1.76f, 0.88f);

            AssertVectorApprox(reference.GetWorldPoint(1f, 0f), adjacent.GetWorldPoint(0f, 0f));
            AssertVectorApprox(reference.GetWorldPoint(1f, 1f), adjacent.GetWorldPoint(0f, 1f));
        }

        [Test]
        public void ExtrapolateAdjacent_EdgeU0_SharesReferencesU0Edge()
        {
            var reference = ReferenceFrame();
            var layout = new TatamiLayoutEntry { Id = "approach1", AdjacentTo = "temae", Edge = TatamiGridMath.EdgeU0 };

            var adjacent = TatamiGridMath.ExtrapolateAdjacent(reference, layout, 1.76f, 0.88f);

            AssertVectorApprox(reference.GetWorldPoint(0f, 0f), adjacent.GetWorldPoint(1f, 0f));
            AssertVectorApprox(reference.GetWorldPoint(0f, 1f), adjacent.GetWorldPoint(1f, 1f));
        }

        [Test]
        public void ExtrapolateAdjacent_EdgeV1_SharesReferencesV1Edge()
        {
            var reference = ReferenceFrame();
            var layout = new TatamiLayoutEntry { Id = "approach1", AdjacentTo = "temae", Edge = TatamiGridMath.EdgeV1 };

            var adjacent = TatamiGridMath.ExtrapolateAdjacent(reference, layout, 1.76f, 0.88f);

            AssertVectorApprox(reference.GetWorldPoint(0f, 1f), adjacent.GetWorldPoint(0f, 0f));
            AssertVectorApprox(reference.GetWorldPoint(1f, 1f), adjacent.GetWorldPoint(1f, 0f));
        }

        [Test]
        public void ExtrapolateAdjacent_EdgeV0_SharesReferencesV0Edge()
        {
            var reference = ReferenceFrame();
            var layout = new TatamiLayoutEntry { Id = "approach1", AdjacentTo = "temae", Edge = TatamiGridMath.EdgeV0 };

            var adjacent = TatamiGridMath.ExtrapolateAdjacent(reference, layout, 1.76f, 0.88f);

            AssertVectorApprox(reference.GetWorldPoint(0f, 0f), adjacent.GetWorldPoint(0f, 1f));
            AssertVectorApprox(reference.GetWorldPoint(1f, 0f), adjacent.GetWorldPoint(1f, 1f));
        }

        [Test]
        public void ExtrapolateAdjacent_Rotated90_SwapsLengthAndWidth()
        {
            var reference = ReferenceFrame();
            var layout = new TatamiLayoutEntry { Id = "approach1", AdjacentTo = "temae", Edge = TatamiGridMath.EdgeU1, Rotated90 = true };

            var adjacent = TatamiGridMath.ExtrapolateAdjacent(reference, layout, 1.76f, 0.88f);

            Assert.AreEqual(0.88f, adjacent.Length, Tolerance);
            Assert.AreEqual(1.76f, adjacent.Width, Tolerance);
        }

        [Test]
        public void CorrectByEdgePoint_NudgesTowardNearestEdgeOnly()
        {
            var frame = new TatamiFrame("temae", Vector3.zero, 0f, 1.76f, 0.88f);
            // Tap 0.05m beyond the u=1 edge (x=1.76), roughly centered in v -- the u1 edge is
            // the nearest of the 4.
            var tapped = new Vector3(1.81f, 0f, 0.44f);

            var corrected = TatamiGridMath.CorrectByEdgePoint(frame, tapped);

            AssertVectorApprox(new Vector3(1.81f, 0f, 0.44f), corrected.GetWorldPoint(1f, 0.5f), 0.02f);
            // Size/orientation must stay untouched -- only a translation happened.
            Assert.AreEqual(frame.Length, corrected.Length, Tolerance);
            Assert.AreEqual(frame.Width, corrected.Width, Tolerance);
            Assert.AreEqual(frame.YawDeg, corrected.YawDeg, Tolerance);
        }

        [Test]
        public void ToggleOrientation_KeepsOriginCornerFixed()
        {
            var frame = new TatamiFrame("approach1", new Vector3(2f, 0.9f, 3f), 15f, 1.76f, 0.88f);

            var toggled = TatamiGridMath.ToggleOrientation(frame);

            AssertVectorApprox(frame.OriginPosition, toggled.OriginPosition);
            Assert.AreEqual(frame.YawDeg + 90f, toggled.YawDeg, Tolerance);
        }

        [Test]
        public void ToggleOrientation_AppliedFourTimes_ReturnsToOriginalYaw()
        {
            var frame = new TatamiFrame("approach1", Vector3.zero, 10f, 1.76f, 0.88f);

            var result = frame;
            for (var i = 0; i < 4; i++) result = TatamiGridMath.ToggleOrientation(result);

            var normalizedOriginal = Mathf.Repeat(frame.YawDeg, 360f);
            var normalizedResult = Mathf.Repeat(result.YawDeg, 360f);
            Assert.AreEqual(normalizedOriginal, normalizedResult, Tolerance);
        }
    }
}
