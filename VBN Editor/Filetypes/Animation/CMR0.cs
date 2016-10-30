using System;
using System.Collections.Generic;
using OpenTK;

namespace Smash_Forge
{
    public class CMR0
    {
        public CMR0()
        {
        }

        public List<Matrix4> frames = new List<Matrix4>();

        public void read(FileData d){
            d.Endian = System.IO.Endianness.Big;
            d.skip(8);
            int count = d.readInt();
            for (int i = 0; i < count; i++)
            {
                Matrix4 m = new Matrix4(
                    new Vector4(d.readFloat(), d.readFloat(), d.readFloat(), d.readFloat()),
                    new Vector4(d.readFloat(), d.readFloat(), d.readFloat(), d.readFloat()),
                    new Vector4(d.readFloat(), d.readFloat(), d.readFloat(), d.readFloat()),
                    new Vector4(0,0,0,1)
                );
                frames.Add(m);
            }
        }
    }
}

