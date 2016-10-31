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

    }
}

