using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace JA
{
    public static class Utils
    {
        public static float ClampValue(this float value, bool periodic = false)
        {
            return ClampValue(value, 0f, 1f, periodic);
        }

        public static float ClampValue(this float value, float min, float max, bool periodic=false)
        {
            if (periodic)
            {
                return min + ((value - min) % (max - min));
            }
            return Math.Max(min, Math.Min(max, value));
        }
        public static double ClampValue(this double value, bool periodic = false)
        {
            return ClampValue(value, 0.0, 1.0, periodic);
        }

        public static double ClampValue(this double value, double min, double max, bool periodic = false)
        {
            if (periodic)
            {
                return min + ((value - min) % (max - min));
            }
            return Math.Max(min, Math.Min(max, value));
        }

        public static float Quantize(this float value, float step, float origin = 0)
        {
            return origin + (float)Math.Round((value - origin) / step, MidpointRounding.AwayFromZero) * step;
        }
        public static double Quantize(this double value, double step, double origin = 0)
        {
            return origin + Math.Round((value - origin) / step, MidpointRounding.AwayFromZero) * step;
        }

        public static float Cross(this Vector2 u, Vector2 v)
            => u.X * v.Y - u.Y * v.X;

        public static Vector2 Orthogonal(this Vector2 vector) => new Vector2(-vector.Y, vector.X);
    }
}
