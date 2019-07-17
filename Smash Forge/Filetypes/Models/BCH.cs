using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK;

namespace SmashForge
{
    /*
     * Everything in this class is adapted from gdkchan's Ohana3DS
     * This version will ONLY support Smash-style BCHs
     * Really WIP
     */
    public class BCH_Old : FileBase
    {

        public override Endianness Endian { get; set; }

        public MBN mbn;
        public List<BCH_Model> models = new List<BCH_Model>();
        public Dictionary<string, BchTexture> textures = new Dictionary<string, BchTexture>();
        public List<TreeNode> tree = new List<TreeNode>();
        public VBN bones = new VBN();


        #region Reading

        public override void Read(string filename)
        {
            bchHeader header = new bchHeader();
            FileData f = new FileData(filename);
            f.endian = System.IO.Endianness.Little;

            f.Skip(4);
            header.backwardCompatibility = f.ReadByte();
            header.forwardCompatibility = f.ReadByte();
            header.version = f.ReadUShort();

            header.mainHeaderOffset = f.ReadInt();
            header.stringTableOffset = f.ReadInt();
            header.gpuCommandsOffset = f.ReadInt();
            header.dataOffset = f.ReadInt();
            if (header.backwardCompatibility > 0x20) header.dataExtendedOffset = f.ReadInt();
            header.relocationTableOffset = f.ReadInt();

            header.mainHeaderLength = f.ReadInt();
            header.stringTableLength = f.ReadInt();
            header.gpuCommandsLength = f.ReadInt();
            header.dataLength = f.ReadInt();
            if (header.backwardCompatibility > 0x20) header.dataExtendedLength = f.ReadInt();
            header.relocationTableLength = f.ReadInt();

            header.uninitializedDataSectionLength = f.ReadInt();
            header.uninitializedDescriptionSectionLength = f.ReadInt();

            if (header.backwardCompatibility > 7)
            {
                header.flags = f.ReadUShort();
                header.addressCount = f.ReadUShort();
            }

            // Relocation table
            for (int i = 0; i < header.relocationTableLength; i += 4)
            {
                f.Seek(header.relocationTableOffset + i);
                int val = f.ReadInt();
                int off = val & 0x1FFFFFF;
                byte flag = (byte)(val >> 25);

                switch (flag)
                {
                    case 0:
                        f.Seek((off * 4) + header.mainHeaderOffset);
                        f.WriteInt((off * 4) + header.mainHeaderOffset, f.ReadInt() + header.mainHeaderOffset);
                        break;

                    case 1:
                        f.Seek(off + header.mainHeaderOffset);
                        f.WriteInt((off) + header.mainHeaderOffset, f.ReadInt() + header.stringTableOffset);
                        break;

                    case 2:
                        f.Seek((off * 4) + header.mainHeaderOffset);
                        f.WriteInt((off * 4) + header.mainHeaderOffset, f.ReadInt() + header.gpuCommandsOffset);
                        break;

                    case 0xc:
                        f.Seek((off * 4) + header.mainHeaderOffset);
                        f.WriteInt((off * 4) + header.mainHeaderOffset, f.ReadInt() + header.dataOffset);
                        break;
                }

                f.Seek((off * 4) + header.gpuCommandsOffset);
                if (header.backwardCompatibility < 6)
                {
                    switch (flag)
                    {
                        case 0x23: f.WriteInt((off * 4) + header.gpuCommandsOffset, f.ReadInt() + header.dataOffset); break; //Texture
                        case 0x25: f.WriteInt((off * 4) + header.gpuCommandsOffset, f.ReadInt() + header.dataOffset); break; //Vertex
                        //case 0x26: f.writeInt((off * 4) + header.gpuCommandsOffset, ((f.readInt() + header.dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode
                        case 0x27: f.WriteInt((off * 4) + header.gpuCommandsOffset, (f.ReadInt() + header.dataOffset) & 0x7fffffff); break; //Index 8 bits mode
                    }
                }
                else if (header.backwardCompatibility < 8)
                {
                    switch (flag)
                    {
                        case 0x24: f.WriteInt((off * 4) + header.gpuCommandsOffset, f.ReadInt() + header.dataOffset); break; //Texture
                        case 0x26: f.WriteInt((off * 4) + header.gpuCommandsOffset, f.ReadInt() + header.dataOffset); break; //Vertex
                        //case 0x27: writer.Write(((peek(input) + header.dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode
                        case 0x28: f.WriteInt((off * 4) + header.gpuCommandsOffset, (f.ReadInt() + header.dataOffset) & 0x7fffffff); break; //Index 8 bits mode
                    }
                }
                else if (header.backwardCompatibility < 0x21)
                {
                    switch (flag)
                    {
                        case 0x25: f.WriteInt((off * 4) + header.gpuCommandsOffset, f.ReadInt() + header.dataOffset); break; //Texture
                        case 0x27: f.WriteInt((off * 4) + header.gpuCommandsOffset, f.ReadInt() + header.dataOffset); break; //Vertex
                        //case 0x28: writer.Write(((peek(input) + header.dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode
                        case 0x29: f.WriteInt((off * 4) + header.gpuCommandsOffset, (f.ReadInt() + header.dataOffset) & 0x7fffffff); break; //Index 8 bits mode
                    }
                }
                else
                {
                    switch (flag)
                    {
                        case 0x25: f.WriteInt((off * 4) + header.gpuCommandsOffset, f.ReadInt() + header.dataOffset); break; //Texture
                        case 0x26: f.WriteInt((off * 4) + header.gpuCommandsOffset, f.ReadInt() + header.dataOffset); break; //Vertex relative to Data Offset
                        //case 0x27: writer.Write(((peek(input) + header.dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode relative to Data Offset
                        case 0x28: f.WriteInt((off * 4) + header.gpuCommandsOffset, (f.ReadInt() + header.dataOffset) & 0x7fffffff); break; //Index 8 bits mode relative to Data Offset
                        case 0x2b: f.WriteInt((off * 4) + header.gpuCommandsOffset, f.ReadInt() + header.dataExtendedOffset); break; //Vertex relative to Data Extended Offset
                        //case 0x2c: writer.Write(((peek(input) + header.dataExtendedOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode relative to Data Extended Offset
                        case 0x2d: f.WriteInt((off * 4) + header.gpuCommandsOffset, (f.ReadInt() + header.dataExtendedOffset) & 0x7fffffff); break; //Index 8 bits mode relative to Data Extended Offset
                    }
                }

            }


            // Content Header
            f.Seek(header.mainHeaderOffset);
            bchContentHeader content = new bchContentHeader();
            {
                content.modelsPointerTableOffset = f.ReadInt();
                content.modelsPointerTableEntries = f.ReadInt();
                content.modelsNameOffset = f.ReadInt();
                content.materialsPointerTableOffset = f.ReadInt();
                content.materialsPointerTableEntries = f.ReadInt();
                content.materialsNameOffset = f.ReadInt();
                content.shadersPointerTableOffset = f.ReadInt();
                content.shadersPointerTableEntries = f.ReadInt();
                content.shadersNameOffset = f.ReadInt();
                content.texturesPointerTableOffset = f.ReadInt();
                content.texturesPointerTableEntries = f.ReadInt();
                content.texturesNameOffset = f.ReadInt();
                content.materialsLUTPointerTableOffset = f.ReadInt();
                content.materialsLUTPointerTableEntries = f.ReadInt();
                content.materialsLUTNameOffset = f.ReadInt();
                content.lightsPointerTableOffset = f.ReadInt();
                content.lightsPointerTableEntries = f.ReadInt();
                content.lightsNameOffset = f.ReadInt();
                content.camerasPointerTableOffset = f.ReadInt();
                content.camerasPointerTableEntries = f.ReadInt();
                content.camerasNameOffset = f.ReadInt();
                content.fogsPointerTableOffset = f.ReadInt();
                content.fogsPointerTableEntries = f.ReadInt();
                content.fogsNameOffset = f.ReadInt();
                content.skeletalAnimationsPointerTableOffset = f.ReadInt();
                content.skeletalAnimationsPointerTableEntries = f.ReadInt();
                content.skeletalAnimationsNameOffset = f.ReadInt();
                content.materialAnimationsPointerTableOffset = f.ReadInt();
                content.materialAnimationsPointerTableEntries = f.ReadInt();
                content.materialAnimationsNameOffset = f.ReadInt();
                content.visibilityAnimationsPointerTableOffset = f.ReadInt();
                content.visibilityAnimationsPointerTableEntries = f.ReadInt();
                content.visibilityAnimationsNameOffset = f.ReadInt();
                content.lightAnimationsPointerTableOffset = f.ReadInt();
                content.lightAnimationsPointerTableEntries = f.ReadInt();
                content.lightAnimationsNameOffset = f.ReadInt();
                content.cameraAnimationsPointerTableOffset = f.ReadInt();
                content.cameraAnimationsPointerTableEntries = f.ReadInt();
                content.cameraAnimationsNameOffset = f.ReadInt();
                content.fogAnimationsPointerTableOffset = f.ReadInt();
                content.fogAnimationsPointerTableEntries = f.ReadInt();
                content.fogAnimationsNameOffset = f.ReadInt();
                content.scenePointerTableOffset = f.ReadInt();
                content.scenePointerTableEntries = f.ReadInt();
                content.sceneNameOffset = f.ReadInt();
            }


            //Skeletal animation
            for (int index1 = 0; index1 < content.skeletalAnimationsPointerTableEntries; index1++)
            {
                f.Seek(content.skeletalAnimationsPointerTableOffset + (index1 * 4));
                int dataOffset = f.ReadInt();
                f.Seek(dataOffset);


                string skeletalAnimationName = f.ReadString(f.ReadInt(), -1);
                int animationFlags = f.ReadInt();
                //int skeletalAnimationloopMode = f.readByte();  //pas �a du tout
                float skeletalAnimationframeSize = f.ReadFloat();
                int boneTableOffset = f.ReadInt();
                int boneTableEntries = f.ReadInt();
                int metaDataPointerOffset = f.ReadInt();

                //Debug.WriteLine("Animation Name: " + skeletalAnimationName);
                //Debug.WriteLine("BonetableOffset: " + boneTableOffset.ToString("X"));
                //Debug.WriteLine("BonetableEntry: " + boneTableEntries.ToString("X"));

                for (int i = 0; i < boneTableEntries; i++)
                {
                    f.Seek(boneTableOffset + (i * 4));
                    int offset = f.ReadInt();

                    OSkeletalAnimationBone bone = new OSkeletalAnimationBone();

                    f.Seek(offset);
                    bone.name = f.ReadString(f.ReadInt(), -1);
                    Console.WriteLine("Bone Name: " + bone.name);
                    int animationTypeFlags = f.ReadInt();
                    int flags = f.ReadInt();

                    OSegmentType segmentType = (OSegmentType)((animationTypeFlags >> 16) & 0xf);
                    switch (segmentType)
                    {
                        case OSegmentType.transform:
                            f.Seek(offset + 0x18);

                            int notExistMask = 0x80000;
                            int constantMask = 0x200;

                            for (int j = 0; j < 2; j++)
                            {
                                for (int axis = 0; axis < 3; axis++)
                                {
                                    bool notExist = (flags & notExistMask) > 0;
                                    bool constant = (flags & constantMask) > 0;

                                    OAnimationKeyFrameGroup frame = new OAnimationKeyFrameGroup();
                                    frame.exists = !notExist;
                                    if (frame.exists)
                                    {
                                        if (constant)
                                        {
                                            frame.interpolation = OInterpolationMode.linear;
                                            frame.keyFrames.Add(new OAnimationKeyFrame(f.ReadFloat(), 0));
                                        }
                                        else
                                        {
                                            int frameOffset = f.ReadInt();
                                            int position = f.Pos();
                                            f.Seek(frameOffset);
                                            //getAnimationKeyFrame(input, frame);
                                            f.Seek(position);
                                        }
                                    }
                                    else
                                        f.Seek(f.Pos() + 0x04);

                                    if (j == 0)
                                    {
                                        switch (axis)
                                        {
                                            case 0: bone.rotationX = frame; break;
                                            case 1: bone.rotationY = frame; break;
                                            case 2: bone.rotationZ = frame; break;
                                        }
                                    }
                                    else
                                    {
                                        switch (axis)
                                        {
                                            case 0: bone.translationX = frame; break;
                                            case 1: bone.translationY = frame; break;
                                            case 2: bone.translationZ = frame; break;
                                        }
                                    }

                                    notExistMask <<= 1;
                                    constantMask <<= 1;
                                }

                                constantMask <<= 1;
                            }

                            break;
                        case OSegmentType.transformQuaternion:
                            bone.isFrameFormat = true;

                            int scaleOffset = f.ReadInt();
                            int rotationOffset = f.ReadInt();
                            int translationOffset = f.ReadInt();

                            if ((flags & 0x20) == 0)
                            {
                                bone.scale.exists = true;
                                f.Seek(scaleOffset);

                                if ((flags & 4) > 0)
                                {
                                    bone.scale.vector.Add(new Vector4(
                                        f.ReadFloat(),
                                        f.ReadFloat(),
                                        f.ReadFloat(),
                                        0));
                                }
                                else
                                {
                                    bone.scale.startFrame = f.ReadFloat();
                                    bone.scale.endFrame = f.ReadFloat();

                                    int scaleFlags = f.ReadInt();
                                    int scaleDataOffset = f.ReadInt();
                                    int scaleEntries = f.ReadInt();

                                    f.Seek(scaleDataOffset);
                                    for (int j = 0; j < scaleEntries; j++)
                                    {
                                        bone.scale.vector.Add(new Vector4(
                                            f.ReadFloat(),
                                            f.ReadFloat(),
                                            f.ReadFloat(),
                                            0));
                                    }
                                }
                            }

                            if ((flags & 0x10) == 0)
                            {
                                bone.rotationQuaternion.exists = true;
                                f.Seek(rotationOffset);

                                if ((flags & 2) > 0)
                                {
                                    bone.rotationQuaternion.vector.Add(new Vector4(
                                        f.ReadFloat(),
                                        f.ReadFloat(),
                                        f.ReadFloat(),
                                        f.ReadFloat()));
                                }
                                else
                                {
                                    bone.rotationQuaternion.startFrame = f.ReadFloat();
                                    bone.rotationQuaternion.endFrame = f.ReadFloat();

                                    int rotationFlags = f.ReadInt();
                                    int rotationDataOffset = f.ReadInt();
                                    int rotationEntries = f.ReadInt();

                                    f.Seek(rotationDataOffset);
                                    for (int j = 0; j < rotationEntries; j++)
                                    {
                                        bone.rotationQuaternion.vector.Add(new Vector4(
                                            f.ReadFloat(),
                                            f.ReadFloat(),
                                            f.ReadFloat(),
                                            f.ReadFloat()));
                                    }
                                }
                            }

                            if ((flags & 8) == 0)
                            {
                                bone.translation.exists = true;
                                f.Seek(translationOffset);

                                if ((flags & 1) > 0)
                                {
                                    bone.translation.vector.Add(new Vector4(
                                        f.ReadFloat(),
                                        f.ReadFloat(),
                                        f.ReadFloat(),
                                        0));
                                }
                                else
                                {
                                    bone.translation.startFrame = f.ReadFloat();
                                    bone.translation.endFrame = f.ReadFloat();

                                    int translationFlags = f.ReadInt();
                                    int translationDataOffset = f.ReadInt();
                                    int translationEntries = f.ReadInt();

                                    f.Seek(translationDataOffset);
                                    for (int j = 0; j < translationEntries; j++)
                                    {
                                        bone.translation.vector.Add(new Vector4(
                                            f.ReadFloat(),
                                            f.ReadFloat(),
                                            f.ReadFloat(),
                                            0));
                                    }
                                }
                            }

                            break;
                        case OSegmentType.transformMatrix:
                            bone.isFullBakedFormat = true;

                            f.ReadInt();
                            f.ReadInt();
                            int matrixOffset = f.ReadInt();
                            int entries = f.ReadInt();

                            f.Seek(matrixOffset);
                            for (int j = 0; j < entries; j++)
                            {
                                /*OMatrix transform = new OMatrix();
                                transform.M11 = f.readFloat();
                                transform.M21 = f.readFloat();
                                transform.M31 = f.readFloat();
                                transform.M41 = f.readFloat();

                                transform.M12 = f.readFloat();
                                transform.M22 = f.readFloat();
                                transform.M32 = f.readFloat();
                                transform.M42 = f.readFloat();

                                transform.M13 = f.readFloat();
                                transform.M23 = f.readFloat();
                                transform.M33 = f.readFloat();
                                transform.M43 = f.readFloat();

                                bone.transform.Add(transform);*/
                            }

                            break;
                        default: throw new Exception(string.Format("BCH: Unknow Segment Type {0} on Skeletal Animation bone {1}! STOP!", segmentType, bone.name));
                    }

                    //skeletalAnimation.bone.Add(bone);
                }





            }

            //Shaders (unused for now, until someone wants to add them)
            for (int index = 0; index < content.shadersPointerTableEntries; index++)
            {
                f.Seek(content.shadersPointerTableOffset + (index * 4));
                int dataOffset = f.ReadInt();
                f.Seek(dataOffset);

                int shaderDataOffset = f.ReadInt();
                int shaderDataLength = f.ReadInt();
            }

            // Textures
            // WIP Section
            for (int index = 0; index < content.texturesPointerTableEntries; index++)
            {
                f.Seek(content.texturesPointerTableOffset + (index * 4));
                int dOffset = f.ReadInt();
                f.Seek(dOffset);

                int textureCommandsOffset = f.ReadInt();
                int textureCommandsWordCount = f.ReadInt();

                f.Seek(f.Pos() + 0x14);
                String textureName = f.ReadString(f.ReadInt(), -1);
                f.Seek(textureCommandsOffset);
                BchTexture tex = new BchTexture();
                textures.Add(textureName, tex);

                tex.height = f.ReadUShort();
                tex.width = f.ReadUShort();
                f.Skip(12);
                int doffset = f.ReadInt();
                f.Skip(4);
                tex.type = f.ReadInt();
                tex.data = f.GetSection(doffset, f.Size() - doffset);

                tex.texture = _3DS.DecodeImage(tex.data, tex.width, tex.height, (_3DS.Tex_Formats)tex.type);
                //Texture texture = new Texture2D(tex.texture);
                //tex.display = texture.Id;
            }

            // Model data

            for (int modelIndex = 0; modelIndex < content.modelsPointerTableEntries; modelIndex++)
            {
                f.Seek(content.modelsPointerTableOffset + (modelIndex * 4));
                int objectsHeaderOffset = f.ReadInt();

                // Objects
                f.Seek(objectsHeaderOffset);
                BCH_Model model = new BCH_Model();
                models.Add(model);

                model.flags = f.ReadByte();
                model.skeletonScaleType = f.ReadByte();
                model.silhouetteMaterialEntries = f.ReadUShort();

                model.worldTransform = new Matrix4(f.ReadFloat(), f.ReadFloat(), f.ReadFloat(), f.ReadFloat()
                    , f.ReadFloat(), f.ReadFloat(), f.ReadFloat(), f.ReadFloat()
                    , f.ReadFloat(), f.ReadFloat(), f.ReadFloat(), f.ReadFloat()
                    , 0, 0, 0, 1);

                int materialsTableOffset = f.ReadInt();
                int materialsTableEntries = f.ReadInt();
                int materialsNameOffset = f.ReadInt();
                int verticesTableOffset = f.ReadInt();
                int verticesTableEntries = f.ReadInt();
                f.Skip(0x28);
                int skeletonOffset = f.ReadInt();
                int skeletonEntries = f.ReadInt();
                int skeletonNameOffset = f.ReadInt();
                int objectsNodeVisibilityOffset = f.ReadInt();
                int objectsNodeCount = f.ReadInt();
                String name = f.ReadString(f.ReadInt(), -1);
                int objectsNodeNameEntries = f.ReadInt();
                int objectsNodeNameOffset = f.ReadInt();
                f.ReadInt(); //0x0
                int metaDataPointerOffset = f.ReadInt();

                f.Seek(objectsNodeVisibilityOffset);
                int nodeVisibility = f.ReadInt();

                string[] objectName = new string[objectsNodeNameEntries];
                f.Seek(objectsNodeNameOffset);
                int rootReferenceBit = f.ReadInt(); //Radix tree
                int rootLeftNode = f.ReadUShort();
                int rootRightNode = f.ReadUShort();
                int rootNameOffset = f.ReadInt();

                for (int i = 0; i < objectsNodeNameEntries; i++)
                {
                    int referenceBit = f.ReadInt();
                    short leftNode = f.ReadShort();
                    short rightNode = f.ReadShort();
                    objectName[i] = f.ReadString(f.ReadInt(), -1);
                }

                // Materials
                // NOTE: MATERIALS AND OBJECT SECTIONS ARE REALLY MESSY ATM

                String[] materialNames = new String[materialsTableEntries];
                for (int index = 0; index < materialsTableEntries; index++)
                {
                    f.Seek(materialsTableOffset + (index * 0x2c));

                    int materialParametersOffset = f.ReadInt();
                    f.ReadInt();
                    f.ReadInt();
                    f.ReadInt();
                    int textureCommandsOffset = f.ReadInt();
                    int textureCommandsWordCount = f.ReadInt();

                    int materialMapperOffset = f.ReadInt();
                    materialNames[index] = f.ReadString(f.ReadInt(), -1);
                }

                // Object Descriptions...
                // Assumes MBN is already loaded for now
                f.Seek(verticesTableOffset);
                List<objDes> objDescriptors = new List<objDes>();
                if (mbn == null)
                {
                    mbn = new MBN();
                    for (int index = 0; index < verticesTableEntries; index++)
                        mbn.mesh.Add(new MBN.Mesh());
                    mbn.PreRender();
                }
                for (int index = 0; index < mbn.mesh.Count; index++)
                {
                    int i = f.ReadUShort();
                    if (index > mbn.mesh.Count) break;
                    if (i > materialNames.Length) break;
                    mbn.mesh[index].texId = textures[materialNames[i]].display;
                    Console.WriteLine("Tex index" + mbn.mesh[index].texId);
                    f.Skip(2); // flags
                    int nameId = f.ReadUShort();
                    mbn.mesh[index].Text = objectName[nameId];

                    // node visibility TODO: finish...
                    mbn.mesh[index].Checked = ((nodeVisibility & (1 << nameId)) > 0);

                    mbn.mesh[index].renderPriority = f.ReadUShort();

                    objDes des = new objDes();
                    objDescriptors.Add(des);
                    des.vshAttBufferCommandOffset = f.ReadInt();
                    des.vshAttBufferCommandCount = f.ReadInt();
                    des.faceOffset = f.ReadInt();
                    des.faceCount = f.ReadInt();
                    des.vshAttBufferCommandOffsetEx = f.ReadInt();
                    des.vshAttBufferCommandCountEx = f.ReadInt();

                    f.Skip(12);// center vector
                    f.Skip(4); // flagsOffset
                    f.Skip(4); // 0?
                    f.ReadInt(); //bbOffsets[i]
                }

                //Skeleton
                f.Seek(skeletonOffset);
                for (int index = 0; index < skeletonEntries; index++)
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

                    f.Skip(4); // Meta data
                    bones.bones.Add(bone);


                    model.skeleton.bones.Add(bone);
                }
                model.skeleton.reset();
                model.skeleton.update();
            }

        }

