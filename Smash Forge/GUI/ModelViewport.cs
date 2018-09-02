using System;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using Gif.Components;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using SALT.Moveset.AnimCMD;
using SFGraphics.Cameras;
using SFGraphics.GLObjects;
using SFGraphics.GLObjects.Textures;
using SFGraphics.Utils;
using Smash_Forge.Params;
using Smash_Forge.Rendering;
using Smash_Forge.Rendering.Lights;
using Smash_Forge.Rendering.Meshes;
using System.Collections.Generic;
using SFGraphics.GLObjects.GLObjectManagement;

namespace Smash_Forge
{
    public partial class ModelViewport : EditorBase
    {
        // View controls
        public ForgeCamera camera = new ForgeCamera();
        public GUI.Menus.CameraSettings cameraPosForm = null;

        // Rendering Stuff
        private Framebuffer colorHdrFbo;

        // Framerate control
        private Thread renderThread;
        private bool renderThreadIsUpdating = false;
        private bool isOpen = true;

        // The texture that will be blurred for bloom.
        private Framebuffer imageBrightHdrFbo;

        // Used for screen renders and color picking.
        private Framebuffer offscreenRenderFbo;

        private Mesh3D screenVao;

        // Shadow Mapping
        private Framebuffer depthMapFbo;
        private DepthTexture depthMap;
        private int shadowWidth = 2048;
        private int shadowHeight = 2048;
        private Matrix4 lightMatrix = Matrix4.Identity;

        // The viewport dimensions should be used for FBOs visible on screen.
        // Larger dimensions can be used for higher quality outputs for FBOs.
        private int fboRenderWidth;
        private int fboRenderHeight;
  
        // Functions of Viewer
        public enum Mode
        {
            Normal = 0,
            Photoshoot,
            Selection
        }
        public Mode currentMode = Mode.Normal;

        private VertexTool vertexTool = new VertexTool();
        private TransformTool transformTool = new TransformTool();

        //Animation
        private Animation currentAnimation;
        public Animation CurrentAnimation
        {
            get
            {
                return currentAnimation;
            }
            set
            {
                //Moveset
                //If moveset is loaded then initialize with null script so handleACMD loads script for frame speed modifiers and FAF (if parameters are imported)
                if (MovesetManager != null && acmdScript == null)
                    acmdScript = new ForgeACMDScript(null);

                if (value != null)
                {
                    string TargetAnimString = value.Text;
                    if (!string.IsNullOrEmpty(TargetAnimString))
                    {
                        if (acmdScript != null)
                        {
                            //Remove manual crc flag
                            //acmdEditor.manualCrc = false;
                            HandleACMD(TargetAnimString);
                            if (acmdScript != null)
                                acmdScript.processToFrame(0);

                        }
                    }
                }
                ResetModels();
                currentMaterialAnimation = null;
                currentAnimation = value;
                totalFrame.Value = value.FrameCount;
                animationTrackBar.TickFrequency = 1;
                currentFrame.Value = 1;
                currentFrame.Value = 0;
            }
        }

        private MTA currentMaterialAnimation;
        public MTA CurrentMaterialAnimation
        {
            get
            {
                return currentMaterialAnimation;
            }
            set
            {
                ResetModels();
                currentAnimation = null;
                currentMaterialAnimation = value;
                totalFrame.Value = value.frameCount;
                animationTrackBar.TickFrequency = 1;
                animationTrackBar.SetRange(0, (int)value.frameCount);
                currentFrame.Value = 1;
                currentFrame.Value = 0;
            }
        }

        private BFRES.MTA bfresMaterialAnimation;
        public BFRES.MTA CurrentBfresMaterialAnimation
        {
            get
            {
                return bfresMaterialAnimation;
            }
            set
            {
                ResetModels();
                currentAnimation = null;
                bfresMaterialAnimation = value;
                totalFrame.Value = value.FrameCount;
                animationTrackBar.TickFrequency = 1;
                animationTrackBar.SetRange(0, (int)value.FrameCount);
                currentFrame.Value = 1;
                currentFrame.Value = 0;
            }
        }

        // ACMD
        public int scriptId = -1;
        public Dictionary<string, int> paramMoveNameIdMapping;
        public CharacterParamManager paramManager;
        public PARAMEditor paramManagerHelper;

        private MovesetManager movesetManager;
        public MovesetManager MovesetManager
        {
            get
            {
                return movesetManager;
            }
            set
            {
                movesetManager = value;
                if (acmdEditor != null)
                    acmdEditor.updateCrcList();
            }
        }

        public ForgeACMDScript acmdScript = null;

        public ACMDPreviewEditor acmdEditor;
        public HitboxList hitboxList;
        public HurtboxList hurtboxList;
        public VariableList variableViewer;

        // Used in ModelContainer for direct UV time animation.
        public static Stopwatch directUvTimeStopWatch = new Stopwatch();

        //LVD
        private LVD lvd;
        public LVD LVD
        {
            get
            {
                return lvd;
            }
            set
            {
                lvd = value;
                lvd.MeshList = meshList;
                lvdEditor.LVD = lvd;
                lvdList.TargetLVD = lvd;
                lvdList.fillList();
            }
        }

        LVDList lvdList = new LVDList();
        LVDEditor lvdEditor = new LVDEditor();

        public BfresMaterialEditor bfresMatEditor = new BfresMaterialEditor();

        //Binary YAML. Used in many Wii U/Switch games
        public BYAML BYAML
        {
            get
            {
                return byaml;
            }
            set
            {
                byaml = value;
                byamlEditor.TargetBYAML = byaml;
                byamlList.TargetBYAML = byaml;
                byamlList.fillList();
            }
        }
        private BYAML byaml;
        ByamlList byamlList = new ByamlList();
        ByamlEditor byamlEditor = new ByamlEditor();


        //Path
        public PathBin pathBin;

        // Selection Functions
        public float sx1, sy1;

        //Animation Functions
        public int animationSpeed = 60;
        public float frame = 0;
        public bool isPlaying;

        // Contents
        public MeshList meshList = new MeshList();
        public AnimListPanel animListPanel = new AnimListPanel();
        public TreeNodeCollection draw;

        // Photoshoot
        public bool freezeCamera = false;
        public int shootX = 0;
        public int shootY = 0;
        public int shootWidth = 50;
        public int shootHeight = 50;

        public ModelViewport()
        {
            InitializeComponent();
            camera = new ForgeCamera();
            FilePath = "";
            Text = "Model Viewport";

            // Wait for everything to be visible.
            Shown += ModelViewport_Shown;

            SetUpMeshList();
            SetUpAnimListPanel();
            SetUpLvdEditors();
            SetUpVertexTool();
            SetUpAcmdEditor();
            SetUpHitBoxList();
            SetUpHurtBoxList();
            SetUpVariableViewer();

            bfresMatEditor.Dock = DockStyle.Left;
            bfresMatEditor.MaximumSize = new Size(500, 2000);
            AddControl(bfresMatEditor);

            byamlList.Dock = DockStyle.Left;
            byamlList.MaximumSize = new Size(300, 2000);
            AddControl(byamlList);
            byamlList.BYAMLEditor = byamlEditor;

            byamlEditor.Dock = DockStyle.Right;
            byamlEditor.MaximumSize = new Size(300, 2000);
            AddControl(byamlEditor);

            // This selection mode is the last annoying mode for now.
            // It doesn't really do anything.
            modeBone.Checked = true;
            modeMesh.Checked = false;
            modePolygon.Checked = false;

            LVD = new LVD();

            ViewComboBox.SelectedIndex = 0;

            draw = meshList.filesTreeView.Nodes;
        }

