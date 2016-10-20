using System;
using System.Collections.Generic;

namespace VBN_Editor
{
    public class GTX
    {

        public struct GX2Surface
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

        public GTX()
        {
        }


        /*---------------------------------------
         * 
         * Code ported from Aboood's GTX Extractor https://github.com/aboood40091/GTX-Extractor/blob/master/gtx_extract.py
         * 
         * With help by Aelan!
         * 
         *---------------------------------------*/

        public static int getFormatBpp (int format)
        {
            int hwFormat = format & 0x3F;
            return formatHwInfo[hwFormat * 4];
        }

        public static int computeSurfaceThickness (int tileMode)
        {
            switch (tileMode) {
                case 3:
                case 7:
                case 11:
                case 13:
                case 15: {
                        return 4;
                    } break;

                case 16:
                case 17: {
                        return 8;
                    } break;
            }

            return 1;
        }

        public static int isThickMacroTiled (int tileMode)
        {
            switch (tileMode) {
                case 7:
                case 11:
                case 13:
                case 15: {
                        return 1;
                    } break;
            }

            return 0;
        }

        public static int isBankSwappedTileMode (int tileMode)
        {
            switch (tileMode) {
                case 8:
                case 9:
                case 10:
                case 11:
                case 14:
                case 15: {
                        return 1;
                    } break;
            }

            return 0;
        }

        public static int computeSurfaceRotationFromTileMode (int tileMode)
        {
            switch (tileMode) {
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11: {
                        return 2;
                    } break;

                case 12:
                case 13:
                case 14:
                case 15: {
                        return 1;
                    } break;
            }

            return 0;
        }

        public static int computePipeFromCoordWoRotation (int x, int y)
        {
            int pipe = ((y >> 3) ^ (x >> 3)) & 1;
            return pipe;
        }

        public static int computeBankFromCoordWoRotation (int x, int y)
        {
            int bankBit0 = ((y / (16 * 2)) ^ (x >> 3)) & 1;
            int bank = bankBit0 | 2 * (((y / (8 * 2)) ^ (x >> 4)) & 1);

            return bank;
        }

        public static int computeMacroTileAspectRatio (int tileMode)
        {
            switch (tileMode) {
                case 8:
                case 12:
                case 14: {
                        return 1;
                    } break;

                case 5:
                case 9: {
                        return 2;
                    } break;

                case 6:
                case 10: {
                        return 4;
                    } break;
            }

            return 1;
        }

        public static int computeSurfaceBankSwappedWidth (int tileMode, int bpp, int numSamples, int pitch, int pSlicesPerTile)
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

            if ((splitSize / bytesPerSample) != 0) {
                slicesPerTile = numSamples / samplesPerTile;
                if ((numSamples / samplesPerTile) == 0) {
                    slicesPerTile = 1;
                }
            }

            if (pSlicesPerTile != 0) {
                pSlicesPerTile = slicesPerTile;
            }

            if (isThickMacroTiled(tileMode) == 1) {
                numSamples = 4;
            }

            int bytesPerTileSlice = numSamples * bytesPerSample / slicesPerTile;

            if (isBankSwappedTileMode(tileMode) == 1) {
                int v7;
                int v8;
                int v9;

                int factor = computeMacroTileAspectRatio(tileMode);
                int swapTiles = (swapSize >> 1) / bpp;

                if (swapTiles != 0) {
                    v9 = swapTiles;
                } else {
                    v9 = 1;
                }

                int swapWidth = v9 * 8 * numBanks;
                int heightBytes = numSamples * factor * numPipes * bpp / slicesPerTile;
                int swapMax = numPipes * numBanks * rowSize / heightBytes;
                int swapMin = groupSize * 8 * numBanks / bytesPerTileSlice;

                if (swapMax >= swapWidth) {
                    if (swapMin <= swapWidth) {
                        v7 = swapWidth;
                    } else {
                        v7 = swapMin;
                    }

                    v8 = v7;
                } else {
                    v8 = swapMax;
                }

                bankSwapWidth = v8;

                while (bankSwapWidth >= (2 * pitch)) {
                    bankSwapWidth >>= 1;
                }
            }

            return bankSwapWidth;
        }

        public static int computePixelIndexWithinMicroTile(int x, int y, int z, int bpp, int tileMode, int microTileType)
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
            int thickness = computeSurfaceThickness(tileMode);

