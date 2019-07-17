using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.Text.RegularExpressions;

namespace SmashForge
{
    public partial class AnimListPanel : DockContent
    {

        public static ImageList iconList = new ImageList();

        public AnimListPanel()
        {
            InitializeComponent();
            iconList.ImageSize = new Size(24, 24);
            iconList.Images.Add("group", Properties.Resources.icon_group);
            iconList.Images.Add("anim", Properties.Resources.icon_anim);
            iconList.Images.Add("bone", Properties.Resources.icon_bone);
            iconList.Images.Add("frame", Properties.Resources.icon_model);
            iconList.Images.Add("image", Properties.Resources.icon_image);
            treeView1.ImageList = iconList;

            ContextMenu m = new ContextMenu();

            // Import Animation

            MenuItem exportAll = new MenuItem("Export All as OMO");
            exportAll.Click += exportAllAsOMOToolStripMenuItem_Click;
            m.MenuItems.Add(exportAll);

            MenuItem exportAllSmd = new MenuItem("Export All as SMD");
            exportAllSmd.Click += exportAllAsSMDToolStripMenuItem_Click;
            m.MenuItems.Add(exportAllSmd);

            MenuItem exportAllAnim = new MenuItem("Export All as ANIM");
            exportAllAnim.Click += exportAllAsANIMToolStripMenuItem_Click;
            m.MenuItems.Add(exportAllAnim);

            MenuItem createAg = new MenuItem("Create Animation Group");
            createAg.Click += createAnimationGroupToolStripMenuItem_Click;
            m.MenuItems.Add(createAg);

            MenuItem importPac = new MenuItem("Import PAC");
            importPac.Click += importPAC_Click;
            m.MenuItems.Add(importPac);

            treeView1.ContextMenu = m;
        }

        private void lstAnims_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Runtime.TargetAnim = Runtime.Animations[lstAnims.SelectedItem.ToString()];
            //Runtime.TargetAnimString = lstAnims.SelectedItem.ToString();
        }