        private void SetUpVariableViewer()
        {
            variableViewer = new VariableList();
            variableViewer.Dock = DockStyle.Right;
        }

        private void SetUpHurtBoxList()
        {
            hurtboxList = new HurtboxList();
            hurtboxList.Dock = DockStyle.Right;
        }

        private void SetUpHitBoxList()
        {
            hitboxList = new HitboxList();
            hitboxList.Dock = DockStyle.Right;
            AddControl(hitboxList);
        }

        private void SetUpAcmdEditor()
        {
            acmdEditor = new ACMDPreviewEditor();
            acmdEditor.Owner = this;
            acmdEditor.Dock = DockStyle.Right;
            acmdEditor.updateCrcList();
            AddControl(acmdEditor);
        }

        private void SetUpVertexTool()
        {
            vertexTool.Dock = DockStyle.Left;
            vertexTool.MaximumSize = new Size(300, 2000);
            AddControl(vertexTool);
            vertexTool.vp = this;
        }

        private void SetUpLvdEditors()
        {
            lvdList.Dock = DockStyle.Left;
            lvdList.MaximumSize = new Size(300, 2000);
            AddControl(lvdList);
            lvdList.lvdEditor = lvdEditor;

            lvdEditor.Dock = DockStyle.Right;
            lvdEditor.MaximumSize = new Size(300, 2000);
            AddControl(lvdEditor);
        }

        private void SetUpAnimListPanel()
        {
            animListPanel.Dock = DockStyle.Left;
            animListPanel.MaximumSize = new Size(300, 2000);
            animListPanel.Size = new Size(300, 2000);
            AddControl(animListPanel);
        }

        private void SetUpMeshList()
        {
            meshList.Dock = DockStyle.Right;
            meshList.MaximumSize = new Size(300, 2000);
            meshList.Size = new Size(300, 2000);
            AddControl(meshList);
        }

        public void BfresOpenMats(BFRES.Mesh poly, string name)
        {
            ViewComboBox.SelectedItem = "BFRES Material Editor";

            bfresMatEditor.LoadMaterial(poly);
            bfresMatEditor.Text = name;
            bfresMatEditor.Show();
        }

        private void SetUpBuffersAndTextures()
        {
            // Use the viewport dimensions by default.
            fboRenderWidth = glViewport.Width;
            fboRenderHeight = glViewport.Height;

            // Render bright and normal images to separate textures.
            colorHdrFbo = new Framebuffer(FramebufferTarget.Framebuffer, glViewport.Width, glViewport.Height, PixelInternalFormat.Rgba16f, 2);

            // Smaller FBO/texture for the brighter, blurred portions.
            int brightTexWidth = (int)(glViewport.Width * Runtime.bloomTexScale);
            int brightTexHeight = (int)(glViewport.Height * Runtime.bloomTexScale);
            imageBrightHdrFbo = new Framebuffer(FramebufferTarget.Framebuffer, brightTexWidth, brightTexHeight, PixelInternalFormat.Rgba16f);

            // Screen Rendering
            offscreenRenderFbo = new Framebuffer(FramebufferTarget.Framebuffer, fboRenderWidth, fboRenderHeight, PixelInternalFormat.Rgba);

            // Bind the default framebuffer again.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            SetUpDepthMap();
        }

