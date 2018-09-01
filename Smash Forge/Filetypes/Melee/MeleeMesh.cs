using System.Collections.Generic;
using SFGenericModel;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge
{
    public struct MeleeVertex
    {
        public Vector3 Pos;
        public Vector3 Nrm;
        public Vector2 UV0;
        public Vector4 Bone;
        public Vector4 Weight;

    }

    public class MeleeMesh : GenericMesh<MeleeVertex>
    {

        public MeleeMesh(List<MeleeVertex> vertices, List<int> vertexIndices)
            : base(vertices, vertexIndices)
        {
            renderSettings.faceCullingSettings.enableFaceCulling = false;
        }
        protected override List<VertexAttributeInfo> GetVertexAttributes()
        {
            return new List<VertexAttributeInfo>()
            {
                new VertexAttributeInfo("vPosition",  3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vNormal",  3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vUV0",  2, VertexAttribPointerType.Float, Vector2.SizeInBytes),
                new VertexAttributeInfo("vBone",  4, VertexAttribPointerType.Float, Vector4.SizeInBytes),
                new VertexAttributeInfo("vWeight",  4, VertexAttribPointerType.Float, Vector4.SizeInBytes)
            };
        }
    }
}
