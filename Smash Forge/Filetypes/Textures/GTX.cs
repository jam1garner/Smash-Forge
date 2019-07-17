using System;

namespace SmashForge
{
    public class Gtx
    {

        public struct Gx2Surface
        {
            public int dim;
            public int width;
            public int height;
            public int depth;
            public int numMips;
            public int format;
            public int aa;
            public int use;
            public int resourceFlags;
            public int imageSize;
            public int imagePtr;
            public int pMem;
            public int mipSize;
            public int mipPtr;
            public int tileMode;
            public int swizzle;
            public int alignment;
            public int pitch;

            public byte[] data;

            public int[] mipOffset;
        };

        public enum Gx2SurfaceDimension
        {
            Gx2SurfaceDim1D = 0x0,
            Gx2SurfaceDim2D = 0x1,
            Gx2SurfaceDim3D = 0x2,
            Gx2SurfaceDimCube = 0x3,
            Gx2SurfaceDim1DArray = 0x4,
            Gx2SurfaceDim2DArray = 0x5,
            Gx2SurfaceDim2DMsaa = 0x6,
            Gx2SurfaceDim2DMsaaArray = 0x7,
            Gx2SurfaceDimFirst = 0x0,
            Gx2SurfaceDimLast = 0x7,
        };
        public enum Gx2SurfaceFormat
        {
            Gx2SurfaceFormatInvalid = 0x0,
            Gx2SurfaceFormatTcR8Unorm = 0x1,
            Gx2SurfaceFormatTcR8Uint = 0x101,
            Gx2SurfaceFormatTcR8Snorm = 0x201,
            Gx2SurfaceFormatTcR8Sint = 0x301,
            Gx2SurfaceFormatTR4G4Unorm = 0x2,
            Gx2SurfaceFormatTcdR16Unorm = 0x5,
            Gx2SurfaceFormatTcR16Uint = 0x105,
            Gx2SurfaceFormatTcR16Snorm = 0x205,
            Gx2SurfaceFormatTcR16Sint = 0x305,
            Gx2SurfaceFormatTcR16Float = 0x806,
            Gx2SurfaceFormatTcR8G8Unorm = 0x7,
            Gx2SurfaceFormatTcR8G8Uint = 0x107,
            Gx2SurfaceFormatTcR8G8Snorm = 0x207,
            Gx2SurfaceFormatTcR8G8Sint = 0x307,
            Gx2SurfaceFormatTcsR5G6B5Unorm = 0x8,
            Gx2SurfaceFormatTcR5G5B5A1Unorm = 0xA,
            Gx2SurfaceFormatTcR4G4B4A4Unorm = 0xB,
            Gx2SurfaceFormatTcA1B5G5R5Unorm = 0xC,
            Gx2SurfaceFormatTcR32Uint = 0x10D,
            Gx2SurfaceFormatTcR32Sint = 0x30D,
            Gx2SurfaceFormatTcdR32Float = 0x80E,
            Gx2SurfaceFormatTcR16G16Unorm = 0xF,
            Gx2SurfaceFormatTcR16G16Uint = 0x10F,
            Gx2SurfaceFormatTcR16G16Snorm = 0x20F,
            Gx2SurfaceFormatTcR16G16Sint = 0x30F,
            Gx2SurfaceFormatTcR16G16Float = 0x810,
            Gx2SurfaceFormatDD24S8Unorm = 0x11,
            Gx2SurfaceFormatTR24UnormX8 = 0x11,
            Gx2SurfaceFormatTX24G8Uint = 0x111,
            Gx2SurfaceFormatDD24S8Float = 0x811,
            Gx2SurfaceFormatTcR11G11B10Float = 0x816,
            Gx2SurfaceFormatTcsR10G10B10A2Unorm = 0x19,
            Gx2SurfaceFormatTcR10G10B10A2Uint = 0x119,
            Gx2SurfaceFormatTR10G10B10A2Snorm = 0x219,
            Gx2SurfaceFormatTcR10G10B10A2Snorm = 0x219,
            Gx2SurfaceFormatTcR10G10B10A2Sint = 0x319,
            Gx2SurfaceFormatTcsR8G8B8A8Unorm = 0x1A,
            Gx2SurfaceFormatTcR8G8B8A8Uint = 0x11A,
            Gx2SurfaceFormatTcR8G8B8A8Snorm = 0x21A,
            Gx2SurfaceFormatTcR8G8B8A8Sint = 0x31A,
            Gx2SurfaceFormatTcsR8G8B8A8Srgb = 0x41A,
            Gx2SurfaceFormatTcsA2B10G10R10Unorm = 0x1B,
            Gx2SurfaceFormatTcA2B10G10R10Uint = 0x11B,
            Gx2SurfaceFormatDD32FloatS8UintX24 = 0x81C,
            Gx2SurfaceFormatTR32FloatX8X24 = 0x81C,
            Gx2SurfaceFormatTX32G8UintX24 = 0x11C,
            Gx2SurfaceFormatTcR32G32Uint = 0x11D,
            Gx2SurfaceFormatTcR32G32Sint = 0x31D,
            Gx2SurfaceFormatTcR32G32Float = 0x81E,
            Gx2SurfaceFormatTcR16G16B16A16Unorm = 0x1F,
            Gx2SurfaceFormatTcR16G16B16A16Uint = 0x11F,
            Gx2SurfaceFormatTcR16G16B16A16Snorm = 0x21F,
            Gx2SurfaceFormatTcR16G16B16A16Sint = 0x31F,
            Gx2SurfaceFormatTcR16G16B16A16Float = 0x820,
            Gx2SurfaceFormatTcR32G32B32A32Uint = 0x122,
            Gx2SurfaceFormatTcR32G32B32A32Sint = 0x322,
            Gx2SurfaceFormatTcR32G32B32A32Float = 0x823,
            Gx2SurfaceFormatTBc1Unorm = 0x31,
            Gx2SurfaceFormatTBc1Srgb = 0x431,
            Gx2SurfaceFormatTBc2Unorm = 0x32,
            Gx2SurfaceFormatTBc2Srgb = 0x432,
            Gx2SurfaceFormatTBc3Unorm = 0x33,
            Gx2SurfaceFormatTBc3Srgb = 0x433,
            Gx2SurfaceFormatTBc4Unorm = 0x34,
            Gx2SurfaceFormatTBc4Snorm = 0x234,
            Gx2SurfaceFormatTBc5Unorm = 0x35,
            Gx2SurfaceFormatTBc5Snorm = 0x235,
            Gx2SurfaceFormatTNv12Unorm = 0x81,
            Gx2SurfaceFormatFirst = 0x1,
            Gx2SurfaceFormatLast = 0x83F,
        };
        public enum Gx2AaMode
        {
            Gx2AaMode1X = 0x0,
            Gx2AaMode2X = 0x1,
            Gx2AaMode4X = 0x2,
            Gx2AaMode8X = 0x3,
            Gx2AaModeFirst = 0x0,
            Gx2AaModeLast = 0x3,
        };
        public enum Gx2SurfaceUse : uint
        {
            Gx2SurfaceUseTexture = 0x1,
            Gx2SurfaceUseColorBuffer = 0x2,
            Gx2SurfaceUseDepthBuffer = 0x4,
            Gx2SurfaceUseScanBuffer = 0x8,
            Gx2SurfaceUseFtv = 0x80000000,
            Gx2SurfaceUseColorBufferTexture = 0x3,
            Gx2SurfaceUseDepthBufferTexture = 0x5,
            Gx2SurfaceUseColorBufferFtv = 0x80000002,
            Gx2SurfaceUseColorBufferTextureFtv = 0x80000003,
            Gx2SurfaceUseFirst = 0x1,
            Gx2SurfaceUseLast = 0x8,
        };
        public enum Gx2RResourceFlags
        {
            Gx2RResourceFlagsNone = 0x0,
            Gx2RBindNone = 0x0,
            Gx2RBindTexture = 0x1,
            Gx2RBindColorBuffer = 0x2,
            Gx2RBindDepthBuffer = 0x4,
            Gx2RBindScanBuffer = 0x8,
            Gx2RBindVertexBuffer = 0x10,
            Gx2RBindIndexBuffer = 0x20,
            Gx2RBindUniformBlock = 0x40,
            Gx2RBindShaderProgram = 0x80,
            Gx2RBindStreamOutput = 0x100,
            Gx2RBindDisplayList = 0x200,
            Gx2RBindGsRing = 0x400,
            Gx2RUsageNone = 0x0,
            Gx2RUsageCpuRead = 0x800,
            Gx2RUsageCpuWrite = 0x1000,
            Gx2RUsageGpuRead = 0x2000,
            Gx2RUsageGpuWrite = 0x4000,
            Gx2RUsageDmaRead = 0x8000,
            Gx2RUsageDmaWrite = 0x10000,
            Gx2RUsageForceMem1 = 0x20000,
            Gx2RUsageForceMem2 = 0x40000,
            Gx2RUsageMemDefault = 0x0,
            Gx2RUsageCpuReadwrite = 0x1800,
            Gx2RUsageGpuReadwrite = 0x6000,
            Gx2RUsageNonCpuWrite = 0x14000,
            Gx2ROptionNone = 0x0,
            Gx2ROptionIgnoreInUse = 0x80000,
            Gx2ROptionFirst = 0x80000,
            Gx2ROptionNoCpuInvalidate = 0x100000,
            Gx2ROptionNoGpuInvalidate = 0x200000,
            Gx2ROptionLockReadonly = 0x400000,
            Gx2ROptionNoTouchDestroy = 0x800000,
            Gx2ROptionLast = 0x800000,
            Gx2ROptionNoInvalidate = 0x300000,
            Gx2ROptionMask = 0xF80000,
            Gx2RResourceFlagReserved2 = 0x10000000,
            Gx2RResourceFlagReserved1 = 0x20000000,
            Gx2RResourceFlagReserved0 = 0x40000000,
        };
        public enum Gx2TileMode
        {
            Gx2TileModeDefault = 0x0,
            Gx2TileModeLinearSpecial = 0x10,
            Gx2TileModeDefaultFix2197 = 0x20,
            Gx2TileModeLinearAligned = 0x1,
            Gx2TileMode1DTiledThin1 = 0x2,
            Gx2TileMode1DTiledThick = 0x3,
            Gx2TileMode2DTiledThin1 = 0x4,
            Gx2TileMode2DTiledThin2 = 0x5,
            Gx2TileMode2DTiledThin4 = 0x6,
            Gx2TileMode2DTiledThick = 0x7,
            Gx2TileMode2BTiledThin1 = 0x8,
            Gx2TileMode2BTiledThin2 = 0x9,
            Gx2TileMode2BTiledThin4 = 0xA,
            Gx2TileMode2BTiledThick = 0xB,
            Gx2TileMode3DTiledThin1 = 0xC,
            Gx2TileMode3DTiledThick = 0xD,
            Gx2TileMode3BTiledThin1 = 0xE,
            Gx2TileMode3BTiledThick = 0xF,
            Gx2TileModeFirst = 0x0,
            Gx2TileModeLast = 0x20,
        };

