using System;
using System.Collections.Generic;
using OpenTK;
using System.IO;

namespace Smash_Forge
{
    public class OMO : FileBase
    {
        public class OMOHeader
        {
            public string magic = "OMO ";
            public int verHi = 1, verLow = 3;
            public int flags = 0x091E100C;
            public ushort unk1 = 0, boneCount;
            public ushort frameCount, frameSize;
            public int nodeOffset, interOffset, keyOffset;
        }

        public OMOHeader header;

        public override Endianness Endian { get; set; }
        List<OMONode> Nodes = new List<OMONode>();
        List<OMOFrame> Frames = new List<OMOFrame>();

        // display
        int frame = 0;

        public OMO(FileData d)
        {
            Read(d);
        }

        public int size()
        {
            return Frames.Count;
        }

        public void Read(FileData d)
        {
            d.Endian = Endianness.Big;

            d.skip(4);

            header = new OMOHeader()
            {
                verHi = d.readShort(),
                verLow = d.readShort(),
                flags = d.readInt(),
                unk1 = (ushort)d.readShort(),
                boneCount = (ushort)d.readShort(),
                frameCount = (ushort)d.readShort(),
                frameSize = (ushort)d.readShort(),
                nodeOffset = d.readInt(),
                interOffset = d.readInt(),
                keyOffset = d.readInt()
            };

            d.seek(header.nodeOffset);
            for (int i = 0; i < header.boneCount; i++)
            {
                OMONode node = new OMONode()
                {
                    flags = d.readInt(),
                    hash = (uint)d.readInt(),
                    interOffset = d.readInt(),
                    keyOffset = d.readInt()
                };
                int temp = d.pos();
                d.seek(header.interOffset + node.interOffset);
                Console.WriteLine(node.hash.ToString("x") + " " + (header.interOffset + node.interOffset).ToString("x"));
                node.Read(d);
                d.seek(temp);
                Nodes.Add(node);
            }

            d.seek(header.keyOffset);
            for (int i = 0; i < header.frameCount; i++)
            {
                OMOFrame frame = new OMOFrame();
                for (int j = 0; j < header.frameSize / 2; j++)
                    frame.keys.Add((ushort)d.readShort());
                Frames.Add(frame);
            }
        }

        public void Apply(VBN vbn, int frame)
        {
            if (frame == 0)
                vbn.reset();

            OMOFrame keys = Frames[frame];

            foreach (OMONode node in Nodes)
                foreach (Bone b in vbn.bones)
                {
                    if (b.boneId == node.hash)
                    {
                        // apply interpolation frames
                        Console.WriteLine(b.Text);
                        node.Apply(b, keys);
                        break;
                    }

                }

            vbn.update();
        }

        public void setFrame(int frame)
        {
            this.frame = frame;
        }

        public override void Read(string filename)
        {
            Read(new FileData(filename));
        }

        public override byte[] Rebuild()
        {
            throw new NotImplementedException();
        }


        public static Vector4 calcW(Vector4 q)
        {
            q.W = (float)Math.Sqrt(Math.Abs(1 - (q.X * q.X + q.Y * q.Y + q.Z * q.Z)));
            return q;
        }

        public static bool hasFlag(int flag, OMOFlags f)
        {
            if (f == OMOFlags.hasPos || f == OMOFlags.hasRot || f == OMOFlags.hasSca)
                return (((flag & 0xFF000000) & (int)f) == (int)f);

            if (f == OMOFlags.PosConst || f == OMOFlags.PosInter)
                return (((flag & 0x00FF0000) & (int)f) == (int)f);

            if (f == OMOFlags.RotInter || f == OMOFlags.RotFrame
             || f == OMOFlags.RotFConst || f == OMOFlags.RotConst)
                return (((flag & 0x0000F000)) == (int)f);

            if (f == OMOFlags.ScaConst || f == OMOFlags.ScaConst2)
                return (((flag & 0x00000F00)) == (int)f);

            return ((flag & 0x000000F0) == (int)f);
        }

        public enum OMOFlags
        {
            hasPos = 0x01000000,
            hasRot = 0x02000000,
            hasSca = 0x04000000,
            PosInter = 0x00080000,
            PosConst = 0x00200000,
            RotInter = 0x00005000,
            RotFConst = 0x00006000,
            RotConst = 0x00007000,
            RotFrame = 0x0000A000,
            ScaConst = 0x00000200,
            ScaConst2 = 0x00000300,
            ScaInter = 0x00000080
        }

        public class OMONode
        {
            public int flags = 0;
            public uint hash = 0;

            //
            Vector3 pos_min = new Vector3(), pos_max = new Vector3();
            Vector4 rot_min = new Vector4(), rot_max = new Vector4();
            Vector3 sca_min = new Vector3(), sca_max = new Vector3();
            public OMOFlags posType = OMOFlags.hasPos, rotType = OMOFlags.hasRot, scaType = OMOFlags.hasSca;

