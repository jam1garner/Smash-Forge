using System;
using System.IO;
using System.Diagnostics;
using OpenTK;

namespace SmashForge
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

            d.endian = Endianness.Big;
            d.Seek(8);
            int ver = d.ReadInt();
            d.Skip(4); //outer offset to brres

            int boneHeader = 0x40; // for version 9 only

            int dlist = d.ReadInt();
            int boneSec = d.ReadInt();
            int vertSec = d.ReadInt();
            int normSec = d.ReadInt();
            int colrSec = d.ReadInt();
            int texcSec = d.ReadInt();
            d.Skip(8);
            int polySec = d.ReadInt();

            d.Seek(0x40);
            d.Skip(16);
            int vertCount = d.ReadInt();
            int faceCount = d.ReadInt();
            d.Skip(4);
            int boneCount = d.ReadInt();
            v.totalBoneCount = (uint)boneCount;
            for (int i = 0; i < 3; i++)
                v.boneCountPerType[i + 1] = 0;
            d.Skip(4);
            int bonetableoff = d.ReadInt() + boneHeader;

            d.Seek(bonetableoff);
            int bcount = d.ReadInt();
            int[] nodeIndex = new int[bcount];
            for (int i = 0; i < bcount; i++)
            {
                nodeIndex[i] = d.ReadInt();
            }

            Random rng = new Random();
            uint boneID = (uint)rng.Next(1, 0xFFFFFF);

            // BONES-----------------------------------------------
            d.Seek(boneSec);
            d.Skip(4); // length
            int bseccount = d.ReadInt();
            for (int i = 0; i < bseccount; i++)
            {
                Debug.Write(i);
                d.Skip(4); // entry id and unknown
                d.Skip(4); // left and right index
                int name = d.ReadInt() + boneSec;
                int data = d.ReadInt() + boneSec;

                int temp = d.Pos();
                if (name != boneSec && data != boneSec)
                {
                    // read bone data
                    d.Seek(data);
                    d.Skip(8);
                    int nameOff = d.ReadInt() + data;
                    int index = d.ReadInt(); // id
                    d.Skip(4); // index
                    d.Skip(8); // idk billboard settings and padding
                    Bone n = new Bone(v);
                    
                    n.scale = new float[3];
                    n.position = new float[3];
					n.rotation = new float[3];
					d.Skip(4); // index

                    n.scale[0] = d.ReadFloat();
                    n.scale[1] = d.ReadFloat();
                    n.scale[2] = d.ReadFloat();
                    n.rotation[0] = toRadians(d.ReadFloat());
                    n.rotation[1] = toRadians(d.ReadFloat());
                    n.rotation[2] = toRadians(d.ReadFloat());
                    n.position[0] = d.ReadFloat();
                    n.position[1] = d.ReadFloat();
					n.position[2] = d.ReadFloat();

					n.pos = new Vector3 (n.position[0], n.position[1], n.position[2]);
					n.sca = new Vector3 (n.scale[0], n.scale[1], n.scale[2]);
					n.rot = (VBN.FromEulerAngles (n.rotation [2], n.rotation [1], n.rotation [0]));

                    d.Skip(24);

                    d.Seek(data + 0x5C);
                    d.Seek(d.ReadInt() + data + 12);
                    int parentid = 0x0FFFFFFF;
                    if (d.Pos() != data + 12)
                        parentid = d.ReadInt();
                    n.parentIndex = (int)parentid;

                    n.Text = d.ReadString(nameOff, -1);
                    n.boneId = boneID;
                    boneID++;

                    v.bones.Add(n);
                }
                else
                    bseccount++;

                d.Seek(temp);
            }
			v.update ();
            //v.updateChildren();
            v.boneCountPerType[0] = (uint)v.bones.Count;

            return v;
        }
    }
}
