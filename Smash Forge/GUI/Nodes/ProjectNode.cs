using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge
{
    public class ProjectExplorerNode : TreeNode
    {
        public static ContextMenuStrip _menu;
        static ProjectExplorerNode()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add("Delete", null, DeleteAction);
        }
        public ProjectExplorerNode()
        {
            this.ContextMenuStrip = _menu;
        }

        public virtual void DeleteFileOrFolder()
        {
            var result = MessageBox.Show($"Are you sure you want to delete {this.Text}?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                if (this.Tag is DirectoryInfo)
                {
                    Directory.Delete(((DirectoryInfo)this.Tag).FullName);
                }
                else if (this.Tag is FileInfo)
                {
                    File.Delete(((FileInfo)this.Tag).FullName);
                }
                this.Remove();
            }
        }
        private static void DeleteAction(object sender, EventArgs e)
        {
            GetInstance<ProjectExplorerNode>().DeleteFileOrFolder();
        }

        protected static T GetInstance<T>() where T : TreeNode
        {
            return MainForm.Instance.project.treeView1.SelectedNode as T;
        }
        public ProjectNode ProjectNode
        {
            get
            {
                TreeNode node = null;
                while ((node = this.Parent) != null)
                {
                    if (node is ProjectNode)
                        break;

                    node = node.Parent;
                }
                return (ProjectNode)node;
            }

        }
    }

    public class ProjectFolderNode : ProjectExplorerNode
    {
        public ProjectFolderNode()
        {
            this.ImageKey = "Folder";
        }
    }
    public class ProjectFileNode: ProjectExplorerNode
    {
        public ProjectFileNode()
        {
            this.ImageKey = "File";
        }
    }

    // Inherit from folder as the proj file is treated as one
    public class ProjectNode : ProjectFolderNode
    {
        public ProjectNode(Project p)
        {
            this.Project = p;
            this.Text = p.ProjName;
            this.ImageKey = "Project";
        }

        public Project Project { get; private set; }
        public string ProjectName
        {
            get
            {
                return Project.ProjName;
            }
            set
            {
                Project.ProjName = value;
                this.Text = value;
            }
        }

        public ProjPlatform Platform { get { return Project.Platform; } }
        public ProjType ProjectType { get { return Project.Type; } }
    }
}
