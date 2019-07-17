using System;
using System.Collections.Generic;
using OpenTK;

namespace SmashForge
{
    public class DAT_Animation
    {
        public int frame = 0;

        //Dicionary<int, int> nodes = new Dicionary<int, int>();
        //Dictionary<int, KeyNode> nodes = new Dictionary<int, KeyNode>();
        public List<List<DATAnimTrack>> nodes = new List<List<DATAnimTrack>>();
        public int frameCount = 0;
        public string Name = "";

        public bool Debug = false;
        
        public enum AnimType
        {
            NONE = 0,
            XROT = 1,
            YROT = 2,
            ZROT = 3,
            XPOS = 5,
            YPOS = 6,
            ZPOS = 7,
            XSCA = 8,
            YSCA = 9,
            ZSCA = 10,
            UNK = 0
        }

        public enum InterpolationType
        {
            Step = 1,
            Linear = 2,
            HermiteValue = 3,
            Hermite = 4,
            HermiteCurve = 5,
            Constant = 6
        }

        public class DATAnimTrack
        {
            public AnimType type;
            public List<KeyNode> keys = new List<KeyNode>();
            private DAT_Animation anim;
        }

        public class KeyNode
        {
            public InterpolationType interpolationType;
            public float value;
            public float tan = 1, frame;
        }

        public DAT_Animation()
        {

        }

        public void Apply(VBN bone)
        {
            for(int i = 0; i < nodes.Count; i++)
            {
                List<AnimType> used = new List<AnimType>();

                Bone b = bone.bones[i];
                Vector3 erot = ANIM.quattoeul(b.rot);

                foreach (DATAnimTrack track in nodes[i])
                {
                    KeyNode node = track.keys[0];

                    if(Debug)
                     Console.WriteLine("Bone " + i + " " + track.type + " " + node.value);

                    switch (track.type)
                    {
                        case AnimType.XPOS:
                            b.pos.X = node.value;
                            break;
                        case AnimType.YPOS:
                            b.pos.Y = node.value;
                            break;
                        case AnimType.ZPOS:
                            b.pos.Z = node.value;
                            break;
                        case AnimType.XROT:
                            erot.X = node.value; 
                            break;
                        case AnimType.YROT:
                            erot.Y = node.value;
                            break;
                        case AnimType.ZROT:
                            erot.Z = node.value;
                            break;
                    }
                }
                b.rot = VBN.FromEulerAngles(erot.Z, erot.Y, erot.X);
            }

            bone.update();
        }

        public void Read(FileData d)
        {
            d.endian = System.IO.Endianness.Big;
            d.Seek(4); // skip filesize
            int dataSize = d.ReadInt();
            int offsetTableCount = d.ReadInt();
            int rootA = d.ReadInt();
            int headerSize = 0x20;

            int rootOffset = headerSize + dataSize + offsetTableCount * 4;
            d.Seek(rootOffset);
            int figtree = d.ReadInt() + headerSize;
            d.Skip(4);
            String name = d.ReadString(d.Pos(), -1);
            this.Name = name;

            d.Seek(figtree);
            d.Skip(8);
            float frameCount = d.ReadFloat();
            int keyOffset = d.ReadInt();

            if(Debug)
                Console.WriteLine(name + "\tCount: " + frameCount);
            this.frameCount = (int)frameCount;

            int temp = d.Pos();

            d.Seek(keyOffset + 0x20);
            //int boneCount = 0x2E; // TODO: Use actual bone count 0x35
            List<int> keyFrameCount = new List<int>();
            int bid = d.ReadByte();
            while(bid != 0xFF)
            {
                keyFrameCount.Add(bid);
                bid = d.ReadByte();
                nodes.Add(new List<DATAnimTrack>());
            }
            int boneCount = keyFrameCount.Count;
            if (Debug)
                Console.WriteLine(boneCount);

            d.Seek(temp);

            int animDataOffset = 0;
            int[] trackcount = new int[offsetTableCount];
            for (int i = 0; i < offsetTableCount; i++)
            {
                trackcount[i] = d.ReadInt();
                if (i == 0)
                    animDataOffset = trackcount[i];
            }

            d.Seek(animDataOffset + 0x20);
            for (int i = 0; i < boneCount; i++)
            { // bonecount

                if(Debug)
                Console.WriteLine("Bone " + i + ": " + keyFrameCount[i] + "\t" + d.Pos().ToString("x"));

                for (int j = 0; j < keyFrameCount[i]; j++)
                {
                    int length = d.ReadShort();
                    d.Skip(2);
                    int trackType = d.ReadByte();
                    int valueFormat = d.ReadByte();
                    int tanFormat = d.ReadByte();
                    d.Skip(1);
                    int dataoff = d.ReadInt() + 0x20;

                    if (Debug)
                        Console.WriteLine((AnimType)trackType + "\tLength: " + length + "\tOffset: " + dataoff.ToString("x") + " " + valueFormat.ToString("x") + " " + tanFormat.ToString("x"));
                    // System.out.println(valueFormat + " " + tanFormat);
                    readKeyFrame(d, length, dataoff, valueFormat, tanFormat, keyFrameCount[i], i, trackType);
                }

            }
        }