        private void SetUpDepthMap()
        {
            // Set up the depth map fbo.
            depthMapFbo = new Framebuffer(FramebufferTarget.Framebuffer);
            depthMapFbo.SetDrawBuffers(DrawBuffersEnum.None);
            depthMapFbo.SetReadBuffer(ReadBufferMode.None);

            // Attach the depth map texture.
            depthMap = new DepthTexture(shadowWidth, shadowHeight, PixelInternalFormat.DepthComponent24);
            depthMapFbo.AttachDepthTexture(FramebufferAttachment.DepthAttachment, depthMap);
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
                    lvd.Save(FilePath);
                    break;
                case ".mtable":
                    movesetManager.Save(FilePath);
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
                    if (sfd.FileName.EndsWith(".lvd") && lvd != null)
                    {
                        FilePath = sfd.FileName;
                        Save();
                    }
                    if (sfd.FileName.EndsWith(".mtable") && movesetManager != null)
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

        private void ModelViewport_Shown(object sender, EventArgs e)
        {
            // Frame time control.
            glViewport.VSync = Runtime.enableVSync;
            renderThread = new Thread(new ThreadStart(RenderAndAnimationLoop));
            renderThread.Start();
        }

        private void RenderAndAnimationLoop()
        {
            if (this.IsDisposed)
                return;

            // TODO: We don't really need two timers.
            Stopwatch renderStopwatch = Stopwatch.StartNew();
            Stopwatch animationStopwatch = Stopwatch.StartNew();

            // Wait for UI to load before triggering paint events.
            int waitTimeMs = 200;
            Thread.Sleep(waitTimeMs);

            glViewport.Invalidate();

            int frameUpdateInterval = 5;
            int animationUpdateInterval = 16;

            while (isOpen)
            {             
                // Always refresh the viewport when animations are playing.
                if (renderThreadIsUpdating || isPlaying)
                {
                    if (renderStopwatch.ElapsedMilliseconds > frameUpdateInterval)
                    {
                        glViewport.Invalidate();
                        renderStopwatch.Restart();
                    }

                    if (animationStopwatch.ElapsedMilliseconds > animationUpdateInterval)
                    {
                        UpdateAnimationFrame();
                        animationStopwatch.Restart();
                    }
                }
                else
                {
                    // Avoid wasting the CPU if we don't need to render anything.
                    Thread.Sleep(1);
                }
            }
        }

        private void UpdateAnimationFrame()
        {
            if (isPlaying)
            {
                if (nextButton.InvokeRequired)
                {
                    // Increase playback speed by not waiting for GUI thread.
                    nextButton.BeginInvoke((Action)(() =>
                    {
                        nextButton.PerformClick();
                    }));
                }
                else
                {
                    nextButton.PerformClick();
                }
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

        private void MouseClickItemSelect(System.Windows.Forms.MouseEventArgs e)
        {
            if (OpenTKSharedResources.SetupStatus != OpenTKSharedResources.SharedResourceStatus.Initialized || glViewport == null)
                return;

            //Mesh Selection Test
            if (e.Button == MouseButtons.Left)
            {
                Ray ray = new Ray(camera, glViewport);

                transformTool.b = null;
                foreach (TreeNode node in draw)
                {
                    if (!(node is ModelContainer))
                        continue;
                    ModelContainer modelContainer = (ModelContainer)node;

                    if (modeBone.Checked)
                    {
                        // Bounding spheres work well because bones aren't close together.
                        SortedList<double, Bone> selected = modelContainer.GetBoneSelection(ray);
                        if (selected.Count > 0)
                            transformTool.b = selected.Values.ElementAt(0);
                        //break;
                    }

                    if (modeMesh.Checked)
                    {
                        // Use a color ID render pass for more precision.
                        SelectMeshAtMousePosition();
                    }

                    if (modePolygon.Checked)
                    {
                        // Use a color ID render pass for more precision.
                        SelectPolygonAtMousePosition();
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Display the MeshList context menus in the viewport.
                // This is faster than right clicking in the treeview.
                if (meshList.filesTreeView.SelectedNode is NUD.Polygon && modePolygon.Checked)
                    meshList.PolyContextMenu.Show(glViewport, e.X, e.Y);
                else if (meshList.filesTreeView.SelectedNode is NUD.Mesh && modeMesh.Checked)
                    meshList.MeshContextMenu.Show(glViewport, e.X, e.Y);
            }
        }

        private void SelectPolygonAtMousePosition()
        {
            RenderNudColorIdPassToFbo(offscreenRenderFbo.Id);

            // Get the color at the mouse's position.
            Color selectedColor = ColorPickPixelAtMousePosition();
            meshList.filesTreeView.SelectedNode = GetSelectedPolygonFromColor(selectedColor);
        }

        private void SelectMeshAtMousePosition()
        {
            RenderNudColorIdPassToFbo(offscreenRenderFbo.Id);

            // Get the color at the mouse's position.
            Color selectedColor = ColorPickPixelAtMousePosition();
            meshList.filesTreeView.SelectedNode = GetSelectedMeshFromColor(selectedColor);
        }

        private void RenderNudColorIdPassToFbo(int fbo)
        {
            // Render the ID map to the offscreen FBO.
            glViewport.MakeCurrent();
            GL.Viewport(0, 0, fboRenderWidth, fboRenderHeight);
            Runtime.drawNudColorIdPass = true;
            Render(null, null, fboRenderWidth, fboRenderHeight, fbo);
            Runtime.drawNudColorIdPass = false;
        }

        private NUD.Polygon GetSelectedPolygonFromColor(Color pixelColor)
        {
            // Determine what polgyon is selected.
            foreach (TreeNode node in draw)
            {
                if (!(node is ModelContainer))
                    continue;
                ModelContainer con = (ModelContainer)node;

                foreach (NUD.Mesh mesh in con.NUD.Nodes)
                {
                    foreach (NUD.Polygon p in mesh.Nodes)
                    {
                        // The color is the polygon index (not the render order).
                        // Convert to Vector3 to ignore the alpha.
                        Vector3 polyColor = ColorTools.Vector4FromColor(Color.FromArgb(p.DisplayId)).Xyz;
                        Vector3 pickedColor = ColorTools.Vector4FromColor(pixelColor).Xyz;

                        if (polyColor == pickedColor)
                            return p;
                    }
                }
            }

            return null;
        }

        private NUD.Mesh GetSelectedMeshFromColor(Color pixelColor)
        {
            // Determine what mesh is selected. 
            // We can still use the poly ID pass.
            NUD.Polygon selectedPolygon = GetSelectedPolygonFromColor(pixelColor);
            if (selectedPolygon != null && selectedPolygon.Parent != null)
                return (NUD.Mesh)selectedPolygon.Parent;
            else
                return null;
        }

        private Color ColorPickPixelAtMousePosition()
        {
            // Colorpick a single pixel from the offscreen FBO at the mouse's location.
            System.Drawing.Point mousePosition = glViewport.PointToClient(Cursor.Position);
            return offscreenRenderFbo.SamplePixelColor(mousePosition.X, glViewport.Height - mousePosition.Y);
        }

        private Vector3 getScreenPoint(Vector3 pos)
        {
            Vector4 n = Vector4.Transform(new Vector4(pos, 1), camera.MvpMatrix);
            n.X /= n.W;
            n.Y /= n.W;
            n.Z /= n.W;
            return n.Xyz;
        }

        private void glViewport_Resize(object sender, EventArgs e)
        {
            if (OpenTKSharedResources.SetupStatus == OpenTKSharedResources.SharedResourceStatus.Initialized
                && currentMode != Mode.Selection && glViewport.Height != 0 && glViewport.Width != 0)
            {
                GL.LoadIdentity();
                GL.Viewport(0, 0, glViewport.Width, glViewport.Height);

                camera.renderWidth = glViewport.Width;
                camera.renderHeight = glViewport.Height;
                fboRenderWidth = glViewport.Width;
                fboRenderHeight = glViewport.Height;
                camera.UpdateFromMouse();

                ResizeTexturesAndBuffers();
            }

            UpdateBoneSizeRelativeToViewport();
        }

        private void glViewport_LostFocus(object sender, EventArgs e)
        {
            renderThreadIsUpdating = false;
        }

        private void UpdateBoneSizeRelativeToViewport()
        {
            // TODO: Adjust bones to be a fixed size on screen.
            //float distance = camera.Position.Length / (float)Math.Tan(camera.FovRadians / 2.0f);
            //Runtime.renderBoneNodeSize = Runtime.difIntensity * distance;
        }

        private void ResizeTexturesAndBuffers()
        {
            // FBOs manage their own resizing.
            // FBOs may not be initialized yet.
            if (imageBrightHdrFbo != null)
            {
                imageBrightHdrFbo.Width = (int)(fboRenderWidth * Runtime.bloomTexScale);
                imageBrightHdrFbo.Height = (int)(fboRenderHeight * Runtime.bloomTexScale);
            }

            if (offscreenRenderFbo != null)
            {
                offscreenRenderFbo.Width = fboRenderWidth;
                offscreenRenderFbo.Height = fboRenderHeight;
            }

            if (colorHdrFbo != null)
            {
                colorHdrFbo.Width = glViewport.Width;
                colorHdrFbo.Height = glViewport.Height;
            }
        }

        #region Animation Events

        private void animationTrackBar_ValueChanged(object sender, EventArgs e)
        {
            if (animationTrackBar.Value > (int)totalFrame.Value)
                animationTrackBar.Value = 0;
            if (animationTrackBar.Value < 0)
                animationTrackBar.Value = (int)totalFrame.Value;
            currentFrame.Value = animationTrackBar.Value;

            int frameNum = animationTrackBar.Value;

            if (currentMaterialAnimation != null)
            {
                foreach (TreeNode node in meshList.filesTreeView.Nodes)
                {
                    if (!(node is ModelContainer)) continue;
                    ModelContainer m = (ModelContainer)node;
                    m.NUD.ApplyMta(currentMaterialAnimation, frameNum);
                }
            }

            if (bfresMaterialAnimation != null)
            {
                foreach (TreeNode node in meshList.filesTreeView.Nodes)
                {
                    if (!(node is ModelContainer)) continue;
                    ModelContainer m = (ModelContainer)node;
                    m.Bfres.ApplyMta(bfresMaterialAnimation, frameNum);
                }
            }

            if (currentAnimation == null) return;

            // Process script first in case we have to speed up the animation
            if (acmdScript != null)
                acmdScript.processToFrame(frameNum);

            float animFrameNum = frameNum;
            if (acmdScript != null && Runtime.useFrameDuration)
                animFrameNum = acmdScript.animationFrame;// - 1;

            foreach (TreeNode node in meshList.filesTreeView.Nodes)
            {
                if (!(node is ModelContainer)) continue;
                ModelContainer m = (ModelContainer)node;
                currentAnimation.SetFrame(animFrameNum);
                if (m.VBN != null)
                    currentAnimation.NextFrame(m.VBN);

                // Deliberately do not ever use ACMD/animFrame to modify these other types of model
                if (m.DatMelee != null)
                {
                    currentAnimation.SetFrame(frameNum);
                    currentAnimation.NextFrame(m.DatMelee.bones);
                }
                if (m.Bch != null)
                {
                    foreach (BCH_Model mod in m.Bch.Models.Nodes)
                    {
                        if (mod.skeleton != null)
                        {
                            currentAnimation.SetFrame(animFrameNum);
                            currentAnimation.NextFrame(mod.skeleton);
                        }
                    }
                }
                if (m.Bfres != null)
                {
                    foreach (BFRES.FMDL_Model mod in m.Bfres.models)
                    {
                        if (mod.skeleton != null)
                        {
                            currentAnimation.SetFrame(animFrameNum);
                            currentAnimation.NextFrame(mod.skeleton);
                        }
                    }
                }
            }

            // If the render thread isn't triggering updates, update the viewport manually.
            if (!renderThreadIsUpdating || !isPlaying)
                glViewport.Invalidate();
        }

        public void ResetModels()
        {
            foreach (TreeNode node in meshList.filesTreeView.Nodes)
            {
                if (!(node is ModelContainer)) continue;
                ModelContainer m = (ModelContainer)node;
                m.NUD.ClearMta();
                if (m.VBN != null)
                    m.VBN.reset();

                // Deliberately do not ever use ACMD/animFrame to modify these other types of model
                if (m.DatMelee != null)
                {
                    m.DatMelee.bones.reset();
                }

                if (m.Bch != null)
                {
                    foreach (BCH_Model mod in m.Bch.Models.Nodes)
                    {
                        if (mod.skeleton != null)
                        {
                            mod.skeleton.reset();
                        }
                    }
                }

                if (m.Bfres != null)
                {
                    foreach (var mod in m.Bfres.models)
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
                directUvTimeStopWatch.Start();
            else
                directUvTimeStopWatch.Stop();
        }

        #endregion

        private void ResetCamera_Click(object sender, EventArgs e)
        {
            // Frame the selected NUD or mesh based on the bounding spheres. Frame the NUD if nothing is selected. 
            FrameSelectionAndSort();
        }

        public void FrameSelectionAndSort()
        {
            if (meshList.filesTreeView.SelectedNode is NUD.Mesh)
            {
                FrameSelectedMesh();
            }
            else if (meshList.filesTreeView.SelectedNode is NUD)
            {
                FrameSelectedNud();
            }
            else if (meshList.filesTreeView.SelectedNode is NUD.Polygon)
            {
                FrameSelectedPolygon();
            }
            else if (meshList.filesTreeView.SelectedNode is ModelContainer)
            {
                FrameSelectedModelContainer();
            }
            else if (meshList.filesTreeView.SelectedNode is BFRES)
            {
                FrameSelectedBfres();
            }
            else
            {
                FrameAllModelContainers();
            }

            // Depth sorting. 
            foreach (TreeNode node in meshList.filesTreeView.Nodes)
            {
                if (node is ModelContainer)
                {
                    ModelContainer modelContainer = (ModelContainer)node;
                    modelContainer.DepthSortModels(camera.Position);
                }
            }
        }

        private void FrameSelectedModelContainer()
        {
            ModelContainer modelContainer = (ModelContainer)meshList.filesTreeView.SelectedNode;
            float[] boundingSphere = new float[] { 0, 0, 0, 0 };

            // Use the main bounding box for the NUD.
            if (modelContainer.NUD.boundingSphere[3] > boundingSphere[3])
            {
                boundingSphere[0] = modelContainer.NUD.boundingSphere[0];
                boundingSphere[1] = modelContainer.NUD.boundingSphere[1];
                boundingSphere[2] = modelContainer.NUD.boundingSphere[2];
                boundingSphere[3] = modelContainer.NUD.boundingSphere[3];
            }

            // It's possible that only the individual meshes have bounding boxes.
            foreach (NUD.Mesh mesh in modelContainer.NUD.Nodes)
            {
                if (mesh.boundingSphere[3] > boundingSphere[3])
                {
                    boundingSphere[0] = mesh.boundingSphere[0];
                    boundingSphere[1] = mesh.boundingSphere[1];
                    boundingSphere[2] = mesh.boundingSphere[2];
                    boundingSphere[3] = mesh.boundingSphere[3];
                }
            }

            camera.FrameBoundingSphere(new Vector3(boundingSphere[0], boundingSphere[1], boundingSphere[2]), boundingSphere[3]);
            camera.UpdateFromMouse();
            UpdateBoneSizeRelativeToViewport();
        }

        private void FrameSelectedMesh()
        {
            NUD.Mesh mesh = (NUD.Mesh)meshList.filesTreeView.SelectedNode;
            float[] boundingSphere = mesh.boundingSphere;
            camera.FrameBoundingSphere(new Vector3(boundingSphere[0], boundingSphere[1], boundingSphere[2]), boundingSphere[3]);
            camera.UpdateFromMouse();
            UpdateBoneSizeRelativeToViewport();
        }

        private void FrameSelectedNud()
        {
            NUD nud = (NUD)meshList.filesTreeView.SelectedNode;
            float[] boundingSphere = nud.boundingSphere;
            camera.FrameBoundingSphere(new Vector3(boundingSphere[0], boundingSphere[1], boundingSphere[2]), boundingSphere[3]);
            camera.UpdateFromMouse();
            UpdateBoneSizeRelativeToViewport();
        }

        private void FrameSelectedBfres()
        {
            Console.WriteLine("BFRES selected");
            BFRES bfres = (BFRES)meshList.filesTreeView.SelectedNode;

            List<float> X = new List<float>();
            List<float> Y = new List<float>();
            List<float> Z = new List<float>();
            List<float> Radius = new List<float>();

            foreach (BFRES.FMDL_Model mdl in bfres.models)
            {
                foreach (BFRES.Mesh msh in mdl.poly)
                {
                    X.Add(msh.boundingBoxes[0].Center.X);
                    Y.Add(msh.boundingBoxes[0].Center.Y);
                    Z.Add(msh.boundingBoxes[0].Center.Z);

                    Radius.Add(msh.radius[0]);
                }
            }

            X.Sort();
            Y.Sort();
            Z.Sort();
            Radius.Sort();

            camera.FrameBoundingSphere(new Vector3(X[X.Count - 1], Y[Y.Count - 1], Z[Z.Count - 1]), Radius[Radius.Count - 1]);
            camera.UpdateMatrices();
        }

        private void FrameSelectedPolygon()
        {
            NUD.Mesh mesh = (NUD.Mesh)meshList.filesTreeView.SelectedNode.Parent;
            float[] boundingSphere = mesh.boundingSphere;
            camera.FrameBoundingSphere(new Vector3(boundingSphere[0], boundingSphere[1], boundingSphere[2]), boundingSphere[3]);
            camera.UpdateFromMouse();
            UpdateBoneSizeRelativeToViewport();
        }

        private void FrameAllModelContainers(float maxBoundingRadius = 400)
        {
            bool hasModelContainers = false;

            // Find the max NUD bounding box for all models. 
            float[] boundingSphere = new float[] { 0, 0, 0, 0 };

            foreach (TreeNode node in meshList.filesTreeView.Nodes)
            {
                if (node is ModelContainer)
                {
                    hasModelContainers = true;
                    ModelContainer modelContainer = (ModelContainer)node;

                    // Use the main bounding box for the NUD.
                    if ((modelContainer.NUD.boundingSphere[3] > boundingSphere[3]) && (modelContainer.NUD.boundingSphere[3] < maxBoundingRadius))
                    {
                        boundingSphere[0] = modelContainer.NUD.boundingSphere[0];
                        boundingSphere[1] = modelContainer.NUD.boundingSphere[1];
                        boundingSphere[2] = modelContainer.NUD.boundingSphere[2];
                        boundingSphere[3] = modelContainer.NUD.boundingSphere[3];
                    }

                    // It's possible that only the individual meshes have bounding boxes.
                    foreach (NUD.Mesh mesh in modelContainer.NUD.Nodes)
                    {
                        if (mesh.boundingSphere[3] > boundingSphere[3] && mesh.boundingSphere[3] < maxBoundingRadius)
                        {
                            boundingSphere[0] = mesh.boundingSphere[0];
                            boundingSphere[1] = mesh.boundingSphere[1];
                            boundingSphere[2] = mesh.boundingSphere[2];
                            boundingSphere[3] = mesh.boundingSphere[3];
                        }
                    }

                    if (modelContainer.Bfres != null)
                    {
                        foreach (var mdl in modelContainer.Bfres.models)
                        {
                            foreach (var m in mdl.poly)
                            {
                                m.GenerateBoundingBoxes();

                                foreach (var box in m.boundingBoxes)
                                {
                                    // HACK: This sort of works.
                                    float maxExtent = Math.Max(Math.Max(box.Extent.X, box.Extent.Y), box.Extent.Z);
                                    if (maxExtent > boundingSphere[3])
                                    {
                                        boundingSphere[0] = box.Center.X;
                                        boundingSphere[1] = box.Center.Y;
                                        boundingSphere[2] = box.Center.Z;
                                        boundingSphere[3] = maxExtent;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (hasModelContainers)
                camera.FrameBoundingSphere(new Vector3(boundingSphere[0], boundingSphere[1], boundingSphere[2]), boundingSphere[3], 0);
            else
                camera.ResetToDefaultPosition();

            camera.UpdateMatrices();
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
                this.acmdScript = null;
                return;
            }

            // Try and set up the editor
            try
            {
                if (acmdEditor.crc != crc)
                    acmdEditor.SetAnimation(crc);
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
                    this.acmdScript = null;
                    hitboxList.refresh();
                    variableViewer.refresh();
                    return;
                }
            }

            ACMDScript acmdScript = (ACMDScript)MovesetManager.Game.Scripts[crc];
            // Only update the script if it changed
            if (acmdScript != null)
            {
                // If script wasn't set, or it was set and it changed, load the new script
                if (this.acmdScript == null || (this.acmdScript != null && this.acmdScript.script != acmdScript))
                {
                    this.acmdScript = new ForgeACMDScript(acmdScript);
                }
            }
            else
                this.acmdScript = null;
        }

        #endregion


        private void CameraSettings_Click(object sender, EventArgs e)
        {
            if (cameraPosForm == null)
                cameraPosForm = new GUI.Menus.CameraSettings(camera);
            cameraPosForm.ShowDialog();
        }

        private void glViewport_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (currentMode != Mode.Selection && !freezeCamera)
            {
                renderThreadIsUpdating = true;
                camera.UpdateFromMouse();
                UpdateBoneSizeRelativeToViewport();
            }
        }

        private void glViewport_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            renderThreadIsUpdating = false;
            glViewport.Invalidate();
        }

        #region Controls

        public void HideAll()
        {
            lvdEditor.Visible = false;
            lvdList.Visible = false;
            meshList.Visible = false;
            animListPanel.Visible = false;
            acmdEditor.Visible = false;
            vertexTool.Visible = false;
            totalFrame.Enabled = false;
        }

        private void ViewComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            HideAll();
            // Use a string so the order of the items can be changed later. 
            switch (ViewComboBox.SelectedItem.ToString())
            {
                case "Model Viewer":
                    meshList.Visible = true;
                    animListPanel.Visible = true;
                    break;
                case "Model Editor":
                    meshList.Visible = true;
                    vertexTool.Visible = true;
                    break;
                case "Animation Editor":
                    animListPanel.Visible = true;
                    totalFrame.Enabled = true;
                    break;
                case "LVD Editor":
                    lvdEditor.Visible = true;
                    lvdList.Visible = true;
                    break;
                case "ACMD Editor":
                    animListPanel.Visible = true;
                    acmdEditor.Visible = true;
                    break;
                case "Clean":
                    lvdEditor.Visible = false;
                    lvdList.Visible = false;
                    meshList.Visible = false;
                    animListPanel.Visible = false;
                    acmdEditor.Visible = false;
                    vertexTool.Visible = false;
                    totalFrame.Enabled = false;
                    break;
            }
        }

        public void BatchRenderNudModels()
        {
            // Ignore warnings.
            Runtime.checkNudTexIdOnOpen = false;

            // Get the source model folder and then the output folder. 
            using (var folderSelect = new FolderSelectDialog())
            {
                folderSelect.Title = "Models Directory";
                if (folderSelect.ShowDialog() == DialogResult.OK)
                {
                    using (var outputFolderSelect = new FolderSelectDialog())
                    {
                        outputFolderSelect.Title = "Output Renders Directory";
                        if (outputFolderSelect.ShowDialog() == DialogResult.OK)
                        {
                            foreach (string file in Directory.EnumerateFiles(folderSelect.SelectedPath, "*model.nud", SearchOption.AllDirectories))
                            {
                                try
                                {
                                    MainForm.Instance.OpenNud(file, "", this);
                                }
                                catch (Exception e)
                                {
                                    // Suppress all exceptions and just keep rendering.
                                    Debug.WriteLine(e.Message);
                                    Debug.WriteLine(e.StackTrace);
                                }

                                BatchRenderViewportToFile(file, folderSelect.SelectedPath, outputFolderSelect.SelectedPath);

                                // Cleanup the models and nodes but keep the same viewport.
                                ClearModelContainers();
                                // Make sure the reference counts get updated for all the GLObjects so we can clean up next frame.
                                GC.WaitForPendingFinalizers();
                            }
                        }
                    }
                }
            }

            Runtime.checkNudTexIdOnOpen = true;
        }

        public void BatchRenderBotwBfresModels()
        {
            // Get the source model folder and then the output folder. 
            using (var folderSelect = new FolderSelectDialog())
            {
                folderSelect.Title = "Models Directory";
                if (folderSelect.ShowDialog() == DialogResult.OK)
                {
                    using (var outputFolderSelect = new FolderSelectDialog())
                    {
                        outputFolderSelect.Title = "Output Renders Directory";
                        if (outputFolderSelect.ShowDialog() == DialogResult.OK)
                        {
                            foreach (string file in Directory.EnumerateFiles(folderSelect.SelectedPath, "*.sbfres", SearchOption.AllDirectories))
                            {
                                if (file.ToLower().Contains("tex") || file.ToLower().Contains("animation"))
                                    continue;

                                try
                                {
                                    MainForm.Instance.OpenBfres(MainForm.GetUncompressedSzsSbfresData(file), file, "", this);

                                    string nameNoExtension = Path.GetFileNameWithoutExtension(file);
                                    string textureFileName = Path.GetDirectoryName(file) + "\\" + String.Format("{0}.Tex1.sbfres", nameNoExtension);

                                    if (File.Exists(textureFileName))
                                        MainForm.Instance.OpenBfres(MainForm.GetUncompressedSzsSbfresData(textureFileName), textureFileName, "", this);
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine(e.Message);
                                    Debug.WriteLine(e.StackTrace);
                                }
                                BatchRenderViewportToFile(file, folderSelect.SelectedPath, outputFolderSelect.SelectedPath);

                                // Cleanup the models and nodes but keep the same viewport.
                                ClearModelContainers();
                                // Make sure the reference counts get updated for all the GLObjects so we can clean up next frame.
                                GC.WaitForPendingFinalizers();
                            }
                        }
                    }
                }
            }
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
                                BatchRenderViewportToFile(stageFolder, sourceFolderSelect.SelectedPath, outputFolderSelect.SelectedPath);
                                MainForm.Instance.ClearWorkSpace(false);
                                ClearModelContainers();
                            }
                        }
                    }
                }
            }
        }

        private void BatchRenderViewportToFile(string nudFileName, string sourcePath, string outputPath)
        {
            SetUpAndRenderViewport();

            using (Bitmap screenCapture = FramebufferTools.ReadFrameBufferPixels(0, FramebufferTarget.Framebuffer, fboRenderWidth, fboRenderHeight, true))
            {
                string renderName = ConvertDirSeparatorsToUnderscore(nudFileName, sourcePath);
                screenCapture.Save(outputPath + "\\" + renderName + ".png");
            }
        }

        private void SetUpAndRenderViewport()
        {
            // Setup before rendering the model. Use a large max radius to show skybox models.
            FrameAllModelContainers();
            // We need to manually call the paint event twice so the textures are refreshed and the screen updates properly.
            glViewport_Paint(null, null);
            glViewport_Paint(null, null);
        }

        public static string ConvertDirSeparatorsToUnderscore(string fullPath, string sourceDirPath)
        {
            // Save the render using the folder structure as the name.
            string renderName = fullPath.Replace(sourceDirPath, "");
            renderName = renderName.Substring(1);
            renderName = renderName.Replace("\\", "_");
            renderName = renderName.Replace("//", "_");
            renderName = renderName.Replace(".nud", "");
            return renderName;
        }

        private void GIFButton_Click(object sender, EventArgs e)
        {
            if (currentAnimation == null)
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
                Render(null, null, glViewport.Width, glViewport.Height);

                if (i != settings.StartFrame) //On i=StartFrame it captures the frame the user had before setting frame to it so ignore that one, the +1 on the for makes it so the last frame is captured
                {
                    using (Bitmap cs = FramebufferTools.ReadFrameBufferPixels(0, FramebufferTarget.Framebuffer, fboRenderWidth, fboRenderWidth))
                    {
                        images.Add(new Bitmap(cs, new Size((int)(cs.Width / ScaleFactor), (int)(cs.Height / settings.ScaleFactor)))); //Resize images
                    }
                }
            }


            if (images.Count > 0 && !settings.StoreFrames)
            {
                SaveFileDialog sf = new SaveFileDialog();

                sf.FileName = "Render.gif";
                sf.Filter = "GIF file (*.gif)|*.gif";

                if (sf.ShowDialog() == DialogResult.OK)
                {
                    GIFProgress g = new GIFProgress(images, sf.FileName, animationSpeed, settings.Repeat, settings.Quality);
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
            isOpen = false;
            ClearModelContainers();
            glViewport.Dispose();
        }

        public void ClearModelContainers()
        {
            foreach (TreeNode node in meshList.filesTreeView.Nodes)
            {
                if (node is ModelContainer)
                {
                    ModelContainer m = (ModelContainer)node;
                    Runtime.TextureContainers.Remove(m.NUT);

                    if (m.BNTX != null)
                    {
                        m.BNTX.textures.Clear();
                        m.BNTX.glTexByName.Clear();
                        Runtime.BNTXList.Remove(m.BNTX);
                    }
                    if (m.Bfres != null && m.Bfres.FTEXContainer != null)
                    {
                        m.Bfres.FTEXContainer.FTEXtextures.Clear();
                        m.Bfres.FTEXContainer.glTexByName.Clear();
                        Runtime.FTEXContainerList.Remove(m.Bfres.FTEXContainer);
                    }
                }
            }
            draw.Clear();
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
            // Loop the animation.
            if (currentFrame.Value == totalFrame.Value)
                currentFrame.Value = 0;
            else
                currentFrame.Value++;
        }

        private void prevButton_Click(object sender, EventArgs e)
        {
            if (currentFrame.Value != 0)
                currentFrame.Value--;
        }

        private void viewStripButtonsBone(object sender, EventArgs e)
        {
            stripPos.Checked = false;
            stripRot.Checked = false;
            stripSca.Checked = false;
            ((ToolStripButton)sender).Checked = true;
            if (stripPos.Checked)
                transformTool.Type = TransformTool.ToolTypes.POSITION;
            if (stripRot.Checked)
                transformTool.Type = TransformTool.ToolTypes.ROTATION;
            if (stripSca.Checked)
                transformTool.Type = TransformTool.ToolTypes.SCALE;
        }

        #endregion

        private void toolStripSaveRenderAlphaButton_Click(object sender, EventArgs e)
        {
            SaveScreenRender(true);
        }

        private void toolStripRenderNoAlphaButton_Click(object sender, EventArgs e)
        {
            SaveScreenRender(false);
        }

        private void totalFrame_ValueChanged(object sender, EventArgs e)
        {
            if (currentAnimation == null) return;
            if (totalFrame.Value < 1)
            {
                totalFrame.Value = 1;
            }
            else
            {
                if (currentAnimation.Tag is Animation)
                    ((Animation)currentAnimation.Tag).FrameCount = (int)totalFrame.Value;
                currentAnimation.FrameCount = (int)totalFrame.Value;
                animationTrackBar.Value = 0;
                animationTrackBar.SetRange(0, currentAnimation.FrameCount);
            }
        }

        private void checkSelect()
        {
            if (currentMode == Mode.Selection)
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
                    Ray r = RenderTools.CreateRay(camera.MvpMatrix, GetMouseOnViewport());
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

                vertexTool.vertexListBox.BeginUpdate();
                vertexTool.vertexListBox.Items.Clear();
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
                                    vertexTool.vertexListBox.Items.Add(v);
                                }
                            }
                        }
                    }
                }
                vertexTool.vertexListBox.EndUpdate();
                currentMode = Mode.Normal;
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

        private void Render(object sender, PaintEventArgs e, int width, int height, int defaultFbo = 0)
        {
            // Don't render if the context and resources aren't set up properly.
            // Watching textures suddenly appear looks weird.
            if (OpenTKSharedResources.SetupStatus != OpenTKSharedResources.SharedResourceStatus.Initialized || Runtime.glTexturesNeedRefreshing)
                return;

            SetupViewport(width, height);

            // Bind the default framebuffer in case it was set elsewhere.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, defaultFbo);

            // Push all attributes so we don't have to clean up later
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            // Return early to avoid rendering other stuff. 
            if (meshList.filesTreeView.SelectedNode != null)
            {
                if (meshList.filesTreeView.SelectedNode is BCH_Texture)
                {
                    DrawBchTex();
                    glViewport.SwapBuffers();
                    return;
                }
                if (meshList.filesTreeView.SelectedNode is NutTexture)
                {
                    DrawNutTex();
                    glViewport.SwapBuffers();
                    return;
                }
                if (meshList.filesTreeView.SelectedNode is BRTI)
                {
                    DrawBNTXTexAndUvs();
                    return;
                }
                if (meshList.filesTreeView.SelectedNode is FTEX)
                {
                    DrawFTEXTexAndUvs();
                    return;
                }
            }

            if (Runtime.usePostProcessing)
            {
                // Render models and background into an HDR buffer. 
                colorHdrFbo.Bind();
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }

            // The screen quad shader draws its own background gradient.
            // This prevents the second color attachment from having a white background.
            if (Runtime.renderBackGround && !Runtime.usePostProcessing)
                DrawViewportBackground();

            // What even is this...
            if (glViewport.ClientRectangle.Contains(glViewport.PointToClient(Cursor.Position))
             && glViewport.Focused
             && (currentMode == Mode.Normal || (currentMode == Mode.Photoshoot && !freezeCamera))
             && !transformTool.hit)
            {
                camera.UpdateFromMouse();
                UpdateBoneSizeRelativeToViewport();
            }

            if (cameraPosForm != null)
                cameraPosForm.ApplyCameraAnimation(camera, animationTrackBar.Value);

            if (Runtime.renderFloor)
                RenderTools.DrawFloor(camera.MvpMatrix);

            // Depth testing isn't set by materials.
            SetDepthTesting();

            // Ignore the background for the ID pass.
            if (Runtime.drawNudColorIdPass)
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            if (Runtime.drawModelShadow)
                DrawModelsIntoShadowMap();

            if (Runtime.usePostProcessing)
                DrawModelsNormally(width, height, colorHdrFbo.Id);
            else
                DrawModelsNormally(width, height, defaultFbo);

            if (Runtime.usePostProcessing)
            {
                // Draw the texture to the screen into a smaller FBO.
                imageBrightHdrFbo.Bind();
                GL.Viewport(0, 0, imageBrightHdrFbo.Width, imageBrightHdrFbo.Height);
                ScreenDrawing.DrawTexturedQuad(colorHdrFbo.ColorAttachments[1].Id, imageBrightHdrFbo.Width, imageBrightHdrFbo.Height, screenVao);

                // Setup the normal viewport dimensions again.
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, defaultFbo);
                GL.Viewport(0, 0, width, height);

                ScreenDrawing.DrawScreenQuadPostProcessing(colorHdrFbo.ColorAttachments[0].Id, imageBrightHdrFbo.ColorAttachments[0].Id, screenVao);
            }

            FixedFunctionRendering();

            GL.PopAttrib();
            glViewport.SwapBuffers();
        }

