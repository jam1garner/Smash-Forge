using System;
using System.Collections.Generic;
using System.IO;

namespace SmashForge
{
    public class MOI : FileBase
    {
        public class Entry
        {
            public string name;
            public int[] values;
        }

        public MOI()
        {
            Text = "model.moi";
            ImageKey = "number";
            SelectedImageKey = "number";
            ToolTipText = "The model index file";
        }

        public MOI(string filename) : this()
        {
            Read(new FileData(filename));
        }

        public Endianness endianess = Endianness.Big;
        public List<Entry> entries = new List<Entry>();
        public List<Entry> otherEntries = new List<Entry>();

        public override Endianness Endian
        {
            get;
            set;
        }

        public void Read(FileData f)
        {
            f.endian = endianess;
            if (f.Size() < 4) return;
            int entryCount = f.ReadInt();
            int otherCount = f.ReadInt();
            int startOffset = f.ReadInt();
            int unk = f.ReadInt();//0x20 - Entry size?
            int unk2 = f.ReadInt();//0x8? - values per entry?
            int unk3 = f.ReadInt();//0x30? - start offset again?
            int otherStartOffset = f.ReadInt();

            if (unk != 0x20 || unk2 != 0x8 || unk3 != 0x30)
                throw new Exception("Unexpected Unkowns Please Report to Jam");

            f.Seek(startOffset);
            for (int i = 0; i < entryCount; i++)
            {
                Entry temp = new Entry();
                temp.values = new int[8];
                for (int j = 0; j < 8; j++)
                    temp.values[j] = f.ReadInt();

                int returnOffset = f.Pos();
                f.Seek(temp.values[0]);
                temp.name = f.ReadString();
                f.Seek(returnOffset);
                entries.Add(temp);
            }

            f.Seek(otherStartOffset);
            for (int i = 0; i < otherCount; i++)
            {
                Entry temp = new Entry();
                temp.values = new int[2];
                for (int j = 0; j < 2; j++)
                    temp.values[j] = f.ReadInt();

                int returnOffset = f.Pos();
                f.Seek(temp.values[0]);
                temp.name = f.ReadString();
                f.Seek(returnOffset);
                otherEntries.Add(temp);
            }
        }

        public byte[] RebuildEntries(FileOutput f)
        {
            List<int> nameOffsets = new List<int>();
            int nameTableOffset = 0x30 + (0x20 * entries.Count);
            FileOutput nameTable = new FileOutput() { endian = Endianness.Big };
            foreach (Entry entry in entries)
            {
                nameOffsets.Add(nameTableOffset + nameTable.Pos());
                nameTable.WriteString(entry.name);
                nameTable.WriteBytes(new byte[4 - (entry.name.Length % 4)]); //Pad to next word
            }
            while (nameTable.Pos() % 0x10 != 0)
                nameTable.WriteByte(0);

            for (int i = 0; i < entries.Count; i++)
                entries[i].values[0] = nameOffsets[i];

            foreach (Entry entry in entries)
                foreach (int value in entry.values)
                    f.WriteInt(value);

            f.WriteBytes(nameTable.GetBytes());

            return f.GetBytes();
        }

        public byte[] RebuildOtherEntries(FileOutput f, int startOffset)
        {
            List<int> nameOffsets = new List<int>();
            int nameTableOffset = startOffset + (8 * otherEntries.Count);
            FileOutput nameTable = new FileOutput() { endian = Endianness.Big };
            foreach (Entry entry in otherEntries)
            {
                nameOffsets.Add(nameTableOffset + nameTable.Pos());
                nameTable.WriteString(entry.name);
                nameTable.WriteBytes(new byte[4 - (entry.name.Length % 4)]); //Pad to next word
            }
            while (nameTable.Pos() % 0x10 != 0)
                nameTable.WriteByte(0);

            for (int i = 0; i < otherEntries.Count; i++)
                otherEntries[i].values[0] = nameOffsets[i];

            foreach (Entry entry in otherEntries)
                foreach (int value in entry.values)
                    f.WriteInt(value);

            f.WriteBytes(nameTable.GetBytes());

            return f.GetBytes();
        }

        public override byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.endian = endianess;

            f.WriteInt(entries.Count);
            f.WriteInt(otherEntries.Count);
            f.WriteInt(0x30);
            f.WriteInt(0x20);
            f.WriteInt(0x8);
            f.WriteInt(0x30);
            byte[] entryData = RebuildEntries(new FileOutput());
            f.WriteInt(entryData.Length + 0x30);
            f.WriteBytes(new byte[0x14]);
            f.WriteBytes(entryData);
            f.WriteBytes(RebuildOtherEntries(new FileOutput(),f.Pos()));

            return f.GetBytes();
        }

        public override void Read(string filename)
        {
            Read(new FileData(filename));
        }
    }
}
