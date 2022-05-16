using System;
using System.Numerics;

namespace JA.Geometry
{
    public interface IShape
    {
        Vector2 Center { get; }
        Vector2 Direction { get; }
        float Angle { get; }
        float GetArea();
        Vector2 GetCentroid();
        Vector2 GetStartPoint();
        bool Contains(Vector2 point);
        Vector2 GetClosestPointTo(Vector2 point);
        bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true);
    }

    public static class Transformations
    {
        public static Vector2 FromLocal(this IShape shape, Vector2 node)
{
            return shape.Center + Vector2.Transform(node, Matrix3x2.CreateRotation(shape.Angle));
        }
        public static Vector2 ToLocal(this IShape shape, Vector2 position)
{
            return Vector2.Transform(position - shape.Center, Matrix3x2.CreateRotation(-shape.Angle));
        }
        public static Vector2 FromLocalDirection(this IShape shape, Vector2 node)
        {
            return Vector2.Transform(node, Matrix3x2.CreateRotation(shape.Angle));
        }
        public static Vector2 ToLocalDirection(this IShape shape, Vector2 vector)
        {
            return Vector2.Transform(vector, Matrix3x2.CreateRotation(-shape.Angle));
}
        public static Ray FromLocal(this IShape shape, Ray ray) => new Ray(shape.FromLocal(ray.Origin), shape.FromLocalDirection(ray.Direction));
        public static Ray ToLocal(this IShape shape, Ray ray) => new Ray(shape.ToLocal(ray.Origin), shape.ToLocalDirection(ray.Direction));

    }
}