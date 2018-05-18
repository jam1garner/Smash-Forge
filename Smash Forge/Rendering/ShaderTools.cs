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

namespace Smash_Forge.Rendering
{
    class ShaderTools
    {
        public static void SetupShaders()
        {
            // Reset the shaders first so that shaders can be replaced.
            Runtime.shaders.Clear();
            CreateShader("Texture",     "/lib/Shader/");
            CreateShader("Screen_Quad", "/lib/Shader/");
            CreateShader("NUD",         "/lib/Shader/");
            CreateShader("MBN",         "/lib/Shader/");
            CreateShader("DAT",         "/lib/Shader/");
            CreateShader("NUD_Debug",   "/lib/Shader/");
            CreateShader("Gradient",    "/lib/Shader/");
            CreateShader("Shadow",      "/lib/Shader/");
            CreateShader("Point",       "/lib/Shader/");
            CreateShader("Nud_Sphere",  "/lib/Shader/");
        }

        public static void CreateShader(string shaderName, string shaderFolder, List<String> additionalShaderFiles = null)
        {
            // All shaders should be named shaderName.frag, shaderName.vert, etc.
            if (!Runtime.shaders.ContainsKey(shaderName))
            {
                Shader shader = new Shader();
                string shaderFile = MainForm.executableDir + shaderFolder + shaderName;
                shader.LoadShader(shaderFile + ".vert");
                shader.LoadShader(shaderFile + ".frag");

                // Geometry shaders are optional.
                if (File.Exists(shaderFile + ".geom"))
                    shader.LoadShader(shaderFile + ".geom");

                if (additionalShaderFiles != null)
                {
                    foreach (string file in additionalShaderFiles)
                    {

                    }
                }

                Runtime.shaders.Add(shaderName, shader);
            }
        }

        public static void BoolToIntShaderUniform(Shader shader, bool value, string name)
        {
            // Else if is faster than ternary operator. 
            if (value)
                GL.Uniform1(shader.GetVertexAttributeUniformLocation(name), 1);
            else
                GL.Uniform1(shader.GetVertexAttributeUniformLocation(name), 0);
        }

        public static void LightColorVector3Uniform(Shader shader, LightColor color, string name)
        {
            GL.Uniform3(shader.GetVertexAttributeUniformLocation(name), color.R, color.G, color.B);
        }

        public static void SystemColorVector3Uniform(Shader shader, System.Drawing.Color color, string name)
        {
            GL.Uniform3(shader.GetVertexAttributeUniformLocation(name), ColorTools.Vector4FromColor(color).Xyz);
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

                Runtime.shaders[shaderName].SaveErrorLog(shaderName);
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
