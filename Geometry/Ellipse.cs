using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace JA.Geometry
{
    using static JA.Utils;

    public readonly struct Ellipse : IShape
    {
        public Ellipse(float majorAxis, float minorAxis)
            : this(Vector2.Zero, Vector2.Zero, majorAxis, minorAxis) { }
        public Ellipse(Vector2 center, Vector2 direction, float majorAxis, float minorAxis)
        {
            Center = center;
            Direction = direction;
            MajorAxis = majorAxis;
            MinorAxis = minorAxis;
        }

        #region Properties
        public Vector2 Center { get; }
        public Vector2 Direction { get; }
        public float Angle { get => (float)Math.Atan2(Direction.Y, Direction.X); }
        public float MajorAxis { get; }
        public float MinorAxis { get; }
        public float GetArea() => (float)(Math.PI * MajorAxis * MinorAxis);
        public Vector2 GetCentroid() => Center;
        public Vector2 GetStartPoint() => Center + MajorAxis * Direction;
        #endregion

        #region Interactions

        public bool Contains(Vector2 point)
        {
            point = this.ToLocal(point);
            return Math.Pow(point.X / MajorAxis, 2) + Math.Pow(point.Y / MinorAxis, 2) <= 1;
        }

        public Vector2 GetClosestPointTo(Vector2 point)
        {
            //https://www.geometrictools.com/Documentation/DistancePointEllipseEllipsoid.pdf
            static float GetRoot(float r0, float z0, float z1, float g)
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
            point = this.ToLocal(point);
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
                        return this.FromLocal(new Vector2(s0 * x0, s1 * x1));
                    }
                    else
                    {
                        return this.FromLocal(new Vector2(s0 * y0, s1 * y1));
                    }
                }
                else
                {
                    return this.FromLocal(new Vector2(0, s1 * e1));
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
                    return this.FromLocal(new Vector2(s0 * x0, s1 * x1));
                }
                else
                {
                    return this.FromLocal(new Vector2(s0 * e0, 0));
                }
            }
        }

        public bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true)
        {
            ray = this.ToLocal(ray);
            float a = MajorAxis, b = MinorAxis;
            float px = ray.Origin.X, py = ray.Origin.Y;
            float ex = ray.Direction.X, ey = ray.Direction.Y;

            if (QuadraticRoot(
                -Sqr(a * b) + Sqr(a * py) + Sqr(b * px),
                2 * (a * a * ey * py + b * b * ex * px),
                Sqr(a * ey) + Sqr(b * ex), out float t_near, out float t_far))
            {

                // select nearest of both postive, otherwise pick far (which should be +)
                var t = t_near >= 0 && t_far >= 0 ? nearest ? t_near : t_far : t_far;
                hit = ray.GetPointAlong(t);
                float x = hit.X, y = hit.Y;
                normal = Vector2.Normalize(new Vector2(b * x / a, a * y / b));
                var sign = -Math.Sign(Vector2.Dot(ray.Direction, normal));
                normal *= sign;
                hit = this.FromLocal(hit);
                normal = this.FromLocalDirection(normal);
                return true;
            }
            hit = this.FromLocal(ray.Origin);
            normal = Vector2.Zero;
            return false;
        }

        #endregion


        #region IEquatable Members
        /// <summary>
        /// Equality overrides from <see cref="System.Object"/>
        /// </summary>
        /// <param name="obj">The object to compare this with</param>
        /// <returns>False if object is a different type, otherwise it calls <code>Equals(Ellipse)</code></returns>
        public override bool Equals(object obj)
        {
            if (obj is Ellipse other)
            {
                return Equals(other);
            }
            return false;
        }

        public static bool operator ==(Ellipse target, Ellipse other) { return target.Equals(other); }
        public static bool operator !=(Ellipse target, Ellipse other) { return !(target == other); }


        /// <summary>
        /// Checks for equality among <see cref="Ellipse"/> classes
        /// </summary>
        /// <param name="other">The other <see cref="Ellipse"/> to compare it to</param>
        /// <returns>True if equal</returns>
        public bool Equals(Ellipse other)
        {
            return Center.Equals(other.Center)
                && Direction.Equals(other.Direction)
                && MajorAxis.Equals(other.MajorAxis)
                && MinorAxis.Equals(other.MinorAxis);
        }

        /// <summary>
        /// Calculates the hash code for the <see cref="Ellipse"/>
        /// </summary>
        /// <returns>The int hash value</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hc = -1817952719;
                hc = (-1521134295) * hc + Center.GetHashCode();
                hc = (-1521134295) * hc + Direction.GetHashCode();
                hc = (-1521134295) * hc + MajorAxis.GetHashCode();
                hc = (-1521134295) * hc + MinorAxis.GetHashCode();
                return hc;
            }
        }

        #endregion

        public override string ToString() => $"Ellipse({MajorAxis},{MinorAxis})";

    }
}