        private void SelectItem(object sender, TreeNodeMouseClickEventArgs e)
        {
            //Runtime.TargetMTA.Clear();
            //Runtime.TargetAnim = null;

            if (e.Node is Animation)
            {
                //Runtime.TargetAnimString = e.Node.Text;

                string animName = e.Node.Text;
                animName = Regex.Match(animName, @"([A-Z][0-9][0-9])(.*)").Groups[0].ToString();
                if (animName.Length > 3)
                    animName = animName.Substring(3);

                Animation running = new Animation(animName);
                running.ReplaceMe((Animation)e.Node);
                running.Tag = e.Node;

                List<MTA> display = new List<MTA>();
                List<MTA> def = new List<MTA>();

                Queue<TreeNode> nodeQueue = new Queue<TreeNode>();
                foreach (TreeNode n in treeView1.Nodes)
                {
                    nodeQueue.Enqueue(n);
                }
                while (nodeQueue.Count > 0)
                {
                    try
                    {
                        TreeNode n = nodeQueue.Dequeue();
                        string nodeName = Regex.Match(n.Text, @"([A-Z][0-9][0-9])(.*)").Groups[0].ToString();
                        if (nodeName.Length <= 3)
                            Console.WriteLine(nodeName);
                        else
                            nodeName = nodeName.Substring(3);
                        if (n is Animation)
                        {
                            if (n == e.Node)
                                continue;
                            if (matchAnim.Checked && nodeName.Equals(animName))
                            {
                                running.children.Add(n);
                            }
                        }
                        if (n is MTA)
                        {
                            if (n == e.Node)
                                continue;
                            if (nodeName.Contains(animName.Replace(".omo", ".")))
                                running.children.Add(n);
                            if (n.Text.Contains("display"))
                                display.Add((MTA)n);
                            if (n.Text.Contains("default.mta"))
                                def.Add((MTA)n);
                        }
                        if (n is AnimationGroupNode)
                        {
                            foreach (TreeNode tn in n.Nodes)
                                nodeQueue.Enqueue(tn);
                        }
                    }
                    catch
                    {

                    }
                    
                }

                ((ModelViewport)Parent).CurrentAnimation = running;

                //reset mtas
                foreach (TreeNode node in ((ModelViewport)Parent).draw)
                {
                    if (node is ModelContainer)
                    {
                        ModelContainer con = (ModelContainer)node;
                        if (con.NUD != null)
                        {
                            con.NUD.ClearMta();
                            con.NUD.ApplyMta(con.MTA, 0);
                            foreach (MTA d in display)
                            {
                                con.NUD.ApplyMta(d, 0);
                            }
                            foreach (MTA d in def)
                            {
                                con.NUD.ApplyMta(d, 0);
                            }

                            /*foreach (KeyValuePair<string, MTA> v in Runtime.MaterialAnimations)
                            {
                                if (v.Key.Contains("display"))
                                {
                                    con.NUD.applyMTA(v.Value, 0);
                                    break;
                                }
                            }*/
                        }
                    }

                }

                /*Runtime.TargetMTA.Clear();
                foreach (KeyValuePair<string, MTA> v in Runtime.MaterialAnimations)
                {
                    if (v.Key.Contains(e.Node.Text.Replace(".omo", "")))
                    {
                        Runtime.TargetMTA.Add(v.Value);
                    }
                }*/

                //MainForm.Instance.viewports[0].loadMTA(Runtime.MaterialAnimations[e.Node.Text]);

                //Console.WriteLine("Selected Anim " + e.Node.Text);
            }
            if (e.Node is MTA)
            {
                //MainForm.Instance.viewports[0].loadMTA((MTA)e.Node);
                //Runtime.TargetMTA = ;
                //Runtime.TargetMTAString = e.Node.Text;
                ((ModelViewport)Parent).CurrentMaterialAnimation = (MTA)e.Node;

                Queue<TreeNode> nodeQueue = new Queue<TreeNode>();
                foreach (TreeNode n in treeView1.Nodes)
                {
                    nodeQueue.Enqueue(n);
                }
                List<MTA> display = new List<MTA>();
                while (nodeQueue.Count > 0)
                {
                    TreeNode n = nodeQueue.Dequeue();
                    if (n is Animation || n is MTA)
                    {
                        if (n == e.Node)
                            continue;
                        if ((n.Text.Contains("default.mta") || n.Text.Contains("display")) && n is MTA)
                            display.Add((MTA)n);
                    }
                    if (n is AnimationGroupNode)
                    {
                        foreach (TreeNode tn in n.Nodes)
                            nodeQueue.Enqueue(tn);
                    }
                }

                foreach (TreeNode node in ((ModelViewport)Parent).draw)
                {
                    if (node is ModelContainer)
                    {
                        ModelContainer con = (ModelContainer)node;
                        if (con.NUD != null && con.MTA != null)
                        {
                            con.NUD.ApplyMta(con.MTA, 0);
                            foreach (MTA d in display)
                                con.NUD.ApplyMta(d, 0);

                            /*foreach (KeyValuePair<string, MTA> v in Runtime.MaterialAnimations)
                            {
                                if (v.Key.Contains("display"))
                                {
                                    con.NUD.applyMTA(v.Value, 0);
                                    break;
                                }
                            }*/
                        }
                    }

                }

            }
            if (e.Node is BFRES.MTA) //For BFRES
            {
                ((ModelViewport)Parent).CurrentBfresMaterialAnimation = (BFRES.MTA)e.Node;

                Queue<TreeNode> nodeQueue = new Queue<TreeNode>();
                foreach (TreeNode n in treeView1.Nodes)
                {
                    nodeQueue.Enqueue(n);
                }
                while (nodeQueue.Count > 0)
                {
                    TreeNode n = nodeQueue.Dequeue();

                    if (n is AnimationGroupNode)
                    {
                        foreach (TreeNode tn in n.Nodes)
                            nodeQueue.Enqueue(tn);
                    }
                }
                foreach (TreeNode node in ((ModelViewport)Parent).draw)
                {
                    if (node is ModelContainer)
                    {
                        ModelContainer con = (ModelContainer)node;
                        if (con.Bfres != null && con.BFRES_MTA != null)
                        {
                            con.Bfres.ApplyMta(con.BFRES_MTA, 0);
                        }
                    }
                }
            }

        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is Animation)
            {
                ((Animation)treeView1.SelectedNode).ExpandBones();
            }
            if (treeView1.SelectedNode is Animation.KeyNode)
            {
                ((Animation.KeyNode)treeView1.SelectedNode).ExpandNodes();
            }
            if (treeView1.SelectedNode is Animation.KeyGroup)
            {
                ((Animation.KeyGroup)treeView1.SelectedNode).ExpandNodes();
            }
            if (treeView1.SelectedNode is MTA)
            {
                ((MTA)treeView1.SelectedNode).ExpandNodes();
            }
            if (treeView1.SelectedNode is BFRES.MTA)
            {
                ((BFRES.MTA)treeView1.SelectedNode).ExpandNodes();
            }
        }

        private void treeView1_Click(object sender, EventArgs e)
        {
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {

            /*if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = treeView1.GetNodeAt(e.Location);
                if (treeView1.SelectedNode.Level == 0 && treeView1.SelectedNode.Text.Equals("Bone Animations"))
                {
                    AnimationCM.Show(this, e.X, e.Y);
                }
            }*/
        }

        private void exportAllAsOMOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new FolderSelectDialog())
            {
                ofd.Title = "Character Folder";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string path = ofd.SelectedPath;

                    foreach (TreeNode v in treeView1.Nodes)
                    {
                        foreach (TreeNode a in v.Nodes)
                        {
                            if (a is Animation)
                                OMOOld.createOMO(((Animation)a), Runtime.TargetVbn, path + "\\" + a.Text + ".omo");
                        }        
                    }
                }
            }
        }
        private void exportAllAsSMDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new FolderSelectDialog())
            {
                ofd.Title = "Character Folder";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string path = ofd.SelectedPath;
                    foreach (TreeNode b in treeView1.Nodes)
                    {
                        foreach (TreeNode v in b.Nodes)
                        {
                            foreach (TreeNode f in v.Nodes)
                            {
                                foreach (TreeNode a in f.Nodes)
                                {
                                    if (a is Animation)
                                    {
                                        Smd.Save(((Animation)a), Runtime.TargetVbn, path + "\\" + a.Text + ".smd");
                                    }
                                }
                            }
                        }
                    }           
                }
            }
        }

        private void exportAllAsANIMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new FolderSelectDialog())
            {
                ofd.Title = "Character Folder";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string path = ofd.SelectedPath;
                    foreach (TreeNode b in treeView1.Nodes)
                    {
                        foreach (TreeNode v in b.Nodes)
                        {
                            foreach (TreeNode f in v.Nodes)
                            {
                                foreach (TreeNode a in f.Nodes)
                                {
                                    if (a is Animation)
                                        ANIM.CreateANIM(path + "\\" + a.Text + ".anim", ((Animation)a), Runtime.TargetVbn);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void createAnimationGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnimationGroupNode ag = new AnimationGroupNode();
            treeView1.Nodes.Add(ag);
            //ag.BeginEdit();
        }

        private void importPAC_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter =
                    "Smash 4 PACK (.pac)|*.pac|" +
                    "All files(*.*)|*.*";
                
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                    foreach (string filename in ofd.FileNames)
                        ImportAnimation(filename);
            }
        }

        public void ImportAnimation(string filename)
        {
            if (filename.ToLower().EndsWith(".pac"))
            {
                PAC p = new PAC();
                p.Read(filename);
                AnimationGroupNode animGroup = new AnimationGroupNode() { Text = filename };
                string groupname = null;
                foreach (var pair in p.Files)
                {
                    if (pair.Key.EndsWith(".omo"))
                    {
                        var anim = OMOOld.read(new FileData(pair.Value));
                        animGroup.Nodes.Add(anim);
                        string animName = Regex.Match(pair.Key, @"([A-Z][0-9][0-9])(.*)").Groups[0].ToString();

                        if (!string.IsNullOrEmpty(animName))
                        {
                            if (groupname == null)
                                groupname = pair.Key.Replace(animName, "");
                            anim.Text = animName;
                        }
                        //Runtime.acmdEditor.updateCrcList();
                    }
                    else if (pair.Key.EndsWith(".mta"))
                    {
                        MTA mta = new MTA();
                        mta.read(new FileData(pair.Value));
                        mta.Text = pair.Key;
                        animGroup.Nodes.Add(mta);
                    }
                }

                if (groupname != null && !groupname.Equals(""))
                {
                    animGroup.useGroupName = true;
                    animGroup.Text = groupname;
                }

                treeView1.Nodes.Add(animGroup);
            }
        }
    }
}