        #endregion

        #region Building
        public override byte[] Rebuild()
        {
            throw new NotImplementedException();
            //FileOutput d = new FileOutput(); // data
            //d.Endian = Endianness.Big;

            //return d.getBytes();
        }
        #endregion

        #region Struct/Class
        //------------------------------------------------------------------------------------------------------------------------
        /*
         * Reads the contents of the bch file into this class
         */
        //------------------------------------------------------------------------------------------------------------------------
        // HELPERS FOR READING

        public struct objDes // bchObjectEntry 
        {
            public short materialId;
            public bool isSilhouette;
            public short nodeId;
            public short renderPriority;
            public int vshAttBufferCommandOffset; // vshAttributesBufferCommandsOffset;
            public int vshAttBufferCommandCount;// vshAttributesBufferCommandsWordCount;
            public int faceOffset; // facesHeaderOffset;
            public int faceCount; // facesHeaderEntries;
            public int vshAttBufferCommandOffsetEx; // vshExtraAttributesBufferCommandsOffset;
            public int vshAttBufferCommandCountEx; // vshExtraAttributesBufferCommandsWordCount;
            public Vector3 centerVector;
            public int flagsOffset;
            public int boundingBoxOffset;
        }

        public struct bchHeader
        {
            public string magic;
            public int backwardCompatibility;
            public int forwardCompatibility;
            public int version;

