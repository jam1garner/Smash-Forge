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

        private ShaderLog errorLog = new ShaderLog();

        // Vertex Attributes and Uniforms
        int activeUniformCount = 0;
        int activeAttributeCount = 0;
		private Dictionary<string, int> vertexAttributeAndUniformLocations = new Dictionary<string, int>();

        // Write these names to the error log later rather than throwing an exception.
        private HashSet<string> invalidUniformNames = new HashSet<string>();

        public Shader()
        {
            programId = GL.CreateProgram();
            errorLog.AppendHardwareAndVersionInfo();
        }

        // Shader Uniforms. Keep track of undeclared variables, so they can be fixed later.
        public void SetFloat(string uniformName, float value)
        {
            if (!vertexAttributeAndUniformLocations.ContainsKey(uniformName) && !invalidUniformNames.Contains(uniformName))
            {
                invalidUniformNames.Add(uniformName);
                return;
            }

            GL.Uniform1(GetVertexAttributeUniformLocation(uniformName), value);
        }

        public void SetInt(string uniformName, int value)
        {
            if (!vertexAttributeAndUniformLocations.ContainsKey(uniformName) && !invalidUniformNames.Contains(uniformName))
            {
                invalidUniformNames.Add(uniformName);
                return;
            }

            GL.Uniform1(GetVertexAttributeUniformLocation(uniformName), value);
        }

        public void SetUint(string uniformName, uint value)
        {
            if (!vertexAttributeAndUniformLocations.ContainsKey(uniformName) && !invalidUniformNames.Contains(uniformName))
            {
                invalidUniformNames.Add(uniformName);
                return;
            }

            GL.Uniform1(GetVertexAttributeUniformLocation(uniformName), value);
        }

        public void SetBoolToInt(string uniformName, bool value)
        {
            if (!vertexAttributeAndUniformLocations.ContainsKey(uniformName) && !invalidUniformNames.Contains(uniformName))
            {
                invalidUniformNames.Add(uniformName);
                return;
            }

            // if/else is faster than the ternary operator. 
            if (value)
                GL.Uniform1(GetVertexAttributeUniformLocation(uniformName), 1);
            else
                GL.Uniform1(GetVertexAttributeUniformLocation(uniformName), 0);
        }

        public void SetVector3(string uniformName, Vector3 value)
        {
            if (!vertexAttributeAndUniformLocations.ContainsKey(uniformName) && !invalidUniformNames.Contains(uniformName))
            {
                invalidUniformNames.Add(uniformName);
                return;
            }

            GL.Uniform3(GetVertexAttributeUniformLocation(uniformName), value);
        }

        public void SetVector3(string uniformName, float x, float y, float z)
        {
            if (!vertexAttributeAndUniformLocations.ContainsKey(uniformName) && !invalidUniformNames.Contains(uniformName))
            {
                invalidUniformNames.Add(uniformName);
                return;
            }

            GL.Uniform3(GetVertexAttributeUniformLocation(uniformName), x, y, z);
        }

        public void SetVector4(string uniformName, Vector4 value)
        {
            if (!vertexAttributeAndUniformLocations.ContainsKey(uniformName) && !invalidUniformNames.Contains(uniformName))
            {
                invalidUniformNames.Add(uniformName);
                return;
            }

            GL.Uniform4(GetVertexAttributeUniformLocation(uniformName), value);
        }

        public void SetVector4(string uniformName, float x, float y, float z, float w)
        {
            if (!vertexAttributeAndUniformLocations.ContainsKey(uniformName) && !invalidUniformNames.Contains(uniformName))
            {
                invalidUniformNames.Add(uniformName);
                return;
            }

            GL.Uniform4(GetVertexAttributeUniformLocation(uniformName), x, y, z, w);
        }

        public void SetMatrix4x4(string uniformName, ref Matrix4 value)
        {
            if (!vertexAttributeAndUniformLocations.ContainsKey(uniformName) && !invalidUniformNames.Contains(uniformName))
            {
                invalidUniformNames.Add(uniformName);
                return;
            }

            GL.UniformMatrix4(GetVertexAttributeUniformLocation(uniformName), false, ref value);
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
            // Don't append program errors until all the shaders are attached and compiled.
            errorLog.AppendProgramInfoLog(programId);
            // Collect all of the spelling mistakes.
            errorLog.AppendUniformNameErrors(invalidUniformNames);

            errorLog.SaveToErrorLogDir(shaderName);
        }

		private void AddVertexAttribute(string name)
        {
            if (vertexAttributeAndUniformLocations.ContainsKey(name))
                vertexAttributeAndUniformLocations.Remove(name);
			int position = GL.GetAttribLocation(programId, name);
            vertexAttributeAndUniformLocations.Add(name, position);

            //errorLog.AppendLine(name + ", " + "Position: " + position);
        }

        private void AddUniform(string name)
        {
            if (vertexAttributeAndUniformLocations.ContainsKey(name))
                vertexAttributeAndUniformLocations.Remove(name);
            int position = GL.GetUniformLocation(programId, name);
            vertexAttributeAndUniformLocations.Add(name, position);

            //errorLog.AppendLine(name + ", " + "Position: " + position);
        }

        private void LoadUniforms()
        {
            GL.GetProgram(programId, GetProgramParameterName.ActiveUniforms, out activeUniformCount);
            //errorLog.AppendLine("Uniform Count: " + activeUniformCount);

            for (int i = 0; i < activeUniformCount; i++)
            {
                ActiveUniformType uniformType;
                int uniformSize;
                string uniform = GL.GetActiveUniform(programId, i, out uniformSize, out uniformType);
                uniform = RemoveEndingBrackets(uniform);
                AddUniform(uniform);
            }
        }

        private void LoadAttributes()
        {
            GL.GetProgram(programId, GetProgramParameterName.ActiveAttributes, out activeAttributeCount);
            //errorLog.AppendLine("Attribute Count: " + activeAttributeCount);

            for (int i = 0; i < activeAttributeCount; i++)
            {
                ActiveAttribType attributeType;
                int attributeSize;
                string attribute = GL.GetActiveAttrib(programId, i, out attributeSize, out attributeType);
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

            LoadAttributes();
            LoadUniforms();
        }

        private void LoadShaderBasedOnType(string filePath)
        {
            if (filePath.EndsWith(".frag"))
            {
                AttachAndCompileShader(filePath, ShaderType.FragmentShader, programId, out fragShaderId);
            }
            else if (filePath.EndsWith(".vert"))
            {
                AttachAndCompileShader(filePath, ShaderType.VertexShader, programId, out vertShaderId);
            }
            else if (filePath.EndsWith(".geom"))
            {
                AttachAndCompileShader(filePath, ShaderType.GeometryShader, programId, out geomShaderId);
                hasGeometryShader = true;
            }
            else
            {
                throw new NotSupportedException(filePath + " does not have a suppported shader type extension.");
            }
        }

        private void AttachAndCompileShader(string shaderFile, ShaderType type, int program, out int id)
        {
            string shaderText = File.ReadAllText(shaderFile);
            id = GL.CreateShader(type);

            GL.ShaderSource(id, shaderText);
            GL.CompileShader(id);
            GL.AttachShader(program, id);

            // Get the name of the shader. 
            string[] parts = shaderFile.Split('\\');
            string shaderName = parts[parts.Length - 1];
            errorLog.AppendShaderInfoLog(shaderName, id, type);
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

