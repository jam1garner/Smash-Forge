using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Syroot.NintenTools.Bfres;
using System.Windows.Forms;
using SFTex = SFGraphics.GLObjects.Textures;


namespace Smash_Forge
{
    //Store FTEX instances in a container
    //This is to access them easier
    public class FTEXContainer
    {
        public Dictionary<string, FTEX> FTEXtextures = new Dictionary<string, FTEX>(); //To get instance of classes
        public Dictionary<string, SFTex.Texture> glTexByName = new Dictionary<string, SFTex.Texture>(); //Rendering

        public void RefreshGlTexturesByName()
        {
            glTexByName.Clear();

            foreach (FTEX tex in FTEXtextures.Values)
            {
                SFTex.Texture2D texture2d = FTEX.CreateTexture2D(tex.texture);
                glTexByName.Add(tex.Text, texture2d);

                tex.texture.display = texture2d.Id;
                tex.display = tex.texture.display;
            }
        }
    }

    public class FTEX : TreeNode
    {
        public int width;
        public int height;
        public int format;
        public int display;
        public byte[] reserve;


        public FTEX_Texture texture = new FTEX_Texture();
        public void ReadFTEX(Texture tex)
        {
            ImageKey = "texture";
            SelectedImageKey = "texture";

            reserve = tex.Data;

            texture.width = (int)tex.Width;
            texture.height = (int)tex.Height;
            format = (int)tex.Format;
            int swizzle = (int)tex.Swizzle;
            int pitch = (int)tex.Pitch;

            texture.data = GTX.swizzleBC(tex.Data, texture.width, texture.height, format, (int)tex.TileMode, pitch, swizzle);
            Text = tex.Name;


            //Setup variables for treenode data

            width = texture.width;
            height = texture.height;
            texture.mipMapCount = (int)tex.MipCount;

            FileData f = new FileData(tex.MipData);
            for (int level = 0; level < tex.MipCount; level++)
            {
                if (level != 0)
                {

                }

                //  byte[] mip = f.getSection((int)tex.MipOffsets[level - 1], (int)tex.MipOffsets[level + 1]);

                //  texture.mipMapData.Add(mip);
            }

            switch (format)
            {
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC1_UNORM):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC1_SRGB):
                    //fix SRGB textures.
                    byte[] fixBC1 = DDS_Decompress.DecompressBC1(texture.data, texture.width, texture.height, true);
                    texture.data = fixBC1;
                    texture.pixelInternalFormat = PixelInternalFormat.Rgba;
                    texture.pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC2_UNORM):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC2_SRGB):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC3_UNORM):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC3_SRGB):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC4_UNORM):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedRedRgtc1;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC4_SNORM):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedSignedRedRgtc1;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC5_UNORM):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedRgRgtc2;
                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_T_BC5_SNORM):
                    //OpenTK doesn't load BC5 SNORM textures right so I'll use the same decompress method bntx has
                    byte[] fixBC5 = DDS_Decompress.DecompressBC5(texture.data, texture.width, texture.height, true);
                    texture.data = fixBC5;
                    texture.pixelInternalFormat = PixelInternalFormat.Rgba;
                    texture.pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;

                    break;
                case ((int)GTX.GX2SurfaceFormat.GX2_SURFACE_FORMAT_TCS_R8_G8_B8_A8_UNORM):
                    texture.pixelInternalFormat = PixelInternalFormat.Rgba;
                    texture.pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                    break;
            }
        }

        public class FTEX_Texture
        {
            public byte[] data;
            public int width, height;
            public int display = 0;
            public PixelInternalFormat pixelInternalFormat;
            public PixelFormat pixelFormat;
            public PixelType pixelType = PixelType.UnsignedByte;
            public int mipMapCount;
            public List<byte[]> mipMapData = new List<byte[]>();
        }

        public static SFTex.Texture2D CreateTexture2D(FTEX_Texture tex, int surfaceIndex = 0)
        {
            bool compressedFormatWithMipMaps = SFGraphics.GLObjects.Textures.TextureFormatTools.IsCompressed(tex.pixelInternalFormat);

            //Todo. Use mip maps from FTEX
            if (compressedFormatWithMipMaps)
            {
                if (tex.mipMapData.Count > 1)
                {
                    // Only load the first level and generate the rest.
                    return new SFTex.Texture2D(tex.width, tex.height, tex.data, tex.mipMapData.Count,
                        (InternalFormat)tex.pixelInternalFormat);
                }
                else
                {
                    // Only load the first level and generate the rest.
                    return new SFTex.Texture2D(tex.width, tex.height, tex.data, tex.mipMapData.Count,
                        (InternalFormat)tex.pixelInternalFormat);
                }
            }
            else
            {
                // Uncompressed.
                return new SFTex.Texture2D(tex.width, tex.height, tex.data, tex.mipMapCount,
                    new SFTex.TextureFormatUncompressed(tex.pixelInternalFormat, tex.pixelFormat, tex.pixelType));
            }
        }
    }
}
