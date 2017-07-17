using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using SALT.Moveset;
using SALT.Moveset.AnimCMD;
using SALT.Moveset.MSC;
using System.Xml;
using SALT.PARAMS;

namespace Smash_Forge
{
    public class WorkspaceManager
    {
        public WorkspaceManager(ProjectTree tree)
        {
            Projects = new SortedList<string, Project>();
            Tree = tree;
        }

        public XmlDocument WorkspaceFile { get; set; }

        public SortedList<string, Project> Projects { get; set; }

        private ProjectTree Tree { get; set; }
        public string WorkspaceRoot { get; set; }
        public string TargetProject { get; set; }
        public string WorkspaceName { get; set; }

        public void OpenWorkspace(string filepath)
        {
            WorkspaceFile = new XmlDocument();
            WorkspaceFile.Load(filepath);

            WorkspaceRoot = Path.GetDirectoryName(filepath);

            var rootNode = WorkspaceFile.SelectSingleNode("//Workspace");

            WorkspaceName = rootNode.Attributes["Name"].Value;
            var nodes = WorkspaceFile.SelectNodes("//Workspace//Project");
            foreach (XmlNode node in nodes)
            {
                var proj = ReadProjectFile(Path.Combine(WorkspaceRoot, node.Attributes["Path"].Value));
                proj.ProjName = node.Attributes["Name"].Value;
                Projects.Add(proj.ProjName, proj);
            }
            PopulateTreeView();
        }

