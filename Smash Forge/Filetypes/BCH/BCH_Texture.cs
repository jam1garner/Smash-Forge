using SFGraphics.GLObjects.Textures;
using System.Drawing;
using System.Windows.Forms;

namespace SmashForge
{
    public class BchTexture : TreeNode
    {
        public int width, height, type;
        public byte[] data;
        public Bitmap texture;

        public Texture display;

        public BchTexture()
        {
            ImageKey = "texture";
            SelectedImageKey = "texture";
        }

        public void ReadParameters(FileData f, int count)
        {

            for(int i = 0; i < count; i++)
            {
                BchTextureParameter p = new BchTextureParameter()
                {
                    value = f.ReadInt()
                };
                p.ParseParameter(f.ReadInt());

                Nodes.Add(p);

                if(p.Id == BchTextureParameter.Parameter.TexUnit0Size)
                {
                    width = p.GetHalf1();
                    height = p.GetHalf2();
                }
                if (p.Id == BchTextureParameter.Parameter.TexUnit0Type)
                {
                    type = p.value;
                }
                if (p.Id == BchTextureParameter.Parameter.TexUnit0Address)
                {
                    data = f.GetSection(p.value, f.Size() - p.value);
                }
            }
            if(width > 0 && height > 0)
            {
                Texture2D texture2D = new Texture2D();
                texture2D.LoadImageData(_3DS.DecodeImage(data, width, height, (_3DS.Tex_Formats)type));
                display = texture2D;
            }
        }
    }

    public class BchTextureParameter : TreeNode
    {
        public int value;
        public Parameter Id {
            get { return p; }
            set {
                p = value;
                Text = p.ToString() + " " + this.value.ToString("x");
            }
        }
        Parameter p;
        int extra = 0xF;

        public int GetHalf1()
        {
            return (value >> 16) & 0xFFFF;
        }
        public int GetHalf2()
        {
            return (value) & 0xFFFF;
        }

        public void ParseParameter(int p)
        {
            Id = (Parameter)((p) & 0xFFFF);
            extra = (p>>16) & 0xFFFF;
        }

