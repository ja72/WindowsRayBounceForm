using System.Drawing;
using System.Numerics;

namespace JA.World
{
    using System;

    public class Polygon : Object
    {
        public Polygon(Color color, Vector2 position, float angle, params Vector2[] nodes)
            : base(color, position, angle)
        {
            Nodes = nodes;
        }

        public Vector2[] Nodes { get; }

        public override void Draw(Graphics g, Scene scene)
        {
            scene.DrawPolygon(g, this, Nodes);
        }
        public float[] GetCoordOfPoint(Vector2 point)
        {
            point = ToLocal(point);

            float d = 0;
            int n = Nodes.Length;
            for (int i = 0; i < n; i++)
            {
                int i_next = (i + 1) % n;
                d += Nodes[i].Cross(Nodes[i_next]);
            }

            float[] w = new float[n];
            for (int index = 0; index < n; index++)
            {
                w[index] = 0;
                for (int i = 0; i < n; i++)
                {
                    int i_next = (i + 1) % n;
                    if (index == i)
                    {
                        w[index] += point.Cross(Nodes[i_next]);
                    }
                    else if (index == i_next)
                    {
                        w[index] += Nodes[i].Cross(point);
                    }
                    else
                    {
                        w[index] += Nodes[i].Cross(Nodes[i_next]);
                    }
                }
                w[index] /= d;
            }
            return w;
        }

        public Vector2 GetPointFromCoord(float[] w)
        {
            Vector2 point = Vector2.Zero;
            for (int i = 0; i < Nodes.Length; i++)
            {
                point += w[i] * Nodes[i];
            }
            return FromLocal(point);
        }
        public override bool Contains(Vector2 point)
        {
            var w = GetCoordOfPoint(point);
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (!(w[i] >= 0 && w[i] <= 1))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true)
        {
            throw new NotImplementedException();
        }
    }
}
