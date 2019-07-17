using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace SmashForge
{
    public class Dds
    {
        public enum DdsFormat
        {
            Rgba,
            Dxt1,
            Dxt3,
            Dxt5,
            Ati1,
            Ati2
        }

        public enum CubemapFace
        {
            PosX,
            NegX,
            PosY,
            NegY,
            PosZ,
            NegZ
        }

        [Flags]
        public enum Ddsd : uint
        {
            Caps = 0x00000001,
            Height = 0x00000002,
            Width = 0x00000004,
            Pitch = 0x00000008,
            Pixelformat = 0x00001000,
            Mipmapcount = 0x00020000,
            Linearsize = 0x00080000,
            Depth = 0x00800000
        }
        [Flags]
        public enum Ddpf : uint
        {
            Alphapixels = 0x00000001,
            Alpha = 0x00000002,
            Fourcc = 0x00000004,
            Rgb = 0x00000040,
            Yuv = 0x00000200,
            Luminance = 0x00020000,
        }
        [Flags]
        public enum Ddscaps : uint
        {
            Complex = 0x00000008,
            Texture = 0x00001000,
            Mipmap = 0x00400000,
        }
        [Flags]
        public enum Ddscaps2 : uint
        {
            Cubemap = 0x00000200,
            CubemapPositivex = 0x00000400 | Cubemap,
            CubemapNegativex = 0x00000800 | Cubemap,
            CubemapPositivey = 0x00001000 | Cubemap,
            CubemapNegativey = 0x00002000 | Cubemap,
            CubemapPositivez = 0x00004000 | Cubemap,
            CubemapNegativez = 0x00008000 | Cubemap,
            CubemapAllfaces = (CubemapPositivex | CubemapNegativex |
                                  CubemapPositivey | CubemapNegativey |
                                  CubemapPositivez | CubemapNegativez),
            Volume = 0x00200000
        }

        //Bytes per block (4x4 pixels) for block formats, bytes per pixel for non-block formats
        public static uint GetFormatSize(uint fourCc)
        {
            switch (fourCc)
            {
                case 0x00000000: //RGBA
                    return 0x4;
                case 0x31545844: //DXT1
                    return 0x8;
                case 0x33545844: //DXT3
                    return 0x10;
                case 0x35545844: //DXT5
                    return 0x10;
                case 0x31495441: //ATI1
                case 0x55344342: //BC4U
                    return 0x8;
                case 0x32495441: //ATI2
                case 0x55354342: //BC5U
                    return 0x10;
                default:
                    return 0;
            }
        }

        public const uint Magic = 0x20534444; //" SDD"
        public Header header;
        public class Header
        {
            public uint size = 0x7C;
            public uint flags = 0x00000000;
            public uint height = 0;
            public uint width = 0;
            public uint pitchOrLinearSize = 0;
            public uint depth = 0;
            public uint mipmapCount = 0;
            public uint[] reserved1 = new uint[11];
            public DdsPixelFormat ddspf = new DdsPixelFormat();
            public class DdsPixelFormat
            {
                public uint size = 0x20;
                public uint flags = 0x00000000;
                public uint fourCc = 0x00000000;
                public uint rgbBitCount = 0;
                public uint rBitMask = 0x00000000;
                public uint gBitMask = 0x00000000;
                public uint bBitMask = 0x00000000;
                public uint aBitMask = 0x00000000;
            }
            public uint caps = 0;
            public uint caps2 = 0;
            public uint caps3 = 0;
            public uint caps4 = 0;
            public uint reserved2 = 0;
        }
        public byte[] bdata;

        public Dds()
        {
            header = new Header();
        }

        public Dds(FileData d)
        {
            d.endian = System.IO.Endianness.Little;

            d.Seek(0);
            if (d.ReadUInt() != Magic)
            {
                MessageBox.Show("The file does not appear to be a valid DDS file.");
            }

            header = new Header();
            header.size = d.ReadUInt();
            header.flags = d.ReadUInt();
            header.height = d.ReadUInt();
            header.width = d.ReadUInt();
            header.pitchOrLinearSize = d.ReadUInt();
            header.depth = d.ReadUInt();
            header.mipmapCount = d.ReadUInt();
            header.reserved1 = new uint[11];
            for (int i = 0; i < 11; ++i)
                header.reserved1[i] = d.ReadUInt();

            header.ddspf.size = d.ReadUInt();
            header.ddspf.flags = d.ReadUInt();
            header.ddspf.fourCc = d.ReadUInt();
            header.ddspf.rgbBitCount = d.ReadUInt();
            header.ddspf.rBitMask = d.ReadUInt();
            header.ddspf.gBitMask = d.ReadUInt();
            header.ddspf.bBitMask = d.ReadUInt();
            header.ddspf.aBitMask = d.ReadUInt();

            header.caps = d.ReadUInt();
            header.caps2 = d.ReadUInt();
            header.caps3 = d.ReadUInt();
            header.caps4 = d.ReadUInt();
            header.reserved2 = d.ReadUInt();

            d.Seek((int)(4 + header.size));
            bdata = d.Read(d.Size() - d.Pos());
        }

        public Dds(NutTexture tex)
        {
            FromNutTexture(tex);
        }

        public void Save(string fname)
        {
            FileOutput f = new FileOutput();
            f.endian = System.IO.Endianness.Little;

            f.WriteUInt(Magic);

            f.WriteUInt(header.size);
            f.WriteUInt(header.flags);
            f.WriteUInt(header.height);
            f.WriteUInt(header.width);
            f.WriteUInt(header.pitchOrLinearSize);
            f.WriteUInt(header.depth);
            f.WriteUInt(header.mipmapCount);
            for (int i = 0; i < 11; ++i)
                f.WriteUInt(header.reserved1[i]);

            f.WriteUInt(header.ddspf.size);
            f.WriteUInt(header.ddspf.flags);
            f.WriteUInt(header.ddspf.fourCc);
            f.WriteUInt(header.ddspf.rgbBitCount);
            f.WriteUInt(header.ddspf.rBitMask);
            f.WriteUInt(header.ddspf.gBitMask);
            f.WriteUInt(header.ddspf.bBitMask);
            f.WriteUInt(header.ddspf.aBitMask);

            f.WriteUInt(header.caps);
            f.WriteUInt(header.caps2);
            f.WriteUInt(header.caps3);
            f.WriteUInt(header.caps4);
            f.WriteUInt(header.reserved2);

            f.WriteBytes(bdata);

            f.Save(fname);
        }

        public void FromNutTexture(NutTexture tex)
        {
            header = new Header();
            header.flags = (uint)(Ddsd.Caps | Ddsd.Height | Ddsd.Width | Ddsd.Pixelformat | Ddsd.Mipmapcount | Ddsd.Linearsize);
            header.width = (uint)tex.Width;
            header.height = (uint)tex.Height;
            header.pitchOrLinearSize = (uint)tex.ImageSize;
            header.mipmapCount = (uint)tex.surfaces[0].mipmaps.Count;
            header.caps2 = tex.DdsCaps2;
            bool isCubemap = (header.caps2 & (uint)Ddscaps2.Cubemap) == (uint)Ddscaps2.Cubemap;
            header.caps = (uint)Ddscaps.Texture;
            if (header.mipmapCount > 1)
                header.caps |= (uint)(Ddscaps.Complex | Ddscaps.Mipmap);
            if (isCubemap)
                header.caps |= (uint)Ddscaps.Complex;

            switch (tex.pixelInternalFormat)
            {
                case PixelInternalFormat.CompressedRgbaS3tcDxt1Ext:
                    header.ddspf.fourCc = 0x31545844;
                    header.ddspf.flags = (uint)Ddpf.Fourcc;
                    break;
                case PixelInternalFormat.CompressedRgbaS3tcDxt3Ext:
                    header.ddspf.fourCc = 0x33545844;
                    header.ddspf.flags = (uint)Ddpf.Fourcc;
                    break;
                case PixelInternalFormat.CompressedRgbaS3tcDxt5Ext:
                    header.ddspf.fourCc = 0x35545844;
                    header.ddspf.flags = (uint)Ddpf.Fourcc;
                    break;
                case PixelInternalFormat.CompressedRedRgtc1:
                    header.ddspf.fourCc = 0x31495441;
                    header.ddspf.flags = (uint)Ddpf.Fourcc;
                    break;
                case PixelInternalFormat.CompressedRgRgtc2:
                    header.ddspf.fourCc = 0x32495441;
                    header.ddspf.flags = (uint)Ddpf.Fourcc;
                    break;
                case PixelInternalFormat.Rgba:
                    header.ddspf.fourCc = 0x00000000;
                    if (tex.pixelFormat == OpenTK.Graphics.OpenGL.PixelFormat.Rgba)
                    {
                        header.ddspf.flags = (uint)(Ddpf.Rgb | Ddpf.Alphapixels);
                        header.ddspf.rgbBitCount = 0x8 * 4;
                        header.ddspf.rBitMask = 0x000000FF;
                        header.ddspf.gBitMask = 0x0000FF00;
                        header.ddspf.bBitMask = 0x00FF0000;
                        header.ddspf.aBitMask = 0xFF000000;
                    }
                    break;
                default:
                    throw new NotImplementedException($"Unknown pixel format 0x{tex.pixelInternalFormat:X}");
            }

            List<byte> d = new List<byte>();
            foreach (byte[] b in tex.GetAllMipmaps())
            {
                d.AddRange(b);
            }
            bdata = d.ToArray();
        }

        public NutTexture ToNutTexture()
        {
            NutTexture tex = new NutTexture();
            tex.isDds = true;
            tex.HashId = 0x48415348;
            tex.Height = (int)header.height;
            tex.Width = (int)header.width;
            byte surfaceCount = 1;
            bool isCubemap = (header.caps2 & (uint)Ddscaps2.Cubemap) == (uint)Ddscaps2.Cubemap;
            if (isCubemap)
            {
                if ((header.caps2 & (uint)Ddscaps2.CubemapAllfaces) == (uint)Ddscaps2.CubemapAllfaces)
                    surfaceCount = 6;
                else
                    throw new NotImplementedException($"Unsupported cubemap face amount for texture. Six faces are required.");
            }

            bool isBlock = true;

            switch (header.ddspf.fourCc)
            {
                case 0x00000000: //RGBA
                    isBlock = false;
                    tex.pixelInternalFormat = PixelInternalFormat.Rgba;
                    tex.pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                    break;
                case 0x31545844: //DXT1
                    tex.pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    break;
                case 0x33545844: //DXT3
                    tex.pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                    break;
                case 0x35545844: //DXT5
                    tex.pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    break;
                case 0x31495441: //ATI1
                case 0x55344342: //BC4U
                    tex.pixelInternalFormat = PixelInternalFormat.CompressedRedRgtc1;
                    break;
                case 0x32495441: //ATI2
                case 0x55354342: //BC5U
                    tex.pixelInternalFormat = PixelInternalFormat.CompressedRgRgtc2;
                    break;
                default:
                    MessageBox.Show("Unsupported DDS format - 0x" + header.ddspf.fourCc.ToString("x"));
                    break;
            }

            uint formatSize = GetFormatSize(header.ddspf.fourCc);

            FileData d = new FileData(bdata);
            if (header.mipmapCount == 0)
                header.mipmapCount = 1;

            uint off = 0;
            for (byte i = 0; i < surfaceCount; ++i)
            {
                TextureSurface surface = new TextureSurface();
                uint w = header.width, h = header.height;
                for (int j = 0; j < header.mipmapCount; ++j)
                {
                    //If texture is DXT5 and isn't square, limit the mipmaps to an amount such that width and height are each always >= 4
                    if (tex.pixelInternalFormat == PixelInternalFormat.CompressedRgbaS3tcDxt5Ext && tex.Width != tex.Height && (w < 4 || h < 4))
                        break;

                    uint s = (w * h); //Total pixels
                    if (isBlock)
                    {
                        s = (uint)(s * ((float)formatSize / 0x10)); //Bytes per 16 pixels
                        if (s < formatSize) //Make sure it's at least one block
                            s = formatSize;
                    }
                    else
                    {
                        s = (uint)(s * (formatSize)); //Bytes per pixel
                    }

                    w /= 2;
                    h /= 2;
                    surface.mipmaps.Add(d.GetSection((int)off, (int)s));
                    off += s;
                }
                tex.surfaces.Add(surface);
            }

            return tex;
        }

        //For bfres / bntx
        public BRTI.BRTI_Texture ToBrtiTexture()
        {
            // TODO: Check these casts.
            BRTI.BRTI_Texture tex = new BRTI.BRTI_Texture();
            tex.height = (int)header.height;
            tex.width = (int)header.width;
            float size = 1;
            int mips = (int)header.mipmapCount;
            /*if (mips > header.mipmapCount)
            {
                mips = header.mipmapCount;
                MessageBox.Show("Possible texture error: Only one mipmap");
            }*/

            switch (header.ddspf.fourCc)
            {
                case 0x0:
                    size = 4f;
                    tex.pixelInternalFormat = PixelInternalFormat.SrgbAlpha;
                    tex.pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                    break;
                case 0x31545844:
                    size = 1 / 2f;
                    tex.pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    break;
                case 0x35545844:
                    size = 1f;
                    tex.pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    break;
                case 0x32495441:
                    size = 1 / 2f;
                    tex.pixelInternalFormat = PixelInternalFormat.CompressedRedRgtc1;
                    break;
                case 0x31495441:
                    size = 1f;
                    tex.pixelInternalFormat = PixelInternalFormat.CompressedRgRgtc2;
                    break;
                default:
                    MessageBox.Show("Unsupported DDS format - 0x" + header.ddspf.fourCc.ToString("x"));
                    break;
            }

            // now for mipmap data...
            FileData d = new FileData(bdata);
            int off = 0, w = (int)header.width, h = (int)header.height;

            if (header.mipmapCount == 0) header.mipmapCount = 1;
            for (int i = 0; i < header.mipmapCount; i++)
            {
                int s = (int)((w * h) * size);
                if (s < 0x8) s = 0x8;
                //Console.WriteLine(off.ToString("x") + " " + s.ToString("x"));
                w /= 2;
                h /= 2;
                tex.mipmaps.Add(d.GetSection(off, s));
                off += s;
            }
            Console.WriteLine(off.ToString("x"));

            return tex;
        }

        public Bitmap ToBitmap()
        {
            byte[] pixels = new byte[header.width * header.height * 4];

            if (header.ddspf.fourCc == 0x31545844)
                DecodeDxt1(pixels, bdata, (int)header.width, (int)header.height);
            else
            if (header.ddspf.fourCc == 0x33545844)
                DecodeDxt3(pixels, bdata, (int)header.width, (int)header.height);
            else
            if (header.ddspf.fourCc == 0x35545844)
                DecodeDxt5(pixels, bdata, (int)header.width, (int)header.height);
            else
                Console.WriteLine("Unknown DDS format " + header.ddspf.fourCc);

            Bitmap bmp = new Bitmap((int)header.width, (int)header.height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);


            bmp.UnlockBits(bmpData);
            return bmp;
        }


        public static Bitmap ToBitmap(byte[] d, int width, int height, DdsFormat format)
        {
            byte[] pixels = new byte[width * height * 4];

            if (format == DdsFormat.Dxt1)
                DecodeDxt1(pixels, d, width, height);
            if (format == DdsFormat.Dxt3)
                DecodeDxt3(pixels, d, width, height);
            if (format == DdsFormat.Dxt5)
                DecodeDxt5(pixels, d, width, height);

            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly, bmp.PixelFormat);

            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);


            bmp.UnlockBits(bmpData);
            return bmp;
        }


        // DECODING TOOLS--------------------------------


        public static void DecodeDxt1(byte[] pixels, byte[] data, int width, int height)
        {
            int x = 0, y = 0;
            int p = 0;

            while (true)
            {
                // Color----------------------------------------------------------------

                byte[] block = new byte[8];
                int blockp = 0;
                for (int i = 0; i < 8; i++)
                    block[i] = data[p++];

                int[] pal = new int[4];
                pal[0] = MakeColor565(block[blockp++] & 0xFF, block[blockp++] & 0xFF);
                pal[1] = MakeColor565(block[blockp++] & 0xFF, block[blockp++] & 0xFF);


                int r = (2 * GetRed(pal[0]) + GetRed(pal[1])) / 3;
                int g = (2 * GetGreen(pal[0]) + GetGreen(pal[1])) / 3;
                int b = (2 * GetBlue(pal[0]) + GetBlue(pal[1])) / 3;

                pal[2] = (0xFF << 24) | (r << 16) | (g << 8) | (b);

                r = (2 * GetRed(pal[1]) + GetRed(pal[0])) / 3;
                g = (2 * GetGreen(pal[1]) + GetGreen(pal[0])) / 3;
                b = (2 * GetBlue(pal[1]) + GetBlue(pal[0])) / 3;

                pal[3] = (0xFF << 24) | (r << 16) | (g << 8) | (b);


                int[] index = new int[16];
                int indexp = 0;
                for (int i = 0; i < 4; i++)
                {
                    int by = block[blockp++] & 0xFF;
                    index[indexp++] = (by & 0x03);
                    index[indexp++] = (by & 0x0C) >> 2;
                    index[indexp++] = (by & 0x30) >> 4;
                    index[indexp++] = (by & 0xC0) >> 6;
                }

                // end----------------------------------------------------------------

                indexp = 0;

                for (int h = 0; h < 4; h++)
                {
                    for (int w = 0; w < 4; w++)
                    {
                        int color = (0xFF << 24) | (pal[index[(w) + (h) * 4]] & 0x00FFFFFF);
                        pixels[((w + x) + (h + y) * width) * 4 + 3] = 0xFF;
                        pixels[((w + x) + (h + y) * width) * 4 + 2] = (byte)((color >> 16) & 0xFF);
                        pixels[((w + x) + (h + y) * width) * 4 + 1] = (byte)((color >> 8) & 0xFF);
                        pixels[((w + x) + (h + y) * width) * 4 + 0] = (byte)((color) & 0xFF);
                    }
                }

                // end positioning------------------------------------------------------------------

                x += 4;
                if (x >= width)
                {
                    x = 0;
                    y += 4;
                }
                if (y >= height)
                    break;
            }
        }

        //INCOMPLETE - But toBitmap is never called anyway *shrug*
        public static void DecodeDxt3(byte[] pixels, byte[] data, int width, int height)
        {
            int x = 0, y = 0;
            int p = 0;

            while (true)
            {
                // Color----------------------------------------------------------------

                byte[] block = new byte[16];
                int blockp = 8;
                for (int i = 0; i < 16; i++)
                    block[i] = data[p++];

                int[] pal = new int[4];
                pal[0] = MakeColor565(block[blockp++] & 0xFF, block[blockp++] & 0xFF);
                pal[1] = MakeColor565(block[blockp++] & 0xFF, block[blockp++] & 0xFF);


                int r = (2 * GetRed(pal[0]) + GetRed(pal[1])) / 3;
                int g = (2 * GetGreen(pal[0]) + GetGreen(pal[1])) / 3;
                int b = (2 * GetBlue(pal[0]) + GetBlue(pal[1])) / 3;

                pal[2] = (0xFF << 24) | (r << 16) | (g << 8) | (b);

                r = (2 * GetRed(pal[1]) + GetRed(pal[0])) / 3;
                g = (2 * GetGreen(pal[1]) + GetGreen(pal[0])) / 3;
                b = (2 * GetBlue(pal[1]) + GetBlue(pal[0])) / 3;

                pal[3] = (0xFF << 24) | (r << 16) | (g << 8) | (b);


                int[] index = new int[16];
                int indexp = 0;
                for (int i = 0; i < 4; i++)
                {
                    int by = block[blockp++] & 0xFF;
                    index[indexp++] = (by & 0x03);
                    index[indexp++] = (by & 0x0C) >> 2;
                    index[indexp++] = (by & 0x30) >> 4;
                    index[indexp++] = (by & 0xC0) >> 6;
                }

                // end----------------------------------------------------------------

                indexp = 0;

                for (int h = 0; h < 4; h++)
                {
                    for (int w = 0; w < 4; w++)
                    {
                        int color = (0xFF << 24) | (pal[index[(w) + (h) * 4]] & 0x00FFFFFF);
                        pixels[((w + x) + (h + y) * width) * 4 + 3] = 0xFF;
                        pixels[((w + x) + (h + y) * width) * 4 + 2] = (byte)((color >> 16) & 0xFF);
                        pixels[((w + x) + (h + y) * width) * 4 + 1] = (byte)((color >> 8) & 0xFF);
                        pixels[((w + x) + (h + y) * width) * 4 + 0] = (byte)((color) & 0xFF);
                    }
                }

                // end positioning------------------------------------------------------------------

                x += 4;
                if (x >= width)
                {
                    x = 0;
                    y += 4;
                }
                if (y >= height)
                    break;
            }
        }

        public static void DecodeDxt5(byte[] pixels, byte[] data, int width, int height)
        {
            int x = 0, y = 0;
            int p = 0;

            while (true)
            {

                //Alpha------------------------------------------------------------------
                byte[] block = new byte[8];
                int blockp = 0;

                for (int i = 0; i < 8; i++)
                    block[i] = data[p++];

                int a1 = block[blockp++] & 0xFF;
                int a2 = block[blockp++] & 0xFF;

                int aWord1 = (block[blockp++] & 0xFF) | ((block[blockp++] & 0xFF) << 8) | ((block[blockp++] & 0xFF) << 16);
                int aWord2 = (block[blockp++] & 0xFF) | ((block[blockp++] & 0xFF) << 8) | ((block[blockp++] & 0xFF) << 16);

                int[] a = new int[16];

                for (int i = 0; i < 16; i++)
                {
                    if (i < 8)
                    {
                        int code = (int)(aWord1 & 0x7);
                        aWord1 >>= 3;
                        a[i] = GetDxtaWord(code, a1, a2) & 0xFF;
                    }
                    else
                    {
                        int code = (int)(aWord2 & 0x7);
                        aWord2 >>= 3;
                        a[i] = GetDxtaWord(code, a1, a2) & 0xFF;
                    }
                }


                // Color----------------------------------------------------------------

                block = new byte[8];
                blockp = 0;
                for (int i = 0; i < 8; i++)
                    block[i] = data[p++];

                int[] pal = new int[4];
                pal[0] = MakeColor565(block[blockp++] & 0xFF, block[blockp++] & 0xFF);
                pal[1] = MakeColor565(block[blockp++] & 0xFF, block[blockp++] & 0xFF);


                int r = (2 * GetRed(pal[0]) + GetRed(pal[1])) / 3;
                int g = (2 * GetGreen(pal[0]) + GetGreen(pal[1])) / 3;
                int b = (2 * GetBlue(pal[0]) + GetBlue(pal[1])) / 3;

                pal[2] = (0xFF << 24) | (r << 16) | (g << 8) | (b);

                r = (2 * GetRed(pal[1]) + GetRed(pal[0])) / 3;
                g = (2 * GetGreen(pal[1]) + GetGreen(pal[0])) / 3;
                b = (2 * GetBlue(pal[1]) + GetBlue(pal[0])) / 3;

                pal[3] = (0xFF << 24) | (r << 16) | (g << 8) | (b);


                int[] index = new int[16];
                int indexp = 0;
                for (int i = 0; i < 4; i++)
                {
                    int by = block[blockp++] & 0xFF;
                    index[indexp++] = (by & 0x03);
                    index[indexp++] = (by & 0x0C) >> 2;
                    index[indexp++] = (by & 0x30) >> 4;
                    index[indexp++] = (by & 0xC0) >> 6;
                }

                // end----------------------------------------------------------------

                indexp = 0;

                for (int h = 0; h < 4; h++)
                {
                    for (int w = 0; w < 4; w++)
                    {
                        int color = (a[(w) + (h) * 4] << 24) | (pal[index[(w) + (h) * 4]] & 0x00FFFFFF);
                        pixels[((w + x) + (h + y) * width) * 4 + 3] = (byte)GetAlpha(color);
                        pixels[((w + x) + (h + y) * width) * 4 + 2] = (byte)((color >> 16) & 0xFF);
                        pixels[((w + x) + (h + y) * width) * 4 + 1] = (byte)((color >> 8) & 0xFF);
                        pixels[((w + x) + (h + y) * width) * 4 + 0] = (byte)((color) & 0xFF);
                    }
                }

                // end positioning------------------------------------------------------------------

                x += 4;
                if (x >= width)
                {
                    x = 0;
                    y += 4;
                }
                if (y >= height)
                    break;
            }
        }


        private static int GetDxtaWord(int code, int alpha0, int alpha1)
        {

            if (alpha0 > alpha1)
            {
                switch (code)
                {
                    case 0: return alpha0;
                    case 1: return alpha1;
                    case 2: return (6 * alpha0 + 1 * alpha1) / 7;
                    case 3: return (5 * alpha0 + 2 * alpha1) / 7;
                    case 4: return (4 * alpha0 + 3 * alpha1) / 7;
                    case 5: return (3 * alpha0 + 4 * alpha1) / 7;
                    case 6: return (2 * alpha0 + 5 * alpha1) / 7;
                    case 7: return (1 * alpha0 + 6 * alpha1) / 7;
                }
            }
            else
            {
                switch (code)
                {
                    case 0: return alpha0;
                    case 1: return alpha1;
                    case 2: return (4 * alpha0 + 1 * alpha1) / 5;
                    case 3: return (3 * alpha0 + 2 * alpha1) / 5;
                    case 4: return (2 * alpha0 + 3 * alpha1) / 5;
                    case 5: return (1 * alpha0 + 4 * alpha1) / 5;
                    case 6: return 0;
                    case 7: return 255;
                }
            }

            return 0;
        }

        public static int GetAlpha(int c)
        {
            return (c >> 24) >> 0xFF;
        }

        public static int GetRed(int c)
        {
            return (c & 0x00FF0000) >> 16;
        }

        public static int GetGreen(int c)
        {
            return (c & 0x0000FF00) >> 8;
        }

        public static int GetBlue(int c)
        {
            return (c & 0x000000FF);
        }

        private static int MakeColor565(int b1, int b2)
        {

            int bt = (b2 << 8) | b1;

            int a = 255;
            int r = (bt >> 11) & 0x1F;
            int g = (bt >> 5) & 0x3F;
            int b = (bt) & 0x1F;

            r = (r << 3) | (r >> 2);
            g = (g << 2) | (g >> 4);
            b = (b << 3) | (b >> 2);

            return ((int)a << 24) | ((int)r << 16) | ((int)g << 8) | (int)b;

        }
    }
}
