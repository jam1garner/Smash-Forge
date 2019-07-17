using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SmashForge
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
            data.endian = Endianness.Big;

            if (data != null)
            {
                var magic = data.ReadString();
                if (magic == "PACK")
                {
                    data.endian = Endianness.Little;
                }
                data.Seek(0x08);
                var count = data.ReadInt();

                for (int i = 0; i < count; i++)
                {
                    data.Seek(0x10 + (i * 4));
                    var strOffset = data.ReadInt();
                    data.Seek(strOffset);
                    strings.Add(data.ReadString());

                    data.Seek(0x10 + (count * 4) + (i * 4));
                    offsets.Add(data.ReadInt());

                    data.Seek((count * 4) + (count * 4) + (i * 4) + 0x10);
                    sizes.Add(data.ReadInt());

                    data.Seek(offsets[i]);
                    var b = data.Read(sizes[i]);
                    Files.Add(strings[i], b);
                }
            }
        }
        public override byte[] Rebuild()
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
                    

                    // OMO
                    foreach (var pair in Files)
                    {
                        // Align to 0x10
                        while (writer.BaseStream.Position % 0x10 > 0)
                            writer.Write((byte)0);

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
