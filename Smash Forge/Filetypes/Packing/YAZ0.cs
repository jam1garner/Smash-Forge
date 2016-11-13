using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smash_Forge
{
    class YAZ0
    {
        public static byte[] Decompress(byte[] data)
        {
            FileData f = new FileData(data);
            FileOutput output = new FileOutput();

            f.Endian = System.IO.Endianness.Big;
            f.seek(4);
            int uncompressedSize = f.readInt();
            f.seek(0x10);

            byte[] src = f.read(data.Length - 0x10);
            byte[] dst = new byte[uncompressedSize];

            int srcPlace = 0, dstPlace = 0; //current read/write positions

            uint validBitCount = 0; //number of valid bits left in "code" byte
            byte currCodeByte = 0;
            while (dstPlace < uncompressedSize)
            {
                //read new "code" byte if the current one is used up
                if (validBitCount == 0)
                {
                    currCodeByte = src[srcPlace];
                    ++srcPlace;
                    validBitCount = 8;
                }

                if ((currCodeByte & 0x80) != 0)
                {
                    //straight copy
                    dst[dstPlace] = src[srcPlace];
                    dstPlace++;
                    srcPlace++;
                }
                else
                {
                    //RLE part
                    byte byte1 = src[srcPlace];
                    byte byte2 = src[srcPlace + 1];
                    srcPlace += 2;

                    uint dist = (uint)(((byte1 & 0xF) << 8) | byte2);
                    uint copySource = (uint)(dstPlace - (dist + 1));

                    uint numBytes = (uint)(byte1 >> 4);
                    if (numBytes == 0)
                    {
                        numBytes = (uint)(src[srcPlace] + 0x12);
                        srcPlace++;
                    }
                    else
                        numBytes += 2;

                    //copy run
                    for (int i = 0; i < numBytes; ++i)
                    {
                        dst[dstPlace] = dst[copySource];
                        copySource++;
                        dstPlace++;
                    }
                }

                //use next bit from "code" byte
                currCodeByte <<= 1;
                validBitCount -= 1;
            }

            return dst;
        }

        public static byte[] LazyCompress(byte[] data)
        {
            FileOutput f = new FileOutput();
            f.Endian = System.IO.Endianness.Little;
            f.writeString("Yaz0");
            byte w1 = (byte)(data.Length & 0xFF);
            byte w2 = (byte)((data.Length >> 8) & 0xFF);
            byte w3 = (byte)((data.Length >> 16) & 0xFF);
            byte w4 = (byte)(data.Length >> 24);
            f.writeInt((w1 << 24) | (w2 << 16) | (w3 << 8) | w4);
            f.writeHex("0000000000000000");
            int pos = 0, posInChunk = 0;
            while (pos < data.Length)
            {
                posInChunk = 0;
                f.writeByte(0xFF);
                while(pos + posInChunk < data.Length && posInChunk < 8)
                {
                    f.writeByte(data[pos + posInChunk]);
                    posInChunk++;
                }
                pos += posInChunk;
            }

            return f.getBytes();
        }
    }
}
