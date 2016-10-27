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

namespace VBN_Editor
{
    public partial class ProjectTree : DockContent
    {
        public ProjectTree()
        {
            InitializeComponent();
        }

        private string acmdDirectory;

        private void fillTree()
        {
            List<TreeNode> acmdFiles = new List<TreeNode>();
            foreach (string f in Directory.GetFiles("workspace/animcmd/"))
            {
                acmdFiles.Add(new TreeNode(Path.GetFileName(f)));
            }
            treeView1.Nodes.Add(new TreeNode("ACMD", acmdFiles.ToArray()));
        }

        public void openACMD(string file)
        {
            string filename = Path.GetFullPath(file);
            acmdDirectory = Path.GetDirectoryName(filename);
            if(Directory.Exists("/workspace/"))
                Directory.Delete("workspace/");
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = "-o workspace \"" + filename + "\"";
            start.FileName = "FITD.exe";
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

        public void openACMD(string filename, string motionPath) {
            acmdDirectory = Path.GetDirectoryName(filename);
            if (Directory.Exists("/workspace/"))
                Directory.Delete("workspace/");
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = "-m \""+motionPath+"\"-o workspace \""+filename+"\"";
            start.FileName = "FITD.exe";
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
            start.Arguments = "-o \"" + acmdDirectory+"\" workspace/fighter.mlist";
            start.FileName = "FITC.exe";
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
            foreach(ACMDEditor a in MainForm.Instance.ACMDEditors)
            {
                if (a.fname.Equals("workspace/animcmd/" + e.Node.Text))
                {
                    a.Focus();
                    return;
                }
            }

            if(e.Node.Level == 1)
            {
                ACMDEditor temp = new ACMDEditor("workspace/animcmd/" + e.Node.Text, this);
                MainForm.Instance.ACMDEditors.Add(temp);
                MainForm.Instance.AddDockedControl(temp);
            }
        }
    }
}
