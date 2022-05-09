using System.Drawing;
using System.Numerics;

namespace JA.World
{
    using JA.Gdi;

    public abstract class Object
    {
        protected Object(Color color)
            : this(color, Vector2.Zero, 0)
        {
        }
        protected Object(Color color, Vector2 position, float angle)
        {
            Color = color;
            Position = position;
            Angle = angle;
        }

        public Vector2 Position { get; set; }
        public float Angle { get; set; }
        public Color Color { get; set; }

        public abstract bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true);
        public abstract Vector2 GetClosestPointTo(Vector2 point);
        public abstract bool Contains(Vector2 point);
        public abstract void Draw(Graphics g, Scene scene);

        #region Transformations
        public Vector2 FromLocal(Vector2 node)
        {
            return Position + Vector2.Transform(node, Matrix3x2.CreateRotation(Angle));
        }
        public Vector2[] FromLocal(params Vector2[] nodes)
        {
            Vector2[] positions = new Vector2[nodes.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = FromLocal(nodes[i]);
            }
            return positions;
        }
        public Vector2 ToLocal(Vector2 position)
        {
            return Vector2.Transform(position - Position, Matrix3x2.CreateRotation(-Angle));
        }
        public Vector2[] ToLocal(params Vector2[] positions)
        {
            Vector2[] nodes = new Vector2[positions.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = ToLocal(positions[i]);
            }
            return nodes;
        }
        public Vector2 FromLocalDirection(Vector2 node)
        {
            return Vector2.Transform(node, Matrix3x2.CreateRotation(Angle));
        }
        public Vector2[] FromLocalDirection(params Vector2[] nodes)
        {
            Vector2[] positions = new Vector2[nodes.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = FromLocalDirection(nodes[i]);
            }
            return positions;
        }
        public Vector2 ToLocalDirection(Vector2 vector)
        {
            return Vector2.Transform(vector, Matrix3x2.CreateRotation(-Angle));
        }
        public Vector2[] ToLocalDirection(params Vector2[] vectors)
        {
            Vector2[] nodes = new Vector2[vectors.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = ToLocalDirection(vectors[i]);
            }
            return nodes;
        }
        public Ray FromLocal(Ray ray) => new Ray(FromLocal(ray.Origin), FromLocalDirection(ray.Direction));
        public Ray ToLocal(Ray ray) => new Ray(ToLocal(ray.Origin), ToLocalDirection(ray.Direction));
        public Segment FromLocal(Segment segment) => new Segment(FromLocal(segment.A), FromLocal(segment.B));
        public Segment ToLocal(Segment segment) => new Segment(ToLocal(segment.A), ToLocal(segment.B)); 
        #endregion
    }
}
