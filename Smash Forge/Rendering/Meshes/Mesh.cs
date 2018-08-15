using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects;
using SFGraphics.Cameras;

namespace Smash_Forge.Rendering.Meshes
{
    public abstract class Mesh<T> where T : struct
    {
        private readonly int vertexSizeInBytes;

        private readonly List<T> vertices = new List<T>();
        private BufferObject vertexBuffer = new BufferObject(BufferTarget.ArrayBuffer);

        private readonly List<int> vertexIndices = new List<int>();
        private BufferObject vertexIndexBuffer = new BufferObject(BufferTarget.ElementArrayBuffer);

        private VertexArrayObject vao = new VertexArrayObject();

        public Mesh(List<T> vertices, int vertexSizeInBytes)
        {
            // The vertex data is immutable, so buffers only need to be initialized once.
            this.vertices = vertices;
            this.vertexSizeInBytes = vertexSizeInBytes;

            // Generate basic indices.
            for (int i = 0; i < vertices.Count; i++)
            {
                vertexIndices.Add(i);
            }

            InitializeBufferData();
        }

        public Mesh(List<T> vertices, List<int> vertexIndices, int vertexSizeInBytes)
        {
            // The vertex data is immutable, so buffers only need to be initialized once.
            this.vertices = vertices;
            this.vertexIndices = vertexIndices;
            this.vertexSizeInBytes = vertexSizeInBytes;

            InitializeBufferData();
        }

        public void Draw(Shader shader, Camera camera, int count, int offset = 0)
        {
            if (!shader.ProgramCreatedSuccessfully)
                return;

            shader.UseProgram();

            // Set shader uniforms.
            //SetCameraUniforms(shader, camera);
            //SetUniforms(shader);

            // TODO: Only do this once.
            ConfigureVertexAttributes(shader);

            vao.Bind();
            GL.DrawElements(PrimitiveType.Triangles, count, DrawElementsType.UnsignedInt, offset);
            vao.Unbind();
        }

        private void ConfigureVertexAttributes(Shader shader)
        {
            vao.Bind();

            vertexBuffer.Bind();
            vertexIndexBuffer.Bind();

            shader.EnableVertexAttributes();
            SetVertexAttributes(shader);

            // Unbind all the buffers.
            // This step may not be necessary.
            vao.Unbind();
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        protected virtual void SetCameraUniforms(Shader shader, Camera camera)
        {
            // Not all shaders will use camera uniforms.
            if (camera == null)
                return;

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
            vertexIndexBuffer.BufferData(vertexIndices.ToArray(), sizeof(int), BufferUsageHint.StaticDraw);
        }

        private void SetVertexAttributes(Shader shader)
        {
            // Setting vertex attributes is handled automatically. 
            List<VertexAttributeInfo> vertexAttributes = GetVertexAttributes();
            int offset = 0;
            foreach (VertexAttributeInfo attribute in vertexAttributes)
            {
                // -1 means not found, which is usually a result of the attribute being unused.
                int index = shader.GetVertexAttributeUniformLocation(attribute.name);
                if (index != -1)
                    GL.VertexAttribPointer(index, attribute.valueCount, attribute.vertexAttribPointerType, false, vertexSizeInBytes, offset);
                // Move offset to next attribute.
                offset += attribute.sizeInBytes;
            }
        }
    }
}
