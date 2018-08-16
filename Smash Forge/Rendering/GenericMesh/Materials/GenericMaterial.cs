using System;
using System.Collections.Generic;
using OpenTK;
using SFGraphics.GLObjects.Shaders;

namespace Smash_Forge.Rendering.GenericMesh
{
    partial class GenericMaterial
    {
        // Scalar uniforms
        private Dictionary<string, float> floatUniformsByName = new Dictionary<string, float>();
        private Dictionary<string, int> intUniformsByName = new Dictionary<string, int>();

        // Vector uniforms
        private Dictionary<string, Vector2> vec2UniformsByName = new Dictionary<string, Vector2>();
        private Dictionary<string, Vector3> vec3UniformsByName = new Dictionary<string, Vector3>();
        private Dictionary<string, Vector4> vec4UniformsByName = new Dictionary<string, Vector4>();

        // Matrix uniforms.
        private Dictionary<string, Matrix4> mat4UniformsByName = new Dictionary<string, Matrix4>();

        // Texture uniforms
        // TODO: name, texture, texture unit

        public void AddFloat(string name, float value)
        {
            floatUniformsByName.Add(name, value);
        }

        public void AddInt(string name, int value)
        {
            intUniformsByName.Add(name, value);
        }

        public void AddVector2(string name, Vector2 value)
        {
            vec2UniformsByName.Add(name, value);
        }

        public void AddVector3(string name, Vector3 value)
        {
            vec3UniformsByName.Add(name, value);
        }

        public void AddVector4(string name, Vector4 value)
        {
            vec4UniformsByName.Add(name, value);
        }

        public void AddMatrix4(string name, Matrix4 value)
        {
            mat4UniformsByName.Add(name, value);
        }

        public void SetShaderUniforms(Shader shader)
        {
            foreach (var uniform in intUniformsByName)
            {
                shader.SetInt(uniform.Key, uniform.Value);
            }

            foreach (var uniform in floatUniformsByName)
            {
                shader.SetFloat(uniform.Key, uniform.Value);
            }

            foreach (var uniform in vec2UniformsByName)
            {
                shader.SetVector2(uniform.Key, uniform.Value);
            }

            foreach (var uniform in vec3UniformsByName)
            {
                shader.SetVector3(uniform.Key, uniform.Value);
            }

            foreach (var uniform in vec4UniformsByName)
            {
                shader.SetVector4(uniform.Key, uniform.Value);
            }

            foreach (var uniform in mat4UniformsByName)
            {
                Matrix4 value = uniform.Value;
                shader.SetMatrix4x4(uniform.Key, ref value);
            }
        }
    }
}
