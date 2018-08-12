using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge.Rendering.Meshes
{           
    public struct VertexAttributeInfo
    {
        public readonly string name;
        public readonly int valueCount;
        public readonly VertexAttribPointerType vertexAttribPointerType;
        public readonly int sizeInBytes;
        public readonly int offset;

        public VertexAttributeInfo(string name, int valueCount, VertexAttribPointerType vertexAttribPointerType, int sizeInBytes, int offset)
        {
            this.name = name;
            this.valueCount = valueCount;
            this.vertexAttribPointerType = vertexAttribPointerType;
            this.sizeInBytes = sizeInBytes;
            this.offset = offset;
        }
    }
}
