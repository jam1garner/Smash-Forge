using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Smash_Forge
{
    public class JTB : FileBase
    {
        public JTB()
        {
            Table1 = new List<short>();
            Table2 = new List<short>();
        }
        public JTB(string filename) : this()
        {
            Read(filename);
        }
        public override Endianness Endian { get; set; }
        public List<short> Table1 { get; set; }
        public List<short> Table2 { get; set; }

        public override void Read(string filename)
        {
            using (var stream = File.Open(filename, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var size1 = reader.ReadBint16();
                    var size2 = reader.ReadBint16();
                    for (int i = 0; i < size1; i++)
                    {
                        Table1.Add(reader.ReadBint16());
                    }
                    for (int i = 0; i < size2; i++)
                    {
                        Table2.Add(reader.ReadBint16());
                    }
                }
            }
        }
        public override byte[] Rebuild()
        {
            throw new NotImplementedException();
        }
    }
}
