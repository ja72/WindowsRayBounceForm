using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JA.Gdi
{
    public static class Colors
    {
        public static Color SetL(this Color color, float l)
        {
            var hsl = ColorHsl.FromColor(color);
            hsl = new ColorHsl(hsl.A, hsl.H, hsl.S, l);
            return hsl.ToColor();
        }
        public static Color SetS(this Color color, float s)
        {
            var hsl = ColorHsl.FromColor(color);
            hsl = new ColorHsl(hsl.A, hsl.H, s, hsl.L);
            return hsl.ToColor();
        }
        public static Color SetH(this Color color, float h)
        {
            var hsl = ColorHsl.FromColor(color);
            hsl = new ColorHsl(hsl.A, h, hsl.S, hsl.L);
            return hsl.ToColor();
        }
        public static Color SetSL(this Color color, float s, float l)
        {
            var hsl = ColorHsl.FromColor(color);
            hsl = new ColorHsl(hsl.A, hsl.H, s, l);
            return hsl.ToColor();
        }
        public static Color SetA(this Color color, float a)
        {
            return Color.FromArgb((int)(a * 255), color);
        }
        public static Color AddH(this Color color, float h)
        {
            var hsl = ColorHsl.FromColor(color);
            hsl = new ColorHsl(hsl.A, hsl.H + h * 360, hsl.S, hsl.L);
            return hsl.ToColor();
        }

        public static Color Blend(this Color color, Color target, float amount)
        {
            var src = ColorHsl.FromColor(color);
            var dst = ColorHsl.FromColor(target);

            return new ColorHsl(
                src.A + (dst.A - src.A) * amount,
                src.H + (dst.H - src.H) * amount,
                src.S + (dst.S - src.S) * amount,
                src.L + (dst.L - src.L) * amount).ToColor();
        }
    }
}