            public int mainHeaderOffset;
            public int stringTableOffset;
            public int gpuCommandsOffset;
            public int dataOffset;
            public int dataExtendedOffset;
            public int relocationTableOffset;

            public int mainHeaderLength;
            public int stringTableLength;
            public int gpuCommandsLength;
            public int dataLength;
            public int dataExtendedLength;
            public int relocationTableLength;
            public int uninitializedDataSectionLength;
            public int uninitializedDescriptionSectionLength;

            public int flags;
            public int addressCount;
        }

        private struct bchContentHeader
        {
            public int modelsPointerTableOffset;
            public int modelsPointerTableEntries;
            public int modelsNameOffset;
            public int materialsPointerTableOffset;
            public int materialsPointerTableEntries;
            public int materialsNameOffset;
            public int shadersPointerTableOffset;
            public int shadersPointerTableEntries;
            public int shadersNameOffset;
            public int texturesPointerTableOffset;
            public int texturesPointerTableEntries;
            public int texturesNameOffset;
            public int materialsLUTPointerTableOffset;
            public int materialsLUTPointerTableEntries;
            public int materialsLUTNameOffset;
            public int lightsPointerTableOffset;
            public int lightsPointerTableEntries;
            public int lightsNameOffset;
            public int camerasPointerTableOffset;
            public int camerasPointerTableEntries;
            public int camerasNameOffset;
            public int fogsPointerTableOffset;
            public int fogsPointerTableEntries;
            public int fogsNameOffset;
            public int skeletalAnimationsPointerTableOffset;
            public int skeletalAnimationsPointerTableEntries;
            public int skeletalAnimationsNameOffset;
            public int materialAnimationsPointerTableOffset;
            public int materialAnimationsPointerTableEntries;
            public int materialAnimationsNameOffset;
            public int visibilityAnimationsPointerTableOffset;
            public int visibilityAnimationsPointerTableEntries;
            public int visibilityAnimationsNameOffset;
            public int lightAnimationsPointerTableOffset;
            public int lightAnimationsPointerTableEntries;
            public int lightAnimationsNameOffset;
            public int cameraAnimationsPointerTableOffset;
            public int cameraAnimationsPointerTableEntries;
            public int cameraAnimationsNameOffset;
            public int fogAnimationsPointerTableOffset;
            public int fogAnimationsPointerTableEntries;
            public int fogAnimationsNameOffset;
            public int scenePointerTableOffset;
            public int scenePointerTableEntries;
            public int sceneNameOffset;
        }

