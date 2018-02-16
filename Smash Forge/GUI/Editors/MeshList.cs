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
        private ContextMenu MainContextMenu;

        public MeshList()
        {
            InitializeComponent();
            refresh();

            iconList.ImageSize = new Size(24, 24);
            iconList.Images.Add("group", Properties.Resources.icon_group);
            iconList.Images.Add("polygon", Properties.Resources.icon_polygon);
            iconList.Images.Add("mesh", Properties.Resources.icon_mesh);
            iconList.Images.Add("model", Properties.Resources.icon_model);
            iconList.Images.Add("texture", Properties.Resources.icon_image);
            iconList.Images.Add("folder", Properties.Resources.icon_group);
            iconList.Images.Add("anim", Properties.Resources.icon_anim);
            iconList.Images.Add("bone", Properties.Resources.icon_bone);
            iconList.Images.Add("frame", Properties.Resources.icon_model);
            iconList.Images.Add("image", Properties.Resources.icon_image);
            iconList.Images.Add("skeleton", Properties.Resources.icon_skeleton);
            iconList.Images.Add("info", Properties.Resources.icon_info);
            iconList.Images.Add("number", Properties.Resources.icon_number);
            iconList.Images.Add("nut", Properties.Resources.UVPattern);
            treeView1.ImageList = iconList;

            MainContextMenu = new ContextMenu();
            MenuItem newMC = new MenuItem("Create Blank Model");
            newMC.Click += delegate (object sender, EventArgs e)
            {
                Console.WriteLine("Adding");
                treeView1.Nodes.Add(new ModelContainer() { Text = "Model_"+treeView1.Nodes.Count });
                refresh();
            };
            MainContextMenu.MenuItems.Add(newMC);
        }

        bool changingValue = false;

        public void refresh()
        {
            Queue<TreeNode> nodes = new Queue<TreeNode>();
            foreach (TreeNode n in treeView1.Nodes)
                nodes.Enqueue(n);

            while(nodes.Count > 0)
            {
                TreeNode node = nodes.Dequeue();

                if(node is NUD.Polygon)
                {
                    if (node.Parent != null)
                        ((NUD.Polygon)node).Text = "Polygon_" + ((NUD.Mesh)node.Parent).Nodes.IndexOf(node);
                }

                foreach (TreeNode n in node.Nodes)
                    nodes.Enqueue(n);
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node is NUD) {
                
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
            Runtime.TargetVBN = null;
            if (e.Node is NUD.Mesh)
            {
                //Since we are changing value but we don't want the entire model order to swap,
                // we are disabling the event for on change value temporarily
                changingValue = true;
                numericUpDown1.Maximum = ((NUD)e.Node.Parent).Nodes.Count - 1;
                numericUpDown1.Value = ((NUD)e.Node.Parent).Nodes.IndexOf((NUD.Mesh)e.Node);

                numericUpDown1.Visible = true;
                label1.Visible = true;
            }
            else if (e.Node is NUD)
            {
                button1.Visible = true;
            }
            else if (e.Node is ModelContainer)
            {
                Runtime.TargetVBN = ((ModelContainer)e.Node).VBN;
            } else
            if (treeView1.SelectedNode is VBN)
            {
                Runtime.TargetVBN = ((VBN)e.Node);
            }
            if (treeView1.SelectedNode is BCH_Model)
            {
                Runtime.TargetVBN = ((BCH_Model)e.Node).skeleton;
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
                NUD n = (NUD)parent;
                n.Nodes.Remove(m);
                n.Nodes.Insert(pos, m);
                parent.Nodes.Remove(node);
                parent.Nodes.Insert(pos, node);
                treeView1.SelectedNode = node;
                n.UpdateVertexDataAndSort();
            }
            changingValue = false;//Set the value back so the user can change values
        }

        private void treeView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //WTF were these even
            /*if (e.KeyChar == 'c')
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
                        if (con.NUD != null)
                            con.NUD.PreRender();
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
                        if (con.NUD != null)
                            con.NUD.PreRender();
                    }
                }
            }*/
            if (e.KeyChar == '=')
            {
                if (treeView1.SelectedNode is NUD.Mesh)
                {
                    TreeNode node = treeView1.SelectedNode;
                    TreeNode parent = node.Parent;
                    NUD.Mesh m = (NUD.Mesh)node;
                    NUD n = (NUD)parent;
                    int pos = n.Nodes.IndexOf(m) + 1;
                    if (pos >= n.Nodes.Count)
                        pos = n.Nodes.Count - 1;
                    n.Nodes.Remove(m);
                    n.Nodes.Insert(pos, m);
                    parent.Nodes.Remove(node);
                    parent.Nodes.Insert(pos, node);
                    treeView1.SelectedNode = node;
                    n.UpdateVertexDataAndSort();
                }
            }
            if (e.KeyChar == '-')
            {
                if (treeView1.SelectedNode is NUD.Mesh)
                {
                    TreeNode node = treeView1.SelectedNode;
                    TreeNode parent = node.Parent;
                    NUD.Mesh m = (NUD.Mesh)node;
                    NUD n = (NUD)parent;
                    int pos = n.Nodes.IndexOf(m) - 1;
                    if (pos < 0)
                        pos = 0;
                    n.Nodes.Remove(m);
                    n.Nodes.Insert(pos, m);
                    parent.Nodes.Remove(node);
                    parent.Nodes.Insert(pos, node);
                    treeView1.SelectedNode = node;
                    n.UpdateVertexDataAndSort();
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
                    NUD unorderedNud = (NUD)treeView1.SelectedNode;
                    //Gonna reorder some NUDs, nud-in to it
                    int meshCount = nud.Nodes.Count;
                    if (unorderedNud.Nodes.Count > meshCount)
                        meshCount = unorderedNud.Nodes.Count;
                    NUD.Mesh[] meshes = new NUD.Mesh[meshCount];

                    //Fill in matching meshes
                    foreach (NUD.Mesh m in nud.Nodes)
                    {
                        foreach (NUD.Mesh m2 in unorderedNud.Nodes)
                        {
                            if (m2.Text.Equals(m.Text))
                            {
                                meshes[nud.Nodes.IndexOf((m))] = m2;
                                break;
                            }
                        }
                    }
                    //Fill in mismatched meshes
                    foreach (NUD.Mesh m in unorderedNud.Nodes)
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
            //if (e.Node is NUD.Mesh)
            //    ((NUD.Mesh) e.Node).Text = e.Label;
            
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
                    NUD parentn = ((NUD)parent.Parent);
                    parentn.UpdateVertexDataAndSort();
                }
                else if (treeView1.SelectedNode is NUD.Mesh)
                {
                    NUD parent = ((NUD)treeView1.SelectedNode.Parent);
                    //parent.Nodes.Remove((NUD.Mesh)treeView1.SelectedNode);
                    treeView1.SelectedNode.Parent.Nodes.Remove(treeView1.SelectedNode);
                    parent.UpdateVertexDataAndSort();
                }
                else if (treeView1.SelectedNode is NUD)
                {
                    NUD model = (NUD)treeView1.SelectedNode;

                    treeView1.Nodes.Remove(treeView1.SelectedNode);
                }
            }
        }

        public void mergeModel()
        {
            if (treeView1.SelectedNode is NUD)
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Namco Model (.nud)|*.nud";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string filename = ofd.FileName;
                        NUD nud = new NUD(filename);
                        foreach (NUD.Mesh mesh in nud.Nodes)
                            ((NUD)treeView1.SelectedNode).Nodes.Add((mesh));
                        ((NUD)treeView1.SelectedNode).UpdateVertexDataAndSort();
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
                else
                if (treeView1.SelectedNode is NUD.Polygon)
                {
                    polyContextMenu.Show(this, e.X, e.Y);
                }
                else
                if (treeView1.SelectedNode is NUD)
                {
                    nudContextMenu.Show(this, e.X, e.Y);
                }
                else
                if (treeView1.SelectedNode is ModelContainer)
                {
                    MCContextMenu.Show(this, e.X, e.Y);
                }
                else
                if(treeView1.SelectedNode == null)
                {
                    MainContextMenu.Show(this, new System.Drawing.Point(e.X, e.Y));
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

                foreach (TreeNode con in treeView1.Nodes)
                {
                    if(con is ModelContainer)
                    {
                        if (((ModelContainer)con).NUD != null)
                            ((ModelContainer)con).NUD.UpdateVertexDataAndSort();
                    }
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
            NUD.Mesh mesh = (NUD.Mesh)treeView1.SelectedNode;

            //The ModelContainer that contains the NUD that contains this mesh (SelectedNode)
            ModelContainer parentModel = (ModelContainer)treeView1.SelectedNode.Parent.Parent;

            short boneIndex = -1;
            if (mesh.singlebind < parentModel.VBN.bones.Count)
                boneIndex = mesh.singlebind;

            BoneRiggingSelector brs = new BoneRiggingSelector(boneIndex);
            brs.ModelContainers.Add(parentModel);
            brs.ShowDialog();
            if (!brs.Cancelled)
            {
                if (brs.SelectedNone)
                    mesh.boneflag = 0;
                else
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
                ((NUD)treeView1.SelectedNode.Parent).UpdateVertexDataAndSort();
            }
        }

        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*nud.Polygon p = (nud.Polygon)treeView1.SelectedNode;

            nud.Polygon np = new nud.Polygon();
            np.faces.AddRange(p.faces);
            np.displayFaceSize = p.displayFaceSize;
            np.polflag = p.polflag;
            np.strip = p.strip;
            np.UVSize = p.UVSize;
            np.vertSize = p.vertSize;*/
        }

        private void makeMetalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeMetal makeMetal = new MakeMetal(((NUD)treeView1.SelectedNode));
            makeMetal.Show();
        }

        private void merge(TreeNode n)
        {
            ModelContainer org = (ModelContainer)treeView1.SelectedNode;
            ModelContainer nud = (ModelContainer)n;
            
            int count = org.NUD.Nodes.Count;
            for (int i = 0; i < count; i++)
            {
                TreeNode node = org.NUD.Nodes[0];
                org.NUD.Nodes.Remove(node);
                nud.NUD.Nodes.Add(node);
            }
            org.NUD.Nodes.Clear();

            org.NUD.Destroy();
            nud.NUD.UpdateVertexDataAndSort();

            treeView1.Nodes.Remove(treeView1.SelectedNode);
            treeView1.SelectedNode = n;

            // remove from model containers too
            //ModelContainer torem = org;
            //Runtime.ModelContainers.Remove(torem);

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
                        ((NUD)treeView1.SelectedNode).Save(filename);
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

        public List<ModelContainer> GetModelContainers()
        {
            List<ModelContainer> models = new List<ModelContainer>();
            foreach (TreeNode n in treeView1.Nodes)
            {
                if (n is ModelContainer)
                    models.Add((ModelContainer)n);
            }
            return models;
        }

        private void copyMaterialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<NUD.Polygon> polys = PolygonSelector.Popup(GetModelContainers());
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
            if (treeView1.SelectedNode is NUD)
            {
                NUD org = (NUD)treeView1.SelectedNode;
                foreach (TreeNode node in treeView1.Nodes)
                {
                    if(node is ModelContainer)
                    {
                        ModelContainer con = (ModelContainer)node;
                        if (con.NUD == org)
                        {
                            ModelViewport v = new ModelViewport();
                            v.draw.Add(con);
                            MainForm.Instance.AddDockedControl(v);
                            break;
                        }
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
                ((NUD)parent.Parent).Nodes.Add(m);
                m.Text = parent.Text + "_" + p.Text;
                m.Nodes.Add(p);

                if (parent.Nodes.Count == 0) ((NUD)parent.Parent).Nodes.Remove(parent);

                refresh();
            }
        }

        private void aboveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD.Mesh)
            {
                NUD.Mesh m = ((NUD.Mesh)treeView1.SelectedNode);

                NUD nud = (NUD)(m.Parent);

                int index = nud.Nodes.IndexOf(m);

                if(index > 0)
                {
                    nud.Nodes.Remove(m);

                    NUD.Mesh merge = (NUD.Mesh)nud.Nodes[index-1];

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

                NUD nud = (NUD)(m.Parent);

                int index = nud.Nodes.IndexOf(m);

                if (index+1 < nud.Nodes.Count)
                {
                    nud.Nodes.Remove(m);

                    NUD.Mesh merge = (NUD.Mesh)nud.Nodes[index];

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
                
                foreach (TreeNode con in treeView1.Nodes)
                {
                    if (con is ModelContainer)
                    {
                        if (((ModelContainer)con).NUD != null)
                            ((ModelContainer)con).NUD.UpdateVertexDataAndSort();
                    }
                }
            }
        }

        private void exportAsXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD)
            {
                NUD nud = (NUD)treeView1.SelectedNode;

                string filename = "";
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "XML Material|*.xml";
                DialogResult result = save.ShowDialog();

                if (result == DialogResult.OK)
                {
                    filename = save.FileName;
                    if (filename.EndsWith(".xml"))
                    {
                        MaterialXML.ExportMaterialAsXml(nud, filename);
                    }
                }
            }
        }

        private void importFromXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD)
            {
                NUD nud = (NUD)treeView1.SelectedNode;

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
                            MaterialXML.ImportMaterialAsXml(nud, filename);
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
            if (treeView1.SelectedNode is NUD)
            {
                NUD nud = (NUD)treeView1.SelectedNode;

                NUD.Mesh m = new NUD.Mesh();
                
                int i = 0;
                bool foundName = false;
                while (!foundName)
                {
                    m.Text = $"Blank Mesh {i++}";
                    foundName = true;
                    foreach (NUD.Mesh mesh in nud.Nodes)
                        if (mesh.Text.Equals(m.Text))
                            foundName = false;
                }

                nud.Nodes.Add(m);

                refresh();
            }
        }

        private void generateTanBitanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD))
                return;

            var messageBox = MessageBox.Show("If the vertex type does not support tangents/bitangents, \n" +
                "the vertex type will be changed to Normals, Tan, Bi-Tan (Float). \n" +
                "This will increase the file size.", treeView1.SelectedNode.Text, MessageBoxButtons.OKCancel);

            if (messageBox == DialogResult.OK)
            {
                foreach (NUD.Mesh mesh in ((NUD)treeView1.SelectedNode).Nodes)
                {
                    foreach (NUD.Polygon poly in mesh.Nodes)
                    {
                        GenerateTanBitanAndFixVertType(poly);
                    }
                }
            }
        }

        private void generateTanBitanToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD.Polygon))
                return;
            
            NUD.Polygon poly = ((NUD.Polygon)treeView1.SelectedNode);
            GenerateTanBitanAndFixVertType(poly);         
        }

        private void calculateNormalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
   
        }

        private void calculateNormalsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD)
            {
                foreach (NUD.Mesh mesh in ((NUD)treeView1.SelectedNode).Nodes)
                {
                    foreach (NUD.Polygon poly in mesh.Nodes)
                    {
                        poly.CalculateNormals();
                    }
                }
            }
        }

        private void useAOAsSpecToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD)
            {
                foreach (NUD.Mesh mesh in ((NUD)treeView1.SelectedNode).Nodes)
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

        private void belowToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            TreeNode n = treeView1.SelectedNode.NextNode;
            if (n != null)
            {
                merge(n);
            }
        }

        private void aboveToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            TreeNode n = treeView1.SelectedNode.PrevNode;
            if (n != null)
            {
                merge(n);
            }
        }

        private void exportAsDAEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Supported Filetypes (DAE)|*.dae;|All files(*.*)|*.*";
            DialogResult result = save.ShowDialog();

            if (result == DialogResult.OK && treeView1.SelectedNode is ModelContainer)
            {
                Collada.Save(save.FileName, (ModelContainer)treeView1.SelectedNode);
            }
        }

        private void importFromDAEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is ModelContainer))
                return;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if(ofd.ShowDialog() == DialogResult.OK)
                {
                    DAEImportSettings daeImport = new DAEImportSettings();
                    daeImport.ShowDialog();
                    if (daeImport.exitStatus == DAEImportSettings.ExitStatus.Opened)
                    {
                        ModelContainer con = (ModelContainer)treeView1.SelectedNode;
                        
                        con.VBN = daeImport.getVBN();

                        Collada.DaetoNud(ofd.FileName, con, daeImport.importTexCB.Checked);

                        // apply settings
                        daeImport.Apply(con.NUD);
                        con.NUD.MergePoly();
                    }
                }
            }
        }

        private void generateBoundingBoxesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((NUD)treeView1.SelectedNode).GenerateBoundingBoxes();
        }

        private void recalculateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD.Mesh)
            {
                foreach (NUD.Polygon poly in ((NUD.Mesh)treeView1.SelectedNode).Nodes)
                {
                    poly.CalculateNormals();
                }
            }
        }

        private void smoothToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD.Mesh)
            {
                foreach (NUD.Polygon poly in ((NUD.Mesh)treeView1.SelectedNode).Nodes)
                {
                    poly.SmoothNormals();
                }
            }
        }

        private void recalculateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD.Polygon)
                ((NUD.Polygon)treeView1.SelectedNode).CalculateNormals();
        }

        private void smoothToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is NUD.Polygon)
                ((NUD.Polygon)treeView1.SelectedNode).SmoothNormals();
        }

        private void smoothToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD))
                return;

            foreach (NUD.Mesh mesh in ((NUD)treeView1.SelectedNode).Nodes)
            {
                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    poly.SmoothNormals();
                }
            }  
        }

        private void recalculateToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD))
                return;

            foreach (NUD.Mesh mesh in ((NUD)treeView1.SelectedNode).Nodes)
            {
                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    poly.CalculateNormals();
                }
            }
        }

        private void generateTanBitanToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD.Mesh))
                return;
            
            string meshName = treeView1.SelectedNode.Text;
            var messageBox = MessageBox.Show("If the vertex type does not support tangents/bitangents, \n" +
                "the vertex type will be changed to Normals, Tan, Bi-Tan (Float). \n" +
                "This will increase the file size.", meshName, MessageBoxButtons.OKCancel);

            if (messageBox == DialogResult.OK)
            {
                foreach (NUD.Polygon poly in ((NUD.Mesh)treeView1.SelectedNode).Nodes)
                {
                    GenerateTanBitanAndFixVertType(poly);
                }
            }               
        }

        private static void GenerateTanBitanAndFixVertType(NUD.Polygon poly)
        {
            int vertType = poly.vertSize & 0xF;
            if (!(vertType == 3 || vertType == 7))
            {
                // Change the vert type to normals, tan, bitan (float)
                poly.vertSize = (poly.vertSize & 0xF0);
                poly.vertSize |= 7;
            }

            // This already checks for the appropriate vertex type. 
            poly.CalculateTangentBitangent();
        }

        private void setToWhiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD.Polygon))
                return;

            NUD.Polygon p = (NUD.Polygon)treeView1.SelectedNode;
            p.SetVertexColor(new OpenTK.Vector4(127, 127, 127, 255));

            // Update the data for rendering.
            NUD n = (NUD)p.Parent.Parent;
            n.UpdateVertexDataAndSort();
        }

        private void selectColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD.Polygon))
                return;

            // Use a dialog so the color isn't set until the color editor is closed. 
            ColorEditor colorEditor = new ColorEditor(new OpenTK.Vector3(1));
            colorEditor.ShowDialog();

            // Remap the color from 1.0 being white to 127 being white.
            OpenTK.Vector3 newVertColor = colorEditor.GetColor() * 127;
            NUD.Polygon p = (NUD.Polygon)treeView1.SelectedNode;
            p.SetVertexColor(new OpenTK.Vector4(newVertColor, 255));

            // Update the data for rendering.
            NUD n = (NUD)p.Parent.Parent;
            n.UpdateVertexDataAndSort();
        }

        private void tangentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD.Polygon))
                return;

            NUD.Polygon p = (NUD.Polygon)treeView1.SelectedNode;
            foreach (NUD.Vertex v in p.vertices)
            {
                OpenTK.Vector3 newTan = v.tan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                v.col = new OpenTK.Vector4(newTan * 127, 255);
            }

            // Update the data for rendering.
            NUD n = (NUD)p.Parent.Parent;
            n.UpdateVertexDataAndSort();
        }

        private void bitangentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD.Polygon))
                return;

            NUD.Polygon p = (NUD.Polygon)treeView1.SelectedNode;
            foreach (NUD.Vertex v in p.vertices)
            {
                OpenTK.Vector3 newBitan = v.bitan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                v.col = new OpenTK.Vector4(newBitan * 127, 255);
            }

            // Update the data for rendering.
            NUD n = (NUD)p.Parent.Parent;
            n.UpdateVertexDataAndSort();
        }

        private void normalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD.Polygon))
                return;

            NUD.Polygon p = (NUD.Polygon)treeView1.SelectedNode;
            foreach (NUD.Vertex v in p.vertices)
            {
                OpenTK.Vector3 newNrm = v.nrm * 0.5f + new OpenTK.Vector3(0.5f);
                v.col = new OpenTK.Vector4(newNrm * 127, 255);
            }

            // Update the data for rendering.
            NUD n = (NUD)p.Parent.Parent;
            n.UpdateVertexDataAndSort();
        }

        private void uVsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD.Polygon))
                return;

            NUD.Polygon p = (NUD.Polygon)treeView1.SelectedNode;
            foreach (NUD.Vertex v in p.vertices)
            {
                v.col = new OpenTK.Vector4(v.uv[0].X * 127, v.uv[0].Y * 127, 127, 255);
            }

            // Update the data for rendering.
            NUD n = (NUD)p.Parent.Parent;
            n.UpdateVertexDataAndSort();
        }

        private void setToWhiteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD.Mesh))
                return;

            NUD.Mesh mesh = (NUD.Mesh)treeView1.SelectedNode;
            foreach (NUD.Polygon poly in mesh.Nodes)
            {
                poly.SetVertexColor(new OpenTK.Vector4(127, 127, 127, 255));
            }

            // Update the data for rendering.
            NUD n = (NUD)mesh.Parent;
            n.UpdateVertexDataAndSort();
        }

        private void selectColorToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD.Mesh))
                return;

            // Use a dialog so the color isn't set until the color editor is closed. 
            ColorEditor colorEditor = new ColorEditor(new OpenTK.Vector3(1));
            colorEditor.ShowDialog();

            // Remap the color from 1.0 being white to 127 being white.
            OpenTK.Vector3 newVertColor = colorEditor.GetColor() * 127;

            NUD.Mesh mesh = (NUD.Mesh)treeView1.SelectedNode;
            foreach (NUD.Polygon poly in mesh.Nodes)
            {
                poly.SetVertexColor(new OpenTK.Vector4(newVertColor, 255));
            }

            // Update the data for rendering.
            NUD n = (NUD)mesh.Parent;
            n.UpdateVertexDataAndSort();
        }

        private void tangentsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD.Mesh))
                return;

            NUD.Mesh mesh = (NUD.Mesh)treeView1.SelectedNode;
            foreach (NUD.Polygon poly in mesh.Nodes)
            {
                foreach (NUD.Vertex v in poly.vertices)
                {
                    OpenTK.Vector3 newTan = v.tan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                    v.col = new OpenTK.Vector4(newTan * 127, 255);
                }
                poly.CreateDisplayVertices();
            }

            // Update the data for rendering.
            NUD n = (NUD)mesh.Parent;
            n.UpdateVertexDataAndSort();
        }

        private void bitangentsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD.Mesh))
                return;

            NUD.Mesh mesh = (NUD.Mesh)treeView1.SelectedNode;
            foreach (NUD.Polygon poly in mesh.Nodes)
            {
                foreach (NUD.Vertex v in poly.vertices)
                {
                    OpenTK.Vector3 newBitan = v.bitan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                    v.col = new OpenTK.Vector4(newBitan * 127, 255);
                }
                poly.CreateDisplayVertices();
            }

            // Update the data for rendering.
            NUD n = (NUD)mesh.Parent;
            n.UpdateVertexDataAndSort();
        }

        private void normalsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD.Mesh))
                return;

            NUD.Mesh mesh = (NUD.Mesh)treeView1.SelectedNode;
            foreach (NUD.Polygon poly in mesh.Nodes)
            {
                foreach (NUD.Vertex v in poly.vertices)
                {
                    OpenTK.Vector3 newNrm = v.nrm * 0.5f + new OpenTK.Vector3(0.5f);
                    v.col = new OpenTK.Vector4(newNrm * 127, 255);
                }
                poly.CreateDisplayVertices();
            }

            // Update the data for rendering.
            NUD n = (NUD)mesh.Parent;
            n.UpdateVertexDataAndSort();
        }

        private void uVsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD.Mesh))
                return;

            NUD.Mesh mesh = (NUD.Mesh)treeView1.SelectedNode;
            foreach (NUD.Polygon poly in mesh.Nodes)
            {
                foreach (NUD.Vertex v in poly.vertices)
                {
                    v.col = new OpenTK.Vector4(v.uv[0].X * 127, v.uv[0].Y * 127, 127, 255);
                }
            }

            // Update the data for rendering.
            NUD n = (NUD)mesh.Parent;
            n.UpdateVertexDataAndSort();
        }

        private void tangentsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD))
                return;

            NUD n = (NUD)treeView1.SelectedNode;

            foreach (NUD.Mesh m in n.Nodes)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    foreach (NUD.Vertex v in p.vertices)
                    {
                        OpenTK.Vector3 newTan = v.tan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                        v.col = new OpenTK.Vector4(newTan * 127, 255);
                    }
                    p.CreateDisplayVertices();
                }
            }

            // Update the data for rendering.
            n.UpdateVertexDataAndSort();
        }

        private void bitangentsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD))
                return;

            NUD n = (NUD)treeView1.SelectedNode;

            foreach (NUD.Mesh m in n.Nodes)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    foreach (NUD.Vertex v in p.vertices)
                    {
                        OpenTK.Vector3 newBitan = v.bitan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                        v.col = new OpenTK.Vector4(newBitan * 127, 255);
                    }
                    p.CreateDisplayVertices();
                }
            }

            // Update the data for rendering.
            n.UpdateVertexDataAndSort();
        }

        private void setToWhiteToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD))
                return;

            NUD n = (NUD)treeView1.SelectedNode;

            foreach (NUD.Mesh m in n.Nodes)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    p.SetVertexColor(new OpenTK.Vector4(127, 127, 127, 255));
                }
            }

            // Update the data for rendering.
            n.UpdateVertexDataAndSort();
        }

        private void normalsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD))
                return;

            NUD n = (NUD)treeView1.SelectedNode;

            foreach (NUD.Mesh m in n.Nodes)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    foreach (NUD.Vertex v in p.vertices)
                    {
                        OpenTK.Vector3 newNrm = v.nrm * 0.5f + new OpenTK.Vector3(0.5f);
                        v.col = new OpenTK.Vector4(newNrm * 127, 255);
                    }
                }
            }

            // Update the data for rendering.
            n.UpdateVertexDataAndSort();
        }

        private void uVsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD))
                return;

            NUD n = (NUD)treeView1.SelectedNode;
            foreach (NUD.Mesh m in n.Nodes)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    foreach (NUD.Vertex v in p.vertices)
                    {
                        v.col = new OpenTK.Vector4(v.uv[0].X * 127, v.uv[0].Y * 127, 127, 255);
                    }
                }
            }

            // Update the data for rendering.
            n.UpdateVertexDataAndSort();
        }

        private void selectColorToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode is NUD))
                return;

            NUD n = (NUD)treeView1.SelectedNode;

            // Use a dialog so the color isn't set until the color editor is closed. 
            ColorEditor colorEditor = new ColorEditor(new OpenTK.Vector3(1));
            colorEditor.ShowDialog();

            // Remap the color from 1.0 being white to 127 being white.
            OpenTK.Vector3 newVertColor = colorEditor.GetColor() * 127;

            foreach (NUD.Mesh m in n.Nodes)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    p.SetVertexColor(new OpenTK.Vector4(newVertColor, 255));
                }
            }

            // Update the data for rendering.
            n.UpdateVertexDataAndSort();
        }
    }
}
