using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using SFGraphics.GLObjects;
using SFGraphics.GLObjects.Textures;

namespace Smash_Forge.Rendering
{
    static class TextureToBitmap
    {
        public static Bitmap RenderBitmap(Texture2D texture, 
            bool r = true, bool g = true, bool b = true, bool a = false)
        {
            // Use the texture's dimensions.
            return DrawTextureToBitmap(texture, texture.Width, texture.Height, r, g, b, a);
        }

        public static Bitmap RenderBitmapUseExistingContext(Texture2D texture,
            bool r = true, bool g = true, bool b = true, bool a = false)
        {
            // Use the texture's dimensions.
            return DrawTextureToBitmapUseExistingContext(texture, texture.Width, texture.Height, r, g, b, a);
        }

        public static Bitmap RenderBitmap(Texture2D texture, int width, int height,
            bool r = true, bool g = true, bool b = true, bool a = false)
        {
            // Scale the image to new dimensions.
            return DrawTextureToBitmap(texture, width, height, r, g, b, a);
        }

        public static Bitmap RenderBitmapUseExistingContext(Texture2D texture, int width, int height,
            bool r = true, bool g = true, bool b = true, bool a = false)
        {
            // Scale the image to new dimensions.
            return DrawTextureToBitmapUseExistingContext(texture, width, height, r, g, b, a);
        }

        private static Bitmap DrawTextureToBitmap(Texture2D texture, int width, int height, bool r, bool g, bool b, bool a)
        {
            using (GameWindow gameWindow = OpenTKSharedResources.CreateGameWindowContext(width, height))
            {
                Framebuffer framebuffer = DrawTextureToNewFbo(texture, width, height, r, g, b, a);
                return framebuffer.ReadImagePixels(a);
            }
        }

        private static Bitmap DrawTextureToBitmapUseExistingContext(Texture2D texture, int width, int height, bool r, bool g, bool b, bool a)
        {
            // Context creation will create CPU bottlenecks for multiple textures.
            Framebuffer framebuffer = DrawTextureToNewFbo(texture, width, height, r, g, b, a);
            return framebuffer.ReadImagePixels(a);
        }

        private static Framebuffer DrawTextureToNewFbo(Texture2D texture, int width, int height, bool r, bool g, bool b, bool a)
        {
            BufferObject screenVbo = ScreenDrawing.CreateScreenQuadBuffer();
            Framebuffer framebuffer = new Framebuffer(FramebufferTarget.Framebuffer, width, height, PixelInternalFormat.Rgba);
            framebuffer.Bind();

            // Draw the specified color channels.
            GL.Viewport(0, 0, width, height);
            ScreenDrawing.DrawTexturedQuad(texture.Id, 1, 1, r, g, b, a);
            return framebuffer;
        }
    }
}
