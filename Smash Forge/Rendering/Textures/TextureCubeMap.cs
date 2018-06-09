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
    class TextureCubeMap : Texture
    {
        public TextureCubeMap(Bitmap cubeMapFaces, int faceResolution = 128) : base(TextureTarget.TextureCubeMap, faceResolution, faceResolution, PixelInternalFormat.Rgba)
        {
            Bind();
            MinFilter = TextureMinFilter.Linear; // #cubemapthings

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
        }
    }
}