        private void DrawModelsNormally(int width, int height, int defaultFbo)
        {
            // Draw the models normally.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, defaultFbo);
            GL.Viewport(0, 0, width, height);
            DrawModels();
        }

        private void DrawModelsIntoShadowMap()
        {
            // Draw the models into the shadow map.
            depthMapFbo.Bind();
            GL.Viewport(0, 0, shadowWidth, shadowHeight);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // Use the direction of the main character diffuse light.
            float directionScale = 550;
            Matrix4 modelView = Matrix4.LookAt(Vector3.Normalize(Runtime.lightSetParam.characterDiffuse.direction) * directionScale, new Vector3(0), new Vector3(0, 1, 0));
            if (Runtime.cameraLight)
                modelView = Matrix4.LookAt(new Vector3(0, 0, 1).Normalized() * directionScale, new Vector3(0), new Vector3(0, 1, 0));
            lightMatrix = modelView * Matrix4.CreateOrthographicOffCenter(-75, 75, -75, 75, 1, 1000) * Matrix4.CreateScale(5);

            DrawModels(true);
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
            RenderTools.SetUp3DFixedFunctionRendering(camera.MvpMatrix);

            // Bounding boxes should not render on top.
            if (Runtime.drawAreaLightBoundingBoxes)
                DrawAreaLightBoundingBoxes();

            DrawOverlays();
        }

