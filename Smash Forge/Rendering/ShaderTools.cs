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
            CreateShader("Texture", "/lib/Shader/Legacy/", "/lib/Shader/");
            CreateShader("Screen_Quad", "/lib/Shader/", "/lib/Shader/");
            CreateShader("NUD", "/lib/Shader/Legacy/", "/lib/Shader/");
            CreateShader("MBN", "/lib/Shader/Legacy/", "/lib/Shader/");
            CreateShader("DAT", "/lib/Shader/Legacy/", "/lib/Shader/");
            CreateShader("NUD_Debug", "/lib/Shader/Legacy/", "/lib/Shader/");
            CreateShader("Gradient", "/lib/Shader/", "/lib/Shader/");
            CreateShader("Shadow", "/lib/Shader/", "/lib/Shader/");
            CreateShader("Point", "/lib/Shader/", "/lib/Shader/");
            CreateShader("Nud_Sphere", "/lib/Shader/", "/lib/Shader/");
        }

        public static void CreateShader(string name, string legacyPath, string normalPath)
        {
            if (!Runtime.shaders.ContainsKey(name))
            {
                Shader shader = new Shader();
                if (Runtime.useLegacyShaders)
                {
                    shader.LoadShader(MainForm.executableDir + legacyPath + name + ".vert", ShaderType.VertexShader);
                    shader.LoadShader(MainForm.executableDir + legacyPath + name + ".frag", ShaderType.FragmentShader);
                } 
                else
                {
                    string shaderFile = MainForm.executableDir + normalPath + name;
                    shader.LoadShader(shaderFile + ".vert", ShaderType.VertexShader);
                    shader.LoadShader(shaderFile + ".frag", ShaderType.FragmentShader);

                    if (File.Exists(shaderFile + ".geom"))
                    {
                        shader.LoadShader(shaderFile + ".geom", ShaderType.GeometryShader);
                    }
                }
                Runtime.shaders.Add(name, shader);
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
                if (!Runtime.shaders[shaderName].CompiledSuccessfully())
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
