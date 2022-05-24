using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace JA.Geometry
{
    public readonly struct Box : IShape, IEquatable<Box>
    {
        public Box(float width, float height)
             : this(Vector2.Zero, Vector2.Zero, width, height) { }
        public Box(Vector2 center, Vector2 direction, float width, float height)
        {
            Center = center;
            Direction = direction;
            Width = width;
            Height = height;
        }

        #region Properties
        public Vector2 Center { get; }
        public Vector2 Direction { get; }
        public float Angle { get => (float)Math.Atan2(Direction.Y, Direction.X); }
        public float Width { get; }
        public float Height { get; }
        public float GetArea() => Width * Height;
        public Vector2 GetCentroid() => Center;
        public Vector2 GetStartPoint() => this.FromLocal(-Width/2 * Vector2.UnitX - Height/2 * Vector2.UnitY);

        public Polygon ToPolygon()
        {
            return new Polygon(Center, Direction, new Vector2[] {
                    new Vector2(-Width / 2, -Height / 2),
                    new Vector2(+Width / 2, -Height / 2),
                    new Vector2(+Width / 2, +Height / 2),
                    new Vector2(-Width / 2, +Height / 2) });
        }

        #endregion

        #region Interactions
        public bool Contains(Vector2 point)
        {
            var local = this.ToLocal(point);

            return local.X >= -Width / 2 && local.X <= Width / 2
                && local.Y >= -Height / 2 && local.Y <= Height / 2;
        }
        public Vector2 GetClosestPointTo(Vector2 point)
            => ToPolygon().GetClosestPointTo(point);
        public bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true)
        {
            ray = this.ToLocal(ray);

            // intersections with four sides of box
            var th1 = ray.GetDistanceToLine(Vector2.UnitY, -Height / 2);
            var th2 = ray.GetDistanceToLine(Vector2.UnitY, +Height / 2);
            var tw1 = ray.GetDistanceToLine(Vector2.UnitX, -Width / 2);
            var tw2 = ray.GetDistanceToLine(Vector2.UnitX, +Width / 2);

            // find nearest and farthest in each direction
            var tx_min = Math.Min(tw1, tw2);
            var ty_min = Math.Min(th1, th2);
            var tx_max = Math.Max(tw1, tw2);
            var ty_max = Math.Max(th1, th2);

            // keep the largest of minimum, and smallest of maximum
            var t_near = Math.Max(tx_min, ty_min);
            var t_far = Math.Min(tx_max, ty_max);

            // if far is smaller than near then we did not hit the box
            if (t_far < t_near)
            {
                hit = this.FromLocal(ray.Origin);
                normal = Vector2.Zero;
                return false;
            }

            // select near (default) or far point
            var t = nearest ? t_near : t_far;
            hit = this.FromLocal(ray.GetPointAlong(t));

            if (t == tw1)
            {
                normal = this.FromLocalDirection(-Vector2.UnitX);
            }
            else if (t == tw2)
            {
                normal = this.FromLocalDirection(Vector2.UnitX);
            }
            else if (t == th1)
            {
                normal = this.FromLocalDirection(-Vector2.UnitY);
            }
            else if (t == th2)
            {
                normal = this.FromLocalDirection(Vector2.UnitY);
            }
            else
            {
                normal = Vector2.Zero;
            }

            return true;
        }

        #endregion

        #region IEquatable Members
        /// <summary>
        /// Equality overrides from <see cref="System.Object"/>
        /// </summary>
        /// <param name="obj">The object to compare this with</param>
        /// <returns>False if object is a different type, otherwise it calls <code>Equals(Box)</code></returns>
        public override bool Equals(object obj)
        {
            if (obj is Box other)
            {
                return Equals(other);
            }
            return false;
        }

        public static bool operator ==(Box target, Box other) { return target.Equals(other); }
        public static bool operator !=(Box target, Box other) { return !(target == other); }


        /// <summary>
        /// Checks for equality among <see cref="Box"/> classes
        /// </summary>
        /// <param name="other">The other <see cref="Box"/> to compare it to</param>
        /// <returns>True if equal</returns>
        public bool Equals(Box other)
        {
            return Center.Equals(other.Center)
                && Direction.Equals(other.Direction)
                && Width.Equals(other.Width)
                && Height.Equals(other.Height);
        }

        /// <summary>
        /// Calculates the hash code for the <see cref="Box"/>
        /// </summary>
        /// <returns>The int hash value</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hc = -1817952719;
                hc = (-1521134295) * hc + Center.GetHashCode();
                hc = (-1521134295) * hc + Direction.GetHashCode();
                hc = (-1521134295) * hc + Width.GetHashCode();
                hc = (-1521134295) * hc + Height.GetHashCode();
                return hc;
            }
        }

        #endregion

        public override string ToString() => $"Box({Width},{Height})";
    }
}
