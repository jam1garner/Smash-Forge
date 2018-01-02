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

namespace Smash_Forge
{
    public partial class ModelViewport : EditorBase
    {
        // setup
        bool ReadyToRender = false;

        // View controls
        Camera Camera = new Camera();
        public Matrix4 lightMatrix, lightProjection;
        public GUI.Menus.CameraSettings cameraPosForm = null;

        // Shadow map
        int sfb, sw = 512, sh = 512, depthmap, hdrFBO;

        // Functions of Viewer
        public enum Mode
        {
            Normal = 0,
            Photoshoot,
            Selection
        }
        public Mode CurrentMode = Mode.Normal;

        FrameTimer frameTime = new FrameTimer();


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
                animationTrackBar.SetRange(0, value.FrameCount);
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
        private LVD _lvd = new LVD();
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

        // controls
        VertexTool vertexTool = new VertexTool();

        // Contents
        //public List<ModelContainer> draw = new List<ModelContainer>();

        public MeshList MeshList = new MeshList();
        public AnimListPanel AnimList = new AnimListPanel();
        public TreeNodeCollection draw;

        public ModelViewport()
        {
            InitializeComponent();
            Camera = new Camera();
            FilePath = "";
            Text = "Model Viewport";

            CalculateLightSource();

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


            ViewComboBox.SelectedIndex = 0;

            draw = MeshList.treeView1.Nodes;
        }

        public ModelViewport(string filename) : this()
        {

        }

        ~ModelViewport()
        {
            
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
            vertexTool.vp = this;
            //Application.Idle += Application_Idle;
            var timer = new Timer();
            timer.Interval = 1000 / 120;
            timer.Tick += new EventHandler(Application_Idle);
            timer.Start();

            for (int i = 0; i < Lights.stageDiffuseLightSet.Length; i++)
            {
                // should properly initialize these eventually
                Lights.stageDiffuseLightSet[i] = new DirectionalLight();
                Lights.stageDiffuseLightSet[i].id = "Stage " + i;
            }

            for (int i = 0; i < Lights.stageFogSet.Length; i++)
            {
                // should properly initialize these eventually
                Lights.stageFogSet[i] = new Vector3(0);
            }
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

        private void glViewport_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (ReadyToRender && glViewport != null)
            {
                glViewport.MakeCurrent();
                GL.LoadIdentity();
                GL.Viewport(glViewport.ClientRectangle);

                Camera.setRenderWidth(glViewport.Width);
                Camera.setRenderHeight(glViewport.Height);
                Camera.Update();
            }
        }

        public void CalculateLightSource()
        {
            Matrix4.CreateOrthographicOffCenter(-10.0f, 10.0f, -10.0f, 10.0f, 1.0f, Runtime.renderDepth, out lightProjection);
            Matrix4 lightView = Matrix4.LookAt(Vector3.Transform(Vector3.Zero, Camera.getMVPMatrix()).Normalized(),
                new Vector3(0),
                new Vector3(0, 1, 0));
            lightMatrix = lightProjection * lightView;
        }

        private Vector3 getScreenPoint(Vector3 pos)
        {
            Vector4 n = Vector4.Transform(new Vector4(pos, 1), Camera.getMVPMatrix());
            n.X /= n.W;
            n.Y /= n.W;
            n.Z /= n.W;
            return n.Xyz;
        }

        private void glViewport_Resize(object sender, EventArgs e)
        {
            if (ReadyToRender && glViewport.Height != 0 && glViewport.Width != 0)
            {
                GL.LoadIdentity();
                GL.Viewport(glViewport.ClientRectangle);

                Camera.setRenderWidth(glViewport.Width);
                Camera.setRenderHeight(glViewport.Height);
                Camera.Update();
            }
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
                foreach (TreeNode node in MeshList.treeView1.Nodes)
                {
                    if (!(node is ModelContainer)) continue;
                    ModelContainer m = (ModelContainer)node;
                    m.NUD.applyMTA(MaterialAnimation, frameNum);
                }
            }
            
            if (Animation == null) return;

            // Process script first in case we have to speed up the animation
            if (ACMDScript != null)
                ACMDScript.processToFrame(frameNum);

            float animFrameNum = frameNum;
            if (ACMDScript != null && Runtime.useFrameDuration)
                animFrameNum = ACMDScript.animationFrame;// - 1;
            
