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
using System.Security.Cryptography;
using SALT.Moveset.AnimCMD;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using Gif.Components;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using Smash_Forge.Rendering.Lights;
using OpenTK.Input;
using Smash_Forge.Rendering;
using Smash_Forge.Params;

namespace Smash_Forge
{
    public partial class ModelViewport : EditorBase
    {
        // setup
        bool ReadyToRender = false;

        // View controls
        public Camera camera = new Camera();
        public GUI.Menus.CameraSettings cameraPosForm = null;

        // Rendering Stuff
        int colorHdrFbo;
        int colorHdrTex0;
        int colorHdrTex1;
        int hdrDepthRbo;

        int brightTexSmall;
        int brightHdrSmallFbo;
        int brightTexWidth;
        int brightTexHeight;

        // Screen Render
        int screenRenderFbo;
        int screenRenderRbo;

        // The viewport dimensions should be used for FBOs visible on screen.
        // Larger dimensions can be used for higher quality outputs for FBOs.
        int fboRenderWidth;
        int fboRenderHeight;

        // Functions of Viewer
        public enum Mode
        {
            Normal = 0,
            Photoshoot,
            Selection
        }
        public Mode CurrentMode = Mode.Normal;

        FrameTimer frameTime = new FrameTimer();

        VertexTool VertexTool = new VertexTool();
        TransformTool TransformTool = new TransformTool();

        //Animation
        private VBN TargetVBN;
        private Animation Animation;
        public Animation CurrentAnimation {
            get
            {
                return Animation;
            }
            set
            {
                //Moveset
                //If moveset is loaded then initialize with null script so handleACMD loads script for frame speed modifiers and FAF (if parameters are imported)
                if (MovesetManager != null && ACMDScript == null)
                    ACMDScript = new ForgeACMDScript(null);

                if(value != null)
                {
                    string TargetAnimString = value.Text;
                    if (!string.IsNullOrEmpty(TargetAnimString))
                    {
                        if (ACMDScript != null)
                        {
                            //Remove manual crc flag
                            //acmdEditor.manualCrc = false;
                            HandleACMD(TargetAnimString);
                            if (ACMDScript != null)
                                ACMDScript.processToFrame(0);

                        }
                    }
                }
                ResetModels();
                MaterialAnimation = null;
                Animation = value;
                totalFrame.Value = value.FrameCount;
                animationTrackBar.TickFrequency = 1;
                currentFrame.Value = 1;
                currentFrame.Value = 0;
            }
        }

        private MTA MaterialAnimation;
        public MTA CurrentMaterialAnimation
        {
            get
            {
                return MaterialAnimation;
            }
            set
            {
                ResetModels();
                Animation = null;
                MaterialAnimation = value;
                totalFrame.Value = value.numFrames;
                animationTrackBar.TickFrequency = 1;
                animationTrackBar.SetRange(0, (int)value.numFrames);
                currentFrame.Value = 1;
                currentFrame.Value = 0;
            }
        }

        // ACMD
        public int scriptId = -1;
        public Dictionary<string, int> ParamMoveNameIdMapping;
        public CharacterParamManager ParamManager;
        public PARAMEditor ParamManagerHelper;
        public MovesetManager MovesetManager
        {
            get
            {
                return _MovesetManager;
            }
            set
            {
                _MovesetManager = value;
                if(ACMDEditor != null)
                    ACMDEditor.updateCrcList();
            }
        }
        public MovesetManager _MovesetManager;
        public ForgeACMDScript ACMDScript = null;

        public ACMDPreviewEditor ACMDEditor;
        public HitboxList HitboxList;
        public HurtboxList HurtboxList;
        public VariableList VariableViewer;

        // Used in ModelContainer for direct UV time animation.
        public static Stopwatch directUVTimeStopWatch = new Stopwatch();

        //LVD
        public LVD LVD
        {
            get
            {
                return _lvd;
            }
            set
            {
                _lvd = value;
                _lvd.MeshList = MeshList;
                LVDEditor.LVD = _lvd;
                LVDList.TargetLVD = _lvd;
                LVDList.fillList();
            }
        }
        private LVD _lvd;
        LVDList LVDList = new LVDList();
        LVDEditor LVDEditor = new LVDEditor();

        //Path
        public PathBin PathBin;
        
        // Selection Functions
        public float sx1, sy1;
        
        //Animation Functions
        public int AnimationSpeed = 60;
        public float Frame = 0;
        public bool isPlaying;

        // Contents
        public MeshList MeshList = new MeshList();
        public AnimListPanel AnimList = new AnimListPanel();
        public TreeNodeCollection draw;

        // Photoshoot
        public bool freezeCamera = false;
        public int ShootX = 0;
        public int ShootY = 0;
        public int ShootWidth = 50;
        public int ShootHeight = 50;

        public ModelViewport()
        {
            InitializeComponent();
            camera = new Camera();
            FilePath = "";
            Text = "Model Viewport";

            MeshList.Dock = DockStyle.Right;
            MeshList.MaximumSize = new Size(300, 2000);
            MeshList.Size = new Size(300, 2000);
            AddControl(MeshList);

            AnimList.Dock = DockStyle.Left;
            AnimList.MaximumSize = new Size(300, 2000);
            AnimList.Size = new Size(300, 2000);
            AddControl(AnimList);

            LVDList.Dock = DockStyle.Left;
            LVDList.MaximumSize = new Size(300, 2000);
            AddControl(LVDList);
            LVDList.lvdEditor = LVDEditor;

            LVDEditor.Dock = DockStyle.Right;
            LVDEditor.MaximumSize = new Size(300, 2000);
            AddControl(LVDEditor);

            VertexTool.Dock = DockStyle.Left;
            VertexTool.MaximumSize = new Size(300, 2000);
            AddControl(VertexTool);
            VertexTool.vp = this;

            ACMDEditor = new ACMDPreviewEditor();
            ACMDEditor.Owner = this;
            ACMDEditor.Dock = DockStyle.Right;
            ACMDEditor.updateCrcList();
            AddControl(ACMDEditor);

            HitboxList = new HitboxList();
            HitboxList.Dock = DockStyle.Right;
            AddControl(HitboxList);

            HurtboxList = new HurtboxList();
            HurtboxList.Dock = DockStyle.Right;

            VariableViewer = new VariableList();
            VariableViewer.Dock = DockStyle.Right;

            LVD = new LVD();

            ViewComboBox.SelectedIndex = 0;

            draw = MeshList.filesTreeView.Nodes;

            RenderTools.Setup();

            SetupBuffersAndTextures();
        }

