using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.Xml;
using System.Globalization;
using System.Diagnostics;
using Smash_Forge.GUI.Menus;


namespace Smash_Forge
{
    public partial class MeshList : DockContent
    {
        
        public static ImageList iconList = new ImageList();

        public MeshList()
        {
            InitializeComponent();
            refresh();

            iconList.ImageSize = new Size(24, 24);
            iconList.Images.Add("sex", Properties.Resources.sexy_green_down_arrow);
            iconList.Images.Add("polygon", Properties.Resources.icon_polygon);
            iconList.Images.Add("mesh", Properties.Resources.icon_mesh);
            iconList.Images.Add("model", Properties.Resources.icon_model);
            treeView1.ImageList = iconList;
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
                    model.ImageKey = "model";
                    model.SelectedImageKey = "model";
                    j++; 
                    foreach (NUD.Mesh mesh in m.nud.meshes)
                    {
                        model.Nodes.Add(mesh);
                        int i = 0;
                        foreach(NUD.Polygon poly in mesh.Nodes)
                        {
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
            if (e.Node.Tag is NUD) {
                
                foreach (TreeNode n in e.Node.Nodes) n.Checked = e.Node.Checked;
                
            }
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
                //Since we are changing value but we don't want the entire model order to swap,
                // we are disabling the event for on change value temporarily
                changingValue = true;
                numericUpDown1.Maximum = ((NUD)e.Node.Parent.Tag).meshes.Count - 1;
                numericUpDown1.Value = ((NUD)e.Node.Parent.Tag).meshes.IndexOf((NUD.Mesh)e.Node);

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
                n.meshes.Remove(m);
                n.meshes.Insert(pos, m);
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
                        v.col += new OpenTK.Vector4(5f);
                    }

                    foreach (ModelContainer con in Runtime.ModelContainers)
                    {
                        if (con.nud != null)
                            con.nud.PreRender();
                    }
                }
            }
            if (e.KeyChar == 'x')
            {
                if (treeView1.SelectedNode is NUD.Polygon)
                {
                    NUD.Polygon p = (NUD.Polygon)treeView1.SelectedNode;

                    foreach (NUD.Vertex v in p.vertices)
                    {
                        v.col -= new OpenTK.Vector4(5f);
                    }

                    foreach (ModelContainer con in Runtime.ModelContainers)
                    {
                        if (con.nud != null)
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
                    int pos = n.meshes.IndexOf(m) + 1;
                    if (pos >= n.meshes.Count)
                        pos = n.meshes.Count - 1;
                    n.meshes.Remove(m);
                    n.meshes.Insert(pos, m);
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
                    int pos = n.meshes.IndexOf(m) - 1;
                    if (pos < 0)
                        pos = 0;
                    n.meshes.Remove(m);
                    n.meshes.Insert(pos, m);
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
                    int meshCount = nud.meshes.Count;
                    if (unorderedNud.meshes.Count > meshCount)
                        meshCount = unorderedNud.meshes.Count;
                    NUD.Mesh[] meshes = new NUD.Mesh[meshCount];

                    //Fill in matching meshes
                    foreach (NUD.Mesh m in nud.meshes)
                    {
                        foreach (NUD.Mesh m2 in unorderedNud.meshes)
                        {
                            if (m2.Text.Equals(m.Text))
                            {
                                meshes[nud.meshes.IndexOf((m))] = m2;
                                break;
                            }
                        }
                    }
                    //Fill in mismatched meshes
                    foreach (NUD.Mesh m in unorderedNud.meshes)
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
                            meshes[i].Text = "dummy";  
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
                ((NUD.Mesh) e.Node).Text = e.Label;
            
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
                DeleteNode();
            }
        }

        public void DeleteNode()
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure? (This cannot be undone)", "Delete Polygon/Mesh", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                if (treeView1.SelectedNode is NUD.Polygon)
                {
                    NUD.Mesh parent = ((NUD.Mesh)treeView1.SelectedNode.Parent);
                    parent.Nodes.Remove((NUD.Polygon)treeView1.SelectedNode);
                    NUD parentn = ((NUD)parent.Parent.Tag);
                    parentn.PreRender();
                }
                else if (treeView1.SelectedNode is NUD.Mesh)
                {
                    NUD parent = ((NUD)treeView1.SelectedNode.Parent.Tag);
                    parent.meshes.Remove((NUD.Mesh)treeView1.SelectedNode);
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
                    if (m != null)
                        Runtime.ModelContainers.Remove(m);
                    if (Runtime.TargetVBN == m.vbn)
                        Runtime.TargetVBN = null;
                    //if (Runtime.TargetMTA == m.mta)
                    //    Runtime.TargetMTA = null;
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
                        foreach (NUD.Mesh mesh in nud.meshes)
                            ((NUD)treeView1.SelectedNode.Tag).meshes.Add((mesh));
                        ((NUD)treeView1.SelectedNode.Tag).PreRender();
                        refresh();
                    }
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = treeView1.GetNodeAt(e.Location);
                if (treeView1.SelectedNode is NUD.Mesh)
                {
                    meshContextMenu.Show(this, e.X, e.Y);
                }
                if (treeView1.SelectedNode is NUD.Polygon)
                {
                    polyContextMenu.Show(this, e.X, e.Y);
                }
                if(treeView1.SelectedNode != null)
                if (treeView1.SelectedNode.Tag is NUD)
                {
                    nudContextMenu.Show(this, e.X, e.Y);
                }
            }
        }

