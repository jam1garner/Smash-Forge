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

        private string errorlog = "";

		Dictionary<string, int> attributes = new Dictionary<string, int>();

		public Shader ()
		{
			programID = GL.CreateProgram();
            errorlog += programID + "\n";
            errorlog += GL.GetInteger(GetPName.MajorVersion) + "\n";
            errorlog += GL.GetInteger(GetPName.MinorVersion) + "\n";
            errorlog += GL.GetInteger(GetPName.MaxUniformBlockSize) + "\n";

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

        public void SaveErrorLog()
        {
            File.WriteAllText("shad_ErrorLog.txt", errorlog.Replace("\n", Environment.NewLine));
        }

		private void addAttribute(string name, bool uniform){
            if (attributes.ContainsKey(name)) attributes.Remove(name);
			int pos = -1;
			if(uniform)
				pos = GL.GetUniformLocation(programID, name);
			else
				pos = GL.GetAttribLocation(programID, name);


            errorlog += name + " " + uniform + " " + pos + "\n";

            attributes.Add (name, pos);
		}
        
        public void LoadAttributes(string src, bool fragment = false)
        {
            int attributeCount;
            GL.GetProgram(programID, GetProgramParameterName.ActiveAttributes, out attributeCount);
            errorlog += "Att:" + attributeCount + "\n";

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
            errorlog += "Uni:" + uniformCount + "\n";

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

        public void vertexShader(string filename){
            //MessageBox.Show("GL major: " + GL.GetInteger(GetPName.MajorVersion) );
            //MessageBox.Show("GL minor: " + GL.GetInteger(GetPName.MinorVersion));
            loadShader(filename, ShaderType.VertexShader, programID, out vsID);
			GL.LinkProgram (programID);
            LoadAttributes(filename);
            string error = GL.GetProgramInfoLog(programID);
            errorlog += error + "\n";
            Console.WriteLine(error);
        }

		public void fragmentShader(string filename){
			loadShader(filename, ShaderType.FragmentShader, programID, out fsID);
			GL.LinkProgram (programID);
            LoadAttributes(filename, true);
            string error = GL.GetProgramInfoLog(programID);
            errorlog += error + "\n";
            Console.WriteLine(error);

            SaveErrorLog();
        }

        void loadShader(string shader, ShaderType type, int program, out int address)
		{
			address = GL.CreateShader(type);
			//using (StreamReader sr = new StreamReader(filename))
			//{
				GL.ShaderSource(address, shader);
			//}
			GL.CompileShader(address);
            GL.AttachShader(program, address);
            //File.WriteAllText("log.txt", GL.GetShaderInfoLog(address).ToLower() + "Shader Log");
            //MessageBox.Show(GL.GetShaderInfoLog(address));
			Console.WriteLine(GL.GetShaderInfoLog(address));
            errorlog += GL.GetShaderInfoLog(address) + "\n";
        }
	}
}

