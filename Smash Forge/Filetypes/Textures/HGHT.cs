using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using GraphicsMagick;

namespace Smash_Forge
{
    class HGHT
    {
        public HGHT(string filename)
        {
            Read(new FileData(filename));
        }

        public HGHT(FileData f)
        {
            Read(f);
        }

        public int width;
        public int height;
        public ushort[,] map;
        public Bitmap bitmap;
        public string name = "HeightMap";

        public void Read(FileData f)
        {
            width = (int)Math.Sqrt(f.eof() / 2);
            height = width;
            map = new ushort[width,height];
            for(int i = 0; i < width; i++)
                for(int j = 0; j < height; j++)
                    map[i, j] = (ushort)f.readShort();
        }

        public MagickImage toMagickImage()
        {
            MagickImage image = new MagickImage(new MagickColor(0,0,0), width, height);
            image.Format = MagickFormat.Png48;
            using (WritablePixelCollection pc = image.GetWritablePixels())
            {
                foreach(var pixel in pc)
                {
                    for (int channel = 0; channel < pc[pixel.X, pixel.Y].Channels; channel++)
                    {
                        pixel.SetChannel(channel, map[pixel.X, pixel.Y]);
                    }
                    pc.Set(pixel);
                }
                pc.Write();
            }
            return image;
        }

        public void generateBitmap()
        {
            Bitmap b = new Bitmap(width, height);
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    b.SetPixel(i, j, Color.FromArgb(255, (byte)((map[i, j] / 32678f) * 256), (byte)((map[i, j] / 32678f) * 256), (byte)((map[i, j] / 32678f) * 256)));
                }
            }
            bitmap = b;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