        public class OSkeletalAnimationBone
        {
            public string name;

            public OAnimationKeyFrameGroup scaleX, scaleY, scaleZ;
            public OAnimationKeyFrameGroup rotationX, rotationY, rotationZ;
            public OAnimationKeyFrameGroup translationX, translationY, translationZ;
            public bool isAxisAngle;

            public OAnimationFrame rotationQuaternion;
            public OAnimationFrame translation;
            public OAnimationFrame scale;
            public bool isFrameFormat;

            //public List<OMatrix> transform;
            public bool isFullBakedFormat;

            public OSkeletalAnimationBone()
            {
                scaleX = new OAnimationKeyFrameGroup();
                scaleY = new OAnimationKeyFrameGroup();
                scaleZ = new OAnimationKeyFrameGroup();

                rotationX = new OAnimationKeyFrameGroup();
                rotationY = new OAnimationKeyFrameGroup();
                rotationZ = new OAnimationKeyFrameGroup();

                translationX = new OAnimationKeyFrameGroup();
                translationY = new OAnimationKeyFrameGroup();
                translationZ = new OAnimationKeyFrameGroup();

                rotationQuaternion = new OAnimationFrame();
                translation = new OAnimationFrame();
                scale = new OAnimationFrame();

                //transform = new List<OMatrix>();
            }
        }

