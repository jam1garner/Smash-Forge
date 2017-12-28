using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Smash_Forge
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
            treeView1.ImageList = iconList;
        }

        private void lstAnims_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Runtime.TargetAnim = Runtime.Animations[lstAnims.SelectedItem.ToString()];
            //Runtime.TargetAnimString = lstAnims.SelectedItem.ToString();
        }

        private void selectItem(object sender, TreeNodeMouseClickEventArgs e)
        {
            Runtime.TargetMTA.Clear();
            Runtime.TargetAnim = null;

            if (e.Node is Animation)
            {
                Runtime.TargetAnimString = e.Node.Text;

                Animation running = new Animation("Running");
                running.ReplaceMe((Animation)e.Node);

                Queue<TreeNode> NodeQueue = new Queue<TreeNode>();
                foreach(TreeNode n in MainForm.animNode.Nodes)
                {
                    NodeQueue.Enqueue(n);
                }
                List<MTA> display = new List<MTA>();
                while (NodeQueue.Count > 0)
                {
                    TreeNode n = NodeQueue.Dequeue();
                    if (n is Animation)
                    {
                        if (n == e.Node)
                            continue;
                        if (n.Text.Equals(e.Node.Text))
                            running.Children.Add(n);
                    }
                    if (n is MTA)
                    {
                        if (n == e.Node)
                            continue;
                        if (n.Text.Contains(e.Node.Text.Replace(".omo", ".")))
                            running.Children.Add(n);
                        if (n.Text.Contains("default.mta"))
                            display.Add((MTA)n);
                    }
                    if (n is AnimationGroupNode)
                    {
                        foreach (TreeNode tn in n.Nodes)
                            NodeQueue.Enqueue(tn);
                    }
                }
                
                Runtime.TargetAnim = running;// Runtime.Animations[e.Node.Text];

                //reset mtas
                foreach (ModelContainer con in Runtime.ModelContainers)
                {
                    if (con.nud != null && con.mta != null)
                    {
                        con.nud.applyMTA(con.mta, 0);
                        foreach(MTA d in display)
                            con.nud.applyMTA(d, 0);

                        /*foreach (KeyValuePair<string, MTA> v in Runtime.MaterialAnimations)
                        {
                            if (v.Key.Contains("display"))
                            {
                                con.nud.applyMTA(v.Value, 0);
                                break;
                            }
                        }*/
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
                MainForm.Instance.viewports[0].loadMTA((MTA)e.Node);
                //Runtime.TargetMTA = ;
                Runtime.TargetMTAString = e.Node.Text;

                Queue<TreeNode> NodeQueue = new Queue<TreeNode>();
                foreach (TreeNode n in MainForm.animNode.Nodes)
                {
                    NodeQueue.Enqueue(n);
                }
                List<MTA> display = new List<MTA>();
                while (NodeQueue.Count > 0)
                {
                    TreeNode n = NodeQueue.Dequeue();
                    if (n is Animation || n is MTA)
                    {
                        if (n == e.Node)
                            continue;
                        if (n.Text.Contains("default.mta") && n is MTA)
                            display.Add((MTA)n);
                    }
                    if (n is AnimationGroupNode)
                    {
                        foreach (TreeNode tn in n.Nodes)
                            NodeQueue.Enqueue(tn);
                    }
                }

                foreach (ModelContainer con in Runtime.ModelContainers)
                {
                    if (con.nud != null && con.mta != null)
                    {
                        con.nud.applyMTA(con.mta, 0);
                        foreach (MTA d in display)
                            con.nud.applyMTA(d, 0);
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
        }

        private void treeView1_Click(object sender, EventArgs e)
        {
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = treeView1.GetNodeAt(e.Location);
                if (treeView1.SelectedNode.Level == 0 && treeView1.SelectedNode.Text.Equals("Bone Animations"))
                {
                    AnimationCM.Show(this, e.X, e.Y);
                }
            }
        }

        private void exportAllAsOMOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new FolderSelectDialog())
            {
                ofd.Title = "Character Folder";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string path = ofd.SelectedPath;

                    foreach(TreeNode v in MainForm.animNode.Nodes)
                    {
                        if (v is Animation)
                            OMOOld.createOMO(((Animation)v), Runtime.TargetVBN, path + "\\" + v.Text + ".omo");
                    }
                }
            }
        }

        private void createAnimationGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AnimationGroupNode ag = new AnimationGroupNode();
            MainForm.animNode.Nodes.Add(ag);
            //ag.BeginEdit();
        }
    }
}
