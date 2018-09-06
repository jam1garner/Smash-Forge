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
            // TODO: Why is this flipped?
            renderSettings.faceCullingSettings.enableFaceCulling = true;
            renderSettings.faceCullingSettings.cullFaceMode = CullFaceMode.Front;
        }

        protected override List<VertexAttributeInfo> GetVertexAttributes()
        {
            return new List<VertexAttributeInfo>()
            {
                new VertexAttributeInfo("vPosition", ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vNormal",   ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vUV0",      ValueCount.Two,   VertexAttribPointerType.Float),
                new VertexAttributeInfo("vBone",     ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexAttributeInfo("vWeight",   ValueCount.Four,  VertexAttribPointerType.Float)
            };
        }
    }
}
