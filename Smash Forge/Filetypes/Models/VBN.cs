using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge
{
    public class Bone
    {
        public char[] boneName;
        public UInt32 boneType;

        public int parentIndex
        {
            set
            {
                if (value == 268435455)
                {
                    ParentBone = null;
                    return;
                }
                ParentBone = vbnParent.bones[value];
            }

            get
            {
                if (ParentBone == null)
                    return -1;
                return vbnParent.bones.IndexOf(ParentBone);
            }
        }

        public Bone ParentBone;
        public UInt32 boneId;
        public float[] position;
        public float[] rotation;
        public float[] scale;
        public VBN vbnParent;
        //public List<int> children;

        public Bone(VBN v)
        {
            vbnParent = v;
        }

        public List<Bone> GetChildren()
        {
            List<Bone> l = new List<Bone>();
            foreach (Bone b in vbnParent.bones)
                if(b.ParentBone == this)
                    l.Add(b);
            return l;
        }

        public override string ToString()
        {
            return new string(boneName).TrimEnd( (char)0 );
        }

        public Vector3 pos = Vector3.Zero, sca = new Vector3(1f, 1f, 1f);
        public Quaternion rot = Quaternion.FromMatrix(Matrix3.Zero);
        public Matrix4 transform, invert;
    }

    public class HelperBone
    {
        public void Read(FileData f)
        {
            f.Endian = Endianness.Little;
            f.seek(4);
            int count = f.readInt();
            f.skip(12);
            int dataCount = f.readInt();
            int boneCount = f.readInt();
            int hashCount = f.readInt();
            int hashOffset = f.readInt() + 0x28;
            f.skip(4);

            Console.WriteLine("Count " + count);

            for (int i = 0; i < dataCount; i++)
            {
                Console.WriteLine("Bone " + i + " start at " + f.pos().ToString("x"));
                // 3 sections
                int secLength = f.readInt();
                int someCount = f.readInt(); // usually 2?

                int size1 = f.readInt();
                Console.Write(size1 + "\t");
                for (int j = 0; j < (size1 / 4) - 1; j++)
                    Console.Write(f.readShort() + " " + f.readShort() + "\t");
                Console.WriteLine();

                int size2 = f.readInt();
                Console.Write(size2 + "\t");
                for (int j = 0; j < (size2 / 4) - 1; j++)
                    Console.Write(f.readShort() + " " + f.readShort() + "\t");
                Console.WriteLine();

                int size3 = f.readInt();
                Console.Write(size3 + "\t");
                for (int j = 0; j < (size3 / 4) - 1; j++)
                    Console.Write(f.readShort() + " " + f.readShort() + "\t");
                Console.WriteLine();

                int size4 = f.readInt();
                Console.Write(size4 + "\t");
                for (int j = 0; j < (size4 / 4) - 1; j++)
                    Console.Write(f.readShort() + " " + f.readShort() + "\t");
                Console.WriteLine();

                int size5 = f.readInt();
                Console.Write(size5 + "\t");
                for (int j = 0; j < (size5 / 4) - 1; j++)
                    Console.Write(f.readShort() + " " + f.readShort() + "\t");
                Console.WriteLine();

                f.skip(8);
            }

            Console.WriteLine("0x" + f.pos().ToString("X"));
            f.skip(8);
            int hashSize = f.readInt();
            int unk = f.readInt();

            for (int i = 0; i < hashCount; i++)
            {
                Console.WriteLine(f.readInt().ToString("X"));
            }
        }
    }

    public class VBN : FileBase
    {
        public VBN() { }
        public VBN(string filename)
        {
            Read(filename);
        }

        public override Endianness Endian { get; set; }

        public Int16 unk_1 = 2, unk_2 = 1;
        public UInt32 totalBoneCount;
        public UInt32[] boneCountPerType = new UInt32[4];
        public List<Bone> bones = new List<Bone>();

        public List<List<int>> jointTable = new List<List<int>>();
        public SB swingBones = new SB();

        public List<Bone> getBoneTreeOrderol()
        {
            List<Bone> bone = new List<Bone>();
            Queue<Bone> q = new Queue<Bone>();

            q.Enqueue(bones[0]);

            while (q.Count > 0)
            {
                Bone b = q.Dequeue();
                foreach (Bone bo in b.GetChildren())
                    q.Enqueue(bo);
                bone.Add(b);
            }
            return bone;
        }

        public List<Bone> getBoneTreeOrder()
        {
            List<Bone> bone = new List<Bone>();
            Queue<Bone> q = new Queue<Bone>();

            queueBones(bones[0], q);

            while (q.Count > 0)
            {
                bone.Add(q.Dequeue());
            }
            return bone;
        }

        public void queueBones(Bone b, Queue<Bone> q)
        {
            q.Enqueue(b);
            foreach (Bone c in b.GetChildren())
                queueBones(c, q);
        }

        public static Quaternion FromEulerAngles(float z, float y, float x)
        {
            {
                Quaternion xRotation = Quaternion.FromAxisAngle(Vector3.UnitX, x);
                Quaternion yRotation = Quaternion.FromAxisAngle(Vector3.UnitY, y);
                Quaternion zRotation = Quaternion.FromAxisAngle(Vector3.UnitZ, z);

                Quaternion q = (zRotation * yRotation * xRotation);

                if (q.W < 0)
                    q *= -1;

                //return xRotation * yRotation * zRotation;
                return q;
            }
        }

        public void update()
        {
            for (int i = 0; i < bones.Count; i++)
            {
                bones[i].transform = Matrix4.CreateScale(bones[i].sca) * Matrix4.CreateFromQuaternion(bones[i].rot) * Matrix4.CreateTranslation(bones[i].pos);
                if (bones[i].parentIndex != 0x0FFFFFFF && bones[i].parentIndex != -1)
                {
                    bones[i].transform = bones[i].transform * bones[(int)bones[i].parentIndex].transform;
                }
            }
        }
        public void reset()
        {
            for (int i = 0; i < bones.Count; i++)
            {
                bones[i].pos = new Vector3(bones[i].position[0], bones[i].position[1], bones[i].position[2]);
                bones[i].rot = (FromEulerAngles(bones[i].rotation[2], bones[i].rotation[1], bones[i].rotation[0]));
                bones[i].sca = new Vector3(bones[i].scale[0], bones[i].scale[1], bones[i].scale[2]);
            }
            update();
            for (int i = 0; i < bones.Count; i++)
            {
                try{
                bones[i].invert = Matrix4.Invert(bones[i].transform);
                } catch (InvalidOperationException){
                    bones[i].invert = Matrix4.Zero;
                }
            }
        }

        public override void Read(string filename)
        {
            FileData file = new FileData(filename);
            if (file != null)
            {
                file.Endian = Endianness.Little;
                Endian = Endianness.Little;
                string magic = file.readString(0, 4);
                if (magic == "VBN ")
                {
                    file.Endian = Endianness.Big;
                    Endian = Endianness.Big;
                }

                file.seek(4);

                unk_1 = (short)file.readShort();
                unk_2 = (short)file.readShort();
                totalBoneCount = (UInt32)file.readInt();
                boneCountPerType[0] = (UInt32)file.readInt();
                boneCountPerType[1] = (UInt32)file.readInt();
                boneCountPerType[2] = (UInt32)file.readInt();
                boneCountPerType[3] = (UInt32)file.readInt();

                for (int i = 0; i < totalBoneCount; i++)
                {
                    Bone temp = new Bone(this);
                    temp.boneName = file.readString(file.pos(), -1).ToCharArray();
                    file.skip(64);
                    temp.boneType = (UInt32)file.readInt();
                    temp.parentIndex = file.readInt();
                    temp.boneId = (UInt32)file.readInt();
                    temp.position = new float[3];
                    temp.rotation = new float[3];
                    temp.scale = new float[3];
                    bones.Add(temp);
                }

                for (int i = 0; i < bones.Count; i++)
                {
                    bones[i].position[0] = file.readFloat();
                    bones[i].position[1] = file.readFloat();
                    bones[i].position[2] = file.readFloat();
                    bones[i].rotation[0] = file.readFloat();
                    bones[i].rotation[1] = file.readFloat();
                    bones[i].rotation[2] = file.readFloat();
                    bones[i].scale[0] = file.readFloat();
                    bones[i].scale[1] = file.readFloat();
                    bones[i].scale[2] = file.readFloat();
                    Bone temp = bones[i];
                    //Debug.Write(temp.parentIndex);
                    //if (temp.parentIndex != 0x0FFFFFFF && temp.parentIndex > -1)
                    //    bones[temp.parentIndex].children.Add(i);
                    bones[i] = temp;
                }
                reset();
            }
        }

        public override byte[] Rebuild()
        {
            FileOutput file = new FileOutput();
            if (file != null)
            {
                
                if (Endian == Endianness.Little) {
                    file.Endian = Endianness.Little;
                    file.writeString(" NBV");
                }
                if (Endian == Endianness.Big) {
                    file.Endian = Endianness.Big;
                    file.writeString("VBN ");
                }

                file.writeShort(0x01);
                file.writeShort(0x02);
                file.writeInt(bones.Count);
                if (boneCountPerType[0] == 0)
                    boneCountPerType[0] = (uint)bones.Count;
                for (int i = 0; i < 4; i++)
                    file.writeInt((int)boneCountPerType[i]);

                for (int i = 0; i < bones.Count; i++)
                {
                    file.writeString(new string(bones[i].boneName));
                    for (int j = 0; j < 64 - bones[i].boneName.Length; j++)
                        file.writeByte(0);
                    file.writeInt((int)bones[i].boneType);
                    if(bones[i].parentIndex == -1)
                        file.writeInt(0x0FFFFFFF);
                    else
                        file.writeInt(bones[i].parentIndex);
                    file.writeInt((int)bones[i].boneId);
                }

                for (int i = 0; i < bones.Count; i++)
                {
                    file.writeFloat(bones[i].position[0]);
                    file.writeFloat(bones[i].position[1]);
                    file.writeFloat(bones[i].position[2]);
                    file.writeFloat(bones[i].rotation[0]);
                    file.writeFloat(bones[i].rotation[1]);
                    file.writeFloat(bones[i].rotation[2]);
                    file.writeFloat(bones[i].scale[0]);
                    file.writeFloat(bones[i].scale[1]);
                    file.writeFloat(bones[i].scale[2]);
                }
            }
            return file.getBytes();
        }

        public void readJointTable(string fname)
        {
            FileData d = new FileData(fname);
            d.Endian = Endianness.Big;

            int tableSize = 2;

            int table1 = d.readShort();

            if (table1 * 2 + 2 >= d.size())
                tableSize = 1;

            int table2 = -1;
            if (tableSize != 1)
                table2 = d.readShort();

            //if (table2 == 0)
            //    d.seek(d.pos() - 2);

            List<int> t1 = new List<int>();
            for (int i = 0; i < table1; i++)
                t1.Add(d.readShort());

            jointTable.Clear();
            jointTable.Add(t1);

            if (tableSize != 1)
            {
                List<int> t2 = new List<int>();
                for (int i = 0; i < table2; i++)
                    t2.Add(d.readShort());
                jointTable.Add(t2);
            }
        }

        public Bone bone(string name)
        {
            foreach (Bone b in bones)
            {
                if (new string(b.boneName) == name)
                {
                    return b;
                }
            }
            throw new Exception("No bone of char[] name");
        }

        public int boneIndex(string name)
        {
            for (int i = 0; i < bones.Count; i++)
            {
                if (new string(bones[i].boneName) == name)
                {
                    return i;
                }
            }

            return -1;
            //throw new Exception("No bone of char[] name");
        }

        public int jtbShiftAmount = 8;

        public int getJTBIndex(string name)
        {
            int index = -1;
            int vbnIndex = boneIndex(name);
            if(jointTable != null)
            {
                for(int i = 0; i < jointTable.Count; i++)
                {
                    for(int j = 0; j < jointTable[i].Count; j++)
                    {
                        if(jointTable[i][j] == vbnIndex)
                        {
                            index = j + (i << 8);
                        }
                    }
                }
            }
            return index;
        }

        public void deleteBone(int index)
        {
            boneCountPerType[bones[index].boneType]--;
            totalBoneCount--;
            List<Bone> children = bones[index].GetChildren();
            bones.RemoveAt(index);
            foreach (Bone b in children)
                deleteBone(bones.IndexOf(b));
        }

        public void deleteBone(string name)
        {
            deleteBone(boneIndex(name));
        }

        public float[] f = null;
        public Matrix4[] bonemat;

        public Matrix4[] getShaderMatrix()
        {
            bonemat = new Matrix4[bones.Count];

            for (int i = 0; i < bones.Count; i++)
            {
                bonemat[i] = bones[i].invert * bones[i].transform;
            }

            return bonemat;
        }

        private static string charsToString(char[] c)
        {
            string boneNameRigging = "";
            foreach (char b in c)
                if (b != (char)0)
                    boneNameRigging += b;
            return boneNameRigging;
        }

        public static string BoneNameFromHash(uint boneHash)
        {
            foreach (ModelContainer m in Runtime.ModelContainers)
                if (m.vbn != null)
                    foreach (Bone b in m.vbn.bones)
                        if (b.boneId == boneHash)
                            return charsToString(b.boneName);

            csvHashes csv = new csvHashes(Path.Combine(MainForm.executableDir, "hashTable.csv"));
            for (int i = 0; i < csv.ids.Count; i++)
                if (csv.ids[i] == boneHash)
                    return csv.names[i]+" (From hashTable.csv)";

            return $"[Bonehash {boneHash.ToString("X")}]";
        }

        public static Bone GetBone(uint boneHash)
        {
            if(boneHash == 3449071621)
                return null;
            foreach (ModelContainer m in Runtime.ModelContainers)
                if (m.vbn != null)
                    foreach (Bone b in m.vbn.bones)
                        if (b.boneId == boneHash)
                            return b;
            MessageBox.Show("Open the VBN before editing the SB");
            return null;
        }
    }

    public class SB : FileBase
    {
        public override Endianness Endian
        {
            get;
            set;
        }

        public class SBEntry
        {
            public uint hash = 3449071621;
            public float param1_1, param2_1;
            public int param1_2, param1_3, param2_2, param2_3;
            public float rx1, rx2, ry1, ry2, rz1, rz2;
            public uint[] boneHashes = new uint[8] { 3449071621, 3449071621, 3449071621, 3449071621, 3449071621, 3449071621, 3449071621, 3449071621 };
            public float[] unks1 = new float[4], unks2 = new float[5];
            public float factor;
            public int[] ints = new int[4];

            public override string ToString()
            {
                return VBN.BoneNameFromHash(hash);
            }
        }

        public List<SBEntry> bones = new List<SBEntry>();

        public void TryGetEntry(uint hash, out SBEntry entry)
        {
            entry = null;
            foreach(SBEntry sb in bones)
                if (sb.hash == hash)
                    entry = sb;
        }

        public override void Read(string filename)
        {
            FileData d = new FileData(filename);
            d.Endian = Endianness.Little; // characters are little
            d.seek(8); // skip magic and version?
            int count = d.readInt(); // entry count

            for(int i = 0; i < count; i++)
            {
                SBEntry sb = new SBEntry()
                {
                    hash = (uint)d.readInt(),
                    param1_1 = d.readFloat(),
                    param1_2 = d.readInt(),
                    param1_3 = d.readInt(),
                    param2_1 = d.readFloat(),
                    param2_2 = d.readInt(),
                    param2_3 = d.readInt(),
                    rx1 = d.readFloat(),
                    rx2 = d.readFloat(),
                    ry1 = d.readFloat(),
                    ry2 = d.readFloat(),
                    rz1 = d.readFloat(),
                    rz2 = d.readFloat()
                };

                for (int j = 0; j < 8; j++)
                    sb.boneHashes[j] = (uint)d.readInt();

                for (int j = 0; j < 4; j++)
                    sb.unks1[j] = d.readFloat();

                for (int j = 0; j < 5; j++)
                    sb.unks2[j] = d.readFloat();

                sb.factor = d.readFloat();

                for (int j = 0; j < 4; j++)
                    sb.ints[j] = d.readInt();

                bones.Add(sb);

                /*Console.WriteLine(sb.hash.ToString("x"));
                Console.WriteLine(d.readFloat() + " " + d.readInt() + " " + d.readInt());
                Console.WriteLine(d.readFloat() + " " + d.readInt() + " " + d.readInt());

                //28 floats?
                Console.WriteLine(d.readFloat() + " " + d.readFloat());
                Console.WriteLine(d.readFloat() + " " + d.readFloat());
                Console.WriteLine(d.readFloat() + " " + d.readFloat());
                Console.WriteLine(d.readFloat() + " " + d.readFloat() + " " + d.readFloat() + " " + d.readFloat());
                Console.WriteLine(d.readFloat() + " " + d.readFloat() + " " + d.readFloat() + " " + d.readFloat());

                Console.WriteLine(d.readFloat() + " " + d.readFloat() + " " + d.readFloat() + " " + d.readFloat());
                Console.WriteLine(d.readFloat() + " " + d.readFloat() + " " + d.readFloat() + " " + d.readFloat());

                Console.WriteLine(d.readFloat() + " " + d.readFloat());
                Console.WriteLine(d.readInt() +  " " + d.readInt());
                Console.WriteLine(d.readInt() + " " + d.readInt());
                Console.WriteLine();*/
            }
        }

        public override byte[] Rebuild()
        {
            FileOutput o = new FileOutput();
            o.Endian = Endianness.Little;

            o.writeString(" BWS");
            o.writeShort(0x05);
            o.writeShort(0x01);
            o.writeInt(bones.Count);

            foreach(SBEntry s in bones)
            {
                o.writeInt((int)s.hash);
                o.writeFloat(s.param1_1);
                o.writeInt(s.param1_2);
                o.writeInt(s.param1_3);
                o.writeFloat(s.param2_1);
                o.writeInt(s.param2_2);
                o.writeInt(s.param2_3);
                o.writeFloat(s.rx1);
                o.writeFloat(s.rx2);
                o.writeFloat(s.ry1);
                o.writeFloat(s.ry2);
                o.writeFloat(s.rz1);
                o.writeFloat(s.rz2);

                for (int j = 0; j < 8; j++)
                    o.writeInt((int)s.boneHashes[j]);

                for (int j = 0; j < 4; j++)
                    o.writeFloat(s.unks1[j]);

                for (int j = 0; j < 5; j++)
                    o.writeFloat(s.unks2[j]);

                o.writeFloat(s.factor);

                for (int j = 0; j < 4; j++)
                    o.writeInt(s.ints[j]);
            }

            return o.getBytes();
        }
    }
}

namespace Smash_Forge
{
    class csvHashes
    {
        public List<string> names = new List<string>();
        public List<uint> ids = new List<uint>();

        public csvHashes(string filename)
        {
            var reader = new StreamReader(File.OpenRead(filename));

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                names.Add(values[0]);
                ids.Add(Convert.ToUInt32(values[1]));
            }
        }
    }
}