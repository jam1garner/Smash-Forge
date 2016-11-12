using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge
{
    public class NUT : FileBase
    {
        public class NUD_Texture
        {
            public List<byte[]> mipmaps = new List<byte[]>();
            public int id;
            public int width;
            public int height;
            public PixelInternalFormat type;
            public OpenTK.Graphics.OpenGL.PixelFormat utype;
            
            public override string ToString()
            {
                return id.ToString("x").ToUpper();
            }

            public int Size
            {
                get
                {
                    switch (type)
                    {
                        case PixelInternalFormat.CompressedRgbaS3tcDxt1Ext:
                            return (width * height / 2);
                        case PixelInternalFormat.CompressedRgbaS3tcDxt3Ext:
                            return (width * height);
                        case PixelInternalFormat.CompressedRgbaS3tcDxt5Ext:
                            return (width * height);
                        case PixelInternalFormat.Rgba:
                            return mipmaps[0].Length;
                        default:
                            return mipmaps[0].Length;
                    }
                }
            }

            public int getNutFormat()
            {
                switch (type)
                {
                    case PixelInternalFormat.CompressedRgbaS3tcDxt1Ext:
                        return 0;
                    case PixelInternalFormat.CompressedRgbaS3tcDxt3Ext:
                        return 1;
                    case PixelInternalFormat.CompressedRgbaS3tcDxt5Ext:
                        return 2;
                    case PixelInternalFormat.Rgba:
                        if (utype == OpenTK.Graphics.OpenGL.PixelFormat.Rgba)
                            return 14;
                        else
                            return 17;
                    case PixelInternalFormat.CompressedRedRgtc1:
                        return 21;
                    case PixelInternalFormat.CompressedRgRgtc2:
                        return 22;
                    default:
                        throw new NotImplementedException($"Unknown pixel format 0x{type:X}");
                }
            }

            public void setPixelFormatFromNutFormat (int typet)
            {
                switch (typet)
                {
                    case 0x0:
                        type = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                        break;
                    case 0x1:
                        type = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                        break;
                    case 0x2:
                        type = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                        break;
                    case 14:
                        type = PixelInternalFormat.Rgba;
                        utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                        break;
                    case 17:
                        type = PixelInternalFormat.Rgba;
                        utype = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
                        break;
                    case 21:
                        type = PixelInternalFormat.CompressedRedRgtc1;
                        break;
                    case 22:
                        type = PixelInternalFormat.CompressedRgRgtc2;
                        break;
                    default:
                        throw new NotImplementedException($"Unknown nut format {typet}");
                }
            }
        }

        public Dictionary<int,int> draw = new Dictionary<int, int>();
        public List<NUD_Texture> textures = new List<NUD_Texture>();

        public override Endianness Endian { get; set; }

        public NUT()
        {
        }

        public NUT (string filename)
        {
            Read(filename);
        }

        public override byte[] Rebuild()
        {
            FileOutput o = new FileOutput();

            o.writeInt(0x4E545033); // "NTP3"
            o.writeShort(0x0200);
            o.writeShort(textures.Count);
            o.writeInt(0);
            o.writeInt(0);

            int offset = textures.Count * 0x60;

            foreach (var texture in textures)
            {
                int size = 0;
                int headerSize = 0x60;

                foreach (var mip in texture.mipmaps)
                {
                    size += mip.Length;
                }

                // // headerSize 0x50 seems to crash with models
                //if (texture.mipmaps.Count == 1)
                //{
                //    headerSize = 0x50;
                //}

                o.writeInt(size + headerSize);
                o.writeInt(0x00);
                o.writeInt(size);
                o.writeShort(headerSize);
                o.writeShort(0);
                o.writeShort(texture.mipmaps.Count);
                o.writeShort(texture.getNutFormat());
                o.writeShort(texture.width);
                o.writeShort(texture.height);
                o.writeInt(0);
                o.writeInt(0);
                o.writeInt(offset);
                o.writeInt(0);
                o.writeInt(0);
                o.writeInt(0);

                if (headerSize > 0x50)
                {
                    foreach (var mip in texture.mipmaps)
                    {
                        o.writeInt(mip.Length);
                    }

                    // Sizes for missing mips.
                    for (int j = 0; j < 4 - texture.mipmaps.Count; j++)
                    {
                        o.writeInt(0);
                    }
                }

                o.writeInt(0x65587400); // "eXt\0"
                o.writeInt(0x20);
                o.writeInt(0x10);
                o.writeInt(0x00);
                o.writeInt(0x47494458); // "GIDX"
                o.writeInt(0x10);

                o.writeInt(texture.id);
                o.writeInt(0);

                offset += (size - headerSize);
            }

            foreach (var texture in textures)
            {
                foreach (var mip in texture.mipmaps)
                {
                    o.writeBytes(mip);
                }
            }

            return o.getBytes();
        }

        public override void Read(string filename)
        {
            FileData d = new FileData(filename);

            Endian = Endianness.Big;
            d.Endian = Endian;
            int magic = d.readInt();

            if (magic == 0x4E545033)
            {
                ReadNTP3(d);
            } else if (magic == 0x4E545755)
            {
                ReadNTWU(d);
            }
        }

        public void ReadNTP3(FileData d)
        {
            d.skip(0x2);
            int count = d.readShort();
            d.skip(0x10);

            int headerPtr = d.pos();
            int dataPtr = 0;

            for (int i = 0; i < count; i++)
            {
                NUD_Texture tex = new NUD_Texture();
                tex.type = PixelInternalFormat.Rgba32ui;

                d.seek(headerPtr);
                int totalSize = d.readInt();
                int headerSize = d.readShort();
                headerPtr += headerSize;

                int numMips = d.readInt();
                tex.setPixelFormatFromNutFormat(d.readShort());
                tex.width = d.readShort();
                tex.height = d.readShort();

                d.skip(8); // mipmaps and padding
                int dataOffset = d.readInt() + dataPtr + 0x10;
                d.skip(0x0C);

                int[] mipSizes = new int[numMips];

                if (headerSize == 0x50)
                {
                    mipSizes[0] = totalSize;
                }
                else
                {
                    for (int j = 0; j < numMips; j++)
                    {
                        mipSizes[j] = d.readInt();
                    }
                }

                // NOTE: I have no clue what these other header sizes are.
                // pls send help.
                if (headerSize > 0x60)
                {
                    d.skip(headerSize - 0x60);
                }

                d.skip(0x18);
                tex.id = d.readInt();

                for (int miplevel = 0; miplevel < numMips; miplevel++)
                {
                    tex.mipmaps.Add(d.getSection(dataOffset, mipSizes[miplevel]));
                    dataOffset += mipSizes[miplevel];
                }

                dataPtr += headerSize;

                textures.Add(tex);
            }

            foreach (var tex in textures)
            {
                if (!draw.ContainsKey(tex.id))
                {
                    draw.Add(tex.id, loadImage(tex));
                }
            }
        }

        public void ReadNTWU(FileData d)
        {
            d.skip(0x02);
            int count = d.readShort();

            d.skip(0x10);
            int headerPtr = d.pos();
            int dataPtr = 0;
            int gtxHeaderOffset = 0;

            for (int i = 0; i < count; i++) {
                NUD_Texture tex = new NUD_Texture();
                tex.type = PixelInternalFormat.Rgba32ui;

                d.seek(headerPtr);
                int totalSize = d.readInt();
                int headerSize = d.readShort();
                int numMips = d.readInt();
                tex.setPixelFormatFromNutFormat(d.readShort());
                tex.width = d.readShort();
                tex.height = d.readShort();

                d.skip(8); // mipmaps and padding
                int dataOffset = d.readInt() + dataPtr + 0x10;

                headerPtr += headerSize;
                dataPtr += headerSize;

                d.skip(0x04);
                if (i == 0)
                {
                    gtxHeaderOffset = d.readInt() + 0x10;
                } else
                {
                    gtxHeaderOffset += 0x80;
                    d.skip(0x04);
                }

                d.skip(0x04);
                d.skip(headerSize - 0x50);

                d.skip(0x18);
                tex.id = d.readInt();

                d.seek(gtxHeaderOffset);
                d.skip(0x04); // dim
                d.skip(0x04); // width
                d.skip(0x04); // height
                d.skip(0x04); // depth
                d.skip(0x04); // numMips
                int format = d.readInt();
                d.skip(0x04); // aa
                d.skip(0x04); // use
                int imageSize = d.readInt();
                d.skip(0x04); // imagePtr
                d.skip(0x04); // mipSize
                d.skip(0x04); // mipPtr
                int tileMode = d.readInt();
                int swizzle = d.readInt();
                d.skip(0x04); // alignment
                int pitch = d.readInt();

                for (int mipLevel = 0; mipLevel < numMips; mipLevel++)
                {
                    // Maybe this is the problem?
                    int mipSize = imageSize >> (mipLevel * 2);

                    if (mipLevel == 0)
                    {
                        tex.mipmaps.Add(GTX.swizzleBC(
                            d.getSection(dataOffset, mipSize),
                            (tex.width >> mipLevel),
                            (tex.height >> mipLevel),
                            format,
                            tileMode,
                            pitch,
                            swizzle
                        ));
                    }

                    dataOffset += mipSize;
                }

                textures.Add(tex);
            }

            foreach (var tex in textures)
            {
                if (!draw.ContainsKey(tex.id))
                {
                    draw.Add(tex.id, loadImage(tex));
                }
            }
        }

        public void Destroy()
        {
            foreach (var kv in draw)
            {
                GL.DeleteTexture(kv.Value);
            }
        }

        public override string ToString()
        {
            return "NUT";
        }

        //texture----------------------------------------------------------

        public static int loadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }

        public static int loadImage(NUD_Texture t)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);

            if (t.type == PixelInternalFormat.CompressedRgbaS3tcDxt1Ext
                || t.type == PixelInternalFormat.CompressedRgbaS3tcDxt3Ext
                || t.type == PixelInternalFormat.CompressedRgbaS3tcDxt5Ext)
            {
                GL.CompressedTexImage2D<byte>(TextureTarget.Texture2D, 0, t.type, 
                    t.width, t.height, 0, t.Size, t.mipmaps[0]);

                Debug.WriteLine(GL.GetError());
            }
            else
            {
                GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, t.type, t.width, t.height, 0,
                    t.utype, PixelType.UnsignedByte, t.mipmaps[0]);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }
    }
}
