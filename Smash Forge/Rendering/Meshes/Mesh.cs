using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects;
using SFGraphics.Cameras;

namespace Smash_Forge.Rendering.Meshes
{
    abstract class Mesh<T> where T : struct
    {
        private int vertexSizeInBytes;

        private List<T> vertices = new List<T>();
        private BufferObject vertexBuffer = new BufferObject(BufferTarget.ArrayBuffer);

        private List<int> vertexIndices = new List<int>();
        private BufferObject vertexIndexBuffer = new BufferObject(BufferTarget.ElementArrayBuffer);

        public Mesh(List<T> vertices, int vertexSizeInBytes)
        {
            // The vertex data is immutable, so buffers only need to be initialized once.
            this.vertices = vertices;
            this.vertexSizeInBytes = vertexSizeInBytes;

            for (int i = 0; i < vertices.Count; i++)
            {
                vertexIndices.Add(i);
            }
            vertexIndexBuffer.BufferData(vertexIndices.ToArray(), sizeof(int), BufferUsageHint.StaticDraw);
            InitializeBufferData();
        }

        public Mesh(List<T> vertices, List<int> vertexIndices, int vertexSizeInBytes)
        {
            // The vertex data is immutable, so buffers only need to be initialized once.
            this.vertices = vertices;
            this.vertexIndices = vertexIndices;
            this.vertexSizeInBytes = vertexSizeInBytes;

            vertexIndexBuffer.BufferData(vertexIndices.ToArray(), sizeof(int), BufferUsageHint.StaticDraw);
            InitializeBufferData();
        }

        public void Draw(Shader shader, Camera camera, int count, int offset = 0)
        {
            if (!shader.ProgramCreatedSuccessfully)
                return;

            // Set up.
            shader.UseProgram();
            shader.EnableVertexAttributes();

            // Set shader values.
            SetCameraUniforms(shader, camera);

            SetUniforms(shader);

            vertexBuffer.Bind();
            SetVertexAttributes(shader);

            // TODO: How to handle count and offset?
            // NUD uses one buffer for multiple meshes.
            vertexIndexBuffer.Bind();
            GL.DrawElements(PrimitiveType.Triangles, count, DrawElementsType.UnsignedInt, offset);

            shader.DisableVertexAttributes();
        }

        protected virtual void SetCameraUniforms(Shader shader, Camera camera)
        {
            Matrix4 matrix = camera.MvpMatrix;
            shader.SetMatrix4x4("mvpMatrix", ref matrix);
        }

        protected virtual void SetUniforms(Shader shader)
        {
            shader.SetVector4("color", new Vector4(1));
            shader.SetVector3("scale", new Vector3(1));
            shader.SetVector3("center", new Vector3(0));
        }

        protected abstract List<VertexAttributeInfo> GetVertexAttributes();

        private void InitializeBufferData()
        {
            vertexBuffer.BufferData(vertices.ToArray(), vertexSizeInBytes, BufferUsageHint.StaticDraw);
        }

        private void SetVertexAttributes(Shader shader)
        {
            // Setting vertex attributes is handled automatically. 
            List<VertexAttributeInfo> vertexAttributes = GetVertexAttributes();
            foreach (VertexAttributeInfo attribute in vertexAttributes)
            {
                // -1 means not found, which is usually a result of the attribute being unused.
                int index = shader.GetVertexAttributeUniformLocation(attribute.name);
                if (index != -1)
                    GL.VertexAttribPointer(index, attribute.valueCount, attribute.vertexAttribPointerType, false, attribute.sizeInBytes, 0);
            }
        }
    }
}
