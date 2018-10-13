using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security.Cryptography;
using WeifenLuo.WinFormsUI.Docking;
using System.Diagnostics;
using System.Threading;
using Smash_Forge.GUI.Menus;
using Smash_Forge.GUI.Editors;
using SALT.PARAMS;
using SALT.Graphics;
using System.ComponentModel;
using Smash_Forge.Rendering;
using Smash_Forge.Rendering.Lights;
using System.Text.RegularExpressions;
using Smash_Forge.Filetypes.Melee;
using Smash_Forge.Filetypes.Melee.Utils;

namespace Smash_Forge
{
    public partial class MainForm : FormBase
    {
        public static MainForm Instance
        {
            get { return _instance != null ? _instance : (_instance = new MainForm()); }
        }

        private static MainForm _instance;

        public static string executableDir = null;
        public static csvHashes Hashes;
        public static DockPanel dockPanel;

        public WorkspaceManager Workspace { get; set; }
        public String[] filesToOpen = null;
        public ProgressAlert Progress = new ProgressAlert();

        // Lists
        public AnimListPanel animList = new AnimListPanel() { ShowHint = DockState.DockRight };
        public ProjectTree project = new ProjectTree() { ShowHint = DockState.DockLeft };
        public LVDList lvdList = new LVDList() { ShowHint = DockState.DockLeft };
        public ByamlList byamlList = new ByamlList() { ShowHint = DockState.DockLeft };
        public MeshList meshList = new MeshList() { ShowHint = DockState.DockRight };
        public HurtboxList hurtboxList = new HurtboxList() { ShowHint = DockState.DockLeft };
        public LMList lmList = new LMList() { ShowHint = DockState.DockLeft };

        // Editors and Forms
        public NUTEditor nutEditor = null;
        public NUS3BANKEditor nusEditor = null;
        public _3DSTexEditor texEditor = null;
        public CameraSettings cameraForm = null;
        public LVDEditor lvdEditor = new LVDEditor() { ShowHint = DockState.DockLeft };
        public ByamlEditor byamlEditor = new ByamlEditor() { ShowHint = DockState.DockLeft };
        public BfresMaterialEditor bfresMatEditor = new BfresMaterialEditor() { ShowHint = DockState.DockLeft };

        public List<PARAMEditor> paramEditors = new List<PARAMEditor>() { };
        public List<MTAEditor> mtaEditors = new List<MTAEditor>() { };
        public List<ACMDEditor> ACMDEditors = new List<ACMDEditor>() { };
        public List<SwagEditor> SwagEditors = new List<SwagEditor>() { };

        public MainForm()
        {
            InitializeComponent();
        }

        public void UpdateProgress(object sender, ProgressChangedEventArgs e)
        {
            Progress.ProgressValue = e.ProgressPercentage;
        }

        private void AppIdle(object sender, EventArgs e)
        {
            if (Smash_Forge.Update.Downloaded && Instance.greenArrowPictureBox.Image == null)
                Instance.greenArrowPictureBox.Image = Resources.Resources.sexy_green_down_arrow;
            DiscordSettings.Update();
        }

        ~MainForm()
        {
            Application.Idle -= AppIdle;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            DiscordSettings.startTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            dockPanel = dockPanel1;
            DiscordSettings.DiscordController = new DiscordController();

            DiscordSettings.DiscordController.Initialize();
            DiscordSettings.Update();

            ThreadStart t = new ThreadStart(Smash_Forge.Update.CheckLatest);
            Thread thread = new Thread(t);
            thread.Start();

            if (File.Exists(Path.Combine(executableDir, "version.txt")))
                Text = "Smash Forge | Build: " + File.ReadAllText(Path.Combine(executableDir, "version.txt"));

            Application.Idle += AppIdle;

            allViewsPreset(new Object(), new EventArgs());

            Hashes = new csvHashes(Path.Combine(executableDir, "hashTable.csv"));

            Config.StartupFromFile(MainForm.executableDir + "\\config.xml");
            DiscordSettings.Update();

            // Make sure everything is loaded before opening files.
            OpenTKSharedResources.InitializeSharedResources();
            if (OpenTKSharedResources.SetupStatus == OpenTKSharedResources.SharedResourceStatus.Failed)
            {
                // Disable options that would cause crashes.
                reloadShadersToolStripMenuItem.Enabled = false;
                exportErrorLogToolStripMenuItem.Enabled = false;
            }

            openFiles();
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
                        OpenNud(nud);
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
            DiscordRpc.Shutdown();
        }

        public void AddDockedControl(DockContent content)
        {
            if (dockPanel1.DocumentStyle == DocumentStyle.SystemMdi)
            {
                content.MdiParent = this;
                content.Show();
            }
            else if (content != null && dockPanel1 != null)
                content.Show(dockPanel1);
        }

        public ModelViewport GetActiveModelViewport()
        {
            // Find the selected modelviewport (there may be many).
            foreach (var c in dockPanel1.Contents)
            {
                if (c is ModelViewport && c == dockPanel1.ActiveDocument)
                    return (ModelViewport)c;
            }
            return null;
        }