        public enum AddrTileMode
        {
            AddrTmLinearGeneral = 0x0,
            AddrTmLinearAligned = 0x1,
            AddrTm1DTiledThin1 = 0x2,
            AddrTm1DTiledThick = 0x3,
            AddrTm2DTiledThin1 = 0x4,
            AddrTm2DTiledThin2 = 0x5,
            AddrTm2DTiledThin4 = 0x6,
            AddrTm2DTiledThick = 0x7,
            AddrTm2BTiledThin1 = 0x8,
            AddrTm2BTiledThin2 = 0x9,
            AddrTm2BTiledThin4 = 0x0A,
            AddrTm2BTiledThick = 0x0B,
            AddrTm3DTiledThin1 = 0x0C,
            AddrTm3DTiledThick = 0x0D,
            AddrTm3BTiledThin1 = 0x0E,
            AddrTm3BTiledThick = 0x0F,
            AddrTm2DTiledXthick = 0x10,
            AddrTm3DTiledXthick = 0x11,
            AddrTmPowerSave = 0x12,
            AddrTmCount = 0x13,
        }
        public enum AddrTileType
        {
            AddrDisplayable = 0,
            AddrNonDisplayable = 1,
            AddrDepthSampleOrder = 2,
            AddrThickTiling = 3,
        }
        public enum AddrPipeCfg
        {
            AddrPipecfgInvalid = 0x0,
            AddrPipecfgP2 = 0x1,
            AddrPipecfgP48X16 = 0x5,
            AddrPipecfgP416X16 = 0x6,
            AddrPipecfgP416X32 = 0x7,
            AddrPipecfgP432X32 = 0x8,
            AddrPipecfgP816X168X16 = 0x9,
            AddrPipecfgP816X328X16 = 0xA,
            AddrPipecfgP832X328X16 = 0xB,
            AddrPipecfgP816X3216X16 = 0xC,
            AddrPipecfgP832X3216X16 = 0xD,
            AddrPipecfgP832X3216X32 = 0xE,
            AddrPipecfgP832X6432X32 = 0xF,
            AddrPipecfgMax = 0x10,
        }
        public enum AddrFormat
        {
            AddrFmtInvalid = 0x0,
            AddrFmt8 = 0x1,
            AddrFmt44 = 0x2,
            AddrFmt332 = 0x3,
            AddrFmtReserved4 = 0x4,
            AddrFmt16 = 0x5,
            AddrFmt16Float = 0x6,
            AddrFmt88 = 0x7,
            AddrFmt565 = 0x8,
            AddrFmt655 = 0x9,
            AddrFmt1555 = 0xA,
            AddrFmt4444 = 0xB,
            AddrFmt5551 = 0xC,
            AddrFmt32 = 0xD,
            AddrFmt32Float = 0xE,
            AddrFmt1616 = 0xF,
            AddrFmt1616Float = 0x10,
            AddrFmt824 = 0x11,
            AddrFmt824Float = 0x12,
            AddrFmt248 = 0x13,
            AddrFmt248Float = 0x14,
            AddrFmt101111 = 0x15,
            AddrFmt101111Float = 0x16,
            AddrFmt111110 = 0x17,
            AddrFmt111110Float = 0x18,
            AddrFmt2101010 = 0x19,
            AddrFmt8888 = 0x1A,
            AddrFmt1010102 = 0x1B,
            AddrFmtX24832Float = 0x1C,
            AddrFmt3232 = 0x1D,
            AddrFmt3232Float = 0x1E,
            AddrFmt16161616 = 0x1F,
            AddrFmt16161616Float = 0x20,
            AddrFmtReserved33 = 0x21,
            AddrFmt32323232 = 0x22,
            AddrFmt32323232Float = 0x23,
            AddrFmtReserved36 = 0x24,
            AddrFmt1 = 0x25,
            AddrFmt1Reversed = 0x26,
            AddrFmtGbGr = 0x27,
            AddrFmtBgRg = 0x28,
            AddrFmt32As8 = 0x29,
            AddrFmt32As88 = 0x2A,
            AddrFmt5999Sharedexp = 0x2B,
            AddrFmt888 = 0x2C,
            AddrFmt161616 = 0x2D,
            AddrFmt161616Float = 0x2E,
            AddrFmt323232 = 0x2F,
            AddrFmt323232Float = 0x30,
            AddrFmtBc1 = 0x31,
            AddrFmtBc2 = 0x32,
            AddrFmtBc3 = 0x33,
            AddrFmtBc4 = 0x34,
            AddrFmtBc5 = 0x35,
            AddrFmtBc6 = 0x36,
            AddrFmtBc7 = 0x37,
            AddrFmt32As32323232 = 0x38,
            AddrFmtApc3 = 0x39,
            AddrFmtApc4 = 0x3A,
            AddrFmtApc5 = 0x3B,
            AddrFmtApc6 = 0x3C,
            AddrFmtApc7 = 0x3D,
            AddrFmtCtx1 = 0x3E,
            AddrFmtReserved63 = 0x3F,
        };

