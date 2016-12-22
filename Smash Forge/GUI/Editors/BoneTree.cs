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
            treeView1.Nodes.Clear();
            buildBoneTree(0);
        }
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

        public DataTable tbl;
        public string currentNode;
        public static Bone selectedBone = null;
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
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
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if(Runtime.TargetVBN != null)
            {
                Runtime.TargetVBN.bones[Runtime.TargetVBN.boneIndex(currentNode)].boneName = textBox1.Text.ToCharArray();
                currentNode = textBox1.Text;
                treeView1.SelectedNode.Text = textBox1.Text;
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Bone editingBone = Runtime.TargetVBN.bones[Runtime.TargetVBN.boneIndex(currentNode)];
            editingBone.boneId = (uint)int.Parse(tbl.Rows[1][1].ToString(), System.Globalization.NumberStyles.HexNumber);
            editingBone.boneType = Convert.ToUInt32(tbl.Rows[2][1]);
            Runtime.TargetVBN.bones[Runtime.TargetVBN.boneIndex(currentNode)] = editingBone;

            Runtime.TargetVBN.bone(currentNode).position[0] = Convert.ToSingle(tbl.Rows[3][1]);
            Runtime.TargetVBN.bone(currentNode).position[1] = Convert.ToSingle(tbl.Rows[4][1]);
            Runtime.TargetVBN.bone(currentNode).position[2] = Convert.ToSingle(tbl.Rows[5][1]);

            Runtime.TargetVBN.bone(currentNode).rotation[0] = Convert.ToSingle(tbl.Rows[6][1]);
            Runtime.TargetVBN.bone(currentNode).rotation[1] = Convert.ToSingle(tbl.Rows[7][1]);
            Runtime.TargetVBN.bone(currentNode).rotation[2] = Convert.ToSingle(tbl.Rows[8][1]);

            Runtime.TargetVBN.bone(currentNode).scale[0] = Convert.ToSingle(tbl.Rows[9][1]);
            Runtime.TargetVBN.bone(currentNode).scale[1] = Convert.ToSingle(tbl.Rows[10][1]);
            Runtime.TargetVBN.bone(currentNode).scale[2] = Convert.ToSingle(tbl.Rows[11][1]);

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
            //if (e.KeyCode == Keys.Delete)
            //{
            //    delete = true;
            //    toDelete = treeView1.SelectedNode.Text;
            //}

            //Deleting is currently broken... gotta find a fix
        }
    }
}
