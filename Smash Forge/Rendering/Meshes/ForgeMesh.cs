using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace Smash_Forge.Rendering.Meshes
{
    class ForgeMesh : Mesh<NUD.DisplayVertex>
    {
        public ForgeMesh(List<NUD.DisplayVertex> vertices, List<int> vertexIndices) : base(vertices, vertexIndices, NUD.DisplayVertex.Size)
        {

        }

        protected override List<VertexAttributeInfo> GetVertexAttributes()
        {
            // TODO: Fix these offsets
            return new List<VertexAttributeInfo>()
            {
                new VertexAttributeInfo("vPosition",  3, VertexAttribPointerType.Float, Vector3.SizeInBytes, 0),
                new VertexAttributeInfo("vColor",     3, VertexAttribPointerType.Float, Vector3.SizeInBytes, 56),
                new VertexAttributeInfo("vNormal",    3, VertexAttribPointerType.Float, Vector3.SizeInBytes, 12),
                new VertexAttributeInfo("vTangent",   3, VertexAttribPointerType.Float, Vector3.SizeInBytes, 24),
                new VertexAttributeInfo("vBiTangent", 3, VertexAttribPointerType.Float, Vector3.SizeInBytes, 36),
                new VertexAttributeInfo("vUV",        2, VertexAttribPointerType.Float, Vector2.SizeInBytes, 48),
                new VertexAttributeInfo("vUV2",       2, VertexAttribPointerType.Float, Vector2.SizeInBytes, 104),
                new VertexAttributeInfo("vUV3",       2, VertexAttribPointerType.Float, Vector2.SizeInBytes, 112),
                new VertexAttributeInfo("vBone",      4, VertexAttribPointerType.Int,   Vector4.SizeInBytes, 72),
                new VertexAttributeInfo("vWeight",    4, VertexAttribPointerType.Float, Vector4.SizeInBytes, 88)
            };
        }
    }
}
