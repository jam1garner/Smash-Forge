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

namespace Smash_Forge
{
    public partial class MaterialSelector : Form
    {
        public MaterialSelector()
        {
            InitializeComponent();
        }

        public static int Running = 0;
        public static int Opened = 1;
        public static int Cancelled = 2;

        public string path = null;

        public int exitStatus = 0; //0 - not done, 1 - one is selected, 2 - cancelled

        public void populate()
        {
            if (Directory.Exists(MainForm.executableDir + "\\materials"))
            {
                Console.WriteLine(Path.GetFullPath(MainForm.executableDir + "\\materials"));
                foreach (string folder in Directory.EnumerateDirectories(Path.GetFullPath(MainForm.executableDir + "\\materials")))
                {
                    DirectoryInfo dir = new DirectoryInfo(folder);
                    TreeNode folderNode = new TreeNode(dir.Name) { Tag = "folder" };
                    foreach (string file in Directory.EnumerateFiles(folder))
                    {
                        if(Path.GetExtension(file) == ".nmt")
                        {
                            string filename = Path.GetFileNameWithoutExtension(file);
                            folderNode.Nodes.Add(new TreeNode(filename) { Tag = file });
                        }
                    }
                    treeView1.Nodes.Add(folderNode);
                }
            }
            treeView1.Refresh();
        }

        private void openButton()
        {
            if (treeView1.SelectedNode != null && ((string)treeView1.SelectedNode.Tag).EndsWith(".nmt"))
            {
                path = ((string)treeView1.SelectedNode.Tag);
                exitStatus = Opened;
                Close();
            }
        }

        private void openButton(object sender, EventArgs e)
        {
            openButton();
        }

        private void closeButton(object sender, EventArgs e)
        {
            exitStatus = Cancelled;
            Close();
        }

        private void MaterialSelector_Load(object sender, EventArgs e)
        {
            populate();
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                openButton();
                e.Handled = true;
            }
        }
    }
}
