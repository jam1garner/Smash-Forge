using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using SFGraphics.GLObjects;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects.Textures;
using Smash_Forge.Rendering.Meshes;
using Smash_Forge.Filetypes.Models.Nuds;

namespace Smash_Forge.Rendering
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
            if (OpenTKSharedResources.SetupStatus != OpenTKSharedResources.SharedResourceStatus.Initialized)
                return;

            renderAllPresetsToFiles = Task.Run(() =>
            {
                RenderPresetsToFiles();
            });
        }

        private static void RenderPresetsToFiles()
        {
            using (GameWindow gameWindow = OpenTKSharedResources.CreateGameWindowContext(width, height))
            {
                // Resource creation.
                screenTriangle = ScreenDrawing.CreateScreenTriangle();
                shader = OpenTKSharedResources.shaders["NudSphere"];

                // Skip thumbnail generation if the shader didn't compile.
                if (!shader.LinkStatusIsOk)
                    return;

                // HACK: This isn't a very clean way to pass resources around.
                NudMatSphereDrawing.LoadMaterialSphereTextures();
                Dictionary<NudEnums.DummyTexture, Texture> dummyTextures = RenderTools.CreateNudDummyTextures();

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
            OpenTKSharedResources.shaders.Remove("NudSphere");
            ShaderTools.CreateAndAddShader("NudSphere", nudMatShaders);
        }

        private static void RenderMaterialPresetToFile(string presetName, NUD.Material material, Dictionary<NudEnums.DummyTexture, Texture> dummyTextures)
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
