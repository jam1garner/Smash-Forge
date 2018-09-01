using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MeleeLib.DAT;
using MeleeLib.DAT.Animation;
using MeleeLib.DAT.MatAnim;

namespace Smash_Forge
{
    public class MeleeMaterialAnimationNode : TreeNode
    {
        DatMatAnim Animation;

        public MeleeMaterialAnimationNode(DatMatAnim Animation)
        {
            ImageKey = "texture";
            SelectedImageKey = "texture";

            this.Animation = Animation;

            Text = "MaterialAnimation";
        }
    }
}
