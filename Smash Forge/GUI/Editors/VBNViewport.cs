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
using OpenTK.Input;
using System.Security.Cryptography;
using SALT.Moveset.AnimCMD;
using OpenTK.Graphics;
using System.Diagnostics;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using Gif.Components;
using Smash_Forge.Rendering.Lights;
using Smash_Forge.Rendering;

namespace Smash_Forge
{
    public partial class VBNViewport : FormBase
    {
        public static int defaulttex = 0;

        public Camera vbnViewportCamera = new Camera();
        public Mode CurrentMode = Mode.Normal;

        public string TargetAnimString = "";
        public Animation TargetAnim;
        public List<ModelContainer> ModelContainers = new List<ModelContainer>();

        public enum Mode
        {
            Normal = 0,
            Photoshoot,
            Selection
        }

        public VBNViewport()
        {
            InitializeComponent();
            this.TabText = "Renderer";
            Application.Idle += Application_Idle;
            Runtime.AnimationChanged += Runtime_AnimationChanged;
            timeSinceSelected.Start();
            Runtime.vbnViewport = this;
        }

        // Explicitly unsubscribe from the static event to 
        // prevent hanging Garbage Collection.
        ~VBNViewport()
        {
            Runtime.AnimationChanged -= Runtime_AnimationChanged;
            Application.Idle -= Application_Idle;
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
            defaulttex = NUT.loadImage(Smash_Forge.Resources.Resources.DefaultTexture);
            renderMode.SelectedIndex = (int)Runtime.renderType;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!DesignMode && _controlLoaded)
            {
                GL.LoadIdentity();
                GL.Viewport(glControl1.ClientRectangle);

                vbnViewportCamera.renderWidth = (glControl1.Width);
                vbnViewportCamera.renderHeight = (glControl1.Height);
                vbnViewportCamera.Update();
            }

        }

        #region Members
        public float x = 0;
        public float nzoom = 0;
        public GUI.Menus.CameraSettings cameraPosForm = null;
        float mouseXLast = 0;
        float mouseYLast = 0;
        float mouseSLast = 0;
        bool render = false;
        bool isPlaying = false;
        bool fpsView = false;
        public Stopwatch timeSinceSelected = new Stopwatch();
        public static FrameTimer frameTimer = new FrameTimer();
        public long renderedFrameCount = 0;
        public long totalRenderTime = 0;
        public Stopwatch directUVTimeStopWatch = new Stopwatch();

        // Selection
        public TransformTool transformTool = new TransformTool();

        // PhotoShoot
        public int shootX = 0, shootY = 0, shootWidth = 512, shootHeight = 512;


        Shader shader;
        #endregion

