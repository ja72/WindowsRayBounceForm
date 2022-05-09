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

            float distance = ModelSize / 2;

            {
                var seg1 = new Segment(new Vector2(1f, -3f), new Vector2(3f, -1f));
                DrawSegment(g, Color.Purple, seg1);
                if (seg1.Hit(ray, out var hit1, out var n1))
                {
                    //DrawPoint(g, Color.Gold, hit1, 8);
                    DrawRay(g, Color.Gold, new Ray(hit1, n1), 1f);
                }

                var seg2 = seg1.Offset(2 * Vector2.UnitX).Flip();
                DrawSegment(g, Color.Brown, seg2);
                if (seg2.Hit(ray, out var hit2, out var n2))
                {
                    //DrawPoint(g, Color.Wheat, hit2, 8);
                    DrawRay(g, Color.Wheat, new Ray(hit2, n2), 1f);
                }
            }

            for (int i = 0; i < Objects.Count; i++)
            {
                if (Objects[i] is Box prism)
                {
                    if (prism.Hit(ray, out var hit, out var normal))
                    {
                        var t = ray.GetDistanceAlong(hit);
                        if (t >= 0 && t<distance)
                        {
                            distance = Math.Min(distance, t);
                            DrawPoint(g, Color.Red, hit, 8, t.ToString("f2"));
                            DrawRay(g, Color.Red, new Ray(hit, normal), 1f);
                            DrawRay(g, Color.White, ray.ReflectFrom(hit, normal), ModelSize / 2);
                        }
                    }
                }
                if (Objects[i] is Circle circle)
                {
                    if (circle.Hit(ray, out var hit, out var normal))
                    {
                        var t = ray.GetDistanceAlong(hit);
                        if (t >= 0 && t<distance)
                        {
                            distance = Math.Min(distance, t);
                            DrawPoint(g, Color.Yellow, hit, 8, t.ToString("f2"));
                            DrawRay(g, Color.Yellow, new Ray(hit, normal), 1f);
                            DrawRay(g, Color.White, ray.ReflectFrom(hit, normal), ModelSize/2);
                        }
                    }
                }
            }
            DrawRay(g, Color.White, ray, distance);
            ResetStrokeAndFill();
        }

        public void DrawPoint(Graphics g, Color color, Vector2 position, float size = 4f, string label = null, ContentAlignment align = ContentAlignment.TopLeft)
        {
            var point = GetPoint(position, Scale);
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
            var pxA = GetPoint(segment.A, Scale);
            var pxB = GetPoint(segment.B, Scale);
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
                distance = Math.Max(view.Width, view.Height)/Scale;
            }
            var px_origin = GetPoint(ray.Origin, Scale);
            var px_end = GetPoint(ray.GetPointAlong(distance), Scale);
            Fill.Color = color;
            g.FillEllipse(Fill, px_origin.X - 2, px_origin.Y - 2, 4, 4);
            Stroke.Color = color;
            Stroke.Width = 1;
            Stroke.CustomEndCap = new AdjustableArrowCap(3f, 9f);
            g.DrawLine(Stroke, px_origin, px_end);
            Stroke.EndCap = LineCap.NoAnchor;
        }

        internal void DrawPolygon(Graphics g, Object item, Vector2[] nodes)
        {
            nodes = item.FromLocal(nodes);
            var points = GetPoint(nodes, Scale);
            var color = item.Color;

            if (IsSelected(item))
            {
                color = color.AddH(0.12f);
            }

            Fill.Color = color.SetA(0.4f);
            g.FillPolygon(Fill, points);
            Stroke.Color = color;
            Stroke.Width = 2;
            g.DrawPolygon(Stroke, points);
        }
        internal void DrawCircle(Graphics g, Object item, float radius)
        {
            var center = item.Position;
            var other = item.FromLocal(radius * Vector2.UnitX);
            radius = Vector2.Distance(center, other) * Scale;

            var px_center = Scene.GetPoint(center, Scale);
            var px_other = Scene.GetPoint(other, Scale);

            var color = item.Color;

            if (IsSelected(item))
            {
                color = color.AddH(0.12f);
            }
            Fill.Color = color.SetA(0.4f);
            g.FillEllipse(Fill, px_center.X - radius, px_center.Y - radius, 2 * radius, 2 * radius);
            Stroke.Color = color;
            Stroke.Width = 2;
            g.DrawEllipse(Stroke, px_center.X - radius, px_center.Y - radius, 2 * radius, 2 * radius);
            g.DrawLine(Stroke, px_center, px_other);
        }

        public static PointF GetPoint(Vector2 position, float scale)
        {
            return new PointF(position.X * scale, -position.Y * scale);
        }
        public static PointF[] GetPoint(Vector2[] nodes, float scale)
        {
            PointF[] points = new PointF[nodes.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = GetPoint(nodes[i], scale);
            }
            return points;
        }
        public static Vector2 GetPosition(PointF point, float scale)
        {
            return new Vector2(point.X / scale, -point.Y / scale);
        }
        public static Vector2[] GetPosition(PointF[] points, float scale)
        {
            Vector2[] positions = new Vector2[points.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = GetPosition(points[i], scale);
            }
            return positions;
        }

        internal void MouseDown(Point location, MouseButtons button)
        {
            dragFrom = GetPosition(location, Scale);
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
            mousePos = GetPosition(location, Scale);
            if (selection >= 0)
            {
                if (button == MouseButtons.Left)
                {
                    var dragTo = GetPosition(location, Scale);

                    Objects[selection].Position = startPos + (dragTo - dragFrom);

                    Target.Invalidate();
                }
                else if (button == MouseButtons.Right)
                {
                    var dragTo = GetPosition(location, Scale);
                    var cen = Objects[selection].Position;
                    var angle1 = Math.Atan2(dragFrom.Y - cen.Y, dragFrom.X - cen.X);
                    var angle2 = Math.Atan2(dragTo.Y - cen.Y, dragTo.X - cen.X);
                    Objects[selection].Angle = startAngle + (float)(angle2 - angle1).Quantize(Math.PI / 48);

                }
            }
            Target.Invalidate();
        }

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
