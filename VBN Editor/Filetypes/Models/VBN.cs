using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace VBN_Editor
{
    public class Bone
    {
        public char[] boneName;
        public UInt32 boneType;
        public int parentIndex;
        public UInt32 boneId;
        public float[] position;
        public float[] rotation;
        public float[] scale;
        public List<int> children;

        public Vector3 pos = Vector3.Zero, sca = new Vector3(1f, 1f, 1f);
        public Quaternion rot = Quaternion.FromMatrix(Matrix3.Zero);
        public Matrix4 transform, invert;
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
            foreach (int c in b.children)
                queueBones(bones[c], q);
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
                    Bone temp = new Bone();
                    temp.children = new List<int>();
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
                    Debug.Write(temp.parentIndex);
                    if (temp.parentIndex != 0x0FFFFFFF && temp.parentIndex > -1)
                        bones[temp.parentIndex].children.Add(i);
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

                file.writeShort(unk_1);
                file.writeShort(unk_2);
                file.writeInt(bones.Count);
                for (int i = 0; i < 4; i++)
                    file.writeInt((int)boneCountPerType[i]);

                for (int i = 0; i < bones.Count; i++)
                {
                    file.writeString(new string(bones[i].boneName));
                    for (int j = 0; j < 64 - bones[i].boneName.Length; j++)
                        file.writeByte(0);
                    file.writeInt((int)bones[i].boneType);
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

        public void deleteBone(int index)
        {
            boneCountPerType[bones[index].boneType]--;
            totalBoneCount--;
            bones[(int)bones[index].parentIndex].children.Remove(index);
            for (int j = 0; j < bones.Count; j++)
            {
                if (bones[j].parentIndex > (uint)index)
                {
                    Bone tmp = bones[j];
                    tmp.parentIndex -= 1;
                    bones[j] = tmp;
                }

                for (int i = 0; i < bones[j].children.Count; i++)
                {
                    if (bones[j].children[i] > index)
                    {
                        bones[j].children[i]--;
                    }
                }
            }
            List<int> temp = bones[index].children;
            bones.Remove(bones[index]);
            foreach (int i in temp)
            {
                deleteBone(i);
            }
        }

        public void deleteBone(string name)
        {
            deleteBone(boneIndex(name));
        }

        public float[] f = null;

        public Matrix4[] getShaderMatrix()
        {
            Matrix4[] bonemat = new Matrix4[bones.Count];

            for (int i = 0; i < bones.Count; i++)
            {
                bonemat[i] = bones[i].invert * bones[i].transform;
            }

            return bonemat;
        }


        public void updateChildren()
        {
            for (int i = 0; i < bones.Count; i++)
            {
                bones[i].children = new List<int>();
            }

            for (int i = 0; i < bones.Count; i++)
            {
                if (bones[i].parentIndex != 0x0FFFFFFF)
                    bones[(int)bones[i].parentIndex].children.Add(i);
            }
        }
    }
}

namespace VBN_Editor
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