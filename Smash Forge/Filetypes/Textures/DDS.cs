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

		public enum DDSFormat {
			RGBA,
			DXT5,
			DXT1,
            ATI1,
            ATI2
		}

		public class Header
		{
			public char[] magic;
			public int size = 0x7C;
			public int flags = 0x000A1007;
			public int height;
			public int width;
			public int pitchOrLinear = 0x00020000;
			public int depth = 1;
			public int mipmapCount;
			public int[] reserved;
			public int dwSize = 0x20;
			public int dwFlags = 0x04;
			public int dwFourCC;
			public int dwBitmask = 0;
			public uint dwCaps = 0;
			public uint dwCaps2 = 0;
			public uint dwCaps3 = 0;
			public uint dwCaps4 = 0;
			public int reserve = 0;
		}

		Header header;
		public byte[] data;

		public DDS (FileData d)
		{
			d.Endian = System.IO.Endianness.Little;
			d.seek (0);

            header = new Header();
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
			header.dwCaps = (uint)d.readInt ();
			header.dwCaps2 = (uint)d.readInt ();
			header.dwCaps3 = (uint)d.readInt ();
			header.dwCaps4 = (uint)d.readInt ();
			header.reserve = d.readInt ();

			d.skip (16);// not needed another header

			data = new byte[d.size() - d.pos()];
			for (int i = 0; i < data.Length; i++)
				data [i] = (byte)d.readByte ();
        }

        public DDS()
        {
            header = new Header();
        }

        public void Save(string fname)
        {
            FileOutput f = new FileOutput();
            f.Endian = System.IO.Endianness.Little;
            f.writeString("DDS ");
            f.writeInt(header.size);
            f.writeInt(header.flags);
            f.writeInt(header.height);
            f.writeInt(header.width);
            f.writeInt(header.pitchOrLinear);
            f.writeInt(header.depth);
            f.writeInt(header.mipmapCount);
            for (int i = 0; i < 11; i++)
                f.writeInt(0);
            f.writeInt(header.dwSize);
            f.writeInt(header.dwFlags);
            f.writeInt(header.dwFourCC);
            f.writeInt(header.dwBitmask);
            f.writeInt((int)header.dwCaps);
            f.writeInt((int)header.dwCaps2);
            f.writeInt((int)header.dwCaps3);
            f.writeInt((int)header.dwCaps4);
            f.writeInt((int)header.reserve);

            for (int i = 0; i < 4; i++)
                f.writeInt(0);

            f.writeBytes(data);

            f.save(fname);
        }

        public void fromNUT_Texture(NUT.NUD_Texture tex)
        {
            header = new Header();
            header.width = tex.width;
            header.height = tex.height;
            header.mipmapCount = tex.mipmaps.Count;
            switch (tex.type)
            {
                case PixelInternalFormat.CompressedRedRgtc1:
                    header.dwFourCC = 0x32495441;
                    break;
                case PixelInternalFormat.CompressedRgRgtc2:
                    header.dwFourCC = 0x31495441;
                    break;
                case PixelInternalFormat.CompressedRgbaS3tcDxt1Ext:
                    header.dwFourCC = 0x31545844;
                    break;
                case PixelInternalFormat.CompressedRgbaS3tcDxt3Ext:
                    header.dwFourCC = 0x33545844;
                    break;
                case PixelInternalFormat.CompressedRgbaS3tcDxt5Ext:
                    header.dwFourCC = 0x35545844;
                    break;
                case PixelInternalFormat.Rgba:
                    if (tex.utype == OpenTK.Graphics.OpenGL.PixelFormat.Rgba) { 
                        header.dwFourCC = 0x0;
                        header.dwBitmask = 0x20;
                        header.dwCaps = 0xFF;
                        header.dwCaps2 = 0xFF00;
                        header.dwCaps3 = 0xFF0000;
                        header.dwCaps4 = 0xFF000000;
                        header.reserve = 0x401008;
                        header.dwFlags = 0x41;
                    }
                    else
                        header.dwFourCC = 0x0;
                    break;
                /*case PixelInternalFormat.CompressedRedRgtc1:
                    break;*/
                /*case PixelInternalFormat.CompressedRgRgtc2:
                    header.dwFourCC = 0x42433553;
                    break;*/
                default:
                    throw new NotImplementedException($"Unknown pixel format 0x{tex.type:X}");
            }
            List<byte> d = new List<byte>();
            foreach(byte[] b in tex.mipmaps)
            {
                d.AddRange(b);
            }
            data = d.ToArray();
        }

        public NUT.NUD_Texture toNUT_Texture()
        {
            NUT.NUD_Texture tex = new NUT.NUD_Texture();
            tex.id = 0x48415348;
            tex.height = header.height;
            tex.width = header.width;
            float size = 1;
            int mips = header.mipmapCount;
            /*if (mips > header.mipmapCount)
            {
                mips = header.mipmapCount;
                MessageBox.Show("Possible texture error: Only one mipmap");
            }*/

            switch (header.dwFourCC)
            {
                case 0x0:
                    size = 4f;
                    tex.type = PixelInternalFormat.SrgbAlpha;
                    tex.utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                    break;
                case 0x31545844:
                    size = 1/2f;
                    tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    break;
                case 0x35545844:
                    size = 1f;
                    tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    break;
                case 0x32495441:
                    size = 1/2f;
                    tex.type = PixelInternalFormat.CompressedRedRgtc1;
                    break;
                case 0x31495441:
                    size = 1f;
                    tex.type = PixelInternalFormat.CompressedRgRgtc2;
                    break;
                default:
                    MessageBox.Show("Unsupported DDS format - 0x" + header.dwFourCC.ToString("x"));
                    break;
            }

            // now for mipmap data...
            FileData d = new FileData(data);
            int off = 0, w = header.width, h = header.height;

            if (header.mipmapCount == 0) header.mipmapCount = 1;
            for (int i = 0; i < header.mipmapCount; i++)
            {
                int s = (int)((w * h) * size);
                if (s < 0x8) s = 0x8;
                //Console.WriteLine(off.ToString("x") + " " + s.ToString("x"));
                w /= 2;
                h /= 2;
                tex.mipmaps.Add(d.getSection(off, s));
                off += s;
            }
            Console.WriteLine(off.ToString("x"));

            return tex;
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

			Bitmap bmp = new Bitmap(header.width, header.height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);  

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

