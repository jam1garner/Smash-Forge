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
        // setup
        bool render = true;

        // View controls
        Matrix4 view;
        Camera cam;
        Matrix4 perspective;

        bool selecting = false;
        public float sx1, sy1;

        public int AnimationSpeed = 60;
        public bool isPlaying;

        // controls
        VertexTool vertexTool = new VertexTool();

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
            vertexTool.vp = this;
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
        
        public Vector2 getMouseOnViewPort()
        {
            float mouse_x = glViewport.PointToClient(Cursor.Position).X;
            float mouse_y = glViewport.PointToClient(Cursor.Position).Y;

            float mx = (2.0f * mouse_x) / glViewport.Width - 1.0f;
            float my = 1.0f - (2.0f * mouse_y) / glViewport.Height;
            return new Vector2(mx, my);
        }

        private void glViewport_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // selects object
            if (selecting)
            {

            }
        }

        private Vector3 getScreenPoint(Vector3 pos)
        {
            Vector4 n = Vector4.Transform(new Vector4(pos, 1), view);
            n.X /= n.W;
            n.Y /= n.W;
            n.Z /= n.W;
            return n.Xyz;
        }

        private void checkSelect()
        {
            if (selecting)
            {
                Vector2 m = getMouseOnViewPort();
                if (!m.Equals(new Vector2(sx1, sy1)))
                {
                    // select group of vertices
                    float minx = Math.Min(sx1, m.X);
                    float miny = Math.Min(sy1, m.Y);
                    float width = Math.Abs(sx1 - m.X);
                    float height = Math.Abs(sy1 - m.Y);

                    foreach (ModelContainer con in draw)
                    {
                        foreach (NUD.Mesh mesh in con.nud.mesh)
                        {
                            foreach (NUD.Polygon poly in mesh.Nodes)
                            {
                                //if (!poly.IsSelected && !mesh.IsSelected) continue;
                                int i = 0;
                                foreach (NUD.Vertex v in poly.vertices)
                                {
                                    poly.selectedVerts[i] = 0;
                                    Vector3 n = getScreenPoint(v.pos);
                                    if (n.X >= minx && n.Y >= miny && n.X <= minx + width && n.Y <= miny + height)
                                        poly.selectedVerts[i] = 1;
                                    i++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // single vertex
                    Ray r = RenderTools.createRay(view, getMouseOnViewPort());
                    Vector3 close = Vector3.Zero;
                    foreach (ModelContainer con in draw)
                    {
                        foreach (NUD.Mesh mesh in con.nud.mesh)
                        {
                            foreach (NUD.Polygon poly in mesh.Nodes)
                            {
                                //if (!poly.IsSelected && !mesh.IsSelected) continue;
                                int i = 0;
                                foreach (NUD.Vertex v in poly.vertices)
                                {
                                    if (!poly.IsSelected) continue;
                                    if (r.TrySphereHit(v.pos, 0.2f, out close))
                                        poly.selectedVerts[i] = 1;
                                    i++;
                                }
                            }
                        }
                    }
                }

                vertexTool.refresh();
                selecting = false;
            }
        }

        private void glViewport_Click(object sender, EventArgs e)
        {
            
        }

        private void glViewport_MouseUp(object sender, MouseEventArgs e)
        {
            checkSelect();
        }

        private void weightToolButton_Click(object sender, EventArgs e)
        {
            vertexTool.Show();
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
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            if (Runtime.renderBackGround)
            {
                GL.Begin(PrimitiveType.Quads);
                GL.Color3(Runtime.back1);
                GL.Vertex2(1.0, 1.0);
                GL.Vertex2(-1.0, 1.0);
                GL.Color3(Runtime.back2);
                GL.Vertex2(-1.0, -1.0);
                GL.Vertex2(1.0, -1.0);
                GL.End();
            }
            
            // set up the viewport projection and send it to GPU

            if (!OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.S))
            {
                selecting = false;
                if (glViewport.Focused && glViewport.ClientRectangle.Contains(PointToClient(Cursor.Position)))
                {
                    cam.Update();
                    view = cam.getViewMatrix() * perspective;
                }
                cam.TrackMouse();
            }
            else
            {
                if (OpenTK.Input.Mouse.GetState().IsButtonDown(OpenTK.Input.MouseButton.Left) && !selecting)
                {
                    selecting = true;
                    Vector2 m = getMouseOnViewPort();
                    sx1 = m.X;
                    sy1 = m.Y;
                }
            }

            GL.Enable(EnableCap.DepthTest);
            GL.ClearDepth(1.0);

            GL.LoadMatrix(ref view);
            // ready to start drawing model stuff
            GL.MatrixMode(MatrixMode.Modelview);

            if (Runtime.renderFloor)
            {
                    RenderTools.drawFloor();
            }

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

            GL.Enable(EnableCap.StencilTest);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

            // draw renderable objects

            foreach (ModelContainer m in draw)
            {
                RenderTools.DrawModel(m, view);
            }
            
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.DepthFunc(DepthFunction.Less);
            GL.AlphaFunc(AlphaFunction.Gequal, 0.1f);
            GL.Disable(EnableCap.CullFace);

            // clear the buffer bit so the skeleton 
            // will be drawn on top of everything
            GL.UseProgram(0);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // drawing the bones
            foreach (ModelContainer m in draw)
                if(m.vbn != null)
                    RenderTools.DrawVBN(m.vbn);
            
            // Mouse selection
            if (selecting)
            {
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Clear(ClearBufferMask.DepthBufferBit);
                
                if (OpenTK.Input.Mouse.GetState().IsButtonDown(OpenTK.Input.MouseButton.Right))
                {
                    selecting = false;
                }

                Vector2 m = getMouseOnViewPort();
                GL.Color3(Color.AliceBlue);
                GL.LineWidth(2f);
                GL.Begin(PrimitiveType.LineLoop);
                GL.Vertex2(sx1, sy1);
                GL.Vertex2(m.X, sy1);
                GL.Vertex2(m.X, m.Y);
                GL.Vertex2(sx1, m.Y);
                GL.End();
            }

            GL.PopAttrib();
            glViewport.SwapBuffers();
        }

    }
}
