using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects;

namespace Smash_Forge.Rendering
{
    class Mesh<T> where T : struct
    {
        private List<T> vertices = new List<T>();
        private int vertexSizeInBytes;
        private BufferObject vertexBuffer = new BufferObject(BufferTarget.ArrayBuffer);

        // Shader uniform values.
        public Vector3 center = new Vector3(0);
        public Vector3 scale = new Vector3(1);
        public Vector4 color = new Vector4(1);

        public Mesh(List<T> vertices, int vertexSizeInBytes)
        {
            // The vertex data is immutable, so buffers only need to be initialized once.
            this.vertices = vertices;
            this.vertexSizeInBytes = vertexSizeInBytes;
            InitializeBufferData();
        }

        public Mesh(List<T> vertices, int vertexSizeInBytes, Vector4 color, Vector3 scale, Vector3 center) 
            : this(vertices, vertexSizeInBytes)
        {
            this.scale = scale;
            this.center = center;
        }

        public void Draw(Shader shader, Matrix4 mvpMatrix)
        {
            if (!shader.ProgramCreatedSuccessfully)
                return;

            // Set up.
            shader.UseProgram();
            shader.EnableVertexAttributes();

            // Set shader values.
            Matrix4 matrix = mvpMatrix;
            shader.SetMatrix4x4("mvpMatrix", ref matrix);

            shader.SetVector4("color", color);
            shader.SetVector3("scale", scale);
            shader.SetVector3("center", center);

            vertexBuffer.Bind();
            VertexAttributeInfo positionAttribute = new VertexAttributeInfo("position", 3, 
                VertexAttribPointerType.Float, Vector3.SizeInBytes);

            SetVertexAttributes(shader, positionAttribute);

            GL.DrawArrays(PrimitiveType.TriangleFan, 0, vertices.Count);

            shader.DisableVertexAttributes();
        }

        private void InitializeBufferData()
        {
            vertexBuffer.BufferData(vertices.ToArray(), Vector3.SizeInBytes, BufferUsageHint.StaticDraw);
        }

        private void SetVertexAttributes(Shader shader, params VertexAttributeInfo[] vertexAttributes)
        {
            foreach (VertexAttributeInfo attribute in vertexAttributes)
            {
                GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation(attribute.name), 
                    attribute.valueCount, attribute.vertexAttribPointerType, false, attribute.sizeInBytes, 0);
            }
        }
    }
}
