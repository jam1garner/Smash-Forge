using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using Smash_Forge.Rendering.Lights;
using System.Windows.Forms;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.Tools;


namespace Smash_Forge.Rendering
{
    class ShaderTools
    {
        public static bool hasSetupShaders = false;

        public static void SetupShaders()
        {
            // Reset the shaders first so that shaders can be replaced.
            Runtime.shaders.Clear();

            SetupScreenShaders();
            SetupNudShaders();
            SetupMiscShaders();

            hasSetupShaders = true;
        }

        private static void SetupMiscShaders()
        {
            CreateShader("Mbn", "\\lib\\Shader\\3ds");
            CreateShader("Dat", "\\lib\\Shader\\Melee");
            CreateShader("Point", "\\lib\\Shader");
        }

        private static void SetupNudShaders()
        {
            // Wii U NUD Shaders.
            List<String> nudShaders = new List<string>() { "Nud\\StageLighting.frag", "Nud\\Bayo.frag", "Nud\\SmashShader.frag", "Utility\\Wireframe.frag",
                                                           "Utility\\Utility.frag", "Nud\\EdgeDistance.geom"};
            CreateShader("Nud", "\\lib\\Shader\\Nud", nudShaders);
            CreateShader("NudDebug", "\\lib\\Shader\\Nud\\", nudShaders);

            // Wii U NUD Material Preview Shaders.
            List<String> nudMatShaders = new List<string>() { "Nud\\StageLighting.frag", "Nud\\Bayo.frag",  "Nud\\SmashShader.frag", "Utility\\Utility.frag" };
            CreateShader("NudSphere", "\\lib\\Shader\\Nud", nudMatShaders);
        }

        private static void SetupScreenShaders()
        {
            // Screen Shaders. A single vertex shader is shared to calculate UVs for all these shaders.
            CreateShader("Texture", "\\lib\\Shader", new List<string>() { "PostProcessing\\ScreenTexCoordMain.vert" });
            CreateShader("ScreenQuad", "\\lib\\Shader\\PostProcessing", new List<string>() { "PostProcessing\\ScreenTexCoordMain.vert" });
            CreateShader("Gradient", "\\lib\\Shader\\PostProcessing", new List<string>() { "PostProcessing\\ScreenTexCoordMain.vert" });
        }

        private static void CreateShader(string shaderName, string shaderFolder, List<String> additionalShaderFiles = null)
        {
            // All shaders should be named shaderName.frag, shaderName.vert, etc.
            if (!Runtime.shaders.ContainsKey(shaderName))
            {
                Shader shader = new Shader();

                // Additional shaders for utility functions. These should be loaded first.
                // The order in which shaders are loaded is important.
                if (additionalShaderFiles != null)
                {
                    foreach (string file in additionalShaderFiles)
                        shader.LoadShader(MainForm.executableDir + "\\lib\\Shader\\" + file);
                }

                string shaderFileName = MainForm.executableDir + shaderFolder + "\\" + shaderName;

                // Required shaders. These files can be omitted and specified in the additionalShaderFiles list.
                if (File.Exists(shaderFileName + ".vert"))
                    shader.LoadShader(shaderFileName + ".vert");
                if (File.Exists(shaderFileName + ".frag"))
                    shader.LoadShader(shaderFileName + ".frag");

                // Geometry shaders are optional.
                if (File.Exists(shaderFileName + ".geom"))
                    shader.LoadShader(shaderFileName + ".geom");



                Runtime.shaders.Add(shaderName, shader);
            }
        }
        
        public static void LightColorVector3Uniform(Shader shader, LightColor color, string name)
        {
            // Not declared in the Shader class to make the Shader class more portable.
            shader.SetVector3(name, color.R, color.G, color.B);
        }

        public static void SystemColorVector3Uniform(Shader shader, System.Drawing.Color color, string name)
        {
            shader.SetVector3(name, ColorTools.Vector4FromColor(color).Xyz);
        }

        public static void SaveErrorLogs()
        {
            // Export error logs for all the shaders.
            List<String> compileErrorList = new List<String>(); 
            int successfulCompilations = Runtime.shaders.Count;
            foreach (string shaderName in Runtime.shaders.Keys)
            {
                if (!Runtime.shaders[shaderName].ProgramCreatedSuccessfully())
                {
                    compileErrorList.Add(shaderName);
                    successfulCompilations -= 1;
                }

                //Runtime.shaders[shaderName].SaveErrorLog(shaderName);
            }

            // Display how many shaders correctly compiled.
            string message = String.Format("{0} of {1} shaders compiled successfully. Error logs have been saved to the Shader Error Logs directory.\n",
                successfulCompilations, Runtime.shaders.Count);

            // Display the shaders that didn't compile.
            if (compileErrorList.Count > 0)
            {
                message += "The following shaders failed to compile:\n";
                foreach (String shader in compileErrorList)
                    message += shader + "\n";
            }

            MessageBox.Show(message, "GLSL Shader Error Logs Exported");
        }
    }
}
