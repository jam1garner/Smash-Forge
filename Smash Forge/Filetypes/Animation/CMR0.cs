using System.Collections.Generic;
using OpenTK;

namespace SmashForge
{
    public class CMR0
    {
        public CMR0()
        {
        }

        public List<Matrix4> frames = new List<Matrix4>();

        public void read(FileData d){
            d.endian = System.IO.Endianness.Big;
            d.Skip(8);
            int count = d.ReadInt();
            for (int i = 0; i < count; i++)
            {
                Matrix4 m = new Matrix4(
                    new Vector4(d.ReadFloat(), d.ReadFloat(), d.ReadFloat(), d.ReadFloat()),
                    new Vector4(d.ReadFloat(), d.ReadFloat(), d.ReadFloat(), d.ReadFloat()),
                    new Vector4(d.ReadFloat(), d.ReadFloat(), d.ReadFloat(), d.ReadFloat()),
                    new Vector4(0,0,0,1)
                );
                frames.Add(m);
            }
        }
    }
}