        public void RemoveProject(Project p)
        {
            Projects.Remove(p.ProjName);
            var nodes = WorkspaceFile.SelectNodes("//Workspace//Project");
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["Name"].Value == p.ProjName)
                {
                    WorkspaceFile.SelectSingleNode("//Workspace").RemoveChild(node);
                }
            }
        }
        public void RemoveProject(string name)
        {
            RemoveProject(Projects[name]);
        }

        public void OpenProject(string filename)
        {
            var p = ReadProjectFile(filename);
            Projects.Add(p.ProjName, p);
            PopulateTreeView();
        }
        private Project ReadProjectFile(string filepath)
        {
            var proj = new Project();
            if (filepath.EndsWith(".fitproj", StringComparison.InvariantCultureIgnoreCase))
            {
                proj = new FitProj();
            }
            else if (filepath.EndsWith(".stproj", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new NotImplementedException("Stage projects not yet supported");
            }
            proj.ReadProject(filepath);
            proj.ProjName = Path.GetFileName(filepath);
            return proj;
        }

        public void PopulateTreeView()
        {
            Tree.treeView1.BeginUpdate();
            TreeNode workspaceNode = null;

            // If we're actually opening a full workspace and
            // not just a single project, add all projects
            // as children to the workspace
            if (!string.IsNullOrEmpty(WorkspaceName))
            {
                workspaceNode = new TreeNode(WorkspaceName);
                workspaceNode.ImageIndex = workspaceNode.SelectedImageIndex = 2;
            }

            foreach (var pair in Projects)
            {
                FitProj p = (FitProj)pair.Value;

                FileInfo fileinfo = new FileInfo(p.ProjFilepath);
                var projNode = new ProjectNode(p);
                projNode.Tag = fileinfo;

                GetDirectories(new DirectoryInfo(p.ProjDirectory).GetDirectories(), projNode, p);
                GetFiles(new DirectoryInfo(p.ProjDirectory), projNode, p);

                if (workspaceNode != null)
                    workspaceNode.Nodes.Add(projNode);
                else
                    Tree.treeView1.Nodes.Add(projNode);
            }
            if (workspaceNode != null)
                Tree.treeView1.Nodes.Add(workspaceNode);

            Tree.treeView1.EndUpdate();
        }

        private void GetDirectories(DirectoryInfo[] subDirs, ProjectFolderNode nodeToAddTo, FitProj p)
        {
            ProjectFolderNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new ProjectFolderNode() { Text = subDir.Name };
                aNode.Tag = subDir;
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                    GetDirectories(subSubDirs, aNode, p);
                }
                GetFiles(subDir, aNode, p);
                nodeToAddTo.Nodes.Add(aNode);
            }
        }
        private void GetFiles(DirectoryInfo dir, ProjectFolderNode nodeToAddTo, FitProj p)
        {
            foreach (var fileinfo in dir.GetFiles())
            {
                if (fileinfo.Name.EndsWith(".fitproj", StringComparison.InvariantCultureIgnoreCase))
                    break;

                var child = new ProjectFileNode() { Text = fileinfo.Name };
                child.Tag = fileinfo;
                foreach (var f in p.IncludedFiles)
                {
                    if (fileinfo.FullName.Contains(f.RelativePath))
                    {
                        nodeToAddTo.Nodes.Add(child);
                        break;
                    }
                }
            }
        }
    }

    public class Project
    {
        public Project()
        {
            IncludedFiles = new List<ProjectItem>();
            IncludedFolders = new List<ProjectItem>();
        }

        // Project Properties
        public XmlDocument ProjFile { get; set; }
        public string ProjFilepath { get; set; }
        public string ProjDirectory { get { return Path.GetDirectoryName(ProjFilepath); } }
        public string ProjName { get; set; }
        public string ToolVer { get; set; }
        public string GameVer { get; set; }
        public ProjType Type { get; set; }
        public ProjPlatform Platform { get; set; }
        public List<ProjectItem> IncludedFiles { get; set; }

        // Folders are only included here if empty.
        public List<ProjectItem> IncludedFolders { get; set; }

        public ProjectItem GetFile(string path)
        {
            return this[path];
        }
        public bool RemoveFile(ProjectItem item)
        {
            bool result = IncludedFiles.Remove(item);
            SaveProject();
            return result;
        }
        public bool RemoveFile(string path)
        {
            if (this[path] != null)
                return RemoveFile(this[path]);
            else
                return false;
        }

        public void AddFile(string filepath)
        {
            var item = new ProjectItem();
            item.RealPath = filepath;
            item.RelativePath = filepath.Replace(ProjDirectory, "");
            IncludedFiles.Add(item);
            SaveProject();
        }
        public void AddFolder(string path)
        {
            var item = new ProjectItem();
            item.RealPath = path;
            item.RelativePath = path.Replace(ProjDirectory, "");
            IncludedFolders.Add(item);
            SaveProject();
        }

        public void RenameFile(string filepath, string oldname, string newname)
        {
            var entry = IncludedFiles.FirstOrDefault(x => x.RealPath == filepath || x.RelativePath == filepath);
            entry.RelativePath = entry.RelativePath.Replace(oldname, newname);
            entry.RealPath = entry.RealPath.Replace(oldname, newname);
            if (entry.RealPath.EndsWith(".fitproj"))
            {
                File.Move(entry.RealPath, entry.RealPath.Remove(Runtime.CanonicalizePath(entry.RealPath).LastIndexOf(Path.DirectorySeparatorChar)) + newname + ".fitproj");
            }
            else
            {
                File.Move(filepath,entry.RealPath);
            }
            SaveProject();
        }
        public ProjectItem this[string key]
        {
            get
            {
                return IncludedFiles.FirstOrDefault(x => x.RelativePath == key);
            }
        }

        public virtual void ReadProject(string filepath) { }
        public virtual void SaveProject(string filepath) { }
        public virtual void SaveProject()
        {
            SaveProject(ProjFilepath);
        }
    }

    public class FitProj : Project
    {
        public FitProj()
        {

        }
        public FitProj(string name)
        {
            ProjName = name;
        }
        public FitProj(string name, string filepath) : this(name)
        {
            ReadProject(filepath);
        }

        public override void ReadProject(string filepath)
        {
            ProjFilepath = filepath;
            var proj = new XmlDocument();
            proj.Load(filepath);

            var node = proj.SelectSingleNode("//Project");
            this.ToolVer = node.Attributes["ToolVer"].Value;
            this.GameVer = node.Attributes["GameVer"].Value;

            if (node.Attributes["Platform"].Value == "WiiU")
                this.Platform = ProjPlatform.WiiU;
            else if (node.Attributes["Platform"].Value == "3DS")
                this.Platform = ProjPlatform.ThreeDS;

            var nodes = proj.SelectNodes("//Project/FileGroup");
            foreach (XmlNode n in nodes)
            {
                foreach (XmlNode child in n.ChildNodes)
                {
                    var item = new ProjectItem();
                    item.RelativePath = Runtime.CanonicalizePath(child.Attributes["Include"].Value);
                    item.RealPath = Runtime.CanonicalizePath(Path.Combine(ProjDirectory, item.RelativePath));
                    if (child.HasChildNodes)
                    {
                        foreach (XmlNode child2 in child.ChildNodes)
                        {
                            if (child2.LocalName == "DependsUpon")
                            {
                                var path = Runtime.CanonicalizePath(Path.Combine(Path.GetDirectoryName(item.RelativePath), child2.InnerText));
                                item.Depends.Add(IncludedFiles.Find(x => x.RelativePath == path));
                            }
                        }
                    }
                    if (child.Name == "Folder")
                    {
                        IncludedFolders.Add(item);
                    }
                    else
                    {
                        IncludedFiles.Add(item);
                    }
                }
            }
            ProjFile = proj;
        }
        public override void SaveProject(string filepath)
        {
            var writer = XmlWriter.Create(filepath, new XmlWriterSettings() { Indent = true, IndentChars = "\t" });
            writer.WriteStartDocument();
            writer.WriteStartElement("Project");
            writer.WriteAttributeString("Name", ProjName);
            writer.WriteAttributeString("ToolVer", ToolVer);
            writer.WriteAttributeString("GameVer", GameVer);
            writer.WriteAttributeString("Platform", Enum.GetName(typeof(ProjPlatform), Platform));

            writer.WriteStartElement("FileGroup");
            foreach (var item in IncludedFolders)
            {
                writer.WriteStartElement("Folder");
                writer.WriteAttributeString("Include", item.RelativePath);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("FileGroup");
            foreach (var item in IncludedFiles)
            {
                writer.WriteStartElement("Content");
                writer.WriteAttributeString("Include", item.RelativePath);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            var doc = new XmlDocument();
            doc.Load(filepath);
            ProjFile = doc;
        }
    }
    public class ProjectItem
    {
        public ProjectItem()
        {
            Depends = new List<ProjectItem>();
        }
        public string RelativePath { get; set; }
        public string RealPath { get; set; }
        public bool IsDirectory
        {
            get
            {
                return RelativePath.EndsWith("/") || RelativePath.EndsWith("\\");
            }
        }
        public List<ProjectItem> Depends { get; set; }
        public override string ToString()
        {
            return RelativePath;
        }
    }

    public enum ProjType
    {
        Fighter,
        Stage
    }
    public enum ProjPlatform
    {
        WiiU,
        ThreeDS
    }
}
