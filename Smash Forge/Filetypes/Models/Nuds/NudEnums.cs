using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Smash_Forge.Filetypes.Models.Nuds
{
    public static class NudEnums
    {
        public static readonly Dictionary<int, BlendingFactor> srcFactorByMatValue = new Dictionary<int, BlendingFactor>()
        {
            { 0x00, BlendingFactor.One },
            { 0x01, BlendingFactor.SrcAlpha },
            { 0x02, BlendingFactor.One },
            { 0x03, BlendingFactor.SrcAlpha },
            { 0x04, BlendingFactor.SrcAlpha },
        };

        public static readonly Dictionary<int, BlendingFactor> dstFactorByMatValue = new Dictionary<int, BlendingFactor>()
        {
            { 0x00, BlendingFactor.Zero },
            { 0x01, BlendingFactor.OneMinusSrcAlpha },
            { 0x02, BlendingFactor.One },
            { 0x03, BlendingFactor.One },
            { 0x05, BlendingFactor.OneMinusSrcColor }
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
