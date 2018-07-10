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
            RefreshNodes();

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
            filesTreeView.ImageList = iconList;

            MainContextMenu = new ContextMenu();
            MenuItem newMC = new MenuItem("Create Blank Model");
            newMC.Click += delegate (object sender, EventArgs e)
            {
                Console.WriteLine("Adding");
                filesTreeView.Nodes.Add(new ModelContainer() { Text = "Model_" + filesTreeView.Nodes.Count });
                RefreshNodes();
            };
            MainContextMenu.MenuItems.Add(newMC);
        }

        bool changingValue = false;

        public ContextMenuStrip PolyContextMenu { get { return polyContextMenu; } }
        public ContextMenuStrip MeshContextMenu { get { return meshContextMenu; } }

        public void RefreshNodes()
        {
            Queue<TreeNode> nodes = new Queue<TreeNode>();
            foreach (TreeNode n in filesTreeView.Nodes)
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
            matchToNudButton.Visible = false;
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
                matchToNudButton.Visible = true;
            }
            else if (e.Node is ModelContainer)
            {
                Runtime.TargetVBN = ((ModelContainer)e.Node).VBN;
            } else
            if (filesTreeView.SelectedNode is VBN)
            {
                Runtime.TargetVBN = ((VBN)e.Node);
            }
            if (filesTreeView.SelectedNode is BCH_Model)
            {
                Runtime.TargetVBN = ((BCH_Model)e.Node).skeleton;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if(filesTreeView.SelectedNode is NUD.Mesh && !changingValue)
            {
                int pos = (int)numericUpDown1.Value;
                TreeNode node = filesTreeView.SelectedNode;
                TreeNode parent = node.Parent;
                NUD.Mesh m = (NUD.Mesh)node;
                NUD n = (NUD)parent;
                n.Nodes.Remove(m);
                n.Nodes.Insert(pos, m);
                parent.Nodes.Remove(node);
                parent.Nodes.Insert(pos, node);
                filesTreeView.SelectedNode = node;
                n.UpdateVertexBuffers();
            }
            changingValue = false;//Set the value back so the user can change values
        }

        private void treeView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '=')
            {
                if (filesTreeView.SelectedNode is NUD.Mesh)
                {
                    TreeNode node = filesTreeView.SelectedNode;
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
                    filesTreeView.SelectedNode = node;
                    n.UpdateVertexBuffers();
                }
            }
            if (e.KeyChar == '-')
            {
                if (filesTreeView.SelectedNode is NUD.Mesh)
                {
                    TreeNode node = filesTreeView.SelectedNode;
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
                    filesTreeView.SelectedNode = node;
                    n.UpdateVertexBuffers();
                }
            }
        }

        private void matchToNudButton_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Namco Model (.nud)|*.nud";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    NUD sourceNud = new NUD(ofd.FileName);
                    NUD targetNud = (NUD)filesTreeView.SelectedNode;

                    //Gonna reorder some NUDs, nud-in to it
                    // Don't remove any meshes.
                    int meshCount = Math.Max(sourceNud.Nodes.Count, targetNud.Nodes.Count);

                    NUD.Mesh[] newMeshes = new NUD.Mesh[meshCount];

                    // Fill in matching meshes
                    foreach (NUD.Mesh sourceMesh in sourceNud.Nodes)
                    {
                        foreach (NUD.Mesh targetMesh in targetNud.Nodes)
                        {
                            if (targetMesh.Text.Equals(sourceMesh.Text))
                            {
                                newMeshes[sourceNud.Nodes.IndexOf((sourceMesh))] = targetMesh;
                                break;
                            }
                        }
                    }

                    // Fill in mismatched meshes
                    foreach (NUD.Mesh targetMesh in targetNud.Nodes)
                    {
                        if (!newMeshes.Contains(targetMesh))
                        {
                            for (int i = 0; i < newMeshes.Length; i++)
                            {
                                if (newMeshes[i] == null)
                                {
                                    newMeshes[i] = targetMesh;
                                    break;
                                }
                            }
                        }
                    }

                    // Dummies for the dummies that don't make enough meshes
                    for (int i = 0; i < newMeshes.Length; i++)
                    {
                        if (newMeshes[i] == null)
                        {
                            newMeshes[i] = new NUD.Mesh();
                            newMeshes[i].Text = "dummy";  
                        }
                    }

                    // Apply the changes.
                    targetNud.Nodes.Clear();
                    targetNud.Nodes.AddRange(newMeshes);

                    RefreshNodes();
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
                if (filesTreeView.SelectedNode is NUD.Polygon)
                {
                    NUD.Mesh parent = ((NUD.Mesh)filesTreeView.SelectedNode.Parent);
                    parent.Nodes.Remove((NUD.Polygon)filesTreeView.SelectedNode);
                    NUD parentNud = ((NUD)parent.Parent);
                    parentNud.UpdateVertexBuffers();
                }
                else if (filesTreeView.SelectedNode is NUD.Mesh)
                {
                    NUD parent = ((NUD)filesTreeView.SelectedNode.Parent);
                    filesTreeView.SelectedNode.Parent.Nodes.Remove(filesTreeView.SelectedNode);
                    parent.UpdateVertexBuffers();
                }
                else if (filesTreeView.SelectedNode is NUD)
                {
                    NUD model = (NUD)filesTreeView.SelectedNode;

                    filesTreeView.Nodes.Remove(filesTreeView.SelectedNode);
                }
            }
        }

        public void mergeModel()
        {
            if (filesTreeView.SelectedNode is NUD)
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Namco Model (.nud)|*.nud";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string filename = ofd.FileName;
                        NUD nud = new NUD(filename);
                        foreach (NUD.Mesh mesh in nud.Nodes)
                            ((NUD)filesTreeView.SelectedNode).Nodes.Add((mesh));
                        ((NUD)filesTreeView.SelectedNode).UpdateVertexBuffers();
                        RefreshNodes();
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
                filesTreeView.SelectedNode = filesTreeView.GetNodeAt(e.Location);
                if (filesTreeView.SelectedNode is NUD.Mesh)
                {
                    meshContextMenu.Show(this, e.X, e.Y);
                }
                else
                if (filesTreeView.SelectedNode is NUD.Polygon)
                {
                    polyContextMenu.Show(this, e.X, e.Y);
                }
                else
                if (filesTreeView.SelectedNode is NUD)
                {
                    nudContextMenu.Show(this, e.X, e.Y);
                }
                else
                if (filesTreeView.SelectedNode is ModelContainer)
                {
                    ModelContainerContextMenu.Show(this, e.X, e.Y);
                }
                else
                if(filesTreeView.SelectedNode == null)
                {
                    MainContextMenu.Show(this, new System.Drawing.Point(e.X, e.Y));
                }
            }
        }

        private void flipUVsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is NUD.Polygon)
            {
                NUD.Polygon p = (NUD.Polygon)filesTreeView.SelectedNode;

                foreach (NUD.Vertex v in p.vertices)
                {
                    for(int i = 0; i < v.uv.Count; i++)
                        v.uv[i] = new OpenTK.Vector2(v.uv[i].X, 1 - v.uv[i].Y);
                }

                foreach (TreeNode con in filesTreeView.Nodes)
                {
                    if(con is ModelContainer)
                    {
                        if (((ModelContainer)con).NUD != null)
                            ((ModelContainer)con).NUD.UpdateVertexBuffers();
                    }
                }
            }
        }

        private void editMaterialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            polySelected((NUD.Polygon)filesTreeView.SelectedNode, $"{filesTreeView.SelectedNode.Parent.Text} {filesTreeView.SelectedNode.Text}");
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
            NUD.Mesh mesh = (NUD.Mesh)filesTreeView.SelectedNode;

            //The ModelContainer that contains the NUD that contains this mesh (SelectedNode)
            ModelContainer parentModel = (ModelContainer)filesTreeView.SelectedNode.Parent.Parent;

            short boneIndex = -1;
            if (mesh.singlebind < parentModel.VBN.bones.Count)
                boneIndex = mesh.singlebind;

            BoneRiggingSelector brs = new BoneRiggingSelector(boneIndex);
            brs.ModelContainers.Add(parentModel);
            brs.ShowDialog();
            if (!brs.Cancelled)
            {
                if (brs.SelectedNone)
                    mesh.boneflag = (int)NUD.Mesh.BoneFlags.NotRigged;
                else
                    mesh.boneflag = (int)NUD.Mesh.BoneFlags.SingleBind;
                mesh.singlebind = brs.boneIndex;
                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    poly.polflag = 0;
                    poly.vertSize = poly.vertSize & 0x0F;
                    foreach (NUD.Vertex vi in poly.vertices)
                    {
                        vi.boneIds.Clear();
                        vi.boneIds.Add(mesh.singlebind);
                        vi.boneWeights.Clear();
                        vi.boneWeights.Add(1);
                    }
                }
                ((NUD)filesTreeView.SelectedNode.Parent).UpdateVertexBuffers();
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
            if (!(filesTreeView.SelectedNode is NUD))
                return;
            MakeMetal makeMetal = new MakeMetal(((NUD)filesTreeView.SelectedNode));
            makeMetal.Show();
        }

        private void merge(TreeNode n)
        {
            ModelContainer originalModelContainer = (ModelContainer)filesTreeView.SelectedNode;
            ModelContainer newModelContainer = (ModelContainer)n;
            
            // Remove nodes from original and add to the new model container. 
            int count = originalModelContainer.NUD.Nodes.Count;
            for (int i = 0; i < count; i++)
            {
                TreeNode node = originalModelContainer.NUD.Nodes[0];
                originalModelContainer.NUD.Nodes.Remove(node);

                // TODO: Account for merging single bound meshes. 

                newModelContainer.NUD.Nodes.Add(node);
            }

            // Remove the original nodes.
            originalModelContainer.NUD.Nodes.Clear();

            newModelContainer.NUD.UpdateVertexBuffers();

            filesTreeView.Nodes.Remove(filesTreeView.SelectedNode);
            filesTreeView.SelectedNode = n;

            RefreshNodes();
        }

        private void belowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode n = filesTreeView.SelectedNode.NextNode;
            if (n != null)
            {
                merge(n);
            }
        }

        private void aboveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode n = filesTreeView.SelectedNode.PrevNode;
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
                        ((NUD)filesTreeView.SelectedNode).Save(filename);
                    }
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MeshMover mm = new MeshMover();
            if(filesTreeView.SelectedNode!=null)
                if(filesTreeView.SelectedNode is NUD.Mesh)
                    mm.mesh = (NUD.Mesh)filesTreeView.SelectedNode;
            mm.Show();
        }

        public List<ModelContainer> GetModelContainers()
        {
            List<ModelContainer> models = new List<ModelContainer>();
            foreach (TreeNode n in filesTreeView.Nodes)
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
                if (((NUD.Polygon)filesTreeView.SelectedNode) != poly)
                {
                    poly.materials.Clear();
                    foreach (NUD.Material m in ((NUD.Polygon)filesTreeView.SelectedNode).materials)
                        poly.materials.Add(m.Clone());
                }
         
            }
        }

        private void openEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is NUD)
            {
                NUD org = (NUD)filesTreeView.SelectedNode;
                foreach (TreeNode node in filesTreeView.Nodes)
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
            if (filesTreeView.SelectedNode is NUD.Polygon)
            {
                NUD.Polygon p = ((NUD.Polygon)filesTreeView.SelectedNode);
                NUD.Mesh parent = (NUD.Mesh)p.Parent;
                p.Parent.Nodes.Remove(p);
                NUD.Mesh m = new NUD.Mesh();
                ((NUD)parent.Parent).Nodes.Add(m);
                m.Text = parent.Text + "_" + p.Text;
                m.Nodes.Add(p);

                if (parent.Nodes.Count == 0) ((NUD)parent.Parent).Nodes.Remove(parent);

                RefreshNodes();
            }
        }

        private void aboveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is NUD.Mesh)
            {
                NUD.Mesh sourceMesh = (NUD.Mesh)filesTreeView.SelectedNode;
                NUD nud = (NUD)(sourceMesh.Parent);
                int index = nud.Nodes.IndexOf(sourceMesh) - 1;
                MergeMeshes(sourceMesh, index);
            }
        }

        private void belowToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is NUD.Mesh)
            {
                NUD.Mesh sourceMesh = (NUD.Mesh)filesTreeView.SelectedNode;
                NUD nud = (NUD)(sourceMesh.Parent);
                int index = nud.Nodes.IndexOf(sourceMesh) + 1;
                MergeMeshes(sourceMesh, index);
            }
        }

        private void MergeMeshes(NUD.Mesh sourceMesh, int targetMeshIndex)
        {
            NUD nud = (NUD)(sourceMesh.Parent);
            if (targetMeshIndex >= nud.Nodes.Count || targetMeshIndex < 0)
                return;

            // Merge the selected mesh onto the next mesh. 
            NUD.Mesh targetMesh = (NUD.Mesh)nud.Nodes[targetMeshIndex];
            nud.Nodes.Remove(sourceMesh);
            TransferMeshPolygons(sourceMesh, targetMesh);
            RefreshNodes();
        }

        private static void TransferMeshPolygons(NUD.Mesh sourceMesh, NUD.Mesh targetMesh)
        {
            List<TreeNode> sourcePolygons = new List<TreeNode>();
            foreach (NUD.Polygon p in sourceMesh.Nodes)
                sourcePolygons.Add(p);

            foreach (NUD.Polygon p in sourcePolygons)
            {
                sourceMesh.Nodes.Remove(p);
                targetMesh.Nodes.Add(p);
            }

            // Check single bind.
            if (sourceMesh.singlebind != targetMesh.singlebind)
            {
                // Change bone flag and generate weights. 
            }
        }

        private void flipUVsHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is NUD.Polygon)
            {
                NUD.Polygon p = (NUD.Polygon)filesTreeView.SelectedNode;

                foreach (NUD.Vertex v in p.vertices)
                {
                    for (int i = 0; i < v.uv.Count; i++)
                        v.uv[i] = new OpenTK.Vector2(1 - v.uv[i].X, v.uv[i].Y);
                }
                
                foreach (TreeNode con in filesTreeView.Nodes)
                {
                    if (con is ModelContainer)
                    {
                        if (((ModelContainer)con).NUD != null)
                            ((ModelContainer)con).NUD.UpdateVertexBuffers();
                    }
                }
            }
        }

        private void exportAsXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is NUD)
            {
                NUD nud = (NUD)filesTreeView.SelectedNode;

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
            if (filesTreeView.SelectedNode is NUD)
            {
                NUD nud = (NUD)filesTreeView.SelectedNode;

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
            if (filesTreeView.SelectedNode is NUD)
            {
                NUD nud = (NUD)filesTreeView.SelectedNode;

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

                RefreshNodes();
            }
        }

        private void generateTanBitanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD))
                return;

            var messageBox = MessageBox.Show("If the vertex type does not support tangents/bitangents, \n" +
                "the vertex type will be changed to Normals, Tan, Bi-Tan (Float). \n" +
                "This will increase the file size.", filesTreeView.SelectedNode.Text, MessageBoxButtons.OKCancel);

            if (messageBox == DialogResult.OK)
            {
                NUD n = (NUD)filesTreeView.SelectedNode;
                foreach (NUD.Mesh mesh in n.Nodes)
                {
                    foreach (NUD.Polygon poly in mesh.Nodes)
                    {
                        GenerateTanBitanAndFixVertType(poly);
                    }
                }

                // Update the data for rendering.
                n.UpdateVertexBuffers();
            }
        }

        private void generateTanBitanToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Polygon))
                return;
            
            NUD.Polygon poly = ((NUD.Polygon)filesTreeView.SelectedNode);
            GenerateTanBitanAndFixVertType(poly);

            // Update the data for rendering.
            NUD n = (NUD)poly.Parent.Parent;
            n.UpdateVertexBuffers();
        }

        private void calculateNormalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
   
        }

        private void calculateNormalsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD))
                return;

            NUD n = (NUD)filesTreeView.SelectedNode;
            foreach (NUD.Mesh mesh in n.Nodes)
            {
                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    poly.CalculateNormals();
                }
            }

            // Update the data for rendering.
            n.UpdateVertexBuffers();
        }

        private void useAOAsSpecToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is NUD)
            {
                foreach (NUD.Mesh mesh in ((NUD)filesTreeView.SelectedNode).Nodes)
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
            if (filesTreeView.SelectedNode is NUD.Polygon)
                ((NUD.Polygon)filesTreeView.SelectedNode).AOSpecRefBlend();
        }

        private void belowToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            TreeNode n = filesTreeView.SelectedNode.NextNode;
            if (n != null)
            {
                merge(n);
            }
        }

        private void aboveToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            TreeNode n = filesTreeView.SelectedNode.PrevNode;
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

            if (result == DialogResult.OK && filesTreeView.SelectedNode is ModelContainer)
            {
                Collada.Save(save.FileName, (ModelContainer)filesTreeView.SelectedNode);
            }
        }

        private void importFromDAEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is ModelContainer))
                return;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if(ofd.ShowDialog() == DialogResult.OK)
                {
                    DAEImportSettings daeImport = new DAEImportSettings();
                    daeImport.ShowDialog();
                    if (daeImport.exitStatus == DAEImportSettings.ExitStatus.Opened)
                    {
                        ModelContainer con = (ModelContainer)filesTreeView.SelectedNode;
                        
                        con.VBN = daeImport.getVBN();

                        Collada.DaetoNud(ofd.FileName, con, daeImport.importTexCB.Checked);

                        // apply settings
                        daeImport.Apply(con.NUD);
                    }
                }
            }
        }

        private void generateBoundingSpheresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((NUD)filesTreeView.SelectedNode).GenerateBoundingSpheres();
        }

        private void recalculateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Mesh))
                return;

            NUD.Mesh mesh = (NUD.Mesh)filesTreeView.SelectedNode;
            foreach (NUD.Polygon poly in mesh.Nodes)
            {
                poly.CalculateNormals();
            }

            // Update the data for rendering.
            NUD n = (NUD)mesh.Parent;
            n.UpdateVertexBuffers();
        }

        private void smoothToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Mesh))
                return;

            NUD.Mesh mesh = (NUD.Mesh)filesTreeView.SelectedNode;
            foreach (NUD.Polygon poly in mesh.Nodes)
            {
                poly.SmoothNormals();
            }

            // Update the data for rendering.
            NUD n = (NUD)mesh.Parent;
            n.UpdateVertexBuffers();
        }

        private void recalculateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Polygon))
                return;

            NUD.Polygon p = (NUD.Polygon)filesTreeView.SelectedNode;
            p.CalculateNormals();

            // Update the data for rendering.
            NUD n = (NUD)p.Parent.Parent;
            n.UpdateVertexBuffers();
        }

        private void smoothToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Polygon))
                return;

            NUD.Polygon p = (NUD.Polygon)filesTreeView.SelectedNode;
            p.SmoothNormals();

            // Update the data for rendering.
            NUD n = (NUD)p.Parent.Parent;
            n.UpdateVertexBuffers();
        }

        private void smoothToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD))
                return;

            NUD n = (NUD)filesTreeView.SelectedNode;
            foreach (NUD.Mesh mesh in ((NUD)filesTreeView.SelectedNode).Nodes)
            {
                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    poly.SmoothNormals();
                }
            }

            // Update the data for rendering.
            n.UpdateVertexBuffers();
        }

        private void recalculateToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD))
                return;

            NUD n = (NUD)filesTreeView.SelectedNode;
            foreach (NUD.Mesh mesh in n.Nodes)
            {
                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    poly.CalculateNormals();
                }
            }

            // Update the data for rendering.
            n.UpdateVertexBuffers();
        }

        private void generateTanBitanToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Mesh))
                return;
            
            string meshName = filesTreeView.SelectedNode.Text;
            var messageBox = MessageBox.Show("If the vertex type does not support tangents/bitangents, \n" +
                "the vertex type will be changed to Normals, Tan, Bi-Tan (Float). \n" +
                "This will increase the file size.", meshName, MessageBoxButtons.OKCancel);

            if (messageBox == DialogResult.OK)
            {
                NUD.Mesh mesh = (NUD.Mesh)filesTreeView.SelectedNode;
                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    GenerateTanBitanAndFixVertType(poly);
                }

                // Update the data for rendering.
                NUD n = (NUD)mesh.Parent;
                n.UpdateVertexBuffers();
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
            if (!(filesTreeView.SelectedNode is NUD.Polygon))
                return;

            NUD.Polygon p = (NUD.Polygon)filesTreeView.SelectedNode;
            p.SetVertexColor(new OpenTK.Vector4(127, 127, 127, 127));

            // Update the data for rendering.
            NUD n = (NUD)p.Parent.Parent;
            n.UpdateVertexBuffers();
        }

        private void selectColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Polygon))
                return;

            // Use a dialog so the color isn't set until the color editor is closed. 
            ColorEditor colorEditor = new ColorEditor(new OpenTK.Vector3(1));
            colorEditor.ShowDialog();

            // Remap the color from 1.0 being white to 127 being white.
            OpenTK.Vector3 newVertColor = colorEditor.GetColor() * 127;
            NUD.Polygon p = (NUD.Polygon)filesTreeView.SelectedNode;
            p.SetVertexColor(new OpenTK.Vector4(newVertColor, 127));

            // Update the data for rendering.
            NUD n = (NUD)p.Parent.Parent;
            n.UpdateVertexBuffers();
        }

        private void tangentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Polygon))
                return;

            NUD.Polygon p = (NUD.Polygon)filesTreeView.SelectedNode;
            foreach (NUD.Vertex v in p.vertices)
            {
                OpenTK.Vector3 newTan = v.tan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                v.color = new OpenTK.Vector4(newTan * 127, 127);
            }

            // Update the data for rendering.
            NUD n = (NUD)p.Parent.Parent;
            n.UpdateVertexBuffers();
        }

        private void bitangentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Polygon))
                return;

            NUD.Polygon p = (NUD.Polygon)filesTreeView.SelectedNode;
            foreach (NUD.Vertex v in p.vertices)
            {
                OpenTK.Vector3 newBitan = v.bitan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                v.color = new OpenTK.Vector4(newBitan * 127, 127);
            }

            // Update the data for rendering.
            NUD n = (NUD)p.Parent.Parent;
            n.UpdateVertexBuffers();
        }

        private void normalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Polygon))
                return;

            NUD.Polygon p = (NUD.Polygon)filesTreeView.SelectedNode;
            foreach (NUD.Vertex v in p.vertices)
            {
                OpenTK.Vector3 newNrm = v.nrm * 0.5f + new OpenTK.Vector3(0.5f);
                v.color = new OpenTK.Vector4(newNrm * 127, 127);
            }

            // Update the data for rendering.
            NUD n = (NUD)p.Parent.Parent;
            n.UpdateVertexBuffers();
        }

        private void uVsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Polygon))
                return;

            NUD.Polygon p = (NUD.Polygon)filesTreeView.SelectedNode;
            foreach (NUD.Vertex v in p.vertices)
            {
                v.color = new OpenTK.Vector4(v.uv[0].X * 127, v.uv[0].Y * 127, 127, 127);
            }

            // Update the data for rendering.
            NUD n = (NUD)p.Parent.Parent;
            n.UpdateVertexBuffers();
        }

        private void polyFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Polygon))
                return;

            NUD.Polygon poly = (NUD.Polygon)filesTreeView.SelectedNode;
            PolygonFormatEditor pfe = new PolygonFormatEditor(poly);
            pfe.ShowDialog();
            ((NUD)poly.Parent.Parent).UpdateVertexBuffers();
        }

        private void setToWhiteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Mesh))
                return;

            NUD.Mesh mesh = (NUD.Mesh)filesTreeView.SelectedNode;
            foreach (NUD.Polygon poly in mesh.Nodes)
            {
                poly.SetVertexColor(new OpenTK.Vector4(127, 127, 127, 127));
            }

            // Update the data for rendering.
            NUD n = (NUD)mesh.Parent;
            n.UpdateVertexBuffers();
        }

        private void selectColorToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Mesh))
                return;

            // Use a dialog so the color isn't set until the color editor is closed. 
            ColorEditor colorEditor = new ColorEditor(new OpenTK.Vector3(1));
            colorEditor.ShowDialog();

            // Remap the color from 1.0 being white to 127 being white.
            OpenTK.Vector3 newVertColor = colorEditor.GetColor() * 127;

            NUD.Mesh mesh = (NUD.Mesh)filesTreeView.SelectedNode;
            foreach (NUD.Polygon poly in mesh.Nodes)
            {
                poly.SetVertexColor(new OpenTK.Vector4(newVertColor, 127));
            }

            // Update the data for rendering.
            NUD n = (NUD)mesh.Parent;
            n.UpdateVertexBuffers();
        }

        private void tangentsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Mesh))
                return;

            NUD.Mesh mesh = (NUD.Mesh)filesTreeView.SelectedNode;
            foreach (NUD.Polygon poly in mesh.Nodes)
            {
                foreach (NUD.Vertex v in poly.vertices)
                {
                    OpenTK.Vector3 newTan = v.tan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                    v.color = new OpenTK.Vector4(newTan * 127, 127);
                }
            }

            // Update the data for rendering.
            NUD n = (NUD)mesh.Parent;
            n.UpdateVertexBuffers();
        }

        private void bitangentsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Mesh))
                return;

            NUD.Mesh mesh = (NUD.Mesh)filesTreeView.SelectedNode;
            foreach (NUD.Polygon poly in mesh.Nodes)
            {
                foreach (NUD.Vertex v in poly.vertices)
                {
                    OpenTK.Vector3 newBitan = v.bitan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                    v.color = new OpenTK.Vector4(newBitan * 127, 127);
                }
            }

            // Update the data for rendering.
            NUD n = (NUD)mesh.Parent;
            n.UpdateVertexBuffers();
        }

        private void normalsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Mesh))
                return;

            NUD.Mesh mesh = (NUD.Mesh)filesTreeView.SelectedNode;
            foreach (NUD.Polygon poly in mesh.Nodes)
            {
                foreach (NUD.Vertex v in poly.vertices)
                {
                    OpenTK.Vector3 newNrm = v.nrm * 0.5f + new OpenTK.Vector3(0.5f);
                    v.color = new OpenTK.Vector4(newNrm * 127, 127);
                }
            }

            // Update the data for rendering.
            NUD n = (NUD)mesh.Parent;
            n.UpdateVertexBuffers();
        }

        private void uVsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Mesh))
                return;

            NUD.Mesh mesh = (NUD.Mesh)filesTreeView.SelectedNode;
            foreach (NUD.Polygon poly in mesh.Nodes)
            {
                foreach (NUD.Vertex v in poly.vertices)
                {
                    v.color = new OpenTK.Vector4(v.uv[0].X * 127, v.uv[0].Y * 127, 127, 127);
                }
            }

            // Update the data for rendering.
            NUD n = (NUD)mesh.Parent;
            n.UpdateVertexBuffers();
        }

        private void tangentsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD))
                return;

            NUD n = (NUD)filesTreeView.SelectedNode;

            foreach (NUD.Mesh m in n.Nodes)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    foreach (NUD.Vertex v in p.vertices)
                    {
                        OpenTK.Vector3 newTan = v.tan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                        v.color = new OpenTK.Vector4(newTan * 127, 127);
                    }
                }
            }

            // Update the data for rendering.
            n.UpdateVertexBuffers();
        }

        private void bitangentsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD))
                return;

            NUD n = (NUD)filesTreeView.SelectedNode;

            foreach (NUD.Mesh m in n.Nodes)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    foreach (NUD.Vertex v in p.vertices)
                    {
                        OpenTK.Vector3 newBitan = v.bitan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                        v.color = new OpenTK.Vector4(newBitan * 127, 127);
                    }
                }
            }

            // Update the data for rendering.
            n.UpdateVertexBuffers();
        }

        private void setToWhiteToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD))
                return;

            NUD n = (NUD)filesTreeView.SelectedNode;

            foreach (NUD.Mesh m in n.Nodes)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    p.SetVertexColor(new OpenTK.Vector4(127, 127, 127, 127));
                }
            }

            // Update the data for rendering.
            n.UpdateVertexBuffers();
        }

        private void normalsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD))
                return;

            NUD n = (NUD)filesTreeView.SelectedNode;

            foreach (NUD.Mesh m in n.Nodes)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    foreach (NUD.Vertex v in p.vertices)
                    {
                        OpenTK.Vector3 newNrm = v.nrm * 0.5f + new OpenTK.Vector3(0.5f);
                        v.color = new OpenTK.Vector4(newNrm * 127, 127);
                    }
                }
            }

            // Update the data for rendering.
            n.UpdateVertexBuffers();
        }

        private void uVsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD))
                return;

            NUD n = (NUD)filesTreeView.SelectedNode;
            foreach (NUD.Mesh m in n.Nodes)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    foreach (NUD.Vertex v in p.vertices)
                    {
                        v.color = new OpenTK.Vector4(v.uv[0].X * 127, v.uv[0].Y * 127, 127, 127);
                    }
                }
            }

            // Update the data for rendering.
            n.UpdateVertexBuffers();
        }

        private void selectColorToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD))
                return;

            NUD n = (NUD)filesTreeView.SelectedNode;

            // Use a dialog so the color isn't set until the color editor is closed. 
            ColorEditor colorEditor = new ColorEditor(new OpenTK.Vector3(1));
            colorEditor.ShowDialog();

            // Remap the color from 1.0 being white to 127 being white.
            OpenTK.Vector3 newVertColor = colorEditor.GetColor() * 127;

            foreach (NUD.Mesh m in n.Nodes)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    p.SetVertexColor(new OpenTK.Vector4(newVertColor, 127));
                }
            }

            // Update the data for rendering.
            n.UpdateVertexBuffers();
        }

        private void texIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD))
                return;

            NUD n = (NUD)filesTreeView.SelectedNode;

            using (var texIdSelector = new TexIdSelector())
            {              
                texIdSelector.Set(n.GetFirstTexId());
                texIdSelector.ShowDialog();
                if (texIdSelector.exitStatus == TexIdSelector.ExitStatus.Opened)
                {
                    n.ChangeTextureIds(texIdSelector.getNewTexId());
                }
            }
        }

        private void texIDNUDNUTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is ModelContainer))
                return;

            ModelContainer modelContainer = (ModelContainer)filesTreeView.SelectedNode;

            if (modelContainer.NUT.Nodes.Count == 0)
                return;

            using (var texIdSelector = new TexIdSelector())
            {
                // Match the texture IDs. Assume the NUT is the correct one to initialize the gui.
                texIdSelector.Set(((NutTexture)modelContainer.NUT.Nodes[0]).HASHID);
                texIdSelector.ShowDialog();
                if (texIdSelector.exitStatus == TexIdSelector.ExitStatus.Opened)
                {
                    modelContainer.NUD.ChangeTextureIds(texIdSelector.getNewTexId());
                    modelContainer.NUT.ChangeTextureIds(texIdSelector.getNewTexId());
                }
            }
        }

        private void uvViewerMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is NUD.Polygon))
                return;

            NUD.Polygon poly = (NUD.Polygon)filesTreeView.SelectedNode;
            NUD nud = (NUD)poly.Parent.Parent;
            UvViewer uvViewer = new UvViewer(nud, poly);
            uvViewer.Show();
        }
    }
}
