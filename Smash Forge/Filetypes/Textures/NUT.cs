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
            public byte[] data;
            public List<byte[]> mipmaps = new List<byte[]>();
            public int id;
            public int width;
            public int height;
            public PixelInternalFormat type;
            public OpenTK.Graphics.OpenGL.PixelFormat utype;
        }

        public Dictionary<int,int> draw = new Dictionary<int, int>();
        public List<NUD_Texture> textures = new List<NUD_Texture>();

        public override Endianness Endian { get; set; }

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
                int baseSize = getImageSize(texture);
                int size = baseSize;
                int mipGenSize = 8;
                int sizeFormat = 0;
                int format = 0;

                switch (texture.type)
                {
                    case PixelInternalFormat.CompressedRgbaS3tcDxt1Ext:
                        format = 0;
                        break;
                    case PixelInternalFormat.CompressedRgbaS3tcDxt3Ext:
                        format = 1;
                        mipGenSize = 16;
                        sizeFormat = 1;
                        break;
                    case PixelInternalFormat.CompressedRgbaS3tcDxt5Ext:
                        format = 2;
                        mipGenSize = 16;
                        sizeFormat = 1;
                        break;
                    case PixelInternalFormat.Rgba:
                        mipGenSize = 4;
                        sizeFormat = 4;

                        if (texture.utype == OpenTK.Graphics.OpenGL.PixelFormat.Rgba)
                            format = 14;
                        else
                            format = 17;

                        break;
                    case PixelInternalFormat.CompressedRedRgtc1:
                        format = 21;
                        break;
                    case PixelInternalFormat.CompressedRgRgtc2:
                        format = 22;
                        break;
                }

                int w = texture.width / 2;
                int h = texture.height / 2;
                for (int j = 0; j < texture.mipmaps.Count; j++)
                {
                    if (sizeFormat == 0)
                        size += Math.Max((w * h) / 2, mipGenSize);
                    else
                        size += Math.Max(w * h * sizeFormat, mipGenSize);

                    w /= 2;
                    h /= 2;
                }

                o.writeInt(size + 0x60);
                o.writeInt(0x00);
                o.writeInt(size);
                o.writeShort(0x60);
                o.writeShort(0);
                o.writeShort(texture.mipmaps.Count + 1);
                o.writeShort(format);
                o.writeShort(texture.width);
                o.writeShort(texture.height);
                o.writeInt(0);
                o.writeInt(0);
                o.writeInt(offset);
                o.writeInt(0);
                o.writeInt(0);
                o.writeInt(0);

                o.writeInt(baseSize);

                w = texture.width / 2;
                h = texture.height / 2;
                for (int j = 0; j < texture.mipmaps.Count; j++)
                {
                    int mipSize;
                    if (sizeFormat == 0)
                        mipSize = Math.Max((w * h) / 2, mipGenSize);
                    else
                        mipSize = Math.Max(w * h * sizeFormat, mipGenSize);

                    w /= 2;
                    h /= 2;

                    o.writeInt(mipSize);
                }

                // Sizes for missing mips.
                for (int j = 0; j < 3 - texture.mipmaps.Count; j++)
                {
                    o.writeInt(0);
                }

                o.writeInt(0x65587400); // "eXt\0"
                o.writeInt(0x20);
                o.writeInt(0x10);
                o.writeInt(0x00);
                o.writeInt(0x47494458); // "GIDX"
                o.writeInt(0x10);

                o.writeInt(texture.id);
                o.writeInt(0);

                offset += size-0x60;
            }

            foreach (var texture in textures)
            {
                o.writeBytes(texture.data);

                foreach (var mip in texture.mipmaps)
                {
                    o.writeBytes(mip);
                }
            }

            return o.getBytes();
        }
        
        public override void Read (string filename)
        {
            FileData d = new FileData(filename);
            //if (type == SMASH)
            // TODO: Pokken uses LE
            Endian = Endianness.Big;
            d.Endian = Endian;
            int header = d.readInt();
            d.seek(0x6);
            int count = d.readShort();

            d.seek(0x10);

            int padfix = 0;
            int headerSize = 0;
            int offheader = 0;

            int[] filen = new int[count];

            for (int i = 0; i < count; i++) {
                NUD_Texture tex = new NUD_Texture();
                tex.type = PixelInternalFormat.Rgba32ui;

                padfix += headerSize;
                d.skip(0x08);
                int size = d.readInt();
                int DDSSize = size;
                int mip1Size = 0;
                int mip2Size = 0;
                int mip3Size = 0;
                headerSize = d.readShort();
                int mipCount = d.readInt();
                int typet = d.readShort();

                tex.width = d.readShort();
                tex.height = d.readShort();

                d.skip(8); // mipmaps and padding

                int offset1 = d.readInt() + 16;
                int offset2 = d.readInt() + 16;
                int offset3 = d.readInt() + 16;
                d.skip(4);
                if (i == 0) {
                    offheader = offset3;
                }

                if (headerSize == 0x90) {
                    DDSSize = d.readInt();
                    d.skip(0x3C);
                }
                if (headerSize == 0x80) {
                    DDSSize = d.readInt();
                    d.skip(44);
                }
                if (headerSize == 0x70) {
                    DDSSize = d.readInt();
                    d.skip(28);
                }
                if (headerSize == 0x60) {
                    DDSSize = d.readInt();
                    mip1Size = d.readInt();
                    mip2Size = d.readInt();
                    mip3Size = d.readInt();
                }

                d.skip(16);
                d.skip(4);
                d.skip(4);
                tex.id = d.readInt();
                d.skip(4);

                Bitmap image = new Bitmap(10, 10);

                int t = d.pos();
                d.seek(offheader);
                d.skip(4);
                int w = d.readInt();
                int h = d.readInt();
                d.skip(8);
                int format = d.readInt();
                d.skip(0x18);
                int tm = d.readInt();
                int swizzle = d.readInt();
                d.skip(4);
                int pitch = 0;

                if (d.pos() < d.size())
                    pitch = d.readInt();

                offheader += 0x80;
                d.seek(t);

                if (header == 0x4E545755)
                {
                    tex.data = GTX.swizzleBC(d.getSection(offset1 + padfix, DDSSize), w, h, format, tm, pitch, swizzle);

                    int off = offset1 + padfix + DDSSize;

                    // FIXME: NTWU can suck it.
                    //if (mipCount >= 2)
                    //{
                    //    tex.mipmaps.Add(GTX.swizzleBC(d.getSection(off, mip1Size), w/2, h/2, format, tm, pitch, swizzle));
                    //    off += mip1Size;
                    //}
                    //
                    //if (mipCount >= 3)
                    //{
                    //    tex.mipmaps.Add(GTX.swizzleBC(d.getSection(off, mip2Size), w/4, h/4, format, tm, pitch, swizzle));
                    //    off += mip2Size;
                    //}
                    //
                    //if (mipCount >= 4)
                    //{
                    //    tex.mipmaps.Add(GTX.swizzleBC(d.getSection(off, mip3Size), w / 8, h / 8, format, tm, pitch, swizzle));
                    //    off += mip3Size;
                    //}
                }
                else
                {
                    tex.data = d.getSection(offset1 + padfix, DDSSize);

                    int off = offset1 + padfix + DDSSize;

                    // FIXME: Ideally, the main texture data would be handled in the same way as mips.
                    // this also greatly simplifies the GL side of things.
                    if (mipCount >= 2)
                    {
                        tex.mipmaps.Add(d.getSection(off, mip1Size));
                        off += mip1Size;
                    }

                    if (mipCount >= 3)
                    {
                        tex.mipmaps.Add(d.getSection(off, mip2Size));
                        off += mip2Size;
                    }

                    if (mipCount >= 4)
                    {
                        tex.mipmaps.Add(d.getSection(off, mip3Size));
                        off += mip3Size;
                    }
                }

                switch (typet)
                {
                    case 0x0:
                        tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                        break;
                    case 0x1:
                        tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                        break;
                    case 0x2:
                        tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                        break;
                    case 14:
                        tex.type = PixelInternalFormat.Rgba;
                        tex.utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                        break;
                    case 17:
                        tex.type = PixelInternalFormat.Rgba;
                        tex.utype = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
                        break;
                    case 21:
                        tex.type = PixelInternalFormat.CompressedRedRgtc1;
                        break;
                    case 22:
                        tex.type = PixelInternalFormat.CompressedRgRgtc2;
                        break;
                    default:
                        Console.WriteLine("\t" + headerSize + " Type 0x" + typet);
                        break;
                }

                textures.Add(tex);
                filen[i] = tex.id;
            }

            for (int i = 0; i < filen.Length; i++)
            {
                if (!draw.ContainsKey(filen[i]))
                    draw.Add (filen[i], loadImage(textures[i]));
            }
        }

        public void Destroy(){
            List<int> keyList = new List<int>(draw.Keys);

            for (int i = 0; i < keyList.Count; i++)
            {
                GL.DeleteTexture(keyList[i]);
            }
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

        public static int getImageSize(NUD_Texture t)
        {
            switch (t.type)
            {
                case PixelInternalFormat.CompressedRgbaS3tcDxt1Ext:
                    return (t.width * t.height / 2);
                case PixelInternalFormat.CompressedRgbaS3tcDxt3Ext:
                    return (t.width * t.height);
                case PixelInternalFormat.CompressedRgbaS3tcDxt5Ext:
                    return (t.width * t.height);
                case PixelInternalFormat.Rgba:
                    return t.data.Length;
                default:
                    return t.data.Length;
            }
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
                    t.width, t.height, 0, getImageSize(t), t.data);

                Debug.WriteLine(GL.GetError());
            }
            else
            {
                GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, t.type, t.width, t.height, 0,
                    t.utype, PixelType.UnsignedByte, t.data);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }
    }
}

