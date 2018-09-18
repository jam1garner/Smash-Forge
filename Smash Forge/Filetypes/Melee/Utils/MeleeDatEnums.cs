namespace Smash_Forge.Filetypes.Melee.Utils
{
    public static class MeleeDatEnums
    {
        public enum TextureFlag : uint
        {
            Diffuse = 0x10,
            Specular = 0x20,
            Unk3 = 0x30, // also diffuse?
            BumpMap = 0x1000000,
            AlphaTest = 0x300000, // whispy woods
            Unk4 = 0x80, // diffuse with inverted colors?
        }

        public enum TexCoordsFlag : uint
        {
            TexCoord = 0x0,
            SphereMap = 0x1
        }

        public enum MiscFlags : uint
        {
            AlphaTest = 0x40000000 // alpha test?
        }
    }
}
