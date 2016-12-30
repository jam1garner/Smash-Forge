using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Smash_Forge
{
    public class MOI
    {
        public class Entry
        {
            public string name;
            public int[] values;
        }

        public MOI(string filename)
        {
            Read(new FileData(filename));
        }

        public Endianness endianess = Endianness.Big;
        public List<Entry> entries = new List<Entry>();
        public List<Entry> otherEntries = new List<Entry>();

        public void Read(FileData f)
        {
            f.Endian = endianess;
            int entryCount = f.readInt();
            int otherCount = f.readInt();
            int startOffset = f.readInt();
            int unk = f.readInt();//0x20 - Entry size?
            int unk2 = f.readInt();//0x8? - values per entry?
            int unk3 = f.readInt();//0x30? - start offset again?
            int otherStartOffset = f.readInt();

            if (unk != 0x20 || unk2 != 0x8 || unk3 != 0x30)
                throw new Exception("Unexpected Unkowns Please Report to Jam");

            f.seek(startOffset);
            for (int i = 0; i < entryCount; i++)
            {
                Entry temp = new Entry();
                temp.values = new int[8];
                for (int j = 0; j < 8; j++)
                    temp.values[j] = f.readInt();

                int returnOffset = f.pos();
                f.seek(temp.values[0]);
                temp.name = f.readString();
                f.seek(returnOffset);
                entries.Add(temp);
            }

            f.seek(otherStartOffset);
            for (int i = 0; i < otherCount; i++)
            {
                Entry temp = new Entry();
                temp.values = new int[2];
                for (int j = 0; j < 2; j++)
                    temp.values[j] = f.readInt();

                int returnOffset = f.pos();
                f.seek(temp.values[0]);
                temp.name = f.readString();
                f.seek(returnOffset);
                otherEntries.Add(temp);
            }
        }

        public byte[] RebuildEntries(FileOutput f)
        {
            List<int> nameOffsets = new List<int>();
            int nameTableOffset = 0x30 + (0x20 * entries.Count);
            FileOutput nameTable = new FileOutput() { Endian = Endianness.Big };
            foreach (Entry entry in entries)
            {
                nameOffsets.Add(nameTableOffset + nameTable.pos());
                nameTable.writeString(entry.name);
                nameTable.writeBytes(new byte[4 - (entry.name.Length % 4)]); //Pad to next word
            }
            while (nameTable.pos() % 0x10 != 0)
                nameTable.writeByte(0);

            for (int i = 0; i < entries.Count; i++)
                entries[i].values[0] = nameOffsets[i];

            foreach (Entry entry in entries)
                foreach (int value in entry.values)
                    f.writeInt(value);

            f.writeBytes(nameTable.getBytes());

            return f.getBytes();
        }

        public byte[] RebuildOtherEntries(FileOutput f, int startOffset)
        {
            List<int> nameOffsets = new List<int>();
            int nameTableOffset = startOffset + (8 * otherEntries.Count);
            FileOutput nameTable = new FileOutput() { Endian = Endianness.Big };
            foreach (Entry entry in otherEntries)
            {
                nameOffsets.Add(nameTableOffset + nameTable.pos());
                nameTable.writeString(entry.name);
                nameTable.writeBytes(new byte[4 - (entry.name.Length % 4)]); //Pad to next word
            }
            while (nameTable.pos() % 0x10 != 0)
                nameTable.writeByte(0);

            for (int i = 0; i < otherEntries.Count; i++)
                otherEntries[i].values[0] = nameOffsets[i];

            foreach (Entry entry in otherEntries)
                foreach (int value in entry.values)
                    f.writeInt(value);

            f.writeBytes(nameTable.getBytes());

            return f.getBytes();
        }

        public byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = endianess;

            f.writeInt(entries.Count);
            f.writeInt(otherEntries.Count);
            f.writeInt(0x30);
            f.writeInt(0x20);
            f.writeInt(0x8);
            f.writeInt(0x30);
            byte[] entryData = RebuildEntries(new FileOutput());
            f.writeInt(entryData.Length + 0x30);
            f.writeBytes(new byte[0x14]);
            f.writeBytes(entryData);
            f.writeBytes(RebuildOtherEntries(new FileOutput(),f.pos()));

            return f.getBytes();
        }
    }
}
