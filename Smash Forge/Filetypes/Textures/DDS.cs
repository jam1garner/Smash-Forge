using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge
{
    public class DDS
    {
        public enum DDSFormat
        {
            RGBA,
            DXT1,
            DXT3,
            DXT5,
            ATI1,
            ATI2
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

        [FlagsAttribute] public enum DDSD : uint
        {
            CAPS              = 0x00000001,
            HEIGHT            = 0x00000002,
            WIDTH             = 0x00000004,
            PITCH             = 0x00000008,
            PIXELFORMAT       = 0x00001000,
            MIPMAPCOUNT       = 0x00020000,
            LINEARSIZE        = 0x00080000,
            DEPTH             = 0x00800000
        }
        [FlagsAttribute] public enum DDPF : uint
        {
            ALPHAPIXELS       = 0x00000001,
            ALPHA             = 0x00000002,
            FOURCC            = 0x00000004,
            RGB               = 0x00000040,
            YUV               = 0x00000200,
            LUMINANCE         = 0x00020000,
        }
        [FlagsAttribute] public enum DDSCAPS : uint
        {
            COMPLEX           = 0x00000008,
            TEXTURE           = 0x00001000,
            MIPMAP            = 0x00400000,
        }
        [FlagsAttribute] public enum DDSCAPS2 : uint
        {
            CUBEMAP           = 0x00000200,
            CUBEMAP_POSITIVEX = 0x00000400 | CUBEMAP,
            CUBEMAP_NEGATIVEX = 0x00000800 | CUBEMAP,
            CUBEMAP_POSITIVEY = 0x00001000 | CUBEMAP,
            CUBEMAP_NEGATIVEY = 0x00002000 | CUBEMAP,
            CUBEMAP_POSITIVEZ = 0x00004000 | CUBEMAP,
            CUBEMAP_NEGATIVEZ = 0x00008000 | CUBEMAP,
            CUBEMAP_ALLFACES  = ( CUBEMAP_POSITIVEX | CUBEMAP_NEGATIVEX |
                                  CUBEMAP_POSITIVEY | CUBEMAP_NEGATIVEY |
                                  CUBEMAP_POSITIVEZ | CUBEMAP_NEGATIVEZ ),
            VOLUME            = 0x00200000
        }

        //Bytes per block (4x4 pixels) for block formats, bytes per pixel for non-block formats
        public static uint getFormatSize(uint fourCC)
        {
            switch (fourCC)
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

        public const uint magic = 0x20534444; //" SDD"
        public Header header;
        public class Header
        {
            public uint size = 0x7C;
            public uint flags = 0x00000000;
            public uint height = 0;
            public uint width = 0;
            public uint pitchOrLinearSize = 0;
            public uint depth = 0;
            public uint mipMapCount = 0;
            public uint[] reserved1 = new uint[11];
            public DDS_PixelFormat ddspf = new DDS_PixelFormat();
            public class DDS_PixelFormat
            {
                public uint size = 0x20;
                public uint flags = 0x00000000;
                public uint fourCC = 0x00000000;
                public uint RGBBitCount = 0;
                public uint RBitMask = 0x00000000;
                public uint GBitMask = 0x00000000;
                public uint BBitMask = 0x00000000;
                public uint ABitMask = 0x00000000;
            }
            public uint caps = 0;
            public uint caps2 = 0;
            public uint caps3 = 0;
            public uint caps4 = 0;
            public uint reserved2 = 0;
        }
        public byte[] bdata;

        public DDS()
        {
            header = new Header();
        }

        public DDS(FileData d)
        {
            d.Endian = System.IO.Endianness.Little;

            d.seek(0);
            if (d.readUInt() != magic)
            {
                MessageBox.Show("The file does not appear to be a valid DDS file.");
            }

            header = new Header();
            header.size = d.readUInt();
            header.flags = d.readUInt();
            header.height = d.readUInt();
            header.width = d.readUInt();
            header.pitchOrLinearSize = d.readUInt();
            header.depth = d.readUInt();
            header.mipMapCount = d.readUInt();
            header.reserved1 = new uint[11];
            for (int i = 0; i < 11; ++i)
                header.reserved1[i] = d.readUInt();

            header.ddspf.size = d.readUInt();
            header.ddspf.flags = d.readUInt();
            header.ddspf.fourCC = d.readUInt();
            header.ddspf.RGBBitCount = d.readUInt();
            header.ddspf.RBitMask = d.readUInt();
            header.ddspf.GBitMask = d.readUInt();
            header.ddspf.BBitMask = d.readUInt();
            header.ddspf.ABitMask = d.readUInt();

            header.caps = d.readUInt();
            header.caps2 = d.readUInt();
            header.caps3 = d.readUInt();
            header.caps4 = d.readUInt();
            header.reserved2 = d.readUInt();

            d.seek((int)(4 + header.size));
            bdata = d.read(d.size() - d.pos());
        }

        public DDS(NutTexture tex)
        {
            fromNUT_Texture(tex);
        }

        public void Save(string fname)
        {
            FileOutput f = new FileOutput();
            f.Endian = System.IO.Endianness.Little;

            f.writeUInt(magic);

            f.writeUInt(header.size);
            f.writeUInt(header.flags);
            f.writeUInt(header.height);
            f.writeUInt(header.width);
            f.writeUInt(header.pitchOrLinearSize);
            f.writeUInt(header.depth);
            f.writeUInt(header.mipMapCount);
            for (int i = 0; i < 11; ++i)
                f.writeUInt(header.reserved1[i]);

            f.writeUInt(header.ddspf.size);
            f.writeUInt(header.ddspf.flags);
            f.writeUInt(header.ddspf.fourCC);
            f.writeUInt(header.ddspf.RGBBitCount);
            f.writeUInt(header.ddspf.RBitMask);
            f.writeUInt(header.ddspf.GBitMask);
            f.writeUInt(header.ddspf.BBitMask);
            f.writeUInt(header.ddspf.ABitMask);

            f.writeUInt(header.caps);
            f.writeUInt(header.caps2);
            f.writeUInt(header.caps3);
            f.writeUInt(header.caps4);
            f.writeUInt(header.reserved2);

            f.writeBytes(bdata);

            f.save(fname);
        }

        public void fromNUT_Texture(NutTexture tex)
        {
            header = new Header();
            header.flags = (uint)(DDSD.CAPS | DDSD.HEIGHT | DDSD.WIDTH | DDSD.PIXELFORMAT | DDSD.MIPMAPCOUNT | DDSD.LINEARSIZE);
            header.width = (uint)tex.Width;
            header.height = (uint)tex.Height;
            header.pitchOrLinearSize = (uint)tex.Size;
            header.mipMapCount = (uint)tex.surfaces[0].mipmaps.Count;
            header.caps2 = tex.ddsCaps2;
            bool isCubemap = (header.caps2 & (uint)DDSCAPS2.CUBEMAP) == (uint)DDSCAPS2.CUBEMAP;
            header.caps = (uint)DDSCAPS.TEXTURE;
            if (header.mipMapCount > 1)
                header.caps |= (uint)(DDSCAPS.COMPLEX | DDSCAPS.MIPMAP);
            if (isCubemap)
                header.caps |= (uint)DDSCAPS.COMPLEX;

            switch (tex.type)
            {
                case PixelInternalFormat.CompressedRgbaS3tcDxt1Ext:
                    header.ddspf.fourCC = 0x31545844;
                    header.ddspf.flags = (uint)DDPF.FOURCC;
                    break;
                case PixelInternalFormat.CompressedRgbaS3tcDxt3Ext:
                    header.ddspf.fourCC = 0x33545844;
                    header.ddspf.flags = (uint)DDPF.FOURCC;
                    break;
                case PixelInternalFormat.CompressedRgbaS3tcDxt5Ext:
                    header.ddspf.fourCC = 0x35545844;
                    header.ddspf.flags = (uint)DDPF.FOURCC;
                    break;
                case PixelInternalFormat.CompressedRedRgtc1:
                    header.ddspf.fourCC = 0x31495441;
                    header.ddspf.flags = (uint)DDPF.FOURCC;
                    break;
                case PixelInternalFormat.CompressedRgRgtc2:
                    header.ddspf.fourCC = 0x32495441;
                    header.ddspf.flags = (uint)DDPF.FOURCC;
                    break;
                case PixelInternalFormat.Rgba:
                    header.ddspf.fourCC = 0x00000000;
                    if (tex.utype == OpenTK.Graphics.OpenGL.PixelFormat.Rgba)
                    {
                        header.ddspf.flags = (uint)(DDPF.RGB | DDPF.ALPHAPIXELS);
                        header.ddspf.RGBBitCount = 0x8 * 4;
                        header.ddspf.RBitMask = 0x000000FF;
                        header.ddspf.GBitMask = 0x0000FF00;
                        header.ddspf.BBitMask = 0x00FF0000;
                        header.ddspf.ABitMask = 0xFF000000;
                    }
                    break;
                default:
                    throw new NotImplementedException($"Unknown pixel format 0x{tex.type:X}");
            }

            List<byte> d = new List<byte>();
            foreach (byte[] b in tex.getAllMipmaps())
            {
                d.AddRange(b);
            }
            bdata = d.ToArray();
        }

        public NutTexture toNUT_Texture()
        {
            NutTexture tex = new NutTexture();
            tex.HASHID = 0x48415348;
            tex.Height = (int)header.height;
            tex.Width = (int)header.width;
            byte surfaceCount = 1;
            bool isCubemap = (header.caps2 & (uint)DDSCAPS2.CUBEMAP) == (uint)DDSCAPS2.CUBEMAP;
            if (isCubemap)
            {
                if ((header.caps2 & (uint)DDSCAPS2.CUBEMAP_ALLFACES) == (uint)DDSCAPS2.CUBEMAP_ALLFACES)
                    surfaceCount = 6;
                else
                    throw new NotImplementedException($"Unsupported cubemap face amount for texture. Six faces are required.");
            }

            bool isBlock = true;

            switch (header.ddspf.fourCC)
            {
                case 0x00000000: //RGBA
                    isBlock = false;
                    tex.type = PixelInternalFormat.Rgba;
                    tex.utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                    break;
                case 0x31545844: //DXT1
                    tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    break;
                case 0x33545844: //DXT3
                    tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                    break;
                case 0x35545844: //DXT5
                    tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    break;
                case 0x31495441: //ATI1
                case 0x55344342: //BC4U
                    tex.type = PixelInternalFormat.CompressedRedRgtc1;
                    break;
                case 0x32495441: //ATI2
                case 0x55354342: //BC5U
                    tex.type = PixelInternalFormat.CompressedRgRgtc2;
                    break;
                default:
                    MessageBox.Show("Unsupported DDS format - 0x" + header.ddspf.fourCC.ToString("x"));
                    break;
            }

            uint formatSize = getFormatSize(header.ddspf.fourCC);

            FileData d = new FileData(bdata);
            if (header.mipMapCount == 0)
                header.mipMapCount = 1;

            uint off = 0;
            for (byte i = 0; i < surfaceCount; ++i)
            {
                TextureSurface surface = new TextureSurface();
                uint w = header.width, h = header.height;
                for (int j = 0; j < header.mipMapCount; ++j)
                {
                    //If texture is DXT5 and isn't square, limit the mipmaps to an amount such that width and height are each always >= 4
                    if (tex.type == PixelInternalFormat.CompressedRgbaS3tcDxt5Ext && tex.Width != tex.Height && (w < 4 || h < 4))
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
                    surface.mipmaps.Add(d.getSection((int)off, (int)s));
                    off += s;
                }
                tex.surfaces.Add(surface);
            }

            return tex;
        }
       
        public Bitmap toBitmap()
        {
            byte[] pixels = new byte[header.width * header.height * 4];

            if (header.ddspf.fourCC == 0x31545844)
                decodeDXT1 (pixels, bdata, (int)header.width, (int)header.height);
            else
            if (header.ddspf.fourCC == 0x33545844)
                decodeDXT3 (pixels, bdata, (int)header.width, (int)header.height);
            else
            if (header.ddspf.fourCC == 0x35545844)
                decodeDXT5 (pixels, bdata, (int)header.width, (int)header.height);
            else
                Console.WriteLine ("Unknown DDS format " + header.ddspf.fourCC);

            Bitmap bmp = new Bitmap((int)header.width, (int)header.height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);  

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);


            bmp.UnlockBits(bmpData);
            return bmp;
        }


        public static Bitmap toBitmap(byte[] d, int width, int height, DDSFormat format)
        {
            byte[] pixels = new byte[width * height * 4];

            if (format == DDSFormat.DXT1)
                decodeDXT1 (pixels, d, width, height);
            if (format == DDSFormat.DXT3)
                decodeDXT3 (pixels, d, width, height);
            if (format == DDSFormat.DXT5)
                decodeDXT5 (pixels, d, width, height);

            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);  

            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),   
                ImageLockMode.WriteOnly, bmp.PixelFormat);

            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);


            bmp.UnlockBits(bmpData);
            return bmp;
        }


        // DECODING TOOLS--------------------------------


        public static void decodeDXT1(byte[] pixels, byte[] data, int width, int height){
            int x = 0, y = 0;
            int p = 0;

            while(true){
                // Color----------------------------------------------------------------

                byte[] block = new byte[8];
                int blockp = 0;
                for(int i = 0 ; i < 8 ; i ++)
                    block[i] = data[p++];

                int[] pal = new int[4];
                pal[0] = makeColor565(block[blockp++]&0xFF,block[blockp++]&0xFF);
                pal[1] = makeColor565(block[blockp++]&0xFF,block[blockp++]&0xFF);


                int r = (2 * getRed(pal[0]) + getRed(pal[1]))/3;
                int g = (2 * getGreen(pal[0]) + getGreen(pal[1]))/3;
                int b = (2 * getBlue(pal[0]) + getBlue(pal[1]))/3;

                pal[2] = (0xFF<<24)|(r<<16)|(g<<8)|(b);

                r = (2 * getRed(pal[1]) + getRed(pal[0]))/3;
                g = (2 * getGreen(pal[1]) + getGreen(pal[0]))/3;
                b = (2 * getBlue(pal[1]) + getBlue(pal[0]))/3;

                pal[3] = (0xFF<<24)|(r<<16)|(g<<8)|(b);


                int[] index = new int[16];
                int indexp = 0;
                for(int i = 0 ; i < 4 ; i++){
                    int by = block[blockp++]&0xFF;
                    index[indexp++] = (by&0x03);
                    index[indexp++] = (by&0x0C)>>2;
                    index[indexp++] = (by&0x30)>>4;
                    index[indexp++] = (by&0xC0)>>6;
                }

                // end----------------------------------------------------------------

                indexp = 0;

                for(int h = 0 ; h < 4 ; h++){
                    for(int w = 0; w < 4 ; w++){
                        int color = (0xFF << 24) | (pal [index [(w) + (h) * 4]] & 0x00FFFFFF);
                        pixels [((w + x) + (h + y) * width) * 4 + 3] = 0xFF;
                        pixels [((w + x) + (h + y) * width) * 4 + 2] = (byte)((color >> 16) & 0xFF);
                        pixels [((w + x) + (h + y) * width) * 4 + 1] = (byte)((color >> 8) & 0xFF);
                        pixels [((w + x) + (h + y) * width) * 4 + 0] = (byte)((color) & 0xFF);
                    }
                }

                // end positioning------------------------------------------------------------------

                x += 4;
                if(x >= width){
                    x = 0;
                    y += 4;
                }
                if(y >= height)
                    break;
            }
        }

        //INCOMPLETE - But toBitmap is never called anyway *shrug*
        public static void decodeDXT3(byte[] pixels, byte[] data, int width, int height){
            int x = 0, y = 0;
            int p = 0;

            while(true){
                // Color----------------------------------------------------------------

                byte[] block = new byte[16];
                int blockp = 8;
                for(int i = 0 ; i < 16 ; i ++)
                    block[i] = data[p++];

                int[] pal = new int[4];
                pal[0] = makeColor565(block[blockp++]&0xFF,block[blockp++]&0xFF);
                pal[1] = makeColor565(block[blockp++]&0xFF,block[blockp++]&0xFF);


                int r = (2 * getRed(pal[0]) + getRed(pal[1]))/3;
                int g = (2 * getGreen(pal[0]) + getGreen(pal[1]))/3;
                int b = (2 * getBlue(pal[0]) + getBlue(pal[1]))/3;

                pal[2] = (0xFF<<24)|(r<<16)|(g<<8)|(b);

                r = (2 * getRed(pal[1]) + getRed(pal[0]))/3;
                g = (2 * getGreen(pal[1]) + getGreen(pal[0]))/3;
                b = (2 * getBlue(pal[1]) + getBlue(pal[0]))/3;

                pal[3] = (0xFF<<24)|(r<<16)|(g<<8)|(b);


                int[] index = new int[16];
                int indexp = 0;
                for(int i = 0 ; i < 4 ; i++){
                    int by = block[blockp++]&0xFF;
                    index[indexp++] = (by&0x03);
                    index[indexp++] = (by&0x0C)>>2;
                    index[indexp++] = (by&0x30)>>4;
                    index[indexp++] = (by&0xC0)>>6;
                }

                // end----------------------------------------------------------------

                indexp = 0;

                for(int h = 0 ; h < 4 ; h++){
                    for(int w = 0; w < 4 ; w++){
                        int color = (0xFF << 24) | (pal [index [(w) + (h) * 4]] & 0x00FFFFFF);
                        pixels [((w + x) + (h + y) * width) * 4 + 3] = 0xFF;
                        pixels [((w + x) + (h + y) * width) * 4 + 2] = (byte)((color >> 16) & 0xFF);
                        pixels [((w + x) + (h + y) * width) * 4 + 1] = (byte)((color >> 8) & 0xFF);
                        pixels [((w + x) + (h + y) * width) * 4 + 0] = (byte)((color) & 0xFF);
                    }
                }

                // end positioning------------------------------------------------------------------

                x += 4;
                if(x >= width){
                    x = 0;
                    y += 4;
                }
                if(y >= height)
                    break;
            }
        }

        public static void decodeDXT5(byte[] pixels, byte[] data, int width, int height){
            int x = 0, y = 0;
            int p = 0;

            while(true){

                //Alpha------------------------------------------------------------------
                byte[] block = new byte[8];
                int blockp = 0;

                for(int i = 0 ; i < 8 ; i ++)
                    block[i] = data[p++];

                int a1 = block[blockp++]&0xFF;
                int a2 = block[blockp++]&0xFF;

                int aWord1 = (block[blockp++]&0xFF)|((block[blockp++]&0xFF)<<8)|((block[blockp++]&0xFF)<<16);
                int aWord2 = (block[blockp++]&0xFF)|((block[blockp++]&0xFF)<<8)|((block[blockp++]&0xFF)<<16);

                int[] a = new int[16];

                for(int i = 0 ; i < 16 ; i++){
                    if(i < 8){
                        int code = (int) (aWord1 & 0x7);
                        aWord1 >>= 3;
                        a[i] = getDXTAWord(code, a1, a2)&0xFF;
                    } else{
                        int code = (int) (aWord2 & 0x7);
                        aWord2 >>= 3;
                        a[i] = getDXTAWord(code, a1, a2)&0xFF;
                    } 
                }


                // Color----------------------------------------------------------------

                block = new byte[8];
                blockp = 0;
                for(int i = 0 ; i < 8 ; i ++)
                    block[i] = data[p++];

                int[] pal = new int[4];
                pal[0] = makeColor565(block[blockp++]&0xFF,block[blockp++]&0xFF);
                pal[1] = makeColor565(block[blockp++]&0xFF,block[blockp++]&0xFF);


                int r = (2 * getRed(pal[0]) + getRed(pal[1]))/3;
                int g = (2 * getGreen(pal[0]) + getGreen(pal[1]))/3;
                int b = (2 * getBlue(pal[0]) + getBlue(pal[1]))/3;

                pal[2] = (0xFF<<24)|(r<<16)|(g<<8)|(b);

                r = (2 * getRed(pal[1]) + getRed(pal[0]))/3;
                g = (2 * getGreen(pal[1]) + getGreen(pal[0]))/3;
                b = (2 * getBlue(pal[1]) + getBlue(pal[0]))/3;

                pal[3] = (0xFF<<24)|(r<<16)|(g<<8)|(b);


                int[] index = new int[16];
                int indexp = 0;
                for(int i = 0 ; i < 4 ; i++){
                    int by = block[blockp++]&0xFF;
                    index[indexp++] = (by&0x03);
                    index[indexp++] = (by&0x0C)>>2;
                    index[indexp++] = (by&0x30)>>4;
                    index[indexp++] = (by&0xC0)>>6;
                }

                // end----------------------------------------------------------------

                indexp = 0;

                for(int h = 0 ; h < 4 ; h++){
                    for(int w = 0; w < 4 ; w++){
                        int color = (a [(w) + (h) * 4] << 24) | (pal [index [(w) + (h) * 4]] & 0x00FFFFFF);
                        pixels [((w + x) + (h + y) * width) * 4 + 3] = (byte)getAlpha(color);
                        pixels [((w + x) + (h + y) * width) * 4 + 2] = (byte)((color >> 16) & 0xFF);
                        pixels [((w + x) + (h + y) * width) * 4 + 1] = (byte)((color >> 8) & 0xFF);
                        pixels [((w + x) + (h + y) * width) * 4 + 0] = (byte)((color) & 0xFF);
                    }
                }

                // end positioning------------------------------------------------------------------

                x += 4;
                if(x >= width){
                    x = 0;
                    y += 4;
                }
                if(y >= height)
                    break;
            }
        }


        private static int getDXTAWord(int code, int alpha0, int alpha1){

            if(alpha0 > alpha1){
                switch(code){
                case 0: return alpha0;
                case 1: return alpha1;
                case 2: return (6*alpha0 + 1*alpha1)/7;
                case 3: return (5*alpha0 + 2*alpha1)/7;
                case 4: return (4*alpha0 + 3*alpha1)/7;
                case 5: return (3*alpha0 + 4*alpha1)/7;
                case 6: return (2*alpha0 + 5*alpha1)/7;
                case 7: return (1*alpha0 + 6*alpha1)/7;
                }
            } else{
                switch(code){
                case 0: return alpha0;
                case 1: return alpha1;
                case 2: return (4*alpha0 + 1*alpha1)/5;
                case 3: return (3*alpha0 + 2*alpha1)/5;
                case 4: return (2*alpha0 + 3*alpha1)/5;
                case 5: return (1*alpha0 + 4*alpha1)/5;
                case 6: return 0;
                case 7: return 255;
                }
            }

            return 0;
        }

        public static int getAlpha(int c){
            return (c>>24)>>0xFF;
        }

        public static int getRed(int c){
            return (c&0x00FF0000)>>16;
        }

        public static int getGreen(int c){
            return (c&0x0000FF00)>>8;
        }

        public static int getBlue(int c){
            return (c&0x000000FF);
        }

        private static int makeColor565(int b1, int b2) {

            int bt = (b2 << 8) | b1;

            int a = 255;
            int r = (bt >> 11) & 0x1F;
            int g = (bt >> 5) & 0x3F;
            int b = (bt) & 0x1F;

            r = (r << 3) | (r >> 2);
            g = (g << 2) | (g >> 4);
            b = (b << 3) | (b >> 2);

            return ((int) a << 24) | ((int) r << 16) | ((int) g << 8) | (int) b;

        }

    }
}

