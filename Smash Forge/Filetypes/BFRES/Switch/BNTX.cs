using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

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
        //Todo: Have these 2 combined into one list
        public static List<BRTI> textures = new List<BRTI>(); //For loading a list of texture instances
        public static Dictionary<string, BRTI> textured = new Dictionary<string, BRTI>(); //For texture mapping
        int BRTIOffset;

        public static int temp; //This variable is so we can get offsets from start of BNTX file

        public void ReadBNTXFile(string FileName) //For single BNTX files
        {
            FileData f = new FileData(FileName);
            f.Endian = Endianness.Little;

            ReadBNTX(f);
        }

        public void ReadBNTX(FileData f)
        {
            ImageKey = "UVPattern";
            SelectedImageKey = "UVPattern";

            textures.Clear();
            textured.Clear();

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
        public static List<BRTI_Texture> RenderableTex = new List<BRTI_Texture>();


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

            texture.mipmaps.Add(result_);

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
                        throw new Exception("Unsupported data type for BC1");

                    break;
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC2):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM)
                        texture.type = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                    else if (DataType == (byte)Formats.BNTXImageTypes.SRGB)
                        texture.type = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
                    else
                        throw new Exception("Unsupported data type for BC2");
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
                        throw new Exception("Unsupported data type for BC3");
                    break;
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC4):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM)
                    {
                        texture.type = PixelInternalFormat.CompressedRedRgtc1;

                    /*    byte[] fixBC4 = DecompressBC4(texture.data, texture.width, texture.height, false);
                        texture.data = fixBC4;
                        texture.type = PixelInternalFormat.Rgba;
                        texture.utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;*/
                    }
                    else if (DataType == (byte)Formats.BNTXImageTypes.SNORM)
                    {
                        texture.type = PixelInternalFormat.CompressedSignedRedRgtc1;

                        /*     byte[] fixBC4 = DecompressBC4(texture.data, texture.width, texture.height, true);
                             texture.data = fixBC4;
                             texture.type = PixelInternalFormat.Rgba;
                             texture.utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;*/
                    }
                    else
                        throw new Exception("Unsupported data type for BC4");
                    break;
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC5):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM)
                    {
                  //      byte[] fixBC5 = DecompressBC5(texture.data, texture.width, texture.height, false);
                    //    texture.data = fixBC5;
                        texture.type = PixelInternalFormat.CompressedRgRgtc2;
                    //    texture.utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                    }
                    else if (DataType == (byte)Formats.BNTXImageTypes.SNORM)
                    {
                           byte[] fixBC5 = DecompressBC5(texture.data, texture.width, texture.height, true);
                           texture.data = fixBC5;
                           texture.type = PixelInternalFormat.Rgba;
                           texture.utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                    }
                    else
                        Console.WriteLine("Unsupported data type for BC5");
                    break;
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC7):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM)
                    {
               /*         ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.CreateNoWindow = false;
                        startInfo.UseShellExecute = false;
                        startInfo.FileName = "lib\texconv.exe";
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.Arguments = "-ft png -f R10G10B10A2_UNORM -y";*/
                    }
                    else if (DataType == (byte)Formats.BNTXImageTypes.SNORM)
                    {

                    }
                    else
                        Console.WriteLine("Unsupported data type for BC7");
                    break;
                case ((uint)Formats.BNTXImageFormat.IMAGE_FORMAT_R8_G8_B8_A8):
                    if (DataType == (byte)Formats.BNTXImageTypes.UNORM || DataType == (byte)Formats.BNTXImageTypes.SRGB)
                    {
                        texture.type = PixelInternalFormat.Rgba;
                        texture.utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                    }
                    else
                        throw new Exception("Unsupported data type for R8_G8_B8_A8");
                    break;
            }
            texture.display = loadImage(texture);
            display = texture.display;

            RenderableTex.Add(texture);
        }


        //Huge thanks to gdkchan and AbooodXD for the method of decomp BC5/BC4.
        
        //Todo. Add these to DDS code and add in methods to compress and decode more formats
        //BC7 also needs to be decompressed properly since OpenTK can't decompress those

        //BC4 actually breaks a bit with artifacts so i'll need to go back and fix
        internal static byte[] DecompressBC4(Byte[] data, int width, int height, bool IsSNORM)
        {
            int W = (width + 3) / 4;
            int H = (height + 3) / 4;

            byte[] Output = new byte[width * height * 4];

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int IOffs = (Y * W + X) * 8;

                    byte[] Red = new byte[8];

                    Red[0] = data[IOffs + 0];
                    Red[1] = data[IOffs + 1];

                    CalculateBC3Alpha(Red);

                    int RedLow = Get32(data, IOffs + 2);
                    int RedHigh = Get16(data, IOffs + 6);

                    ulong RedCh = (uint)RedLow | (ulong)RedHigh << 32;

                    int TOffset = 0;
                    int TW = Math.Min(width - X * 4, 4);
                    int TH = Math.Min(height - Y * 4, 4);

                    for (int TY = 0; TY < TH; TY++)
                    {
                        for (int TX = 0; TX < TW; TX++)
                        {
                            int OOffset = ((Y * 4 + TY) * width + (X * 4 + TX)) * 4;

                            byte RedPx = Red[(RedCh >> (TY * 12 + TX * 3)) & 7];

                            Output[OOffset + 0] = RedPx;
                            Output[OOffset + 1] = RedPx;
                            Output[OOffset + 2] = RedPx;
                            Output[OOffset + 3] = 0xff;

                            TOffset += 4;
                        }
                    }
                }
            }
        
            return Output;
        }

        internal static byte[] DecompressBC5(Byte[] data, int width, int height, bool IsSNORM)
        {
            int W = (width + 3) / 4;
            int H = (height + 3) / 4;


            byte[] Output = new byte[width * height * 4];

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)

                {
                    int IOffs = (Y * W + X) * 16;
                    byte[] Red = new byte[8];
                    byte[] Green = new byte[8];

                    Red[0] = data[IOffs + 0];
                    Red[1] = data[IOffs + 1];

                    Green[0] = data[IOffs + 8];
                    Green[1] = data[IOffs + 9];

                    if (IsSNORM == true)
                    {
                        CalculateBC3AlphaS(Red);
                        CalculateBC3AlphaS(Green);
                    }
                    else
                    {
                        CalculateBC3Alpha(Red);
                        CalculateBC3Alpha(Green);
                    }

                    int RedLow = Get32(data, IOffs + 2);
                    int RedHigh = Get16(data, IOffs + 6);

                    int GreenLow = Get32(data, IOffs + 10);
                    int GreenHigh = Get16(data, IOffs + 14);

                    ulong RedCh = (uint)RedLow | (ulong)RedHigh << 32;
                    ulong GreenCh = (uint)GreenLow | (ulong)GreenHigh << 32;

                    int TW = Math.Min(width - X * 4, 4);
                    int TH = Math.Min(height - Y * 4, 4);


                    if (IsSNORM == true)
                    {
                        for (int TY = 0; TY < TH; TY++)
                        {
                            for (int TX = 0; TX < TW; TX++)
                            {

                                int Shift = TY * 12 + TX * 3;
                                int OOffset = ((Y * 4 + TY) * width + (X * 4 + TX)) * 4;

                                byte RedPx = Red[(RedCh >> Shift) & 7];
                                byte GreenPx = Green[(GreenCh >> Shift) & 7];

                                if (IsSNORM == true)
                                {
                                    RedPx += 0x80;
                                    GreenPx += 0x80;
                                }

                                float NX = (RedPx / 255f) * 2 - 1;
                                float NY = (GreenPx / 255f) * 2 - 1;
                                float NZ = (float)Math.Sqrt(1 - (NX * NX + NY * NY));

                                Output[OOffset + 0] = Clamp((NX + 1) * 0.5f);
                                Output[OOffset + 1] = Clamp((NY + 1) * 0.5f);
                                Output[OOffset + 2] = Clamp((NZ + 1) * 0.5f);
                                Output[OOffset + 3] = 0xff;
                            }
                        }
                    }
                    else
                    {
                        for (int TY = 0; TY < TH; TY++)
                        {
                            for (int TX = 0; TX < TW; TX++)
                            {

                                int Shift = TY * 12 + TX * 3;
                                int OOffset = ((Y * 4 + TY) *width + (X * 4 + TX)) * 4;

                                byte RedPx = Red[(RedCh >> Shift) & 7];
                                byte GreenPx = Green[(GreenCh >> Shift) & 7];

                                Output[OOffset + 0] = RedPx;
                                Output[OOffset + 1] = GreenPx;
                                Output[OOffset + 2] = 255;
                                Output[OOffset + 3] = 255;

                            }
                        }
                    }
                }
            }
            return Output;
        }

        public static int Get16(byte[] Data, int Address)
        {
            return
                Data[Address + 0] << 0 |
                Data[Address + 1] << 8;
        }

        public static int Get32(byte[] Data, int Address)
        {
            return
                Data[Address + 0] << 0 |
                Data[Address + 1] << 8 |
                Data[Address + 2] << 16 |
                Data[Address + 3] << 24;
        }

        private static byte Clamp(float Value)
        {
            if (Value > 1)
            {
                return 0xff;
            }
            else if (Value < 0)
            {
                return 0;
            }
            else
            {
                return (byte)(Value * 0xff);
            }
        }

        private static void CalculateBC3Alpha(byte[] Alpha)
        {
            for (int i = 2; i < 8; i++)
            {
                if (Alpha[0] > Alpha[1])
                {
                    Alpha[i] = (byte)(((8 - i) * Alpha[0] + (i - 1) * Alpha[1]) / 7);
                }
                else if (i < 6)
                {
                    Alpha[i] = (byte)(((6 - i) * Alpha[0] + (i - 1) * Alpha[1]) / 7);
                }
                else if (i == 6)
                {
                    Alpha[i] = 0;
                }
                else /* i == 7 */
                {
                    Alpha[i] = 0xff;
                }
            }
        }
        private static void CalculateBC3AlphaS(byte[] Alpha)
        {
            for (int i = 2; i < 8; i++)
            {
                if ((sbyte)Alpha[0] > (sbyte)Alpha[1])
                {
                    Alpha[i] = (byte)(((8 - i) * (sbyte)Alpha[0] + (i - 1) * (sbyte)Alpha[1]) / 7);
                }
                else if (i < 6)
                {
                    Alpha[i] = (byte)(((6 - i) * (sbyte)Alpha[0] + (i - 1) * (sbyte)Alpha[1]) / 7);
                }
                else if (i == 6)
                {
                    Alpha[i] = 0x80;
                }
                else /* i == 7 */
                {
                    Alpha[i] = 0x7f;
                }
            }
        }

        public static byte[] DecodeBC7(int X, int Y, int block)
        {
            byte[] result = null;

            //Alright so BC7 decompression as multple modes

            return result;
        }

        public class BRTI_Texture
        {
            public List<byte[]> mipmaps = new List<byte[]>();
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


        public static int LoadBitmap(Bitmap image)
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

        public unsafe void ExportAsImage(BRTI_Texture t, int id , string path)
        {
            Bitmap bitmap = new Bitmap(t.width, t.height);
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, t.width, t.height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.GetTexImage(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);

            bitmap.UnlockBits(bitmapData);
            bitmap.Save(path);
        }
    }
}

