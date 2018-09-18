using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MeleeLib.DAT;

namespace Smash_Forge.Filetypes.Melee
{
    public class MeleeNode : TreeNode
    {
        public MeleeDataNode GetDatFile()
        {
            TreeNode P = Parent;
            while (P != null)
            {
                if (P is MeleeDataNode)
                    return (MeleeDataNode)P;
                P = P.Parent;
            }
            return null;
        }

        public MeleeRootNode GetRoot()
        {
            TreeNode P = Parent;
            while (P != null)
            {
                if (P is MeleeRootNode)
                    return (MeleeRootNode)P;
                P = P.Parent;
            }
            return null;
        }


        public List<MeleeRootNode> GetAllRoots()
        {
            List<MeleeRootNode> Nodes = new List<MeleeRootNode>(5);
            GetAllRoots(Nodes);
            return Nodes;
        }

        private void GetAllRoots(List<MeleeRootNode> Nodes)
        {
            if (this is MeleeRootNode)
                Nodes.Add((MeleeRootNode)this);

            foreach (TreeNode n in this.Nodes)
            {
                if(n is MeleeNode)
                    ((MeleeNode)n).GetAllRoots(Nodes);
            }
        }
    }
}
