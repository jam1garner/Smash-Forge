using MeleeLib.DAT;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGenericModel;
using System.Collections.Generic;
using SFGenericModel.VertexAttributes;
using Smash_Forge.Filetypes.Melee.Utils;
using SFGenericModel.RenderState;

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
        public MeleeMesh(IList<MeleeVertex> vertices, IList<int> vertexIndices, PrimitiveType primitiveType)
            : base(vertices, vertexIndices, primitiveType)
        {
            // TODO: Why is this flipped?
            renderSettings.faceCullingSettings = new FaceCullingSettings(false, CullFaceMode.Front);
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
            bool enabled = (datDOBJ.Material.Flags & (uint)MeleeDatEnums.MiscFlags.AlphaTest) > 0;
            float refAlpha = AlphaTestSettings.Default.referenceAlpha;
            AlphaFunction alphaFunction = AlphaTestSettings.Default.alphaFunction;
            if (datDOBJ?.Material.PixelProcessing != null)
            {
                refAlpha = datDOBJ.Material.PixelProcessing.AlphaRef0 / 255.0f;
                alphaFunction = MeleeDatToOpenGL.GetAlphaFunction(datDOBJ.Material.PixelProcessing.AlphaComp0);
            }

            renderSettings.alphaTestSettings = new AlphaTestSettings(enabled, alphaFunction, refAlpha);
        }

        public override List<VertexAttribute> GetVertexAttributes()
        {
            return new List<VertexAttribute>()
            {
                new VertexFloatAttribute("vPosition", ValueCount.Three, VertexAttribPointerType.Float),
                new VertexFloatAttribute("vNormal",   ValueCount.Three, VertexAttribPointerType.Float),
                new VertexFloatAttribute("vBitan",    ValueCount.Three, VertexAttribPointerType.Float),
                new VertexFloatAttribute("vTan",      ValueCount.Three, VertexAttribPointerType.Float),
                new VertexFloatAttribute("vUV0",      ValueCount.Two,   VertexAttribPointerType.Float),
                new VertexFloatAttribute("vColor",    ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexFloatAttribute("vBone",     ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexFloatAttribute("vWeight",   ValueCount.Four,  VertexAttribPointerType.Float)
            };
        }
    }
}
