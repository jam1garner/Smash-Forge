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

                TreeNode node = this;
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
        static ProjectFolderNode()
        {
            _menu.Items.Add(new ToolStripMenuItem("Add", null,
                                                 new ToolStripMenuItem("New Item", null, NewFileAction),
                                                 new ToolStripMenuItem("New Folder", null, AddFolderAction),
                                                 new ToolStripMenuItem("Existing Item", null, ImportFileAction))

                           );
        }
        private static void NewFileAction(object sender, EventArgs e)
        {
            GetInstance<ProjectFolderNode>().NewFile();
        }
        private static void ImportFileAction(object sender, EventArgs e)
        {
            GetInstance<ProjectFolderNode>().ImportFile();
        }
        private static void AddFolderAction(object sender, EventArgs e)
        {
            GetInstance<ProjectFolderNode>().AddFolder();
        }

        public void NewFile()
        {
            throw new NotImplementedException();
        }
        public void AddFolder()
        {

            int i = 0;
            foreach (TreeNode n in this.Nodes)
            {
                if (n.Text == $"NewFolder{i}")
                {
                    i++;
                }
                else break;
            }

            string path = "";
            if (this is ProjectNode)
                path = Path.Combine(Path.GetDirectoryName((((FileInfo)this.Tag).FullName)), $"NewFolder{i}");
            else
                path = Path.Combine((((DirectoryInfo)this.Tag).FullName), $"NewFolder{i}");

            ProjectNode.Project.AddFolder(path);
            Directory.CreateDirectory(path);
            var node = new ProjectFolderNode()
            {
                Tag = new DirectoryInfo(path)
            };
            node.Text = $"NewFolder{i}";
            Nodes.Add(node);
            node.EnsureVisible();
            node.BeginEdit();
        }

        public void ImportFile()
        {
            using (var ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ProjectNode.Project.AddFile(ofd.FileName);
                    Nodes.Add(new ProjectFileNode() { Tag = new FileInfo(ofd.FileName) });
                }
            }
        }

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
        public ProjectNode(Project p)
        {
            this.Project = p;
            this.Text = p.ProjName;
            this.ContextMenuStrip = _menu;
            this.ImageIndex = this.SelectedImageIndex = 3;
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