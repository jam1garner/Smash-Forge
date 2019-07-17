using System.Collections.Generic;

namespace SmashForge
{
    public class ATKD
    {
        public class Entry
        {
            public ushort subaction;
            public ushort startFrame;
            public ushort lastFrame;
            public float xmin;
            public float xmax;
            public float ymin;
            public float ymax;

            public Entry Read(FileData f)
            {
                subaction = f.ReadUShort();
                f.ReadUShort();//skip padding
                startFrame = f.ReadUShort();
                lastFrame = f.ReadUShort();
                xmin = f.ReadFloat();
                xmax = f.ReadFloat();
                ymin = f.ReadFloat();
                ymax = f.ReadFloat();
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
            f.Skip(4);
            int entryCount = f.ReadInt();
            commonSubactions = (uint)f.ReadInt();
            uniqueSubactions = (uint)f.ReadInt();
            for(int i = 0; i < entryCount; i++)
                entries.Add(new Entry().Read(f));
            return this;
        }

        public void Save(string filename)
        {
            FileOutput f = new FileOutput();
            f.endian = System.IO.Endianness.Big;
            f.WriteChars("ATKD".ToCharArray());
            f.WriteInt(entries.Count);
            f.WriteUInt(commonSubactions);
            f.WriteUInt(uniqueSubactions);
            foreach (Entry e in entries)
            {
                f.WriteUShort(e.subaction);
                f.WriteUShort(0);
                f.WriteUShort(e.startFrame);
                f.WriteUShort(e.lastFrame);
                f.WriteFloat(e.xmin);
                f.WriteFloat(e.xmax);
                f.WriteFloat(e.ymin);
                f.WriteFloat(e.ymax);
            }
            f.Save(filename);
        }
    }
}
