using OpenTK.Graphics.OpenGL;

namespace Smash_Forge.Rendering.Meshes
{
    partial class RenderSettings
    {
        public class AlphaTestSettings
        {
            public bool enableAlphaTesting = false;

            public AlphaFunction alphaFunction = AlphaFunction.Gequal;

            public int referenceAlpha = 128;
        }
    }
}
