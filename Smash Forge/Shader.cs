using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;

namespace Smash_Forge
{
	public class Shader
	{
		public int programID;
		int vsID;
		int fsID;

        private string errorLog = "";

		Dictionary<string, int> attributes = new Dictionary<string, int>();

		public Shader ()
		{
			programID = GL.CreateProgram();
            errorLog += "Program ID: "+ programID + "\n";
            errorLog += "MaxUniformBlockSize: " + GL.GetInteger(GetPName.MaxUniformBlockSize) + "\n";
            errorLog += "Vendor: " + GL.GetString(StringName.Vendor) + "\n";
            errorLog += "Renderer: " + GL.GetString(StringName.Renderer) + "\n";
            errorLog += "OpenGL Version: " + GL.GetString(StringName.Version) + "\n";
            errorLog += "GLSL Version: " + GL.GetString(StringName.ShadingLanguageVersion) + "\n";
        }

        /*~Shader()
        {
            GL.DeleteProgram(programID);
        }*/

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
            File.WriteAllText(shaderName + "_ErrorLog.txt", errorLog.Replace("\n", Environment.NewLine));
        }

		private void addAttribute(string name, bool uniform){
            if (attributes.ContainsKey(name)) attributes.Remove(name);
			int position = -1;
			if(uniform)
				position = GL.GetUniformLocation(programID, name);
			else
				position = GL.GetAttribLocation(programID, name);


            errorLog += name + ", " + "Position: " + position + "\n";

            attributes.Add (name, position);
		}
        
        public void LoadAttributes(string src, bool fragment = false)
        {
            int attributeCount;
            GL.GetProgram(programID, GetProgramParameterName.ActiveAttributes, out attributeCount);
            errorLog += "Attribute Count: " + attributeCount + "\n";

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
            errorLog += "\n" + "Uniform Count: " + uniformCount + "\n";

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

        public void vertexShader(string shaderText){
            //MessageBox.Show("GL major: " + GL.GetInteger(GetPName.MajorVersion) );
            //MessageBox.Show("GL minor: " + GL.GetInteger(GetPName.MinorVersion));
            loadShader(shaderText, ShaderType.VertexShader, programID, out vsID);
			GL.LinkProgram (programID);
            errorLog += "Vertex Shader" + "\n";
            LoadAttributes(shaderText);
            string error = GL.GetProgramInfoLog(programID);
            errorLog += error + "\n";
            Console.WriteLine(error);
        }

		public void fragmentShader(string shaderText){
			loadShader(shaderText, ShaderType.FragmentShader, programID, out fsID);
			GL.LinkProgram (programID);
            errorLog += "Fragment Shader" + "\n";
            LoadAttributes(shaderText, true);
            string error = GL.GetProgramInfoLog(programID);
            errorLog += error + "\n";
            Console.WriteLine(error);
        }

        void loadShader(string shaderText, ShaderType type, int program, out int address)
		{
			address = GL.CreateShader(type);
			//using (StreamReader sr = new StreamReader(filename))
			//{
				GL.ShaderSource(address, shaderText);
			//}
			GL.CompileShader(address);
            GL.AttachShader(program, address);
            //File.WriteAllText("log.txt", GL.GetShaderInfoLog(address).ToLower() + "Shader Log");
            //MessageBox.Show(GL.GetShaderInfoLog(address));
			Console.WriteLine(GL.GetShaderInfoLog(address));
            errorLog += GL.GetShaderInfoLog(address) + "\n";
        }
	}
}

