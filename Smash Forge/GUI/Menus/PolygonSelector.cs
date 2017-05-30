using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge
{
    public partial class PolygonSelector : Form
    {
        public PolygonSelector()
        {
            InitializeComponent();
        }

        public List<NUD.Polygon> selected = new List<NUD.Polygon>();
        public bool finished = false;

        private void PolygonSelector_Load(object sender, EventArgs e)
        {
            int modelCount = 0;
            foreach (ModelContainer mc in Runtime.ModelContainers)
            {
                if (mc.nud != null)
                {
                    TreeNode model = new TreeNode($"Model {modelCount}") { Tag = mc.nud };
                    foreach (NUD.Mesh m in mc.nud.mesh)
                    {
                        TreeNode mesh = new TreeNode(m.Text) { Tag = m };
                        foreach (NUD.Polygon p in m.Nodes)
                        {
                            TreeNode poly = new TreeNode(p.Text) { Tag = p };
                            mesh.Nodes.Add(poly);
                        }
                        model.Nodes.Add(mesh);
                    }
                    treeView1.Nodes.Add(model);
                }
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            bool isChecked = (e.Node).Checked;
            if (e.Node.Tag is NUD)
            {
                foreach (TreeNode mesh in e.Node.Nodes)
                {
                    mesh.Checked = isChecked;
                    foreach (TreeNode poly in mesh.Nodes)
                        poly.Checked = isChecked;
                }
            }
            if (e.Node.Tag is NUD.Mesh)
                foreach (TreeNode poly in e.Node.Nodes)
                    poly.Checked = isChecked;
        }

        private void click_ok(object sender, EventArgs e)
        {
            foreach (TreeNode model in treeView1.Nodes)
                foreach (TreeNode mesh in model.Nodes)
                    foreach (TreeNode poly in mesh.Nodes)
                        if (poly.Checked)
                            selected.Add((NUD.Polygon)poly.Tag);
            finished = true;
            Close();
        }

        private void click_cancel(object sender, EventArgs e)
        {
            finished = true;
            Close();
        }

        public static List<NUD.Polygon> Popup()
        {
            PolygonSelector selector = new PolygonSelector();
            selector.ShowDialog();
            return selector.selected;
        }
    }
}
