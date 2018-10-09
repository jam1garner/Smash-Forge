using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.Utils;
using Smash_Forge.Rendering.Lights;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SFGraphics.GlUtils;

namespace Smash_Forge.Rendering
{
    static class ShaderTools
    {
        private static string shaderSourceDirectory;
        private static string shaderCacheDirectory;

        public static void SetUpShaders(bool forceBinaryUpdate = false)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            shaderSourceDirectory = Path.Combine(MainForm.executableDir, "Shader");
            shaderCacheDirectory = Path.Combine(MainForm.executableDir, "Shader", "Shader Cache");

            if (forceBinaryUpdate)
                DeleteShaderBinaries(forceBinaryUpdate);

            // Reset the shaders first so that shaders can be replaced.
            OpenTKSharedResources.shaders.Clear();
            SetUpAllShaders();

            System.Diagnostics.Debug.WriteLine("Shader Setup: {0} ms", stopwatch.ElapsedMilliseconds);
        }

        private static void DeleteShaderBinaries(bool clearBinaries)
        {
            if (Directory.Exists(shaderCacheDirectory))
            {
                foreach (string file in Directory.EnumerateFiles(shaderCacheDirectory))
                {
                    File.Delete(file);
                }
            }
        }

        private static void SetUpAllShaders()
        {
            SetUpScreenShaders();
            SetUpNudShaders();
            SetUpMiscShaders();
            SetUpBfresShaders();
            SetUpMeleeShaders();
        }
        
        private static void SetUpMeleeShaders()
        {
            CreateAndAddShader("Dat", "Melee\\Dat.vert", "Melee\\Dat.frag", "Melee\\MeleeUtils.frag");
            CreateAndAddShader("DatDebug", "Melee\\Dat.vert", "Melee\\DatDebug.frag", "Melee\\MeleeUtils.frag");
        }

        private static void SetUpBfresShaders()
        {
            List<string> bfresSharedShaders = new List<string>
            {
                "Bfres\\BFRES.vert",
                "Bfres\\BFRES_utility.frag",
                "Utility\\Utility.frag"
            };

            List<string> bfresDebugShaders = new List<string>(bfresSharedShaders);
            bfresDebugShaders.Add("Bfres\\BFRES_Debug.frag");

            List<string> bfresShaders = new List<string>(bfresSharedShaders);
            bfresShaders.Add("Bfres\\BFRES.frag");

            List<string> bfresPBRShaders = new List<string>(bfresSharedShaders);
            bfresPBRShaders.Add("Bfres\\BFRES_PBR.frag");


            List<string> bfresBotwShaders = new List<string>(bfresSharedShaders);
            bfresBotwShaders.Add("Bfres\\BFRES_Botw.frag");

            CreateAndAddShader("BFRES", bfresShaders.ToArray());
            CreateAndAddShader("BFRES_PBR", bfresPBRShaders.ToArray());
            CreateAndAddShader("BFRES_Debug", bfresDebugShaders.ToArray());
            CreateAndAddShader("BFRES_Botw", bfresBotwShaders.ToArray());
            CreateAndAddShader("KCL", "KCL.frag", "KCL.vert");
        }

        private static void SetUpMiscShaders()
        {
            CreateAndAddShader("Mbn", "3ds\\Mbn.frag", "3ds\\Mbn.vert");
            CreateAndAddShader("Point", "Point.frag", "Point.vert");
            CreateAndAddShader("Shadow", "Shadow.frag", "Shadow.vert");
            CreateAndAddShader("ForgeMesh", "ForgeMesh.frag", "ForgeMesh.vert");
            CreateAndAddShader("SolidColor3D", "SolidColor3D.frag", "SolidColor3D.vert");
            CreateAndAddShader("UV", "NUD\\UV.frag", "NUD\\UV.vert", "NUD\\UV.geom", "NUD\\EdgeDistance.geom", "Utility\\Utility.frag", "Utility\\Wireframe.frag");
        }

