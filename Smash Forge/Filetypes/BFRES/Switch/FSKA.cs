using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Syroot.NintenTools.Bfres;
using System.IO;
using Syroot.NintenTools.Yaz0;
using ResNSW = Syroot.NintenTools.NSW.Bfres;


namespace Smash_Forge
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


    public class FSKA
    {
        public static AnimationGroupNode ThisAnimation;

        public static AnimationGroupNode Read(byte[] file_data, ResFile TargetWiiUBFRES, ResNSW.ResFile b)
        {
            Console.WriteLine("Reading Animations ...");

            if (b != null)
            {
                AnimationGroupNode ThisAnimation = new AnimationGroupNode() { Text = "Skeleton Animations" };

                TreeNode dummy = new TreeNode() { Text = "Animation Set" };

                int i = 0;
                foreach (ResNSW.SkeletalAnim ska in b.SkeletalAnims)
                {
                    if (i == 0)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "0 - 100" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 100)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "100 - 200" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 200)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "200 - 300" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 300)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "300 - 400" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 400)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "400 - 500" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 500)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "500 - 600" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 600)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "600 - 700" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 700)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "700 - 800" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 800)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "800 - 900" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 900)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "900 - 1000" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 1000)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "1000+" };
                        ThisAnimation.Nodes.Add(dummy);
                    }

                    Animation a = new Animation(ska.Name);

                    if (i >= 0 && i < 100)
                        ThisAnimation.Nodes[0].Nodes.Add(a);
                    if (i >= 100 && i < 200)
                        ThisAnimation.Nodes[1].Nodes.Add(a);
                    if (i >= 200 && i < 300)
                        ThisAnimation.Nodes[2].Nodes.Add(a);
                    if (i >= 300 && i < 400)
                        ThisAnimation.Nodes[3].Nodes.Add(a);
                    if (i >= 400 && i < 500)
                        ThisAnimation.Nodes[4].Nodes.Add(a);
                    if (i >= 500 && i < 600)
                        ThisAnimation.Nodes[5].Nodes.Add(a);
                    if (i >= 600 && i < 700)
                        ThisAnimation.Nodes[6].Nodes.Add(a);
                    if (i >= 700 && i < 800)
                        ThisAnimation.Nodes[7].Nodes.Add(a);
                    if (i >= 800 && i < 900)
                        ThisAnimation.Nodes[8].Nodes.Add(a);
                    if (i >= 900 && i < 1000)
                        ThisAnimation.Nodes[9].Nodes.Add(a);


                    a.FrameCount = ska.FrameCount;
                    i++;
                    try
                    {
                        foreach (Syroot.NintenTools.NSW.Bfres.BoneAnim bn in ska.BoneAnims)
                        {
                            FSKANode bonean = new FSKANode(bn);

                            Animation.KeyNode bone = new Animation.KeyNode("");
                            a.Bones.Add(bone);
                            if (ska.FlagsRotate == ResNSW.SkeletalAnimFlagsRotate.EulerXYZ)
                                bone.RotType = Animation.RotationType.EULER;
                            else
                                bone.RotType = Animation.RotationType.QUATERNION;

                            bone.Text = bonean.Text;

                            for (int Frame = 0; Frame < ska.FrameCount; Frame++)
                            {

                                //Set base/start values for bones.
                                //Note. BOTW doesn't use base values as it uses havok engine. Need to add option to disable these
                                if (Frame == 0 && Runtime.HasNoAnimationBaseValues == false)
                                {
                                    bone.XSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = 1 });
                                    bone.YSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = 1 });
                                    bone.ZSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = 1 });
                                    bone.XROT.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.rot.X });
                                    bone.YROT.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.rot.Y });
                                    bone.ZROT.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.rot.Z });
                                    bone.XPOS.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.pos.X });
                                    bone.YPOS.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.pos.Y });
                                    bone.ZPOS.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.pos.Z });
                                }
                                foreach (FSKATrack track in bonean.tracks)
                                {
                                    Animation.KeyFrame frame = new Animation.KeyFrame();
                                    frame.InterType = Animation.InterpolationType.HERMITE;
                                    frame.Frame = Frame;

                                    FSKAKey left = track.GetLeft(Frame);
                                    FSKAKey right = track.GetRight(Frame);
                                    float value;



                                    value = Animation.Hermite(Frame, left.frame, right.frame, 0, 0, left.unk1, right.unk1);

                                    // interpolate the value and apply
                                    switch (track.flag)
                                    {
                                        case (int)TrackType.XPOS: frame.Value = value; bone.XPOS.Keys.Add(frame); break;
                                        case (int)TrackType.YPOS: frame.Value = value; bone.YPOS.Keys.Add(frame); break;
                                        case (int)TrackType.ZPOS: frame.Value = value; bone.ZPOS.Keys.Add(frame); break;
                                        case (int)TrackType.XROT: frame.Value = value; bone.XROT.Keys.Add(frame); break;
                                        case (int)TrackType.YROT: frame.Value = value; bone.YROT.Keys.Add(frame); break;
                                        case (int)TrackType.ZROT: frame.Value = value; bone.ZROT.Keys.Add(frame); break;
                                        case (int)TrackType.XSCA: frame.Value = value; bone.XSCA.Keys.Add(frame); break;
                                        case (int)TrackType.YSCA: frame.Value = value; bone.YSCA.Keys.Add(frame); break;
                                        case (int)TrackType.ZSCA: frame.Value = value; bone.ZSCA.Keys.Add(frame); break;
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                return ThisAnimation;
            }
            else
            {
                ThisAnimation = new AnimationGroupNode() { Text = "Skeleton Animations" };

                TreeNode dummy = new TreeNode() { Text = "Animation Set" };

                int i = 0;
                foreach (SkeletalAnim ska in TargetWiiUBFRES.SkeletalAnims.Values)
                {

                    if (i == 0)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "0 - 100" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 100)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "100 - 200" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 200)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "200 - 300" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 300)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "300 - 400" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 400)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "400 - 500" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 500)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "500 - 600" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 600)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "600 - 700" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 700)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "700 - 800" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 800)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "800 - 900" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 900)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "900 - 1000" };
                        ThisAnimation.Nodes.Add(dummy);
                    }
                    if (i == 1000)
                    {
                        dummy = new TreeNode() { Text = "Animation Set " + "1000+" };
                        ThisAnimation.Nodes.Add(dummy);
                    }

                    Animation a = new Animation(ska.Name);

                    if (i >= 0 && i < 100)
                        ThisAnimation.Nodes[0].Nodes.Add(a);
                    if (i >= 100 && i < 200)
                        ThisAnimation.Nodes[1].Nodes.Add(a);
                    if (i >= 200 && i < 300)
                        ThisAnimation.Nodes[2].Nodes.Add(a);
                    if (i >= 300 && i < 400)
                        ThisAnimation.Nodes[3].Nodes.Add(a);
                    if (i >= 400 && i < 500)
                        ThisAnimation.Nodes[4].Nodes.Add(a);
                    if (i >= 500 && i < 600)
                        ThisAnimation.Nodes[5].Nodes.Add(a);
                    if (i >= 600 && i < 700)
                        ThisAnimation.Nodes[6].Nodes.Add(a);
                    if (i >= 700 && i < 800)
                        ThisAnimation.Nodes[7].Nodes.Add(a);
                    if (i >= 800 && i < 900)
                        ThisAnimation.Nodes[8].Nodes.Add(a);
                    if (i >= 900 && i < 1000)
                        ThisAnimation.Nodes[9].Nodes.Add(a);


                    a.FrameCount = ska.FrameCount;
                    i++;
                    try
                    {
                        foreach (BoneAnim bn in ska.BoneAnims)
                        {
                            FSKANodeWiiU bonean = new FSKANodeWiiU(bn);

                            Animation.KeyNode bone = new Animation.KeyNode("");
                            a.Bones.Add(bone);
                            if (ska.FlagsRotate == SkeletalAnimFlagsRotate.EulerXYZ)
                                bone.RotType = Animation.RotationType.EULER;
                            else
                                bone.RotType = Animation.RotationType.QUATERNION;

                            bone.Text = bonean.Text;


                            for (int Frame = 0; Frame < ska.FrameCount; Frame++)
                            {

                                //Set base/start values for bones.
                                //Note. BOTW doesn't use base values as it uses havok engine. Need to add option to disable these
                                if (Frame == 0 && Runtime.HasNoAnimationBaseValues == false)
                                {
                                    bone.XSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.sca.X });
                                    bone.YSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.sca.Y });
                                    bone.ZSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.sca.Z });
                                    bone.XROT.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.rot.X });
                                    bone.YROT.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.rot.Y });
                                    bone.ZROT.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.rot.Z });
                                    bone.XPOS.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.pos.X });
                                    bone.YPOS.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.pos.Y });
                                    bone.ZPOS.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = bonean.pos.Z });
                                }
                                foreach (FSKATrack track in bonean.tracks)
                                {
                                    Animation.KeyFrame frame = new Animation.KeyFrame();
                                    frame.InterType = Animation.InterpolationType.HERMITE;
                                    frame.Frame = Frame;

                                    FSKAKey left = track.GetLeft(Frame);
                                    FSKAKey right = track.GetRight(Frame);
                                    float value;



                                    value = Animation.Hermite(Frame, left.frame, right.frame, 0, 0, left.unk1, right.unk1);

                                    // interpolate the value and apply
                                    switch (track.flag)
                                    {
                                        case (int)TrackType.XPOS: frame.Value = value; bone.XPOS.Keys.Add(frame); break;
                                        case (int)TrackType.YPOS: frame.Value = value; bone.YPOS.Keys.Add(frame); break;
                                        case (int)TrackType.ZPOS: frame.Value = value; bone.ZPOS.Keys.Add(frame); break;
                                        case (int)TrackType.XROT: frame.Value = value; bone.XROT.Keys.Add(frame); break;
                                        case (int)TrackType.YROT: frame.Value = value; bone.YROT.Keys.Add(frame); break;
                                        case (int)TrackType.ZROT: frame.Value = value; bone.ZROT.Keys.Add(frame); break;
                                        case (int)TrackType.XSCA: frame.Value = value; bone.XSCA.Keys.Add(frame); break;
                                        case (int)TrackType.YSCA: frame.Value = value; bone.YSCA.Keys.Add(frame); break;
                                        case (int)TrackType.ZSCA: frame.Value = value; bone.ZSCA.Keys.Add(frame); break;
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                return ThisAnimation;
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

            public Vector3 sca, rot, pos;
            public List<FSKATrack> tracks = new List<FSKATrack>();

            public FSKANode(Syroot.NintenTools.NSW.Bfres.BoneAnim b)
            {
                Text = b.Name;

                // offset 1 is base positions
                //Console.WriteLine(off1.ToString("x"));
                if (b.BaseData.Scale != new Syroot.Maths.Vector3F(0f, 0f, 0f))
                {
                    sca = new Vector3(b.BaseData.Scale.X, b.BaseData.Scale.Y, b.BaseData.Scale.Z);
                    rot = new Vector3(b.BaseData.Rotate.X, b.BaseData.Rotate.Y, b.BaseData.Rotate.Z);
                    pos = new Vector3(b.BaseData.Translate.X, b.BaseData.Translate.Y, b.BaseData.Translate.Z);
                }
                else
                {
                    sca = new Vector3(1, 1, 1);
                    rot = new Vector3(0, 0, 0);
                    pos = new Vector3(0, 0, 0);

                }

                //       Console.WriteLine("Name = " + b.Name);

                foreach (Syroot.NintenTools.NSW.Bfres.AnimCurve tr in b.Curves)
                {

                    //  Console.WriteLine(tr.AnimDataOffset);

                    FSKATrack t = new FSKATrack();
                    t.flag = (int)tr.AnimDataOffset;
                    tracks.Add(t);



                    //     Console.WriteLine("Flag = " + (int)tr.AnimDataOffset + " Offset = " + tr.Offset + "  Scale = " + tr.Scale);


                    //   Console.WriteLine();

                    float tanscale = tr.Delta;
                    if (tanscale == 0)
                        tanscale = 1;

                    for (int i = 0; i < (ushort)tr.Frames.Length; i++)
                    {
                        if (tr.CurveType == Syroot.NintenTools.NSW.Bfres.AnimCurveType.Cubic)
                        {
                            int framedata = (int)tr.Frames[i];
                            float keydata = tr.Offset + ((tr.Keys[i, 0] * tr.Scale));
                            float keydata2 = tr.Offset + ((tr.Keys[i, 1] * tr.Scale));
                            float keydata3 = tr.Offset + ((tr.Keys[i, 2] * tr.Scale));
                            float keydata4 = tr.Offset + ((tr.Keys[i, 3] * tr.Scale));
                            //    Console.WriteLine($"{framedata} {keydata} {keydata2} {keydata3} {keydata4} ");
                            //     Console.WriteLine($"Raw Data = " + tr.Keys[i, 0]);

                        }
                        if (tr.KeyType == ResNSW.AnimCurveKeyType.Int16)
                        {

                        }
                        else if(tr.KeyType == ResNSW.AnimCurveKeyType.Single)
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

            public Vector3 sca, rot, pos;
            public List<FSKATrack> tracks = new List<FSKATrack>();

            public FSKANodeWiiU(BoneAnim b)
            {
                Text = b.Name;

                // offset 1 is base positions
                //Console.WriteLine(off1.ToString("x"));
                if (b.BaseData.Scale != null && b.BaseData.Translate != null && b.BaseData.Rotate != null)
                {
                    sca = new Vector3(b.BaseData.Scale.X, b.BaseData.Scale.Y, b.BaseData.Scale.Z);
                    rot = new Vector3(b.BaseData.Rotate.X, b.BaseData.Rotate.Y, b.BaseData.Rotate.Z);
                    pos = new Vector3(b.BaseData.Translate.X, b.BaseData.Translate.Y, b.BaseData.Translate.Z);
                }
                else
                {
                    sca = new Vector3(1, 1, 1);
                    rot = new Vector3(0, 0, 0);
                    pos = new Vector3(0, 0, 0);

                }

                //       Console.WriteLine("Name = " + b.Name);

                foreach (AnimCurve tr in b.Curves)
                {

                    //  Console.WriteLine(tr.AnimDataOffset);

                    FSKATrack t = new FSKATrack();
                    t.flag = (int)tr.AnimDataOffset;
                    tracks.Add(t);



                    //     Console.WriteLine("Flag = " + (int)tr.AnimDataOffset + " Offset = " + tr.Offset + "  Scale = " + tr.Scale);


                    //   Console.WriteLine();

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
                            //    Console.WriteLine($"{framedata} {keydata} {keydata2} {keydata3} {keydata4} ");
                            //     Console.WriteLine($"Raw Data = " + tr.Keys[i, 0]);

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
