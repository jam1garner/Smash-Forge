using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace Smash_Forge.Rendering.Meshes
{
    class Mesh3D : GenericMesh<Vector3>
    {
        public Mesh3D(List<Vector3> vertices) : base(vertices, Vector3.SizeInBytes)
        {

        }

        public Mesh3D(List<Vector3> vertices, List<int> indices) : base(vertices, indices, Vector3.SizeInBytes)
        {

        }

        protected override List<VertexAttributeInfo> GetVertexAttributes()
        {
            return new List<VertexAttributeInfo>()
            {
                new VertexAttributeInfo("position", 3, VertexAttribPointerType.Float, Vector3.SizeInBytes)
            };
        }
    }
}