            if (microTileType == 3) {
                pixelBit0 = x & 1;
                pixelBit1 = y & 1;
                pixelBit2 = z & 1;
                pixelBit3 = (x & 2) >> 1;
                pixelBit4 = (y & 2) >> 1;
                pixelBit5 = (z & 2) >> 1;
                pixelBit6 = (x & 4) >> 2;
                pixelBit7 = (y & 4) >> 2;
            } else {
                if (microTileType != 0) {
                    pixelBit0 = x & 1;
                    pixelBit1 = y & 1;
                    pixelBit2 = (x & 2) >> 1;
                    pixelBit3 = (y & 2) >> 1;
                    pixelBit4 = (x & 4) >> 2;
                    pixelBit5 = (y & 4) >> 2;
                } else {
                    if (bpp == 0x08) {
                        pixelBit0 = x & 1;
                        pixelBit1 = (x & 2) >> 1;
                        pixelBit2 = (x & 4) >> 2;
                        pixelBit3 = (y & 2) >> 1;
                        pixelBit4 = y & 1;
                        pixelBit5 = (y & 4) >> 2;
                    } else if (bpp == 0x10) {
                        pixelBit0 = x & 1;
                        pixelBit1 = (x & 2) >> 1;
                        pixelBit2 = (x & 4) >> 2;
                        pixelBit3 = y & 1;
                        pixelBit4 = (y & 2) >> 1;
                        pixelBit5 = (y & 4) >> 2;
                    } else if (bpp == 0x20 || bpp == 0x60) {
                        pixelBit0 = x & 1;
                        pixelBit1 = (x & 2) >> 1;
                        pixelBit2 = y & 1;
                        pixelBit3 = (x & 4) >> 2;
                        pixelBit4 = (y & 2) >> 1;
                        pixelBit5 = (y & 4) >> 2;
                    } else if (bpp == 0x40) {
                        pixelBit0 = x & 1;
                        pixelBit1 = y & 1;
                        pixelBit2 = (x & 2) >> 1;
                        pixelBit3 = (x & 4) >> 2;
                        pixelBit4 = (y & 2) >> 1;
                        pixelBit5 = (y & 4) >> 2;
                    } else if (bpp == 0x80) {
                        pixelBit0 = y & 1;
                        pixelBit1 = x & 1;
                        pixelBit2 = (x & 2) >> 1;
                        pixelBit3 = (x & 4) >> 2;
                        pixelBit4 = (y & 2) >> 1;
                        pixelBit5 = (y & 4) >> 2;
                    } else {
                        pixelBit0 = x & 1;
                        pixelBit1 = (x & 2) >> 1;
                        pixelBit2 = y & 1;
                        pixelBit3 = (x & 4) >> 2;
                        pixelBit4 = (y & 2) >> 1;
                        pixelBit5 = (y & 4) >> 2;
                    }
                }

                if (thickness > 1) {
                    pixelBit6 = z & 1;
                    pixelBit7 = (z & 2) >> 1;
                }
            }

            if (thickness == 8) {
                pixelBit8 = (z & 4) >> 2;
            }

