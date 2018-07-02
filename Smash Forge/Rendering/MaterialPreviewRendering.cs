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

            renderingCompleted = Task.Run(() =>
            {
                SetUpContextWindow(width, height);
                BufferObject screenVbo = RenderTools.CreateScreenQuadBuffer();

                // TODO: Set up the material sphere textures and dummy textures for this thread.

                foreach (string file in Directory.EnumerateFiles(MainForm.executableDir + "\\materials", "*.nmt", SearchOption.AllDirectories))
                {
                    NUD.Material material = NUDMaterialEditor.ReadMaterialListFromPreset(file)[0];
                    RenderMaterialPresetToFile(width, height, file, material, screenVbo);
                }
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

        private static void RenderMaterialPresetToFile(int width, int height, string file, NUD.Material material, BufferObject screenVbo)
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

            // TODO: Doesn't work on Intel.
            RenderTools.DrawNudMaterialSphere(material, screenVbo);

            using (Bitmap image = framebuffer.ReadImagePixels(true))
            {
                string outputPath = MainForm.executableDir + "\\Preview Images\\" + presetName;
                image.Save(outputPath);
            }
        }
    }
}
