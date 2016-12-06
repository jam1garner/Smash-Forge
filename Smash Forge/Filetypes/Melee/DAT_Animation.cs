using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Smash_Forge
{
    class DAT_Animation
    {
        public int frame = 0;

        //Dicionary<int, int> nodes = new Dicionary<int, int>();
        //Dictionary<int, KeyNode> nodes = new Dictionary<int, KeyNode>();
        List<KeyNode> f1node = new List<KeyNode>();
        
        public enum AnimType
        {
            NONE = 0,
            XROT = 1,
            YROT = 2,
            ZROT = 3,
            XPOS = 4,
            YPOS = 5,
            ZPOS = 6,
            XSCA = 7,
            YSCA = 8,
            ZSCA = 9,
            UNK = 10
        }

        public class KeyNode
        {
            public AnimType type;
            public int boneID;
            public float value;
            public float tan;
        }

        public DAT_Animation()
        {

        }

        public void Apply(VBN bone)
        {

            foreach(KeyNode node in f1node)
            {
                Bone b = bone.bones[node.boneID];

                Vector3 erot = ANIM.quattoeul(b.rot);

                Console.WriteLine(node.boneID + " " + node.type + " " + node.value);

                switch (node.type)
                {
                    case AnimType.XROT:
                        b.rot = VBN.FromEulerAngles(node.value, erot.Y, erot.Z);
                        break;
                    case AnimType.YROT:
                        b.rot = VBN.FromEulerAngles(erot.X, node.value, erot.Z);
                        break;
                    case AnimType.ZROT:
                        b.rot = VBN.FromEulerAngles(erot.X, erot.Y, node.value);
                        break;
                }
            }

            foreach(Bone b in bone.bones)
            {
                Console.WriteLine(new string(b.boneName));
                Console.WriteLine(b.rotation[0] + " " + b.rotation[1] + " " + b.rotation[2]);
                Console.WriteLine(b.position[0] + " " + b.position[1] + " " + b.position[2]);
            }

            bone.update();
        }

        public void Read(FileData d)
        {
            d.Endian = System.IO.Endianness.Big;
            d.seek(4); // skip filesize
            int dataSize = d.readInt();
            int offsetTableCount = d.readInt();
            int rootA = d.readInt();
            int headerSize = 0x20;

            int rootOffset = headerSize + dataSize + offsetTableCount * 4;
            d.seek(rootOffset);
            int figtree = d.readInt() + headerSize;
            d.skip(4);
            String name = d.readString(d.pos(), -1);

            d.seek(figtree);
            d.skip(8);
            float frameCount = d.readFloat();
            int keyOffset = d.readInt();

            Console.WriteLine(name + "\tCount: " + frameCount);

            int temp = d.pos();

            d.seek(keyOffset + 0x20);
            int boneCount = 0x35; // TODO: Use actual bone count
            int[] keyFrameCount = new int[boneCount];
            for (int i = 0; i < boneCount; i++)
                keyFrameCount[i] = d.readByte();

            d.seek(temp);

            int animDataOffset = 0;
            int[] trackcount = new int[offsetTableCount];
            for (int i = 0; i < offsetTableCount; i++)
            {
                trackcount[i] = d.readInt();
                if (i == 0)
                    animDataOffset = trackcount[i];
            }

            d.seek(animDataOffset + 0x20);
            for (int i = 0; i < boneCount; i++)
            { // bonecount

                Console.WriteLine("Bone " + i + ": " + keyFrameCount[i] + "\t" + d.pos().ToString("x"));

                for (int j = 0; j < keyFrameCount[i]; j++)
                {
                    int length = d.readShort();
                    d.skip(2);
                    int trackType = d.readByte();
                    int valueFormat = d.readByte();
                    int tanFormat = d.readByte();
                    d.skip(1);
                    int dataoff = d.readInt() + 0x20;

                    Console.WriteLine(AnimData.type[trackType] + "\tLength: " + length + "\tOffset: " + dataoff.ToString("x") + " " + valueFormat);
                    // System.out.println(valueFormat + " " + tanFormat);
                    readKeyFrame(d, length, dataoff, valueFormat, tanFormat, keyFrameCount[i], i, trackType);
                }

            }
        }

        public void readKeyFrame(FileData d, int length, int dataoff, int valueFormat, int tanFormat, int keyframeCount, int boneId, int trackType)
        {
            int temp = d.pos();
            d.seek(dataoff);

            List<int> types = new List<int>();
            List<float> values = new List<float>();
            List<float> tans = new List<float>();
            List<int> frames = new List<int>();

            Console.WriteLine("Start 0x" + d.pos() + " " + keyframeCount);

            while (d.pos() < dataoff + length)
            {
                int type = d.readByte();

                int interpolation = ((type) & 0x0F);
                int numOfKey = ((type >> 4) & 0x0F) + 1;

                Console.WriteLine("Interpolation type: " + interpolation + "\tnumofkey: " + numOfKey);
                float value = 0, tan = 0;
                int time = 0;
                for (int i = 0; i < numOfKey; i++)
                {
                    if (interpolation == 1)
                    { // step
                        value = readVal(d, valueFormat);
                        time = readExtendedByte(d);
                        Console.WriteLine("\t" + value + "\t" + time);
                    }
                    if (interpolation == 2)
                    { // linear
                        value = readVal(d, valueFormat);
                        time = readExtendedByte(d);
                        Console.WriteLine("\t" + value + "\t" + time);
                    }
                    if (interpolation == 3)
                    { // hermite
                        value = readVal(d, valueFormat);
                        time = readExtendedByte(d);
                        Console.WriteLine("\t" + value + "\t" + time);
                    }
                    if (interpolation == 4)
                    { // hermite
                        value = readVal(d, valueFormat);
                        tan = readVal(d, tanFormat);
                        time = readExtendedByte(d);
                        Console.WriteLine("\t" + value + "\t" + tan + "\t" + time);
                    }
                    if (interpolation == 5)
                    { // hermite
                        tan = readVal(d, tanFormat);
                        //time = readExtendedByte(d);
                        Console.WriteLine("\t" + "Tan" + tan + "\t" + time);
                    }
                    if (interpolation == 6)
                    { // constant
                        value = readVal(d, valueFormat);
                        time = readExtendedByte(d);
                        Console.WriteLine("\t" + value + "\t" + time);
                    }

                    types.Add(interpolation);
                    frames.Add(time);
                    tans.Add(tan);
                    values.Add(value);
                }
                d.Endian = System.IO.Endianness.Big;

            }
            Console.WriteLine("Ends at: " + (d.pos() + 8 - (d.pos() % 8)).ToString("x"));

            /*SkelAnimation.KeyNode node = anim.getNode(0, boneId);*/
            KeyNode node = new KeyNode();
            node.value = values[0];
            node.type = (AnimType)trackType;
            node.boneID = boneId;
            f1node.Add(node);

            /*switch (trackType)
            {
                case 1:
                    node.val.X = values[0];
                    node.type = 1;
                    break;
                case 2:
                    node.val.Y = values[0];
                    node.type = 1;
                    break;
                case 3:
                    node.val.Z = values.get(0);
                    node.type = 1;
                    break;
                case 4:
                    node.val.X = values.get(0);
                    node.type = 1;
                    break;
                case 5:
                    node.val.Y = values.get(0);
                    node.type = 1;
                    break;
                case 6:
                    node.val.Z = values.get(0);
                    node.type = 1;
                    break;
                default:
                    System.out.println("Unknown track type " + trackType);
            }*/

            d.seek(temp);
        }

        public static int readExtendedByte(FileData d)
        {
            int type = d.readByte(); //??
            int i = type;
            while ((i & 0x80) != 0)
            {
                i = d.readByte();
                type = (type & 0x7F) | (i << 7);
            }
            return type;
        }


        public static float readVal(FileData d, int valueFormat)
        {
            int scale = (int)Math.Pow(2, valueFormat & 0x1F);

            d.Endian = System.IO.Endianness.Little; // why?

            switch (valueFormat & 0xF0)
            {
                case 0x00:
                    return 0f + d.readFloat();
                case 0x20:
                    return 0f + (short)d.readShort() / (float)scale;
                case 0x40:
                    return 0f + d.readShort() / (float)scale;
                case 0x60:
                    return 0f + (byte)d.readByte() / (float)scale;
                case 0x80:
                    return 0f + d.readByte() / (float)scale;
                default:
                    return 0;
            }
        }

        public static class AnimData
        {
            public static String[] type = { "", "XRot", "YRot", "ZRot", "XTrans", "YTrans", "ZTrans", "XScale", "YScale", "ZScale", "Unknown" };
        }
    }
}
