using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace VBN_Editor.GUI
{
    public partial class VBNViewport : GLControl
    {
        public VBNViewport()
        {
            InitializeComponent();
        }

        private void VBNViewport_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                GL.ClearColor(Color.LightSteelBlue);
                SetupViewPort();
            }
            render = true;
        }

        // for drawing
        public static Matrix4 scale = Matrix4.CreateScale(new Vector3(0.5f, 0.5f, 0.5f));
        Matrix4 v;
        float rot = 0;
        float lookup = 0;
        float height = 0;
        float width = 0;
        float zoom = 0;
        float mouseXLast = 0;
        float mouseYLast = 0;
        float mouseSLast = 0;
        bool render = false;

        private void SetupViewPort()
        {
            int h = Height;
            int w = Width;
            GL.LoadIdentity();
            GL.Viewport(0, 0, w, h);
            v = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1.0f, 40.0f);

            GotFocus += (object sender, EventArgs e) =>
            {
                mouseXLast = OpenTK.Input.Mouse.GetState().X;
                mouseYLast = OpenTK.Input.Mouse.GetState().Y;
                zoom = OpenTK.Input.Mouse.GetState().WheelPrecise;
                mouseSLast = zoom;
            };
        }
        private bool MouseIsOverViewport()
        {
            if (ClientRectangle.Contains(PointToClient(Cursor.Position)))
            {
                return true;
            }
            return false;
        }

        public void Render(VBN skeleton)
        {
            if (!render)
                return;
            // clear the gf buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // enable depth test for grid...
            GL.Enable(EnableCap.DepthTest);

            // set up the viewport projection and send it to GPU
            GL.MatrixMode(MatrixMode.Projection);

            if (Focused && MouseIsOverViewport())
            {
                if (OpenTK.Input.Mouse.GetState().IsButtonDown(OpenTK.Input.MouseButton.Right))
                {
                    height += 0.025f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                    width += 0.025f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
                    height = clampControl(height);
                    width = clampControl(width);
                }
                if (OpenTK.Input.Mouse.GetState().IsButtonDown(OpenTK.Input.MouseButton.Left))
                {
                    rot += 0.025f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
                    lookup += 0.025f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                    rot = clampControl(rot);
                    lookup = clampControl(lookup);
                }
                v = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1.0f, 40.0f);

                mouseXLast = OpenTK.Input.Mouse.GetState().X;
                mouseYLast = OpenTK.Input.Mouse.GetState().Y;

                zoom += OpenTK.Input.Mouse.GetState().WheelPrecise - mouseSLast;
                zoom = clampControl(zoom);
            }
            mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;

            GL.LoadMatrix(ref v);

            // ready to start drawing model stuff
            GL.MatrixMode(MatrixMode.Modelview);

            // draw the grid floor first
            drawGridFloor(Matrix4.CreateTranslation(Vector3.Zero));

            // clear the buffer bit so the skeleton will be drawn
            // on top of everything
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // drawing the bones
            if (skeleton != null)
            {
                foreach (Bone bone in skeleton.bones)
                {
                    // first calcuate the point and draw a point
                    GL.PointSize(3.5f);
                    GL.Color3(Color.Red);
                    GL.Begin(PrimitiveType.Points);
                    Vector3 pos_c = Vector3.Transform(Vector3.Zero, bone.transform * scale);
                    GL.Vertex3(pos_c);
                    GL.End();


                    // now draw line between parent 
                    GL.Color3(Color.Blue);
                    GL.LineWidth(1f);

                    GL.Begin(PrimitiveType.Lines);
                    if (bone.parentIndex != 0x0FFFFFFF && bone.parentIndex != -1)
                    {
                        int i = bone.parentIndex;
                        Vector3 pos_p = Vector3.Transform(Vector3.Zero, skeleton.bones[i].transform * scale);
                        GL.Vertex3(pos_c);
                        GL.Vertex3(pos_p);
                    }
                    GL.End();

                }
            }

            SwapBuffers();
        }
        private float clampControl(float f)
        {
            if (f < -5)
                f = -5;
            if (f > 5)
                f = 5;
            return f;
        }
        public void drawGridFloor(Matrix4 s)
        {
            // Dropping some grid lines
            GL.Disable(EnableCap.DepthTest);
            GL.Color3(Color.DarkGray);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(-10f, 0f, -10f);
            GL.Vertex3(10f, 0f, -10f);
            GL.Vertex3(10f, 0f, 10f);
            GL.Vertex3(-10f, 0f, 10f);
            GL.End();

            GL.Color3(Color.DimGray);
            GL.LineWidth(1f);
            GL.Begin(PrimitiveType.Lines);
            for (var i = -10; i <= 10; i++)
            {
                GL.Vertex3(Vector3.Transform(new Vector3(-10f, 0f, i), s));
                GL.Vertex3(Vector3.Transform(new Vector3(10f, 0f, i), s));
                GL.Vertex3(Vector3.Transform(new Vector3(i, 0f, -10f), s));
                GL.Vertex3(Vector3.Transform(new Vector3(i, 0f, 10f), s));
            }
            GL.End();
            GL.Enable(EnableCap.DepthTest);
        }

        private void VBNViewport_Resize(object sender, EventArgs e)
        {
            int h = Height;
            int w = Width;
            GL.LoadIdentity();
            GL.Viewport(0, 0, w, h);
            v = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1.0f, 40.0f);
        }
    }
}
