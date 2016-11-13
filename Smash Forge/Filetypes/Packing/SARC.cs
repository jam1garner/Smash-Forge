using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Smash_Forge
{
    class SARC
    {
        public Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
        public Endianness endian;

        struct SFATNode
        {
            public int nameHash;
            public int nameTableOffset;
            public int fileDataOffset;
            public int endFileDataOffset;
        }

        public SARC()
        {

        }

        public SARC(string filename)
        {
            FileData f = new FileData(filename);
            f.Endian = Endianness.Big;
            f.seek(6); //SARC
            int endnss = f.readShort();
            if (endnss == -257)
                f.Endian = Endianness.Big;
            if (endnss == -2)
                f.Endian = Endianness.Little;

            endian = f.Endian;

            int archiveSize = f.readInt();
            int startOffset = f.readInt();

            f.skip(10);//SFAT
            int nodeCount = f.readShort();
            int hashMultiplier = f.readInt();

            List<SFATNode> sfatNodes = new List<SFATNode>();
            for (int i = 0; i < nodeCount; i++)
            {
                SFATNode temp;
                temp.nameHash = f.readInt();
                f.skip(1);
                temp.nameTableOffset = ((f.readByte() * 256) + f.readByte()) * 256 + f.readByte();
                temp.fileDataOffset = f.readInt();
                temp.endFileDataOffset = f.readInt();
                sfatNodes.Add(temp);
            }

            f.skip(8);//53 46 4E 54 00 08 00 00 SFNT is dumb

            int nameTableStart = f.pos();
            foreach (SFATNode sfat in sfatNodes)
            {
                f.seek(sfat.nameTableOffset * 4 + nameTableStart);
                string tempName = f.readString();
                f.seek(sfat.fileDataOffset + startOffset);
                byte[] tempFile = f.read(sfat.endFileDataOffset - sfat.fileDataOffset);
                files.Add(tempName, tempFile);
            }
        }

        uint GetHash(char[] name, int length, uint multiplier)
        {
            uint result = 0;
            for (int i = 0; i < length; i++)
            {
                result = name[i] + result * multiplier;
            }
            return result;
        }

        int GetSizeInChunks(int length, int chunkSize)
        {
            int i = length;
            while (i % 4 != 0)
                i++;
            return i;
        }

        private int sfatStartOffset;

        private byte[] RebuildSFATArchive()
        {
            FileOutput f = new FileOutput();
            f.Endian = endian;

            f.writeString("SFAT");
            f.writeShort(0xC);
            f.writeShort(files.Count);
            f.writeInt(0x65);

            int stringPos = 0;
            int dataPos = 0;
            foreach (string filename in files.Keys)
            {
                f.writeInt((int)GetHash(filename.ToArray(), filename.Length, 0x65));
                f.writeByte(1);
                if(endian == Endianness.Big)
                {
                    f.writeByte((byte)((stringPos & 0xFF0000) >> 64));
                    f.writeByte((byte)((stringPos & 0xFF00) >> 32));
                    f.writeByte((byte)(stringPos & 0xFF));
                }
                else
                {
                    f.writeByte((byte)(stringPos & 0xFF));
                    f.writeByte((byte)((stringPos & 0xFF00) >> 32));
                    f.writeByte((byte)((stringPos & 0xFF0000) >> 64));
                }
                stringPos += GetSizeInChunks(filename.Length + 1, 4);
                f.writeInt(dataPos);
                f.writeInt(dataPos + files[filename].Length);
                dataPos += files[filename].Length;
            }

            f.writeHex("53464E5400080000");

            foreach (string filename in files.Keys)
            {
                f.writeString(filename);
                f.writeByte(0);
                while (f.pos() % 4 != 0)
                    f.writeByte(0);
            }

            sfatStartOffset = f.pos();

            foreach (string filename in files.Keys)
            {
                f.writeBytes(files[filename]);
            }

            return f.getBytes();
        }

        public byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            f.writeString("SARC");
            f.writeShort(0x14);
            f.Endian = endian;
            f.writeShort(65279);

            byte[] sfat = RebuildSFATArchive();

            f.writeInt(sfat.Length + 0x14);
            f.writeInt(sfatStartOffset + 0x14);
            f.writeInt(0x1000000);
            f.writeBytes(sfat);

            return f.getBytes();
        }  
    }
}
