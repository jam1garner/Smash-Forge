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

			for(int i = 0 ; i < count ; i++){
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
				if(i == 0){
					offheader = offset3;
				}

				if(headerSize == 0x90){
					DDSSize = d.readInt();
					d.skip(0x3C);
				}
				if(headerSize == 0x80){
					DDSSize = d.readInt();
					d.skip(44);
				}
				if(headerSize == 0x70){
					DDSSize = d.readInt();
					d.skip(28);
				}
				if(headerSize == 0x60){
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
				fileNum = (fileNum<<16)|subfile;
				//}

				Bitmap image = new Bitmap(10,10);

				if (true) {
					FileOutput o = new FileOutput ();
					String[] mem = "47x66x78x32x00x00x00x20x00x00x00x07x00x00x00x01x00x00x00x02x00x00x00x00x00x00x00x00x00x00x00x00x42x4Cx4Bx7Bx00x00x00x20x00x00x00x01x00x00x00x00x00x00x00x0Bx00x00x00x9Cx00x00x00x00x00x00x00x00".Split ('x');
					foreach (String s in mem)
						o.writeByte (byte.Parse(s, System.Globalization.NumberStyles.HexNumber));

					int t = d.pos ();
					d.seek (offheader);
					for (int k = 0; k < 0x80; k++)
						o.writeByte (d.readByte ());
					offheader += 0x80;
					d.seek (t);

					mem = "00x00x00x01x00x01x02x03x1FxF8x7Fx21xC4x00x03xFFx06x88x80x00x00x00x00x0Ax80x00x00x10x42x4Cx4Bx7Bx00x00x00x20x00x00x00x01x00x00x00x00x00x00x00x0Cx00x08x00x00x00x00x00x00x00x00x00x00".Split('x');
					foreach (String s in mem)
						o.writeByte (byte.Parse(s, System.Globalization.NumberStyles.HexNumber));
					
					byte[] by = d.getSection (offset1 + padfix, DDSSize);
					foreach (byte b in by)
						o.writeByte (b);

					mem = "42x4Cx4Bx7Bx00x00x00x20x00x00x00x01x00x00x00x00x00x00x00x01x00x00x00x00x00x00x00x00x00x00x00x00".Split ('x');
					foreach (String s in mem)
						o.writeByte (byte.Parse(s, System.Globalization.NumberStyles.HexNumber));

                    o.Endian = Endianness.Big;
					o.writeIntAt (1, 0x50);
					o.writeIntAt (DDSSize, 0xF0);

					o.save ("temp.gtx");

					String command = @"-i temp.gtx -o temp.dds";
					ProcessStartInfo cmdsi = new ProcessStartInfo("TexConv2.exe");
					cmdsi.Arguments = command;
					cmdsi.CreateNoWindow = true;
					cmdsi.WindowStyle = ProcessWindowStyle.Hidden;

					Process cmd = Process.Start(cmdsi);
					cmd.WaitForExit(); 

					image = new DDS(new FileData("temp.dds")).toBitmap ();
				}
				else
				switch (typet) {
				case 0x0:
					image = DDS.toBitmap (d.getSection (offset1, DDSSize), width, height, DDS.DDSFormat.DXT1);
					break;
				case 0x2:
					image = DDS.toBitmap(d.getSection(offset1, DDSSize), width, height, DDS.DDSFormat.DXT5);
					break;
					//default:
					//System.out.println("\t" + headerSize + " Type 0x" + Integer.toHexString(type));
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

