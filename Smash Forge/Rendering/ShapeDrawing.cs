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
        public static void DrawCubeShader(Matrix4 mvpMatrix)
        {
            Shader shader = Runtime.shaders["SolidColor3D"];
            if (!shader.ProgramCreatedSuccessfully())
                return;

            shader.UseProgram();

            shader.EnableVertexAttributes();

            // Create the buffer.
            BufferObject bufferObject = new BufferObject(BufferTarget.ArrayBuffer);
            bufferObject.Bind();

            float sizeX = 1;
            float sizeY = 1;
            float sizeZ = 1;

            Vector3[] vertices = new Vector3[]
            {
                new Vector3(+sizeX, +sizeY, -sizeZ),
                new Vector3(-sizeX, +sizeY, -sizeZ),
                new Vector3(-sizeX, +sizeY, +sizeZ),
                new Vector3(+sizeX, +sizeY, +sizeZ),

                new Vector3(+sizeX, -sizeY, +sizeZ),
                new Vector3(-sizeX, -sizeY, +sizeZ),
                new Vector3(-sizeX, -sizeY, -sizeZ),
                new Vector3(+sizeX, -sizeY, -sizeZ),

                new Vector3(+sizeX, +sizeY, +sizeZ),
                new Vector3(-sizeX, +sizeY, +sizeZ),
                new Vector3(-sizeX, -sizeY, +sizeZ),
                new Vector3(+sizeX, -sizeY, +sizeZ),

                new Vector3(+sizeX, -sizeY, -sizeZ),
                new Vector3(-sizeX, -sizeY, -sizeZ),
                new Vector3(-sizeX, +sizeY, -sizeZ),
                new Vector3(+sizeX, +sizeY, -sizeZ),

                new Vector3(-sizeX, +sizeY, +sizeZ),
                new Vector3(-sizeX, +sizeY, -sizeZ),
                new Vector3(-sizeX, -sizeY, -sizeZ),
                new Vector3(-sizeX, -sizeY, +sizeZ),

                new Vector3(+sizeX, +sizeY, -sizeZ),
                new Vector3(+sizeX, +sizeY, +sizeZ),
                new Vector3(+sizeX, -sizeY, +sizeZ),
                new Vector3(+sizeX, -sizeY, -sizeZ)
            };

            GL.BufferData(bufferObject.BufferTarget, (IntPtr)(sizeof(float) * 3 * vertices.Length),
                vertices, BufferUsageHint.StaticDraw);

            // Set everytime because multiple shaders use this for drawing.
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("position"), 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);

            // Shader uniforms.
            shader.SetVector3("center", new Vector3(0));
            shader.SetFloat("scale", 1);
            shader.SetVector4("color", new Vector4(1, 0, 0, 1));
            Matrix4 matrix = mvpMatrix;
            shader.SetMatrix4x4("mvpMatrix", ref matrix);

            // Draw.
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, vertices.Length);
            shader.DisableVertexAttributes();
        }
    }
}
