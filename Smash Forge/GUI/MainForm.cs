using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using WeifenLuo.WinFormsUI.Docking;
using System.Linq;
using OpenTK;
using System.Data;
using Octokit;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Threading;
using Microsoft.VisualBasic.Devices;
using Smash_Forge.GUI.Menus;
using Smash_Forge.GUI.Editors;
using SALT.PARAMS;
using SALT.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge
{
    public partial class MainForm : FormBase
    {

        public static MainForm Instance
        {
            get { return _instance != null ? _instance : (_instance = new MainForm()); }
        }

        private static MainForm _instance;

        public WorkspaceManager Workspace { get; set; }

        public String[] filesToOpen = null;
        public static string executableDir = null;
        public static csvHashes Hashes;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ThreadStart t = new ThreadStart(Smash_Forge.Update.CheckLatest);
            Thread thread = new Thread(t);
            thread.Start();
            Runtime.renderDepth = 100000.0f;
            foreach (var vp in viewports)
                AddDockedControl(vp);

            animationsWindowToolStripMenuItem.Checked =
                boneTreeToolStripMenuItem.Checked = true;

            allViewsPreset(new Object(), new EventArgs());

            Hashes = new csvHashes(Path.Combine(executableDir, "hashTable.csv"));
            animList.treeView1.Nodes.Add(animNode);
            animList.treeView1.Nodes.Add(mtaNode);
            Runtime.renderBones = true;
            Runtime.renderLVD = true;
            Runtime.renderFloor = true;
            Runtime.renderBackGround = true;
            Runtime.renderHitboxes = true;
            Runtime.renderInterpolatedHitboxes = true;
            Runtime.renderSpecialBubbles = true;
            Runtime.hitboxRenderMode = Hitbox.RENDER_KNOCKBACK;
            Runtime.hitboxAlpha = 130;
            Runtime.hurtboxAlpha = 80;
            Runtime.hurtboxColor = System.Drawing.Color.FromArgb(0x00, 0x53, 0x8A);     // Strong blue
            Runtime.hurtboxColorHi = System.Drawing.Color.FromArgb(0xFF, 0x8E, 0x00);  //Vivid Orange Yellow
            Runtime.hurtboxColorMed = System.Drawing.Color.FromArgb(0xF6, 0x76, 0x8E);  //Strong Purplish Pink
            Runtime.hurtboxColorLow = System.Drawing.Color.FromArgb(0x00, 0x53, 0x8A);  // Strong blue
            Runtime.hurtboxColorSelected = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF); //White
            Runtime.windboxColor = System.Drawing.Color.Blue;
            Runtime.grabboxColor = System.Drawing.Color.Purple;
            Runtime.searchboxColor = System.Drawing.Color.DarkOrange;
            Runtime.counterBubbleColor = System.Drawing.Color.FromArgb(0x89, 0x89, 0x89);
            Runtime.reflectBubbleColor = System.Drawing.Color.Cyan;
            Runtime.shieldBubbleColor = System.Drawing.Color.Red;
            Runtime.absorbBubbleColor = System.Drawing.Color.SteelBlue;
            Runtime.wtSlowdownBubbleColor = System.Drawing.Color.FromArgb(0x9a, 0x47, 0x9a);

            Runtime.useFrameDuration = false;
            Runtime.hitboxKnockbackColors = new List<System.Drawing.Color>();
            Runtime.hitboxIdColors = new List<System.Drawing.Color>();
            Runtime.renderModel = true;
            Runtime.renderPath = true;
            Runtime.renderCollisions = true;
            Runtime.renderCollisionNormals = false;
            Runtime.renderGeneralPoints = true;
            Runtime.renderItemSpawners = true;
            Runtime.renderSpawns = true;
            Runtime.renderRespawns = true;
            Runtime.renderOtherLVDEntries = true;
            Runtime.renderAlpha = true;
            Runtime.renderVertColor = true;
            Runtime.renderSwag = false;
            Runtime.renderHurtboxes = true;
            Runtime.renderHurtboxesZone = true;
            Runtime.renderECB = false;
            Runtime.renderIndicators = false;
            Runtime.renderLedgeGrabboxes = false;
            Runtime.renderTetherLedgeGrabboxes = false;
            Runtime.renderReverseLedgeGrabboxes = false;
            Runtime.paramDir = "";

            viewportWindowToolStripMenuItem.Checked = true;
            openFiles();

            Runtime.StartupFromConfig(MainForm.executableDir + "\\config.xml");

            SetupShaders();

            RenderTools.Setup();
        }

        private static void SetupShaders()
        {
            // load up the shaders
            Shader cub = new Shader();
            cub.vertexShader(RenderTools.cubevs);
            cub.fragmentShader(RenderTools.cubefs);
            Runtime.shaders.Add("SkyBox", cub);

            Shader sha = new Shader();
            sha.vertexShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/Shadow_vs.txt"));
            sha.fragmentShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/Shadow_fs.txt"));
            Runtime.shaders.Add("Shadow", sha);

            Shader texture = new Shader();
            texture.vertexShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/Texture_vs.txt"));
            texture.fragmentShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/Texture_fs.txt"));
            Runtime.shaders.Add("Texture", texture);

            Shader gradient = new Shader();
            gradient.vertexShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/Gradient_vs.txt"));
            gradient.fragmentShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/Gradient_fs.txt"));
            Runtime.shaders.Add("Gradient", gradient);

            if (!Runtime.shaders.ContainsKey("NUD"))
            {
                Shader nud = new Shader();
                nud.vertexShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/NUD_vs.txt"));
                nud.fragmentShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/NUD_fs.txt"));
                Runtime.shaders.Add("NUD", nud);
            }

            if (!Runtime.shaders.ContainsKey("NUD_Debug"))
            {
                Shader debug = new Shader();
                debug.vertexShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/NUD_vs.txt"));
                debug.fragmentShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/NUD_Debug_fs.txt"));
                Runtime.shaders.Add("NUD_Debug", debug);
            }

            if (!Runtime.shaders.ContainsKey("MBN"))
            {
                Shader mbn = new Shader();
                mbn.vertexShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/MBN_vs.txt"));
                mbn.fragmentShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/MBN_fs.txt"));
                Runtime.shaders.Add("MBN", mbn);
            }

            Shader quad = new Shader();
            quad.vertexShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/Quad_vs.txt"));
            quad.fragmentShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/Quad_fs.txt"));
            Runtime.shaders.Add("Quad", quad);

            Shader blur = new Shader();
            blur.vertexShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/Blur_vs.txt"));
            blur.fragmentShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/Blur_fs.txt"));
            Runtime.shaders.Add("Blur", blur);

            if (!Runtime.shaders.ContainsKey("DAT"))
            {
                Shader DAT = new Shader();
                DAT.vertexShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/DAT_vs.txt"));
                DAT.fragmentShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/DAT_fs.txt"));
                Runtime.shaders.Add("DAT", DAT);
            }
        }

        public void openFiles()
        {
            for (int i = 0; i < filesToOpen.Length; i++)
            {
                string file = filesToOpen[i];
                if (file.Equals("--clean"))
                {
                    clearWorkspaceToolStripMenuItem_Click(new object(), new EventArgs());
                    cleanPreset(new object(), new EventArgs());
                }
                else if (file.Equals("--superclean"))
                {
                    clearWorkspaceToolStripMenuItem_Click(new object(), new EventArgs());
                    superCleanPreset(new object(), new EventArgs());
                }
                else if (file.Equals("--preview"))
                {
                    Text = "Meteor Preview";
                    superCleanPreset(new object(), new EventArgs());
                    meshList.Show();
                    NUT chr_00_nut = null, chr_11_nut = null, chr_13_nut = null, stock_90_nut = null;
                    string chr_00_loc = null, chr_11_loc = null, chr_13_loc = null, stock_90_loc = null;
                    String nud = null, nut, vbn;
                    for (int j = i + 1; j < filesToOpen.Length; j++)
                    {
                        switch (filesToOpen[j])
                        {
                            case "-nud":
                                nud = filesToOpen[j + 1];
                                break;
                            case "-nut":
                                nut = filesToOpen[j + 1];
                                break;
                            case "-vbn":
                                vbn = filesToOpen[j + 1];
                                break;
                            case "-chr_00":
                                chr_00_loc = filesToOpen[j + 1];
                                if (!File.Exists(filesToOpen[j + 1])) break;
                                chr_00_nut = new NUT(filesToOpen[j + 1]);
                                Runtime.TextureContainers.Add(chr_00_nut);
                                break;
                            case "-chr_11":
                                chr_11_loc = filesToOpen[j + 1];
                                if (!File.Exists(filesToOpen[j + 1])) break;
                                chr_11_nut = new NUT(filesToOpen[j + 1]);
                                Runtime.TextureContainers.Add(chr_11_nut);
                                break;
                            case "-chr_13":
                                chr_13_loc = filesToOpen[j + 1];
                                if (!File.Exists(filesToOpen[j + 1])) break;
                                chr_13_nut = new NUT(filesToOpen[j + 1]);
                                Runtime.TextureContainers.Add(chr_13_nut);
                                break;
                            case "-stock_90":
                                stock_90_loc = filesToOpen[j + 1];
                                if (!File.Exists(filesToOpen[j + 1])) break;
                                stock_90_nut = new NUT(filesToOpen[j + 1]);
                                Runtime.TextureContainers.Add(stock_90_nut);
                                break;
                        }
                        i++;
                    }
                    if (nud != null)
                    {
                        openNud(nud);
                    }

                    UIPreview uiPreview = new UIPreview(chr_00_nut, chr_11_nut, chr_13_nut, stock_90_nut);
                    uiPreview.chr_00_loc = chr_00_loc;
                    uiPreview.chr_11_loc = chr_11_loc;
                    uiPreview.chr_13_loc = chr_13_loc;
                    uiPreview.stock_90_loc = stock_90_loc;
                    uiPreview.ShowHint = DockState.DockRight;
                    dockPanel1.DockRightPortion = 270;
                    AddDockedControl(uiPreview);
                    i += 4;
                }
                else
                {
                    openFile(file);
                }
            }
            filesToOpen = null;
        }

        private void MainForm_Close(object sender, EventArgs e)
        {
            if (Runtime.TargetNUD != null)
                Runtime.TargetNUD.Destroy();

            foreach (ModelContainer n in Runtime.ModelContainers)
            {
                n.Destroy();
            }
            foreach (NUT n in Runtime.TextureContainers)
            {
                n.Destroy();
            }
        }

        public void AddDockedControl(DockContent content)
        {
            if (dockPanel1.DocumentStyle == DocumentStyle.SystemMdi)
            {
                content.MdiParent = this;
                content.Show();
            }
            else if(content != null && dockPanel1 != null)
                content.Show(dockPanel1);
        }

        private void RegenPanels()
        {
            if (animList.IsDisposed)
            {
                animList = new AnimListPanel();
                animNode = (TreeNode)animNode.Clone();
                mtaNode = (TreeNode)mtaNode.Clone();
                animList.treeView1.Nodes.Add(animNode);
                animList.treeView1.Nodes.Add(mtaNode);
            }
            if (boneTreePanel.IsDisposed)
            {
                boneTreePanel = new BoneTreePanel();
                boneTreePanel.treeRefresh();
            }
            if (project.IsDisposed)
            {
                project = new ProjectTree();
                project.fillTree();
            }
            if (lvdList.IsDisposed)
            {
                lvdList = new LVDList();
                lvdList.fillList();
            }
            if (lvdEditor.IsDisposed)
            {
                lvdEditor = new LVDEditor();
            }
            if (meshList.IsDisposed)
            {
                meshList = new MeshList();
                meshList.refresh();
            }
            if (hurtboxList.IsDisposed)
            {
                hurtboxList = new HurtboxList();
                hurtboxList.refresh();
            }
            if (Runtime.hitboxList.IsDisposed)
            {
                Runtime.hitboxList = new HitboxList();
                Runtime.hitboxList.refresh();
            }
            if (Runtime.variableViewer.IsDisposed)
            {
                Runtime.variableViewer = new VariableList();
                Runtime.variableViewer.refresh();
            }
            if (Runtime.acmdEditor != null && Runtime.acmdEditor.IsDisposed)
            {
                Runtime.acmdEditor = new ACMDPreviewEditor();
            }
        }

        #region Members

        public AnimListPanel animList = new AnimListPanel() { ShowHint = DockState.DockRight };
        public BoneTreePanel boneTreePanel = new BoneTreePanel() { ShowHint = DockState.DockLeft };
        public static TreeNode animNode = new TreeNode("Bone Animations");
        public TreeNode mtaNode = new TreeNode("Material Animations");
        public ProjectTree project = new ProjectTree() { ShowHint = DockState.DockLeft };
        public LVDList lvdList = new LVDList() { ShowHint = DockState.DockLeft };
        public LVDEditor lvdEditor = new LVDEditor() { ShowHint = DockState.DockRight };
        public List<PARAMEditor> paramEditors = new List<PARAMEditor>() { };
        public List<MTAEditor> mtaEditors = new List<MTAEditor>() { };
        public List<ACMDEditor> ACMDEditors = new List<ACMDEditor>() { };
        public List<SwagEditor> SwagEditors = new List<SwagEditor>() { };
        public MeshList meshList = new MeshList() { ShowHint = DockState.DockRight };
        public HurtboxList hurtboxList = new HurtboxList() { ShowHint = DockState.DockLeft };
        public List<VBNViewport> viewports = new List<VBNViewport>() { new VBNViewport() }; // Default viewport (may mess up with more or less?)
        public NUTEditor nutEditor = null;
        public NUS3BANKEditor nusEditor = null;
        public _3DSTexEditor texEditor = null;
        public CameraSettings cameraForm = null;

        #endregion

        #region ToolStripMenu

        private void openNUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PARAMEditor currentParam = null;
            ACMDEditor currentACMD = null;
            SwagEditor currentSwagEditor = null;
            foreach (PARAMEditor p in paramEditors)
                if (p.ContainsFocus)
                    currentParam = p;

            foreach (ACMDEditor a in ACMDEditors)
                if (a.ContainsFocus)
                    currentACMD = a;

            foreach (SwagEditor s in SwagEditors)
                if (s.ContainsFocus)
                    currentSwagEditor = s;

            if (currentParam != null)
                currentParam.saveAs();
            else if (currentACMD != null)
                currentACMD.save();
            else if (currentSwagEditor != null)
                currentSwagEditor.save();
            else
            {
                string filename = "";
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Supported Filetypes (VBN,LVD,DAE,DAT)|*.vbn;*.lvd;*.dae;*.dat;|Smash 4 Boneset|*.vbn|All files(*.*)|*.*";
                DialogResult result = save.ShowDialog();

                if (result == DialogResult.OK)
                {
                    filename = save.FileName;
                    saveFile(filename);
                    //OMO.createOMO (anim, vbn, "C:\\Users\\ploaj_000\\Desktop\\WebGL\\test_outut.omo", -1, -1);
                }
            }
        }

        public static void DAEReadSemantic(int p, Dictionary<string, string> semantic)
        {

        }

        private void openNud(string filename, string name = "")
        {
            string[] files = Directory.GetFiles(System.IO.Path.GetDirectoryName(filename));

            string pathNUD = filename;
            string pathNUT = "";
            string pathJTB = "";
            string pathVBN = "";
            string pathMTA = "";
            string pathSB = "";
            string pathMOI = "";
            string pathXMB = "";
            List<string> pacs = new List<string>();

            foreach (string file in files)
            {
                if (file.EndsWith(".nut"))
                    pathNUT = file;
                if (file.EndsWith(".vbn"))
                    pathVBN = file;
                if (file.EndsWith(".jtb"))
                    pathJTB = file;
                if (file.EndsWith(".mta"))
                    pathMTA = file;
                if (file.EndsWith(".sb"))
                    pathSB = file;
                if (file.EndsWith(".moi"))
                    pathMOI = file;
                if (file.EndsWith(".pac"))
                    pacs.Add(file);
                if (file.EndsWith("xmb"))
                    pathXMB = file;
            }

            ModelContainer model = new ModelContainer();
            model.name = name;
            if (!pathVBN.Equals(""))
            {
                model.vbn = new VBN(pathVBN);
                Runtime.TargetVBN = model.vbn;
                if (!pathJTB.Equals(""))
                    model.vbn.readJointTable(pathJTB);
                if (!pathSB.Equals(""))
                    model.vbn.swingBones.Read(pathSB);
            }

            NUT nut = null;
            if (!pathNUT.Equals(""))
            {
                nut = new NUT(pathNUT);
                Runtime.TextureContainers.Add(nut);
            }

            if (!pathNUD.Equals(""))
            {
                model.nud = new NUD(pathNUD);

                foreach (string s in pacs)
                {
                    PAC p = new PAC();
                    p.Read(s);
                    byte[] data;
                    p.Files.TryGetValue("default.mta", out data);
                    if (data != null)
                    {
                        MTA m = new MTA();
                        m.read(new FileData(data));
                        model.nud.applyMTA(m, 0);
                    }
                }
            }

            if (!pathXMB.Equals(""))
            {
                model.xmb = new XMBFile(pathXMB);
                model.nud.SetPropertiesFromXMB(model.xmb);
            }

            if (!pathMTA.Equals(""))
            {
                try
                {
                    model.mta = new MTA();
                    model.mta.Read(pathMTA);
                    string mtaName = Path.Combine(Path.GetFileName(Path.GetDirectoryName(pathMTA)), Path.GetFileName(pathMTA));
                    Console.WriteLine($"MTA Name - {mtaName}");
                    addMaterialAnimation(mtaName, model.mta);
                }
                catch (EndOfStreamException)
                {
                    model.mta = null;
                }
            }

            if (!pathMOI.Equals(""))
            {
                model.moi = new MOI(pathMOI);
            }

            if (model.nud != null)
            {
                model.nud.MergePoly();
            }

            Runtime.ModelContainers.Add(model);
            List<ModelContainer> sortedModelContainers = Runtime.ModelContainers.OrderBy(o => (o.nud.drawingOrder)).ToList();
            Runtime.ModelContainers = sortedModelContainers;

            meshList.refresh();
            
            ModelNode n = new ModelNode();
            n.NUD.Model = model.nud;
            n.VBN.Skeleton = model.vbn;
            n.NUT.Texture = nut;

            project.treeView1.BeginUpdate();
            project.treeView1.Nodes.Add(n);
            project.treeView1.EndUpdate();
            project.treeView1.Refresh();
        }

        private void addMaterialAnimation(string name, MTA m)
        {
            if (!Runtime.MaterialAnimations.ContainsValue(m) && !Runtime.MaterialAnimations.ContainsKey(name))
                Runtime.MaterialAnimations.Add(name, m);
            viewports[0].loadMTA(m);
            mtaNode.Nodes.Add(name);
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Supported Formats|*.omo;*.anim;*.bch;*.chr0;*.smd;*.mta;*.pac;*.dat;*.xmb|" +
                             "Object Motion|*.omo|" +
                             "Maya Animation|*.anim|" +
                             "3DS BCH Animation|*.bch|" +
                             "NW4R Animation|*.chr0|" +
                             "Source Animation (SMD)|*.smd|" +
                             "Smash 4 Material Animation (MTA)|*.mta|" +
                             "All files(*.*)|*.*";

                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                    foreach (string filename in ofd.FileNames)
                        openAnimation(filename);

            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {

            /*if (Runtime.TargetVBN == null)
            {
                return;
            }*/

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Supported Files (.omo, .anim, .pac, .mta .smd)|*.omo;*.anim;*.pac;*.mta;*.smd|" +
                             "Maya Anim (.anim)|*.anim|" +
                             "Material Animation (.mta)|*.mta|" +
                             "Object Motion (.omo)|*.omo|" +
                             "OMO Pack (.pac)|*.pac|" +
                             "Source Animation (.smd)|*.smd|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    sfd.FileName = sfd.FileName;

                    if (sfd.FileName.EndsWith(".anim") & Runtime.TargetAnim != null)
                    {
                        if (Runtime.TargetAnim.Tag is AnimTrack)
                            ((AnimTrack)Runtime.TargetAnim.Tag).createANIM(sfd.FileName, Runtime.TargetVBN);
                        else
                            if (sfd.FileName.Contains("ALL.anim"))
                                foreach (string animName in Runtime.Animations.Keys)
                                {
                                    ANIM.CreateANIM(sfd.FileName.Replace("ALL.anim",animName + ".anim"), Runtime.Animations[animName], Runtime.TargetVBN);
                                }
                            else
                                ANIM.CreateANIM(sfd.FileName, Runtime.TargetAnim, Runtime.TargetVBN);
                        
                    }

                    if (sfd.FileName.EndsWith(".omo"))
                    {
                        if (Runtime.TargetAnim.Tag is FileData)
                        {
                            FileOutput o = new FileOutput();
                            o.writeBytes(((FileData)Runtime.TargetAnim.Tag).getSection(0,
                                ((FileData)Runtime.TargetAnim.Tag).size()));
                            o.save(sfd.FileName);
                        }
                        else
                            OMOOld.createOMO(Runtime.TargetAnim, Runtime.TargetVBN, sfd.FileName);
                    }


                    if (sfd.FileName.EndsWith(".smd"))
                    {
                        SMD.Save(Runtime.TargetAnim, Runtime.TargetVBN, sfd.FileName);
                    }

                    if (sfd.FileName.EndsWith(".pac"))
                    {
                        var pac = new PAC();
                        foreach (var anim in Runtime.Animations)
                        {
                            var bytes = OMOOld.CreateOMOFromAnimation(anim.Value, Runtime.TargetVBN);
                            if (Runtime.TargetAnim.Tag is FileData)
                                bytes = ((FileData)Runtime.TargetAnim.Tag).getSection(0,
                                    ((FileData)Runtime.TargetAnim.Tag).size());

                            pac.Files.Add(anim.Key, bytes);
                        }
                        pac.Save(sfd.FileName);
                    }

                    if (sfd.FileName.EndsWith(".mta"))
                    {
                        FileOutput f = new FileOutput();
                        f.writeBytes(Runtime.TargetMTA[0].Rebuild());
                        f.save(sfd.FileName);
                    }
                }
            }
        }

        private void hashMatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HashMatch();
            //boneTreePanel.treeRefresh();
        }

        public static void HashMatch()
        {
            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if (m.vbn != null)
                {
                    foreach (Bone bone in m.vbn.bones)
                    {
                        uint bi = 0;
                        Hashes.names.TryGetValue(bone.Text, out bi);
                        bone.boneId = bi;
                        if (bone.boneId == 0)
                            bone.boneId = Crc32.Compute(bone.Text);
                    }
                }

                if (m.dat_melee != null)
                {
                    foreach (Bone bone in m.dat_melee.bones.bones)
                    {
                        uint bi = 0;
                        Hashes.names.TryGetValue(bone.Text, out bi);
                        bone.boneId = bi;
                        if (bone.boneId == 0)
                            bone.boneId = Crc32.Compute(bone.Text);
                    }
                }
                if (m.bch != null)
                {
                    foreach (BCH_Model mod in m.bch.Models.Nodes)
                    {
                        foreach (Bone bone in mod.skeleton.bones)
                        {
                            for (int i = 0; i < Hashes.names.Count; i++)
                            {
                                uint bi = 0;
                                Hashes.names.TryGetValue(bone.Text, out bi);
                                bone.boneId = bi;
                            }
                        }
                    }
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var abt = new About())
            {
                abt.ShowDialog();
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            animNode.Nodes.Clear();
            mtaNode.Nodes.Clear();
            Runtime.SoundContainers.Clear();
            Runtime.Animations.Clear();
            Runtime.Animnames.Clear();
            Runtime.MaterialAnimations.Clear();
            Runtime.TargetVBN.reset();
            Runtime.acmdEditor.updateCrcList();
        }

        private void importToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog() { Filter = "Motion Table (.mtable)|*.mtable|All Files (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Runtime.Moveset = new MovesetManager(ofd.FileName);
                    Runtime.acmdEditor.updateCrcList();
                }
            }
        }

        private void animationPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            animList.Show(dockPanel1);
        }

        private void viewportWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (viewportWindowToolStripMenuItem.Checked == false)
            {
                var vp = new VBNViewport();
                viewports.Add(vp);
                AddDockedControl(vp);
                viewportWindowToolStripMenuItem.Checked = true;
            }

        }

        private void animationsWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (animationsWindowToolStripMenuItem.Checked)
                animList.Show(dockPanel1);
            else
                animList.Hide();
        }

        private void boneTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (boneTreeToolStripMenuItem.Checked)
                boneTreePanel.Show(dockPanel1);
            else
                boneTreePanel.Hide();
        }

        #endregion

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.TargetVBN.Endian = Endianness.Big;
            Runtime.TargetVBN.unk_1 = 1;
            Runtime.TargetVBN.unk_2 = 2;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.TargetVBN.Endian = Endianness.Little;
            Runtime.TargetVBN.unk_1 = 2;
            Runtime.TargetVBN.unk_2 = 1;
        }

        public void openMats(NUD.Polygon poly, string name)
        {
            (new NUDMaterialEditor(poly) { ShowHint = DockState.DockLeft, Text = name }).Show();
        }

        private void clearWorkspaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Runtime.killWorkspace = true;
            Runtime.ParamManager.Reset();
            hurtboxList.refresh();
            Runtime.Animnames.Clear();
            Runtime.clearMoveset();
            Lights.areaLights.Clear();
            Lights.lightMaps.Clear();
        }

        private void renderSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GUI.RenderSettings renderSettings = new GUI.RenderSettings();
            renderSettings.Show();
        }

        private void meshListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            meshList.refresh();
            AddDockedControl(meshList);
        }

        private void projectTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (project.DockState == DockState.Unknown)
                project = new ProjectTree();
            else
                project.Focus();
        }

        private void openCharacterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new FolderSelectDialog())
            {
                ofd.Title = "Character Folder";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    //project.openACMD($"{ofd.SelectedPath}\\script\\animcmd\\body\\motion.mtable",
                    //    $"{ofd.SelectedPath}\\motion");

                    string fighterName = new DirectoryInfo(ofd.SelectedPath).Name;
                    string[] dirs = Directory.GetDirectories(ofd.SelectedPath);

                    foreach (string s in dirs)
                    {
                        if (s.EndsWith("model"))
                        {
                            // load default model
                            openNud(s + "\\body\\c00\\model.nud");
                        }
                        if (s.EndsWith("motion"))
                        {
                            string[] anims = Directory.GetFiles(s + "\\body\\");
                            Array.Sort(anims, (a, b) =>
                            {
                                if (a.Contains("main.pac"))
                                    return -1;
                                if (b.Contains("main.pac"))
                                    return 1;

                                return 0;
                            }); //Sort files so main.pac is opened first
                            foreach (string a in anims)
                                openAnimation(a);
                            
                        }
                        if (s.EndsWith("script"))
                        {
                            if (File.Exists(s + "\\animcmd\\body\\motion.mtable"))
                            {
                                //openFile(s + "\\animcmd\\body\\motion.mtable");
                                Runtime.Moveset = new MovesetManager(s + "\\animcmd\\body\\motion.mtable");
                                Runtime.acmdEditor.updateCrcList();
                            }
                        }
                    }

                    resyncTargetVBN();

                    if (!String.IsNullOrEmpty(Runtime.paramDir))
                    {
                        // If they set the wrong dir, oh well
                        try
                        {
                            Runtime.ParamManager = new CharacterParamManager(Runtime.paramDir + $"\\fighter\\fighter_param_vl_{fighterName}.bin", fighterName);
                            hurtboxList.refresh();
                            Runtime.ParamManagerHelper = new PARAMEditor(Runtime.paramDir + $"\\fighter\\fighter_param_vl_{fighterName}.bin");
                            Runtime.ParamMoveNameIdMapping = Runtime.ParamManagerHelper.getMoveNameIdMapping();
                            Runtime.ModelContainers[0].name = fighterName;

                            // Model render size
                            ParamFile param = new ParamFile(Runtime.paramDir + "\\fighter\\fighter_param.bin");
                            ParamEntry[] characterParams = ((ParamGroup)param.Groups[0])[CharacterParamManager.FIGHTER_ID[fighterName]];
                            // index 44 is model_scale
                            Runtime.model_scale = Convert.ToSingle(characterParams[44].Value);
                        }
                        catch { }
                    }
                }
            }
        }

        private void saveNUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Runtime.ModelContainers.Count > 0)
            {
                string filename = "";
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Namco Universal Data|*.nud|All files(*.*)|*.*";
                DialogResult result = save.ShowDialog();

                if (result == DialogResult.OK)
                {
                    filename = save.FileName;
                    if (filename.EndsWith(".nud"))
                        if (Runtime.ModelContainers[0].dat_melee != null)
                        {
                            ModelContainer m = Runtime.ModelContainers[0].dat_melee.wrapToNUD();
                            m.nud.Save(filename);
                            m.vbn.Save(filename.Replace(".nud", ".vbn"));
                        }
                        /*else 
                        if(Runtime.ModelContainers[0].bch != null)
                        {
                            NUD m = Runtime.ModelContainers[0].bch.mbn.toNUD();
                            VBN v = Runtime.ModelContainers[0].bch.models[0].skeleton;
                            m.Save(filename);
                            v.Save(filename.Replace(".nud", ".vbn"));
                        }*/
                        else
                        {
                            foreach (ModelContainer c in Runtime.ModelContainers)
                            {
                                if (c.nud != null)
                                {
                                    Runtime.ModelContainers[0].nud.Save(filename);
                                    break;
                                }
                            }
                        }
                }
            }
        }

        private void collisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Runtime.TargetLVD == null)
                Runtime.TargetLVD = new LVD();
            Runtime.TargetLVD.collisions.Add(new Collision() { name = "COL_00_NewCollision", subname = "00_NewCollision" });
            lvdList.fillList();
        }

        private void spawnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Runtime.TargetLVD == null)
                Runtime.TargetLVD = new LVD();
            Runtime.TargetLVD.spawns.Add(new Spawn() { name = "START_00_NEW", subname = "00_NEW" });
            lvdList.fillList();

        }

        private void respawnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Runtime.TargetLVD == null)
                Runtime.TargetLVD = new LVD();
            Runtime.TargetLVD.respawns.Add(new Spawn() { name = "RESTART_00_NEW", subname = "00_NEW" });
            lvdList.fillList();
        }

        private void cameraBoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Runtime.TargetLVD == null)
                Runtime.TargetLVD = new LVD();
            Runtime.TargetLVD.cameraBounds.Add(new Bounds() { name = "CAMERA_00_NEW", subname = "00_NEW" });
            lvdList.fillList();

        }

        private void blastzonesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Runtime.TargetLVD == null)
                Runtime.TargetLVD = new LVD();
            Runtime.TargetLVD.blastzones.Add(new Bounds() { name = "DEATH_00_NEW", subname = "00_NEW" });
            lvdList.fillList();
        }

        private void itemSpawnerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Runtime.TargetLVD == null)
                Runtime.TargetLVD = new LVD();
            Runtime.TargetLVD.itemSpawns.Add(new ItemSpawner() { name = "ITEM_00_NEW", subname = "00_NEW" });
            lvdList.fillList();
        }

        private void generalPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Runtime.TargetLVD == null)
                Runtime.TargetLVD = new LVD();
            Runtime.TargetLVD.generalPoints.Add(new GeneralPoint() { name = "POINT_00_NEW", subname = "00_NEW" });
            lvdList.fillList();
        }

        private void pointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            return;
            //Disabled this because I'm implementing general points as separate from general shapes
            /*GeneralPoint g = new GeneralPoint() { name = "POINT_00_NEW", subname = "00_NEW" };
            Runtime.TargetLVD.generalShapes.Add(g);
            lvdList.fillList();*/
        }

        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GeneralShape g = new GeneralShape() { name = "RECT_00_NEW", subname = "00_NEW", type = 3 };
            Runtime.TargetLVD.generalShapes.Add(g);
            lvdList.fillList();
        }

        private void pathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GeneralShape g = new GeneralShape() { name = "PATH_00_NEW", subname = "00_NEW", type = 4 };
            Runtime.TargetLVD.generalShapes.Add(g);
            lvdList.fillList();
        }

        private void openNUTEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (nutEditor == null || nutEditor.IsDisposed)
            {
                nutEditor = new NUTEditor();
                nutEditor.Show();
            }
            else
            {
                nutEditor.BringToFront();
            }
            nutEditor.FillForm();
        }

        private void exportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (var ofd = new FolderSelectDialog())
            {
                ofd.Title = "Export ACMD Folder";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Runtime.Moveset.MotionTable.Export(ofd.SelectedPath + "\\motion.mtable");
                    Runtime.Moveset.Game.Export(ofd.SelectedPath + "\\game.bin");
                    Runtime.Moveset.Sound.Export(ofd.SelectedPath + "\\sound.bin");
                    Runtime.Moveset.Expression.Export(ofd.SelectedPath + "\\expression.bin");
                    Runtime.Moveset.Effect.Export(ofd.SelectedPath + "\\effect.bin");
                }
            }
        }

        private void deleteLVDEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvdList.deleteSelected();
        }

        private void openStageToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (var ofd = new FolderSelectDialog())
            {
                ofd.Title = "Stage Folder";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string stagePath = ofd.SelectedPath;
                    string modelPath = stagePath + "\\model\\";
                    string paramPath = stagePath + "\\param\\";
                    string animationPath = stagePath + "\\animation\\";
                    string renderPath = stagePath + "\\render\\";
                    List<string> nuds = new List<string>();

                    if (Directory.Exists(modelPath))
                    {
                        foreach (string d in Directory.GetDirectories(modelPath))
                        {
                            foreach (string f in Directory.GetFiles(d))
                            {
                                if (f.EndsWith(".nud"))
                                {
                                    openNud(f, Path.GetFileName(d));
                                }
                            }
                        }
                    }

                    if (Directory.Exists(paramPath))
                    {
                        Runtime.TargetLVD = null;
                        foreach (string fileName in Directory.GetFiles(paramPath))
                        {
                            if (Path.GetExtension(fileName).Equals(".lvd") && Runtime.TargetLVD == null)
                            {
                                Runtime.TargetLVD = new LVD(fileName);
                                lvdList.fillList();
                            }

                            if (fileName.EndsWith("stprm.bin"))
                            {
                                // should this always replace existing settings?
                                Runtime.stprmParam = new ParamFile(fileName);
                                Camera.SetCameraFromSTPRM(Runtime.stprmParam);
                            }
                        }
                    }

                    if (Directory.Exists(renderPath))
                    {
                        foreach (string fileName in Directory.GetFiles(renderPath))
                        {
                            if (fileName.EndsWith("light_set_param.bin"))
                            {
                                // should this always replace existing settings?
                                Runtime.lightSetParam = new ParamFile(fileName);
                                Lights.SetLightsFromLightSetParam(Runtime.lightSetParam);
                            }

                            if (fileName.EndsWith("area_light.xmb"))
                            {
                                Lights.CreateAreaLightsFromXMB(new XMBFile(fileName));
                            }

                            if (fileName.EndsWith("lightmap.xmb"))
                            {
                                Lights.CreateLightMapsFromXMB(new XMBFile(fileName));
                            }
                        }
                    }

                    if (Directory.Exists(animationPath))
                    {
                        foreach (string d in Directory.GetDirectories(animationPath))
                        {
                            foreach (string f in Directory.GetFiles(d))
                            {
                                if (f.EndsWith(".omo"))
                                {
                                    //Runtime.Animations.Add(f, );
                                    Animation a = OMOOld.read(new FileData(f));
                                    a.Text = f;
                                    animNode.Nodes.Add(a);
                                }
                                else if (f.EndsWith("path.bin"))
                                {
                                    Runtime.TargetPath = new PathBin();
                                    Runtime.TargetPath.Read(f);
                                }
                            }
                        }
                    }
                }
            }

        }

        //<summary>
        //Open an animation based on filename
        //</summary>
        public void openAnimation(string filename)
        {

            //Runtime.Animations.Clear();
            animList.treeView1.BeginUpdate();
            if (filename.EndsWith(".mta"))
            {
                MTA mta = new MTA();
                try
                {
                    mta.Read(filename);
                    Runtime.MaterialAnimations.Add(filename, mta);
                    mtaNode.Nodes.Add(filename);
                    MainForm.Instance.viewports[0].loadMTA(mta);
                    Runtime.TargetMTAString = filename;
                }
                catch (EndOfStreamException)
                {
                    mta = null;
                }
            }
            else if (filename.EndsWith(".smd"))
            {
                var anim = new Animation(filename);
                if (Runtime.TargetVBN == null)
                    Runtime.TargetVBN = new VBN();
                SMD.read(filename, anim, Runtime.TargetVBN);
                boneTreePanel.treeRefresh();
                //Runtime.Animations.Add(filename, anim);
                animNode.Nodes.Add(anim);
            }
            else if (filename.EndsWith(".pac"))
            {
                PAC p = new PAC();
                p.Read(filename);

                foreach (var pair in p.Files)
                {
                    if (pair.Key.EndsWith(".omo"))
                    {
                        var anim = OMOOld.read(new FileData(pair.Value));
                        string AnimName = Regex.Match(pair.Key, @"([A-Z][0-9][0-9])(.*)").Groups[0].ToString();
                        //AnimName = pair.Key;
                        //AnimName = AnimName.Remove(AnimName.Length - 4);
                        //AnimName = AnimName.Insert(3, "_");
                        if (!string.IsNullOrEmpty(AnimName))
                        {
                            Console.WriteLine("Adding " + AnimName);
                            anim.Text = AnimName;
                            if (Runtime.Animations.ContainsKey(AnimName))
                                Runtime.Animations[AnimName].Children.Add(anim);
                            else
                            {
                                animNode.Nodes.Add(anim);
                                Runtime.Animations.Add(AnimName, anim);
                            }

                            AddAnimName(AnimName.Substring(3).Replace(".omo",""));
                        }
                        else
                        {
                            if (Runtime.Animations.ContainsKey(pair.Key))
                                Runtime.Animations[pair.Key].Children.Add(anim);
                            else
                            {
                                animNode.Nodes.Add(pair.Key);
                                Runtime.Animations.Add(pair.Key, anim);
                            }
                        }
                        Runtime.acmdEditor.updateCrcList();
                    }
                    else if (pair.Key.EndsWith(".mta"))
                    {
                        MTA mta = new MTA();
                        try
                        {
                            if (!Runtime.MaterialAnimations.ContainsKey(pair.Key))
                            {
                                mta.read(new FileData(pair.Value));
                                Runtime.MaterialAnimations.Add(pair.Key, mta);
                                mtaNode.Nodes.Add(pair.Key);
                            }

                            // matching
                            string AnimName =
                                Regex.Match(pair.Key, @"([A-Z][0-9][0-9])(.*)").Groups[0].ToString()
                                    .Replace(".mta", ".omo");
                            if (Runtime.Animations.ContainsKey(AnimName))
                            {
                                Runtime.Animations[AnimName].Children.Add(mta);
                            }

                        }
                        catch (EndOfStreamException)
                        {
                            mta = null;
                        }
                    }
                }
            }

            if (filename.EndsWith(".dat"))
            {
                if (!filename.EndsWith("AJ.dat"))
                    MessageBox.Show("Not a DAT animation");
                else
                {
                    if (Runtime.ModelContainers[0].dat_melee == null)
                        MessageBox.Show("Load a DAT model first");
                    else
                        DAT_Animation.LoadAJ(filename, Runtime.ModelContainers[0].dat_melee.bones);
                }

            }
            //if (Runtime.TargetVBN.bones.Count > 0)
            //{
            if (filename.EndsWith(".omo"))
            {
                //Runtime.Animations.Add(filename, );
                Animation a = OMOOld.read(new FileData(filename));
                a.Text = filename;
                animNode.Nodes.Add(a);
            }
            if (filename.EndsWith(".chr0"))
            {
                //Runtime.Animations.Add(filename, CHR0.read(new FileData(filename), Runtime.TargetVBN));
                animNode.Nodes.Add(CHR0.read(new FileData(filename), Runtime.TargetVBN));
            }
            if (filename.EndsWith(".anim"))
            {
                //Runtime.Animations.Add(filename, ANIM.read(filename, Runtime.TargetVBN));
                animNode.Nodes.Add(ANIM.read(filename, Runtime.TargetVBN));
            }
            if (filename.EndsWith(".bch"))
            {
                //Runtime.Animations.Add(filename, );
                //animNode.Nodes.Add();
                BCHan.Read(filename);
            }

            animList.treeView1.EndUpdate();
        }

        private static void writeDatJobjPositions(TreeNode node, FileOutput f)
        {
            if(node.Tag is DAT.JOBJ)
            {
                DAT.JOBJ jobj = (DAT.JOBJ)node.Tag;
                f.writeFloatAt((float)jobj.pos.X, jobj.posOff);
                f.writeFloatAt((float)jobj.pos.Y, jobj.posOff + 4);
                f.writeFloatAt((float)jobj.pos.Z, jobj.posOff + 8);
            }
            foreach(TreeNode child in node.Nodes)
                writeDatJobjPositions(child, f);
        }

        ///<summary>
        /// Save file as if "Save" option was selected
        /// </summary>
        /// <param name="filename"> Filename of file to save</param>
        public void saveFile(string filename)
        {
            if (filename.EndsWith(".vbn"))
            {
                Runtime.TargetVBN.Endian = Endianness.Big;
                if (!checkBox1.Checked)
                    Runtime.TargetVBN.Endian = Endianness.Little;
                Runtime.TargetVBN.Save(filename);
            }

            if (filename.EndsWith(".lvd") && Runtime.TargetLVD != null)
                File.WriteAllBytes(filename, Runtime.TargetLVD.Rebuild());
            else if (filename.EndsWith(".lvd"))
            {
                DAT d = null;
                foreach (ModelContainer c in Runtime.ModelContainers)
                    if (c.dat_melee != null)
                        d = c.dat_melee;
                if (d != null)
                {
                    DialogResult r =
                        MessageBox.Show(
                            "Would you like to save in safe mode?\n(This is not suggested, only use when needed)",
                            "DAT -> LVD safe mode", MessageBoxButtons.YesNo);
                    if (r == DialogResult.Yes)
                    {
                        File.WriteAllBytes(filename, d.toLVD(true).Rebuild());
                    }
                    else if (r == DialogResult.No)
                    {
                        File.WriteAllBytes(filename, d.toLVD(false).Rebuild());
                    }
                }

            }

            if (filename.EndsWith(".dae"))
            {
                if (Runtime.ModelContainers.Count > 0)
                {
                    Collada.Save(filename, Runtime.ModelContainers[0]);
                }
            }

            if (filename.EndsWith(".dat"))
            {
                foreach(ModelContainer mc in Runtime.ModelContainers)
                {
                    if(mc.dat_melee != null)
                    {
                        FileOutput f = new FileOutput();
                        f.writeBytes(File.ReadAllBytes(mc.dat_melee.filename));

                        foreach (TreeNode node in mc.dat_melee.tree)
                            writeDatJobjPositions(node, f);

                        if (mc.dat_melee.spawns != null)
                        {
                            for (int i = 0; i < mc.dat_melee.spawns.Count; i++)
                            {
                                f.writeFloatAt(mc.dat_melee.spawns[i].x / mc.dat_melee.stageScale, mc.dat_melee.spawnOffs[i]);
                                f.writeFloatAt(mc.dat_melee.spawns[i].y / mc.dat_melee.stageScale, mc.dat_melee.spawnOffs[i] + 4);
                                f.writeFloatAt(0, mc.dat_melee.spawnOffs[i] + 8);
                            }
                        }

                        if (mc.dat_melee.respawns != null)
                        {
                            for(int i = 0; i < mc.dat_melee.respawns.Count; i++)
                            {
                                f.writeFloatAt(mc.dat_melee.respawns[i].x / mc.dat_melee.stageScale, mc.dat_melee.respawnOffs[i]);
                                f.writeFloatAt(mc.dat_melee.respawns[i].y / mc.dat_melee.stageScale, mc.dat_melee.respawnOffs[i] + 4);
                                f.writeFloatAt(0, mc.dat_melee.respawnOffs[i] + 8);
                            }
                        }

                        if (mc.dat_melee.itemSpawns != null)
                        {
                            for (int i = 0; i < mc.dat_melee.itemSpawns.Count; i++)
                            {
                                f.writeFloatAt(mc.dat_melee.itemSpawns[i].x / mc.dat_melee.stageScale, mc.dat_melee.itemSpawnOffs[i]);
                                f.writeFloatAt(mc.dat_melee.itemSpawns[i].y / mc.dat_melee.stageScale, mc.dat_melee.itemSpawnOffs[i] + 4);
                                f.writeFloatAt(0, mc.dat_melee.itemSpawnOffs[i] + 8);
                            }
                        }

                        if (mc.dat_melee.targets != null)
                        {
                            for (int i = 0; i < mc.dat_melee.targets.Count; i++)
                            {
                                f.writeFloatAt(mc.dat_melee.targets[i].x / mc.dat_melee.stageScale, mc.dat_melee.targetOffs[i]);
                                f.writeFloatAt(mc.dat_melee.targets[i].y / mc.dat_melee.stageScale, mc.dat_melee.targetOffs[i] + 4);
                                f.writeFloatAt(0, mc.dat_melee.targetOffs[i] + 8);
                            }
                        }

                        if (mc.dat_melee.blastzones != null)
                        {
                            f.writeFloatAt(mc.dat_melee.blastzones.left / mc.dat_melee.stageScale, mc.dat_melee.blastzoneOffs[0]);
                            f.writeFloatAt(mc.dat_melee.blastzones.top / mc.dat_melee.stageScale, mc.dat_melee.blastzoneOffs[0] + 4);
                            f.writeFloatAt(0, mc.dat_melee.blastzoneOffs[0] + 8);

                            f.writeFloatAt(mc.dat_melee.blastzones.right / mc.dat_melee.stageScale, mc.dat_melee.blastzoneOffs[1]);
                            f.writeFloatAt(mc.dat_melee.blastzones.bottom / mc.dat_melee.stageScale, mc.dat_melee.blastzoneOffs[1] + 4);
                            f.writeFloatAt(0, mc.dat_melee.blastzoneOffs[1] + 8);
                        }

                        if (mc.dat_melee.cameraBounds != null)
                        {
                            f.writeFloatAt(mc.dat_melee.cameraBounds.left / mc.dat_melee.stageScale, mc.dat_melee.cameraBoundOffs[0]);
                            f.writeFloatAt(mc.dat_melee.cameraBounds.top / mc.dat_melee.stageScale, mc.dat_melee.cameraBoundOffs[0] + 4);
                            f.writeFloatAt(0, mc.dat_melee.cameraBoundOffs[0] + 8);

                            f.writeFloatAt(mc.dat_melee.cameraBounds.right / mc.dat_melee.stageScale, mc.dat_melee.cameraBoundOffs[1]);
                            f.writeFloatAt(mc.dat_melee.cameraBounds.bottom / mc.dat_melee.stageScale, mc.dat_melee.cameraBoundOffs[1] + 4);
                            f.writeFloatAt(0, mc.dat_melee.cameraBoundOffs[1] + 8);
                        }
                        
                        if (MessageBox.Show("Overwrite collisions?","DAT Saving", MessageBoxButtons.YesNo) == DialogResult.Yes && mc.dat_melee.collisions != null)
                        {
                            while(f.pos() % 0x10 != 0)//get it back to being 0x10 alligned if it isn't already
                                f.writeByte(0);
                            
                            f.writeIntAt(f.pos() - 0x20, mc.dat_melee.collisions.vertOffOff);
                            f.writeIntAt(mc.dat_melee.collisions.vertices.Count, mc.dat_melee.collisions.vertOffOff + 4);
                            foreach(Vector2D vert in mc.dat_melee.collisions.vertices)
                            {
                                f.writeFloat(vert.x);
                                f.writeFloat(vert.y);
                            }
                            f.writeIntAt(f.pos() - 0x20, mc.dat_melee.collisions.linkOffOff);
                            f.writeIntAt(mc.dat_melee.collisions.links.Count, mc.dat_melee.collisions.linkOffOff + 4);
                            foreach(DAT.COLL_DATA.Link link in mc.dat_melee.collisions.links)
                            {
                                f.writeShort(link.vertexIndices[0]);
                                f.writeShort(link.vertexIndices[1]);
                                f.writeShort(link.connectors[0]);
                                f.writeShort(link.connectors[1]);
                                f.writeShort(link.idxVertFromLink);
                                f.writeShort(link.idxVertToLink);
                                f.writeShort(link.collisionAngle);
                                f.writeByte(link.flags);
                                f.writeByte(link.material);
                            }
                            f.writeIntAt(f.pos() - 0x20, mc.dat_melee.collisions.polyOffOff);
                            f.writeIntAt(mc.dat_melee.collisions.areaTable.Count, mc.dat_melee.collisions.polyOffOff + 4);
                            //Recalculate "area table" and write it to file
                            foreach (DAT.COLL_DATA.AreaTableEntry ate in mc.dat_melee.collisions.areaTable)
                            {
                                int ceilingCount = 0, floorCount = 0, leftWallCount = 0, rightWallCount = 0;
                                int firstCeiling = -1, firstFloor = -1, firstLeftWall = -1, firstRightWall = -1;
                                float lowX = float.MaxValue, highX = float.MinValue, lowY = float.MaxValue, highY = float.MinValue;
                                for (int i = ate.idxLowestSpot; i < ate.idxLowestSpot + ate.nbLinks && i < mc.dat_melee.collisions.links.Count; i++)
                                {
                                    DAT.COLL_DATA.Link link = mc.dat_melee.collisions.links[i];
                                    
                                    if ((link.collisionAngle & 4) != 0)//left wall
                                    {
                                        leftWallCount++;
                                        if (firstLeftWall == -1)
                                            firstLeftWall = i;
                                    }
                                    if ((link.collisionAngle & 8) != 0)//right wall
                                    {
                                        rightWallCount++;
                                        if (firstRightWall == -1)
                                            firstRightWall = i;
                                    }
                                    if ((link.collisionAngle & 1) != 0)//floor
                                    {
                                        floorCount++;
                                        if (firstFloor == -1)
                                            firstFloor = i;
                                    }
                                    if ((link.collisionAngle & 2) != 0)//ceiling
                                    {
                                        ceilingCount++;
                                        if (firstCeiling == -1)
                                            firstCeiling = i;
                                    }

                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[0]].x < lowX)
                                        lowX = mc.dat_melee.collisions.vertices[link.vertexIndices[0]].x;
                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[0]].x > highX)
                                        highX = mc.dat_melee.collisions.vertices[link.vertexIndices[0]].x;
                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[0]].y < lowY)
                                        lowY = mc.dat_melee.collisions.vertices[link.vertexIndices[0]].y;
                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[0]].y > highY)
                                        highY = mc.dat_melee.collisions.vertices[link.vertexIndices[0]].y;
                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[1]].x < lowX)
                                        lowX = mc.dat_melee.collisions.vertices[link.vertexIndices[1]].x;
                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[1]].x > highX)
                                        highX = mc.dat_melee.collisions.vertices[link.vertexIndices[1]].x;
                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[1]].y < lowY)
                                        lowY = mc.dat_melee.collisions.vertices[link.vertexIndices[1]].y;
                                    if (mc.dat_melee.collisions.vertices[link.vertexIndices[1]].y > highY)
                                        highY = mc.dat_melee.collisions.vertices[link.vertexIndices[1]].y;
                                }

                                if (firstCeiling == -1)
                                    firstCeiling = 0;
                                if (firstFloor == -1)
                                    firstFloor = 0;
                                if (firstLeftWall == -1)
                                    firstLeftWall = 0;
                                if (firstRightWall == -1)
                                    firstRightWall = 0;

                                f.writeShort(firstFloor);
                                f.writeShort(floorCount);
                                f.writeShort(firstCeiling);
                                f.writeShort(ceilingCount);
                                f.writeShort(firstLeftWall);
                                f.writeShort(leftWallCount);
                                f.writeShort(firstRightWall);
                                f.writeShort(rightWallCount);
                                f.writeInt(0);
                                f.writeFloat(lowX - 10);
                                f.writeFloat(lowY - 10);
                                f.writeFloat(highX + 10);
                                f.writeFloat(highY + 10);
                                f.writeShort(ate.idxLowestSpot);
                                f.writeShort(ate.nbLinks);
                            }
                        }
                        f.writeIntAt(f.pos(), 0);
                        f.save(filename);
                    }
                }
            }

            if (filename.EndsWith(".nud"))
            {
                if (Runtime.ModelContainers[0].dat_melee != null)
                {
                    ModelContainer m = Runtime.ModelContainers[0].dat_melee.wrapToNUD();
                    m.nud.Save(filename);
                    m.vbn.Save(filename.Replace(".nud", ".vbn"));
                }
                if (Runtime.ModelContainers[0].bch != null)
                {
                    //Runtime.ModelContainers[0].bch.mbn.toNUD().Save(filename);
                    //Runtime.ModelContainers[0].bch.models[0].skeleton.Save(filename.Replace(".nud", ".vbn"));
                }
            }
            if (filename.EndsWith(".mbn"))
            {
                if (Runtime.ModelContainers[0].nud != null)
                {
                    MBN m = Runtime.ModelContainers[0].nud.toMBN();
                    m.Save(filename);
                }
            }
        }

        ///<summary>
        ///Open a file based on the filename
        ///</summary>
        /// <param name="fileName"> Filename of file to open</param>
        public void openFile(string fileName)
        {
            if (!fileName.EndsWith(".mta") && !fileName.EndsWith(".dat") && !fileName.EndsWith(".smd"))
                openAnimation(fileName);

            if (fileName.EndsWith(".vbn"))
            {
                Runtime.TargetVBN = new VBN(fileName);

                ModelContainer con = resyncTargetVBN();
                if (Directory.Exists("Skapon\\"))
                {
                    NUD nud = Skapon.Create(Runtime.TargetVBN);
                    con.nud = nud;
                }
            }

            if (fileName.EndsWith(".sb"))
            {
                SB sb = new SB();
                sb.Read(fileName);
                SwagEditor swagEditor = new SwagEditor(sb) {ShowHint = DockState.DockRight};
                AddDockedControl(swagEditor);
                SwagEditors.Add(swagEditor);
            }

            if (fileName.EndsWith(".dat"))
            {
                if (fileName.EndsWith("AJ.dat"))
                {
                    MessageBox.Show("This is animation; load with Animation -> Import");
                    return;
                }
                DAT dat = new DAT();
                dat.filename = fileName;
                dat.Read(new FileData(fileName));
                ModelContainer c = new ModelContainer();
                Runtime.ModelContainers.Add(c);
                c.dat_melee = dat;
                dat.PreRender();

                HashMatch();

                Runtime.TargetVBN = dat.bones;
                if (dat.collisions != null)//if the dat is a stage
                {
                    DAT_stage_list stageList = new DAT_stage_list(dat) { ShowHint = DockState.DockLeft };
                    AddDockedControl(stageList);
                }
                DAT_TreeView p = new DAT_TreeView() {ShowHint = DockState.DockLeft};
                p.setDAT(dat);
                AddDockedControl(p);

                meshList.refresh();
                resyncTargetVBN();
            }

            if (fileName.EndsWith(".nut"))
            {
                Runtime.TextureContainers.Add(new NUT(fileName));
                if (nutEditor == null || nutEditor.IsDisposed)
                {
                    nutEditor = new NUTEditor();
                    nutEditor.Show();
                }
                else
                {
                    nutEditor.BringToFront();
                }
                nutEditor.FillForm();
            }

            if (fileName.EndsWith(".tex"))
            {
                if (texEditor == null || texEditor.IsDisposed)
                {
                    texEditor = new _3DSTexEditor();
                    texEditor.Show();
                }
                else
                {
                    texEditor.BringToFront();
                }
                texEditor.OpenTEX(fileName);
            }

            if (fileName.EndsWith(".lvd"))
            {
                if (Runtime.TargetLVD == null)
                    Runtime.TargetLVD = new LVD(fileName);
                else
                    Runtime.TargetLVD.Read(fileName);
                LVD test = Runtime.TargetLVD;
                lvdList.fillList();
            }
            
            if (fileName.EndsWith(".nus3bank"))
            {
                NUS3BANK nus = new NUS3BANK();
                nus.Read(fileName);
                Runtime.SoundContainers.Add(nus);
                if (nusEditor == null || nusEditor.IsDisposed)
                {
                    nusEditor = new NUS3BANKEditor();
                    nusEditor.Show();
                }
                else
                {
                    nusEditor.BringToFront();
                }
                nusEditor.FillForm();
            }

            if (fileName.EndsWith(".wav"))
            {
                WAVE wav = new WAVE();
                wav.Read(fileName);
            }

            if (fileName.EndsWith(".mta"))
            {
                MTA TargetMTA = new MTA();
                TargetMTA.Read(fileName);
                viewports[0].loadMTA(TargetMTA);
                Runtime.TargetMTA.Clear();
                Runtime.TargetMTA.Add(TargetMTA);
                MTAEditor temp = new MTAEditor(TargetMTA) {ShowHint = DockState.DockLeft};
                temp.Text = Path.GetFileName(fileName);
                AddDockedControl(temp);
                mtaEditors.Add(temp);
            }

            if (fileName.EndsWith(".mtable"))
            {
                Runtime.Moveset = new MovesetManager(fileName);
                Runtime.acmdEditor.updateCrcList();
            }
            if (fileName.EndsWith(".atkd"))
            {
                AddDockedControl(new ATKD_Editor(new ATKD().Read(fileName)));
            }
            if (fileName.EndsWith("path.bin"))
            {
                Runtime.TargetPath = new PathBin(fileName);
            }
            else if (fileName.EndsWith(".bin"))
            {
                FileData f = new FileData(fileName);
                if(f.readShort() == 0xFFFF)
                {
                    PARAMEditor p = new PARAMEditor(fileName) { ShowHint = DockState.Document };
                    p.Text = Path.GetFileName(fileName);
                    AddDockedControl(p);
                    paramEditors.Add(p);

                    if (fileName.EndsWith("light_set_param.bin"))
                    {
                        Runtime.lightSetParam = new ParamFile(fileName);
                        Lights.SetLightsFromLightSetParam(Runtime.lightSetParam);
                    }

                    if (fileName.EndsWith("stprm.bin"))
                    {
                        // should this always replace existing settings?
                        Runtime.stprmParam = new ParamFile(fileName);
                        Camera.SetCameraFromSTPRM(Runtime.stprmParam);
                    }

                }
                else if (f.readString(4,4) == "PATH")
                {
                    Runtime.TargetPath = new PathBin(fileName);
                }
                else if (f.readString(0,4) == "ATKD")
                {
                    AddDockedControl(new ATKD_Editor(new ATKD().Read(fileName)));
                }
                else
                {
                    Runtime.TargetCMR0 = new CMR0();
                    Runtime.TargetCMR0.read(new FileData(fileName));
                }
            }

            if (fileName.EndsWith(".mdl0"))
            {
                MDL0Bones mdl0 = new MDL0Bones();
                Runtime.TargetVBN = mdl0.GetVBN(new FileData(fileName));

                resyncTargetVBN();
            }

            if (fileName.EndsWith(".smd"))
            {
                Runtime.TargetVBN = new VBN();
                SMD.read(fileName, new Animation(fileName), Runtime.TargetVBN);

                ModelContainer m = resyncTargetVBN();
                if (m != null)
                {
                    m.nud = SMD.toNUD(fileName);
                    meshList.refresh();
                }
            }

            if (fileName.ToLower().EndsWith(".dae"))
            {
                DAEImportSettings m = new DAEImportSettings();
                m.ShowDialog();
                if (m.exitStatus == DAEImportSettings.Opened)
                {
                    ModelContainer con = new ModelContainer();

                    // load vbn
                    con.vbn = m.getVBN();

                    Collada.DAEtoNUD(fileName, con, m.checkBox5.Checked);
                    Runtime.ModelContainers.Add(con);

                    Runtime.TargetVBN = con.vbn;
                    resyncTargetVBN();

                    // apply settings
                    m.Apply(con.nud);
                    con.nud.MergePoly();

                    meshList.refresh();
                }
            }

            if (fileName.EndsWith("area_light.xmb"))
            {
                Lights.CreateAreaLightsFromXMB(new XMBFile(fileName));
            }

            if (fileName.EndsWith("lightmap.xmb"))
            {
                Lights.CreateLightMapsFromXMB(new XMBFile(fileName));
            }

            if (fileName.ToLower().EndsWith(".obj"))
            {
                ModelViewport vp = new ModelViewport();
                OBJ obj = new OBJ();
                obj.Read(fileName);
                vp.draw.Add(new ModelContainer() { nud = obj.toNUD() });
                Runtime.ModelContainers.Add(new ModelContainer() { nud = obj.toNUD() });
                meshList.refresh();
                AddDockedControl(vp);
            }

            if (fileName.EndsWith(".mbn"))
            {
                /*MBN m = new MBN();
                m.Read(fileName);
                ModelContainer con = new ModelContainer();
                BCH b = new BCH();
                con.bch = b;
                b.mbn = m;
                b.Read(fileName.Replace(".mbn", ".bch"));
                Runtime.ModelContainers.Add(con);
                Runtime.TargetVBN = b.bones;
                resyncTargetVBN();
                meshList.refresh();*/

                ModelContainer con = new ModelContainer();
                BCH b = new BCH();
                b.Read(fileName.Replace(".mbn", ".bch"));
                ((BCH_Model)b.Models.Nodes[0]).OpenMBN(new FileData(fileName));
                con.bch = b;
                Runtime.ModelContainers.Add(con);
                meshList.refresh();
            }

            if (fileName.EndsWith(".bch"))
            {
            }

            if (fileName.EndsWith(".nud"))
            {
                openNud(fileName);
                resyncTargetVBN();
            }

            if (fileName.EndsWith(".moi"))
            {
                MOI moi = new MOI(fileName);
                AddDockedControl(new MOIEditor(moi) {ShowHint = DockState.DockRight});
            }

            if (fileName.EndsWith(".drp"))
            {
                DRP drp = new DRP(fileName);
                DRPViewer v = new DRPViewer();
                v.treeView1.Nodes.Add(drp);
                v.Show();
                //project.treeView1.Nodes.Add(drp);
                //project.treeView1.Invalidate();
                //project.treeView1.Refresh();
            }

            if (fileName.EndsWith(".wrkspc"))
            {
                Workspace = new WorkspaceManager(project);
                Workspace.OpenWorkspace(fileName);
            }

            // Don't want to mess up the project tree if we
            // just set it up already
            if (!fileName.EndsWith(".wrkspc"))
                project.fillTree();
        }

        private ModelContainer resyncTargetVBN()
        {
            ModelContainer modelContainer = null;
            if (Runtime.TargetVBN != null)
            {
                // Make sure the TargetVBN is in use *somewhere* in our models
                bool found = false;
                foreach (ModelContainer m in Runtime.ModelContainers)
                {
                    if (m.vbn != null)
                    if (m.vbn.essentialComparison(Runtime.TargetVBN))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    modelContainer = new ModelContainer();
                    modelContainer.vbn = Runtime.TargetVBN;
                    Runtime.ModelContainers.Add(modelContainer);
                }
            }
            else
            {
                // Fetch the TargetVBN from the first model we come across
                foreach (ModelContainer m in Runtime.ModelContainers)
                {
                    if (m.vbn != null)
                    {
                        // Use the first VBN we find
                        Runtime.TargetVBN = Runtime.ModelContainers[0].vbn;
                        modelContainer = m;
                        break;
                    }
                }
            }
            boneTreePanel.treeRefresh();
            return modelContainer;
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter =
                    "Supported Formats(.vbn, .mdl0, .smd, .nud, .lvd, .bin, .dae, .mta, .wrkspc, .mbn)|*.vbn;*.mdl0;*.smd;*.lvd;*.nud;*.xmb;*.mtable;*.bin;*.dae;*.obj;*.dat;*.mta;*.wrkspc;*.nut;*.sb;*.mbn;*.tex;*.drp;*.nus3bank;*.wav|" +
                    "Smash 4 Boneset (.vbn)|*.vbn|" +
                    "Namco Model (.nud)|*.nud|" +
                    "Smash 4 Level Data (.lvd)|*.lvd|" +
                    "NW4R Model (.mdl0)|*.mdl0|" +
                    "Source Model (.SMD)|*.smd|" +
                    "Smash 4 Parameters (.bin)|*.bin|" +
                    "Collada Model Format (.dae)|*.dae|" +
                    "Wavefront Object (.obj)|*.obj|" +
                    "All files(*.*)|*.*";

                ofd.Multiselect = true;
                // "Namco Universal Data Folder (.NUD)|*.nud|" +

                if (ofd.ShowDialog() == DialogResult.OK)
                    foreach (string filename in ofd.FileNames)
                        openFile(filename);
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !Text.Equals("Meteor Preview"))
            {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                foreach (string filePath in files)
                {
                    openFile(filePath);
                }
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !Text.Equals("Meteor Preview"))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (Smash_Forge.Update.Downloaded &&
                MessageBox.Show(
                    $"Would you like to download the following update?\n{Smash_Forge.Update.DownloadedRelease.Name}\n{Smash_Forge.Update.DownloadedRelease.Body}",
                    "Smash Forge Updater", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Process p = new Process();
                p.StartInfo.FileName = Path.Combine(executableDir, "updater/ForgeUpdater.exe");
                p.StartInfo.WorkingDirectory = Path.Combine(executableDir, "updater/");
                p.StartInfo.Arguments = "-i -r";
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                System.Windows.Forms.Application.Exit();
            }
        }

        private void mergeModelsMeshListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            meshList.mergeModel();
        }

        private void mergeBonesBoneListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //stub
            //low key probably not going to make this feature
        }

        private void saveAsDAEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sfd = new FolderSelectDialog())
            {
                sfd.Title = "Model Save Folder";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string folder = sfd.SelectedPath;
                    string model = Path.Combine(folder, "model.dae");
                    if (Runtime.ModelContainers.Count > 0)
                    {
                        Collada.Save(model, Runtime.ModelContainers[0]);
                        if (Runtime.ModelContainers[0].nud != null)
                        {
                            NUD nud = Runtime.ModelContainers[0].nud;
                            List<int> texIds = nud.GetTexIds();

                            foreach (var nut in Runtime.TextureContainers)
                            {
                                foreach (var tex in nut.textures)
                                {
                                    if (texIds.Contains(tex.id))
                                    {
                                        DDS dds = new DDS();
                                        dds.fromNUT_Texture(tex);
                                        dds.Save(Path.Combine(folder, $"Tex_0x{tex.id.ToString("X")}.png"));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        private void allViewsPreset(object sender, EventArgs e)
        {
            Runtime.acmdEditor = new ACMDPreviewEditor() { ShowHint = DockState.DockRight };
            Runtime.hitboxList = new HitboxList() { ShowHint = DockState.DockLeft };
            Runtime.variableViewer = new VariableList() { ShowHint = DockState.DockLeft };
            animList.ShowHint = DockState.DockRight;
            boneTreePanel.ShowHint = DockState.DockLeft;
            project.ShowHint = DockState.DockLeft;
            lvdList.ShowHint = DockState.DockLeft;
            lvdEditor.ShowHint = DockState.DockRight;
            Runtime.acmdEditor.ShowHint = DockState.DockLeft;
            meshList.ShowHint = DockState.DockRight;
            Runtime.hitboxList.ShowHint = DockState.DockLeft;
            Runtime.variableViewer.ShowHint = DockState.DockLeft;

            

            AddDockedControl(Runtime.acmdEditor);
            AddDockedControl(boneTreePanel);
            AddDockedControl(animList);
            AddDockedControl(lvdEditor);
            AddDockedControl(lvdList);
            AddDockedControl(Runtime.variableViewer);
            AddDockedControl(Runtime.hitboxList);
            AddDockedControl(hurtboxList);
            AddDockedControl(project);
            AddDockedControl(meshList);
        }

        private void modelViewPreset(object sender, EventArgs e)
        {
            animList.Hide();
            boneTreePanel.ShowHint = DockState.DockLeft;
            project.Hide();
            lvdList.Hide();
            lvdEditor.Hide();
            RegenPanels();
            meshList.ShowHint = DockState.DockRight;
            Runtime.acmdEditor.ShowHint = DockState.Hidden;
            AddDockedControl(boneTreePanel);
            AddDockedControl(meshList);
        }

        private void movesetModdingPreset(object sender, EventArgs e)
        {
            animList.ShowHint = DockState.DockLeft;
            boneTreePanel.Hide();
            project.Hide();
            lvdList.Hide();
            lvdEditor.Hide();
            meshList.Hide();
            RegenPanels();
            Runtime.acmdEditor.ShowHint = DockState.Float;
            AddDockedControl(Runtime.acmdEditor);
            AddDockedControl(animList);
        }

        private void stageWorkPreset(object sender, EventArgs e)
        {
            animList.Hide();
            boneTreePanel.ShowHint = DockState.DockLeft;
            project.Hide();
            lvdList.ShowHint = DockState.DockLeft;
            lvdEditor.ShowHint = DockState.DockRight;
            Runtime.acmdEditor.Hide();
            meshList.ShowHint = DockState.DockRight;
            RegenPanels();
            AddDockedControl(boneTreePanel);
            AddDockedControl(meshList);
            AddDockedControl(lvdEditor);
            AddDockedControl(lvdList);
        }

        private void cleanPreset(object sender, EventArgs e)
        {
            animList.Hide();
            boneTreePanel.Hide();
            project.Hide();
            lvdList.Hide();
            lvdEditor.Hide();
            Runtime.acmdEditor.Hide();
            meshList.Hide();
            RegenPanels();
        }

        private void superCleanPreset(object sender, EventArgs e)
        {
            animList.Hide();
            boneTreePanel.Hide();
            project.Hide();
            lvdList.Hide();
            lvdEditor.Hide();
            Runtime.acmdEditor.Hide();
            meshList.Hide();
            //RegenPanels();
            Runtime.acmdEditor.Hide();
            Runtime.hitboxList.Hide();
            hurtboxList.Hide();
            Runtime.variableViewer.Hide();
            viewports[0].groupBox2.Visible = false;
        }

        private void open3DSTEXEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (texEditor == null || texEditor.IsDisposed)
            {
                texEditor = new _3DSTexEditor();
                texEditor.Show();
            }
            else
            {
                texEditor.BringToFront();
            }
        }

        private void MainForm_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void editVerticesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Runtime.ViewportMode == Runtime.ViewportModes.NORMAL) Runtime.ViewportMode = Runtime.ViewportModes.EDITVERT; else
            if (Runtime.ViewportMode == Runtime.ViewportModes.EDITVERT) Runtime.ViewportMode = Runtime.ViewportModes.NORMAL;
        }

        private void exportErrorLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (string key in Runtime.shaders.Keys)
            {
                Runtime.shaders[key].SaveErrorLog(key);
            }

            MessageBox.Show("Error logs saved to Forge directory");
        }

        private void nESROMInjectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ROM_Injector r = new ROM_Injector();
            r.ShowDialog();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = false;
        }

        private void importParamsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog() { Filter = "Character fighter_param_vl file (.bin)|*.bin|All Files (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Runtime.ParamManager = new CharacterParamManager(ofd.FileName);
                    hurtboxList.refresh();
                }
            }
        }

        private void clearParamsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Runtime.ParamManager.Reset();
            hurtboxList.refresh();
        }

        private void openDATTextureEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DAT datToOpen = null;
            foreach (ModelContainer mc in Runtime.ModelContainers)
                if (mc.dat_melee != null)
                    datToOpen = mc.dat_melee;
            if(datToOpen != null)
                AddDockedControl(new DatTexEditor(datToOpen));
        }

        private void saveConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Runtime.SaveConfig();
        }

        private void cameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cameraForm == null || cameraForm.IsDisposed)
            {
                cameraForm = new CameraSettings();
                viewports[0].cameraPosForm = cameraForm;
            }
            cameraForm.Show();
        }

        private void AddAnimName(string AnimName)
        {
            uint crc = Crc32.Compute(AnimName.ToLower());
            if (Runtime.Animnames.ContainsValue(AnimName) || Runtime.Animnames.ContainsKey(crc))
                return;

            Runtime.Animnames.Add(crc, AnimName);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Runtime.clearMoveset();
        }

        private void exportParamsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Runtime.ParamManager.param == null)
                return;

            using (var sfd = new SaveFileDialog())
            {
                sfd.FileName = "fighter_param_vl_";
                sfd.Filter = "Fighter parameter file (*.bin)|*.bin|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Runtime.ParamManager.param.Export(sfd.FileName);
                    }catch
                    {
                        
                    }
                }
            }
        }

        private void stageLightingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StageLighting stageLightForm = new StageLighting();
            stageLightForm.Show();
        }
    }
}
