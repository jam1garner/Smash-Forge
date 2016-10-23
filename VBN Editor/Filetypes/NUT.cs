using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace VBN_Editor
{
	public class NUT
	{
		public Dictionary<int,int> draw = new Dictionary<int, int>();

		public List<Bitmap> textures = new List<Bitmap>();

		public NUT (FileData d)
		{
			//if (type == SMASH)
			// TODO: Pokken uses LE
			d.Endian = System.IO.Endianness.Big;
			d.seek (0);
			int header = d.readInt();
			d.seek(0x6);
			int count = d.readShort();

			d.seek(0x10);

			int padfix = 0;
			int headerSize = 0;
			int offheader = 0;

			int[] filen = new int[count];

            for (int i = 0; i < count; i++) {
                padfix += headerSize;
                d.skip(4);
                d.skip(4);
                int size = d.readInt();
                int DDSSize = size;
                headerSize = d.readShort();
                d.skip(5);
                int typet = d.readByte();

                int width = 0, height = 0;
                width = d.readShort();
                height = d.readShort();

                d.skip(8);// mipmaps and padding

                int offset1 = d.readInt() + 16;
                int offset2 = d.readInt() + 16;
                //			d.skip(4);
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
                    d.skip(12);
                }

                d.skip(16);
                d.skip(4);
                d.skip(4);
                int fileNum = d.readShort();
                int subfile = d.readShort();
                d.skip(4);

                //if(type == SMASH){
                // TODO: Pokken type....
                fileNum = (fileNum << 16) | subfile;
                //}

                Bitmap image = new Bitmap(10, 10);

                // Thanks Aelan for GTX support!
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


                //Console.WriteLine("Texture " + w + " " + h + " " + format + " " + tm + " " + swizzle + " " +  (offheader - 0x80));
                if (header == 0x4E545755) {
                byte[] data = GTX.swizzleBC(d.getSection(offset1 + padfix, DDSSize), w, h, format, tm, pitch, swizzle);
                    switch (typet)
                    {
                        case 0x0:
                            image = DDS.toBitmap(data, width, height, DDS.DDSFormat.DXT1);
                            break;
                        case 0x2:
                            image = DDS.toBitmap(data, width, height, DDS.DDSFormat.DXT5);
                            break;
                        default:
                            Console.WriteLine("\t" + headerSize + " Type 0x" + typet);
                            break;
                    }
                }
                if (header == 0x4E545033)
                {
                    byte[] data = d.getSection(offset1 + padfix, DDSSize);
                    switch (typet)
                    {
                        case 0x0:
                            image = DDS.toBitmap(data, width, height, DDS.DDSFormat.DXT1);
                            break;
                        case 0x2:
                            image = DDS.toBitmap(data, width, height, DDS.DDSFormat.DXT5);
                            break;
                        default:
                            Console.WriteLine("\t" + headerSize + " Type 0x" + typet);
                            break;
                    }
                }

                

				// read the bitmap image

				filen [i] = fileNum;
				textures.Add (image);
			}

			for (int i = 0; i < filen.Length; i++) {
				draw.Add (filen[i], NUD.loadImage(textures[i]));
			}
		}

        public void Destroy(){
            List<int> keyList = new List<int>(draw.Keys);
            for (int i = 0; i < keyList.Count; i++)
            {
                GL.DeleteTexture(keyList[i]);
            }
        }
	}
}

