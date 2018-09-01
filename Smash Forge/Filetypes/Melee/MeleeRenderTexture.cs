using MeleeLib.DAT;
using MeleeLib.GCX;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Textures;
using System.Drawing;

namespace Smash_Forge
{
    public class MeleeRenderTexture
    {
        public Texture2D texture = new Texture2D();
        public uint Flag;
        public float WScale;
        public float HScale;

        public MeleeRenderTexture(Bitmap image)
        {
            texture.LoadImageData(image);
        }

        public MeleeRenderTexture(DatTexture datTexture)
        {
            WScale = datTexture.WScale;
            HScale = datTexture.HScale;

            using (Bitmap b = datTexture.GetBitmap())
            {
                texture.LoadImageData(b);
            }

            texture.TextureWrapS = GetGLWrapModeFromGX(datTexture.WrapS);
            texture.TextureWrapT = GetGLWrapModeFromGX(datTexture.WrapT);
        }

        private TextureWrapMode GetGLWrapModeFromGX(GXWrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case GXWrapMode.CLAMP:
                    return TextureWrapMode.Clamp;
                case GXWrapMode.MIRROR:
                    return TextureWrapMode.MirroredRepeat;
                case GXWrapMode.REPEAT:
                    return TextureWrapMode.Repeat;
                default:
                    return TextureWrapMode.Repeat;
            }
        }
    }
}
