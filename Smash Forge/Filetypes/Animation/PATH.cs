using System.Collections.Generic;
using System.IO;

namespace SmashForge
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
            f.endian = Endianness.Big;
            f.Seek(4);
            if (f.ReadUInt() != Magic)
                return;

            f.Seek(0);
            uint bom = f.ReadUInt();
            if (bom == 0xFFFE0000)
                Endian = Endianness.Little;
            else if (bom == 0x0000FEFF)
                Endian = Endianness.Big;
            else
                return;
            f.endian = Endian;

            f.Seek(8);
            f.ReadInt(); // Always 0
            int frameCount = f.ReadInt();
            for (int i = 0; i < frameCount; i++)
            {
                pathFrame temp;
                temp.qx = f.ReadFloat();
                temp.qy = f.ReadFloat();
                temp.qz = f.ReadFloat();
                temp.qw = f.ReadFloat();
                temp.x = f.ReadFloat();
                temp.y = f.ReadFloat();
                temp.z = f.ReadFloat();
                Frames.Add(temp);
            }
        }
        public override byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.endian = Endian;
            f.WriteUInt(0x0000FEFF);
            f.endian = Endianness.Big;
            f.WriteUInt(Magic);

            f.endian = Endian;
            f.WriteInt(0); // Always 0
            f.WriteInt(Frames.Count);
            for (int i = 0; i < Frames.Count; i++)
            {
                f.WriteFloat(Frames[i].qx);
                f.WriteFloat(Frames[i].qy);
                f.WriteFloat(Frames[i].qz);
                f.WriteFloat(Frames[i].qw);
                f.WriteFloat(Frames[i].x);
                f.WriteFloat(Frames[i].y);
                f.WriteFloat(Frames[i].z);
            }

            return f.GetBytes();
        }
    }
}
