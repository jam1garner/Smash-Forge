using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Smash_Forge.Rendering;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge.GUI.Menus
{
    public partial class UvViewer : Form
    {
        private List<NUD.Polygon> polygons;

        public UvViewer(List<NUD.Polygon> polygons)
        {
            InitializeComponent();
            this.polygons = polygons;
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            RenderTools.SetUpOpenTkRendering();
            if (polygons != null)
            {
                RenderTools.InitializeUVBufferData(polygons);
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            glControl1.MakeCurrent();
            GL.Viewport(glControl1.ClientRectangle);
            RenderTools.DrawTexturedQuad(RenderTools.uvTestPattern.Id);
            RenderTools.DrawUv();
            glControl1.SwapBuffers();
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            glControl1.Invalidate();
        }
    }
}
