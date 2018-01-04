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
            Tables = new List<List<short>>();
        }
        public JTB(string filename) : this()
        {
            Read(filename);
            Text = Path.GetFileName(filename);
            FilePath = filename;
        }

        public string FilePath;
        public override Endianness Endian { get; set; }
        public List<List<short>> Tables { get; set; }


        public override void Read(string filename)
        {
            {
                {
                    FileData d = new FileData(filename);
                    var size1 = d.readShort();
                    if (size1 > 255)
                    {
                        d.seek(0);
                        d.Endian = Endianness.Little;
                        size1 = d.readShort();
                    }
                    var size2 = d.readShort();
                    List<short> Table1 = new List<short>();
                    for (int i = 0; i < size1; i++)
                    {
                        Table1.Add((short)d.readShort());
                    }
                    List<short> Table2 = new List<short>();
                    for (int i = 0; i < size2; i++)
                    {
                        Table2.Add((short)d.readShort());
                    }
                    Tables = new List<List<short>>();
                    Tables.Add(Table1);
                    Tables.Add(Table2);
                }
            }
        }
        public override byte[] Rebuild()
        {
            throw new NotImplementedException();
        }
    }
}
