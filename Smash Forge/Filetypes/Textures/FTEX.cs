using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Smash_Forge
{
    public class FTEX
    {
        public class Header
        {
            //0xC0 In Lenth
            public int Name = 0x46544558;
            public int dim;
            public int width;
            public int height;
            public int depth;
            public int mipCount;
            public int format;
            public int AA;
            public int usage;
            public int textureSize;
            public int u1;
            public int mipmapSize;
            public int u2;
            public int tilemode;
            public int swizzle;
            public int alignment;
            public int pitch;
            public byte[] gtxFluff = new byte[0x6C];
            public int textureOffset;
            public int mipmapOffset;
            public int pad0;
            public int pad1;
        }
        public FTEX_Texture texture = new FTEX_Texture();
        public int readOffset(FileData f)
        {
            return f.pos() + f.readInt();
        }
        public void ReadFTEX(FileData f)
        {
            texture.width = f.readInt();
            texture.height = f.readInt();
            f.skip(8);
            int format = f.readInt();
            f.skip(8);
            int size = f.readInt();
            f.skip(0xC);
            int tm = f.readInt();
            int swizzle = f.readInt();
            f.skip(4);
            int pitch = f.readInt();
            f.skip(0x6C);
            int gtxOffset = readOffset(f);
            texture.data = GTX.swizzleBC(f.getSection(gtxOffset, size), texture.width, texture.height, format, tm, pitch, swizzle);

            switch (format)
            {
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC1_UNORM):
                    texture.type = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC1_SRGB):
                    texture.type = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt1Ext;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC2_UNORM):
                    texture.type = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC2_SRGB):
                    texture.type = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC3_UNORM):
                    texture.type = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC3_SRGB):
                    texture.type = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC4_UNORM):
                    texture.type = PixelInternalFormat.CompressedRedRgtc1;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC4_SNORM):
                    texture.type = PixelInternalFormat.CompressedSignedRedRgtc1;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC5_UNORM):
                    texture.type = PixelInternalFormat.CompressedRgRgtc2;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC5_SNORM):
                    texture.type = PixelInternalFormat.CompressedSignedRgRgtc2;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_TCS_R8_G8_B8_A8_UNORM):
                    texture.type = PixelInternalFormat.Rgba;
                    texture.utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                    break;
            }
            texture.display = loadImage(texture);
        }

        public class FTEX_Texture
        {
            public byte[] data;
            public int width, height;
            public int display = 0;
            public PixelInternalFormat type;
            public OpenTK.Graphics.OpenGL.PixelFormat utype;
        }
        public static int getImageSize(FTEX_Texture t)
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

        public static int loadImage(FTEX_Texture t)
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
