using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALT.Scripting.AnimCMD;
using SALT.Scripting.MSC;
using SALT.PARAMS;
using System.Windows.Forms;

namespace Smash_Forge.GUI.Nodes
{
    class MovesetNode : BaseNode
    {
        public MTable MotionTable { get; set; }
        public ACMDFile Game { get; set; }
        public ACMDFile GFX { get; set; }
        public ACMDFile SFX { get; set; }
        public ACMDFile Expression { get; set; }

    }
    public class ACMDMoveDefNode : BaseNode
    {
        public MoveDef Script { get; set; }
    }
    public class ACMDScriptNode : BaseNode
    {

    }
}
