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
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects.Textures;


namespace Smash_Forge.Rendering
{
    static class MaterialPreviewRendering
    {
        public static Task RenderAllPresetsToFiles { get { return renderAllPresetsToFiles; } }
        private static Task renderAllPresetsToFiles;

        // Resources for rendering.
        // These need to be regenerated every time due to using a separate thread.
        private static BufferObject screenVbo;
        private static Dictionary<NUD.DummyTextures, Texture> dummyTextures = new Dictionary<NUD.DummyTextures, Texture>();
        private static Shader shader;

        // Reduce file size.
        private static readonly int width = 256;
        private static readonly int height = 256;

        public static void RenderMaterialPresetPreviewsToFilesThreaded()
        {
            renderAllPresetsToFiles = Task.Run(() =>
            {
                RenderPresetsToFiles();
            });
        }

        private static void RenderPresetsToFiles()
        {
            using (GameWindow gameWindow = RenderTools.CreateGameWindowContext(width, height))
            {
                // Resource creation.
                screenVbo = RenderTools.CreateScreenQuadBuffer();
                shader = Runtime.shaders["NudSphere"];

                // Skip thumbnail generation if the shader didn't compile.
                if (!shader.ProgramCreatedSuccessfully())
                    return;

                // HACK: This isn't a very clean way to pass resources around.
                RenderTools.LoadMaterialSphereTextures();
                Dictionary<NUD.DummyTextures, Texture> dummyTextures = RenderTools.CreateNudDummyTextures();

                CreateNudSphereShader();

                foreach (string file in Directory.EnumerateFiles(MainForm.executableDir + "\\materials", "*.nmt", SearchOption.AllDirectories))
                {
                    NUD.Material material = NUDMaterialEditor.ReadMaterialListFromPreset(file)[0];
                    string presetName = Path.GetFileNameWithoutExtension(file);
                    RenderMaterialPresetToFile(presetName, material, dummyTextures);
                }
            }
        }

        private static void CreateNudSphereShader()
        {
            // HACK: Recreating static resources is dumb. 
            // Don't add this to the main shaders to begin with.
            string[] nudMatShaders = new string[]
            {
                    "Nud\\NudSphere.frag",
                    "Nud\\NudSphere.vert",
                    "Nud\\StageLighting.frag",
                    "Nud\\Bayo.frag",
                    "Nud\\SmashShader.frag",
                    "Utility\\Utility.frag"
            };
            Runtime.shaders.Remove("NudSphere");
            ShaderTools.CreateAndAddShader("NudSphere", nudMatShaders);
        }

        private static void RenderMaterialPresetToFile(string presetName, NUD.Material material, Dictionary<NUD.DummyTextures, Texture> dummyTextures)
        {
            // Setup new dimensions.
            GL.Viewport(0, 0, width, height);

            Framebuffer framebuffer = new Framebuffer(FramebufferTarget.Framebuffer, width, height, PixelInternalFormat.Rgba);
            framebuffer.Bind();

            // Draw the material to a textured quad.
            RenderTools.DrawNudMaterialSphere(shader, material, screenVbo, dummyTextures);

            // Save output.
            using (Bitmap image = framebuffer.ReadImagePixels(true))
            {
                string outputPath =  String.Format("{0}\\Preview Images\\{1}.png", MainForm.executableDir, presetName);
                image.Save(outputPath);
            }
        }
    }
}