            foreach (TreeNode node in MeshList.treeView1.Nodes)
            {
                if (!(node is ModelContainer)) continue;
                ModelContainer m = (ModelContainer)node;
                Animation.SetFrame(animFrameNum);
                if (m.VBN != null)
                    Animation.NextFrame(m.VBN);

                // Deliberately do not ever use ACMD/animFrame to modify these other types of model
                if (m.dat_melee != null)
                {
                    Animation.SetFrame(frameNum);
                    Animation.NextFrame(m.dat_melee.bones);
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
            foreach (TreeNode node in MeshList.treeView1.Nodes)
            {
                if (!(node is ModelContainer)) continue;
                ModelContainer m = (ModelContainer)node;
                m.NUD.clearMTA();
                if (m.VBN != null)
                    m.VBN.reset();

                // Deliberately do not ever use ACMD/animFrame to modify these other types of model
                if (m.dat_melee != null)
                {
                    m.dat_melee.bones.reset();
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
        }

        #endregion

        #region Camera Bar Functions

        private void ResetCamera_Click(object sender, EventArgs e)
        {
            Camera.setPosition(new Vector3(0, 10, -80));
            Camera.setRotX(0);
            Camera.setRotY(0);
            Camera.Update();
        }

        #endregion

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

            //Console.WriteLine("Handling " + animname);
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
            cameraPosForm = new GUI.Menus.CameraSettings(Camera);
            cameraPosForm.ShowDialog();
        }

        private void glViewport_MouseMove(object sender, MouseEventArgs e)
        {
            Camera.Update();
        }

        private void ViewComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LVDEditor.Visible = false;
            LVDList.Visible = false;
            MeshList.Visible = false;
            AnimList.Visible = false;
            ACMDEditor.Visible = false;
            switch (ViewComboBox.SelectedIndex)
            {
                case 0:
                    MeshList.Visible = true;
                    AnimList.Visible = true;
                    break;
                case 1:
                    LVDEditor.Visible = true;
                    LVDList.Visible = true;
                    break;
                case 2:
                    AnimList.Visible = true;
                    ACMDEditor.Visible = true;
                    break;
            }
        }

        private void RenderButton_Click(object sender, EventArgs e)
        {
            CaptureScreen(true).Save(MainForm.executableDir + "\\Render.png");
        }

        public Bitmap CaptureScreen(bool saveAlpha)
        {
            int width = glViewport.Width;
            int height = glViewport.Height;

            byte[] pixels = new byte[width * height * 4];
            glViewport.MakeCurrent();
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

        List<Bitmap> images = new List<Bitmap>();
        float ScaleFactor = 1f;
        private void GIFButton_Click(object sender, EventArgs e)
        {
            if (Animation == null)
                return;

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

            currentFrame.Value = cFrame;
        }

        private void ModelViewport_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (TreeNode n in MeshList.treeView1.Nodes)
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

                    foreach (ModelContainer con in draw)
                    {
                        foreach (NUD.Mesh mesh in con.NUD.Nodes)
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
                    Ray r = RenderTools.createRay(Camera.getMVPMatrix(), GetMouseOnViewport());
                    Vector3 close = Vector3.Zero;
                    foreach (ModelContainer con in draw)
                    {
                        foreach (NUD.Mesh mesh in con.NUD.Nodes)
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
                CurrentMode = Mode.Normal;
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

        private void Render(object sender, PaintEventArgs e)
        {
            if (!ReadyToRender)
                return;

            glViewport.MakeCurrent();

            GL.LoadIdentity();
            GL.Viewport(glViewport.ClientRectangle);

            // Push all attributes so we don't have to clean up later
            GL.PushAttrib(AttribMask.AllAttribBits);
            // clear the gf buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            
            // use fixed function pipeline for drawing background and floor grid
            GL.UseProgram(0);

            if (MeshList.treeView1.SelectedNode != null)
            {
                if (MeshList.treeView1.SelectedNode is BCH_Texture)
                {
                    GL.PopAttrib();
                    BCH_Texture tex = ((BCH_Texture)MeshList.treeView1.SelectedNode);
                    RenderTools.DrawTexturedQuad(tex.display, tex.Width, tex.Height, true, true, true, true, false, true);
                    glViewport.SwapBuffers();
                    return;
                }
                if (MeshList.treeView1.SelectedNode is NUT_Texture)
                {
                    GL.PopAttrib();
                    NUT_Texture tex = ((NUT_Texture)MeshList.treeView1.SelectedNode);
                    RenderTools.DrawTexturedQuad(((NUT)tex.Parent).draw[tex.HASHID], tex.Width, tex.Height, true, true, true, true, false, true);
                    glViewport.SwapBuffers();
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

            // Camera Update
            // -------------------------------------------------------------
            GL.MatrixMode(MatrixMode.Projection);
            if (glViewport.ClientRectangle.Contains(glViewport.PointToClient(Cursor.Position))
             && glViewport.Focused 
             && CurrentMode == Mode.Normal)
            {
                Camera.Update();
                //if (cameraPosForm != null && !cameraPosForm.IsDisposed)
                //    cameraPosForm.updatePosition();
            }
            try
            {
                if (OpenTK.Input.Mouse.GetState() != null)
                    Camera.mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;
            } catch
            {

            }

            Matrix4 matrix = Camera.getMVPMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref matrix);

            // Floor
            // -------------------------------------------------------------
            if (Runtime.renderFloor)
                RenderTools.drawFloor();

            // Shadows
            // -------------------------------------------------------------
            if (Runtime.drawModelShadow)
            {
                CalculateLightSource();
                // update light matrix and setup shadowmap rendering
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadMatrix(ref lightMatrix);
                GL.Enable(EnableCap.DepthTest);
                GL.Viewport(0, 0, sw, sh);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, sfb);

                foreach (ModelContainer m in draw)
                    m.RenderShadow(Camera, 0, Matrix4.Zero, Camera.getMVPMatrix());

                // reset matrices and viewport for model rendering again
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.LoadMatrix(ref matrix);
                GL.Viewport(glViewport.ClientRectangle);
            }

            // render models into hdr buffer
            // -------------------------------------------------------------
            if (Runtime.useDepthTest)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Lequal);
            }

            else
                GL.Disable(EnableCap.DepthTest);

            GL.Enable(EnableCap.DepthTest);

            // Models
            // -------------------------------------------------------------
            //frameTime.Start();
            if (Runtime.renderModel)
                foreach (TreeNode m in draw)
                    if(m is ModelContainer)
                        ((ModelContainer)m).Render(Camera, 0, Matrix4.Zero, Camera.getMVPMatrix());


            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            /*// render gaussian blur stuff
            if (Runtime.drawQuadBlur)
                DrawQuadBlur();



            // render full screen quad for post processing
            if (Runtime.drawQuadFinalOutput)
                DrawQuadFinalOutput();*/

            // use fixed function pipeline again for area lights, lvd, bones, hitboxes, etc
            SetupFixedFunctionRendering();

            /*// draw path.bin
            if (Runtime.renderPath)
                DrawPathDisplay();

            // area light bounding boxes should intersect stage geometry and not render on top
            if (Runtime.drawAreaLightBoundingBoxes)
                DrawAreaLightBoundingBoxes();*/

            // clear depth buffer so stuff will render on top of the models
            GL.Clear(ClearBufferMask.DepthBufferBit);

            if (Runtime.renderLVD)
                _lvd.Render();

            if (Runtime.renderBones)
                foreach (ModelContainer m in draw)
                    m.RenderBones();


            // ACMD
            if (ParamManager != null && Runtime.renderHurtboxes && draw.Count > 0 && (draw[0] is ModelContainer))
            {
                ParamManager.RenderHurtboxes(Frame, scriptId, ACMDScript, ((ModelContainer)draw[0]).GetVBN());
            }

            if (ACMDScript!=null && draw.Count > 0 && (draw[0] is ModelContainer))
                ACMDScript.Render(((ModelContainer)draw[0]).GetVBN());
            //Debug.WriteLine(frameTime.getAverageRenderTime());


            // Mouse selection
            // -------------------------------------------------------------

            if (CurrentMode == Mode.Selection)
            {
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Clear(ClearBufferMask.DepthBufferBit);

                if (OpenTK.Input.Mouse.GetState().IsButtonDown(OpenTK.Input.MouseButton.Right))
                {
                    CurrentMode = Mode.Normal;
                }

                Vector2 m = GetMouseOnViewport();
                GL.Color3(Color.AliceBlue);
                GL.LineWidth(2f);
                GL.Begin(PrimitiveType.LineLoop);
                GL.Vertex2(sx1, sy1);
                GL.Vertex2(m.X, sy1);
                GL.Vertex2(m.X, m.Y);
                GL.Vertex2(sx1, m.Y);
                GL.End();
            }

            /*if (CurrentMode == Mode.Photoshoot)
            {
                freezeCamera = false;
                if (Keyboard.GetState().IsKeyDown(Key.W) && Mouse.GetState().IsButtonDown(MouseButton.Left))
                {
                    shootX = this.PointToClient(Cursor.Position).X;
                    shootY = this.PointToClient(Cursor.Position).Y;
                    freezeCamera = true;
                }
                // Hold on to your pants, boys
                RenderTools.DrawPhotoshoot(glViewport, shootX, shootY, shootWidth, shootHeight);
            }*/

            GL.PopAttrib();
            glViewport.SwapBuffers();
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

    }
}
