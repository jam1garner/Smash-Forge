using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmashForge.Filetypes.Melee;
using SmashForge.Gui.Menus;
using WeifenLuo.WinFormsUI.Docking;

namespace SmashForge
{
    public partial class MeshList : DockContent
    {

        public static ImageList iconList = new ImageList();
        private ContextMenu mainContextMenu;

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
            iconList.Images.Add("bfres", Properties.Resources.icon_bfres);
            iconList.Images.Add("dat", Properties.Resources.icon_dat);
            iconList.Images.Add("script", Properties.Resources.node_file);
            filesTreeView.ImageList = iconList;

            mainContextMenu = new ContextMenu();
            MenuItem newMc = new MenuItem("Create Blank Model");
            newMc.Click += delegate (object sender, EventArgs e)
            {
                Console.WriteLine("Adding");
                filesTreeView.Nodes.Add(new ModelContainer() { Text = "Model_" + filesTreeView.Nodes.Count });
                RefreshNodes();
            };
            mainContextMenu.MenuItems.Add(newMc);

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

                if(node is Nud.Polygon)
                {
                    if (node.Parent != null)
                        ((Nud.Polygon)node).Text = "Polygon_" + ((Nud.Mesh)node.Parent).Nodes.IndexOf(node);
                }

                foreach (TreeNode n in node.Nodes)
                    nodes.Enqueue(n);
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node is Nud) {

                foreach (TreeNode n in e.Node.Nodes) n.Checked = e.Node.Checked;

            }
            if (e.Node is BFRES)
            {
                foreach (TreeNode n in e.Node.Nodes)
                {
                    n.Checked = e.Node.Checked;
                    foreach (TreeNode m in n.Nodes)
                    {
                        m.Checked = e.Node.Checked;
                    }
                }
            }
            if (e.Node is BFRES.FMDL_Model)
                foreach (TreeNode n in e.Node.Nodes) n.Checked = e.Node.Checked;
            if (e.Node is BFRES.Mesh)
                foreach (TreeNode n in e.Node.Nodes) n.Checked = e.Node.Checked;