        private static byte[] formatHwInfo = {
            0x00, 0x00, 0x00, 0x01, 0x08, 0x03, 0x00, 0x01, 0x08, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x00, 0x00, 0x00, 0x01, 0x10, 0x07, 0x00, 0x00, 0x10, 0x03, 0x00, 0x01, 0x10, 0x03, 0x00, 0x01,
            0x10, 0x0B, 0x00, 0x01, 0x10, 0x01, 0x00, 0x01, 0x10, 0x03, 0x00, 0x01, 0x10, 0x03, 0x00, 0x01,
            0x10, 0x03, 0x00, 0x01, 0x20, 0x03, 0x00, 0x00, 0x20, 0x07, 0x00, 0x00, 0x20, 0x03, 0x00, 0x00,
            0x20, 0x03, 0x00, 0x01, 0x20, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x03, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x20, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x00, 0x00, 0x00, 0x01, 0x20, 0x0B, 0x00, 0x01, 0x20, 0x0B, 0x00, 0x01, 0x20, 0x0B, 0x00, 0x01,
            0x40, 0x05, 0x00, 0x00, 0x40, 0x03, 0x00, 0x00, 0x40, 0x03, 0x00, 0x00, 0x40, 0x03, 0x00, 0x00,
            0x40, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x80, 0x03, 0x00, 0x00, 0x80, 0x03, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x10, 0x01, 0x00, 0x00,
            0x10, 0x01, 0x00, 0x00, 0x20, 0x01, 0x00, 0x00, 0x20, 0x01, 0x00, 0x00, 0x20, 0x01, 0x00, 0x00,
            0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x60, 0x01, 0x00, 0x00,
            0x60, 0x01, 0x00, 0x00, 0x40, 0x01, 0x00, 0x01, 0x80, 0x01, 0x00, 0x01, 0x80, 0x01, 0x00, 0x01,
            0x40, 0x01, 0x00, 0x01, 0x80, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        /*---------------------------------------
         * 
         * Code ported from AboodXD's GTX Extractor https://github.com/aboood40091/GTX-Extractor/blob/master/gtx_extract.py
         * 
         * With help by Aelan!
         * 
         *---------------------------------------*/

        /*var s_textureFormats[] = {
        // internalFormat,  gxFormat,                                 glFormat,                         fourCC,    nutFormat, name, bpp, compressed
        { FORMAT_RGBA_8888, GX2_SURFACE_FORMAT_TCS_R8_G8_B8_A8_UNORM, GL_RGBA8,                         0x00000000, 0x11, "RGBA_8888", 0x20, 0 },
        { FORMAT_ABGR_8888, GX2_SURFACE_FORMAT_TCS_R8_G8_B8_A8_UNORM, GL_RGBA8,                         0x00000000, 0x0E, "ABGR_8888 (WIP)", 0x20, 0 },
        { FORMAT_DXT1,      GX2_SURFACE_FORMAT_T_BC1_UNORM,           GL_COMPRESSED_RGBA_S3TC_DXT1_EXT, 0x31545844, 0x00, "DXT1",  0x40,     1 },
        { FORMAT_DXT3,      GX2_SURFACE_FORMAT_T_BC2_UNORM,           GL_COMPRESSED_RGBA_S3TC_DXT3_EXT, 0x33545844, 0x01, "DXT3",  0x80,     1 },
        { FORMAT_DXT5,      GX2_SURFACE_FORMAT_T_BC3_UNORM,           GL_COMPRESSED_RGBA_S3TC_DXT5_EXT, 0x35545844, 0x02, "DXT5",  0x80,     1 },
        { FORMAT_ATI1,      GX2_SURFACE_FORMAT_T_BC4_UNORM,           GL_COMPRESSED_RED_RGTC1,          0x31495441, 0x15, "ATI1",  0x40,     1 },
        { FORMAT_ATI2,      GX2_SURFACE_FORMAT_T_BC5_UNORM,           GL_COMPRESSED_RG_RGTC2,           0x32495441, 0x16, "ATI2",  0x80,     1 },
        { FORMAT_INVALID,   GX2_SURFACE_FORMAT_INVALID,               0,                                0xFFFFFFFF, 0x00, nullptr, 0x00,     0 }
    };*/


        public static byte[] SwizzleBc(byte[] data, int width, int height, int format, int tileMode, int pitch, int swizzle)
        {
            Gx2Surface sur = new Gx2Surface();
            sur.width = width;
            sur.height = height;
            sur.tileMode = tileMode;
            sur.format = format;
            sur.swizzle = swizzle;
            sur.pitch = pitch;
            sur.data = data;
            sur.imageSize = data.Length;
            //return swizzleBC(sur);
            return SwizzleSurface(sur, (Gx2SurfaceFormat)sur.format != Gx2SurfaceFormat.Gx2SurfaceFormatTcsR8G8B8A8Unorm &
                (Gx2SurfaceFormat)sur.format != Gx2SurfaceFormat.Gx2SurfaceFormatTcsR8G8B8A8Srgb);
        }

        public static int GetBpp(int i)
        {
            switch ((Gx2SurfaceFormat)i)
            {
                case Gx2SurfaceFormat.Gx2SurfaceFormatTcR5G5B5A1Unorm:
                    return 0x10;
                case Gx2SurfaceFormat.Gx2SurfaceFormatTcsR8G8B8A8Unorm:
                    return 0x20;
                case Gx2SurfaceFormat.Gx2SurfaceFormatTBc1Unorm:
                case Gx2SurfaceFormat.Gx2SurfaceFormatTBc4Unorm:
                case Gx2SurfaceFormat.Gx2SurfaceFormatTBc1Srgb:
                case Gx2SurfaceFormat.Gx2SurfaceFormatTBc4Snorm:
                    return 0x40;
                case Gx2SurfaceFormat.Gx2SurfaceFormatTBc2Unorm:
                case Gx2SurfaceFormat.Gx2SurfaceFormatTBc3Unorm:
                case Gx2SurfaceFormat.Gx2SurfaceFormatTBc5Unorm:
                case Gx2SurfaceFormat.Gx2SurfaceFormatTBc2Srgb:
                case Gx2SurfaceFormat.Gx2SurfaceFormatTBc3Srgb:
                case Gx2SurfaceFormat.Gx2SurfaceFormatTBc5Snorm:
                    return 0x80;
            }
            return -1;
        }

        public static byte[] SwizzleSurface(Gx2Surface surface, bool isCompressed)
        {
            byte[] original = new byte[surface.data.Length];

            surface.data.CopyTo(original, 0);

            int swizzle = ((surface.swizzle >> 8) & 1) + (((surface.swizzle >> 9) & 3) << 1);
            int blockSize;
            int width = surface.width;
            int height = surface.height;

            int format = GetBpp(surface.format);
            Console.WriteLine(((Gx2SurfaceFormat)surface.format).ToString());

            if (isCompressed)
            {
                width /= 4;
                height /= 4;

                if ((Gx2SurfaceFormat)surface.format == Gx2SurfaceFormat.Gx2SurfaceFormatTBc1Unorm ||
                    (Gx2SurfaceFormat)surface.format == Gx2SurfaceFormat.Gx2SurfaceFormatTBc1Srgb ||
                    (Gx2SurfaceFormat)surface.format == Gx2SurfaceFormat.Gx2SurfaceFormatTBc4Unorm ||
                    (Gx2SurfaceFormat)surface.format == Gx2SurfaceFormat.Gx2SurfaceFormatTBc4Snorm)
                {
                    blockSize = 8;
                }
                else
                {
                    blockSize = 16;
                }
            }
            else
            {
                /*if ((GX2SurfaceFormat)surface.format == GX2SurfaceFormat.GX2_SURFACE_FORMAT_TC_R5_G5_B5_A1_UNORM)
                {
                    blockSize = format / 4;
                }
                else*/
                blockSize = format / 8;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = SurfaceAddrFromCoordMacroTiled(x, y, format, surface.pitch, swizzle);
                    int size = (y * width + x) * blockSize;

                    for (int k = 0; k < blockSize; k++)
                    {
                        if (pos + k >= original.Length || size + k >= surface.data.Length)
                        {
                            Console.WriteLine("Break Point " + size + " " + pos);
                            break;
                        }
                        surface.data[size + k] = original[pos + k];
                    }
                }
            }
            return surface.data;
        }

