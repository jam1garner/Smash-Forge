using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects;
using SFGraphics.Cameras;

namespace Smash_Forge.Rendering.Meshes
{
    public abstract class GenericMesh<T> where T : struct
    {
        private readonly int vertexSizeInBytes;

        // The vertex data is immutable, so buffers only need to be initialized once.
        private BufferObject vertexBuffer = new BufferObject(BufferTarget.ArrayBuffer);
        private BufferObject vertexIndexBuffer = new BufferObject(BufferTarget.ElementArrayBuffer);
        private VertexArrayObject vertexArrayObject = new VertexArrayObject();

        // Use default settings.
        protected RenderSettings renderSettings = new RenderSettings();
        protected GenericMaterial material = new GenericMaterial();

        public GenericMesh(List<T> vertices, int vertexSizeInBytes)
        {
            this.vertexSizeInBytes = vertexSizeInBytes;

            // Generate a unique index for each vertex.
            List<int> vertexIndices = GenerateIndices(vertices);

            // Generate basic indices.
            InitializeBufferData(vertices, vertexIndices);
        }

        public GenericMesh(List<T> vertices, List<int> vertexIndices, int vertexSizeInBytes)
        {
            this.vertexSizeInBytes = vertexSizeInBytes;

            InitializeBufferData(vertices, vertexIndices);
        }

        private static List<int> GenerateIndices(List<T> vertices)
        {
            List<int> vertexIndices = new List<int>();
            for (int i = 0; i < vertices.Count; i++)
            {
                vertexIndices.Add(i);
            }

            return vertexIndices;
        }

        public void Draw(Shader shader, Camera camera, int count, int offset = 0)
        {
            if (!shader.ProgramCreatedSuccessfully)
                return;

            shader.UseProgram();

            // Set shader uniforms.
            SetCameraUniforms(shader, camera);
            SetUniforms(shader, material);

            // TODO: Only do this once.
            ConfigureVertexAttributes(shader);

            vertexArrayObject.Bind();
            GL.DrawElements(PrimitiveType.Triangles, count, DrawElementsType.UnsignedInt, offset);
            vertexArrayObject.Unbind();
        }

        protected abstract List<VertexAttributeInfo> GetVertexAttributes();

        protected virtual void SetCameraUniforms(Shader shader, Camera camera)
        {
            // Not all shaders will use camera uniforms.
            if (camera == null)
                return;

            Matrix4 matrix = camera.MvpMatrix;
            shader.SetMatrix4x4("mvpMatrix", ref matrix);
        }

        private void SetUniforms(Shader shader, GenericMaterial material)
        {
            material.SetShaderUniforms(shader);
        }

        private static void SetGLEnableCap(EnableCap enableCap, bool enabled)
        {
            if (enabled)
                GL.Enable(enableCap);
            else
                GL.Disable(enableCap);
        }

        private void SetFaceCulling(RenderSettings.FaceCullingSettings settings)
        {
            SetGLEnableCap(EnableCap.CullFace, settings.enableFaceCulling);

            GL.CullFace(settings.cullFaceMode);
        }

        private void SetDepthTesting(RenderSettings.DepthTestSettings settings)
        {
            SetGLEnableCap(EnableCap.DepthTest, settings.enableDepthTest);

            GL.DepthFunc(settings.depthFunction);
            GL.DepthMask(settings.depthMask);
        }

        private void SetAlphaBlending(RenderSettings.AlphaBlendSettings settings)
        {
            SetGLEnableCap(EnableCap.Blend, settings.enableAlphaBlending);

            GL.BlendFunc(settings.sourceFactor, settings.destinationFactor);
            GL.BlendEquationSeparate(settings.blendingEquationRgb, settings.blendingEquationAlpha);
        }

        private void InitializeBufferData(List<T> vertices, List<int> vertexIndices)
        {
            vertexBuffer.BufferData(vertices.ToArray(), vertexSizeInBytes, BufferUsageHint.StaticDraw);
            vertexIndexBuffer.BufferData(vertexIndices.ToArray(), sizeof(int), BufferUsageHint.StaticDraw);
        }

        private void ConfigureVertexAttributes(Shader shader)
        {
            vertexArrayObject.Bind();

            vertexBuffer.Bind();
            vertexIndexBuffer.Bind();

            shader.EnableVertexAttributes();
            SetVertexAttributes(shader);

            // Unbind all the buffers.
            // This step may not be necessary.
            vertexArrayObject.Unbind();
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
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
