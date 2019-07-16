using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGenericModel;
using SFGenericModel.VertexAttributes;

namespace SmashForge.Rendering.Meshes
{
    class Mesh3D : GenericMesh<Vector3>
    {
        public Mesh3D(List<Vector3> vertices) : base(vertices, PrimitiveType.Triangles)
        {

        }

        public Mesh3D(List<Vector3> vertices, List<int> indices) : base(vertices, indices, PrimitiveType.Triangles)
        {

        }

        public override List<VertexAttribute> GetVertexAttributes()
        {
            return new List<VertexAttribute>()
            {
                new VertexFloatAttribute("position", ValueCount.Three, VertexAttribPointerType.Float, false)
            };
        }
    }
}
