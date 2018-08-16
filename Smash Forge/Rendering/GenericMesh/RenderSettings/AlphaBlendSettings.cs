using OpenTK.Graphics.OpenGL;

namespace Smash_Forge.Rendering.Meshes
{
    partial class RenderSettings
    {
        public class AlphaBlendSettings
        {
            public bool enableAlphaBlending = true;

            public BlendingFactor sourceFactor = BlendingFactor.SrcAlpha;

            public BlendingFactor destinationFactor = BlendingFactor.OneMinusSrcAlpha;

            public BlendEquationMode blendingEquation = BlendEquationMode.FuncAdd;
        }
    }
}
