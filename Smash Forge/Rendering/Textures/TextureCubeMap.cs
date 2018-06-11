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
    public class TextureCubeMap : Texture
    {
        public TextureCubeMap(Bitmap cubeMapFaces, int sideLength = 128) : base(TextureTarget.TextureCubeMap, sideLength, sideLength, PixelInternalFormat.Rgba)
        {
            Bind();

            // #cubemapthings
            MagFilter = TextureMagFilter.Linear;
            MinFilter = TextureMinFilter.Linear; 

            const int cubeMapFaceCount = 6;

            // The cube map resolution is currently hardcoded...
            // Faces are arranged vertically in the following order from top to bottom:
            // X+, X-, Y+, Y-, Z+, Z-
            Rectangle[] cubeMapFaceRegions = new Rectangle[cubeMapFaceCount];
            for (int i = 0; i < cubeMapFaceRegions.Length; i++)
            {
                cubeMapFaceRegions[i] = new Rectangle(0, i * sideLength, sideLength, sideLength);
            }

            for (int i = 0; i < cubeMapFaceCount; i++)
            {
                // Copy the pixels for the appropriate face.
                Bitmap image = cubeMapFaces.Clone(cubeMapFaceRegions[i], cubeMapFaces.PixelFormat);

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
