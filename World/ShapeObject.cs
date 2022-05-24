using System;
using System.Drawing;
using System.Numerics;

namespace JA.World
{
    using JA.Gdi;
    using JA.Geometry;

    public class ShapeObject : GraphicsObject
    {
        public ShapeObject(Color color, IShape shape)
            : this(color, Vector2.Zero, 0, shape) { }
        public ShapeObject(Color color, Vector2 position, float angle, IShape shape) 
            : base(color, position, angle)
        {
            Shape = shape;
        }

        public IShape Shape { get; }

        public override bool Contains(Vector2 point)
        {
            return Shape.Contains(ToLocal(point));
        }
        public override Vector2 GetClosestPointTo(Vector2 point)
        {
            return FromLocal(Shape.GetClosestPointTo(ToLocal(point)));
        }
        public override bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true)
        {
            if (Shape.Hit(ToLocal(ray), out hit, out normal, nearest))
            {
                hit = FromLocal(hit);
                normal = FromLocalDirection(normal);
                return true;
            }
            hit = FromLocal(hit);
            normal = Vector2.Zero;
            return false;
        }

        public override void Draw(Graphics g, GraphicsScene scene)
        {
            var color = scene.IsSelected(this) ? Color.AddH(0.12f) : Color;
            DrawShape(g, scene, Shape, color);
        }
    }
}
