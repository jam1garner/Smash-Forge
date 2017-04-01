using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge
{

    public partial class ModelViewport : DockContent
    {

        public class Camera
        {
            Vector3 pos = new Vector3(0, -10, -30);
            float rot = 0, lookup = 0;

            public float mouseSLast, mouseYLast, mouseXLast;

            public Camera()
            {
                mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;
                mouseXLast = OpenTK.Input.Mouse.GetState().X;
                mouseYLast = OpenTK.Input.Mouse.GetState().Y;
            }

            public Matrix4 getViewMatrix()
            {
                return Matrix4.CreateRotationY(0.5f * rot) *
                    Matrix4.CreateRotationX(0.2f * lookup) *
                    Matrix4.CreateTranslation(pos);
            }

            public void Update()
            {
                float zoomscale = 1;

                if ((OpenTK.Input.Mouse.GetState().RightButton == OpenTK.Input.ButtonState.Pressed))
                {
                    pos.Y -= 0.15f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                    pos.X += 0.15f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
                }
                if ((OpenTK.Input.Mouse.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed))
                {
                    rot += 0.025f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
                    lookup += 0.025f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                }

                if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ShiftLeft) || OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ShiftRight))
                    zoomscale = 6;

                if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.Down))
                    pos.Z -= 1 * zoomscale;
                if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.Up))
                    pos.Z += 1 * zoomscale;

                pos.Z += (OpenTK.Input.Mouse.GetState().WheelPrecise - mouseSLast) * zoomscale;
            }

            public void TrackMouse()
            {
                mouseXLast = OpenTK.Input.Mouse.GetState().X;
                mouseYLast = OpenTK.Input.Mouse.GetState().Y;
                mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;
            }
        }

        // setup
        bool render = true;

        // View controls
        Matrix4 view;
        Camera cam;
        Matrix4 perspective;

        public int AnimationSpeed = 60;
        public bool isPlaying;

        // visuals
        Color back1 = Color.FromArgb((255 << 24) | (26 << 16) | (26 << 8) | (26));
        Color back2 = Color.FromArgb((255 << 24) | (77 << 16) | (77 << 8) | (77));


        // Contents
        public List<ModelContainer> draw = new List<ModelContainer>();

        
        public ModelViewport()
        {
            InitializeComponent();
            cam = new Camera();
        }

        private void ModelViewport_Load(object sender, EventArgs e)
        {
            Setup();
            //Application.Idle += Application_Idle;
            var timer = new Timer();
            timer.Interval = 1000 / 60;
            timer.Tick += new EventHandler(Application_Idle);
            timer.Start();
        }

        ~ModelViewport()
        {
            //Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            if (this.IsDisposed)
                return;

            if (render)
            {
                if (isPlaying)
                {
                    if (currentFrame.Value < totalFrame.Value)
                        currentFrame.Value++;
                    else
                    {
                        /*if (!checkBox2.Checked)
                        {
                            isPlaying = false;
                            playButton.Text = "Play";
                        }
                        else*/
                        {
                            currentFrame.Value = 0;
                        }
                    }
                }
                Render();
                //System.Threading.Thread.Sleep(1000 / AnimationSpeed);
            }

        }

        private void ModelViewport_Resize(object sender, EventArgs e)
        {
            perspective = Matrix4.CreatePerspectiveFieldOfView(Runtime.fov, glViewport.Width / (float)glViewport.Height, 1.0f, Runtime.renderDepth);
        }

        public void Setup()
        {
            perspective = Matrix4.CreatePerspectiveFieldOfView(Runtime.fov, glViewport.Width / (float)glViewport.Height, 1.0f, Runtime.renderDepth);
            view = cam.getViewMatrix() * perspective;
        }
        
        // Rendering
        private void Render()
        {
            if (!render)
                return;

            glViewport.MakeCurrent();

            GL.LoadIdentity();
            GL.Viewport(glViewport.ClientRectangle);
            
            // Push all attributes so we don't have to clean up later
            GL.PushAttrib(AttribMask.AllAttribBits);
            // clear the gf buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            if (Runtime.renderBackGround)
            {
                GL.Begin(PrimitiveType.Quads);
                GL.Color3(back1);
                GL.Vertex2(1.0, 1.0);
                GL.Vertex2(-1.0, 1.0);
                GL.Color3(back2);
                GL.Vertex2(-1.0, -1.0);
                GL.Vertex2(1.0, -1.0);
                GL.End();
            }
            
            // set up the viewport projection and send it to GPU
            GL.MatrixMode(MatrixMode.Projection);

            if (glViewport.Focused && glViewport.ClientRectangle.Contains(PointToClient(Cursor.Position)))
            {
                cam.Update();
                view = cam.getViewMatrix() * perspective;
            }
            cam.TrackMouse();

            GL.Enable(EnableCap.DepthTest);
            GL.ClearDepth(1.0);

            GL.LoadMatrix(ref view);
            // ready to start drawing model stuff
            GL.MatrixMode(MatrixMode.Modelview);

            RenderTools.drawFloor(Matrix4.CreateTranslation(Vector3.Zero));
            
            GL.Enable(EnableCap.Normalize);  // These is critical to have
            GL.Enable(EnableCap.RescaleNormal);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Gequal, 0.1f);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            GL.Enable(EnableCap.LineSmooth);

            foreach (ModelContainer m in draw)
            {
                RenderTools.DrawModel(m, view);
            }
            
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.DepthFunc(DepthFunction.Less);
            GL.AlphaFunc(AlphaFunction.Gequal, 0.1f);
            GL.Disable(EnableCap.CullFace);

            GL.UseProgram(0);
            // clear the buffer bit so the skeleton 
            // will be drawn on top of everything
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // drawing the bones
            foreach (ModelContainer m in draw)
                if(m.vbn != null)
                    RenderTools.DrawVBN(m.vbn);

            GL.PopAttrib();
            glViewport.SwapBuffers();
        }

    }
}
