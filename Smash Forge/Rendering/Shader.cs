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
        private StringBuilder errorLog = new StringBuilder();

        private string vertFilePath = "";
        private string fragFilePath = "";

		Dictionary<string, int> attributes = new Dictionary<string, int>();

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

        public string getVertPath()
        {
            return vertFilePath;
        }

        public string getFragPath()
        {
            return fragFilePath;
        }

        public bool hasCheckedCompilation()
        {
            return checkedCompilation;
        }

		public int getAttribute(string s){
			int v;
			bool success = attributes.TryGetValue (s, out v);

            if (success)
                return v;
            else
                return -1;
		}

		public void enableAttrib(){
			foreach(KeyValuePair<string, int> entry in attributes)
			{
				GL.EnableVertexAttribArray (entry.Value);
			}
		}

		public void disableAttrib(){
			foreach(KeyValuePair<string, int> entry in attributes)
			{
				GL.DisableVertexAttribArray (entry.Value);
			}
		}

        public void SaveErrorLog(string shaderName)
        {
            string logExport = errorLog.ToString();
            File.WriteAllText(shaderName + "_ErrorLog.txt", logExport.Replace("\n", Environment.NewLine));
        }

		private void addAttribute(string name, bool uniform){
            if (attributes.ContainsKey(name)) attributes.Remove(name);
			int position = -1;
			if(uniform)
				position = GL.GetUniformLocation(programID, name);
			else
				position = GL.GetAttribLocation(programID, name);


            errorLog.AppendLine(name + ", " + "Position: " + position);

            attributes.Add (name, position);
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

                addAttribute(attribute, false);
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

                addAttribute(uniform, true);
            }
        }

        public void vertexShader(string filePath){
            string shaderText = File.ReadAllText(filePath);
            vertFilePath = filePath;
            loadShader(shaderText, ShaderType.VertexShader, programID, out vsID);
			GL.LinkProgram (programID);
            LoadAttributes(shaderText);

            errorLog.AppendLine("Vertex Shader");
            string error = GL.GetProgramInfoLog(programID);
            errorLog.AppendLine(error);
        }

		public void fragmentShader(string filePath){
            string shaderText = File.ReadAllText(filePath);
            fragFilePath = filePath;
			loadShader(shaderText, ShaderType.FragmentShader, programID, out fsID);
			GL.LinkProgram (programID);
            LoadAttributes(shaderText, true);

            errorLog.AppendLine("Fragment Shader");
            string error = GL.GetProgramInfoLog(programID);
            errorLog.AppendLine(error);
        }

        void loadShader(string shaderText, ShaderType type, int program, out int address)
		{
			address = GL.CreateShader(type);
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

