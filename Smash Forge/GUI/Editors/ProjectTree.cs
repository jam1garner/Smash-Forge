using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using WeifenLuo.WinFormsUI.Docking;

namespace Smash_Forge
{
    public partial class ProjectTree : DockContent
    {
        public ProjectTree()
        {
            InitializeComponent();
        }

        private string acmdDirectory;

        private Dictionary<TreeNode, ModelContainer> modelLinks = new Dictionary<TreeNode, ModelContainer>();
        private Dictionary<TreeNode, NUT> textureLinks = new Dictionary<TreeNode, NUT>();

        public void PopulateTreeView(string root)
        {
            treeView1.BeginUpdate();

            TreeNode rootNode;
            DirectoryInfo info = new DirectoryInfo(root);
            if (info.Exists)
            {
                rootNode = new TreeNode(info.Name);
                rootNode.Tag = info;
                rootNode.ImageIndex = 0;
                rootNode.SelectedImageIndex = 0;

                GetDirectories(info.GetDirectories(), rootNode);
                GetFiles(info, rootNode);
                treeView1.Nodes.Add(rootNode);
            }
            treeView1.EndUpdate();
        }

        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageIndex = 0;
                aNode.SelectedImageIndex = 0;
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                    GetDirectories(subSubDirs, aNode);
                }
                GetFiles(subDir, aNode);
                nodeToAddTo.Nodes.Add(aNode);
            }
        }
        private void GetFiles(DirectoryInfo dir, TreeNode nodeToAddTo)
        {
            foreach (var fileinfo in dir.GetFiles())
            {
                var child = new TreeNode(fileinfo.Name, 0, 0);
                child.Tag = fileinfo;
                child.ImageIndex = 1;
                child.SelectedImageIndex = 1;
                nodeToAddTo.Nodes.Add(child);
            }
        }
        public void fillTree()
        {
            if (!Directory.Exists(Path.Combine(Application.StartupPath, "workspace/animcmd/")))
                Directory.CreateDirectory(Path.Combine(Application.StartupPath, "workspace/animcmd/"));

            treeView1.Nodes.Clear();
            List<TreeNode> acmdFiles = new List<TreeNode>();
            foreach (string f in Directory.GetFiles(Path.Combine(Application.StartupPath, "workspace/animcmd/")))
            {
                acmdFiles.Add(new TreeNode(Path.GetFileName(f)));
            }
            treeView1.Nodes.Add(new TreeNode("ACMD", acmdFiles.ToArray()));


            /*List<TreeNode> models = new List<TreeNode>();
            modelLinks.Clear();
            foreach (ModelContainer con in Runtime.ModelContainers)
            {
                List<TreeNode> modelCon = new List<TreeNode>();
                if(con.nud != null)
                    modelCon.Add(new TreeNode("Mesh"));
                
                if(con.vbn != null)
                    modelCon.Add(new TreeNode("Bones"));
                
                TreeNode node = new TreeNode(con.name, modelCon.ToArray());
                modelLinks.Add(node, con);
                models.Add(node);
            }
            treeView1.Nodes.Add(new TreeNode("Models", models.ToArray()));
            List<TreeNode> textures = new List<TreeNode>();
            modelLinks.Clear();
            foreach (NUT n in Runtime.TextureContainers)
            {
                TreeNode node = new TreeNode("NUT");
                textureLinks.Add(node, n);
                textures.Add(node);
            }
            treeView1.Nodes.Add(new TreeNode("Textures", textures.ToArray()));*/

        }

        public void openACMD(string file)
        {
            string filename = Path.GetFullPath(file);
            acmdDirectory = Path.GetDirectoryName(filename);
            if (Directory.Exists(Path.Combine(Application.StartupPath, "workspace/")))
                Directory.Delete(Path.Combine(Application.StartupPath, "workspace/"));
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = $"\"{filename}\" -o \"{Path.Combine(Application.StartupPath, "workspace/")}\"";
            start.FileName = $"\"{Path.Combine(Application.StartupPath, "/lib/FITD.exe")}\"";
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            int exit;
            using (var proc = Process.Start(start))
            {
                proc.WaitForExit();
                exit = proc.ExitCode;
            }
            fillTree();
        }

        public void openACMD(string filename, string motionPath)
        {
            acmdDirectory = Path.GetDirectoryName(filename);
            if (Directory.Exists(Path.Combine(Application.StartupPath, "workspace/")))
                Directory.Delete(Path.Combine(Application.StartupPath, "workspace/"));
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = $"-m \"{motionPath}\" -o \"{Path.Combine(Application.StartupPath, "workspace/")}\" \"{filename}\"";
            start.FileName = $"{Path.Combine(Application.StartupPath, "/lib/FITD.exe")}";
            Console.WriteLine(start.FileName + " " + start.Arguments);
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            int exit;
            using (Process proc = Process.Start(start))
            {
                proc.WaitForExit();
                exit = proc.ExitCode;
            }
            fillTree();
        }//"C:\Users\jam1garner\Source\Repos\Smash-Forge\Smash Forge\bin\Debug\lib\FITD.exe" -m "C:\Smash\Sm4shExplorer\extract\data\fighter\captain\motion\" -o "C:\Users\jam1garner\Source\Repos\Smash-Forge\Smash Forge\bin\Debug\workspace" "C:\Smash\Sm4shExplorer\extract\data\fighter\captain\script\animcmd\body\motion.mtable"

        public void build()
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = "-o \"" + acmdDirectory + "\" workspace/fighter.mlist";
            start.FileName = "lib/FITC.exe";
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            int exit;
            using (Process proc = Process.Start(start))
            {
                proc.WaitForExit();
                exit = proc.ExitCode;
            }
        }

        private void openFile(object sender, TreeNodeMouseClickEventArgs e)
        {
            foreach (ACMDEditor a in MainForm.Instance.ACMDEditors)
            {
                if (a.fname.Equals("workspace/animcmd/" + e.Node.Text))
                {
                    a.Focus();
                    return;
                }
            }

            if (e.Node.Level == 1)
            {
                if (e.Node.Parent.Text.Equals("ACMD"))
                {
                    ACMDEditor temp = new ACMDEditor(Path.Combine(Application.StartupPath, "workspace/animcmd/") + e.Node.Text, this);
                    MainForm.Instance.ACMDEditors.Add(temp);
                    MainForm.Instance.AddDockedControl(temp);
                }
            }
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            // this is kind of a hack at the moment. If the previous name for this item
            // is also present elsewhere in the path, it will be replaced as well.
            // this is mostly a problem for nested folders / files of the same names
            if (e.Node.Tag is DirectoryInfo)
            {
                Directory.Move(((DirectoryInfo)e.Node.Tag).FullName, ((DirectoryInfo)e.Node.Tag).FullName.Replace(e.Node.Text, e.Label));
            }
            if (e.Node.Tag is FileInfo)
            {
                ((ProjectExplorerNode)e.Node).ProjectNode.Project.RenameFile(((FileInfo)e.Node.Tag).FullName, e.Node.Text, e.Label);
            }
        }
    }
}