using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects;

namespace Smash_Forge.Rendering
{
    class Mesh3D
    {
        private List<Vector3> vertexPositions = new List<Vector3>();
        private BufferObject positionBuffer;

        public Mesh3D(Vector4 color, float scaleX = 1, float scaleY = 1, float scaleZ = 1, 
            float centerX = 0, float centerY = 0, float centerZ = 0)
        {
            UpdateShaderUniforms(color, scaleX, scaleY, scaleZ, centerX, centerY, centerZ);
        }

        private static void UpdateShaderUniforms(Vector4 color, float scaleX, float scaleY, float scaleZ, 
            float centerX, float centerY, float centerZ)
        {
            Shader shader = Runtime.shaders["SolidColor3D"];
            shader.UseProgram();

            shader.SetVector3("center", centerX, centerY, centerZ);
            shader.SetVector3("scale", scaleX, scaleY, scaleZ);
            shader.SetVector4("color", color);
        }

        public void AddVertex(Vector3 position)
        {
            vertexPositions.Add(position);
            UpdateBuffers();
        }

        public void Draw(Matrix4 mvpMatrix, float scale)
        {
            Shader shader = Runtime.shaders["SolidColor3D"];
            if (!shader.ProgramCreatedSuccessfully())
                return;

            // Set up.
            shader.UseProgram();
            shader.EnableVertexAttributes();
            positionBuffer.Bind();

            // Set shader values.
            Matrix4 matrix = mvpMatrix;
            shader.SetMatrix4x4("mvpMatrix", ref matrix);
            shader.SetFloat("scale", scale);

            // Draw.
            int rectangularPrismVertCount = 24;
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, rectangularPrismVertCount);

            shader.DisableVertexAttributes();
        }

        private void UpdateBuffers()
        {
            if (positionBuffer == null)
                positionBuffer = new BufferObject(BufferTarget.ArrayBuffer);

            positionBuffer.Bind();
            UpdateBufferData();

            Shader shader = Runtime.shaders["SolidColor3D"];
            SetVertexAttributes(shader);
        }

        private void UpdateBufferData()
        {
            Vector3[] vertexPositionsArray = vertexPositions.ToArray();
            GL.BufferData(positionBuffer.BufferTarget, (IntPtr)(sizeof(float) * 3 * vertexPositionsArray.Length),
                vertexPositionsArray, BufferUsageHint.StaticDraw);
        }

        private void SetVertexAttributes(Shader shader)
        {
            shader.UseProgram();
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("position"), 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
        }
    }
}
