using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects;
using SFGraphics.Cameras;

namespace Smash_Forge.Rendering.Meshes
{
    class Mesh<T> where T : struct
    {
        private List<T> vertices = new List<T>();
        private int vertexSizeInBytes;
        private BufferObject vertexBuffer = new BufferObject(BufferTarget.ArrayBuffer);


        public Mesh(List<T> vertices, int vertexSizeInBytes)
        {
            // The vertex data is immutable, so buffers only need to be initialized once.
            this.vertices = vertices;
            this.vertexSizeInBytes = vertexSizeInBytes;
            InitializeBufferData();
        }

        public void Draw(Shader shader, Camera camera)
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
            VertexAttributeInfo positionAttribute = new VertexAttributeInfo("position", 3,
                VertexAttribPointerType.Float, Vector3.SizeInBytes);

            SetVertexAttributes(shader, positionAttribute);

            GL.DrawArrays(PrimitiveType.TriangleFan, 0, vertices.Count);

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
