using System;
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

namespace VBN_Editor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Application.Idle += AppIdle;
        }

        public DataTable tbl;
        private bool delete = false;
        private string toDelete;
        private string currentNode;

        private AnimListPanel rightPanel = new AnimListPanel() { ShowHint = DockState.DockRight };
        private List<VBNViewport> viewports = new List<VBNViewport>() { new VBNViewport() }; // Default viewport

        private TreeNode buildBoneTree(int index)
        {
            treeView1.BeginUpdate();

            List<TreeNode> children = new List<TreeNode>();
            foreach (int i in Runtime.TargetVBN.bones[index].children)
            {
                children.Add(buildBoneTree(i));
            }

            TreeNode temp = new TreeNode(new string(Runtime.TargetVBN.bones[index].boneName), children.ToArray());

            if (index == 0)
                treeView1.Nodes.Add(temp);

            temp.Expand();
            foreach (TreeNode t in children)
                t.Expand();

            treeView1.EndUpdate();
            return temp;

        }

        public void treeRefresh()
        {
            treeView1.Nodes.Clear();
            buildBoneTree(0);
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

        private void openVBNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Supported Formats(.vbn, .mdl0, .smd)|*.vbn;*.mdl0;*.smd|" +
                             "Smash 4 Boneset (.vbn)|*.vbn|" +
                             "NW4R Model (.mdl0)|*.mdl0|" +
                             "Source Model (.SMD)|*.smd|" +
                             "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileName.EndsWith(".vbn"))
                        Runtime.TargetVBN = new VBN(ofd.FileName);

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

                    treeRefresh();
                    if (Runtime.TargetVBN.littleEndian)
                    {
                        radioButton2.Checked = true;
                    }
                    else
                    {
                        radioButton1.Checked = true;
                    }
                    //loadAnimation (ANIM.read ("C:\\Users\\ploaj_000\\Desktop\\WebGL\\Wait1.anim", vbn));
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
                        treeRefresh();
                        Runtime.Animations.Add(ofd.FileName, anim);
                        rightPanel.lstAnims.Items.Add(ofd.FileName);
                    }
                    else if (ofd.FileName.EndsWith(".pac"))
                    {
                        PAC p = new PAC();
                        p.Read(ofd.FileName);

                        foreach (var pair in p.Files)
                        {
                            var anim = OMO.read(new FileData(pair.Value), Runtime.TargetVBN);
                            string AnimName = Regex.Match(pair.Key, @"(.*)([A-Z])([0-9][0-9])(.*)").Groups[4].ToString();
                            AnimName = AnimName.TrimEnd(new char[] { '.', 'o', 'm', 'o' });
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
                    if (Runtime.TargetVBN.bones.Count > 0)
                    {
                        if (ofd.FileName.EndsWith(".omo"))
                        {
                            Runtime.Animations.Add(ofd.FileName, OMO.read(new FileData(ofd.FileName), Runtime.TargetVBN));
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
                    }
                    rightPanel.lstAnims.EndUpdate();
                }
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Runtime.TargetVBN == null || Runtime.TargetVBN == null)
                return;

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

        private void AppIdle(object sender, EventArgs e)
        {
            if (delete)
            {
                Runtime.TargetVBN.deleteBone(toDelete);
                treeRefresh();
                delete = false;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currentNode = treeView1.SelectedNode.Text;
            textBox1.Text = treeView1.SelectedNode.Text;
            tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("Name") { ReadOnly = true });
            tbl.Columns.Add("Value");
            dataGridView1.DataSource = tbl;
            tbl.Rows.Clear();

            tbl.Rows.Add("Bone Hash", Runtime.TargetVBN.bone(treeView1.SelectedNode.Text).boneId.ToString("X"));
            tbl.Rows.Add("Bone Type", Runtime.TargetVBN.bone(treeView1.SelectedNode.Text).boneType);
            tbl.Rows.Add("X Pos", Runtime.TargetVBN.bone(treeView1.SelectedNode.Text).position[0]);
            tbl.Rows.Add("Y Pos", Runtime.TargetVBN.bone(treeView1.SelectedNode.Text).position[1]);
            tbl.Rows.Add("Z Pos", Runtime.TargetVBN.bone(treeView1.SelectedNode.Text).position[2]);
            tbl.Rows.Add("X Rot", Runtime.TargetVBN.bone(treeView1.SelectedNode.Text).rotation[0]);
            tbl.Rows.Add("Y Rot", Runtime.TargetVBN.bone(treeView1.SelectedNode.Text).rotation[1]);
            tbl.Rows.Add("Z Rot", Runtime.TargetVBN.bone(treeView1.SelectedNode.Text).rotation[2]);
            tbl.Rows.Add("X Scale", Runtime.TargetVBN.bone(treeView1.SelectedNode.Text).scale[0]);
            tbl.Rows.Add("Y Scale", Runtime.TargetVBN.bone(treeView1.SelectedNode.Text).scale[1]);
            tbl.Rows.Add("Z Scale", Runtime.TargetVBN.bone(treeView1.SelectedNode.Text).scale[2]);
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Bone editingBone = Runtime.TargetVBN.bones[Runtime.TargetVBN.boneIndex(currentNode)];
            editingBone.boneId = (uint)int.Parse(tbl.Rows[0][1].ToString(), System.Globalization.NumberStyles.HexNumber);
            editingBone.boneType = Convert.ToUInt32(tbl.Rows[1][1]);
            Runtime.TargetVBN.bones[Runtime.TargetVBN.boneIndex(currentNode)] = editingBone;

            Runtime.TargetVBN.bone(currentNode).position[0] = Convert.ToSingle(tbl.Rows[2][1]);
            Runtime.TargetVBN.bone(currentNode).position[1] = Convert.ToSingle(tbl.Rows[3][1]);
            Runtime.TargetVBN.bone(currentNode).position[2] = Convert.ToSingle(tbl.Rows[4][1]);

            Runtime.TargetVBN.bone(currentNode).rotation[0] = Convert.ToSingle(tbl.Rows[5][1]);
            Runtime.TargetVBN.bone(currentNode).rotation[1] = Convert.ToSingle(tbl.Rows[6][1]);
            Runtime.TargetVBN.bone(currentNode).rotation[2] = Convert.ToSingle(tbl.Rows[7][1]);

            Runtime.TargetVBN.bone(currentNode).scale[0] = Convert.ToSingle(tbl.Rows[8][1]);
            Runtime.TargetVBN.bone(currentNode).scale[1] = Convert.ToSingle(tbl.Rows[9][1]);
            Runtime.TargetVBN.bone(currentNode).scale[2] = Convert.ToSingle(tbl.Rows[10][1]);

            //vbn.update ();
            Runtime.TargetVBN.reset();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Runtime.TargetVBN.bones[Runtime.TargetVBN.boneIndex(currentNode)].boneName = textBox1.Text.ToCharArray();
            currentNode = textBox1.Text;
            treeView1.SelectedNode.Text = textBox1.Text;
        }

        private void addBoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newForm = new AddBone(this);
            newForm.ShowDialog();
        }

        private bool isAChildOfB(TreeNode a, TreeNode b)
        {
            return (a.Parent != null && (a.Parent == b || isAChildOfB(a.Parent, b)));
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            Point targetPoint = treeView1.PointToClient(new Point(e.X, e.Y));

            TreeNode targetNode = treeView1.GetNodeAt(targetPoint);

            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (!draggedNode.Equals(targetNode) && targetNode != null && !isAChildOfB(targetNode, draggedNode))
            {
                int oldParent = (int)Runtime.TargetVBN.bones[Runtime.TargetVBN.boneIndex(draggedNode.Text)].parentIndex;
                Runtime.TargetVBN.bones[oldParent].children.Remove(Runtime.TargetVBN.boneIndex(draggedNode.Text));
                int newParent = Runtime.TargetVBN.boneIndex(targetNode.Text);
                Bone temp = Runtime.TargetVBN.bones[Runtime.TargetVBN.boneIndex(draggedNode.Text)];
                temp.parentIndex = (int)newParent;
                Runtime.TargetVBN.bones[Runtime.TargetVBN.boneIndex(draggedNode.Text)] = temp;
                Runtime.TargetVBN.bones[newParent].children.Add(Runtime.TargetVBN.boneIndex(draggedNode.Text));

                draggedNode.Remove();
                targetNode.Nodes.Add(draggedNode);

                targetNode.Expand();
            }
        }

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                delete = true;
                toDelete = treeView1.SelectedNode.Text;
            }

            //Deleting is currently broken... gotta find a fix
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
            treeRefresh();
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
                    Viewport.Moveset = new MovesetManager(ofd.FileName);
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            foreach (var vp in viewports)
                AddDockedControl(vp);

            AddDockedControl(rightPanel);
        }

        private void animationPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rightPanel.Show(dockPanel1);
        }
    }
}
