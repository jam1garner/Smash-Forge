using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

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
        public static AnimationGroupNode Read(string filename)
        {
            FileData f = new FileData(filename);
            f.Endian = System.IO.Endianness.Little;

            f.skip(56);
            long FSKAOffset = f.readInt64();
            f.skip(126);
            int FSKACount = f.readShort();

            Console.WriteLine(FSKAOffset + " test " + FSKACount);

            AnimationGroupNode ThisAnimation = new AnimationGroupNode() { Text = filename };

    
            for (int i = 0; i < FSKACount; i++)
            {
                f.seek((int)FSKAOffset + (i * 96));
                FSKAData anim = new FSKAData(f);

                Animation a = new Animation(anim.Name);
                ThisAnimation.Nodes.Add(a);

                a.FrameCount = anim.frameCount;

                f.seek((int)anim.BoneAnimArrayOffset);
                for (int b = 0; b < anim.boneCount - 1; b++)
                {
                    FSKANode bonean = new FSKANode(f);

                    Animation.KeyNode bone = new Animation.KeyNode("");
                    a.Bones.Add(bone);
                    bone.RotType = Animation.RotationType.EULER;
                    bone.Text = bonean.Text;

                    for (int Frame = 0; Frame < anim.frameCount; Frame++)
                    {

                        //Set base/start values for bones.
                        //Note. BOTW doesn't use base values as it uses havok engine
                        if (Frame == 0)
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

                            value = CHR0.interHermite(Frame, left.frame, right.frame, 0, 0, left.unk1, right.unk1);


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
            return ThisAnimation;
        }
        public class FSKAData
        {
            public int HeaderLength1;
            public int HeaderLength2;

            public int frameCount;
            public int boneCount;
            public int flags;
            public int curveCount;
            public int bakeSize;
            public long BoneAnimArrayOffset;
            public long SkeletonOffset;
            public string Name;

            public FSKAData(FileData f)
            {
                f.Endian = System.IO.Endianness.Little;

                f.skip(4); //Magic
                HeaderLength1 = f.readInt();
                HeaderLength2 = f.readInt();
                f.skip(4); //padding
                Name = f.readString(f.readInt() + 2, -1);
                f.skip(4); //padding
                long FilePath = f.readInt64();
                f.skip(8); //padding
                long unk3 = f.readInt64(); ;
                BoneAnimArrayOffset = f.readInt64(); // offset to start of base values
                SkeletonOffset = f.readInt64();
                f.skip(8); //padding
                flags = f.readInt();
                frameCount = f.readInt();
                curveCount = f.readInt();
                bakeSize = f.readInt();
                boneCount = f.readShort();
                int userDataCount = f.readShort();
                f.skip(4); //padding
                f.seek((int)BoneAnimArrayOffset);
                for (int i = 0; i < boneCount - 1; i++)
                {
                    new FSKANode(f);
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

            public Vector3 sca, rot, pos;
            public List<FSKATrack> tracks = new List<FSKATrack>();


            public FSKANode(FileData f)
            {
                Text = f.readString(f.readInt() + 2, -1);
                f.skip(4); // padding
                offTrack = f.readInt64();
                offBase = f.readInt64();
                flags = f.readInt();
                BeginRotate = f.readByte();
                BeginTranslate = f.readByte();
                trackCount = f.readByte();
                stride = f.readByte();
                f.skip(8); // padding

                trackFlag = (flags & 0x0000FF00) >> 8;

                int temp = f.pos();

                // offset 1 is base positions
                //Console.WriteLine(off1.ToString("x"));
                if (offBase != 0)
                {
                    f.seek((int)offBase);
                    sca = new Vector3(f.readFloat(), f.readFloat(), f.readFloat());
                    rot = new Vector3(f.readFloat(), f.readFloat(), f.readFloat());
                    f.skip(4); // for quaternion, but 1.0 if eul
                    pos = new Vector3(f.readFloat(), f.readFloat(), f.readFloat());
                }
                else
                {
                    sca = new Vector3(1, 1, 1);
                    rot = new Vector3(0, 0, 0);
                    f.skip(4); // for quaternion, but 1.0 if eul
                    pos = new Vector3(0, 0, 0);

                }


                f.seek((int)offTrack);
                for (int tr = 0; tr < trackCount; tr++)
                {
                    FSKATrack t = (new FSKATrack()
                    {
                        offset = f.pos(),
                        offtolastKeys = f.readInt64(),
                        offtolastData = f.readInt64(),
                        type = (short)f.readShort(),
                        keyCount = (short)f.readShort(),
                        flag = f.readInt(), //targetOffset
                        unk2 = f.readInt(), //StartFrame
                        frameCount = f.readFloat(), //EndFrame
                        scale = f.readFloat(),
                        init = f.readFloat(), //union
                        unkf3 = f.readFloat(), //DataDelta
                        padding1 = f.readInt(),

                    });
                    tracks.Add(t);

                    if (t.type != 0x2 && t.type != 0x5 && t.type != 0x6 && t.type != 0x9 && t.type != 0xA)
                        Console.WriteLine(Text + " " + t.type.ToString("x"));

                    int tem = f.pos();
                    // bone section
                    f.seek((int)t.offtolastKeys);
                    int[] frames = new int[t.keyCount];
                    for (int i = 0; i < t.keyCount; i++)
                        if (t.type == 0x1 || t.type == 0x5 || t.type == 0x9)
                            frames[i] = f.readShort() >> 5;
                        else
                            frames[i] = f.readByte();
                    f.align(4);

                    float tanscale = t.unkf3;
                    if (tanscale == 0)
                        tanscale = 1;
                    f.seek((int)t.offtolastData);
                    for (int i = 0; i < t.keyCount; i++)
                        switch (t.type)
                        {
                            case 0x0:
                                t.keys.Add(new FSKAKey()
                                {
                                    frame = frames[i],
                                    unk1 = t.init + ((f.readFloat() * t.scale)),
                                    unk2 = f.readFloat(),
                                    unk3 = f.readFloat(),
                                    unk4 = f.readFloat(),
                                });
                                break;
                            case 0x1:
                                t.keys.Add(new FSKAKey()
                                {
                                    frame = frames[i],
                                    unk1 = t.init + ((f.readShort() * t.scale)),
                                    unk2 = f.readShort(),
                                    unk3 = f.readShort(),
                                    unk4 = f.readShort(),
                                });
                                break;
                            case 0x2:
                                t.keys.Add(new FSKAKey()
                                {
                                    frame = frames[i],
                                    unk1 = t.init + ((f.readFloat() * t.scale)),
                                    unk2 = f.readFloat(),
                                    unk3 = f.readFloat(),
                                    unk4 = f.readFloat(),
                                });
                                break;
                            case 0x5:
                                t.keys.Add(new FSKAKey()
                                {
                                    frame = frames[i],
                                    unk1 = t.init + (((short)f.readShort() * t.scale)),
                                    unk2 = t.unkf3 + (((short)f.readShort() * t.scale)),
                                    unk3 = t.unkf3 + (((short)f.readShort() * t.scale)),
                                    unk4 = t.unkf3 + (((short)f.readShort() * t.scale))
                                });
                                break;
                            case 0x6:
                                t.keys.Add(new FSKAKey()
                                {
                                    frame = frames[i],
                                    unk1 = t.init + (((short)f.readShort() * t.scale)),
                                    unk2 = t.unkf3 + (((short)f.readShort() / (float)0x7FFF)),
                                    unk3 = t.unkf3 + (((short)f.readShort() / (float)0x7FFF)),
                                    unk4 = t.unkf3 + (((short)f.readShort() / (float)0x7FFF))
                                });
                                break;
                            case 0x9:
                                t.keys.Add(new FSKAKey()
                                {
                                    frame = frames[i],
                                    unk1 = t.init + (((sbyte)f.readByte() * t.scale)),
                                    unk2 = t.unkf3 + (((sbyte)f.readByte() * t.scale)),
                                    unk3 = t.unkf3 + (((sbyte)f.readByte() * t.scale)),
                                    unk4 = t.unkf3 + (((sbyte)f.readByte() * t.scale))
                                });
                                break;
                            case 0xA:
                                t.keys.Add(new FSKAKey()
                                {
                                    frame = frames[i],
                                    unk1 = t.init + (((sbyte)f.readByte() * t.scale)),
                                    unk2 = t.unkf3 + (((sbyte)f.readByte() * t.scale)),
                                    unk3 = t.unkf3 + (((sbyte)f.readByte() * t.scale)),
                                    unk4 = t.unkf3 + (((sbyte)f.readByte() * t.scale))
                                });
                                break;
                            default:
                                break;
                        }

                    f.seek(tem);
                }

                f.seek(temp);
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
