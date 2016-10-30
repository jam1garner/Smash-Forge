using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace VBN_Editor
{
    public class Pixel
    {
        public Pixel()
        {
            
        }

        public static Bitmap fromRGBA(FileData d, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);  

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            int[] pixels = new int[width*height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = (d.readByte()) | (d.readByte() << 8) | (d.readByte() << 16) | (d.readByte() << 24);
            }

            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }
    }
}

