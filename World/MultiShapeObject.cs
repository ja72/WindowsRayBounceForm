using System.Linq;
using System.Drawing;
using System.Numerics;

namespace JA.World
{
    using JA.Gdi;
    using JA.Geometry;
    using System.Collections.Generic;

    public class MultiShapeObject : GraphicsObject
    {
        public MultiShapeObject(Color color, params IShape[] shapes)
            : this(color, Vector2.Zero, 0, shapes) { }
        public MultiShapeObject(Color color, Vector2 position, float angle, params IShape[] shapes)
            : base(color, position, angle)
        {
            Shapes = new List<IShape>(shapes);
        }
        public MultiShapeObject(Color color, Vector2 position, float angle, IEnumerable<IShape> shapes)
            : base(color, position, angle)
        {
            Shapes = new List<IShape>(shapes);
        }

        public List<IShape> Shapes { get; }

        public override bool Contains(Vector2 point)
        {
            point = ToLocal(point);
            return Shapes.Any((s) => s.Contains(point));
        }
        public override void Draw(Graphics g, GraphicsScene scene)
        {
            var color = scene.IsSelected(this) ? Color.AddH(0.12f) : Color;
            foreach (var shape in Shapes)
            {
                DrawShape(g, scene, shape, color);
            }
        }
        public override Vector2 GetClosestPointTo(Vector2 point)
        {
            int index = -1;
            float minDistance = 0;
            Vector2 minPoint = Vector2.Zero;
            for (int i = 0; i < Shapes.Count; i++)
            {
                var temp = Shapes[i].GetClosestPointTo(point);
                float distance = Vector2.Distance(temp, point);
                if (index == -1 || distance < minDistance)
                {
                    index = i;
                    minDistance = distance;
                    minPoint = temp;
                }
            }
            return minPoint;
        }
        public override bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true)
        {
            ray = ToLocal(ray);
            int index = -1;
            float hitDistance = 0;
            Vector2 hitPoint = Vector2.Zero;
            Vector2 normalPoint = Vector2.Zero;
            for (int i = 0; i < Shapes.Count; i++)
            {
                if (Shapes[i].Hit(ray, out var tempHit, out var tempNorm))
                {
                    float distance = ray.GetDistanceAlong(tempHit);
                    if (index == -1 || ( distance < hitDistance && distance>0))
                    {
                        index = i;
                        hitDistance = distance;
                        hitPoint = tempHit;
                        normalPoint = tempNorm;
                    }
                }
            }
            if (index >= 0)
            {
                hit = FromLocal(hitPoint);
                normal = FromLocalDirection(normalPoint);
                return true;
            }
            hit = FromLocal(ray.Origin);
            normal = Vector2.Zero;
            return false;
        }
    }
}