        public static int SurfaceAddrFromCoordMacroTiled(int x, int y, int bpp, int pitch, int swizzle)
        {
            int pixelIndex = ComputePixelIndexWithinMicroTile(x, y, bpp);
            int elemOffset = (bpp * pixelIndex) >> 3;

            int pipe = ComputePipeFromCoordWoRotation(x, y);
            int bank = ComputeBankFromCoordWoRotation(x, y);
            int bankPipe = ((pipe + 2 * bank) ^ swizzle) % 9;

            pipe = bankPipe % 2;
            bank = bankPipe / 2;

            int macroTileBytes = (bpp * 512 + 7) >> 3;
            int macroTileOffset = (x / 32 + pitch / 32 * (y / 16)) * macroTileBytes;

            int unk1 = elemOffset + (macroTileOffset >> 3);
            int unk2 = unk1 & ~0xFF;

            return (unk2 << 3) | (0xFF & unk1) | (pipe << 8) | (bank << 9);
        }

        public static int ComputePixelIndexWithinMicroTile(int x, int y, int bpp)
        {
            int bits = ((x & 4) << 1) | ((y & 2) << 3) | ((y & 4) << 3);

            if (bpp == 0x20 || bpp == 0x60)
            {
                bits |= (x & 1) | (x & 2) | ((y & 1) << 2);
            }
            else if (bpp == 0x40)
            {
                bits |= (x & 1) | ((y & 1) << 1) | ((x & 2) << 1);
            }
            else if (bpp == 0x80)
            {
                bits |= (y & 1) | ((x & 1) << 1) | ((x & 2) << 1);
            }

            return bits;
        }

