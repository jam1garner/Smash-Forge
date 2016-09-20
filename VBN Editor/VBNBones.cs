using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

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

	public Vector3 pos = Vector3.Zero, sca = new Vector3(1f,1f,1f);
	public Quaternion rot = Quaternion.FromMatrix(Matrix3.Zero);
	public Matrix4 transform;
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

	public static Quaternion FromEulerAngles(float z, float y, float x){
		{
			Quaternion xRotation = Quaternion.FromAxisAngle(Vector3.UnitX, x);
			Quaternion yRotation = Quaternion.FromAxisAngle(Vector3.UnitY, y);
			Quaternion zRotation = Quaternion.FromAxisAngle(Vector3.UnitZ, z);

			//return xRotation * yRotation * zRotation;
			return (zRotation * yRotation * xRotation);
		}
	}

	public void update(){
		for (int i = 0; i < bones.Count; i++) {
			bones [i].transform = Matrix4.CreateScale (bones [i].sca) * Matrix4.CreateFromQuaternion (bones [i].rot) * Matrix4.CreateTranslation (bones[i].pos);
			if (bones [i].parentIndex != 0x0FFFFFFF && bones[i].parentIndex!=-1) {
				bones [i].transform = bones [i].transform * bones [(int)bones [i].parentIndex].transform;
			}
		}
	}

	public VBN(string filename)
	{
		using (BinaryReader file = new BinaryReader (File.Open (filename, FileMode.Open))) {
			file.ReadInt32 (); //Skip past magic
			unk_1 = file.ReadUInt16 ();
			unk_2 = file.ReadUInt16 ();
			totalBoneCount = file.ReadUInt32 ();
			boneCountPerType [0] = file.ReadUInt32 ();
			boneCountPerType [1] = file.ReadUInt32 ();
			boneCountPerType [2] = file.ReadUInt32 ();
			boneCountPerType [3] = file.ReadUInt32 ();

			for (int i = 0; i < totalBoneCount; i++) {
				Bone temp = new Bone ();
				temp.children = new List<int> ();
				char[] foo = file.ReadChars (64);
				int j = foo.Length - 1;
				while (foo [j] == (char)0)
					--j;
				temp.boneName = new char[j + 1];
				Array.Copy (foo, temp.boneName, j + 1);
				temp.boneType = file.ReadUInt32 ();
				temp.parentIndex = file.ReadInt32 ();
				temp.boneId = file.ReadUInt32 ();
				temp.position = new float[3];
				temp.rotation = new float[3];
				temp.scale = new float[3];
				bones.Add (temp);
			}

			for (int i = 0; i < totalBoneCount; i++) {
				bones [i].position [0] = file.ReadSingle ();
				bones [i].position [1] = file.ReadSingle ();
				bones [i].position [2] = file.ReadSingle ();
				bones [i].rotation [0] = file.ReadSingle ();
				bones [i].rotation [1] = file.ReadSingle ();
				bones [i].rotation [2] = file.ReadSingle ();
				bones [i].scale [0] = file.ReadSingle ();
				bones [i].scale [1] = file.ReadSingle ();
				bones [i].scale [2] = file.ReadSingle ();
				Bone temp = bones [i];
				if (temp.parentIndex != 0x0FFFFFFF)
					bones [(int)temp.parentIndex].children.Add (i);
				bones [i] = temp;

				bones [i].pos = new Vector3 (bones[i].position[0], bones[i].position[1], bones[i].position[2]);
				bones [i].rot = (FromEulerAngles (bones [i].rotation [2], bones [i].rotation [1], bones [i].rotation [0]));
			}

			update ();
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
			file.Write(bones.Count);
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

			for(int i = 0; i < bones[j].children.Count; i++)
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
}
