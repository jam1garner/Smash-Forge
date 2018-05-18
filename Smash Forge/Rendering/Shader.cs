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
	public class Shader
	{
		public int programId;

        private bool programStatusIsOk = true;

        private int vertShaderId;
        private int fragShaderId;

        private bool hasGeometryShader = false;
        private int geomShaderId;

        private bool checkedCompilation = false;
        public bool HasCheckedCompilation { get { return checkedCompilation; } }

        private StringBuilder errorLog = new StringBuilder();

        int activeUniformCount = 0;
        int activeAttributeCount = 0;
		private Dictionary<string, int> vertexAttributeAndUniformLocations = new Dictionary<string, int>();

        public Shader()
        {
            programId = GL.CreateProgram();
            AppendHardwareAndVersionInfo();
        }

        public int GetVertexAttributeUniformLocation(string name)
        {
			int value;
            if (vertexAttributeAndUniformLocations.TryGetValue(name, out value))
            {
                return value;
            }
            else
                return -1;
		}

        public void EnableVertexAttributes()
        {
            // Only enable the necessary vertex attributes.
            for (int location = 0; location < activeAttributeCount; location++)
			{
                GL.EnableVertexAttribArray(location);
			}
		}

		public void DisableVertexAttributes()
        {
            // Disable all the vertex attributes. This could be used with a VAO in the future.
            for (int location = 0; location < activeAttributeCount; location++)
			{
				GL.DisableVertexAttribArray(location);
			}
		}

        public void SaveErrorLog(string shaderName)
        {
            // Create the error logs directory if not found.
            string errorLogDirectory = MainForm.executableDir + "\\Shader Error Logs\\";
            if (!Directory.Exists(errorLogDirectory))
                Directory.CreateDirectory(errorLogDirectory);

            // Export the error log.
            string logExport = errorLog.ToString();
            File.WriteAllText(errorLogDirectory +  shaderName + " Error Log.txt", logExport.Replace("\n", Environment.NewLine));
        }

		private void AddVertexAttribute(string name)
        {
            if (vertexAttributeAndUniformLocations.ContainsKey(name))
                vertexAttributeAndUniformLocations.Remove(name);
			int position = GL.GetAttribLocation(programId, name);
            vertexAttributeAndUniformLocations.Add(name, position);

            errorLog.AppendLine(name + ", " + "Position: " + position);
        }

        private void AddUniform(string name)
        {
            if (vertexAttributeAndUniformLocations.ContainsKey(name))
                vertexAttributeAndUniformLocations.Remove(name);
            int position = GL.GetUniformLocation(programId, name);
            vertexAttributeAndUniformLocations.Add(name, position);

            errorLog.AppendLine(name + ", " + "Position: " + position);
        }

        private void LoadUniforms()
        {
            GL.GetProgram(programId, GetProgramParameterName.ActiveUniforms, out activeUniformCount);
            errorLog.AppendLine("Uniform Count: " + activeUniformCount);

            for (int i = 0; i < activeUniformCount; i++)
            {
                ActiveUniformType uniformType;
                string uniform = GL.GetActiveUniform(programId, i, out int uniformSize, out uniformType);
                uniform = RemoveEndingBrackets(uniform);
                AddUniform(uniform);
            }
        }

        private void LoadAttributes()
        {
            GL.GetProgram(programId, GetProgramParameterName.ActiveAttributes, out activeAttributeCount);
            errorLog.AppendLine("Attribute Count: " + activeAttributeCount);

            for (int i = 0; i < activeAttributeCount; i++)
            {
                ActiveAttribType attributeType;
                string attribute = GL.GetActiveAttrib(programId, i, out int attributeSize, out attributeType);
                attribute = RemoveEndingBrackets(attribute);
                AddVertexAttribute(attribute);
            }
        }

        private static string RemoveEndingBrackets(string name)
        {
            // Removes the brackets at the end of the name.
            // Ex: "name[0]" becomes "name".
            int index = name.IndexOf('[');
            if (index > 0)
                name = name.Substring(0, index);
            return name;
        }

        public void LoadShader(string filePath)
        {
            // Compile and attach before linking.
            LoadShaderBasedOnType(filePath);
            GL.LinkProgram(programId);

            AppendShaderCompilationErrors();

            LoadAttributes();
            LoadUniforms();
        }

        private void LoadShaderBasedOnType(string filePath)
        {
            string shaderText = File.ReadAllText(filePath);
            if (filePath.EndsWith(".frag"))
            {
                AttachAndCompileShader(shaderText, ShaderType.FragmentShader, programId, out fragShaderId);
            }
            else if (filePath.EndsWith(".vert"))
            {
                AttachAndCompileShader(shaderText, ShaderType.VertexShader, programId, out vertShaderId);
            }
            else if (filePath.EndsWith(".geom"))
            {
                AttachAndCompileShader(shaderText, ShaderType.GeometryShader, programId, out geomShaderId);
                hasGeometryShader = true;
            }
            else
            {
                throw new NotSupportedException(filePath + " does not have a suppported shader type extension.");
            }
        }

        void AttachAndCompileShader(string shaderText, ShaderType type, int program, out int id)
        {
            id = GL.CreateShader(type);

            GL.ShaderSource(id, shaderText);
            GL.CompileShader(id);
            GL.AttachShader(program, id);

            errorLog.AppendLine(GL.GetShaderInfoLog(id));
        }

        private void AppendShaderCompilationErrors()
        {
            errorLog.AppendLine("Compilation Errors:");
            string error = GL.GetProgramInfoLog(programId);
            errorLog.AppendLine(error);
        }

        private void AppendHardwareAndVersionInfo()
        {
            errorLog.AppendLine("Vendor: " + GL.GetString(StringName.Vendor));
            errorLog.AppendLine("Renderer: " + GL.GetString(StringName.Renderer));
            errorLog.AppendLine("OpenGL Version: " + GL.GetString(StringName.Version));
            errorLog.AppendLine("GLSL Version: " + GL.GetString(StringName.ShadingLanguageVersion));
        }

        public bool ProgramCreatedSuccessfully()
        {
            if (!checkedCompilation)
                programStatusIsOk = CheckProgramStatus();
            return programStatusIsOk;
        }

        private bool CheckProgramStatus()
        {
            // This is checked frequently, so only do it once.
            checkedCompilation = true;

            int majorVersion = GL.GetInteger(GetPName.MajorVersion);
            int minorVersion = GL.GetInteger(GetPName.MinorVersion);
            if (majorVersion < 3 && minorVersion < 3)
                return false;

            // Rendering should be disabled if any error occurs.
            // Check for linker errors first. 
            int linkStatus = 1;
            GL.GetProgram(programId, GetProgramParameterName.LinkStatus, out linkStatus);
            if (linkStatus == 0)
                return false;

            // Make sure the shaders were compiled correctly.
            int compileStatusVS = 1;
            GL.GetShader(vertShaderId, ShaderParameter.CompileStatus, out compileStatusVS);

            int compileStatusFS = 1;
            GL.GetShader(fragShaderId, ShaderParameter.CompileStatus, out compileStatusFS);

            // Most shaders won't use a geometry shader.
            int compileStatusGS = 1;
            if (hasGeometryShader)
                GL.GetShader(geomShaderId, ShaderParameter.CompileStatus, out compileStatusGS);

            // The program was linked, but the shaders may have minor syntax errors.
            return (compileStatusFS != 0 && compileStatusVS != 0 && compileStatusGS != 0);
        }

        private void ShowCompileWarning(string shaderName, string shaderType)
        {
            string message = "The {0} {1} shader failed to compile."
                + " Please export a shader error log and upload it when reporting rendering issues.\n"
                + "The application will still function, but rendering for this shader will be disabled.";
            MessageBox.Show(String.Format(message, shaderName, shaderType), "Shader Compilation Error");
        }

        public void DisplayCompilationWarnings(string shaderName)
        {
            if (checkedCompilation)
                return;

            int compileStatusVS;
            GL.GetShader(vertShaderId, ShaderParameter.CompileStatus, out compileStatusVS);
            if (compileStatusVS == 0)
                ShowCompileWarning(shaderName, "vertex shader");

            int compileStatusFS;
            GL.GetShader(fragShaderId, ShaderParameter.CompileStatus, out compileStatusFS);
            if (compileStatusFS == 0)
                ShowCompileWarning(shaderName, "fragment shader");

            // Most shaders won't use a geometry shader.
            if (hasGeometryShader)
            {
                int compileStatusGS;
                GL.GetShader(geomShaderId, ShaderParameter.CompileStatus, out compileStatusGS);
                if (compileStatusGS == 0)
                    ShowCompileWarning(shaderName, "geometry shader");
            }

            checkedCompilation = true;      
        }
	}
}