        private void DrawViewportBackground()
        {
            Vector3 topColor = ColorTools.Vector4FromColor(Runtime.backgroundGradientTop).Xyz;
            Vector3 bottomColor = ColorTools.Vector4FromColor(Runtime.backgroundGradientBottom).Xyz;

            // Only use the top color for solid color rendering.
            if (Runtime.backgroundStyle == Runtime.BackgroundStyle.Solid)
                ScreenDrawing.DrawQuadGradient(topColor, topColor, screenVao);
            else
                ScreenDrawing.DrawQuadGradient(topColor, bottomColor, screenVao);
        }

        private void SetupViewport(int width, int height)
        {
            glViewport.MakeCurrent();
            GL.LoadIdentity();
            GL.Viewport(0, 0, width, height);
        }

        private void DrawModels(bool drawShadow = false)
        {
            if (Runtime.renderModel || Runtime.renderModelWireframe)
                foreach (TreeNode m in draw)
                    if (m is ModelContainer)
                        ((ModelContainer)m).Render(camera, depthMap.Id, lightMatrix, new Vector2(glViewport.Width, glViewport.Height), drawShadow);

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
                lvd.Render();

            if (Runtime.renderBones)
                foreach (ModelContainer m in draw)
                    m.RenderBones();

            // ACMD
            if (paramManager != null && Runtime.renderHurtboxes && draw.Count > 0 && (draw[0] is ModelContainer))
            {
                // Doesn't do anything. ParamManager is always null.
                paramManager.RenderHurtboxes(frame, scriptId, acmdScript, ((ModelContainer)draw[0]).GetVBN());
            }

            if (acmdScript != null && draw.Count > 0 && (draw[0] is ModelContainer))
                acmdScript.Render(((ModelContainer)draw[0]).GetVBN());

            if (ViewComboBox.SelectedIndex == 2)
            {
                DrawBoneTransformTool();
            }

            if (ViewComboBox.SelectedIndex == 1)
            {
                MouseSelectionStuff();
            }

            if (currentMode == Mode.Photoshoot)
            {
                freezeCamera = false;
                if (Keyboard.GetState().IsKeyDown(Key.W) && Mouse.GetState().IsButtonDown(MouseButton.Left))
                {
                    shootX = glViewport.PointToClient(Cursor.Position).X;
                    shootY = glViewport.PointToClient(Cursor.Position).Y;
                    freezeCamera = true;
                }
                RenderTools.DrawPhotoshoot(glViewport, shootX, shootY, shootWidth, shootHeight);
            }
        }

