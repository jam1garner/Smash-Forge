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

        private void loadImage(NUT selected)
        {
            NUT.NUD_Texture tex = selected.textures[0];
            selected.draw.Add(tex.id, NUT.loadImage(tex));
        }

        private void UIPreview_Load(object sender, EventArgs e)
        {
            //loadImage(chr_00);
            //loadImage(chr_11);
            //loadImage(chr_13);
            //loadImage(stock_90);
            RenderTexture(chr_00_renderer, chr_00);
            RenderTexture(chr_11_renderer, chr_11);
            RenderTexture(chr_13_renderer, chr_13);
            RenderTexture(stock_90_renderer, stock_90);
        }

        //TODO fix?
        private void RenderTexture(GLControl glControl1, NUT nut)
        {
            glControl1.MakeCurrent();
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcColor, BlendingFactorDest.OneMinusSrcAlpha);

            NUT.NUD_Texture tex = nut.textures[0];

            if (tex == null)
                return;

            int rt = nut.draw[tex.id];

            GL.BindTexture(TextureTarget.Texture2D, rt);
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(1, 1);
            GL.Vertex2(1, -1);
            GL.TexCoord2(0, 1);
            GL.Vertex2(-1, -1);
            GL.TexCoord2(0, 0);
            GL.Vertex2(-1, 1);
            GL.TexCoord2(1, 0);
            GL.Vertex2(1, 1);
            GL.End();

            glControl1.SwapBuffers();
        }
    }
}
