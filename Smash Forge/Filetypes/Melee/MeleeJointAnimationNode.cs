using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MeleeLib.DAT;
using MeleeLib.DAT.Animation;

namespace Smash_Forge
{
    public class MeleeJointAnimationNode : MeleeNode
    {
        DatAnimation Animation;

        public MeleeJointAnimationNode(DatAnimation Animation)
        {
            ImageKey = "animation";
            SelectedImageKey = "animation";

            this.Animation = Animation;

            Text = Animation.Text;
        }
    }
}
