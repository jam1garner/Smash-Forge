using MeleeLib.DAT.MatAnim;

namespace SmashForge.Filetypes.Melee
{
    public class MeleeMaterialAnimationNode : MeleeNode
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
