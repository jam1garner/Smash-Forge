using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smash_Forge
{
    public struct pathFrame
    {
        public float x;
        public float y;
        public float z;
        public float qx;
        public float qy;
        public float qz;
        public float qw;
    }

    public class PathBin : FileBase
    {
        public PathBin()
        {
            Frames = new List<pathFrame>();
        }
        public PathBin(string filename) : this()
        {
            Read(filename);
        }

        public const uint Magic = 0x50415448; // "PATH"

        public List<pathFrame> Frames { get; set; }
        public override Endianness Endian { get; set; }

        public override void Read(string filename)
        {
            FileData f = new FileData(filename);
            f.Endian = Endianness.Big;
            f.seek(4);
            if (f.readUInt() != Magic)
                return;

            f.seek(0);
            uint bom = f.readUInt();
            if (bom == 0xFFFE0000)
                Endian = Endianness.Little;
            else if (bom == 0x0000FEFF)
                Endian = Endianness.Big;
            else
                return;
            f.Endian = Endian;

            f.seek(8);
            f.readInt(); // Always 0
            int frameCount = f.readInt();
            for (int i = 0; i < frameCount; i++)
            {
                pathFrame temp;
                temp.qx = f.readFloat();
                temp.qy = f.readFloat();
                temp.qz = f.readFloat();
                temp.qw = f.readFloat();
                temp.x = f.readFloat();
                temp.y = f.readFloat();
                temp.z = f.readFloat();
                Frames.Add(temp);
            }
        }
        public override byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = Endian;
            f.writeUInt(0x0000FEFF);
            f.Endian = Endianness.Big;
            f.writeUInt(Magic);

            f.Endian = Endian;
            f.writeInt(0); // Always 0
            f.writeInt(Frames.Count);
            for (int i = 0; i < Frames.Count; i++)
            {
                f.writeFloat(Frames[i].qx);
                f.writeFloat(Frames[i].qy);
                f.writeFloat(Frames[i].qz);
                f.writeFloat(Frames[i].qw);
                f.writeFloat(Frames[i].x);
                f.writeFloat(Frames[i].y);
                f.writeFloat(Frames[i].z);
            }

            return f.getBytes();
        }
    }
}
