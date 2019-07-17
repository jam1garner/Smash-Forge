using System;
using System.Collections.Generic;
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
                    var size1 = d.ReadUShort();
                    if (size1 > 255)
                    {
                        d.Seek(0);
                        d.endian = Endianness.Little;
                        size1 = d.ReadUShort();
                    }
                    if (d.Size() < 4) return;
                    var size2 = d.ReadUShort();
                    List<short> Table1 = new List<short>();
                    for (int i = 0; i < size1; i++)
                    {
                        if (d.Pos() + 2 > d.Size()) break;
                        Table1.Add(d.ReadShort());
                    }
                    List<short> Table2 = new List<short>();
                    for (int i = 0; i < size2; i++)
                    {
                        if (d.Pos() + 2 > d.Size()) break;
                        Table2.Add(d.ReadShort());
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
