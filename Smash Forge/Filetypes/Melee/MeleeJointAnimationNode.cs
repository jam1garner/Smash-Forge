using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MeleeLib.DAT;
using MeleeLib.DAT.Animation;
using MeleeLib.DAT.Helpers;

namespace Smash_Forge
{
    public class MeleeJointAnimationNode : MeleeNode
    {
        public DatAnimation DatAnimation;

        public MeleeJointAnimationNode(DatAnimation Animation)
        {
            ImageKey = "anim";
            SelectedImageKey = "anim";

            this.DatAnimation = Animation;

            Text = DatAnimation.Text;

            ContextMenu = new ContextMenu();


            MenuItem ImportM = new MenuItem("Import");
            ImportM.Click += Import;
            ContextMenu.MenuItems.Add(ImportM);

            MenuItem SaveAsM = new MenuItem("Save As");
            SaveAsM.Click += SaveAs;
            ContextMenu.MenuItems.Add(SaveAsM);
        }

        public void Import(object sender, EventArgs args)
        {
            Animation a = new Animation(Text);
            a.ReplaceAnimation(sender, args);
            VBN bonetree = Runtime.TargetVBN;
        }

        public void SaveAs(object sender, EventArgs args)
        {
            //lol haxs
            GetAnimation().SaveAs(sender, args);
        }

        public Animation GetAnimation()
        {
            Animation a = new Smash_Forge.Animation(Text);

            a.FrameCount = (int)DatAnimation.FrameCount;

            int bid = 0;
            foreach(DatAnimationNode bone in DatAnimation.Nodes)
            {
                Animation.KeyNode node = new Animation.KeyNode("Bone_" + bid++);
                a.Bones.Add(node);
                node.RotType = Animation.RotationType.EULER;

                AnimationHelperTrack[] helper;
                try
                {
                    helper = AnimationKeyFrameHelper.DecodeKeyFrames(bone);
                }
                catch (Exception)
                {
                    helper = new AnimationHelperTrack[0];
                }

                foreach (AnimationHelperTrack track in helper)
                {
                    float prevValue = 0;
                    float prevTan = 0;
                    Animation.KeyFrame prevkey = null;
                    Animation.KeyGroup Group = new Animation.KeyGroup();
                    /*for(int i =0; i < DatAnimation.FrameCount; i++)
                    {
                        Animation.KeyFrame f = new Animation.KeyFrame();
                        f.Frame = i;
                        f.Value = track.GetValueAt(i);
                        f.InterType = Animation.InterpolationType.LINEAR;
                        Group.Keys.Add(f);
                    }*/
                    
                    foreach(AnimationHelperKeyFrame key in track.KeyFrames)
                    {
                        Animation.KeyFrame f = new Animation.KeyFrame();
                        f.Frame = key.Frame;
                        f.Value = key.Value;
                        f.In = key.Tan;
                        switch (key.InterpolationType)
                        {
                            case InterpolationType.Constant: f.InterType = Animation.InterpolationType.CONSTANT; break;
                            case InterpolationType.Hermite: f.InterType = Animation.InterpolationType.HERMITE; break;
                            case InterpolationType.Linear: f.InterType = Animation.InterpolationType.LINEAR; break;
                            case InterpolationType.Step: f.InterType = Animation.InterpolationType.STEP; break;

                            case InterpolationType.HermiteValue:
                                f.InterType = Animation.InterpolationType.HERMITE;
                                f.In = prevTan;
                                break;
                            case InterpolationType.HermiteCurve:
                                prevkey.Out = key.Tan;
                                continue;
                        }
                        prevValue = key.Value;
                        prevTan = key.Tan;
                        prevkey = f;
                        Group.Keys.Add(f);
                    }

                    switch (track.TrackType)
                    {
                        case AnimTrackType.XPOS: node.XPOS = Group; break;
                        case AnimTrackType.YPOS: node.YPOS = Group; break;
                        case AnimTrackType.ZPOS: node.ZPOS = Group; break;
                        case AnimTrackType.XROT: node.XROT = Group; break;
                        case AnimTrackType.YROT: node.YROT = Group; break;
                        case AnimTrackType.ZROT: node.ZROT = Group; break;
                        case AnimTrackType.XSCA: node.XSCA = Group; break;
                        case AnimTrackType.YSCA: node.YSCA = Group; break;
                        case AnimTrackType.ZSCA: node.ZSCA = Group; break;
                    }
                }
            }

            return a;
        }
    }
}
