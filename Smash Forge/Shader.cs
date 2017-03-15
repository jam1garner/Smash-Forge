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

		Dictionary<string, int> attributes = new Dictionary<string, int>();

		public Shader ()
		{
			programID = GL.CreateProgram();
		}

		public int getAttribute(string s){
			int v;
			attributes.TryGetValue (s, out v);

			return v;
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

		public void addAttribute(string name, bool uniform){
			int pos;
			if(uniform)
				pos = GL.GetUniformLocation(programID, name);
			else
				pos = GL.GetAttribLocation(programID, name);
			attributes.Add (name, pos);
		}

		public void vertexShader(string filename){
            //MessageBox.Show("GL major: " + GL.GetInteger(GetPName.MajorVersion) );
            //MessageBox.Show("GL minor: " + GL.GetInteger(GetPName.MinorVersion));
            loadShader(filename, ShaderType.VertexShader, programID, out vsID);
			GL.LinkProgram (programID);
            Console.WriteLine(GL.GetProgramInfoLog(programID));
        }

		public void fragmentShader(string filename){
			loadShader(filename, ShaderType.FragmentShader, programID, out fsID);
			GL.LinkProgram (programID);
            Console.WriteLine(GL.GetProgramInfoLog(programID));
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
		}
	}
}

