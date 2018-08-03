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
        private static string shaderMainDir;

        public static void SetupShaders()
        {
            shaderMainDir = MainForm.executableDir + " \\lib\\Shader\\";

            // Reset the shaders first so that shaders can be replaced.
            OpenTKSharedResources.shaders.Clear();
            SetupAllShaders();
        }

        private static void SetupAllShaders()
        {
            SetUpScreenShaders();
            SetUpNudShaders();
            SetUpMiscShaders();
            SetUpBfresShaders();
        }

        private static void SetUpBfresShaders()
        {
            CreateAndAddShader("BFRES", "BFRES.frag", "BFRES.vert");
            CreateAndAddShader("BFRES_PBR", "BFRES_PBR.frag", "BFRES_PBR.vert");
            CreateAndAddShader("KCL", "KCL.frag", "KCL.vert");
        }

        private static void SetUpMiscShaders()
        {
            CreateAndAddShader("Mbn", "3ds\\Mbn.frag", "3ds\\Mbn.vert");
            CreateAndAddShader("Dat", "Melee\\Dat.frag", "Melee\\Dat.vert");
            CreateAndAddShader("Point", "Point.frag", "Point.vert");
            CreateAndAddShader("Shadow", "Shadow.frag", "Shadow.vert");
            CreateAndAddShader("SolidColor3D", "SolidColor3D.frag", "SolidColor3D.vert");
            CreateAndAddShader("UV", "NUD\\UV.frag", "NUD\\UV.vert", "NUD\\UV.geom", "NUD\\EdgeDistance.geom", "Utility\\Utility.frag", "Utility\\Wireframe.frag");
        }

        private static void SetUpNudShaders()
        {
            // Wii U NUD Shaders.
            string[] nudShaders = new string[]
            {
                "Nud\\NUD.frag",
                "Nud\\NUD.vert",
                "Nud\\NUD.geom",
                "Nud\\StageLighting.frag",
                "Nud\\Bayo.frag",
                "Nud\\SmashShader.frag",
                "Utility\\Wireframe.frag",
                "Utility\\Utility.frag", "Nud\\EdgeDistance.geom"
            };
            CreateAndAddShader("Nud", nudShaders);

            string[] nudDebugShaders = new string[]
            {
                "Nud\\NudDebug.frag",
                "Nud\\NudDebug.vert",
                "Nud\\NudDebug.geom",
                "Nud\\StageLighting.frag",
                "Nud\\Bayo.frag",
                "Nud\\SmashShader.frag",
                "Utility\\Wireframe.frag",
                "Utility\\Utility.frag", "Nud\\EdgeDistance.geom"
            };
            CreateAndAddShader("NudDebug", nudDebugShaders);

            // Wii U NUD Material Preview Shaders.
            string[] nudMatShaders = new string[]
            {
                "Nud\\NudSphere.frag",
                "Nud\\NudSphere.vert",
                "Nud\\StageLighting.frag",
                "Nud\\Bayo.frag",
                "Nud\\SmashShader.frag",
                "Utility\\Utility.frag"
            };
            CreateAndAddShader("NudSphere", nudMatShaders);
        }

        private static void SetUpScreenShaders()
        {
            // Fullscreen "quad" shaders.
            // A single vertex shader is shared to calculate UVs for all these shaders.
            CreateAndAddShader("Texture", "Texture.frag", "PostProcessing\\ScreenTexCoordMain.vert");
            CreateAndAddShader("ScreenQuad", "PostProcessing\\ScreenQuad.frag", "PostProcessing\\ScreenTexCoordMain.vert");
            CreateAndAddShader("Gradient", "PostProcessing\\Gradient.frag", "PostProcessing\\ScreenTexCoordMain.vert");
        }
        
        public static void CreateAndAddShader(string shaderProgramName, params string[] shaderRelativePaths)
        {
            if (!OpenTKSharedResources.shaders.ContainsKey(shaderProgramName))
            {
                Shader shader = CreateShader(shaderRelativePaths);
                OpenTKSharedResources.shaders.Add(shaderProgramName, shader);
            }
        }

        private static Shader CreateShader(string[] shaderRelativePaths)
        {
            Shader shader = new Shader();
            LoadShaderFiles(shader, shaderRelativePaths);
            return shader;
        }

        private static void LoadShaderFiles(Shader shader, string[] shaderRelativePaths)
        {
            foreach (string file in shaderRelativePaths)
            {
                // The input paths are relative to the main shader directory.
                string shaderPath = shaderMainDir + "\\" + file;
                if (!File.Exists(shaderPath))
                    continue;

                // Read the shader file.
                string shaderName = Path.GetFileNameWithoutExtension(shaderPath);
                string shaderSource = File.ReadAllText(shaderPath);

                // Determine the shader type based on the file extension.
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
            int successfulCompilations = OpenTKSharedResources.shaders.Count;
            foreach (string shaderName in OpenTKSharedResources.shaders.Keys)
            {
                if (!OpenTKSharedResources.shaders[shaderName].ProgramCreatedSuccessfully)
                {
                    compileErrorList.Add(shaderName);
                    successfulCompilations -= 1;
                }

                // Create the error logs directory if not found.
                string errorLogDirectory = MainForm.executableDir + "\\Shader Error Logs\\";
                if (!Directory.Exists(errorLogDirectory))
                    Directory.CreateDirectory(errorLogDirectory);

                // Export the error log.
                string logExport = OpenTKSharedResources.shaders[shaderName].GetErrorLog();
                File.WriteAllText(errorLogDirectory + shaderName + " Error Log.txt", logExport.Replace("\n", Environment.NewLine));
            }

            // Display how many shaders correctly compiled.
            string message = String.Format("{0} of {1} shaders compiled successfully. Error logs have been saved to the Shader Error Logs directory.\n",
                successfulCompilations, OpenTKSharedResources.shaders.Count);

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
