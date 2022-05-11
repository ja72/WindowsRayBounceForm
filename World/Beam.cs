using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JA.World
{
    public class Beam
    {
        public Beam(Ray center, int count, float width)
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
}