        private void MouseSelectionStuff()
        {
            try
            {
                if (currentMode == Mode.Normal && OpenTK.Input.Mouse.GetState().IsButtonDown(OpenTK.Input.MouseButton.Right))
                {
                    currentMode = Mode.Selection;
                    Vector2 m = GetMouseOnViewport();
                    sx1 = m.X;
                    sy1 = m.Y;
                }
            }
            catch
            {

            }
            if (currentMode == Mode.Selection)
            {
                if (!OpenTK.Input.Mouse.GetState().IsButtonDown(OpenTK.Input.MouseButton.Right))
                {
                    checkSelect();
                    currentMode = Mode.Normal;
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
                transformTool.Render(camera, new Ray(camera, glViewport));
                if (transformTool.state == 1)
                    currentMode = Mode.Selection;
                else
                    currentMode = Mode.Normal;
            }

            if (transformTool.HasChanged())
            {
                if (currentAnimation != null && transformTool.b != null)
                {
                    // get the node group for the current bone in animation
                    Animation.KeyNode ThisNode = null;
                    foreach (Animation.KeyNode node in currentAnimation.Bones)
                    {
                        if (node.Text.Equals(transformTool.b.Text))
                        {
                            // found
                            ThisNode = node;
                            break;
                        }
                    }
                    if (ThisNode == null)
                    {
                        ThisNode = new Animation.KeyNode(transformTool.b.Text);
                        currentAnimation.Bones.Add(ThisNode);
                    }

                    // update or add the key frame
                    ThisNode.SetKeyFromBone((float)currentFrame.Value, transformTool.b);
                }
            }
        }

        private void glViewport_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == 'f')
                FrameSelectionAndSort();

