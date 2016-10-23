using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace VBN_Editor
{
	public class DDS
	{

		public enum DDSFormat {
			RGBA,
			DXT5,
			DXT1
		}

		public struct Header
		{
			public char[] magic;
			public int size;
			public int flags;
			public int height;
			public int width;
			public int pitchOrLinear;
			public int depth;
			public int mipmapCount;
			public int[] reserved;
			public int dwSize;
			public int dwFlags;
			public int dwFourCC;
			public int dwBitmask;
			public int dwCaps;
			public int dwCaps2;
			public int dwCaps3;
			public int dwCaps4;
			public int reserve;
		}

		Header header;
		public byte[] data;

		public DDS (FileData d)
		{
			d.Endian = System.IO.Endianness.Little;
			d.seek (0);

			header.magic = new char[4];
			header.magic [0] = (char)d.readByte ();
			header.magic [1] = (char)d.readByte ();
			header.magic [2] = (char)d.readByte ();
			header.magic [3] = (char)d.readByte ();
			header.size = d.readInt ();
			header.flags = d.readInt ();
			header.height = d.readInt ();
			header.width = d.readInt ();
			header.pitchOrLinear = d.readInt ();
			header.depth = d.readInt ();
			header.mipmapCount = d.readInt ();
			header.reserved = new int[11];
			for (int i = 0; i < 11; i++)
				header.reserved [i] = d.readInt ();
			header.dwSize = d.readInt ();
			header.dwFlags = d.readInt ();
			header.dwFourCC = d.readInt ();
			header.dwBitmask = d.readInt ();
			header.dwCaps = d.readInt ();
			header.dwCaps2 = d.readInt ();
			header.dwCaps3 = d.readInt ();
			header.dwCaps4 = d.readInt ();
			header.reserve = d.readInt ();

			d.skip (16);// not needed another header

			data = new byte[d.size() - d.pos()];
			for (int i = 0; i < data.Length; i++)
				data [i] = (byte)d.readByte ();
		}

		public Bitmap toBitmap(){

			byte[] pixels = new byte[header.width * header.height * 4];

			if (header.dwFourCC == 0x31545844)
				decodeDXT1 (pixels, data, header.width, header.height);
            else
			if (header.dwFourCC == 0x35545844)
				decodeDXT5 (pixels, data, header.width, header.height);
			else
				Console.WriteLine ("Unknown DDS format " + header.dwFourCC);

			Bitmap bmp = new Bitmap(header.width, header.height, PixelFormat.Format32bppArgb);  

			BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

			Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);


			bmp.UnlockBits(bmpData);
			return bmp;
		}


		public static Bitmap toBitmap(byte[] d, int width, int height, DDSFormat format){

			byte[] pixels = new byte[width * height * 4];

			if(format == DDSFormat.DXT1)
				decodeDXT1 (pixels, d, width, height);
			if(format == DDSFormat.DXT5)
				decodeDXT5 (pixels, d, width, height);

			Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);  

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

