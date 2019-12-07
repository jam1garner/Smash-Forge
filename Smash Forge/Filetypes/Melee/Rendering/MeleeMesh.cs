using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGenericModel;
using SFGenericModel.VertexAttributes;
using System.Collections.Generic;
using System.Linq;

namespace SmashForge.Filetypes.Melee.Rendering
{
    public struct MeleeVertex
    {
        [VertexFloat("vPosition", ValueCount.Three, VertexAttribPointerType.Float, false)]
        public Vector3 Pos;

        [VertexFloat("vNormal", ValueCount.Three, VertexAttribPointerType.Float, false)]
        public Vector3 Nrm;

        [VertexFloat("vBitan", ValueCount.Three, VertexAttribPointerType.Float, false)]
        public Vector3 Bit;

        [VertexFloat("vTan", ValueCount.Three, VertexAttribPointerType.Float, false)]
        public Vector3 Tan;

        [VertexFloat("vUV0", ValueCount.Two, VertexAttribPointerType.Float, false)]
        public Vector2 UV0;

        [VertexFloat("vColor", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 Clr;

        [VertexFloat("vBone", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 Bone;

        [VertexFloat("vWeight", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 Weight;
    }

    public class MeleeMesh : GenericMesh<MeleeVertex>
    {
        public MeleeMesh(IList<MeleeVertex> vertices, IList<int> vertexIndices, PrimitiveType primitiveType)
            : base(vertices.ToArray(), vertexIndices.ToArray(), primitiveType)
        {

        }
    }
}