        public enum Parameter
        {
            Null = 0x0,
            Culling = 0x40,
            PolygonOffsetEnable = 0x4c,
            PolygonOffsetZScale = 0x4d,
            PolygonOffsetZBias = 0x4e,
            TexUnitsConfig = 0x80,
            TexUnit0BorderColor = 0x81,
            TexUnit0Size = 0x82,
            TexUnit0Param = 0x83,
            TexUnit0LevelOfDetail = 0x84,
            TexUnit0Address = 0x85,
            TexUnit0Type = 0x8e,
            TexUnit1BorderColor = 0x91,
            TexUnit1Size = 0x92,
            TexUnit1Param = 0x93,
            TexUnit1LevelOfDetail = 0x94,
            TexUnit1Address = 0x95,
            TexUnit1Type = 0x96,
            TexUnit2BorderColor = 0x99,
            TexUnit2Size = 0x9a,
            TexUnit2Param = 0x9b,
            TexUnit2LevelOfDetail = 0x9c,
            TexUnit2Address = 0x9d,
            TexUnit2Type = 0x9e,
            TevStage0Source = 0xc0,
            TevStage0Operand = 0xc1,
            TevStage0Combine = 0xc2,
            TevStage0Constant = 0xc3,
            TevStage0Scale = 0xc4,
            TevStage1Source = 0xc8,
            TevStage1Operand = 0xc9,
            TevStage1Combine = 0xca,
            TevStage1Constant = 0xcb,
            TevStage1Scale = 0xcc,
            TevStage2Source = 0xd0,
            TevStage2Operand = 0xd1,
            TevStage2Combine = 0xd2,
            TevStage2Constant = 0xd3,
            TevStage2Scale = 0xd4,
            TevStage3Source = 0xd8,
            TevStage3Operand = 0xd9,
            TevStage3Combine = 0xda,
            TevStage3Constant = 0xdb,
            TevStage3Scale = 0xdc,
            FragmentBufferInput = 0xe0,
            TevStage4Source = 0xf0,
            TevStage4Operand = 0xf1,
            TevStage4Combine = 0xf2,
            TevStage4Constant = 0xf3,
            TevStage4Scale = 0xf4,
            TevStage5Source = 0xf8,
            TevStage5Operand = 0xf9,
            TevStage5Combine = 0xfa,
            TevStage5Constant = 0xfb,
            TevStage5Scale = 0xfc,
            FragmentBufferColor = 0xfd,
            ColorOutputConfig = 0x100,
            BlendConfig = 0x101,
            ColorLogicOperationConfig = 0x102,
            BlendColor = 0x103,
            AlphaTestConfig = 0x104,
            StencilTestConfig = 0x105,
            StencilOperationConfig = 0x106,
            DepthTestConfig = 0x107,
            CullModeConfig = 0x108,
            FrameBufferInvalidate = 0x110,
            FrameBufferFlush = 0x111,
            ColorBufferRead = 0x112,
            ColorBufferWrite = 0x113,
            DepthBufferRead = 0x114,
            DepthBufferWrite = 0x115,
            DepthTestConfig2 = 0x126,
            FragmentShaderLookUpTableConfig = 0x1c5,
            FragmentShaderLookUpTableData = 0x1c8,
            LutSamplerAbsolute = 0x1d0,
            LutSamplerInput = 0x1d1,
            LutSamplerScale = 0x1d2,
            VertexShaderAttributesBufferAddress = 0x200,
            VertexShaderAttributesBufferFormatLow = 0x201,
            VertexShaderAttributesBufferFormatHigh = 0x202,
            VertexShaderAttributesBuffer0Address = 0x203,
            VertexShaderAttributesBuffer0Permutation = 0x204,
            VertexShaderAttributesBuffer0Stride = 0x205,
            VertexShaderAttributesBuffer1Address = 0x206,
            VertexShaderAttributesBuffer1Permutation = 0x207,
            VertexShaderAttributesBuffer1Stride = 0x208,
            VertexShaderAttributesBuffer2Address = 0x209,
            VertexShaderAttributesBuffer2Permutation = 0x20a,
            VertexShaderAttributesBuffer2Stride = 0x20b,
            VertexShaderAttributesBuffer3Address = 0x20c,
            VertexShaderAttributesBuffer3Permutation = 0x20d,
            VertexShaderAttributesBuffer3Stride = 0x20e,
            VertexShaderAttributesBuffer4Address = 0x20f,
            VertexShaderAttributesBuffer4Permutation = 0x210,
            VertexShaderAttributesBuffer4Stride = 0x211,
            VertexShaderAttributesBuffer5Address = 0x212,
            VertexShaderAttributesBuffer5Permutation = 0x213,
            VertexShaderAttributesBuffer5Stride = 0x214,
            VertexShaderAttributesBuffer6Address = 0x215,
            VertexShaderAttributesBuffer6Permutation = 0x216,
            VertexShaderAttributesBuffer6Stride = 0x217,
            VertexShaderAttributesBuffer7Address = 0x218,
            VertexShaderAttributesBuffer7Permutation = 0x219,
            VertexShaderAttributesBuffer7Stride = 0x21a,
            VertexShaderAttributesBuffer8Address = 0x21b,
            VertexShaderAttributesBuffer8Permutation = 0x21c,
            VertexShaderAttributesBuffer8Stride = 0x21d,
            VertexShaderAttributesBuffer9Address = 0x21e,
            VertexShaderAttributesBuffer9Permutation = 0x21f,
            VertexShaderAttributesBuffer9Stride = 0x220,
            VertexShaderAttributesBuffer10Address = 0x221,
            VertexShaderAttributesBuffer10Permutation = 0x222,
            VertexShaderAttributesBuffer10Stride = 0x223,
            VertexShaderAttributesBuffer11Address = 0x224,
            VertexShaderAttributesBuffer11Permutation = 0x225,
            VertexShaderAttributesBuffer11Stride = 0x226,
            IndexBufferConfig = 0x227,
            IndexBufferTotalVertices = 0x228,
            BlockEnd = 0x23d,
            VertexShaderTotalAttributes = 0x242,
            VertexShaderBooleanUniforms = 0x2b0,
            VertexShaderIntegerUniforms0 = 0x2b1,
            VertexShaderIntegerUniforms1 = 0x2b2,
            VertexShaderIntegerUniforms2 = 0x2b3,
            VertexShaderIntegerUniforms3 = 0x2b4,
            VertexShaderInputBufferConfig = 0x2b9,
            VertexShaderEntryPoint = 0x2ba,
            VertexShaderAttributesPermutationLow = 0x2bb,
            VertexShaderAttributesPermutationHigh = 0x2bc,
            VertexShaderOutmapMask = 0x2bd,
            VertexShaderCodeTransferEnd = 0x2bf,
            VertexShaderFloatUniformConfig = 0x2c0,
            VertexShaderFloatUniformData = 0x2c1,
        }
    }
}
