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
        public bool HasCheckedCompilation { get => checkedCompilation; }

        private StringBuilder errorLog = new StringBuilder();

		private Dictionary<string, int> attributes = new Dictionary<string, int>();

        public Shader ()
		{
			programID = GL.CreateProgram();

            errorLog.AppendLine("Program ID: " + programID);
            errorLog.AppendLine("MaxUniformBlockSize: " + GL.GetInteger(GetPName.MaxUniformBlockSize));
            errorLog.AppendLine("Vendor: " + GL.GetString(StringName.Vendor));
            errorLog.AppendLine("Renderer: " + GL.GetString(StringName.Renderer));
            errorLog.AppendLine("OpenGL Version: " + GL.GetString(StringName.Version));
            errorLog.AppendLine("GLSL Version: " + GL.GetString(StringName.ShadingLanguageVersion));
        }

		public int GetAttribute(string name)
        {
			int value;
			bool success = attributes.TryGetValue(name, out value);

            if (success)
                return value;
            else
                return -1;
		}

		public void EnableVertexAttributes()
        {
			foreach(KeyValuePair<string, int> entry in attributes)
			{
				GL.EnableVertexAttribArray(entry.Value);
			}
		}

		public void DisableVertexAttributes()
        {
			foreach(KeyValuePair<string, int> entry in attributes)
			{
				GL.DisableVertexAttribArray(entry.Value);
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

		private void AddAttribute(string name, bool isUniform)
        {
            if (attributes.ContainsKey(name))
                attributes.Remove(name);
			int position = -1;
			if(isUniform)
				position = GL.GetUniformLocation(programID, name);
			else
				position = GL.GetAttribLocation(programID, name);


            errorLog.AppendLine(name + ", " + "Position: " + position);

            attributes.Add(name, position);
		}

        public void LoadAttributes(string src, bool fragment = false)
        {
            int attributeCount;
            GL.GetProgram(programID, GetProgramParameterName.ActiveAttributes, out attributeCount);
            errorLog.AppendLine("Attribute Count: " + attributeCount);

            for (int i = 0; i < attributeCount; i++)
            {
                int attributeSize;
                ActiveAttribType attributeType;
                string attribute = GL.GetActiveAttrib(programID, i, out attributeSize, out attributeType);

                int index = attribute.IndexOf('[');
                if (index > 0)
                    attribute = attribute.Substring(0, index);

                AddAttribute(attribute, false);
            }

            int uniformCount;
            GL.GetProgram(programID, GetProgramParameterName.ActiveUniforms, out uniformCount);
            errorLog.AppendLine("Uniform Count: " + uniformCount);

            for (int i = 0; i < uniformCount; i++)
            {
                int uniformSize;
                ActiveUniformType uniformType;
                string uniform = GL.GetActiveUniform(programID, i, out uniformSize, out uniformType);

                int index = uniform.IndexOf('[');
                if (index > 0)
                    uniform = uniform.Substring(0, index);

                AddAttribute(uniform, true);
            }
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
            LoadAttributes(shaderText);

            errorLog.AppendLine(shaderType.ToString());
            string error = GL.GetProgramInfoLog(programID);
            errorLog.AppendLine(error);
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

