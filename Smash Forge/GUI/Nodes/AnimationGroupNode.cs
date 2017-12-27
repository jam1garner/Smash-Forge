using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Windows.Forms;
using System.ComponentModel;

namespace Smash_Forge
{
    class AnimationGroupNode : TreeNode
    {

        public AnimationGroupNode()
        {
            Text = "Animation Group";
            ImageKey = "group";
            SelectedImageKey = "group";
            
            ContextMenu cm = new ContextMenu();

            MenuItem resave = new MenuItem("Save Group As");
            resave.Click += Resave;
            cm.MenuItems.Add(resave);

            ContextMenu = cm;
        }

        public static void Save(object sender, EventArgs args)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            float f = 0;
            if (FileName.ToLower().EndsWith(".bch"))
            {
                List<Animation> anims = new List<Animation>();
                foreach (Animation a in ((TreeNode)Node).Nodes)
                {
                    worker.ReportProgress((int)((f / Node.Nodes.Count) * 100f));
                    f++;
                    anims.Add(a);
                }
                BCH_Animation.Rebuild(FileName, anims);
            }
            if (FileName.ToLower().EndsWith(".pac"))
            {
                var pac = new PAC();
                foreach (Animation anim in Node.Nodes)
                {
                    worker.ReportProgress((int)((f / Node.Nodes.Count) * 100f));
                    f++;
                    //MainForm.Instance.Progress.Message = ("Working on " + anim.Text);
                    var bytes = OMOOld.CreateOMOFromAnimation(anim, Runtime.TargetVBN);
                    if (anim.Tag is FileData)
                        bytes = ((FileData)Runtime.TargetAnim.Tag).getSection(0, ((FileData)Runtime.TargetAnim.Tag).size());

                    pac.Files.Add(anim.Text + ".omo", bytes);
                }
                pac.Save(FileName);
            }
        }

        public static String FileName;
        public static TreeNode Node;

        public static void Resave(object sender, EventArgs args)
        {
            TreeNode n = MainForm.Instance.animList.treeView1.SelectedNode;

            if(Runtime.TargetVBN == null)
            {
                MessageBox.Show("You must have a bone set (VBN) open before saving animations");
                return;
            }

            if (n is AnimationGroupNode)
            {
                using (SaveFileDialog sf = new SaveFileDialog())
                {
                    sf.Filter = "Supported Files (.bch, .pac)|*.bch;*.pac|" +
                                 "PAC (.pac)|*.pac|" +
                                 "BCH (.bch)|*.bch|" +
                                 "All Files (*.*)|*.*";

                    if (sf.ShowDialog() == DialogResult.OK)
                    {
                        Node = MainForm.Instance.animList.treeView1.SelectedNode;
                        FileName = sf.FileName;

                        MainForm.Instance.Progress.ProgressValue = 0;
                        MainForm.Instance.Progress.Message = ("Please Wait... Baking Animation Frames");

                        MainForm.Instance.backgroundWorker1.DoWork += new DoWorkEventHandler(Save);
                        MainForm.Instance.backgroundWorker1.RunWorkerAsync();

                        MainForm.Instance.Progress.ShowDialog();
                    }
                }
            }
        }

    }
}
