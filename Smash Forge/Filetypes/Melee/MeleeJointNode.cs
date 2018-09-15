using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MeleeLib.DAT;

namespace Smash_Forge
{
    public class MeleeJointNode : MeleeNode
    {
        private DatJOBJ JOBJ;
        public Bone RenderBone;

        public MeleeJointNode(DatJOBJ jobj)
        {
            ImageKey = "bone";
            SelectedImageKey = "bone";
            this.JOBJ = jobj;
        }
    }
}
