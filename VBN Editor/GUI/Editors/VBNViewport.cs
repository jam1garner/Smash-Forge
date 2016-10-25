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
using System.Security.Cryptography;
using SALT.Scripting.AnimCMD;
using OpenTK.Graphics;
using System.Diagnostics;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;

namespace VBN_Editor
{
    public partial class VBNViewport : DockContent
    {
        public VBNViewport()
        {
            InitializeComponent();
            this.TabText = "Renderer";
            Hitboxes = new SortedList<int, Hitbox>();
            Application.Idle += Application_Idle;
            Runtime.AnimationChanged += Runtime_AnimationChanged;
        }

        // Explicitly unsubscribe from the static event to 
        // prevent hanging Garbage Collection.
        ~VBNViewport()
        {
            Runtime.AnimationChanged -= Runtime_AnimationChanged;
        }

        public bool _controlLoaded;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                SetupViewPort();
            }
            render = true;
            _controlLoaded = true;
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!DesignMode && _controlLoaded)
            {
                GL.LoadIdentity();
                GL.Viewport(glControl1.ClientRectangle);


                v = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1.0f, 500.0f);
            }
        }

        #region Members
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
        bool isPlaying = false;
        Shader shader;
        #endregion

        #region Event Handlers
        private void Application_Idle(object sender, EventArgs e)
        {
            // Fix accessing the control after 
            // the editor has been closed 
            if (Runtime.killWorkspace)
            {
                foreach (ModelContainer n in Runtime.ModelContainers)
                {
                    n.Destroy();
                }
                foreach (NUT n in Runtime.TextureContainers)
                {
                    n.Destroy();
                }
                Runtime.ModelContainers = new List<ModelContainer>();
                Runtime.TextureContainers = new List<NUT>();
                Runtime.TargetVBN = null;
                Runtime.TargetAnim = null;
                Runtime.TargetLVD = null;
                Runtime.TargetPath = null;
                Runtime.TargetCMR0 = null;
                Runtime.TargetNUD = null;
                Runtime.killWorkspace = false;
                Runtime.Animations = new Dictionary<string, SkelAnimation>();
            }

            if (this.IsDisposed == true)
                return;

            while (glControl1.IsIdle && render && _controlLoaded)
            {
                if (isPlaying)
                {
                    if (nupdFrame.Value < nupdMaxFrame.Value)
                    {
                        nupdFrame.Value++;
                    }
                    else if (nupdFrame.Value == nupdMaxFrame.Value)
                    {
                        if (!checkBox2.Checked)
                        {
                            isPlaying = false;
                            btnPlay.Text = "Play";
                        }
                        else
                        {
                            nupdFrame.Value = 0;
                        }
                    }
                }
                Render();
                System.Threading.Thread.Sleep(1000 / AnimationSpeed);
            }
        }
        private void Runtime_AnimationChanged(object sender, EventArgs e)
        {
            loadAnimation(Runtime.TargetAnim);
        }

        private void btnFirstFrame_Click(object sender, EventArgs e)
        {
            this.nupdFrame.Value = 0;
        }
        private void btnPrevFrame_Click(object sender, EventArgs e)
        {
            if (this.nupdFrame.Value - 1 >= 0)
                this.nupdFrame.Value -= 1;
        }
        private void btnLastFrame_Click(object sender, EventArgs e)
        {
            if (Runtime.TargetAnim != null)
                this.nupdFrame.Value = Runtime.TargetAnim.size() - 1;
        }
        private void btnNextFrame_Click(object sender, EventArgs e)
        {
            if (Runtime.TargetAnim != null)
                this.nupdFrame.Value += 1;
        }
        private void btnPlay_Click(object sender, EventArgs e)
        {
            // If we're already at final frame and we hit play again
            // start from the beginning of the anim
            if (nupdMaxFrame.Value == nupdFrame.Value)
                nupdFrame.Value = 0;

            isPlaying = !isPlaying;
            if (isPlaying)
            {
                btnPlay.Text = "Pause";
            }
            else
            {
                btnPlay.Text = "Play";
            }
        }
        private void nupdFrame_ValueChanged(Object sender, EventArgs e)
        {
            if (Runtime.TargetAnim == null)
                return;

            if (this.nupdFrame.Value >= Runtime.TargetAnim.size())
            {
                this.nupdFrame.Value = 0;
                return;
            }
            if (this.nupdFrame.Value < 0)
            {
                this.nupdFrame.Value = Runtime.TargetAnim.size() - 1;
                return;
            }
            Runtime.TargetAnim.setFrame((int)this.nupdFrame.Value);

            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if (m.vbn != null)
                    Runtime.TargetAnim.nextFrame(m.vbn);
            }

            Frame = (int)this.nupdFrame.Value;

            if (script != null)
                ProcessFrame();
        }
        private void nupdSpeed_ValueChanged(object sender, EventArgs e)
        {
            AnimationSpeed = (int)nupdFrameRate.Value;
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateMousePosition();
        }

        public event EventHandler FrameChanged;
        protected virtual void OnFrameChanged(EventArgs e)
        {
            if (FrameChanged != null)
                FrameChanged(this, e);
            //HandleACMD(Runtime.TargetAnimString);
        }
        #endregion

        #region Properties
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal int Frame
        {
            get
            {
                return _frame;
            }
            set
            {
                _frame = value;
                OnFrameChanged(new EventArgs());
            }
        }
        private int _frame = 0;

        [DefaultValue(60)]
        internal int AnimationSpeed { get { return _animSpeed; } set { _animSpeed = value; } }
        private int _animSpeed = 60;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private SortedList<int, Hitbox> Hitboxes { get; set; }
        #endregion

        #region Rendering

        string vs = "#version 330\n \nin vec3 vPosition;\nin vec3 vColor;\nin vec3 vNormal;\nin vec2 vUV;\nin vec4 vBone;\nin vec4 vWeight;\n\nout vec2 f_texcoord;\nout float normal;\nout vec4 color;\n\nuniform mat4 modelview;\nuniform mat4 bones[150];\n \nvoid\nmain()\n{\n    ivec4 index = ivec4(vBone); \n\n    vec4 objPos = vec4(vPosition.xyz, 1.0);\n\n    if(vBone.x != -1){\n        objPos = bones[index.x] * vec4(vPosition, 1.0) * vWeight.x;\n        objPos += bones[index.y] * vec4(vPosition, 1.0) * vWeight.y;\n        objPos += bones[index.z] * vec4(vPosition, 1.0) * vWeight.z;\n        objPos += bones[index.w] * vec4(vPosition, 1.0) * vWeight.w;\n    } \n\n    gl_Position = modelview * vec4(objPos.xyz, 1.0);\n\n    vec3 distance = (objPos.xyz + vec3(5, 5, 5))/2;\n\n    f_texcoord = vUV;\n    normal = dot(vec4(vNormal, 1.0), vec4(0.15,0.15,0.15,1.0)) ;// vec4(distance, 1.0)\n    color = vec4( vColor, 1.0);\n}";

        string fs = "#version 330\n\nin vec2 f_texcoord;\nin vec4 color;\nin float normal;\n\nuniform sampler2D tex;\n\nvoid\nmain()\n{\n    vec4 ambiant = vec4(0.3,0.3,0.3,1.0) * texture(tex, f_texcoord).rgba;\n\n    vec4 alpha = texture2D(tex, f_texcoord).aaaa;\n    vec4 outputColor = ambiant + (vec4(texture(tex, f_texcoord).rgb, 1) * normal);\n    gl_FragColor = vec4((alpha*outputColor).xyz, alpha.x);\n}";

        private void SetupViewPort()
        {

            if (shader != null)
                GL.DeleteShader(shader.programID);

            if (shader == null)
            {
                shader = new Shader();

                {
                    shader.vertexShader(vs);
                    shader.fragmentShader(fs);

                    shader.addAttribute("vPosition", false);
                    shader.addAttribute("vColor", false);
                    shader.addAttribute("vNormal", false);
                    shader.addAttribute("vUV", false);
                    shader.addAttribute("vBone", false);
                    shader.addAttribute("vWeight", false);

                    shader.addAttribute("tex", true);
                    shader.addAttribute("modelview", true);
                    shader.addAttribute("bones", true);
                }
            }

            int h = Height - groupBox2.ClientRectangle.Top;
            int w = Width;
            GL.LoadIdentity();
            GL.Viewport(glControl1.ClientRectangle);
            v = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1.0f, 2500.0f);

            GotFocus += (object sender, EventArgs e) =>
                {
                    mouseXLast = OpenTK.Input.Mouse.GetState().X;
                    mouseYLast = OpenTK.Input.Mouse.GetState().Y;
                    zoom = OpenTK.Input.Mouse.GetState().WheelPrecise;
                    mouseSLast = zoom;
                };
        }

        int cf = 0;

        public void Render()
        {
            if (!render)
                return;

            GL.ClearColor(Color.AliceBlue);
            // Push all attributes so we don't have to clean up later
            GL.PushAttrib(AttribMask.AllAttribBits);
            // clear the gf buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.ClearDepth(1.0);


            // set up the viewport projection and send it to GPU
            GL.MatrixMode(MatrixMode.Projection);

            if (IsMouseOverViewport() && glControl1.Focused)
                UpdateMousePosition();

            SetCameraAnimation();

            GL.LoadMatrix(ref v);
            // ready to start drawing model stuff
            GL.MatrixMode(MatrixMode.Modelview);


            GL.UseProgram(0);
            // drawing floor---------------------------
            if (Runtime.renderFloor)
                RenderTools.drawFloor(Matrix4.CreateTranslation(Vector3.Zero));


            GL.Enable(EnableCap.LineSmooth); // This is Optional 
            GL.Enable(EnableCap.Normalize);  // These is critical to have
            GL.Enable(EnableCap.RescaleNormal);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Gequal, 0.1f);


            // draw models
            if (Runtime.renderModel)
                DrawModels();

            GL.UseProgram(0);
            // draw path.bin
            if (Runtime.renderPath)
                DrawPathDisplay();
            // clear the buffer bit so the skeleton 
            // will be drawn on top of everything
            GL.Clear(ClearBufferMask.DepthBufferBit);
            // draw lvd
            if (Runtime.renderLVD)
                DrawLVD();
            // drawing the bones
            if (Runtime.renderBones)
                DrawBones();

            // Clean up
            GL.PopAttrib();
            glControl1.SwapBuffers();
        }

        public void UpdateMousePosition()
        {
            if ((OpenTK.Input.Mouse.GetState().RightButton == OpenTK.Input.ButtonState.Pressed))
            {
                height += 0.025f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                width += 0.025f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
                //height = clampControl(height);
                //width = clampControl(width);
            }
            if ((OpenTK.Input.Mouse.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed))
            {
                rot += 0.025f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
                lookup += 0.025f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
            }

            mouseXLast = OpenTK.Input.Mouse.GetState().X;
            mouseYLast = OpenTK.Input.Mouse.GetState().Y;

            zoom += OpenTK.Input.Mouse.GetState().WheelPrecise - mouseSLast;
            mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;

            v = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1.0f, 2500.0f);
        }
        public bool IsMouseOverViewport()
        {
            if (glControl1.ClientRectangle.Contains(PointToClient(Cursor.Position)))
                return true;
            else
                return false;
        }

        private void SetCameraAnimation()
        {
            if (Runtime.TargetPath != null && checkBox1.Checked)
            {
                if (cf >= Runtime.TargetPath.Frames.Count)
                    cf = 0;
                pathFrame f = Runtime.TargetPath.Frames[cf];
                v = (Matrix4.CreateTranslation(f.x, f.y, f.z) * Matrix4.CreateFromQuaternion(new Quaternion(f.qx, f.qy, f.qz, f.qw))).Inverted() * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1.0f, 90000.0f);
                cf++;
            }
            else if (Runtime.TargetCMR0 != null && checkBox1.Checked)
            {
                if (cf >= Runtime.TargetCMR0.frames.Count)
                    cf = 0;
                Matrix4 m = Runtime.TargetCMR0.frames[cf].Inverted();
                v = Matrix4.CreateTranslation(m.M14, m.M24, m.M34) * Matrix4.CreateFromQuaternion(m.ExtractRotation()) * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1.0f, 90000.0f);
                cf++;
            }
        }

        private void DrawModels()
        {
            GL.UseProgram(shader.programID);
            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if (m.nud != null)
                {
                    GL.UniformMatrix4(shader.getAttribute("modelview"), false, ref v);

                    if (m.vbn != null)
                    {
                        m.vbn.getShaderMatrix();
                        int shad = shader.getAttribute("bone");
                        GL.UniformMatrix4(shad, m.vbn.f.Length, false, m.vbn.f);
                    }

                    shader.enableAttrib();
                    m.nud.Render(shader);
                    shader.disableAttrib();
                }
            }
        }

        private void DrawBones()
        {
            if (Runtime.ModelContainers.Count > 0)
            {
                // Render the hitboxes
                if (!string.IsNullOrEmpty(Runtime.TargetAnimString))
                    HandleACMD(Runtime.TargetAnimString.Substring(4));

                RenderHitboxes();

                foreach (ModelContainer m in Runtime.ModelContainers)
                {
                    if (m.vbn != null)
                    {
                        foreach (Bone bone in m.vbn.bones)
                        {
                            // first calcuate the point and draw a point
                            GL.Color3(Color.GreenYellow);

                            Vector3 pos_c = Vector3.Transform(Vector3.Zero, bone.transform);
                            RenderTools.drawCube(pos_c, .085f);

                            // now draw line between parent 
                            GL.Color3(Color.Blue);
                            GL.LineWidth(1f);

                            GL.Begin(PrimitiveType.Lines);
                            if (bone.parentIndex != 0x0FFFFFFF && bone.parentIndex != -1)
                            {
                                int i = bone.parentIndex;
                                Vector3 pos_p = Vector3.Transform(Vector3.Zero, m.vbn.bones[i].transform);
                                GL.Vertex3(pos_c);
                                GL.Vertex3(pos_p);
                            }
                            GL.End();
                        }
                    }
                }
            }
        }

        public void DrawLVD()
        {
            if (Runtime.TargetLVD != null)
            {
                if (Runtime.renderCollisions)
                {
                    foreach (Collision c in Runtime.TargetLVD.collisions)
                    {
                        // draw the ground quads
                        int dir = 1;
                        int cg = 0;
                        GL.LineWidth(3);
                        GL.Color4(Color.FromArgb(100, Color.Red));
                        GL.Begin(PrimitiveType.Quads);
                        foreach (Vector2D vi in c.verts)
                        {
                            GL.Vertex3(vi.x, vi.y, 5 * dir);
                            GL.Vertex3(vi.x, vi.y, -5 * dir);
                            if (cg > 0)
                            {
                                GL.Vertex3(vi.x, vi.y, 5 * dir);
                                GL.Vertex3(vi.x, vi.y, -5 * dir);
                            }
                            cg++;
                            dir *= -1;
                        }
                        GL.End();


                        // draw outside borders
                        GL.Color3(Color.DarkRed);
                        GL.Begin(PrimitiveType.LineStrip);
                        foreach (Vector2D vi in c.verts)
                        {
                            GL.Vertex3(vi.x, vi.y, 5);
                        }
                        GL.End();
                        GL.Begin(PrimitiveType.LineStrip);
                        foreach (Vector2D vi in c.verts)
                        {
                            GL.Vertex3(vi.x, vi.y, -5);
                        }
                        GL.End();


                        // draw vertices
                        GL.Color3(Color.White);
                        GL.Begin(PrimitiveType.Lines);
                        foreach (Vector2D vi in c.verts)
                        {
                            GL.Vertex3(vi.x, vi.y, 5);
                            GL.Vertex3(vi.x, vi.y, -5);
                        }
                        GL.End();
                    }
                }

                if (Runtime.renderItemSpawners)
                {
                    foreach (ItemSpawner c in Runtime.TargetLVD.items)
                    {
                        foreach (Section s in c.sections)
                        {
                            // draw the item spawn quads
                            int dir = 1;
                            int cg = 0;
                            GL.LineWidth(2);

                            // draw outside borders
                            GL.Color3(Color.Black);
                            GL.Begin(PrimitiveType.LineStrip);
                            foreach (Vector2D vi in s.points)
                            {
                                GL.Vertex3(vi.x, vi.y, 5);
                            }
                            GL.End();
                            GL.Begin(PrimitiveType.LineStrip);
                            foreach (Vector2D vi in s.points)
                            {
                                GL.Vertex3(vi.x, vi.y, -5);
                            }
                            GL.End();


                            // draw vertices
                            GL.Color3(Color.White);
                            GL.Begin(PrimitiveType.Lines);
                            foreach (Vector2D vi in s.points)
                            {
                                GL.Vertex3(vi.x, vi.y, 5);
                                GL.Vertex3(vi.x, vi.y, -5);
                            }
                            GL.End();
                        }
                    }
                }

                if (Runtime.renderSpawns)
                {
                    foreach (Point s in Runtime.TargetLVD.spawns)
                    {
                        GL.Color4(Color.FromArgb(100, Color.Blue));
                        GL.Begin(PrimitiveType.QuadStrip);
                        GL.Vertex3(s.x - 3f, s.y, 0f);
                        GL.Vertex3(s.x + 3f, s.y, 0f);
                        GL.Vertex3(s.x - 3f, s.y + 10f, 0f);
                        GL.Vertex3(s.x + 3f, s.y + 10f, 0f);
                        GL.End();
                    }
                }

                if (Runtime.renderRespawns)
                {
                    foreach (Point s in Runtime.TargetLVD.respawns)
                    {
                        GL.Color4(Color.FromArgb(100, Color.Blue));
                        GL.Begin(PrimitiveType.QuadStrip);
                        GL.Vertex3(s.x - 3f, s.y, 0f);
                        GL.Vertex3(s.x + 3f, s.y, 0f);
                        GL.Vertex3(s.x - 3f, s.y + 10f, 0f);
                        GL.Vertex3(s.x + 3f, s.y + 10f, 0f);
                        GL.End();

                        //Draw respawn platform
                        GL.Color4(Color.FromArgb(200, Color.Gray));
                        GL.Begin(PrimitiveType.Triangles);
                        GL.Vertex3(s.x - 5, s.y, 0);
                        GL.Vertex3(s.x + 5, s.y, 0);
                        GL.Vertex3(s.x, s.y, 5);

                        GL.Vertex3(s.x - 5, s.y, 0);
                        GL.Vertex3(s.x + 5, s.y, 0);
                        GL.Vertex3(s.x, s.y, -5);

                        GL.Vertex3(s.x - 5, s.y, 0);
                        GL.Vertex3(s.x, s.y - 5, 0);
                        GL.Vertex3(s.x, s.y, 5);

                        GL.Vertex3(s.x + 5, s.y, 0);
                        GL.Vertex3(s.x, s.y - 5, 0);
                        GL.Vertex3(s.x, s.y, -5);

                        GL.Vertex3(s.x + 5, s.y, 0);
                        GL.Vertex3(s.x, s.y - 5, 0);
                        GL.Vertex3(s.x, s.y, 5);

                        GL.Vertex3(s.x - 5, s.y, 0);
                        GL.Vertex3(s.x, s.y - 5, 0);
                        GL.Vertex3(s.x, s.y, -5);
                        GL.End();

                        GL.Color4(Color.FromArgb(200, Color.Black));
                        GL.Begin(PrimitiveType.Lines);
                        GL.Vertex3(s.x - 5, s.y, 0);
                        GL.Vertex3(s.x, s.y - 5, 0);
                        GL.Vertex3(s.x + 5, s.y, 0);
                        GL.Vertex3(s.x, s.y - 5, 0);

                        GL.Vertex3(s.x, s.y, -5);
                        GL.Vertex3(s.x, s.y - 5, 0);
                        GL.Vertex3(s.x, s.y, 5);
                        GL.Vertex3(s.x, s.y - 5, 0);

                        GL.Vertex3(s.x, s.y, -5);
                        GL.Vertex3(s.x + 5, s.y, 0);
                        GL.Vertex3(s.x, s.y, -5);
                        GL.Vertex3(s.x - 5, s.y, 0);

                        GL.Vertex3(s.x, s.y, 5);
                        GL.Vertex3(s.x + 5, s.y, 0);
                        GL.Vertex3(s.x, s.y, 5);
                        GL.Vertex3(s.x - 5, s.y, 0);

                        GL.End();
                    }
                }

                if (Runtime.renderGeneralPoints)
                {
                    foreach (Point g in Runtime.TargetLVD.generalPoints)
                    {
                        GL.Color4(Color.FromArgb(200, Color.Fuchsia));
                        RenderTools.drawCubeWireframe(new Vector3(g.x, g.y, 0), 3);
                    }
                }

                if (Runtime.renderOtherLVDEntries)
                {
                    GL.Color4(Color.FromArgb(128, Color.Yellow));
                    foreach (Sphere s in Runtime.TargetLVD.damageSpheres)
                    {
                        RenderTools.drawSphere(new Vector3(s.x, s.y, s.z), s.radius, 24);
                    }

                    foreach (Capsule c in Runtime.TargetLVD.damageCapsules)
                    {
                        RenderTools.drawCylinder(new Vector3(c.x, c.y, c.z), new Vector3(c.x + c.dx, c.y + c.dy, c.z + c.dz), c.r);
                    }
                }


            }
        }

        private void DrawPathDisplay()
        {
            if (Runtime.TargetPath != null && !checkBox1.Checked)
            {
                GL.Color3(Color.Yellow);
                GL.LineWidth(2);
                for (int i = 1; i < Runtime.TargetPath.Frames.Count; i++)
                {
                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex3(Runtime.TargetPath.Frames[i].x, Runtime.TargetPath.Frames[i].y, Runtime.TargetPath.Frames[i].z);
                    GL.Vertex3(Runtime.TargetPath.Frames[i - 1].x, Runtime.TargetPath.Frames[i - 1].y, Runtime.TargetPath.Frames[i - 1].z);
                    GL.End();
                }
            }

            if (Runtime.TargetCMR0 != null && !checkBox1.Checked)
            {
                GL.Color3(Color.Yellow);
                GL.LineWidth(2);
                for (int i = 1; i < Runtime.TargetCMR0.frames.Count; i++)
                {
                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex3(Runtime.TargetCMR0.frames[i].M14, Runtime.TargetCMR0.frames[i].M24, Runtime.TargetCMR0.frames[i].M34);
                    GL.Vertex3(Runtime.TargetCMR0.frames[i - 1].M14, Runtime.TargetCMR0.frames[i - 1].M24, Runtime.TargetCMR0.frames[i - 1].M34);
                    GL.End();
                }
            }
        }

        public void RenderHitboxes()
        {
            if (Hitboxes.Count > 0)
            {
                GL.Color4(Color.FromArgb(85, Color.Red));
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.Blend);

                foreach (var pair in Hitboxes)
                {
                    var h = pair.Value;
                    var va = new Vector3(h.X, h.Y, h.Z);

                    int bid = h.Bone;
                    int gr = 0;
                    if (bid > 1000)
                    {
                        /*while (bid >= 1000)
                        {
                            bid -= 1000;
                            gr++;
                        }
                        gr = gr % 2;
                        //bid = bid >> 6;
                        Console.WriteLine(h.Bone + " " + gr + " " + bid);*/
                        bid >>= 8;
                    }

                    Bone b = new Bone();

                    if (h.Bone != -1)
                    {
                        foreach (ModelContainer m in Runtime.ModelContainers)
                        {
                            if (m.vbn != null)
                            {
                                if (m.vbn.jointTable.Count < 1)
                                    b = m.vbn.bones[bid];
                                else
                                {
                                    b = m.vbn.bones[m.vbn.jointTable[gr][bid]];
                                }
                            }
                        }
                    }

                    va = Vector3.Transform(va, b.transform.ClearScale());


                    // Draw angle marker
                    /*GL.LineWidth(7f);
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color3(Color.Black);
                    GL.Vertex3(va);
                    GL.Vertex3(va + Vector3.Transform(new Vector3(0,0,h.Size), Matrix4.CreateRotationX(-h.Angle * ((float)Math.PI / 180f))));
                    GL.End();*/

                    GL.Color4(Color.FromArgb(85, Color.Red));

                    switch (h.Type)
                    {
                        case Hitbox.HITBOX:
                            GL.Color4(Color.FromArgb(85, Color.Red));
                            break;
                        case Hitbox.GRABBOX:
                            GL.Color4(Color.FromArgb(85, Color.Purple));
                            break;
                        case Hitbox.WINDBOX:
                            GL.Color4(Color.FromArgb(85, Color.Blue));
                            break;
                    }

                    GL.DepthMask(false);
                    if (h.Extended)
                    {
                        var va2 = new Vector3(h.X2, h.Y2, h.Z2);

                        if (h.Bone != -1)
                            va2 = Vector3.Transform(va2, b.transform.ClearScale());

                        RenderTools.drawCylinder(va, va2, h.Size);
                    }
                    else
                    {
                        RenderTools.drawSphere(va, h.Size, 30);
                    }
                }

                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.DepthTest);
            }
        }

        ACMDScript script;

        public void ProcessFrame()
        {
            Hitboxes.Clear();
            int halt = 0;
            int e = 0;
            int setLoop = 0;
            int iterations = 0;
            var cmd = script[e];

            while (halt < Frame)
            {
                switch (cmd.Ident)
                {
                    case 0x42ACFE7D: // Asynchronous Timer
                        {
                            halt = (int)(float)cmd.Parameters[0] - 2;
                            break;
                        }
                    case 0x4B7B6E51: // Synchronous Timer
                        {
                            halt += (int)(float)cmd.Parameters[0];
                            break;
                        }
                    case 0xB738EABD: // hitbox 
                        {
                            Hitbox h = new Hitbox();
                            int id = (int)cmd.Parameters[0];
                            if (Hitboxes.ContainsKey(id))
                                Hitboxes.Remove(id);
                            h.Type = Hitbox.HITBOX;
                            h.Bone = ((int)cmd.Parameters[2] - 1).Clamp(0, int.MaxValue);
                            h.Damage = (float)cmd.Parameters[3];
                            h.Angle = (int)cmd.Parameters[4];
                            h.KnockbackGrowth = (int)cmd.Parameters[5];
                            //FKB = (float)cmd.Parameters[6]
                            h.KnockbackBase = (int)cmd.Parameters[7];
                            h.Size = (float)cmd.Parameters[8];
                            h.X = (float)cmd.Parameters[9];
                            h.Y = (float)cmd.Parameters[10];
                            h.Z = (float)cmd.Parameters[11];
                            Hitboxes.Add(id, h);
                            break;
                        }
                    case 0x2988D50F: // Extended hitbox
                        {
                            Hitbox h = new Hitbox();
                            int id = (int)cmd.Parameters[0];
                            if (Hitboxes.ContainsKey(id))
                                Hitboxes.Remove(id);
                            h.Type = Hitbox.HITBOX;
                            h.Extended = true;
                            h.Bone = ((int)cmd.Parameters[2] - 1).Clamp(0, int.MaxValue);
                            h.Damage = (float)cmd.Parameters[3];
                            h.Angle = (int)cmd.Parameters[4];
                            h.KnockbackGrowth = (int)cmd.Parameters[5];
                            //FKB = (float)cmd.Parameters[6]
                            h.KnockbackBase = (int)cmd.Parameters[7];
                            h.Size = (float)cmd.Parameters[8];
                            h.X = (float)cmd.Parameters[9];
                            h.Y = (float)cmd.Parameters[10];
                            h.Z = (float)cmd.Parameters[11];
                            h.X2 = (float)cmd.Parameters[24];
                            h.Y2 = (float)cmd.Parameters[25];
                            h.Z2 = (float)cmd.Parameters[26];
                            Hitboxes.Add(id, h);
                            break;
                        }
                    case 0x14FCC7E4: // special hitbox
                        {
                            Hitbox h = new Hitbox();
                            int id = (int)cmd.Parameters[0];
                            if (Hitboxes.ContainsKey(id))
                                Hitboxes.Remove(id);
                            h.Type = Hitbox.HITBOX;
                            h.Bone = ((int)cmd.Parameters[2] - 1).Clamp(0, int.MaxValue);
                            h.Damage = (float)cmd.Parameters[3];
                            h.Angle = (int)cmd.Parameters[4];
                            h.KnockbackGrowth = (int)cmd.Parameters[5];
                            //FKB = (float)cmd.Parameters[6]
                            h.KnockbackBase = (int)cmd.Parameters[7];
                            h.Size = (float)cmd.Parameters[8];
                            h.X = (float)cmd.Parameters[9];
                            h.Y = (float)cmd.Parameters[10];
                            h.Z = (float)cmd.Parameters[11];
                            Hitboxes.Add(id, h);
                            break;
                        }
                    case 0x7075DC5A: // Extended special hitbox
                        {
                            Hitbox h = new Hitbox();
                            int id = (int)cmd.Parameters[0];
                            if (Hitboxes.ContainsKey(id))
                                Hitboxes.Remove(id);
                            h.Type = Hitbox.HITBOX;
                            h.Extended = true;
                            h.Bone = ((int)cmd.Parameters[2] - 1).Clamp(0, int.MaxValue);
                            h.Damage = (float)cmd.Parameters[3];
                            h.Angle = (int)cmd.Parameters[4];
                            h.KnockbackGrowth = (int)cmd.Parameters[5];
                            //FKB = (float)cmd.Parameters[6]
                            h.KnockbackBase = (int)cmd.Parameters[7];
                            h.Size = (float)cmd.Parameters[8];
                            h.X = (float)cmd.Parameters[9];
                            h.Y = (float)cmd.Parameters[10];
                            h.Z = (float)cmd.Parameters[11];
                            h.X2 = (float)cmd.Parameters[40];
                            h.Y2 = (float)cmd.Parameters[41];
                            h.Z2 = (float)cmd.Parameters[42];
                            Hitboxes.Add(id, h);
                            break;
                        }
                    case 0x9245E1A8: // clear all hitboxes
                        Hitboxes.Clear();
                        break;
                    case 0xFF379EB6: // delete hitbox
                        if (Hitboxes.ContainsKey((int)cmd.Parameters[0]))
                        {
                            Hitboxes.Remove((int)cmd.Parameters[0]);
                        }
                        break;
                    case 0x7698BB42: // deactivate previous hitbox
                        Hitboxes.Remove(Hitboxes.Keys.Max());
                        break;
                    case 0xEB375E3: // Set Loop
                        iterations = int.Parse(cmd.Parameters[0] + "") - 1;
                        setLoop = e;
                        break;
                    case 0x38A3EC78: // goto
                        if (iterations > 0)
                        {
                            e = setLoop;
                            iterations -= 1;
                        }
                        break;

                    case 0x7B48FE1C: // grabbox 
                        {
                            Hitbox h = new Hitbox();
                            int id = (int)cmd.Parameters[0];
                            h.Type = Hitbox.GRABBOX;
                            h.Bone = (int.Parse(cmd.Parameters[1] + "") - 1).Clamp(0, int.MaxValue);
                            h.Size = (float)cmd.Parameters[2];
                            h.X = (float)cmd.Parameters[3];
                            h.Y = (float)cmd.Parameters[4];
                            h.Z = (float)cmd.Parameters[5];

                            if (cmd.Parameters.Count > 8)
                            {
                                h.X2 = float.Parse(cmd.Parameters[8] + "");
                                h.Y2 = float.Parse(cmd.Parameters[9] + "");
                                h.Z2 = float.Parse(cmd.Parameters[10] + "");
                            }

                            Hitboxes.Add(id, h);
                            break;
                        }
                    case 0xF3A464AC: // Terminate_Grab_Collisions
                        List<Hitbox> toDelete = new List<Hitbox>();
                        for (int i = 0; i < Hitboxes.Count; i++)
                        {
                            if (Hitboxes.Values[i].Type == Hitbox.GRABBOX)
                                toDelete.Add(Hitboxes.Values[i]);
                        }
                        foreach (Hitbox h in toDelete)
                            Hitboxes.Remove(Hitboxes.IndexOfValue(h));
                        break;
                    case 0xFAA85333:
                        break;
                    case 0x321297B0:
                        break;
                    case 0xED67D5DA:
                        break;
                    case 0x7640AEEB:
                        break;

                }

                e++;
                if (e >= script.Count)
                    break;
                else
                    cmd = script[e];

                if (halt > Frame)
                    break;
            }
        }

        public void HandleACMD(string animname)
        {
            var crc = Crc32.Compute(animname.ToLower());

            if (Runtime.Moveset == null)
            {
                script = null;
                return;
            }

            if (!Runtime.Moveset.Game.Scripts.ContainsKey(crc))
            {
                script = null;
                return;
            }

            script = (ACMDScript)Runtime.Moveset.Game.Scripts[crc];
        }

        #endregion

        public void SetFrame(int frame)
        {
            Runtime.TargetAnim.setFrame(frame);
            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if (m.vbn != null)
                    Runtime.TargetAnim.nextFrame(m.vbn);
            }
        }
        public void loadAnimation(SkelAnimation a)
        {
            a.setFrame(0);
            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if (m.vbn != null)
                    Runtime.TargetAnim.nextFrame(m.vbn);
            }
            nupdMaxFrame.Value = a.size() > 1 ? a.size() - 1 : a.size();
            nupdFrame.Value = 0;
        }
    }
}
