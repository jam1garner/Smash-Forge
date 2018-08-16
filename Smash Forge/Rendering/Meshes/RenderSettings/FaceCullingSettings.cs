using OpenTK.Graphics.OpenGL;

namespace Smash_Forge.Rendering.Meshes
{
    partial class RenderSettings
    {
        public class FaceCullingSettings
        {
            public bool enableFaceCulling = true;

            public CullFaceMode cullFaceMode = CullFaceMode.Back;
        }
    }
}
