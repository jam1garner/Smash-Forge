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
                    TreeNode model;
                    if (string.IsNullOrWhiteSpace(m.name))
                        model = new TreeNode($"Model {j}") { Tag = m.nud };
                    else
                        model = new TreeNode(m.name) { Tag = m.nud };
                    treeView1.Nodes.Add(model);
                    j++; 
                    foreach (NUD.Mesh mesh in m.nud.mesh)
                    {
                        model.Nodes.Add(mesh);
                        int i = 0;
                        mesh.Nodes.Clear();
                        foreach(NUD.Polygon poly in mesh.polygons)
                        {
                            mesh.Nodes.Add(poly);
                            poly.Text = "Polygon_" + i;
                            i++;
                        }
                    }
                }
            }
            //treeView1.ExpandAll();
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            /*if (e.Node.Tag is NUD.Mesh) {
                ((NUD.Mesh)e.Node.Tag).isVisible = e.Node.Checked;
                
            }
            else if (e.Node.Tag is NUD.Polygon) {
                ((NUD.Polygon)e.Node.Tag).isVisible = e.Node.Checked;
            }
            else if (e.Node.Tag is NUD){
                foreach (NUD.Mesh mesh in ((NUD)e.Node.Tag).mesh)
                {
                    //mesh.isVisible = e.Node.Checked;
                    //foreach(NUD.Polygon poly in mesh.polygons)
                    //{
                     //   poly.isVisible = e.Node.Checked;
                   // }
                }
                foreach (TreeNode meshNode in e.Node.Nodes)
                {
                    meshNode.Checked = e.Node.Checked;
                    foreach (TreeNode polyNode in meshNode.Nodes)
                        polyNode.Checked = e.Node.Checked;
                }
            }*/
        }

        private void polySelected(NUD.Polygon poly, string name)
        {
            MainForm.Instance.openMats(poly,name);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            numericUpDown1.Visible = false;
            label1.Visible = false;
            button1.Visible = false;
            if (e.Node is NUD.Mesh)
            {
                changingValue = true;//Since we are changing value but we don't want the entire model order to swap we are disabling the event for on change value temporarily
                numericUpDown1.Value = ((NUD)e.Node.Parent.Tag).mesh.IndexOf((NUD.Mesh)e.Node);
                numericUpDown1.Maximum = ((NUD)e.Node.Parent.Tag).mesh.Count - 1;

                numericUpDown1.Visible = true;
                label1.Visible = true;
            }
            else if (e.Node.Tag is NUD)
            {
                button1.Visible = true;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if(treeView1.SelectedNode is NUD.Mesh && !changingValue)
            {
                int pos = (int)numericUpDown1.Value;
                TreeNode node = treeView1.SelectedNode;
                TreeNode parent = node.Parent;
                NUD.Mesh m = (NUD.Mesh)node;
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
            if (e.KeyChar == 'c')
            {
                if (treeView1.SelectedNode is NUD.Polygon)
                {
                    NUD.Polygon p = (NUD.Polygon)treeView1.SelectedNode;

                    foreach (NUD.Vertex v in p.vertices)
                    {
                        v.col /= 2;
                    }

                    foreach (ModelContainer con in Runtime.ModelContainers)
                    {
                        if (con.nud != null)
                            con.nud.PreRender();
                    }
                }
            }
            if (e.KeyChar == 'f')
            {
                if (treeView1.SelectedNode is NUD.Polygon)
                {

                    NUD.Polygon p = (NUD.Polygon)treeView1.SelectedNode;

                    foreach(NUD.Vertex v in p.vertices)
                    {
                        v.tx[0] = new OpenTK.Vector2(v.tx[0].X, 1 - v.tx[0].Y);
                    }
                    
                    foreach(ModelContainer con in Runtime.ModelContainers)
                    {
                        if(con.nud != null)
                            con.nud.PreRender();
                    }
                }
            }
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

        private void button1_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Namco Model (.nud)|*.nud";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string filename = ofd.FileName;
                    NUD nud = new NUD(filename);
                    NUD unorderedNud = (NUD)treeView1.SelectedNode.Tag;
                    //Gonna reorder some NUDs, NUD-in to it
                    int meshCount = nud.mesh.Count;
                    if (unorderedNud.mesh.Count > meshCount)
                        meshCount = unorderedNud.mesh.Count;
                    NUD.Mesh[] meshes = new NUD.Mesh[meshCount];

                    //Fill in matching meshes
                    foreach (NUD.Mesh m in nud.mesh)
                    {
                        foreach (NUD.Mesh m2 in unorderedNud.mesh)
                        {
                            if (m2.Name == m.Name)
                            {
                                meshes[nud.mesh.IndexOf((m))] = m2;
                                break;
                            }
                        }
                    }
                    //Fill in mismatched meshes
                    foreach (NUD.Mesh m in unorderedNud.mesh)
                    {
                        if (!meshes.Contains(m))
                        {
                            for (int i = 0; i < meshes.Length; i++)
                            {
                                if (meshes[i] == null)
                                {
                                    meshes[i] = m;
                                    break;
                                }
                            }
                        }
                    }
                    //Dummies for the dummies that don't make enough meshes
                    for (int i = 0; i < meshes.Length; i++)
                    {
                        if (meshes[i] == null)
                        {
                            meshes[i] = new NUD.Mesh();
                            meshes[i].Name = "dummy";  
                            break;
                        }
                    }

                    refresh();
                }
            }
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node is NUD.Mesh)
                ((NUD.Mesh) e.Node).Name = e.Label;
            
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
                if (treeView1.SelectedNode is NUD.Polygon)
                {
                    NUD.Mesh parent = ((NUD.Mesh)treeView1.SelectedNode.Parent);
                    parent.polygons.Remove((NUD.Polygon)treeView1.SelectedNode);
                    parent.Nodes.Remove((NUD.Polygon)treeView1.SelectedNode);
                }
                else if (treeView1.SelectedNode is NUD.Mesh)
                {
                    NUD parent = ((NUD) treeView1.SelectedNode.Parent.Tag);
                    parent.mesh.Remove((NUD.Mesh)treeView1.SelectedNode);
                    treeView1.SelectedNode.Parent.Nodes.Remove(treeView1.SelectedNode);
                    parent.PreRender();
                    
                }
                else if (treeView1.SelectedNode.Tag is NUD)
                {
                    NUD model = (NUD)treeView1.SelectedNode.Tag;
                    ModelContainer m = null;
                    foreach (ModelContainer modelContainer in Runtime.ModelContainers)
                    {
                        if (modelContainer.nud == model)
                            m = modelContainer;
                    }
                    if(m != null)
                        Runtime.ModelContainers.Remove(m);
                    if (Runtime.TargetVBN == m.vbn)
                        Runtime.TargetVBN = null;
                    if (Runtime.TargetMTA == m.mta)
                        Runtime.TargetMTA = null;
                    if (Runtime.TargetNUD == m.nud)
                        Runtime.TargetNUD = null;

                    treeView1.Nodes.Remove(treeView1.SelectedNode);
                }
            }
        }

        public void mergeModel()
        {
            if (treeView1.SelectedNode.Tag is NUD)
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Namco Model (.nud)|*.nud";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string filename = ofd.FileName;
                        NUD nud = new NUD(filename);
                        foreach (NUD.Mesh mesh in nud.mesh)
                            ((NUD)treeView1.SelectedNode.Tag).mesh.Add((mesh));
                        ((NUD)treeView1.SelectedNode.Tag).PreRender();
                        refresh();
                    }
                }
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (treeView1.SelectedNode is NUD.Mesh)
            {
                NUD.Mesh mesh = (NUD.Mesh) treeView1.SelectedNode;
                char[] d = "None".ToCharArray();
                LVDEditor.StringWrapper str = new LVDEditor.StringWrapper() { data = d };
                foreach (ModelContainer mc in Runtime.ModelContainers)
                    if (treeView1.SelectedNode.Parent.Tag == mc.nud)
                        if (mc.vbn.bones.Count > mesh.singlebind && mesh.singlebind != -1)
                            str = new LVDEditor.StringWrapper() {data = mc.vbn.bones[mesh.singlebind].boneName};
                        
                BoneRiggingSelector brs = new BoneRiggingSelector(str);
                brs.ShowDialog();
                if (!brs.Cancelled)
                {
                    mesh.singlebind = brs.boneIndex;
                    foreach(NUD.Polygon poly in mesh.polygons)
                    {
                        foreach (NUD.Vertex vi in poly.vertices)
                        {
                            vi.node.Clear();
                            vi.node.Add(mesh.singlebind);
                            vi.weight.Clear();
                            vi.weight.Add(1);
                        }
                    }
                    ((NUD)treeView1.SelectedNode.Parent.Tag).PreRender();
                }
            }
            else if (treeView1.SelectedNode is NUD.Polygon)
            {
                polySelected((NUD.Polygon) treeView1.SelectedNode, $"{treeView1.SelectedNode.Parent.Text} {treeView1.SelectedNode.Text}");
            }
        }
    }
}