        public static int GetFormatBpp(int format)
        {
            int hwFormat = format & 0x3F;
            return formatHwInfo[hwFormat * 4];
        }

        public static int ComputeSurfaceThickness(AddrTileMode tileMode)
        {
            switch (tileMode)
            {
                case AddrTileMode.AddrTm1DTiledThick:
                case AddrTileMode.AddrTm2DTiledThick:
                case AddrTileMode.AddrTm2BTiledThick:
                case AddrTileMode.AddrTm3DTiledThick:
                case AddrTileMode.AddrTm3BTiledThick:
                    {
                        return 4;
                    }
                case AddrTileMode.AddrTm2DTiledXthick:
                case AddrTileMode.AddrTm3DTiledXthick:
                    {
                        return 8;
                    }
            }

            return 1;
        }

        public static int IsThickMacroTiled(AddrTileMode tileMode)
        {
            switch (tileMode)
            {
                case AddrTileMode.AddrTm2DTiledThick:
                case AddrTileMode.AddrTm2BTiledThick:
                case AddrTileMode.AddrTm3DTiledThick:
                case AddrTileMode.AddrTm3BTiledThick:
                    {
                        return 1;
                    }
            }

            return 0;
        }

        public static int IsBankSwappedTileMode(AddrTileMode tileMode)
        {
            switch (tileMode)
            {
                case AddrTileMode.AddrTm2BTiledThin1:
                case AddrTileMode.AddrTm2BTiledThin2:
                case AddrTileMode.AddrTm2BTiledThin4:
                case AddrTileMode.AddrTm2BTiledThick:
                case AddrTileMode.AddrTm3BTiledThin1:
                case AddrTileMode.AddrTm3BTiledThick:
                    {
                        return 1;
                    }
            }
            return 0;
        }

