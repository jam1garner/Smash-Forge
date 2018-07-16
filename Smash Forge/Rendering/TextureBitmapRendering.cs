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
    static class TextureBitmapRendering
    {
        public static Task RenderBitmap { get { return renderBitmap; } }
        private static Task renderBitmap;

        public static void RenderTextureToBitmap(Texture2D texture, int width, int height)
        {
            renderBitmap = Task.Run(() =>
            {
                SetUpContextWindow(width, height);
                BufferObject screenVbo = RenderTools.CreateScreenQuadBuffer();

                // 1. Create texture shader
                // 2. Render textured quad
                // 3. Copy framebuffer to bitmap
            });
        }

        private static void SetUpContextWindow(int width, int height)
        {
            // Set up a context for this thread.
            GraphicsMode mode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 0, 0, ColorFormat.Empty, 1);
            GameWindow window = new GameWindow(width, height, mode, "", OpenTK.GameWindowFlags.Default, OpenTK.DisplayDevice.Default, 3, 0, GraphicsContextFlags.Default);
            window.Visible = false;
            window.MakeCurrent();
        }
    }
}