        #region Event Handlers
        private void Application_Idle(object sender, EventArgs e)
        {
            // Fix accessing the control after 
            // the editor has been closed 
            if (Runtime.killWorkspace)
            {
                /*foreach (ModelContainer n in ModelContainers)
                {
                    n.Destroy();
                }
                foreach (NUT n in Runtime.TextureContainers)
                {
                    //n.Destroy();
                }
                ModelContainers = new List<ModelContainer>();
                Runtime.TextureContainers = new List<NUT>();
                Runtime.TargetVBN = null;
                TargetAnim = null;
                Runtime.TargetLVD = null;
                Runtime.TargetPath = null;
                Runtime.TargetCMR0 = null;
                Runtime.TargetNUD = null;
                Runtime.killWorkspace = false;
                Runtime.Animations = new Dictionary<string, Animation>();
                MainForm.Instance.lvdList.fillList();
                MainForm.Instance.meshList.refresh();
                MainForm.Instance.paramEditors = new List<PARAMEditor>();
                MainForm.Instance.lvdEditor.Clear();
                //MainForm.Instance.boneTreePanel.Clear();
                string acmdpath = Path.Combine(MainForm.executableDir, "workspace/animcmd/");
                if (Directory.Exists(acmdpath))
                {
                    foreach (string file in Directory.EnumerateFiles(acmdpath))
                        File.Delete(file);
                    Directory.Delete(acmdpath);
                }

                MainForm.Instance.project.fillTree();

                GC.Collect();*/
            }

            if (this.IsDisposed == true)
                return;

            while (render && _controlLoaded && glControl1.IsIdle)
            {
                if (Smash_Forge.Update.Downloaded)
                    MainForm.Instance.pictureBox1.Image = Resources.Resources.sexy_green_down_arrow;

                if (Keyboard.GetState().IsKeyDown(Key.S)
                    && Keyboard.GetState().IsKeyDown(Key.K)
                    && Keyboard.GetState().IsKeyDown(Key.A)
                    && Keyboard.GetState().IsKeyDown(Key.P)
                    && Keyboard.GetState().IsKeyDown(Key.O)
                    && Keyboard.GetState().IsKeyDown(Key.N))
                {
                    DialogResult dialogResult = MessageBox.Show("Activate Skapon?", "Skapon Code", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        foreach (ModelContainer m in ModelContainers)
                        {
                            if (m.VBN != null && m.NUD == null)
                                m.NUD = Skapon.Create(m.VBN);
                        }
                    }
                }

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
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
                            nupdFrame.Value = 1;
                        }
                    }
                }
                Render();
                stopWatch.Stop();

                Overlay.Visible = fpsCheckbox.Checked;
                if (fpsCheckbox.Checked)
                {
                    Overlay.Text = "FPS: " + (AnimationSpeed - stopWatch.ElapsedMilliseconds / 1000f) + "\n";
                    /*if (MainForm.Instance.boneTreePanel.treeView1.SelectedNode != null)
                    {
                        Bone bone = (Bone)MainForm.Instance.boneTreePanel.treeView1.SelectedNode;
                        Overlay.Text += bone.Text + "\n";
                        Overlay.Text += bone.pos.ToString() + "\n";
                        Overlay.Text += bone.rot.ToString() + "\n";
                        Overlay.Text += ANIM.quattoeul(bone.rot).ToString() + "\n";
                        Overlay.Text += bone.sca.ToString() + "\n";
                    }*/
                }

                if (((1000 / AnimationSpeed) - stopWatch.ElapsedMilliseconds > 0))
                    System.Threading.Thread.Sleep((int)((1000 / AnimationSpeed) - stopWatch.ElapsedMilliseconds));
            }
        }

        private void Runtime_AnimationChanged(object sender, EventArgs e)
        {
            //If moveset is loaded then initialize with null script so handleACMD loads script for frame speed modifiers and FAF (if parameters are imported)
            if (Runtime.Moveset != null && Runtime.gameAcmdScript == null)
                Runtime.gameAcmdScript = new ForgeACMDScript(null);

            if (!string.IsNullOrEmpty(TargetAnimString))
            {
                if (Runtime.gameAcmdScript != null)
                {
                    //Remove manual crc flag
                    Runtime.acmdEditor.manualCrc = false;
                    HandleACMD(TargetAnimString.Substring(3));
                    if (Runtime.gameAcmdScript != null)
                        Runtime.gameAcmdScript.processToFrame(0);

                }
            }

            if (TargetAnim == null)
            {
                foreach (ModelContainer m in ModelContainers)
                {
                    if (m.VBN != null)
                        m.VBN.reset();
                }
            }
            else
                LoadAnimation(TargetAnim);
        }

        private void btnFirstFrame_Click(object sender, EventArgs e)
        {
            this.nupdFrame.Value = 1;
        }
        private void btnPrevFrame_Click(object sender, EventArgs e)
        {
            if (this.nupdFrame.Value - 1 >= 1)
                this.nupdFrame.Value -= 1;
        }
        private void btnLastFrame_Click(object sender, EventArgs e)
        {
            if (TargetAnim != null)
                this.nupdFrame.Value = this.nupdMaxFrame.Value;
        }
        private void btnNextFrame_Click(object sender, EventArgs e)
        {
            if (TargetAnim != null)
                this.nupdFrame.Value += 1;
        }
        private void btnPlay_Click(object sender, EventArgs e)
        {
            // If we're already at final frame and we hit play again
            // start from the beginning of the anim
            if (nupdMaxFrame.Value == nupdFrame.Value)
                nupdFrame.Value = 1;

            isPlaying = !isPlaying;
            if (isPlaying)
            {
                btnPlay.Text = "Pause";

                directUVTimeStopWatch.Start();
            }
            else
            {
                btnPlay.Text = "Play";

                directUVTimeStopWatch.Stop();
            }
        }
        private void nupdFrame_ValueChanged(Object sender, EventArgs e)
        {
            if (TargetAnim == null)
                return;

            if (this.nupdFrame.Value > this.nupdMaxFrame.Value)
            {
                this.nupdFrame.Value = 1;
            }
            if (this.nupdFrame.Value < 1)
            {
                this.nupdFrame.Value = this.nupdMaxFrame.Value;
            }
            SetAnimationFrame((int)this.nupdFrame.Value - 1);
        }

        public void SetAnimationFrame(int frameNum)
        {
            // Process script first in case we have to speed up the animation
            if (Runtime.gameAcmdScript != null)
            {
                Runtime.gameAcmdScript.processToFrame(frameNum);
            }

            float animFrameNum = frameNum;
            if (Runtime.gameAcmdScript != null && Runtime.useFrameDuration)
                animFrameNum = Runtime.gameAcmdScript.animationFrame;// - 1;

            TargetAnim.SetFrame(animFrameNum);
            foreach (ModelContainer m in ModelContainers)
            {
                TargetAnim.SetFrame(animFrameNum);
                if (m.VBN != null)
                    TargetAnim.NextFrame(m.VBN);

                // Deliberately do not ever use ACMD/animFrame to modify these other types of model
                if (m.DAT_MELEE != null)
                {
                    TargetAnim.NextFrame(m.DAT_MELEE.bones);
                }
                if (m.BCH != null)
                {
                    foreach (BCH_Model mod in m.BCH.Models.Nodes)
                    {
                        if (mod.skeleton != null)
                            TargetAnim.NextFrame(mod.skeleton);
                    }
                }
            }

            Frame = (int)animFrameNum;
        }

        private void nupdSpeed_ValueChanged(object sender, EventArgs e)
        {
            setAnimationSpeed((int)nupdFrameRate.Value);
        }

        public void setAnimationSpeed(int fps)
        {
            AnimationSpeed = fps;
        }

        System.Drawing.Point _LastPoint = System.Drawing.Point.Empty;
        private void glControl1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!freezeCamera)
                if (!fpsView)
                {
                    UpdateMousePosition();
                    UpdateCameraPositionControl();
                }
            if (_LastPoint != e.Location) // can add some slack by checking the distance
            {
                dbdistance = 0;
            }
            /*if(Runtime.TargetVBN != null && freezeCamera && CurrentMode == Mode.Selection)
            {
                ((Bone)MainForm.Instance.boneTreePanel.treeView1.SelectedNode).rot += Quaternion.FromAxisAngle(Vector3.UnitX, 2);
                Runtime.TargetVBN.update();
            }*/
        }

        public event EventHandler FrameChanged;
        protected virtual void OnFrameChanged(EventArgs e)
        {
            if (FrameChanged != null)
                FrameChanged(this, e);
            //HandleACMD(TargetAnimString);
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
        #endregion

        #region Rendering

        public int ubo_bones, ubo_bonesIT;
        public static int cubeTex;

        private void SetupViewPort()
        {
            if (shader != null)
                GL.DeleteShader(shader.programID);

            if (shader == null)
            {
                // should vbn do this instead?
                GL.GenBuffers(1, out ubo_bones);
                GL.GenBuffers(1, out ubo_bonesIT);
            }

            int h = Height - groupBox2.ClientRectangle.Top;
            int w = Width;
            GL.LoadIdentity();
            GL.Viewport(glControl1.ClientRectangle);
            vbnViewportCamera.Update();

            SetupFrameBuffersRenderBuffers();

            for (int i = 0; i < LightTools.stageDiffuseLightSet.Length; i++)
            {
                // should properly initialize these eventually
                LightTools.stageDiffuseLightSet[i] = new DirectionalLight();
                LightTools.stageDiffuseLightSet[i].id = "Stage " + i;
            }

            for (int i = 0; i < LightTools.stageFogSet.Length; i++)
            {
                // should properly initialize these eventually
                LightTools.stageFogSet[i] = new Vector3(0);
            }

            Debug.WriteLine(GL.GetError());
            CalculateLightSource();
        }

        private void SetupFrameBuffersRenderBuffers()
        {
            // shadowmap
            GL.GenTextures(1, out depthmap);
            GL.BindTexture(TextureTarget.Texture2D, depthmap);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, sw, sh, 0, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            // shadow framebuffer
            GL.GenFramebuffers(1, out sfb);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, sfb);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthmap, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            Debug.WriteLine(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // hdr framebuffer
            //int hdrFBO;
            GL.GenFramebuffers(1, out hdrFBO);

            // color texture (result of drawing nud shader)
            int screenWidth = glControl1.Width; // how to handle screen resizing?
            int screenHeight = glControl1.Height;

            //colorTexture1
            GL.GenTextures(1, out colorTexture1);
            GL.BindTexture(TextureTarget.Texture2D, colorTexture1);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, screenWidth, screenHeight, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            //colorTexture2 (bright color)
            GL.GenTextures(1, out colorTexture2);
            GL.BindTexture(TextureTarget.Texture2D, colorTexture2);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, screenWidth, screenHeight, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // depth render buffer
            int rboDepth;
            GL.GenRenderbuffers(1, out rboDepth);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rboDepth);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent,
                screenWidth, screenHeight);

            // attach buffers and stuff
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, hdrFBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, colorTexture1, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1,
                TextureTarget.Texture2D, colorTexture2, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                RenderbufferTarget.Renderbuffer, rboDepth);
            DrawBuffersEnum[] bufs = new DrawBuffersEnum[2] { (DrawBuffersEnum)FramebufferAttachment.ColorAttachment0,
                (DrawBuffersEnum)FramebufferAttachment.ColorAttachment1 };
            GL.DrawBuffers(bufs.Length, bufs);

            // multiple textures and fbos for 2 pass guassian blur
            GL.GenFramebuffers(1, out pingPongFBO1);
            GL.GenFramebuffers(1, out pingPongFBO2);

            // pingpong texture 1
            GL.GenTextures(1, out pingPongColorTexture1);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, pingPongFBO1);
            GL.BindTexture(TextureTarget.Texture2D, pingPongColorTexture1);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, screenWidth, screenHeight, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, pingPongColorTexture1, 0);

            // pingpong texture 2
            GL.GenTextures(1, out pingPongColorTexture2);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, pingPongFBO2);
            GL.BindTexture(TextureTarget.Texture2D, pingPongColorTexture2);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, screenWidth, screenHeight, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, pingPongColorTexture2, 0);

            Debug.WriteLine(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        int cf = 0;
        int sfb, sw = 512, sh = 512, depthmap, hdrFBO;
        int colorTexture1, colorTexture2;
        int pingPongColorTexture1, pingPongColorTexture2;
        int pingPongFBO1, pingPongFBO2;
        Matrix4 lightMatrix;
        Matrix4 modelMatrix;
        Matrix4 lightProjection;

        public void CalculateLightSource()
        {
            Matrix4.CreateOrthographicOffCenter(-10.0f, 10.0f, -10.0f, 10.0f, 1.0f, Runtime.renderDepth, out lightProjection);
            Matrix4 lightView = Matrix4.LookAt(Vector3.TransformVector(Vector3.Zero, vbnViewportCamera.mvpMatrix).Normalized(),
                new Vector3(0),
                new Vector3(0, 1, 0));
            lightMatrix = lightProjection * lightView;
            //lightMatrix = Matrix4.CreateTranslation(width, -height, zoom)
            // * lightProjection * Matrix4.CreateRotationY(LightTools.diffuseLight.rotY) * Matrix4.CreateRotationX(LightTools.diffuseLight.rotX);
        }

        public void Render()
        {
            if (!render)
                return;

            glControl1.MakeCurrent();
            GL.Viewport(glControl1.ClientRectangle);

            // Push all attributes so we don't have to clean up later
            GL.PushAttrib(AttribMask.AllAttribBits);

            // clear all the buffers
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            // use fixed function pipeline for drawing background and floor grid
            GL.UseProgram(0);

            if (MainForm.Instance.meshList.treeView1.SelectedNode != null)
            {
                if (MainForm.Instance.meshList.treeView1.SelectedNode is BCH_Texture)
                {
                    GL.PopAttrib();
                    BCH_Texture tex = ((BCH_Texture)MainForm.Instance.meshList.treeView1.SelectedNode);
                    RenderTools.DrawTexturedQuad(tex.display, tex.Width, tex.Height, true, true, true, true, true);
                    glControl1.SwapBuffers();
                    return;
                }
                if (MainForm.Instance.meshList.treeView1.SelectedNode is NUT_Texture)
                {
                    GL.PopAttrib();
                    NUT_Texture tex = ((NUT_Texture)MainForm.Instance.meshList.treeView1.SelectedNode);
                    RenderTools.DrawTexturedQuad(((NUT)tex.Parent).draw[tex.HASHID], tex.Width, tex.Height, true, true, true, true, true);
                    glControl1.SwapBuffers();
                    return;
                }
            }


            if (Runtime.renderBackGround)
            {
                // background uses different matrices
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();

                RenderTools.RenderBackground();
            }

            // set up the matrices for drawing models and floor
            GL.MatrixMode(MatrixMode.Projection);
            if (IsMouseOverViewport() && glControl1.Focused && !freezeCamera && !transformTool.hit)
            {
                if (fpsView)
                    FPSCamera();
                else
                    UpdateMousePosition();
                UpdateCameraPositionControl();
            }
            vbnViewportCamera.mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;
            SetCameraAnimation();

            Matrix4 matrix = vbnViewportCamera.mvpMatrix;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref matrix);

            if (Runtime.renderFloor)
                RenderTools.drawFloor();

            if (Runtime.drawModelShadow)
                DrawModelShadow();

            // render models into hdr buffer
            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, hdrFBO);
            //GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            if (Runtime.useDepthTest)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Lequal);
            }

            else
                GL.Disable(EnableCap.DepthTest);

            GL.Enable(EnableCap.DepthTest);
            // GL.DepthFunc(DepthFunction.Lequal);

            if (Runtime.renderModel)
                foreach (ModelContainer m in ModelContainers)
                    m.Render(vbnViewportCamera, depthmap, lightMatrix, modelMatrix);

            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // render gaussian blur stuff
            if (Runtime.drawQuadBlur)
                DrawQuadBlur();



            // render full screen quad for post processing
            if (Runtime.drawQuadFinalOutput)
                DrawQuadFinalOutput();

            // use fixed function pipeline again for area lights, lvd, bones, hitboxes, etc
            SetupFixedFunctionRendering();

            // draw path.bin
            if (Runtime.renderPath)
                DrawPathDisplay();

            // area light bounding boxes should intersect stage geometry and not render on top


            // clear depth buffer so stuff will render on top of the models
            GL.Clear(ClearBufferMask.DepthBufferBit);

            if (Runtime.renderLVD)
                DrawLVD();

            if (Runtime.renderBones)
                DrawBones();

            DrawHitboxesHurtboxes();

            if (CurrentMode == Mode.Photoshoot)
            {
                freezeCamera = false;
                if (Keyboard.GetState().IsKeyDown(Key.W) && Mouse.GetState().IsButtonDown(MouseButton.Left))
                {
                    shootX = this.PointToClient(Cursor.Position).X;
                    shootY = this.PointToClient(Cursor.Position).Y;
                    freezeCamera = true;
                }
                // Hold on to your pants, boys
                RenderTools.DrawPhotoshoot(glControl1, shootX, shootY, shootWidth, shootHeight);
            }

            // Clean up
            GL.PopAttrib();

            glControl1.SwapBuffers();
        }

        public Vector3 UnProject(float x, float y, float z)
        {
            return Vector4.Transform(new Vector4(
                2.0f * (x / glControl1.Width) - 1.0f,
                2.0f * ((glControl1.Height - y) / glControl1.Height) - 1.0f,
                2.0f * z - 1.0f,
                1.0f), vbnViewportCamera.mvpMatrix.Inverted()).Xyz;
        }


        private static void SetupFixedFunctionRendering()
        {
            GL.UseProgram(0);

            GL.Enable(EnableCap.LineSmooth); // This is Optional 
            GL.Enable(EnableCap.Normalize);  // This is critical to have
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
        }

        private void DrawModelShadow()
        {
            // update light matrix and setup shadowmap rendering
            CalculateLightSource();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lightMatrix);
            GL.Enable(EnableCap.DepthTest);
            GL.Viewport(0, 0, sw, sh);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, sfb);

            // critical to clear depth buffer
            GL.Clear(ClearBufferMask.DepthBufferBit);

            foreach (ModelContainer c in ModelContainers)
            {
                if (c.NUD != null)
                {
                    c.NUD.RenderShadow(lightMatrix, vbnViewportCamera.mvpMatrix, modelMatrix);
                }
            }

            Matrix4 matrix = vbnViewportCamera.mvpMatrix;
            // reset matrices and viewport for model rendering again
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.LoadMatrix(ref matrix);
            GL.Viewport(glControl1.ClientRectangle);
        }

        private void DrawQuadBlur()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.DepthTest);
            bool horizontal = true;
            bool first_iteration = true;
            int blur_amount = 10;
            DrawScreenQuadBlur(blur_amount, horizontal, first_iteration);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private void DrawQuadFinalOutput()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, colorTexture1);

            GL.ActiveTexture(TextureUnit.Texture11);
            GL.BindTexture(TextureTarget.Texture2D, depthmap);

            GL.Disable(EnableCap.DepthTest);
            DrawScreenQuad();
        }

        private void DrawHitboxesHurtboxes()
        {
            if (!string.IsNullOrEmpty(TargetAnimString))
            {
                HandleACMD(TargetAnimString.Substring(3));
            }

            // Hurtboxes and ECBs first so they appear under hitboxes
            if (Runtime.renderHurtboxes)
                RenderHurtboxes();
            if (Runtime.renderECB)
                RenderECB();

            if (Runtime.renderLedgeGrabboxes || Runtime.renderReverseLedgeGrabboxes)
                RenderLedgeGrabboxes();

            if (Runtime.renderSpecialBubbles)
                RenderSpecialBubbles();

            if (Runtime.renderHitboxes && Runtime.renderInterpolatedHitboxes)
                RenderInterpolatedHitboxes();

            // Must come after interpolated boxes to appear on top
            if (Runtime.renderHitboxes)
                RenderHitboxes();
        }

      



        public void UpdateCameraPositionControl()
        {
            if (cameraPosForm != null && !cameraPosForm.IsDisposed)
                cameraPosForm.updatePosition();
        }

        public void UpdateMousePosition()
        {
            vbnViewportCamera.Update();
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
                vbnViewportCamera.Update();
                cf++;
            }
            else if (Runtime.TargetCMR0 != null && checkBox1.Checked)
            {
                if (cf >= Runtime.TargetCMR0.frames.Count)
                    cf = 0;
                Matrix4 m = Runtime.TargetCMR0.frames[cf].Inverted();
                vbnViewportCamera.Update();
                cf++;
            }
        }

        private void DrawScreenQuad()
        {
            // draw a full screen quad for fbo debugging and post processing
            shader = Runtime.shaders["Quad"];
            GL.UseProgram(shader.programID);

            GL.ActiveTexture(TextureUnit.Texture11);
            GL.BindTexture(TextureTarget.Texture2D, depthmap);
            GL.Uniform1(shader.getAttribute("ShadowMap"), 11);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, colorTexture1);
            GL.Uniform1(shader.getAttribute("ScreenRender"), 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, pingPongColorTexture1);
            GL.Uniform1(shader.getAttribute("ScreenRenderBlur"), 1);

            // just use a big triangle instead of a quad
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.BindVertexArray(0);

        }


        private void DrawScreenQuadBlur(int blur_amount, bool horizontal, bool first_iteration)
        {
            // draw a full screen quad for fbo debugging and post processing
            shader = Runtime.shaders["Blur"];
            GL.UseProgram(shader.programID);

            GL.ActiveTexture(TextureUnit.Texture0);// should I bind a texture here
            GL.Uniform1(shader.getAttribute("image"), 0);

            // this whole section in general is pretty broken
            for (int i = 0; i < blur_amount; i++)
            {

                if (horizontal)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, pingPongFBO1);
                }

                else
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, pingPongFBO2);
                }


                GL.Uniform1(shader.getAttribute("horizontal"), horizontal ? 1 : 0);

                if (first_iteration)
                {
                    GL.BindTexture(TextureTarget.Texture2D, colorTexture2);
                }

                else
                {

                    if (horizontal)
                    {
                        GL.BindTexture(TextureTarget.Texture2D, pingPongColorTexture1);
                    }

                    else if (!horizontal)
                    {
                        GL.BindTexture(TextureTarget.Texture2D, pingPongColorTexture2);
                    }

                }

                // render screen quad
                GL.DrawArrays(PrimitiveType.Triangles, 0, 3); // just use a big triangle instead
                GL.BindVertexArray(0);

                horizontal = !horizontal;
                if (first_iteration)
                    first_iteration = false;

            }

        }

        private void DrawBones()
        {
            if (ModelContainers.Count > 0)
            {
                foreach (ModelContainer m in ModelContainers)
                {
                    RenderTools.DrawVBN(m.VBN);
                    if (m.BCH != null)
                    {
                        foreach (BCH_Model mo in m.BCH.Models.Nodes)
                            RenderTools.DrawVBN(mo.skeleton);
                    }

                    if (m.DAT_MELEE != null)
                    {
                        RenderTools.DrawVBN(m.DAT_MELEE.bones);
                    }
                }
            }
        }


        public void DrawLVD()
        {
            GL.Disable(EnableCap.CullFace);

            foreach (ModelContainer m in ModelContainers)
            {

                if (m.DAT_MELEE != null && m.DAT_MELEE.collisions != null)
                {
                    LVD.DrawDATCollisions(m);

                }

                if (m.DAT_MELEE != null && m.DAT_MELEE.blastzones != null)
                {
                    LVD.DrawBounds(m.DAT_MELEE.blastzones, Color.Red);
                }

                if (m.DAT_MELEE != null && m.DAT_MELEE.cameraBounds != null)
                {
                    LVD.DrawBounds(m.DAT_MELEE.cameraBounds, Color.Blue);
                }

                if (m.DAT_MELEE != null && m.DAT_MELEE.targets != null)
                {
                    foreach (Point target in m.DAT_MELEE.targets)
                    {
                        RenderTools.drawCircleOutline(new Vector3(target.x, target.y, 0), 2, 30);
                        RenderTools.drawCircleOutline(new Vector3(target.x, target.y, 0), 4, 30);
                    }
                }

                if (m.DAT_MELEE != null && m.DAT_MELEE.respawns != null)
                    foreach (Point r in m.DAT_MELEE.respawns)
                    {
                        Spawn temp = new Spawn() { x = r.x, y = r.y };
                        LVD.DrawSpawn(temp, true);
                    }

                if (m.DAT_MELEE != null && m.DAT_MELEE.spawns != null)
                    foreach (Point r in m.DAT_MELEE.spawns)
                    {
                        Spawn temp = new Spawn() { x = r.x, y = r.y };
                        LVD.DrawSpawn(temp, false);
                    }

                GL.Color4(Color.FromArgb(200, Color.Fuchsia));
                if (m.DAT_MELEE != null && m.DAT_MELEE.itemSpawns != null)
                    foreach (Point r in m.DAT_MELEE.itemSpawns)
                        RenderTools.DrawCube(new Vector3(r.x, r.y, 0), 3, true);
            }

            if (Runtime.TargetLVD != null)
            {
                if (Runtime.renderCollisions)
                {
                    Runtime.TargetLVD.DrawCollisions();
                }

                if (Runtime.renderItemSpawners)
                {
                    Runtime.TargetLVD.DrawItemSpawners();
                }

                if (Runtime.renderSpawns)
                {
                    foreach (Spawn s in Runtime.TargetLVD.spawns)
                        LVD.DrawSpawn(s, false);
                }

                if (Runtime.renderRespawns)
                {
                    foreach (Spawn s in Runtime.TargetLVD.respawns)
                        LVD.DrawSpawn(s, true);
                }

                if (Runtime.renderGeneralPoints)
                {
                    foreach (GeneralPoint p in Runtime.TargetLVD.generalPoints)
                        LVD.DrawPoint(p);

                    foreach (GeneralShape s in Runtime.TargetLVD.generalShapes)
                        LVD.DrawShape(s);
                }

                if (Runtime.renderOtherLVDEntries)
                {
                    foreach (EnemyGenerator e in Runtime.TargetLVD.enemySpawns)
                        LVD.DrawEnemyGenerator(e);

                    foreach (DamageShape s in Runtime.TargetLVD.damageShapes)
                        LVD.DrawDamageShape(s);

                    foreach (Bounds b in Runtime.TargetLVD.cameraBounds)
                        LVD.DrawBounds(b, Color.Blue);

                    foreach (Bounds b in Runtime.TargetLVD.blastzones)
                        LVD.DrawBounds(b, Color.Red);
                }
            }

            GL.Enable(EnableCap.CullFace);
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

        public Tuple<int, int> translateScriptBoneId(int boneId)
        {
            int jtbIndex = 0;
            while (boneId >= 1000)
            {
                boneId -= 1000;
                jtbIndex++;  // look in a different joint table
            }
            return Tuple.Create(boneId, jtbIndex);
        }

        public Bone getBone(int bone)
        {
            Tuple<int, int> boneInfo = translateScriptBoneId(bone);
            int bid = boneInfo.Item1;
            int jtbIndex = boneInfo.Item2;

            Bone b = new Bone(null);

            if (bone != -1)
            {
                foreach (ModelContainer m in ModelContainers)
                {
                    // ModelContainers should store Hitbox data or have them linked since it will use last
                    // modelcontainer bone for hitbox display (which might not be the character model).
                    // This is especially important for the future when importing weapons for some moves.
                    if (m.VBN != null)
                    {
                        try //Try used to avoid bone not found issue that crashes the application
                        {
                            /*if (m.VBN.jointTable.Count < 1)
                                b = m.VBN.bones[bid];
                            else*/
                            {
                                if (jtbIndex == 0)
                                {
                                    // Special rule for table 0, index 0 is *always* TransN, and index 1 counts as index 0
                                    if (bid <= 0)
                                    {
                                        b = m.VBN.bones.Find(item => item.Name == "TransN");
                                        if (b == null)
                                            b = m.VBN.bones[0];
                                    }
                                   // else  // Index 2 counts as index 1, etc
                                        //b = m.VBN.bones[m.VBN.jointTable[jtbIndex][bid - 1]];
                                }
                                //else if (jtbIndex < m.VBN.jointTable.Count)
                                {
                                    // Extra joint tables don't have the TransN rule
                                    //b = m.VBN.bones[m.VBN.jointTable[jtbIndex][bid]];
                                }
                                //else
                                {
                                    //If there is no jointTable but bone is >1000 then don't look into a another joint table
                                    //This makes some weapons like Luma have hitboxes visualized
                                    //b = m.vbn.bones[bid];
                                    //b = m.VBN.bones[m.VBN.jointTable[m.VBN.jointTable.Count - 1][bid]];
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            return b;
        }

        public void RenderHitboxes()
        {
            if (Runtime.gameAcmdScript == null || Runtime.gameAcmdScript.Hitboxes.Count <= 0)
                return;

            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            foreach (var pair in Runtime.gameAcmdScript.Hitboxes)
            {
                var h = pair.Value;

                if (Runtime.HiddenHitboxes.Contains(h.ID))
                    continue;

                Bone b = getBone(h.Bone);
                h.va = Vector3.TransformVector(new Vector3(h.X, h.Y, h.Z), b.transform.ClearScale());

                // Draw angle marker
                /*GL.LineWidth(7f);
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(Color.Black);
                GL.Vertex3(va);
                GL.Vertex3(va + Vector3.Transform(new Vector3(0,0,h.Size), Matrix4.CreateRotationX(-h.Angle * ((float)Math.PI / 180f))));
                GL.End();*/

                GL.Color4(h.GetDisplayColor());

                // Draw everything to the stencil buffer
                RenderTools.beginTopLevelStencil();
                if (!h.IsSphere())
                {
                    h.va2 = new Vector3(h.X2, h.Y2, h.Z2);
                    if (h.Bone != -1) h.va2 = Vector3.TransformVector(h.va2, b.transform.ClearScale());
                    RenderTools.drawCylinder(h.va, h.va2, h.Size);
                }
                else
                {
                    RenderTools.drawSphere(h.va, h.Size, 30);
                }

                // n factorial (n!) algorithm (NOT EFFICIENT) to draw subsequent hitboxes around each other.
                // Will work fine for the low amounts of hitboxes in smash4.
                if (Runtime.renderHitboxesNoOverlap)
                {
                    // Remove the stencil for the already drawn hitboxes
                    RenderTools.beginTopLevelAntiStencil();
                    foreach (var pair2 in Runtime.gameAcmdScript.Hitboxes.Reverse())
                    {
                        if (pair2.Key == pair.Key)
                            break;  // this only works because the list is sorted
                        var h2 = pair2.Value;

                        if (!Runtime.HiddenHitboxes.Contains(h2.ID))
                        {

                            Bone b2 = getBone(h2.Bone);
                            var va = Vector3.TransformVector(new Vector3(h2.X, h2.Y, h2.Z), b2.transform.ClearScale());
                            if (!h2.IsSphere())
                            {
                                var va2 = new Vector3(h2.X2, h2.Y2, h2.Z2);
                                if (h2.Bone != -1) va2 = Vector3.TransformVector(va2, b2.transform.ClearScale());
                                RenderTools.drawCylinder(va, va2, h2.Size);
                            }
                            else
                            {
                                RenderTools.drawSphere(va, h2.Size, 30);
                            }
                        }
                    }
                }

                if (Runtime.SelectedHitboxID == h.ID)
                {
                    GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Runtime.hurtboxColorSelected));
                    if (!h.IsSphere())
                    {
                        RenderTools.drawWireframeCylinder(h.va, h.va2, h.Size);
                    }
                    else
                    {
                        RenderTools.drawWireframeSphere(h.va, h.Size, 10);
                    }
                }

                // End stenciling and draw over all the stenciled bits
                RenderTools.endTopLevelStencilAndDraw();
            }
            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
        }

        public void RenderInterpolatedHitboxes()
        {
            if (Runtime.gameAcmdScript != null && Runtime.gameAcmdScript.Hitboxes.Count > 0)
            {
                // Interpolation is one color for all Hitbox types and rendered all
                // at once to make a giant block. If people want sub-type
                // interpolation later we can add it.
                GL.Color4(Color.FromArgb(40, 0xC1, 0x0, 0x20));  // Vivid red
                GL.Enable(EnableCap.Blend);
                GL.Disable(EnableCap.CullFace);
                // Draw everything to the stencil buffer
                RenderTools.beginTopLevelStencil();

                // Render the interpolated area between the last frame's set of hitboxes
                // and the current frame's set of hitboxes
                foreach (var pair in Runtime.gameAcmdScript.Hitboxes)
                {
                    var h = pair.Value;

                    if (Runtime.HiddenHitboxes.Contains(h.ID))
                        continue;

                    Bone b = getBone(h.Bone);
                    Vector3 va = Vector3.TransformVector(new Vector3(h.X, h.Y, h.Z), b.transform.ClearScale());

                    // Draw a cylinder between the last known area and the current one
                    Hitbox lastMatchingHitbox = null;
                    bool success = Runtime.gameAcmdScript.LastHitboxes.TryGetValue(pair.Key, out lastMatchingHitbox);
                    if (success)
                    {
                        // Don't interpolate extended hitboxes - there is data to suggest they don't interpolate
                        if (!h.Extended)
                            RenderTools.drawCylinder(va, lastMatchingHitbox.va, h.Size);
                    }
                }

                // Now remove the spots for the current frame hitboxes so the colors don't get mixed
                RenderTools.beginTopLevelAntiStencil();
                foreach (var pair in Runtime.gameAcmdScript.Hitboxes)
                {
                    var h = pair.Value;

                    if (Runtime.HiddenHitboxes.Contains(h.ID))
                        continue;

                    Bone b = getBone(h.Bone);
                    Vector3 va = Vector3.TransformVector(new Vector3(h.X, h.Y, h.Z), b.transform.ClearScale());

                    if (!h.IsSphere())
                    {
                        Vector3 va2 = new Vector3(h.X2, h.Y2, h.Z2);
                        if (h.Bone != -1) va2 = Vector3.TransformVector(va2, b.transform.ClearScale());
                        RenderTools.drawCylinder(va, va2, h.Size);
                    }
                    else
                    {
                        RenderTools.drawSphere(va, h.Size, 30);
                    }
                }

                // End stenciling and draw over all the stenciled bits
                // which will now be the entire interpolated area minus
                // the new hitboxes
                RenderTools.endTopLevelStencilAndDraw();
                GL.Enable(EnableCap.CullFace);
                GL.Disable(EnableCap.Blend);
            }
        }

        public void RenderHurtboxes()
        {
            if (Runtime.ParamManager.Hurtboxes.Count > 0)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.Blend);

                if (Runtime.gameAcmdScript != null)
                {
                    if (Runtime.gameAcmdScript.BodyIntangible)
                        return;
                }

                if (Runtime.scriptId != -1)
                    if (Frame + 1 >= Runtime.ParamManager.MovesData[Runtime.scriptId].IntangibilityStart && Frame + 1 < Runtime.ParamManager.MovesData[Runtime.scriptId].IntangibilityEnd)
                        return;

                foreach (var pair in Runtime.ParamManager.Hurtboxes)
                {
                    var h = pair.Value;
                    if (!h.Visible)
                        continue;

                    var va = new Vector3(h.X, h.Y, h.Z);
                    Bone b = getBone(h.Bone);

                    if (Runtime.gameAcmdScript != null)
                    {
                        if (Runtime.gameAcmdScript.IntangibleBones.Contains(h.Bone))
                            continue;
                    }

                    //va = Vector3.Transform(va, b.transform);

                    GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Runtime.hurtboxColor));

                    if (Runtime.renderHurtboxesZone)
                    {
                        switch (h.Zone)
                        {
                            case Hurtbox.LW_ZONE:
                                GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Runtime.hurtboxColorLow));
                                break;
                            case Hurtbox.N_ZONE:
                                GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Runtime.hurtboxColorMed));
                                break;
                            case Hurtbox.HI_ZONE:
                                GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Runtime.hurtboxColorHi));
                                break;
                        }
                    }

                    if (Runtime.gameAcmdScript != null)
                    {
                        if (Runtime.gameAcmdScript.SuperArmor)
                            GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, 0x73, 0x0a, 0x43));

                        if (Runtime.gameAcmdScript.BodyInvincible)
                            GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Color.White));

                        if (Runtime.gameAcmdScript.InvincibleBones.Contains(h.Bone))
                            GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Color.White));
                    }

                    var va2 = new Vector3(h.X2, h.Y2, h.Z2);

                    //if (h.Bone != -1)va2 = Vector3.Transform(va2, b.transform);

                    if (h.isSphere)
                    {
                        RenderTools.drawSphereTransformedVisible(va, h.Size, 30, b.transform);
                    }
                    else
                    {
                        RenderTools.drawReducedCylinderTransformed(va, va2, h.Size, b.transform);
                    }
                    if (Runtime.SelectedHurtboxID == h.ID)
                    {
                        GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Runtime.hurtboxColorSelected));
                        if (h.isSphere)
                        {
                            RenderTools.drawWireframeSphereTransformedVisible(va, h.Size, 20, b.transform);
                        }
                        else
                        {
                            RenderTools.drawWireframeCylinderTransformed(va, va2, h.Size, b.transform);
                        }
                    }
                }

                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.DepthTest);
            }
        }

        public void RenderLedgeGrabboxes()
        {
            if (Runtime.gameAcmdScript != null)
                if (Runtime.gameAcmdScript.LedgeGrabDisallowed)
                    return;

            if (Runtime.ParamManager.LedgeGrabboxes.Count > 0)
            {
                GL.Enable(EnableCap.Blend);
                GL.Disable(EnableCap.CullFace);

                foreach (var pair in Runtime.ParamManager.LedgeGrabboxes)
                {
                    if (pair.Key >= 3) //ID 3 is only used for ZSS Brawl Up B which is unused in this game
                        return;

                    var l = pair.Value;

                    var va = new Vector3(0, l.Y1, l.Z1);
                    //Attached to bone 0
                    Bone b = getBone(0);
                    var va2 = new Vector3(0, l.Y2, l.Z2);

                    va = Vector3.TransformVector(va, b.transform);
                    va2 = Vector3.TransformVector(va2, b.transform);

                    if (Runtime.renderLedgeGrabboxes)
                    {
                        if (l.Tether)
                        {
                            if (!Runtime.renderTetherLedgeGrabboxes)
                                continue;
                            GL.Color4(Color.FromArgb(90, Color.DarkBlue));
                        }
                        else
                            switch (l.ID)
                            {
                                case 0:
                                    GL.Color4(Color.FromArgb(90, Color.DarkRed));
                                    break;
                                default:
                                    continue;
                                    //case 1:
                                    //    GL.Color4(Color.FromArgb(90, Color.DarkGreen));
                                    //    break;
                                    //case 2:
                                    //    GL.Color4(Color.FromArgb(90, Color.DarkOrange));
                                    //    break;
                                    //default:
                                    //    GL.Color4(Color.FromArgb(90, Color.DarkRed));
                                    //    break;
                            }


                        RenderTools.beginTopLevelStencil();

                        RenderTools.DrawRectangularPrism((va2 + va) / 2, 10, (va2.Y - va.Y) / 2, (va2.Z - va.Z) / 2);

                        RenderTools.endTopLevelStencilAndDraw();


                    }


                    //Reverse ledge grabbox
                    if (!l.Tether && Runtime.renderReverseLedgeGrabboxes)
                    {
                        Vector3 r = new Vector3(1, 1, 0.6f);

                        GL.Color4(Color.FromArgb(90, Color.DarkViolet));

                        RenderTools.beginTopLevelStencil();

                        if (Runtime.gameAcmdScript != null)
                        {
                            if (Runtime.gameAcmdScript.ReverseLedgeGrabAllowed)
                                RenderTools.DrawRectangularPrism((((va2 + va) / 2) - new Vector3(0, 0, (va2.Z - va.Z))) * r, 10, (va2.Y - va.Y) / 2, (va2.Z - va.Z) * r.Z / 2);
                        }
                        else
                        {
                            RenderTools.DrawRectangularPrism((((va2 + va) / 2) - new Vector3(0, 0, (va2.Z - va.Z))) * r, 10, (va2.Y - va.Y) / 2, (va2.Z - va.Z) * r.Z / 2);
                        }

                        RenderTools.endTopLevelStencilAndDraw();
                    }
                }
                GL.Disable(EnableCap.Blend);
                GL.Enable(EnableCap.CullFace);
            }
        }

        public void RenderECB()
        {
            if (Runtime.ParamManager.ECBs.Count > 0)
            {
                GL.Color4(Color.FromArgb(160, Color.DarkRed));
                GL.Disable(EnableCap.CullFace);
                GL.Enable(EnableCap.Blend);

                foreach (var pair in Runtime.ParamManager.ECBs)
                {
                    var e = pair.Value;
                    var va = new Vector3(e.X, e.Y, e.Z);
                    Bone b = getBone(e.Bone);

                    va = Vector3.TransformVector(va, b.transform.ClearScale());

                    // Draw everything to the stencil buffer
                    RenderTools.beginTopLevelStencil();
                    RenderTools.drawSphere(va, 0.5f, 30);
                    // End stenciling and draw over all the stenciled bits
                    RenderTools.endTopLevelStencilAndDraw();
                }

                //Render HipN bone point
                GL.Color4(Color.FromArgb(160, Color.DarkBlue));
                var v = new Vector3(0, 0, 0);
                Bone bone = getBone(3);
                v = Vector3.TransformVector(v, bone.transform.ClearScale());

                RenderTools.beginTopLevelStencil();
                RenderTools.drawSphere(v, 0.5f, 30);
                RenderTools.endTopLevelStencilAndDraw();

                GL.Disable(EnableCap.Blend);
                GL.Enable(EnableCap.CullFace);
            }
        }

        public void RenderSpecialBubbles()
        {

            if (TargetAnimString == null)
                return;

            if (Runtime.ParamManager.SpecialBubbles.Count > 0)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.Blend);

                foreach (var pair in Runtime.ParamManager.SpecialBubbles.Reverse())
                {
                    var h = pair.Value;

                    if (!h.Animations.Contains(TargetAnimString.Substring(3).Replace(".omo", "").ToLower()) && !h.Animations.Contains("*"))
                        continue;

                    if ((int)nupdFrame.Value < h.StartFrame)
                        continue;

                    if ((int)nupdFrame.Value > h.EndFrame && h.EndFrame != -1)
                        continue;

                    Bone b = getBone(h.Bone);

                    var va = new Vector3(h.X, h.Y, h.Z);

                    GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, h.Color));

                    var va2 = new Vector3(h.X2, h.Y2, h.Z2);

                    if (h.isSphere)
                    {
                        RenderTools.drawSphereTransformedVisible(va, h.Size, 30, b.transform.ClearScale());
                    }
                    else
                    {
                        RenderTools.drawReducedCylinderTransformed(va, va2, h.Size, b.transform.ClearScale());
                    }

                }
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.Blend);
            }
        }

        ACMDScript scr_sound;

        int lastFrame = 0;
        bool enterFrame = false;
        public void ProcessANMCMD_SOUND()
        {
            enterFrame = Frame != lastFrame;

            if (enterFrame)
            {
                int halt = 0;
                int e = 0;
                var cmd = scr_sound[e];

                List<int> soundtoplay = new List<int>();

                while (halt < Frame)
                {
                    switch (cmd.Ident)
                    {
                        case 0x42ACFE7D: // Asynchronous Timer
                            {
                                soundtoplay.Clear();
                                halt = (int)(float)cmd.Parameters[0] - 2;
                                break;
                            }
                        case 0x4B7B6E51: // Synchronous Timer
                            {
                                soundtoplay.Clear();
                                halt += (int)(float)cmd.Parameters[0];
                                break;
                            }
                        case 0xE5751F0A: // play sound 
                            {
                                if (Runtime.SoundContainers.Count > 0)
                                {
                                    soundtoplay.Add((int)cmd.Parameters[0] & 0xFF);
                                }
                                break;
                            }

                    }

                    e++;
                    if (e >= scr_sound.Count)
                        break;
                    else
                        cmd = scr_sound[e];

                    if (halt > Frame)
                        break;
                }

                foreach (int s in soundtoplay)
                {
                    //Runtime.SoundContainers[0].tone.tones[s].Play();
                }
            }

            lastFrame = Frame;


        }

        public void HandleACMD(string animname)
        {
            if (Runtime.acmdEditor.manualCrc)
                return;

            //Console.WriteLine("Handling " + animname);
            var crc = Crc32.Compute(animname.Replace(".omo", "").ToLower());

            Runtime.scriptId = -1;

            if (Runtime.Moveset == null)
            {
                Runtime.gameAcmdScript = null;
                return;
            }

            // Try and set up the editor
            try
            {
                if (Runtime.acmdEditor.crc != crc)
                    Runtime.acmdEditor.SetAnimation(crc);
            }
            catch { }

            //Putting scriptId here to get intangibility of the animation, previous method only did it for animations that had game scripts
            if (Runtime.Moveset.ScriptsHashList.Contains(crc))
                Runtime.scriptId = Runtime.Moveset.ScriptsHashList.IndexOf(crc);

            // Game script specific processing stuff below here
            if (!Runtime.Moveset.Game.Scripts.ContainsKey(crc))
            {
                //Some characters don't have AttackS[3-4]S and use attacks[3-4] crc32 hash on scripts making forge unable to access the script, thus not visualizing these hitboxes
                //If the character doesn't have angled ftilt/fsmash
                if (animname == "AttackS4S.omo" || animname == "AttackS3S.omo")
                {
                    HandleACMD(animname.Replace("S.omo", ".omo"));
                    return;
                }
                //Ryu ftilts
                else if (animname == "AttackS3Ss.omo")
                {
                    HandleACMD(animname.Replace("Ss.omo", "s.omo"));
                    return;
                }
                else if (animname == "AttackS3Sw.omo")
                {
                    HandleACMD(animname.Replace("Sw.omo", "w.omo"));
                    return;
                }
                //Rapid Jab Finisher
                else if (animname == "AttackEnd.omo")
                {
                    HandleACMD("Attack100End.omo");
                    return;
                }
                else if (animname.Contains("ZeldaPhantomMainPhantom"))
                {
                    HandleACMD(animname.Replace("ZeldaPhantomMainPhantom", ""));
                    return;
                }
                else if (animname == "SpecialHi1.omo")
                {
                    HandleACMD("SpecialHi.omo");
                    return;
                }
                else if (animname == "SpecialAirHi1.omo")
                {
                    HandleACMD("SpecialAirHi.omo");
                    return;
                }
                else
                {
                    Runtime.gameAcmdScript = null;
                    Runtime.hitboxList.refresh();
                    Runtime.variableViewer.refresh();
                    return;
                }
            }

            //Console.WriteLine("Handling " + animname);
            ACMDScript acmdScript = (ACMDScript)Runtime.Moveset.Game.Scripts[crc];
            // Only update the script if it changed
            if (acmdScript != null)
            {
                // If script wasn't set, or it was set and it changed, load the new script
                if (Runtime.gameAcmdScript == null || (Runtime.gameAcmdScript != null && Runtime.gameAcmdScript.script != acmdScript))
                {
                    Runtime.gameAcmdScript = new ForgeACMDScript(acmdScript);
                }
            }
            else
                Runtime.gameAcmdScript = null;
        }

        #endregion

        public void LoadAnimation(Animation a)
        {
            a.SetFrame(0);
            setAnimMaxFrames(a);

            // Will trigger a nupdFrame_ValueChanged event which will execute the vbn next frame
            nupdFrame.Value = 1;
        }

        public void setAnimMaxFrames(Animation a)
        {
            int totalAnimFrames = a.Size() > 1 ? a.Size() : 1;
            if (Runtime.ParamManager.MovesData.Count > 0 && Runtime.scriptId >= 0)
                if (Runtime.useFAFasAnimationLength)
                    totalAnimFrames = Runtime.ParamManager.MovesData[Runtime.scriptId].FAF > 0 ? Runtime.ParamManager.MovesData[Runtime.scriptId].FAF : totalAnimFrames;


            if (Runtime.useFAFasAnimationLength)
            {
                if (Runtime.useFrameDuration && Runtime.gameAcmdScript != null)
                    nupdMaxFrame.Value = (int)Runtime.gameAcmdScript.calculateFAF(totalAnimFrames);
                else
                    nupdMaxFrame.Value = totalAnimFrames;
            }
            else
            {
                if (Runtime.useFrameDuration && Runtime.gameAcmdScript != null)
                    nupdMaxFrame.Value = (int)Runtime.gameAcmdScript.calculateTotalFrames(totalAnimFrames);
                else
                    nupdMaxFrame.Value = totalAnimFrames;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            vbnViewportCamera.ResetPositionRotation();
            UpdateMousePosition();
            UpdateCameraPositionControl();
        }

        public void loadMTA(MTA m)
        {
            Console.WriteLine("MTA Loaded");
            Frame = 0;
            nupdFrame.Value = 1;
            nupdMaxFrame.Value = m.numFrames;
            //Console.WriteLine(m.numFrames);
            Runtime.TargetMTA.Clear();
            Runtime.TargetMTA.Add(m);
        }

        List<Bitmap> images = new List<Bitmap>();
        float ScaleFactor = 1f;

        private void VBNViewport_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == 'i')
            {
            }
            if (e.KeyChar == 'r')
            {
                CaptureScreen(true).Save(MainForm.executableDir + "\\Render.png");
            }
            if (e.KeyChar == 'p')
            {
                CaptureScreen(false).Save(MainForm.executableDir + "\\Render.png");
            }
            if (e.KeyChar == 'g')
            {
                if (TargetAnim == null)
                    return;

                isPlaying = false;
                btnPlay.Text = "Play";

                GIFSettings settings = new GIFSettings((int)this.nupdMaxFrame.Value, ScaleFactor, images.Count > 0);
                settings.ShowDialog();

                if (settings.ClearFrames)
                    images.Clear();

                if (!settings.OK)
                    return;

                ScaleFactor = settings.ScaleFactor;

                int cFrame = (int)this.nupdFrame.Value; //Get current frame so at the end of capturing all frames of the animation it goes back to this frame
                //Disable controls
                this.Enabled = false;

                for (int i = settings.StartFrame; i <= settings.EndFrame + 1; i++)
                {
                    this.nupdFrame.Value = i;
                    this.nupdFrame.Refresh(); //Refresh the frame counter control
                    Render();

                    if (i != settings.StartFrame) //On i=StartFrame it captures the frame the user had before setting frame to it so ignore that one, the +1 on the for makes it so the last frame is captured
                    {
                        Bitmap cs = CaptureScreen(false);
                        images.Add(new Bitmap(cs, new Size((int)(cs.Width / ScaleFactor), (int)(cs.Height / settings.ScaleFactor)))); //Resize images
                        cs.Dispose();
                    }
                }


                if (images.Count > 0 && !settings.StoreFrames)
                {
                    SaveFileDialog sf = new SaveFileDialog();

                    sf.FileName = "Render.gif";
                    sf.Filter = "GIF file (*.gif)|*.gif";

                    if (sf.ShowDialog() == DialogResult.OK)
                    {
                        GIFProgress g = new GIFProgress(images, sf.FileName, AnimationSpeed, settings.Repeat, settings.Quality);
                        g.Show();
                    }

                    images = new List<Bitmap>();

                }
                //Enable controls
                this.Enabled = true;

                this.nupdFrame.Value = cFrame;
            }
            if (e.KeyChar == ']')
            {
                foreach (TreeNode v in MainForm.Instance.animList.treeView1.Nodes)
                {
                    uint crc = Crc32.Compute(v.Text.Replace(".omo", "").Substring(3).ToLower());
                    if (Runtime.Moveset.Game.Scripts.ContainsKey(crc))
                        Console.WriteLine(v.Text);
                }
                Console.WriteLine("Done");
            }
            if (e.KeyChar == 'f')
            {
                fpsView = !fpsView;

                // I need to convert the camera stuff...
                if (fpsView)
                {
                    //Console.WriteLine(trans);
                    /*Quaternion rot = v.ExtractRotation();
                    Vector3 tr = Vector3.Transform(Vector3.Zero, v.Inverted());
                    width = tr.X / 5f;
                    height = (tr.Y+5f) / -5f;
                    zoom = -(tr.Z + 15f);*/
                }
                else
                {
                }
            }
        }

        System.Drawing.Point mouseDownPos = new System.Drawing.Point();
        public static Vector3 p1 = Vector3.Zero, p2 = Vector3.Zero;
        public int dbdistance = 0;
        bool freezeCamera = false;

        private void cbRenderMode_SelectionChangeCommitted(object sender, EventArgs e)
        {
            Runtime.renderType = (Runtime.RenderTypes)renderMode.SelectedIndex;
            Runtime.useDebugShading = renderMode.SelectedIndex > 0;
        }

        private void fpsCheckbox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void glControl1_DoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            float mouse_x = this.PointToClient(Cursor.Position).X;
            float mouse_y = this.PointToClient(Cursor.Position).Y;

            float x = (2.0f * mouse_x) / glControl1.Width - 1.0f;
            float y = 1.0f - (2.0f * mouse_y) / glControl1.Height;
            Vector4 va = Vector4.Transform(new Vector4(x, y, -1.0f, 1.0f), vbnViewportCamera.mvpMatrix.Inverted());
            Vector4 vb = Vector4.Transform(new Vector4(x, y, 1.0f, 1.0f), vbnViewportCamera.mvpMatrix.Inverted());

            p1 = va.Xyz;
            p2 = p1 - (va - (va + vb)).Xyz * 100;

            SortedList<double, Bone> selected = new SortedList<double, Bone>(new DuplicateKeyComparer<double>());

            foreach (ModelContainer con in ModelContainers)
            {
                if (con.VBN != null)
                {
                    foreach (Bone b in con.VBN.bones)
                    {
                        /*Vector3 closest = Vector3.Zero;
                        Vector3 cen = b.transform.ExtractTranslation();
                        if (!selected.Values.Contains(b) && RenderTools.CheckSphereHit(cen, .3f, p1, p2, out closest))
                        {
                            double dis = Math.Pow(closest.X - p1.X, 2) + Math.Pow(closest.Y - p1.Y, 2) + Math.Pow(closest.Z - p1.Z, 2);
                            selected.Add(dis, b);
                        }*/
                    }
                }
                if (con.NUD != null)
                {
                    foreach (NUD.Mesh mesh in con.NUD.Nodes)
                    {
                        Vector3 closest = Vector3.Zero;
                        foreach (NUD.Polygon poly in mesh.Nodes)
                        {
                            int i = 0;
                            foreach (NUD.Vertex v in poly.vertices)
                            {
                                poly.selectedVerts[i] = 0;
                                /*if (RenderTools.CheckSphereHit(v.pos, 1f, p1, p2, out closest))
                                {
                                    //Console.WriteLine("Selected Vert");
                                    poly.selectedVerts[i] = 1;
                                }*/
                                i++;
                            }
                        }
                    }
                }
            }
            //TransformTool.b = null;
            if (selected.Count > dbdistance)
            {
                if (Runtime.TargetVBN.bones.Contains(selected.Values.ElementAt(dbdistance)))
                {
                    /*MainForm.Instance.boneTreePanel.treeView1.SelectedNode = selected.Values.ElementAt(dbdistance);
                    transformTool.b = (Bone)MainForm.Instance.boneTreePanel.treeView1.SelectedNode;*/
                }
            }

            dbdistance += 1;
            if (dbdistance >= selected.Count) dbdistance = 0;
            _LastPoint = e.Location;
        }

        private void cbFAFanimation_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.useFAFasAnimationLength = cbFAFanimation.Checked;

            if (TargetAnim != null)
            {
                setAnimMaxFrames(TargetAnim);
            }
        }

        private void glControl1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mouseDownPos = this.PointToClient(Cursor.Position);
        }

        public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
        {
            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1;
                else
                    return result;
            }
        }

        private void glControl1_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            freezeCamera = false;
            if (transformTool.hit) freezeCamera = true;
            if (mouseDownPos == this.PointToClient(Cursor.Position) && 1 == 2)
            {
                float mouse_x = this.PointToClient(Cursor.Position).X;
                float mouse_y = this.PointToClient(Cursor.Position).Y;

                float x = (2.0f * mouse_x) / glControl1.Width - 1.0f;
                float y = 1.0f - (2.0f * mouse_y) / glControl1.Height;
                Vector4 va = Vector4.Transform(new Vector4(x, y, -1.0f, 1.0f), vbnViewportCamera.mvpMatrix.Inverted());
                Vector4 vb = Vector4.Transform(new Vector4(x, y, 1.0f, 1.0f), vbnViewportCamera.mvpMatrix.Inverted());

                p1 = va.Xyz;
                p2 = p1 - (va - (va + vb)).Xyz * 100;

                //freezeCamera = (RenderTools.intersectCircle(new Vector3(6, 6, 6), 5, 90, p1, p2));
            }
        }

        private void cbUseFrameSpeed_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.useFrameDuration = cbUseFrameSpeed.Checked;

            if (TargetAnim != null)
            {
                setAnimMaxFrames(TargetAnim);
            }
        }

        public void FPSCamera()
        {
            float cameraYRotation = 0;
            float cameraXRotation = 0;
            float width = 0;
            float zoom = 0;
            float height = 0;

            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.A))
                width += 1.0f;
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.D))
                width -= 1.0f;
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.W))
                zoom += 1.0f;
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.S))
                zoom -= 1.0f;

            cameraYRotation += 0.0125f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
            cameraXRotation += 0.005f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);

            mouseXLast = OpenTK.Input.Mouse.GetState().X;
            mouseYLast = OpenTK.Input.Mouse.GetState().Y;

            vbnViewportCamera.Update();
            modelMatrix = Matrix4.CreateRotationY(cameraYRotation) * Matrix4.CreateRotationX(cameraXRotation) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom);
        }

        public Bitmap CaptureScreen(bool saveAlpha)
        {
            int width = glControl1.Width;
            int height = glControl1.Height;

            byte[] pixels = new byte[width * height * 4];
            GL.ReadPixels(0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
            // Flip data because glReadPixels reads it in from bottom row to top row
            byte[] fixedPixels = new byte[width * height * 4];
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    // Remove alpha blending from the end image - we just want the post-render colors
                    if (!saveAlpha)
                        pixels[((w + h * width) * 4) + 3] = 255;

                    // Copy a 4 byte pixel one at a time
                    Array.Copy(pixels, (w + h * width) * 4, fixedPixels, ((height - h - 1) * width + w) * 4, 4);
                }
            }

            // Format and save the data
            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            Marshal.Copy(fixedPixels, 0, bmpData.Scan0, fixedPixels.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
        }
    }
}
