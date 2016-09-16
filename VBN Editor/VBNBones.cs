using System;
using System.IO;

public struct Bone
{
    public char[] boneName;
    public UInt32 boneType;
    public UInt32 treeDepth;
    public UInt32 boneId;
    public float[] position;
    public float[] rotation;
    public float[] scale;
}

public class VBN
{

    public UInt16 unk_1,unk_2;
    public UInt32 totalBoneCount;
    public UInt32[] boneCountPerType = new UInt32[4];
    public Bone[] bones;

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
            bones = new Bone[totalBoneCount];
            for (int i = 0; i < totalBoneCount;i++)
            {
                bones[i].boneName = file.ReadChars(64);
                bones[i].boneType = file.ReadUInt32();
                bones[i].treeDepth = file.ReadUInt32();
                bones[i].boneId = file.ReadUInt32();
            }

            for (int i = 0; i < totalBoneCount; i++)
            {
                bones[i].position = new float[3];
                bones[i].rotation = new float[3];
                bones[i].scale = new float[3];
                bones[i].position[0] = file.ReadSingle();
                bones[i].position[1] = file.ReadSingle();
                bones[i].position[2] = file.ReadSingle();
                bones[i].rotation[0] = file.ReadSingle();
                bones[i].rotation[1] = file.ReadSingle();
                bones[i].rotation[2] = file.ReadSingle();
                bones[i].scale[0] = file.ReadSingle();
                bones[i].scale[1] = file.ReadSingle();
                bones[i].scale[2] = file.ReadSingle();
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
                file.Write(bones[i].boneType);
                file.Write(bones[i].treeDepth);
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
}