        /*public void readStageAnim()
        {
            FileData d = new FileData("C:\\Users\\ploaj_000\\Desktop\\Melee\\GrIz.dat");
            d.Endian = System.IO.Endianness.Big;
            Debug = true;
            readKeyFrame(d, 8, 0x107300, 0x04, 0x00, 0x08, 0, 0x06);
        }*/

        public void readKeyFrame(FileData d, int length, int dataoff, int valueFormat, int tanFormat, int keyframeCount, int boneId, int trackType)
        {
            int temp = d.Pos();
            d.Seek(dataoff);

            if (Debug)
                Console.WriteLine("Start 0x" + d.Pos() + " " + keyframeCount);

            DATAnimTrack track = new DATAnimTrack();
            track.type = (AnimType)trackType;
            nodes[boneId].Add(track);

            while (d.Pos() < dataoff + length)
            {
                int type = readExtendedByte(d);

                int interpolation = ((type) & 0x0F);
                int numOfKey = ((type >> 4) & 0xFF) + 1;

                if (Debug)
                    Console.WriteLine("Interpolation type: " + (InterpolationType)interpolation + "\tnumofkey: " + numOfKey);
                
                for (int i = 0; i < numOfKey; i++)
                {
                    double value = -99, tan = -99;
                    int time = 0;
                    if (interpolation == 1)
                    { // step
                        value = readVal(d, valueFormat);
                        time = readExtendedByte(d);
                        if (Debug)
                            Console.WriteLine("\t" + value + "\t" + time);
                    }
                    if (interpolation == 2)
                    { // linear
                        value = readVal(d, valueFormat);
                        time = readExtendedByte(d);
                        //Console.WriteLine("\t" + value + "\t" + time);
                    }
                    if (interpolation == 3)
                    { // hermite
                        value = readVal(d, valueFormat);
                        tan = 0;
                        time = readExtendedByte(d);
                        if (Debug)
                            Console.WriteLine("\t" + value + "\t" + time);
                    }
                    if (interpolation == 4)
                    { // hermite
                        value = readVal(d, valueFormat);
                        tan = readVal(d, tanFormat);
                        time = readExtendedByte(d);
                        if (Debug)
                            Console.WriteLine("\t" + value + "\t" + tan + "\t" + time);
                    }
                    if (interpolation == 5)
                    { // hermite
                        tan = readVal(d, tanFormat);
                        if (Debug)
                            Console.WriteLine("\t" + "Tan" + tan);
                    }
                    if (interpolation == 6)
                    { // constant
                        value = readVal(d, valueFormat);
                        time = readExtendedByte(d);
                        //Console.WriteLine("\t" + value + "\t" + time);
                    }

                    KeyNode node = new KeyNode();
                    node.value = (float)value;
                    node.frame = time;
                    node.tan = (float)tan;
                    node.interpolationType = (InterpolationType)interpolation;
                    track.keys.Add(node);
                    //node.boneID = boneId;
                }
                d.endian = System.IO.Endianness.Big;

            }
            //Console.WriteLine("Ends at: " + (d.pos() + 8 - (d.pos() % 8)).ToString("x"));

            d.Seek(temp);
        }

