using System;
using TemaeTrainer.Sequence.Data;
using UnityEngine;

namespace TemaeTrainer.Calibration
{
    public static class TatamiGridMath
    {
        public const string EdgeU0 = "u0";
        public const string EdgeU1 = "u1";
        public const string EdgeV0 = "v0";
        public const string EdgeV1 = "v1";

        // cornersInPickOrder: exactly 4 points, picked walking around the tatami's perimeter
        // starting at the origin corner: (u=0,v=0) -> (u=1,v=0) -> (u=1,v=1) -> (u=0,v=1)
        // (SPEC §6.1 leaves the origin corner "TBD"; this project defines it as "whichever
        // corner the user picks first", with CalibrationHintText telling them which physical
        // corner to start at and which way to go).
        public static TatamiFrame FitRectangle(string role, Vector3[] cornersInPickOrder, float presetLength, float presetWidth)
        {
            if (cornersInPickOrder == null || cornersInPickOrder.Length != 4)
                throw new ArgumentException("exactly 4 corners are required", nameof(cornersInPickOrder));

            var p0 = Flatten(cornersInPickOrder[0]);
            var p1 = Flatten(cornersInPickOrder[1]);
            var p2 = Flatten(cornersInPickOrder[2]);
            var p3 = Flatten(cornersInPickOrder[3]);

            var uEdgeA = p1 - p0;
            var uEdgeB = p2 - p3;
            var vEdgeA = p3 - p0;
            var vEdgeB = p2 - p1;

            var measuredLength = (uEdgeA.magnitude + uEdgeB.magnitude) * 0.5f;
            var measuredWidth = (vEdgeA.magnitude + vEdgeB.magnitude) * 0.5f;
            if (measuredLength < 0.01f) measuredLength = presetLength;
            if (measuredWidth < 0.01f) measuredWidth = presetWidth;

            var uDir = (uEdgeA.normalized + uEdgeB.normalized).normalized;
            // FromToRotation(right, uDir) is guaranteed to be a pure Y-axis rotation here
            // because both vectors are flattened to the horizontal plane, so extracting yaw
            // this way stays consistent with TatamiFrame.UAxis without hand-deriving trig signs.
            var yawDeg = Quaternion.FromToRotation(Vector3.right, uDir).eulerAngles.y;

            return new TatamiFrame(role, cornersInPickOrder[0], yawDeg, measuredLength, measuredWidth);
        }

        // Places a new tatami flush against one edge of an already-calibrated reference
        // tatami, per calibration.layout in the JSON (adjacentTo/edge/rotated90). Used to
        // generate the extrapolated preview that the user then confirms or nudges.
        public static TatamiFrame ExtrapolateAdjacent(TatamiFrame reference, TatamiLayoutEntry layout, float length, float width)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (layout == null) throw new ArgumentNullException(nameof(layout));

            var placedLength = layout.Rotated90 ? width : length;
            var placedWidth = layout.Rotated90 ? length : width;
            var yawDeg = reference.YawDeg + (layout.Rotated90 ? 90f : 0f);

            Vector3 originCorner = layout.Edge switch
            {
                EdgeU1 => reference.GetWorldPoint(1f, 0f),
                EdgeU0 => reference.OriginPosition - reference.UAxis * placedLength,
                EdgeV1 => reference.GetWorldPoint(0f, 1f),
                EdgeV0 => reference.OriginPosition - reference.VAxis * placedWidth,
                _ => throw new ArgumentException($"unknown edge \"{layout.Edge}\"", nameof(layout))
            };

            return new TatamiFrame(layout.Id, originCorner, yawDeg, placedLength, placedWidth);
        }

        // "縁上の点を1点タップして補正": nudges the frame along the normal of whichever of its
        // 4 edges is nearest the tapped point, so that edge passes through it. One degree of
        // freedom only -- size and orientation are left untouched (provisional decision; see
        // Docs plan).
        public static TatamiFrame CorrectByEdgePoint(TatamiFrame frame, Vector3 tappedWorldPoint)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));

            var tapped = Flatten(tappedWorldPoint);
            var corners = frame.GetCornerPointsWorld(); // [C00, C10, C11, C01]
            var uAxis = frame.UAxis;
            var vAxis = frame.VAxis;

            var distU0 = Vector3.Dot(tapped - corners[0], -uAxis);
            var distU1 = Vector3.Dot(tapped - corners[1], uAxis);
            var distV0 = Vector3.Dot(tapped - corners[0], -vAxis);
            var distV1 = Vector3.Dot(tapped - corners[3], vAxis);

            var offsetU0 = -uAxis * distU0;
            var offsetU1 = uAxis * distU1;
            var offsetV0 = -vAxis * distV0;
            var offsetV1 = vAxis * distV1;

            var bestOffset = offsetU0;
            var bestDist = Mathf.Abs(distU0);
            if (Mathf.Abs(distU1) < bestDist) { bestDist = Mathf.Abs(distU1); bestOffset = offsetU1; }
            if (Mathf.Abs(distV0) < bestDist) { bestDist = Mathf.Abs(distV0); bestOffset = offsetV0; }
            if (Mathf.Abs(distV1) < bestDist) { bestOffset = offsetV1; }

            return frame.WithOrigin(frame.OriginPosition + bestOffset);
        }

        // Rotates the tatami 90 degrees "in place". By construction, ExtrapolateAdjacent always
        // places a new frame's own origin corner (u=0,v=0) at the corner shared with its
        // neighbor, so pivoting around that shared corner is just a yaw change -- OriginPosition
        // does not need to move.
        public static TatamiFrame ToggleOrientation(TatamiFrame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            return frame.WithYawDeg(frame.YawDeg + 90f);
        }

        private static Vector3 Flatten(Vector3 v) => new(v.x, 0f, v.z);
    }
}
