using System;
using System.Diagnostics;

namespace VBN_Editor
{
    public class MDL0Bones
    {

        private float toRadians(float a)
        {
            return a * ((float)Math.PI / 180f);
        }

        public MDL0Bones() { }

        public VBN GetVBN(FileData d)
        {

            VBN v = new VBN();

            d.littleEndian = false;
            d.seek(8);
            int ver = d.readInt();
            d.skip(4); //outer offset to brres

            int boneHeader = 0x40; // for version 9 only

            int dlist = d.readInt();
            int boneSec = d.readInt();
            int vertSec = d.readInt();
            int normSec = d.readInt();
            int colrSec = d.readInt();
            int texcSec = d.readInt();
            d.skip(8);
            int polySec = d.readInt();

            d.seek(0x40);
            d.skip(16);
            int vertCount = d.readInt();
            int faceCount = d.readInt();
            d.skip(4);
            int boneCount = d.readInt();
            v.totalBoneCount = (uint)boneCount;
            v.boneCountPerType[0] = (uint)boneCount;
            for (int i = 0; i < 3; i++)
                v.boneCountPerType[i + 1] = 0;
            d.skip(4);
            int bonetableoff = d.readInt() + boneHeader;

            d.seek(bonetableoff);
            int bcount = d.readInt();
            int[] nodeIndex = new int[bcount];
            for (int i = 0; i < bcount; i++)
            {
                nodeIndex[i] = d.readInt();
            }

            Random rng = new Random();
            uint boneID = (uint)rng.Next(1, 0xFFFFFF);

            // BONES-----------------------------------------------
            d.seek(boneSec);
            d.skip(4); // length
            int bseccount = d.readInt();
            for (int i = 0; i < bseccount; i++)
            {
                Debug.Write(i);
                d.skip(4); // entry id and unknown
                d.skip(4); // left and right index
                int name = d.readInt() + boneSec;
                int data = d.readInt() + boneSec;

                int temp = d.pos();
                if (name != boneSec && data != boneSec)
                {
                    // read bone data
                    d.seek(data);
                    d.skip(8);
                    int nameOff = d.readInt() + data;
                    int index = d.readInt(); // id
                    d.skip(4); // index
                    d.skip(8); // idk billboard settings and padding
                    Bone n = new Bone();
                    
                    n.scale = new float[3];
                    n.position = new float[3];
                    n.rotation = new float[3];
                    n.scale[0] = d.readFloat();
                    n.scale[1] = d.readFloat();
                    n.scale[2] = d.readFloat();
                    n.rotation[0] = toRadians(d.readFloat());
                    n.rotation[1] = toRadians(d.readFloat());
                    n.rotation[2] = toRadians(d.readFloat());
                    n.position[0] = d.readFloat();
                    n.position[1] = d.readFloat();
                    n.position[2] = d.readFloat();

                    d.skip(24);

                    d.seek(data + 0x5C);
                    d.seek(d.readInt() + data + 12);
                    int parentid = 0x0FFFFFFF;
                    if (d.pos() != data + 12)
                        parentid = d.readInt();
                    n.parentIndex = (int)parentid;

                    n.boneName = d.readString(nameOff, -1).ToCharArray();
                    n.boneId = boneID;
                    boneID++;

                    v.bones.Add(n);
                }
                else
                    bseccount++;

                d.seek(temp);
            }
            v.updateChildren();

            return v;
        }
    }
}