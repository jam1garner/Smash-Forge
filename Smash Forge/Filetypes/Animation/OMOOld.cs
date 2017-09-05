using System;
using System.Collections.Generic;
using OpenTK;
using System.IO;

namespace Smash_Forge
{
    public class OMOOld
    {
        public static int flagsused = 0;

        // For a specific type of rotation data
        public static float epsilon = 1.0e-12f;
        public static float rot6CalculateW(float x, float y, float z)
        {
            // float cumulative = (float)Math.Sqrt(Math.Abs(1 - (x * x + y * y + z * z)));
            float cumulative = 1 - (x * x + y * y + z * z);
            float f12 = (float)(1 / Math.Sqrt((double)cumulative));
            float sqrt_cumulative = (cumulative - epsilon) < 0 ? 0f : f12;

            float f7 = (0.5f * cumulative) * sqrt_cumulative;
            float f8 = 1.5f - (f7 * sqrt_cumulative);
            float f0 = f8 * sqrt_cumulative;
            float f9 = (0.5f * cumulative) * f0;
            float f10 = 1.5f - (f9 * f0);
            f0 = f0 * f10;
            float f11 = (0.5f * cumulative) * f0;
            float f13 = 1.5f - (f11 * f0);
            f0 = f0 * f13;
            f7 = cumulative * f0;

            return f7;
        }


        // For a specific type of rotation data
        public static float scale1 = 1 / (float)Math.Sqrt(2f);
        public static float scale2 = (scale1 * 2) / 1048575f;
        public static float encodedRot10ToQuaternionComponent(float toConvert)
        {
            return (toConvert * scale2) - scale1;
        }

