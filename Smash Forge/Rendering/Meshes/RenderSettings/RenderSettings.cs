using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smash_Forge.Rendering.Meshes
{
    partial class RenderSettings
    {
        public AlphaBlendSettings alphaBlendSettings = new AlphaBlendSettings();

        public AlphaTestSettings alphaTestSettings = new AlphaTestSettings();

        public DepthTestSettings depthTestSettings = new DepthTestSettings();

        public FaceCullingSettings faceCullingSettings = new FaceCullingSettings();
    }
}