        private static void SetUpNudShaders()
        {
            List<string> sharedNudShaders = new List<string>()
            {
                "Nud\\NUD.vert",
                "Nud\\NUD.geom",
                "Nud\\StageLighting.frag",
                "Nud\\Bayo.frag",
                "Nud\\SmashShader.frag",
                "Utility\\Wireframe.frag",
                "Utility\\Utility.frag", "Nud\\EdgeDistance.geom"
            };

            // Wii U NUD Shaders.
            List<string> nudShaders = new List<string>(sharedNudShaders);
            nudShaders.Add("Nud\\NUD.frag");

            CreateAndAddShader("Nud", nudShaders.ToArray());

            List<string> nudDebugShaders = new List<string>(sharedNudShaders);
            nudDebugShaders.Add("Nud\\NudDebug.frag");
                
            CreateAndAddShader("NudDebug", nudDebugShaders.ToArray());

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
                Shader shader = CreateShader(shaderProgramName, shaderRelativePaths);
                OpenTKSharedResources.shaders.Add(shaderProgramName, shader);
            }
        }

        private static Shader CreateShader(string shaderProgramName, string[] shaderRelativePaths)
        {
            Shader shader = new Shader();

            if (!Directory.Exists(shaderCacheDirectory))
                Directory.CreateDirectory(shaderCacheDirectory);

            // Loading precompiled binaries isn't core in 3.30.
            // Most people will have more modern GPUs that support this, however.
            bool canLoadBinaries = OpenGLExtensions.IsAvailable("GL_ARB_get_program_binary");

            // We can't load the binary without the proper format.
            string compiledBinaryPath = Path.Combine(shaderCacheDirectory, shaderProgramName + ".bin");
            string compiledFormatPath = Path.Combine(shaderCacheDirectory, shaderProgramName + "_format.bin");

            if (canLoadBinaries && File.Exists(compiledBinaryPath) && File.Exists(compiledFormatPath))
            {
                LoadFromPrecompiledBinary(shader, compiledBinaryPath, compiledFormatPath);

                if (!shader.LinkStatusIsOk)
                {
                    // Load from source and generate binary.
                    LoadShaderFiles(shader, shaderRelativePaths);
                }
            }
            else
            {
                // Load from source.
                // Don't generate binaries for programs that did not link.
                // Attempting to load them will crash.
                LoadShaderFiles(shader, shaderRelativePaths);
                if (canLoadBinaries && shader.LinkStatusIsOk)
                    SavePrecompiledBinaryAndFormat(shader, compiledBinaryPath, compiledFormatPath);
            }

            return shader;
        }

        private static void LoadFromPrecompiledBinary(Shader shader, string compiledBinaryPath, string compiledFormatPath)
        {
            byte[] programBinary = File.ReadAllBytes(compiledBinaryPath);

            int formatValue = BitConverter.ToInt32(File.ReadAllBytes(compiledFormatPath), 0);
            BinaryFormat binaryFormat = (BinaryFormat)formatValue;

            // Number of supported binary formats.
            int binaryFormatCount;
            GL.GetInteger(GetPName.NumProgramBinaryFormats, out binaryFormatCount);

            // Get all supported formats.
            int[] binaryFormats = new int[binaryFormatCount];
            GL.GetInteger(GetPName.ProgramBinaryFormats, binaryFormats);

            if (binaryFormats.Contains(formatValue))
                shader.LoadProgramBinary(programBinary, binaryFormat);
        }

        private static void SavePrecompiledBinaryAndFormat(Shader shader, string compiledBinaryPath, string compiledFormatPath)
        {
            // Save program binary and format.
            BinaryFormat binaryFormat;
            byte[] programBinary = shader.GetProgramBinary(out binaryFormat);

            File.WriteAllBytes(compiledBinaryPath, programBinary);
            File.WriteAllBytes(compiledFormatPath, BitConverter.GetBytes((int)binaryFormat));
        }

        private static void LoadShaderFiles(Shader shader, string[] shaderRelativePaths)
        {
            var shaders = new List<Tuple<string, ShaderType, string>>();
            foreach (string file in shaderRelativePaths)
            {
                // The input paths are relative to the main shader directory.
                string shaderPath = shaderSourceDirectory + "\\" + file;
                if (!File.Exists(shaderPath))
                    continue;

                // Read the shader file.
                string shaderName = Path.GetFileNameWithoutExtension(shaderPath);
                string shaderSource = File.ReadAllText(shaderPath);

                // Determine the shader type based on the file extension.
                ShaderType shaderType = ShaderType.FragmentShader;
                if (file.EndsWith(".vert"))
                    shaderType = ShaderType.VertexShader;
                else if (file.EndsWith(".frag"))
                    shaderType = ShaderType.FragmentShader;
                else if (file.EndsWith(".geom"))
                    shaderType = ShaderType.GeometryShader;

                shaders.Add(new Tuple<string, ShaderType, string>(shaderSource, shaderType, shaderName));
            }
            shader.LoadShaders(shaders);
        }

        public static void LightColorVector3Uniform(Shader shader, LightColor color, string name)
        {
            // Not declared in the Shader class to make the Shader class more portable.
            shader.SetVector3(name, color.R, color.G, color.B);
        }

        public static void SystemColorVector3Uniform(Shader shader, System.Drawing.Color color, string name)
        {
            shader.SetVector3(name, ColorUtils.Vector4FromColor(color).Xyz);
        }

        public static void SaveErrorLogs()
        {
            // Export error logs for all the shaders.
            List<String> compileErrorList = new List<String>(); 
            int successfulCompilations = OpenTKSharedResources.shaders.Count;
            foreach (string shaderName in OpenTKSharedResources.shaders.Keys)
            {
                if (!OpenTKSharedResources.shaders[shaderName].LinkStatusIsOk)
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
