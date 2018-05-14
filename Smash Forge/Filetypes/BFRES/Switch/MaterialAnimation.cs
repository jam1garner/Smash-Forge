using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smash_Forge
{
    public class MaterialAnimation
    {
        /*
         This section stores:
         = SRT Animations
         = Texture Pattern Animations
         = Color Animations
         - Shader Animations
        */

        public string Name;

        public void readFMAA(FileData f)
        {
            f.skip(8); //Magic
            long BlockHeader = f.readInt64();
            Name = f.readString((int)f.readInt64() + 2, -1);
            long PathOffset = f.readInt64();
            long TexturePatternAnimIndexOffset = f.readInt64();

        }
    }
}
