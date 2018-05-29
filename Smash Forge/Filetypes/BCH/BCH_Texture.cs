using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Smash_Forge
{
    public class BCH_Texture : TreeNode
    {
        public int Width, Height, type;
        public byte[] data;
        public Bitmap texture;

        public int display = 0;

        public BCH_Texture()
        {
            ImageKey = "texture";
            SelectedImageKey = "texture";
        }

        public void ReadParameters(FileData f, int count)
        {

            for(int i = 0; i < count; i++)
            {
                BCH_TextureParameter p = new BCH_TextureParameter()
                {
                    Value = f.readInt()
                };
                p.ParseParameter(f.readInt());

                Nodes.Add(p);

                if(p.Id == BCH_TextureParameter.Parameter.texUnit0Size)
                {
                    Width = p.GetHalf1();
                    Height = p.GetHalf2();
                }
                if (p.Id == BCH_TextureParameter.Parameter.texUnit0Type)
                {
                    type = p.Value;
                }
                if (p.Id == BCH_TextureParameter.Parameter.texUnit0Address)
                {
                    data = f.getSection(p.Value, f.size() - p.Value);
                }
            }
            if(Width > 0 && Height > 0)
            {
                texture = _3DS.DecodeImage(data, Width, Height, (_3DS.Tex_Formats)type);
                display = Rendering.Texture.CreateGlTextureFromBitmap(texture);
            }
        }
    }

    public class BCH_TextureParameter : TreeNode
    {
        public int Value;
        public Parameter Id {
            get { return p; }
            set {
                p = value;
                Text = p.ToString() + " " + Value.ToString("x");
            }
        }
        Parameter p;
        int extra = 0xF;

        public int GetHalf1()
        {
            return (Value >> 16) & 0xFFFF;
        }
        public int GetHalf2()
        {
            return (Value) & 0xFFFF;
        }

        public void ParseParameter(int p)
        {
            Id = (Parameter)((p) & 0xFFFF);
            extra = (p>>16) & 0xFFFF;
        }

        public enum Parameter
        {
            Null = 0x0,
            culling = 0x40,
            polygonOffsetEnable = 0x4c,
            polygonOffsetZScale = 0x4d,
            polygonOffsetZBias = 0x4e,
            texUnitsConfig = 0x80,
            texUnit0BorderColor = 0x81,
            texUnit0Size = 0x82,
            texUnit0Param = 0x83,
            texUnit0LevelOfDetail = 0x84,
            texUnit0Address = 0x85,
            texUnit0Type = 0x8e,
            texUnit1BorderColor = 0x91,
            texUnit1Size = 0x92,
            texUnit1Param = 0x93,
            texUnit1LevelOfDetail = 0x94,
            texUnit1Address = 0x95,
            texUnit1Type = 0x96,
            texUnit2BorderColor = 0x99,
            texUnit2Size = 0x9a,
            texUnit2Param = 0x9b,
            texUnit2LevelOfDetail = 0x9c,
            texUnit2Address = 0x9d,
            texUnit2Type = 0x9e,
            tevStage0Source = 0xc0,
            tevStage0Operand = 0xc1,
            tevStage0Combine = 0xc2,
            tevStage0Constant = 0xc3,
            tevStage0Scale = 0xc4,
            tevStage1Source = 0xc8,
            tevStage1Operand = 0xc9,
            tevStage1Combine = 0xca,
            tevStage1Constant = 0xcb,
            tevStage1Scale = 0xcc,
            tevStage2Source = 0xd0,
            tevStage2Operand = 0xd1,
            tevStage2Combine = 0xd2,
            tevStage2Constant = 0xd3,
            tevStage2Scale = 0xd4,
            tevStage3Source = 0xd8,
            tevStage3Operand = 0xd9,
            tevStage3Combine = 0xda,
            tevStage3Constant = 0xdb,
            tevStage3Scale = 0xdc,
            fragmentBufferInput = 0xe0,
            tevStage4Source = 0xf0,
            tevStage4Operand = 0xf1,
            tevStage4Combine = 0xf2,
            tevStage4Constant = 0xf3,
            tevStage4Scale = 0xf4,
            tevStage5Source = 0xf8,
            tevStage5Operand = 0xf9,
            tevStage5Combine = 0xfa,
            tevStage5Constant = 0xfb,
            tevStage5Scale = 0xfc,
            fragmentBufferColor = 0xfd,
            colorOutputConfig = 0x100,
            blendConfig = 0x101,
            colorLogicOperationConfig = 0x102,
            blendColor = 0x103,
            alphaTestConfig = 0x104,
            stencilTestConfig = 0x105,
            stencilOperationConfig = 0x106,
            depthTestConfig = 0x107,
            cullModeConfig = 0x108,
            frameBufferInvalidate = 0x110,
            frameBufferFlush = 0x111,
            colorBufferRead = 0x112,
            colorBufferWrite = 0x113,
            depthBufferRead = 0x114,
            depthBufferWrite = 0x115,
            depthTestConfig2 = 0x126,
            fragmentShaderLookUpTableConfig = 0x1c5,
            fragmentShaderLookUpTableData = 0x1c8,
            lutSamplerAbsolute = 0x1d0,
            lutSamplerInput = 0x1d1,
            lutSamplerScale = 0x1d2,
            vertexShaderAttributesBufferAddress = 0x200,
            vertexShaderAttributesBufferFormatLow = 0x201,
            vertexShaderAttributesBufferFormatHigh = 0x202,
            vertexShaderAttributesBuffer0Address = 0x203,
            vertexShaderAttributesBuffer0Permutation = 0x204,
            vertexShaderAttributesBuffer0Stride = 0x205,
            vertexShaderAttributesBuffer1Address = 0x206,
            vertexShaderAttributesBuffer1Permutation = 0x207,
            vertexShaderAttributesBuffer1Stride = 0x208,
            vertexShaderAttributesBuffer2Address = 0x209,
            vertexShaderAttributesBuffer2Permutation = 0x20a,
            vertexShaderAttributesBuffer2Stride = 0x20b,
            vertexShaderAttributesBuffer3Address = 0x20c,
            vertexShaderAttributesBuffer3Permutation = 0x20d,
            vertexShaderAttributesBuffer3Stride = 0x20e,
            vertexShaderAttributesBuffer4Address = 0x20f,
            vertexShaderAttributesBuffer4Permutation = 0x210,
            vertexShaderAttributesBuffer4Stride = 0x211,
            vertexShaderAttributesBuffer5Address = 0x212,
            vertexShaderAttributesBuffer5Permutation = 0x213,
            vertexShaderAttributesBuffer5Stride = 0x214,
            vertexShaderAttributesBuffer6Address = 0x215,
            vertexShaderAttributesBuffer6Permutation = 0x216,
            vertexShaderAttributesBuffer6Stride = 0x217,
            vertexShaderAttributesBuffer7Address = 0x218,
            vertexShaderAttributesBuffer7Permutation = 0x219,
            vertexShaderAttributesBuffer7Stride = 0x21a,
            vertexShaderAttributesBuffer8Address = 0x21b,
            vertexShaderAttributesBuffer8Permutation = 0x21c,
            vertexShaderAttributesBuffer8Stride = 0x21d,
            vertexShaderAttributesBuffer9Address = 0x21e,
            vertexShaderAttributesBuffer9Permutation = 0x21f,
            vertexShaderAttributesBuffer9Stride = 0x220,
            vertexShaderAttributesBuffer10Address = 0x221,
            vertexShaderAttributesBuffer10Permutation = 0x222,
            vertexShaderAttributesBuffer10Stride = 0x223,
            vertexShaderAttributesBuffer11Address = 0x224,
            vertexShaderAttributesBuffer11Permutation = 0x225,
            vertexShaderAttributesBuffer11Stride = 0x226,
            indexBufferConfig = 0x227,
            indexBufferTotalVertices = 0x228,
            blockEnd = 0x23d,
            vertexShaderTotalAttributes = 0x242,
            vertexShaderBooleanUniforms = 0x2b0,
            vertexShaderIntegerUniforms0 = 0x2b1,
            vertexShaderIntegerUniforms1 = 0x2b2,
            vertexShaderIntegerUniforms2 = 0x2b3,
            vertexShaderIntegerUniforms3 = 0x2b4,
            vertexShaderInputBufferConfig = 0x2b9,
            vertexShaderEntryPoint = 0x2ba,
            vertexShaderAttributesPermutationLow = 0x2bb,
            vertexShaderAttributesPermutationHigh = 0x2bc,
            vertexShaderOutmapMask = 0x2bd,
            vertexShaderCodeTransferEnd = 0x2bf,
            vertexShaderFloatUniformConfig = 0x2c0,
            vertexShaderFloatUniformData = 0x2c1,
        }
    }
}
