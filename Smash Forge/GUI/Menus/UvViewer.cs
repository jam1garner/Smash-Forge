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
        private NUD nud;

        public UvViewer(NUD nud)
        {
            InitializeComponent();
            this.nud = nud;
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            RenderTools.SetUpOpenTkRendering();
            if (nud != null)
            {
                NudUvRendering.InitializeUVBufferData(nud);
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            glControl1.MakeCurrent();
            GL.Viewport(glControl1.ClientRectangle);
            RenderTools.DrawTexturedQuad(RenderTools.uvTestPattern.Id);
            NudUvRendering.DrawUv(nud, glControl1.Width, glControl1.Height);
            glControl1.SwapBuffers();
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            glControl1.Invalidate();
        }
    }
}