            // Update viewport after hiding/showing meshes.
            MainForm.Instance.GetActiveModelViewport()?.glViewport?.Invalidate();
        }

        private void PolySelected(Nud.Polygon poly, string name)
        {
            MainForm.Instance.OpenMats(poly,name);
        }
        private void BfresShapeSelected(BFRES.Mesh poly, string name)
        {
            MainForm.Instance.OpenBfresMats(poly, name);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            numericUpDown1.Visible = false;
            label1.Visible = false;
            matchToNudButton.Visible = false;
            //Runtime.TargetVBN = null;
            if (e.Node is Nud.Mesh)
            {
                //Since we are changing value but we don't want the entire model order to swap,
                // we are disabling the event for on change value temporarily
                changingValue = true;
                numericUpDown1.Maximum = ((Nud)e.Node.Parent).Nodes.Count - 1;
                numericUpDown1.Value = ((Nud)e.Node.Parent).Nodes.IndexOf((Nud.Mesh)e.Node);

                numericUpDown1.Visible = true;
                label1.Visible = true;
            }
            else if (e.Node is Nud)
            {
                matchToNudButton.Visible = true;
            }
            else if (e.Node is ModelContainer)
            {
                Runtime.TargetVbn = ((ModelContainer)e.Node).VBN;
            }
            else if (filesTreeView.SelectedNode is VBN)
            {
                Runtime.TargetVbn = ((VBN)e.Node);
            }
            else if (filesTreeView.SelectedNode is BCH_Model)
            {
                Runtime.TargetVbn = ((BCH_Model)e.Node).skeleton;
            }
            else if (filesTreeView.SelectedNode is BchTexture)
            {
                MainForm.Instance.GetActiveModelViewport()?.glViewport?.Invalidate();
            }
            else if (filesTreeView.SelectedNode is NutTexture)
            {
                MainForm.Instance.GetActiveModelViewport()?.glViewport?.Invalidate();
            }
            if (filesTreeView.SelectedNode is BRTI)
            {
                MainForm.Instance.GetActiveModelViewport()?.glViewport?.Invalidate();
            }
            else if (filesTreeView.SelectedNode is FTEX)
            {
                MainForm.Instance.GetActiveModelViewport()?.glViewport?.Invalidate();
            }
            else if (filesTreeView.SelectedNode is MeleeRootNode)
            {
                Runtime.TargetVbn = ((MeleeRootNode)e.Node).RenderBones;
            }
            else if (filesTreeView.SelectedNode is MeleeJointAnimationNode)
            {
                ((ModelViewport)Parent).CurrentAnimation = ((MeleeJointAnimationNode)filesTreeView.SelectedNode).GetAnimation();
            }
            else if (filesTreeView.SelectedNode is MeleeJointNode)
            {
                ((MeleeJointNode)e.Node).RenderBone.Selected = true;
            }

            // Update selection render.
            MainForm.Instance.GetActiveModelViewport()?.glViewport?.Invalidate();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if(filesTreeView.SelectedNode is Nud.Mesh && !changingValue)
            {
                int pos = (int)numericUpDown1.Value;
                TreeNode node = filesTreeView.SelectedNode;
                TreeNode parent = node.Parent;
                Nud.Mesh m = (Nud.Mesh)node;
                Nud n = (Nud)parent;
                n.Nodes.Remove(m);
                n.Nodes.Insert(pos, m);
                parent.Nodes.Remove(node);
                parent.Nodes.Insert(pos, node);
                filesTreeView.SelectedNode = node;
                n.UpdateRenderMeshes();
            }
            changingValue = false;//Set the value back so the user can change values
        }

        private void treeView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '=')
            {
                if (filesTreeView.SelectedNode is Nud.Mesh)
                {
                    TreeNode node = filesTreeView.SelectedNode;
                    TreeNode parent = node.Parent;
                    Nud.Mesh m = (Nud.Mesh)node;
                    Nud n = (Nud)parent;
                    int pos = n.Nodes.IndexOf(m) + 1;
                    if (pos >= n.Nodes.Count)
                        pos = n.Nodes.Count - 1;
                    n.Nodes.Remove(m);
                    n.Nodes.Insert(pos, m);
                    parent.Nodes.Remove(node);
                    parent.Nodes.Insert(pos, node);
                    filesTreeView.SelectedNode = node;
                    n.UpdateRenderMeshes();
                }
            }
            if (e.KeyChar == '-')
            {
                if (filesTreeView.SelectedNode is Nud.Mesh)
                {
                    TreeNode node = filesTreeView.SelectedNode;
                    TreeNode parent = node.Parent;
                    Nud.Mesh m = (Nud.Mesh)node;
                    Nud n = (Nud)parent;
                    int pos = n.Nodes.IndexOf(m) - 1;
                    if (pos < 0)
                        pos = 0;
                    n.Nodes.Remove(m);
                    n.Nodes.Insert(pos, m);
                    parent.Nodes.Remove(node);
                    parent.Nodes.Insert(pos, node);
                    filesTreeView.SelectedNode = node;
                    n.UpdateRenderMeshes();
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
                    Nud sourceNud = new Nud(ofd.FileName);
                    Nud targetNud = (Nud)filesTreeView.SelectedNode;

                    //Gonna reorder some NUDs, nud-in to it
                    // Don't remove any meshes.
                    int meshCount = Math.Max(sourceNud.Nodes.Count, targetNud.Nodes.Count);

                    Nud.Mesh[] newMeshes = new Nud.Mesh[meshCount];

                    // Fill in matching meshes
                    foreach (Nud.Mesh sourceMesh in sourceNud.Nodes)
                    {
                        foreach (Nud.Mesh targetMesh in targetNud.Nodes)
                        {
                            if (targetMesh.Text.Equals(sourceMesh.Text))
                            {
                                newMeshes[sourceNud.Nodes.IndexOf((sourceMesh))] = targetMesh;
                                break;
                            }
                        }
                    }

                    // Fill in mismatched meshes
                    foreach (Nud.Mesh targetMesh in targetNud.Nodes)
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
                            newMeshes[i] = new Nud.Mesh();
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
                if (filesTreeView.SelectedNode is Nud.Polygon)
                {
                    Nud.Mesh parent = ((Nud.Mesh)filesTreeView.SelectedNode.Parent);
                    parent.Nodes.Remove((Nud.Polygon)filesTreeView.SelectedNode);
                    Nud parentNud = ((Nud)parent.Parent);
                    parentNud.UpdateRenderMeshes();
                }
                else if (filesTreeView.SelectedNode is Nud.Mesh)
                {
                    Nud parent = ((Nud)filesTreeView.SelectedNode.Parent);
                    filesTreeView.SelectedNode.Parent.Nodes.Remove(filesTreeView.SelectedNode);
                    parent.UpdateRenderMeshes();
                }
                else if (filesTreeView.SelectedNode is Nud)
                {
                    Nud model = (Nud)filesTreeView.SelectedNode;

                    filesTreeView.Nodes.Remove(filesTreeView.SelectedNode);
                }
            }
        }

        public void MergeModel()
        {
            if (filesTreeView.SelectedNode is Nud)
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Namco Model (.nud)|*.nud";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string filename = ofd.FileName;
                        Nud nud = new Nud(filename);
                        foreach (Nud.Mesh mesh in nud.Nodes)
                            ((Nud)filesTreeView.SelectedNode).Nodes.Add((mesh));
                        ((Nud)filesTreeView.SelectedNode).UpdateRenderMeshes();
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

                // Check for null first to avoid exceptions.
                if (filesTreeView.SelectedNode == null)
                {
                    mainContextMenu.Show(this, new System.Drawing.Point(e.X, e.Y));
                }
                else if (filesTreeView.SelectedNode is Nud.Mesh)
                {
                    meshContextMenu.Show(this, e.X, e.Y);
                }
                else if (filesTreeView.SelectedNode is Nud.Polygon)
                {
                    polyContextMenu.Show(this, e.X, e.Y);
                }
                else if (filesTreeView.SelectedNode is Nud)
                {
                    nudContextMenu.Show(this, e.X, e.Y);
                }
                else if (filesTreeView.SelectedNode.Tag is SALT.Graphics.XMBFile)
                {
                    xmbContextMenu.Show(this, e.X, e.Y);
                }
                else if (filesTreeView.SelectedNode is BFRES.Mesh)
                {
                    bfresMeshContextMenu.Show(this, e.X, e.Y);
                }
                else if (filesTreeView.SelectedNode is BFRES)
                {
                    bfresToolStripMenu.Show(this, e.X, e.Y);
                }
                else if (filesTreeView.SelectedNode is BFRES.FMDL_Model)
                {
                    bfresFmdlcontextMenuStrip1.Show(this, e.X, e.Y);
                }
                else if (filesTreeView.SelectedNode is KCL)
                {
                    kclContextMenuStrip1.Show(this, e.X, e.Y);
                }
                else if (filesTreeView.SelectedNode is ModelContainer)
                {
                    ModelContainerContextMenu.Show(this, e.X, e.Y);
                }
            }
        }

        private void flipUVsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is Nud.Polygon)
            {
                Nud.Polygon p = (Nud.Polygon)filesTreeView.SelectedNode;

                foreach (Nud.Vertex v in p.vertices)
                {
                    for(int i = 0; i < v.uv.Count; i++)
                        v.uv[i] = new OpenTK.Vector2(v.uv[i].X, 1 - v.uv[i].Y);
                }

                foreach (TreeNode con in filesTreeView.Nodes)
                {
                    if(con is ModelContainer)
                    {
                        if (((ModelContainer)con).NUD != null)
                            ((ModelContainer)con).NUD.UpdateRenderMeshes();
                    }
                }
            }
        }

        private void editMaterialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PolySelected((Nud.Polygon)filesTreeView.SelectedNode, $"{filesTreeView.SelectedNode.Parent.Text} {filesTreeView.SelectedNode.Text}");
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
            Nud.Mesh mesh = (Nud.Mesh)filesTreeView.SelectedNode;

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
                    mesh.boneflag = (int)Nud.Mesh.BoneFlags.NotRigged;
                else
                    mesh.boneflag = (int)Nud.Mesh.BoneFlags.SingleBind;
                mesh.singlebind = brs.boneIndex;
                foreach (Nud.Polygon poly in mesh.Nodes)
                {
                    poly.boneType = (int)Nud.Polygon.BoneTypes.NoBones;
                    foreach (Nud.Vertex vi in poly.vertices)
                    {
                        vi.boneIds.Clear();
                        vi.boneIds.Add(mesh.singlebind);
                        vi.boneWeights.Clear();
                        vi.boneWeights.Add(1);
                    }
                }
                ((Nud)filesTreeView.SelectedNode.Parent).UpdateRenderMeshes();
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
            if (!(filesTreeView.SelectedNode is Nud))
                return;
            MakeMetal makeMetal = new MakeMetal(((Nud)filesTreeView.SelectedNode));
            makeMetal.Show();
        }

        private void Merge(TreeNode n)
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

            newModelContainer.NUD.UpdateRenderMeshes();

            filesTreeView.Nodes.Remove(filesTreeView.SelectedNode);
            filesTreeView.SelectedNode = n;

            RefreshNodes();
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
                        ((Nud)filesTreeView.SelectedNode).Save(filename);
                    }
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MeshMover mm = new MeshMover();
            if(filesTreeView.SelectedNode!=null)
                if(filesTreeView.SelectedNode is Nud.Mesh)
                    mm.mesh = (Nud.Mesh)filesTreeView.SelectedNode;
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
            List<Nud.Polygon> polys = PolygonSelector.Popup(GetModelContainers());
            foreach (Nud.Polygon poly in polys)
            {
                // link materials. don't link a material to itself
                if (((Nud.Polygon)filesTreeView.SelectedNode) != poly)
                {
                    poly.materials.Clear();
                    foreach (Nud.Material m in ((Nud.Polygon)filesTreeView.SelectedNode).materials)
                        poly.materials.Add(m.Clone());
                }

            }
        }

        private void openEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is Nud)
            {
                Nud org = (Nud)filesTreeView.SelectedNode;
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
            if (filesTreeView.SelectedNode is Nud.Polygon)
            {
                Nud.Polygon p = ((Nud.Polygon)filesTreeView.SelectedNode);
                Nud.Mesh parent = (Nud.Mesh)p.Parent;
                p.Parent.Nodes.Remove(p);
                Nud.Mesh m = new Nud.Mesh();
                ((Nud)parent.Parent).Nodes.Add(m);
                m.Text = parent.Text + "_" + p.Text;
                m.Nodes.Add(p);

                if (parent.Nodes.Count == 0) ((Nud)parent.Parent).Nodes.Remove(parent);

                RefreshNodes();
            }
        }

        private void aboveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is Nud.Mesh)
            {
                Nud.Mesh sourceMesh = (Nud.Mesh)filesTreeView.SelectedNode;
                Nud nud = (Nud)(sourceMesh.Parent);
                int index = nud.Nodes.IndexOf(sourceMesh) - 1;
                MergeMeshes(sourceMesh, index);
            }
        }

        private void belowToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is Nud.Mesh)
            {
                Nud.Mesh sourceMesh = (Nud.Mesh)filesTreeView.SelectedNode;
                Nud nud = (Nud)(sourceMesh.Parent);
                int index = nud.Nodes.IndexOf(sourceMesh) + 1;
                MergeMeshes(sourceMesh, index);
            }
        }

        private void MergeMeshes(Nud.Mesh sourceMesh, int targetMeshIndex)
        {
            Nud nud = (Nud)(sourceMesh.Parent);
            if (targetMeshIndex >= nud.Nodes.Count || targetMeshIndex < 0)
                return;

            // Merge the selected mesh onto the next mesh.
            Nud.Mesh targetMesh = (Nud.Mesh)nud.Nodes[targetMeshIndex];
            nud.Nodes.Remove(sourceMesh);
            TransferMeshPolygons(sourceMesh, targetMesh);
            RefreshNodes();
        }

        private static void TransferMeshPolygons(Nud.Mesh sourceMesh, Nud.Mesh targetMesh)
        {
            List<TreeNode> sourcePolygons = new List<TreeNode>();
            foreach (Nud.Polygon p in sourceMesh.Nodes)
                sourcePolygons.Add(p);

            foreach (Nud.Polygon p in sourcePolygons)
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
            if (filesTreeView.SelectedNode is Nud.Polygon)
            {
                Nud.Polygon p = (Nud.Polygon)filesTreeView.SelectedNode;

                foreach (Nud.Vertex v in p.vertices)
                {
                    for (int i = 0; i < v.uv.Count; i++)
                        v.uv[i] = new OpenTK.Vector2(1 - v.uv[i].X, v.uv[i].Y);
                }

                foreach (TreeNode con in filesTreeView.Nodes)
                {
                    if (con is ModelContainer)
                    {
                        if (((ModelContainer)con).NUD != null)
                            ((ModelContainer)con).NUD.UpdateRenderMeshes();
                    }
                }
            }
        }

        private void exportAsXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is Nud)
            {
                Nud nud = (Nud)filesTreeView.SelectedNode;

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
            if (filesTreeView.SelectedNode is Nud)
            {
                Nud nud = (Nud)filesTreeView.SelectedNode;

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
            if (filesTreeView.SelectedNode is Nud)
            {
                Nud nud = (Nud)filesTreeView.SelectedNode;

                Nud.Mesh m = new Nud.Mesh();

                int i = 0;
                bool foundName = false;
                while (!foundName)
                {
                    m.Text = $"Blank Mesh {i++}";
                    foundName = true;
                    foreach (Nud.Mesh mesh in nud.Nodes)
                        if (mesh.Text.Equals(m.Text))
                            foundName = false;
                }

                nud.Nodes.Add(m);

                RefreshNodes();
            }
        }

        private void generateTanBitanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud))
                return;

            var messageBox = MessageBox.Show("If the vertex type does not support tangents/bitangents, \n" +
                "the vertex type will be changed to Normals, Tan, Bi-Tan (Float). \n" +
                "This will increase the file size.", filesTreeView.SelectedNode.Text, MessageBoxButtons.OKCancel);

            if (messageBox == DialogResult.OK)
            {
                Nud n = (Nud)filesTreeView.SelectedNode;
                foreach (Nud.Mesh mesh in n.Nodes)
                {
                    foreach (Nud.Polygon poly in mesh.Nodes)
                    {
                        GenerateTanBitanAndFixVertType(poly);
                    }
                }

                // Update the data for rendering.
                n.UpdateRenderMeshes();
            }
        }

        private void generateTanBitanToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Polygon))
                return;

            Nud.Polygon poly = ((Nud.Polygon)filesTreeView.SelectedNode);
            GenerateTanBitanAndFixVertType(poly);

            // Update the data for rendering.
            Nud n = (Nud)poly.Parent.Parent;
            n.UpdateRenderMeshes();
        }

        private void useAOAsSpecToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is Nud)
            {
                foreach (Nud.Mesh mesh in ((Nud)filesTreeView.SelectedNode).Nodes)
                {
                    foreach (Nud.Polygon poly in mesh.Nodes)
                    {
                        poly.AOSpecRefBlend();
                    }
                }
            }
        }

        private void useAOAsSpecToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is Nud.Polygon)
                ((Nud.Polygon)filesTreeView.SelectedNode).AOSpecRefBlend();
        }

        private void belowToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            TreeNode n = filesTreeView.SelectedNode.NextNode;
            if (n != null)
            {
                Merge(n);
            }
        }

        private void aboveToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            TreeNode n = filesTreeView.SelectedNode.PrevNode;
            if (n != null)
            {
                Merge(n);
            }
        }

        private void exportAsDAEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Supported Filetypes (DAE)|*.dae;*.obj;|All files(*.*)|*.*";
            DialogResult result = save.ShowDialog();

            if (result == DialogResult.OK && filesTreeView.SelectedNode is ModelContainer)
            {
                if (save.FileName.EndsWith(".dae"))
                    Collada.Save(save.FileName, (ModelContainer)filesTreeView.SelectedNode);
                if (save.FileName.EndsWith(".obj"))
                    OBJ.Save(save.FileName, (ModelContainer)filesTreeView.SelectedNode);
            }
        }

        private void importFromDAEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is ModelContainer))
                return;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ModelContainer con = (ModelContainer)filesTreeView.SelectedNode;

                    if (con.Bfres != null)
                    {
                        for (int p = 0; p < con.Bfres.models.Count; p++)
                        {
                            Collada.DaetoBfresReplace(ofd.FileName, con, p, false);
                        }
                    }
                    else
                    {
                        DAEImportSettings daeImport = new DAEImportSettings();
                        daeImport.ShowDialog();
                        if (daeImport.exitStatus == DAEImportSettings.ExitStatus.Opened)
                        {
                            con.VBN = daeImport.getVBN();

                            Collada.DaetoNud(ofd.FileName, con, daeImport.importTexCB.Checked);

                            // apply settings
                            if (con.NUD != null)
                                daeImport.Apply(con.NUD);
                        }
                    }
                }
            }
        }

        private void generateBoundingSpheresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((Nud)filesTreeView.SelectedNode).GenerateBoundingSpheres();
        }

        private void recalculateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Mesh))
                return;

            Nud.Mesh mesh = (Nud.Mesh)filesTreeView.SelectedNode;
            foreach (Nud.Polygon poly in mesh.Nodes)
            {
                poly.CalculateNormals();
            }

            // Update the data for rendering.
            Nud n = (Nud)mesh.Parent;
            n.UpdateRenderMeshes();
        }

        private void smoothToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Mesh))
                return;

            Nud.Mesh mesh = (Nud.Mesh)filesTreeView.SelectedNode;
            foreach (Nud.Polygon poly in mesh.Nodes)
            {
                poly.SmoothNormals();
            }

            // Update the data for rendering.
            Nud n = (Nud)mesh.Parent;
            n.UpdateRenderMeshes();
        }

        private void recalculateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Polygon))
                return;

            Nud.Polygon p = (Nud.Polygon)filesTreeView.SelectedNode;
            p.CalculateNormals();

            // Update the data for rendering.
            Nud n = (Nud)p.Parent.Parent;
            n.UpdateRenderMeshes();
        }

        private void smoothToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Polygon))
                return;

            Nud.Polygon p = (Nud.Polygon)filesTreeView.SelectedNode;
            p.SmoothNormals();

            // Update the data for rendering.
            Nud n = (Nud)p.Parent.Parent;
            n.UpdateRenderMeshes();
        }

        private void smoothToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud))
                return;

            Nud n = (Nud)filesTreeView.SelectedNode;
            foreach (Nud.Mesh mesh in ((Nud)filesTreeView.SelectedNode).Nodes)
            {
                foreach (Nud.Polygon poly in mesh.Nodes)
                {
                    poly.SmoothNormals();
                }
            }

            // Update the data for rendering.
            n.UpdateRenderMeshes();
        }

        private void generateTanBitanToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Mesh))
                return;

            string meshName = filesTreeView.SelectedNode.Text;
            var messageBox = MessageBox.Show("If the vertex type does not support tangents/bitangents, \n" +
                "the vertex type will be changed to Normals, Tan, Bi-Tan (Float). \n" +
                "This will increase the file size.", meshName, MessageBoxButtons.OKCancel);

            if (messageBox == DialogResult.OK)
            {
                Nud.Mesh mesh = (Nud.Mesh)filesTreeView.SelectedNode;
                foreach (Nud.Polygon poly in mesh.Nodes)
                {
                    GenerateTanBitanAndFixVertType(poly);
                }

                // Update the data for rendering.
                Nud n = (Nud)mesh.Parent;
                n.UpdateRenderMeshes();
            }
        }

        private static void GenerateTanBitanAndFixVertType(Nud.Polygon poly)
        {
            if (poly.normalType == (int)Nud.Polygon.VertexTypes.NormalsFloat)
                poly.normalType = (int)Nud.Polygon.VertexTypes.NormalsTanBiTanFloat;
            else if (poly.normalType == (int)Nud.Polygon.VertexTypes.NormalsHalfFloat)
                poly.normalType = (int)Nud.Polygon.VertexTypes.NormalsTanBiTanHalfFloat;

            // This does nothing if the vertex type doesn't support it.
            poly.CalculateTangentBitangent();
        }

        private void setToWhiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Polygon))
                return;

            Nud.Polygon p = (Nud.Polygon)filesTreeView.SelectedNode;
            p.SetVertexColor(new OpenTK.Vector4(127, 127, 127, 127));

            // Update the data for rendering.
            Nud n = (Nud)p.Parent.Parent;
            n.UpdateRenderMeshes();
        }

        private void selectColorUtilstripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Polygon))
                return;

            // Use a dialog so the color isn't set until the color editor is closed.
            ColorEditor colorEditor = new ColorEditor(new OpenTK.Vector3(1));
            colorEditor.ShowDialog();

            // Remap the color from 1.0 being white to 127 being white.
            OpenTK.Vector3 newVertColor = colorEditor.GetColor() * 127;
            Nud.Polygon p = (Nud.Polygon)filesTreeView.SelectedNode;
            p.SetVertexColor(new OpenTK.Vector4(newVertColor, 127));

            // Update the data for rendering.
            Nud n = (Nud)p.Parent.Parent;
            n.UpdateRenderMeshes();
        }

        private void tangentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Polygon))
                return;

            Nud.Polygon p = (Nud.Polygon)filesTreeView.SelectedNode;
            foreach (Nud.Vertex v in p.vertices)
            {
                OpenTK.Vector3 newTan = v.tan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                v.color = new OpenTK.Vector4(newTan * 127, 127);
            }

            // Update the data for rendering.
            Nud n = (Nud)p.Parent.Parent;
            n.UpdateRenderMeshes();
        }

        private void bitangentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Polygon))
                return;

            Nud.Polygon p = (Nud.Polygon)filesTreeView.SelectedNode;
            foreach (Nud.Vertex v in p.vertices)
            {
                OpenTK.Vector3 newBitan = v.bitan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                v.color = new OpenTK.Vector4(newBitan * 127, 127);
            }

            // Update the data for rendering.
            Nud n = (Nud)p.Parent.Parent;
            n.UpdateRenderMeshes();
        }

        private void normalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Polygon))
                return;

            Nud.Polygon p = (Nud.Polygon)filesTreeView.SelectedNode;
            foreach (Nud.Vertex v in p.vertices)
            {
                OpenTK.Vector3 newNrm = v.nrm * 0.5f + new OpenTK.Vector3(0.5f);
                v.color = new OpenTK.Vector4(newNrm * 127, 127);
            }

            // Update the data for rendering.
            Nud n = (Nud)p.Parent.Parent;
            n.UpdateRenderMeshes();
        }

        private void uVsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Polygon))
                return;

            Nud.Polygon p = (Nud.Polygon)filesTreeView.SelectedNode;
            foreach (Nud.Vertex v in p.vertices)
            {
                v.color = new OpenTK.Vector4(v.uv[0].X * 127, v.uv[0].Y * 127, 127, 127);
            }

            // Update the data for rendering.
            Nud n = (Nud)p.Parent.Parent;
            n.UpdateRenderMeshes();
        }

        private void polyFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Polygon))
                return;

            Nud.Polygon poly = (Nud.Polygon)filesTreeView.SelectedNode;
            PolygonFormatEditor pfe = new PolygonFormatEditor(poly);
            pfe.ShowDialog();
            ((Nud)poly.Parent.Parent).UpdateRenderMeshes();
        }

        private void setToWhiteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Mesh))
                return;

            Nud.Mesh mesh = (Nud.Mesh)filesTreeView.SelectedNode;
            foreach (Nud.Polygon poly in mesh.Nodes)
            {
                poly.SetVertexColor(new OpenTK.Vector4(127, 127, 127, 127));
            }

            // Update the data for rendering.
            Nud n = (Nud)mesh.Parent;
            n.UpdateRenderMeshes();
        }

        private void selectColorUtilstripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Mesh))
                return;

            // Use a dialog so the color isn't set until the color editor is closed.
            ColorEditor colorEditor = new ColorEditor(new OpenTK.Vector3(1));
            colorEditor.ShowDialog();

            // Remap the color from 1.0 being white to 127 being white.
            OpenTK.Vector3 newVertColor = colorEditor.GetColor() * 127;

            Nud.Mesh mesh = (Nud.Mesh)filesTreeView.SelectedNode;
            foreach (Nud.Polygon poly in mesh.Nodes)
            {
                poly.SetVertexColor(new OpenTK.Vector4(newVertColor, 127));
            }

            // Update the data for rendering.
            Nud n = (Nud)mesh.Parent;
            n.UpdateRenderMeshes();
        }

        private void tangentsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Mesh))
                return;

            Nud.Mesh mesh = (Nud.Mesh)filesTreeView.SelectedNode;
            foreach (Nud.Polygon poly in mesh.Nodes)
            {
                foreach (Nud.Vertex v in poly.vertices)
                {
                    OpenTK.Vector3 newTan = v.tan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                    v.color = new OpenTK.Vector4(newTan * 127, 127);
                }
            }

            // Update the data for rendering.
            Nud n = (Nud)mesh.Parent;
            n.UpdateRenderMeshes();
        }

        private void bitangentsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Mesh))
                return;

            Nud.Mesh mesh = (Nud.Mesh)filesTreeView.SelectedNode;
            foreach (Nud.Polygon poly in mesh.Nodes)
            {
                foreach (Nud.Vertex v in poly.vertices)
                {
                    OpenTK.Vector3 newBitan = v.bitan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                    v.color = new OpenTK.Vector4(newBitan * 127, 127);
                }
            }

            // Update the data for rendering.
            Nud n = (Nud)mesh.Parent;
            n.UpdateRenderMeshes();
        }

        private void normalsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Mesh))
                return;

            Nud.Mesh mesh = (Nud.Mesh)filesTreeView.SelectedNode;
            foreach (Nud.Polygon poly in mesh.Nodes)
            {
                foreach (Nud.Vertex v in poly.vertices)
                {
                    OpenTK.Vector3 newNrm = v.nrm * 0.5f + new OpenTK.Vector3(0.5f);
                    v.color = new OpenTK.Vector4(newNrm * 127, 127);
                }
            }

            // Update the data for rendering.
            Nud n = (Nud)mesh.Parent;
            n.UpdateRenderMeshes();
        }

        private void uVsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Mesh))
                return;

            Nud.Mesh mesh = (Nud.Mesh)filesTreeView.SelectedNode;
            foreach (Nud.Polygon poly in mesh.Nodes)
            {
                foreach (Nud.Vertex v in poly.vertices)
                {
                    v.color = new OpenTK.Vector4(v.uv[0].X * 127, v.uv[0].Y * 127, 127, 127);
                }
            }

            // Update the data for rendering.
            Nud n = (Nud)mesh.Parent;
            n.UpdateRenderMeshes();
        }

        private void tangentsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud))
                return;

            Nud n = (Nud)filesTreeView.SelectedNode;

            foreach (Nud.Mesh m in n.Nodes)
            {
                foreach (Nud.Polygon p in m.Nodes)
                {
                    foreach (Nud.Vertex v in p.vertices)
                    {
                        OpenTK.Vector3 newTan = v.tan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                        v.color = new OpenTK.Vector4(newTan * 127, 127);
                    }
                }
            }

            // Update the data for rendering.
            n.UpdateRenderMeshes();
        }

        private void bitangentsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud))
                return;

            Nud n = (Nud)filesTreeView.SelectedNode;

            foreach (Nud.Mesh m in n.Nodes)
            {
                foreach (Nud.Polygon p in m.Nodes)
                {
                    foreach (Nud.Vertex v in p.vertices)
                    {
                        OpenTK.Vector3 newBitan = v.bitan.Xyz * 0.5f + new OpenTK.Vector3(0.5f);
                        v.color = new OpenTK.Vector4(newBitan * 127, 127);
                    }
                }
            }

            // Update the data for rendering.
            n.UpdateRenderMeshes();
        }

        private void setToWhiteToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud))
                return;

            Nud n = (Nud)filesTreeView.SelectedNode;

            foreach (Nud.Mesh m in n.Nodes)
            {
                foreach (Nud.Polygon p in m.Nodes)
                {
                    p.SetVertexColor(new OpenTK.Vector4(127, 127, 127, 127));
                }
            }

            // Update the data for rendering.
            n.UpdateRenderMeshes();
        }

        private void normalsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud))
                return;

            Nud n = (Nud)filesTreeView.SelectedNode;

            foreach (Nud.Mesh m in n.Nodes)
            {
                foreach (Nud.Polygon p in m.Nodes)
                {
                    foreach (Nud.Vertex v in p.vertices)
                    {
                        OpenTK.Vector3 newNrm = v.nrm * 0.5f + new OpenTK.Vector3(0.5f);
                        v.color = new OpenTK.Vector4(newNrm * 127, 127);
                    }
                }
            }

            // Update the data for rendering.
            n.UpdateRenderMeshes();
        }

        private void uVsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud))
                return;

            Nud n = (Nud)filesTreeView.SelectedNode;
            foreach (Nud.Mesh m in n.Nodes)
            {
                foreach (Nud.Polygon p in m.Nodes)
                {
                    foreach (Nud.Vertex v in p.vertices)
                    {
                        v.color = new OpenTK.Vector4(v.uv[0].X * 127, v.uv[0].Y * 127, 127, 127);
                    }
                }
            }

            // Update the data for rendering.
            n.UpdateRenderMeshes();
        }

        private void selectColorUtilstripMenuItem2_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud))
                return;

            Nud n = (Nud)filesTreeView.SelectedNode;

            // Use a dialog so the color isn't set until the color editor is closed.
            ColorEditor colorEditor = new ColorEditor(new OpenTK.Vector3(1));
            colorEditor.ShowDialog();

            // Remap the color from 1.0 being white to 127 being white.
            OpenTK.Vector3 newVertColor = colorEditor.GetColor() * 127;

            foreach (Nud.Mesh m in n.Nodes)
            {
                foreach (Nud.Polygon p in m.Nodes)
                {
                    p.SetVertexColor(new OpenTK.Vector4(newVertColor, 127));
                }
            }

            // Update the data for rendering.
            n.UpdateRenderMeshes();
        }

        private void texIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud))
                return;

            Nud n = (Nud)filesTreeView.SelectedNode;

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
                texIdSelector.Set(((NutTexture)modelContainer.NUT.Nodes[0]).HashId);
                texIdSelector.ShowDialog();
                if (texIdSelector.exitStatus == TexIdSelector.ExitStatus.Opened)
                {
                    modelContainer.NUD.ChangeTextureIds(texIdSelector.getNewTexId());
                    modelContainer.NUT.ChangeTextureIds(texIdSelector.getNewTexId());
                }
            }
        }

