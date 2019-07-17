using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MeleeLib.DAT;
using MeleeLib.IO;
using MeleeLib.DAT.Animation;
using MeleeLib.DAT.Helpers;

namespace SmashForge.Filetypes.Melee
{
    public class MeleeJointAnimationNode : MeleeNode
    {
        public DatAnimation DatAnimation;

        public MeleeJointAnimationNode()
        {

        }

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

            MenuItem ExportM = new MenuItem("Export As DAT");
            ExportM.Click += Export;
            ContextMenu.MenuItems.Add(ExportM);
        }

        public void Import(object sender, EventArgs args)
        {
            VBN bonetree = Runtime.TargetVbn;

            if(bonetree == null)
            {
                MessageBox.Show("Select the model root first");
                return;
            }
            Animation a = new Animation(Text);
            a.ReplaceAnimation(sender, args);

            List<Bone> bones = bonetree.getBoneTreeOrder();

            DatAnimation.FrameCount = a.frameCount;
            DatAnimation.Nodes.Clear();
            foreach (Bone b in bones)
            {
                DatAnimationNode node = new DatAnimationNode();
                string BoneName = b.Text;
                foreach (Animation.KeyNode n in a.bones)
                {
                    if (n.Text.Equals(BoneName))
                    {
                        List<AnimationHelperTrack> tracks = new List<AnimationHelperTrack>();

                        if (n.xpos.keys.Count > 0) tracks.Add(EncodeTrack(n.xpos, AnimTrackType.XPOS));
                        if (n.ypos.keys.Count > 0) tracks.Add(EncodeTrack(n.ypos, AnimTrackType.YPOS));
                        if (n.zpos.keys.Count > 0) tracks.Add(EncodeTrack(n.zpos, AnimTrackType.ZPOS));
                        if (n.xrot.keys.Count > 0) tracks.Add(EncodeTrack(n.xrot, AnimTrackType.XROT));
                        if (n.yrot.keys.Count > 0) tracks.Add(EncodeTrack(n.yrot, AnimTrackType.YROT));
                        if (n.zrot.keys.Count > 0) tracks.Add(EncodeTrack(n.zrot, AnimTrackType.ZROT));
                        if (n.xsca.keys.Count > 0) tracks.Add(EncodeTrack(n.xsca, AnimTrackType.XSCA));
                        if (n.ysca.keys.Count > 0) tracks.Add(EncodeTrack(n.ysca, AnimTrackType.YSCA));
                        if (n.zsca.keys.Count > 0) tracks.Add(EncodeTrack(n.zsca, AnimTrackType.ZSCA));

                        node = (AnimationKeyFrameHelper.EncodeKeyFrames(tracks.ToArray(), (int)a.frameCount));
                        break;
                    }
                }
                DatAnimation.Nodes.Add(node);
            }
        }

        public AnimationHelperTrack EncodeTrack(Animation.KeyGroup Group, AnimTrackType Type)
        {
            AnimationHelperTrack t = new AnimationHelperTrack();
            t.TrackType = Type;
            foreach (Animation.KeyFrame kf in Group.keys)
            {
                AnimationHelperKeyFrame f = new AnimationHelperKeyFrame();
                f.Frame = (int)kf.Frame;
                f.Value = kf.Value;
                f.Tan = kf.In;
                t.KeyFrames.Add(f);
                switch (kf.interType)
                {
                    case Animation.InterpolationType.Constant: f.InterpolationType = InterpolationType.Constant; break;
                    case Animation.InterpolationType.Hermite: f.InterpolationType = InterpolationType.Hermite; break;
                    case Animation.InterpolationType.Linear:
                        if (Group.keys.Count == 1)
                            f.InterpolationType = InterpolationType.Constant;
                        else
                            f.InterpolationType = InterpolationType.Linear; break;
                    case Animation.InterpolationType.Step: f.InterpolationType = InterpolationType.Step; break;
                }
            }
            return t;
        }

        public void SaveAs(object sender, EventArgs args)
        {
            //lol haxs
            GetAnimation().SaveAs(sender, args);
        }

        public void Export(object sender, EventArgs args)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter += "HAL DAT|*.dat";

                sfd.DefaultExt = "dat";

                if(sfd.ShowDialog() == DialogResult.OK)
                {
                    Compiler.Compile(GetAsDATFile(), sfd.FileName);
                }
            }
        }

        public DATFile GetAsDATFile()
        {
            DATFile d = new DATFile();
            DATRoot root = new DATRoot();
            root.Text = Text;
            root.Animations.Add(DatAnimation);
            d.AddRoot(root);

            return d;
        }

        public virtual Animation GetAnimation()
        {
            Animation a = new Animation(Text);

            a.frameCount = (int)DatAnimation.FrameCount;

            int bid = 0;
            foreach(DatAnimationNode bone in DatAnimation.Nodes)
            {
                Animation.KeyNode node = new Animation.KeyNode("Bone_" + bid++);
                //Console.WriteLine(node.Text + " " + bone.Tracks.Count);
                a.bones.Add(node);
                node.rotType = Animation.RotationType.Euler;

                AnimationHelperTrack[] helper;
                try
                {
                    helper = AnimationKeyFrameHelper.DecodeKeyFrames(bone);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error Loading animation");
                    helper = new AnimationHelperTrack[0];
                }

                foreach (AnimationHelperTrack track in helper)
                {
                    float prevValue = 0;
                    float prevTan = 0;
                    Animation.KeyFrame prevkey = null;
                    Animation.KeyGroup Group = new Animation.KeyGroup();
                    
                    foreach(AnimationHelperKeyFrame key in track.KeyFrames)
                    {
                        Animation.KeyFrame f = new Animation.KeyFrame();
                        f.Frame = key.Frame;
                        f.Value = key.Value;
                        f.In = key.Tan;
                        switch (key.InterpolationType)
                        {
                            case InterpolationType.Constant: f.interType = Animation.InterpolationType.Constant; break;
                            case InterpolationType.Hermite: f.interType = Animation.InterpolationType.Hermite; break;
                            case InterpolationType.Linear: f.interType = Animation.InterpolationType.Linear; break;
                            case InterpolationType.Step: f.interType = Animation.InterpolationType.Step; break;

                            case InterpolationType.HermiteValue:
                                f.interType = Animation.InterpolationType.Hermite;
                                f.In = prevTan;
                                break;
                            case InterpolationType.HermiteCurve:
                                prevkey.Out = key.Tan;
                                continue;
                        }
                        prevValue = key.Value;
                        prevTan = key.Tan;
                        prevkey = f;
                        Group.keys.Add(f);
                    }

                    switch (track.TrackType)
                    {
                        case AnimTrackType.XPOS: node.xpos = Group; break;
                        case AnimTrackType.YPOS: node.ypos = Group; break;
                        case AnimTrackType.ZPOS: node.zpos = Group; break;
                        case AnimTrackType.XROT: node.xrot = Group; break;
                        case AnimTrackType.YROT: node.yrot = Group; break;
                        case AnimTrackType.ZROT: node.zrot = Group; break;
                        case AnimTrackType.XSCA: node.xsca = Group; break;
                        case AnimTrackType.YSCA: node.ysca = Group; break;
                        case AnimTrackType.ZSCA: node.zsca = Group; break;
                    }
                }
            }

            return a;
        }
    }
}
