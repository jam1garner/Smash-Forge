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
            CreateShader("Quad", "/lib/Shader/", "/lib/Shader/");
            CreateShader("Blur", "/lib/Shader/", "/lib/Shader/");
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
                    shader.LoadShader(MainForm.executableDir + legacyPath + name + "_vs.txt", ShaderType.VertexShader);
                    shader.LoadShader(MainForm.executableDir + legacyPath + name + "_fs.txt", ShaderType.FragmentShader);
                }
                else
                {
                    shader.LoadShader(MainForm.executableDir + normalPath + name + "_vs.txt", ShaderType.VertexShader);
                    shader.LoadShader(MainForm.executableDir + normalPath + name + "_fs.txt", ShaderType.FragmentShader);
                }
                Runtime.shaders.Add(name, shader);
            }
        }

        public static void BoolToIntShaderUniform(Shader shader, bool value, string name)
        {
            // Else if is faster than ternary operator. 
            if (value)
                GL.Uniform1(shader.GetAttribute(name), 1);
            else
                GL.Uniform1(shader.GetAttribute(name), 0);
        }

        public static void LightColorVector3Uniform(Shader shader, LightColor color, string name)
        {
            GL.Uniform3(shader.GetAttribute(name), color.R, color.G, color.B);
        }

        public static void SystemColorVector3Uniform(Shader shader, System.Drawing.Color color, string name)
        {
            GL.Uniform3(shader.GetAttribute(name), ColorTools.Vector4FromColor(color).Xyz);
        }

        public static void SaveErrorLogs()
        {
            int successfulCompilations = Runtime.shaders.Count;
            foreach (string key in Runtime.shaders.Keys)
            {
                if (!Runtime.shaders[key].CompiledSuccessfully())
                    successfulCompilations -= 1;

                Runtime.shaders[key].SaveErrorLog(key);
            }

            MessageBox.Show(String.Format("{0} of {1} shaders compiled successfully. Error logs have been saved to the Shader Error Logs directory.", 
                successfulCompilations, Runtime.shaders.Count), "GLSL Shader Error Logs Export");
        }
    }
}
