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
                        treeView1.Nodes.Add(new TreeNode(mesh.name) { Tag = mesh, Checked = mesh.isVisible });
                    }
                }
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            ((NUD.Mesh)e.Node.Tag).isVisible = e.Node.Checked;
        }
    }
}
