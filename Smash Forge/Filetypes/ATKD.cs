using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smash_Forge
{
    public class ATKD
    {
        public class Entry
        {
            public ushort attackId;
            public ushort start;
            public ushort end;
            public float xmin;
            public float xmax;
            public float ymin;
            public float ymax;

            public Entry Read(FileData f)
            {
                attackId = f.readUShort();
                f.readUShort();//skip padding
                start = f.readUShort();
                end = f.readUShort();
                xmin = f.readFloat();
                xmax = f.readFloat();
                ymin = f.readFloat();
                ymax = f.readFloat();
                return this;
            }
        }

        public List<Entry> entries = new List<Entry>();
        public uint commonSubactions;
        public uint uniqueSubactions;

        public ATKD Read(string filename)
        {
            return Read(new FileData(filename));
        }

        public ATKD Read(FileData f)
        {
            f.skip(4);
            int entryCount = f.readInt();
            commonSubactions = (uint)f.readInt();
            uniqueSubactions = (uint)f.readInt();
            for(int i = 0; i < entryCount; i++)
                entries.Add(new Entry().Read(f));
            return this;
        }

        public void Save(string filename)
        {
            FileOutput f = new FileOutput();
            f.Endian = System.IO.Endianness.Big;
            f.writeChars("ATKD".ToCharArray());
            f.writeInt(entries.Count);
            f.writeUInt(commonSubactions);
            f.writeUInt(uniqueSubactions);
            foreach (Entry e in entries)
            {
                f.writeUShort(e.attackId);
                f.writeUShort(0);
                f.writeUShort(e.start);
                f.writeUShort(e.end);
                f.writeFloat(e.xmin);
                f.writeFloat(e.xmax);
                f.writeFloat(e.ymin);
                f.writeFloat(e.ymax);
            }
            f.save(filename);
        }
    }
}
