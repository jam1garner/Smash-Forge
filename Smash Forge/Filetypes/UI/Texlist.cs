using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VBN_Editor
{
    public class Texlist : FileBase
    {
        public struct Texture
        {
            public enum Flags : int
            {
                None = 0x00000000,
                Dynamic = 0x01000000
            }

            public string name;

            public Flags flags;

            public Vector2 topLeft;
            public Vector2 botRight;

            public short width;
            public short height;

            public short atlasId;
        }

        public List<Texture> textures { get; set; }
        public int numAtlases;

        public override System.IO.Endianness Endian { get; set; }

        public Texlist()
        {
            textures = new List<Texture>();
            numAtlases = 0;
        }

        public Texlist(string filename) : this()
        {
            Read(filename);
        }

        public override void Read(string filename)
        {
            FileData f = new FileData(filename);
            // TODO: 3ds 
            f.Endian = System.IO.Endianness.Big;

            f.seek(0x06);

            numAtlases = f.readUShort();
            int numTextures = f.readUShort();
            int flagsOffset = f.readUShort();
            int entriesOffset = f.readUShort();
            int stringsOffset = f.readUShort();

            List<Texture.Flags> flags = new List<Texture.Flags>();

            f.seek(flagsOffset);
            for (int i = 0; i < numTextures; i++)
            {
                flags.Add((Texture.Flags)f.readInt());
            }

            f.seek(entriesOffset);
            for (int i = 0; i < numTextures; i++)
            {
                Texture entry = new Texture();
                int nameOffset = f.readInt();
                int nameOffset2 = f.readInt();

                // I have yet to see this.
                if (nameOffset != nameOffset2)
                {
                    throw new NotImplementedException("texlist name offsets don't match?");
                }

                entry.name = f.readString(stringsOffset + nameOffset, -1);
                entry.flags = flags[i];

                entry.topLeft = new Vector2(f.readFloat(), f.readFloat());
                entry.botRight = new Vector2(f.readFloat(), f.readFloat());

                entry.width = (short)f.readShort();
                entry.height = (short)f.readShort();
                entry.atlasId = (short)f.readShort();

                textures.Add(entry);

                f.skip(0x02); // Padding.
            }
        }

        public override byte[] Rebuild()
        {
            throw new NotImplementedException();
        }
    }
}
