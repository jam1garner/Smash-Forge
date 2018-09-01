using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MeleeLib.DAT;

namespace Smash_Forge
{
    public class MeleeJointNode : TreeNode
    {
        private DatJOBJ JOBJ;

        public MeleeJointNode(DatJOBJ jobj)
        {
            ImageKey = "bone";
            SelectedImageKey = "bone";
            this.JOBJ = jobj;
        }
    }
}
