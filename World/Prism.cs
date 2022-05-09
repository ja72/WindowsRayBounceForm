using System.Drawing;
using System.Numerics;

namespace JA.World
{
    using System;

    public class Box : Object
    {
        public Box(Color color, float width, float height)
            : base(color)
        {
            Width = width;
            Height = height;
        }
        public Box(Color color, Vector2 position, float angle, float width, float height)
            : base(color, position, angle)
        {
            Width = width;
            Height = height;
        }

        public float Width { get; set; }
        public float Height { get; set; }

        public override bool Contains(Vector2 point)
        {
            var local = ToLocal(point);

            return local.X >= -Width / 2 && local.X <= Width / 2
                && local.Y >= -Height / 2 && local.Y <= Height / 2;
        }
        public override bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true)
        {
            var localRay = ToLocal(ray);

            //tex:Ray with origin $\vec{p}$ and direction $\vec{e}$ intersectrs a plane
            // with normal $\vec{n}$ and distance $d$ from the origin at the following
            // lolcation $$t = \frac{d-\vec{n}\cdot\vec{o}}{\vec{n}\cdot\vec{e}}$$

            // intersections with four sides of box
            var ty1 = (-Height / 2 - Vector2.Dot(Vector2.UnitY, localRay.Origin)) / Vector2.Dot(Vector2.UnitY, localRay.Direction);
            var ty2 = (+Height / 2 - Vector2.Dot(Vector2.UnitY, localRay.Origin)) / Vector2.Dot(Vector2.UnitY, localRay.Direction);
            var tx1 = (-Width  / 2 - Vector2.Dot(Vector2.UnitX, localRay.Origin)) / Vector2.Dot(Vector2.UnitX, localRay.Direction);
            var tx2 = (+Width  / 2 - Vector2.Dot(Vector2.UnitX, localRay.Origin)) / Vector2.Dot(Vector2.UnitX, localRay.Direction);

            // find nearest and farthest in each direction
            var tx_min = Math.Min(tx1, tx2);
            var ty_min = Math.Min(ty1, ty2);
            var tx_max = Math.Max(tx1, tx2);
            var ty_max = Math.Max(ty1, ty2);

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

            if (t == tx1) normal = FromLocalDirection(-Vector2.UnitX);
            else if (t == tx2) normal = FromLocalDirection(Vector2.UnitX);
            else if (t == ty1) normal = FromLocalDirection(-Vector2.UnitY);
            else if (t == ty2) normal = FromLocalDirection(Vector2.UnitY);
            else normal = Vector2.Zero;

            return true;
        }
        public override void Draw(Graphics g, Scene scene)
        {
            scene.DrawPolygon(g, this, 
                new Vector2[] {
                    new Vector2(-Width/2, -Height/2),
                    new Vector2(+Width/2, -Height/2),
                    new Vector2(+Width/2, +Height/2),
                    new Vector2(-Width/2, +Height/2) });
        }

    }
}
