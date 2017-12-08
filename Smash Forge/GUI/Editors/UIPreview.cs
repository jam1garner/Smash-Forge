using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using WeifenLuo.WinFormsUI.Docking;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge
{
    public partial class UIPreview : DockContent
    {
        public UIPreview(NUT chr_00, NUT chr_11, NUT chr_13, NUT stock_90)
        {
            InitializeComponent();
            this.chr_00 = chr_00;
            this.chr_11 = chr_11;
            this.chr_13 = chr_13;
            this.stock_90 = stock_90;
        }

        NUT chr_00, chr_11, chr_13, stock_90;

        private void chr_00_renderer_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture(chr_00_renderer, chr_00);
        }

        private void chr_11_renderer_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture(chr_11_renderer, chr_11);
        }

        private void chr_13_renderer_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture(chr_13_renderer, chr_13);
        }

        private void UIPreview_Load(object sender, EventArgs e)
        {
        }

        private void stock_90_renderer_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture(stock_90_renderer, stock_90);
        }
        
        private void RenderTexture(GLControl glControl1, NUT nut)
        {
            if (nut == null) return;
            glControl1.MakeCurrent();
            GL.Viewport(glControl1.ClientRectangle);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.White);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);

            foreach(NUT.NUD_Texture tex in nut.textures)
            {
                RenderTools.DrawTexturedQuad(nut.draw[tex.id], tex.width, tex.height, true, true, true, true, false, true);
            }

            glControl1.SwapBuffers();
        }
    }
}
