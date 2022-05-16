using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JA.Geometry
{
    public interface IBeam
    {
        Ray Center { get; set; }
        int Count { get; set; }

        IEnumerable<Ray> GetRays();
    }

    public class ParallelRays : IBeam
    {
        public ParallelRays(Ray center, int count, float width)
        {
            Center = center;
            Count = count;
            Width = width;
        }

        public Ray Center { get; set; }
        public int Count { get; set; }
        public float Width { get; set; }

        public IEnumerable<Ray> GetRays()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return Center.Parallel(Width * (i / (Count - 1f) - 0.5f));
            }
        }
    }
    public class RadialRays : IBeam
    {
        public RadialRays(Ray center, int count)
        {
            Center = center;
            Count = count;
        }
        public float Angle { get => (float)(2 * Math.PI / Count); }
        public Ray Center { get; set; }
        public int Count { get; set; }

        public IEnumerable<Ray> GetRays()
        {
            for (int i = 0; i < Count; i++)
            {
                var R = Matrix3x2.CreateRotation(i * Angle);
                var dir = Vector2.Transform(Center.Direction, R);
                yield return new Ray(Center.Origin, dir);                    
            }
        }
    }
}
