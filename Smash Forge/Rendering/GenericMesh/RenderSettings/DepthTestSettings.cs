using OpenTK.Graphics.OpenGL;

namespace Smash_Forge.Rendering.Meshes
{
    partial class RenderSettings
    {
        public class DepthTestSettings
        {
            public bool enableDepthTest = true;

            public bool depthMask = true;

            public DepthFunction depthFunction = DepthFunction.Lequal;
        }
    }
}
