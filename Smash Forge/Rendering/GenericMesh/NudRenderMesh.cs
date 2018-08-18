using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Smash_Forge.Filetypes.Models.Nuds;
using SFGenericModel;
using SFGenericModel.Materials;

namespace Smash_Forge.Rendering.Meshes
{
    public class NudRenderMesh : GenericMesh<NUD.DisplayVertex>
    {
        public NudRenderMesh(List<NUD.DisplayVertex> vertices, List<int> vertexIndices) : base(vertices, vertexIndices, NUD.DisplayVertex.Size)
        {

        }

        public void SetMaterialValues(NUD.Material nudMaterial)
        {
            material = new GenericMaterial();
            NudUniforms.SetMaterialPropertyUniforms(material, nudMaterial);
        }

        public void SetRenderSettings(NUD.Material material)
        {
            SetAlphaBlending(material);
            SetAlphaTesting(material);
            SetDepthTesting(material);
            SetFaceCulling(material);
        }

        private void SetFaceCulling(NUD.Material material)
        {
            renderSettings.faceCullingSettings.enableFaceCulling = true;
            renderSettings.faceCullingSettings.cullFaceMode = CullFaceMode.Back;
            switch (material.cullMode)
            {
                case 0x0000:
                    renderSettings.faceCullingSettings.enableFaceCulling = false;
                    break;
                case 0x0404:
                    renderSettings.faceCullingSettings.cullFaceMode = CullFaceMode.Front;
                    break;
                case 0x0405:
                    renderSettings.faceCullingSettings.cullFaceMode = CullFaceMode.Back;
                    break;
                default:
                    renderSettings.faceCullingSettings.enableFaceCulling = false;
                    break;
            }
        }

        private void SetAlphaTesting(NUD.Material material)
        {
            renderSettings.alphaTestSettings.enableAlphaTesting = (material.alphaTest == (int)NUD.Material.AlphaTest.Enabled);

            renderSettings.alphaTestSettings.alphaFunction = AlphaFunction.Always;
            if (NUD.Material.alphaFunctionByMatValue.ContainsKey(material.alphaFunction))
                renderSettings.alphaTestSettings.alphaFunction = NUD.Material.alphaFunctionByMatValue[material.alphaFunction];

            renderSettings.alphaTestSettings.referenceAlpha = material.RefAlpha;
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
            renderSettings.alphaBlendSettings.enableAlphaBlending = material.srcFactor != 0 || material.dstFactor != 0;
            if (NUD.srcFactorsByMatValue.ContainsKey(material.srcFactor))
                renderSettings.alphaBlendSettings.sourceFactor = NUD.srcFactorsByMatValue[material.srcFactor];

            if (NUD.dstFactorsByMatValue.ContainsKey(material.dstFactor))
                renderSettings.alphaBlendSettings.destinationFactor = NUD.dstFactorsByMatValue[material.dstFactor];

            renderSettings.alphaBlendSettings.blendingEquationRgb = BlendEquationMode.FuncAdd;
            if (material.dstFactor == 3)
                renderSettings.alphaBlendSettings.blendingEquationRgb = BlendEquationMode.FuncReverseSubtract;

            renderSettings.alphaBlendSettings.blendingEquationAlpha = BlendEquationMode.FuncAdd;
        }

        protected override List<VertexAttributeInfo> GetVertexAttributes()
        {
            return new List<VertexAttributeInfo>()
            {
                new VertexAttributeInfo("vPosition",   3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vNormal",     3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vTangent",    3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vBiTangent",  3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vUV",         2, VertexAttribPointerType.Float, Vector2.SizeInBytes),
                new VertexAttributeInfo("vColor",      4, VertexAttribPointerType.Float, Vector4.SizeInBytes),
                new VertexAttributeInfo("vBone",       4, VertexAttribPointerType.Float, Vector4.SizeInBytes),
                new VertexAttributeInfo("vWeight",     4, VertexAttribPointerType.Float, Vector4.SizeInBytes),
                new VertexAttributeInfo("vUV2",        2, VertexAttribPointerType.Float, Vector2.SizeInBytes),
                new VertexAttributeInfo("vUV3",        2, VertexAttribPointerType.Float, Vector2.SizeInBytes),
            };
        }
    }
}
