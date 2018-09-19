using MeleeLib.DAT;
using System;

namespace Smash_Forge.Filetypes.Melee
{
    public class MeleeJointNode : MeleeNode
    {
        public DatJOBJ JOBJ;
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

        // for rendering
        public string ParentBone = "";

        public MeleeJointPath(DatPath jobj)
        {
            ImageKey = "anim";
            SelectedImageKey = "anim";
            Path = jobj;
        }

        public MeleeJointPath(DatPath jobj, string ParentBone)
        {
            ImageKey = "anim";
            SelectedImageKey = "anim";
            Path = jobj;
            this.ParentBone = ParentBone;
        }

        public override Animation GetAnimation()
        {
            return GetAnimation(null);
        }

        public Animation GetAnimation(DatJOBJ Base = null)
        {
            Animation a = new Animation("PathAnimation");
            a.FrameCount = (int)Path.Duration;

            string name = ParentBone;// ((MeleeJointNode)Parent).RenderBone.Parent.Text;

            DatJOBJ ParentJOBJ = Base;

            if (!ParentBone.Equals(""))
                name = ParentBone;
            else
            {
                ParentJOBJ = ((MeleeJointNode)Parent).JOBJ;
                name = ((MeleeJointNode)Parent).RenderBone.Text;
            }
            
            Animation.KeyNode kn = new Animation.KeyNode(name);
            a.Bones.Add(kn);
            kn.XPOS = new Animation.KeyGroup();
            kn.YPOS = new Animation.KeyGroup();
            kn.ZPOS = new Animation.KeyGroup();

            for (int i = 0; i < Path.PathPoints.Count; i++)
            {
                {
                    Animation.KeyFrame kf = new Animation.KeyFrame();
                    kf.Frame = Path.PathPoints[i].Time * Path.Duration;
                    kf.Value = Path.PathPoints[i].X + (ParentJOBJ != null ? ParentJOBJ.TX : 0) * -1;
                    kf.InterType = Animation.InterpolationType.LINEAR;
                    kn.XPOS.Keys.Add(kf);
                }
                {
                    Animation.KeyFrame kf = new Animation.KeyFrame();
                    kf.Frame = Path.PathPoints[i].Time * Path.Duration;
                    kf.Value = (ParentJOBJ != null ? ParentJOBJ.TY : 0) * -1 + Path.PathPoints[i].Y;
                    kf.InterType = Animation.InterpolationType.LINEAR;
                    kn.YPOS.Keys.Add(kf);
                }
                {
                    Animation.KeyFrame kf = new Animation.KeyFrame();
                    kf.Frame = Path.PathPoints[i].Time * Path.Duration;
                    kf.Value = (ParentJOBJ != null ? ParentJOBJ.TZ : 0) + Path.PathPoints[i].Z;
                    kf.InterType = Animation.InterpolationType.LINEAR;
                    kn.ZPOS.Keys.Add(kf);
                }
            }

            return a;
        }
    }
}