        public static int readExtendedByte(FileData d)
        {
            int type = d.ReadByte(); 
            int i = type;
            if ((i & 0x80) != 0) // max 16 bit I think
            {
                i = d.ReadByte();
                type = (type & 0x7F) | (i << 7);
            }
            return type;
        }


        public static double readVal(FileData d, int valueFormat)
        {
            int scale = (int)Math.Pow(2, valueFormat & 0x1F);

            d.endian = System.IO.Endianness.Little;

            switch (valueFormat & 0xF0)
            {
                case 0x00:
                    return d.ReadFloat();
                case 0x20:
                    return d.ReadShort() / (double)scale;
                case 0x40:
                    return d.ReadUShort() / (double)scale;
                case 0x60:
                    return d.ReadSByte() / (double)scale;
                case 0x80:
                    return d.ReadByte() / (double)scale;
                default:
                    return 0;
            }
        }


        // temp stuff
        public static Dictionary<string, SkelAnimation> LoadAJ(string fname, VBN vbn)
        {
            // a note, I know that the main player file has the offsets for
            // animations, this is just for viewing
            FileData f = new FileData(fname);
            f.endian = System.IO.Endianness.Big;

            int pos = 0;

            Dictionary<string, SkelAnimation> animations = new Dictionary<string, SkelAnimation>();
            AnimationGroupNode group = new AnimationGroupNode() { Text = fname};
            MainForm.Instance.animList.treeView1.Nodes.Add(group);
            while (pos < f.Size())
            {
                Console.WriteLine(pos.ToString("x"));
                int len = f.ReadInt();
                DAT_Animation anim = new DAT_Animation();
                anim.Read(new FileData(f.GetSection(pos, len)));
                AnimTrack track = new AnimTrack(anim);

                if (pos == 0)
                {
                    //track.Show();
                }
                group.Nodes.Add(track.ToAnimation(vbn));
                SkelAnimation sa = track.BakeToSkel(vbn);
                //sa.Tag = track;
                //Runtime.Animations.Add(anim.Name, sa);
               // MainForm.Instance.animList.treeView1.Nodes.Add(anim.Name);
                animations.Add(anim.Name, sa);

                if (pos != 0)
                {
                    track.Dispose();
                    track.Close();
                }

                f.Skip(len - 4);
                f.Align(32);
                pos = f.Pos();
            }

            return animations;
        }

        public static Dictionary<string, Animation> GetTracks(string fname, VBN vbn)
        {
            // a note, I know that the main player file has the offsets for
            // animations, this is just for viewing
            FileData f = new FileData(fname);
            f.endian = System.IO.Endianness.Big;

            int pos = 0;

            Dictionary<string, Animation> animations = new Dictionary<string, Animation>();
            AnimationGroupNode group = new AnimationGroupNode() { Text = fname };
            MainForm.Instance.animList.treeView1.Nodes.Add(group);
            while (pos < f.Size())
            {
                Console.WriteLine(pos.ToString("x"));
                int len = f.ReadInt();
                DAT_Animation anim = new DAT_Animation();
                anim.Read(new FileData(f.GetSection(pos, len)));
                AnimTrack track = new AnimTrack(anim);

                if (pos == 0)
                {
                    //track.Show();
                }
                group.Nodes.Add(track.ToAnimation(vbn));
                //sa.Tag = track;
                //Runtime.Animations.Add(anim.Name, sa);
                // MainForm.Instance.animList.treeView1.Nodes.Add(anim.Name);
                animations.Add(anim.Name, track.ToAnimation(vbn));

                if (pos != 0)
                {
                    track.Dispose();
                    track.Close();
                }

                f.Skip(len - 4);
                f.Align(32);
                pos = f.Pos();
            }

            return animations;
        }


        // Writing

        public void Save(string fname)
        {
            FileOutput f = new FileOutput();
            f.endian = System.IO.Endianness.Big;

            // header
            FileOutput header = new FileOutput();
            header.endian = System.IO.Endianness.Big;

            FileOutput data = new FileOutput();
            data.endian = System.IO.Endianness.Big;


            f.Save(fname);
        }

    }
}
