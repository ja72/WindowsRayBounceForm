using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JA.World
{
    using JA.Gdi;
    using JA.Geometry;

    public interface IBeam
    {
        Ray Center { get; set; }
        int Count { get; set; }
        Color Color { get; set; }
        IEnumerable<GraphicsRay> GetGraphicsRays(float maxDistance);
    }

    public class ParallelRays : IBeam
    {
        public ParallelRays(Color color, Ray center, int count, float width)
        {
            Color = color;
            Center = center;
            Count = count;
            Width = width;
        }

        public Ray Center { get; set; }
        public int Count { get; set; }
        public float Width { get; set; }
        public Color Color { get; set; }

        public IEnumerable<GraphicsRay> GetGraphicsRays(float maxDistance)
        {
            if (Count > 1)
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return new GraphicsRay( Center.Parallel(Width * (i / (Count - 1f) - 0.5f)), Color, maxDistance);
                }
            }
            else if (Count == 1)
            {
                yield return new GraphicsRay(Center, Color, maxDistance);
            }
        }
    }
    public class RadialRays : IBeam
    {
        public RadialRays(Color color, Ray center, int count)
        {
            Color = color;
            Center = center;
            Count = count;
        }
        public float Angle { get => (float)(2 * Math.PI / Count); }
        public Ray Center { get; set; }
        public int Count { get; set; }
        public Color Color { get; set; }
        public IEnumerable<GraphicsRay> GetGraphicsRays(float maxDistance)
        {
            for (int i = 0; i < Count; i++)
            {
                yield return new GraphicsRay(Center.Rotate(i*Angle), Color.SetA(0.6f), maxDistance);
            }
        }
    }
}
