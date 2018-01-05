using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Smash_Forge
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

        public BaseNode(string FilePath) : this()
        {
            FileInfo = new FileInfo(FilePath);
            Text = FileInfo.Name;
        }

        public virtual void DoubleClicked(object sender, TreeNodeMouseClickEventArgs e)
        {
            
        }
    }

    public class NUDNode : BaseNode
    {
        public NUDNode()
        {
            ImageKey = "nud";
            SelectedImageKey = "nud";
            Openable = true;

            ContextMenu = new ContextMenu();
            
        }

        public NUDNode(string FilePath) : this()
        {
            FileInfo = new FileInfo(FilePath);
            Text = FileInfo.Name;
        }
    }

    public class NUTNode : BaseNode
    {
        public NUTNode()
        {
            ImageKey = "nut";
            SelectedImageKey = "nut";
            Openable = true;

            ContextMenu = new ContextMenu();
            
        }

        public NUTNode(string FilePath) : this()
        {
            FileInfo = new FileInfo(FilePath);
            Text = FileInfo.Name;
        }
    }

    public class VBNNode : BaseNode
    {
        public VBNNode()
        {
            ImageKey = "vbn";
            SelectedImageKey = "vbn";
            Openable = true;

            ContextMenu = new ContextMenu();
            
        }

        public VBNNode(string FilePath) : this()
        {
            FileInfo = new FileInfo(FilePath);
            Text = FileInfo.Name;
        }
    }
}