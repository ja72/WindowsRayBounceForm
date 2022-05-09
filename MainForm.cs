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

    public partial class MainForm : Form
    {
        Scene scene;

        public MainForm()
        {
            InitializeComponent();

            scene = new Scene(pictureBox1, 10f);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            scene.Add(new Box(Color.Red, -2*Vector2.UnitX, 0, 1f, 1f));

            scene.Add(new Triangle(Color.Blue, 2*Vector2.UnitY, 0,
                new Vector2(-1f, -0.5f),
                new Vector2(1f, -0.5f),
                new Vector2(0f, 1f)));

            scene.Add(new Polygon(Color.Green, -2*Vector2.UnitY, 0,
                new Vector2(-1f, 0f),
                new Vector2(-0.5f, -0.5f),
                new Vector2(0.5f, -0.5f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(-1f, 0f)));

            scene.Add(new Circle(Color.Yellow, 2*Vector2.UnitX, 0, 0.75f));

        }
    }
}
