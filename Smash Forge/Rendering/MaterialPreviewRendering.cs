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

namespace Smash_Forge.Rendering
{
    class MaterialPreviewRendering
    {
        public static Task RenderingCompleted { get { return renderingCompleted; } }
        private static Task renderingCompleted;

        public static void RenderMaterialPresetPreviewsToFilesThreaded()
        {
            // Save on file size.
            int width = 256;
            int height = 256;

            // Render each material preview on a separate thread and save to a file.
            string[] files = Directory.GetFiles(MainForm.executableDir + "\\materials", "*.nmt", SearchOption.AllDirectories);

            renderingCompleted = Task.Run(() => {
                // Set up a context for this thread.
                GraphicsMode mode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 0, 0, ColorFormat.Empty, 1);
                GameWindow window = new GameWindow(width, height, mode, "", OpenTK.GameWindowFlags.Default, OpenTK.DisplayDevice.Default, 3, 0, GraphicsContextFlags.Default);
                window.Visible = false;
                window.MakeCurrent();
                BufferObject screenVbo = RenderTools.CreateScreenQuadBuffer();

                for (int i = 0; i < files.Length; i++)
                {
                    NUD.Material material = NUDMaterialEditor.ReadMaterialListFromPreset(files[i])[0];
                    RenderMaterialPresetToFileThreaded(width, height, files[i], material, screenVbo);
                }
            });
        }

        private static void RenderMaterialPresetToFileThreaded(int width, int height, string file, NUD.Material material, BufferObject screenVbo)
        {
            // Save the image file using the name of the preset.
            string[] parts = file.Split('\\');
            string presetName = parts[parts.Length - 1];
            presetName = presetName.Replace(".nmt", ".png");

            // Setup new dimensions.
            GL.Viewport(0, 0, width, height);

            // Draw the material to a textured quad.
            Framebuffer framebuffer = new Framebuffer(FramebufferTarget.Framebuffer, width, height, PixelInternalFormat.Rgba);
            framebuffer.Bind();
            GL.ClearColor(1, 0, 0, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // TODO: Fix access violation. Context is still current.
            // Probably a buffer or something...
            RenderTools.DrawNudMaterialSphere(material, screenVbo);

            using (Bitmap image = framebuffer.ReadImagePixels(true))
            {
                string outputPath = MainForm.executableDir + "\\Preview Images\\" + presetName;
                image.Save(outputPath);
            }
        }

    }
}
