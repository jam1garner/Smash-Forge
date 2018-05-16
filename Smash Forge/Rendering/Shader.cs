using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;

namespace Smash_Forge
{
	public class Shader
	{
		public int programID;
		private int vsID;
		private int fsID;

        private bool checkedCompilation = false;
        public bool HasCheckedCompilation { get { return checkedCompilation; } }

        private StringBuilder errorLog = new StringBuilder();

		private Dictionary<string, int> vertexAttributeLocations = new Dictionary<string, int>();
        private Dictionary<string, int> uniformLocations = new Dictionary<string, int>();

        public Shader ()
		{
			programID = GL.CreateProgram();

            errorLog.AppendLine("Vendor: " + GL.GetString(StringName.Vendor));
            errorLog.AppendLine("Renderer: " + GL.GetString(StringName.Renderer));
            errorLog.AppendLine("OpenGL Version: " + GL.GetString(StringName.Version));
            errorLog.AppendLine("GLSL Version: " + GL.GetString(StringName.ShadingLanguageVersion));
        }

		public int GetVertexAttributeLocation(string name)
        {
			int value;
            if (vertexAttributeLocations.TryGetValue(name, out value))
            {
                return value;
            }
            else
                return -1;
		}

        public int GetUniformLocation(string name)
        {
            int value;
            if (vertexAttributeLocations.TryGetValue(name, out value))
            {
                return value;
            }
            else
                return -1;
        }

        public void EnableVertexAttributes()
        {
			foreach(int location in vertexAttributeLocations.Values)
			{
                Debug.WriteLine(location);
                GL.EnableVertexAttribArray(location);
			}
		}

		public void DisableVertexAttributes()
        {
			foreach(int location in vertexAttributeLocations.Values)
			{
				GL.DisableVertexAttribArray(location);
			}
		}

        public void SaveErrorLog(string shaderName)
        {
            string logExport = errorLog.ToString();
            string errorLogDirectory = MainForm.executableDir + "\\Shader Error Logs\\";
            if (!Directory.Exists(errorLogDirectory))
                Directory.CreateDirectory(errorLogDirectory);
            File.WriteAllText(errorLogDirectory +  shaderName + " Error Log.txt", logExport.Replace("\n", Environment.NewLine));
        }

		private void AddVertexAttribute(string name)
        {
            if (vertexAttributeLocations.ContainsKey(name))
                vertexAttributeLocations.Remove(name);
			int position = GL.GetAttribLocation(programID, name);
            vertexAttributeLocations.Add(name, position);

            errorLog.AppendLine(name + ", " + "Position: " + position);
        }

        private void AddUniform(string name)
        {
            if (vertexAttributeLocations.ContainsKey(name))
                vertexAttributeLocations.Remove(name);
            int position = GL.GetUniformLocation(programID, name);
            vertexAttributeLocations.Add(name, position);

            errorLog.AppendLine(name + ", " + "Position: " + position);
        }

        private void LoadUniforms()
        {
            int uniformCount;
            GL.GetProgram(programID, GetProgramParameterName.ActiveUniforms, out uniformCount);
            errorLog.AppendLine("Uniform Count: " + uniformCount);

            for (int i = 0; i < uniformCount; i++)
            {
                int uniformSize;
                ActiveUniformType uniformType;
                string uniform = GL.GetActiveUniform(programID, i, out uniformSize, out uniformType);
                uniform = RemoveEndingBrackets(uniform);
                AddUniform(uniform);
            }
        }

