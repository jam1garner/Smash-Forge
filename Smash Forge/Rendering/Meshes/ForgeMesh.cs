using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace Smash_Forge.Rendering.Meshes
{
    public class ForgeMesh : Mesh<NUD.DisplayVertex>
    {
        public ForgeMesh(List<NUD.DisplayVertex> vertices, List<int> vertexIndices) : base(vertices, vertexIndices, NUD.DisplayVertex.Size)
        {

        }

        protected override List<VertexAttributeInfo> GetVertexAttributes()
        {
            return new List<VertexAttributeInfo>()
            {
                new VertexAttributeInfo("position",  3, VertexAttribPointerType.Float, Vector3.SizeInBytes, 0),
            };
        }
    }
}
