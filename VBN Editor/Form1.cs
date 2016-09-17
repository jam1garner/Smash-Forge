using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace VBN_Editor
{
    public partial class VBNRebuilder : Form
    {
        public VBN vbn;
        public bool vbnSet = false;
        public bool loaded = false;

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
            open.Filter = "Smash 4 Boneset|*.vbn|All files(*.*)|*.*";
            DialogResult result = open.ShowDialog();

            if(result == DialogResult.OK)
            {
                filename = open.FileName;
                vbn = new VBN(filename);
                buildBoneTree(0);
                vbnSet = true;
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
            GL.ClearColor(Color.SkyBlue);
            Application.Idle += AppIdle;
            SetupViewPort();
        }

        private void SetupViewPort()
        {
            int h = glControl1.Height;
            int w = glControl1.Width;
            GL.MatrixMode(MatrixMode.Projection);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Ortho(0, w, 0, h, -1, 1);
            GL.Viewport(0, 0, w, h);
        }

        private void Render()
        {
            if (!loaded)
                return;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (vbnSet)
            {
                foreach (Bone bone in vbn.bones)
                {
                    foreach (int i in bone.children)
                    {
                        GL.Color3(Color.Green);
                        GL.LineWidth(1f);
                        GL.Begin(BeginMode.Lines);
                        GL.Vertex3(vbn.bones[i].position[0], vbn.bones[i].position[1], vbn.bones[i].position[2]);
                        GL.Vertex3(bone.position[0], bone.position[1], bone.position[2]);
                        //GL.Vertex3(0, 0, 0);
                        //GL.Vertex3(100, 100, 0);
                        GL.End();
                    }
                }
            }

            glControl1.SwapBuffers();
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
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            textBox1.Text = treeView1.SelectedNode.Text;
        }
    }
}
