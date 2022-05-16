using System;
using System.Drawing;
using System.Numerics;
using System.Drawing.Drawing2D;

namespace JA.World
{
    using JA.Gdi;
    using JA.Geometry;

    public class CircleGraphics : GraphicsObject
    {
        public CircleGraphics(Color color, Vector2 position, float angle, float radius)
            : base(color, position, angle)
        {
            this.Radius = radius;
        }

        public float Radius { get; }

        #region Interactions
        public override Vector2 GetClosestPointTo(Vector2 point)
        {
            // add 1 radius towards point from position
            return Position + Radius * Vector2.Normalize(point - Position);
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

        public override bool Contains(Vector2 position)
        {
            position = ToLocal(position);
            return position.LengthSquared() <= Radius * Radius;
        }
        #endregion

        #region Drawing
        public override void Draw(Graphics g, GraphicsScene scene)
        {
            var color = scene.IsSelected(this) ? Color.AddH(0.12f) : Color;
            scene.DrawCircle(g, color, Position, Angle, Radius);
        }
        #endregion
    }
}
