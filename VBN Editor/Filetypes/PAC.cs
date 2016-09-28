using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VBN_Editor
{
    public class PAC : FileBase
    {
        public PAC()
        {
            Files = new Dictionary<string, byte[]>();
        }
        public override Endianness Endian { get; set; }

        public Dictionary<string, byte[]> Files { get; set; }

        public override void Read(string filename)
        {
            var offsets = new List<int>();
            var strings = new List<string>();
            var sizes = new List<int>();

            Files = new Dictionary<string, byte[]>();
            FileData data = new FileData(filename);
            data.Endian = Endianness.Big;

            if (data != null)
            {
                var magic = data.readString();
                if (magic == "PACK")
                {
                    data.Endian = Endianness.Little;
                }
                data.seek(0x08);
                var count = data.readInt();

                for (int i = 0; i < count; i++)
                {
                    data.seek(0x10 + (i * 4));
                    var strOffset = data.readInt();
                    data.seek(strOffset);
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
        public override void Rebuild()
        {
            // Nothing really to rebuild here. All file and header data can be 
            // generated from Files
            return;
        }
        public override byte[] GetBytes()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    var dataOffs = new List<int>();
                    var strOffs = new List<int>();

                    // header
                    if (Endian == Endianness.Little)
                        writer.Write("PACK".ToCharArray());
                    else
                        writer.Write("KCAP".ToCharArray());
                    writer.Write(0);
                    writer.Write(Files.Count, Endian);
                    writer.Write(0);

                    // Write the info arrays as null first
                    // we'll fill in the data later
                    for (int i = 0; i < Files.Count*3; i++)
                        writer.Write(0);

                    // Strings
                    foreach (var pair in Files)
                    {
                        strOffs.Add((int)writer.BaseStream.Position);
                        writer.WriteStringNT(pair.Key);
                    }

                    // Align to 0x10
                    while (writer.BaseStream.Position % 0x10 > 0)
                        writer.Write((byte)0);

                    // OMO
                    foreach (var pair in Files)
                    {
                        dataOffs.Add((int)writer.BaseStream.Position);
                        writer.Write(pair.Value);
                    }

                    // Seek back and fill in offset + size data
                    for (int i = 0; i < Files.Count; i++)
                    {
                        var pair = Files.ElementAt(i);
                        // string offsets
                        writer.BaseStream.Seek(0x10 + (i * 4), SeekOrigin.Begin);
                        writer.Write(strOffs[i], Endian);
                        // data offsets
                        writer.BaseStream.Seek(0x10 + (strOffs.Count * 4) + (i * 4), SeekOrigin.Begin);
                        writer.Write(dataOffs[i], Endian);
                        // sizes
                        writer.BaseStream.Seek(0x10 + (strOffs.Count * 8) + (i * 4), SeekOrigin.Begin);
                        writer.Write(pair.Value.Length, Endian);
                    }
                }
                return stream.ToArray();
            }
        }
    }
}
