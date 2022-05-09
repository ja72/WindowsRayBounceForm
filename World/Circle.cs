using System.Drawing;
using System.Numerics;

namespace JA.World
{
    using System;

    public class Circle : Object
    {
        public Circle(Color color, Vector2 position, float angle, float radius)
            : base(color, position, angle)
        {
            this.Radius = radius;
        }

        public float Radius { get; }

        public Vector2 GetClosestPointTo(Vector2 point)
        {
            return Position - Radius * Vector2.Normalize(point - Position);
        }

        public override bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true)
        {
            float A_sq = Radius * Radius - Vector2.DistanceSquared(ray.Origin, Position);
            float B = Vector2.Dot(ray.Direction, ray.Origin - Position);
            float C = A_sq + B * B;
            if (C >= 0)
            {
                float t_min = -(float)Math.Sqrt(C) - B;
                float t_max = +(float)Math.Sqrt(C) - B;

                hit = ray.GetPointAlong(nearest ? t_min : t_max);
                normal = Vector2.Normalize(hit - Position);
                return true;
            }
            hit = ray.Origin;
            normal = Vector2.Zero;
            return false;
        }

        public override void Draw(Graphics g, Scene scene)
        {
            scene.DrawCircle(g, this, Radius);
        }

        public override bool Contains(Vector2 position)
        {
            position = ToLocal(position);
            return position.LengthSquared() <= Radius*Radius;
        }
    }
}
