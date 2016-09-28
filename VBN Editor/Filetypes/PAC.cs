using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VBN_Editor
{
    public class PAC
    {
        public Dictionary<string, byte[]> Files { get; set; }

        public PAC(string filename)
        {
            var offsets = new List<int>();
            var strings = new List<string>();
            var sizes = new List<int>();

            Files = new Dictionary<string, byte[]>();
            FileData data = new FileData(filename);
            data.Endian = Endianness.Big;

            if(data != null)
            {
                var magic = data.readString();
                if(magic == "PACK")
                {
                    data.Endian = Endianness.Little;
                }
                data.seek(0x08);
                var count = data.readInt();

                for (int i = 0; i < count; i++)
                {
                    data.seek(0x10 + (i * 4));
                    var strOffset = data.readInt();
                    data.seek(strOffset + 0x10);
                    strings.Add(data.readString());

                    data.seek(0x10 + (count * 4) + (i * 4));
                    offsets.Add(data.readInt());

                    data.seek((count * 4) + (count * 4) + (i * 4) + 0x10);
                    sizes.Add(data.readInt());

                    data.seek(offsets[i]);
                    var b = data.read(sizes[i]);
                    Files.Add(strings[i], b);
                }
            }
        }
    }
}
