using MeleeLib.DAT;

namespace Smash_Forge.Filetypes.Melee
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
