using System.Drawing;
using System.Numerics;
using System;

namespace JA.World
{
    using JA.Gdi;
    using JA.Geometry;

    public class PolygonGraphics : GraphicsObject
    {
        public PolygonGraphics(Color color, Vector2 position, float angle, params Vector2[] nodes)
            : base(color, position, angle)
        {
            Nodes = nodes;
        }

        public Vector2[] Nodes { get; }

        public Vector2[] GetNodes(bool local = true)
        {
            if (!local)
            {
                return FromLocal(GetNodes(true));
            }
            return Nodes;
        }
        public Segment[] GetSegments(bool local = true)
        {
            var nodes = GetNodes(local);
            if (nodes.Length < 2) return Array.Empty<Segment>();
            Segment[] segments = new Segment[nodes.Length];
            for (int i = 0; i < segments.Length; i++)
            {
                int i_next = (i + 1) % nodes.Length;
                segments[i] = new Segment(nodes[i], nodes[i_next]);
            }
            return segments;
        }

        #region Interactions
        public override Vector2 GetClosestPointTo(Vector2 point)
        {
            var nodes = GetNodes(false);
            if (nodes.Length == 0) return point;
            if (nodes.Length == 1) return nodes[0];
            Segment[] segments = GetSegments(false);
            Vector2[] closePoints = new Vector2[segments.Length];
            float[] closeDist = new float[segments.Length];
            for (int i = 0; i < closePoints.Length; i++)
            {
                closePoints[i] = segments[i].GetClosestPointTo(point);
                closeDist[i] = Vector2.Distance(closePoints[i], point);
            }
            float minDist = closeDist[0];
            Vector2 minPoint = closePoints[0];
            for (int i = 1; i < closeDist.Length; i++)
            {
                if (minDist > closeDist[i])
                {
                    minDist = closeDist[i];
                    minPoint = closePoints[i];
                }
            }
            return minPoint;
        }
        public override bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true)
        {
            var segments = GetSegments(false);
            float[] t_seg = new float[segments.Length];
            bool[] include = new bool[segments.Length];
            for (int i = 0; i < t_seg.Length; i++)
            {
                //t_seg[i] = ray.GetDistanceToLine(segments[i].Normal, segments[i].Offset);
                if (segments[i].Hit(ray, out hit, out normal))
                {
                    include[i] = true;
                    t_seg[i] = ray.GetDistanceAlong(hit);
                }
                else
                {
                    include[i] = false;
                }
            }

            float t_near = 0, t_far = 0;
            bool first = false;
            for (int i = 0; i < t_seg.Length; i++)
            {
                if (include[i] && !first)
                {
                    t_near = t_seg[i];
                    t_far = t_seg[i];
                    first = true;
                }
                else if (include[i])
                {
                    t_near = Math.Min(t_near, t_seg[i]);
                    t_far = Math.Max(t_far, t_seg[i]);
                }
            }
            // if far is smaller than near then we did not hit the box
            normal = Vector2.Zero;
            if (t_far < t_near || !first)
            {
                hit = ray.Origin;
                return false;
            }

            // select near (default) or far point
            var t = nearest ? t_near : t_far;
            hit = ray.GetPointAlong(t);

            for (int i = 0; i < t_seg.Length; i++)
            {
                if (t == t_seg[i])
                {
                    normal = -segments[i].Normal;
                }
            }
            return true;
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

        #endregion

        #region Drawing

        public override void Draw(Graphics g, GraphicsScene scene)
        {
            var color = scene.IsSelected(this) ? Color.AddH(0.12f) : Color;
            scene.DrawPolygon(g, color, GetNodes(false));
        }

        #endregion
    }
}
