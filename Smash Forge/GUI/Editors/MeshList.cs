using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Smash_Forge
{
    public partial class MeshList : DockContent
    {
        public MeshList()
        {
            InitializeComponent();
            refresh();
        }

        bool changingValue = false;

        public void refresh()
        {
            treeView1.Nodes.Clear();
            int j = 0;
            foreach(ModelContainer m in Runtime.ModelContainers)
            {
                if (m.nud != null)
                {
                    TreeNode model = new TreeNode($"Model {j}") { Tag = m.nud };
                    treeView1.Nodes.Add(model);
                    j++; 
                    foreach (NUD.Mesh mesh in m.nud.mesh)
                    {
                        TreeNode tempMesh = new TreeNode(mesh.name) { Tag = mesh, Checked = mesh.isVisible };
                        model.Nodes.Add(tempMesh);
                        int i = 0;
                        foreach(NUD.Polygon poly in mesh.polygons)
                        {
                            tempMesh.Nodes.Add(new TreeNode($"Polygon {i}") { Tag = poly, Checked = poly.isVisible });
                            i++;
                        }
                    }
                }
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is NUD.Mesh) {
                ((NUD.Mesh)e.Node.Tag).isVisible = e.Node.Checked;
                
            }
            else if (e.Node.Tag is NUD.Polygon) {
                ((NUD.Polygon)e.Node.Tag).isVisible = e.Node.Checked;
            }
            else if (e.Node.Tag is NUD){
                foreach (NUD.Mesh mesh in ((NUD)e.Node.Tag).mesh)
                {
                    mesh.isVisible = e.Node.Checked;
                    foreach(NUD.Polygon poly in mesh.polygons)
                    {
                        poly.isVisible = e.Node.Checked;
                    }
                }
                foreach (TreeNode meshNode in e.Node.Nodes)
                {
                    meshNode.Checked = e.Node.Checked;
                    foreach (TreeNode polyNode in meshNode.Nodes)
                        polyNode.Checked = e.Node.Checked;
                }
            }
        }

        private void polySelected(NUD.Polygon poly, string name)
        {
            MainForm.Instance.openMats(poly,name);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is NUD.Polygon)
                polySelected((NUD.Polygon)e.Node.Tag, $"{e.Node.Parent.Text} {e.Node.Text}");
            if (e.Node.Tag is NUD.Mesh)
            {
                changingValue = true;//Since we are changing value but we don't want the entire model order to swap we are disabling the event for on change value temporarily
                numericUpDown1.Value = ((NUD)e.Node.Parent.Tag).mesh.IndexOf((NUD.Mesh)e.Node.Tag);
                numericUpDown1.Maximum = ((NUD)e.Node.Parent.Tag).mesh.Count - 1;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if(treeView1.SelectedNode.Tag is NUD.Mesh && !changingValue)
            {
                int pos = (int)numericUpDown1.Value;
                TreeNode node = treeView1.SelectedNode;
                TreeNode parent = node.Parent;
                NUD.Mesh m = (NUD.Mesh)node.Tag;
                NUD n = (NUD)parent.Tag;
                n.mesh.Remove(m);
                n.mesh.Insert(pos, m);
                parent.Nodes.Remove(node);
                parent.Nodes.Insert(pos, node);
                treeView1.SelectedNode = node;
                n.PreRender();
            }
            changingValue = false;//Set the value back so the user can change values
        }

        private void treeView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '=')
            {
                if (treeView1.SelectedNode.Tag is NUD.Mesh)
                {
                    TreeNode node = treeView1.SelectedNode;
                    TreeNode parent = node.Parent;
                    NUD.Mesh m = (NUD.Mesh)node.Tag;
                    NUD n = (NUD)parent.Tag;
                    int pos = n.mesh.IndexOf(m) + 1;
                    if (pos >= n.mesh.Count)
                        pos = n.mesh.Count - 1;
                    n.mesh.Remove(m);
                    n.mesh.Insert(pos, m);
                    parent.Nodes.Remove(node);
                    parent.Nodes.Insert(pos, node);
                    treeView1.SelectedNode = node;
                    n.PreRender();
                }
            }
            if (e.KeyChar == '-')
            {
                if (treeView1.SelectedNode.Tag is NUD.Mesh)
                {
                    TreeNode node = treeView1.SelectedNode;
                    TreeNode parent = node.Parent;
                    NUD.Mesh m = (NUD.Mesh)node.Tag;
                    NUD n = (NUD)parent.Tag;
                    int pos = n.mesh.IndexOf(m) - 1;
                    if (pos < 0)
                        pos = 0;
                    n.mesh.Remove(m);
                    n.mesh.Insert(pos, m);
                    parent.Nodes.Remove(node);
                    parent.Nodes.Insert(pos, node);
                    treeView1.SelectedNode = node;
                    n.PreRender();
                }
            }
        }
    }
}
