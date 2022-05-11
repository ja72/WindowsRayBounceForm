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
        public static float Sqrt(float x) => (float)Math.Sqrt(x);
        public static float Sqr(float x) => x * x;
        public static float GetNorm(float x, float y)
        {
            x = Math.Abs(x);
            y = Math.Abs(y);
            float m = Math.Max(x, y);
            if (x == m)
            {
                float r = y / x;
                return m * Sqrt(1 + r * r);
            }
            else
            {
                float r = x / y;
                return m * Sqrt(1 + r * r);
            }
        }
        public static bool QuadraticRoot(float c0, float c1, float c2, out float x1, out float x2)
        {
            if (c2 == 0)
            {
                if (c1 != 0)
                {
                    x1 = x2 = -c0 / c1;
                    return true;
                }
                else
                {
                    x1 = x2 = 0;
                    return c0 == 0;
                }
            }
            if (c0 == 0)
            {
                if (c2 != 0)
                {
                    x1 = 0;
                    x2 = -c1 / c2;
                    return true;
                }
                else
                {
                    x1 = x2 = 0;
                    return true;
                }
            }
            c1 /= 2;
            float r1 = c1 / c2, r0 = c0 / c2;
            float d = r1 * r1 - r0;
            float s = Sqrt(r1 * r1 - r0);
            if (d >= 0)
            {
                x1 = -s - r1;
                x2 = +s - r1;
                return true;
            }
            else
            {
                x1 = x2 = -r1;
                return false;
            }
        }

        public static float ClampValue(this float value, bool periodic = false)
        {
            return ClampValue(value, 0f, 1f, periodic);
        }

        public static float ClampValue(this float value, float min, float max, bool periodic = false)
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

        /// <summary>
        /// Projects a vector along a direction
        /// </summary>
        /// <param name="vector">A vector</param>
        /// <param name="direction">The direction/axis to project to</param>
        /// <param name="perpendicular">The perpendicular vector to the direction</param>
        /// <returns>The parallel vector to the direction</returns>
        public static Vector2 Project(this Vector2 vector, Vector2 direction, out Vector2 perpendicular)
        {
            var parallel = direction * Vector2.Dot(direction, vector);
            perpendicular = vector - parallel;
            return parallel;
        }
    }
}
