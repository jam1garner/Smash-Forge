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

        public void refresh()
        {
            treeView1.Nodes.Clear();
            foreach(ModelContainer m in Runtime.ModelContainers)
            {
                if (m.nud != null)
                {
                    foreach (NUD.Mesh mesh in m.nud.mesh)
                    {
                        TreeNode tempMesh = new TreeNode(mesh.name) { Tag = mesh, Checked = mesh.isVisible };
                        treeView1.Nodes.Add(tempMesh);
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
            if (e.Node.Tag is NUD.Mesh)
                ((NUD.Mesh)e.Node.Tag).isVisible = e.Node.Checked;
            else if (e.Node.Tag is NUD.Polygon)
                ((NUD.Polygon)e.Node.Tag).isVisible = e.Node.Checked;
        }

        private void polySelected(NUD.Polygon poly, string name)
        {
            //DO STUFF HERE PLOAJ
            MainForm.Instance.openMats(poly,name);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is NUD.Polygon)
                polySelected((NUD.Polygon)e.Node.Tag, $"{e.Node.Parent.Text} {e.Node.Text}");
        }
    }
}
