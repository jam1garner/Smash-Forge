using System.Windows.Forms;
using System.IO;

namespace SmashForge
{
    public class BaseNode : TreeNode
    {
        public FileInfo FileInfo { get; set; }
        public bool Openable { get; set; }

        public BaseNode()
        {
            ImageKey = "file";
            SelectedImageKey = "file";
            Openable = false;

            ContextMenu = new ContextMenu();

            MenuItem openContain = new MenuItem("Open containing folder");
            ContextMenu.MenuItems.Add(openContain);
        }

        public BaseNode(string filePath) : this()
        {
            FileInfo = new FileInfo(filePath);
            Text = FileInfo.Name;
        }

        public virtual void DoubleClicked(object sender, TreeNodeMouseClickEventArgs e)
        {
            
        }
    }

    public class NudNode : BaseNode
    {
        public NudNode()
        {
            ImageKey = "nud";
            SelectedImageKey = "nud";
            Openable = true;

            ContextMenu = new ContextMenu();
            
        }

        public NudNode(string filePath) : this()
        {
            FileInfo = new FileInfo(filePath);
            Text = FileInfo.Name;
        }
    }

    public class NutNode : BaseNode
    {
        public NutNode()
        {
            ImageKey = "nut";
            SelectedImageKey = "nut";
            Openable = true;

            ContextMenu = new ContextMenu();
            
        }

        public NutNode(string filePath) : this()
        {
            FileInfo = new FileInfo(filePath);
            Text = FileInfo.Name;
        }
    }

    public class VbnNode : BaseNode
    {
        public VbnNode()
        {
            ImageKey = "vbn";
            SelectedImageKey = "vbn";
            Openable = true;

            ContextMenu = new ContextMenu();
            
        }

        public VbnNode(string filePath) : this()
        {
            FileInfo = new FileInfo(filePath);
            Text = FileInfo.Name;
        }
    }
}