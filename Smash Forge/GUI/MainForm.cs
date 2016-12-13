using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using WeifenLuo.WinFormsUI.Docking;
using OpenTK;
using System.Data;

namespace Smash_Forge
{
    public partial class MainForm : Form
    {
        public static MainForm Instance;

        public MainForm()
        {
            InitializeComponent();
            Instance = this;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            foreach (var vp in viewports)
                AddDockedControl(vp);

            animationsWindowToolStripMenuItem.Checked =
            boneTreeToolStripMenuItem.Checked = true;

            AddDockedControl(meshList);
            AddDockedControl(leftPanel);
            AddDockedControl(rightPanel);
            AddDockedControl(lvdEditor);
            AddDockedControl(project);
            AddDockedControl(lvdList);
            rightPanel.treeView1.Nodes.Add(animNode);
            rightPanel.treeView1.Nodes.Add(mtaNode);
            Runtime.renderBones = true;
            Runtime.renderLVD = true;
            Runtime.renderFloor = true;
            Runtime.renderHitboxes = true;
            Runtime.renderModel = true;
            Runtime.renderPath = true;
            Runtime.renderCollisions = true;
            Runtime.renderGeneralPoints = true;
            Runtime.renderItemSpawners = true;
            Runtime.renderSpawns = true;
            Runtime.renderRespawns = true;
            Runtime.renderOtherLVDEntries = true;
            
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
            else
                content.Show(dockPanel1);
        }

        #region Members
        public AnimListPanel rightPanel = new AnimListPanel() { ShowHint = DockState.DockRight };
        public BoneTreePanel leftPanel = new BoneTreePanel() { ShowHint = DockState.DockLeft };
        public TreeNode animNode = new TreeNode("Bone Animations");
        public TreeNode mtaNode = new TreeNode("Material Animations");
        public ProjectTree project = new ProjectTree() { ShowHint = DockState.DockLeft };
        public LVDList lvdList = new LVDList() { ShowHint = DockState.DockLeft };
        public LVDEditor lvdEditor = new LVDEditor() { ShowHint = DockState.DockRight };
        public List<PARAMEditor> paramEditors = new List<PARAMEditor>() { };
        public List<MTAEditor> mtaEditors = new List<MTAEditor>() { };
        public List<ACMDEditor> ACMDEditors = new List<ACMDEditor>() { };
        public MeshList meshList = new MeshList() { ShowHint = DockState.DockRight };
        public List<VBNViewport> viewports = new List<VBNViewport>() { new VBNViewport() }; // Default viewport
        #endregion

        #region ToolStripMenu
        private void openNUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PARAMEditor currentParam = null;
            ACMDEditor currentACMD = null;
            foreach(PARAMEditor p in paramEditors)
            {
                if(p.ContainsFocus)
                {
                    currentParam = p;
                }
            }
            foreach(ACMDEditor a in ACMDEditors)
            {
                if (a.ContainsFocus)
                {
                    currentACMD = a;
                }
            }
            if(currentParam != null)
            {
                currentParam.saveAs();
            }
            else
            if(currentACMD != null)
            {
                currentACMD.save();
            }
            else
            {
                string filename = "";
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Supported Filetypes (VBN,LVD)|*.vbn;*.lvd|Smash 4 Boneset|*.vbn|All files(*.*)|*.*";
                DialogResult result = save.ShowDialog();

                if (result == DialogResult.OK)
                {
                    filename = save.FileName;
                    if (filename.EndsWith(".vbn"))
                    {
                        Runtime.TargetVBN.Endian = Endianness.Big;
                        if (!checkBox1.Checked)
                            Runtime.TargetVBN.Endian = Endianness.Little;
                        Runtime.TargetVBN.Save(filename);
                    }
                    if (filename.EndsWith(".lvd") && Runtime.TargetLVD != null)
                        File.WriteAllBytes(filename, Runtime.TargetLVD.Rebuild());
                    //OMO.createOMO (anim, vbn, "C:\\Users\\ploaj_000\\Desktop\\WebGL\\test_outut.omo", -1, -1);
                }
            }
        }

        public static void DAEReadSemantic(int p, Dictionary<string, string> semantic)
        {

        }