        private void SetupBuffersAndTextures()
        {
            // Use the viewport dimensions by default.
            fboRenderWidth = glViewport.Width;
            fboRenderHeight = glViewport.Height;

            FramebufferTools.CreateHdrFboTwoTextures(out colorHdrFbo, out hdrDepthRbo, out colorHdrTex0, out colorHdrTex1, fboRenderWidth, fboRenderHeight);
            brightTexWidth = (int)(glViewport.Width * Runtime.bloomTexScale);
            brightTexHeight = (int)(glViewport.Height * Runtime.bloomTexScale);
            FramebufferTools.CreateHdrFboSingleTextureNoDepth(out brightHdrSmallFbo, FramebufferTarget.Framebuffer, out brightTexSmall, brightTexWidth, brightTexHeight);

            SetupOffScreenFboRbo();

            // Bind the default framebuffer again.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private void SetupOffScreenFboRbo()
        {
            // Create an FBO/RBO just for saving screen renders.
            FramebufferTools.CreateOffscreenRenderFboRbo(out screenRenderFbo, out screenRenderRbo, FramebufferTarget.DrawFramebuffer, fboRenderWidth, fboRenderHeight);

            // Bind the default framebuffer again.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public ModelViewport(string filename) : this()
        {

        }

        ~ModelViewport()
        {
            
        }

        public Camera GetCamera()
        {
            return camera;
        }

        public override void Save()
        {
            if (FilePath.Equals(""))
            {
                SaveAs();
                return;
            }
            switch (Path.GetExtension(FilePath).ToLower())
            {
                case ".lvd":
                    _lvd.Save(FilePath);
                    break;
                case ".mtable":
                    _MovesetManager.Save(FilePath);
                    break;
            }
            Edited = false;
        }

        public override void SaveAs()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Smash 4 Level Data (.lvd)|*.lvd|" +
                             "ACMD|*.mtable|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".lvd") && _lvd != null)
                    {
                        FilePath = sfd.FileName;
                        Save();
                    }
                    if (sfd.FileName.EndsWith(".mtable") && _MovesetManager != null)
                    {
                        FilePath = sfd.FileName;
                        Save();
                    }
                }
            }
        }

        public void AddControl(Form frm)
        {
            frm.TopLevel = false;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Visible = false;
            Controls.Add(frm);
        }

        private void ModelViewport_Load(object sender, EventArgs e)
        {
            ReadyToRender = true;
            var timer = new Timer();
            timer.Interval = 1000 / 120;
            timer.Tick += new EventHandler(Application_Idle);
            timer.Start();

            InitializeLights();
        }

        private static void InitializeLights()
        {
            // TODO: Initialize Lights
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            if (this.IsDisposed)
                return;

            if (ReadyToRender)
            {
                if (isPlaying)
                {
                    if (animationTrackBar.Value == totalFrame.Value)
                        animationTrackBar.Value = 0;
                    else
                        animationTrackBar.Value++;
                }
                glViewport.Invalidate();
            }
        }

        public Vector2 GetMouseOnViewport()
        {
            float mouse_x = glViewport.PointToClient(Cursor.Position).X;
            float mouse_y = glViewport.PointToClient(Cursor.Position).Y;

            float mx = (2.0f * mouse_x) / glViewport.Width - 1.0f;
            float my = 1.0f - (2.0f * mouse_y) / glViewport.Height;
            return new Vector2(mx, my);
        }

        int dbdistance = 0;
        System.Drawing.Point _LastPoint;

