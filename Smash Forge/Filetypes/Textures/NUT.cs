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
    public class NUT : FileBase
    {
        public class NUD_Texture
        {
            public List<byte[]> mipmaps = new List<byte[]>();
            public int id;
            public int width;
            public int height;
            public PixelInternalFormat type;
            public OpenTK.Graphics.OpenGL.PixelFormat utype;
            
            public override string ToString()
            {
                return id.ToString("x").ToUpper();
            }

            public int Size
            {
                get
                {
                    switch (type)
                    {
                        case PixelInternalFormat.CompressedRedRgtc1:
                            return (width * height / 2);
                        case PixelInternalFormat.CompressedRgRgtc2:
                            return (width * height / 2);
                        case PixelInternalFormat.CompressedRgbaS3tcDxt1Ext:
                            return (width * height / 2);
                        case PixelInternalFormat.CompressedRgbaS3tcDxt3Ext:
                            return (width * height);
                        case PixelInternalFormat.CompressedRgbaS3tcDxt5Ext:
                            return (width * height);
                        case PixelInternalFormat.Rgba:
                            return mipmaps[0].Length;
                        default:
                            return mipmaps[0].Length;
                    }
                }
            }

            public int getNutFormat()
            {
                switch (type)
                {
                    case PixelInternalFormat.CompressedRgbaS3tcDxt1Ext:
                        return 0;
                    case PixelInternalFormat.CompressedRgbaS3tcDxt3Ext:
                        return 1;
                    case PixelInternalFormat.CompressedRgbaS3tcDxt5Ext:
                        return 2;
                    case PixelInternalFormat.Rgba:
                        if (utype == OpenTK.Graphics.OpenGL.PixelFormat.Rgba)
                            return 14;
                        else
                        if (utype == OpenTK.Graphics.OpenGL.PixelFormat.AbgrExt)
                            return 16;
                        else
                            return 17;
                    case PixelInternalFormat.CompressedRedRgtc1:
                        return 21;
                    case PixelInternalFormat.CompressedRgRgtc2:
                        return 22;
                    default:
                        throw new NotImplementedException($"Unknown pixel format 0x{type:X}");
                }
            }

            public void setPixelFormatFromNutFormat (int typet)
            {
                switch (typet)
                {
                    case 0x0:
                        type = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                        break;
                    case 0x1:
                        type = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                        break;
                    case 0x2:
                        type = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                        break;
                    case 14:
                        type = PixelInternalFormat.Rgba;
                        utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                        break;
                    case 16:
                        type = PixelInternalFormat.Rgba;
                        utype = OpenTK.Graphics.OpenGL.PixelFormat.AbgrExt;
                        break;
                    case 17:
                        type = PixelInternalFormat.Rgba;
                        utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                        break;
                    case 21:
                        type = PixelInternalFormat.CompressedRedRgtc1;
                        break;
                    case 22:
                        type = PixelInternalFormat.CompressedRgRgtc2;
                        break;
                    default:
                        throw new NotImplementedException($"Unknown nut format {typet}");
                }
            }
        }

        public Dictionary<int,int> draw = new Dictionary<int, int>();
        public List<NUD_Texture> textures = new List<NUD_Texture>();

        public override Endianness Endian { get; set; }

        public NUT()
        {
        }

        public NUT (string filename)
        {
            Read(filename);
        }

        public NUT(FileData d)
        {
            Read(d);
        }

        ~NUT()
        {
            //Destroy();
        }

        public bool getTextureByID(int hash, out NUD_Texture suc)
        {
            suc = null;
            foreach (NUD_Texture t in textures)
                if (t.id == hash)
                {
                    suc = t;
                    return true;
                }

            return false;
        }

        public override byte[] Rebuild()
        {
            FileOutput o = new FileOutput();
            FileOutput data = new FileOutput();

            o.writeInt(0x4E545033); // "NTP3"
            o.writeShort(0x0200);
            o.writeShort(textures.Count);
            o.writeInt(0);
            o.writeInt(0);

            //calculate total header size
            int headerLength = 0;

            foreach (var texture in textures)
            {
                int headerSize = 0x50;
                
                if (texture.mipmaps.Count > 1)
                {
                    headerSize += texture.mipmaps.Count * 4;
                    while (headerSize % 16 != 0)
                        headerSize += 4;
                }
                headerLength += headerSize;
            }

            // write headers+data
            foreach (var texture in textures)
            {
                int size = 0;
                
                foreach (var mip in texture.mipmaps)
                {
                    size += mip.Length;
                    while (size % 16 != 0)
                        size += 1;
                }

                int headerSize = 0x50;

                // calculate header size
                if(texture.mipmaps.Count > 1)
                {
                    headerSize += texture.mipmaps.Count * 4;
                    //align to 16
                    while (headerSize % 16 != 0)
                        headerSize += 1;
                }

                o.writeInt(size + headerSize);
                o.writeInt(0x00);//padding
                o.writeInt(size);
                o.writeShort(headerSize); //+ (texture.mipmaps.Count - 4 > 0 ? texture.mipmaps.Count - 4 : 0) * 4
                o.writeShort(0);
                o.writeShort(texture.mipmaps.Count);
                o.writeShort(texture.getNutFormat());
                o.writeShort(texture.width);
                o.writeShort(texture.height);
                o.writeInt(0);
                o.writeInt(0);
                o.writeInt(headerLength + data.size());
                headerLength -= headerSize;
                o.writeInt(0);
                o.writeInt(0);
                o.writeInt(0);
                
                if (texture.getNutFormat() == 14 || texture.getNutFormat() == 17)
                {
                    foreach (byte[] mip in texture.mipmaps)
                    {
                        for (int t = 0; t < mip.Length; t += 4)
                        {
                            byte t1 = mip[t + 3];
                            mip[t + 3] = mip[t + 2];
                            mip[t + 2] = mip[t + 1];
                            mip[t + 1] = mip[t];
                            mip[t] = t1;
                        }
                    }
                }

                foreach (var mip in texture.mipmaps)
                {
                    int ds = data.size();
                    data.writeBytes(mip);
                    data.align(0x10);
                    if (texture.mipmaps.Count > 1)
                        o.writeInt(data.size() - ds);
                }
                o.align(16);
                
                if (texture.getNutFormat() == 14 || texture.getNutFormat() == 17)
                {
                    foreach (byte[] mip in texture.mipmaps)
                    {
                        for (int t = 0; t < mip.Length; t += 4)
                        {
                            byte t1 = mip[t];
                            mip[t] = mip[t + 1];
                            mip[t + 1] = mip[t + 2];
                            mip[t + 2] = mip[t + 3];
                            mip[t + 3] = t1;
                        }
                    }
                }

                o.writeInt(0x65587400); // "eXt\0"
                o.writeInt(0x20);
                o.writeInt(0x10);
                o.writeInt(0x00);
                o.writeInt(0x47494458); // "GIDX"
                o.writeInt(0x10);

                o.writeInt(texture.id);
                o.writeInt(0);
            }
            o.writeOutput(data);

            return o.getBytes();
        }

        public override void Read(string filename)
        {
            Read(new FileData(filename));
        }

        public void Read(FileData d)
        {
            Endian = Endianness.Big;
            d.Endian = Endian;
            int magic = d.readInt();

            if (magic == 0x4E545033)
            {
                ReadNTP3(d);
            }
            else if (magic == 0x4E545755)
            {
                ReadNTWU(d);
            }
            else if (magic == 0x4E545744)
            {
                d.Endian = Endianness.Little;
                ReadNTP3(d);
            }
        }

        public void ReadNTP3(FileData d)
        {
            d.skip(0x2);
            int count = d.readShort();
            d.skip(0x8);

            int dataPtr = 0;

            for (int i = 0; i < count; i++)
            {
                Debug.WriteLine(d.pos().ToString("x"));
                NUD_Texture tex = new NUD_Texture();
                tex.type = PixelInternalFormat.Rgba32ui;

                int totalSize = d.readInt();
                d.skip(4); // padding

                int dataSize = d.readInt();
                int headerSize = d.readShort();
                d.skip(3);
                int numMips = d.readByte();
                Debug.WriteLine(numMips);
                d.skip(1);
                tex.setPixelFormatFromNutFormat(d.readByte());
                tex.width = d.readShort();
                tex.height = d.readShort();

                d.skip(8); // padding?

                int dataOffset = d.readInt() + dataPtr + 0x10;
                d.skip(0x0C);

                int[] mipSizes = new int[numMips];

                if (numMips == 1) mipSizes[0] = dataSize;
                else
                for (int j = 0; j < numMips; j++)
                {
                    mipSizes[j] = d.readInt();
                }
                d.align(16);

                d.skip(0x18);
                tex.id = d.readInt();
                d.skip(4); // padding align 8

                // add mipmap data
                for (int miplevel = 0; miplevel < numMips; miplevel++)
                {
                    byte[] texArray = d.getSection(dataOffset, mipSizes[miplevel]);
                    tex.mipmaps.Add(texArray);
                    dataOffset += mipSizes[miplevel];
                }

                dataPtr += headerSize;
                
                if (tex.getNutFormat() == 14 || tex.getNutFormat() == 17)
                {
                    Console.WriteLine("Endian swap");
                    // swap 
                    foreach (byte[] mip in tex.mipmaps)
                    {
                        for (int t = 0; t < mip.Length; t += 4)
                        {
                            byte t1 = mip[t];
                            mip[t] = mip[t + 1];
                            mip[t + 1] = mip[t + 2];
                            mip[t + 2] = mip[t + 3];
                            mip[t + 3] = t1;
                            /*byte t1 = mip[t];
                            byte t2 = mip[t+1];
                            mip[t] = mip[t + 3];
                            mip[t + 1] = mip[t + 2];
                            mip[t + 2] = t2;
                            mip[t + 3] = t1;*/
                        }
                    }
                }

                textures.Add(tex);

                /*for (int miplevel = 0; miplevel < numMips; miplevel++)
                {
                    byte[] texArray = d.getSection(dataOffset, mipSizes[miplevel]);

                    if (tex.getNutFormat() == 14)
                    {
                        byte[] oldArray = texArray;
                        for (int pos = 0; pos < mipSizes[miplevel]; pos+=4)
                        {

                            for (int p = 0; p < 4; p++)
                            {
                                if (p == 0)
                                    texArray[pos + 3] = oldArray[pos];
                                else
                                    texArray[pos + p - 1] = oldArray[pos + p];
                            }

                        }
                    }
                    tex.mipmaps.Add(texArray);
                    dataOffset += mipSizes[miplevel];
                }*/
            }

            foreach (var tex in textures)
            {
                if (!draw.ContainsKey(tex.id))
                {
                    draw.Add(tex.id, loadImage(tex, true));
                }
            }
        }

        public void ReadNTWU(FileData d)
        {
            d.skip(0x02);
            int count = d.readShort();

            d.skip(0x10);
            int headerPtr = d.pos();
            int dataPtr = 0;
            int gtxHeaderOffset = 0;

            for (int i = 0; i < count; i++) {
                NUD_Texture tex = new NUD_Texture();
                tex.type = PixelInternalFormat.Rgba32ui;

                d.seek(headerPtr);
                int totalSize = d.readInt();
                int headerSize = d.readShort();
                int numMips = d.readInt();
                tex.setPixelFormatFromNutFormat(d.readShort());
                tex.width = d.readShort();
                tex.height = d.readShort();

                d.skip(8); // mipmaps and padding
                int dataOffset = d.readInt() + dataPtr + 0x10;

                headerPtr += headerSize;
                dataPtr += headerSize;

                d.skip(0x04);
                if (i == 0)
                {
                    gtxHeaderOffset = d.readInt() + 0x10;
                } else
                {
                    gtxHeaderOffset += 0x80;
                    d.skip(0x04);
                }

                d.skip(0x04);
                
                // check for cubemap
                bool cmap = (d.readInt() == d.readInt());
                d.seek(d.pos() - 8);
                if (cmap) Console.WriteLine("cubemap detected");

                d.skip(headerSize - 0x50);

                d.skip(0x18);
                tex.id = d.readInt();

                d.seek(gtxHeaderOffset);
                d.skip(0x04); // dim
                d.skip(0x04); // width
                d.skip(0x04); // height
                d.skip(0x04); // depth
                d.skip(0x04); // numMips
                int format = d.readInt();
                d.skip(0x04); // aa
                d.skip(0x04); // use
                int imageSize = d.readInt();
                d.skip(0x04); // imagePtr
                d.skip(0x04); // mipSize
                d.skip(0x04); // mipPtr
                int tileMode = d.readInt();
                int swizzle = d.readInt();
                d.skip(0x04); // alignment
                int pitch = d.readInt();

                for (int mipLevel = 0; mipLevel < numMips; mipLevel++)
                {
                    // Maybe this is the problem?
                    int mipSize = imageSize >> (mipLevel * 2);
                    int p = pitch >> mipLevel;

                    //Console.WriteLine(tex.id.ToString("x") + " " + dataOffset.ToString("x") + " " + mipSize.ToString("x") + " " + p + " " + swizzle);
                    //Console.WriteLine((tex.width >> mipLevel) + " " + (tex.height >> mipLevel));
                    
                    //if (cmap) tex.height *= 2;

                    int w = (tex.width >> mipLevel);
                    int h = (tex.height >> mipLevel);

                    //if (mipSize % 0x10 != 0) mipSize += mipSize % 0x10;
                    //if (cmap) mipSize /= 6;

                    //if (p <= 16) p = 64;
                    {
                        tex.mipmaps.Add(GTX.swizzleBC(
                            d.getSection(dataOffset, mipSize),
                            w,
                            h,
                            format,
                            tileMode,
                            p,
                            swizzle
                        ));
                    }
                    dataOffset += mipSize;
                    
                    /*if (cmap)
                    {
                        for(int k = 0; k < 5; k++)
                        {
                            p = pitch >> (mipLevel + k + 1);
                            tex.mipmaps.Add(GTX.swizzleBC(
                                d.getSection(dataOffset, mipSize),
                                w,
                                h,
                                format,
                                tileMode,
                                p,
                                swizzle
                            ));

                            dataOffset += mipSize;
                        }
                    }*/

                    //while (dataOffset % 1024 != 0) dataOffset++;
                    //if (mipSize == 0x4000) dataOffset += 0x400;
                }

                // fix mipmap swizzle for rgba types
                if(tex.getNutFormat() == 14 || tex.getNutFormat() == 17)
                {
                    Console.WriteLine("Endian swap");
                    // swap 
                    foreach(byte[] mip in tex.mipmaps)
                    {
                        for(int t = 0; t < mip.Length; t+=4)
                        {
                            /*byte t1 = mip[t];
                            byte t2 = mip[t+1];
                            mip[t] = mip[t + 3];
                            mip[t + 1] = mip[t + 2];
                            mip[t + 2] = t2;
                            mip[t + 3] = t1;*/
                        }
                    }
                }

                textures.Add(tex);
            }

            foreach (var tex in textures)
            {
                if (!draw.ContainsKey(tex.id))
                {
                    draw.Add(tex.id, loadImage(tex));
                }
            }
        }

        public static bool texIdUsed(int texId)
        {
            foreach (var nut in Runtime.TextureContainers)
                foreach(var tex in nut.textures)
                    if (tex.id == texId)
                        return true;
            return false;
        }

        public void Destroy()
        {
            foreach (var kv in draw)
            {
                if (GL.IsTexture(kv.Value))
                    GL.DeleteTexture(kv.Value);
            }
        }

        public override string ToString()
        {
            return "NUT";
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

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 2);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }

        public static int loadImage(NUD_Texture t, bool DDS = false)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);

            if (t.type == PixelInternalFormat.CompressedRgbaS3tcDxt1Ext
                || t.type == PixelInternalFormat.CompressedRgbaS3tcDxt3Ext
                || t.type == PixelInternalFormat.CompressedRgbaS3tcDxt5Ext
                || t.type == PixelInternalFormat.CompressedRedRgtc1
                || t.type == PixelInternalFormat.CompressedRgRgtc2)
            {
                GL.CompressedTexImage2D<byte>(TextureTarget.Texture2D, 0, t.type,
                    t.width, t.height, 0, t.Size, t.mipmaps[0]);
                
                if (t.mipmaps.Count > 1 && DDS)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, t.mipmaps.Count);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                    for(int i = 0; i <t.mipmaps.Count; i++)
                        GL.CompressedTexImage2D<byte>(TextureTarget.Texture2D, i, t.type,
                        t.width / (int)Math.Pow(2,i), t.height / (int)Math.Pow(2, i), 0, t.mipmaps[i].Length, t.mipmaps[i]);
                }
                else
                {
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                }

                Debug.WriteLine(GL.GetError());
            }
            else
            {
                GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, t.type, t.width, t.height, 0,
                    t.utype, PixelType.UnsignedByte, t.mipmaps[0]);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }
    }
}