        private void flipUVsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD.Polygon)
            {
                NUD.Polygon p = (NUD.Polygon)treeView1.SelectedNode;

                foreach (NUD.Vertex v in p.vertices)
                {
                    for(int i = 0; i < v.uv.Count; i++)
                        v.uv[i] = new OpenTK.Vector2(v.uv[i].X, 1 - v.uv[i].Y);
                }

                foreach (ModelContainer con in Runtime.ModelContainers)
                {
                    if (con.nud != null)
                        con.nud.PreRender();
                }
            }
        }

        private void editMaterialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            polySelected((NUD.Polygon)treeView1.SelectedNode, $"{treeView1.SelectedNode.Parent.Text} {treeView1.SelectedNode.Text}");
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteNode();
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DeleteNode();
        }

        private void singleBindToBoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*if(mc.vbn.bones == null)
            {
                MessageBox.Show("No Skeleton Found");
                return;
            }*/
            NUD.Mesh mesh = (NUD.Mesh)treeView1.SelectedNode;
            char[] d = "None".ToCharArray();
            LVDEditor.StringWrapper str = new LVDEditor.StringWrapper() { data = d };
            foreach (ModelContainer mc in Runtime.ModelContainers)
                if (treeView1.SelectedNode.Parent.Tag == mc.nud)
                    if (mc.vbn.bones.Count > mesh.singlebind && mesh.singlebind != -1)
                        str = new LVDEditor.StringWrapper() { data = mc.vbn.bones[mesh.singlebind].Text.ToCharArray() };

            BoneRiggingSelector brs = new BoneRiggingSelector(str);
            brs.ShowDialog();
            if (!brs.Cancelled)
            {
                mesh.boneflag = 8;
                mesh.singlebind = brs.boneIndex;
                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    poly.polflag = 0;
                    poly.vertSize = poly.vertSize & 0x0F;
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

        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*NUD.Polygon p = (NUD.Polygon)treeView1.SelectedNode;

            NUD.Polygon np = new NUD.Polygon();
            np.faces.AddRange(p.faces);
            np.displayFaceSize = p.displayFaceSize;
            np.polflag = p.polflag;
            np.strip = p.strip;
            np.UVSize = p.UVSize;
            np.vertSize = p.vertSize;*/
        }

        private void makeMetalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeMetal makeMetal = new MakeMetal(((NUD)treeView1.SelectedNode.Tag));
            makeMetal.Show();
        }

        private void merge(TreeNode n)
        {
            NUD org = (NUD)treeView1.SelectedNode.Tag;
            NUD nud = (NUD)n.Tag;

            nud.meshes.AddRange(org.meshes);
            org.meshes.Clear();

            org.Destroy();
            nud.PreRender();

            treeView1.Nodes.Remove(treeView1.SelectedNode);
            treeView1.SelectedNode = n;

            // remove from model containers too
            ModelContainer torem = null;
            foreach (ModelContainer con in Runtime.ModelContainers)
            {
                if (con.nud == org)
                {
                    torem = con;
                    break;
                }
            }
            Runtime.ModelContainers.Remove(torem);

            refresh();
        }

        private void belowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode n = treeView1.SelectedNode.NextNode;
            if (n != null)
            {
                merge(n);
            }
        }

        private void aboveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode n = treeView1.SelectedNode.PrevNode;
            if (n != null)
            {
                merge(n);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = "";
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Namco Universal Data|*.nud|All files(*.*)|*.*";
            DialogResult result = save.ShowDialog();

            if (result == DialogResult.OK)
            {
                filename = save.FileName;
                if (filename.EndsWith(".nud"))
                    {
                        ((NUD)treeView1.SelectedNode.Tag).Save(filename);
                    }
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MeshMover mm = new MeshMover();
            if(treeView1.SelectedNode!=null)
                if(treeView1.SelectedNode is NUD.Mesh)
                    mm.mesh = (NUD.Mesh)treeView1.SelectedNode;
            mm.Show();
        }

        private void smoothNormalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(treeView1.SelectedNode is NUD.Polygon)
                ((NUD.Polygon)treeView1.SelectedNode).SmoothNormals();
        }

        private void copyMaterialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<NUD.Polygon> polys = PolygonSelector.Popup();
            foreach (NUD.Polygon poly in polys)
            {
                // link materials. don't link a material to itself
                if (((NUD.Polygon)treeView1.SelectedNode) != poly)
                {
                    poly.materials.Clear();
                    foreach (NUD.Material m in ((NUD.Polygon)treeView1.SelectedNode).materials)
                        poly.materials.Add(m.Clone());
                }
         
            }
        }

        private void openEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is NUD)
            {
                NUD org = (NUD)treeView1.SelectedNode.Tag;
                foreach (ModelContainer con in Runtime.ModelContainers)
                {
                    if (con.nud == org)
                    {
                        ModelViewport v = new ModelViewport();
                        v.draw.Add(con);
                        MainForm.Instance.AddDockedControl(v);
                        break;
                    }
                }
            }
        }

        private void detachToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD.Polygon)
            {
                NUD.Polygon p = ((NUD.Polygon)treeView1.SelectedNode);
                NUD.Mesh parent = (NUD.Mesh)p.Parent;
                p.Parent.Nodes.Remove(p);
                NUD.Mesh m = new NUD.Mesh();
                ((NUD)parent.Parent.Tag).meshes.Add(m);
                m.Text = parent.Text + "_" + p.Text;
                m.Nodes.Add(p);

                if (parent.Nodes.Count == 0) ((NUD)parent.Parent.Tag).meshes.Remove(parent);

                refresh();
            }
        }

        private void aboveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD.Mesh)
            {
                NUD.Mesh m = ((NUD.Mesh)treeView1.SelectedNode);

                NUD nud = (NUD)(m.Parent.Tag);

                int index = nud.meshes.IndexOf(m);

                if(index > 0)
                {
                    nud.meshes.Remove(m);

                    NUD.Mesh merge = nud.meshes[index-1];

                    List<TreeNode> polygons = new List<TreeNode>();
                    foreach(NUD.Polygon p in m.Nodes)
                        polygons.Add(p);

                    foreach(NUD.Polygon p in polygons)
                    {
                        m.Nodes.Remove(p);
                        merge.Nodes.Add(p);
                    }

                    refresh();
                }
            }
        }

        private void belowToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD.Mesh)
            {
                NUD.Mesh m = ((NUD.Mesh)treeView1.SelectedNode);

                NUD nud = (NUD)(m.Parent.Tag);

                int index = nud.meshes.IndexOf(m);

                if (index+1 < nud.meshes.Count)
                {
                    nud.meshes.Remove(m);

                    NUD.Mesh merge = nud.meshes[index];

                    List<TreeNode> polygons = new List<TreeNode>();
                    foreach (NUD.Polygon p in m.Nodes)
                        polygons.Add(p);

                    foreach (NUD.Polygon p in polygons)
                    {
                        m.Nodes.Remove(p);
                        merge.Nodes.Add(p);
                    }

                    refresh();
                }
            }
        }

        private void flipUVsHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD.Polygon)
            {
                NUD.Polygon p = (NUD.Polygon)treeView1.SelectedNode;

                foreach (NUD.Vertex v in p.vertices)
                {
                    for (int i = 0; i < v.uv.Count; i++)
                        v.uv[i] = new OpenTK.Vector2(1 - v.uv[i].X, v.uv[i].Y);
                }

                foreach (ModelContainer con in Runtime.ModelContainers)
                {
                    if (con.nud != null)
                        con.nud.PreRender();
                }
            }
        }

        private void exportAsXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is NUD)
            {
                NUD nud = (NUD)treeView1.SelectedNode.Tag;

                string filename = "";
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "XML Material|*.xml";
                DialogResult result = save.ShowDialog();

                if (result == DialogResult.OK)
                {
                    filename = save.FileName;
                    if (filename.EndsWith(".xml"))
                    {
                        MaterialXML.exportMaterialAsXML(nud, filename);
                    }
                }
            }
        }

        private void importFromXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is NUD)
            {
                NUD nud = (NUD)treeView1.SelectedNode.Tag;

                string filename = "";
                OpenFileDialog save = new OpenFileDialog();
                save.Filter = "XML Material|*.xml";
                DialogResult result = save.ShowDialog();

                if (result == DialogResult.OK)
                {
                    filename = save.FileName;
                    if (filename.EndsWith(".xml"))
                    {
                        try
                        {
                            MaterialXML.importMaterialAsXML(nud, filename);
                        }
                        catch (MaterialXML.ParamArrayLengthException ex)
                        {
                            MessageBox.Show(ex.errorMessage);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("An error occurred reading the XML file. \n" + ex.Message);
                        }
                    }
                }
            }
        }

        private void addBlankMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is NUD)
            {
                NUD nud = (NUD)treeView1.SelectedNode.Tag;

                NUD.Mesh m = new NUD.Mesh();
                
                int i = 0;
                bool foundName = false;
                while (!foundName)
                {
                    m.Text = $"Blank Mesh {i++}";
                    foundName = true;
                    foreach (NUD.Mesh mesh in nud.meshes)
                        if (mesh.Text.Equals(m.Text))
                            foundName = false;
                }

                nud.meshes.Add(m);

                refresh();
            }
        }

        private void generateTanBitanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is NUD)
            {
                foreach (NUD.Mesh mesh in ((NUD)treeView1.SelectedNode.Tag).meshes)
                {
                    foreach (NUD.Polygon poly in mesh.Nodes)
                    {
                        poly.computeTangentBitangent();
                    }
                }              
            }
        }

        private void generateTanBitanToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD.Polygon)
                ((NUD.Polygon)treeView1.SelectedNode).computeTangentBitangent();
        }

        private void calculateNormalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
   
        }

        private void calculateNormalsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is NUD)
            {
                foreach (NUD.Mesh mesh in ((NUD)treeView1.SelectedNode.Tag).meshes)
                {
                    foreach (NUD.Polygon poly in mesh.Nodes)
                    {
                        poly.CalculateNormals();
                    }
                }
            }
        }

        private void smoothNormalsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is NUD)
            {
                foreach (NUD.Mesh mesh in ((NUD)treeView1.SelectedNode.Tag).meshes)
                {
                    foreach (NUD.Polygon poly in mesh.Nodes)
                    {
                        poly.SmoothNormals();
                    }
                }
            }
        }

        private void useAOAsSpecToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is NUD)
            {
                foreach (NUD.Mesh mesh in ((NUD)treeView1.SelectedNode.Tag).meshes)
                {
                    foreach (NUD.Polygon poly in mesh.Nodes)
                    {
                        poly.AOSpecRefBlend();
                    }
                }
            }
        }

        private void useAOAsSpecToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD.Polygon)
                ((NUD.Polygon)treeView1.SelectedNode).AOSpecRefBlend();
        }
    }
}
