using OpenTK.Graphics.OpenGL;
using SFGenericModel;
using SFGenericModel.Materials;
using SFGenericModel.RenderState;
using SFGraphics.GLObjects.Shaders;
using SmashForge.Filetypes.Models.Nuds;
using SmashForge.Rendering;
using System.Collections.Generic;

namespace SmashForge
{
    public class NudRenderMesh : GenericMesh<Nud.DisplayVertex>
    {
        private readonly UniformBlock uniformBlock = new UniformBlock(OpenTkSharedResources.shaders["Nud"], "MaterialProperties") { BlockBinding = 1 };

        private RenderSettings renderSettings = new RenderSettings();

        public NudRenderMesh(List<Nud.DisplayVertex> vertices, List<int> vertexIndices) : base(vertices.ToArray(), vertexIndices.ToArray(), PrimitiveType.Triangles)
        {

        }

        public void SetWireFrame(bool enabled)
        {
            if (enabled)
                renderSettings.polygonModeSettings = new PolygonModeSettings(MaterialFace.Front, PolygonMode.Line);
            else
                renderSettings.polygonModeSettings = PolygonModeSettings.Default;

            GLRenderSettings.SetRenderSettings(renderSettings);
        }

        public void SetMaterialValues(Shader shader, Nud.Material material)
        {
            NudUniforms.SetMaterialPropertyUniforms(uniformBlock, shader, material);
        }

        public void SetRenderSettings(Nud.Material material)
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

        private void SetFaceCulling(Nud.Material material)
        {
            bool enabled = true;
            CullFaceMode cullFaceMode = CullFaceMode.Back;
            switch (material.CullMode)
            {
                case 0x0000:
                case 0x0001:
                    enabled = false;
                    break;
                case 0x0404:
                case 0x0003:
                    cullFaceMode = CullFaceMode.Front;
                    break;
                case 0x0405:
                case 0x0002:
                    cullFaceMode = CullFaceMode.Back;
                    break;
                default:
                    enabled = false;
                    break;
            }

            renderSettings.faceCullingSettings = new FaceCullingSettings(enabled, cullFaceMode);
        }

        private void SetAlphaTesting(Nud.Material material)
        {

            bool enabled = (material.AlphaTest == (int)NudEnums.AlphaTest.Enabled);

            AlphaFunction alphaFunc = AlphaFunction.Always;
            if (NudEnums.alphaFunctionByMatValue.ContainsKey(material.AlphaFunction))
                alphaFunc = NudEnums.alphaFunctionByMatValue[material.AlphaFunction];

            float refAlpha = material.RefAlpha / 255.0f;

            renderSettings.alphaTestSettings = new AlphaTestSettings(enabled, alphaFunc, refAlpha);
        }

        private void SetDepthTesting(Nud.Material material)
        {
            bool depthMask = (material.SrcFactor != 4) && (material.SrcFactor != 51) && (material.SrcFactor != 50);
            renderSettings.depthTestSettings = new DepthTestSettings(true, depthMask, DepthFunction.Lequal);
        }

        private void SetAlphaBlending(Nud.Material material)
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
    }
}
