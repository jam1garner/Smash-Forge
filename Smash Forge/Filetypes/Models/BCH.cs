using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;

namespace Smash_Forge
{
    /*
     * Everything in this class is adapted from gdkchan's Ohana3DS
     * This version will ONLY support Smash-style BCHs
     * Really WIP
     */
    public class BCH : FileBase
    {

        public override Endianness Endian{ get; set; }

        public MBN mesh;
        public Dictionary<string, BCH_Texture> textures = new Dictionary<string, BCH_Texture>();
        public Dictionary<string, int> textureDisplay = new Dictionary<string, int>();

        public BCH()
        {
        }

        public override void Read(string filename)
        {
            FileData f = new FileData(filename);
            f.Endian = System.IO.Endianness.Little;

            f.skip(8);
            int mainHeaderOffset = f.readInt();
            int stringTableOffset = f.readInt();
            int gpuCommandOffset = f.readInt();
            int dataOffset = f.readInt();
            int dataExtendOffset = f.readInt();
            int relocationTableOffset = f.readInt();

            int mainHeaderLength = f.readInt();
            int stringTableLength = f.readInt();
            int gpuCommandLength = f.readInt();
            int dataLength = f.readInt();
            int dataExtendLength = f.readInt();
            int relocationTableLength = f.readInt();

            int datsSecLength = f.readInt();
            int desSecLength = f.readInt();

            int flags = f.readShort();
            int addressCount = f.readShort();
            
            // TODO: Finished Relocation table stuff
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

                        break;
                }
            }

            // Content Header
            f.seek(mainHeaderOffset);
            int modelsPointerTableOffset = f.readInt() + mainHeaderOffset;
            int modelsPointerTableEntries = f.readInt();
            int modelsNameOffset = f.readInt() + mainHeaderOffset;
            int materialsPointerTableOffset = f.readInt() + mainHeaderOffset;
            int materialsPointerTableEntries = f.readInt();
            int materialsNameOffset = f.readInt() + mainHeaderOffset;
            int shadersPointerTableOffset = f.readInt() + mainHeaderOffset;
            int shadersPointerTableEntries = f.readInt();
            int shadersNameOffset = f.readInt() + mainHeaderOffset;
            int texturesPointerTableOffset = f.readInt() + mainHeaderOffset;
            int texturesPointerTableEntries = f.readInt();
            int texturesNameOffset = f.readInt() + mainHeaderOffset;
            int materialsLUTPointerTableOffset = f.readInt() + mainHeaderOffset;
            int materialsLUTPointerTableEntries = f.readInt();
            int materialsLUTNameOffset = f.readInt() + mainHeaderOffset;
            int lightsPointerTableOffset = f.readInt() + mainHeaderOffset;
            int lightsPointerTableEntries = f.readInt();
            int lightsNameOffset = f.readInt() + mainHeaderOffset;
            int camerasPointerTableOffset = f.readInt() + mainHeaderOffset;
            int camerasPointerTableEntries = f.readInt();
            int camerasNameOffset = f.readInt() + mainHeaderOffset;
            int fogsPointerTableOffset = f.readInt() + mainHeaderOffset;
            int fogsPointerTableEntries = f.readInt();
            int fogsNameOffset = f.readInt() + mainHeaderOffset;
            int skeletalAnimationsPointerTableOffset = f.readInt() + mainHeaderOffset;
            int skeletalAnimationsPointerTableEntries = f.readInt();
            int skeletalAnimationsNameOffset = f.readInt() + mainHeaderOffset;
            int materialAnimationsPointerTableOffset = f.readInt() + mainHeaderOffset;
            int materialAnimationsPointerTableEntries = f.readInt();
            int materialAnimationsNameOffset = f.readInt() + mainHeaderOffset;
            int visibilityAnimationsPointerTableOffset = f.readInt() + mainHeaderOffset;
            int visibilityAnimationsPointerTableEntries = f.readInt();
            int visibilityAnimationsNameOffset = f.readInt() + mainHeaderOffset;
            int lightAnimationsPointerTableOffset = f.readInt() + mainHeaderOffset;
            int lightAnimationsPointerTableEntries = f.readInt();
            int lightAnimationsNameOffset = f.readInt() + mainHeaderOffset;
            int cameraAnimationsPointerTableOffset = f.readInt() + mainHeaderOffset;
            int cameraAnimationsPointerTableEntries = f.readInt();
            int cameraAnimationsNameOffset = f.readInt() + mainHeaderOffset;
            int fogAnimationsPointerTableOffset = f.readInt() + mainHeaderOffset;
            int fogAnimationsPointerTableEntries = f.readInt();
            int fogAnimationsNameOffset = f.readInt() + mainHeaderOffset;
            int scenePointerTableOffset = f.readInt() + mainHeaderOffset;
            int scenePointerTableEntries = f.readInt();
            int sceneNameOffset = f.readInt() + mainHeaderOffset;

