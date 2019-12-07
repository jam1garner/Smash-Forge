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
using SFGraphics.GLObjects.Framebuffers;
using SFGraphics.GLObjects.Textures;
using SFGraphics.Utils;
using System.Collections.Generic;
using SFGraphics.GLObjects.GLObjectManagement;
using SmashForge.Filetypes.Melee;
using SmashForge.Params;
using SmashForge.Rendering;
using SmashForge.Rendering.Lights;
using SmashForge.Rendering.Meshes;

namespace SmashForge
{
    public partial class ModelViewport : EditorBase
    {
        // View controls
        public ForgeCamera camera = new ForgePerspCamera();
        public Gui.Menus.CameraSettings cameraPosForm = null;

        // Rendering Stuff
        private Framebuffer colorHdrFbo;

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
                    acmdScript = new ForgeAcmdScript(null);

                if (value != null)
                {
                    string targetAnimString = value.Text;
                    if (!string.IsNullOrEmpty(targetAnimString))
                    {
                        if (acmdScript != null)
                        {
                            //Remove manual crc flag
                            //acmdEditor.manualCrc = false;
                            HandleAcmd(targetAnimString);
                            if (acmdScript != null)
                                acmdScript.ProcessToFrame(0);

                        }
                        if (atkdEditor != null && scriptId >= 0)
                            atkdEditor.ViewportEvent_SetSelectedSubaction();
                    }
                }
                ResetModels();
                currentMaterialAnimation = null;
                currentAnimation = value;
                totalFrame.Value = value.frameCount;
                animationTrackBar.TickFrequency = 1;
                currentFrameUpDown.Value = 1;
                currentFrameUpDown.Value = 0;
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
                currentFrameUpDown.Value = 1;
                currentFrameUpDown.Value = 0;
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
                currentFrameUpDown.Value = 1;
                currentFrameUpDown.Value = 0;
            }
        }

        //Moveset
        public int scriptId = -1;
        public Dictionary<string, int> paramMoveNameIdMapping;
        public CharacterParamManager paramManager;
        public ParamEditor paramManagerHelper;

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
                    acmdEditor.UpdateCrcList();
            }
        }

        public ForgeAcmdScript acmdScript = null;

        public AcmdPreviewEditor acmdEditor;
        public HitboxList hitboxList;
        public HurtboxList hurtboxList;
        public VariableList variableViewer;

        public AtkdEditor atkdEditor;
        public bool atkdRectClicked = false;

        // Used in ModelContainer for direct UV time animation.
        public static Stopwatch directUvTimeStopWatch = new Stopwatch();

        //LVD
        private LVD lvd;
        public LVD Lvd
        {
            get
            {
                return lvd;
            }
            set
            {
                lvd = value;
                lvd.MeshList = meshList;
                lvdEditor.lvd = lvd;
                lvdList.targetLvd = lvd;
                lvdList.FillList();
            }
        }

        LvdList lvdList = new LvdList();
        LvdEditor lvdEditor = new LvdEditor();

        public BfresMaterialEditor bfresMatEditor = new BfresMaterialEditor();

        //Binary YAML. Used in many Wii U/Switch games
        public BYAML Byaml
        {
            get
            {
                return byaml;
            }
            set
            {
                byaml = value;
                byamlEditor.targetByaml = byaml;
                byamlList.targetByaml = byaml;
                byamlList.FillList();
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
            byamlList.byamlEditor = byamlEditor;

            byamlEditor.Dock = DockStyle.Right;
            byamlEditor.MaximumSize = new Size(300, 2000);
            AddControl(byamlEditor);

            // This selection mode is the least annoying mode for now.
            // It doesn't really do anything.
            modeBone.Checked = true;
            modeMesh.Checked = false;
            modePolygon.Checked = false;

            Lvd = new LVD();

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
            acmdEditor = new AcmdPreviewEditor();
            acmdEditor.owner = this;
            acmdEditor.Dock = DockStyle.Right;
            acmdEditor.UpdateCrcList();
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
            depthMapFbo.AddAttachment(FramebufferAttachment.DepthAttachment, depthMap);
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
            glViewport.RenderFrameInterval = 16;
            glViewport.OnRenderFrame += GlViewportOnOnRenderFrame;
            glViewport.ResumeRendering();
        }

        private void GlViewportOnOnRenderFrame(object sender, EventArgs e)
        {
            if (isPlaying)
                UpdateAnimationFrame();
            glViewport_Paint(null, null);
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
            float mouseX = glViewport.PointToClient(Cursor.Position).X;
            float mouseY = glViewport.PointToClient(Cursor.Position).Y;

            float mx = (2.0f * mouseX) / glViewport.Width - 1.0f;
            float my = 1.0f - (2.0f * mouseY) / glViewport.Height;
            return new Vector2(mx, my);
        }

        private void MouseClickItemSelect(System.Windows.Forms.MouseEventArgs e)
        {
            if (OpenTkSharedResources.SetupStatus != OpenTkSharedResources.SharedResourceStatus.Initialized || glViewport == null)
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
                if (meshList.filesTreeView.SelectedNode is Nud.Polygon && modePolygon.Checked)
                    meshList.PolyContextMenu.Show(glViewport, e.X, e.Y);
                else if (meshList.filesTreeView.SelectedNode is Nud.Mesh && modeMesh.Checked)
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

        private Nud.Polygon GetSelectedPolygonFromColor(Color pixelColor)
        {
            // Determine what polgyon is selected.
            foreach (TreeNode node in draw)
            {
                if (!(node is ModelContainer))
                    continue;
                ModelContainer con = (ModelContainer)node;

                foreach (Nud.Mesh mesh in con.NUD.Nodes)
                {
                    foreach (Nud.Polygon p in mesh.Nodes)
                    {
                        // The color is the polygon index (not the render order).
                        // Convert to Vector3 to ignore the alpha.
                        Vector3 polyColor = ColorUtils.GetVector3(Color.FromArgb(p.DisplayId));
                        Vector3 pickedColor = ColorUtils.GetVector3(pixelColor);

                        if (polyColor == pickedColor)
                            return p;
                    }
                }
            }

            return null;
        }

        private Nud.Mesh GetSelectedMeshFromColor(Color pixelColor)
        {
            // Determine what mesh is selected. 
            // We can still use the poly ID pass.
            Nud.Polygon selectedPolygon = GetSelectedPolygonFromColor(pixelColor);
            if (selectedPolygon != null && selectedPolygon.Parent != null)
                return (Nud.Mesh)selectedPolygon.Parent;
            else
                return null;
        }

        private Color ColorPickPixelAtMousePosition()
        {
            // Colorpick a single pixel from the offscreen FBO at the mouse's location.
            System.Drawing.Point mousePosition = glViewport.PointToClient(Cursor.Position);
            return offscreenRenderFbo.SamplePixelColor(mousePosition.X, glViewport.Height - mousePosition.Y);
        }

        private Vector3 GetScreenPoint(Vector3 pos)
        {
            Vector4 n = Vector4.Transform(new Vector4(pos, 1), camera.MvpMatrix);
            n.X /= n.W;
            n.Y /= n.W;
            n.Z /= n.W;
            return n.Xyz;
        }

        private void glViewport_Resize(object sender, EventArgs e)
        {
            if (OpenTkSharedResources.SetupStatus == OpenTkSharedResources.SharedResourceStatus.Initialized
                && currentMode != Mode.Selection && glViewport.Height != 0 && glViewport.Width != 0)
            {
                GL.LoadIdentity();
                GL.Viewport(0, 0, glViewport.Width, glViewport.Height);

                camera.RenderWidth = glViewport.Width;
                camera.RenderHeight = glViewport.Height;
                fboRenderWidth = glViewport.Width;
                fboRenderHeight = glViewport.Height;
                camera.UpdateFromMouse();

                ResizeTexturesAndBuffers();
            }
        }

        private void ResizeTexturesAndBuffers()
        {
            SetUpBuffersAndTextures();
        }

        #region Animation Events

        private void animationTrackBar_ValueChanged(object sender, EventArgs e)
        {
            if (animationTrackBar.Value > (int)totalFrame.Value)
                animationTrackBar.Value = 0;
            if (animationTrackBar.Value < 0)
                animationTrackBar.Value = (int)totalFrame.Value;
            currentFrameUpDown.Value = animationTrackBar.Value;

            int currentFrame = animationTrackBar.Value;

            SetAnimationsToFrame(currentFrame);
        }

        private void SetAnimationsToFrame(int frameNum)
        {
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

            if (currentAnimation == null)
                return;

            // Process script first in case we have to speed up the animation
            if (acmdScript != null)
                acmdScript.ProcessToFrame(frameNum);

            float animFrameNum = frameNum;
            if (acmdScript != null && Runtime.useFrameDuration)
                animFrameNum = acmdScript.AnimationFrame;// - 1;

            foreach (TreeNode node in meshList.filesTreeView.Nodes)
            {
                if (node is MeleeDataNode)
                {
                    foreach (MeleeRootNode n in ((MeleeDataNode)node).GetAllRoots())
                    {
                        currentAnimation.SetFrame(animFrameNum);
                        currentAnimation.NextFrame(n.RenderBones);
                    }
                }
                if (!(node is ModelContainer))
                    continue;
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
                m.DatMelee?.bones.reset();

                if (m.Bch != null)
                {
                    foreach (BCH_Model mod in m.Bch.Models.Nodes)
                    {
                        mod.skeleton?.reset();
                    }
                }

                if (m.Bfres != null)
                {
                    foreach (var mod in m.Bfres.models)
                    {
                        mod.skeleton?.reset();
                    }
                }
            }
        }

        private void currentFrame_ValueChanged(object sender, EventArgs e)
        {
            if (currentFrameUpDown.Value > totalFrame.Value)
                currentFrameUpDown.Value = totalFrame.Value;

            animationTrackBar.Value = (int)currentFrameUpDown.Value;
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
            FrameSelectionAndSort();
        }

        public void FrameSelectionAndSort()
        {
            // Frame the selected NUD or mesh based on the bounding spheres. 
            // Frame the model container if nothing is selected. 
            if (meshList.filesTreeView.SelectedNode is IBoundableModel)
                FrameBoundableModel((IBoundableModel)meshList.filesTreeView.SelectedNode);
            else if (meshList.filesTreeView.SelectedNode is ModelContainer)
                FrameSelectedModelContainer();
            else if (meshList.filesTreeView.SelectedNode is BFRES)
                FrameSelectedBfres();
            else
                FrameAllModelContainers();

            // Depth sorting. 
            foreach (TreeNode node in meshList.filesTreeView.Nodes)
            {
                if (node is ModelContainer)
                {
                    ModelContainer modelContainer = (ModelContainer)node;
                    modelContainer.DepthSortModels(camera.TransformedPosition);
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
            foreach (Nud.Mesh mesh in modelContainer.NUD.Nodes)
            {
                if (mesh.boundingSphere[3] > boundingSphere[3])
                {
                    boundingSphere[0] = mesh.boundingSphere[0];
                    boundingSphere[1] = mesh.boundingSphere[1];
                    boundingSphere[2] = mesh.boundingSphere[2];
                    boundingSphere[3] = mesh.boundingSphere[3];
                }
            }

            camera.FrameBoundingSphere(new Vector3(boundingSphere[0], boundingSphere[1], boundingSphere[2]), boundingSphere[3], 10);
            camera.UpdateFromMouse();
        }

        private void FrameBoundableModel(IBoundableModel model)
        {
            if (model != null)
                camera.FrameBoundingSphere(model.BoundingSphere, 10);
        }

        private void FrameSelectedBfres()
        {
            BFRES bfres = (BFRES)meshList.filesTreeView.SelectedNode;
            var spheres = new List<Vector4>();
            foreach (BFRES.FMDL_Model mdl in bfres.models)
            {
                foreach (BFRES.Mesh msh in mdl.poly)
                {
                    spheres.Add(msh.BoundingSphere);
                }
            }

            Vector4 result = BoundingSphereGenerator.GenerateBoundingSphere(spheres);
            camera.FrameBoundingSphere(result, 10);
        }

        private void FrameAllModelContainers(float maxBoundingRadius = 400)
        {
            bool hasModelContainers = false;

            // Find the max NUD bounding box for all models. 
            var spheres = new List<Vector4>();

            foreach (TreeNode node in meshList.filesTreeView.Nodes)
            {
                if (node is ModelContainer)
                {
                    hasModelContainers = true;
                    ModelContainer modelContainer = (ModelContainer)node;

                    // Use the main bounding box for the NUD.
                    spheres.Add(modelContainer.NUD.BoundingSphere);

                    // It's possible that only the individual meshes have bounding boxes.
                    foreach (Nud.Mesh mesh in modelContainer.NUD.Nodes)
                    {
                        spheres.Add(mesh.BoundingSphere);
                    }

                    if (modelContainer.Bfres != null)
                    {
                        foreach (var mdl in modelContainer.Bfres.models)
                        {
                            foreach (var m in mdl.poly)
                            {
                                m.GenerateBoundingSpheres();
                                spheres.Add(m.BoundingSphere);
                            }
                        }
                    }
                }
            }

            Vector4 result = BoundingSphereGenerator.GenerateBoundingSphere(spheres);

            if (hasModelContainers)
                camera.FrameBoundingSphere(result.Xyz, result.W, 0);
            else
                camera.ResetTransforms();
        }

        #region Moveset

        public void HandleAcmd(string animname)
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
                    HandleAcmd(animname.Replace("S.omo", ".omo"));
                    return;
                }
                //Ryu ftilts
                else if (animname == "AttackS3Ss.omo")
                {
                    HandleAcmd(animname.Replace("Ss.omo", "s.omo"));
                    return;
                }
                else if (animname == "AttackS3Sw.omo")
                {
                    HandleAcmd(animname.Replace("Sw.omo", "w.omo"));
                    return;
                }
                //Rapid Jab Finisher
                else if (animname == "AttackEnd.omo")
                {
                    HandleAcmd("Attack100End.omo");
                    return;
                }
                else if (animname.Contains("ZeldaPhantomMainPhantom"))
                {
                    HandleAcmd(animname.Replace("ZeldaPhantomMainPhantom", ""));
                    return;
                }
                else if (animname == "SpecialHi1.omo")
                {
                    HandleAcmd("SpecialHi.omo");
                    return;
                }
                else if (animname == "SpecialAirHi1.omo")
                {
                    HandleAcmd("SpecialAirHi.omo");
                    return;
                }
                else
                {
                    this.acmdScript = null;
                    hitboxList.Refresh();
                    variableViewer.Refresh();
                    return;
                }
            }

            ACMDScript acmdScript = (ACMDScript)MovesetManager.Game.Scripts[crc];
            // Only update the script if it changed
            if (acmdScript != null)
            {
                // If script wasn't set, or it was set and it changed, load the new script
                if (this.acmdScript == null || (this.acmdScript != null && this.acmdScript.Script != acmdScript))
                {
                    this.acmdScript = new ForgeAcmdScript(acmdScript);
                }
            }
            else
                this.acmdScript = null;
        }

        #endregion


        private void CameraSettings_Click(object sender, EventArgs e)
        {
            if (cameraPosForm == null)
                cameraPosForm = new Gui.Menus.CameraSettings(camera);
            cameraPosForm.ShowDialog();
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
            BatchRenderModels("*model.nud", BatchRendering.OpenNud);
        }

        public void BatchRenderMeleeDatModels()
        {
            BatchRenderModels("*.dat", BatchRendering.OpenMeleeDat);
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
                                    BatchRendering.OpenBfres(file, this);
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

        private void BatchRenderModels(string searchPattern, Action<string, ModelViewport> openFiles)
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
                            foreach (string file in Directory.EnumerateFiles(folderSelect.SelectedPath, searchPattern, SearchOption.AllDirectories))
                            {
                                try
                                {
                                    openFiles(file, this);
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

        private void BatchRenderViewportToFile(string fileName, string sourcePath, string outputPath)
        {
            SetUpAndRenderViewport();

            using (Bitmap screenCapture = Framebuffer.ReadDefaultFramebufferImagePixels(fboRenderWidth, fboRenderHeight, true))
            {
                string renderName = ConvertDirSeparatorsToUnderscore(fileName, sourcePath);
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
            float scaleFactor = 1f;
            isPlaying = false;
            playButton.Text = "Play";

            GIFSettings settings = new GIFSettings((int)totalFrame.Value, scaleFactor, images.Count > 0);
            settings.ShowDialog();

            if (settings.ClearFrames)
                images.Clear();

            if (!settings.OK)
                return;

            scaleFactor = settings.ScaleFactor;

            int cFrame = (int)currentFrameUpDown.Value; //Get current frame so at the end of capturing all frames of the animation it goes back to this frame
                                                  //Disable controls
            this.Enabled = false;

            for (int i = settings.StartFrame; i <= settings.EndFrame + 1; i++)
            {
                currentFrameUpDown.Value = i;
                currentFrameUpDown.Refresh(); //Refresh the frame counter control
                Render(null, null, glViewport.Width, glViewport.Height);

                if (i != settings.StartFrame) //On i=StartFrame it captures the frame the user had before setting frame to it so ignore that one, the +1 on the for makes it so the last frame is captured
                {
                    using (Bitmap cs = Framebuffer.ReadDefaultFramebufferImagePixels(fboRenderWidth, fboRenderWidth))
                    {
                        images.Add(new Bitmap(cs, new Size((int)(cs.Width / scaleFactor), (int)(cs.Height / settings.ScaleFactor)))); //Resize images
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

            currentFrameUpDown.Value = cFrame;
        }

        private void ModelViewport_FormClosed(object sender, FormClosedEventArgs e)
        {
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
                    Runtime.textureContainers.Remove(m.NUT);

                    if (m.BNTX != null)
                    {
                        m.BNTX.textures.Clear();
                        m.BNTX.glTexByName.Clear();
                        Runtime.bntxList.Remove(m.BNTX);
                    }
                    if (m.Bfres != null && m.Bfres.FTEXContainer != null)
                    {
                        m.Bfres.FTEXContainer.FTEXtextures.Clear();
                        m.Bfres.FTEXContainer.glTexByName.Clear();
                        Runtime.ftexContainerList.Remove(m.Bfres.FTEXContainer);
                    }
                }
            }
            draw.Clear();
        }

        private void beginButton_Click(object sender, EventArgs e)
        {
            currentFrameUpDown.Value = 0;
        }

        private void endButton_Click(object sender, EventArgs e)
        {
            currentFrameUpDown.Value = totalFrame.Value;
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            // Loop the animation.
            if (currentFrameUpDown.Value == totalFrame.Value)
                currentFrameUpDown.Value = 0;
            else
                currentFrameUpDown.Value++;
        }

        private void prevButton_Click(object sender, EventArgs e)
        {
            if (currentFrameUpDown.Value != 0)
                currentFrameUpDown.Value--;
        }

        private void ViewStripButtonsBone(object sender, EventArgs e)
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
                    ((Animation)currentAnimation.Tag).frameCount = (int)totalFrame.Value;
                currentAnimation.frameCount = (int)totalFrame.Value;
                animationTrackBar.Value = 0;
                animationTrackBar.SetRange(0, currentAnimation.frameCount);
            }
        }

        private void CheckSelect()
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
                        foreach (Nud.Mesh mesh in con.NUD.Nodes)
                        {
                            foreach (Nud.Polygon poly in mesh.Nodes)
                            {
                                //if (!poly.IsSelected && !mesh.IsSelected) continue;
                                int i = 0;
                                foreach (Nud.Vertex v in poly.vertices)
                                {
                                    if (!OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ControlLeft))
                                        poly.selectedVerts[i] = 0;
                                    Vector3 n = GetScreenPoint(v.pos);
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
                    foreach (TreeNode node in draw)
                    {
                        if (!(node is ModelContainer)) continue;
                        ModelContainer con = (ModelContainer)node;
                        Nud.Polygon closestPolygon = null;
                        int index = 0;
                        double mindis = 999;
                        foreach (Nud.Mesh mesh in con.NUD.Nodes)
                        {
                            foreach (Nud.Polygon poly in mesh.Nodes)
                            {
                                //if (!poly.IsSelected && !mesh.IsSelected) continue;
                                int i = 0;
                                foreach (Nud.Vertex v in poly.vertices)
                                {
                                    //if (!poly.IsSelected) continue;
                                    if (!OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ControlLeft))
                                        poly.selectedVerts[i] = 0;

                                    Vector3 closest;
                                    if (r.TrySphereHit(v.pos, 0.2f, out closest))
                                    {
                                        double dis = r.Distance(closest);
                                        if (dis < mindis)
                                        {
                                            mindis = dis;
                                            closestPolygon = poly;
                                            index = i;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        if (closestPolygon != null)
                        {
                            closestPolygon.selectedVerts[index] = 1;
                        }
                    }
                }

                vertexTool.vertexListBox.BeginUpdate();
                vertexTool.vertexListBox.Items.Clear();
                foreach (TreeNode node in draw)
                {
                    if (!(node is ModelContainer)) continue;
                    ModelContainer con = (ModelContainer)node;
                    foreach (Nud.Mesh mesh in con.NUD.Nodes)
                    {
                        foreach (Nud.Polygon poly in mesh.Nodes)
                        {
                            int i = 0;
                            foreach (Nud.Vertex v in poly.vertices)
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

        private bool GetMouseYzPlaneProjection(out Vector3 projection)
        {
            projection = new Vector3(0, 0, 0);
            try
            {
                Matrix4 mat = camera.RotationMatrix; mat.Transpose();
                Vector3 pCamera = Vector3.TransformVector(
                    new Vector3(-camera.Translation.X, camera.Translation.Y, -camera.Translation.Z), mat);
                Vector3 pMouse = new Ray(camera, glViewport).p1;

                Vector3 direction = pMouse - pCamera;
                direction.Normalize();
                float dist = -pMouse.X / direction.X;
                if (dist <= 0)
                    return false;
                projection = pMouse + dist * direction;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void glViewport_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (currentMode != Mode.Selection && !freezeCamera)
            {
                camera.UpdateFromMouse();
            }

            if (atkdEditor != null && atkdEditor.isRendered)
            {
                Vector3 projection;
                if (GetMouseYzPlaneProjection(out projection))
                {
                    if (atkdRectClicked)
                        atkdEditor.ViewportEvent_SetSelectedXY(projection.Z, projection.Y);
                    else
                        atkdEditor.ViewportEvent_SetSelection(projection.Z, projection.Y);
                }
            }
        }

        private void glViewport_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (Mouse.GetState().LeftButton != OpenTK.Input.ButtonState.Pressed)
                return;
            if (atkdEditor != null && atkdEditor.isRendered && atkdEditor.selectedPart > 0)
            {
                atkdRectClicked = true;
                currentMode = Mode.Selection;
            }
        }

        private void glViewport_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (atkdRectClicked)
            {
                atkdRectClicked = false;
                currentMode = Mode.Normal;
            }
        }

        private void weightToolButton_Click(object sender, EventArgs e)
        {
            //vertexTool.Show();
        }

        private void Render(object sender, PaintEventArgs e, int width, int height, int defaultFbo = 0)
        {
            // Don't render if the context and resources aren't set up properly.
            // Watching textures suddenly appear looks weird.
            if (OpenTkSharedResources.SetupStatus != OpenTkSharedResources.SharedResourceStatus.Initialized || Runtime.glTexturesNeedRefreshing)
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
                if (meshList.filesTreeView.SelectedNode is BchTexture)
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
                    DrawBntxTexAndUvs();
                    return;
                }
                if (meshList.filesTreeView.SelectedNode is FTEX)
                {
                    DrawFtexTexAndUvs();
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
            }

            if (cameraPosForm != null)
                cameraPosForm.ApplyCameraAnimation(camera, animationTrackBar.Value);

            if (Runtime.renderFloor)
                FloorDrawing.DrawFloor(camera.MvpMatrix);

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
                ScreenDrawing.DrawTexturedQuad((Texture)colorHdrFbo.Attachments[1], imageBrightHdrFbo.Width, imageBrightHdrFbo.Height, screenVao);

                // Setup the normal viewport dimensions again.
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, defaultFbo);
                GL.Viewport(0, 0, width, height);

                ScreenDrawing.DrawScreenQuadPostProcessing((Texture)colorHdrFbo.Attachments[0], (Texture)imageBrightHdrFbo.Attachments[0], screenVao);
            }

            FixedFunctionRendering();

            GL.PopAttrib();
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
            Vector3 topColor = ColorUtils.GetVector3(Runtime.backgroundGradientTop);
            Vector3 bottomColor = ColorUtils.GetVector3(Runtime.backgroundGradientBottom);

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
                {
                    if (m is MeleeDataNode)
                    {
                        ((MeleeDataNode)m).Render(camera);
                    }
                    if (m is ModelContainer)
                        ((ModelContainer)m).Render(camera, depthMap, lightMatrix, new Vector2(glViewport.Width, glViewport.Height), drawShadow);

                }

            if (ViewComboBox.SelectedIndex == 1)
                foreach (TreeNode m in draw)
                    if (m is ModelContainer)
                        ((ModelContainer)m).RenderPoints(camera);
        }

        private void DrawOverlays()
        {
            // Clearing the depth buffer allows stuff to render on top of the models.
            GL.Clear(ClearBufferMask.DepthBufferBit);

            if (Runtime.renderLvd)
                lvd.Render();

            if (Runtime.renderBones)
                foreach (TreeNode m in draw)
                    if(m is ModelContainer)
                        ((ModelContainer)m).RenderBones();

            // ACMD
            if (paramManager != null && Runtime.renderHurtboxes && draw.Count > 0 && (draw[0] is ModelContainer))
            {
                // Doesn't do anything. ParamManager is always null.
                paramManager.RenderHurtboxes(frame, scriptId, acmdScript, ((ModelContainer)draw[0]).GetVBN());
            }

            if (acmdScript != null && draw.Count > 0 && (draw[0] is ModelContainer))
                acmdScript.Render(((ModelContainer)draw[0]).GetVBN());

            // ATKD Render
            if (atkdEditor != null)
            {
                atkdEditor.isRendered = false;
                if (Runtime.loadAndRenderAtkd && draw.Count > 0 && (draw[0] is ModelContainer))
                    atkdEditor.Viewport_Render(((ModelContainer)draw[0]).GetVBN());
            }

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
                    CheckSelect();
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
                    Animation.KeyNode thisNode = null;
                    foreach (Animation.KeyNode node in currentAnimation.bones)
                    {
                        if (node.Text.Equals(transformTool.b.Text))
                        {
                            // found
                            thisNode = node;
                            break;
                        }
                    }
                    if (thisNode == null)
                    {
                        thisNode = new Animation.KeyNode(transformTool.b.Text);
                        currentAnimation.bones.Add(thisNode);
                    }

                    // update or add the key frame
                    thisNode.SetKeyFromBone((float)currentFrameUpDown.Value, transformTool.b);
                }
            }
        }

        private void glViewport_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == 'f')
                FrameSelectionAndSort();

            if (e.KeyChar == 'i')
            {
                ShaderTools.SetUpShaders(true);
                ShaderTools.SaveErrorLogs();
            }
        }

        private void ModelViewport_KeyDown(object sender, KeyEventArgs e)
        {
            // Super secret commands. I'm probably going to be the only one that uses them anyway...
            if (Keyboard.GetState().IsKeyDown(Key.L) && Keyboard.GetState().IsKeyDown(Key.S) && Keyboard.GetState().IsKeyDown(Key.T))
                ParamTools.BatchExportParamValuesAsCsv("light_set");

            if (Keyboard.GetState().IsKeyDown(Key.R) && Keyboard.GetState().IsKeyDown(Key.N) && Keyboard.GetState().IsKeyDown(Key.D))
                ParamTools.BatchExportParamValuesAsCsv("stprm");
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

        private void DrawBntxTexAndUvs()
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
                DrawNswbfresUvsForSelectedTexture(tex);

            glViewport.SwapBuffers();
        }

        private void DrawNutTex()
        {
            GL.PopAttrib();
            NutTexture tex = ((NutTexture)meshList.filesTreeView.SelectedNode);
            ScreenDrawing.DrawTexturedQuad(((NUT)tex.Parent).glTexByHashId[tex.HashId], tex.Width, tex.Height, screenVao);
        }

        private void DrawBchTex()
        {
            GL.PopAttrib();
            BchTexture tex = ((BchTexture)meshList.filesTreeView.SelectedNode);
            ScreenDrawing.DrawTexturedQuad(tex.display, tex.width, tex.height, screenVao);
        }

        private void DrawFtexTexAndUvs()
        {
            GL.PopAttrib();

            FTEX tex = ((FTEX)meshList.filesTreeView.SelectedNode);
            switch (tex.format)
            {
                case (int)Gtx.Gx2SurfaceFormat.Gx2SurfaceFormatTBc4Unorm:
                    ScreenDrawing.DrawTexturedQuad(tex.display, tex.width, tex.height, screenVao, true, false, false);
                    break;
                case (int)Gtx.Gx2SurfaceFormat.Gx2SurfaceFormatTBc4Snorm:
                    ScreenDrawing.DrawTexturedQuad(tex.display, tex.width, tex.height, screenVao, true, false, false);
                    break;
                default:
                    ScreenDrawing.DrawTexturedQuad(tex.display, tex.width, tex.height, screenVao);
                    break;
            }

            if (Runtime.drawUv)
                DrawBfresUvsForSelectedTexture(tex);

            glViewport.SwapBuffers();
        }

        private void DrawAreaLightBoundingBoxes()
        {
            foreach (AreaLight light in LightTools.areaLights)
            {
                ShapeDrawing.DrawRectangularPrism(new Vector3(light.positionX, light.positionY, light.positionZ),
                    light.scaleX, light.scaleY, light.scaleZ, true);
            }
        }

        private void DrawNswbfresUvsForSelectedTexture(BRTI tex)
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

        private void DrawBfresUvsForSelectedTexture(FTEX tex)
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
            if (OpenTkSharedResources.SetupStatus != OpenTkSharedResources.SharedResourceStatus.Initialized)
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

            OpenTkSharedResources.InitializeSharedResources();

            if (OpenTkSharedResources.SetupStatus == OpenTkSharedResources.SharedResourceStatus.Initialized)
            {
                screenVao = ScreenDrawing.CreateScreenTriangle();

                // Use the viewport dimensions by default.
                fboRenderWidth = glViewport.Width;
                fboRenderHeight = glViewport.Height;

                SetUpBuffersAndTextures();

                if (Runtime.enableOpenTkDebugOutput)
                {
                    glViewport.MakeCurrent();
                    OpenTkSharedResources.EnableOpenTkDebugOutput();
                }
            }
        }

        private void viewportPanel_Resize(object sender, EventArgs e)
        {
            int spacing = 8;
            glViewport.Width = viewportPanel.Width - spacing;
            glViewport.Height = viewportPanel.Height - spacing;
        }

        private void RefreshGlTextures()
        {
            // Regenerate all the texture objects.
            foreach (TreeNode node in meshList.filesTreeView.Nodes)
            {
                if (!(node is ModelContainer))
                    continue;

                ModelContainer m = (ModelContainer)node;

                m.NUT?.RefreshGlTexturesByHashId();
                m.BNTX?.RefreshGlTexturesByName();
                m.Bfres?.FTEXContainer?.RefreshGlTexturesByName();
            }
        }
    }
}