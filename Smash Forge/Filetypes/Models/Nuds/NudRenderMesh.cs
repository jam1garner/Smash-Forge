using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Smash_Forge.Filetypes.Models.Nuds;
using SFGenericModel;
using SFGenericModel.Materials;

namespace Smash_Forge
{
    public class NudRenderMesh : GenericMesh<NUD.DisplayVertex>
    {
        public NudRenderMesh(List<NUD.DisplayVertex> vertices, List<int> vertexIndices) : base(vertices, vertexIndices, PrimitiveType.Triangles)
        {

        }

        public void SetMaterialValues(NUD.Material nudMaterial)
        {
            this.material = new GenericMaterial();
            NudUniforms.SetMaterialPropertyUniforms(material, nudMaterial);
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
            renderSettings = new SFGenericModel.RenderState.RenderSettings();
        }

        private void SetFaceCulling(NUD.Material material)
        {
            renderSettings.faceCullingSettings.enabled = true;
            renderSettings.faceCullingSettings.cullFaceMode = CullFaceMode.Back;
            switch (material.cullMode)
            {
                case 0x0000:
                    renderSettings.faceCullingSettings.enabled = false;
                    break;
                case 0x0404:
                    renderSettings.faceCullingSettings.cullFaceMode = CullFaceMode.Front;
                    break;
                case 0x0405:
                    renderSettings.faceCullingSettings.cullFaceMode = CullFaceMode.Back;
                    break;
                default:
                    renderSettings.faceCullingSettings.enabled = false;
                    break;
            }
        }

        private void SetAlphaTesting(NUD.Material material)
        {
            renderSettings.alphaTestSettings.enabled = (material.alphaTest == (int)NUD.Material.AlphaTest.Enabled);

            renderSettings.alphaTestSettings.alphaFunction = AlphaFunction.Always;
            if (NUD.Material.alphaFunctionByMatValue.ContainsKey(material.alphaFunction))
                renderSettings.alphaTestSettings.alphaFunction = NUD.Material.alphaFunctionByMatValue[material.alphaFunction];

            renderSettings.alphaTestSettings.referenceAlpha = material.RefAlpha / 255.0f;
        }

        private void SetDepthTesting(NUD.Material material)
        {
            if ((material.srcFactor == 4) || (material.srcFactor == 51) || (material.srcFactor == 50))
                renderSettings.depthTestSettings.depthMask = false;
            else
                renderSettings.depthTestSettings.depthMask = true;
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

        public override List<VertexAttributeInfo> GetVertexAttributes()
        {
            return new List<VertexAttributeInfo>()
            {                                                 
                new VertexAttributeInfo("vPosition",  ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vNormal",    ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vTangent",   ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vBiTangent", ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vUV",        ValueCount.Two,   VertexAttribPointerType.Float),
                new VertexAttributeInfo("vColor",     ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexAttributeInfo("vBone",      ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexAttributeInfo("vWeight",    ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexAttributeInfo("vUV2",       ValueCount.Two,   VertexAttribPointerType.Float),
                new VertexAttributeInfo("vUV3",       ValueCount.Two,   VertexAttribPointerType.Float),
            };
        }
    }
}
