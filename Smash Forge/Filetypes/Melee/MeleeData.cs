using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MeleeLib.DAT;

namespace Smash_Forge
{
    public class MeleeDataNode : TreeNode
    {
        TreeNode Skeleton = new TreeNode("Joints") { SelectedImageKey = "folder", ImageKey = "folder"};
        TreeNode DataObjects = new TreeNode("Data Objects") { SelectedImageKey = "folder", ImageKey = "folder" };
        TreeNode MatAnims = new TreeNode("Material Animations") { SelectedImageKey = "folder", ImageKey = "folder" };
        TreeNode JointAnims = new TreeNode("Joint Animations") { SelectedImageKey = "folder", ImageKey = "folder" };

        public MeleeDataNode()
        {
            Nodes.Add(Skeleton);
            Nodes.Add(DataObjects);
            Nodes.Add(MatAnims);
            Nodes.Add(JointAnims);
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {

        }
    }
}
