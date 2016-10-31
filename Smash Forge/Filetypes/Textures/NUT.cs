using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge
{
    public class NUT
    {
        public class NUD_Texture
        {
            public byte[] data;
            public int width;
            public int height;
            public int mipCount;
            public PixelInternalFormat type;
            public OpenTK.Graphics.OpenGL.PixelFormat utype;
        }

        public Dictionary<int,int> draw = new Dictionary<int, int>();
        public List<NUD_Texture> textures = new List<NUD_Texture>();

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

                int mipCount = d.readInt();
                d.skip(4);// mipmaps and padding

                int offset1 = d.readInt() + 16;
                int offset2 = d.readInt() + 16;
                //          d.skip(4);
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

                byte[] data = header == 0x4E545755 ?GTX.swizzleBC(d.getSection(offset1 + padfix, DDSSize), w, h, format, tm, pitch, swizzle):d.getSection(offset1 + padfix, DDSSize);

                NUD_Texture tex = new NUD_Texture();
                textures.Add(tex);
                tex.data = data;
                tex.width = width;
                tex.height = height;
                tex.mipCount = mipCount;
                tex.type = PixelInternalFormat.Rgba32ui;

                switch (typet)
                {
                    case 0x0:
                        tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                        break;
                    case 0x2:
                        tex.type = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                        break;
                    case 14:
                        tex.type = PixelInternalFormat.Rgba;
                        tex.utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                        break;
                    case 17:
                        tex.type = PixelInternalFormat.Rgba;
                        tex.utype = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
                        break;
                    default:
                        Console.WriteLine("\t" + headerSize + " Type 0x" + typet);
                        break;
                }

                filen [i] = fileNum;
            }

            for (int i = 0; i < filen.Length; i++) {
                if(!draw.ContainsKey(filen[i]))
                    draw.Add (filen[i], loadImage(textures[i]));
            }
        }

        public void Destroy(){
            List<int> keyList = new List<int>(draw.Keys);
            for (int i = 0; i < keyList.Count; i++)
            {
                GL.DeleteTexture(keyList[i]);
            }
        }

        //texture----------------------------------------------------------

        public static int loadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }

        public static int getImageSize(NUD_Texture t)
        {
            switch (t.type)
            {
                case PixelInternalFormat.CompressedRgbaS3tcDxt1Ext:
                    return (t.width * t.height / 2);
                case PixelInternalFormat.CompressedRgbaS3tcDxt5Ext:
                    return (t.width * t.height);
                case PixelInternalFormat.Rgba:
                    return t.data.Length;
                default:
                    return t.data.Length;
            }
        }

        public static int loadImage(NUD_Texture t)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);

            if (t.type == PixelInternalFormat.CompressedRgbaS3tcDxt1Ext
                || t.type == PixelInternalFormat.CompressedRgbaS3tcDxt5Ext)
            {
                GL.CompressedTexImage2D<byte>(TextureTarget.Texture2D, 0, t.type, 
                    t.width, t.height, 0, getImageSize(t), t.data);
            }
            else
            {
                GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, t.type, t.width, t.height, 0,
                    t.utype, PixelType.UnsignedByte, t.data);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }
    }
}