            // Model data
            for (int modelIndex = 0; modelIndex < modelsPointerTableEntries; modelIndex++)
            {
                f.seek(modelsPointerTableOffset + (modelIndex * 4));
                int objectsHeaderOffset = f.readInt() + mainHeaderOffset;

                // Objects
                f.seek(objectsHeaderOffset);
                BCH_Model model = new BCH_Model();

                model.flags = f.readByte();
                model.skeletonScaleType = f.readByte();
                model.silhouetteMaterialEntries = f.readShort();

                model.worldTransform = new Matrix4(f.readFloat(), f.readFloat(), f.readFloat(), f.readFloat()
                    , f.readFloat(), f.readFloat(), f.readFloat(), f.readFloat()
                    , f.readFloat(), f.readFloat(), f.readFloat(), f.readFloat()
                    , 0, 0, 0, 1);

                int materialsTableOffset = f.readInt() + mainHeaderOffset;
                int materialsTableEntries = f.readInt();
                int materialsNamesOffset = f.readInt() + mainHeaderOffset;
                int verticesTableOffset = f.readInt() + mainHeaderOffset;
                int verticesTableEntries = f.readInt();
                f.skip(0x28);
                int skeletonOffset = f.readInt() + mainHeaderOffset;
                int skeletonEntries = f.readInt();
                int skeletonNameOffset = f.readInt() + mainHeaderOffset;
                int objectsNodeVisibilityOffset = f.readInt() + mainHeaderOffset;
                int objectsNodeCount = f.readInt();
                model.name = f.readString(f.readInt() + stringTableOffset, -1);
                int objectsNodeNameEntries = f.readInt();
                int objectsNodeNameOffset = f.readInt() + mainHeaderOffset;
                f.readInt(); //0x0
                int metaDataPointerOffset = f.readInt() + mainHeaderOffset;

                string[] objectName = new string[objectsNodeNameEntries];
                f.seek(objectsNodeNameOffset);
                int rootReferenceBit = f.readInt(); //Radix tree
                int rootLeftNode = f.readShort();
                int rootRightNode = f.readShort();
                int rootNameOffset = f.readInt() + mainHeaderOffset;

                for (int i = 0; i < objectsNodeNameEntries; i++)
                {
                    int referenceBit = f.readInt();
                    short leftNode = (short)f.readShort();
                    short rightNode = (short)f.readShort();
                    objectName[i] = f.readString(f.readInt(), -1);
                    Debug.WriteLine(objectName[i]);
                }

                //Skeleton
                f.seek(skeletonOffset);
                for (int index = 0; index < skeletonEntries; index++)
                {
                    Bone bone = new Smash_Forge.Bone();
                    int boneFlags = f.readInt();
                    bone.parentIndex = (short)f.readShort();
                    short boneSpace = (short)f.readShort();
                    bone.scale = new float[3];
                    bone.rotation = new float[3];
                    bone.position = new float[3];
                    bone.scale[0] = f.readFloat();
                    bone.scale[1] = f.readFloat();
                    bone.scale[2] = f.readFloat();
                    bone.position[0] = f.readFloat();
                    bone.position[1] = f.readFloat();
                    bone.position[2] = f.readFloat();
                    bone.rotation[0] = f.readFloat();
                    bone.rotation[1] = f.readFloat();
                    bone.rotation[2] = f.readFloat();

                    // bone matrix... not really needed to be stored per say
                    f.skip(4*4*3);

                    bone.boneName = f.readString(f.readInt() + stringTableOffset, -1).ToCharArray();

                    f.skip(4); // Meta data

                    model.skeleton.bones.Add(bone);
                }
                model.skeleton.reset();
            }
        }

        public override byte[] Rebuild()
        {
            throw new NotImplementedException();
        }

        public class BCH_Texture
        {
            public int width, height, type;
            public byte[] data;
        }

        public class BCH_Model
        {
            public int flags;
            public int skeletonScaleType;
            public int silhouetteMaterialEntries;

            public VBN skeleton = new VBN();

            public Matrix4 worldTransform;

            public string name;
        }
    }
}