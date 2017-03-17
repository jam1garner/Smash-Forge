using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Smash_Forge
{
    public class Pixel
    {
        public Pixel()
        {

        }

        public static Bitmap fromBGRA(FileData d, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            int[] pixels = new int[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = (d.readByte() << 16) | (d.readByte() << 8) | (d.readByte()) | (d.readByte() << 24);
            }

            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public static Bitmap fromRGBA(FileData d, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            int[] pixels = new int[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = (d.readByte()) | (d.readByte() << 8) | (d.readByte() << 16) | (d.readByte() << 24);
            }

            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }



        // transformations




        // 3ds stuff

        public static Bitmap decodeETC(byte[] bytes, int width, int height)
        {
            int[] pixels = new int[width * height];

            int p = 0;
            int i, j;

            for (i = 0; i < height; i += 8)
            {
                for (j = 0; j < width; j += 8)
                {
                    int x, y;

                    if (p + 8 > bytes.Length)
                        break;

                    byte[] temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);


                    int[] pix = decodeETCBlock(bytesToLong(temp), null);

                    int pi = 0;
                    for (x = i; x < i + 4; x++)
                        for (y = j; y < j + 4; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;

                    if (p + 8 > bytes.Length)
                        break;

                    temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);

                    pix = decodeETCBlock(bytesToLong(temp), null);

                    pi = 0;
                    for (x = i; x < i + 4; x++)
                        for (y = j + 4; y < j + 8; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;

                    if (p + 8 > bytes.Length)
                        break;

                    temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);

                    pix = decodeETCBlock(bytesToLong(temp), null);

                    pi = 0;
                    for (x = i + 4; x < i + 8; x++)
                        for (y = j; y < j + 4; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;

                    if (p + 8 > bytes.Length)
                        break;

                    temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);

                    pix = decodeETCBlock(bytesToLong(temp), null);

                    pi = 0;
                    for (x = i + 4; x < i + 8; x++)
                        for (y = j + 4; y < j + 8; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;


                }
            }

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public static Bitmap decodeAlpha(byte[] bytes, int width, int height)
        {
            int[] pixels = new int[width * height];

            int p = 0;
            int i, j;

            for (i = 0; i < height; i += 8)
            {
                for (j = 0; j < width; j += 8)
                {
                    int x, y;

                    if (p + 16 > bytes.Length)
                        break;

                    byte[] alpha = new byte[8];
                    Buffer.BlockCopy(bytes, p, alpha, 0, 8);
                    p += 8;

                    byte[] temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);

                    int[] pix = decodeETCBlock(bytesToLong(temp), alpha);

                    int pi = 0;
                    for (x = i; x < i + 4; x++)
                        for (y = j; y < j + 4; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;

                    if (p + 16 > bytes.Length)
                        break;

                    alpha = new byte[8];
                    Buffer.BlockCopy(bytes, p, alpha, 0, 8);
                    p += 8;

                    temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);

                    pix = decodeETCBlock(bytesToLong(temp), alpha);

                    pi = 0;
                    for (x = i; x < i + 4; x++)
                        for (y = j + 4; y < j + 8; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;

                    if (p + 16 > bytes.Length)
                        break;

                    alpha = new byte[8];
                    Buffer.BlockCopy(bytes, p, alpha, 0, 8);
                    p += 8;

                    temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);

                    pix = decodeETCBlock(bytesToLong(temp), alpha);

                    pi = 0;
                    for (x = i + 4; x < i + 8; x++)
                        for (y = j; y < j + 4; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;

                    if (p + 16 > bytes.Length)
                        break;

                    alpha = new byte[8];
                    Buffer.BlockCopy(bytes, p, alpha, 0, 8);
                    p += 8;

                    temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);

                    pix = decodeETCBlock(bytesToLong(temp), alpha);

                    pi = 0;
                    for (x = i + 4; x < i + 8; x++)
                        for (y = j + 4; y < j + 8; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;


                }
            }
            
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private static long bytesToLong(byte[] b)
        {
            long result = 0;
            for (int i = 7; i >= 0; i--)
            {
                result <<= 8;
                result |= (long)(b[i] & 0xFF);
            }
            return result;
        }

        public static Color[] DecodeETC1(ulong bl)
        {
            int[] b = decodeETCBlock((long)bl, null);

            Color[] c = new Color[b.Length];

            for (int i = 0; i < b.Length; i++)
                c[i] = Color.FromArgb((b[i] >> 24)&0xFF, (b[i] >> 16) & 0xFF, (b[i] >> 8) & 0xFF, (b[i]) & 0xFF);

            return c;
        }

        private static int[] decodeETCBlock(long block, byte[] alpha)
        {

            /*
             * If flipbit=0, the block is divided into two 2x4 and 4x2 otherwise
             * Diffbit is for layouts
             */

            bool hasAlpha = alpha != null;

            int[,] aChannel = new int[4, 4];

            if (hasAlpha)
            {
                int ai = 0;
                for (int w = 0; w < 4; w++)
                    for (int h = 0; h < 4; h++)
                    {
                        aChannel[w, h++] = (alpha[ai] & 0x0F) * 17;
                        aChannel[w, h] = ((alpha[ai++] & 0xF0) >> 4) * 17;
                    }
            }

            int[,] codeWord = {{-8,-2,2,8},{-17,-5,5,17},{-29,-9,9,29},{-42,-13,13,42}
                            ,{-60,-18,18,60},{-80,-24,24,80},{-106,-33,33,106},{-183,-47,47,183}};

            bool flipbit = extendBits(block, 32, 1) == 1;
            bool diffbit = extendBits(block, 33, 1) == 1;

            //Note, extend 2 extends 3 but without double word
            int codeWord1 = extendBits(block, 37, 2);
            int codeWord2 = extendBits(block, 34, 2);

            /*
             *  In the 'individual' mode (diffbit = 0), the base color for
             *  subblock 1 is derived from the codewords R1 (bit 63-60), G1 (bit 55-52) and B1 (bit 47-44)
             */

            int r1, r2, g1, g2, b1, b2;

            if (diffbit == false)
            {

                r1 = extendBits(block, 60, 4);
                r2 = extendBits(block, 56, 4);
                g1 = extendBits(block, 52, 4);
                g2 = extendBits(block, 48, 4);
                b1 = extendBits(block, 44, 4);
                b2 = extendBits(block, 40, 4);

            }
            else
            {
                r1 = extendBits(block, 59, 5);
                g1 = extendBits(block, 51, 5);
                b1 = extendBits(block, 43, 5);

                r2 = extendBits(block, 59, 6) + extendBits(block, 56, 3);
                g2 = extendBits(block, 51, 6) + extendBits(block, 48, 3);
                b2 = extendBits(block, 43, 6) + extendBits(block, 40, 3);

                r2 = (r2 << 3) | (r2 >> 2);
                g2 = (g2 << 3) | (g2 >> 2);
                b2 = (b2 << 3) | (b2 >> 2);

            }

            //Now for making the pixels

            int[] difc = new int[16];

            int cw = 0;
            for (int w = 0; w < 4; w++)
            {
                for (int h = 0; h < 4; h++)
                {
                    switch ((extendBits(block, cw, 1)) | (extendBits(block, cw + 16, 1) << 1))
                    {
                        case 3:
                            difc[w + h * 4] = 0;
                            break;
                        case 2:
                            difc[w + h * 4] = 1;
                            break;
                        case 0:
                            difc[w + h * 4] = 2;
                            break;
                        case 1:
                            difc[w + h * 4] = 3;
                            break;
                    }

                    cw++;
                }
            }

            int[] pixels = new int[4 * 4];

            cw = 0;
            if (flipbit)
            {
                for (int h = 0; h < 4; h++)
                {
                    for (int w = 0; w < 4; w++)
                    {
                        if (h < 2)
                        {
                            int dif = codeWord[codeWord1, difc[h * 4 + w]];
                            int a = hasAlpha ? aChannel[w, h] : 255;
                            pixels[h * 4 + w] = (a << 24) | (clamp(r1 + dif) << 16) | (clamp(g1 + dif) << 8) | clamp(b1 + dif);
                        }
                        else
                        {
                            int dif = codeWord[codeWord2, difc[h * 4 + w]];
                            int a = hasAlpha ? aChannel[w, h] : 255;
                            pixels[h * 4 + w] = (a << 24) | (clamp(r2 + dif) << 16) | (clamp(g2 + dif) << 8) | clamp(b2 + dif);
                        }
                        cw++;
                    }
                }
            }
            else
            {
                for (int h = 0; h < 4; h++)
                {
                    for (int w = 0; w < 2; w++)
                    {
                        int dif = codeWord[codeWord1, difc[h * 4 + w]];
                        int a = hasAlpha ? aChannel[w, h] : 255;
                        pixels[h * 4 + w] = (a << 24) | (clamp(r1 + dif) << 16) | (clamp(g1 + dif) << 8) | clamp(b1 + dif);
                        cw++;
                    }
                }
                for (int h = 0; h < 4; h++)
                {
                    for (int w = 2; w < 4; w++)
                    {
                        int dif = codeWord[codeWord2, difc[h * 4 + w]];
                        int a = hasAlpha ? aChannel[w, h] : 255;
                        pixels[h * 4 + w] = (a << 24) | (clamp(r2 + dif) << 16) | (clamp(g2 + dif) << 8) | clamp(b2 + dif);
                        cw++;
                    }
                }
            }

            return pixels;

        }

        private static int clamp(int n)
        {
            if (n > 255) n = 255;
            if (n < 0) n = 0;
            return n;
        }

        private static int extendBits(long n, int p, int amt)
        {
            int num;
            switch (amt)
            {
                case 1:
                    return (int)((n >> p) & 0x1);
                case 2:
                    num = (int)((n >> p) & 0x7);
                    return num;
                case 3:
                    int[] complement = { 0, 1, 2, 3, -4, -3, -2, -1 };
                    num = (int)((n >> p) & 0x7);
                    return complement[num];
                case 4:
                    num = (int)((n >> p) & 0xF);
                    return (num << 4) | (num);
                case 5:
                    num = (int)((n >> p) & 0x1F);
                    return (num << 3) | (num >> 2);
                case 6:
                    num = (int)((n >> p) & 0x1F);
                    return num & 0x1F;
                default:
                    return 0;
            }

        }


        // to etc

        public static byte[] encodeETC(Bitmap b)
        {
            int width = b.Width;
            int height = b.Height;
            int[] pixels = new int[width * height];
            
            int i, j;

            FileOutput o = new FileOutput();
            o.Endian = System.IO.Endianness.Little;

            for (i = 0; i < height; i += 8)
            {
                for (j = 0; j < width; j += 8)
                {
                    int x, y;

                    Color[] temp = new Color[16];
                    int pi = 0;
                    for (x = i; x < i + 4; x++)
                        for (y = j; y < j + 4; y++)
                            temp[pi++] = b.GetPixel(y, x);

                    ulong g = GenETC1(temp);
                    o.writeInt((int)(g & 0xFFFFFFFF));
                    o.writeInt((int)((g>>32)&0xFFFFFFFF));


                    temp = new Color[16];
                    pi = 0;
                    for (x = i; x < i + 4; x++)
                        for (y = j + 4; y < j + 8; y++)
                            temp[pi++] = b.GetPixel(y, x);

                    g = GenETC1(temp);
                    o.writeInt((int)(g & 0xFFFFFFFF));
                    o.writeInt((int)((g >> 32) & 0xFFFFFFFF));


                    temp = new Color[16];
                    pi = 0;
                    for (x = i + 4; x < i + 8; x++)
                        for (y = j; y < j + 4; y++)
                            temp[pi++] = b.GetPixel(y, x);

                    g = GenETC1(temp);
                    o.writeInt((int)(g & 0xFFFFFFFF));
                    o.writeInt((int)((g >> 32) & 0xFFFFFFFF));


                    temp = new Color[16];
                    pi = 0;
                    for (x = i + 4; x < i + 8; x++)
                        for (y = j + 4; y < j + 8; y++)
                            temp[pi++] = b.GetPixel(y, x);

                    g = GenETC1(temp);
                    o.writeInt((int)(g & 0xFFFFFFFF));
                    o.writeInt((int)((g >> 32) & 0xFFFFFFFF));
                }
            }
            
            return o.getBytes();
        }


        public static byte[] encodeETCa4(Bitmap b)
        {
            int width = b.Width;
            int height = b.Height;
            int[] pixels = new int[width * height];

            int i, j;

            FileOutput o = new FileOutput();
            o.Endian = System.IO.Endianness.Little;

            for (i = 0; i < height; i += 8)
            {
                for (j = 0; j < width; j += 8)
                {
                    int x, y;

                    Color[] temp = new Color[16];
                    int pi = 0;
                    for (x = i; x < i + 4; x++)
                        for (y = j; y < j + 4; y++)
                            temp[pi++] = b.GetPixel(y, x);

                    for (int ax = 0; ax < 4; ax++)
                        for (int ay = 0; ay < 4; ay++)
                        {
                            int a = (temp[ax + ay * 4].A >> 4);
                            ay++;
                            a |= (temp[ax + ay * 4].A >> 4) << 4;
                            o.writeByte(a);
                        }

                    ulong g = GenETC1(temp);
                    o.writeInt((int)(g & 0xFFFFFFFF));
                    o.writeInt((int)((g >> 32) & 0xFFFFFFFF));


                    temp = new Color[16];
                    pi = 0;
                    for (x = i; x < i + 4; x++)
                        for (y = j + 4; y < j + 8; y++)
                            temp[pi++] = b.GetPixel(y, x);

                    for (int ax = 0; ax < 4; ax++)
                        for (int ay = 0; ay < 4; ay++)
                        {
                            int a = (temp[ax + ay * 4].A >> 4);
                            ay++;
                            a |= (temp[ax + ay * 4].A >> 4) << 4;
                            o.writeByte(a);
                        }

                    g = GenETC1(temp);
                    o.writeInt((int)(g & 0xFFFFFFFF));
                    o.writeInt((int)((g >> 32) & 0xFFFFFFFF));


                    temp = new Color[16];
                    pi = 0;
                    for (x = i + 4; x < i + 8; x++)
                        for (y = j; y < j + 4; y++)
                            temp[pi++] = b.GetPixel(y, x);

                    for (int ax = 0; ax < 4; ax++)
                        for (int ay = 0; ay < 4; ay++)
                        {
                            int a = (temp[ax + ay * 4].A >> 4);
                            ay++;
                            a |= (temp[ax + ay * 4].A >> 4) << 4;
                            o.writeByte(a);
                        }

                    g = GenETC1(temp);
                    o.writeInt((int)(g & 0xFFFFFFFF));
                    o.writeInt((int)((g >> 32) & 0xFFFFFFFF));


                    temp = new Color[16];
                    pi = 0;
                    for (x = i + 4; x < i + 8; x++)
                        for (y = j + 4; y < j + 8; y++)
                            temp[pi++] = b.GetPixel(y, x);

                    for (int ax = 0; ax < 4; ax++)
                        for (int ay = 0; ay < 4; ay++)
                        {
                            int a = (temp[ax + ay * 4].A >> 4);
                            ay++;
                            a |= (temp[ax + ay * 4].A >> 4) << 4;
                            o.writeByte(a);
                        }

                    g = GenETC1(temp);
                    o.writeInt((int)(g & 0xFFFFFFFF));
                    o.writeInt((int)((g >> 32) & 0xFFFFFFFF));
                }
            }

            return o.getBytes();
        }


        // Copied from EveryFileExplorer by Gericom
        // https://github.com/Gericom/EveryFileExplorer/blob/master/LibEveryFileExplorer/GFX/ETC1.cs

        public static ulong GenETC1(Color[] Colors)
        {
            ulong Horizontal = GenHorizontal(Colors);
            ulong Vertical = GenVertical(Colors);
            int HorizontalScore = GetScore(Colors, DecodeETC1(Horizontal));
            int VerticalScore = GetScore(Colors, DecodeETC1(Vertical));
            return (HorizontalScore < VerticalScore) ? Horizontal : Vertical;
        }

        private static ulong GenHorizontal(Color[] Colors)
        {
            ulong data = 0;
            SetFlipMode(ref data, false);
            //Left
            Color[] Left = GetLeftColors(Colors);
            Color basec1;
            int mod = GenModifier(out basec1, Left);
            SetTable1(ref data, mod);
            GenPixDiff(ref data, Left, basec1, mod, 0, 2, 0, 4);
            //Right
            Color[] Right = GetRightColors(Colors);
            Color basec2;
            mod = GenModifier(out basec2, Right);
            SetTable2(ref data, mod);
            GenPixDiff(ref data, Right, basec2, mod, 2, 4, 0, 4);
            SetBaseColors(ref data, basec1, basec2);
            return data;
        }

        private static ulong GenVertical(Color[] Colors)
        {
            ulong data = 0;
            SetFlipMode(ref data, true);
            //Top
            Color[] Top = GetTopColors(Colors);
            Color basec1;
            int mod = GenModifier(out basec1, Top);
            SetTable1(ref data, mod);
            GenPixDiff(ref data, Top, basec1, mod, 0, 4, 0, 2);
            //Bottom
            Color[] Bottom = GetBottomColors(Colors);
            Color basec2;
            mod = GenModifier(out basec2, Bottom);
            SetTable2(ref data, mod);
            GenPixDiff(ref data, Bottom, basec2, mod, 0, 4, 2, 4);
            SetBaseColors(ref data, basec1, basec2);
            return data;
        }


        private static int GenModifier(out Color BaseColor, Color[] Pixels)
        {
            Color Max = Color.White;
            Color Min = Color.Black;
            int MinY = int.MaxValue;
            int MaxY = int.MinValue;
            for (int i = 0; i < 8; i++)
            {
                if (Pixels[i].A == 0) continue;
                int Y = (Pixels[i].R + Pixels[i].G + Pixels[i].B) / 3;
                if (Y > MaxY)
                {
                    MaxY = Y;
                    Max = Pixels[i];
                }
                if (Y < MinY)
                {
                    MinY = Y;
                    Min = Pixels[i];
                }
            }

            int DiffMean = ((Max.R - Min.R) + (Max.G - Min.G) + (Max.B - Min.B)) / 3;
            
            int ModDiff = int.MaxValue;
            int Modifier = -1;
            int Mode = -1;

            for (int i = 0; i < 8; i++)
            {
                int SS = ETC1Modifiers[i, 0] * 2;
                int SB = ETC1Modifiers[i, 0] + ETC1Modifiers[i, 1];
                int BB = ETC1Modifiers[i, 1] * 2;
                if (SS > 255) SS = 255;
                if (SB > 255) SB = 255;
                if (BB > 255) BB = 255;
                if (System.Math.Abs(DiffMean - SS) < ModDiff)
                {
                    ModDiff = System.Math.Abs(DiffMean - SS);
                    Modifier = i;
                    Mode = 0;
                }
                if (System.Math.Abs(DiffMean - SB) < ModDiff)
                {
                    ModDiff = System.Math.Abs(DiffMean - SB);
                    Modifier = i;
                    Mode = 1;
                }
                if (System.Math.Abs(DiffMean - BB) < ModDiff)
                {
                    ModDiff = System.Math.Abs(DiffMean - BB);
                    Modifier = i;
                    Mode = 2;
                }
            }

            if (Mode == 1)
            {
                float div1 = (float)ETC1Modifiers[Modifier, 0] / (float)ETC1Modifiers[Modifier, 1];
                float div2 = 1f - div1;
                BaseColor = Color.FromArgb((int)(Min.R * div1 + Max.R * div2), (int)(Min.G * div1 + Max.G * div2), (int)(Min.B * div1 + Max.B * div2));
            }
            else
            {
                BaseColor = Color.FromArgb((Min.R + Max.R) / 2, (Min.G + Max.G) / 2, (Min.B + Max.B) / 2);
            }
            return Modifier;
        }


        private static int GetScore(Color[] Original, Color[] Encode)
        {
            int Diff = 0;
            for (int i = 0; i < 4 * 4; i++)
            {
                Diff += System.Math.Abs(Encode[i].R - Original[i].R);
                Diff += System.Math.Abs(Encode[i].G - Original[i].G);
                Diff += System.Math.Abs(Encode[i].B - Original[i].B);
            }
            return Diff;
        }
        

        private static void SetFlipMode(ref ulong Data, bool Mode)
        {
            Data &= ~(1ul << 32);
            Data |= (Mode ? 1ul : 0ul) << 32;
        }

        private static void SetDiffMode(ref ulong Data, bool Mode)
        {
            Data &= ~(1ul << 33);
            Data |= (Mode ? 1ul : 0ul) << 33;
        }

        private static void SetTable1(ref ulong Data, int Table)
        {
            Data &= ~(7ul << 37);
            Data |= (ulong)(Table & 0x7) << 37;
        }

        private static void SetTable2(ref ulong Data, int Table)
        {
            Data &= ~(7ul << 34);
            Data |= (ulong)(Table & 0x7) << 34;
        }


        private static Color[] GetLeftColors(Color[] Pixels)
        {
            Color[] Left = new Color[4 * 2];
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 2; x++)
                {
                    Left[y * 2 + x] = Pixels[y * 4 + x];
                }
            }
            return Left;
        }

        private static Color[] GetRightColors(Color[] Pixels)
        {
            Color[] Right = new Color[4 * 2];
            for (int y = 0; y < 4; y++)
            {
                for (int x = 2; x < 4; x++)
                {
                    Right[y * 2 + x - 2] = Pixels[y * 4 + x];
                }
            }
            return Right;
        }

        private static Color[] GetTopColors(Color[] Pixels)
        {
            Color[] Top = new Color[4 * 2];
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    Top[y * 4 + x] = Pixels[y * 4 + x];
                }
            }
            return Top;
        }

        private static Color[] GetBottomColors(Color[] Pixels)
        {
            Color[] Bottom = new Color[4 * 2];
            for (int y = 2; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    Bottom[(y - 2) * 4 + x] = Pixels[y * 4 + x];
                }
            }
            return Bottom;
        }
        
        private static readonly int[,] ETC1Modifiers =
        {
            { 2, 8 },
            { 5, 17 },
            { 9, 29 },
            { 13, 42 },
            { 18, 60 },
            { 24, 80 },
            { 33, 106 },
            { 47, 183 }
        };

        private static void GenPixDiff(ref ulong Data, Color[] Pixels, Color BaseColor, int Modifier, int XOffs, int XEnd, int YOffs, int YEnd)
        {
            int BaseMean = (BaseColor.R + BaseColor.G + BaseColor.B) / 3;
            int i = 0;
            for (int yy = YOffs; yy < YEnd; yy++)
            {
                for (int xx = XOffs; xx < XEnd; xx++)
                {
                    int Diff = ((Pixels[i].R + Pixels[i].G + Pixels[i].B) / 3) - BaseMean;

                    if (Diff < 0) Data |= 1ul << (xx * 4 + yy + 16);
                    int tbldiff1 = System.Math.Abs(Diff) - ETC1Modifiers[Modifier, 0];
                    int tbldiff2 = System.Math.Abs(Diff) - ETC1Modifiers[Modifier, 1];

                    if (System.Math.Abs(tbldiff2) < System.Math.Abs(tbldiff1)) Data |= 1ul << (xx * 4 + yy);
                    i++;
                }
            }
        }

        private static void SetBaseColors(ref ulong Data, Color Color1, Color Color2)
        {
            int R1 = Color1.R;
            int G1 = Color1.G;
            int B1 = Color1.B;
            int R2 = Color2.R;
            int G2 = Color2.G;
            int B2 = Color2.B;
            //First look if differencial is possible.
            int RDiff = (R2 - R1) / 8;
            int GDiff = (G2 - G1) / 8;
            int BDiff = (B2 - B1) / 8;
            if (RDiff > -4 && RDiff < 3 && GDiff > -4 && GDiff < 3 && BDiff > -4 && BDiff < 3)
            {
                SetDiffMode(ref Data, true);
                R1 /= 8;
                G1 /= 8;
                B1 /= 8;
                Data |= (ulong)R1 << 59;
                Data |= (ulong)G1 << 51;
                Data |= (ulong)B1 << 43;
                Data |= (ulong)(RDiff & 0x7) << 56;
                Data |= (ulong)(GDiff & 0x7) << 48;
                Data |= (ulong)(BDiff & 0x7) << 40;
            }
            else
            {
                Data |= (ulong)(R1 / 0x11) << 60;
                Data |= (ulong)(G1 / 0x11) << 52;
                Data |= (ulong)(B1 / 0x11) << 44;

                Data |= (ulong)(R2 / 0x11) << 56;
                Data |= (ulong)(G2 / 0x11) << 48;
                Data |= (ulong)(B2 / 0x11) << 40;
            }
        }


    }
}

