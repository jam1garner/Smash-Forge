using OpenTK.Graphics.OpenGL;
using MeleeLib.DAT;
using System.Drawing;
using System.Drawing.Imaging;
using MeleeLib.GCX;

namespace Smash_Forge
{
    public class MeleeRenderTexture
    {
        public int ID;
        public uint Flag;
        public float WScale, HScale;
        public TextureWrapMode WrapS;
        public TextureWrapMode WrapT;


        public MeleeRenderTexture(DatTexture t)
        {
            Bitmap b = t.GetBitmap();
            ID = CreateGlTextureFromBitmap(b);
            b.Dispose();

            WScale = t.WScale;
            HScale = t.HScale;
            WrapS = GetGLWrapModeFromGX(t.WrapS);
            WrapT = GetGLWrapModeFromGX(t.WrapT);
        }

        private TextureWrapMode GetGLWrapModeFromGX(GXWrapMode wm)
        {
            switch (wm)
            {
                case GXWrapMode.CLAMP: return TextureWrapMode.Clamp;
                case GXWrapMode.MIRROR: return TextureWrapMode.MirroredRepeat;
                case GXWrapMode.REPEAT: return TextureWrapMode.Repeat;
            }
            return TextureWrapMode.Repeat;
        }

        public MeleeRenderTexture(Bitmap image)
        {
            ID = CreateGlTextureFromBitmap(image);
        }

        public void Dispose()
        {
            GL.DeleteTexture(ID);
            ID = -1;
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, ID);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)WrapS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)WrapT);

        }

        public static int CreateGlTextureFromBitmap(Bitmap image)
        {
            int texID = GL.GenTexture();

            // Read the pixel data from the bitmap.
            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 1);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return texID;
        }
    }
}
