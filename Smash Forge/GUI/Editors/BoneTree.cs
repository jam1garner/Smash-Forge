using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Smash_Forge
{
    public partial class BoneTreePanel : DockContent
    {
        public BoneTreePanel()
        {
            InitializeComponent();
        }
        public void treeRefresh()
        {
            if (Runtime.TargetVBN == null)
                return;
            treeView1.Nodes.Clear();
            Runtime.TargetVBN.reset();
            treeView1.BeginUpdate();
            foreach (Bone b in Runtime.TargetVBN.bones)
                if (b.Parent == null)
                    treeView1.Nodes.Add(b);
            treeView1.EndUpdate();
            treeView1.ExpandAll();
            listBox1.Items.Clear();
            foreach (var item in Runtime.TargetVBN.bones)
                listBox1.Items.Add(item);
        }

        public void Clear()
        {
            treeRefresh();
            textBox1.Text = "";
            tbl = new DataTable();
            tbl.Rows.Clear();
            tbl.Columns.Clear();
            dataGridView1.DataSource = tbl;
        }

        public DataTable tbl;
        public string currentNode;
        public static Bone selectedBone = null;
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            listBox1.SelectedItem = treeView1.SelectedNode;
            currentNode = treeView1.SelectedNode.Text;
            textBox1.Text = treeView1.SelectedNode.Text;
            tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("Name") { ReadOnly = true });
            tbl.Columns.Add("Value");
            dataGridView1.DataSource = tbl;
            tbl.Rows.Clear();

            selectedBone = Runtime.TargetVBN.bone(treeView1.SelectedNode.Text);

            tbl.Rows.Add("Bone Index", Runtime.TargetVBN.getJTBIndex(treeView1.SelectedNode.Text));
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
            Runtime.TargetVBN.reset();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            /*if(Runtime.TargetVBN != null)
            {
                Runtime.TargetVBN.bones[Runtime.TargetVBN.boneIndex(currentNode)].Text = textBox1.Text.ToCharArray();
                currentNode = textBox1.Text;
                treeView1.SelectedNode.Text = textBox1.Text;
            }*/
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Bone editingBone = (Bone)treeView1.SelectedNode;
            editingBone.boneId = uint.Parse(tbl.Rows[1][1].ToString(), System.Globalization.NumberStyles.HexNumber);
            
            uint.TryParse(tbl.Rows[2][1].ToString(), out editingBone.boneType);

            float.TryParse(tbl.Rows[3][1].ToString(), out editingBone.position[0]);
            float.TryParse(tbl.Rows[4][1].ToString(), out editingBone.position[1]);
            float.TryParse(tbl.Rows[5][1].ToString(), out editingBone.position[2]);

            float.TryParse(tbl.Rows[6][1].ToString(), out editingBone.rotation[0]);
            float.TryParse(tbl.Rows[7][1].ToString(), out editingBone.rotation[1]);
            float.TryParse(tbl.Rows[8][1].ToString(), out editingBone.rotation[2]);

            float.TryParse(tbl.Rows[11][1].ToString(), out editingBone.scale[0]);
            float.TryParse(tbl.Rows[10][1].ToString(), out editingBone.scale[1]);
            float.TryParse(tbl.Rows[9][1].ToString(), out editingBone.scale[2]);

            //vbn.update ();
            Runtime.TargetVBN.reset();
        }

        private bool isAChildOfB(TreeNode a, TreeNode b)
        {
            return (a.Parent != null && (a.Parent == b || isAChildOfB(a.Parent, b)));
        }
        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            System.Drawing.Point targetPoint = treeView1.PointToClient(new System.Drawing.Point(e.X, e.Y));

            TreeNode targetNode = treeView1.GetNodeAt(targetPoint);

            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(Bone));

            if (!draggedNode.Equals(targetNode) && targetNode != null && !isAChildOfB(targetNode, draggedNode))
            {
                int oldParent = (int)Runtime.TargetVBN.bones[Runtime.TargetVBN.boneIndex(draggedNode.Text)].parentIndex;
                //Runtime.TargetVBN.bones[oldParent].children.Remove(Runtime.TargetVBN.boneIndex(draggedNode.Text));
                int newParent = Runtime.TargetVBN.boneIndex(targetNode.Text);
                Bone temp = Runtime.TargetVBN.bones[Runtime.TargetVBN.boneIndex(draggedNode.Text)];
                temp.parentIndex = (int)newParent;
                Runtime.TargetVBN.bones[Runtime.TargetVBN.boneIndex(draggedNode.Text)] = temp;
                //Runtime.TargetVBN.bones[newParent].children.Add(Runtime.TargetVBN.boneIndex(draggedNode.Text));

                draggedNode.Remove();
                targetNode.Nodes.Add(draggedNode);

                targetNode.Expand();
            }
            if (targetNode == null)
            {
                draggedNode.Remove();
                treeView1.Nodes.Add(draggedNode);
            }
            Runtime.TargetVBN.reset();
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
            //if (e.KeyCode == Keys.Delete)
            //{
            //    delete = true;
            //    toDelete = treeView1.SelectedNode.Text;
            //}

            //Deleting is currently broken... gotta find a fix
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.listBox1.SelectedItem == null) return;
            this.listBox1.DoDragDrop(this.listBox1.SelectedItem, DragDropEffects.Move);
        }

        private void listBox1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            System.Drawing.Point point = listBox1.PointToClient(new System.Drawing.Point(e.X, e.Y));
            int index = this.listBox1.IndexFromPoint(point);
            if (index < 0) index = this.listBox1.Items.Count - 1;
            object data = listBox1.SelectedItem;
            this.listBox1.Items.Remove(data);
            this.listBox1.Items.Insert(index, data);
            Runtime.TargetVBN.bones.Clear();
            foreach (var item in listBox1.Items)
                Runtime.TargetVBN.bones.Add((Bone)item);
        }

        private void selectBone(object bone, TreeNode t)
        {
            if (t == bone)
                treeView1.SelectedNode = t;
            foreach (TreeNode child in t.Nodes)
                selectBone(bone, child);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (TreeNode child in treeView1.Nodes)
                selectBone(listBox1.SelectedItem, child);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (treeView1.SelectedNode == null)
                removeBoneToolStripMenuItem.Visible = false;
            else
                removeBoneToolStripMenuItem.Visible = true;
        }

        private void addBoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bone parent = null;
            if (treeView1.SelectedNode != null)
                parent = (Bone) treeView1.SelectedNode;
            new AddBone(parent).Show();
        }

        private void removeBoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
                return;
            Runtime.TargetVBN.bones.Remove((Bone) treeView1.SelectedNode);
            // Reassign sub-bones to their new parent
            Bone parentBone = (Bone)treeView1.SelectedNode.Parent;
            if (parentBone != null)
            {
                int parentIndex = Runtime.TargetVBN.bones.IndexOf(parentBone);
                foreach (Bone b in treeView1.SelectedNode.Nodes)
                    b.parentIndex = parentIndex;
            }
            treeView1.SelectedNode.Remove();
            treeRefresh();
        }
    }
}
