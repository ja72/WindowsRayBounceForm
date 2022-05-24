using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JA
{
    using JA.Gdi;
    using JA.World;
    using System.Diagnostics;

    public partial class MainForm : Form
    {
        static readonly float deg = (float)(Math.PI / 180);

        GraphicsScene scene;
        Timer timer;
        Stopwatch sw;

        public MainForm()
        {
            InitializeComponent();

            scene = new GraphicsScene(pictureBox1, 10f);
            timer = new Timer();
            timer.Interval = 25;
            sw = new Stopwatch();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //scene.AddShape(Color.Red, 2 * Vector2.UnitX - 3f * Vector2.UnitY, 0,
            //    new Geometry.Box(1f, 1f));
            //scene.AddShape(Color.Blue, 2 * Vector2.UnitX - 1f * Vector2.UnitY, 0,
            //    new Geometry.Triangle(
            //        new Vector2(-1f, -0.5f),
            //        new Vector2(1f, -0.5f),
            //        new Vector2(0f, 1f)));
            scene.AddShape(Color.Green, 0 * Vector2.UnitX + 2f * Vector2.UnitY, 0,
                new Geometry.Polygon(new Vector2[] {
                new Vector2(-1f, 0f),
                new Vector2(-0.5f, -0.5f),
                new Vector2(0.5f, -0.5f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f)}));

            scene.AddShape(Color.Cyan, -2 * Vector2.UnitX, 15 * deg,
                new Geometry.Ellipse(1.75f, 0.35f));

            scene.AddShape(Color.DarkSeaGreen, 2 * Vector2.UnitX, 15 * deg,
                new Geometry.Circle(0.75f));

            //scene.AddShapes(Color.Crimson, 2 * Vector2.UnitX, 0,
            //    new Geometry.Box(-3 * Vector2.UnitY, Vector2.UnitX, 1f, 1f),
            //    new Geometry.Triangle(-1 * Vector2.UnitY, Vector2.UnitX,
            //        new Vector2(-1f, -0.5f),
            //        new Vector2(1f, -0.5f),
            //        new Vector2(0f, 1f)),
            //    new Geometry.Polygon(+1 * Vector2.UnitY, Vector2.UnitX,
            //        new Vector2[] {
            //            new Vector2(-1f, 0f),
            //            new Vector2(-0.5f, -0.5f),
            //            new Vector2(0.5f, -0.5f),
            //            new Vector2(1f, 0f),
            //            new Vector2(0f, 1f),
            //            new Vector2(-1f, 0f)}
            //        ));
            //scene.AddShapes(Color.Tomato, 2 * Vector2.UnitX, 0,
            //    new Geometry.Box(.6f * Vector2.UnitY, Vector2.UnitX, 1.25f, 0.75f),
            //    new Geometry.Box(-.6f * Vector2.UnitY, Vector2.UnitX, 1.25f, 0.75f)
            //    );

            //scene.AddShapes(Color.Yellow, -2*Vector2.UnitX, 0,
            //    new Geometry.Circle(1 * Vector2.UnitY, 0.75f),
            //    new Geometry.Circle(-1 * Vector2.UnitY, 0.75f)
            //    );
            //scene.AddShape(Color.Cyan, -2 * Vector2.UnitX, 15 * deg,
            //    new Geometry.Ellipse(1.75f, 0.35f));

            sw.Start();
            timer.Tick += (s, ev) => { scene.Update((float)sw.Elapsed.TotalSeconds); sw.Restart(); };
            timer.Start();
        }
    }
}
