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

        public Texture(TextureTarget target, int width, int height, PixelInternalFormat pixelInternalFormat = PixelInternalFormat.Rgba)
        {
            // These should only be set once at object creation.
            Id = GL.GenTexture();
            textureTarget = target;
            PixelInternalFormat = pixelInternalFormat;

            Bind();

            // Setup the format and mip maps.
            GL.TexImage2D(textureTarget, 0, PixelInternalFormat, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public Texture(TextureTarget target, Bitmap image) : this(target, image.Width, image.Height)
        {
            LoadImageDataAutoGenerateMipmaps(image);
        }

        private void LoadImageDataAutoGenerateMipmaps(Bitmap image)
        {
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
            // TODO: Manage resources.
        }

        public void Bind()
        {
            GL.BindTexture(textureTarget, Id);
        }

        // TODO: Cube maps should be a separate class that inherits from Texture.
        public static int CreateGlCubeMap(Bitmap cubeMapFaces)
        {
            int id;
            GL.GenTextures(1, out id);
            GL.BindTexture(TextureTarget.TextureCubeMap, id);

            Bitmap bmp = cubeMapFaces;

            // The cube map resolution is currently hardcoded...
            // Faces are arranged vertically in the following order from top to bottom:
            // X+, X-, Y+, Y-, Z+, Z-
            Rectangle[] cubeMapFaceRegions = new Rectangle[] {
            new Rectangle(0, 0, 128, 128),
            new Rectangle(0, 128, 128, 128),
            new Rectangle(0, 256, 128, 128),
            new Rectangle(0, 384, 128, 128),
            new Rectangle(0, 512, 128, 128),
            new Rectangle(0, 640, 128, 128),
            };

            const int cubeMapFaceCount = 6;
            for (int i = 0; i < cubeMapFaceCount; i++)
            {
                // Copy the pixels for the appropriate face.
                Bitmap image = bmp.Clone(cubeMapFaceRegions[i], bmp.PixelFormat);

                // Load the data to the texture.
                BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                image.UnlockBits(data);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            return id;
        }
    }
}
