using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects;

namespace Smash_Forge.Rendering
{
    class Mesh3d
    {
        private List<Vector3> vertexPositions = new List<Vector3>();
        private BufferObject positionBuffer = new BufferObject(BufferTarget.ArrayBuffer);

        // Shader uniform values.
        public Vector3 center = new Vector3(0);
        public Vector3 scale = new Vector3(1);
        public Vector4 color = new Vector4(1);

        public Mesh3d(List<Vector3> vertexPositions, Vector4 color)
        {
            // The vertex data is immutable, buffers only need to be initialized once.
            this.vertexPositions = vertexPositions;
            UpdateBufferData();

            this.color = color;
        }

        public Mesh3d(List<Vector3> vertexPositions, Vector4 color, Vector3 scale, Vector3 center) 
            : this(vertexPositions, color)
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

            positionBuffer.Bind();
            SetVertexAttributes(shader);

            GL.DrawArrays(PrimitiveType.TriangleFan, 0, vertexPositions.Count);

            shader.DisableVertexAttributes();
        }

        private void UpdateBufferData()
        {
            Vector3[] vertexPositionsArray = vertexPositions.ToArray();
            positionBuffer.Bind();
            GL.BufferData(positionBuffer.BufferTarget, (IntPtr)(sizeof(float) * 3 * vertexPositionsArray.Length),
                vertexPositionsArray, BufferUsageHint.StaticDraw);
        }

        private void SetVertexAttributes(Shader shader)
        {
            int valueCount = 3; // Vector3
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("position"), valueCount, 
                VertexAttribPointerType.Float, false, sizeof(float) * valueCount, 0);
        }
    }
}
