using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge.GUI.Nodes
{
    public class ModelNode : BaseNode
    {
        public override void DoubleClicked(object sender, TreeNodeMouseClickEventArgs e)
        {
            base.DoubleClicked(sender, e);
            ModelContainer m = new ModelContainer();
            m.vbn = Runtime.TargetVBN;
            Runtime.ModelContainers.Add(m);
        }

        public VBNNode VBN { get; set; }
        public NUDNode NUD { get; set; }
        public MTANode MTA { get; set; }
        public NUTNode NUT { get; set; }

    }
    public class VBNNode : BaseNode
    {
        public VBN Skeleton { get; set; }
        public override void DoubleClicked(object sender, TreeNodeMouseClickEventArgs e)
        {
            Runtime.TargetVBN = Skeleton;
            ModelContainer m = new ModelContainer();
            m.vbn = Runtime.TargetVBN;
            Runtime.ModelContainers.Add(m);
        }
    }
    public class NUTNode : BaseNode
    {

    }
    public class MTANode : BaseNode
    {

    }
    public class NUDNode : BaseNode
    {
        public NUD Model { get; set; }
    }
}
