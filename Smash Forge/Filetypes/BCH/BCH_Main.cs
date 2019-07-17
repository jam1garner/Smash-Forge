using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using OpenTK;

namespace SmashForge
{
    public class BCH : FileBase
    {
        public override Endianness Endian { get; set; }

        public TreeNode Textures = new TreeNode() { Text = "Textures" };
        public TreeNode Models = new TreeNode() { Text = "Models" };
        public TreeNode Materials = new TreeNode() { Text = "Materials" };
        public TreeNode Animations = new TreeNode() { Text = "Animations" };

        public BCH()
        {
            Nodes.Add(Models);
            Nodes.Add(Textures);
            Nodes.Add(Materials);
            Nodes.Add(Animations);
            ImageKey = "model";
            SelectedImageKey = "model";
        }

        public BCH(string fname) : this()
        {
            Text = Path.GetFileName(fname);
            Read(fname);
        }

        public override void Read(string filename)
        {
            FileData f = new FileData(filename);
            f.endian = System.IO.Endianness.Little;
            f.Skip(4);
            int backwardCompatibility = f.ReadByte();
            int forwardCompatibility = f.ReadByte();
            int version = f.ReadUShort();

            int mainHeaderOffset = f.ReadInt();
            int stringTableOffset = f.ReadInt();
            int gpuCommandsOffset = f.ReadInt();
            int dataOffset = f.ReadInt();
            int dataExtendedOffset = 0;
            int dataExtendedLength = 0;
            if (backwardCompatibility > 0x20) dataExtendedOffset = f.ReadInt();
            int relocationTableOffset = f.ReadInt();

            int mainHeaderLength = f.ReadInt();
            int stringTableLength = f.ReadInt();
            int gpuCommandsLength = f.ReadInt();
            int dataLength = f.ReadInt();
            if (backwardCompatibility > 0x20) dataExtendedLength = f.ReadInt();
            int relocationTableLength = f.ReadInt();

            int uninitializedDataSectionLength = f.ReadInt();
            int uninitializedDescriptionSectionLength = f.ReadInt();

            if (backwardCompatibility > 7)
            {
                int flags = f.ReadUShort();
                int addressCount = f.ReadUShort();
            }

            // Relocation table
            for (int i = 0; i < relocationTableLength; i += 4)
            {
                f.Seek(relocationTableOffset + i);
                int val = f.ReadInt();
                int off = val & 0x1FFFFFF;
                byte flag = (byte)(val >> 25);

                switch (flag)
                {
                    case 0:
                        f.Seek((off * 4) + mainHeaderOffset);
                        f.WriteInt((off * 4) + mainHeaderOffset, f.ReadInt() + mainHeaderOffset);
                        break;

                    case 1:
                        f.Seek(off + mainHeaderOffset);
                        f.WriteInt((off) + mainHeaderOffset, f.ReadInt() + stringTableOffset);
                        break;

                    case 2:
                        f.Seek((off * 4) + mainHeaderOffset);
                        f.WriteInt((off * 4) + mainHeaderOffset, f.ReadInt() + gpuCommandsOffset);
                        break;

                    case 0xc:
                        f.Seek((off * 4) + mainHeaderOffset);
                        f.WriteInt((off * 4) + mainHeaderOffset, f.ReadInt() + dataOffset);
                        break;
                }

                f.Seek((off * 4) + gpuCommandsOffset);
                if (backwardCompatibility < 6)
                {
                    switch (flag)
                    {
                        case 0x23: f.WriteInt((off * 4) + gpuCommandsOffset, f.ReadInt() + dataOffset); break; //Texture
                        case 0x25: f.WriteInt((off * 4) + gpuCommandsOffset, f.ReadInt() + dataOffset); break; //Vertex
                        //case 0x26: f.writeInt((off * 4) + int gpuCommandsOffset, ((f.readInt() + int dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode
                        case 0x27: f.WriteInt((off * 4) + gpuCommandsOffset, (f.ReadInt() + dataOffset) & 0x7fffffff); break; //Index 8 bits mode
                    }
                }
                else if (backwardCompatibility < 8)
                {
                    switch (flag)
                    {
                        case 0x24: f.WriteInt((off * 4) + gpuCommandsOffset, f.ReadInt() + dataOffset); break; //Texture
                        case 0x26: f.WriteInt((off * 4) + gpuCommandsOffset, f.ReadInt() + dataOffset); break; //Vertex
                        //case 0x27: writer.Write(((peek(input) + int dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode
                        case 0x28: f.WriteInt((off * 4) + gpuCommandsOffset, (f.ReadInt() + dataOffset) & 0x7fffffff); break; //Index 8 bits mode
                    }
                }
                else if (backwardCompatibility < 0x21)
                {
                    switch (flag)
                    {
                        case 0x25: f.WriteInt((off * 4) + gpuCommandsOffset, f.ReadInt() + dataOffset); break; //Texture
                        case 0x27: f.WriteInt((off * 4) + gpuCommandsOffset, f.ReadInt() + dataOffset); break; //Vertex
                        //case 0x28: writer.Write(((peek(input) + int dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode
                        case 0x29: f.WriteInt((off * 4) + gpuCommandsOffset, (f.ReadInt() + dataOffset) & 0x7fffffff); break; //Index 8 bits mode
                    }
                }
                else
                {
                    switch (flag)
                    {
                        case 0x25: f.WriteInt((off * 4) + gpuCommandsOffset, f.ReadInt() + dataOffset); break; //Texture
                        case 0x26: f.WriteInt((off * 4) + gpuCommandsOffset, f.ReadInt() + dataOffset); break; //Vertex relative to Data Offset
                        //case 0x27: writer.Write(((peek(input) + int dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode relative to Data Offset
                        case 0x28: f.WriteInt((off * 4) + gpuCommandsOffset, (f.ReadInt() + dataOffset) & 0x7fffffff); break; //Index 8 bits mode relative to Data Offset
                        case 0x2b: f.WriteInt((off * 4) + gpuCommandsOffset, f.ReadInt() + dataExtendedOffset); break; //Vertex relative to Data Extended Offset
                        //case 0x2c: writer.Write(((peek(input) + int dataExtendedOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode relative to Data Extended Offset
                        case 0x2d: f.WriteInt((off * 4) + gpuCommandsOffset, (f.ReadInt() + dataExtendedOffset) & 0x7fffffff); break; //Index 8 bits mode relative to Data Extended Offset
                    }
                }
            }

            //File.WriteAllBytes(filename + "_offset", f.getSection(0, f.size()));

            f.Seek(mainHeaderOffset);
            int modelsPointerTableOffset = f.ReadInt();
            int modelsPointerTableEntries = f.ReadInt();
            int modelsNameOffset = f.ReadInt();
            int materialsPointerTableOffset = f.ReadInt();
            int materialsPointerTableEntries = f.ReadInt();
            int materialsNameOffset = f.ReadInt();
            int shadersPointerTableOffset = f.ReadInt();
            int shadersPointerTableEntries = f.ReadInt();
            int shadersNameOffset = f.ReadInt();
            int texturesPointerTableOffset = f.ReadInt();
            int texturesPointerTableEntries = f.ReadInt();
            int texturesNameOffset = f.ReadInt();
            int materialsLUTPointerTableOffset = f.ReadInt();
            int materialsLUTPointerTableEntries = f.ReadInt();
            int materialsLUTNameOffset = f.ReadInt();
            int lightsPointerTableOffset = f.ReadInt();
            int lightsPointerTableEntries = f.ReadInt();
            int lightsNameOffset = f.ReadInt();
            int camerasPointerTableOffset = f.ReadInt();
            int camerasPointerTableEntries = f.ReadInt();
            int camerasNameOffset = f.ReadInt();
            int fogsPointerTableOffset = f.ReadInt();
            int fogsPointerTableEntries = f.ReadInt();
            int fogsNameOffset = f.ReadInt();
            int skeletalAnimationsPointerTableOffset = f.ReadInt();
            int skeletalAnimationsPointerTableEntries = f.ReadInt();
            int skeletalAnimationsNameOffset = f.ReadInt();
            int materialAnimationsPointerTableOffset = f.ReadInt();
            int materialAnimationsPointerTableEntries = f.ReadInt();
            int materialAnimationsNameOffset = f.ReadInt();
            int visibilityAnimationsPointerTableOffset = f.ReadInt();
            int visibilityAnimationsPointerTableEntries = f.ReadInt();
            int visibilityAnimationsNameOffset = f.ReadInt();
            int lightAnimationsPointerTableOffset = f.ReadInt();
            int lightAnimationsPointerTableEntries = f.ReadInt();
            int lightAnimationsNameOffset = f.ReadInt();
            int cameraAnimationsPointerTableOffset = f.ReadInt();
            int cameraAnimationsPointerTableEntries = f.ReadInt();
            int cameraAnimationsNameOffset = f.ReadInt();
            int fogAnimationsPointerTableOffset = f.ReadInt();
            int fogAnimationsPointerTableEntries = f.ReadInt();
            int fogAnimationsNameOffset = f.ReadInt();
            int scenePointerTableOffset = f.ReadInt();
            int scenePointerTableEntries = f.ReadInt();
            int sceneNameOffset = f.ReadInt();

            Console.WriteLine(modelsPointerTableEntries > 0 ? "Has Models" : "");
            Console.WriteLine(shadersPointerTableEntries > 0 ? "Has Shaders" : "");
            Console.WriteLine(texturesPointerTableEntries > 0 ? "Has Textures" : "");
            Console.WriteLine(materialsPointerTableEntries > 0 ? "Has Materials" : "");
            Console.WriteLine(materialsLUTPointerTableEntries > 0 ? "Has Material LUT" : "");
            Console.WriteLine(materialAnimationsPointerTableEntries > 0 ? "Has Material Animation" : "");
            Console.WriteLine(lightsPointerTableEntries > 0 ? "Has Lights" : "");
            Console.WriteLine(lightAnimationsPointerTableEntries > 0 ? "Has LightAnimations" : "");
            Console.WriteLine(camerasPointerTableEntries > 0 ? "Has Camera" : "");
            Console.WriteLine(cameraAnimationsPointerTableEntries > 0 ? "Has CameraAnimation" : "");
            Console.WriteLine(fogsPointerTableEntries > 0 ? "Has Fog" : "");
            Console.WriteLine(fogAnimationsPointerTableEntries > 0 ? "Has FogAnimation" : "");
            Console.WriteLine(skeletalAnimationsPointerTableEntries > 0 ? "Has Skeletal Animations" : "");
            Console.WriteLine(visibilityAnimationsPointerTableEntries > 0 ? "Has Visibility" : "");
            Console.WriteLine(scenePointerTableEntries > 0 ? "Has Scene" : "");

            // Textures
            for (int index = 0; index < texturesPointerTableEntries; index++)
            {
                f.Seek(texturesPointerTableOffset + (index * 4));
                int dOffset = f.ReadInt();
                f.Seek(dOffset);

                // one for each mip I assume
                int textureCommandsOffset = f.ReadInt();
                int textureCommandsWordCount = f.ReadInt();
                int textureCommandsOffset2 = f.ReadInt();
                int textureCommandsWordCount2 = f.ReadInt();
                int textureCommandsOffset3 = f.ReadInt();
                int textureCommandsWordCount3 = f.ReadInt();

                int unk = f.ReadInt();

                BchTexture tex = new BchTexture();
                tex.Text = f.ReadString(f.ReadInt(), -1);
                Textures.Nodes.Add(tex);

                f.Seek(textureCommandsOffset);
                tex.ReadParameters(f, textureCommandsWordCount);
            }


            //Models

            for (int index = 0; index < modelsPointerTableEntries; index++)
            {
                f.Seek(modelsPointerTableOffset + (index * 4));

                f.Seek(f.ReadInt());

                BCH_Model model = new BCH_Model();
                Models.Nodes.Add(model);
                model.flags = f.ReadByte();
                model.skeletonScaleType = f.ReadByte();
                model.silhouetteMaterialEntries = f.ReadUShort();
                model.worldTransform = new OpenTK.Matrix4(f.ReadFloat(), f.ReadFloat(), f.ReadFloat(), f.ReadFloat()
                     , f.ReadFloat(), f.ReadFloat(), f.ReadFloat(), f.ReadFloat()
                     , f.ReadFloat(), f.ReadFloat(), f.ReadFloat(), f.ReadFloat()
                     , 0, 0, 0, 1);

                int materialsTableOffset = f.ReadInt();
                int materialsTableEntries = f.ReadInt();
                int materialNameOffset = f.ReadInt();
                int verticesTableOffset = f.ReadInt();
                int verticesTableEntries = f.ReadInt();
                f.Skip(0x28);
                int skeletonOffset = f.ReadInt();
                int skeletonEntries = f.ReadInt();
                int skeletonNameOffset = f.ReadInt();
                int objectsNodeVisibilityOffset = f.ReadInt();
                int objectsNodeCount = f.ReadInt();
                model.Text = f.ReadString(f.ReadInt(), -1);
                int objectsNodeNameEntries = f.ReadInt();
                int objectsNodeNameOffset = f.ReadInt();
                f.ReadInt(); //0x0
                int metaDataPointerOffset = f.ReadInt();

                f.Seek(objectsNodeVisibilityOffset);
                int nodeVisibility = f.ReadInt();

                string[] objectName = new string[objectsNodeNameEntries];
                f.Seek(objectsNodeNameOffset);
                int rootReferenceBit = f.ReadInt();
                int rootLeftNode = f.ReadUShort();
                int rootRightNode = f.ReadUShort();
                int rootNameOffset = f.ReadInt();

                //Console.WriteLine(rootReferenceBit.ToString("x") + " " + f.readString(rootNameOffset, -1) + " " + rootLeftNode + " " + rootRightNode);
                // Object name tree Radix Tree
                for (int i = 0; i < objectsNodeNameEntries; i++)
                {
                    int referenceBit = f.ReadInt();
                    short leftNode = f.ReadShort();
                    short rightNode = f.ReadShort();
                    objectName[i] = f.ReadString(f.ReadInt(), -1);
                    Console.WriteLine((referenceBit>>3) + " " + (referenceBit&0x7) + " " + objectName[i] + " " + leftNode + " " + rightNode);
                }

                //TODO: Metadata, boundingbox, normal mesh, materials
                f.Seek(verticesTableOffset);
                Dictionary<int, BCH_Mesh> MeshIndex = new Dictionary<int, BCH_Mesh>();
                int nim = 0;
                for (int i = 0; i < verticesTableEntries; i++)
                {
                    BCH_Mesh Mesh = new BCH_Mesh();
                    Mesh.MaterialIndex = f.ReadUShort();
                    int mflags = f.ReadUShort();
                    int meshId = f.ReadUShort();
                    if (!MeshIndex.ContainsKey(meshId))
                    {
                        MeshIndex.Add(meshId, Mesh);
                        Mesh.Text = nim < objectName.Length ? objectName[nim++] : i + "";
                        model.Nodes.Add(Mesh);
                    }
                    else
                    {
                        BCH_Mesh m = MeshIndex[meshId];
                        Mesh.Text = m.Text;
                        model.Nodes.Insert(model.Nodes.IndexOf(m)-1, Mesh);
                    }

                    // node visibility TODO: finish...
                    Mesh.Checked = ((nodeVisibility & (1 << i)) > 0);

                    Mesh.renderPriority = f.ReadUShort();
                    
                    int vshAttBufferCommandOffset = f.ReadInt();
                    int vshAttBufferCommandCount = f.ReadInt();
                    int faceOffset = f.ReadInt();
                    int faceCount = f.ReadInt();
                    int vshAttBufferCommandOffsetEx = f.ReadInt();
                    int vshAttBufferCommandCountEx = f.ReadInt();

                    Vector3 Center = new Vector3(f.ReadFloat(), f.ReadFloat(), f.ReadFloat());
                    int flagoffset = f.ReadInt(); // flagsOffset
                    f.Skip(4); // 0?
                    int boundingBoxOffset = f.ReadInt();
                }


                //Materials
                Console.WriteLine(materialsTableOffset.ToString("x") + " " + materialsPointerTableOffset.ToString("x"));
                for(int i = 0; i < materialsTableEntries; i++)
                {
                    f.Seek(materialsTableOffset + (i * 0x2c));
                    int paramOffset = f.ReadInt();
                    f.Skip(12); // other offsets
                    int texCommandOffset = f.ReadInt();
                    int texCommandCount = f.ReadInt();
                    int mapperOffset = f.ReadInt();

                    BCH_Material mat = new BCH_Material();
                    Materials.Nodes.Add(mat);
                    mat.Text = f.ReadString(f.ReadInt(), -1);
                    Console.WriteLine(mat.Text);
                    //Console.WriteLine(f.readString(f.readInt(), -1));
                    //Console.WriteLine(f.readString(f.readInt(), -1));
                    //Console.WriteLine(f.readString(f.readInt(), -1));

                    // TODO: Parameters
                }


                //Skeleton
                f.Seek(skeletonOffset);
                for (int bindex = 0; bindex < skeletonEntries; bindex++)
                {
                    Bone bone = new Bone(model.skeleton);
                    int boneFlags = f.ReadInt();
                    bone.parentIndex = f.ReadShort();
                    short boneSpace = f.ReadShort();
                    bone.scale = new float[3];
                    bone.rotation = new float[3];
                    bone.position = new float[3];
                    bone.scale[0] = f.ReadFloat();
                    bone.scale[1] = f.ReadFloat();
                    bone.scale[2] = f.ReadFloat();
                    bone.rotation[0] = f.ReadFloat();
                    bone.rotation[1] = f.ReadFloat();
                    bone.rotation[2] = f.ReadFloat();
                    bone.position[0] = f.ReadFloat();
                    bone.position[1] = f.ReadFloat();
                    bone.position[2] = f.ReadFloat();

                    // bone matrix... not really needed to be stored per say
                    f.Skip(4 * 4 * 3);

                    bone.Text = f.ReadString(f.ReadInt(), -1);

                    int metaDataPointerOffset2 = f.ReadInt();
                    if (metaDataPointerOffset2 != 0)
                    {
                        int position = f.Pos();
                        f.Seek(metaDataPointerOffset2);
                        //bone.userData = getMetaData(input);
                        f.Seek(position);
                    }

                    model.skeleton.bones.Add(bone);
                }
                model.skeleton.reset();
                model.skeleton.update();
            }
        }

        public BCH_Material GetMaterial(String name)
        {
            foreach (BCH_Material mat in Materials.Nodes)
                if (mat.Text.Equals(name)) return mat;
            return null;
        }

        public BchTexture GetTexture(String name)
        {
            foreach (BchTexture mat in Textures.Nodes)
            {
                if (mat.Text.Equals(name)) return mat;
            }
            return null;
        }

        public override byte[] Rebuild()
        {
            throw new NotImplementedException();
        }


        
    }
}
