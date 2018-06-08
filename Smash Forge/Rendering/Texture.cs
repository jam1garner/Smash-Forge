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

        public PixelInternalFormat PixelInternalFormat { get; }

        private TextureMinFilter minFilter = TextureMinFilter.Nearest;
        public TextureMinFilter MinFilter
        {
            get { return minFilter; }
            set
            {
                Bind();
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
                Bind();
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
                Bind();
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
                Bind();
                textureWrapS = value;
                GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int)value);
            }
        }

        private TextureWrapMode textureWrapR = TextureWrapMode.ClampToEdge;
        public TextureWrapMode TextureWrapR
        {
            get { return textureWrapR; }
            set
            {
                Bind();
                textureWrapR = value;
                GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (int)value);
            }
        }

        // TODO: This is gross and needs to be fixed eventually.
        public Texture(TextureTarget target, int width, int height, PixelInternalFormat pixelInternalFormat = PixelInternalFormat.Rgba)
        {
            // These should only be set once at object creation.
            Id = GL.GenTexture();
            textureTarget = target;
            PixelInternalFormat = pixelInternalFormat;

            // Setup the format and mip maps.
            Bind();
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        // TODO: This is gross and needs to be fixed eventually.
        public Texture(TextureTarget target, Bitmap image) : this(target, 1, 1)
        {
            Bind();

            // Load the image data.
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(textureTarget, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        ~Texture()
        {
            // The context probably isn't current here, so any GL function will crash.
            //GL.GenTexture();
            // Delete the texture's resources.
            //GL.DeleteTexture(Id);
        }

        public void Bind()
        {
            GL.BindTexture(textureTarget, Id);
        }

        // TODO: This method is redundant and should be removed.
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