        private void RegenPanels()
        {
            if (animList.IsDisposed)
            {
                animList = new AnimListPanel();
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
            if (lmList.IsDisposed)
            {
                lmList = new LMList();
                lmList.fillList();
            }
            if (byamlList.IsDisposed)
            {
                byamlList = new ByamlList();
                byamlList.fillList();
            }
            if (lvdEditor.IsDisposed)
            {
                lvdEditor = new LVDEditor();
            }
            if (bfresMatEditor.IsDisposed)
            {
                bfresMatEditor = new BfresMaterialEditor();
            }
            if (byamlEditor.IsDisposed)
            {
                byamlEditor = new ByamlEditor();
            }
            if (meshList.IsDisposed)
            {
                meshList = new MeshList();
                meshList.RefreshNodes();
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

        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dockPanel1.ActiveContent is EditorBase)
            {
                ((EditorBase)dockPanel1.ActiveContent).Save();
                return;
            }
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
                currentParam.SaveAs();
            else if (currentACMD != null)
                currentACMD.save();
            else if (currentSwagEditor != null)
                currentSwagEditor.save();
            else
            {
                string filename = "";
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Supported Filetypes (VBN,LVD,DAE,DAT)|*.vbn;*.lvd;*.dae;*.dat;*.byaml*.byml;;|Smash 4 Boneset|*.vbn|All files(*.*)|*.*";
                DialogResult result = save.ShowDialog();

                if (result == DialogResult.OK)
                {
                    filename = save.FileName;
                    saveFile(filename);
                }
            }
        }
        string pathNUT = "";

        public ModelViewport OpenNud(string pathNud, string viewportTitle = "", ModelViewport mvp = null)
        {
            // All the model files will be in the same directory as the model.nud file.
            string[] files = Directory.GetFiles(Path.GetDirectoryName(pathNud));

            string pathJTB = "";
            string pathVBN = "";
            string pathMTA = "";
            string pathSB = "";
            string pathMOI = "";
            string pathXMB = "";
            List<string> pacs = new List<string>();

            foreach (string fPath in files)
            {
                string fName = Path.GetFileName(fPath);
                if (fName.EndsWith(".nut"))
                    pathNUT = fPath;
                if (fName.EndsWith(".vbn") || fName.StartsWith("bindpose"))
                    pathVBN = fPath;
                if (fName.EndsWith(".jtb"))
                    pathJTB = fPath;
                if (fName.EndsWith(".mta"))
                    pathMTA = fPath;
                if (fName.EndsWith(".sb") /*|| fName.StartsWith("swingbone")*/)
                    pathSB = fPath;
                if (fName.EndsWith(".moi"))
                    pathMOI = fPath;
                if (fName.EndsWith(".pac"))
                    pacs.Add(fPath);
                if (fName.EndsWith("xmb"))
                    pathXMB = fPath;
            }

            if (mvp == null)
            {
                mvp = new ModelViewport();
                AddDockedControl(mvp);
            }

            ModelContainer modelContainer = new ModelContainer();
            mvp.draw.Add(modelContainer);
            modelContainer.Text = viewportTitle;
            mvp.Text = viewportTitle;

            modelContainer.NUD = new NUD(pathNud);
            if (modelContainer.NUD != null)
                modelContainer.NUD.MergePoly();

            OpenSkeleton(pathJTB, pathVBN, pathSB, modelContainer);
            OpenNut(pathNUT, modelContainer);
            OpenPacs(pacs, modelContainer);
            OpenModelXmb(pathXMB, modelContainer);
            OpenMta(pathMTA, modelContainer);
            OpenMoi(pathMOI, modelContainer);

            // Reset the camera. 
            mvp.FrameSelectionAndSort();
            return mvp;
        }

        public ModelViewport OpenKcl(byte[] fileData, string fileName, string viewportTitle = "", ModelViewport mvp = null)
        {
            if (mvp == null)
            {
                mvp = new ModelViewport();
                AddDockedControl(mvp);
            }

            ModelContainer modelContainer = new ModelContainer();
            mvp.draw.Add(modelContainer);
            modelContainer.Text = fileName;
            mvp.Text = fileName;

            modelContainer.Kcl = new KCL(fileData);

            return mvp;
        }

        public ModelViewport OpenMeleeDat(byte[] fileData, string fileName, string viewportTitle = "", ModelViewport mvp = null)
        {
            //OldStage Stuff
            {
                /*DAT dat = new DAT();
                dat.filename = fileName;
                dat.Read(new FileData(fileName));

                if (dat.collisions != null)//if the dat is a stage
                {
                    DatStageList stageList = new DatStageList(dat) { ShowHint = DockState.DockLeft };
                    AddDockedControl(stageList);
                }*/
            }

            if (mvp == null)
            {
                mvp = new ModelViewport();
                AddDockedControl(mvp);
            }

            //ModelContainer modelContainer = new ModelContainer();

            //modelContainer.MeleeData = new MeleeDataNode(fileName);

            MeleeDataNode n = new MeleeDataNode(fileName) { Text = Path.GetFileName(fileName) };
            if (Regex.Match(Path.GetFileName(fileName), "Pl[A-Z][a-z]\\.dat").Success)
            {
                string animationfileName = fileName.Replace(".dat", "AJ.dat");
                n.LoadPlayerAJ(animationfileName);
            }

            mvp.draw.Add(n);
            //modelContainer.Text = fileName;
            mvp.Text = fileName;

            return mvp;
        }

        public ModelViewport OpenBfres(byte[] fileData, string fileName, string viewportTitle = "", ModelViewport mvp = null)
        {
            //Todo. Support loading bfres texture and animations if seperate and in same directory
            if (mvp == null)
            {
                Console.WriteLine("Creating new viewport");

                mvp = new ModelViewport();
                AddDockedControl(mvp);
            }

            ModelContainer modelContainer = new ModelContainer();
            mvp.draw.Add(modelContainer);
            modelContainer.Text = fileName;
            mvp.Text = fileName;


            modelContainer.Bfres = new BFRES(fileName, fileData);

            modelContainer.BNTX = modelContainer.Bfres.getBntx();

            if (modelContainer.BNTX != null)
                Runtime.BNTXList.Add(modelContainer.BNTX);

            if (modelContainer.Bfres.models.Count != 0)
            {
                Runtime.TargetVBN = modelContainer.Bfres.models[0].skeleton;
                resyncTargetVBN();
            }

            if (Runtime.BNTXList.Count > 0 || Runtime.FTEXContainerList.Count > 0)
                Runtime.glTexturesNeedRefreshing = true;

            if (modelContainer.Bfres.AnimationCountTotal != 0)
            {
                if (dockPanel1.ActiveContent is ModelViewport)
                {
                    mvp = (ModelViewport)dockPanel1.ActiveContent;

                    AnimationGroupNode anim = new AnimationGroupNode();
                    anim.Text = Path.GetFileName(fileName);

                    if (modelContainer.Bfres.FSKACount != 0)
                    {
                        BFRES.FSKA fska = new BFRES.FSKA();
                        fska.Read(modelContainer.Bfres.TargetWiiUBFRES, anim, modelContainer.Bfres.TargetSwitchBFRES);
                    }

                    if (modelContainer.Bfres.FSHUCount != 0)
                    {
                        BFRES.FSHU fshu = new BFRES.FSHU();
                        fshu.Read(modelContainer.Bfres.TargetWiiUBFRES, anim, modelContainer);
                    }

                    if (modelContainer.Bfres.FMAACount != 0)
                    {
                        BFRES.FMAA fmaa = new BFRES.FMAA();
                        fmaa.Read(modelContainer.Bfres.TargetSwitchBFRES, anim, modelContainer);
                    }
                    if (modelContainer.Bfres.FTXPCount != 0)
                    {
                        BFRES.FTXP ftxp = new BFRES.FTXP();
                        ftxp.Read(modelContainer.Bfres.TargetWiiUBFRES, anim, modelContainer);
                    }
                    if (modelContainer.Bfres.FSHACount != 0)
                    {
                        BFRES.FSHA fsha = new BFRES.FSHA();
                        fsha.Read(modelContainer.Bfres.TargetSwitchBFRES, anim, modelContainer);
                    }
                    if (modelContainer.Bfres.FVISCount != 0)
                    {
                        BFRES.FVIS fvis = new BFRES.FVIS();
                        fvis.Read(modelContainer.Bfres.TargetSwitchBFRES, anim, modelContainer);

                    }
                    mvp.animListPanel.treeView1.Nodes.Add(anim);
                }
            }

            // Reset the camera. 
            mvp.FrameSelectionAndSort();

            return mvp;
        }

        private static void OpenPacs(List<string> pacs, ModelContainer modelContainer)
        {
            foreach (string s in pacs)
            {
                PAC p = new PAC();
                p.Read(s);
                byte[] data;
                if (p.Files.TryGetValue("display", out data))
                {
                    MTA mta = new MTA();
                    mta.read(new FileData(data));
                    modelContainer.NUD.ApplyMta(mta, 0);
                }
                if (p.Files.TryGetValue("default.mta", out data))
                {
                    MTA mta = new MTA();
                    mta.read(new FileData(data));
                    modelContainer.NUD.ApplyMta(mta, 0);
                }
            }
        }

        private static void OpenSkeleton(string fileNameJtb, string fileNameVbn, string fileNameSB, ModelContainer modelContainer)
        {
            if (!File.Exists(fileNameVbn))
                return;
            modelContainer.VBN = new VBN(fileNameVbn);

            if (File.Exists(fileNameJtb))
                modelContainer.JTB = new JTB(fileNameJtb);
            if (File.Exists(fileNameSB))
                modelContainer.VBN.SwingBones.Read(fileNameSB);
        }

        private static void OpenMoi(string fileName, ModelContainer modelContainer)
        {
            if (!File.Exists(fileName))
                return;

            modelContainer.MOI = new MOI(fileName);
        }

        private static void OpenMta(string fileName, ModelContainer modelContainer)
        {
            if (!File.Exists(fileName))
                return;

            try
            {
                modelContainer.MTA = new MTA();
                modelContainer.MTA.Read(fileName);
            }
            catch (EndOfStreamException)
            {
                modelContainer.MTA = null;
            }
        }

        private static void OpenModelXmb(string fileName, ModelContainer modelContainer)
        {
            if (!File.Exists(fileName))
                return;

            modelContainer.XMB = new XMBFile(fileName);
        }

        private static void OpenNut(string fileName, ModelContainer modelContainer)
        {
            if (!File.Exists(fileName))
                return;

            NUT nut = new NUT(fileName);
            modelContainer.NUT = nut;
            Runtime.TextureContainers.Add(nut);
            // Multiple windows was a mistake...
            Runtime.glTexturesNeedRefreshing = true;
        }

        private static void OpenStprmBin(ModelViewport modelViewport, string fileName)
        {
            if (fileName.EndsWith("stprm.bin"))
            {
                Runtime.stprmParam = new ParamFile(fileName);
                Rendering.RenderTools.SetCameraValuesFromParam(modelViewport.GetCamera(), Runtime.stprmParam);
            }
        }

        private void addMaterialAnimation(string name, MTA m)
        {
            if (!Runtime.MaterialAnimations.ContainsValue(m) && !Runtime.MaterialAnimations.ContainsKey(name))
                Runtime.MaterialAnimations.Add(name, m);
            animList.treeView1.Nodes.Add(name);
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

        public static void HashMatch()
        {
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
            Runtime.SoundContainers.Clear();
            Runtime.Animations.Clear();
            Runtime.Animnames.Clear();
            Runtime.MaterialAnimations.Clear();
            if (Runtime.TargetVBN != null)
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
        }

        private void animationsWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void boneTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

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

        public void openBFRESMats(BFRES.Mesh poly, string name)
        {
            ModelViewport mvp = (ModelViewport)dockPanel1.ActiveContent;
            mvp.BfresOpenMats(poly, name);
        }
        public void bfresOpenMeshEditor(BFRES.Mesh mesh, BFRES.FMDL_Model mdl, BFRES bfres, string name)
        {
            (new BfresMeshEditor(mesh, mdl, bfres) { Text = name }).Show();
        }
        private void clearWorkspaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearWorkSpace();
        }

        public void ClearWorkSpace(bool closeEditors = true)
        {
            Runtime.killWorkspace = true;

            Runtime.ParamManager.Reset();
            hurtboxList.refresh();
            Runtime.Animnames.Clear();
            Runtime.clearMoveset();
            animList.treeView1.Nodes.Clear();

            LightTools.areaLights.Clear();
            LightTools.lightMaps.Clear();

            //Close all Editors
            List<DockContent> openContent = new List<DockContent>();
            foreach (DockContent c in dockPanel1.Contents)
                openContent.Add(c);
            foreach (DockContent c in openContent)
                if (c is EditorBase && closeEditors)
                    c.Close();
        }

        private void renderSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GUI.RenderSettingsMenu renderSettings = new GUI.RenderSettingsMenu();
            renderSettings.Show();
        }

