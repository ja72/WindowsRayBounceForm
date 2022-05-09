    using System;
using System.Drawing;
using System.Numerics;

namespace JA.World
{
    using JA.Gdi;

    public class Triangle : Polygon
    {
        public Triangle(Color color, Vector2 position, float angle, Vector2 a, Vector2 b, Vector2 c)
            : base(color, position,angle, a,b, c)
        { }

        #region Properties
        public Vector2 A { get => Nodes[0]; }
        public Vector2 B { get => Nodes[1]; }
        public Vector2 C { get => Nodes[2]; }

        public float Area => (A.Cross(B) + B.Cross(C) + C.Cross(A)) / 2;

        #endregion

        #region Interactions

        public new (float w_a, float w_b, float w_c) GetCoordOfPoint(Vector2 point)
        {
            point = ToLocal(point);

            float d = A.Cross(B) + B.Cross(C) + C.Cross(A);
            float w_a = point.Cross(B) + B.Cross(C) + C.Cross(point);
            float w_b = A.Cross(point) + point.Cross(C) + C.Cross(A);
            float w_c = A.Cross(B) + B.Cross(point) + point.Cross(A);

            return (w_a / d, w_b / d, w_c / d);
        }

        public Vector2 GetPointFromCoord(float w_a, float w_b, float w_c)
        {
            return FromLocal(w_a * A + w_b * B + w_c * C);
        }

        public override bool Contains(Vector2 point)
        {
            var (w_a, w_b, w_c) = GetCoordOfPoint(point);

            return w_a >= 0 && w_a <= 1
                && w_b >= 0 && w_b <= 1
                && w_c >= 0 && w_c <= 1;
        }

        #endregion

    }
}
