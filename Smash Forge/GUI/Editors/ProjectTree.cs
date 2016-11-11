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

        public void fillTree()
        {
            if (!Directory.Exists("workspace/animcmd/"))
                Directory.CreateDirectory("workspace/animcmd/");

            treeView1.Nodes.Clear();
            List<TreeNode> acmdFiles = new List<TreeNode>();
            foreach (string f in Directory.GetFiles("workspace/animcmd/"))
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
            if (Directory.Exists("/workspace/"))
                Directory.Delete("workspace/");
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = $"\"{filename}\" -o \"{Application.StartupPath}/workspace\"";
            start.FileName = $"\"{Application.StartupPath}/lib/FITD.exe\"";
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
            if (Directory.Exists("/workspace/"))
                Directory.Delete("workspace/");
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = $"-m \"{motionPath}\"-o \"{Application.StartupPath}/workspace\" \" {filename}\"";
            start.FileName = $"{Application.StartupPath}/lib/FITD.exe";
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            int exit;
            using (Process proc = Process.Start(start))
            {
                proc.WaitForExit();
                exit = proc.ExitCode;
            }
            fillTree();
        }

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

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            foreach (ACMDEditor a in MainForm.Instance.ACMDEditors)
            {
                if (a.fname.Equals(((FileInfo)e.Node.Tag).FullName))
                {
                    a.Focus();
                    return;
                }
            }

            if (e.Node.Level >= 1)
            {
                if (e.Node.Text.EndsWith(".acm"))
                {
                    ACMDEditor temp = new ACMDEditor(((FileInfo)e.Node.Tag).FullName, this);
                    MainForm.Instance.ACMDEditors.Add(temp);
                    MainForm.Instance.AddDockedControl(temp);
                }
            }
        }
    }
}
