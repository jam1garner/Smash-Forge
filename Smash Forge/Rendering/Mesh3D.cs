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
        private BufferObject positionBuffer = new BufferObject(BufferTarget.ArrayBuffer);

        public float ScaleX
        {
            get { return scaleX; }
            set
            {
                scaleX = value;
                SetScaleUniform();
            }
        }
        private float scaleX = 1;

        public float ScaleY
        {
            get { return scaleY; }
            set
            {
                scaleY = value;
                SetScaleUniform();
            }
        }
        private float scaleY = 1;

        public float ScaleZ
        {
            get { return scaleZ; }
            set
            {
                scaleZ = value;
                SetScaleUniform();
            }
        }
        private float scaleZ = 1;

        public Vector4 Color
        {
            get { return Color; }
            set
            {
                color = value;
                SetColorUniform();
            }
        }
        private Vector4 color = new Vector4(1);

        public Mesh3D(Vector4 color, float scaleX = 1, float scaleY = 1, float scaleZ = 1, 
            float centerX = 0, float centerY = 0, float centerZ = 0)
        {
            ScaleX = scaleX;
            ScaleY = scaleY;
            ScaleZ = scaleZ;
            Color = color;
            SetCenterUniform(centerX, centerY, centerZ);
        }

        private void SetColorUniform()
        {
            Shader shader = OpenTKSharedResources.shaders["SolidColor3D"];
            shader.UseProgram();
            shader.SetVector4("color", color);
        }

        private void SetScaleUniform()
        {
            Shader shader = OpenTKSharedResources.shaders["SolidColor3D"];
            shader.UseProgram();
            shader.SetVector3("scale", scaleX, scaleY, scaleZ);
        }

        private void SetCenterUniform(float centerX, float centerY, float centerZ)
        {
            Shader shader = OpenTKSharedResources.shaders["SolidColor3D"];
            shader.UseProgram();
            shader.SetVector3("center", centerX, centerY, centerZ);
        }

        public void AddVertex(Vector3 position)
        {
            vertexPositions.Add(position);
            UpdateBuffers();
        }

        public void Draw(Matrix4 mvpMatrix)
        {
            Shader shader = OpenTKSharedResources.shaders["SolidColor3D"];
            if (!shader.ProgramCreatedSuccessfully())
                return;

            // Set up.
            shader.UseProgram();
            shader.EnableVertexAttributes();
            positionBuffer.Bind();

            // Set shader values.
            Matrix4 matrix = mvpMatrix;
            shader.SetMatrix4x4("mvpMatrix", ref matrix);

            // Draw.
            int rectangularPrismVertCount = 24;
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, rectangularPrismVertCount);

            shader.DisableVertexAttributes();
        }

        private void UpdateBuffers()
        {
            positionBuffer.Bind();
            UpdateBufferData();

            Shader shader = OpenTKSharedResources.shaders["SolidColor3D"];
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
