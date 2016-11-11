using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Smash_Forge
{
    public class PatData
    {
        public struct keyframe
        {
            public int frameNum;
            public int texId;
        }

        public int defaultTexId;
        public int unknown;
        private int frameCount;
        public List<keyframe> keyframes = new List<keyframe>();

        public PatData() { }

        public int getTexId(int frame)
        {
            frame = frame % frameCount;
            int lastTexId = defaultTexId;
            for(int i = 0; i < keyframes.Count; i++)
            {
                if (frame < keyframes[i].frameNum)
                    return lastTexId;
                else
                    lastTexId = keyframes[i].texId;
            }
            return lastTexId;
        }

        public void read(FileData f)
        {
            keyframe temp;
            defaultTexId = f.readInt();
            int keyframeCount = f.readInt();
            int keyframeOffset = f.readInt();
            frameCount = f.readInt() + 1;
            unknown = f.readInt();
            if(keyframeOffset != f.eof())
            {
                f.seek(keyframeOffset);
                for(int i = 0;i < keyframeCount;i++) {
                    temp.texId = f.readInt();
                    temp.frameNum = f.readInt();
                    keyframes.Add(temp);
                }
            } 
        }

        public byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            return f.getBytes();
        }
    }

    public class MatData
    {
        public struct frame
        {
            public int size;
            public float[] values;
        }

        public string name;
        public List<frame> frames = new List<frame>();
        public int unknown, unknown2, unknown3;

        public MatData(){ }

        public void read(FileData f)
        {
            int nameOff = f.readInt();
            unknown = f.readInt();
            int valueCount = f.readInt();
            int frameCount = f.readInt();
            unknown2 = f.readShort();
            unknown3 = f.readShort();
            int dataOff = f.readInt();
            f.seek(nameOff);
            name = f.readString();
            f.seek(dataOff);
            for(int i = 0; i < frameCount; i++)
            {
                frame temp;
                temp.size = valueCount;
                temp.values = new float[valueCount];
                for (int j = 0; j < valueCount; j++)
                    temp.values[j] = f.readFloat();
                frames.Add(temp);
            }
        }

        public byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            return f.getBytes();
        }

    }

    public class MatEntry
    {
        public int matHash;
        public int matHash2;
        public bool hasPat;
        public string name;
        public string name2;
        public PatData pat0 = new PatData();
        public List<MatData> properties = new List<MatData>();

        public MatEntry() { }

        public void read(FileData f)
        {
            int nameOffset = f.readInt();
            matHash = f.readInt();
            int propertyCount = f.readInt();
            int propertyPos = f.readInt();
            hasPat = (0 != f.readByte());
            f.skip(3);
            int patOffset = f.readInt();
            int secondNameOff = f.readInt();
            matHash2 = f.readInt();
            if(secondNameOff != 0)
            {
                f.seek(secondNameOff);
                name2 = f.readString();
            }

            if (hasPat)
            {
                f.seek(patOffset);
                int patDataPos = f.readInt();
                if (patDataPos != 0)
                {
                    f.seek(patDataPos);
                    pat0.read(f);
                }
            }
            f.seek(propertyPos);
            for(int i = 0; i < propertyCount; i++)
            {
                int propOffset = f.readInt();
                int returnPos = f.pos();
                f.seek(propOffset);
                MatData temp = new MatData();
                temp.read(f);
                properties.Add(temp);
                f.seek(returnPos);
            }
        }

        public byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            return f.getBytes();
        }
    }

    public class VisEntry
    {
        public struct frame
        {
            public short frameNum;
            public byte state;
            public byte unknown;
        }

        int unk1;
        short unk2;
        int frameCount;
        public string name;
        List<frame> frames = new List<frame>();

        public VisEntry() { }

        public void read(FileData f)
        {
            int nameOff = f.readInt();
            unk1 = f.readInt();
            int dataOff = f.readInt();
            f.seek(nameOff);
            name = f.readString();
            f.seek(dataOff);
            frameCount = f.readInt();
            unk2 = (short)f.readShort();
            short keyframeCount = (short)f.readShort();
            int keyframeOffset = f.readInt();
            f.seek(keyframeOffset);
            frame tempFrame;
            for (int i = 0; i < keyframeCount; i++)
            {
                tempFrame.frameNum = (short)f.readShort();
                tempFrame.state = (byte)f.readByte();
                tempFrame.unknown = (byte)f.readByte();
                frames.Add(tempFrame);
                tempFrame = new frame();
            }
        }

        public int getState(int frame){
            int state = -1;
            foreach (frame f in frames)
            {
                if (f.frameNum > frame)
                {
                    break;
                }
                state = f.state;
            }
            return state;
        }

        public byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            f.writeInt(f.pos() + 0x20);
            f.writeInt(unk1);
            int offset = f.pos() + 0x18;
            offset += name.Length + 1;
            while (offset % 16 != 0)
                offset++;
            offset += 0x10;
            f.writeInt(offset);
            f.writeBytes(new byte[0x14]);
            f.writeString(name);
            f.writeByte(0);
            while (f.pos() % 16 != 0)
                f.writeByte(0);
            f.writeBytes(new byte[0x10]);
            f.writeInt(frameCount);
            f.writeShort(unk2);
            f.writeShort(frames.Count);
            f.writeInt(f.pos() + 0x18);
            f.writeBytes(new byte[0x14]);
            foreach(frame keyframe in frames)
            {
                f.writeShort(keyframe.frameNum);
                f.writeByte(keyframe.state);
                f.writeByte(keyframe.unknown);
            }

            return f.getBytes();
        }
    }

    public class MTA : FileBase
    {
        public uint unknown;
        public uint numFrames;
        public uint frameRate;
        public List<MatEntry> matEntries = new List<MatEntry>();
        public List<VisEntry> visEntries = new List<VisEntry>();

        public MTA()
        {
            Endian = Endianness.Big;
        }

        public override Endianness Endian { get; set; }

        public override void Read(string filename)
        {
            read(new FileData(filename));
        }

        public void read(FileData f)
        {
            //Console.WriteLine("MTA - " + filename);
            f.Endian = Endian;
            if (f.size() < 4)
                throw new EndOfStreamException("Blank/Broken MTA");
            f.seek(4);
            unknown = (uint)f.readInt();
            numFrames = (uint)f.readInt();
            f.skip(8);
            frameRate = (uint)f.readInt();
            int matCount = f.readInt();
            int matOffset = f.readInt();
            int visCount = f.readInt();
            int visOffset = f.readInt();
            int returnPos;
            f.seek(matOffset);
            for (int i = 0; i < matCount; i++)
            {
                returnPos = f.pos() + 4;
                f.seek(f.readInt());
                MatEntry tempMatEntry = new MatEntry();
                tempMatEntry.read(f);
                matEntries.Add(tempMatEntry);
                f.seek(returnPos);
            }
            f.seek(visOffset);
            for (int i = 0; i < visCount; i++)
            {
                returnPos = f.pos() + 4;
                f.seek(f.readInt());
                VisEntry tempVisEntry = new VisEntry();
                tempVisEntry.read(f);
                visEntries.Add(tempVisEntry);
                f.seek(returnPos);
            }
        }

        public override byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            f.writeString("MTA4");
            f.writeInt((int)unknown);
            f.writeInt((int)numFrames);
            f.writeInt(0);
            f.writeInt((int)numFrames - 1);
            f.writeInt((int)frameRate);
            f.writeInt(matEntries.Count);
            if (matEntries.Count > 0)
                f.writeInt(0x38);
            else
                f.writeInt(0);
            f.writeInt(visEntries.Count);
            if (visEntries.Count > 0)
                f.writeInt(0x38 + 4 * matEntries.Count);
            else
                f.writeInt(0);
            for (int i = 0; i < 0x10; i++)
                f.writeByte(0);

            List<byte[]> matEntriesBuilt = new List<byte[]>();
            List<byte[]> visEntriesBuilt = new List<byte[]>();
            foreach (MatEntry m in matEntries)
                matEntriesBuilt.Add(m.Rebuild());
            foreach (VisEntry v in visEntries)
                visEntriesBuilt.Add(v.Rebuild());

            int position = 0x38 + matEntries.Count + visEntries.Count;
            while (position % 0x10 != 0)
                position++;

            foreach (byte[] b in matEntriesBuilt)
            {
                f.writeInt(position);
                position += b.Length;
                while (position % 0x10 != 0)
                    position++;
            }

            foreach (byte[] b in visEntriesBuilt)
            {
                f.writeInt(position);
                position += b.Length;
                while (position % 0x10 != 0)
                    position++;
            }

            while (f.pos() % 0x10 != 0)
                f.writeByte(0);

            foreach(byte[] b in matEntriesBuilt)
            {
                f.writeBytes(b);
                while (f.pos() % 0x10 != 0)
                    f.writeByte(0);
            }

            foreach (byte[] b in visEntriesBuilt)
            {
                f.writeBytes(b);
                while (f.pos() % 0x10 != 0)
                    f.writeByte(0);
            }

            return f.getBytes();
        }
    }
    
}
