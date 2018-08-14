using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Shaders;

namespace Smash_Forge.Rendering.Meshes
{
    public class ForgeMesh : Mesh<NUD.DisplayVertex>
    {
        public ForgeMesh(List<NUD.DisplayVertex> vertices, List<int> vertexIndices) : base(vertices, vertexIndices, NUD.DisplayVertex.Size)
        {

        }

        protected override void SetUniforms(Shader shader)
        {

        }

        protected override List<VertexAttributeInfo> GetVertexAttributes()
        {
            return new List<VertexAttributeInfo>()
            {
                new VertexAttributeInfo("vPosition",   3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vNormal",     3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vTangent",    3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vBitangent",  3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vUV",         2, VertexAttribPointerType.Float, Vector2.SizeInBytes),
                new VertexAttributeInfo("vColor",      4, VertexAttribPointerType.Float, Vector4.SizeInBytes),
                new VertexAttributeInfo("vBone",       4, VertexAttribPointerType.Float, Vector4.SizeInBytes),
                new VertexAttributeInfo("vWeight",     4, VertexAttribPointerType.Float, Vector4.SizeInBytes),
                new VertexAttributeInfo("vUV2",        2, VertexAttribPointerType.Float, Vector2.SizeInBytes),
                new VertexAttributeInfo("vUV3",        2, VertexAttribPointerType.Float, Vector2.SizeInBytes),
            };
        }
    }
}
