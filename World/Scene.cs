using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JA.World
{
    using JA.Gdi;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

    public class Scene : IDisposable
    {

        public Scene(Control target, float modelSize)
        {
            Stroke = new Pen(Color.White, 0);
            Fill = new SolidBrush(Color.Black);
            TextFont = new Font(SystemFonts.CaptionFont, FontStyle.Regular);
            Target = target;
            ModelSize = modelSize;
            Objects = new List<Object>();

            Scale = 1;
            dragFrom = Vector2.Zero;
            selection = -1;

            target.Paint += (s, ev) => Draw(ev.Graphics);
            target.Resize += (s, ev) => target.Invalidate();
            target.MouseDown += (s, ev) =>
            {
                var point = new Point(
                    ev.Location.X - Target.ClientRectangle.Left - Target.ClientRectangle.Width / 2,
                    ev.Location.Y - Target.ClientRectangle.Top - Target.ClientRectangle.Height / 2);
                MouseDown(point, ev.Button);
            };
            target.MouseUp += (s, ev) =>
            {
                var point = new Point(
                    ev.Location.X - Target.ClientRectangle.Left - Target.ClientRectangle.Width / 2,
                    ev.Location.Y - Target.ClientRectangle.Top - Target.ClientRectangle.Height / 2);
                MouseUp(point, ev.Button);
            };
            target.MouseMove += (s, ev) =>
            {
                var point = new Point(
                    ev.Location.X - Target.ClientRectangle.Left - Target.ClientRectangle.Width / 2,
                    ev.Location.Y - Target.ClientRectangle.Top - Target.ClientRectangle.Height / 2);
                MouseMove(point, ev.Button);
            };
        }

        public void Update(float elapsedSeconds)
        {
            for (int i = 0; i < Objects.Count; i++)
            {
                Objects[i].Angle += (float)(Math.PI / 144);
            }
            Target.Invalidate();
        }

        public Control Target { get; }
        public float ModelSize { get; set; }
        public List<Object> Objects { get; }
        public SolidBrush Fill { get; private set; }
        public Pen Stroke { get; private set; }
        public Font TextFont { get; private set; }
        public float Scale { get; private set; }

        Vector2 dragFrom, mousePos;
        int selection;
        Vector2 startPos;
        float startAngle;
        Ray ray;
        float rayAngle = 0;

        public void Add(Object item) => Objects.Add(item);
        public bool IsSelected(Object item) => selection >= 0 ? Objects[selection] == item : false;
        public void ResetStrokeAndFill()
        {
            Stroke.Color = Color.White;
            Stroke.Width = 0;
            Stroke.EndCap = LineCap.NoAnchor;
            Stroke.StartCap = LineCap.NoAnchor;
            Stroke.DashStyle = DashStyle.Solid;
            Fill.Color = Color.Black;
        }
        public void Draw(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            var view = Target.ClientRectangle;
            g.TranslateTransform(view.Left + view.Width / 2, view.Top + view.Height / 2);
            float sx = view.Width / ModelSize, sy = view.Height / ModelSize;
            Scale = Math.Min(sx, sy);

            for (int i = 0; i < Objects.Count; i++)
            {
                Objects[i].Draw(g, this);
            }
            ResetStrokeAndFill();

            Vector2 dir = Vector2.Transform(Vector2.UnitX, Matrix3x2.CreateRotation(rayAngle));
            ray = new Ray(mousePos, dir);

            var beams = new Beam(ray, 17, 1.2f);

            foreach (var beam in beams.GetRays())
            {
                float distance = ModelSize;
                Vector2 p = Vector2.Zero, n = Vector2.Zero;
                Object target = null;

                foreach (var item in Objects)
                {
                    if (item.Hit(beam, out var hit, out var normal))
                    {
                        var t = beam.GetDistanceAlong(hit);
                        if (t >= 0 && t < distance)
                        {
                            distance = Math.Min(distance, t);
                            p = hit;
                            n = normal;
                            target = item;
                        }
                    }
                }
                DrawRay(g, Color.Ivory.SetA(0.9f), beam, distance);
                if (target != null)
                {
                    //DrawRay(g, target.Color.Blend(Color.Ivory, 0.2f), new Ray(p, n), 0.5f);
                    if (beam.CanReflectFrom(p, n, out var reflect))
                    {
                        DrawRay(g, target.Color.Blend(Color.Ivory, 0.2f).SetA(0.8f), reflect, ModelSize / 2);
                    }
                    if (beam.CanRefractFrom(p, n, 1.3f, out var refract))
                    {
                        DrawRay(g, target.Color.Blend(Color.DarkSlateGray, 0.6f).SetA(0.8f), refract, ModelSize / 2);
                    }
                }
            }

            ResetStrokeAndFill();
        }

        #region Draw Primitives
        public void DrawPoint(Graphics g, Color color, Vector2 position, float size = 4f, string label = null, ContentAlignment align = ContentAlignment.TopLeft)
        {
            var point = GetPixel(position, Scale);
            Fill.Color = color;
            g.FillEllipse(Fill, point.X - size / 2, point.Y - size / 2, size, size);

            // Draw label
            if (!string.IsNullOrWhiteSpace(label))
            {
                float space = 2 * size;
                var tsz = g.MeasureString(label, SystemFonts.CaptionFont);
                float px, py;
                switch (align)
                {
                    case ContentAlignment.TopLeft:
                        px = point.X - tsz.Width - space;
                        py = point.Y - tsz.Height - space;
                        break;
                    case ContentAlignment.TopCenter:
                        px = point.X - tsz.Width / 2;
                        py = point.Y - tsz.Height - space;
                        break;
                    case ContentAlignment.TopRight:
                        px = point.X + space;
                        py = point.Y - tsz.Height - space;
                        break;
                    case ContentAlignment.MiddleLeft:
                        px = point.X - tsz.Width - space;
                        py = point.Y - tsz.Height / 2;
                        break;
                    case ContentAlignment.MiddleCenter:
                        px = point.X - tsz.Width / 2;
                        py = point.Y - tsz.Height / 2;
                        break;
                    case ContentAlignment.MiddleRight:
                        px = point.X + space;
                        py = point.Y - tsz.Height / 2;
                        break;
                    case ContentAlignment.BottomLeft:
                        px = point.X - tsz.Width - space;
                        py = point.Y + space;
                        break;
                    case ContentAlignment.BottomCenter:
                        px = point.X - tsz.Width / 2;
                        py = point.Y + space;
                        break;
                    case ContentAlignment.BottomRight:
                        px = point.X + space;
                        py = point.Y + space;
                        break;
                    default:
                        throw new NotSupportedException();
                }
                g.DrawString(label, SystemFonts.CaptionFont, Fill, px, py);
            }
        }

        public void DrawSegment(Graphics g, Color color, Segment segment)
        {
            var pxA = GetPixel(segment.A, Scale);
            var pxB = GetPixel(segment.B, Scale);
            Stroke.Color = color;
            Stroke.Width = 1;
            g.DrawLine(Stroke, pxA, pxB);
            DrawPoint(g, color, segment.A);
            DrawPoint(g, color, segment.B);
        }

        public void DrawRay(Graphics g, Color color, Ray ray, float distance = -1)
        {
            if (distance < 0)
            {
                var view = Target.ClientRectangle;
                distance = Math.Max(view.Width, view.Height) / Scale;
            }
            var px_origin = GetPixel(ray.Origin, Scale);
            var px_end = GetPixel(ray.GetPointAlong(distance), Scale);
            Fill.Color = color;
            g.FillEllipse(Fill, px_origin.X - 1, px_origin.Y - 1, 2, 2);
            Stroke.Color = color;
            Stroke.Width = 1;
            Stroke.CustomEndCap = new AdjustableArrowCap(1.5f, 4.5f);
            g.DrawLine(Stroke, px_origin, px_end);
            Stroke.EndCap = LineCap.NoAnchor;
        }
        internal void DrawPolygon(Graphics g, Color color, Vector2[] points)
        {
            var pixels = GetPixel(points, Scale);
            Fill.Color = color.SetA(0.4f);
            g.FillPolygon(Fill, pixels);
            Stroke.Color = color;
            Stroke.Width = 2;
            g.DrawPolygon(Stroke, pixels);
            Stroke.Width = 0;
        }
        internal void DrawCircle(Graphics g, Color color, Vector2 center, float radius)
        {
            var px_center = GetPixel(center, Scale);

            radius *= Scale;
            Fill.Color = color.SetA(0.4f);
            g.FillEllipse(Fill, px_center.X - radius, px_center.Y - radius, 2 * radius, 2 * radius);
            Stroke.Color = color;
            Stroke.Width = 2;
            g.DrawEllipse(Stroke, px_center.X - radius, px_center.Y - radius, 2 * radius, 2 * radius);
        }

        internal void DrawCircle(Graphics g, Color color, Vector2 center, float angle, float radius)
        {
            var other = center + Vector2.Transform(radius * Vector2.UnitX, Matrix3x2.CreateRotation(angle));
            var px_center = GetPixel(center, Scale);
            var px_other = GetPixel(other, Scale);

            radius *= Scale;

            Fill.Color = color.SetA(0.4f);
            Stroke.Color = color;
            Stroke.Width = 2;

            g.FillEllipse(Fill, px_center.X - radius, px_center.Y - radius, 2 * radius, 2 * radius);
            g.DrawEllipse(Stroke, px_center.X - radius, px_center.Y - radius, 2 * radius, 2 * radius);
            g.DrawLine(Stroke, px_center, px_other);
        }

        public void DrawEllipse(Graphics g, Color color, Vector2 center, float angle, float majorAxis, float minorAxis)
        {
            var other = center + Vector2.Transform(majorAxis * Vector2.UnitX, Matrix3x2.CreateRotation(angle));
            var px_center = GetPixel(center, Scale);
            var px_other = GetPixel(other, Scale);

            majorAxis *= Scale;
            minorAxis *= Scale;

            Fill.Color = color.SetA(0.4f);
            Stroke.Color = color;
            Stroke.Width = 2;

            var state = g.Save();
            g.TranslateTransform(px_center.X, px_center.Y);
            g.RotateTransform(-(float)(angle * 180 / Math.PI));
            g.FillEllipse(Fill, -majorAxis, -minorAxis, 2 * majorAxis, 2 * minorAxis);
            g.DrawEllipse(Stroke, -majorAxis, -minorAxis, 2 * majorAxis, 2 * minorAxis);
            g.Restore(state);
            g.DrawLine(Stroke, px_center, px_other);
        }

        #endregion

        #region Viewport
        public static PointF GetPixel(Vector2 position, float scale)
        {
            return new PointF(position.X * scale, -position.Y * scale);
        }
        public static PointF[] GetPixel(Vector2[] nodes, float scale)
        {
            PointF[] points = new PointF[nodes.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = GetPixel(nodes[i], scale);
            }
            return points;
        }
        public static Vector2 GetPoint(PointF pixel, float scale)
        {
            return new Vector2(pixel.X / scale, -pixel.Y / scale);
        }
        public static Vector2[] GetPoint(PointF[] pixel, float scale)
        {
            Vector2[] positions = new Vector2[pixel.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = GetPoint(pixel[i], scale);
            }
            return positions;
        }
        #endregion

        #region Mouse Events
        internal void MouseDown(Point location, MouseButtons button)
        {
            dragFrom = GetPoint(location, Scale);
            mousePos = dragFrom;

            if (button == MouseButtons.Left || button == MouseButtons.Right)
            {
                selection = -1;
                for (int i = 0; i < Objects.Count; i++)
                {
                    if (Objects[i].Contains(dragFrom))
                    {
                        selection = i;
                        break;
                    }
                }
                if (button == MouseButtons.Right && selection == -1)
                {
                    rayAngle += (float)(Math.PI / 36);
                }
            }
            if (selection >= 0)
            {
                startPos = Objects[selection].Position;
                startAngle = Objects[selection].Angle;
            }
            Target.Invalidate();
        }

        internal void MouseUp(Point location, MouseButtons button)
        {
            selection = -1;
            Target.Invalidate();
        }

        internal void MouseMove(Point location, MouseButtons button)
        {
            mousePos = GetPoint(location, Scale);
            if (selection >= 0)
            {
                if (button == MouseButtons.Left)
                {
                    var dragTo = GetPoint(location, Scale);

                    Objects[selection].Position = startPos + (dragTo - dragFrom);

                    Target.Invalidate();
                }
                else if (button == MouseButtons.Right)
                {
                    var dragTo = GetPoint(location, Scale);
                    var cen = Objects[selection].Position;
                    var angle1 = Math.Atan2(dragFrom.Y - cen.Y, dragFrom.X - cen.X);
                    var angle2 = Math.Atan2(dragTo.Y - cen.Y, dragTo.X - cen.X);
                    Objects[selection].Angle = startAngle + (float)(angle2 - angle1).Quantize(Math.PI / 48);

                }
            }
            Target.Invalidate();
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Fill.Dispose();
                    Stroke.Dispose();
                    TextFont.Dispose();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
