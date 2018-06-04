using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge.Rendering
{
    class Texture
    {
        // This should only be set once by GL.GenTexture().
        public int Id { get; }

        private TextureTarget textureTarget = TextureTarget.Texture2D;

        private TextureMinFilter minFilter = TextureMinFilter.Nearest;
        public TextureMinFilter MinFilter
        {
            get { return minFilter; }
            set
            {
                GL.BindTexture(textureTarget, Id);
                minFilter = value;
                GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)value);
            }
        }

        private TextureMagFilter magFilter = TextureMagFilter.Linear;
        public TextureMagFilter MagFilter
        {
            get { return magFilter; }
            set
            {
                GL.BindTexture(textureTarget, Id);
                magFilter = value;
                GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)value);
            }
        }

        private TextureWrapMode textureWrapS = TextureWrapMode.ClampToEdge;
        public TextureWrapMode TextureWrapS
        {
            get { return textureWrapS; }
            set
            {
                GL.BindTexture(textureTarget, Id);
                textureWrapS = value;
                GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int)value);
            }
        }

        private TextureWrapMode textureWrapT = TextureWrapMode.ClampToEdge;
        public TextureWrapMode TextureWrapT
        {
            get { return textureWrapT; }
            set
            {
                GL.BindTexture(textureTarget, Id);
                textureWrapS = value;
                GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int)value);
            }
        }

        public Texture()
        {
            Id = GL.GenTexture();
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        ~Texture()
        {
            // TODO: Delete the texture's resources.
            Debug.WriteLine("Deleted texture");
        }

        public Texture(Bitmap image) : this()
        {
            GL.BindTexture(textureTarget, Id);

            // Load the image data.
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
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
