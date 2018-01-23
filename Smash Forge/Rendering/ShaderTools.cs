using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Smash_Forge
{
    class ShaderTools
    {
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
    }
}