        private void openNud(string filename)
        {
            string[] files = Directory.GetFiles(System.IO.Path.GetDirectoryName(filename));

            string pnud = "";
            string pnut = "";
            string pjtb = "";
            string pvbn = "";
            string pmta = "";
            string psb = "";
            string pmoi = "";
            List<string> pacs = new List<string>();

            foreach (string s in files)
            {
                if (s.EndsWith(".nud"))
                    pnud = s;
                if (s.EndsWith(".nut"))
                    pnut = s;
                if (s.EndsWith(".vbn"))
                    pvbn = s;
                if (s.EndsWith(".jtb"))
                    pjtb = s;
                if (s.EndsWith(".mta"))
                    pmta = s;
                if (s.EndsWith(".sb"))
                    psb = s;
                if (s.EndsWith(".moi"))
                    pmoi = s;
                if (s.EndsWith(".pac"))
                    pacs.Add(s);
            }

            ModelContainer model = new ModelContainer();

            if (!pvbn.Equals(""))
            {
                model.vbn = new VBN(pvbn);
                Runtime.TargetVBN = model.vbn;
                if (!pjtb.Equals(""))
                    model.vbn.readJointTable(pjtb);
                if (!psb.Equals(""))
                    model.vbn.swingBones.Read(psb);
            }

            if (!pnut.Equals(""))
            {
                NUT nut = new NUT(pnut);
                Runtime.TextureContainers.Add(nut);
            }

            if (!pnud.Equals(""))
            {
                model.nud = new NUD(pnud);

                //AddDockedControl(new NUDMaterialEditor(model.nud.mesh[0].polygons[0].materials));

                foreach (string s in pacs)
                {
                    PAC p = new PAC();
                    p.Read(s);
                    byte[] data;
                    p.Files.TryGetValue("default.mta", out data);
                    if (data != null) { 
                        MTA m = new MTA();
                        m.read(new FileData(data));
                        model.nud.applyMTA(m, 0);
                    }
                }
            }

            if (!pmta.Equals(""))
            {
                try
                {
                    model.mta = new MTA();
                    model.mta.Read(pmta);
                    viewports[0].loadMTA(model.mta);
                }
                catch (EndOfStreamException)
                {
                    model.mta = null;
                }
            }

            if (!pmoi.Equals(""))
            {
                model.moi = new MOI(pmoi);
            }
            
            if(model.nud != null)
            {
                model.nud.MergePoly();
            }

            Runtime.ModelContainers.Add(model);
            meshList.refresh();
        }

        private void openVBNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Supported Formats(.vbn, .mdl0, .smd, .nud, .lvd, .bin, .dae)|*.vbn;*.mdl0;*.smd;*.lvd;*.nud;*.mtable;*.bin;*.dae;*.dat|" +
                             "Smash 4 Boneset (.vbn)|*.vbn|" +
                             "Namco Model (.nud)|*.nud|" +
                             "Smash 4 Level Data (.lvd)|*.lvd|" +
                             "NW4R Model (.mdl0)|*.mdl0|" +
                             "Source Model (.SMD)|*.smd|" +
                             "Smash 4 Parameters (.bin)|*.bin|"+
                             "Collada Model Format (.dae)|*.dae|"+
                             "All files(*.*)|*.*";