        public static int ComputeSurfaceRotationFromTileMode(AddrTileMode tileMode)
        {
            switch ((int)tileMode)
            {
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                    {
                        return 2;
                    }
                case 12:
                case 13:
                case 14:
                case 15:
                    {
                        return 1;
                    }
            }

            return 0;
        }

        public static int ComputePipeFromCoordWoRotation(int x, int y)
        {
            int pipe = ((y >> 3) ^ (x >> 3)) & 1;
            return pipe;
        }

        public static int ComputeBankFromCoordWoRotation(int x, int y)
        {
            int bankBit0 = ((y / (16 * 2)) ^ (x >> 3)) & 1;
            int bank = bankBit0 | 2 * (((y / (8 * 2)) ^ (x >> 4)) & 1);

            return bank;
        }

        public static int ComputeMacroTileAspectRatio(AddrTileMode tileMode)
        {
            switch (tileMode)
            {
                case AddrTileMode.AddrTm2BTiledThin1:
                case AddrTileMode.AddrTm3DTiledThin1:
                case AddrTileMode.AddrTm3BTiledThin1:
                    {
                        return 1;
                    }
                case AddrTileMode.AddrTm2DTiledThin2:
                case AddrTileMode.AddrTm2BTiledThin2:
                    {
                        return 2;
                    }
                case AddrTileMode.AddrTm2DTiledThin4:
                case AddrTileMode.AddrTm2BTiledThin4:
                    {
                        return 4;
                    }
            }

            return 1;
        }

        public static int ComputeSurfaceBankSwappedWidth(AddrTileMode tileMode, int bpp, int numSamples, int pitch, int pSlicesPerTile)
        {
            int bankSwapWidth = 0;
            int numBanks = 4;
            int numPipes = 2;
            int swapSize = 256;
            int rowSize = 2048;
            int splitSize = 2048;
            int groupSize = 256;
            int slicesPerTile = 1;
            int bytesPerSample = 8 * bpp & 0x1FFFFFFF;
            int samplesPerTile = splitSize / bytesPerSample;

            if ((splitSize / bytesPerSample) != 0)
            {
                slicesPerTile = numSamples / samplesPerTile;
                if ((numSamples / samplesPerTile) == 0)
                {
                    slicesPerTile = 1;
                }
            }

            if (pSlicesPerTile != 0)
            {
                pSlicesPerTile = slicesPerTile;
            }

            if (IsThickMacroTiled(tileMode) == 1)
            {
                numSamples = 4;
            }

            int bytesPerTileSlice = numSamples * bytesPerSample / slicesPerTile;

            if (IsBankSwappedTileMode(tileMode) == 1)
            {
                int v7;
                int v8;
                int v9;

                int factor = ComputeMacroTileAspectRatio(tileMode);
                int swapTiles = (swapSize >> 1) / bpp;

                if (swapTiles != 0)
                {
                    v9 = swapTiles;
                }
                else
                {
                    v9 = 1;
                }

                int swapWidth = v9 * 8 * numBanks;
                int heightBytes = numSamples * factor * numPipes * bpp / slicesPerTile;
                int swapMax = numPipes * numBanks * rowSize / heightBytes;
                int swapMin = groupSize * 8 * numBanks / bytesPerTileSlice;

                if (swapMax >= swapWidth)
                {
                    if (swapMin <= swapWidth)
                    {
                        v7 = swapWidth;
                    }
                    else
                    {
                        v7 = swapMin;
                    }

                    v8 = v7;
                }
                else
                {
                    v8 = swapMax;
                }

                bankSwapWidth = v8;

                while (bankSwapWidth >= (2 * pitch))
                {
                    bankSwapWidth >>= 1;
                }
            }

            return bankSwapWidth;
        }

