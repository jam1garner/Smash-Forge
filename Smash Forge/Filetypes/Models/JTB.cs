using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SmashForge
{
    public class JTB : FileBase
    {
        public JTB()
        {
            Tables = new List<List<short>>();

            Text = "model.jtb";
            ImageKey = "number";
            SelectedImageKey = "number";
            ToolTipText = "The joint index file";
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
                    var size1 = d.readUShort();
                    if (size1 > 255)
                    {
                        d.seek(0);
                        d.Endian = Endianness.Little;
                        size1 = d.readUShort();
                    }
                    if (d.size() < 4) return;
                    var size2 = d.readUShort();
                    List<short> Table1 = new List<short>();
                    for (int i = 0; i < size1; i++)
                    {
                        if (d.pos() + 2 > d.size()) break;
                        Table1.Add(d.readShort());
                    }
                    List<short> Table2 = new List<short>();
                    for (int i = 0; i < size2; i++)
                    {
                        if (d.pos() + 2 > d.size()) break;
                        Table2.Add(d.readShort());
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
