using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
        // TODO: Use separate thread.
        public static Bitmap RenderBitmap(Texture2D texture, 
            bool r = true, bool g = true, bool b = true, bool a = false)
        {
            // Use the texture's dimensions.
            Framebuffer framebuffer = DrawTextureToNewFbo(texture, texture.Width, texture.Height, r, g, b, a);
            return framebuffer.ReadImagePixels(a);
        }

        public static Bitmap RenderBitmap(Texture2D texture, int width, int height,
            bool r = true, bool g = true, bool b = true, bool a = false)
        {
            // Set up the framebuffer and context to match the texture's dimensions.
            using (GameWindow gameWindow = RenderTools.CreateGameWindowContext(width, height))
            {
                Framebuffer framebuffer = DrawTextureToNewFbo(texture, width, height, r, g, b, a);
                return framebuffer.ReadImagePixels(a);
            }
        }

        private static Framebuffer DrawTextureToNewFbo(Texture2D texture, int width, int height, bool r, bool g, bool b, bool a)
        {
            BufferObject screenVbo = RenderTools.CreateScreenQuadBuffer();
            Framebuffer framebuffer = new Framebuffer(FramebufferTarget.Framebuffer, width, height, PixelInternalFormat.Rgba);
            framebuffer.Bind();

            // Draw the specified color channels.
            GL.Viewport(0, 0, width, height);
            RenderTools.DrawTexturedQuad(texture.Id, 1, 1, r, g, b, a);
            return framebuffer;
        }
    }
}
