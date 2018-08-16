using System;
using System.Collections.Generic;
using OpenTK;
using SFGraphics.GLObjects.Shaders;


namespace Smash_Forge.Rendering.Meshes
{
    class MeshColored3D : MeshSimple3D
    {
        // Basic material data.
        private Vector4 color = new Vector4(1);

        public MeshColored3D(List<Vector3> vertices, Vector4 color) : base(vertices)
        {
            this.color = color;
        }

        // Override uniform setters used in base Render() method.
        protected override void SetUniforms(Shader shader)
        {
            base.SetUniforms(shader);
            shader.SetVector4("color", color);
        }
    }
}
