using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects;

namespace Smash_Forge.Rendering
{
    static class ShapeDrawing
    {
        private static BufferObject rectangularPrismPositionBuffer;

        public static Vector3[] GetRectangularPrismPositions(float scaleX = 1, float scaleY = 1, float scaleZ = 1)
        {
            Vector3[] rectangularPrismPositions = new Vector3[]
            {
                new Vector3(+scaleX, +scaleY, -scaleZ),
                new Vector3(-scaleX, +scaleY, -scaleZ),
                new Vector3(-scaleX, +scaleY, +scaleZ),
                new Vector3(+scaleX, +scaleY, +scaleZ),

                new Vector3(+scaleX, -scaleY, +scaleZ),
                new Vector3(-scaleX, -scaleY, +scaleZ),
                new Vector3(-scaleX, -scaleY, -scaleZ),
                new Vector3(+scaleX, -scaleY, -scaleZ),

                new Vector3(+scaleX, +scaleY, +scaleZ),
                new Vector3(-scaleX, +scaleY, +scaleZ),
                new Vector3(-scaleX, -scaleY, +scaleZ),
                new Vector3(+scaleX, -scaleY, +scaleZ),

                new Vector3(+scaleX, -scaleY, -scaleZ),
                new Vector3(-scaleX, -scaleY, -scaleZ),
                new Vector3(-scaleX, +scaleY, -scaleZ),
                new Vector3(+scaleX, +scaleY, -scaleZ),

                new Vector3(-scaleX, +scaleY, +scaleZ),
                new Vector3(-scaleX, +scaleY, -scaleZ),
                new Vector3(-scaleX, -scaleY, -scaleZ),
                new Vector3(-scaleX, -scaleY, +scaleZ),

                new Vector3(+scaleX, +scaleY, -scaleZ),
                new Vector3(+scaleX, +scaleY, +scaleZ),
                new Vector3(+scaleX, -scaleY, +scaleZ),
                new Vector3(+scaleX, -scaleY, -scaleZ)
            };

            return rectangularPrismPositions;
        }

        public static void SetUp()
        {
            // Create the buffer.
            rectangularPrismPositionBuffer = new BufferObject(BufferTarget.ArrayBuffer);
            rectangularPrismPositionBuffer.Bind();

            Vector3[] rectangularPrismPositions = GetRectangularPrismPositions(1, 1, 1);
            GL.BufferData(rectangularPrismPositionBuffer.BufferTarget, (IntPtr)(sizeof(float) * 3 * rectangularPrismPositions.Length),
                rectangularPrismPositions, BufferUsageHint.StaticDraw);
        }

        public static void DrawCube(Matrix4 mvpMatrix, float scale = 1, float centerX = 0, float centerY = 0, float centerZ = 0)
        {
            DrawRectangularPrism(mvpMatrix, scale, scale, scale, centerX, centerY, centerZ);
        }

        public static void DrawRectangularPrism(Matrix4 mvpMatrix, float scaleX = 1, float scaleY = 1, float scaleZ = 1,
            float centerX = 0, float centerY = 0, float centerZ = 0)
        {
            Shader shader = Runtime.shaders["SolidColor3D"];
            if (!shader.ProgramCreatedSuccessfully())
                return;

            // Set up.
            shader.UseProgram();
            shader.EnableVertexAttributes();
            rectangularPrismPositionBuffer.Bind();

            // Set shader values.
            SetVertexAttributes(shader);
            SetShaderUniforms(mvpMatrix, scaleX, scaleY, scaleZ, centerX, centerY, centerZ, shader);

            // Draw.
            int rectangularPrismVertCount = 24;
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, rectangularPrismVertCount);

            shader.DisableVertexAttributes();
        }

        private static void SetVertexAttributes(Shader shader)
        {
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("position"), 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
        }

        private static void SetShaderUniforms(Matrix4 mvpMatrix, float scaleX, float scaleY, float scaleZ, float centerX, float centerY, float centerZ, Shader shader)
        {
            shader.SetVector3("center", centerX, centerY, centerZ);
            shader.SetVector3("scale", scaleX, scaleY, scaleZ);
            shader.SetVector4("color", new Vector4(1, 0, 1, 1));
            Matrix4 matrix = mvpMatrix;
            shader.SetMatrix4x4("mvpMatrix", ref matrix);
        }
    }
}
