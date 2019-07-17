using System;
using System.Collections.Generic;
using OpenTK;
using System.IO;

namespace SmashForge
{
    public class Texlist : FileBase
    {
        public enum AtlasFlag : int
        {
            None = 0x00000000,
            Dynamic = 0x01000000
        }

        public class Texture
        {
            public string name;

            public Vector2 topLeft;
            public Vector2 botRight;

            public short width;
            public short height;

            public short atlasId;
        }

        public List<Texture> textures = new List<Texture>();
        public List<AtlasFlag> atlases = new List<AtlasFlag>();

        public override Endianness Endian
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public Texlist()
        {
        }

        public Texlist(string filename)
        {
            Read(filename);
        }

        public override void Read(string filename)
        {
            FileData buf = new FileData(filename);
            buf.endian = Endianness.Little;

            buf.Seek(0x06);

            short numAtlases = buf.ReadShort();
            short numTextures = buf.ReadShort();
            short flagsOffset = buf.ReadShort();
            short entriesOffset = buf.ReadShort();
            short stringsOffset = buf.ReadShort();


            buf.Seek(flagsOffset);
            for (int i = 0; i < numAtlases; i++)
            {
                atlases.Add((AtlasFlag)buf.ReadInt());
            }

            buf.Seek(entriesOffset);
            for (int i = 0; i < numTextures; i++)
            {
                Texture entry = new Texture();
                int nameOffset = buf.ReadInt();
                int nameOffset2 = buf.ReadInt();

                // I have yet to see this.
                if (nameOffset != nameOffset2)
                {
                    throw new NotImplementedException("texlist name offsets don't match?");
                }

                buf.Seek(stringsOffset + nameOffset);
                entry.name = buf.ReadString();

                entry.topLeft = new Vector2(buf.ReadFloat(), buf.ReadFloat());
                entry.botRight = new Vector2(buf.ReadFloat(), buf.ReadFloat());

                entry.width = buf.ReadShort();
                entry.height = buf.ReadShort();
                entry.atlasId = buf.ReadShort();

                textures.Add(entry);

                buf.Skip(0x02); // Padding.
            }
        }

        public override byte[] Rebuild()
        {
            FileOutput buf = new FileOutput();
            buf.endian = Endianness.Little;

            var flagsOffset = 0x10;
            var entriesOffset = flagsOffset + (atlases.Count * 4);
            var stringsOffset = entriesOffset + (textures.Count * 0x20);

            buf.WriteInt(0x544C5354); // TLST
            buf.WriteShort(0); // idk
            buf.WriteShort((short)atlases.Count);
            buf.WriteShort((short)textures.Count);
            buf.WriteShort((short)flagsOffset);
            buf.WriteShort((short)entriesOffset);
            buf.WriteShort((short)stringsOffset);

            // flags
            foreach (var flag in atlases)
            {
                buf.WriteInt((int)flag);
            }

            // entries
            int namePtr = 0;
            foreach (var texture in textures)
            {
                buf.WriteInt(namePtr);
                buf.WriteInt(namePtr);
                namePtr += texture.name.Length + 1;

                buf.WriteFloat(texture.topLeft.X);
                buf.WriteFloat(texture.topLeft.Y);
                buf.WriteFloat(texture.botRight.X);
                buf.WriteFloat(texture.botRight.Y);

                buf.WriteShort(texture.width);
                buf.WriteShort(texture.height);
                buf.WriteShort(texture.atlasId);
                buf.WriteShort(0); // pad
            }

            //strings
            foreach (var texture in textures)
            {
                buf.WriteString(texture.name);
                buf.WriteByte(0);
            }

            buf.WriteByte(0);

            return buf.GetBytes();
        }
    }
}
