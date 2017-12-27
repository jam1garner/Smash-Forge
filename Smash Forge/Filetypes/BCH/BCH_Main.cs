using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;

namespace Smash_Forge
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

        public override void Read(string filename)
        {
            FileData f = new FileData(filename);
            f.Endian = System.IO.Endianness.Little;
            f.skip(4);
            int backwardCompatibility = f.readByte();
            int forwardCompatibility = f.readByte();
            int version = f.readShort();

            int mainHeaderOffset = f.readInt();
            int stringTableOffset = f.readInt();
            int gpuCommandsOffset = f.readInt();
            int dataOffset = f.readInt();
            int dataExtendedOffset = 0;
            int dataExtendedLength = 0;
            if (backwardCompatibility > 0x20) dataExtendedOffset = f.readInt();
            int relocationTableOffset = f.readInt();

            int mainHeaderLength = f.readInt();
            int stringTableLength = f.readInt();
            int gpuCommandsLength = f.readInt();
            int dataLength = f.readInt();
            if (backwardCompatibility > 0x20) dataExtendedLength = f.readInt();
            int relocationTableLength = f.readInt();

            int uninitializedDataSectionLength = f.readInt();
            int uninitializedDescriptionSectionLength = f.readInt();

            if (backwardCompatibility > 7)
            {
                int flags = f.readShort();
                int addressCount = f.readShort();
            }

            // Relocation table
            for (int i = 0; i < relocationTableLength; i += 4)
            {
                f.seek(relocationTableOffset + i);
                int val = f.readInt();
                int off = val & 0x1FFFFFF;
                byte flag = (byte)(val >> 25);

                switch (flag)
                {
                    case 0:
                        f.seek((off * 4) + mainHeaderOffset);
                        f.writeInt((off * 4) + mainHeaderOffset, f.readInt() + mainHeaderOffset);
                        break;

                    case 1:
                        f.seek(off + mainHeaderOffset);
                        f.writeInt((off) + mainHeaderOffset, f.readInt() + stringTableOffset);
                        break;

                    case 2:
                        f.seek((off * 4) + mainHeaderOffset);
                        f.writeInt((off * 4) + mainHeaderOffset, f.readInt() + gpuCommandsOffset);
                        break;

                    case 0xc:
                        f.seek((off * 4) + mainHeaderOffset);
                        f.writeInt((off * 4) + mainHeaderOffset, f.readInt() + dataOffset);
                        break;
                }

                f.seek((off * 4) + gpuCommandsOffset);
                if (backwardCompatibility < 6)
                {
                    switch (flag)
                    {
                        case 0x23: f.writeInt((off * 4) + gpuCommandsOffset, f.readInt() + dataOffset); break; //Texture
                        case 0x25: f.writeInt((off * 4) + gpuCommandsOffset, f.readInt() + dataOffset); break; //Vertex
                        //case 0x26: f.writeInt((off * 4) + int gpuCommandsOffset, ((f.readInt() + int dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode
                        case 0x27: f.writeInt((off * 4) + gpuCommandsOffset, (f.readInt() + dataOffset) & 0x7fffffff); break; //Index 8 bits mode
                    }
                }
                else if (backwardCompatibility < 8)
                {
                    switch (flag)
                    {
                        case 0x24: f.writeInt((off * 4) + gpuCommandsOffset, f.readInt() + dataOffset); break; //Texture
                        case 0x26: f.writeInt((off * 4) + gpuCommandsOffset, f.readInt() + dataOffset); break; //Vertex
                        //case 0x27: writer.Write(((peek(input) + int dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode
                        case 0x28: f.writeInt((off * 4) + gpuCommandsOffset, (f.readInt() + dataOffset) & 0x7fffffff); break; //Index 8 bits mode
                    }
                }
                else if (backwardCompatibility < 0x21)
                {
                    switch (flag)
                    {
                        case 0x25: f.writeInt((off * 4) + gpuCommandsOffset, f.readInt() + dataOffset); break; //Texture
                        case 0x27: f.writeInt((off * 4) + gpuCommandsOffset, f.readInt() + dataOffset); break; //Vertex
                        //case 0x28: writer.Write(((peek(input) + int dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode
                        case 0x29: f.writeInt((off * 4) + gpuCommandsOffset, (f.readInt() + dataOffset) & 0x7fffffff); break; //Index 8 bits mode
                    }
                }
                else
                {
                    switch (flag)
                    {
                        case 0x25: f.writeInt((off * 4) + gpuCommandsOffset, f.readInt() + dataOffset); break; //Texture
                        case 0x26: f.writeInt((off * 4) + gpuCommandsOffset, f.readInt() + dataOffset); break; //Vertex relative to Data Offset
                        //case 0x27: writer.Write(((peek(input) + int dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode relative to Data Offset
                        case 0x28: f.writeInt((off * 4) + gpuCommandsOffset, (f.readInt() + dataOffset) & 0x7fffffff); break; //Index 8 bits mode relative to Data Offset
                        case 0x2b: f.writeInt((off * 4) + gpuCommandsOffset, f.readInt() + dataExtendedOffset); break; //Vertex relative to Data Extended Offset
                        //case 0x2c: writer.Write(((peek(input) + int dataExtendedOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode relative to Data Extended Offset
                        case 0x2d: f.writeInt((off * 4) + gpuCommandsOffset, (f.readInt() + dataExtendedOffset) & 0x7fffffff); break; //Index 8 bits mode relative to Data Extended Offset
                    }
                }
            }

            //File.WriteAllBytes(filename + "_offset", f.getSection(0, f.size()));

            f.seek(mainHeaderOffset);
            int modelsPointerTableOffset = f.readInt();
            int modelsPointerTableEntries = f.readInt();
            int modelsNameOffset = f.readInt();
            int materialsPointerTableOffset = f.readInt();
            int materialsPointerTableEntries = f.readInt();
            int materialsNameOffset = f.readInt();
            int shadersPointerTableOffset = f.readInt();
            int shadersPointerTableEntries = f.readInt();
            int shadersNameOffset = f.readInt();
            int texturesPointerTableOffset = f.readInt();
            int texturesPointerTableEntries = f.readInt();
            int texturesNameOffset = f.readInt();
            int materialsLUTPointerTableOffset = f.readInt();
            int materialsLUTPointerTableEntries = f.readInt();
            int materialsLUTNameOffset = f.readInt();
            int lightsPointerTableOffset = f.readInt();
            int lightsPointerTableEntries = f.readInt();
            int lightsNameOffset = f.readInt();
            int camerasPointerTableOffset = f.readInt();
            int camerasPointerTableEntries = f.readInt();
            int camerasNameOffset = f.readInt();
            int fogsPointerTableOffset = f.readInt();
            int fogsPointerTableEntries = f.readInt();
            int fogsNameOffset = f.readInt();
            int skeletalAnimationsPointerTableOffset = f.readInt();
            int skeletalAnimationsPointerTableEntries = f.readInt();
            int skeletalAnimationsNameOffset = f.readInt();
            int materialAnimationsPointerTableOffset = f.readInt();
            int materialAnimationsPointerTableEntries = f.readInt();
            int materialAnimationsNameOffset = f.readInt();
            int visibilityAnimationsPointerTableOffset = f.readInt();
            int visibilityAnimationsPointerTableEntries = f.readInt();
            int visibilityAnimationsNameOffset = f.readInt();
            int lightAnimationsPointerTableOffset = f.readInt();
            int lightAnimationsPointerTableEntries = f.readInt();
            int lightAnimationsNameOffset = f.readInt();
            int cameraAnimationsPointerTableOffset = f.readInt();
            int cameraAnimationsPointerTableEntries = f.readInt();
            int cameraAnimationsNameOffset = f.readInt();
            int fogAnimationsPointerTableOffset = f.readInt();
            int fogAnimationsPointerTableEntries = f.readInt();
            int fogAnimationsNameOffset = f.readInt();
            int scenePointerTableOffset = f.readInt();
            int scenePointerTableEntries = f.readInt();
            int sceneNameOffset = f.readInt();

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
                f.seek(texturesPointerTableOffset + (index * 4));
                int dOffset = f.readInt();
                f.seek(dOffset);

                // one for each mip I assume
                int textureCommandsOffset = f.readInt();
                int textureCommandsWordCount = f.readInt();
                int textureCommandsOffset2 = f.readInt();
                int textureCommandsWordCount2 = f.readInt();
                int textureCommandsOffset3 = f.readInt();
                int textureCommandsWordCount3 = f.readInt();

                int unk = f.readInt();

                BCH_Texture tex = new BCH_Texture();
                tex.Text = f.readString(f.readInt(), -1);
                Textures.Nodes.Add(tex);

                f.seek(textureCommandsOffset);
                tex.ReadParameters(f, textureCommandsWordCount);
            }


            //Models

            for (int index = 0; index < modelsPointerTableEntries; index++)
            {
                f.seek(modelsPointerTableOffset + (index * 4));

                f.seek(f.readInt());

                BCH_Model model = new BCH_Model();
                Models.Nodes.Add(model);
                model.flags = f.readByte();
                model.skeletonScaleType = f.readByte();
                model.silhouetteMaterialEntries = f.readShort();
                model.worldTransform = new OpenTK.Matrix4(f.readFloat(), f.readFloat(), f.readFloat(), f.readFloat()
                     , f.readFloat(), f.readFloat(), f.readFloat(), f.readFloat()
                     , f.readFloat(), f.readFloat(), f.readFloat(), f.readFloat()
                     , 0, 0, 0, 1);

                int materialsTableOffset = f.readInt();
                int materialsTableEntries = f.readInt();
                int materialNameOffset = f.readInt();
                int verticesTableOffset = f.readInt();
                int verticesTableEntries = f.readInt();
                f.skip(0x28);
                int skeletonOffset = f.readInt();
                int skeletonEntries = f.readInt();
                int skeletonNameOffset = f.readInt();
                int objectsNodeVisibilityOffset = f.readInt();
                int objectsNodeCount = f.readInt();
                model.Text = f.readString(f.readInt(), -1);
                int objectsNodeNameEntries = f.readInt();
                int objectsNodeNameOffset = f.readInt();
                f.readInt(); //0x0
                int metaDataPointerOffset = f.readInt();

                f.seek(objectsNodeVisibilityOffset);
                int nodeVisibility = f.readInt();

                string[] objectName = new string[objectsNodeNameEntries];
                f.seek(objectsNodeNameOffset);
                int rootReferenceBit = f.readInt();
                int rootLeftNode = f.readShort();
                int rootRightNode = f.readShort();
                int rootNameOffset = f.readInt();

                //Console.WriteLine(rootReferenceBit.ToString("x") + " " + f.readString(rootNameOffset, -1) + " " + rootLeftNode + " " + rootRightNode);
                // Object name tree Radix Tree
                for (int i = 0; i < objectsNodeNameEntries; i++)
                {
                    int referenceBit = f.readInt();
                    short leftNode = (short)f.readShort();
                    short rightNode = (short)f.readShort();
                    objectName[i] = f.readString(f.readInt(), -1);
                    Console.WriteLine((referenceBit>>3) + " " + (referenceBit&0x7) + " " + objectName[i] + " " + leftNode + " " + rightNode);
                }

                //TODO: Metadata, boundingbox, normal mesh, materials
                f.seek(verticesTableOffset);
                Dictionary<int, BCH_Mesh> MeshIndex = new Dictionary<int, BCH_Mesh>();
                int nim = 0;
                for (int i = 0; i < verticesTableEntries; i++)
                {
                    BCH_Mesh Mesh = new BCH_Mesh();
                    Mesh.MaterialIndex = f.readShort();
                    int mflags = f.readShort();
                    int meshId = f.readShort();
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

                    Mesh.renderPriority = f.readShort();
                    
                    int vshAttBufferCommandOffset = f.readInt();
                    int vshAttBufferCommandCount = f.readInt();
                    int faceOffset = f.readInt();
                    int faceCount = f.readInt();
                    int vshAttBufferCommandOffsetEx = f.readInt();
                    int vshAttBufferCommandCountEx = f.readInt();

                    Vector3 Center = new Vector3(f.readFloat(), f.readFloat(), f.readFloat());
                    int flagoffset = f.readInt(); // flagsOffset
                    f.skip(4); // 0?
                    int boundingBoxOffset = f.readInt();
                }


                //Materials
                Console.WriteLine(materialsTableOffset.ToString("x") + " " + materialsPointerTableOffset.ToString("x"));
                for(int i = 0; i < materialsTableEntries; i++)
                {
                    f.seek(materialsTableOffset + (i * 0x2c));
                    int paramOffset = f.readInt();
                    f.skip(12); // other offsets
                    int texCommandOffset = f.readInt();
                    int texCommandCount = f.readInt();
                    int mapperOffset = f.readInt();

                    BCH_Material mat = new BCH_Material();
                    Materials.Nodes.Add(mat);
                    mat.Text = f.readString(f.readInt(), -1);
                    Console.WriteLine(mat.Text);
                    //Console.WriteLine(f.readString(f.readInt(), -1));
                    //Console.WriteLine(f.readString(f.readInt(), -1));
                    //Console.WriteLine(f.readString(f.readInt(), -1));

                    // TODO: Parameters
                }


                //Skeleton
                f.seek(skeletonOffset);
                for (int bindex = 0; bindex < skeletonEntries; bindex++)
                {
                    Bone bone = new Bone(model.skeleton);
                    int boneFlags = f.readInt();
                    bone.parentIndex = (short)f.readShort();
                    short boneSpace = (short)f.readShort();
                    bone.scale = new float[3];
                    bone.rotation = new float[3];
                    bone.position = new float[3];
                    bone.scale[0] = f.readFloat();
                    bone.scale[1] = f.readFloat();
                    bone.scale[2] = f.readFloat();
                    bone.rotation[0] = f.readFloat();
                    bone.rotation[1] = f.readFloat();
                    bone.rotation[2] = f.readFloat();
                    bone.position[0] = f.readFloat();
                    bone.position[1] = f.readFloat();
                    bone.position[2] = f.readFloat();

                    // bone matrix... not really needed to be stored per say
                    f.skip(4 * 4 * 3);

                    bone.Text = f.readString(f.readInt(), -1);

                    int metaDataPointerOffset2 = f.readInt();
                    if (metaDataPointerOffset2 != 0)
                    {
                        int position = f.pos();
                        f.seek(metaDataPointerOffset2);
                        //bone.userData = getMetaData(input);
                        f.seek(position);
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

        public BCH_Texture GetTexture(String name)
        {
            foreach (BCH_Texture mat in Textures.Nodes)
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
