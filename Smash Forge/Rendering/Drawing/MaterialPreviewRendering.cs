using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Framebuffers;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects.Textures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using SmashForge.Filetypes.Models.Nuds;
using SmashForge.Rendering.Meshes;

namespace SmashForge.Rendering
{
    static class MaterialPreviewRendering
    {
        public static Task RenderAllPresetsToFiles { get { return renderAllPresetsToFiles; } }
        private static Task renderAllPresetsToFiles;

        // Resources for rendering.
        // These need to be regenerated every time due to using a separate thread.
        private static Mesh3D screenTriangle;

        private static Dictionary<NudEnums.DummyTexture, Texture> dummyTextures = new Dictionary<NudEnums.DummyTexture, Texture>();
        private static Shader shader;

        // Reduce file size.
        private static readonly int width = 256;
        private static readonly int height = 256;

        public static void RenderMaterialPresetPreviewsToFilesThreaded()
        {
            // Shaders weren't loaded.
            if (OpenTkSharedResources.SetupStatus != OpenTkSharedResources.SharedResourceStatus.Initialized)
                return;

            renderAllPresetsToFiles = Task.Run(() =>
            {
                RenderPresetsToFiles();
            });
        }

        private static void RenderPresetsToFiles()
        {
            using (GameWindow gameWindow = OpenTkSharedResources.CreateGameWindowContext(width, height))
            {
                // Resource creation.
                screenTriangle = ScreenDrawing.CreateScreenTriangle();
                shader = OpenTkSharedResources.shaders["NudSphere"];

                // Skip thumbnail generation if the shader didn't compile.
                if (!shader.LinkStatusIsOk)
                    return;

                // HACK: This isn't a very clean way to pass resources around.
                NudMatSphereDrawing.LoadMaterialSphereTextures();
                Dictionary<NudEnums.DummyTexture, Texture> dummyTextures = RenderTools.CreateNudDummyTextures();

                CreateNudSphereShader();

                foreach (string file in Directory.EnumerateFiles(MainForm.executableDir + "\\materials", "*.nmt", SearchOption.AllDirectories))
                {
                    Nud.Material material = NudMaterialEditor.ReadMaterialListFromPreset(file)[0];
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
            OpenTkSharedResources.shaders.Remove("NudSphere");
            ShaderTools.CreateAndAddShader("NudSphere", nudMatShaders);
        }

        private static void RenderMaterialPresetToFile(string presetName, Nud.Material material, Dictionary<NudEnums.DummyTexture, Texture> dummyTextures)
        {
            // Setup new dimensions.
            GL.Viewport(0, 0, width, height);

            Framebuffer framebuffer = new Framebuffer(FramebufferTarget.Framebuffer, width, height, PixelInternalFormat.Rgba);
            framebuffer.Bind();

            // Draw the material to a textured quad.
            NudMatSphereDrawing.DrawNudMaterialSphere(shader, material, screenTriangle, dummyTextures);

            // Save output.
            using (Bitmap image = framebuffer.ReadImagePixels(true))
            {
                string outputPath =  String.Format("{0}\\Preview Images\\{1}.png", MainForm.executableDir, presetName);
                image.Save(outputPath);
            }
        }
    }
}