#region BFRES Menus

        private void openMaterialEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BfresShapeSelected((BFRES.Mesh)filesTreeView.SelectedNode, $"");
        }

        private void openPolygonEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainForm.Instance.BfresOpenMeshEditor((BFRES.Mesh)filesTreeView.SelectedNode, (BFRES.FMDL_Model)filesTreeView.SelectedNode.Parent, (BFRES)filesTreeView.SelectedNode.Parent.Parent.Parent, $"");
        }

        private void bfresSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = "";
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Binary caFe RESource|*.bfres|All files(*.*)|*.*";
            DialogResult result = save.ShowDialog();

            if (result == DialogResult.OK)
            {
                filename = save.FileName;
                if (filename.EndsWith(".bfres"))
                {
                    if (((BFRES)filesTreeView.SelectedNode).TargetSwitchBFRES != null)
                    {
                        ((BFRES)filesTreeView.SelectedNode).InjectToFile(filename);
                    }
                    else
                    {
                        ((BFRES)filesTreeView.SelectedNode).InjectToWiiUBFRES(filename);
                    }


                }
            }
        }

        private void flipUVsVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is BFRES.Mesh)
            {
                BFRES.Mesh msh = (BFRES.Mesh)filesTreeView.SelectedNode;

                foreach (BFRES.Vertex v in msh.vertices)
                {
                    v.uv0 = new OpenTK.Vector2(v.uv0.X, 1 - v.uv0.Y);
                }

                foreach (TreeNode con in filesTreeView.Nodes)
                {
                    if (con is ModelContainer)
                    {
                        if (((ModelContainer)con).Bfres != null)
                            ((ModelContainer)con).Bfres.UpdateRenderMeshes();
                    }
                }
            }
        }

        private void flipUVsHorizontalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (filesTreeView.SelectedNode is BFRES.Mesh)
            {
                BFRES.Mesh msh = (BFRES.Mesh)filesTreeView.SelectedNode;

                foreach (BFRES.Vertex v in msh.vertices)
                {
                    v.uv0 = new OpenTK.Vector2(1 - v.uv0.X, v.uv0.Y);
                }

                foreach (TreeNode con in filesTreeView.Nodes)
                {
                    if (con is ModelContainer)
                    {
                        if (((ModelContainer)con).Bfres != null)
                            ((ModelContainer)con).Bfres.UpdateRenderMeshes();
                    }
                }
            }
        }
        private void bfresGenerateTanBitanToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is BFRES.Mesh))
                return;

            BFRES.Mesh mesh = (BFRES.Mesh)filesTreeView.SelectedNode;

            BfresGenerateTanBitanAndFixVertType(mesh);

            // Update the data for rendering.
            foreach (TreeNode con in filesTreeView.Nodes)
            {
                if (con is ModelContainer)
                {
                    if (((ModelContainer)con).Bfres != null)
                        ((ModelContainer)con).Bfres.UpdateRenderMeshes();
                }
            }
        }

        private static void BfresGenerateTanBitanAndFixVertType(BFRES.Mesh mesh)
        {
            // This already checks for the appropriate vertex type.
            mesh.CalculateTangentBitangent();
        }

        private void smoothNormalsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void bfresRecalculateToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is BFRES.Mesh))
                return;

            BFRES.Mesh mesh = (BFRES.Mesh)filesTreeView.SelectedNode;
            mesh.CalculateNormals();


            // Update the data for rendering.
            foreach (TreeNode con in filesTreeView.Nodes)
            {
                if (con is ModelContainer)
                {
                    if (((ModelContainer)con).Bfres != null)
                        ((ModelContainer)con).Bfres.UpdateRenderMeshes();
                }
            }
        }

        private void bfresSmoothToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is BFRES.Mesh))
                return;

            BFRES.Mesh mesh = (BFRES.Mesh)filesTreeView.SelectedNode;
            mesh.SmoothNormals();


            // Update the data for rendering.
            foreach (TreeNode con in filesTreeView.Nodes)
            {
                if (con is ModelContainer)
                {
                    if (((ModelContainer)con).Bfres != null)
                        ((ModelContainer)con).Bfres.UpdateRenderMeshes();
                }
            }
        }

        private void bfresNormalsToolStripMenuItem3_Click(object sender, EventArgs e)
        {

        }

        private void bfresSetVertexColors_Click(object sender, EventArgs e)
        {

        }

        private void setColorUtilstripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is BFRES.Mesh))
                return;

            BFRES.Mesh mesh = (BFRES.Mesh)filesTreeView.SelectedNode;

            ColorDialog colorDialog1 = new ColorDialog();
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                mesh.SetVertexColor(new OpenTK.Vector4(
                    colorDialog1.Color.R / 255.0f,
                    colorDialog1.Color.G / 255.0f,
                    colorDialog1.Color.B / 255.0f,
                    colorDialog1.Color.A / 255.0f));
            }

            // Update the data for rendering.
            foreach (TreeNode con in filesTreeView.Nodes)
            {
                if (con is ModelContainer)
                {
                    if (((ModelContainer)con).Bfres != null)
                        ((ModelContainer)con).Bfres.UpdateRenderMeshes();
                }
            }
        }

        private void bfresMeshContextMenu_Opening(object sender, CancelEventArgs e)
        {

        }

        private void bfresSetWhiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is BFRES.Mesh))
                return;

            BFRES.Mesh mesh = (BFRES.Mesh)filesTreeView.SelectedNode;

            mesh.SetVertexColor(new OpenTK.Vector4(
                127.0f / 255.0f,
                127.0f / 255.0f,
                127.0f / 255.0f,
                127.0f / 255.0f));

            // Update the data for rendering.
            foreach (TreeNode con in filesTreeView.Nodes)
            {
                if (con is ModelContainer)
                {
                    if (((ModelContainer)con).Bfres != null)
                        ((ModelContainer)con).Bfres.UpdateRenderMeshes();
                }
            }

        }

        public void UpdateBfresMeshList()
        {

            // Update the data for rendering.
            foreach (TreeNode con in filesTreeView.Nodes)
            {
                if (con is ModelContainer)
                {
                    if (((ModelContainer)con).Bfres != null)
                    {
                        foreach (BFRES.FMDL_Model mdl in ((ModelContainer)con).Bfres.models)
                        {
                            foreach (BFRES.Mesh m in mdl.poly)
                            {
                                ((ModelContainer)con).Bfres.UpdateRenderMeshes();
                            }
                        }
                    }
                }
            }
        }

        private void exportMaterialsXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is BFRES.Mesh))
                return;

            BFRES.Mesh msh = (BFRES.Mesh)filesTreeView.SelectedNode;

            msh.ExportMaterials2XML();
        }

        private void bfresConvertWiiU2SwitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is BFRES))
                return;

            // Update the data for rendering.
            foreach (TreeNode con in filesTreeView.Nodes)
            {
                if (con is ModelContainer)
                {
                    if (((ModelContainer)con).Bfres != null)
                    {
                        OpenFileDialog ofd = new OpenFileDialog();
                        ofd.Filter =
                                "Supported Formats|*.bfres;|" +
                                "All files(*.*)|*.*";

                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            if (ofd.FileName.EndsWith(".bfres"))
                            {
                                BFRES.WiiU2Switch(ofd.FileName, filesTreeView.SelectedNode.Index, ((ModelContainer)con).Bfres);
                            }
                        }
                    }
                }
            }
        }

        private void generateTanBitanToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is BFRES.FMDL_Model))
                return;

            BFRES.FMDL_Model mdl = (BFRES.FMDL_Model)filesTreeView.SelectedNode;

            mdl.GenerateTansBitansEachMesh();
            UpdateBfresMeshList();
        }

        private void recalculateToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is BFRES.FMDL_Model))
                return;

            BFRES.FMDL_Model mdl = (BFRES.FMDL_Model)filesTreeView.SelectedNode;

            mdl.GenerateNormalEachMesh();
            UpdateBfresMeshList();
        }

        private void smoothToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is BFRES.FMDL_Model))
                return;

            BFRES.FMDL_Model mdl = (BFRES.FMDL_Model)filesTreeView.SelectedNode;

            mdl.SmoothNormalEachMesh();
            UpdateBfresMeshList();
        }

        private void copyChannel1To2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is BFRES.Mesh))
                return;

            BFRES.Mesh m = (BFRES.Mesh)filesTreeView.SelectedNode;
            m.CopyUVChannel2();
            UpdateBfresMeshList();
        }

        private void singleBindToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is BFRES.Mesh))
                return;

            BFRES.Mesh m = (BFRES.Mesh)filesTreeView.SelectedNode;

            foreach (TreeNode con in filesTreeView.Nodes)
            {
                if (con is ModelContainer)
                {
                    if (((ModelContainer)con).Bfres != null)
                    {
                        m.SingleBindMesh(); //Add BFRES instance so we can use the fmdl and skeleton classes
                        ((ModelContainer)con).Bfres.UpdateRenderMeshes();
                    }
                }
            }
        }

        #endregion

        private void KCLtoolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Supported Filetypes (OBJ)|*.obj;|All files(*.*)|*.*";
            DialogResult result = save.ShowDialog();

            if (result == DialogResult.OK && filesTreeView.SelectedNode is KCL)
            {
                OBJ.KCL2OBJ(save.FileName, (KCL)filesTreeView.SelectedNode);
            }
        }

        private void uvViewerMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode is Nud.Polygon))
                return;

            Nud.Polygon poly = (Nud.Polygon)filesTreeView.SelectedNode;
            Nud nud = (Nud)poly.Parent.Parent;
            UvViewer uvViewer = new UvViewer(nud, poly);
            uvViewer.Show();
        }

        private void openViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(filesTreeView.SelectedNode.Tag is SALT.Graphics.XMBFile))
                return;

            try
            {
                SALT.Graphics.XMBFile xmb = (SALT.Graphics.XMBFile)filesTreeView.SelectedNode.Tag;
                XmbViewer xmbViewer = new XmbViewer(xmb);
                xmbViewer.Show();
            }
            catch (Exception)
            {
                // Something broke. Let's just pretend it didn't happen.
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            filesTreeView.SelectedNode?.BeginEdit();
        }
    }
}
