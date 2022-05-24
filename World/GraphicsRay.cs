using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace JA.World
{
    using JA.Geometry;
    using static JA.Utils;
    public class GraphicsRay
    {
        public GraphicsRay(Ray ray, Color color, float distance)
        {
            Ray = ray;
            Color = color;
            Distance = distance;
        }

        public Ray Ray { get; set; }
        public Color Color { get; set; }
        public float Distance {get; set; }

        public void Draw(Graphics g, GraphicsScene scene)
        {
            scene.DrawRay(g, Color, Ray, Distance);
        }
    }
}
