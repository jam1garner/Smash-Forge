using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using Syroot.NintenTools.Bfres;
using System.Windows.Forms;
using SFTex = SFGraphics.GLObjects.Textures;


namespace SmashForge
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

                tex.texture.display = texture2d;
                tex.display = tex.texture.display;
            }
        }
    }

    public class FTEX : TreeNode
    {
        public int width;
        public int height;
        public int format;
        public SFTex.Texture display;
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

            texture.data = Gtx.SwizzleBc(tex.Data, texture.width, texture.height, format, (int)tex.TileMode, pitch, swizzle);
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
                case ((int)Gtx.Gx2SurfaceFormat.Gx2SurfaceFormatTBc1Unorm):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    break;
                case ((int)Gtx.Gx2SurfaceFormat.Gx2SurfaceFormatTBc1Srgb):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                    break;
                case ((int)Gtx.Gx2SurfaceFormat.Gx2SurfaceFormatTBc2Unorm):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                    break;
                case ((int)Gtx.Gx2SurfaceFormat.Gx2SurfaceFormatTBc2Srgb):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
                    break;
                case ((int)Gtx.Gx2SurfaceFormat.Gx2SurfaceFormatTBc3Unorm):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                    break;
                case ((int)Gtx.Gx2SurfaceFormat.Gx2SurfaceFormatTBc3Srgb):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
                    break;
                case ((int)Gtx.Gx2SurfaceFormat.Gx2SurfaceFormatTBc4Unorm):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedRedRgtc1;
                    break;
                case ((int)Gtx.Gx2SurfaceFormat.Gx2SurfaceFormatTBc4Snorm):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedSignedRedRgtc1;
                    break;
                case ((int)Gtx.Gx2SurfaceFormat.Gx2SurfaceFormatTBc5Unorm):
                    texture.pixelInternalFormat = PixelInternalFormat.CompressedRgRgtc2;
                    break;
                case ((int)Gtx.Gx2SurfaceFormat.Gx2SurfaceFormatTBc5Snorm):
                    //OpenTK doesn't load BC5 SNORM textures right so I'll use the same decompress method bntx has
                    byte[] fixBC5 = DDS_Decompress.DecompressBC5(texture.data, texture.width, texture.height, true);
                    texture.data = fixBC5;
                    texture.pixelInternalFormat = PixelInternalFormat.Rgba;
                    texture.pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;

                    break;
                case ((int)Gtx.Gx2SurfaceFormat.Gx2SurfaceFormatTcsR8G8B8A8Unorm):
                    texture.pixelInternalFormat = PixelInternalFormat.Rgba;
                    texture.pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
                    break;
            }
        }

        public class FTEX_Texture
        {
            public byte[] data;
            public int width, height;
            public SFTex.Texture display;
            public PixelInternalFormat pixelInternalFormat;
            public PixelFormat pixelFormat;
            public PixelType pixelType = PixelType.UnsignedByte;
            public int mipMapCount;
            public List<byte[]> mipMapData = new List<byte[]>();
        }

        public static SFTex.Texture2D CreateTexture2D(FTEX_Texture tex, int surfaceIndex = 0)
        {
            bool compressedFormatWithMipMaps = SFTex.TextureFormats.TextureFormatTools.IsCompressed(tex.pixelInternalFormat);

            //Todo. Use mip maps from FTEX
            if (compressedFormatWithMipMaps)
            {
                if (tex.mipMapData.Count > 1)
                {
                    // Only load the first level and generate the rest.
                    SFTex.Texture2D texture = new SFTex.Texture2D();
                    texture.LoadImageData(tex.width, tex.height, tex.data, (InternalFormat)tex.pixelInternalFormat);
                    return texture;
                }
                else
                {
                    // Only load the first level and generate the rest.
                    SFTex.Texture2D texture = new SFTex.Texture2D();
                    texture.LoadImageData(tex.width, tex.height, tex.data, (InternalFormat)tex.pixelInternalFormat);
                    return texture;
                }
            }
            else
            {
                // Uncompressed.
                SFTex.Texture2D texture = new SFTex.Texture2D();
                texture.LoadImageData(tex.width, tex.height, tex.data,
                    new SFTex.TextureFormats.TextureFormatUncompressed(tex.pixelInternalFormat, tex.pixelFormat, tex.pixelType));
                return texture;
            }
        }
    }
}