        private void LoadAttributes()
        {
            int attributeCount;
            GL.GetProgram(programID, GetProgramParameterName.ActiveAttributes, out attributeCount);
            errorLog.AppendLine("Attribute Count: " + attributeCount);

            for (int i = 0; i < attributeCount; i++)
            {
                int attributeSize;
                ActiveAttribType attributeType;
                string attribute = GL.GetActiveAttrib(programID, i, out attributeSize, out attributeType);
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

        public void LoadShader(string filePath, ShaderType shaderType)
        {
            string shaderText = File.ReadAllText(filePath);
            if (shaderType == ShaderType.FragmentShader)
                LoadShader(shaderText, shaderType, programID, out fsID);
            else if (shaderType == ShaderType.VertexShader)
                LoadShader(shaderText, shaderType, programID, out vsID);
            else
                throw new NotSupportedException(shaderType.ToString() + " is not a supported shader type.");

            GL.LinkProgram(programID);

            // Append shader compilation errors.
            errorLog.AppendLine(shaderType.ToString());
            errorLog.AppendLine("Compilation Errors:");
            string error = GL.GetProgramInfoLog(programID);
            errorLog.AppendLine(error);

            LoadAttributes();
            LoadUniforms();
        }

        void LoadShader(string shaderText, ShaderType type, int program, out int address)
		{
			address = GL.CreateShader(type);

            if (shaderText.Contains("#include"))
            {
                // Hard coded #include for reducing redundant shader code. 
                string smashShaderText = File.ReadAllText(MainForm.executableDir + "\\lib\\shader\\SMASH_SHADER.txt");
                shaderText = shaderText.Replace("#include SMASH_SHADER", smashShaderText);

                string nuUniformText = File.ReadAllText(MainForm.executableDir + "\\lib\\shader\\NU_UNIFORMS.txt");
                shaderText = shaderText.Replace("#include NU_UNIFORMS", nuUniformText);

                string stageUniformText = File.ReadAllText(MainForm.executableDir + "\\lib\\shader\\STAGE_LIGHTING_UNIFORMS.txt");
                shaderText = shaderText.Replace("#include STAGE_LIGHTING_UNIFORMS", stageUniformText);

                string miscUniformsText = File.ReadAllText(MainForm.executableDir + "\\lib\\shader\\MISC_UNIFORMS.txt");
                shaderText = shaderText.Replace("#include MISC_UNIFORMS", miscUniformsText);
            }

            GL.ShaderSource(address, shaderText);
			GL.CompileShader(address);
            GL.AttachShader(program, address);
			Console.WriteLine(GL.GetShaderInfoLog(address));
            errorLog.AppendLine(GL.GetShaderInfoLog(address));
        }

        public bool CompiledSuccessfully()
        {
            // Make sure the shaders were compiled correctly.
            // Don't try to render if the shaders have errors to avoid crashes.
            int compileStatusVS;
            GL.GetShader(vsID, ShaderParameter.CompileStatus, out compileStatusVS);

            int compileStatusFS;
            GL.GetShader(fsID, ShaderParameter.CompileStatus, out compileStatusFS);
            if (compileStatusFS == 0 || compileStatusVS == 0)
                return false;
            else
                return true;
        }

        public void DisplayCompilationWarning(string shaderName)
        {
            if (checkedCompilation)
                return;

            int compileStatusVS;
            GL.GetShader(vsID, ShaderParameter.CompileStatus, out compileStatusVS);
            if (compileStatusVS == 0)
            {
                MessageBox.Show("The " + shaderName
                    + " vertex shader failed to compile. Check that your system supports OpenGL 3.30. Enable legacy shading in the config for OpenGL 2.10." +
                    " Please export a shader error log and " +
                    "upload it when reporting rendering issues.", "Shader Compilation Error");
            }

            int compileStatusFS;
            GL.GetShader(fsID, ShaderParameter.CompileStatus, out compileStatusFS);
            if (compileStatusFS == 0)
            {
                MessageBox.Show("The " + shaderName
                    + " fragment shader failed to compile. Check that your system supports OpenGL 3.30. Enable legacy shading in the config for OpenGL 2.10." +
                    " Please export a shader error log and " +
                    "upload it when reporting rendering issues.", "Shader Compilation Error");
            }

            checkedCompilation = true;      
        }
	}
}

