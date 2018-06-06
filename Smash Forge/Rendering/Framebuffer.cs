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
    class Framebuffer
    {
        // This should only be set once by GL.GenTexture().
        public int Id { get; }

        // To change targets, a new FBO should be created instead.
        public FramebufferTarget FramebufferTarget { get; }

        private int width;
        public int Width
        {
            get { return width; }
            set
            {
                width = value;
                Resize();
            }
        }

        private int height;
        public int Height
        {
            get { return height; }
            set
            {
                height = value;
                Resize();
            }
        }

        private int rboDepth;
        private int colorAttachment0Tex;

        public Framebuffer(FramebufferTarget target, int width, int height)
        {
            Id = GL.GenFramebuffer();
            GL.BindFramebuffer(target, Id);
            this.width = width;
            this.height = height;

            // First color attachment (regular texture).
            GL.GenTextures(1, out colorAttachment0Tex);
            GL.BindTexture(TextureTarget.Texture2D, colorAttachment0Tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.FramebufferTexture2D(target, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorAttachment0Tex, 0);

            // Render buffer for the depth attachment, which is necessary for depth testing.
            GL.GenRenderbuffers(1, out rboDepth);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rboDepth);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height);
            GL.FramebufferRenderbuffer(target, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, rboDepth);

            // Bind the default framebuffer again.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public Bitmap ReadImagePixels(bool saveAlpha = false)
        {
            // Calculate the number of bytes needed.
            int pixelByteLength = width * height * sizeof(float);
            byte[] pixels = new byte[pixelByteLength];

            // Read the pixels from the framebuffer. PNG uses the BGRA format.
            Bind();
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

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget, Id);
        }

        private void Resize()
        {
            Bind();

            // First color attachment (regular texture).
            GL.BindTexture(TextureTarget.Texture2D, colorAttachment0Tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorAttachment0Tex, 0);

            // Render buffer for the depth attachment, which is necessary for depth testing.
            GL.GenRenderbuffers(1, out rboDepth);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rboDepth);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, rboDepth);

            // Bind the default framebuffer again.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
}
