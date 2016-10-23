using System;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using WeifenLuo.WinFormsUI.Docking;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace VBN_Editor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            foreach (var vp in viewports)
                AddDockedControl(vp);

            animationsWindowToolStripMenuItem.Checked =
            boneTreeToolStripMenuItem.Checked = true;

            AddDockedControl(rightPanel);
            AddDockedControl(leftPanel);

            Runtime.renderBones = true;
            Runtime.renderLVD = true;
            Runtime.renderFloor = true;
            Runtime.renderHitboxes = true;
            Runtime.renderModel = true;
            Runtime.renderPath = true;
            Runtime.renderCollisions = true;
            Runtime.renderOtherLVDEntries = true;
        }

        private void MainForm_Close(object sender, EventArgs e)
        {
            if (Runtime.TargetNUD != null)
                Runtime.TargetNUD.Destroy();

            foreach(ModelContainer n in Runtime.ModelContainers){
                n.Destroy();
            }
            foreach(NUT n in Runtime.TextureContainers){
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
        private List<VBNViewport> viewports = new List<VBNViewport>() { new VBNViewport() }; // Default viewport
        #endregion

        #region ToolStripMenu
        private void openNUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = "";
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Smash 4 Boneset|*.vbn|All files(*.*)|*.*";
            DialogResult result = save.ShowDialog();

            if (result == DialogResult.OK)
            {
                filename = save.FileName;
                Runtime.TargetVBN.save(filename);
                //OMO.createOMO (anim, vbn, "C:\\Users\\ploaj_000\\Desktop\\WebGL\\test_outut.omo", -1, -1);
            }
        }

        private void openNud(string filename)
        {
            string[] files = Directory.GetFiles(System.IO.Path.GetDirectoryName(filename));

            string pnud = "";
            string pnut = "";
            string pjtb = "";
            string pvbn = "";

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
            }

            ModelContainer model = new ModelContainer();

            if (!pvbn.Equals(""))
            {
                model.vbn = new VBN(pvbn);
                if (!pjtb.Equals(""))
                    model.vbn.readJointTable(pjtb);

            }

            if (!pnut.Equals(""))
            {
                NUT nut = new NUT(new FileData(pnut));
                Runtime.TextureContainers.Add(nut);
            }

            if (!pnud.Equals(""))
                model.nud = new NUD(pnud);

            Runtime.ModelContainers.Add(model);
        }

        private void openVBNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Supported Formats(.vbn, .mdl0, .smd, .nud, .lvd)|*.vbn;*.mdl0;*.smd;*.lvd;*.nud|" +
                             "Smash 4 Boneset (.vbn)|*.vbn|" +
                             "Namco Model (.nud)|*.nud|" +
                             "Smash 4 Level Data (.lvd)|*.lvd|" +
                             "NW4R Model (.mdl0)|*.mdl0|" +
                             "Source Model (.SMD)|*.smd|" +
                             "All files(*.*)|*.*";

                // "Namco Universal Data Folder (.NUD)|*.nud|" +

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileName.EndsWith(".vbn"))
                        Runtime.TargetVBN = new VBN(ofd.FileName);

                    if (ofd.FileName.EndsWith(".lvd"))
                    {
                        Runtime.TargetLVD = new LVD();
                        Runtime.TargetLVD.read(new FileData(ofd.FileName));
                    }

                    if (ofd.FileName.EndsWith("path.bin"))
                    {
                        Runtime.TargetPath = new PathBin();
                        Runtime.TargetPath.read(new FileData(ofd.FileName));
                    }
                    else
                    if (ofd.FileName.EndsWith(".bin"))
                    {
                        Runtime.TargetCMR0 = new CMR0();
                        Runtime.TargetCMR0.read(new FileData(ofd.FileName));
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

                    if (ofd.FileName.EndsWith(".nud"))
                    {

                        openNud(ofd.FileName);
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
                        m.vbn = Runtime.TargetVBN;
                        Runtime.ModelContainers.Add(m);

                        leftPanel.treeRefresh();
                        if (Runtime.TargetVBN.littleEndian)
                        {
                            radioButton2.Checked = true;
                        }
                        else
                        {
                            radioButton1.Checked = true;
                        }
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
                }
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Supported Formats|*.omo;*.anim;*.chr0;*.smd;*.pac|" +
                             "Object Motion|*.omo|" +
                             "Maya Animation|*.anim|" +
                             "NW4R Animation|*.chr0|" +
                             "Source Animation (SMD)|*.smd|" +
                             "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    rightPanel.lstAnims.BeginUpdate();
                    Runtime.Animations.Clear();

                    if (ofd.FileName.EndsWith(".smd"))
                    {
                        var anim = new SkelAnimation();
                        if (Runtime.TargetVBN == null)
                            Runtime.TargetVBN = new VBN();
                        SMD.read(ofd.FileName, anim, Runtime.TargetVBN);
                        leftPanel.treeRefresh();
                        Runtime.Animations.Add(ofd.FileName, anim);
                        rightPanel.lstAnims.Items.Add(ofd.FileName);
                    }
                    else if (ofd.FileName.EndsWith(".pac"))
                    {
                        PAC p = new PAC();
                        p.Read(ofd.FileName);

                        foreach (var pair in p.Files)
                        {
                            var anim = OMO.read(new FileData(pair.Value));
                            string AnimName = Regex.Match(pair.Key, @"([A-Z][0-9][0-9])(.*)").Groups[0].ToString();
                            //AnimName = pair.Key;
                            AnimName = AnimName.Remove(AnimName.Length - 4);
                            AnimName = AnimName.Insert(3, "_");
                            if (!string.IsNullOrEmpty(AnimName))
                            {
                                rightPanel.lstAnims.Items.Add(AnimName);
                                Runtime.Animations.Add(AnimName, anim);
                            }
                            else
                            {
                                rightPanel.lstAnims.Items.Add(pair.Key);
                                Runtime.Animations.Add(pair.Key, anim);
                            }
                        }
                    }
                    //if (Runtime.TargetVBN.bones.Count > 0)
                    //{
                        if (ofd.FileName.EndsWith(".omo"))
                        {
                            Runtime.Animations.Add(ofd.FileName, OMO.read(new FileData(ofd.FileName)));
                            rightPanel.lstAnims.Items.Add(ofd.FileName);
                        }
                        if (ofd.FileName.EndsWith(".chr0"))
                        {
                            Runtime.Animations.Add(ofd.FileName, CHR0.read(new FileData(ofd.FileName), Runtime.TargetVBN));
                            rightPanel.lstAnims.Items.Add(ofd.FileName);
                        }
                        if (ofd.FileName.EndsWith(".anim"))
                        {
                            Runtime.Animations.Add(ofd.FileName, ANIM.read(ofd.FileName, Runtime.TargetVBN));
                            rightPanel.lstAnims.Items.Add(ofd.FileName);
                        }
                    //}
                    rightPanel.lstAnims.EndUpdate();
                }
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (Runtime.TargetVBN == null)
            {
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Supported Files (.omo, .anim, .pac)|*.omo;*.anim;*.pac|" +
                             "Object Motion (.omo)|*.omo|" +
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
                }
            }
        }
        private void hashMatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            csvHashes csv = new csvHashes("hashTable.csv");
            foreach (Bone bone in Runtime.TargetVBN.bones)
            {
                for (int i = 0; i < csv.names.Count; i++)
                {
                    if (csv.names[i] == new string(bone.boneName))
                    {
                        bone.boneId = csv.ids[i];
                    }
                }
            }
            leftPanel.treeRefresh();
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
            rightPanel.lstAnims.Items.Clear();
            Runtime.Animations.Clear();
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
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.TargetVBN.littleEndian = false;
            Runtime.TargetVBN.unk_1 = 1;
            Runtime.TargetVBN.unk_2 = 2;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.TargetVBN.littleEndian = true;
            Runtime.TargetVBN.unk_1 = 2;
            Runtime.TargetVBN.unk_2 = 1;
        }

        private void openStageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new CommonOpenFileDialog())
            {
                ofd.EnsureReadOnly = true;
                ofd.IsFolderPicker = true;
                ofd.AllowNonFileSystemItems = false;
                ofd.Multiselect = false;
                //ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder);
                ofd.Title = "Stage Folder";
                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string stagePath = ofd.FileName;
                    string modelPath = stagePath + "\\model\\";
                    string paramPath = stagePath + "\\param\\";
                    string animationPath = stagePath + "\\animation\\";
                    List<string> nuds = new List<string>();
                    foreach (string d in Directory.GetDirectories(modelPath))
                    {
                        foreach (string f in Directory.GetFiles(d))
                        {
                            if (f.EndsWith(".nud"))
                                openNud(f);
                        }
                    }

                    foreach (string f in Directory.GetFiles(paramPath))
                    {
                        if (f.EndsWith(".lvd"))
                        {
                            Runtime.TargetLVD = new LVD();
                            Runtime.TargetLVD.read(new FileData(f));
                            break;
                        }
                    }

                    foreach (string d in Directory.GetDirectories(animationPath))
                    {
                        foreach(string f in Directory.GetFiles(d))
                        {
                            if (f.EndsWith(".omo"))
                            {
                                Runtime.Animations.Add(f, OMO.read(new FileData(f)));
                                rightPanel.lstAnims.Items.Add(f);
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
    }
}
