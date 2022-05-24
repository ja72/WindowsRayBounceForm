using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace JA.Geometry
{
    public readonly struct Segment : IEquatable<Segment>
    {
        public Segment(Vector2 a, Vector2 b) : this()
        {
            A = a;
            B = b;
        }

        public Vector2 A { get; }
        public Vector2 B { get; }

        public Vector2 Direction { get => A.Equals(B) ? Vector2.Zero : Vector2.Normalize(B - A); }
        public Vector2 Normal { get => A.Equals(B) ? Vector2.Zero : Vector2.Normalize(new Vector2(A.Y - B.Y, B.X - A.X)); }
        public float Offset => Vector2.Dot(Normal, A);
        public float Length { get => Vector2.Distance(A, B); }
        public float LengthSquared { get => Vector2.DistanceSquared(A, B); }
        public Segment Flip() => new Segment(B, A);
        public Segment Translate(Vector2 delta) => new Segment(A + delta, B + delta);
        public Segment Rotate(float angle)
        {
            var R = Matrix3x2.CreateRotation(angle);

            return new Segment(
                Vector2.Transform(A, R),
                Vector2.Transform(B, R));
        }
        public Segment Rotate(float angle, Vector2 pivot)
        {
            var R = Matrix3x2.CreateRotation(angle, pivot);

            return new Segment(
                Vector2.Transform(A, R),
                Vector2.Transform(B, R));
        }

        #region IEquatable Members

        /// <summary>
        /// Equality overrides from <see cref="System.Object"/>
        /// </summary>
        /// <param name="obj">The object to compare this with</param>
        /// <returns>False if object is a different type, otherwise it calls <code>Equals(Segment)</code></returns>
        public override bool Equals(object obj)
        {
            if (obj is Segment item)
            {
                return Equals(item);
            }
            return false;
        }

        /// <summary>
        /// Checks for equality among <see cref="Segment"/> classes
        /// </summary>
        /// <returns>True if equal</returns>
        public bool Equals(Segment other)
        {
            // TODO: Implement equality check here
            return A.Equals(other.A)
                && B.Equals(other.B);
        }
        /// <summary>
        /// Calculates the hash code for the <see cref="Segment"/>
        /// </summary>
        /// <returns>The int hash value</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hc = 17;
                hc = 23 * hc + A.GetHashCode();
                hc = 23 * hc + B.GetHashCode();
                return hc;
            }
        }
        public static bool operator ==(Segment target, Segment other) { return target.Equals(other); }
        public static bool operator !=(Segment target, Segment other) { return !target.Equals(other); }

        #endregion

        public bool Hit(Ray ray, out Vector2 hit, out Vector2 normnal, bool insideAlso = true)
        {
            float d = ray.Direction.Cross(B - A);

            if (Math.Abs(d) > 0)
            {
                float w_B = ray.Direction.Cross(ray.Origin - A) / d;
                float w_A = ray.Direction.Cross(B - ray.Origin) / d;

                hit = GetPointFromCoord(w_A, w_B);
                normnal = Math.Sign(d) * Normal;

                return (w_A >= 0 && w_A <= 1
                    && w_B >= 0 && w_B <= 1);
            }

            hit = ray.Origin;
            normnal = Vector2.Zero;
            return false;
        }

        public (float w_A, float w_B) GetCoordOfPoint(Vector2 point)
        {
            Vector2 AB = B-A;
            float L2 = AB.LengthSquared();

            return ( Vector2.Dot(B - point, AB) / L2, Vector2.Dot(point - A, AB) / L2 );
        }

        public Vector2 GetPointFromCoord(float w_A, float w_B) 
        {
            return w_A * A + w_B * B;
        }

        public Vector2 GetClosestPointTo(Vector2 point)
        {
            var (w_A, w_B) = GetCoordOfPoint(point);
            w_A = w_A.ClampValue(0f, 1f);
            w_B = w_B.ClampValue(0f, 1f);
            return w_A * A + w_B * B;
        }

        public bool Contains(Vector2 point, float tolerance = 1e-6f)
        {
            var L = Length;
            // Area = 1/2*L*h = (A×B + B×P + P×A)/2 } Find h
            if (Math.Abs(A.Cross(B) + B.Cross(point) + point.Cross(A)) / L <= tolerance)
            {
                var (w_A, w_B) = GetCoordOfPoint(point);

                return w_A >= 0 && w_A <= 1
                    && w_B >= 0 && w_B <= 1;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{A}-{B}";
        }
    }
}
