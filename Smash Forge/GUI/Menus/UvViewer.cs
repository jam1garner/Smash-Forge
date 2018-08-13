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
using SFGraphics.GLObjects;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge.GUI.Menus
{
    public partial class UvViewer : Form
    {
        private NUD sourceNud;
        private NUD.Polygon polygonToRender;

        private VertexArrayObject screenVao;

        public UvViewer(NUD sourceNud, NUD.Polygon polygonToRender)
        {
            // We need the nud to generate buffers due to the way nud rendering works.
            InitializeComponent();
            this.sourceNud = sourceNud;
            this.polygonToRender = polygonToRender;
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            OpenTKSharedResources.InitializeSharedResources();
            if (OpenTKSharedResources.SetupStatus == OpenTKSharedResources.SharedResourceStatus.Initialized)
            {
                screenVao = ScreenDrawing.CreateScreenTriangleVao();
                if (sourceNud != null)
                {
                    NudUvRendering.InitializeUVBufferData(sourceNud);
                }
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (OpenTKSharedResources.SetupStatus != OpenTKSharedResources.SharedResourceStatus.Initialized)
                return;

            glControl1.MakeCurrent();
            GL.Viewport(glControl1.ClientRectangle);
            // Draw darker to make the UVs visible.
            ScreenDrawing.DrawTexturedQuad(RenderTools.uvTestPattern.Id, 0.5f, screenVao);
            NudUvRendering.DrawUv(polygonToRender, glControl1.Width, glControl1.Height);
            glControl1.SwapBuffers();
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            glControl1.Invalidate();
        }
    }
}
