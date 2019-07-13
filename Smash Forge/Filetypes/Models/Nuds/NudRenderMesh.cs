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
        private RenderSettings renderSettings = new RenderSettings();

        public NudRenderMesh(List<NUD.DisplayVertex> vertices, List<int> vertexIndices) : base(vertices, vertexIndices, PrimitiveType.Triangles)
        {

        }

        public void SetMaterialValues(NUD.Material nudMaterial, SFGraphics.GLObjects.Shaders.Shader shader)
        {
            var material = new GenericMaterial();
            NudUniforms.SetMaterialPropertyUniforms(material, nudMaterial);

            material.SetShaderUniforms(shader);
        }

        public void SetWireFrame(bool enabled)
        {
            if (enabled)
                renderSettings.polygonModeSettings = new PolygonModeSettings(MaterialFace.Front, PolygonMode.Line);
            else
                renderSettings.polygonModeSettings = PolygonModeSettings.Default;

            GLRenderSettings.SetRenderSettings(renderSettings);
        }

        public void SetRenderSettings(NUD.Material material)
        {
            SetAlphaBlending(material);
            SetAlphaTesting(material);
            SetDepthTesting(material);
            SetFaceCulling(material);

            GLRenderSettings.SetRenderSettings(renderSettings);
        }

        public void ResetRenderSettings()
        {
            renderSettings = new RenderSettings();
        }

        private void SetFaceCulling(NUD.Material material)
        {
            bool enabled = true;
            CullFaceMode cullFaceMode = CullFaceMode.Back;
            switch (material.CullMode)
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

            bool enabled = (material.AlphaTest == (int)NudEnums.AlphaTest.Enabled);

            AlphaFunction alphaFunc = AlphaFunction.Always;
            if (NudEnums.alphaFunctionByMatValue.ContainsKey(material.AlphaFunction))
                alphaFunc = NudEnums.alphaFunctionByMatValue[material.AlphaFunction];

            float refAlpha = material.RefAlpha / 255.0f;

            renderSettings.alphaTestSettings = new AlphaTestSettings(enabled, alphaFunc, refAlpha);
        }

        private void SetDepthTesting(NUD.Material material)
        {
            bool depthMask = (material.SrcFactor != 4) && (material.SrcFactor != 51) && (material.SrcFactor != 50);
            renderSettings.depthTestSettings = new DepthTestSettings(true, depthMask, DepthFunction.Lequal);
        }

        private void SetAlphaBlending(NUD.Material material)
        {
            renderSettings.alphaBlendSettings.enabled = material.SrcFactor != 0 || material.DstFactor != 0;
            if (NudEnums.srcFactorByMatValue.ContainsKey(material.SrcFactor))
                renderSettings.alphaBlendSettings.sourceFactor = NudEnums.srcFactorByMatValue[material.SrcFactor];

            if (NudEnums.dstFactorByMatValue.ContainsKey(material.DstFactor))
                renderSettings.alphaBlendSettings.destinationFactor = NudEnums.dstFactorByMatValue[material.DstFactor];

            renderSettings.alphaBlendSettings.blendingEquationRgb = BlendEquationMode.FuncAdd;
            if (material.DstFactor == 3 || material.DstFactor == 5)
                renderSettings.alphaBlendSettings.blendingEquationRgb = BlendEquationMode.FuncReverseSubtract;

            renderSettings.alphaBlendSettings.blendingEquationAlpha = BlendEquationMode.FuncAdd;
        }

        public override List<VertexAttribute> GetVertexAttributes()
        {
            return new List<VertexAttribute>()
            {                                                 
                new VertexFloatAttribute("vPosition",  ValueCount.Three, VertexAttribPointerType.Float, false),
                new VertexFloatAttribute("vNormal",    ValueCount.Three, VertexAttribPointerType.Float, false),
                new VertexFloatAttribute("vTangent",   ValueCount.Three, VertexAttribPointerType.Float, false),
                new VertexFloatAttribute("vBiTangent", ValueCount.Three, VertexAttribPointerType.Float, false),
                new VertexFloatAttribute("vUV",        ValueCount.Two,   VertexAttribPointerType.Float, false),
                new VertexFloatAttribute("vColor",     ValueCount.Four,  VertexAttribPointerType.Float, false),
                new VertexIntAttribute("vBone",        ValueCount.Four,  VertexAttribIntegerType.Int),
                new VertexFloatAttribute("vWeight",    ValueCount.Four,  VertexAttribPointerType.Float, false),
                new VertexFloatAttribute("vUV2",       ValueCount.Two,   VertexAttribPointerType.Float, false),
                new VertexFloatAttribute("vUV3",       ValueCount.Two,   VertexAttribPointerType.Float, false),
            };
        }
    }
}
