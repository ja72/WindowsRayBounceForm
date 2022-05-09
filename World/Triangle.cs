using System.Drawing;
using System.Numerics;

namespace JA.World
{
    using System;

    public class Triangle : Object
    {
        public Triangle(Color color, Vector2 position, float angle, Vector2 a, Vector2 b, Vector2 c)
            : base(color, position,angle)
        {
            A = a;
            B = b;
            C = c;
        }
        public Vector2 A { get; }
        public Vector2 B { get; }
        public Vector2 C { get; }

        public float Area => A.Cross(B) + B.Cross(C) + C.Cross(A);

        public override void Draw(Graphics g, Scene scene)
        {
            scene.DrawPolygon(g, this,
                new Vector2[] { A, B, C });
        }

        public Vector2 GetClosestPointTo(Vector2 point)
        {
            var AB = new Segment(A, B);
            var BC = new Segment(B, C);
            var CA = new Segment(C, A);

            var pAB = AB.GetClosestPointTo(point);
            var pBC = BC.GetClosestPointTo(point);
            var pCA = CA.GetClosestPointTo(point);

            var dAB = Vector2.Distance(pAB, point);
            var dBC = Vector2.Distance(pBC, point);
            var dCA = Vector2.Distance(pCA, point);

            var d = Math.Min(dAB, Math.Min(dBC, dCA));

            if (d == dAB) { point = pAB; }
            if (d == dBC) { point = pBC; }
            if (d == dCA) { point = pCA; }

            return point;
        }

        public (float w_a, float w_b, float w_c) GetCoordOfPoint(Vector2 point)
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

        public override bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true)
        {
            var AB = new Segment(A, B);
            var BC = new Segment(B, C);
            var CA = new Segment(C, A);

            float t_AB = 0, t_BC = 0, t_CA = 0;

            if (AB.Hit(ray, out var pAB, out var nAB))
            {
                t_AB = ray.GetDistanceAlong(pAB);
            }
            if (BC.Hit(ray, out var pBC, out var nBC))
            {
                t_BC = ray.GetDistanceAlong(pBC);
            }
            if (CA.Hit(ray, out var pCA, out var nCA))
            {
                t_CA = ray.GetDistanceAlong(pCA);
            }

            //float t = Math.Min(

            throw new NotImplementedException();
        }
    }
}
