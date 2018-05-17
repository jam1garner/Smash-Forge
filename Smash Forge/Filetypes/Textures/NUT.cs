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
                        return (Width * Height / 2);
                    case PixelInternalFormat.CompressedRgRgtc2:
                        return (Width * Height / 2);
                    case PixelInternalFormat.CompressedRgbaS3tcDxt1Ext:
                        return (Width * Height / 2);
                    case PixelInternalFormat.CompressedRgbaS3tcDxt3Ext:
                        return (Width * Height);
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
                    throw new NotImplementedException($"Unknown nut format {typet}");
            }
        }
    }

    public class NUT : FileBase
    {
        // Dictionary<hash ID, OpenTK texture>
        public Dictionary<int, int> draw = new Dictionary<int, int>();

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

        ~NUT()
        {
            //Destroy();
        }

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
            int headerLength = 0;

            foreach (NutTexture texture in Nodes)
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
            foreach (NutTexture texture in Nodes)
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
                o.writeShort(texture.Width);
                o.writeShort(texture.Height);
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
                //Debug.WriteLine(d.pos().ToString("x"));
                NutTexture tex = new NutTexture();
                tex.type = PixelInternalFormat.Rgba32ui;

                int totalSize = d.readInt();
                d.skip(4); // padding

                int dataSize = d.readInt();
                int headerSize = d.readUShort();
                d.skip(3);
                int numMips = d.readByte();
                //Debug.WriteLine(numMips);
                d.skip(1);
                tex.setPixelFormatFromNutFormat(d.readByte());
                tex.Width = d.readUShort();
                tex.Height = d.readUShort();

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
                tex.HASHID = d.readInt();
                d.skip(4); // padding align 8

                if (Version == 0x100)
                    dataOffset = d.pos();

                // add mipmap data
                for (int miplevel = 0; miplevel < numMips; miplevel++)
                {
                    byte[] texArray = d.getSection(dataOffset, mipSizes[miplevel]);
                    //Debug.WriteLine(texArray.Length.ToString("x"));
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

                Nodes.Add(tex);

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
                if (!draw.ContainsKey(tex.HASHID))
                {
                    draw.Add(tex.HASHID, loadImage(tex, true));
                }
            }
        }

        public void ReadNTWU(FileData d)
        {
            d.skip(0x02);
            int count = d.readUShort();

            d.skip(0x10);
            int headerPtr = d.pos();
            int dataPtr = 0;
            int gtxHeaderOffset = 0;

            for (int i = 0; i < count; i++) {
                NutTexture tex = new NutTexture();
                tex.type = PixelInternalFormat.Rgba32ui;

                d.seek(headerPtr);
                int totalSize = d.readInt();
                int headerSize = d.readUShort();
                int numMips = d.readInt();
                tex.setPixelFormatFromNutFormat(d.readUShort());
                tex.Width = d.readUShort();
                tex.Height = d.readUShort();

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
                Console.WriteLine(totalSize.ToString("x"));
                for (int mipLevel = 0; mipLevel < numMips; mipLevel++)
                {
                    // Maybe this is the problem?
                    int mipSize = imageSize >> (mipLevel * 2);
                    int p = pitch >> mipLevel;
                    
                    size = d.readInt();
                    //Console.WriteLine("\tMIP: " + size.ToString("x") + " " + dataOffset.ToString("x") + " " + mipSize.ToString("x") + " " + p + " " + (size == 0 ? ds + totalSize - dataOffset : size));

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
                if (!draw.ContainsKey(tex.HASHID))
                {
                    draw.Add(tex.HASHID, loadImage(tex, false));
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
                int originalTexture = draw[tex.HASHID];
                draw.Remove(tex.HASHID);

                // Only change the first 3 bytes.
                tex.HASHID = tex.HASHID & 0xFF;
                int first3Bytes = (int)(newTexId & 0xFFFFFF00);
                tex.HASHID = tex.HASHID | first3Bytes;

                draw.Add(tex.HASHID, originalTexture);
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
            foreach (var kv in draw)
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

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 1);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return texID;
        }

        public static int loadImage(NutTexture t, bool DDS = false)
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
                    t.Width, t.Height, 0, t.Size, t.mipmaps[0]);
                
                if (t.mipmaps.Count > 1 && DDS)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, t.mipmaps.Count);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                    for (int i = 0; i <t.mipmaps.Count; i++)
                        GL.CompressedTexImage2D<byte>(TextureTarget.Texture2D, i, t.type,
                        t.Width / (int)Math.Pow(2,i), t.Height / (int)Math.Pow(2, i), 0, t.mipmaps[i].Length, t.mipmaps[i]);
                }
                else
                {
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                }

                //Debug.WriteLine(GL.GetError());
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, t.mipmaps.Count);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, t.type, t.Width, t.Height, 0,
                    t.utype, t.PixelType, t.mipmaps[0]);

               /* for (int i = 0; i < t.mipmaps.Count; i++)
                    GL.TexImage2D<byte>(TextureTarget.Texture2D, i, t.type, t.width, t.height, 0,
                    t.utype, t.PixelType, t.mipmaps[i]);*/

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }
    }
}