        private void meshListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            meshList.RefreshNodes();
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
                    MainForm.Instance.Progress = new ProgressAlert();
                    MainForm.Instance.Progress.StartPosition = FormStartPosition.CenterScreen;
                    MainForm.Instance.Progress.ProgressValue = 0;
                    MainForm.Instance.Progress.ControlBox = false;
                    MainForm.Instance.Progress.Message = "Please Wait... Opening Character";
                    MainForm.Instance.Progress.Show();

                    string fighterName = new DirectoryInfo(ofd.SelectedPath).Name;
                    string[] dirs = Directory.GetDirectories(ofd.SelectedPath);

                    ModelViewport mvp = new ModelViewport();

                    foreach (string s in dirs)
                    {
                        if (s.EndsWith("model"))
                        {
                            MainForm.Instance.Progress.ProgressValue = 10;
                            MainForm.Instance.Progress.Message = "Please Wait... Opening Character Model";
                            MainForm.Instance.Progress.Refresh();
                            // load default model
                            mvp = OpenNud(s + "\\body\\c00\\model.nud", "", mvp);

                            MainForm.Instance.Progress.ProgressValue = 25;
                            MainForm.Instance.Progress.Message = "Please Wait... Opening Character Expressions";
                            string[] anims = Directory.GetFiles(s + "\\body\\c00\\");
                            float a = 0;
                            foreach (string ss in anims)
                            {
                                MainForm.Instance.Progress.ProgressValue = 25 + (int)((a++ / anims.Length) * 25f);
                                if (ss.EndsWith(".pac"))
                                {
                                    mvp.animListPanel.treeView1.Nodes.Add(openAnimation(ss));
                                }
                            }
                        }

                        if (s.EndsWith("motion"))
                        {
                            MainForm.Instance.Progress.ProgressValue = 50;
                            MainForm.Instance.Progress.Message = "Please Wait... Opening Character Animation";
                            string[] anims = Directory.GetFiles(s + "\\body\\");
                            //Sort files so main.pac is opened first
                            Array.Sort(anims, (a, b) =>
                            {
                                if (a.Contains("main.pac"))
                                    return -1;
                                if (b.Contains("main.pac"))
                                    return 1;

                                return 0;
                            });
                            foreach (string a in anims)

                                mvp.animListPanel.treeView1.Nodes.Add(openAnimation(a));
                        }
                        if (s.EndsWith("script"))
                        {
                            MainForm.Instance.Progress.ProgressValue = 75;
                            MainForm.Instance.Progress.Message = ("Please Wait... Opening Character Scripts");
                            if (File.Exists(s + "\\animcmd\\body\\motion.mtable"))
                            {
                                mvp.MovesetManager = new MovesetManager(s + "\\animcmd\\body\\motion.mtable");
                            }

                            if (Runtime.LoadAndRenderATKD && File.Exists(s + "\\ai\\attack_data.bin"))
                                Runtime.currentATKD = s + "\\ai\\attack_data.bin";
                        }
                    }

                    mvp.Text = fighterName;

