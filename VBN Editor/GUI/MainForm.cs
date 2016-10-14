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

namespace VBN_Editor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Application.Idle += AppIdle;

            Animations = new Dictionary<string, SkelAnimation>(); ;
        }

		public VBN TargetVBN { get; set; }
		public NUD TargetNUD { get; set; }
        public SkelAnimation TargetAnim
        {
            get
            {
                if (lstAnims.SelectedItem == null)
                {
                    return null;
                }
                if (Animations.ContainsKey(lstAnims.SelectedItem.ToString()))
                {
                    return Animations[lstAnims.SelectedItem.ToString()];
                }
                else
                {
                    return null;
                }
            }
        }
        public Dictionary<string, SkelAnimation> Animations { get; set; }
        public MovesetManager ACMDManager { get; set; }

        public bool isPlaying = false;
        public DataTable tbl;
        private bool delete = false;
        private string toDelete;
        private string currentNode;

        private TreeNode buildBoneTree(int index)
        {
            treeView1.BeginUpdate();

            List<TreeNode> children = new List<TreeNode>();
            foreach (int i in TargetVBN.bones[index].children)
            {
                children.Add(buildBoneTree(i));
            }

            TreeNode temp = new TreeNode(new string(TargetVBN.bones[index].boneName), children.ToArray());

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

        private void openNUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = "";
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Smash 4 Boneset|*.vbn|All files(*.*)|*.*";
            DialogResult result = save.ShowDialog();

            if (result == DialogResult.OK)
            {
                filename = save.FileName;
                TargetVBN.save(filename);
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
                        TargetVBN = new VBN(ofd.FileName);

                    if (ofd.FileName.EndsWith(".mdl0"))
                    {
                        MDL0Bones mdl0 = new MDL0Bones();
                        TargetVBN = mdl0.GetVBN(new FileData(ofd.FileName));
                    }

                    if (ofd.FileName.EndsWith(".smd"))
                    {
                        TargetVBN = new VBN();
                        SMD.read(ofd.FileName, new SkelAnimation(), TargetVBN);
                    }
                    Viewport.TargetVBN = TargetVBN;

                    treeRefresh();
                    if (TargetVBN.littleEndian)
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
                    lstAnims.BeginUpdate();
                    Animations.Clear();

                    if (ofd.FileName.EndsWith(".smd"))
                    {
                        var anim = new SkelAnimation();
                        if (TargetVBN == null)
                            TargetVBN = new VBN();
                        SMD.read(ofd.FileName, anim, TargetVBN);
                        treeRefresh();
                        Animations.Add(ofd.FileName, anim);
                        lstAnims.Items.Add(ofd.FileName);
                    }
                    else if (ofd.FileName.EndsWith(".pac"))
                    {
                        PAC p = new PAC();
                        p.Read(ofd.FileName);

                        foreach (var pair in p.Files)
                        {
                            var anim = OMO.read(new FileData(pair.Value), TargetVBN);
                            string AnimName = Regex.Match(pair.Key, @"(.*)([A-Z])([0-9][0-9])(.*)").Groups[4].ToString();
                            AnimName = AnimName.TrimEnd(new char[] { '.', 'o', 'm', 'o' });
                            if (!string.IsNullOrEmpty(AnimName))
                            {
                                lstAnims.Items.Add(AnimName);
                                Animations.Add(AnimName, anim);
                            }
                            else
                            {
                                lstAnims.Items.Add(pair.Key);
                                Animations.Add(pair.Key, anim);
                            }
                        }
                    }
                    if (TargetVBN.bones.Count > 0)
                    {
                        if (ofd.FileName.EndsWith(".omo"))
                        {
                            Animations.Add(ofd.FileName, OMO.read(new FileData(ofd.FileName), TargetVBN));
                            lstAnims.Items.Add(ofd.FileName);
                        }
                        if (ofd.FileName.EndsWith(".chr0"))
                        {
                            Animations.Add(ofd.FileName, CHR0.read(new FileData(ofd.FileName), TargetVBN));
                            lstAnims.Items.Add(ofd.FileName);
                        }
                        if (ofd.FileName.EndsWith(".anim"))
                        {
                            Animations.Add(ofd.FileName, ANIM.read(ofd.FileName, TargetVBN));
                            lstAnims.Items.Add(ofd.FileName);
                        }
                    }
                    lstAnims.EndUpdate();
                }
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TargetVBN == null || TargetAnim == null)
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
                        ANIM.createANIM(sfd.FileName, TargetAnim, TargetVBN);

                    if (sfd.FileName.EndsWith(".omo"))
                        OMO.createOMO(TargetAnim, TargetVBN, sfd.FileName);

                    if (sfd.FileName.EndsWith(".pac"))
                    {
                        var pac = new PAC();
                        foreach (var anim in Animations)
                        {
                            var bytes = OMO.createOMO(anim.Value, TargetVBN);
                            pac.Files.Add(anim.Key, bytes);
                        }
                        pac.Save(sfd.FileName);
                    }
                }
            }
        }

        private void AppIdle(object sender, EventArgs e)
        {
            while (Viewport.IsIdle)
            {
                if (isPlaying)
                {
                    if (numericUpDown2.Value < numericUpDown1.Value)
                    {
                        numericUpDown2.Value++;
                    }
                    else if (numericUpDown2.Value == numericUpDown1.Value)
                    {
                        isPlaying = false;
                        button5.Text = "Play";
                    }
                }
				Viewport.Render(TargetVBN, TargetNUD);
                Thread.Sleep(1000 / 60);

                if (delete)
                {
                    TargetVBN.deleteBone(toDelete);
                    treeRefresh();
                    delete = false;
                }
            }
        }

        // loads a skeletal animation into the viewing system
        public void loadAnimation(SkelAnimation a)
        {
            a.setFrame(0);
            a.nextFrame(TargetVBN);
            this.numericUpDown1.Value = a.size() > 1 ? a.size() - 1 : a.size();
            this.numericUpDown2.Value = 0;
        }

        // events for controls
        // this is the function that will actually update the skeleton
        private void NumericUpDown1_ValueChanged(Object sender, EventArgs e)
        {
            if (TargetAnim == null)
                return;

            if (this.numericUpDown2.Value >= TargetAnim.size())
            {
                this.numericUpDown2.Value = 0;
                return;
            }
            if (this.numericUpDown2.Value < 0)
            {
                this.numericUpDown2.Value = TargetAnim.size() - 1;
                return;
            }
            TargetAnim.setFrame((int)this.numericUpDown2.Value);
            TargetAnim.nextFrame(TargetVBN);
            Viewport.Frame = (int)this.numericUpDown2.Value;
        }

        // play and frame increment buttons
        private void button1_Click(object sender, EventArgs e)
        {
            this.numericUpDown2.Value = 0;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (this.numericUpDown2.Value - 1 >= 0)
                this.numericUpDown2.Value -= 1;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (TargetAnim != null)
                this.numericUpDown2.Value = TargetAnim.size() - 1;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (TargetAnim != null)
                this.numericUpDown2.Value += 1;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            // If we're already at final frame and we hit play again
            // start from the beginning of the anim
            if (numericUpDown1.Value == numericUpDown2.Value)
                numericUpDown2.Value = 0;

            isPlaying = !isPlaying;
            if (isPlaying)
                button5.Text = "Pause";
            else
                button5.Text = "Play";
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

            tbl.Rows.Add("Bone Hash", TargetVBN.bone(treeView1.SelectedNode.Text).boneId.ToString("X"));
            tbl.Rows.Add("Bone Type", TargetVBN.bone(treeView1.SelectedNode.Text).boneType);
            tbl.Rows.Add("X Pos", TargetVBN.bone(treeView1.SelectedNode.Text).position[0]);
            tbl.Rows.Add("Y Pos", TargetVBN.bone(treeView1.SelectedNode.Text).position[1]);
            tbl.Rows.Add("Z Pos", TargetVBN.bone(treeView1.SelectedNode.Text).position[2]);
            tbl.Rows.Add("X Rot", TargetVBN.bone(treeView1.SelectedNode.Text).rotation[0]);
            tbl.Rows.Add("Y Rot", TargetVBN.bone(treeView1.SelectedNode.Text).rotation[1]);
            tbl.Rows.Add("Z Rot", TargetVBN.bone(treeView1.SelectedNode.Text).rotation[2]);
            tbl.Rows.Add("X Scale", TargetVBN.bone(treeView1.SelectedNode.Text).scale[0]);
            tbl.Rows.Add("Y Scale", TargetVBN.bone(treeView1.SelectedNode.Text).scale[1]);
            tbl.Rows.Add("Z Scale", TargetVBN.bone(treeView1.SelectedNode.Text).scale[2]);
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Bone editingBone = TargetVBN.bones[TargetVBN.boneIndex(currentNode)];
            editingBone.boneId = (uint)int.Parse(tbl.Rows[0][1].ToString(), System.Globalization.NumberStyles.HexNumber);
            editingBone.boneType = Convert.ToUInt32(tbl.Rows[1][1]);
            TargetVBN.bones[TargetVBN.boneIndex(currentNode)] = editingBone;

            TargetVBN.bone(currentNode).position[0] = Convert.ToSingle(tbl.Rows[2][1]);
            TargetVBN.bone(currentNode).position[1] = Convert.ToSingle(tbl.Rows[3][1]);
            TargetVBN.bone(currentNode).position[2] = Convert.ToSingle(tbl.Rows[4][1]);

            TargetVBN.bone(currentNode).rotation[0] = Convert.ToSingle(tbl.Rows[5][1]);
            TargetVBN.bone(currentNode).rotation[1] = Convert.ToSingle(tbl.Rows[6][1]);
            TargetVBN.bone(currentNode).rotation[2] = Convert.ToSingle(tbl.Rows[7][1]);

            TargetVBN.bone(currentNode).scale[0] = Convert.ToSingle(tbl.Rows[8][1]);
            TargetVBN.bone(currentNode).scale[1] = Convert.ToSingle(tbl.Rows[9][1]);
            TargetVBN.bone(currentNode).scale[2] = Convert.ToSingle(tbl.Rows[10][1]);

            //vbn.update ();
            TargetVBN.reset();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TargetVBN.bones[TargetVBN.boneIndex(currentNode)].boneName = textBox1.Text.ToCharArray();
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
                int oldParent = (int)TargetVBN.bones[TargetVBN.boneIndex(draggedNode.Text)].parentIndex;
                TargetVBN.bones[oldParent].children.Remove(TargetVBN.boneIndex(draggedNode.Text));
                int newParent = TargetVBN.boneIndex(targetNode.Text);
                Bone temp = TargetVBN.bones[TargetVBN.boneIndex(draggedNode.Text)];
                temp.parentIndex = (int)newParent;
                TargetVBN.bones[TargetVBN.boneIndex(draggedNode.Text)] = temp;
                TargetVBN.bones[newParent].children.Add(TargetVBN.boneIndex(draggedNode.Text));

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
            TargetVBN.littleEndian = false;
            TargetVBN.unk_1 = 1;
            TargetVBN.unk_2 = 2;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            TargetVBN.littleEndian = true;
            TargetVBN.unk_1 = 2;
            TargetVBN.unk_2 = 1;
        }

        private void hashMatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            csvHashes csv = new csvHashes("hashTable.csv");
            foreach (Bone bone in TargetVBN.bones)
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

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TargetAnim != null)
            {
                loadAnimation(TargetAnim);
                Viewport.SetTarget(lstAnims.SelectedItem.ToString(), TargetAnim, TargetVBN);
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lstAnims.Items.Clear();
            Animations.Clear();
            TargetVBN.reset();
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

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