        public static int ComputePixelIndexWithinMicroTile(int x, int y, int z, int bpp, AddrTileMode tileMode, int microTileType)
        {
            int pixelBit0 = 0;
            int pixelBit1 = 0;
            int pixelBit2 = 0;
            int pixelBit3 = 0;
            int pixelBit4 = 0;
            int pixelBit5 = 0;
            int pixelBit6 = 0;
            int pixelBit7 = 0;
            int pixelBit8 = 0;
            int thickness = ComputeSurfaceThickness(tileMode);

            if (microTileType == 3)
            {
                pixelBit0 = x & 1;
                pixelBit1 = y & 1;
                pixelBit2 = z & 1;
                pixelBit3 = (x & 2) >> 1;
                pixelBit4 = (y & 2) >> 1;
                pixelBit5 = (z & 2) >> 1;
                pixelBit6 = (x & 4) >> 2;
                pixelBit7 = (y & 4) >> 2;
            }
            else
            {
                if (microTileType != 0)
                {
                    pixelBit0 = x & 1;
                    pixelBit1 = y & 1;
                    pixelBit2 = (x & 2) >> 1;
                    pixelBit3 = (y & 2) >> 1;
                    pixelBit4 = (x & 4) >> 2;
                    pixelBit5 = (y & 4) >> 2;
                }
                else
                {
                    if (bpp == 0x08)
                    {
                        pixelBit0 = x & 1;
                        pixelBit1 = (x & 2) >> 1;
                        pixelBit2 = (x & 4) >> 2;
                        pixelBit3 = (y & 2) >> 1;
                        pixelBit4 = y & 1;
                        pixelBit5 = (y & 4) >> 2;
                    }
                    else if (bpp == 0x10)
                    {
                        pixelBit0 = x & 1;
                        pixelBit1 = (x & 2) >> 1;
                        pixelBit2 = (x & 4) >> 2;
                        pixelBit3 = y & 1;
                        pixelBit4 = (y & 2) >> 1;
                        pixelBit5 = (y & 4) >> 2;
                    }
                    else if (bpp == 0x20 || bpp == 0x60)
                    {
                        pixelBit0 = x & 1;
                        pixelBit1 = (x & 2) >> 1;
                        pixelBit2 = y & 1;
                        pixelBit3 = (x & 4) >> 2;
                        pixelBit4 = (y & 2) >> 1;
                        pixelBit5 = (y & 4) >> 2;
                    }
                    else if (bpp == 0x40)
                    {
                        pixelBit0 = x & 1;
                        pixelBit1 = y & 1;
                        pixelBit2 = (x & 2) >> 1;
                        pixelBit3 = (x & 4) >> 2;
                        pixelBit4 = (y & 2) >> 1;
                        pixelBit5 = (y & 4) >> 2;
                    }
                    else if (bpp == 0x80)
                    {
                        pixelBit0 = y & 1;
                        pixelBit1 = x & 1;
                        pixelBit2 = (x & 2) >> 1;
                        pixelBit3 = (x & 4) >> 2;
                        pixelBit4 = (y & 2) >> 1;
                        pixelBit5 = (y & 4) >> 2;
                    }
                    else
                    {
                        pixelBit0 = x & 1;
                        pixelBit1 = (x & 2) >> 1;
                        pixelBit2 = y & 1;
                        pixelBit3 = (x & 4) >> 2;
                        pixelBit4 = (y & 2) >> 1;
                        pixelBit5 = (y & 4) >> 2;
                    }
                }

                if (thickness > 1)
                {
                    pixelBit6 = z & 1;
                    pixelBit7 = (z & 2) >> 1;
                }
            }

            if (thickness == 8)
            {
                pixelBit8 = (z & 4) >> 2;
            }

            return pixelBit0 |
                (pixelBit8 << 8) |
                (pixelBit7 << 7) |
                (pixelBit6 << 6) |
                (pixelBit5 << 5) |
                (pixelBit4 << 4) |
                (pixelBit3 << 3) |
                (pixelBit2 << 2) |
                (pixelBit1 << 1);
        }

