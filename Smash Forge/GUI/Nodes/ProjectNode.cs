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
            var result = MessageBox.Show($"Are you sure you want to delete {this.Text}? This cannot be undone!", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                if (this.Tag is DirectoryInfo)
                {
                    string name = ((DirectoryInfo)this.Tag).FullName;
                    Directory.Delete(name, true);
                    ProjectNode.Project.RemoveFile(name);
                }
                else if (this is ProjectNode)
                {
                    string name = ((FileInfo)this.Tag).FullName;
                    File.Delete(name);
                    var n = this as ProjectNode;
                    MainForm.Instance.Workspace.RemoveProject(n.ProjectName);
                }
                else if (this.Tag is FileInfo)
                {
                    string name = ((FileInfo)this.Tag).FullName;
                    File.Delete(name);
                    ProjectNode.Project.RemoveFile(name);
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
                node = this.Parent;
                while (node != null)
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
            this.ImageIndex = this.SelectedImageIndex = 0;
        }
    }
    public class ProjectFileNode : ProjectExplorerNode
    {
        public ProjectFileNode()
        {
            this.ImageIndex = this.SelectedImageIndex = 1;
        }
    }

    // Inherit from folder as the proj file is treated as one
    public class ProjectNode : ProjectFolderNode
    {
        //public static ContextMenuStrip _menu;
        static ProjectNode()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add("Delete Project", null, DeleteAction);
        }
        public ProjectNode(Project p)
        {
            this.Project = p;
            this.Text = p.ProjName;
            this.ContextMenuStrip = _menu;
            this.ImageIndex = this.SelectedImageIndex = 3;
        }
        public static void DeleteAction(object sender, EventArgs e)
        {
            GetInstance<ProjectNode>().DeleteFileOrFolder();
        }
        protected static T GetInstance<T>() where T : TreeNode
        {
            return MainForm.Instance.project.treeView1.SelectedNode as T;
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
