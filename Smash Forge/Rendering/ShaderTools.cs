using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using Smash_Forge.Rendering.Lights;

namespace Smash_Forge.Rendering
{
    class ShaderTools
    {
        public static void SetupShaders()
        {
            // Reset the shaders first so that shaders can be replaced.
            Runtime.shaders = new Dictionary<string, Shader>();
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
        }

        public static void CreateShader(string name, string legacyPath, string normalPath)
        {
            if (!Runtime.shaders.ContainsKey(name))
            {
                Shader shader = new Shader();
                if (Runtime.useLegacyShaders)
                {
                    shader.vertexShader(MainForm.executableDir + legacyPath + name + "_vs.txt");
                    shader.fragmentShader(MainForm.executableDir + legacyPath + name + "_fs.txt");
                }
                else
                {
                    shader.vertexShader(MainForm.executableDir + normalPath + name + "_vs.txt");
                    shader.fragmentShader(MainForm.executableDir + normalPath + name + "_fs.txt");
                }
                Runtime.shaders.Add(name, shader);
            }
        }

        public static void BoolToIntShaderUniform(Shader shader, bool value, string name)
        {
            // Else if is faster than ternary operator. 
            if (value)
                GL.Uniform1(shader.getAttribute(name), 1);
            else
                GL.Uniform1(shader.getAttribute(name), 0);
        }

        public static void LightColorVector3Uniform(Shader shader, LightColor color, string name)
        {
            GL.Uniform3(shader.getAttribute(name), color.R, color.G, color.B);
        }
    }
}
