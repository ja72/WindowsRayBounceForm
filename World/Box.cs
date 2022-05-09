using System.Drawing;
using System.Numerics;
    using System;

namespace JA.World
{
    using JA.Gdi;

    public class Box : Polygon
    {
        public Box(Color color, Vector2 position, float angle, float width, float height)
            : base(color, position, angle,
                    new Vector2(-width / 2, -height / 2),
                    new Vector2(+width / 2, -height / 2),
                    new Vector2(+width / 2, +height / 2),
                    new Vector2(-width / 2, +height / 2))
        {
            Width = width;
            Height = height;
        }

        public float Width { get;  }
        public float Height { get;  }


        #region Interactions
        public override bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true)
        {
            var localRay = ToLocal(ray);

            // intersections with four sides of box
            var th1 = localRay.GetDistanceToLine(Vector2.UnitY, -Height / 2);
            var th2 = localRay.GetDistanceToLine(Vector2.UnitY, +Height / 2);
            var tw1 = localRay.GetDistanceToLine(Vector2.UnitX, -Width / 2);
            var tw2 = localRay.GetDistanceToLine(Vector2.UnitX, +Width / 2);

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
                hit = ray.Origin;
                normal = Vector2.Zero;
                return false;
            }

            // select near (default) or far point
            var t = nearest ? t_near : t_far;
            hit = ray.GetPointAlong(t);

            if (t == tw1) normal = FromLocalDirection(-Vector2.UnitX);
            else if (t == tw2) normal = FromLocalDirection(Vector2.UnitX);
            else if (t == th1) normal = FromLocalDirection(-Vector2.UnitY);
            else if (t == th2) normal = FromLocalDirection(Vector2.UnitY);
            else normal = Vector2.Zero;

            return true;
        }

        public override bool Contains(Vector2 point)
        {
            var local = ToLocal(point);

            return local.X >= -Width / 2 && local.X <= Width / 2
                && local.Y >= -Height / 2 && local.Y <= Height / 2;
        }
        #endregion

    }
}
