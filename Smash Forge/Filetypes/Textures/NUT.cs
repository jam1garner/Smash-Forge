using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Smash_Forge
{
    public class NutTexture : TreeNode
    {
        public List<byte[]> mipmaps = new List<byte[]>();
        //Each surface must have mipMapCount many mipmaps, which means: mipmaps.Count == (surfaceCount * mipMapCount)
        //Use mipMapCount, not mipmaps.Count, to check the mipmap amount
        public byte mipMapCount = 1;
        //Either 1 (standard textures) or 6 (cubemaps). No other values are explicitly supported
        public byte surfaceCount = 1;
        public int HASHID
        {
            get
            {
                return id;
            }
            set
            {
                Text = value.ToString("x").ToUpper();
                id = value;
            }
        }
        private int id;
        public int Width;
        public int Height;
        public PixelInternalFormat type;
        public OpenTK.Graphics.OpenGL.PixelFormat utype;
        public PixelType PixelType = PixelType.UnsignedByte;

        public uint ddsCaps2
        {
            get
            {
                if (surfaceCount == 6) return (uint)DDS.DDSCAPS2.CUBEMAP_ALLFACES;
                else                   return (uint)0;
            }
        }

        public NutTexture()
        {
            ImageKey = "texture";
            SelectedImageKey = "texture";
        }
        
        public override string ToString()
        {
            return HASHID.ToString("x").ToUpper();
        }

        public int Size
        {
            get
            {
                switch (type)
                {
                    case PixelInternalFormat.CompressedRedRgtc1:
                    case PixelInternalFormat.CompressedRgbaS3tcDxt1Ext:
                        return (Width * Height / 2);
                    case PixelInternalFormat.CompressedRgRgtc2:
                    case PixelInternalFormat.CompressedRgbaS3tcDxt3Ext:
                    case PixelInternalFormat.CompressedRgbaS3tcDxt5Ext:
                        return (Width * Height);
                    case PixelInternalFormat.Rgba16:
                        return mipmaps[0].Length / 2;
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
                case PixelInternalFormat.Rgb16:
                    return 8;
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

        public void setPixelFormatFromNutFormat(int typet)
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
                case 8:
                    type = PixelInternalFormat.Rgb16;
                    utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgb;
                    PixelType = PixelType.UnsignedShort565Reversed;
                    break;
                case 12:
                    type = PixelInternalFormat.Rgba16;
                    utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
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
                    throw new NotImplementedException($"Unknown nut texture format 0x{typet:X}");
            }
        }
    }

    public class NUT : FileBase
    {
        // Dictionary<hash ID, OpenTK texture>
        public Dictionary<int, int> glTexByHashId = new Dictionary<int, int>();

        public int Version = 0x200;

        public override Endianness Endian { get; set; }

        public NUT()
        {
            Text = "model.nut";
            ImageKey = "nut";
            SelectedImageKey = "nut";

            ContextMenu = new ContextMenu();
            MenuItem edit = new MenuItem("Edit");
            ContextMenu.MenuItems.Add(edit);
            edit.Click += OpenEditor;
            MenuItem save = new MenuItem("Save As");
            ContextMenu.MenuItems.Add(save);
            save.Click += Save;
        }

        public NUT (string filename) : this()
        {
            Read(filename);
        }

        public NUT(FileData d) : this()
        {
            Read(d);
        }

        /*~NUT()
        {
            //Destroy();
        }*/

        public bool getTextureByID(int hash, out NutTexture suc)
        {
            suc = null;
            foreach (NutTexture t in Nodes)
                if (t.HASHID == hash)
                {
                    suc = t;
                    return true;
                }

            return false;
        }

        #region Functions
        NUTEditor Editor;

        private void OpenEditor(object sender, EventArgs args)
        {
            if (Editor == null || Editor.IsDisposed)
            {
                Editor = new NUTEditor(this);
                Editor.Text = Parent.Text + "\\" + Text;
                //Editor.ShowDialog();
                MainForm.Instance.AddDockedControl(Editor);
            }
            else
            {
                Editor.BringToFront();
            }
        }

        public void Save(object sender, EventArgs args)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Namco Universal Texture (.nut)|*.nut|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(sfd.FileName, Rebuild());
                }
            }
        }
        #endregion

        public override byte[] Rebuild()
        {
            FileOutput o = new FileOutput();
            FileOutput data = new FileOutput();

            o.writeInt(0x4E545033); // "NTP3"
            o.writeShort(Version);
            o.writeShort(Nodes.Count);
            o.writeInt(0);
            o.writeInt(0);

            //calculate total header size
            uint headerLength = 0;

            foreach (NutTexture texture in Nodes)
            {
                bool isCubemap = texture.surfaceCount == 6;
                ushort headerSize = 0x50;
                if (isCubemap)
                {
                    headerSize += 0x10;
                }
                if (texture.mipMapCount > 1)
                {
                    headerSize += (ushort)(texture.mipMapCount * 4);
                    while (headerSize % 0x10 != 0)
                        headerSize += 1;
                }

                headerLength += headerSize;
            }

            // write headers+data
            foreach (NutTexture texture in Nodes)
            {
                bool isCubemap = texture.surfaceCount == 6;

                uint dataSize = 0;

                foreach (var mip in texture.mipmaps)
                {
                    dataSize += (uint)mip.Length;
                    while (dataSize % 0x10 != 0)
                        dataSize += 1;
                }

                ushort headerSize = 0x50;
                if (isCubemap)
                {
                    headerSize += 0x10;
                }
                if (texture.mipMapCount > 1)
                {
                    headerSize += (ushort)(texture.mipMapCount * 4);
                    while (headerSize % 0x10 != 0)
                        headerSize += 1;
                }

                o.writeUInt(dataSize + headerSize);
                o.writeInt(0x00);//padding
                o.writeUInt(dataSize);
                o.writeShort(headerSize); //+ (texture.mipmaps.Count - 4 > 0 ? texture.mipmaps.Count - 4 : 0) * 4
                o.writeShort(0);

                o.writeByte(0);
                o.writeByte(texture.mipMapCount);
                o.writeByte(0);
                o.writeByte(texture.getNutFormat());
                o.writeShort(texture.Width);
                o.writeShort(texture.Height);
                o.writeInt(0);
                o.writeUInt(texture.ddsCaps2);

                o.writeUInt((uint)(headerLength + data.size()));
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

                if (isCubemap)
                {
                    o.writeInt(texture.mipmaps[0].Length);
                    o.writeInt(texture.mipmaps[0].Length);
                    o.writeInt(0);
                    o.writeInt(0);
                }

                for (int surfaceLevel = 0; surfaceLevel < texture.surfaceCount; ++surfaceLevel)
                {
                    for (int mipLevel = 0; mipLevel < texture.mipMapCount; ++mipLevel)
                    {
                        int ds = data.size();
                        data.writeBytes(texture.mipmaps[(surfaceLevel*texture.mipMapCount)+mipLevel]);
                        data.align(0x10);
                        if (texture.mipMapCount > 1 && surfaceLevel == 0)
                            o.writeInt(data.size() - ds);
                    }
                }
                o.align(0x10);
                
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
                o.writeInt(texture.HASHID);
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
            Version = d.readUShort();
            int count = d.readUShort();
            d.skip(0x8);
            if (Version == 0x100) count -= 1;

            int dataPtr = 0;

            for (int i = 0; i < count; i++)
            {
                int currentTexStart = d.pos();

                NutTexture tex = new NutTexture();
                tex.type = PixelInternalFormat.Rgba32ui;

                int totalSize = d.readInt();
                d.skip(4);
                int dataSize = d.readInt();
                int headerSize = d.readUShort();
                d.skip(2);

                //It might seem that mipMapCount and pixelFormat would be shorts, but they're bytes because they stay in the same place regardless of endianness
                d.skip(1);
                tex.mipMapCount = d.readByte();
                d.skip(1);
                tex.setPixelFormatFromNutFormat(d.readByte());
                tex.Width = d.readUShort();
                tex.Height = d.readUShort();
                d.skip(4);
                uint caps2 = d.readUInt();

                int dataOffset = d.readInt() + dataPtr + 0x10;
                d.skip(0x0C);

                bool isCubemap = false;
                tex.surfaceCount = 1;
                if ((caps2 & (uint)DDS.DDSCAPS2.CUBEMAP) == (uint)DDS.DDSCAPS2.CUBEMAP)
                {
                    //Only supporting all six faces
                    if ((caps2 & (uint)DDS.DDSCAPS2.CUBEMAP_ALLFACES) == (uint)DDS.DDSCAPS2.CUBEMAP_ALLFACES)
                    {
                        isCubemap = true;
                        tex.surfaceCount = 6;
                    }
                    else
                    {
                        throw new NotImplementedException($"Unsupported cubemap face amount for texture {i} with hash 0x{tex.HASHID:X}. Six faces are required.");
                    }
                }

                //The size of a single cubemap face (discounting mipmaps). I don't know why it is repeated. If mipmaps are present, this is also specified in the mipSize section anyway.
                int cmapSize1 = 0;
                int cmapSize2 = 0;
                if (isCubemap)
                {
                    cmapSize1 = d.readInt();
                    cmapSize2 = d.readInt();
                    d.skip(8);
                }

                int[] mipSizes = new int[tex.mipMapCount];
                if (tex.mipMapCount == 1)
                {
                    if (isCubemap)
                        mipSizes[0] = cmapSize1;
                    else
                        mipSizes[0] = dataSize;
                }
                else
                {
                    for (int j = 0; j < tex.mipMapCount; j++)
                    {
                        mipSizes[j] = d.readInt();
                    }
                }
                d.align(0x10);

                d.skip(0x10); //eXt data - always the same

                d.skip(4); //GIDX
                d.readInt(); //Always 0x10
                tex.HASHID = d.readInt();
                d.skip(4); // padding align 8

                if (Version == 0x100)
                    dataOffset = d.pos();

                for (int surfaceLevel = 0; surfaceLevel < tex.surfaceCount; ++surfaceLevel)
                {
                    for (int mipLevel = 0; mipLevel < tex.mipMapCount; ++mipLevel)
                    {
                        byte[] texArray = d.getSection(dataOffset, mipSizes[mipLevel]);
                        tex.mipmaps.Add(texArray);
                        dataOffset += mipSizes[mipLevel];
                    }
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

                Nodes.Add(tex);
                d.seek(currentTexStart + headerSize);

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

            foreach (NutTexture tex in Nodes)
            {
                if (!glTexByHashId.ContainsKey(tex.HASHID))
                {
                    glTexByHashId.Add(tex.HASHID, CreateGlTexture(tex, true));
                }
            }
        }

        public void ReadNTWU(FileData d)
        {
            d.skip(0x02);
            int count = d.readUShort();

            int headerPtr = 0x10;
            int dataPtr = 0;
            int gtxHeaderOffset = 0;

            for (int i = 0; i < count; i++) {
                d.seek(headerPtr);
                NutTexture tex = new NutTexture();
                tex.type = PixelInternalFormat.Rgba32ui;

                int totalSize = d.readInt();
                d.skip(4);
                int dataSize = d.readInt();
                int headerSize = d.readUShort();
                headerPtr += headerSize;
                d.skip(2);

                d.skip(1);
                tex.mipMapCount = d.readByte();
                d.skip(1);
                tex.setPixelFormatFromNutFormat(d.readByte());
                tex.Width = d.readUShort();
                tex.Height = d.readUShort();
                d.readInt(); //Always 1?
                uint caps2 = d.readUInt();

                //Todo: implement cubemaps. Flags are the same as in NTP3
                bool isCubemap = false;
                tex.surfaceCount = 1;

                int dataOffset = d.readInt() + dataPtr + 0x10;
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
                tex.HASHID = d.readInt();

                Console.WriteLine(gtxHeaderOffset.ToString("x"));
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
                int maxSize = d.readInt(); // mipSize
                d.skip(0x04); // mipPtr
                int tileMode = d.readInt();
                int swizzle = d.readInt();
                d.skip(0x04); // alignment
                int pitch = d.readInt();

                int ds = dataOffset;
                int s1 = 0;
                int size = 0;
                Console.WriteLine(dataSize.ToString("x"));
                for (int mipLevel = 0; mipLevel < tex.mipMapCount; ++mipLevel)
                {
                    // Maybe this is the problem?
                    int mipSize = imageSize >> (mipLevel * 2);
                    int p = pitch >> mipLevel;
                    
                    size = d.readInt();
                    //Console.WriteLine("\tMIP: " + size.ToString("x") + " " + dataOffset.ToString("x") + " " + mipSize.ToString("x") + " " + p + " " + (size == 0 ? ds + dataSize - dataOffset : size));

                    //Console.WriteLine(tex.id.ToString("x") + " " + dataOffset.ToString("x") + " " + mipSize.ToString("x") + " " + p + " " + swizzle);
                    //Console.WriteLine((tex.width >> mipLevel) + " " + (tex.height >> mipLevel));

                    //if (cmap) tex.height *= 2;

                    int w = (tex.Width >> mipLevel);
                    int h = (tex.Height >> mipLevel);
                    
                    {
                        byte[] deswiz = GTX.swizzleBC(
                            d.getSection(dataOffset, d.size() - dataOffset),
                            w,
                            h,
                            format,
                            tileMode,
                            p,
                            swizzle
                        );
                        tex.mipmaps.Add(new FileData(deswiz).getSection(0, mipSize));
                    }
                    if (mipLevel == 0)
                    {
                        s1 = size;
                        dataOffset = ds + size;
                    }else
                    {
                        dataOffset = ds + s1 +size;
                    }
                    //dataOffset += mipSize;
                    
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

                Nodes.Add(tex);
            }

            foreach (NutTexture tex in Nodes)
            {
                if (!glTexByHashId.ContainsKey(tex.HASHID))
                {
                    glTexByHashId.Add(tex.HASHID, CreateGlTexture(tex, false));
                }

                // redo mipmaps
                /*GL.BindTexture(TextureTarget.Texture2D, draw[tex.id]);
                for (int k = 1; k < tex.mipmaps.Count; k++)
                {
                    tex.mipmaps[k] = new byte[tex.mipmaps[k].Length];
                    GCHandle pinnedArray = GCHandle.Alloc(tex.mipmaps[k], GCHandleType.Pinned);
                    IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                    GL.GetCompressedTexImage(TextureTarget.Texture2D, 0, pointer);
                    pinnedArray.Free();
                }*/
            }

            
            //File.WriteAllBytes("C:\\s\\Smash\\extract\\data\\fighter\\duckhunt\\model\\body\\mip1.bin", bytearray);

            //Console.WriteLine(GL.GetError());
            /*int j = 0;
            foreach(byte[] b in textures[0].mipmaps)
            {
                if (j == 3)
                {
                    for(int w = 3; w < 8; w++)
                    {
                        for (int p = 3; p < 6; p++)
                        {
                            byte[] deswiz = GTX.swizzleBC(
                                b,
                                (int)Math.Pow(2, w),
                                64,
                                51,
                                4,
                                 (int)Math.Pow(2, p),
                                197632
                            );
                            File.WriteAllBytes("C:\\s\\Smash\\extract\\data\\fighter\\duckhunt\\model\\body\\chunk_" + (int)Math.Pow(2, p) + "_" + (int)Math.Pow(2, w), deswiz);
                        }
                    }
                    
                }
                j++;
            }*/
        }

        public static bool texIdUsed(int texId)
        {
            foreach (var nut in Runtime.TextureContainers)
                foreach(NutTexture tex in nut.Nodes)
                    if (tex.HASHID == texId)
                        return true;
            return false;
        }

        public void ChangeTextureIds(int newTexId)
        {
            // Check if tex ID fixing would cause any naming conflicts. 
            if (TexIdDuplicate4thByte())
            {
                MessageBox.Show("The first six digits should be the same for all textures to prevent duplicate IDs after changing the Tex ID.",
                    "Duplicate Texture ID");
                return;
            }

            foreach (NutTexture tex in Nodes)
            {
                int originalTexture = glTexByHashId[tex.HASHID];
                glTexByHashId.Remove(tex.HASHID);

                // Only change the first 3 bytes.
                tex.HASHID = tex.HASHID & 0xFF;
                int first3Bytes = (int)(newTexId & 0xFFFFFF00);
                tex.HASHID = tex.HASHID | first3Bytes;

                glTexByHashId.Add(tex.HASHID, originalTexture);
            }
        }

        public bool TexIdDuplicate4thByte()
        {
            // Check for duplicates. 
            List<byte> previous4thBytes = new List<byte>();
            foreach (NutTexture tex in Nodes)
            {
                byte fourthByte = (byte) (tex.HASHID & 0xFF);
                if (!(previous4thBytes.Contains(fourthByte)))
                    previous4thBytes.Add(fourthByte);
                else
                    return true;
                    
            }

            return false;
        }

        public void Destroy()
        {
            foreach (var kv in glTexByHashId)
            {
                if (GL.IsTexture(kv.Value))
                    GL.DeleteTexture(kv.Value);
            }
            Nodes.Clear();
        }

        public override string ToString()
        {
            return "NUT";
        }

        public static int CreateGlTexture(NutTexture t, bool isDds = false)
        {
            int texID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texID);

            bool compressedFormatWithMipMaps = t.type == PixelInternalFormat.CompressedRgbaS3tcDxt1Ext
                || t.type == PixelInternalFormat.CompressedRgbaS3tcDxt3Ext
                || t.type == PixelInternalFormat.CompressedRgbaS3tcDxt5Ext
                || t.type == PixelInternalFormat.CompressedRedRgtc1
                || t.type == PixelInternalFormat.CompressedRgRgtc2;

            if (compressedFormatWithMipMaps)
            {
                // Always load the first level.
                GL.CompressedTexImage2D<byte>(TextureTarget.Texture2D, 0, t.type, t.Width, t.Height, 0, t.Size, t.mipmaps[0]);

                // Reading mip maps past the first level is only supported for DDS currently.
                if (t.mipmaps.Count > 1 && isDds)
                    LoadMipMapsCompressed(t);
                else
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
            else
            {
                AutoGenerateMipMaps(t);
            }

            return texID;
        }

        private static void AutoGenerateMipMaps(NutTexture t)
        {
            // Only load the first level and generate the other mip maps.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, t.mipmaps.Count);
            GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, t.type, t.Width, t.Height, 0, t.utype, t.PixelType, t.mipmaps[0]);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        private static void LoadMipMapsCompressed(NutTexture t)
        {
            // Generate the mip maps.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, t.mipmaps.Count);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            // Initialize the data for each level.
            for (int i = 0; i < t.mipmaps.Count; i++)
                GL.CompressedTexImage2D<byte>(TextureTarget.Texture2D, i, t.type,
                t.Width / (int)Math.Pow(2, i), t.Height / (int)Math.Pow(2, i), 0, t.mipmaps[i].Length, t.mipmaps[i]);
        }
    }
}
