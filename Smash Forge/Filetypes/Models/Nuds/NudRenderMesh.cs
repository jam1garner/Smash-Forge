using OpenTK.Graphics.OpenGL;
using SFGenericModel;
using SFGenericModel.Materials;
using SFGenericModel.RenderState;
using SFGenericModel.VertexAttributes;
using Smash_Forge.Filetypes.Models.Nuds;
using System.Collections.Generic;

namespace Smash_Forge
{
    public class NudRenderMesh : GenericMesh<NUD.DisplayVertex>
    {
        public NudRenderMesh(List<NUD.DisplayVertex> vertices, List<int> vertexIndices) : base(vertices, vertexIndices, PrimitiveType.Triangles)
        {

        }

        public void SetMaterialValues(NUD.Material nudMaterial)
        {
            material = new GenericMaterial();
            NudUniforms.SetMaterialPropertyUniforms(material, nudMaterial);
        }

        public void SetWireFrame(bool enabled)
        {
            if (enabled)
                renderSettings.polygonModeSettings = new PolygonModeSettings(MaterialFace.Front, PolygonMode.Line);
            else
                renderSettings.polygonModeSettings = PolygonModeSettings.Default;
        }

        public void SetRenderSettings(NUD.Material material)
        {
            SetAlphaBlending(material);
            SetAlphaTesting(material);
            SetDepthTesting(material);
            SetFaceCulling(material);
        }

        public void ResetRenderSettings()
        {
            renderSettings = new RenderSettings();
        }

        private void SetFaceCulling(NUD.Material material)
        {
            bool enabled = true;
            CullFaceMode cullFaceMode = CullFaceMode.Back;
            switch (material.cullMode)
            {
                case 0x0000:
                    enabled = false;
                    break;
                case 0x0404:
                    cullFaceMode = CullFaceMode.Front;
                    break;
                case 0x0405:
                    cullFaceMode = CullFaceMode.Back;
                    break;
                default:
                    enabled = false;
                    break;
            }

            renderSettings.faceCullingSettings = new FaceCullingSettings(enabled, cullFaceMode);
        }

        private void SetAlphaTesting(NUD.Material material)
        {

            bool enabled = (material.alphaTest == (int)NUD.Material.AlphaTest.Enabled);

            AlphaFunction alphaFunc = AlphaFunction.Always;
            if (NUD.Material.alphaFunctionByMatValue.ContainsKey(material.alphaFunction))
                alphaFunc = NUD.Material.alphaFunctionByMatValue[material.alphaFunction];

            float refAlpha = material.RefAlpha / 255.0f;

            renderSettings.alphaTestSettings = new AlphaTestSettings(enabled, alphaFunc, refAlpha);
        }

        private void SetDepthTesting(NUD.Material material)
        {
            bool depthMask = (material.srcFactor != 4) && (material.srcFactor != 51) && (material.srcFactor != 50);
            renderSettings.depthTestSettings = new DepthTestSettings(true, depthMask, DepthFunction.Lequal);
        }

        private void SetAlphaBlending(NUD.Material material)
        {
            renderSettings.alphaBlendSettings.enabled = material.srcFactor != 0 || material.dstFactor != 0;
            if (NudEnums.srcFactorByMatValue.ContainsKey(material.srcFactor))
                renderSettings.alphaBlendSettings.sourceFactor = NudEnums.srcFactorByMatValue[material.srcFactor];

            if (NudEnums.dstFactorByMatValue.ContainsKey(material.dstFactor))
                renderSettings.alphaBlendSettings.destinationFactor = NudEnums.dstFactorByMatValue[material.dstFactor];

            // A weird multiplicative blend mode with premultiplied alpha.
            if (material.dstFactor == 5)
                renderSettings.alphaBlendSettings.sourceFactor = BlendingFactor.Zero;

            renderSettings.alphaBlendSettings.blendingEquationRgb = BlendEquationMode.FuncAdd;
            if (material.dstFactor == 3)
                renderSettings.alphaBlendSettings.blendingEquationRgb = BlendEquationMode.FuncReverseSubtract;

            renderSettings.alphaBlendSettings.blendingEquationAlpha = BlendEquationMode.FuncAdd;
        }

        public override List<VertexAttribute> GetVertexAttributes()
        {
            return new List<VertexAttribute>()
            {                                                 
                new VertexAttributeInfo("vPosition",  ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vNormal",    ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vTangent",   ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vBiTangent", ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vUV",        ValueCount.Two,   VertexAttribPointerType.Float),
                new VertexAttributeInfo("vColor",     ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexAttributeIntInfo("vBone",   ValueCount.Four,  VertexAttribIntegerType.Int),
                new VertexAttributeInfo("vWeight",    ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexAttributeInfo("vUV2",       ValueCount.Two,   VertexAttribPointerType.Float),
                new VertexAttributeInfo("vUV3",       ValueCount.Two,   VertexAttribPointerType.Float),
            };
        }
    }
}