            // reading only
            public int interOffset;
            public int keyOffset;

            public void Apply(Bone b, OMOFrame f)
            {
                if (posType == OMOFlags.PosConst)
                    b.pos = pos_min;
                if (rotType == OMOFlags.RotConst || rotType == OMOFlags.RotFConst)
                    b.rot = new Quaternion(rot_min.Xyz, rot_min.W);
                if (scaType == OMOFlags.ScaConst || scaType == OMOFlags.ScaConst2)
                    b.sca = sca_min;

                int i = keyOffset / 2;

                if (posType == OMOFlags.PosInter)
                {
                    b.pos = new Vector3(
                        pos_min.X + (pos_max.X * ((float)f.keys[i++] / 0xFFFF)),
                        pos_min.Y + (pos_max.Y * ((float)f.keys[i++] / 0xFFFF)),
                        pos_min.Z + (pos_max.Z * ((float)f.keys[i++] / 0xFFFF))
                        );
                }

                if (rotType == OMOFlags.RotInter)
                {
                    Vector4 v = calcW(new Vector4(
                        rot_min.X + (rot_max.X * ((float)f.keys[i++] / 0xFFFF)),
                        rot_min.Y + (rot_max.Y * ((float)f.keys[i++] / 0xFFFF)),
                        rot_min.Z + (rot_max.Z * ((float)f.keys[i++] / 0xFFFF))
                        , 0));
                    //Console.WriteLine(rot_min.ToString() + " " + rot_max.ToString());
                    b.rot = new Quaternion(v.Xyz, v.W);
                }

                if (rotType == OMOFlags.RotFrame)
                {
                    Vector4 v = new Vector4(
                        ((ushort)f.keys[i++] / (float)0xFFFF),
                        ((ushort)f.keys[i++] / (float)0xFFFF),
                        ((ushort)f.keys[i++] / (float)0xFFFF),
                        ((ushort)f.keys[i++] / (float)0xFFFF));
                    Console.WriteLine("WHAT DO I DO HERE?");
                    Console.WriteLine(b.rot.ToString() + " " + v.ToString() + " " + Math.Sqrt(v.X*v.X + v.Y*v.Y + v.Z*v.Z + v.W*v.W) + "\n" + Matrix4.CreateFromQuaternion(b.rot).Inverted().ToString());
                    //4adbb195
                }

                if (scaType == OMOFlags.ScaInter)
                {
                    b.sca = new Vector3(
                        sca_min.X + (sca_max.X * ((float)f.keys[i++] / 0xFFFF)),
                        sca_min.Y + (sca_max.Y * ((float)f.keys[i++] / 0xFFFF)),
                        sca_min.Z + (sca_max.Z * ((float)f.keys[i++] / 0xFFFF))
                        );
                }
            }

            public void Read(FileData d)
            {
                if (hasFlag(flags, OMOFlags.hasPos))
                {
                    if (hasFlag(flags, OMOFlags.PosConst))
                    {
                        posType = OMOFlags.PosConst;
                        pos_min = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                    }

                    if (hasFlag(flags, OMOFlags.PosInter))
                    {
                        posType = OMOFlags.PosInter;
                        pos_min = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                        pos_max = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                    }
                }
                if (hasFlag(flags, OMOFlags.hasRot))
                {
                    if (hasFlag(flags, OMOFlags.RotConst))
                    {
                        rotType = OMOFlags.RotConst;
                        rot_min = calcW(new Vector4(d.readFloat(), d.readFloat(), d.readFloat(), 0));
                    }
                    if (hasFlag(flags, OMOFlags.RotFConst))
                    {
                        rotType = OMOFlags.RotFConst;
                        rot_min = new Vector4(d.readFloat(), d.readFloat(), d.readFloat(), d.readFloat());
                    }
                    if (hasFlag(flags, OMOFlags.RotInter))
                    {
                        rotType = OMOFlags.RotInter;
                        rot_min = new Vector4(d.readFloat(), d.readFloat(), d.readFloat(), 0);
                        rot_max = new Vector4(d.readFloat(), d.readFloat(), d.readFloat(), 0);
                    }
                    if (hasFlag(flags, OMOFlags.RotFrame))
                        rotType = OMOFlags.RotFrame;

                }
                if (hasFlag(flags, OMOFlags.hasSca))
                {
                    if (hasFlag(flags, OMOFlags.ScaConst) || hasFlag(flags, OMOFlags.ScaConst2))
                    {
                        scaType = OMOFlags.ScaConst;
                        if (hasFlag(flags, OMOFlags.ScaConst2))
                            scaType = OMOFlags.ScaConst2;
                        sca_min = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                    }

                    if (hasFlag(flags, OMOFlags.ScaInter))
                    {
                        scaType = OMOFlags.ScaInter;
                        sca_min = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                        sca_max = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                    }
                }
            }
        }

        public class OMOFrame
        {
            public List<ushort> keys = new List<ushort>();
        }
    }
}

