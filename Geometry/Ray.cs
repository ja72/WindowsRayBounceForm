using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace JA.Geometry
{
    using static JA.Utils;

    public readonly struct Ray : IEquatable<Ray>
    {
        public Ray(Vector2 origin, Vector2 direction)
        {
            Origin = origin;
            Direction = Vector2.Normalize(direction);
        }
        public static readonly Ray Default = new Ray(Vector2.Zero, Vector2.Zero);

        #region IEquatable Members

        /// <summary>
        /// Equality overrides from <see cref="System.Object"/>
        /// </summary>
        /// <param name="obj">The object to compare this with</param>
        /// <returns>False if object is a different type, otherwise it calls <code>Equals(Ray)</code></returns>
        public override bool Equals(object obj)
        {
            if (obj is Ray item)
            {
                return Equals(item);
            }
            return false;
        }

        /// <summary>
        /// Checks for equality among <see cref="Ray"/> classes
        /// </summary>
        /// <returns>True if equal</returns>
        public bool Equals(Ray other)
        {
            // TODO: Implement equality check here
            return Origin.Equals(other.Origin)
                && Direction.Equals(other.Direction);
        }
        /// <summary>
        /// Calculates the hash code for the <see cref="Ray"/>
        /// </summary>
        /// <returns>The int hash value</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hc = 17;
                hc = 23 * hc + Origin.GetHashCode();
                hc = 23 * hc + Direction.GetHashCode();
                return hc;
            }
        }
        public static bool operator ==(Ray target, Ray other) { return target.Equals(other); }
        public static bool operator !=(Ray target, Ray other) { return !target.Equals(other); }

        #endregion


        public Vector2 Origin { get; }
        public Vector2 Direction { get; }

        public Ray Flip() => new Ray(Origin, -Direction);
        public Ray Parallel(float distance) => new Ray(Origin + Direction.Orthogonal() * distance, Direction);

        public Vector2 GetPointAlong(float distance) => Origin + distance * Direction;

        public float GetDistanceAlong(Vector2 point)
            => Vector2.Dot(Direction, point - Origin);

        public bool Contains(Vector2 point, float tolerance = 1e-6f)
            => Math.Abs(Direction.Cross(point - Origin)) <= tolerance;

        public bool CanReflectFrom(Vector2 point, Vector2 normal, out Ray result)
        {
            result = new Ray(point, Vector2.Reflect(Direction, normal));
            return true;
        }
        public bool CanRefractFrom(Vector2 point, Vector2 normal, float coef, out Ray result)
        {
            //normal = Vector2.Negate(normal);
            coef = 1 / coef;
            var ni = Vector2.Dot(Direction, normal);
            var np = Direction - ni * normal;
            float d = 1 - coef * coef * (1 - ni * ni);
            if (d >= 0)
            {
                float f = Sqrt(d);
                result = new Ray(point, -f * normal + coef * np);
                return true;
            }
            result = Ray.Default;
            return false;
        }

        public bool IntersectDistance(Ray ray, out float distance)
        {
            var x = ray.Direction.Cross(Direction);
            if (Math.Abs(x) > 0)
            {
                distance = ray.Direction.Cross(ray.Origin - Origin) / x;
                return distance >= 0;
            }
            distance = 0;
            return false;
        }

        public bool IntersectDistance(Segment segment, out float distance)
        {
            var ray = new Ray(segment.A, segment.B - segment.A);
            var x = ray.Direction.Cross(Direction);
            if (Math.Abs(x) > 0)
            {
                distance = ray.Direction.Cross(ray.Origin - Origin) / x;
                var point = ray.GetPointAlong(distance);
                return segment.Contains(point);
            }
            distance = 0;
            return false;
        }

        public float GetDistanceToLine(Vector2 normal, float offset)
        {
            //tex:Ray with origin $\vec{p}$ and direction $\vec{e}$ intersectrs a plane
            // with normal $\vec{n}$ and offset $h$ from the origin at the following
            // lolcation $$t = \frac{h-\vec{n}\cdot\vec{o}}{\vec{n}\cdot\vec{e}}$$

            return (offset - Vector2.Dot(normal, Origin)) / Vector2.Dot(normal, Direction);
        }
    }
}