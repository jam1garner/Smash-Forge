using System;
using System.Collections.Generic;
using System.IO;

public struct Bone
{
    public char[] boneName;
    public UInt32 boneType;
    public UInt32 parentIndex;
    public UInt32 boneId;
    public float[] position;
    public float[] rotation;
    public float[] scale;
    public List<int> children;
}

public class VBN
{

    public UInt16 unk_1,unk_2;
    public UInt32 totalBoneCount;
    public UInt32[] boneCountPerType = new UInt32[4];
    public List<Bone> bones = new List<Bone>();

    public VBN()
    {
        
    }

    public VBN(string filename)
	{
        using (BinaryReader file = new BinaryReader(File.Open(filename, FileMode.Open)))
        {
            file.ReadInt32(); //Skip past magic
            unk_1 = file.ReadUInt16();
            unk_2 = file.ReadUInt16();
            totalBoneCount = file.ReadUInt32();
            boneCountPerType[0] = file.ReadUInt32();
            boneCountPerType[1] = file.ReadUInt32();
            boneCountPerType[2] = file.ReadUInt32();
            boneCountPerType[3] = file.ReadUInt32();
            
            for (int i = 0; i < totalBoneCount;i++)
            {
                Bone temp = new Bone();
                temp.children = new List<int>();
                char[] foo = file.ReadChars(64);
                int j = foo.Length - 1;
                while (foo[j] == (char)0)
                    --j;
                temp.boneName = new char[j + 1];
                Array.Copy(foo, temp.boneName, j + 1);
                temp.boneType = file.ReadUInt32();
                temp.parentIndex = file.ReadUInt32();
                temp.boneId = file.ReadUInt32();
                temp.position = new float[3];
                temp.rotation = new float[3];
                temp.scale = new float[3];
                bones.Add(temp);
            }

            for (int i = 0; i < totalBoneCount; i++)
            {
                bones[i].position[0] = file.ReadSingle();
                bones[i].position[1] = file.ReadSingle();
                bones[i].position[2] = file.ReadSingle();
                bones[i].rotation[0] = file.ReadSingle();
                bones[i].rotation[1] = file.ReadSingle();
                bones[i].rotation[2] = file.ReadSingle();
                bones[i].scale[0] = file.ReadSingle();
                bones[i].scale[1] = file.ReadSingle();
                bones[i].scale[2] = file.ReadSingle();
                Bone temp = bones[i];
                if (temp.parentIndex != 0x0FFFFFFF)
                    bones[(int)temp.parentIndex].children.Add(i);
                bones[i] = temp;
            }
        }

    }

    public void save(string filename)
    {
        using (BinaryWriter file = new BinaryWriter(File.Open(filename, FileMode.Create)))
        {
            file.Write(' ');
            file.Write('N');
            file.Write('B');
            file.Write('V');
            file.Write(unk_1);
            file.Write(unk_2);
            file.Write(totalBoneCount);
            for(int i = 0; i < 4; i++)
                file.Write(boneCountPerType[i]);

            for (int i = 0; i < totalBoneCount; i++)
            {
                file.Write(bones[i].boneName);
                for (int j = 0; j < 64 - bones[i].boneName.Length; j++)
                    file.Write((char)0);
                file.Write(bones[i].boneType);
                file.Write(bones[i].parentIndex);
                file.Write(bones[i].boneId);
            }

            for (int i = 0; i < totalBoneCount; i++)
            {
                file.Write(bones[i].position[0]);
                file.Write(bones[i].position[1]);
                file.Write(bones[i].position[2]);
                file.Write(bones[i].rotation[0]);
                file.Write(bones[i].rotation[1]);
                file.Write(bones[i].rotation[2]);
                file.Write(bones[i].scale[0]);
                file.Write(bones[i].scale[1]);
                file.Write(bones[i].scale[2]);
            }
        }
    }

    public Bone bone(string name)
    {
        foreach(Bone b in bones)
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
        for(int i = 0; i < bones.Count; i++)
        {
            if (new string(bones[i].boneName) == name)
            {
                return i;
            }
        }
        throw new Exception("No bone of char[] name");
    }
}
