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

        struct SFATNode
        {
            public int nameHash;
            public int nameTableOffset;
            public int fileDataOffset;
            public int endFileDataOffset;
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
    }
}