            if (e.KeyChar == 'i')
            {
                ShaderTools.SetupShaders(true);
                ShaderTools.SaveErrorLogs();
            }
        }

        private void ModelViewport_KeyDown(object sender, KeyEventArgs e)
        {
            // Super secret commands. I'm probably going to be the only one that uses them anyway...
            if (Keyboard.GetState().IsKeyDown(Key.X) && Keyboard.GetState().IsKeyDown(Key.M) && Keyboard.GetState().IsKeyDown(Key.L))
                MaterialXmlBatchExport.ExportAllMaterialsFromFolder();

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
                    BatchRenderViewportToFile(nudFile, sourcePath, outputPath);
                }
            }
        }

        public void SaveScreenRender(bool saveAlpha = false)
        {
            // Set these dimensions back again before normal rendering so the viewport doesn't look glitchy.
            int oldWidth = glViewport.Width;
            int oldHeight = glViewport.Height;

            // The scissor test is causing issues with viewport resizing. Just disable it for now.
            glViewport.MakeCurrent();
            GL.Disable(EnableCap.ScissorTest);

            // Render screenshots in a higher quality.
            fboRenderWidth = oldWidth * 2;
            fboRenderHeight = oldHeight * 2;

            // Make sure the framebuffers and viewport match the new drawing size.
            ResizeTexturesAndBuffers();
            GL.Viewport(0, 0, fboRenderWidth, fboRenderHeight);

            // Render the viewport.       
            offscreenRenderFbo.Bind();
            Render(null, null, fboRenderWidth, fboRenderHeight, offscreenRenderFbo.Id);

            // Save the render as a PNG.
            using (Bitmap screenCapture = offscreenRenderFbo.ReadImagePixels(saveAlpha))
            {
                string outputPath = CalculateUniqueName();
                screenCapture.Save(outputPath);
            }

            // Cleanup
            fboRenderWidth = oldWidth;
            fboRenderHeight = oldHeight;
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
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

        private void DrawBNTXTexAndUvs()
        {
            GL.PopAttrib();
            BRTI tex = ((BRTI)meshList.filesTreeView.SelectedNode);
            switch (tex.format >> 8)
            {
                case (uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC4:
                    ScreenDrawing.DrawTexturedQuad(tex.display, tex.Width, tex.Height, screenVao, true, false, false);
                    break;
                default:
                    ScreenDrawing.DrawTexturedQuad(tex.display, tex.Width, tex.Height, screenVao);
                    break;
            }

            if (Runtime.drawUv)
                DrawNSWBFRESUvsForSelectedTexture(tex);

            glViewport.SwapBuffers();
        }

        private void DrawNutTex()
        {
            GL.PopAttrib();
            NutTexture tex = ((NutTexture)meshList.filesTreeView.SelectedNode);
            ScreenDrawing.DrawTexturedQuad(((NUT)tex.Parent).glTexByHashId[tex.HashId].Id, tex.Width, tex.Height, screenVao);
        }

        private void DrawBchTex()
        {
            GL.PopAttrib();
            BCH_Texture tex = ((BCH_Texture)meshList.filesTreeView.SelectedNode);
            ScreenDrawing.DrawTexturedQuad(tex.display, tex.Width, tex.Height, screenVao);
        }

        private void DrawFTEXTexAndUvs()
        {
            GL.PopAttrib();

            FTEX tex = ((FTEX)meshList.filesTreeView.SelectedNode);
            switch (tex.format)
            {
                case (int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC4_UNORM:
                    ScreenDrawing.DrawTexturedQuad(tex.display, tex.width, tex.height, screenVao, true, false, false);
                    break;
                case (int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC4_SNORM:
                    ScreenDrawing.DrawTexturedQuad(tex.display, tex.width, tex.height, screenVao, true, false, false);
                    break;
                default:
                    ScreenDrawing.DrawTexturedQuad(tex.display, tex.width, tex.height, screenVao);
                    break;
            }

            if (Runtime.drawUv)
                DrawBFRESUvsForSelectedTexture(tex);

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

        private void DrawNSWBFRESUvsForSelectedTexture(BRTI tex)
        {
            foreach (TreeNode node in meshList.filesTreeView.Nodes)
            {
                if (!(node is ModelContainer))
                    continue;

                ModelContainer m = (ModelContainer)node;

                int textureHash = 0;
                int.TryParse(tex.Text, NumberStyles.HexNumber, null, out textureHash);
                //   RenderTools.BFRES_DrawUv(camera, m.BFRES, tex.Text, tex.display, 4, Color.Red, 1, Color.White);
            }
        }

        private void DrawBFRESUvsForSelectedTexture(FTEX tex)
        {
            foreach (TreeNode node in meshList.filesTreeView.Nodes)
            {
                if (!(node is ModelContainer))
                    continue;

                ModelContainer m = (ModelContainer)node;

                int textureHash = 0;
                int.TryParse(tex.Text, NumberStyles.HexNumber, null, out textureHash);
                //       RenderTools.BFRES_DrawUv(camera, m.BFRES, tex.Text, tex.display, 4, Color.Red, 1, Color.White);
            }
        }

        private void glViewport_Paint(object sender, PaintEventArgs e)
        {
            if (OpenTKSharedResources.SetupStatus != OpenTKSharedResources.SharedResourceStatus.Initialized)
                return;

            Render(sender, e, glViewport.Width, glViewport.Height);

            // Make sure unused resources get cleaned up.
            GLObjectManager.DeleteUnusedGLObjects();

            // Deleting the context will require all the textures to be reloaded.
            if (Runtime.glTexturesNeedRefreshing)
            {
                RefreshGlTextures();
                Runtime.glTexturesNeedRefreshing = false;
            }
        }

        private void glViewport_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseClickItemSelect(e);
        }

        private void modePolygon_Click(object sender, EventArgs e)
        {
            // These should act like radio buttons.
            modeBone.Checked = false;
            modePolygon.Checked = true;
            modeMesh.Checked = false;
        }

        private void modeMesh_Click(object sender, EventArgs e)
        {
            // These should act like radio buttons.
            modeBone.Checked = false;
            modePolygon.Checked = false;
            modeMesh.Checked = true;
        }

        private void modeBone_Click(object sender, EventArgs e)
        {
            // These should act like radio buttons.
            modeBone.Checked = true;
            modePolygon.Checked = false;
            modeMesh.Checked = false;
        }

        private void glViewport_Load(object sender, EventArgs e)
        {
            glViewport.MakeCurrent();

            OpenTKSharedResources.InitializeSharedResources();

            if (OpenTKSharedResources.SetupStatus == OpenTKSharedResources.SharedResourceStatus.Initialized)
            {
                screenVao = ScreenDrawing.CreateScreenTriangle();

                SetUpBuffersAndTextures();

                if (Runtime.enableOpenTKDebugOutput)
                {
                    glViewport.MakeCurrent();
                    OpenTKSharedResources.EnableOpenTKDebugOutput();
                }
            }
        }

        private void viewportPanel_Resize(object sender, EventArgs e)
        {
            int spacing = 8;
            glViewport.Width = viewportPanel.Width - spacing;
            glViewport.Height = viewportPanel.Height - spacing;
        }

        private void glViewport_Enter(object sender, EventArgs e)
        {
            // Only render when the control is focused, so the gui remains responsive.
            renderThreadIsUpdating = true;
        }

        private void glViewport_Leave(object sender, EventArgs e)
        {
            renderThreadIsUpdating = false;
        }

        private void RefreshGlTextures()
        {
            // Regenerate all the texture objects.
            foreach (TreeNode node in meshList.filesTreeView.Nodes)
            {
                if (!(node is ModelContainer))
                    continue;

                ModelContainer m = (ModelContainer)node;

                if (m.NUT != null)
                    m.NUT.RefreshGlTexturesByHashId();
                if (m.BNTX != null)
                {
                    m.BNTX.RefreshGlTexturesByName();
                }
                if (m.Bfres != null && m.Bfres.FTEXContainer != null)
                {
                    m.Bfres.FTEXContainer.RefreshGlTexturesByName();
                }
            }
        }
    }
}