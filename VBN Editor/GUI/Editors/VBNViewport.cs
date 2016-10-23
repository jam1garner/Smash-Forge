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
                Runtime.ModelContainers = new List<ModelContainer>();
                Runtime.TextureContainers = new List<NUT>();
                Runtime.TargetVBN = null;
                Runtime.TargetAnim = null;
                Runtime.TargetLVD = null;
                Runtime.TargetPath = null;
                Runtime.TargetCMR0 = null;
                Runtime.TargetNUD = null;
                Runtime.killWorkspace = false;
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
                        isPlaying = false;
                        btnPlay.Text = "Play";
                    }
                }
                Render();
                System.Threading.Thread.Sleep(1000 / 60);
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
                if(m.vbn != null)
                    Runtime.TargetAnim.nextFrame(m.vbn);
            }
            
            Frame = (int)this.nupdFrame.Value;

            if(script != null)
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
            drawFloor(Matrix4.CreateTranslation(Vector3.Zero));


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
            DrawModels();

            GL.UseProgram(0);
            // draw path.bin
            DrawPathDisplay();
            // clear the buffer bit so the skeleton 
            // will be drawn on top of everything
            GL.Clear(ClearBufferMask.DepthBufferBit);
            // draw lvd
            DrawLVD();
            // drawing the bones
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

        private void SetCameraAnimation(){
            if (Runtime.TargetPath != null && checkBox1.Checked)
            {
                if (cf >= Runtime.TargetPath.frames.Count)
                    cf = 0;
                pathFrame f = Runtime.TargetPath.frames[cf];
                v = (Matrix4.CreateTranslation(f.x, f.y, f.z) * Matrix4.CreateFromQuaternion(new Quaternion(f.qx, f.qy, f.qz, f.qw))).Inverted() * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1.0f, 90000.0f);
                cf++;
            }else
                if (Runtime.TargetCMR0 != null && checkBox1.Checked)
                {
                    if (cf >= Runtime.TargetCMR0.frames.Count)
                        cf = 0;
                    Matrix4 m = Runtime.TargetCMR0.frames[cf].Inverted();
                    v =  Matrix4.CreateTranslation(m.M14, m.M24, m.M34) * Matrix4.CreateFromQuaternion(m.ExtractRotation()) * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1.0f, 90000.0f);
                    cf++;
                }
        }

        private void DrawModels(){
            GL.UseProgram(shader.programID);
            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if (m.nud != null)
                {
                    GL.UniformMatrix4(shader.getAttribute("modelview"), false, ref v);

                    if (m.vbn != null)
                    {
                        float[] f = m.vbn.getShaderMatrix();
                        GL.UniformMatrix4(shader.getAttribute("bone"), f.Length, false, f);
                    }

                    shader.enableAttrib();
                    m.nud.Render(shader);
                    shader.disableAttrib();
                }
            }
        }

        private void DrawBones(){
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
                            drawCube(pos_c, .085f);

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

        public void DrawLVD(){
            if (Runtime.TargetLVD != null)
            {
                foreach (Collision c in Runtime.TargetLVD.collisions)
                {
                    // draw the ground quads
                    int dir = 1;
                    int cg = 0;
                    GL.LineWidth(2);
                    GL.Color3(Color.FromArgb(50,Color.Red));
                    GL.Begin(PrimitiveType.Quads);
                    foreach (Vector2D vi in c.verts)
                    {
                        GL.Vertex3(vi.x, vi.y, 5 * dir);
                        GL.Vertex3(vi.x, vi.y, -5 * dir);
                        if (cg>0)
                        {
                            GL.Vertex3(vi.x, vi.y, 5 * dir);
                            GL.Vertex3(vi.x, vi.y, -5 * dir);
                        }
                        cg++;
                        dir *= -1;
                    }
                    GL.End();


                    // draw outside borders
                    GL.Color3(Color.Black);
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

                foreach (ItemSpawner c in Runtime.TargetLVD.items)
                {
                    foreach (Section s in c.sections) {
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

                    foreach(Point s in Runtime.TargetLVD.spawns)
                    {
                        GL.Color4(Color.FromArgb(100, Color.Blue));
                        GL.Begin(PrimitiveType.QuadStrip);
                        GL.Vertex3(s.x - 3f, s.y, 0f);
                        GL.Vertex3(s.x + 3f, s.y, 0f);
                        GL.Vertex3(s.x - 3f, s.y + 10f, 0f);
                        GL.Vertex3(s.x + 3f, s.y + 10f, 0f);
                        GL.End();
                    }

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

                    foreach (Point g in Runtime.TargetLVD.generalPoints)
                    {
                        GL.Color4(Color.FromArgb(200,Color.Fuchsia));
                        drawCubeWireframe(new Vector3(g.x, g.y, 0), 3);
                    }
                }
            }
        }

        private void DrawPathDisplay(){
            if(Runtime.TargetPath != null && !checkBox1.Checked)
            {
                GL.Color3(Color.Yellow);
                GL.LineWidth(2);
                for (int i = 1; i < Runtime.TargetPath.frames.Count; i++)
                {
                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex3(Runtime.TargetPath.frames[i].x, Runtime.TargetPath.frames[i].y, Runtime.TargetPath.frames[i].z);
                    GL.Vertex3(Runtime.TargetPath.frames[i - 1].x, Runtime.TargetPath.frames[i - 1].y, Runtime.TargetPath.frames[i - 1].z);
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
                    if (h.Extended) {
                            var va2 = new Vector3(h.X2, h.Y2, h.Z2);

                            if(h.Bone != -1)
                                va2 = Vector3.Transform(va2, b.transform.ClearScale());

							drawCylinder (va, va2, h.Size);
						} else {
							drawSphere(va, h.Size, 30);
						}
                }

                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.DepthTest);
            }
        }

        ACMDScript script;

        public void ProcessFrame(){
            Hitboxes.Clear ();
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
                        Hitboxes.Clear ();
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
                            h.Bone = (int.Parse(cmd.Parameters[1]+"") - 1).Clamp(0, int.MaxValue);
                            h.Size = (float)cmd.Parameters[2];
                            h.X = (float)cmd.Parameters[3];
                            h.Y = (float)cmd.Parameters[4];
                            h.Z = (float)cmd.Parameters[5];

                            if (cmd.Parameters.Count > 8)
                            {
                                h.X2 = float.Parse(cmd.Parameters[8]+"");
                                h.Y2 = float.Parse(cmd.Parameters[9]+"");
                                h.Z2 = float.Parse(cmd.Parameters[10]+"");
                            }

                            Hitboxes.Add(id, h);
                            break;
                        }
                    case 0xF3A464AC: // Terminate_Grab_Collisions
                        List<Hitbox> toDelete = new List<Hitbox>();
                        for(int i =0 ; i < Hitboxes.Count ; i++)
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
        private float clampControl(float f)
        {
            if (f < -5)
                f = -5;
            if (f > 5)
                f = 5;
            return f;
        }
        public void drawFloor(Matrix4 s)
        {
            // Draw floor plane
            /*GL.Color3(Color.LightGray);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(-20f, 0f, -20f);
            GL.Vertex3(20f, 0f, -20f);
            GL.Vertex3(20f, 0f, 20f);
            GL.Vertex3(-20f, 0f, 20f);
            GL.End();*/

            // Draw grid over it
            GL.Disable(EnableCap.DepthTest);

            GL.Color3(Color.DimGray);
            GL.LineWidth(1f);
            GL.Begin(PrimitiveType.Lines);
            for (var i = -10; i <= 10; i++)
            {
                GL.Vertex3(Vector3.Transform(new Vector3(-10f * 2, 0f, i * 2), s));
                GL.Vertex3(Vector3.Transform(new Vector3(10f * 2, 0f, i * 2), s));
                GL.Vertex3(Vector3.Transform(new Vector3(i * 2, 0f, -10f * 2), s));
                GL.Vertex3(Vector3.Transform(new Vector3(i * 2, 0f, 10f * 2), s));
            }
            GL.End();

            GL.Enable(EnableCap.DepthTest);
        }

        public void drawCircle(float x, float y, float z, float radius, uint precision)
        {
            drawCircle(new Vector3(x, y, z), radius, precision);
        }

        public void drawCircle(Vector3 center, float radius, uint precision)
        {
            float theta = 2.0f * (float)Math.PI / precision;
            float cosine = (float)Math.Cos(theta);
            float sine = (float)Math.Sin(theta);

            float x = radius;
            float y = 0;

            GL.Begin(PrimitiveType.TriangleFan);
            for (int i = 0; i < precision; i++)
            {
                GL.Vertex2(x + center.X, y + center.Y);

                //apply the rotation matrix
                var temp = x;
                x = cosine * x - sine * y;
                y = sine * temp + cosine * y;
            }
            GL.End();
        }

        public void drawCube(Vector3 center, float size)
        {
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);

            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);

            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);
            GL.End();
        }
        public void drawCubeWireframe(Vector3 center, float size)
        {
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);

            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);

            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);
            GL.End();
        }

        // Taken from Brawllib render TKContext.cs
        public static void drawSphere(Vector3 center, float radius, uint precision)
        {

            if (radius < 0.0f)
                radius = -radius;

            if (radius == 0.0f)
                throw new DivideByZeroException("DrawSphere: Radius cannot be zero.");

            if (precision == 0)
                throw new DivideByZeroException("DrawSphere: Precision of 8 or greater is required.");

            float halfPI = (float)(Math.PI * 0.5);
            float oneThroughPrecision = 1.0f / precision;
            float twoPIThroughPrecision = (float)(Math.PI * 2.0 * oneThroughPrecision);

            float theta1, theta2, theta3;
            Vector3 norm = new Vector3(), pos = new Vector3();

            for (uint j = 0; j < precision / 2; j++)
            {
                theta1 = (j * twoPIThroughPrecision) - halfPI;
                theta2 = ((j + 1) * twoPIThroughPrecision) - halfPI;

                GL.Begin(PrimitiveType.TriangleStrip);
                for (uint i = 0; i <= precision; i++)
                {
                    theta3 = i * twoPIThroughPrecision;

                    norm.X = (float)(Math.Cos(theta2) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta2);
                    norm.Z = (float)(Math.Cos(theta2) * Math.Sin(theta3));
                    pos.X = center.X + radius * norm.X;
                    pos.Y = center.Y + radius * norm.Y;
                    pos.Z = center.Z + radius * norm.Z;

                    GL.Normal3(norm.X, norm.Y, norm.Z);
                    GL.TexCoord2(i * oneThroughPrecision, 2.0f * (j + 1) * oneThroughPrecision);
                    GL.Vertex3(pos.X, pos.Y, pos.Z);

                    norm.X = (float)(Math.Cos(theta1) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta1);
                    norm.Z = (float)(Math.Cos(theta1) * Math.Sin(theta3));
                    pos.X = center.X + radius * norm.X;
                    pos.Y = center.Y + radius * norm.Y;
                    pos.Z = center.Z + radius * norm.Z;

                    GL.Normal3(norm.X, norm.Y, norm.Z);
                    GL.TexCoord2(i * oneThroughPrecision, 2.0f * j * oneThroughPrecision);
                    GL.Vertex3(pos.X, pos.Y, pos.Z);
                }
                GL.End();
            }
        }

		public static void drawCylinder(Vector3 p1, Vector3 p2, float R){
			int q = 8, p = 20;

			Vector3 yAxis = new Vector3 (0, 1, 0);
			Vector3 d = p2 - p1;
			float height = (float)Math.Sqrt (d.X*d.X + d.Y*d.Y + d.Z*d.Z) / 2;

			Vector3 mid = (p1 + p2) / 2;

			Vector3 axis = Vector3.Cross (d, yAxis);
			float angle = (float)Math.Acos (Vector3.Dot(d.Normalized(), yAxis));

			GL.PushMatrix ();
			GL.Translate(p1);
			GL.Rotate (-(float)((angle) * (180/Math.PI)), axis);
			for(int j = 0; j < q; j++)
			{
				GL.Begin(PrimitiveType.TriangleStrip);
				for(int i = 0; i <= p; i++)
				{
					GL.Vertex3( R * Math.Cos( (float)(j+1)/q * Math.PI/2.0 ) * Math.Cos( 2.0 * (float)i/p * Math.PI ),
						-R * Math.Sin( (float)(j+1)/q * Math.PI/2.0 ),
						R * Math.Cos( (float)(j+1)/q * Math.PI/2.0 ) * Math.Sin( 2.0 * (float)i/p * Math.PI ) );
					GL.Vertex3( R * Math.Cos( (float)j/q * Math.PI/2.0 ) * Math.Cos( 2.0 * (float)i/p * Math.PI ),
						-R * Math.Sin( (float)j/q * Math.PI/2.0 ),
						R * Math.Cos( (float)j/q * Math.PI/2.0 ) * Math.Sin( 2.0 * (float)i/p * Math.PI ) );         
				}
				GL.End();
			}
			GL.PopMatrix ();

			GL.PushMatrix ();
			GL.Translate(p2);
			GL.Rotate (-(float)(angle * (180/Math.PI)), axis);
			for(int j = 0; j < q; j++)
			{
				GL.Begin(PrimitiveType.TriangleStrip);
				for(int i = 0; i <= p; i++)
				{
					GL.Vertex3( R * Math.Cos( (float)(j+1)/q * Math.PI/2.0 ) * Math.Cos( 2.0 * (float)i/p * Math.PI ),
						R * Math.Sin( (float)(j+1)/q * Math.PI/2.0 ),
						R * Math.Cos( (float)(j+1)/q * Math.PI/2.0 ) * Math.Sin( 2.0 * (float)i/p * Math.PI ) );
					GL.Vertex3( R * Math.Cos( (float)j/q * Math.PI/2.0 ) * Math.Cos( 2.0 * (float)i/p * Math.PI ),
						R * Math.Sin( (float)j/q * Math.PI/2.0 ),
						R * Math.Cos( (float)j/q * Math.PI/2.0 ) * Math.Sin( 2.0 * (float)i/p * Math.PI ) );         
				}
				GL.End();
			}
			GL.PopMatrix ();


			/*  sides */
			GL.PushMatrix ();

			GL.Translate(mid);
			GL.Rotate (-(float)(angle * (180/Math.PI)), axis);

			GL.Begin(PrimitiveType.QuadStrip);
			for (int j=0;j<=360;j+=1) {
				GL.Vertex3(Math.Cos(j)*R,+height,Math.Sin(j)*R);
				GL.Vertex3(Math.Cos(j)*R,-height,Math.Sin(j)*R);
			}
			GL.End();

			GL.PopMatrix ();
		}
        #endregion

        public void SetFrame(int frame)
        {
            Runtime.TargetAnim.setFrame(frame);
            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if(m.vbn != null)
                    Runtime.TargetAnim.nextFrame(m.vbn);
            }
        }
        public void loadAnimation(SkelAnimation a)
        {
            a.setFrame(0);
            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if(m.vbn != null)
                    Runtime.TargetAnim.nextFrame(m.vbn);
            }
            nupdMaxFrame.Value = a.size() > 1 ? a.size() - 1 : a.size();
            nupdFrame.Value = 0;
        }
    }
}