        public static SkelAnimation read(FileData d)
        {

            d.Endian = Endianness.Big;

            d.skip(4); // header OMO
            d.skip(4); // two shorts, idk

            d.skip(4); //flags?

            d.skip(2);
            int boneCount = d.readShort();
            int frameCount = d.readShort();
            int frameSize = d.readShort();

            int offset1 = d.readInt();  // nodeOffset
            int offset2 = d.readInt();  // interOffset
            int offset3 = d.readInt();  // keyOffset

            SkelAnimation anim = new SkelAnimation();
            anim.Tag = d;
            //anim.setModel(m);

            // base frames
            // These are linked to bones via hashes
            d.seek(offset1); // 
            int[] framekey = new int[boneCount];
            KeyNode[] baseNode = new KeyNode[boneCount];

            // Start positions for bones/nodes
            for (int i = 0; i < boneCount; i++)
            {
                flagsused = flagsused | d.readInt(); d.seek(d.pos() - 4);
                //Console.WriteLine(flagsused.ToString("x"));

                int flags = d.readByte();
                int tFlag = d.readByte();
                int rFlag = d.readByte();
                int sFlag = d.readByte();
                int hash = d.readInt(); // used to find the identifying bone
                int off1 = d.readInt() + offset2;
                framekey[i] = d.readInt();

                bool hasTrans = (flags & 0x01) == 0x01;
                bool hasScale = (flags & 0x04) == 0x04;
                bool hasRot = (flags & 0x02) == 0x02;

                KeyNode node = new KeyNode();
                baseNode[i] = node;

                node.hash = (uint)hash;

                int temp = d.pos();
                d.seek(off1);

                if (hasTrans)
                {
                    if (tFlag == 0x8)
                    { // interpolated
                        node.t_type = KeyNode.INTERPOLATED;
                        node.t = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                        node.t2 = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                    }
                    else if (tFlag == 0x20)
                    {
                        node.t_type = KeyNode.CONSTANT;
                        node.t = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                    }
                    else if (tFlag == 0x4)
                    {
                        // entire Vector3 provided in keyframe data i.e. KEYFRAME type
                        node.t_type = KeyNode.KEYFRAME;
                    }
                }
                if (hasRot)
                {
                    if ((rFlag & 0xF0) != 0x50 && (rFlag & 0xF0) != 0x70 && (rFlag & 0xF0) != 0x60 && (rFlag & 0xF0) != 0xA0)
                    {
                        //Console.WriteLine(rFlag);
                    }

                    if ((rFlag & 0xF0) == 0xA0)
                    {
                        // All data is in the keyframe for this type
                        node.r_type = KeyNode.COMPRESSED;
                    }

                    if ((rFlag & 0xF0) == 0x50)
                    { // interpolated
                        node.r_type = KeyNode.INTERPOLATED;
                        node.rv = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                        node.rv2 = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                    }

                    if ((rFlag & 0xF0) == 0x60)
                    {
                        node.r_type = KeyNode.KEYFRAME;
                        node.rv = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                        node.r_extra = d.readFloat() / 65535;
                    }

                    if ((rFlag & 0xF0) == 0x70)
                    {
                        node.r_type = KeyNode.CONSTANT;
                        node.rv = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                    }
                }

                if (hasScale)
                {
                    if ((sFlag & 0xF0) == 0x80)
                    { // interpolated
                        node.s_type = KeyNode.INTERPOLATED;
                        node.s = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                        node.s2 = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                    }
                    // TODO: investigate the difference between these
                    if ((rFlag & 0x0F) == 0x02 || (rFlag & 0x0F) == 0x03)
                    { // constant
                        node.s_type = KeyNode.CONSTANT;
                        node.s = new Vector3(d.readFloat(), d.readFloat(), d.readFloat());
                    }
                }
                d.seek(temp);
            }

            // Interpolated type below here is always a set translation/rotation/scale
            // from the coords specified with the bone node above
            d.seek(offset3);
            for (int i = 0; i < frameCount; i++)
            {
                KeyFrame key = new KeyFrame();
                key.frame = i;

                int off = d.pos();

                for (int j = 0; j < boneCount; j++)
                {
                    KeyNode node = new KeyNode();

                    node.t_type = baseNode[j].t_type;
                    node.r_type = baseNode[j].r_type;
                    node.s_type = baseNode[j].s_type;

                    node.id = baseNode[j].id;
                    node.hash = baseNode[j].hash;

                    d.seek(off + framekey[j]);

                    if (baseNode[j].t_type == KeyNode.INTERPOLATED)
                    {
                        float i1 = ((float)d.readShort() / 0xffff);
                        float i2 = ((float)d.readShort() / 0xffff);
                        float i3 = ((float)d.readShort() / 0xffff);

                        float x = baseNode[j].t.X + (baseNode[j].t2.X * (i1));
                        float y = baseNode[j].t.Y + (baseNode[j].t2.Y * (i2));
                        float z = baseNode[j].t.Z + (baseNode[j].t2.Z * (i3));

                        node.t = new Vector3(x, y, z);  // Translation
                    }
                    else if (baseNode[j].t_type == KeyNode.CONSTANT)
                    {
                        node.t = baseNode[j].t;
                    }
                    else if (baseNode[j].t_type == 2)
                    {
                        float x = d.readFloat();
                        float y = d.readFloat();
                        float z = d.readFloat();

                        node.t = new Vector3(x, y, z);
                    }

                    if (baseNode[j].r_type == KeyNode.COMPRESSED)
                    {
                        // There are 64 bits present for each node of this rot type
                        // The format is: 20bits * 3 of encoded floats, and 4 bits of flags
                        // The flags describe which 3 components of the quaternion are being presented
                        int b1 = d.readByte();
                        int b2 = d.readByte();
                        int b3 = d.readByte();
                        int b4 = d.readByte();
                        int b5 = d.readByte();
                        int b6 = d.readByte();
                        int b7 = d.readByte();
                        int b8 = d.readByte();

                        // Capture 20 bits at a time of the raw data
                        int f1 = (b1 << 12) | (b2 << 4) | ((b3 & 0xF0) >> 4);
                        int f2 = ((b3 & 0xF) << 16) | (b4 << 8) | b5;
                        int f3 = (b6 << 12) | (b7 << 4) | ((b8 & 0xF0) >> 4);
                        int flags = b8 & 0xF;

                        node.r = new Quaternion();
                        switch (flags)
                        {
                            case 0:  // y, z, w provided
                                node.r.Y = encodedRot10ToQuaternionComponent(f1);
                                node.r.Z = encodedRot10ToQuaternionComponent(f2);
                                node.r.W = encodedRot10ToQuaternionComponent(f3);

                                node.r.X = (float)Math.Sqrt(Math.Abs(1 - (node.r.Y * node.r.Y + node.r.Z * node.r.Z + node.r.W * node.r.W)));
                                break;
                            case 1:  // x, z, w provided
                                node.r.X = encodedRot10ToQuaternionComponent(f1);
                                node.r.Z = encodedRot10ToQuaternionComponent(f2);
                                node.r.W = encodedRot10ToQuaternionComponent(f3);

                                node.r.Y = (float)Math.Sqrt(Math.Abs(1 - (node.r.X * node.r.X + node.r.Z * node.r.Z + node.r.W * node.r.W)));
                                break;
                            case 2:  // x, y, w provided
                                node.r.X = encodedRot10ToQuaternionComponent(f1);
                                node.r.Y = encodedRot10ToQuaternionComponent(f2);
                                node.r.W = encodedRot10ToQuaternionComponent(f3);

                                node.r.Z = (float)Math.Sqrt(Math.Abs(1 - (node.r.X * node.r.X + node.r.Y * node.r.Y + node.r.W * node.r.W)));
                                break;
                            case 3:  // x, y, z, provided
                                node.r.X = encodedRot10ToQuaternionComponent(f1);
                                node.r.Y = encodedRot10ToQuaternionComponent(f2);
                                node.r.Z = encodedRot10ToQuaternionComponent(f3);

                                node.r.W = (float)Math.Sqrt(Math.Abs(1 - (node.r.X * node.r.X + node.r.Y * node.r.Y + node.r.Z * node.r.Z)));
                                break;
                            default:
                                Console.WriteLine("Unknown rotation type3 flags: " + flags);
                                break;
                        }
                    }
                    else if (baseNode[j].r_type == KeyNode.INTERPOLATED)
                    {
                        float i1 = ((float)d.readShort() / (0xffff));
                        float i2 = ((float)d.readShort() / (0xffff));
                        float i3 = ((float)d.readShort() / (0xffff));

                        float x = baseNode[j].rv.X + (baseNode[j].rv2.X * (i1));
                        float y = baseNode[j].rv.Y + (baseNode[j].rv2.Y * (i2));
                        float z = baseNode[j].rv.Z + (baseNode[j].rv2.Z * (i3));

                        float w = (float)Math.Sqrt(Math.Abs(1 - (x * x + y * y + z * z)));

                        node.r = new Quaternion(new Vector3(x, y, z), w);
                        node.r.Normalize();
                    }
                    else if (baseNode[j].r_type == KeyNode.KEYFRAME)
                    {
                        float scale = d.readShort() * baseNode[j].r_extra;
                        float x = baseNode[j].rv.X;
                        float y = baseNode[j].rv.Y;
                        float z = baseNode[j].rv.Z + scale;
                        float w = rot6CalculateW(x, y, z);

                        node.r = new Quaternion(x, y, z, w);
                    }
                    else
                    {
                        float x = baseNode[j].rv.X;
                        float y = baseNode[j].rv.Y;
                        float z = baseNode[j].rv.Z;
                        float w = (float)Math.Sqrt(Math.Abs(1 - (x * x + y * y + z * z)));

                        node.r = new Quaternion(baseNode[j].rv, w);
                        node.r.Normalize();
                    }

                    if (baseNode[j].s_type == KeyNode.INTERPOLATED)
                    {
                        float i1 = ((float)d.readShort() / (0xffff));
                        float i2 = ((float)d.readShort() / (0xffff));
                        float i3 = ((float)d.readShort() / (0xffff));

                        float x = baseNode[j].s.X + (baseNode[j].s2.X * (i1));
                        float y = baseNode[j].s.Y + (baseNode[j].s2.Y * (i2));
                        float z = baseNode[j].s.Z + (baseNode[j].s2.Z * (i3));

                        node.s = new Vector3(x, y, z);
                    }
                    else
                    {
                        node.s = baseNode[j].s;
                    }

                    key.addNode(node);
                }
                d.seek(off + frameSize);
                
                anim.addKeyframe(key);
            }

            return anim;
        }