                // "Namco Universal Data Folder (.NUD)|*.nud|" +

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileName.EndsWith(".vbn"))
                    {
                        Runtime.TargetVBN = new VBN(ofd.FileName);

                        if (Directory.Exists("Skapon\\"))
                        {
                            NUD nud = Skapon.Create(Runtime.TargetVBN);
                            ModelContainer con = new ModelContainer();
                            con.vbn = Runtime.TargetVBN;
                            con.nud = nud;
                            nud.PreRender();
                            Runtime.ModelContainers.Add(con);
                        }
                    }

                    if (ofd.FileName.EndsWith(".dat"))
                    {
                        DAT dat = new DAT();
                        dat.Read(new FileData(ofd.FileName));
                        ModelContainer c = new ModelContainer();
                        c.dat_melee = dat;
                        dat.PreRender();
                        //DAT_Animation anim = new DAT_Animation();
                        //anim.Read(new FileData("C:\\Users\\ploaj_000\\Desktop\\Melee\\zJiggyWait"));
                        //anim.Apply(dat.bones);
                        Runtime.ModelContainers.Add(c);
                        //Runtime.ModelContainers.Add(dat.wrapToNUD());
                        //Collada dae = new Collada();
                        //dae.Save("C:\\Users\\ploaj_000\\Desktop\\Melee\\Test.dae", dat);
                        DAT_TreeView p = new DAT_TreeView() { ShowHint = DockState.DockLeft };
                        p.setDAT(dat);
                        AddDockedControl(p);
                        //Runtime.TargetVBN = dat.bones;
                        meshList.refresh();
                    }



                    if (ofd.FileName.EndsWith(".nut"))
                        Runtime.TextureContainers.Add(new NUT(ofd.FileName));

                    if (ofd.FileName.EndsWith(".lvd"))
                    {
                        Runtime.TargetLVD = new LVD(ofd.FileName);
                        LVD test = Runtime.TargetLVD;
                        lvdList.fillList();
                    }

                    if (ofd.FileName.EndsWith(".mta"))
                    {
                        Runtime.TargetMTA = new MTA();
                        Runtime.TargetMTA.Read(ofd.FileName);
                        viewports[0].loadMTA(Runtime.TargetMTA);
                        MTAEditor temp = new MTAEditor(Runtime.TargetMTA) { ShowHint = DockState.Document };
                        temp.Text = Path.GetFileName(ofd.FileName);
                        AddDockedControl(temp);
                        mtaEditors.Add(temp);
                    }

                    if (ofd.FileName.EndsWith(".mtable"))
                    {
                        project.openACMD(ofd.FileName);
                    }

                    if (ofd.FileName.EndsWith("path.bin"))
                    {
                        Runtime.TargetPath = new PathBin(ofd.FileName);
                    }
                    else
                    if (ofd.FileName.EndsWith(".bin"))
                    {
                        //Note to whoever is readin this: 
                        //Eventually we need to look at the magic here (and also make all .bins look at magic)
                        //Runtime.TargetCMR0 = new CMR0();
                        //Runtime.TargetCMR0.read(new FileData(ofd.FileName));
                        PARAMEditor p = new PARAMEditor(ofd.FileName) { ShowHint = DockState.Document };
                        p.Text = Path.GetFileName(ofd.FileName);
                        AddDockedControl(p);
                        paramEditors.Add(p);
                    }

                    if (ofd.FileName.EndsWith(".mdl0"))
                    {
                        MDL0Bones mdl0 = new MDL0Bones();
                        Runtime.TargetVBN = mdl0.GetVBN(new FileData(ofd.FileName));
                    }

                    if (ofd.FileName.EndsWith(".smd"))
                    {
                        Runtime.TargetVBN = new VBN();
                        SMD.read(ofd.FileName, new SkelAnimation(), Runtime.TargetVBN);
                    }
                    //Viewport.Runtime.TargetVBN = Runtime.TargetVBN;

                    if (ofd.FileName.ToLower().EndsWith(".dae"))
                    {
                        DAEImportSettings m = new DAEImportSettings();
                        m.ShowDialog();
                        if (m.exitStatus == DAEImportSettings.Opened)
                        {
                            if (Runtime.ModelContainers.Count < 1)
                                Runtime.ModelContainers.Add(new ModelContainer());

                            Collada.DAEtoNUD(ofd.FileName, Runtime.ModelContainers[0]);

                            // apply settings
                            m.Apply(Runtime.ModelContainers[0].nud);
                            Runtime.ModelContainers[0].nud.MergePoly();

                            meshList.refresh();
                        }
                    }

                    if (ofd.FileName.EndsWith(".mbn"))
                    {
                        MBN m = new MBN();
                        m.Read(ofd.FileName);
                        ModelContainer con = new ModelContainer();
                        BCH b = new BCH();
                        con.bch = b;
                        b.mbn = m;
                        b.Read("C:\\s\\Smash\\extract\\data\\fighter\\lucas\\Ness3DS - h00\\normal.bch");
                        //m.mesh.RemoveAt(m.mesh.Count - 1);
                        //m.mesh.RemoveAt(m.mesh.Count - 2);
                        //m.Save("C:\\s\\Smash\\extract\\data\\fighter\\lucas\\Ness3DS - h00\\rebuild.mbn");
                        Runtime.ModelContainers.Add(con);
                        //m.Save("C:\\s\\Smash\\extract\\data\\fighter\\lucas\\Ness3DS - h00\\test.mbn");
                        /*NUD n = m.toNUD();
                        n.PreRender();
                        n.Save("C:\\s\\Smash\\extract\\data\\fighter\\lucas\\Ness3DS - h00\\mbn.nud");*/
                    }

                    /*if (ofd.FileName.EndsWith(".bch"))
                    {
                        ModelContainer con = new ModelContainer();
                        BCH b = new BCH();
                        b.Read(ofd.FileName);
                        con.bch = b;
                        Runtime.ModelContainers.Add(con);
                    }*/

                    if (ofd.FileName.EndsWith(".nud"))
                    {
                        openNud(ofd.FileName);
                        //File.WriteAllBytes("C:\\s\\Smash\\extract\\data\\fighter\\lucas\\model_new.nud",Runtime.ModelContainers[0].nud.Rebuild());
                        /*PAC p = new PAC();
                        p.Read("C:\\s\\Smash\\extract\\data\\fighter\\lucas\\model\\body\\c00\\material_anime_face.pac");
                        byte[] data;
                        p.Files.TryGetValue ("display.mta", out data);
                        MTA m = new MTA();
                        m.read(new FileData(data));
                        Runtime.TargetNUD.applyMTA (m, 0);


                        p = new PAC();
                        p.Read("C:\\s\\Smash\\extract\\data\\fighter\\lucas\\model\\body\\c00\\material_anime_eyelid.pac");
                        p.Files.TryGetValue ("display.mta", out data);
                        m = new MTA();
                        m.read(new FileData(data));
                        Runtime.TargetNUD.applyMTA (m, 0);*/


                    }

                    if (Runtime.TargetVBN != null)
                    {
                        ModelContainer m = new ModelContainer();
                        m.name = new DirectoryInfo(ofd.FileName).Name;
                        m.vbn = Runtime.TargetVBN;
                        Runtime.ModelContainers.Add(m);

                        if (ofd.FileName.EndsWith(".smd"))
                        {
                            m.nud = SMD.toNUD(ofd.FileName);
                            meshList.refresh();
                        }

                        leftPanel.treeRefresh();
                    }
                    else
                    {
                        foreach (ModelContainer m in Runtime.ModelContainers)
                        {
                            if (m.vbn != null)
                            {
                                Runtime.TargetVBN = Runtime.ModelContainers[0].vbn;
                                break;
                            }
                        }
                    }
                    project.fillTree();
                }
            }
        }

        private void addMaterialAnimation(string name,MTA m)
        {
            Runtime.MaterialAnimations.Add(name, m);
            MainForm.Instance.viewports[0].loadMTA(m);
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Supported Formats|*.omo;*.anim;*.chr0;*.smd;*.mta;*.pac|" +
                             "Object Motion|*.omo|" +
                             "Maya Animation|*.anim|" +
                             "NW4R Animation|*.chr0|" +
                             "Source Animation (SMD)|*.smd|" +
                             "Smash 4 Material Animation (MTA)|*.mta|"+
                             "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    //Runtime.Animations.Clear();
                    if (ofd.FileName.EndsWith(".mta"))
                    {
                        MTA mta = new MTA();
                        try
                        {
                            mta.Read(ofd.FileName);
                            Runtime.MaterialAnimations.Add(ofd.FileName, mta);
                            mtaNode.Nodes.Add(ofd.FileName);
                            MainForm.Instance.viewports[0].loadMTA(mta);
                            Runtime.TargetMTAString = ofd.FileName;
                        }
                        catch (EndOfStreamException)
                        {
                            mta = null;
                        }
                    }
                    else if (ofd.FileName.EndsWith(".smd"))
                    {
                        var anim = new SkelAnimation();
                        if (Runtime.TargetVBN == null)
                            Runtime.TargetVBN = new VBN();
                        SMD.read(ofd.FileName, anim, Runtime.TargetVBN);
                        leftPanel.treeRefresh();
                        Runtime.Animations.Add(ofd.FileName, anim);
                        animNode.Nodes.Add(ofd.FileName);
                    }
                    else if (ofd.FileName.EndsWith(".pac"))
                    {
                        PAC p = new PAC();
                        p.Read(ofd.FileName);

                        foreach (var pair in p.Files)
                        {
                            if (pair.Key.EndsWith(".omo"))
                            {
                                var anim = OMO.read(new FileData(pair.Value));
                                string AnimName = Regex.Match(pair.Key, @"([A-Z][0-9][0-9])(.*)").Groups[0].ToString();
                                //AnimName = pair.Key;
                                //AnimName = AnimName.Remove(AnimName.Length - 4);
                                //AnimName = AnimName.Insert(3, "_");
                                if (!string.IsNullOrEmpty(AnimName))
                                {
                                    animNode.Nodes.Add(AnimName);
                                    Runtime.Animations.Add(AnimName, anim);
                                }
                                else
                                {
                                    animNode.Nodes.Add(pair.Key);
                                    Runtime.Animations.Add(pair.Key, anim);
                                }
                            }
                            else if (pair.Key.EndsWith(".mta"))
                            {
                                MTA mta = new MTA();
                                try
                                {
                                    mta.read(new FileData(pair.Value));
                                    Runtime.MaterialAnimations.Add(pair.Key, mta);
                                    mtaNode.Nodes.Add(pair.Key);
                                }
                                catch (EndOfStreamException)
                                {
                                    mta = null;
                                }
                            }
                        }
                    }
                    //if (Runtime.TargetVBN.bones.Count > 0)
                    //{
                    if (ofd.FileName.EndsWith(".omo"))
                    {
                        Runtime.Animations.Add(ofd.FileName, OMO.read(new FileData(ofd.FileName)));
                        animNode.Nodes.Add(ofd.FileName);
                    }
                    if (ofd.FileName.EndsWith(".chr0"))
                    {
                        Runtime.Animations.Add(ofd.FileName, CHR0.read(new FileData(ofd.FileName), Runtime.TargetVBN));
                        animNode.Nodes.Add(ofd.FileName);
                    }
                    if (ofd.FileName.EndsWith(".anim"))
                    {
                        Runtime.Animations.Add(ofd.FileName, ANIM.read(ofd.FileName, Runtime.TargetVBN));
                        animNode.Nodes.Add(ofd.FileName);
                    }
                    //}
                }
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
                sfd.Filter = "Supported Files (.omo, .anim, .pac, .mta)|*.omo;*.anim;*.pac;*.mta|" +
                             "Object Motion (.omo)|*.omo|" +
                             "Material Animation (.mta)|*.mta|" +
                             "Maya Anim (.anim)|*.anim|" +
                             "OMO Pack (.pac)|*.pac|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    sfd.FileName = sfd.FileName;

                    if (sfd.FileName.EndsWith(".anim"))
                        ANIM.createANIM(sfd.FileName, Runtime.TargetAnim, Runtime.TargetVBN);

                    if (sfd.FileName.EndsWith(".omo"))
                        OMO.createOMO(Runtime.TargetAnim, Runtime.TargetVBN, sfd.FileName);

                    if (sfd.FileName.EndsWith(".pac"))
                    {
                        var pac = new PAC();
                        foreach (var anim in Runtime.Animations)
                        {
                            var bytes = OMO.createOMO(anim.Value, Runtime.TargetVBN);
                            pac.Files.Add(anim.Key, bytes);
                        }
                        pac.Save(sfd.FileName);
                    }

                    if (sfd.FileName.EndsWith(".mta"))
                    {
                        FileOutput f = new FileOutput();
                        f.writeBytes(Runtime.TargetMTA.Rebuild());
                        f.save(sfd.FileName);
                    }
                }
            }
        }
        private void hashMatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            csvHashes csv = new csvHashes("hashTable.csv");
            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if (m.vbn != null)
                {
                    foreach (Bone bone in m.vbn.bones)
                    {
                        for (int i = 0; i < csv.names.Count; i++)
                        {
                            if (csv.names[i] == new string(bone.boneName))
                            {
                                bone.boneId = csv.ids[i];
                            }
                        }
                        if (bone.boneId == 0)
                            bone.boneId = Crc32.Compute(new string(bone.boneName));
                    }
                }
                if (m.bch != null)
                {
                    foreach (BCH.BCH_Model mod in m.bch.models)
                    {
                        foreach (Bone bone in mod.skeleton.bones)
                        {
                            for (int i = 0; i < csv.names.Count; i++)
                            {
                                if (csv.names[i] == new string(bone.boneName))
                                {
                                    bone.boneId = csv.ids[i];
                                }
                            }
                        }
                    }
                }
            }
            //leftPanel.treeRefresh();
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
            Runtime.Animations.Clear();
            Runtime.MaterialAnimations.Clear();
            Runtime.TargetVBN.reset();
        }

        private void importToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog() { Filter = "Motion Table (.mtable)|*.mtable|All Files (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Runtime.Moveset = new MovesetManager(ofd.FileName);
                }
            }
        }
        private void animationPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rightPanel.Show(dockPanel1);
        }
        private void viewportWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //var vp = new VBNViewport();
            //viewports.Add(vp);
            //AddDockedControl(vp);
        }

        private void animationsWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (animationsWindowToolStripMenuItem.Checked)
                rightPanel.Show(dockPanel1);
            else
                rightPanel.Hide();
        }

        private void boneTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (boneTreeToolStripMenuItem.Checked)
                leftPanel.Show(dockPanel1);
            else
                leftPanel.Hide();
        }
        #endregion

        private void addBoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newForm = new AddBone(this);
            newForm.ShowDialog();
            Console.WriteLine("Done");
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
            AddDockedControl(new NUDMaterialEditor(poly) { ShowHint = DockState.Float, Text = name});
        }

        private void openStageToolStripMenuItem_Click(object sender, EventArgs e)
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
                    List<string> nuds = new List<string>();

                    if (Directory.Exists(modelPath))
                    {
                        foreach (string d in Directory.GetDirectories(modelPath))
                        {
                            foreach (string f in Directory.GetFiles(d))
                            {
                                if (f.EndsWith(".nud"))
                                {
                                    //Console.WriteLine(f);
                                    openNud(f);
                                }
                            }
                        }
                    }

                    if (Directory.Exists(paramPath))
                    {
                        Runtime.TargetLVD = null;
                        foreach (string f in Directory.GetFiles(paramPath))
                        {
                            if (f.EndsWith(".lvd") && Runtime.TargetLVD != null)
                            {
                                Runtime.TargetLVD = new LVD(f);
                                lvdList.fillList();
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
                                    Runtime.Animations.Add(f, OMO.read(new FileData(f)));
                                    animNode.Nodes.Add(f);
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
        private void clearWorkspaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Runtime.killWorkspace = true;
        }
        private void renderSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            using (var rndr = new GUI.RenderSettings())
            {
                rndr.ShowDialog();
            }
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
                    project.openACMD($"{ofd.SelectedPath}\\script\\animcmd\\body\\motion.mtable", $"{ofd.SelectedPath}\\motion");
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
                        if(Runtime.ModelContainers[0].dat_melee != null)
                        {
                            ModelContainer m = Runtime.ModelContainers[0].dat_melee.wrapToNUD();
                            m.nud.Save(filename);
                            m.vbn.Save(filename.Replace(".nud", ".vbn"));
                        }
                        else
                            Runtime.ModelContainers[0].nud.Save(filename);
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
            Runtime.TargetLVD.spawns.Add(new Point() { name = "START_00_NEW", subname = "00_NEW" });
            lvdList.fillList();
            
        }

        private void respawnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Runtime.TargetLVD == null)
                Runtime.TargetLVD = new LVD();
            Runtime.TargetLVD.respawns.Add(new Point() { name = "RESPAWN_00_NEW", subname = "00_NEW" });
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

        private void openNUTEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NUTEditor ev = new NUTEditor();
            ev.Show();
        }
    }
}
