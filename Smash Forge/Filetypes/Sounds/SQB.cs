using System;
using System.Collections.Generic;
using System.IO;

namespace Smash_Forge
{
    public class SQB : FileBase
    {
        public override Endianness Endian { get; set; }

        public short unk1; //Possibly version, only seen as 4
        public short unk2;

        public List<SoundSequence> sequences;

        public SQB()
        {
            unk1 = 4;
            unk2 = 0;

            sequences = new List<SoundSequence>();
        }
        public SQB(string filePath)
        {
            Read(filePath);
        }
        public SQB(FileData file)
        {
            Read(file);
        }

        public override void Read(string filePath)
        {
            Read(new FileData(filePath));
        }
        public void Read(FileData file)
        {
            file.Endian = Endianness.Little;

            file.skip(0x4);

            unk1 = file.readShort();
            unk2 = file.readShort();

            int sequenceCount = file.readInt();
            int sequenceDataOffset = file.readInt();

            List<int> sequenceOffsets = new List<int>();
            for (int i = 0; i < sequenceCount; ++i)
            {
                sequenceOffsets.Add(file.readInt());
            }

            sequences = new List<SoundSequence>();
            for (int i = 0; i < sequenceCount; ++i)
            {
                if (sequenceOffsets[i] == -1)
                {
                    sequences.Add(new SoundSequence() {empty = true});
                    continue;
                }
                file.seek(0x10 + sequenceDataOffset + sequenceOffsets[i]);
                sequences.Add(new SoundSequence(file));
            }
        }
        public override byte[] Rebuild()
        {
            FileOutput file = new FileOutput();
            file.Endian = Endianness.Little;

            file.writeHex("53514200"); //SQB

            file.writeShort(unk1);
            file.writeShort(unk2);

            file.writeInt(sequences.Count);
            file.writeInt(sequences.Count * 0x4);

            int currentOffset = 0;
            for (int i = 0; i < sequences.Count; ++i)
            {
                if (sequences[i].empty)
                {
                    file.writeInt(-1);
                    continue;
                }
                file.writeInt(currentOffset);
                currentOffset += 0x8 + (0x10 * sequences[i].events.Count);
            }

            for (int i = 0; i < sequences.Count; ++i)
            {
                if (sequences[i].empty)
                {
                    continue;
                }
                sequences[i].Write(file);
            }

            return file.getBytes();
        }
    }

    public class SoundSequence
    {
        public bool empty;

        public short unk1;
        public short unk2;
        public short unk3;

        public List<SoundSequenceEvent> events;

        public SoundSequence()
        {
            empty = false;

            unk1 = 0;
            unk2 = 0;
            unk3 = 0;

            events = new List<SoundSequenceEvent>();
        }
        public SoundSequence(FileData file)
        {
            Read(file);
        }

        public void Read(FileData file)
        {
            unk1 = file.readShort();
            short eventCount = file.readShort();
            unk2 = file.readShort();
            unk3 = file.readShort();

            events = new List<SoundSequenceEvent>();
            for (int i = 0; i < eventCount; ++i)
            {
                events.Add(new SoundSequenceEvent(file));
            }
        }
        public void Write(FileOutput file)
        {
            file.writeShort(unk1);
            file.writeShort(events.Count);
            file.writeShort(unk2);
            file.writeShort(unk3);

            for (int i = 0; i < events.Count; ++i)
            {
                events[i].Write(file);
            }
        }
    }

    public class SoundSequenceEvent
    {
        public uint hash;
        public short type;
        public short frame;
        public short unk1;
        public short unk2;
        public short unk3;
        public short unk4;

        public SoundSequenceEvent()
        {
            hash = 0x00000000;
            type = 1;
            frame = 0;
            unk1 = 0;
            unk2 = 0;
            unk3 = 0;
            unk4 = 0;
        }
        public SoundSequenceEvent(FileData file)
        {
            Read(file);
        }

        public void Read(FileData file)
        {
            hash = (uint)file.readInt();
            type = file.readShort();
            frame = file.readShort();
            unk1 = file.readShort();
            unk2 = file.readShort();
            unk3 = file.readShort();
            unk4 = file.readShort();
        }
        public void Write(FileOutput file)
        {
            file.writeInt((int)hash);
            file.writeShort(type);
            file.writeShort(frame);
            file.writeShort(unk1);
            file.writeShort(unk2);
            file.writeShort(unk3);
            file.writeShort(unk4);
        }
    }
}

