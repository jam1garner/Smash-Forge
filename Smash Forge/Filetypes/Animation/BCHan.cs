using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using System.Windows.Forms;

namespace Smash_Forge
{
    /*
     * Everything in this class is adapted from gdkchan's Ohana3DS
     * This version will ONLY support Smash-style BCHs
     * Really WIP
     */
    public class BCHan
    {

        #region Reading

        public static void Read(string filename)
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
            AnimationGroupNode ThisAnimation = new AnimationGroupNode() { Text = filename};
            MainForm.Instance.animList.treeView1.Nodes.Add(ThisAnimation);

            for (int index1 = 0; index1 < content.skeletalAnimationsPointerTableEntries; index1++)//
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
                
                //Runtime.Animations.Add(skeletalAnimationName, a);
                //MainForm.animNode.Nodes.Add(skeletalAnimationName);
                
                Debug.WriteLine("Animation Name: " + skeletalAnimationName);
                Animation a = new Animation(skeletalAnimationName);
                ThisAnimation.Nodes.Add(a);

                for (int i = 0; i < boneTableEntries; i++)
                {
                    f.seek(boneTableOffset + (i * 4));
                    int offset = f.readInt();
                    
                    Animation.KeyNode bone = new Animation.KeyNode("");
                    a.Bones.Add(bone);
                    f.seek(offset);
                    bone.Text = f.readString(f.readInt(), -1);
                    //Console.WriteLine("Bone Name: " + bone.name);
                    int animationTypeFlags = f.readInt();
                    uint flags = (uint)f.readInt();

                    OSegmentType segmentType = (OSegmentType)((animationTypeFlags >> 16) & 0xf);
                    //Debug.WriteLine(bone.Text + " " + flags.ToString("x"));
                    switch (segmentType)
                    {
                        case OSegmentType.transform:
                            f.seek(offset + 0xC);
                            //Console.WriteLine(f.pos().ToString("x") + " " + flags.ToString("x"));

                            uint notExistMask = 0x10000;
                            uint constantMask = 0x40;

                            for (int j = 0; j < 3; j++)
                            {
                                for (int axis = 0; axis < 3; axis++)
                                {
                                    bool notExist = (flags & notExistMask) > 0;
                                    bool constant = (flags & constantMask) > 0;
                                    //Console.WriteLine(notExist + " " + constant);

                                    Animation.KeyGroup group = new Animation.KeyGroup();
                                    //frame.exists = !notExist;
                                    if (!notExist)
                                    {
                                        if (constant)
                                        {
                                            Animation.KeyFrame frame = new Animation.KeyFrame();
                                            frame.InterType = Animation.InterpolationType.LINEAR;
                                            frame.Value = f.readFloat();
                                            frame.Frame = 0;
                                            group.Keys.Add(frame);
                                        }
                                        else
                                        {
                                            int frameOffset = f.readInt();
                                            int position = f.pos();
                                            f.seek(frameOffset);
                                            float c = 0;
                                            //Debug.WriteLine(j + " " + axis + " " + bone.Text);
                                            getAnimationKeyFrame(f, group, out c);
                                            if (c > a.FrameCount)
                                                a.FrameCount = (int)c;
                                            f.seek(position);
                                        }
                                    }
                                    else
                                        f.seek(f.pos() + 0x04);
                                    bone.RotType = Animation.RotationType.EULER;

                                    if (j == 0)
                                    {
                                        switch (axis)
                                        {
                                            case 0: bone.XSCA = group; break;
                                            case 1: bone.YSCA = group; break;
                                            case 2: bone.ZSCA = group; break;
                                        }
                                    }else
                                    if (j == 1)
                                    {
                                        switch (axis)
                                        {
                                            case 0: bone.XROT = group; break;
                                            case 1: bone.YROT = group; break;
                                            case 2: bone.ZROT = group; break;
                                        }
                                    }
                                    else
                                    if (j == 2)
                                    {
                                        switch (axis)
                                        {
                                            case 0: bone.XPOS = group; break;
                                            case 1: bone.YPOS = group; break;
                                            case 2: bone.ZPOS = group; break;
                                        }
                                    }

                                    notExistMask <<= 1;
                                    constantMask <<= 1;
                                }
                                if(j == 1)
                                constantMask <<= 1;
                            }

                            break;
                        /*case OSegmentType.transformQuaternion:
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
                                OMatrix transform = new OMatrix();
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

                                bone.transform.Add(transform);
                            }

                            break;*/
                        default: throw new Exception(string.Format("BCH: Unknow Segment Type {0} on Skeletal Animation bone {1}! STOP!", segmentType, bone.Text));
                    }

                    //skeletalAnimation.bone.Add(bone);
                }
            }
            //return a;
        }
        
        private static void getAnimationKeyFrame(FileData input, Animation.KeyGroup group, out float endFrame)
        {
            float startFrame = input.readFloat();
            endFrame = input.readFloat();

            uint frameFlags = (uint)input.readInt();
            //Debug.WriteLine(frameFlags.ToString("x"));
            //int preRepeat = (RenderBase.ORepeatMethod)(frameFlags & 0xf);
            //int postRepeat = (RenderBase.ORepeatMethod)((frameFlags >> 8) & 0xf);

            uint segmentFlags = (uint)input.readInt();
            int interpolation = ((int)segmentFlags & 0xf);
            uint quantization = ((segmentFlags >> 8) & 0xff);
            uint entries = segmentFlags >> 16;
            float valueScale = input.readFloat();
            float valueOffset = input.readFloat();
            float frameScale = input.readFloat();
            float frameOffset = input.readFloat();

            uint offset = (uint)input.readInt();
            if (offset < input.size()) input.seek((int)offset);
            for (int key = 0; key < entries; key++)
            {
                Animation.KeyFrame keyFrame = new Animation.KeyFrame();
                //Console.WriteLine(quantization);
                switch (quantization)
                {
                    /*case RenderBase.OSegmentQuantization.hermite128:
                        keyFrame.frame = input.ReadSingle();
                        keyFrame.value = input.ReadSingle();
                        keyFrame.inSlope = input.ReadSingle();
                        keyFrame.outSlope = input.ReadSingle();
                        break;
                    case RenderBase.OSegmentQuantization.hermite64:
                        uint h64Value = input.ReadUInt32();
                        keyFrame.frame = h64Value & 0xfff;
                        keyFrame.value = h64Value >> 12;
                        keyFrame.inSlope = input.ReadInt16() / 256f;
                        keyFrame.outSlope = input.ReadInt16() / 256f;
                        break;
                    case RenderBase.OSegmentQuantization.hermite48:
                        keyFrame.frame = input.ReadByte();
                        keyFrame.value = input.ReadUInt16();
                        byte slope0 = input.ReadByte();
                        byte slope1 = input.ReadByte();
                        byte slope2 = input.ReadByte();
                        keyFrame.inSlope = IOUtils.signExtend(slope0 | ((slope1 & 0xf) << 8), 12) / 32f;
                        keyFrame.outSlope = IOUtils.signExtend((slope1 >> 4) | (slope2 << 4), 12) / 32f;
                        break;
                    case RenderBase.OSegmentQuantization.unifiedHermite96:
                        keyFrame.frame = input.ReadSingle();
                        keyFrame.value = input.ReadSingle();
                        keyFrame.inSlope = input.ReadSingle();
                        keyFrame.outSlope = keyFrame.inSlope;
                        break;
                    case RenderBase.OSegmentQuantization.unifiedHermite48:
                        keyFrame.frame = input.ReadUInt16() / 32f;
                        keyFrame.value = input.ReadUInt16();
                        keyFrame.inSlope = input.ReadInt16() / 256f;
                        keyFrame.outSlope = keyFrame.inSlope;
                        break;
                    case RenderBase.OSegmentQuantization.unifiedHermite32:
                        keyFrame.frame = input.ReadByte();
                        ushort uH32Value = input.ReadUInt16();
                        keyFrame.value = uH32Value & 0xfff;
                        keyFrame.inSlope = IOUtils.signExtend((uH32Value >> 12) | (input.ReadByte() << 4), 12) / 32f;
                        keyFrame.outSlope = keyFrame.inSlope;
                        break;
                    case RenderBase.OSegmentQuantization.stepLinear64:
                        keyFrame.frame = input.ReadSingle();
                        keyFrame.value = input.ReadSingle();
                        break;*/
                    case 7:// RenderBase.OSegmentQuantization.stepLinear32:
                        uint sL32Value = (uint)input.readInt();
                        keyFrame.Frame = sL32Value & 0xfff;
                        keyFrame.Value = sL32Value >> 12;
                        break;
                    default: Console.WriteLine("Unknown type " + quantization); break;
                }

                keyFrame.Frame = (keyFrame.Frame * frameScale) + frameOffset;
                keyFrame.Value = (keyFrame.Value * valueScale) + valueOffset;

                group.Keys.Add(keyFrame);
            }
        }

        #endregion

        #region Struct/Class
        //------------------------------------------------------------------------------------------------------------------------
        /*
         * Reads the contents of the bch file into this class
         */
        //------------------------------------------------------------------------------------------------------------------------
        // HELPERS FOR READING

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
