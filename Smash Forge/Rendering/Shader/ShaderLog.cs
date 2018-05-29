using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;

namespace Smash_Forge.Rendering
{
    class ShaderLog
    {
        private StringBuilder errorLog = new StringBuilder();

        public void AppendProgramInfoLog(int programId)
        {
            errorLog.AppendLine("Program Errors:");
            string error = GL.GetProgramInfoLog(programId);
            errorLog.AppendLine(error);
        }

        public void AppendUniformNameErrors(HashSet<string> invalidUniformNames)
        {
            foreach (string uniform in invalidUniformNames)
                errorLog.AppendLine(String.Format("[Warning] Attempted to set undeclared uniform variable {0}.", uniform));
        }

        public void AppendHardwareAndVersionInfo()
        {
            errorLog.AppendLine("Vendor: " + GL.GetString(StringName.Vendor));
            errorLog.AppendLine("Renderer: " + GL.GetString(StringName.Renderer));
            errorLog.AppendLine("OpenGL Version: " + GL.GetString(StringName.Version));
            errorLog.AppendLine("GLSL Version: " + GL.GetString(StringName.ShadingLanguageVersion));
            errorLog.AppendLine();
        }

        public void AppendShaderInfoLog(string shaderName, int shader, ShaderType type)
        {
            // Append compilation errors for the current shader. 
            errorLog.AppendLine(shaderName + " Shader Log:");
            string error = GL.GetShaderInfoLog(shader);
            if (error == "")
                errorLog.AppendLine("No Error");
            else
                errorLog.AppendLine(GL.GetShaderInfoLog(shader));

            errorLog.AppendLine(); // line between shaders
        }

        public void SaveToErrorLogDir(string shaderName)
        {
            // Create the error logs directory if not found.
            string errorLogDirectory = MainForm.executableDir + "\\Shader Error Logs\\";
            if (!Directory.Exists(errorLogDirectory))
                Directory.CreateDirectory(errorLogDirectory);

            // Export the error log.
            string logExport = errorLog.ToString();
            File.WriteAllText(errorLogDirectory + shaderName + " Error Log.txt", logExport.Replace("\n", Environment.NewLine));
        }

        override public string ToString()
        {
            return errorLog.ToString();
        }
    }
}
