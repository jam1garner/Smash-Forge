using OpenTK;
using System;
using System.Collections.Generic;

namespace SmashForge
{
    /*
     * Everything in this class is adapted from gdkchan's Ohana3DS
     * This version will ONLY support Smash-style BCHs
     * Really WIP
     */
    public class BCHan
    {

        #region Reading

        public static AnimationGroupNode Read(string filename)
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
            AnimationGroupNode ThisAnimation = new AnimationGroupNode() { Text = filename };

            for (int index1 = 0; index1 < content.skeletalAnimationsPointerTableEntries; index1++)//
            {
                f.Seek(content.skeletalAnimationsPointerTableOffset + (index1 * 4));
                int dataOffset = f.ReadInt();
                f.Seek(dataOffset);


                string skeletalAnimationName = f.ReadString(f.ReadInt(), -1);
                int animationFlags = f.ReadInt();
                //int skeletalAnimationloopMode = f.readByte();  //pas ça du tout
                float skeletalAnimationframeSize = f.ReadFloat();
                int boneTableOffset = f.ReadInt();
                int boneTableEntries = f.ReadInt();
                int metaDataPointerOffset = f.ReadInt();

                //Runtime.Animations.Add(skeletalAnimationName, a);
                //MainForm.animNode.Nodes.Add(skeletalAnimationName);

                Animation a = new Animation(skeletalAnimationName);
                ThisAnimation.Nodes.Add(a);

                for (int i = 0; i < boneTableEntries; i++)
                {
                    f.Seek(boneTableOffset + (i * 4));
                    int offset = f.ReadInt();

                    Animation.KeyNode bone = new Animation.KeyNode("");
                    a.bones.Add(bone);
                    f.Seek(offset);
                    bone.Text = f.ReadString(f.ReadInt(), -1);
                    //Console.WriteLine("Bone Name: " + bone.name);
                    int animationTypeFlags = f.ReadInt();
                    uint flags = (uint)f.ReadInt();

                    OSegmentType segmentType = (OSegmentType)((animationTypeFlags >> 16) & 0xf);
                    //Debug.WriteLine(bone.Text + " " + flags.ToString("x"));
                    switch (segmentType)
                    {
                        case OSegmentType.transform:
                            f.Seek(offset + 0xC);
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
                                            frame.interType = Animation.InterpolationType.Linear;
                                            frame.Value = f.ReadFloat();
                                            frame.Frame = 0;
                                            group.keys.Add(frame);
                                        }
                                        else
                                        {
                                            int frameOffset = f.ReadInt();
                                            int position = f.Pos();
                                            f.Seek(frameOffset);
                                            float c = 0;
                                            //Debug.WriteLine(j + " " + axis + " " + bone.Text);
                                            getAnimationKeyFrame(f, group, out c);
                                            if (c > a.frameCount)
                                                a.frameCount = (int)c;
                                            f.Seek(position);
                                        }
                                    }
                                    else
                                        f.Seek(f.Pos() + 0x04);
                                    bone.rotType = Animation.RotationType.Euler;

                                    if (j == 0)
                                    {
                                        switch (axis)
                                        {
                                            case 0: bone.xsca = group; break;
                                            case 1: bone.ysca = group; break;
                                            case 2: bone.zsca = group; break;
                                        }
                                    }
                                    else
                                    if (j == 1)
                                    {
                                        switch (axis)
                                        {
                                            case 0: bone.xrot = group; break;
                                            case 1: bone.yrot = group; break;
                                            case 2: bone.zrot = group; break;
                                        }
                                    }
                                    else
                                    if (j == 2)
                                    {
                                        switch (axis)
                                        {
                                            case 0: bone.xpos = group; break;
                                            case 1: bone.ypos = group; break;
                                            case 2: bone.zpos = group; break;
                                        }
                                    }

                                    notExistMask <<= 1;
                                    constantMask <<= 1;
                                }
                                if (j == 1)
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
            return ThisAnimation;
        }

        private static void getAnimationKeyFrame(FileData input, Animation.KeyGroup group, out float endFrame)
        {
            float startFrame = input.ReadFloat();
            endFrame = input.ReadFloat();

            uint frameFlags = (uint)input.ReadInt();
            //Debug.WriteLine(frameFlags.ToString("x"));
            //int preRepeat = (RenderBase.ORepeatMethod)(frameFlags & 0xf);
            //int postRepeat = (RenderBase.ORepeatMethod)((frameFlags >> 8) & 0xf);

            uint segmentFlags = (uint)input.ReadInt();
            int interpolation = ((int)segmentFlags & 0xf);
            uint quantization = ((segmentFlags >> 8) & 0xff);
            uint entries = segmentFlags >> 16;
            float valueScale = input.ReadFloat();
            float valueOffset = input.ReadFloat();
            float frameScale = input.ReadFloat();
            float frameOffset = input.ReadFloat();

            uint offset = (uint)input.ReadInt();
            if (offset < input.Size()) input.Seek((int)offset);
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
                        uint sL32Value = (uint)input.ReadInt();
                        keyFrame.Frame = sL32Value & 0xfff;
                        keyFrame.Value = sL32Value >> 12;
                        break;
                    default: Console.WriteLine("Unknown type " + quantization); break;
                }

                keyFrame.Frame = (keyFrame.Frame * frameScale) + frameOffset;
                keyFrame.Value = (keyFrame.Value * valueScale) + valueOffset;

                group.keys.Add(keyFrame);
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
            Step = 0,
            Linear = 1,
            Hermite = 2
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