        public static Bone getNodeId(VBN vbn, int node)
        {
            return vbn.bones[node];
        }

        public static byte[] createOMO(SkelAnimation a, VBN vbn)
        {
            List<int> nodeid = a.getNodes(true, vbn);

            int startNode = 0;
            int sizeNode = nodeid.Count;

            FileOutput o = new FileOutput();
            o.Endian = Endianness.Big;

            FileOutput t1 = new FileOutput();
            t1.Endian = Endianness.Big;

            FileOutput t2 = new FileOutput();
            t2.Endian = Endianness.Big;

            o.writeString("OMO ");
            o.writeShort(1); //idk
            o.writeShort(3);//idk

            o.writeInt(0x091E100C); //flags??


            o.writeShort(0); //padding
            o.writeShort(sizeNode); // numOfNodes

            o.writeShort(a.frames.Count); // frame size
            o.writeShort(0); // frame start ??

            o.writeInt(0);
            o.writeInt(0);
            o.writeInt(0);

            o.writeIntAt(o.size(), 0x14);


            // ASSESSMENT
            KeyNode[] minmax = new KeyNode[sizeNode];
            bool[] hasScale = new bool[sizeNode];
            bool[] hasTrans = new bool[sizeNode];
            bool[] hasRot = new bool[sizeNode];

            bool[] conScale = new bool[sizeNode];
            bool[] conTrans = new bool[sizeNode];
            bool[] conRot = new bool[sizeNode];

            a.setFrame(0);

            for (int i = 0; i < a.size(); i++)
            {
                a.nextFrame(vbn);

                for (int j = 0; j < nodeid.Count; j++)
                {
                    Bone node = getNodeId(vbn, nodeid[j]);

                    if (minmax[j] == null)
                    {

                        hasRot[j] = false;
                        hasScale[j] = false;
                        hasTrans[j] = false;

                        KeyNode n = a.getFirstNode(nodeid[j]);
                        if (n != null)
                        {
                            if (n.r_type != -1)
                                hasRot[j] = true;
                            if (n.t_type != -1)
                                hasTrans[j] = true;
                            if (n.s_type != -1)
                                hasScale[j] = true;
                        }

                        minmax[j] = new KeyNode();
                        minmax[j].t = new Vector3(999f, 999f, 999f);
                        minmax[j].r = new Quaternion(999f, 999f, 999f, 999f);
                        minmax[j].s = new Vector3(999f, 999f, 999f);
                        minmax[j].t2 = new Vector3(-999f, -999f, -999f);
                        minmax[j].r2 = new Quaternion(-999f, -999f, -999f, -999f);
                        minmax[j].s2 = new Vector3(-999f, -999f, -999f);
                    }

                    if (node.pos.X < minmax[j].t.X)
                        minmax[j].t.X = node.pos.X;
                    if (node.pos.X > minmax[j].t2.X)
                        minmax[j].t2.X = node.pos.X;

                    if (node.pos.Y < minmax[j].t.Y)
                        minmax[j].t.Y = node.pos.Y;
                    if (node.pos.Y > minmax[j].t2.Y)
                        minmax[j].t2.Y = node.pos.Y;

                    if (node.pos.Z < minmax[j].t.Z)
                        minmax[j].t.Z = node.pos.Z;
                    if (node.pos.Z > minmax[j].t2.Z)
                        minmax[j].t2.Z = node.pos.Z;

                    //				float[] fix = Node.fix360(node.nrx, node.nry, node.nrz);
                    //float[] f = Bone.CalculateRotation(node.nrx, node.nry, node.nrz);
                    Quaternion r = node.rot;

                    if (r.X < minmax[j].r.X)
                        minmax[j].r.X = r.X;
                    if (r.X > minmax[j].r2.X)
                        minmax[j].r2.X = r.X;

                    if (r.Y < minmax[j].r.Y)
                        minmax[j].r.Y = r.Y;
                    if (r.Y > minmax[j].r2.Y)
                        minmax[j].r2.Y = r.Y;

                    if (r.Z < minmax[j].r.Z)
                        minmax[j].r.Z = r.Z;
                    if (r.Z > minmax[j].r2.Z)
                        minmax[j].r2.Z = r.Z;


                    if (node.sca.X < minmax[j].s.X)
                        minmax[j].s.X = node.sca.X;
                    if (node.sca.X > minmax[j].s2.X)
                        minmax[j].s2.X = node.sca.X;

                    if (node.sca.Y < minmax[j].s.Y)
                        minmax[j].s.Y = node.sca.Y;
                    if (node.sca.Y > minmax[j].s2.Y)
                        minmax[j].s2.Y = node.sca.Y;

                    if (node.sca.Z < minmax[j].s.Z)
                        minmax[j].s.Z = node.sca.Z;
                    if (node.sca.Z > minmax[j].s2.Z)
                        minmax[j].s2.Z = node.sca.Z;

                }
            }

            // NODE INFO

            int t2Size = 0;
            for (int i = 0; i < sizeNode; i++)
            {

                int flag = 0;

                conRot[i] = false;
                conScale[i] = false;
                conTrans[i] = false;

                // check for constant
                if (minmax[i].t.Equals(minmax[i].t2))
                    conTrans[i] = true;
                if (minmax[i].r.Equals(minmax[i].r2))
                    conRot[i] = true;
                if (minmax[i].s.Equals(minmax[i].s2))
                    conScale[i] = true;

                if (hasTrans[i])
                    flag |= 0x01000000;
                if (hasRot[i])
                    flag |= 0x02000000;
                if (hasScale[i])
                    flag |= 0x04000000;

                if (conTrans[i] && hasTrans[i])
                    flag |= 0x00200000;
                else
                    flag |= 0x00080000;

                if (conRot[i] && hasRot[i])
                    flag |= 0x00007000;
                else
                    flag |= 0x00005000;

                if (conScale[i] && hasScale[i])
                    flag |= 0x00000200;
                else
                    flag |= 0x00000080;

                flag |= 0x00000001;

                int hash = (int)getNodeId(vbn, nodeid[i]).boneId;
                //if(hash == -1)
                //hash = (int)FileData.crc32(getNodeId(nodeid.get(i)).name);
                o.writeInt(flag); // flags...
                o.writeInt(hash); //hash
                o.writeInt(t1.size()); // Offset in 1 table
                o.writeInt(t2Size); // Offset in 2 table

                // calculate size needed
                if (hasTrans[i])
                {
                    t1.writeFloat(minmax[i].t.X);
                    t1.writeFloat(minmax[i].t.Y);
                    t1.writeFloat(minmax[i].t.Z);

                    if (!conTrans[i])
                    {
                        minmax[i].t2.X -= minmax[i].t.X;
                        minmax[i].t2.Y -= minmax[i].t.Y;
                        minmax[i].t2.Z -= minmax[i].t.Z;

                        t1.writeFloat(minmax[i].t2.X);
                        t1.writeFloat(minmax[i].t2.Y);
                        t1.writeFloat(minmax[i].t2.Z);

                        t2Size += 6;
                    }
                }

                if (hasRot[i])
                {
                    t1.writeFloat(minmax[i].r.X);
                    t1.writeFloat(minmax[i].r.Y);
                    t1.writeFloat(minmax[i].r.Z);

                    if (!conRot[i])
                    {
                        minmax[i].r2.X -= minmax[i].r.X;
                        minmax[i].r2.Y -= minmax[i].r.Y;
                        minmax[i].r2.Z -= minmax[i].r.Z;

                        t1.writeFloat(minmax[i].r2.X);
                        t1.writeFloat(minmax[i].r2.Y);
                        t1.writeFloat(minmax[i].r2.Z);

                        t2Size += 6;
                    }
                }

                if (hasScale[i])
                {
                    t1.writeFloat(minmax[i].s.X);
                    t1.writeFloat(minmax[i].s.Y);
                    t1.writeFloat(minmax[i].s.Z);

                    if (!conScale[i])
                    {
                        minmax[i].s2.X -= minmax[i].s.X;
                        minmax[i].s2.Y -= minmax[i].s.Y;
                        minmax[i].s2.Z -= minmax[i].s.Z;

                        t1.writeFloat(minmax[i].s2.X);
                        t1.writeFloat(minmax[i].s2.Y);
                        t1.writeFloat(minmax[i].s2.Z);

                        t2Size += 6;
                    }
                }
            }

            o.writeIntAt(o.size(), 0x18);

            o.writeOutput(t1);

            o.writeIntAt(o.size(), 0x1C);

            // INTERPOLATION

            a.setFrame(0);

            for (int i = 0; i < a.size(); i++)
            {
                a.nextFrame(vbn);
                for (int j = 0; j < nodeid.Count; j++)
                {
                    Bone node = getNodeId(vbn, nodeid[j]);

                    if (hasTrans[j] && !conTrans[j])
                    {
                        t2.writeShort((int)(((node.pos.X - minmax[j].t.X) / minmax[j].t2.X) * 0xFFFF));
                        t2.writeShort((int)(((node.pos.Y - minmax[j].t.Y) / minmax[j].t2.Y) * 0xFFFF));
                        t2.writeShort((int)(((node.pos.Z - minmax[j].t.Z) / minmax[j].t2.Z) * 0xFFFF));
                    }

                    if (hasRot[j] && !conRot[j])
                    {
                        //					float[] fix = Node.fix360(node.nrx, node.nry, node.nrz);
                        //float[] f = CalculateRotation(node.nrx, node.nry, node.nrz);
                        Quaternion r = node.rot;

                        t2.writeShort((int)(((r.X - minmax[j].r.X) / minmax[j].r2.X) * 0xFFFF));
                        t2.writeShort((int)(((r.Y - minmax[j].r.Y) / minmax[j].r2.Y) * 0xFFFF));
                        t2.writeShort((int)(((r.Z - minmax[j].r.Z) / minmax[j].r2.Z) * 0xFFFF));
                    }

                    if (hasScale[j] && !conScale[j])
                    {
                        t2.writeShort((int)(((node.sca.X - minmax[j].s.X) / minmax[j].s2.X) * 0xFFFF));
                        t2.writeShort((int)(((node.sca.Y - minmax[j].s.Y) / minmax[j].s2.Y) * 0xFFFF));
                        t2.writeShort((int)(((node.sca.Z - minmax[j].s.Z) / minmax[j].s2.Z) * 0xFFFF));
                    }
                }

                if (i == 0)
                {
                    o.writeShortAt(t2.size(), 0x12);
                }
            }

            o.writeOutput(t2);
            return o.getBytes();
        }
        public static void createOMO(SkelAnimation a, VBN vbn, String fname)
        {
            File.WriteAllBytes(fname, createOMO(a, vbn));
        }
    }
}

