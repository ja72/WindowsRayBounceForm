using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace JA.Geometry
{
    public readonly struct Circle : IShape, IEquatable<Circle>
    {
        public Circle(float radius)
            : this(Vector2.Zero, radius) { }
        public Circle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        #region Properties
        public Vector2 Center { get; }
        public float Radius { get; }
        public Vector2 Direction { get => Vector2.UnitX; }
        public float Angle { get => 0; }
        public float GetArea()
        {
            return (float)(Math.PI * Radius * Radius);
        }

        public Vector2 GetCentroid()
        {
            return Center;
        }
        public Vector2 GetStartPoint() => this.FromLocal(Radius * Vector2.UnitX);
        #endregion

        #region Interactions
        public bool Contains(Vector2 point)
        {
            return Vector2.DistanceSquared(Center, point) <= Radius * Radius;
        }

        public Vector2 GetClosestPointTo(Vector2 point)
        {
            return Center + Radius * Vector2.Normalize(point - Center);
        }

        public bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true)
        {
            float A_sq = Radius * Radius - Vector2.DistanceSquared(ray.Origin, Center);
            float B = Vector2.Dot(ray.Direction, ray.Origin - Center);
            float C = A_sq + B * B;
            if (C >= 0)
            {
                float t_near = -(float)Math.Sqrt(C) - B;
                float t_far = +(float)Math.Sqrt(C) - B;

                if (t_near < 0 && t_far < 0)
                {
                    hit = ray.Origin;
                    normal = Vector2.Zero;
                    return false;
                }
                // select nearest of both postive, otherwise pick far (which should be +)
                var t = t_near >= 0 && t_far >= 0 ? nearest ? t_near : t_far : t_far;
                hit = ray.GetPointAlong(t);
                normal = Vector2.Normalize(hit - Center);
                var sign = -Math.Sign(Vector2.Dot(ray.Direction, normal));
                normal *= sign;
                return true;
            }
            hit = ray.Origin;
            normal = Vector2.Zero;
            return false;
        }

        #endregion

        #region IEquatable Members
        /// <summary>
        /// Equality overrides from <see cref="System.Object"/>
        /// </summary>
        /// <param name="obj">The object to compare this with</param>
        /// <returns>False if object is a different type, otherwise it calls <code>Equals(Circle)</code></returns>
        public override bool Equals(object obj)
        {
            if (obj is Circle other)
            {
                return Equals(other);
            }
            return false;
        }

        public static bool operator ==(Circle target, Circle other) { return target.Equals(other); }
        public static bool operator !=(Circle target, Circle other) { return !(target == other); }


        /// <summary>
        /// Checks for equality among <see cref="Circle"/> classes
        /// </summary>
        /// <param name="other">The other <see cref="Circle"/> to compare it to</param>
        /// <returns>True if equal</returns>
        public bool Equals(Circle other) => Center.Equals(other.Center) && Radius.Equals(other.Radius);

        /// <summary>
        /// Calculates the hash code for the <see cref="Circle"/>
        /// </summary>
        /// <returns>The int hash value</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hc = -1817952719;
                hc = (-1521134295) * hc + Center.GetHashCode();
                hc = (-1521134295) * hc + Radius.GetHashCode();
                return hc;
            }
        }

        #endregion

        public override string ToString() => $"Circle({Radius})";

    }
}
