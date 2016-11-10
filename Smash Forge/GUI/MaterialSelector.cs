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
            if (Directory.Exists("materials\\"))
            {
                foreach (string folder in Directory.EnumerateDirectories(Path.GetFullPath("materials\\")))
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
        }

        private void openButton(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null && ((string)treeView1.SelectedNode.Tag).EndsWith(".nmt"))
            {
                path = ((string)treeView1.SelectedNode.Tag);
                exitStatus = Opened;
                Close();
            }

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
    }
}
