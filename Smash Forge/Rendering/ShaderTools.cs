using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using Smash_Forge.Rendering.Lights;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.Tools;


namespace Smash_Forge.Rendering
{
    class ShaderTools
    {
        public static void SetupShaders()
        {
            // Reset the shaders first so that shaders can be replaced.
            Runtime.shaders = new Dictionary<string, ShaderOld>();
            CreateShader("Texture", "/lib/Shader/");
            CreateShader("Screen_Quad", "/lib/Shader/");
            CreateShader("NUD", "/lib/Shader/");
            CreateShader("MBN", "/lib/Shader/");
            CreateShader("DAT", "/lib/Shader/");
            CreateShader("BFRES", "/lib/Shader/");
            CreateShader("BFRES_PBR", "/lib/Shader/");         
            CreateShader("KCL", "/lib/Shader/");
            CreateShader("NUD_Debug", "/lib/Shader/");
            CreateShader("Gradient", "/lib/Shader/");
            CreateShader("Quad", "/lib/Shader/");
            CreateShader("Blur", "/lib/Shader/");
            CreateShader("Shadow", "/lib/Shader/");
            CreateShader("Point", "/lib/Shader/");
        }

        public static void CreateShader(string name, string normalPath)
        {
            if (!Runtime.shaders.ContainsKey(name))
            {
                ShaderOld shader = new ShaderOld();
                shader.vertexShader(MainForm.executableDir + normalPath + name + "_vs.txt");
                shader.fragmentShader(MainForm.executableDir + normalPath + name + "_fs.txt");
                Runtime.shaders.Add(name, shader);
            }
        }

        public static void BoolToIntShaderUniform(ShaderOld shader, bool value, string name)
        {
            // Else if is faster than ternary operator. 
            if (value)
                GL.Uniform1(shader.getAttribute(name), 1);
            else
                GL.Uniform1(shader.getAttribute(name), 0);
        }

        public static void LightColorVector3Uniform(ShaderOld shader, LightColor color, string name)
        {
            GL.Uniform3(shader.getAttribute(name), color.R, color.G, color.B);
        }

        public static void SystemColorVector3Uniform(Shader shader, System.Drawing.Color color, string name)
        {
            shader.SetVector3(name, ColorTools.Vector4FromColor(color).Xyz);
        }
    }
}
