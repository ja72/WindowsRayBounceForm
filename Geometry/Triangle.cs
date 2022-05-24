using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace JA.Geometry
{
    public readonly struct Triangle : IShape, IEquatable<Triangle>
    {
        readonly (Vector2 a, Vector2 b, Vector2 c) data;
        public Triangle(Vector2 a, Vector2 b, Vector2 c)
            : this(Vector2.Zero, Vector2.Zero, a, b, c) { }
        public Triangle(Vector2 center, Vector2 direction, Vector2 a, Vector2 b, Vector2 c)
        {
            Center = center;
            Direction = direction;
            data = (a, b, c);
        }

        #region Properties
        public Vector2 A { get => data.a; }
        public Vector2 B { get => data.b; }
        public Vector2 C { get => data.c; }
        public Vector2 Center { get; }
        public Vector2 Direction { get; }
        public float Angle { get => (float)Math.Atan2(Direction.Y, Direction.X); }

        public Segment[] GetSegments()
            => new Segment[] { new Segment(A, B), new Segment(B, C), new Segment(C, A) };

        public Polygon ToPolygon() 
            => new Polygon(Center, Direction, new Vector2[] { A, B, C });

        public float GetArea()
        {
            return (A.Cross(B) + B.Cross(C) + C.Cross(A)) / 2;
        }
        public int GetNormal()
        {
            return Math.Sign(A.Cross(B) + B.Cross(C) + C.Cross(A));
        }
        public Vector2 GetCentroid() => this.FromLocal((A + B + C) / 3);

        public Vector2 GetStartPoint() => this.FromLocal(A);

        #endregion

        #region Barycentric
        public float[] GetCoordOfPoint(Vector2 point)
        {
            point = this.ToLocal(point);
            float d = A.Cross(B) + B.Cross(C) + C.Cross(A);
            float w_a = point.Cross(B) + B.Cross(C) + C.Cross(point);
            float w_b = A.Cross(point) + point.Cross(C) + C.Cross(A);
            float w_c = A.Cross(B) + B.Cross(point) + point.Cross(A);

            return new float[] { w_a / d, w_b / d, w_c / d };
        }

        public Vector2 GetPointFromCoord(params float[] w)
        {
            if (w.Length != 3)
            {
                throw new ArgumentException("Expecting three coordinates.", nameof(w));
            }
            return this.FromLocal(w[0] * A + w[1] * B + w[2] * C);
        }

        #endregion

        #region Interaction

        public bool Contains(Vector2 point)
        {
            var w = GetCoordOfPoint(point);

            return w[0] >= 0 && w[0] <= 1
                && w[1] >= 0 && w[1] <= 1
                && w[2] >= 0 && w[2] <= 1;
        }

        public Vector2 GetClosestPointTo(Vector2 point)
        {
            return ToPolygon().GetClosestPointTo(point);
        }
        public bool Hit(Ray ray, out Vector2 hit, out Vector2 normal, bool nearest = true)
        {
            return ToPolygon().Hit(ray, out hit, out normal, nearest);
        }

        #endregion

        #region Equality
        public override bool Equals(object obj)
        {
            return obj is Triangle triangle 
                && Equals(triangle);
        }
        public bool Equals(Triangle other)=> data.Equals(other.data);
        public override int GetHashCode()
        {
            return 1768953197 + data.GetHashCode();
        }
        public static bool operator ==(Triangle left, Triangle right) => left.Equals(right);
        public static bool operator !=(Triangle left, Triangle right) => !left.Equals(right);
        #endregion

        public override string ToString() => $"Triangle({string.Join(",",GetSegments().Select((s)=>s.Length))})";
    }
}
