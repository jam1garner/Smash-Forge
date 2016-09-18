using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace VBN_Editor
{
    public partial class VBNRebuilder : Form
    {
        public VBN vbn;
        public bool vbnSet = false;
        public bool loaded = false;
        public DataTable tbl;
        private bool delete = false;
        private string toDelete;

        public VBNRebuilder()
        {
            InitializeComponent();
        }
        
        private TreeNode buildBoneTree(int index)
        {
            List<TreeNode> children = new List<TreeNode>();
            foreach (int i in vbn.bones[index].children)
            {
                children.Add(buildBoneTree(i));
            }
            
            TreeNode temp = new TreeNode(new string(vbn.bones[index].boneName),children.ToArray());

            if (index == 0)
                treeView1.Nodes.Add(temp);

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

            if(result == DialogResult.OK)
            {
                filename = save.FileName;
                vbn.save(filename);
            }
        }

        private void openVBNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = "";
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Smash 4 Boneset|*.vbn|COLLADA|*.dae|All files(*.*)|*.*";
            DialogResult result = open.ShowDialog();

            if(result == DialogResult.OK)
            {
                filename = open.FileName;
                if (filename.EndsWith("dae") || filename.EndsWith("DAE"))
                {
                    vbn = new VBN();
                    //vbn
                }
                else
                {
                    vbn = new VBN(filename);
                    treeRefresh();
                    vbnSet = true;
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newForm = new Form2 ();
            newForm.ShowDialog();
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            loaded = true;
            GL.ClearColor(Color.White);
            Application.Idle += AppIdle;
            SetupViewPort();
        }

        // for drawing
        public static Matrix4 scale = Matrix4.CreateScale(new Vector3(0.5f, 0.5f, 0.5f));
        Matrix4 v;
        float rot = 0;
        float mouseLast = 0;

        private void SetupViewPort()
        {
            int h = glControl1.Height;
            int w = glControl1.Width;
            GL.LoadIdentity();
            GL.Viewport(0, 0, w, h);
            v = Matrix4.CreateRotationX(0.2f) * Matrix4.CreateTranslation(0, -5f, -15f) * Matrix4.CreatePerspectiveFieldOfView(1.3f, glControl1.Width / (float)glControl1.Height, 1.0f, 40.0f);
        }

        private void Render()
        {
            if (!loaded)
                return;
            // clear the gf buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // enable depth test for grid...
            GL.Enable(EnableCap.DepthTest);

            // set up the viewport projection and send it to GPU
            GL.MatrixMode(MatrixMode.Projection);

            if (OpenTK.Input.Mouse.GetState().IsButtonDown(OpenTK.Input.MouseButton.Left))
            {
                v = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f) * Matrix4.CreateTranslation(0, -5f, -15f) * Matrix4.CreatePerspectiveFieldOfView(1.3f, glControl1.Width / (float)glControl1.Height, 1.0f, 40.0f);
                rot += 0.05f * (OpenTK.Input.Mouse.GetState().X - mouseLast);
            }
            mouseLast = OpenTK.Input.Mouse.GetState().X;
            GL.LoadMatrix(ref v);

            // ready to start drawing model stuff
            GL.MatrixMode(MatrixMode.Modelview);

            // draw the grid floor first
            drawGridFloor(Matrix4.CreateTranslation(Vector3.Zero));

            // clear the buffer bit so the skeleton will be drawn
            // on top of everything
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // drawing the bones
            if (vbnSet)
            {
                foreach (Bone bone in vbn.bones)
                {
                    // first calcuate the point and draw a point
                    GL.PointSize(3.5f);
                    GL.Begin(PrimitiveType.Points);
                    Vector3 pos_c = Vector3.Transform(Vector3.Zero, bone.transform * scale);
                    GL.Vertex3(pos_c);
                    GL.End();

                    // now draw line between parent
                    GL.Color3(Color.Blue);
                    GL.LineWidth(1f);

                    GL.Begin(PrimitiveType.Lines);
                    if (bone.parentIndex != 0x0FFFFFFF)
                    {
                        uint i = bone.parentIndex;
                        Vector3 pos_p = Vector3.Transform(Vector3.Zero, vbn.bones[(int)i].transform * scale);
                        GL.Vertex3(pos_c);
                        GL.Vertex3(pos_p);
                    }
                    GL.End();

                }
            }

            glControl1.SwapBuffers();
        }


        public void drawGridFloor(Matrix4 s)
        {

            // Dropping some grid lines
            GL.Color3(Color.Blue);
            GL.LineWidth(1f);
            GL.Begin(PrimitiveType.Lines);
            for (var i = -10; i <= 10; i++)
            {
                GL.Vertex3(Vector3.Transform(new Vector3(-10f, 0f, i), s));
                GL.Vertex3(Vector3.Transform(new Vector3(10f, 0f, i), s));
                GL.Vertex3(Vector3.Transform(new Vector3(i, 0f, -10f), s));
                GL.Vertex3(Vector3.Transform(new Vector3(i, 0f, 10f), s));
            }
            GL.End();
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded)
                return;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            glControl1.SwapBuffers();
        }

        private void AppIdle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                Render();
                if (delete)
                {
                    vbn.deleteBone(toDelete);
                    treeRefresh();
                    delete = false;
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            textBox1.Text = treeView1.SelectedNode.Text;
            tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("Name") { ReadOnly = true });
            tbl.Columns.Add("Value");
            dataGridView1.DataSource = tbl;
            tbl.Rows.Clear();

            tbl.Rows.Add("Bone Hash",vbn.bone(treeView1.SelectedNode.Text).boneId.ToString("X"));
            tbl.Rows.Add("Bone Type", vbn.bone(treeView1.SelectedNode.Text).boneType);
            tbl.Rows.Add("X Pos", vbn.bone(treeView1.SelectedNode.Text).position[0]);
            tbl.Rows.Add("Y Pos", vbn.bone(treeView1.SelectedNode.Text).position[1]);
            tbl.Rows.Add("Z Pos", vbn.bone(treeView1.SelectedNode.Text).position[2]);
            tbl.Rows.Add("X Rot", vbn.bone(treeView1.SelectedNode.Text).rotation[0]);
            tbl.Rows.Add("Y Rot", vbn.bone(treeView1.SelectedNode.Text).rotation[1]);
            tbl.Rows.Add("Z Rot", vbn.bone(treeView1.SelectedNode.Text).rotation[2]);
            tbl.Rows.Add("X Scale", vbn.bone(treeView1.SelectedNode.Text).scale[0]);
            tbl.Rows.Add("Y Scale", vbn.bone(treeView1.SelectedNode.Text).scale[1]);
            tbl.Rows.Add("Z Scale", vbn.bone(treeView1.SelectedNode.Text).scale[2]);
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Bone editingBone = vbn.bones[vbn.boneIndex(treeView1.SelectedNode.Text)];
            editingBone.boneId = (uint)int.Parse(tbl.Rows[0][1].ToString(), System.Globalization.NumberStyles.HexNumber);
            editingBone.boneType = Convert.ToUInt32(tbl.Rows[1][1]);
            vbn.bones[vbn.boneIndex(treeView1.SelectedNode.Text)] = editingBone;

            vbn.bone(treeView1.SelectedNode.Text).position[0] = Convert.ToSingle(tbl.Rows[2][1]);
            vbn.bone(treeView1.SelectedNode.Text).position[1] = Convert.ToSingle(tbl.Rows[3][1]);
            vbn.bone(treeView1.SelectedNode.Text).position[2] = Convert.ToSingle(tbl.Rows[4][1]);

            vbn.bone(treeView1.SelectedNode.Text).rotation[0] = Convert.ToSingle(tbl.Rows[5][1]);
            vbn.bone(treeView1.SelectedNode.Text).rotation[1] = Convert.ToSingle(tbl.Rows[6][1]);
            vbn.bone(treeView1.SelectedNode.Text).rotation[2] = Convert.ToSingle(tbl.Rows[7][1]);

            vbn.bone(treeView1.SelectedNode.Text).scale[0] = Convert.ToSingle(tbl.Rows[8][1]);
            vbn.bone(treeView1.SelectedNode.Text).scale[1] = Convert.ToSingle(tbl.Rows[9][1]);
            vbn.bone(treeView1.SelectedNode.Text).scale[2] = Convert.ToSingle(tbl.Rows[10][1]);

            vbn.update();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Bone temp = vbn.bones[vbn.boneIndex(treeView1.SelectedNode.Text)];
            temp.boneName = textBox1.Text.ToCharArray();
            vbn.bones[vbn.boneIndex(treeView1.SelectedNode.Text)] = temp;
            treeView1.SelectedNode.Text = textBox1.Text;
        }

        private void addBoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newForm = new Form3(this);
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
                int oldParent = (int)vbn.bones[vbn.boneIndex(draggedNode.Text)].parentIndex;
                vbn.bones[oldParent].children.Remove(vbn.boneIndex(draggedNode.Text));
                int newParent = vbn.boneIndex(targetNode.Text);
                Bone temp = vbn.bones[vbn.boneIndex(draggedNode.Text)];
                temp.parentIndex = (uint)newParent;
                vbn.bones[vbn.boneIndex(draggedNode.Text)] = temp;
                vbn.bones[newParent].children.Add(vbn.boneIndex(draggedNode.Text));

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
            /*if(e.KeyCode == Keys.Delete)
            {
                delete = true;
                toDelete = treeView1.SelectedNode.Text;
            }*/

            //Deleting is currently broken... gotta find a fix
        }
    }
}