        public static int SurfaceAddrFromCoordMacroTiled(
            int x, int y, int slice, int sample, int bpp,
            int pitch, int height, int numSamples, AddrTileMode tileMode,
            int isDepth, int tileBase, int compBits,
            int pipeSwizzle, int bankSwizzle
        )
        {
            const int numPipes = 2;
            const int numBanks = 4;
            const int numGroupBits = 8;
            const int numPipeBits = 1;
            const int numBankBits = 2;

            int microTileThickness = ComputeSurfaceThickness(tileMode);
            int microTileBits = numSamples * bpp * (microTileThickness * (8 * 8));
            int microTileBytes = microTileBits >> 3;
            int microTileType = (isDepth == 1) ? 1 : 0;
            int pixelIndex = ComputePixelIndexWithinMicroTile(x, y, slice, bpp, tileMode, microTileType);

            int sampleOffset;
            int pixelOffset;
            if (isDepth == 1)
            {
                if (compBits != 0 && compBits != bpp)
                {
                    sampleOffset = tileBase + compBits * sample;
                    pixelOffset = numSamples * compBits * pixelIndex;
                }
                else
                {
                    sampleOffset = bpp * sample;
                    pixelOffset = numSamples * compBits * pixelIndex;
                }
            }
            else
            {
                sampleOffset = sample * (microTileBits / numSamples);
                pixelOffset = bpp * pixelIndex;
            }

            int elemOffset = pixelOffset + sampleOffset;
            int bytesPerSample = microTileBytes / numSamples;

            int samplesPerSlice;
            int numSampleSplits;
            int sampleSlice;

            if (numSamples <= 1 || microTileBytes <= 2048)
            {
                samplesPerSlice = numSamples;
                numSampleSplits = 1;
                sampleSlice = 0;
            }
            else
            {
                samplesPerSlice = 2048 / bytesPerSample;
                numSampleSplits = numSamples / samplesPerSlice;
                numSamples = samplesPerSlice;
                sampleSlice = elemOffset / (microTileBits / numSampleSplits);
                elemOffset %= microTileBits / numSampleSplits;
            }

            elemOffset >>= 3;

            int pipe = ComputePipeFromCoordWoRotation(x, y);
            int bank = ComputeBankFromCoordWoRotation(x, y);
            int bankPipe = pipe + numPipes * bank;
            int rotation = ComputeSurfaceRotationFromTileMode(tileMode);
            int swizzle = pipeSwizzle + numPipes * bankSwizzle;
            int sliceIn = slice;

            if (IsThickMacroTiled(tileMode) == 1)
            {
                sliceIn >>= 2;
            }

            bankPipe ^= numPipes * sampleSlice * ((numBanks >> 1) + 1) ^ (swizzle + sliceIn * rotation);
            bankPipe %= numPipes * numBanks;
            pipe = bankPipe % numPipes;
            bank = bankPipe / numPipes;

            int sliceBytes = (height * pitch * microTileThickness * bpp * numSamples + 7) / 8;
            int sliceOffset = sliceBytes * ((sampleSlice + numSampleSplits * slice) / microTileThickness);
            int macroTilePitch = 8 * 4; // m_banks
            int macroTileHeight = 8 * 2; // m_pipes
            int v18 = (int)tileMode - 5;

            if ((int)tileMode == 5 || (int)tileMode == 9)
            {
                macroTilePitch >>= 1;
                macroTileHeight *= 2;
            }
            else if ((int)tileMode == 6 || (int)tileMode == 10)
            {
                macroTilePitch >>= 2;
                macroTileHeight *= 4;
            }

            int macroTilesPerRow = pitch / macroTilePitch;
            int macroTileBytes = (numSamples * microTileThickness * bpp * macroTileHeight * macroTilePitch + 7) >> 3;
            int macroTileIndexX = x / macroTilePitch;
            int macroTileIndexY = y / macroTileHeight;
            int macroTileOffset = (x / macroTilePitch + pitch / macroTilePitch * (y / macroTileHeight)) * macroTileBytes;

            int bankSwapWidth;
            int swapIndex;
            int bankMask;

            byte[] bankSwapOrder = { 0, 1, 3, 2 };
            switch ((int)tileMode)
            {
                case 8:
                case 9:
                case 10:
                case 11:
                case 14:
                case 15:
                    {
                        bankSwapWidth = ComputeSurfaceBankSwappedWidth(tileMode, bpp, numSamples, pitch, 0);
                        swapIndex = macroTilePitch * macroTileIndexX / bankSwapWidth;
                        bankMask = 3; // m_banks-1
                        bank ^= bankSwapOrder[swapIndex & bankMask];
                    }
                    break;
            }

            int p4 = pipe << numGroupBits;
            int p5 = bank << (numPipeBits + numGroupBits);
            int numSwizzleBits = numBankBits + numPipeBits;
            int unk1 = (macroTileOffset + sliceOffset) >> numSwizzleBits;
            int unk2 = ~((1 << numGroupBits) - 1);
            int unk3 = (elemOffset + unk1) & unk2;
            int groupMask = (1 << numGroupBits) - 1;
            int offset1 = macroTileOffset + sliceOffset;
            int unk4 = elemOffset + (offset1 >> numSwizzleBits);

            int subOffset1 = unk3 << numSwizzleBits;
            int subOffset2 = groupMask & unk4;

            return subOffset1 | subOffset2 | p4 | p5;
        }

        public static byte[] SwizzleBc(Gx2Surface surface)
        {
            //std::vector<u8> result;
            //List<byte> result = new List<byte>();

            //result.resize(surface->imageSize);

            //u8 *data = (u8*)surface->imagePtr;
            byte[] data = surface.data;
            byte[] result = new byte[surface.imageSize];

            int width = surface.width / 4;
            int height = surface.height / 4;

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    int bpp = GetFormatBpp(surface.format);
                    int pos = 0;

                    switch (surface.tileMode)
                    {
                        case 0:
                        case 1:
                            {
                                // pos = surfaceAddrFromCoordLinear(
                                //  x, y, 0, 0, bpp,
                                //  surface->pitch, height, surface->depth, 0
                                // );

                                //printf("Unsupported tilemode %d\n", surface->tileMode);
                                //exit(1);
                            }
                            break;

                        case 2:
                        case 3:
                            {
                                // pos = surfaceAddrFromCoordMicroTiled(
                                //  x, y, 0, bpp, surface->pitch, height,
                                //  surface->tileMode, 0, 0, 0, 0
                                // );

                                //printf("Unsupported tilemode %d\n", surface->tileMode);
                                //exit(1);
                            }
                            break;

                        default:
                            {
                                pos = SurfaceAddrFromCoordMacroTiled(
                                    x, y, 0, 0, bpp, surface.pitch, height,
                                    1, (AddrTileMode)surface.tileMode, 0, 0, 0,
                                    (surface.swizzle >> 8) & 1,
                                    (surface.swizzle >> 9) & 3
                                );
                            }
                            break;
                    }

                    int q = y * width + x;
                    switch (surface.format)
                    {
                        case 0x31:
                        case 0x34:
                        case 0x234:
                        case 0x431:
                            {
                                System.Array.Copy(data, pos, result, q * 8, 8);
                                //memcpy(result.data() + q*8, data+pos, 8);
                            }
                            break;

                        default:
                            {
                                System.Array.Copy(data, pos, result, q * 16, 16);
                                //memcpy(result.data() + q*16, data+pos, 16);
                            }
                            break;
                    }
                }
            }

            return result;

            //memcpy(data, result.data(), result.size());
        }
    }
}
