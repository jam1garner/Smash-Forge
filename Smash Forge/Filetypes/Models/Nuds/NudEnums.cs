using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace SmashForge.Filetypes.Models.Nuds
{
    public static class NudEnums
    {
        public enum AlphaTest
        {
            Enabled = 0x02,
            Disabled = 0x00
        }

        public static readonly Dictionary<int, BlendingFactor> srcFactorByMatValue = new Dictionary<int, BlendingFactor>()
        {
            { 0x00, BlendingFactor.One },
            { 0x01, BlendingFactor.SrcAlpha },
            { 0x02, BlendingFactor.One },
            { 0x03, BlendingFactor.SrcAlpha },
            { 0x04, BlendingFactor.Zero },
            { 0x05, BlendingFactor.SrcAlpha },
            { 0x06, BlendingFactor.DstAlpha },
            { 0x07, BlendingFactor.DstAlpha },
            { 0x08, BlendingFactor.DstColor },
            { 0x0a, BlendingFactor.DstColor },
            // TODO:
            { 0x0b, BlendingFactor.One },
            { 0x0f, BlendingFactor.One },
            { 0x010, BlendingFactor.One },
            { 0x021, BlendingFactor.One },
        };

        public static readonly Dictionary<int, BlendingFactor> dstFactorByMatValue = new Dictionary<int, BlendingFactor>()
        {
            { 0x00, BlendingFactor.Zero },
            { 0x01, BlendingFactor.OneMinusSrcAlpha },
            { 0x02, BlendingFactor.One },
            { 0x03, BlendingFactor.One },
            { 0x04, BlendingFactor.SrcAlpha },
            { 0x05, BlendingFactor.SrcAlpha },
            { 0x06, BlendingFactor.OneMinusDstAlpha },
            { 0x07, BlendingFactor.One },
            { 0x08, BlendingFactor.Zero },
            { 0x0a, BlendingFactor.Zero },
            // TODO:
            { 0x0b, BlendingFactor.Zero },
            { 0x0c, BlendingFactor.Zero },
            { 0x011, BlendingFactor.Zero },
            { 0x012, BlendingFactor.Zero },
            { 0x014, BlendingFactor.Zero },
            { 0x022, BlendingFactor.Zero },
            { 0x040, BlendingFactor.Zero },
            { 0x042, BlendingFactor.Zero },
            { 0x070, BlendingFactor.Zero },
            { 0x072, BlendingFactor.Zero },
            { 0x081, BlendingFactor.Zero },
            { 0x082, BlendingFactor.Zero },
        };

        public static readonly Dictionary<int, AlphaFunction> alphaFunctionByMatValue = new Dictionary<int, AlphaFunction>()
        {
            { 0x0, AlphaFunction.Never },
            { 0x4, AlphaFunction.Gequal },
            { 0x6, AlphaFunction.Gequal },
        };

        public static readonly Dictionary<int, TextureWrapMode> wrapModeByMatValue = new Dictionary<int, TextureWrapMode>()
        {
            { 0x01, TextureWrapMode.Repeat},
            { 0x02, TextureWrapMode.MirroredRepeat},
            { 0x03, TextureWrapMode.ClampToEdge}
        };

        public static readonly Dictionary<int, TextureMinFilter> minFilterByMatValue = new Dictionary<int, TextureMinFilter>()
        {
            { 0x00, TextureMinFilter.LinearMipmapLinear},
            { 0x01, TextureMinFilter.Nearest},
            { 0x02, TextureMinFilter.Linear},
            { 0x03, TextureMinFilter.NearestMipmapLinear},
        };

        public static readonly Dictionary<int, TextureMagFilter> magFilterByMatValue = new Dictionary<int, TextureMagFilter>()
        {
            { 0x00, TextureMagFilter.Linear},
            { 0x01, TextureMagFilter.Nearest},
            { 0x02, TextureMagFilter.Linear}
        };

        public enum TextureFlag
        {
            Glow = 0x00000080,
            Shadow = 0x00000040,
            DummyRamp = 0x00000020,
            SphereMap = 0x00000010,
            StageAOMap = 0x00000008,
            RampCubeMap = 0x00000004,
            NormalMap = 0x00000002,
            DiffuseMap = 0x00000001
        }

        public enum DummyTexture
        {
            StageMapLow = 0x10101000,
            StageMapHigh = 0x10102000,
            PokemonStadium = 0x10040001,
            PunchOut = 0x10040000,
            DummyRamp = 0x10080000,
            ShadowMap = 0x10100000
        }
    }
}
