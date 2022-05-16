using System;
using System.Drawing;
using System.Numerics;

namespace JA.World
{
    using JA.Gdi;
    using JA.Geometry;

    using static Utils;

    public class EllipseGraphics : GraphicsObject
    {
        public EllipseGraphics(Color color, Vector2 position, float angle, float majorAxis, float minorAxis)
            : base(color, position, angle)
        {
            this.MajorAxis = Math.Max(majorAxis, minorAxis);
            this.MinorAxis = Math.Min(majorAxis, minorAxis);
        }
        public float MajorAxis { get; }
        public float MinorAxis { get; }

        #region Interactions
        public override bool Contains(Vector2 point)
        {
            point = ToLocal(point);
            return Math.Pow(point.X / MajorAxis, 2) + Math.Pow(point.Y / MinorAxis, 2) <= 1;
        }
        public override Vector2 GetClosestPointTo(Vector2 point)
        {
            //https://www.geometrictools.com/Documentation/DistancePointEllipseEllipsoid.pdf
            float GetRoot(float r0, float z0, float z1, float g)
            {
                float n0 = r0 * z0;
                float s0 = z1 - 1, s1 = (g < 0 ? 0 : GetNorm(n0, z1) - 1);
                float s = 0;
                int maxIter = 200;
                while (maxIter > 0)
                {
                    s = (s0 + s1) / 2;
                    if (s == s0 || s == s1) { break; }
                    float ratio0 = n0 / (s + r0), ratio1 = z1 / (s + 1);
                    g = Sqr(ratio0) + Sqr(ratio1) - 1;
                    if (g > 0)
                    {
                        s0 = s;
                    }
                    else if (g < 0)
                    {
                        s1 = s;
                    }
                    else
                    {
                        break;
                    }
                    maxIter--;
                }
                return s;
            };
            point = ToLocal(point);
            float e0 = MajorAxis, e1 = MinorAxis;
            int s0 = Math.Sign(point.X), s1 = Math.Sign(point.Y);
            float y0 = Math.Abs(point.X), y1 = Math.Abs(point.Y);
            float x0, x1;
            if (y1 > 0)
            {
                if (y0 > 0)
                {
                    float z0 = y0 / e0, z1 = y1 / e1, g = Sqr(z0) + Sqr(z1) - 1;
                    if (g != 0)
                    {
                        float r0 = Sqr(e0 / e1), sbar = GetRoot(r0, z0, z1, g);
                        x0 = r0 * y0 / (sbar + r0);
                        x1 = y1 / (sbar + 1);
                        return FromLocal(new Vector2(s0 * x0, s1 * x1));
                    }
                    else
                    {
                        return FromLocal(new Vector2(s0 * y0, s1 * y1));
                    }
                }
                else
                {
                    return FromLocal(new Vector2(0, s1 * e1));
                }
            }
            else
            {
                float numer0 = e0 * y0;
                float denom0 = Sqr(e0) - Sqr(e1);
                if (numer0 < denom0)
                {
                    float xde0 = numer0 / denom0;
                    x0 = e0 * xde0;
                    x1 = e1 * Sqrt(1 - xde0 * xde0);
                    return FromLocal(new Vector2(s0 * x0, s1 * x1));
                }
                else
                {
                    return FromLocal(new Vector2(s0 * e0, 0));
                }
            }
        }
        public override bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true)
        {
            ray = ToLocal(ray);
            float a = MajorAxis, b = MinorAxis;
            float px = ray.Origin.X, py = ray.Origin.Y;
            float ex = ray.Direction.X, ey = ray.Direction.Y;

            if (QuadraticRoot(
                -Sqr(a * b) + Sqr(a * py) + Sqr(b * px),
                2 * (a * a * ey * py + b * b * ex * px),
                Sqr(a * ey) + Sqr(b * ex), out float t1, out float t2))
            {
                float t = Math.Min(t1, t2);
                hit = ray.GetPointAlong(t);
                float x = hit.X, y = hit.Y;
                float d = b * b * x * x + a * a * y * y;
                float s = Sqrt(d);
                hit = FromLocal(hit);
                normal = FromLocalDirection(
                    Vector2.Normalize(
                        new Vector2(b*x/a, a*y/b)));
                return true;
            }
            hit = FromLocal(ray.Origin);
            normal = Vector2.Zero;
            return false;
        }
        #endregion

        #region Drawing
        public override void Draw(Graphics g, GraphicsScene scene)
        {
            var color = scene.IsSelected(this) ? Color.AddH(0.12f) : Color;
            scene.DrawEllipse(g, color, Position, Angle, MajorAxis, MinorAxis);
        }
        #endregion
    }
}
