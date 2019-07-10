using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGenericModel;
using SFGenericModel.VertexAttributes;
using System.Collections.Generic;

namespace Smash_Forge.Filetypes.Melee.Rendering
{
    public struct MeleeVertex
    {
        [VertexFloatAttribute("vPosition", ValueCount.Three, VertexAttribPointerType.Float, false)]
        public Vector3 Pos;

        [VertexFloatAttribute("vNormal", ValueCount.Three, VertexAttribPointerType.Float, false)]
        public Vector3 Nrm;

        [VertexFloatAttribute("vBitan", ValueCount.Three, VertexAttribPointerType.Float, false)]
        public Vector3 Bit;

        [VertexFloatAttribute("vTan", ValueCount.Three, VertexAttribPointerType.Float, false)]
        public Vector3 Tan;

        [VertexFloatAttribute("vUV0", ValueCount.Two, VertexAttribPointerType.Float, false)]
        public Vector2 UV0;

        [VertexFloatAttribute("vColor", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 Clr;

        [VertexFloatAttribute("vBone", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 Bone;

        [VertexFloatAttribute("vWeight", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 Weight;
    }

    public class MeleeMesh : GenericMesh<MeleeVertex>
    {
        public MeleeMesh(IList<MeleeVertex> vertices, IList<int> vertexIndices, PrimitiveType primitiveType)
            : base(vertices, vertexIndices, primitiveType)
        {

        }
    }
}
