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
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge
{
    public partial class BNTXMaterialTextureEditor : Form
    {

        public BRTI Texture = null;
        public bool _loaded;

        public BNTXMaterialTextureEditor()
        {
            InitializeComponent();
            glControl1.Scroll += mouseWheel;
        }
        public void LoadTexture(BRTI tex)
        {
            RenderTexture(tex);
        }

        float rot = 0;
        float lookup = 0;
        float height = 1;
        float width = 0;
        float zoom = -20f;
        float mouseXLast = 0;
        float mouseYLast = 0;
        float mouseSLast = 0;

        private void RenderTexture(BRTI tex)
        {
            if (!_loaded || glControl1 == null)
                return;

            glControl1.MakeCurrent();
            GL.Viewport(glControl1.ClientRectangle);

            if (tex == null)
            {
                glControl1.SwapBuffers();
                return;
            }

            int width = tex.Width;
            int height = tex.Height;

            int texture = tex.display;

            //    Rendering.RenderTools.DrawTexturedQuad(texture, width, height);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Enable(EnableCap.DepthTest);
            GL.ClearDepth(1.0);


            Matrix4 v = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, zoom) * Matrix4.CreatePerspectiveFieldOfView(1.3f, glControl1.Width / (float)glControl1.Height, 1.0f, 1000.0f);

            GL.MatrixMode(MatrixMode.Projection);

            GL.LoadMatrix(ref v);

            GL.MatrixMode(MatrixMode.Modelview);


            glControl1.SwapBuffers();

            if (!Runtime.shaders["Texture"].hasCheckedCompilation())
            {
                Runtime.shaders["Texture"].DisplayCompilationWarning("Texture");
            }
        }

        private void mouseWheel(object sender, ScrollEventArgs e)
        {
            zoom += (e.NewValue - e.OldValue);
            glControl1.Invalidate();
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            _loaded = true;
        }
    }
}
