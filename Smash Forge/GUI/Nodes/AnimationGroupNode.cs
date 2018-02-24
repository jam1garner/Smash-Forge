using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Windows.Forms;
using System.ComponentModel;

using System.IO;

namespace Smash_Forge
{
    public class AnimationGroupNode : TreeNode
    {
        public bool UseGroupName = false;

        public AnimationGroupNode()
        {
            Text = "Animation Group";
            ImageKey = "group";
            SelectedImageKey = "group";
            
            ContextMenu cm = new ContextMenu();

            MenuItem resave = new MenuItem("Save Group As");
            resave.Click += Resave;
            cm.MenuItems.Add(resave);

            MenuItem import = new MenuItem("Import New Animation");
            import.Click += Import;
            cm.MenuItems.Add(import);

            MenuItem create = new MenuItem("Create New Skeletal Animation");
            create.Click += Create;
            cm.MenuItems.Add(create);

            MenuItem createmat = new MenuItem("Create New Material Animation");
            createmat.Click += CreateMat;
            cm.MenuItems.Add(createmat);

            MenuItem remove = new MenuItem("Remove Group");
            remove.Click += Remove;
            cm.MenuItems.Add(remove);

            MenuItem replaceImport = new MenuItem("Replace Import");
            replaceImport.Click += ReplaceImport;
            //cm.MenuItems.Add(replaceImport);

            ContextMenu = cm;
        }

        public void ReplaceImport(object sender, EventArgs args)
        {
            using (var ofd = new FolderSelectDialog())
            {
                ofd.Title = "Animation Folder";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    String Files = ofd.SelectedPath;

                    string[] fls = Directory.GetFiles(ofd.SelectedPath);

                    foreach(string s in fls)
                    {
                        string f = Path.GetFileName(s);
                        string key = f.Substring(0, f.IndexOf("-"));
                        //Console.WriteLine(key);

                        foreach(Animation a in Nodes)
                        {
                            if (a.Text.Contains(key))
                            {
                                Console.WriteLine("Matched " + key);
                                Animation an = new Animation("");
                                SMD.read(s, an, null);
                                
                                a.ReplaceMe(an);
                                break;
                            }
                        }
                    }

                }
            }
        }

        public void Remove(object sender, EventArgs args)
        {
            if(Parent != null)
            {
                Nodes.Clear();
                Parent.Nodes.Remove(this);
            }
        }

        public void Create(object sender, EventArgs args)
        {
            Nodes.Add(new Animation("NewSkeletalAnimation"));
        }

        public void CreateMat(object sender, EventArgs args)
        {
            Nodes.Add(new MTA() { Text = "NewMaterialAnimation" });
        }

        public void Import(object sender, EventArgs args)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.Filter = "Supported Formats|*.omo;*.anim;*.chr0;*.smd;*.mta;|" +
                             "Object Motion|*.omo|" +
                             "Maya Animation|*.anim|" +
                             "NW4R Animation|*.chr0|" +
                             "Source Animation (SMD)|*.smd|" +
                             "Smash 4 Material Animation (MTA)|*.mta|" +
                             "All files(*.*)|*.*";
                if(fd.ShowDialog() == DialogResult.OK)
                {
                    foreach(string filename in fd.FileNames)
                    {
                        if (filename.EndsWith(".mta"))
                        {
                            MTA mta = new MTA();
                            try
                            {
                                mta.Read(filename);
                                Runtime.MaterialAnimations.Add(filename, mta);
                                Nodes.Add(filename);
                            }
                            catch (Exception)
                            {
                                mta = null;
                            }
                        }
                        else if (filename.EndsWith(".smd"))
                        {
                            var anim = new Animation(filename);
                            if (Runtime.TargetVBN == null)
                                Runtime.TargetVBN = new VBN();
                            SMD.read(filename, anim, Runtime.TargetVBN);
                            Nodes.Add(anim);
                        }
                        if (filename.EndsWith(".omo"))
                        {
                            Animation a = OMOOld.read(new FileData(filename));
                            a.Text = filename;
                            Nodes.Add(a);
                        }
                        if (filename.EndsWith(".chr0"))
                            Nodes.Add(CHR0.read(new FileData(filename), Runtime.TargetVBN));
                        if (filename.EndsWith(".anim"))
                            Nodes.Add(ANIM.read(filename, Runtime.TargetVBN));
                    }
                }
            }
        }

        public static String FileName;
        public static TreeNode Node;

        public void Save(object sender, EventArgs args)
        {
            //BackgroundWorker worker = sender as BackgroundWorker;

            float f = 1;
            if (FileName.ToLower().EndsWith(".bch"))
            {
                List<Animation> anims = new List<Animation>();
                foreach (Animation a in ((TreeNode)Node).Nodes)
                {
                    //worker.ReportProgress((int)((f / Node.Nodes.Count) * 100f));
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
                    //worker.ReportProgress((int)((f / Node.Nodes.Count) * 100f));
                    f++;
                    //Console.WriteLine("Working on " + anim.Text + " " + (anim.Tag is FileData));
                    var bytes = new byte[1];
                    if (anim.Tag != null && anim.Tag is FileData)
                        bytes = ((FileData)anim.Tag).getSection(0, ((FileData)anim.Tag).size());
                    else
                        bytes = OMOOld.CreateOMOFromAnimation(anim, Runtime.TargetVBN);

                    pac.Files.Add((UseGroupName ? Text : "") + (anim.Text.EndsWith(".omo") ? anim.Text : anim.Text + ".omo"), bytes);
                }
                pac.Save(FileName);
            }
        }
        
        public void Resave(object sender, EventArgs args)
        {
            TreeNode n = this;

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
                        if (sf.FileName.ToLower().EndsWith(".bch"))
                            MessageBox.Show("Note: Only animations with linear interpolation are currently supported.");

                        Node = n;
                        FileName = sf.FileName;

                        Save(null, null);

                        /*MainForm.Instance.Progress = new ProgessAlert();
                        MainForm.Instance.Progress.ProgressValue = 0;
                        MainForm.Instance.Progress.Message = ("Please Wait... Baking Animation Frames");
                        MainForm.Instance.Progress.ControlBox = true;

                        DoWorkEventHandler hand = new DoWorkEventHandler(Save);
                        MainForm.Instance.backgroundWorker1.DoWork += hand;
                        MainForm.Instance.backgroundWorker1.RunWorkerAsync();

                        MainForm.Instance.Progress.ShowDialog();

                        MainForm.Instance.backgroundWorker1.DoWork -= hand;*/

                    }
                }
            }
        }

    }
}
