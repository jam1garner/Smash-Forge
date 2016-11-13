using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Smash_Forge
{
    public class KCL
    {
        public class header
        {
           public uint magic;
           public uint octreeOffset;
           public uint modelOffArray; // Check me on that
           public uint modelCount;
           public Vector3 minModelCoord;
           public Vector3 maxModelCoord;
           public Vector3 coordianteShift;
           public uint unk1;

            public void readHeader(FileData f)
            {
                f.Endian = Endianness.Big;

                f.seek(0);
                magic = (uint)f.readInt();
                octreeOffset = (uint)f.readInt();
                modelOffArray = (uint)f.readInt();
                modelCount = (uint)f.readInt();

                minModelCoord = new Vector3 { X = f.readFloat(), Y = f.readFloat(), Z = f.readFloat() };
                maxModelCoord = new Vector3 { X = f.readFloat(), Y = f.readFloat(), Z = f.readFloat() };
                coordianteShift = new Vector3 { X = f.readInt(), Y = f.readInt(), Z = f.readInt() }; // Vector3 of uints?

                unk1 = (uint)f.readInt();
            }
        }

        public class tris
        {
            float length;
            ushort positionIndex;
            ushort directionIndex;
            ushort normalAIndex;
            ushort normalBIndex;
            ushort normalCIndex;
            ushort colFlag;
            uint unk1; // I don't even know
        }

        public class modelData
        {
            public void readModelData(FileData f)
            {
                f.Endian = Endianness.Big;
            }
        }
    }
}
