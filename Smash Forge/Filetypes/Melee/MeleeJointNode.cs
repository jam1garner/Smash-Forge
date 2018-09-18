using MeleeLib.DAT;
using System;

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
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            if(JOBJ.Path != null)
            {
                Nodes.Add(new MeleeJointPath(JOBJ.Path));
            }
        }
    }

    public class MeleeJointPath : MeleeJointAnimationNode
    {
        public DatPath Path;

        public MeleeJointPath(DatPath jobj)
        {
            ImageKey = "anim";
            SelectedImageKey = "anim";
            Path = jobj;
        }

        public override Animation GetAnimation()
        {
            Animation a = new Animation("PathAnimation");
            a.FrameCount = (int)Path.Duration;

            string name = ((MeleeJointNode)Parent).RenderBone.Parent.Text;

            Animation.KeyNode kn = new Animation.KeyNode(name);
            a.Bones.Add(kn);
            kn.XPOS = new Animation.KeyGroup();
            kn.YPOS = new Animation.KeyGroup();
            kn.ZPOS = new Animation.KeyGroup();

            DatPathPoint delta = new DatPathPoint();
            if (Path.PathPoints.Count > 0)
                delta = Path.PathPoints[0];

            for (int i = 0; i < Path.PathPoints.Count; i++)
            {
                {
                    Animation.KeyFrame kf = new Animation.KeyFrame();
                    kf.Frame = Path.PathPoints[i].Time * Path.Duration;
                    kf.Value = delta.X - Path.PathPoints[i].X;
                    kf.InterType = Animation.InterpolationType.LINEAR;
                    kn.XPOS.Keys.Add(kf);
                }
                {
                    Animation.KeyFrame kf = new Animation.KeyFrame();
                    kf.Frame = Path.PathPoints[i].Time * Path.Duration;
                    kf.Value = delta.Y - Path.PathPoints[i].Y;
                    kf.InterType = Animation.InterpolationType.LINEAR;
                    kn.YPOS.Keys.Add(kf);
                }
                {
                    Animation.KeyFrame kf = new Animation.KeyFrame();
                    kf.Frame = Path.PathPoints[i].Time * Path.Duration;
                    kf.Value = delta.Z - Path.PathPoints[i].Z;
                    kf.InterType = Animation.InterpolationType.LINEAR;
                    kn.ZPOS.Keys.Add(kf);
                }
            }

            return a;
        }
    }
}