            return pixelBit0     |
                (pixelBit8 << 8) |
                (pixelBit7 << 7) |
                (pixelBit6 << 6) |
                (pixelBit5 << 5) |
                (pixelBit4 << 4) |
                (pixelBit3 << 3) |
                (pixelBit2 << 2) |
                (pixelBit1 << 1);
        }

        public static int surfaceAddrFromCoordMacroTiled (
            int x, int y, int slice, int sample, int bpp,
            int pitch, int height, int numSamples, int tileMode,
            int isDepth, int tileBase, int compBits,
            int pipeSwizzle, int bankSwizzle
        )
        {
            const int numPipes     = 2;
            const int numBanks     = 4;
            const int numGroupBits = 8;
            const int numPipeBits  = 1;
            const int numBankBits  = 2;

            int microTileThickness = computeSurfaceThickness(tileMode);
            int microTileBits = numSamples * bpp * (microTileThickness * (8*8));
            int microTileBytes = microTileBits >> 3;
            int microTileType = (isDepth==1) ? 1 : 0;
            int pixelIndex = computePixelIndexWithinMicroTile(x, y, slice, bpp, tileMode, microTileType);

            int sampleOffset;
            int pixelOffset;
            if (isDepth==1) {
                if (compBits != 0 && compBits != bpp) {
                    sampleOffset = tileBase + compBits * sample;
                    pixelOffset = numSamples * compBits * pixelIndex;
                } else {
                    sampleOffset = bpp * sample;
                    pixelOffset = numSamples * compBits * pixelIndex;
                }
            } else {
                sampleOffset = sample * (microTileBits / numSamples);
                pixelOffset = bpp * pixelIndex;
            }

            int elemOffset = pixelOffset + sampleOffset;
            int bytesPerSample = microTileBytes / numSamples;

            int samplesPerSlice;
            int numSampleSplits;
            int sampleSlice;

            if (numSamples <= 1 || microTileBytes <= 2048) {
                samplesPerSlice = numSamples;
                numSampleSplits = 1;
                sampleSlice = 0;
            } else {
                samplesPerSlice = 2048 / bytesPerSample;
                numSampleSplits = numSamples / samplesPerSlice;
                numSamples = samplesPerSlice;
                sampleSlice = elemOffset / (microTileBits / numSampleSplits);
                elemOffset %= microTileBits / numSampleSplits;
            }

            elemOffset >>= 3;

            int pipe = computePipeFromCoordWoRotation(x, y);
            int bank = computeBankFromCoordWoRotation(x, y);
            int bankPipe = pipe + numPipes * bank;
            int rotation = computeSurfaceRotationFromTileMode(tileMode);
            int swizzle = pipeSwizzle + numPipes * bankSwizzle;
            int sliceIn = slice;

            if (isThickMacroTiled(tileMode)==1) {
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
            int v18 = tileMode - 5;

            if (tileMode == 5 || tileMode == 9) {
                macroTilePitch >>= 1;
                macroTileHeight *= 2;
            } else if (tileMode == 6 || tileMode == 10) {
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
            switch (tileMode) {
                case 8:
                case 9:
                case 10:
                case 11:
                case 14:
                case 15: {
                        bankSwapWidth = computeSurfaceBankSwappedWidth(tileMode, bpp, numSamples, pitch, 0);
                        swapIndex = macroTilePitch * macroTileIndexX / bankSwapWidth;
                        bankMask = 3; // m_banks-1
                        bank ^= bankSwapOrder[swapIndex & bankMask];
                    } break;
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

        public static byte[] swizzleBC(byte[] data, int width, int height, int format, int tileMode, int pitch, int swizzle){
            GX2Surface sur = new GX2Surface();
            sur.width = width;
            sur.height = height;
            sur.tileMode = tileMode;
            sur.format = format;
            sur.swizzle = swizzle;
            sur.pitch = pitch;
            sur.data = data;
            sur.imageSize = data.Length;
            return swizzleBC (sur);
        }

        public static byte[] swizzleBC (GX2Surface surface)
        {
            //std::vector<u8> result;
            //List<byte> result = new List<byte>();

            //result.resize(surface->imageSize);

            //u8 *data = (u8*)surface->imagePtr;
            byte[] data = surface.data;
            byte[] result = new byte[surface.imageSize];

            int width = surface.width / 4;
            int height = surface.height / 4;

            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    int bpp = getFormatBpp(surface.format);
                    int pos = 0;

                    switch (surface.tileMode) {
                        case 0:
                        case 1: {
                                // pos = surfaceAddrFromCoordLinear(
                                //  x, y, 0, 0, bpp,
                                //  surface->pitch, height, surface->depth, 0
                                // );

                                //printf("Unsupported tilemode %d\n", surface->tileMode);
                                //exit(1);
                            } break;

                        case 2:
                        case 3: {
                                // pos = surfaceAddrFromCoordMicroTiled(
                                //  x, y, 0, bpp, surface->pitch, height,
                                //  surface->tileMode, 0, 0, 0, 0
                                // );

                                //printf("Unsupported tilemode %d\n", surface->tileMode);
                                //exit(1);
                            } break;

                        default: {
                                pos = surfaceAddrFromCoordMacroTiled(
                                    x, y, 0, 0, bpp, surface.pitch, height,
                                    1, surface.tileMode, 0, 0, 0,
                                    (surface.swizzle >> 8) & 1,
                                    (surface.swizzle >> 9) & 3
                                );
                            } break;
                    }

                    int q = y * width + x;
                    switch (surface.format) {
                        case 0x31:
                        case 0x34:
                        case 0x234:
                        case 0x431: {
                                System.Array.Copy(data, pos, result, q * 8, 8);
                                //memcpy(result.data() + q*8, data+pos, 8);
                            } break;

                        default: {
                                System.Array.Copy(data, pos, result, q * 16, 16);
                                //memcpy(result.data() + q*16, data+pos, 16);
                            } break;
                    }
                }
            }

            return result;

            //memcpy(data, result.data(), result.size());
        }
    }
}

