using MeleeLib.DAT;

namespace SmashForge.Filetypes.Melee
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
            a.frameCount = (int)Path.Duration;

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
            a.bones.Add(kn);
            kn.xpos = new Animation.KeyGroup();
            kn.ypos = new Animation.KeyGroup();
            kn.zpos = new Animation.KeyGroup();

            for (int i = 0; i < Path.PathPoints.Count; i++)
            {
                {
                    Animation.KeyFrame kf = new Animation.KeyFrame();
                    kf.Frame = Path.PathPoints[i].Time * Path.Duration;
                    kf.Value = Path.PathPoints[i].X + (ParentJOBJ != null ? ParentJOBJ.TX : 0) * -1;
                    kf.interType = Animation.InterpolationType.Linear;
                    kn.xpos.keys.Add(kf);
                }
                {
                    Animation.KeyFrame kf = new Animation.KeyFrame();
                    kf.Frame = Path.PathPoints[i].Time * Path.Duration;
                    kf.Value = (ParentJOBJ != null ? ParentJOBJ.TY : 0) * -1 + Path.PathPoints[i].Y;
                    kf.interType = Animation.InterpolationType.Linear;
                    kn.ypos.keys.Add(kf);
                }
                {
                    Animation.KeyFrame kf = new Animation.KeyFrame();
                    kf.Frame = Path.PathPoints[i].Time * Path.Duration;
                    kf.Value = (ParentJOBJ != null ? ParentJOBJ.TZ : 0) + Path.PathPoints[i].Z;
                    kf.interType = Animation.InterpolationType.Linear;
                    kn.zpos.keys.Add(kf);
                }
            }

            return a;
        }
    }
}