        private void glViewport_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (ReadyToRender && glViewport != null)
            {
                glViewport.MakeCurrent();
                GL.LoadIdentity();
                GL.Viewport(0, 0, fboRenderWidth, fboRenderHeight);

                camera.renderWidth = glViewport.Width;
                camera.renderHeight = glViewport.Height;
                camera.Update();
            }
            //Mesh Selection Test
            if (e.Button == MouseButtons.Left)
            {
                Ray ray = new Ray(camera, glViewport);
                int selectedSize = 0;

                TransformTool.b = null;

                foreach (TreeNode node in draw)
                {
                    if (!(node is ModelContainer)) continue;
                    ModelContainer con = (ModelContainer)node;
                    if (modeBone.Checked)
                    {
                        SortedList<double, Bone> selected = con.GetBoneSelection(ray);
                        selectedSize = selected.Count;
                        if (selected.Count > dbdistance)// && MeshList.treeView1.Nodes.Contains(selected.Values.ElementAt(dbdistance)))
                            TransformTool.b = (Bone)selected.Values.ElementAt(dbdistance);
                        break;
                    }
                    if (modeMesh.Checked)
                    {
                        SortedList<double, NUD.Mesh> selected = con.GetMeshSelection(ray);
                        selectedSize = selected.Count;
                        if (selected.Count > dbdistance)
                            MeshList.filesTreeView.SelectedNode = selected.Values.ElementAt(dbdistance);
                    }
                }
                
                dbdistance += 1;
                if (dbdistance >= selectedSize) dbdistance = 0;
                _LastPoint = e.Location;
            }
        }

        private Vector3 getScreenPoint(Vector3 pos)
        {
            Vector4 n = Vector4.Transform(new Vector4(pos, 1), camera.mvpMatrix);
            n.X /= n.W;
            n.Y /= n.W;
            n.Z /= n.W;
            return n.Xyz;
        }

        private void glViewport_Resize(object sender, EventArgs e)
        {
            if (ReadyToRender && CurrentMode != Mode.Selection && glViewport.Height != 0 && glViewport.Width != 0)
            {
                GL.LoadIdentity();
                GL.Viewport(0, 0, fboRenderWidth, fboRenderHeight);

                camera.renderWidth = glViewport.Width;
                camera.renderHeight = glViewport.Height;
                fboRenderWidth = glViewport.Width;
                fboRenderHeight = glViewport.Height;
                camera.Update();

                ResizeTexturesAndBuffers();
            }
        }

        private void ResizeTexturesAndBuffers()
        {
            ResizeHdrFboRboTwoColorAttachments();
            ResizeSmallBrightHdrTexture();
            ResizeOffScreenRbo();
        }

        private void ResizeOffScreenRbo()
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, screenRenderFbo);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, screenRenderRbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, fboRenderWidth, fboRenderHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, screenRenderRbo);
        }

        private void ResizeSmallBrightHdrTexture()
        {
            // Small bright texture.
            GL.BindTexture(TextureTarget.Texture2D, brightTexSmall);
            brightTexWidth = (int)(glViewport.Width * Runtime.bloomTexScale);
            brightTexHeight = (int)(glViewport.Height * Runtime.bloomTexScale);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, brightTexWidth, brightTexHeight, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
        }

        private void ResizeHdrFboRboTwoColorAttachments()
        {
            // Resize the textures and buffers everytime the dimensions change.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, colorHdrFbo);

            int textureWidth = fboRenderWidth;
            int textureHeight = fboRenderHeight;

            // First color attachment.
            GL.BindTexture(TextureTarget.Texture2D, colorHdrTex0);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, textureWidth, textureHeight, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            // Second color attachment.
            GL.BindTexture(TextureTarget.Texture2D, colorHdrTex1);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, textureWidth, textureHeight, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            // Render buffer for the depth attachment.
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, hdrDepthRbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, textureWidth, textureHeight);
        }

        #region Animation Events

        private void animationTrackBar_ValueChanged(object sender, EventArgs e)
        {
            if (animationTrackBar.Value > totalFrame.Value)
                animationTrackBar.Value = 0;
            if (animationTrackBar.Value < 0)
                animationTrackBar.Value = (int)totalFrame.Value;
            currentFrame.Value = animationTrackBar.Value;


            int frameNum = animationTrackBar.Value;

            if (MaterialAnimation != null)
            {
                foreach (TreeNode node in MeshList.filesTreeView.Nodes)
                {
                    if (!(node is ModelContainer)) continue;
                    ModelContainer m = (ModelContainer)node;
                    m.NUD.ApplyMta(MaterialAnimation, frameNum);
                }
            }
            
            if (Animation == null) return;

            // Process script first in case we have to speed up the animation
            if (ACMDScript != null)
                ACMDScript.processToFrame(frameNum);

            float animFrameNum = frameNum;
            if (ACMDScript != null && Runtime.useFrameDuration)
                animFrameNum = ACMDScript.animationFrame;// - 1;
            
            foreach (TreeNode node in MeshList.filesTreeView.Nodes)
            {
                if (!(node is ModelContainer)) continue;
                ModelContainer m = (ModelContainer)node;
                Animation.SetFrame(animFrameNum);
                if (m.VBN != null)
                    Animation.NextFrame(m.VBN);

                // Deliberately do not ever use ACMD/animFrame to modify these other types of model
                if (m.DAT_MELEE != null)
                {
                    Animation.SetFrame(frameNum);
                    Animation.NextFrame(m.DAT_MELEE.bones);
                }
                if (m.BCH != null)
                {
                    foreach (BCH_Model mod in m.BCH.Models.Nodes)
                    {
                        if (mod.skeleton != null)
                        {
                            Animation.SetFrame(animFrameNum);
                            Animation.NextFrame(mod.skeleton);
                        }
                    }
                }
            }

            //Frame = (int)animFrameNum;
        }

        public void ResetModels()
        {
            foreach (TreeNode node in MeshList.filesTreeView.Nodes)
            {
                if (!(node is ModelContainer)) continue;
                ModelContainer m = (ModelContainer)node;
                m.NUD.ClearMta();
                if (m.VBN != null)
                    m.VBN.reset();

                // Deliberately do not ever use ACMD/animFrame to modify these other types of model
                if (m.DAT_MELEE != null)
                {
                    m.DAT_MELEE.bones.reset();
                }
                if (m.BCH != null)
                {
                    foreach (BCH_Model mod in m.BCH.Models.Nodes)
                    {
                        if (mod.skeleton != null)
                        {
                            mod.skeleton.reset();
                        }
                    }
                }
            }
        }

        private void currentFrame_ValueChanged(object sender, EventArgs e)
        {
            if (currentFrame.Value > totalFrame.Value)
                currentFrame.Value = totalFrame.Value;
            animationTrackBar.Value = (int)currentFrame.Value;
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            isPlaying = !isPlaying;
            playButton.Text = isPlaying ? "Pause" : "Play";

            if (isPlaying)
                directUVTimeStopWatch.Start();
            else
                directUVTimeStopWatch.Stop();
        }

        #endregion

        private void ResetCamera_Click(object sender, EventArgs e)
        {
            // Frame the selected NUD or mesh based on the bounding spheres. Frame the NUD if nothing is selected. 
            FrameSelectionAndSort();
        }

        public void FrameSelectionAndSort()
        {
            if (MeshList.filesTreeView.SelectedNode is NUD.Mesh)
            {
                FrameSelectedMesh();
            }
            else if (MeshList.filesTreeView.SelectedNode is NUD)
            {
                FrameSelectedNud();
            }
            else if (MeshList.filesTreeView.SelectedNode is NUD.Polygon)
            {
                FrameSelectedPolygon();
            }
            else if (MeshList.filesTreeView.SelectedNode is ModelContainer)
            {
                FrameSelectedModelContainer();
            }
            else
            {
                FrameAllModelContainers();
            }

            // Depth sorting. 
            foreach (TreeNode node in MeshList.filesTreeView.Nodes)
            {
                if (node is ModelContainer)
                {
                    ModelContainer modelContainer = (ModelContainer)node;
                    modelContainer.DepthSortModels(camera.position);
                }
            }
        }

        private void FrameSelectedModelContainer()
        {
            ModelContainer modelContainer = (ModelContainer)MeshList.filesTreeView.SelectedNode;
            float[] boundingBox = new float[] { 0, 0, 0, 0 };

            // Use the main bounding box for the NUD.
            if (modelContainer.NUD.boundingBox[3] > boundingBox[3])
            {
                boundingBox[0] = modelContainer.NUD.boundingBox[0];
                boundingBox[1] = modelContainer.NUD.boundingBox[1];
                boundingBox[2] = modelContainer.NUD.boundingBox[2];
                boundingBox[3] = modelContainer.NUD.boundingBox[3];
            }

            // It's possible that only the individual meshes have bounding boxes.
            foreach (NUD.Mesh mesh in modelContainer.NUD.Nodes)
            {
                if (mesh.boundingBox[3] > boundingBox[3])
                {
                    boundingBox[0] = mesh.boundingBox[0];
                    boundingBox[1] = mesh.boundingBox[1];
                    boundingBox[2] = mesh.boundingBox[2];
                    boundingBox[3] = mesh.boundingBox[3];
                }
            }

            camera.FrameSelection(new Vector3(boundingBox[0], boundingBox[1], boundingBox[2]), boundingBox[3]);
            camera.Update();
        }

        private void FrameSelectedMesh()
        {
            NUD.Mesh mesh = (NUD.Mesh)MeshList.filesTreeView.SelectedNode;
            float[] boundingBox = mesh.boundingBox;
            camera.FrameSelection(new Vector3(boundingBox[0], boundingBox[1], boundingBox[2]), boundingBox[3]);
            camera.Update();
        }

        private void FrameSelectedNud()
        {
            NUD nud = (NUD)MeshList.filesTreeView.SelectedNode;
            float[] boundingBox = nud.boundingBox;
            camera.FrameSelection(new Vector3(boundingBox[0], boundingBox[1], boundingBox[2]), boundingBox[3]);
            camera.Update();
        }

        private void FrameSelectedPolygon()
        {
            NUD.Mesh mesh = (NUD.Mesh)MeshList.filesTreeView.SelectedNode.Parent;
            float[] boundingBox = mesh.boundingBox;
            camera.FrameSelection(new Vector3(boundingBox[0], boundingBox[1], boundingBox[2]), boundingBox[3]);
            camera.Update();
        }

        private void FrameAllModelContainers(float maxBoundingRadius = 400)
        {
            // Find the max NUD bounding box for all models. 
            float[] boundingBox = new float[] { 0, 0, 0, 0 };
            foreach (TreeNode node in MeshList.filesTreeView.Nodes)
            {
                if (node is ModelContainer)
                {
                    ModelContainer modelContainer = (ModelContainer)node;

                    // Use the main bounding box for the NUD.
                    if ((modelContainer.NUD.boundingBox[3] > boundingBox[3]) && (modelContainer.NUD.boundingBox[3] < maxBoundingRadius))
                    {
                        boundingBox[0] = modelContainer.NUD.boundingBox[0];
                        boundingBox[1] = modelContainer.NUD.boundingBox[1];
                        boundingBox[2] = modelContainer.NUD.boundingBox[2];
                        boundingBox[3] = modelContainer.NUD.boundingBox[3];
                    }

                    // It's possible that only the individual meshes have bounding boxes.
                    foreach (NUD.Mesh mesh in modelContainer.NUD.Nodes)
                    {
                        if (mesh.boundingBox[3] > boundingBox[3] && mesh.boundingBox[3] < maxBoundingRadius)
                        {
                            boundingBox[0] = mesh.boundingBox[0];
                            boundingBox[1] = mesh.boundingBox[1];
                            boundingBox[2] = mesh.boundingBox[2];
                            boundingBox[3] = mesh.boundingBox[3];
                        }
                    }
                }

            }

            camera.FrameSelection(new Vector3(boundingBox[0], boundingBox[1], boundingBox[2]), boundingBox[3]);
            camera.Update();
        }

        #region Moveset

        public void HandleACMD(string animname)
        {
            //if (ACMDEditor.manualCrc)
            //    return;

            var crc = Crc32.Compute(animname.Replace(".omo", "").ToLower());

            scriptId = -1;

            if (MovesetManager == null)
            {
                ACMDScript = null;
                return;
            }

            // Try and set up the editor
            try
            {
                if (ACMDEditor.crc != crc)
                    ACMDEditor.SetAnimation(crc);
            }
            catch { }

            //Putting scriptId here to get intangibility of the animation, previous method only did it for animations that had game scripts
            if (MovesetManager.ScriptsHashList.Contains(crc))
                scriptId = MovesetManager.ScriptsHashList.IndexOf(crc);

            // Game script specific processing stuff below here
            if (!MovesetManager.Game.Scripts.ContainsKey(crc))
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
                    ACMDScript = null;
                    HitboxList.refresh();
                    VariableViewer.refresh();
                    return;
                }
            }

            ACMDScript acmdScript = (ACMDScript)MovesetManager.Game.Scripts[crc];
            // Only update the script if it changed
            if (acmdScript != null)
            {
                // If script wasn't set, or it was set and it changed, load the new script
                if (ACMDScript == null || (ACMDScript != null && ACMDScript.script != acmdScript))
                {
                    ACMDScript = new ForgeACMDScript(acmdScript);
                }
            }
            else
                ACMDScript = null;
        }

        #endregion


        private void CameraSettings_Click(object sender, EventArgs e)
        {
            if(cameraPosForm == null)
                cameraPosForm = new GUI.Menus.CameraSettings(camera);
            cameraPosForm.ShowDialog();
        }

        private void glViewport_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (CurrentMode != Mode.Selection && !freezeCamera)
                camera.Update();
        }

        #region Controls

        public void HideAll()
        {
            LVDEditor.Visible = false;
            LVDList.Visible = false;
            MeshList.Visible = false;
            AnimList.Visible = false;
            ACMDEditor.Visible = false;
            VertexTool.Visible = false;
            totalFrame.Enabled = false;
        }

        private void ViewComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            HideAll();
            // Use a string so the order of the items can be changed later. 
            switch (ViewComboBox.SelectedItem.ToString())
            {
                case "Model Viewer":
                    MeshList.Visible = true;
                    AnimList.Visible = true;
                    break;
                case "Model Editor":
                    MeshList.Visible = true;
                    VertexTool.Visible = true;
                    break;
                case "Animation Editor":
                    AnimList.Visible = true;
                    totalFrame.Enabled = true;
                    break;
                case "LVD Editor":
                    LVDEditor.Visible = true;
                    LVDList.Visible = true;
                    break;
                case "ACMD Editor":
                    AnimList.Visible = true;
                    ACMDEditor.Visible = true;
                    break;
                case "Clean":
                    LVDEditor.Visible = false;
                    LVDList.Visible = false;
                    MeshList.Visible = false;
                    AnimList.Visible = false;
                    ACMDEditor.Visible = false;
                    VertexTool.Visible = false;
                    totalFrame.Enabled = false;
                    break;
            }
        }

        private void toolStripSaveRenderAlphaButton_Click(object sender, EventArgs e)
        {
            SaveScreenRender(true);
        }

        private void BatchRenderModels()
        {
            Runtime.checkNudTexIdOnOpen = false;

            // Get the source model folder and then the output folder. 
            using (var folderSelect = new FolderSelectDialog())
            {
                folderSelect.Title = "Models Directory";
                if (folderSelect.ShowDialog() == DialogResult.OK)
                {
                    string[] files = Directory.GetFiles(folderSelect.SelectedPath, "*model.nud", SearchOption.AllDirectories);

                    using (var outputFolderSelect = new FolderSelectDialog())
                    {
                        outputFolderSelect.Title = "Output Renders Directory";
                        if (outputFolderSelect.ShowDialog() == DialogResult.OK)
                        {
                            for (int i = 0; i < files.Length; i++)
                            {
                                try
                                {
                                    MainForm.Instance.OpenNud(files[i], "", this);
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine(e.Message);
                                    Debug.WriteLine(e.StackTrace);
                                }
                                RenderModel(files[i], folderSelect.SelectedPath, outputFolderSelect.SelectedPath);
                            }
                        }
                    }
                }
            }

            Runtime.checkNudTexIdOnOpen = true;
        }

        private void BatchRenderStages()
        {
            // Get the source model folder and then the output folder. 
            using (var sourceFolderSelect = new FolderSelectDialog())
            {
                sourceFolderSelect.Title = "Stages Directory";
                if (sourceFolderSelect.ShowDialog() == DialogResult.OK)
                {
                    using (var outputFolderSelect = new FolderSelectDialog())
                    {
                        outputFolderSelect.Title = "Output Renders Directory";
                        if (outputFolderSelect.ShowDialog() == DialogResult.OK)
                        {
                            foreach (string stageFolder in Directory.GetDirectories(sourceFolderSelect.SelectedPath))
                            {
                                MainForm.Instance.OpenStageFolder(stageFolder, this);
                                RenderStageModels(stageFolder, outputFolderSelect.SelectedPath, sourceFolderSelect.SelectedPath);
                                RenderModel(stageFolder, sourceFolderSelect.SelectedPath, outputFolderSelect.SelectedPath);
                                MainForm.Instance.ClearWorkSpace(false);
                            }
                        }
                    }
                }
            }
        }

        private void RenderModel(string nudFileName, string sourcePath, string outputPath)
        {
            SetupNextRender();
            string renderName = FormatFileName(nudFileName, sourcePath);
            // Manually dispose the bitmap to avoid memory leaks. 
            Bitmap screenCapture = FramebufferTools.ReadFrameBufferPixels(0, FramebufferTarget.Framebuffer, fboRenderWidth, fboRenderHeight, true);
            screenCapture.Save(outputPath + "\\" + renderName + ".png");
            screenCapture.Dispose();

            // Cleanup the models and nodes.
            Runtime.TextureContainers.Clear();
            draw.Clear();
        }

        private void SetupNextRender()
        {
            // Setup before rendering the model. Use a large max radius to show skybox models.
            FrameAllModelContainers();
            Render(null, null);
            glViewport.SwapBuffers();
        }

        private static string FormatFileName(string fileName, string path)
        {
            // Save the render using the folder structure as the name.
            string renderName = fileName.Replace(path, "");
            renderName = renderName.Substring(1);
            renderName = renderName.Replace("\\", "_");
            renderName = renderName.Replace("//", "_");
            renderName = renderName.Replace(".nud", "");
            return renderName;
        }

        private void GIFButton_Click(object sender, EventArgs e)
        {
            if (Animation == null)
                return;

            List<Bitmap> images = new List<Bitmap>();
            float ScaleFactor = 1f;
            isPlaying = false;
            playButton.Text = "Play";

            GIFSettings settings = new GIFSettings((int)totalFrame.Value, ScaleFactor, images.Count > 0);
            settings.ShowDialog();

            if (settings.ClearFrames)
                images.Clear();

            if (!settings.OK)
                return;

            ScaleFactor = settings.ScaleFactor;

            int cFrame = (int)currentFrame.Value; //Get current frame so at the end of capturing all frames of the animation it goes back to this frame
                                                    //Disable controls
            this.Enabled = false;

            for (int i = settings.StartFrame; i <= settings.EndFrame + 1; i++)
            {
                currentFrame.Value = i;
                currentFrame.Refresh(); //Refresh the frame counter control
                Render(null, null);

                if (i != settings.StartFrame) //On i=StartFrame it captures the frame the user had before setting frame to it so ignore that one, the +1 on the for makes it so the last frame is captured
                {
                    Bitmap cs = FramebufferTools.ReadFrameBufferPixels(0, FramebufferTarget.Framebuffer, fboRenderWidth, fboRenderWidth);
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

            currentFrame.Value = cFrame;
        }

        private void ModelViewport_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (TreeNode n in MeshList.filesTreeView.Nodes)
            {
                if (n is ModelContainer)
                {
                    Runtime.TextureContainers.Remove(((ModelContainer)n).NUT);
                    ((ModelContainer)n).NUT.Destroy();
                    ((ModelContainer)n).NUD.Destroy();
                }
            }
            
            GC.Collect();
        }

        private void beginButton_Click(object sender, EventArgs e)
        {
            currentFrame.Value = 0;
        }

        private void endButton_Click(object sender, EventArgs e)
        {
            currentFrame.Value = totalFrame.Value;
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if (currentFrame.Value != totalFrame.Value)
                currentFrame.Value++;
        }

        private void prevButton_Click(object sender, EventArgs e)
        {
            if(currentFrame.Value != 0)
                currentFrame.Value--;
        }

        private void viewStripButtons(object sender, EventArgs e)
        {
            modeBone.Checked = false;
            modePolygon.Checked = false;
            modeMesh.Checked = false;
            ((ToolStripButton)sender).Checked = true;
        }

        private void viewStripButtonsBone(object sender, EventArgs e)
        {
            stripPos.Checked = false;
            stripRot.Checked = false;
            stripSca.Checked = false;
            ((ToolStripButton)sender).Checked = true;
            if (stripPos.Checked)
                TransformTool.Type = TransformTool.ToolTypes.POSITION;
            if (stripRot.Checked)
                TransformTool.Type = TransformTool.ToolTypes.ROTATION;
            if (stripSca.Checked)
                TransformTool.Type = TransformTool.ToolTypes.SCALE;
        }

        #endregion

        private void ModelViewport_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach(TreeNode n in MeshList.filesTreeView.Nodes)
            {
                if(n is ModelContainer)
                {
                    ((ModelContainer)n).NUD.Dispose();
                    ((ModelContainer)n).NUT.Destroy();
                }
            }
        }

        private void toolStripRenderNoAlphaButton_Click(object sender, EventArgs e)
        {
            SaveScreenRender();
        }
        
        private void ModelViewport_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {

        }

        private void totalFrame_ValueChanged(object sender, EventArgs e)
        {
            if (Animation == null) return;
            if(totalFrame.Value < 1)
            {
                totalFrame.Value = 1;
            }else
            {
                if(Animation.Tag is Animation)
                    ((Animation)Animation.Tag).FrameCount = (int)totalFrame.Value;
                Animation.FrameCount = (int)totalFrame.Value;
                animationTrackBar.Value = 0;
                animationTrackBar.SetRange(0, Animation.FrameCount);
            }
        }

        private void checkSelect()
        {
            if (CurrentMode == Mode.Selection)
            {
                Vector2 m = GetMouseOnViewport();
                if (!m.Equals(new Vector2(sx1, sy1)))
                {
                    // select group of vertices
                    float minx = Math.Min(sx1, m.X);
                    float miny = Math.Min(sy1, m.Y);
                    float width = Math.Abs(sx1 - m.X);
                    float height = Math.Abs(sy1 - m.Y);

                    foreach (TreeNode node in draw)
                    {
                        if (!(node is ModelContainer)) continue;
                        ModelContainer con = (ModelContainer)node;
                        foreach (NUD.Mesh mesh in con.NUD.Nodes)
                        {
                            foreach (NUD.Polygon poly in mesh.Nodes)
                            {
                                //if (!poly.IsSelected && !mesh.IsSelected) continue;
                                int i = 0;
                                foreach (NUD.Vertex v in poly.vertices)
                                {
                                    if (!OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ControlLeft))
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
                    // Selects the closest vertex
                    Ray r = RenderTools.CreateRay(camera.mvpMatrix, GetMouseOnViewport());
                    Vector3 close = Vector3.Zero;
                    foreach (TreeNode node in draw)
                    {
                        if (!(node is ModelContainer)) continue;
                        ModelContainer con = (ModelContainer)node;
                        NUD.Polygon Close = null;
                        int index = 0;
                        double mindis = 999;
                        foreach (NUD.Mesh mesh in con.NUD.Nodes)
                        {
                            foreach (NUD.Polygon poly in mesh.Nodes)
                            {
                                //if (!poly.IsSelected && !mesh.IsSelected) continue;
                                int i = 0;
                                foreach (NUD.Vertex v in poly.vertices)
                                {
                                    //if (!poly.IsSelected) continue;
                                    if (!OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ControlLeft))
                                        poly.selectedVerts[i] = 0;

                                    if (r.TrySphereHit(v.pos, 0.2f, out close))
                                    {
                                        double dis = r.Distance(close);
                                        if (dis < mindis)
                                        {
                                            mindis = dis;
                                            Close = poly;
                                            index = i;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        if (Close != null)
                        {
                            Close.selectedVerts[index] = 1;
                        }
                    }
                }

                VertexTool.vertexListBox.BeginUpdate();
                VertexTool.vertexListBox.Items.Clear();
                foreach (TreeNode node in draw)
                {
                    if (!(node is ModelContainer)) continue;
                    ModelContainer con = (ModelContainer)node;
                    foreach (NUD.Mesh mesh in con.NUD.Nodes)
                    {
                        foreach (NUD.Polygon poly in mesh.Nodes)
                        {
                            int i = 0;
                            foreach (NUD.Vertex v in poly.vertices)
                            {
                                if (poly.selectedVerts[i++] == 1)
                                {
                                    VertexTool.vertexListBox.Items.Add(v);
                                }
                            }
                        }
                    }
                }
                VertexTool.vertexListBox.EndUpdate();
                CurrentMode = Mode.Normal;
            }
        }

        private void glViewport_Click(object sender, EventArgs e)
        {
            
        }

        private void glViewport_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //checkSelect();
        }

        private void weightToolButton_Click(object sender, EventArgs e)
        {
            //vertexTool.Show();
        }

        private void Render(object sender, PaintEventArgs e)
        {
            if (!ReadyToRender)
                return;

            SetupViewport();

            // Bind the default framebuffer in case it was set elsewhere.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // Push all attributes so we don't have to clean up later
            GL.PushAttrib(AttribMask.AllAttribBits);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            if (MeshList.filesTreeView.SelectedNode != null)
            {
                // Return early to avoid rendering other stuff. 
                if (MeshList.filesTreeView.SelectedNode is BCH_Texture)
                {
                    DrawBchTex();
                    return;
                }
                if (MeshList.filesTreeView.SelectedNode is NutTexture)
                {
                    DrawNutTexAndUvs();
                    return;
                }
            }

            if (Runtime.usePostProcessing)
            {
                // Render models and background into an HDR buffer. 
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, colorHdrFbo);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }

            if (Runtime.renderBackGround && !Runtime.usePostProcessing)
            {
                // The screen quad shader draws its own background gradient.
                // This prevents the second color attachment from having a white background.
                DrawViewportBackground();
            }

            if (glViewport.ClientRectangle.Contains(glViewport.PointToClient(Cursor.Position))
             && glViewport.Focused
             && (CurrentMode == Mode.Normal || (CurrentMode == Mode.Photoshoot && !freezeCamera))
             && !TransformTool.hit)
            {
                camera.Update();
            }
            try
            {
                if (OpenTK.Input.Mouse.GetState() != null)
                    camera.mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;
            }
            catch
            {

            }

            if (cameraPosForm != null)
                cameraPosForm.ApplyCameraAnimation(camera, animationTrackBar.Value);

            if (Runtime.renderFloor)
                RenderTools.DrawFloor(camera.mvpMatrix);

            SetDepthTesting();

            DrawModels();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            if (Runtime.usePostProcessing)
            {
                // Draw the texture to the screen into a smaller FBO.
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, brightHdrSmallFbo);
                GL.Viewport(0, 0, brightTexWidth, brightTexHeight);
                RenderTools.DrawTexturedQuad(colorHdrTex1, brightTexWidth, brightTexHeight);

                // Setup the normal viewport dimensions again.
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Viewport(0, 0, fboRenderWidth, fboRenderHeight);

                RenderTools.DrawScreenQuadPostProcessing(colorHdrTex0, brightTexSmall);
            }

            FixedFunctionRendering();

            GL.PopAttrib();
            glViewport.SwapBuffers();
        }

        private static void SetDepthTesting()
        {
            // Allow disabling depth testing for experimental "flat" rendering. 
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            if (!Runtime.useDepthTest)
                GL.Disable(EnableCap.DepthTest);
        }

        private void FixedFunctionRendering()
        {
            RenderTools.Setup3DFixedFunctionRendering(camera.mvpMatrix);

            // Bounding boxes should not render on top.
            if (Runtime.drawAreaLightBoundingBoxes)
                DrawAreaLightBoundingBoxes();

            DrawOverlays();
        }

        private static void DrawViewportBackground()
        {
            Vector3 topColor = ColorTools.Vector4FromColor(Runtime.backgroundGradientTop).Xyz;
            Vector3 bottomColor = ColorTools.Vector4FromColor(Runtime.backgroundGradientBottom).Xyz;

            // Only use the top color for solid color rendering.
            if (Runtime.backgroundStyle == Runtime.BackgroundStyle.Solid)
                RenderTools.DrawQuadGradient(topColor, topColor);
            else
                RenderTools.DrawQuadGradient(topColor, bottomColor);
        }

        private void SetupViewport()
        {
            glViewport.MakeCurrent();
            GL.LoadIdentity();
            GL.Viewport(0, 0, fboRenderWidth, fboRenderHeight);
        }

        private void DrawModels()
        {
            if (Runtime.renderModel || Runtime.renderModelWireframe)
                foreach (TreeNode m in draw)
                    if (m is ModelContainer)
                        ((ModelContainer)m).Render(camera, 0, Matrix4.Zero, camera.mvpMatrix, new Vector2(glViewport.Width, glViewport.Height));

            if (ViewComboBox.SelectedIndex == 1)
                foreach (TreeNode m in draw)
                    if (m is ModelContainer)
                        ((ModelContainer)m).RenderPoints(camera);
        }

        private void DrawOverlays()
        {
            // Clearing the depth buffer allows stuff to render on top of the models.
            GL.Clear(ClearBufferMask.DepthBufferBit);

            if (Runtime.renderLVD)
                _lvd.Render();

            if (Runtime.renderBones)
                foreach (ModelContainer m in draw)
                    m.RenderBones();

            // ACMD
            if (ParamManager != null && Runtime.renderHurtboxes && draw.Count > 0 && (draw[0] is ModelContainer))
            {
                // Doesn't do anything. ParamManager is always null.
                ParamManager.RenderHurtboxes(Frame, scriptId, ACMDScript, ((ModelContainer)draw[0]).GetVBN());
            }

            if (ACMDScript != null && draw.Count > 0 && (draw[0] is ModelContainer))
                ACMDScript.Render(((ModelContainer)draw[0]).GetVBN());

            if (ViewComboBox.SelectedIndex == 2)
            {
                DrawBoneTransformTool();
            }

            if (ViewComboBox.SelectedIndex == 1)
            {
                MouseSelectionStuff();
            }

            if (CurrentMode == Mode.Photoshoot)
            {
                freezeCamera = false;
                if (Keyboard.GetState().IsKeyDown(Key.W) && Mouse.GetState().IsButtonDown(MouseButton.Left))
                {
                    ShootX = glViewport.PointToClient(Cursor.Position).X;
                    ShootY = glViewport.PointToClient(Cursor.Position).Y;
                    freezeCamera = true;
                }
                RenderTools.DrawPhotoshoot(glViewport, ShootX, ShootY, ShootWidth, ShootHeight);
            }
        }

        private void MouseSelectionStuff()
        {
            try
            {
                if (CurrentMode == Mode.Normal && OpenTK.Input.Mouse.GetState().IsButtonDown(OpenTK.Input.MouseButton.Right))
                {
                    CurrentMode = Mode.Selection;
                    Vector2 m = GetMouseOnViewport();
                    sx1 = m.X;
                    sy1 = m.Y;
                }
            }
            catch
            {

            }
            if (CurrentMode == Mode.Selection)
            {
                if (!OpenTK.Input.Mouse.GetState().IsButtonDown(OpenTK.Input.MouseButton.Right))
                {
                    checkSelect();
                    CurrentMode = Mode.Normal;
                }

                GL.MatrixMode(MatrixMode.Modelview);
                GL.PushMatrix();
                GL.LoadIdentity();

                Vector2 m = GetMouseOnViewport();
                GL.Color3(Color.Black);
                GL.LineWidth(2f);
                GL.Begin(PrimitiveType.LineLoop);
                GL.Vertex2(sx1, sy1);
                GL.Vertex2(m.X, sy1);
                GL.Vertex2(m.X, m.Y);
                GL.Vertex2(sx1, m.Y);
                GL.End();

                GL.Color3(Color.White);
                GL.LineWidth(1f);
                GL.Begin(PrimitiveType.LineLoop);
                GL.Vertex2(sx1, sy1);
                GL.Vertex2(m.X, sy1);
                GL.Vertex2(m.X, m.Y);
                GL.Vertex2(sx1, m.Y);
                GL.End();
                GL.PopMatrix();
            }
        }

        private void DrawBoneTransformTool()
        {
            if (modeBone.Checked)
            {
                TransformTool.Render(camera, new Ray(camera, glViewport));
                if (TransformTool.state == 1)
                    CurrentMode = Mode.Selection;
                else
                    CurrentMode = Mode.Normal;
            }

            if (TransformTool.HasChanged())
            {
                if (Animation != null && TransformTool.b != null)
                {
                    // get the node group for the current bone in animation
                    Animation.KeyNode ThisNode = null;
                    foreach (Animation.KeyNode node in Animation.Bones)
                    {
                        if (node.Text.Equals(TransformTool.b.Text))
                        {
                            // found
                            ThisNode = node;
                            break;
                        }
                    }
                    if (ThisNode == null)
                    {
                        ThisNode = new Animation.KeyNode(TransformTool.b.Text);
                        Animation.Bones.Add(ThisNode);
                    }

                    // update or add the key frame
                    ThisNode.SetKeyFromBone((float)currentFrame.Value, TransformTool.b);
                }
            }
        }

        private void glViewport_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == 'f')
                FrameSelectionAndSort();

            if (e.KeyChar == 'i')
            {
                ShaderTools.SetupShaders();
                ShaderTools.SaveErrorLogs();
            }
        }

        private void ModelViewport_KeyDown(object sender, KeyEventArgs e)
        {
            // Super secret commands. I'm probably going to be the only one that uses them anyway...
            if (Keyboard.GetState().IsKeyDown(Key.C) && Keyboard.GetState().IsKeyDown(Key.H) && Keyboard.GetState().IsKeyDown(Key.M))
                BatchRenderModels();

            if (Keyboard.GetState().IsKeyDown(Key.X) && Keyboard.GetState().IsKeyDown(Key.M) && Keyboard.GetState().IsKeyDown(Key.L))
                BatchExportMaterialXml();

            if (Keyboard.GetState().IsKeyDown(Key.S) && Keyboard.GetState().IsKeyDown(Key.T) && Keyboard.GetState().IsKeyDown(Key.M))
                BatchRenderStages();

            if (Keyboard.GetState().IsKeyDown(Key.L) && Keyboard.GetState().IsKeyDown(Key.S) && Keyboard.GetState().IsKeyDown(Key.T))
                ParamTools.BatchExportParamValuesAsCsv("light_set");

            if (Keyboard.GetState().IsKeyDown(Key.R) && Keyboard.GetState().IsKeyDown(Key.N) && Keyboard.GetState().IsKeyDown(Key.D))
                ParamTools.BatchExportParamValuesAsCsv("stprm");
        }

        private void RenderStageModels(string stageFolder, string outputPath, string sourcePath)
        {
            string renderPath = stageFolder + "//render";
            if (Directory.Exists(renderPath))
            {
                if (File.Exists(renderPath + "//light_set_param.bin"))
                {
                    try
                    {
                        Runtime.lightSetParam = new LightSetParam(renderPath + "//light_set_param.bin");
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }

                }
            }

            string modelPath = stageFolder + "//model//";
            if (Directory.Exists(modelPath))
            {
                // We can assume one NUD per folder. 
                string[] nudFileNames = Directory.GetFiles(modelPath, "*.nud", SearchOption.AllDirectories);
                foreach (string nudFile in nudFileNames)
                {
                    RenderModel(nudFile, sourcePath, outputPath);
                }
            }
        }

        public void SaveScreenRender(bool saveAlpha = false)
        {
            // Setup the new render dimensions.
            glViewport.MakeCurrent();
            GL.Disable(EnableCap.ScissorTest);
            //ResizeTexturesAndBuffers();
            GL.Viewport(0, 0, fboRenderWidth, fboRenderHeight);

            // Render the viewport.
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, screenRenderFbo);
            Render(null, null);

            // Save the render as a PNG.
            Bitmap screenCapture = FramebufferTools.ReadFrameBufferPixels(screenRenderFbo, FramebufferTarget.DrawFramebuffer, fboRenderWidth, fboRenderHeight, saveAlpha);
            string outputPath = CalculateUniqueName();
            screenCapture.Save(outputPath);
            screenCapture.Dispose(); // Manually dispose the bitmap to avoid memory leaks. 

            // Cleanup
            GL.Enable(EnableCap.ScissorTest);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        }

        private static string CalculateUniqueName()
        {
            // Keep incrementing the number until unique.
            int i = 0;
            string outputPath = MainForm.executableDir + "\\render_" + i + ".png";
            while (File.Exists(outputPath))
            {
                outputPath = MainForm.executableDir + "\\render_" + i + ".png";
                i++;
            }

            return outputPath;
        }

        private void BatchExportMaterialXml()
        {
            // Get the source model folder and then the output folder. 
            using (var folderSelect = new FolderSelectDialog())
            {
                if (folderSelect.ShowDialog() == DialogResult.OK)
                {
                    string[] files = Directory.GetFiles(folderSelect.SelectedPath, "*.nud", SearchOption.AllDirectories);

                    using (var outputFolderSelect = new FolderSelectDialog())
                    {
                        if (outputFolderSelect.ShowDialog() == DialogResult.OK)
                        {
                            for (int i = 0; i < files.Length; i++)
                            {
                                string xmlName = FormatFileName(files[i], folderSelect.SelectedPath);
                                NUD nud = new NUD(files[i]);
                                string outputFileName = outputFolderSelect.SelectedPath + "\\" + xmlName + ".xml";
                                MaterialXML.ExportMaterialAsXml(nud, outputFileName);
                            }
                        }
                    }
                }
            }
        }

        private void DrawNutTexAndUvs()
        {
            GL.PopAttrib();
            NutTexture tex = ((NutTexture)MeshList.filesTreeView.SelectedNode);
            RenderTools.DrawTexturedQuad(((NUT)tex.Parent).draw[tex.HASHID], tex.Width, tex.Height);

            if (Runtime.drawUv)
                DrawUvsForSelectedTexture(tex);

            glViewport.SwapBuffers();
        }

        private void DrawBchTex()
        {
            GL.PopAttrib();
            BCH_Texture tex = ((BCH_Texture)MeshList.filesTreeView.SelectedNode);
            RenderTools.DrawTexturedQuad(tex.display, tex.Width, tex.Height);
            glViewport.SwapBuffers();
        }

        private void DrawAreaLightBoundingBoxes()
        {
            foreach (AreaLight light in LightTools.areaLights)
            {
                Color color = Color.White;

                RenderTools.DrawRectangularPrism(new Vector3(light.positionX, light.positionY, light.positionZ),
                    light.scaleX, light.scaleY, light.scaleZ, true);
            }
        }

        private void DrawUvsForSelectedTexture(NutTexture tex)
        {
            foreach (TreeNode node in MeshList.filesTreeView.Nodes)
            {
                if (!(node is ModelContainer))
                    continue;

                ModelContainer m = (ModelContainer)node;

                int textureHash = 0;
                int.TryParse(tex.Text, NumberStyles.HexNumber, null, out textureHash);
                RenderTools.DrawUv(camera, m.NUD, textureHash, 4, Color.Red, 1, Color.White);
            }
        }
    }
}
