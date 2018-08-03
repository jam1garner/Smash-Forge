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
            public ushort unk; //may be bone index? (according to KingClubber)
            public ushort start;
            public ushort end;
            public float xmin;
            public float xmax;
            public float ymin;
            public float ymax;

            public Entry Read(FileData f)
            {
                attackId = f.readUShort();
                unk = f.readUShort();
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
        public uint unknown1;
        public uint unknown2;

        public ATKD Read(string filename)
        {
            return Read(new FileData(filename));
        }

        public ATKD Read(FileData f)
        {
            f.skip(4);
            int entryCount = f.readInt();
            unknown1 = (uint)f.readInt();
            unknown2 = (uint)f.readInt();
            for(int i = 0; i < entryCount; i++)
                entries.Add((new Entry()).Read(f));
            return this;
        }
    }
}
