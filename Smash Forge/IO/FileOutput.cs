using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SmashForge
{
    public class FileOutput
    {

        List<byte> data = new List<byte>();

        public Endianness endian;

        public byte[] GetBytes()
        {
            return data.ToArray();
        }

        public void WriteString(string s)
        {
            char[] c = s.ToCharArray();
            for (int i = 0; i < c.Length; i++)
                data.Add((byte)c[i]);
        }

        public int Size()
        {
            return data.Count;
        }

        public void WriteOutput(FileOutput d)
        {
            foreach (RelocOffset o in d.offsets)
            {
                o.position += data.Count;
                offsets.Add(o);
            }
            foreach (RelocOffset o in offsets)
            {
                if (o.output == d || o.output == null)
                    o.value += data.Count;
            }
            foreach (byte b in d.data)
                data.Add(b);
        }

        private static char[] HexToCharArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .Select(x => Convert.ToChar(x))
                             .ToArray();
        }

        public void WriteHex(string s)
        {
            char[] c = HexToCharArray(s);
            for (int i = 0; i < c.Length; i++)
                data.Add((byte)c[i]);
        }

        public void WriteInt(int i)
        {
            if (endian == Endianness.Little)
            {
                data.Add((byte)((i) & 0xFF));
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i >> 16) & 0xFF));
                data.Add((byte)((i >> 24) & 0xFF));
            }
            else
            {
                data.Add((byte)((i >> 24) & 0xFF));
                data.Add((byte)((i >> 16) & 0xFF));
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i) & 0xFF));
            }
        }

        public void WriteUInt(uint i)
        {
            if (endian == Endianness.Little)
            {
                data.Add((byte)((i) & 0xFF));
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i >> 16) & 0xFF));
                data.Add((byte)((i >> 24) & 0xFF));
            }
            else
            {
                data.Add((byte)((i >> 24) & 0xFF));
                data.Add((byte)((i >> 16) & 0xFF));
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i) & 0xFF));
            }
        }

        public void WriteIntAt(int i, int p)
        {
            if (endian == Endianness.Little)
            {
                data[p++] = (byte)((i) & 0xFF);
                data[p++] = (byte)((i >> 8) & 0xFF);
                data[p++] = (byte)((i >> 16) & 0xFF);
                data[p++] = (byte)((i >> 24) & 0xFF);
            }
            else
            {
                data[p++] = (byte)((i >> 24) & 0xFF);
                data[p++] = (byte)((i >> 16) & 0xFF);
                data[p++] = (byte)((i >> 8) & 0xFF);
                data[p++] = (byte)((i) & 0xFF);
            }
        }
        public void WriteShortAt(int i, int p)
        {
            if (endian == Endianness.Little)
            {
                data[p++] = (byte)((i) & 0xFF);
                data[p++] = (byte)((i >> 8) & 0xFF);
            }
            else
            {
                data[p++] = (byte)((i >> 8) & 0xFF);
                data[p++] = (byte)((i) & 0xFF);
            }
        }

        public void Align(int i)
        {
            while ((data.Count % i) != 0)
                WriteByte(0);
        }

        public void Align(int i, int v)
        {
            while ((data.Count % i) != 0)
                WriteByte(v);
        }

        public void WriteFloat(float f)
        {
            WriteInt(SingleToInt32Bits(f));
        }

        public void WriteFloatAt(float f, int p)
        {
            WriteIntAt(SingleToInt32Bits(f), p);
        }

        //The return value is big endian representation
        public static int SingleToInt32Bits(float value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        }

        public void WriteHalfFloat(float f)
        {
            WriteShort(FileData.FromFloat(f));
        }

        public void WriteShort(int i)
        {
            if (endian == Endianness.Little)
            {
                data.Add((byte)((i) & 0xFF));
                data.Add((byte)((i >> 8) & 0xFF));
            }
            else
            {
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i) & 0xFF));
            }
        }

        public void WriteUShort(ushort i)
        {
            if (endian == Endianness.Little)
            {
                data.Add((byte)((i) & 0xFF));
                data.Add((byte)((i >> 8) & 0xFF));
            }
            else
            {
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i) & 0xFF));
            }
        }

        public void WriteByte(int i)
        {
            data.Add((byte)((i) & 0xFF));
        }

        public void WriteSByte(sbyte i)
        {
            data.Add((byte)((i) & 0xFF));
        }

        public void WriteChars(char[] c)
        {
            foreach (char ch in c)
                WriteByte(Convert.ToByte(ch));
        }

        public void WriteBytes(byte[] bytes)
        {
            foreach (byte b in bytes)
                WriteByte(b);
        }

        public void WriteFlag(bool b)
        {
            if (b)
                WriteByte(1);
            else
                WriteByte(0);
        }

        public int Pos()
        {
            return data.Count;
        }

        public void Save(String fname)
        {
            File.WriteAllBytes(fname, data.ToArray());
        }

        public class RelocOffset
        {
            public int value;
            public int position;
            public FileOutput output;
        }
        public List<RelocOffset> offsets = new List<RelocOffset>();
        public void WriteOffset(int i, FileOutput fo)
        {
            offsets.Add(new RelocOffset() { value = i, output = fo, position = data.Count });
            WriteInt(i);
        }
    }
}
