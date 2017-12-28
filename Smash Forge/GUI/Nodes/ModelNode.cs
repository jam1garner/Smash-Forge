using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge
{
    public class ModelNode : BaseNode
    {
        public ModelNode()
        {
            Text = "Model";
            NUD = new NUDNode();
            NUT = new NUTNode();
            MTA = new MTANode();
            VBN = new VBNNode();
            Nodes.Add(NUD);
            Nodes.Add(NUT);
            Nodes.Add(VBN);
            Nodes.Add(MTA);
        }

        public override void DoubleClicked(object sender, TreeNodeMouseClickEventArgs e)
        {
            base.DoubleClicked(sender, e);
            ModelContainer m = new ModelContainer();
            m.VBN = Runtime.TargetVBN;
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
            m.VBN = Runtime.TargetVBN;
            Runtime.ModelContainers.Add(m);
        }
    }
    public class NUTNode : BaseNode
    {
        public NUT Texture { get; set; }
    }
    public class MTANode : BaseNode
    {
        public MTA MTA { get; set; }
    }
    public class NUDNode : BaseNode
    {
        public NUD Model { get; set; }
    }
}