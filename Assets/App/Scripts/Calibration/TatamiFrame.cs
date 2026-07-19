using UnityEngine;

namespace TemaeTrainer.Calibration
{
    // A tatami's local coordinate frame: OriginPosition is the (u=0,v=0) corner, u-axis runs
    // along the long edge (Length), v-axis along the short edge (Width). This is the project's
    // provisional convention for SPEC §6.1 (origin corner / axis meaning are not pinned down
    // in the spec itself); see Docs plan for the rationale.
    public class TatamiFrame
    {
        public string Role { get; }
        public Vector3 OriginPosition { get; }
        public float YawDeg { get; }
        public float Length { get; }
        public float Width { get; }

        public TatamiFrame(string role, Vector3 originPosition, float yawDeg, float length, float width)
        {
            Role = role;
            OriginPosition = originPosition;
            YawDeg = yawDeg;
            Length = length;
            Width = width;
        }

        public Vector3 UAxis => Quaternion.Euler(0f, YawDeg, 0f) * Vector3.right;
        public Vector3 VAxis => Quaternion.Euler(0f, YawDeg, 0f) * Vector3.forward;

        public Vector3 GetWorldPoint(float u, float v)
        {
            return OriginPosition + UAxis * (u * Length) + VAxis * (v * Width);
        }

        public Quaternion GetWorldRotation(float yawDegOffset = 0f)
        {
            return Quaternion.Euler(0f, YawDeg + yawDegOffset, 0f);
        }

        public Vector3[] GetCornerPointsWorld()
        {
            return new[]
            {
                GetWorldPoint(0f, 0f),
                GetWorldPoint(1f, 0f),
                GetWorldPoint(1f, 1f),
                GetWorldPoint(0f, 1f)
            };
        }

        public TatamiFrame WithOrigin(Vector3 newOrigin) => new(Role, newOrigin, YawDeg, Length, Width);
        public TatamiFrame WithYawDeg(float newYawDeg) => new(Role, OriginPosition, newYawDeg, Length, Width);
    }
}