        public class OAnimationFrame
        {
            public List<Vector4> vector;
            public float startFrame, endFrame;
            public bool exists;

            public ORepeatMethod preRepeat;
            public ORepeatMethod postRepeat;

        }


        /// <summary>
        ///     Interpolation mode of the animation.
        ///     Step = Jump from key frames, like the big pointer of a clock.
        ///     Linear =  Linear interpolation between values.
        ///     Hermite = Hermite interpolation between values, have two slope values too.
        /// </summary>
        public enum OInterpolationMode
        {
            step = 0,
            linear = 1,
            hermite = 2
        }

        /// <summary>
        ///     Key frame of an animation.
        /// </summary>
        public class OAnimationKeyFrameGroup
        {
            public List<OAnimationKeyFrame> keyFrames;
            public OInterpolationMode interpolation;
            public float startFrame, endFrame;
            public bool exists;
            public bool defaultValue;

            public ORepeatMethod preRepeat;
            public ORepeatMethod postRepeat;

            public OAnimationKeyFrameGroup()
            {
                keyFrames = new List<OAnimationKeyFrame>();
            }
        }

        public class OAnimationKeyFrame
        {
            public float frame;
            public float value;
            public float inSlope;
            public float outSlope;
            public bool bValue;

            /// <summary>
            ///     Creates a new Key Frame.
            ///     This Key Frame can be used on Hermite Interpolation.
            /// </summary>
            /// <param name="_value">The point value</param>
            /// <param name="_inSlope">The input slope</param>
            /// <param name="_outSlope">The output slope</param>
            /// <param name="_frame">The frame number</param>
            public OAnimationKeyFrame(float _value, float _inSlope, float _outSlope, float _frame)
            {
                value = _value;
                inSlope = _inSlope;
                outSlope = _outSlope;
                frame = _frame;
            }

