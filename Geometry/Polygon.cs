using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace JA.Geometry
{
    public readonly struct Polygon : IShape, IEquatable<Polygon>
    {
        public Polygon(Vector2[] nodes)
            : this(Vector2.Zero, Vector2.Zero, nodes) { }
        public Polygon(Vector2 center, Vector2 direction, Vector2[] nodes)
        {
            Center = center;
            Direction = direction;
            Nodes = nodes;
        }

        public Polygon ScaleNodes(float factor)
            => new Polygon(Center, Direction, Nodes.Select((n) => factor * n).ToArray());
        public Polygon MoveNodes(Vector2 delta)
            => new Polygon(Center, Direction, Nodes.Select((n) => delta + n).ToArray());
        public Polygon RotateNodes(float angle)
            => new Polygon(Center, Direction, Nodes.Select((n) => Vector2.Transform(n, Matrix3x2.CreateRotation(angle))).ToArray());
        public Polygon RotateNodes(float angle, Vector2 localPivot)
            => new Polygon(Center, Direction, Nodes.Select((n) => Vector2.Transform(n, Matrix3x2.CreateRotation(angle, localPivot))).ToArray());

        #region Properties
        public Vector2[] Nodes { get; }
        public Vector2 Center { get; }
        public Vector2 Direction { get; }
        public float Angle { get => (float)Math.Atan2(Direction.Y, Direction.X); }

        public Vector2[] GetNodes()
        {
            IObject obj = this;
            return Nodes.Select((v) => Transformations.FromLocal(obj, v)).ToArray();
        }

        public Segment[] GetSegments()
        {
            if (Nodes.Length < 2) return Array.Empty<Segment>();
            var segments = new Segment[Nodes.Length];
            for (int i = 0; i < segments.Length; i++)
            {
                int i_next = (i + 1) % Nodes.Length;
                var a = this.FromLocal(Nodes[i]);
                var b = this.FromLocal(Nodes[i_next]);
                segments[i] = new Segment(a, b);
            }
            return segments;
        }
        public Triangle[] GetTriangles()
        {
            Vector2 cg = GetCentroid();
            if (Nodes.Length < 2) return Array.Empty<Triangle>();
            var triangles = new Triangle[Nodes.Length];
            for (int i = 0; i < triangles.Length; i++)
            {
                int i_next = (i + 1) % Nodes.Length;
                var a = this.FromLocal(Nodes[i]);
                var b = this.FromLocal(Nodes[i_next]);
                var c = this.FromLocal(cg);
                triangles[i] = new Triangle(Center, Direction, c, a, b);
            }
            return triangles;
        }
        public Vector2 GetCentroid()
        {
            Vector2 point = Vector2.Zero;
            for (int i = 0; i < Nodes.Length; i++)
            {
                point += Nodes[i];
            }
            return point / Nodes.Length;
        }
        public Vector2 GetStartPoint() => Nodes.Length>0 ? Nodes[0] : Vector2.Zero;
        public float GetArea()
        {
            return GetTriangles().Sum((trig) => trig.GetArea());
        }
        #endregion

        #region Barycentric
        public float[] GetCoordOfPoint(Vector2 point)
        {
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
            return point;
        }

        #endregion

        #region Interactions
        public bool Contains(Vector2 point)
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
        public Vector2 GetClosestPointTo(Vector2 point)
        {
            if (Nodes.Length == 0) return point;
            if (Nodes.Length == 1) return this.FromLocal(Nodes[0]);
            Segment[] segments = GetSegments();
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
        public bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true)
        {
            var segments = GetSegments();
            float[] t_seg = new float[segments.Length];
            bool[] include = new bool[segments.Length];
            for (int i = 0; i < t_seg.Length; i++)
            {
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

            if (t_near < 0 && t_far < 0)
            {
                hit = ray.Origin;
                return false;
            }
            // select nearest of both postive, otherwise pick far (which should be +)
            var t = t_near >= 0 && t_far >= 0 ? nearest ? t_near : t_far : t_far;
            hit = ray.GetPointAlong(t);
            for (int i = 0; i < t_seg.Length; i++)
            {
                if (t == t_seg[i])
                {
                    var sign = Math.Sign(Vector2.Dot(ray.Direction, segments[i].Normal));
                    normal = -sign * segments[i].Normal;
                }
            }
            return true;
        }

        #endregion

        #region Equality
        public override bool Equals(object obj)
        {
            return obj is Polygon polygon
                && Equals(polygon);

        }
        public bool Equals(Polygon other)
        {
            return Enumerable.SequenceEqual(Nodes, other.Nodes);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return Nodes.Aggregate(249714186, (x, n) => 1768953197 * x + n.GetHashCode());
            }
        }

        public static bool operator ==(Polygon left, Polygon right) => left.Equals(right);

        public static bool operator !=(Polygon left, Polygon right) => !(left == right);

        #endregion

        public override string ToString() => $"Polygon(n={Nodes.Length})";
    }
}
