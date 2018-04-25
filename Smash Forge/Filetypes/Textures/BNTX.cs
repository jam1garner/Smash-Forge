using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Smash_Forge
{
    public class Formats
    {
        public enum BNTXImageFormat
        {
            IMAGE_FORMAT_INVALID = 0x0,
            IMAGE_FORMAT_R8_G8_B8_A8 = 0x0b,
            IMAGE_FORMAT_R5_G6_B5 = 0x07,
            IMAGE_FORMAT_R8 = 0x02,
            IMAGE_FORMAT_R8_G8 = 0x09,
            IMAGE_FORMAT_BC1 = 0x1a,
            IMAGE_FORMAT_BC2 = 0x1b,
            IMAGE_FORMAT_BC3 = 0x1c,
            IMAGE_FORMAT_BC4 = 0x1d,
            IMAGE_FORMAT_BC5 = 0x1e,
            IMAGE_FORMAT_BC6 = 0x1f,
            IMAGE_FORMAT_BC7 = 0x20,
        };

        public enum BNTXImageTypes
        {
            UNORM = 0x01,
            SNORM = 0x02,
            SRGB = 0x06,
        };

        public static uint blk_dims(uint format)
        {
            switch (format)
            {
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC1:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC2:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC3:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC4:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC5:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC6:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC7:
                case 0x2d:
                    return 0x44;

                case 0x2e: return 0x54;
                case 0x2f: return 0x55;
                case 0x30: return 0x65;
                case 0x31: return 0x66;
                case 0x32: return 0x85;
                case 0x33: return 0x86;
                case 0x34: return 0x88;
                case 0x35: return 0xa5;
                case 0x36: return 0xa6;
                case 0x37: return 0xa8;
                case 0x38: return 0xaa;
                case 0x39: return 0xca;
                case 0x3a: return 0xcc;

                default: return 0x11;
            }
        }

        public static uint bpps(uint format)
        {
            switch (format)
            {
                case (uint)BNTXImageFormat.IMAGE_FORMAT_R8_G8_B8_A8: return 4;
                case (uint)BNTXImageFormat.IMAGE_FORMAT_R8: return 1;

                case (uint)BNTXImageFormat.IMAGE_FORMAT_R5_G6_B5:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_R8_G8:
                    return 2;

                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC1:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC4:
                    return 8;

                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC2:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC3:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC5:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC6:
                case (uint)BNTXImageFormat.IMAGE_FORMAT_BC7:
                case 0x2e:
                case 0x2f:
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35:
                case 0x36:
                case 0x37:
                case 0x38:
                case 0x39:
                case 0x3a:
                    return 16;
                default: return 0x00;
            }
        }
    }
  

    public class BNTX : TreeNode
    {
        public static List<BRTI> textures = new List<BRTI>();
        public static Dictionary<string, BRTI> textured = new Dictionary<string, BRTI>();
        int BRTIOffset;

        public static int temp; //This variable is so we can get offsets from start of BNTX file

        public void ReadBNTX(FileData f)
        {
            ImageKey = "UVPattern";
            SelectedImageKey = "UVPattern";

            textures.Clear();

            temp = f.pos();

            f.skip(8); //Magic
            int Version = f.readInt();
            int ByteOrderMark = f.readShort();
            int FormatRevision = f.readShort();
            Text = f.readString(f.readInt() + temp, -1);
            f.skip(2);
            int strOffset = f.readShort();
            int relocOffset = f.readInt();
            int FileSize = f.readInt();
            f.skip(4); //NX Magic
            int TexturesCount = f.readInt();
            int InfoPtrsOffset = f.readInt();
            int DataBlockOffset = f.readInt();
            int DictOffset = f.readInt();
            int strDictSize = f.readInt();

            for (int i = 0; i < TexturesCount; i++)
            {
                f.seek(InfoPtrsOffset + temp + i * 8);
                BRTIOffset = f.readInt();


                f.seek(BRTIOffset + temp);

              //  textures.Add(new BRTI(f));
                BRTI texture = new BRTI(f);

                textured.Add(texture.Text, texture);
                textures.Add(texture);

            }
            Nodes.AddRange(textures.ToArray());
        }
    }


    public class BRTI : TreeNode
    {
        Swizzle.Surface surf;

        public BRTI_Texture texture = new BRTI_Texture();
        public byte DataType;
        public byte[] result_;

        public int Width, Height, display;
        public uint format;

        public BRTI(FileData f) //Docs thanks to gdkchan!!
        {
            ImageKey = "texture";
            SelectedImageKey = "texture";

            f.skip(4);

            int BRTISize1 = f.readInt();
            long BRTISize2 = (f.readInt() | f.readInt() << 32);
            surf = new Swizzle.Surface();

            surf.tileMode = (sbyte)f.readByte();
            surf.dim = (sbyte)f.readByte();
            ushort Flags = (ushort)f.readShort();
            surf.swizzle = (ushort)f.readShort();
            surf.numMips = (ushort)f.readShort();
            uint unk18 = (uint)f.readInt();
            surf.format = (uint)f.readInt();
            DataType = (byte)(surf.format & 0xFF);
            uint unk20 = (uint)f.readInt();
            surf.width = f.readInt();
            surf.height = f.readInt();
            surf.depth = f.readInt();
            int FaceCount = f.readInt();
            surf.sizeRange = f.readInt();
            uint unk38 = (uint)f.readInt();
            uint unk3C = (uint)f.readInt();
            uint unk40 = (uint)f.readInt();
            uint unk44 = (uint)f.readInt();
            uint unk48 = (uint)f.readInt();
            uint unk4C = (uint)f.readInt();
            surf.imageSize = f.readInt();
            surf.alignment = f.readInt();
            int ChannelType = f.readInt();
            int TextureType = f.readInt();
            Text = f.readString((f.readInt() | f.readInt() << 32) + BNTX.temp + 2, -1);
            long ParentOffset = f.readInt() | f.readInt() << 32;
            long PtrsOffset = f.readInt() | f.readInt() << 32;

            format = surf.format;

            f.seek((int)PtrsOffset + BNTX.temp);
            long dataOff = f.readInt() | f.readInt() << 32;
            surf.data = f.getSection((int)dataOff + BNTX.temp, surf.imageSize);
            //Console.WriteLine(surf.data.Length + " " + dataOff.ToString("x") + " " + surf.imageSize);

            uint blk_dim = Formats.blk_dims(surf.format >> 8);
            uint blkWidth = blk_dim >> 4;
            uint blkHeight = blk_dim & 0xF;

            uint bpp = Formats.bpps(surf.format >> 8);

       //     Console.WriteLine($"{Name} Height {surf.height}wdith = {surf.width}allignment = {surf.alignment}blkwidth = {blkWidth}blkheight = {blkHeight}blkdims = {blk_dim} format = {surf.format} datatype = {DataType} dataoffset = {dataOff}");


            // byte[] result = surf.data;

            


            byte[] result = Swizzle.deswizzle((uint)surf.width, (uint)surf.height, blkWidth, blkHeight, bpp, (uint)surf.tileMode, (uint)surf.alignment, surf.sizeRange, surf.data, 0);


            uint width = Swizzle.DIV_ROUND_UP((uint)surf.width, blkWidth);
            uint height = Swizzle.DIV_ROUND_UP((uint)surf.height, blkHeight);

            result_ = new byte[width * height * bpp];

            Array.Copy(result, 0, result_, 0, width * height * bpp);

            texture.data = result_;
            texture.width = surf.width;
            texture.height = surf.height;

            Width = surf.width;
            Height = surf.height;

            switch (surf.format >> 8)
            {
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC1):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM)
                    {
                        texture.type = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    }
                    else if (DataType == (byte)Formats.BNTXImageTypes.SRGB)
                    {
                        texture.type = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt1Ext;
                    }
                    else
                        throw new Exception("Unsupported data type");

                    break;
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC2):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM)
                        texture.type = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                    else if (DataType == (byte)Formats.BNTXImageTypes.SRGB)
                        texture.type = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
                    else
                        throw new Exception("Unsupported data type");
                    break;
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC3):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM)
                    {
                        texture.type = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    }
                    else if (DataType == (byte)Formats.BNTXImageTypes.SRGB)
                    {
                        texture.type = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
                    }
                    else
                        throw new Exception("Unsupported data type");
                    break;
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC4):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM)
                    {
                        texture.type = PixelInternalFormat.CompressedRedRgtc1;


                    }
                    else if (DataType == (byte)Formats.BNTXImageTypes.SNORM)
                    {
                        texture.type = PixelInternalFormat.CompressedSignedRedRgtc1;
                    }
                    else
                        throw new Exception("Unsupported data type");

                    break;

                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC5):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM)
                    {
                        texture.type = PixelInternalFormat.CompressedRgRgtc2;
                    }
                    else if (DataType == (byte)Formats.BNTXImageTypes.SNORM)
                    {
                        texture.type = PixelInternalFormat.CompressedSignedRgRgtc2;
                    }
                    else
                        Console.WriteLine("Unsupported data type");

                    break;
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_R8_G8_B8_A8):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM || DataType == (byte)Formats.BNTXImageTypes.SRGB)
                    {
                        texture.type = PixelInternalFormat.Rgba;
                        texture.utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                    }
                    else
                        throw new Exception("Unsupported data type");

                    break;
            }
            texture.display = loadImage(texture);
            display = texture.display;
        }

        public class BRTI_Texture
        {
            public byte[] data;
            public int width, height;
            public int display = 0;
            public PixelInternalFormat type;
            public OpenTK.Graphics.OpenGL.PixelFormat utype;
        }
        public static int getImageSize(BRTI_Texture t)
        {
            switch (t.type)
            {
                case PixelInternalFormat.CompressedRgbaS3tcDxt1Ext:
                case PixelInternalFormat.CompressedSrgbAlphaS3tcDxt1Ext:
                case PixelInternalFormat.CompressedRedRgtc1:
                case PixelInternalFormat.CompressedSignedRedRgtc1:
                    return (t.width * t.height / 2);
                case PixelInternalFormat.CompressedRgbaS3tcDxt3Ext:
                case PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext:
                case PixelInternalFormat.CompressedRgbaS3tcDxt5Ext:
                case PixelInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext:
                case PixelInternalFormat.CompressedSignedRgRgtc2:
                case PixelInternalFormat.CompressedRgRgtc2:
                    return (t.width * t.height);
                case PixelInternalFormat.Rgba:
                    return t.data.Length;
                default:
                    return t.data.Length;
            }
        }

        public static int loadImage(BRTI_Texture t)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);

            if (t.type != PixelInternalFormat.Rgba)
            {
                GL.CompressedTexImage2D<byte>(TextureTarget.Texture2D, 0, t.type,
                    t.width, t.height, 0, getImageSize(t), t.data);
                //Debug.WriteLine(GL.GetError());
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