            /// <summary>
            ///     Creates a new Key Frame.
            ///     This Key Frame can be used on Linear or Step interpolation.
            /// </summary>
            /// <param name="_value">The point value</param>
            /// <param name="_frame">The frame number</param>
            public OAnimationKeyFrame(float _value, float _frame)
            {
                value = _value;
                frame = _frame;
            }

            /// <summary>
            ///     Creates a new Key Frame.
            ///     This Key Frame can be used on Boolean values animation.
            /// </summary>
            /// <param name="_value">The point value</param>
            /// <param name="_frame">The frame number</param>
            public OAnimationKeyFrame(bool _value, float _frame)
            {
                bValue = _value;
                frame = _frame;
            }

            /// <summary>
            ///     Creates a new Key Frame.
            /// </summary>
            public OAnimationKeyFrame()
            {
            }

            public override string ToString()
            {
                return string.Format("Frame:{0}; Value (float):{1}; Value (boolean):{2}; InSlope:{3}; OutSlope:{4}", frame, value, bValue, inSlope, outSlope);
            }
        }




        public enum ORepeatMethod
        {
            none = 0,
            repeat = 1,
            mirror = 2,
            relativeRepeat = 3
        }

        public enum OSegmentType
        {
            single = 0,
            vector2 = 2,
            vector3 = 3,
            transform = 4,
            rgbaColor = 5,
            integer = 6,
            transformQuaternion = 7,
            boolean = 8,
            transformMatrix = 9
        }

        #endregion

    }
}