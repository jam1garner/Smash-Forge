using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace Smash_Forge.Rendering
{
    class FramebufferTools
    {
        public static Bitmap ReadFrameBufferPixels(int fbo, FramebufferTarget target, int width, int height, bool saveAlpha = false)
        {
            // Calculate the number of bytes needed.
            int pixelByteLength = width * height * sizeof(float);
            byte[] pixels = new byte[pixelByteLength];

            // Read the pixels from the framebuffer.
            GL.BindFramebuffer(target, fbo);
            GL.ReadPixels(0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
            byte[] fixedPixels = CopyImagePixels(width, height, saveAlpha, pixelByteLength, pixels);

            // Format and save the data
            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            Marshal.Copy(fixedPixels, 0, bmpData.Scan0, fixedPixels.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        private static byte[] CopyImagePixels(int width, int height, bool saveAlpha, int pixelByteLength, byte[] pixels)
        {
            // Flip data because glReadPixels reads it in from bottom row to top row
            byte[] fixedPixels = new byte[pixelByteLength];
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    // Remove alpha blending from the end image - we just want the post-render colors
                    if (!saveAlpha)
                        pixels[((w + h * width) * sizeof(float)) + 3] = 255;

                    // Copy a 4 byte pixel one at a time
                    Array.Copy(pixels, (w + h * width) * sizeof(float), fixedPixels, ((height - h - 1) * width + w) * sizeof(float), sizeof(float));
                }
            }

            return fixedPixels;
        }
    }
}
