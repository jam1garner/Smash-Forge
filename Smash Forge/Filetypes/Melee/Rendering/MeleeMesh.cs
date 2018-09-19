using MeleeLib.DAT;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGenericModel;
using System.Collections.Generic;
using SFGenericModel.VertexAttributes;
using Smash_Forge.Filetypes.Melee.Utils;

namespace Smash_Forge.Filetypes.Melee.Rendering
{
    public struct MeleeVertex
    {
        public Vector3 Pos;
        public Vector3 Nrm;
        public Vector3 Bit;
        public Vector3 Tan;
        public Vector2 UV0;
        public Vector4 Clr;
        public Vector4 Bone;
        public Vector4 Weight;
    }

    public class MeleeMesh : GenericMesh<MeleeVertex>
    {
        public MeleeMesh(List<MeleeVertex> vertices, List<int> vertexIndices, PrimitiveType primitiveType)
            : base(vertices, vertexIndices, primitiveType)
        {
            // TODO: Why is this flipped?
            renderSettings.faceCullingSettings.enabled = false;
            renderSettings.faceCullingSettings.cullFaceMode = CullFaceMode.Front;
        }

        public void SetRenderSettings(DatDOBJ datDOBJ)
        {
            if (datDOBJ.Material == null)
                return;

            SetAlphaTesting(datDOBJ);
            SetAlphaBlending(datDOBJ);
        }

        private void SetAlphaBlending(DatDOBJ datDOBJ)
        {
            if (datDOBJ?.Material.PixelProcessing != null)
            {
                renderSettings.alphaBlendSettings.enabled = datDOBJ?.Material?.PixelProcessing.BlendMode == MeleeLib.GCX.GXBlendMode.Blend;
            }
        }

        private void SetAlphaTesting(DatDOBJ datDOBJ)
        {
            renderSettings.alphaTestSettings.enabled = (datDOBJ.Material.Flags & (uint)MeleeDatEnums.MiscFlags.AlphaTest) > 0;
            if (datDOBJ?.Material.PixelProcessing != null)
            {
                renderSettings.alphaTestSettings.referenceAlpha = datDOBJ.Material.PixelProcessing.AlphaRef0 / 255.0f;
                renderSettings.alphaTestSettings.alphaFunction = MeleeDatToOpenGL.GetAlphaFunction(datDOBJ.Material.PixelProcessing.AlphaComp0);
            }
        }

        public override List<VertexAttributeInfo> GetVertexAttributes()
        {
            return new List<VertexAttributeInfo>()
            {
                new VertexAttributeInfo("vPosition", ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vNormal",   ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vBitan",    ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vTan",      ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vUV0",      ValueCount.Two,   VertexAttribPointerType.Float),
                new VertexAttributeInfo("vColor",    ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexAttributeInfo("vBone",     ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexAttributeInfo("vWeight",   ValueCount.Four,  VertexAttribPointerType.Float)
            };
        }
    }
}
