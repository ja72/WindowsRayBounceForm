using System;
using System.Numerics;

namespace JA.Geometry
{
    public interface IObject
    {
        Vector2 Center { get; }
        Vector2 Direction { get; }
        float Angle { get; }
    }
    public interface IShape : IObject
    {
        float GetArea();
        Vector2 GetCentroid();
        Vector2 GetStartPoint();
        bool Contains(Vector2 point);
        Vector2 GetClosestPointTo(Vector2 point);
        bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true);
    }

    public static class Transformations
    {
        public static Vector2 FromLocal(this IObject obj, Vector2 node)
{
            return obj.Center + Vector2.Transform(node, Matrix3x2.CreateRotation(obj.Angle));
        }
        public static Vector2 ToLocal(this IObject obj, Vector2 position)
{
            return Vector2.Transform(position - obj.Center, Matrix3x2.CreateRotation(-obj.Angle));
        }
        public static Vector2 FromLocalDirection(this IObject obj, Vector2 node)
        {
            return Vector2.Transform(node, Matrix3x2.CreateRotation(obj.Angle));
        }
        public static Vector2 ToLocalDirection(this IObject obj, Vector2 vector)
        {
            return Vector2.Transform(vector, Matrix3x2.CreateRotation(-obj.Angle));
}
        public static Ray FromLocal(this IObject obj, Ray ray) => new Ray(obj.FromLocal(ray.Origin), obj.FromLocalDirection(ray.Direction));
        public static Ray ToLocal(this IObject obj, Ray ray) => new Ray(obj.ToLocal(ray.Origin), obj.ToLocalDirection(ray.Direction));

    }
}