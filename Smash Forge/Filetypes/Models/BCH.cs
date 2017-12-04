using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using OpenTK;
using System.Drawing;

namespace Smash_Forge
{
    /*
     * Everything in this class is adapted from gdkchan's Ohana3DS
     * This version will ONLY support Smash-style BCHs
     * Really WIP
     */
    public class BCH : FileBase
    {

        public override Endianness Endian { get; set; }

        public MBN mbn;
        public List<BCH_Model> models = new List<BCH_Model>();
        public Dictionary<string, BCH_Texture> textures = new Dictionary<string, BCH_Texture>();
        public List<TreeNode> tree = new List<TreeNode>();
        public VBN bones = new VBN();


        #region Reading

        public override void Read(string filename)
        {
            bchHeader header = new bchHeader();
            FileData f = new FileData(filename);
            f.Endian = System.IO.Endianness.Little;

            f.skip(4);
            header.backwardCompatibility = f.readByte();
            header.forwardCompatibility = f.readByte();
            header.version = f.readShort();

            header.mainHeaderOffset = f.readInt();
            header.stringTableOffset = f.readInt();
            header.gpuCommandsOffset = f.readInt();
            header.dataOffset = f.readInt();
            if (header.backwardCompatibility > 0x20) header.dataExtendedOffset = f.readInt();
            header.relocationTableOffset = f.readInt();

            header.mainHeaderLength = f.readInt();
            header.stringTableLength = f.readInt();
            header.gpuCommandsLength = f.readInt();
            header.dataLength = f.readInt();
            if (header.backwardCompatibility > 0x20) header.dataExtendedLength = f.readInt();
            header.relocationTableLength = f.readInt();

            header.uninitializedDataSectionLength = f.readInt();
            header.uninitializedDescriptionSectionLength = f.readInt();

            if (header.backwardCompatibility > 7)
            {
                header.flags = f.readShort();
                header.addressCount = f.readShort();
            }

            // Relocation table
            for (int i = 0; i < header.relocationTableLength; i += 4)
            {
                f.seek(header.relocationTableOffset + i);
                int val = f.readInt();
                int off = val & 0x1FFFFFF;
                byte flag = (byte)(val >> 25);

                switch (flag)
                {
                    case 0:
                        f.seek((off * 4) + header.mainHeaderOffset);
                        f.writeInt((off * 4) + header.mainHeaderOffset, f.readInt() + header.mainHeaderOffset);
                        break;

                    case 1:
                        f.seek(off + header.mainHeaderOffset);
                        f.writeInt((off) + header.mainHeaderOffset, f.readInt() + header.stringTableOffset);
                        break;

                    case 2:
                        f.seek((off * 4) + header.mainHeaderOffset);
                        f.writeInt((off * 4) + header.mainHeaderOffset, f.readInt() + header.gpuCommandsOffset);
                        break;

                    case 0xc:
                        f.seek((off * 4) + header.mainHeaderOffset);
                        f.writeInt((off * 4) + header.mainHeaderOffset, f.readInt() + header.dataOffset);
                        break;
                }

                f.seek((off * 4) + header.gpuCommandsOffset);
                if (header.backwardCompatibility < 6)
                {
                    switch (flag)
                    {
                        case 0x23: f.writeInt((off * 4) + header.gpuCommandsOffset, f.readInt() + header.dataOffset); break; //Texture
                        case 0x25: f.writeInt((off * 4) + header.gpuCommandsOffset, f.readInt() + header.dataOffset); break; //Vertex
                        //case 0x26: f.writeInt((off * 4) + header.gpuCommandsOffset, ((f.readInt() + header.dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode
                        case 0x27: f.writeInt((off * 4) + header.gpuCommandsOffset, (f.readInt() + header.dataOffset) & 0x7fffffff); break; //Index 8 bits mode
                    }
                }
                else if (header.backwardCompatibility < 8)
                {
                    switch (flag)
                    {
                        case 0x24: f.writeInt((off * 4) + header.gpuCommandsOffset, f.readInt() + header.dataOffset); break; //Texture
                        case 0x26: f.writeInt((off * 4) + header.gpuCommandsOffset, f.readInt() + header.dataOffset); break; //Vertex
                        //case 0x27: writer.Write(((peek(input) + header.dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode
                        case 0x28: f.writeInt((off * 4) + header.gpuCommandsOffset, (f.readInt() + header.dataOffset) & 0x7fffffff); break; //Index 8 bits mode
                    }
                }
                else if (header.backwardCompatibility < 0x21)
                {
                    switch (flag)
                    {
                        case 0x25: f.writeInt((off * 4) + header.gpuCommandsOffset, f.readInt() + header.dataOffset); break; //Texture
                        case 0x27: f.writeInt((off * 4) + header.gpuCommandsOffset, f.readInt() + header.dataOffset); break; //Vertex
                        //case 0x28: writer.Write(((peek(input) + header.dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode
                        case 0x29: f.writeInt((off * 4) + header.gpuCommandsOffset, (f.readInt() + header.dataOffset) & 0x7fffffff); break; //Index 8 bits mode
                    }
                }
                else
                {
                    switch (flag)
                    {
                        case 0x25: f.writeInt((off * 4) + header.gpuCommandsOffset, f.readInt() + header.dataOffset); break; //Texture
                        case 0x26: f.writeInt((off * 4) + header.gpuCommandsOffset, f.readInt() + header.dataOffset); break; //Vertex relative to Data Offset
                        //case 0x27: writer.Write(((peek(input) + header.dataOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode relative to Data Offset
                        case 0x28: f.writeInt((off * 4) + header.gpuCommandsOffset, (f.readInt() + header.dataOffset) & 0x7fffffff); break; //Index 8 bits mode relative to Data Offset
                        case 0x2b: f.writeInt((off * 4) + header.gpuCommandsOffset, f.readInt() + header.dataExtendedOffset); break; //Vertex relative to Data Extended Offset
                        //case 0x2c: writer.Write(((peek(input) + header.dataExtendedOffset) & 0x7fffffff) | 0x80000000); break; //Index 16 bits mode relative to Data Extended Offset
                        case 0x2d: f.writeInt((off * 4) + header.gpuCommandsOffset, (f.readInt() + header.dataExtendedOffset) & 0x7fffffff); break; //Index 8 bits mode relative to Data Extended Offset
                    }
                }

            }


            // Content Header
            f.seek(header.mainHeaderOffset);
            bchContentHeader content = new bchContentHeader();
            {
                content.modelsPointerTableOffset = f.readInt();
                content.modelsPointerTableEntries = f.readInt();
                content.modelsNameOffset = f.readInt();
                content.materialsPointerTableOffset = f.readInt();
                content.materialsPointerTableEntries = f.readInt();
                content.materialsNameOffset = f.readInt();
                content.shadersPointerTableOffset = f.readInt();
                content.shadersPointerTableEntries = f.readInt();
                content.shadersNameOffset = f.readInt();
                content.texturesPointerTableOffset = f.readInt();
                content.texturesPointerTableEntries = f.readInt();
                content.texturesNameOffset = f.readInt();
                content.materialsLUTPointerTableOffset = f.readInt();
                content.materialsLUTPointerTableEntries = f.readInt();
                content.materialsLUTNameOffset = f.readInt();
                content.lightsPointerTableOffset = f.readInt();
                content.lightsPointerTableEntries = f.readInt();
                content.lightsNameOffset = f.readInt();
                content.camerasPointerTableOffset = f.readInt();
                content.camerasPointerTableEntries = f.readInt();
                content.camerasNameOffset = f.readInt();
                content.fogsPointerTableOffset = f.readInt();
                content.fogsPointerTableEntries = f.readInt();
                content.fogsNameOffset = f.readInt();
                content.skeletalAnimationsPointerTableOffset = f.readInt();
                content.skeletalAnimationsPointerTableEntries = f.readInt();
                content.skeletalAnimationsNameOffset = f.readInt();
                content.materialAnimationsPointerTableOffset = f.readInt();
                content.materialAnimationsPointerTableEntries = f.readInt();
                content.materialAnimationsNameOffset = f.readInt();
                content.visibilityAnimationsPointerTableOffset = f.readInt();
                content.visibilityAnimationsPointerTableEntries = f.readInt();
                content.visibilityAnimationsNameOffset = f.readInt();
                content.lightAnimationsPointerTableOffset = f.readInt();
                content.lightAnimationsPointerTableEntries = f.readInt();
                content.lightAnimationsNameOffset = f.readInt();
                content.cameraAnimationsPointerTableOffset = f.readInt();
                content.cameraAnimationsPointerTableEntries = f.readInt();
                content.cameraAnimationsNameOffset = f.readInt();
                content.fogAnimationsPointerTableOffset = f.readInt();
                content.fogAnimationsPointerTableEntries = f.readInt();
                content.fogAnimationsNameOffset = f.readInt();
                content.scenePointerTableOffset = f.readInt();
                content.scenePointerTableEntries = f.readInt();
                content.sceneNameOffset = f.readInt();
            }


            //Skeletal animation
            for (int index1 = 0; index1 < content.skeletalAnimationsPointerTableEntries; index1++)
            {
                f.seek(content.skeletalAnimationsPointerTableOffset + (index1 * 4));
                int dataOffset = f.readInt();
                f.seek(dataOffset);


                string skeletalAnimationName = f.readString(f.readInt(), -1);
                int animationFlags = f.readInt();
                //int skeletalAnimationloopMode = f.readByte();  //pas ça du tout
                float skeletalAnimationframeSize = f.readFloat();
                int boneTableOffset = f.readInt();
                int boneTableEntries = f.readInt();
                int metaDataPointerOffset = f.readInt();

                Debug.WriteLine("Animation Name: " + skeletalAnimationName);
                Debug.WriteLine("BonetableOffset: " + boneTableOffset.ToString("X"));
                Debug.WriteLine("BonetableEntry: " + boneTableEntries.ToString("X"));

                for (int i = 0; i < boneTableEntries; i++)
                {
                    f.seek(boneTableOffset + (i * 4));
                    int offset = f.readInt();

                    OSkeletalAnimationBone bone = new OSkeletalAnimationBone();

                    f.seek(offset);
                    bone.name = f.readString(f.readInt(), -1);
                    Console.WriteLine("Bone Name: " + bone.name);
                    int animationTypeFlags = f.readInt();
                    int flags = f.readInt();

                    OSegmentType segmentType = (OSegmentType)((animationTypeFlags >> 16) & 0xf);
                    switch (segmentType)
                    {
                        case OSegmentType.transform:
                            f.seek(offset + 0x18);

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
                                            frame.keyFrames.Add(new OAnimationKeyFrame(f.readFloat(), 0));
                                        }
                                        else
                                        {
                                            int frameOffset = f.readInt();
                                            int position = f.pos();
                                            f.seek(frameOffset);
                                            //getAnimationKeyFrame(input, frame);
                                            f.seek(position);
                                        }
                                    }
                                    else
                                        f.seek(f.pos() + 0x04);

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

                            int scaleOffset = f.readInt();
                            int rotationOffset = f.readInt();
                            int translationOffset = f.readInt();

                            if ((flags & 0x20) == 0)
                            {
                                bone.scale.exists = true;
                                f.seek(scaleOffset);

                                if ((flags & 4) > 0)
                                {
                                    bone.scale.vector.Add(new Vector4(
                                        f.readFloat(),
                                        f.readFloat(),
                                        f.readFloat(),
                                        0));
                                }
                                else
                                {
                                    bone.scale.startFrame = f.readFloat();
                                    bone.scale.endFrame = f.readFloat();

                                    int scaleFlags = f.readInt();
                                    int scaleDataOffset = f.readInt();
                                    int scaleEntries = f.readInt();

                                    f.seek(scaleDataOffset);
                                    for (int j = 0; j < scaleEntries; j++)
                                    {
                                        bone.scale.vector.Add(new Vector4(
                                            f.readFloat(),
                                            f.readFloat(),
                                            f.readFloat(),
                                            0));
                                    }
                                }
                            }

                            if ((flags & 0x10) == 0)
                            {
                                bone.rotationQuaternion.exists = true;
                                f.seek(rotationOffset);

                                if ((flags & 2) > 0)
                                {
                                    bone.rotationQuaternion.vector.Add(new Vector4(
                                        f.readFloat(),
                                        f.readFloat(),
                                        f.readFloat(),
                                        f.readFloat()));
                                }
                                else
                                {
                                    bone.rotationQuaternion.startFrame = f.readFloat();
                                    bone.rotationQuaternion.endFrame = f.readFloat();

                                    int rotationFlags = f.readInt();
                                    int rotationDataOffset = f.readInt();
                                    int rotationEntries = f.readInt();

                                    f.seek(rotationDataOffset);
                                    for (int j = 0; j < rotationEntries; j++)
                                    {
                                        bone.rotationQuaternion.vector.Add(new Vector4(
                                            f.readFloat(),
                                            f.readFloat(),
                                            f.readFloat(),
                                            f.readFloat()));
                                    }
                                }
                            }

                            if ((flags & 8) == 0)
                            {
                                bone.translation.exists = true;
                                f.seek(translationOffset);

                                if ((flags & 1) > 0)
                                {
                                    bone.translation.vector.Add(new Vector4(
                                        f.readFloat(),
                                        f.readFloat(),
                                        f.readFloat(),
                                        0));
                                }
                                else
                                {
                                    bone.translation.startFrame = f.readFloat();
                                    bone.translation.endFrame = f.readFloat();

                                    int translationFlags = f.readInt();
                                    int translationDataOffset = f.readInt();
                                    int translationEntries = f.readInt();

                                    f.seek(translationDataOffset);
                                    for (int j = 0; j < translationEntries; j++)
                                    {
                                        bone.translation.vector.Add(new Vector4(
                                            f.readFloat(),
                                            f.readFloat(),
                                            f.readFloat(),
                                            0));
                                    }
                                }
                            }

                            break;
                        case OSegmentType.transformMatrix:
                            bone.isFullBakedFormat = true;

                            f.readInt();
                            f.readInt();
                            int matrixOffset = f.readInt();
                            int entries = f.readInt();

                            f.seek(matrixOffset);
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
                f.seek(content.shadersPointerTableOffset + (index * 4));
                int dataOffset = f.readInt();
                f.seek(dataOffset);

                int shaderDataOffset = f.readInt();
                int shaderDataLength = f.readInt();
            }

            // Textures
            // WIP Section
            for (int index = 0; index < content.texturesPointerTableEntries; index++)
            {
                f.seek(content.texturesPointerTableOffset + (index * 4));
                int dOffset = f.readInt();
                f.seek(dOffset);

                int textureCommandsOffset = f.readInt();
                int textureCommandsWordCount = f.readInt();

                f.seek(f.pos() + 0x14);
                String textureName = f.readString(f.readInt(), -1);
                //Debug.WriteLine("gpuCommandOffset: " + header.gpuCommandsOffset.ToString("X"));
                f.seek(textureCommandsOffset);
                //Debug.WriteLine("textureCommandOffset: " + textureCommandsOffset.ToString("X"));
                BCH_Texture tex = new BCH_Texture();
                textures.Add(textureName, tex);

                tex.height = f.readShort();
                tex.width = f.readShort();
                f.skip(12);
                int doffset = f.readInt();
                //Debug.WriteLine("doffset: " + doffset.ToString("X"));
                f.skip(4);
                tex.type = f.readInt();
                tex.data = f.getSection(doffset, f.size() - doffset);

                tex.texture = _3DS.DecodeImage(tex.data, tex.width, tex.height, (_3DS.Tex_Formats)tex.type);
                tex.display = NUT.loadImage(tex.texture);
            }

            // Model data

            for (int modelIndex = 0; modelIndex < content.modelsPointerTableEntries; modelIndex++)
            {
                f.seek(content.modelsPointerTableOffset + (modelIndex * 4));
                int objectsHeaderOffset = f.readInt();

                // Objects
                f.seek(objectsHeaderOffset);
                BCH_Model model = new BCH_Model();
                models.Add(model);

                model.flags = f.readByte();
                model.skeletonScaleType = f.readByte();
                model.silhouetteMaterialEntries = f.readShort();

                model.worldTransform = new Matrix4(f.readFloat(), f.readFloat(), f.readFloat(), f.readFloat()
                    , f.readFloat(), f.readFloat(), f.readFloat(), f.readFloat()
                    , f.readFloat(), f.readFloat(), f.readFloat(), f.readFloat()
                    , 0, 0, 0, 1);

                model.materialsTableOffset = f.readInt();
                model.materialsTableEntries = f.readInt();
                model.materialsNameOffset = f.readInt();
                model.verticesTableOffset = f.readInt();
                //Debug.WriteLine("Mesh Count: " + f.pos().ToString("X"));
                model.verticesTableEntries = f.readInt();
                f.skip(0x28);
                model.skeletonOffset = f.readInt();
                model.skeletonEntries = f.readInt();
                model.skeletonNameOffset = f.readInt();
                model.objectsNodeVisibilityOffset = f.readInt();
                model.objectsNodeCount = f.readInt();
                model.name = f.readString(f.readInt(), -1);
                model.objectsNodeNameEntries = f.readInt();
                model.objectsNodeNameOffset = f.readInt();
                f.readInt(); //0x0
                model.metaDataPointerOffset = f.readInt();

                f.seek(model.objectsNodeVisibilityOffset);
                int nodeVisibility = f.readInt();

                string[] objectName = new string[model.objectsNodeNameEntries];
                f.seek(model.objectsNodeNameOffset);
                int rootReferenceBit = f.readInt(); //Radix tree
                int rootLeftNode = f.readShort();
                int rootRightNode = f.readShort();
                int rootNameOffset = f.readInt();

                for (int i = 0; i < model.objectsNodeNameEntries; i++)
                {
                    int referenceBit = f.readInt();
                    short leftNode = (short)f.readShort();
                    short rightNode = (short)f.readShort();
                    objectName[i] = f.readString(f.readInt(), -1);
                    //Debug.WriteLine(objectName[i]);
                }

                // Materials
                // NOTE: MATERIALS AND OBJECT SECTIONS ARE REALLY MESSY ATM

                String[] materialNames = new String[model.materialsTableEntries];
                for (int index = 0; index < model.materialsTableEntries; index++)
                {
                    f.seek(model.materialsTableOffset + (index * 0x2c));

                    int materialParametersOffset = f.readInt();
                    f.readInt();
                    f.readInt();
                    f.readInt();
                    int textureCommandsOffset = f.readInt();
                    int textureCommandsWordCount = f.readInt();

                    int materialMapperOffset = f.readInt();
                    materialNames[index] = f.readString(f.readInt(), -1);
                }

                // Object Descriptions...
                // Assumes MBN is already loaded for now
                f.seek(model.verticesTableOffset);
                List<objDes> objDescriptors = new List<objDes>();
                Debug.WriteLine(model.name);
                if (mbn == null)
                {
                    mbn = new Smash_Forge.MBN();
                    for (int index = 0; index < model.verticesTableEntries; index++)
                        mbn.mesh.Add(new MBN.Mesh());
                    mbn.PreRender();
                }
                for (int index = 0; index < mbn.mesh.Count; index++)
                {
                    int i = f.readShort();
                    if (index > mbn.mesh.Count) break;
                    if (i > materialNames.Length) break;
                    mbn.mesh[index].texId = textures[materialNames[i]].display;
                    Console.WriteLine("Tex index" + mbn.mesh[index].texId);
                    f.skip(2); // flags
                    int nameId = f.readShort();
                    mbn.mesh[index].Text = objectName[nameId];

                    // node visibility TODO: finish...
                    mbn.mesh[index].Checked = ((nodeVisibility & (1 << nameId)) > 0);

                    mbn.mesh[index].renderPriority = f.readShort();

                    objDes des = new objDes();
                    objDescriptors.Add(des);
                    des.vshAttBufferCommandOffset = f.readInt();
                    des.vshAttBufferCommandCount = f.readInt();
                    des.faceOffset = f.readInt();
                    des.faceCount = f.readInt();
                    des.vshAttBufferCommandOffsetEx = f.readInt();
                    des.vshAttBufferCommandCountEx = f.readInt();

                    f.skip(12);// center vector
                    f.skip(4); // flagsOffset
                    f.skip(4); // 0?
                    f.readInt(); //bbOffsets[i]

                    //Debug.WriteLine(des.vshAttBufferCommandOffset.ToString("X"));
                }

                //Skeleton
                f.seek(model.skeletonOffset);
                for (int index = 0; index < model.skeletonEntries; index++)
                {
                    Bone bone = new Smash_Forge.Bone(model.skeleton);
                    //Bone bone = new Bone(bones);
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

                    f.skip(4); // Meta data
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

        public class BCH_Texture
        {
            public int width, height, type;
            public byte[] data;
            public Bitmap texture;
            // for display only
            public int display = 0;
        }

        public class BCH_Model
        {
            public int flags;
            public int skeletonScaleType;
            public int silhouetteMaterialEntries;

            public VBN skeleton = new VBN();

            public Matrix4 worldTransform;

            public int materialsTableOffset;
            public int materialsTableEntries;
            public int materialsNameOffset;
            public int verticesTableOffset;
            public int verticesTableEntries;
            public int skeletonOffset;
            public int skeletonEntries;
            public int skeletonNameOffset;
            public int objectsNodeVisibilityOffset;
            public int objectsNodeCount;
            public string name; //model name
            public int objectsNodeNameEntries;
            public int objectsNodeNameOffset;
            public int metaDataPointerOffset;
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
