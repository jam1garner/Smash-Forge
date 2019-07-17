using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK;
using Syroot.NintenTools.Bfres;
using ResNSW = Syroot.NintenTools.NSW.Bfres;


namespace SmashForge
{
    public enum TrackType
    {
        XSCA = 0x4,
        YSCA = 0x8,
        ZSCA = 0xC,
        XPOS = 0x10,
        YPOS = 0x14,
        ZPOS = 0x18,
        XROT = 0x20,
        YROT = 0x24,
        ZROT = 0x28,
    }

    public partial class BFRES : TreeNode
    {
        public class FSKA
        {
            //Here I set an animation list. It's easier to grab a list by the FSKA instance from the bfres
            public static List<Animation> SkeletonAnimations = new List<Animation>();

            public void Read(ResFile TargetWiiUBFRES, AnimationGroupNode ThisAnimation, ResNSW.ResFile b)
            {
                Console.WriteLine("Reading Skeleton Animations ...");

                if (b != null)
                {
                    TreeNode SkeletonAnimation = new TreeNode() { Text = "Skeleton Animations" };

                    ThisAnimation.Nodes.Add(SkeletonAnimation);

                    TreeNode dummy = new TreeNode() { Text = "Animation Set" };

                    int i = 0;
                    foreach (ResNSW.SkeletalAnim ska in b.SkeletalAnims)
                    {
                        if (i == 0)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "0 - 100" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 100)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "100 - 200" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 200)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "200 - 300" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 300)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "300 - 400" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 400)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "400 - 500" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 500)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "500 - 600" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 600)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "600 - 700" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 700)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "700 - 800" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 800)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "800 - 900" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 900)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "900 - 1000" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 1000)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "1000+" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }

                        Animation a = new Animation(ska.Name);
                        SkeletonAnimations.Add(a);

                        a.frameCount = ska.FrameCount;

                        if (i >= 0 && i < 100)
                            SkeletonAnimation.Nodes[0].Nodes.Add(a);
                        if (i >= 100 && i < 200)
                            SkeletonAnimation.Nodes[1].Nodes.Add(a);
                        if (i >= 200 && i < 300)
                            SkeletonAnimation.Nodes[2].Nodes.Add(a);
                        if (i >= 300 && i < 400)
                            SkeletonAnimation.Nodes[3].Nodes.Add(a);
                        if (i >= 400 && i < 500)
                            SkeletonAnimation.Nodes[4].Nodes.Add(a);
                        if (i >= 500 && i < 600)
                            SkeletonAnimation.Nodes[5].Nodes.Add(a);
                        if (i >= 600 && i < 700)
                            SkeletonAnimation.Nodes[6].Nodes.Add(a);
                        if (i >= 700 && i < 800)
                            SkeletonAnimation.Nodes[7].Nodes.Add(a);
                        if (i >= 800 && i < 900)
                            SkeletonAnimation.Nodes[8].Nodes.Add(a);
                        if (i >= 900 && i < 1000)
                            SkeletonAnimation.Nodes[9].Nodes.Add(a);

                        i++;
                        try
                        {
                            foreach (ResNSW.BoneAnim bn in ska.BoneAnims)
                            {
                                FSKANode bonean = new FSKANode(bn);

                                Animation.KeyNode bone = new Animation.KeyNode("");
                                a.bones.Add(bone);
                                if (ska.FlagsRotate == ResNSW.SkeletalAnimFlagsRotate.EulerXYZ)
                                    bone.rotType = Animation.RotationType.Euler;
                                else
                                    bone.rotType = Animation.RotationType.Quaternion;

                                bone.Text = bonean.Text;

                                for (int Frame = 0; Frame < ska.FrameCount; Frame++)
                                {

                                    //Set base/start values for bones.
                                    //Note. BOTW doesn't use base values as it uses havok engine. Need to add option to disable these
                                    if (Frame == 0)
                                    {
                                        if (bn.FlagsBase.HasFlag(ResNSW.BoneAnimFlagsBase.Scale))
                                        {
                                            bone.xsca.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.sca.X });
                                            bone.ysca.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.sca.Y });
                                            bone.zsca.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.sca.Z });
                                        }
                                        if (bn.FlagsBase.HasFlag(ResNSW.BoneAnimFlagsBase.Rotate))
                                        {
                                            bone.xrot.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.rot.X });
                                            bone.yrot.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.rot.Y });
                                            bone.zrot.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.rot.Z });
                                            bone.wrot.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.rot.W });
                                        }
                                        if (bn.FlagsBase.HasFlag(ResNSW.BoneAnimFlagsBase.Translate))
                                        {
                                            bone.xpos.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.pos.X });
                                            bone.ypos.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.pos.Y });
                                            bone.zpos.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.pos.Z });
                                        }
                                    }
                                    foreach (FSKATrack track in bonean.tracks)
                                    {
                                        Animation.KeyFrame frame = new Animation.KeyFrame();
                                        frame.interType = Animation.InterpolationType.Hermite;
                                        frame.Frame = Frame;

                                        FSKAKey left = track.GetLeft(Frame);
                                        FSKAKey right = track.GetRight(Frame);
                                        float value;



                                        value = Animation.Hermite(Frame, left.frame, right.frame, 0, 0, left.unk1, right.unk1);

                                        // interpolate the value and apply
                                        switch (track.flag)
                                        {
                                            case (int)TrackType.XPOS: frame.Value = value; bone.xpos.keys.Add(frame); break;
                                            case (int)TrackType.YPOS: frame.Value = value; bone.ypos.keys.Add(frame); break;
                                            case (int)TrackType.ZPOS: frame.Value = value; bone.zpos.keys.Add(frame); break;
                                            case (int)TrackType.XROT: frame.Value = value; bone.xrot.keys.Add(frame); break;
                                            case (int)TrackType.YROT: frame.Value = value; bone.yrot.keys.Add(frame); break;
                                            case (int)TrackType.ZROT: frame.Value = value; bone.zrot.keys.Add(frame); break;
                                            case (int)TrackType.XSCA: frame.Value = value; bone.xsca.keys.Add(frame); break;
                                            case (int)TrackType.YSCA: frame.Value = value; bone.ysca.keys.Add(frame); break;
                                            case (int)TrackType.ZSCA: frame.Value = value; bone.zsca.keys.Add(frame); break;
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    TreeNode SkeletonAnimation = new TreeNode() { Text = "Skeleton Animations" };

                    ThisAnimation.Nodes.Add(SkeletonAnimation);

                    TreeNode dummy = new TreeNode() { Text = "Animation Set" };

                    int i = 0;
                    foreach (SkeletalAnim ska in TargetWiiUBFRES.SkeletalAnims.Values)
                    {

                        if (i == 0)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "0 - 100" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 100)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "100 - 200" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 200)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "200 - 300" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 300)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "300 - 400" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 400)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "400 - 500" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 500)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "500 - 600" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 600)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "600 - 700" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 700)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "700 - 800" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 800)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "800 - 900" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 900)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "900 - 1000" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }
                        if (i == 1000)
                        {
                            dummy = new TreeNode() { Text = "Animation Set " + "1000+" };
                            SkeletonAnimation.Nodes.Add(dummy);
                        }

                        Animation a = new Animation(ska.Name);

                        if (i >= 0 && i < 100)
                            SkeletonAnimation.Nodes[0].Nodes.Add(a);
                        if (i >= 100 && i < 200)
                            SkeletonAnimation.Nodes[1].Nodes.Add(a);
                        if (i >= 200 && i < 300)
                            SkeletonAnimation.Nodes[2].Nodes.Add(a);
                        if (i >= 300 && i < 400)
                            SkeletonAnimation.Nodes[3].Nodes.Add(a);
                        if (i >= 400 && i < 500)
                            SkeletonAnimation.Nodes[4].Nodes.Add(a);
                        if (i >= 500 && i < 600)
                            SkeletonAnimation.Nodes[5].Nodes.Add(a);
                        if (i >= 600 && i < 700)
                            SkeletonAnimation.Nodes[6].Nodes.Add(a);
                        if (i >= 700 && i < 800)
                            SkeletonAnimation.Nodes[7].Nodes.Add(a);
                        if (i >= 800 && i < 900)
                            SkeletonAnimation.Nodes[8].Nodes.Add(a);
                        if (i >= 900 && i < 1000)
                            SkeletonAnimation.Nodes[9].Nodes.Add(a);


                        a.frameCount = ska.FrameCount;
                        i++;
                        try
                        {
                            foreach (BoneAnim bn in ska.BoneAnims)
                            {
                                FSKANodeWiiU bonean = new FSKANodeWiiU(bn);

                                Animation.KeyNode bone = new Animation.KeyNode("");
                                a.bones.Add(bone);
                                if (ska.FlagsRotate == SkeletalAnimFlagsRotate.EulerXYZ)
                                    bone.rotType = Animation.RotationType.Euler;
                                else
                                    bone.rotType = Animation.RotationType.Quaternion;

                                bone.Text = bonean.Text;


                                for (int Frame = 0; Frame < ska.FrameCount; Frame++)
                                {

                                    //Set base/start values for bones.
                                    //Note. BOTW doesn't use base values as it uses havok engine. Need to add option to disable these
                                    if (Frame == 0)
                                    {
                                        if (bn.FlagsBase.HasFlag(BoneAnimFlagsBase.Scale))
                                        {
                                            bone.xsca.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.sca.X });
                                            bone.ysca.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.sca.Y });
                                            bone.zsca.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.sca.Z });
                                        }
                                        if (bn.FlagsBase.HasFlag(BoneAnimFlagsBase.Rotate))
                                        {
                                            bone.xrot.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.rot.X });
                                            bone.yrot.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.rot.Y });
                                            bone.zrot.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.rot.Z });
                                            bone.wrot.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.rot.W });
                                        }
                                        if (bn.FlagsBase.HasFlag(BoneAnimFlagsBase.Translate))
                                        {
                                            bone.xpos.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.pos.X });
                                            bone.ypos.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.pos.Y });
                                            bone.zpos.keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.pos.Z });
                                        }
                                    }
                                    foreach (FSKATrack track in bonean.tracks)
                                    {
                                        Animation.KeyFrame frame = new Animation.KeyFrame();
                                        frame.interType = Animation.InterpolationType.Hermite;
                                        frame.Frame = Frame;

                                        FSKAKey left = track.GetLeft(Frame);
                                        FSKAKey right = track.GetRight(Frame);
                                        float value;



                                        value = Animation.Hermite(Frame, left.frame, right.frame, 0, 0, left.unk1, right.unk1);

                                        // interpolate the value and apply
                                        switch (track.flag)
                                        {
                                            case (int)TrackType.XPOS: frame.Value = value; bone.xpos.keys.Add(frame); break;
                                            case (int)TrackType.YPOS: frame.Value = value; bone.ypos.keys.Add(frame); break;
                                            case (int)TrackType.ZPOS: frame.Value = value; bone.zpos.keys.Add(frame); break;
                                            case (int)TrackType.XROT: frame.Value = value; bone.xrot.keys.Add(frame); break;
                                            case (int)TrackType.YROT: frame.Value = value; bone.yrot.keys.Add(frame); break;
                                            case (int)TrackType.ZROT: frame.Value = value; bone.zrot.keys.Add(frame); break;
                                            case (int)TrackType.XSCA: frame.Value = value; bone.xsca.keys.Add(frame); break;
                                            case (int)TrackType.YSCA: frame.Value = value; bone.ysca.keys.Add(frame); break;
                                            case (int)TrackType.ZSCA: frame.Value = value; bone.zsca.keys.Add(frame); break;
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }

            public class FSKANode
            {
                public int flags;
                public int flags2;
                public int stride;
                public int BeginRotate;
                public int BeginTranslate;
                public long offBase;
                public int trackCount;
                public int trackFlag;
                public long offTrack;
                public string Text;

                public Vector3 sca, pos;
                public Vector4 rot;
                public List<FSKATrack> tracks = new List<FSKATrack>();

                public FSKANode(ResNSW.BoneAnim b)
                {
                    Text = b.Name;

                    if (b.BaseData.Scale != Syroot.Maths.Vector3F.Zero)
                        sca = new Vector3(b.BaseData.Scale.X, b.BaseData.Scale.Y, b.BaseData.Scale.Z);
                    if (b.BaseData.Rotate != Syroot.Maths.Vector4F.Zero)
                        rot = new Vector4(b.BaseData.Rotate.X, b.BaseData.Rotate.Y, b.BaseData.Rotate.Z, b.BaseData.Rotate.W);
                    if (b.BaseData.Translate != Syroot.Maths.Vector3F.Zero)
                        pos = new Vector3(b.BaseData.Translate.X, b.BaseData.Translate.Y, b.BaseData.Translate.Z);

                    foreach (ResNSW.AnimCurve tr in b.Curves)
                    {

                        FSKATrack t = new FSKATrack();
                        t.flag = (int)tr.AnimDataOffset;
                        tracks.Add(t);

                        float tanscale = tr.Delta;
                        if (tanscale == 0)
                            tanscale = 1;

                        for (int i = 0; i < (ushort)tr.Frames.Length; i++)
                        {
                            if (tr.CurveType == ResNSW.AnimCurveType.Cubic)
                            {
                                int framedata = (int)tr.Frames[i];
                                float keydata = tr.Offset + ((tr.Keys[i, 0] * tr.Scale));
                                float keydata2 = tr.Offset + ((tr.Keys[i, 1] * tr.Scale));
                                float keydata3 = tr.Offset + ((tr.Keys[i, 2] * tr.Scale));
                                float keydata4 = tr.Offset + ((tr.Keys[i, 3] * tr.Scale));

                            }
                            if (tr.KeyType == ResNSW.AnimCurveKeyType.Int16)
                            {

                            }
                            else if (tr.KeyType == ResNSW.AnimCurveKeyType.Single)
                            {

                            }
                            else if (tr.KeyType == ResNSW.AnimCurveKeyType.SByte)
                            {

                            }
                            t.keys.Add(new FSKAKey()
                            {
                                frame = (int)tr.Frames[i],
                                unk1 = tr.Offset + ((tr.Keys[i, 0] * tr.Scale)),
                                unk2 = tr.Offset + ((tr.Keys[i, 1] * tr.Scale)),
                                unk3 = tr.Offset + ((tr.Keys[i, 2] * tr.Scale)),
                                unk4 = tr.Offset + ((tr.Keys[i, 3] * tr.Scale)),
                            });
                        }
                    }
                }
            }

            public class FSKANodeWiiU
            {
                public int flags;
                public int flags2;
                public int stride;
                public int BeginRotate;
                public int BeginTranslate;
                public long offBase;
                public int trackCount;
                public int trackFlag;
                public long offTrack;
                public string Text;

                public Vector3 sca, pos;
                public Vector4 rot;
                public List<FSKATrack> tracks = new List<FSKATrack>();

                public FSKANodeWiiU(BoneAnim b)
                {
                    Text = b.Name;

                    if (b.BaseData.Scale != Syroot.Maths.Vector3F.Zero)
                        sca = new Vector3(b.BaseData.Scale.X, b.BaseData.Scale.Y, b.BaseData.Scale.Z);
                    if (b.BaseData.Rotate != Syroot.Maths.Vector4F.Zero)
                        rot = new Vector4(b.BaseData.Rotate.X, b.BaseData.Rotate.Y, b.BaseData.Rotate.Z, b.BaseData.Rotate.W);
                    if (b.BaseData.Translate != Syroot.Maths.Vector3F.Zero)
                        pos = new Vector3(b.BaseData.Translate.X, b.BaseData.Translate.Y, b.BaseData.Translate.Z);
                
                    foreach (AnimCurve tr in b.Curves)
                    {
                        FSKATrack t = new FSKATrack();
                        t.flag = (int)tr.AnimDataOffset;
                        tracks.Add(t);

                        float tanscale = tr.Delta;
                        if (tanscale == 0)
                            tanscale = 1;

                        for (int i = 0; i < (ushort)tr.Frames.Length; i++)
                        {
                            if (tr.CurveType == AnimCurveType.Cubic)
                            {
                                int framedata = (int)tr.Frames[i];
                                float keydata = tr.Offset + ((tr.Keys[i, 0] * tr.Scale));
                                float keydata2 = tr.Offset + ((tr.Keys[i, 1] * tr.Scale));
                                float keydata3 = tr.Offset + ((tr.Keys[i, 2] * tr.Scale));
                                float keydata4 = tr.Offset + ((tr.Keys[i, 3] * tr.Scale));
                            }

                            t.keys.Add(new FSKAKey()
                            {
                                frame = (int)tr.Frames[i],
                                unk1 = tr.Offset + ((tr.Keys[i, 0] * tr.Scale)),
                            });
                        }
                    }
                }
            }

            public class FSKATrack
            {
                public short type;
                public short keyCount;
                public int flag;
                public int unk2;
                public int padding1;
                public int padding2;
                public int padding3;
                public float frameCount;
                public float scale, init, unkf3;
                public long offtolastKeys, offtolastData;
                public List<FSKAKey> keys = new List<FSKAKey>();

                public int offset;

                public FSKAKey GetLeft(int frame)
                {
                    FSKAKey prev = keys[0];

                    for (int i = 0; i < keys.Count - 1; i++)
                    {
                        FSKAKey key = keys[i];
                        if (key.frame > frame && prev.frame <= frame)
                            break;
                        prev = key;
                    }

                    return prev;
                }
                public FSKAKey GetRight(int frame)
                {
                    FSKAKey cur = keys[0];
                    FSKAKey prev = keys[0];

                    for (int i = 1; i < keys.Count; i++)
                    {
                        FSKAKey key = keys[i];
                        cur = key;
                        if (key.frame > frame && prev.frame <= frame)
                            break;
                        prev = key;
                    }

                    return cur;
                }
            }

            public class FSKAKey
            {
                public int frame;
                public float unk1, unk2, unk3, unk4;

                public int offset;
            }
        }

    }
}
