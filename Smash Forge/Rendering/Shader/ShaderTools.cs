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
        public static void SetupShaders()
        {
            // Reset the shaders first so that shaders can be replaced.
            Runtime.shaders.Clear();

            SetupScreenShaders();
            SetupNudShaders();
            SetupMiscShaders();
        }

        private static void SetupMiscShaders()
        {
            CreateAndAddShader("Mbn", "\\lib\\Shader\\3ds");
            CreateAndAddShader("Dat", "\\lib\\Shader\\Melee");
            CreateAndAddShader("Point", "\\lib\\Shader");
            CreateAndAddShader("Shadow", "\\lib\\Shader");
        }

        private static void SetupNudShaders()
        {
            // Wii U NUD Shaders.
            List<String> nudShaders = new List<string>() { "Nud\\StageLighting.frag", "Nud\\Bayo.frag", "Nud\\SmashShader.frag", "Utility\\Wireframe.frag",
                                                           "Utility\\Utility.frag", "Nud\\EdgeDistance.geom"};
            CreateAndAddShader("Nud", "\\lib\\Shader\\Nud", nudShaders);
            CreateAndAddShader("NudDebug", "\\lib\\Shader\\Nud\\", nudShaders);

            // Wii U NUD Material Preview Shaders.
            List<String> nudMatShaders = new List<string>() { "Nud\\StageLighting.frag", "Nud\\Bayo.frag",  "Nud\\SmashShader.frag", "Utility\\Utility.frag" };
            CreateAndAddShader("NudSphere", "\\lib\\Shader\\Nud", nudMatShaders);
        }

        private static void SetupScreenShaders()
        {
            // Screen Shaders. A single vertex shader is shared to calculate UVs for all these shaders.
            CreateAndAddShader("Texture", "\\lib\\Shader", new List<string>() { "PostProcessing\\ScreenTexCoordMain.vert" });
            CreateAndAddShader("ScreenQuad", "\\lib\\Shader\\PostProcessing", new List<string>() { "PostProcessing\\ScreenTexCoordMain.vert" });
            CreateAndAddShader("Gradient", "\\lib\\Shader\\PostProcessing", new List<string>() { "PostProcessing\\ScreenTexCoordMain.vert" });
        }

        private static void CreateAndAddShader(string shaderProgramName, string shaderFolder, List<String> additionalShaderFiles = null)
        {
            // All shaders should be named shaderName.frag, shaderName.vert, etc.
            if (!Runtime.shaders.ContainsKey(shaderProgramName))
            {
                Shader shader = CreateShader(shaderProgramName, shaderFolder, additionalShaderFiles);
                Runtime.shaders.Add(shaderProgramName, shader);
            }
        }

        private static Shader CreateShader(string shaderProgramName, string shaderFolder, List<string> additionalShaderFiles)
        {
            Shader shader = new Shader();

            // Additional shaders for utility functions. These should be loaded first.
            // The order in which shaders are loaded is important.
            if (additionalShaderFiles != null)
                LoadAdditionalShaderFiles(additionalShaderFiles, shader);

            string shaderPath = MainForm.executableDir + shaderFolder + "\\" + shaderProgramName;
            string shaderName = Path.GetFileNameWithoutExtension(shaderPath);

            // Required shaders. These files can be omitted and specified in the additionalShaderFiles list.
            if (File.Exists(shaderPath + ".vert"))
            {
                string shaderSource = File.ReadAllText(shaderPath + ".vert");
                shader.LoadShader(shaderSource, ShaderType.VertexShader, shaderName);
            }
            if (File.Exists(shaderPath + ".frag"))
            {
                string shaderSource = File.ReadAllText(shaderPath + ".frag");
                shader.LoadShader(shaderSource, ShaderType.FragmentShader, shaderName);
            }
            // Geometry shaders are optional.
            if (File.Exists(shaderPath + ".geom"))
            {
                string shaderSource = File.ReadAllText(shaderPath + ".geom");
                shader.LoadShader(shaderSource, ShaderType.GeometryShader, shaderName);
            }

            return shader;
        }

        private static void LoadAdditionalShaderFiles(List<string> additionalShaderFiles, Shader shader)
        {
            foreach (string file in additionalShaderFiles)
            {
                string shaderPath = MainForm.executableDir + "\\lib\\Shader\\" + file;
                string shaderName = Path.GetFileNameWithoutExtension(shaderPath);
                string shaderSource = File.ReadAllText(shaderPath);

                if (file.EndsWith(".vert"))
                    shader.LoadShader(shaderSource, ShaderType.VertexShader, shaderName);
                else if (file.EndsWith(".frag"))
                    shader.LoadShader(shaderSource, ShaderType.FragmentShader, shaderName);
                else if (file.EndsWith(".geom"))
                    shader.LoadShader(shaderSource, ShaderType.GeometryShader, shaderName);
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

                // Create the error logs directory if not found.
                string errorLogDirectory = MainForm.executableDir + "\\Shader Error Logs\\";
                if (!Directory.Exists(errorLogDirectory))
                    Directory.CreateDirectory(errorLogDirectory);

                // Export the error log.
                string logExport = Runtime.shaders[shaderName].GetErrorLog();
                File.WriteAllText(errorLogDirectory + shaderName + " Error Log.txt", logExport.Replace("\n", Environment.NewLine));
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