                    MainForm.Instance.Progress.ProgressValue = 95;
                    MainForm.Instance.Progress.Message = "Please Wait... Opening Character Params";
                    if (!String.IsNullOrEmpty(Runtime.paramDir))
                    {
                        // If they set the wrong dir, oh well
                        try
                        {
                            mvp.paramManager = new CharacterParamManager(Runtime.paramDir + $"\\fighter\\fighter_param_vl_{fighterName}.bin", fighterName);
                            mvp.hurtboxList.refresh();
                            mvp.paramManagerHelper = new PARAMEditor(Runtime.paramDir + $"\\fighter\\fighter_param_vl_{fighterName}.bin");
                            mvp.paramMoveNameIdMapping = mvp.paramManagerHelper.getMoveNameIdMapping();

                            // Model render size
                            ParamFile param = new ParamFile(Runtime.paramDir + "\\fighter\\fighter_param.bin");
                            ParamEntry[] characterParams = ((ParamGroup)param.Groups[0])[CharacterParamManager.FIGHTER_ID[fighterName]];
                            int modelScaleIndex = 44;
                            Runtime.modelScale = Convert.ToSingle(characterParams[modelScaleIndex].Value);
                        }
                        catch { }
                    }
                    MainForm.Instance.Progress.ProgressValue = 99;
                    MainForm.Instance.Progress.Message = "Please Wait... Opening Character ATKD";
                    if (!string.IsNullOrEmpty(Runtime.currentATKD))
                    {
                        ATKD_Editor atkd_editor = new ATKD_Editor(Runtime.currentATKD, mvp);
                        mvp.atkdEditor = atkd_editor;
                        AddDockedControl(atkd_editor);
                    }
                    MainForm.Instance.Progress.ProgressValue = 100;
                    AddDockedControl(mvp);
                }
            }
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
                    OpenStageFolder(ofd.SelectedPath);
                }
            }

        }

        public void OpenStageFolder(string stagePath, ModelViewport mvp = null)
        {
            MainForm.Instance.Progress = new ProgressAlert();
            MainForm.Instance.Progress.StartPosition = FormStartPosition.CenterScreen;
            MainForm.Instance.Progress.ProgressValue = 0;
            MainForm.Instance.Progress.ControlBox = false;
            MainForm.Instance.Progress.Message = ("Please Wait... Opening Stage Models");
            MainForm.Instance.Progress.Show();

            string modelPath = stagePath + "\\model\\";
            string paramPath = stagePath + "\\param\\";
            string animationPath = stagePath + "\\animation\\";
            string renderPath = stagePath + "\\render\\";

            if (mvp == null)
            {
                mvp = new ModelViewport();
                mvp.Text = Path.GetFileName(stagePath);
            }

            if (Directory.Exists(modelPath))
            {
                foreach (string d in Directory.GetDirectories(modelPath))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        if (f.EndsWith(".nud"))
                        {
                            OpenNud(f, Path.GetFileName(d), mvp);
                        }
                    }
                }
            }

            MainForm.Instance.Progress.ProgressValue = 50;
            MainForm.Instance.Progress.Message = ("Please Wait... Opening Stage Parameters");
            if (Directory.Exists(paramPath))
            {
                foreach (string fileName in Directory.GetFiles(paramPath))
                {
                    if (Path.GetExtension(fileName).Equals(".lvd") && Runtime.TargetLVD == null)
                    {
                        mvp.LVD = new LVD(fileName);
                        lvdList.fillList();
                    }

                    OpenStprmBin(mvp, fileName);
                }
            }

            MainForm.Instance.Progress.ProgressValue = 75;
            MainForm.Instance.Progress.Message = ("Please Wait... Opening Stage Path");
            if (Directory.Exists(renderPath))
            {
                foreach (string fileName in Directory.GetFiles(renderPath))
                {
                    if (fileName.EndsWith("light_set_param.bin"))
                    {
                        Runtime.lightSetParam = new Params.LightSetParam(fileName);
                        Runtime.lightSetDirectory = fileName;
                    }

                    if (fileName.EndsWith("area_light.xmb"))
                    {
                        LightTools.CreateAreaLightsFromXMB(new XMBFile(fileName));
                    }

                    if (fileName.EndsWith("lightmap.xmb"))
                    {
                        LightTools.CreateLightMapsFromXMB(new XMBFile(fileName));
                    }
                }
            }

            MainForm.Instance.Progress.ProgressValue = 80;
            MainForm.Instance.Progress.Message = ("Please Wait... Opening Stage Animation");
            if (Directory.Exists(animationPath))
            {
                foreach (string d in Directory.GetDirectories(animationPath))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        if (f.EndsWith(".omo"))
                        {
                            Animation a = OMOOld.read(new FileData(f));
                            a.Text = f;
                            mvp.animListPanel.treeView1.Nodes.Add(a);
                        }
                        else if (f.EndsWith("path.bin"))
                        {
                            mvp.pathBin = new PathBin();
                            mvp.pathBin.Read(f);
                        }
                    }
                }
            }
            MainForm.Instance.Progress.ProgressValue = 100;
            AddDockedControl(mvp);
        }

        //<summary>
        //Open an animation based on filename
        //</summary>
        public TreeNode openAnimation(string filename)
        {
            if (filename.EndsWith(".mta"))
            {
                MTA mta = new MTA();
                try
                {
                    mta.Read(filename);
                    return (mta);
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
                return (anim);
            }
            else if (filename.EndsWith(".pac"))
            {
                PAC p = new PAC();
                p.Read(filename);
                AnimationGroupNode animGroup = new AnimationGroupNode() { Text = Path.GetFileName(filename) };

                foreach (var pair in p.Files)
                {
                    if (pair.Key.EndsWith(".omo"))
                    {
                        Console.WriteLine("Adding " + pair.Key);
                        var anim = OMOOld.read(new FileData(pair.Value));
                        animGroup.Nodes.Add(anim);
                        string AnimName = pair.Key;
                        if (!string.IsNullOrEmpty(AnimName))
                        {
                            anim.Text = AnimName;
                            AddAnimName(AnimName.Substring(3).Replace(".omo", ""));
                        }
                    }
                    else if (pair.Key.EndsWith(".mta"))
                    {
                        MTA mta = new MTA();
                        mta.read(new FileData(pair.Value));
                        mta.Text = pair.Key;
                        animGroup.Nodes.Add(mta);
                    }
                }
                return animGroup;
            }

            if (filename.EndsWith(".dat"))
            {
                if (!filename.EndsWith("AJ.dat"))
                    MessageBox.Show("Not a DAT animation");
            }

            if (filename.EndsWith(".omo"))
            {
                Animation a = OMOOld.read(new FileData(filename));
                a.Text = filename;
                return a;
            }

            if (filename.EndsWith(".chr0"))
            {
                return (CHR0.read(new FileData(filename), Runtime.TargetVBN));
            }

            if (filename.EndsWith(".anim"))
            {
                return (ANIM.read(filename, Runtime.TargetVBN));
            }

            if (filename.EndsWith(".bch"))
            {
                BCHan.Read(filename);
                BCH bch = new Smash_Forge.BCH();
                bch.Read(filename);
            }

            return null;
        }

        private static void writeDatJobjPositions(TreeNode node, FileOutput f)
        {
            if (node.Tag is DAT.JOBJ)
            {
                DAT.JOBJ jobj = (DAT.JOBJ)node.Tag;
                f.writeFloatAt((float)jobj.pos.X, jobj.posOff);
                f.writeFloatAt((float)jobj.pos.Y, jobj.posOff + 4);
                f.writeFloatAt((float)jobj.pos.Z, jobj.posOff + 8);
            }
            foreach (TreeNode child in node.Nodes)
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


            if (filename.EndsWith(".byaml") && Runtime.TargetBYAML != null)
                Runtime.TargetBYAML.Rebuild(filename);

        }


        ///<summary>
        ///Open a file based on the filename
        ///</summary>
        /// <param name="fileName"> Filename of file to open</param>
        public void openFile(string fileName)
        {
            DiscordSettings.lastFileOpened = Path.GetFileName(fileName);

            // Reassigned if a valid model file is opened. 
            ModelViewport mvp = new ModelViewport();

            if (fileName.EndsWith(".omo"))
            {
                mvp = new ModelViewport();
                if (dockPanel1.ActiveContent is ModelViewport)
                {
                    DialogResult dialogResult = MessageBox.Show("Import Animation Data into active viewport?", "", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        mvp = (ModelViewport)dockPanel1.ActiveContent;
                        mvp.animListPanel.treeView1.Nodes.Add(OMOOld.read(new FileData(fileName)));
                        return;
                    }
                    else
                    {
                        mvp.animListPanel.treeView1.Nodes.Add(OMOOld.read(new FileData(fileName)));
                    }
                    mvp.Text = fileName;
                    AddDockedControl(mvp);
                }
            }

            if (fileName.EndsWith(".pac"))
            {
                mvp = new ModelViewport();
                if (dockPanel1.ActiveContent is ModelViewport)
                {
                    DialogResult dialogResult = MessageBox.Show("Import Animation Data into active viewport?", "", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        mvp = (ModelViewport)dockPanel1.ActiveContent;
                        mvp.animListPanel.treeView1.Nodes.Add(openAnimation(fileName));
                        return;
                    }
                    else
                    {
                        mvp.animListPanel.treeView1.Nodes.Add(openAnimation(fileName));
                    }
                    mvp.Text = fileName;
                    AddDockedControl(mvp);
                }
            }

            if (fileName.EndsWith(".bch"))
            {
                mvp = new ModelViewport();
                // Load as Animation
                if (dockPanel1.ActiveContent is ModelViewport)
                {
                    DialogResult dialogResult = MessageBox.Show("Import Animation Data into active viewport?", "", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        mvp = (ModelViewport)dockPanel1.ActiveContent;
                        mvp.animListPanel.treeView1.Nodes.Add(BCHan.Read(fileName));
                        return;
                    }
                    else
                    {
                        mvp.animListPanel.treeView1.Nodes.Add(BCHan.Read(fileName));
                    }
                }

                mvp.Text = fileName;
                AddDockedControl(mvp);
            }

            if (fileName.EndsWith(".nud"))
            {
                if (dockPanel1.ActiveContent is ModelViewport)
                {
                    DialogResult dialogResult = MessageBox.Show("Import into active viewport?", "", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        OpenNud(fileName, new DirectoryInfo(Path.GetDirectoryName(fileName)).Name, (ModelViewport)dockPanel1.ActiveContent);
                    }
                    if (dialogResult == DialogResult.No)
                    {
                        OpenNud(fileName, new DirectoryInfo(Path.GetDirectoryName(fileName)).Name);
                    }
                }
                else
                {
                    OpenNud(fileName, new DirectoryInfo(Path.GetDirectoryName(fileName)).Name);
                }
            }

            if (fileName.EndsWith(".byaml") || fileName.EndsWith(".sprm") || fileName.EndsWith(".byml"))
            {
                if (CheckCurrentViewport(out mvp))
                {
                    mvp.ViewComboBox.SelectedItem = "BYAML Editor";
                    //    mvp.BYAML = new BYAML(fileName);
                }
                else
                {
                    mvp.ViewComboBox.SelectedItem = "BYAML Editor";
                    mvp.Text = fileName;
                    //       mvp.BYAML = new BYAML(fileName);
                }
            }

            if (fileName.EndsWith(".kcl"))
            {
                FileData f = new FileData(fileName);
                byte[] data = f.getSection(0, f.eof());

                if (dockPanel1.ActiveContent is ModelViewport)
                {
                    DialogResult dialogResult = MessageBox.Show("Import into active viewport?", "", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        OpenKcl(data, fileName, fileName, (ModelViewport)dockPanel1.ActiveContent);
                    }
                    if (dialogResult == DialogResult.No)
                    {
                        OpenKcl(data, fileName);
                    }
                }
                else
                {
                    OpenKcl(data, fileName);
                }
            }

            if (fileName.EndsWith(".szs") || fileName.EndsWith(".sbfres"))
            {
                byte[] fileByteData = GetUncompressedSzsSbfresData(fileName);
                FileData uncompressedFileData = new FileData(fileByteData);

                int hexM = uncompressedFileData.readInt();
                string magic2 = uncompressedFileData.readString(0, 4);

                if (magic2 == "FRES") //YAZO compressed
                {
                    if (dockPanel1.ActiveContent is ModelViewport)
                    {
                        DialogResult dialogResult = MessageBox.Show("Import into active viewport?", "", MessageBoxButtons.YesNo);

                        if (dialogResult == DialogResult.Yes)
                            OpenBfres(fileByteData, fileName, fileName, (ModelViewport)dockPanel1.ActiveContent);

                        if (dialogResult == DialogResult.No)
                            OpenBfres(fileByteData, fileName);
                    }
                    else
                    {
                        OpenBfres(fileByteData, fileName);
                    }
                }

                if (hexM == 0x02020000) //YAZO compressed
                {
                    OpenKcl(fileByteData, fileName, fileName, (ModelViewport)dockPanel1.ActiveContent);
                }

                if (magic2 == "BNTX") //SARC compressed
                {
                    BNTX BNTX = new BNTX();
                    BNTX.ReadBNTXFile(fileByteData);
                    Runtime.BNTXList.Add(BNTX);

                    BNTXEditor editor = new BNTXEditor(BNTX);
                    AddDockedControl(editor);

                }

                if (magic2 == "SARC") //SARC compressed
                {
                    var SzsFiles = new SARC().unpackRam(uncompressedFileData.getSection(0, uncompressedFileData.eof()));

                    foreach (var s in SzsFiles.Keys)
                    {
                        if (s.Contains(".bfres"))
                        {
                            Console.WriteLine("Found bfres");
                            fileByteData = SzsFiles[s];
                            uncompressedFileData = new FileData(fileByteData);

                            OpenBfres(fileByteData, s, s, (ModelViewport)dockPanel1.ActiveContent);
                        }

                        if (s.Contains(".kcl"))
                        {
                            Console.WriteLine("Found kcl");
                            fileByteData = SzsFiles[s];
                            uncompressedFileData = new FileData(fileByteData);

                            OpenKcl(fileByteData, s, s, (ModelViewport)dockPanel1.ActiveContent);
                        }
                    }
                }
            }

            if (fileName.EndsWith(".bfres"))
            {
                FileData f = new FileData(fileName);
                byte[] data = f.getSection(0, f.eof());

                if (dockPanel1.ActiveContent is ModelViewport)
                {
                    DialogResult dialogResult = MessageBox.Show("Import into active viewport?", "", MessageBoxButtons.YesNo);

                    if (dialogResult == DialogResult.Yes)
                        OpenBfres(data, fileName, fileName, (ModelViewport)dockPanel1.ActiveContent);

                    if (dialogResult == DialogResult.No)
                        OpenBfres(data, fileName);
                }
                else
                {
                    OpenBfres(data, fileName);
                }

            }

            if (fileName.EndsWith(".mbn"))
            {
                if (!File.Exists(fileName.Replace(".mbn", ".bch")))
                {
                    MessageBox.Show("Missing BCH file");
                    return;
                }

                BCH b = new BCH(fileName.Replace(".mbn", ".bch"));
                if (b.Models.Nodes.Count == 0)
                {
                    MessageBox.Show("BCH is Missing Model Data");
                    return;
                }
                ((BCH_Model)b.Models.Nodes[0]).OpenMBN(new FileData(fileName));

                mvp = new ModelViewport();
                mvp.draw.Add(new ModelContainer() { Bch = b });
                mvp.Text = fileName;
                AddDockedControl(mvp);
            }

            if (fileName.EndsWith(".vbn"))
            {
                BoneTreePanel editor = new BoneTreePanel(fileName);
                AddDockedControl(editor);
            }

            if (fileName.EndsWith(".nut"))
            {
                NUTEditor editor = new NUTEditor(fileName);
                AddDockedControl(editor);
            }

            if (fileName.EndsWith(".bntx"))
            {
                ModelContainer modelContainer = new ModelContainer();

                modelContainer.BNTX = new BNTX();
                modelContainer.BNTX.ReadBNTXFile(fileName);
                Runtime.BNTXList.Add(modelContainer.BNTX);
                Runtime.glTexturesNeedRefreshing = true;

                BNTXEditor editor = new BNTXEditor(modelContainer.BNTX);
                AddDockedControl(editor);

                mvp.meshList.filesTreeView.Nodes.Add(modelContainer);
            }

            if (fileName.EndsWith(".tex"))
            {
                _3DSTexEditor editor = new _3DSTexEditor(fileName);
                AddDockedControl(editor);
            }

            if (fileName.EndsWith(".sb"))
            {
                //needs vbn
                if (File.Exists(fileName.Replace(".sb", ".vbn")))
                {
                    VBN vbn = new VBN(fileName.Replace(".sb", ".vbn"));
                    SB sb = new SB();
                    sb.Read(fileName);
                    vbn.SwingBones = sb;
                    sb.OpenEditor(null, null);
                }
                else
                {
                    MessageBox.Show("Swing Bones need a VBN with the same name");
                }
            }

            if (fileName.EndsWith(".dat"))
            {
                /*if (fileName.EndsWith("AJ.dat"))
                {
                    MessageBox.Show("This is animation; load with Animation -> Import");

                    ModelViewport modelViewport;
                    if (CheckCurrentViewport(out modelViewport))
                    {
                        foreach (ModelContainer modelContainer in modelViewport.meshList.filesTreeView.Nodes)
                        {
                            if (modelContainer.DatMelee != null)
                            {
                                Dictionary<string, Animation> Anims = DAT_Animation.GetTracks(fileName, modelContainer.DatMelee.bones);
                                foreach (string key in Anims.Keys)
                                {
                                    Anims[key].Text = key;
                                    modelViewport.animListPanel.treeView1.Nodes.Add(Anims[key]);
                                }
                                return;
                            }
                        }
                    }

                    return;
                }*/
                byte[] data = File.ReadAllBytes(fileName);
                if (dockPanel1.ActiveContent is ModelViewport)
                {
                    DialogResult dialogResult = MessageBox.Show("Import into active viewport?", "", MessageBoxButtons.YesNo);

                    if (dialogResult == DialogResult.Yes)
                        OpenMeleeDat(data, fileName, fileName, (ModelViewport)dockPanel1.ActiveContent);

                    if (dialogResult == DialogResult.No)
                        OpenMeleeDat(data, fileName);
                }
                else
                {
                    OpenMeleeDat(data, fileName);
                }
            }

            if (fileName.EndsWith(".lvd"))
            {
                if (CheckCurrentViewport(out mvp))
                {
                    mvp.ViewComboBox.SelectedItem = "LVD Editor";
                    mvp.LVD = new LVD(fileName);
                }
                else
                {
                    mvp.ViewComboBox.SelectedItem = "LVD Editor";
                    mvp.Text = Path.GetFileName(fileName);
                    mvp.LVD = new LVD(fileName);
                }
            }
            if (fileName.EndsWith(".lm"))
            {
                mvp.ViewComboBox.SelectedItem = "LM Editor";
                AddDockedControl(new LMList(fileName));
            }

            if (fileName.EndsWith(".mdl0"))
            {
                MDL0Bones mdl0 = new MDL0Bones();
                BoneTreePanel editor = new BoneTreePanel(mdl0.GetVBN(new FileData(fileName)));
                AddDockedControl(editor);
            }

            if (fileName.ToLower().EndsWith(".obj"))
            {
                OBJ obj = new OBJ();
                obj.Read(fileName);
                ModelContainer modelContainer = (new ModelContainer() { NUD = obj.toNUD() });

                if (CheckCurrentViewport(out mvp))
                {
                    mvp.meshList.filesTreeView.Nodes.Add(modelContainer);
                }
                else
                {
                    mvp.Text = fileName;
                    mvp.meshList.filesTreeView.Nodes.Add(modelContainer);
                }
            }

            if (fileName.EndsWith(".smd"))
            {
                ModelContainer modelContainer = new ModelContainer();
                VBN TargetVBN = new VBN();
                SMD.read(fileName, new Animation(fileName), TargetVBN);
                modelContainer.VBN = TargetVBN;
                modelContainer.NUD = SMD.toNUD(fileName);
                modelContainer.VBN.reset();

                if (CheckCurrentViewport(out mvp))
                {
                    mvp.meshList.filesTreeView.Nodes.Add(modelContainer);
                }
                else
                {
                    mvp.Text = fileName;
                    mvp.meshList.filesTreeView.Nodes.Add(modelContainer);
                }
            }

            if (fileName.ToLower().EndsWith(".dae"))
            {
                DAEImportSettings daeImportSettings = new DAEImportSettings();
                daeImportSettings.ShowDialog();
                if (daeImportSettings.exitStatus == DAEImportSettings.ExitStatus.Opened)
                {
                    ModelContainer modelContainer = new ModelContainer();

                    // load vbn
                    modelContainer.VBN = daeImportSettings.getVBN();

                    Collada.DaetoNud(fileName, modelContainer, daeImportSettings.importTexCB.Checked);
                    if (modelContainer.NUD == null)
                        return;

                    // apply settings
                    daeImportSettings.Apply(modelContainer.NUD);

                    if (CheckCurrentViewport(out mvp))
                    {
                        mvp.meshList.filesTreeView.Nodes.Add(modelContainer);
                        // Makes sure the vertex data is updated properly and the model will be visible. 
                        mvp.FrameSelectionAndSort();
                    }
                    else
                    {
                        modelContainer.Text = fileName;
                        mvp.Text = fileName;
                        mvp.meshList.filesTreeView.Nodes.Add(modelContainer);
                        // Makes sure the vertex data is updated properly and the model will be visible. 
                        mvp.FrameSelectionAndSort();
                    }
                }
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
                Runtime.TargetMTA.Clear();
                Runtime.TargetMTA.Add(TargetMTA);
                MTAEditor temp = new MTAEditor(TargetMTA) { ShowHint = DockState.DockLeft };
                temp.Text = Path.GetFileName(fileName);
                AddDockedControl(temp);
                mtaEditors.Add(temp);
            }

            if (fileName.EndsWith(".mtable"))
            {
                Runtime.Moveset = new MovesetManager(fileName);
                Runtime.acmdEditor.updateCrcList();
            }
            if (fileName.EndsWith("path.bin"))
            {
                Runtime.TargetPath = new PathBin(fileName);
            }
            else if (fileName.EndsWith("light.bin"))
            {
                Runtime.TargetLigh = new LIGH.LighBin(fileName);
            }
            else if (fileName.EndsWith(".bin"))
            {
                FileData f = new FileData(fileName);
                if (f.readUShort() == 0xFFFF)
                {
                    PARAMEditor p = new PARAMEditor(fileName) { ShowHint = DockState.Document };
                    p.Text = Path.GetFileName(fileName);
                    AddDockedControl(p);
                    paramEditors.Add(p);

                    if (fileName.EndsWith("light_set_param.bin"))
                    {
                        Runtime.lightSetParam = new Params.LightSetParam(fileName);
                        Runtime.lightSetDirectory = fileName;
                    }

                    if (fileName.EndsWith("stprm.bin"))
                    {
                        // should this always replace existing settings?
                        Runtime.stprmParam = new ParamFile(fileName);
                        Rendering.RenderTools.SetCameraValuesFromParam(mvp.GetCamera(), Runtime.stprmParam);
                    }

                }
                else if (f.readString(4, 4) == "PATH")
                {
                    Runtime.TargetPath = new PathBin(fileName);
                }
                else if (f.readString(0, 4) == "LIGH")
                {
                    Runtime.TargetLigh = new LIGH.LighBin(fileName);
                }
                else if (f.readString(6, 4) == "LVD1")
                {
                    if (Runtime.TargetLVD == null)
                        Runtime.TargetLVD = new LVD(fileName);
                    else
                        Runtime.TargetLVD.Read(fileName);
                    lvdList.fillList();
                }
                else if (f.readString(0, 4) == "ATKD")
                {
                    AddDockedControl(new ATKD_Editor(fileName));
                }
                else
                {
                    Runtime.TargetCMR0 = new CMR0();
                    Runtime.TargetCMR0.read(new FileData(fileName));
                }
            }

            if (fileName.EndsWith("area_light.xmb"))
            {
                LightTools.CreateAreaLightsFromXMB(new XMBFile(fileName));
            }

            if (fileName.EndsWith("lightmap.xmb"))
            {
                LightTools.CreateLightMapsFromXMB(new XMBFile(fileName));
            }

            if (fileName.EndsWith(".moi"))
            {
                MOI moi = new MOI(fileName);
                AddDockedControl(new MOIEditor(moi) { ShowHint = DockState.DockRight });
            }

            if (fileName.EndsWith(".drp"))
            {
                DRP drp = new DRP(fileName);
                DRPViewer v = new DRPViewer();
                v.treeView1.Nodes.Add(drp);
                v.Show();
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

        public static byte[] GetUncompressedSzsSbfresData(string fileName)
        {
            FileData f = new FileData(fileName);
            byte[] fileBytes = f.getSection(0, f.eof());
            string Magic = f.readString(0, 4);

            if (Magic == "Yaz0") //YAZO compressed
                fileBytes = EveryFileExplorer.YAZ0.Decompress(fileName);

            return fileBytes;
        }

        public ModelViewport GetActiveViewport()
        {
            if (dockPanel1.ActiveContent is ModelViewport)
                return (ModelViewport)dockPanel1.ActiveContent;
            else
                return null;
        }

        // returns true if importing into active viewport
        public bool CheckCurrentViewport(out ModelViewport mvp)
        {
            mvp = null;
            if (dockPanel1.ActiveContent is ModelViewport)
            {
                DialogResult dialogResult = MessageBox.Show("Import into active viewport?", "", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    mvp = ((ModelViewport)dockPanel1.ActiveContent);
                    return true;
                }
                if (dialogResult == DialogResult.No)
                {
                    mvp = new ModelViewport();
                    AddDockedControl(mvp);
                    return false;
                }
            }
            else
            {
                mvp = new ModelViewport();
                AddDockedControl(mvp);
                return false;
            }
            return false;
        }

        private ModelContainer resyncTargetVBN()
        {
            ModelContainer modelContainer = null;
            return modelContainer;
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter =
                    "Supported Formats|*.vbn;*.lvd;*.nud;*.xmb;*.bin;*.dae;*.obj;*.wrkspc;*.nut;*.sb;*.tex;*.smd;*.mta;*.pac;*.xmb;*.bch;*.mbn;*.bfres;*.mdl0;*.bntx;*.szs;*.sbfres;*.sarc;*.pack;*.byaml;*.byml;*.kcl;*.dat;*.lm;*.nulm|" +
                    "Smash 4 Boneset (.vbn)|*.vbn|" +
                    "Namco Model (.nud)|*.nud|" +
                    "Smash 4 Level Data (.lvd)|*.lvd|" +
                    "NW4R Model (.mdl0)|*.mdl0|" +
                    "Source Model (.SMD)|*.smd|" +
                    "Smash 4 Parameters (.bin)|*.bin|" +
                    "Lumen UI (.lm)|*.lm;*.nulm|" +
                    "Collada Model Format (.dae)|*.dae|" +
                    "Wavefront Object (.obj)|*.obj|" +
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
                        openFile(filename);
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !Text.Equals("Meteor Preview"))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
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

        private void greenArrowPictureBox_Click(object sender, EventArgs e)
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

        private void allViewsPreset(object sender, EventArgs e)
        {
        }

        private void modelViewPreset(object sender, EventArgs e)
        {
        }

        private void movesetModdingPreset(object sender, EventArgs e)
        {
        }

        private void stageWorkPreset(object sender, EventArgs e)
        {
        }

        private void cleanPreset(object sender, EventArgs e)
        {
        }

        private void superCleanPreset(object sender, EventArgs e)
        {
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

        private void exportErrorLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Rendering.ShaderTools.SaveErrorLogs();
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
            if (datToOpen != null)
                AddDockedControl(new DatTexEditor(datToOpen));
        }

        private void saveConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Config.Save();
        }

        private void cameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
                    }
                    catch
                    {

                    }
                }
            }
        }

        private void stageLightingToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (Runtime.lightSetDirectory == "")
            {
                // There isn't a way to create a new light_set_param.bin currently, so the user has to open an existing one.
                var lightSetWarning = MessageBox.Show("No light_set_param.bin detected. Please open an existing light_set_param.bin file.",
                    "No light_set detected.", MessageBoxButtons.OKCancel);
                if (lightSetWarning == DialogResult.Cancel)
                {
                    return;
                }
                else if (LightSetEditor.OpenLightSet())
                {
                    LightSetEditor stageLightForm = new LightSetEditor();
                    stageLightForm.Show();
                }
            }
            else
            {
                LightSetEditor stageLightForm = new LightSetEditor();
                stageLightForm.Show();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dockPanel1.ActiveContent is EditorBase)
            {
                ((EditorBase)dockPanel1.ActiveContent).SaveAs();
            }
        }

        private void nutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NUT nut = new NUT();
            NUTEditor editor = new NUTEditor(nut);
            AddDockedControl(editor);
        }

        private void modelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddModelViewport();
        }

        private void AddModelViewport()
        {
            ModelViewport mvp = new ModelViewport();
            AddDockedControl(mvp);
        }

        private void dSTexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _3DSTexEditor mvp = new _3DSTexEditor();
            AddDockedControl(mvp);
        }

        private void reloadShadersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Force the binaries to be regenerated.
            ShaderTools.SetUpShaders(true);
        }

        private void open3DSCharacterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new FolderSelectDialog())
            {
                ofd.Title = "Character Folder";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string fighterName = new DirectoryInfo(ofd.SelectedPath).Name;

                    ModelViewport mvp = new ModelViewport();
                    mvp.Text = fighterName;

                    String ModelFolder = ofd.SelectedPath + "\\body\\h00\\";
                    Console.WriteLine(ModelFolder);
                    if (Directory.Exists(ModelFolder))
                    {
                        ModelContainer modelContainer = new ModelContainer();
                        if (File.Exists(ModelFolder + "normal.bch"))
                        {
                            BCH bch = new Smash_Forge.BCH(ModelFolder + "normal.bch");
                            if (bch.Models.Nodes.Count > 0 && File.Exists(ModelFolder + "normal.mbn"))
                                ((BCH_Model)bch.Models.Nodes[0]).OpenMBN(new FileData(ModelFolder + "normal.mbn"));
                            modelContainer.Bch = bch;
                        }

                        if (File.Exists(ofd.SelectedPath + "\\body\\c00\\" + "model.jtb"))
                        {
                            modelContainer.JTB = new Smash_Forge.JTB(ofd.SelectedPath + "\\body\\c00\\" + "model.jtb");
                        }
                        mvp.draw.Add(modelContainer);
                    }

                    String AnimationFolder = ofd.SelectedPath.Replace("model", "motion") + "\\body\\";
                    if (Directory.Exists(AnimationFolder))
                    {
                        string[] anims = Directory.GetFiles(ModelFolder);
                        foreach (string s in anims)
                        {
                            if (s.EndsWith("main.bch"))
                                mvp.animListPanel.treeView1.Nodes.Add(BCHan.Read(s));
                        }

                    }

                    String ACMDFolder = ofd.SelectedPath.Replace("model", "animcmd") + "\\";
                    if (Directory.Exists(ACMDFolder))
                    {
                        mvp.MovesetManager = new MovesetManager(ACMDFolder + "motion.mtable");
                    }

                    AddDockedControl(mvp);
                }
            }
        }

        private void forgeWikiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/jam1garner/Smash-Forge/wiki");
        }

        private void importWiiUNUTAsPS3NUTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists("TexConv/TexConv2.exe"))
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Namco Universal Texture|*.nut|" +
                                 "All files(*.*)|*.*";

                    ofd.Multiselect = true;

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        foreach (string filename in ofd.FileNames)
                        {
                            HackyWiiUtoPS3NUT(new Smash_Forge.FileData(filename));
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Error: Get TexConv2.exe and put it in Forge's Directory");
            }
        }

        private void HackyWiiUtoPS3NUT(FileData data)
        {
            NUT n = new NUT();
            data.Endian = Endianness.Big;
            data.seek(0x6);
            int count = data.readUShort();

            data.seek(0x10);

            int padfix = 0;
            int headerSize = 0;
            int offheader = 0;
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                    padfix += headerSize;
                data.skip(4); //int fullsize = d.readInt();
                data.skip(4);
                int size = data.readInt();
                int DDSSize = size;
                headerSize = data.readUShort();
                data.skip(5);
                int typet = data.readByte();
                int Width = data.readUShort();
                int Height = data.readUShort();

                data.skip(8);// mipmaps and padding

                int offset1 = data.readInt() + 16;
                int offset2 = data.readInt() + 16;
                int offset3 = data.readInt() + 16;
                data.skip(4);
                if (i == 0)
                {
                    offheader = offset3;
                }
                if (headerSize == 0x90)
                {
                    DDSSize = data.readInt();
                    data.skip(0x3C);
                }
                if (headerSize == 0x80)
                {
                    DDSSize = data.readInt();
                    data.skip(44);
                }
                if (headerSize == 0x70)
                {
                    DDSSize = data.readInt();
                    data.skip(28);
                }
                if (headerSize == 0x60)
                {
                    DDSSize = data.readInt();
                    data.skip(12);
                }

                data.skip(16);
                data.skip(4);
                data.skip(4);
                int fileNum = data.readInt();
                data.skip(4);
                {
                    {
                        FileOutput o = new FileOutput();
                        String mem = "4766783200000020000000070000000100000002000000000000000000000000424C4B7B0000002000000001000000000000000B0000009C0000000000000000";
                        o.writeHex(mem);

                        int t = data.pos();
                        data.seek(offheader);
                        for (int k = 0; k < 0x80; k++)
                            o.writeByte(data.readByte());
                        offheader += 0x80;
                        data.seek(t);
                        if (i > 0)
                        {
                            //offset1 += padfix;
                            //offheader += 0x80;
                        }

                        mem = "00000001000102031FF87F21C40003FF068880000000000A80000010424C4B7B0000002000000001000000000000000C000800000000000000000000";
                        o.writeHex(mem);

                        Console.WriteLine(fileNum.ToString("x"));
                        o.writeBytes(data.getSection(offset1 + padfix, DDSSize));

                        mem = "424C4B7B00000020000000010000000000000001000000000000000000000000";
                        o.writeHex(mem);

                        o.Endian = Endianness.Big;
                        o.writeIntAt(1, 0x50);
                        o.writeIntAt(DDSSize, 0xF0);

                        o.save("TexConv/temp.gtx");

                        String command = " -i temp.gtx -o temp.dds";
                        ProcessStartInfo cmdsi = new ProcessStartInfo();
                        cmdsi.Arguments = command;
                        cmdsi.WorkingDirectory = @"TexConv\";
                        cmdsi.FileName = @"TexConv2.exe";
                        cmdsi.Arguments = command;
                        Process cmd = Process.Start(cmdsi);
                        cmd.WaitForExit();

                        NutTexture tex = new DDS(new FileData("TexConv/temp.dds")).ToNutTexture();
                        tex.HashId = fileNum;
                        n.glTexByHashId.Add(fileNum, NUT.CreateTexture2D(tex));
                        n.Nodes.Add(tex);
                    }
                }
            }
            NUTEditor editor = new NUTEditor();
            editor.SelectNUT(n);
            AddDockedControl(editor);
        }

        private void openOdysseyCostumeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OdysseyCostumeSelector OCS = new OdysseyCostumeSelector();
            OCS.ShowDialog(this);
        }

        public void LoadCostumes(string fileName)
        {
            fileName = Path.ChangeExtension(fileName, null);

            // Reassigned if a valid model file is opened. 
            ModelViewport mvp = new ModelViewport();
            AddDockedControl(mvp);

            List<string> CostumeNames = new List<string>();
            CostumeNames.Add($"{fileName}.szs");
            CostumeNames.Add($"{fileName}Face.szs");
            CostumeNames.Add($"{fileName}Eye.szs");
            CostumeNames.Add($"{fileName}Head.szs");
            CostumeNames.Add($"{fileName}HeadTexture.szs");
            CostumeNames.Add($"{fileName}Under.szs");
            CostumeNames.Add($"{fileName}HandL.szs");
            CostumeNames.Add($"{fileName}HandR.szs");
            CostumeNames.Add($"{fileName}HandTexture.szs");
            CostumeNames.Add($"{fileName}BodyTexture.szs");
            CostumeNames.Add($"{fileName}Shell.szs");
            CostumeNames.Add($"{fileName}Tail.szs");
            CostumeNames.Add($"{fileName}Hair.szs");
            //     CostumeNames.Add($"{fileName}Hakama.szs");
            CostumeNames.Add($"{fileName}Skirt.szs");
            //     CostumeNames.Add($"{fileName}Poncho.szs");
            CostumeNames.Add($"{fileName}Guitar.szs");


            foreach (string path in CostumeNames)
            {
                Console.WriteLine("Path = " + path);

                if (File.Exists(path))
                {
                    LoadCostume(path, mvp);
                }
                else
                {
                    //Load default meshes unless it's these file names
                    List<string> ExcludeFileList = new List<string>(new string[] {
                    "MarioHack","MarioDot",
                     });

                    bool Exluded = ExcludeFileList.Any(path.Contains);

                    if (Exluded == false)
                    {
                        string parent = Directory.GetParent(path).FullName;

                        if (path.Contains($"{fileName}Face"))
                            LoadCostume($"{parent}\\MarioFace.szs", mvp);
                        else if (path.Contains($"{fileName}Eye"))
                            LoadCostume($"{parent}\\MarioEye.szs", mvp);
                        else if (path.Contains($"{fileName}HeadTexture"))
                            LoadCostume($"{parent}\\MarioHeadTexture.szs", mvp);
                        else if (path.Contains($"{fileName}Head"))
                            LoadCostume($"{parent}\\MarioHead.szs", mvp);
                        else if (path.Contains($"{fileName}HandL"))
                            LoadCostume($"{parent}\\MarioHandL.szs", mvp);
                        else if (path.Contains($"{fileName}HandR"))
                            LoadCostume($"{parent}\\MarioHandR.szs", mvp);
                        else if (path.Contains($"{fileName}HandTexture"))
                            LoadCostume($"{parent}\\MarioHandTexture.szs", mvp);

                    }
                }
            }

        }
        public void LoadCostume(string fileName, ModelViewport mvp)
        {


            byte[] data = data = EveryFileExplorer.YAZ0.Decompress(fileName);

            FileData t = new FileData(data);

            var SzsFiles = new SARC().unpackRam(t.getSection(0, t.eof()));

            foreach (var s in SzsFiles.Keys)
            {
                if (s.Contains(".bfres"))
                {
                    data = SzsFiles[s];
                    t = new FileData(data);

                    OpenBfres(data, s, s, mvp);
                }
            }
        }

        private void batchRenderNUDToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void batchRenderBOTWBfresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ModelViewport mvp = (ModelViewport)GetActiveModelViewport();
            if (mvp != null)
                mvp.BatchRenderBotwBfresModels();
        }

        private void nudToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filetypes.MaterialXmlBatchExport.BatchExportNudMaterialXml();

        }

        private void meleeDatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filetypes.MaterialXmlBatchExport.BatchExportMeleeDatMaterialXml();
        }

        private void nudToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ModelViewport mvp = (ModelViewport)GetActiveModelViewport();
            if (mvp != null)
                mvp.BatchRenderNudModels();
        }

        private void meleeDatToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ModelViewport mvp = (ModelViewport)GetActiveModelViewport();
            if (mvp != null)
                mvp.BatchRenderMeleeDatModels();
        }

        private void lMToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
