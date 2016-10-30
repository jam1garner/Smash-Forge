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

        public List<pathFrame> Frames { get; set; }
        public override Endianness Endian { get; set; }

        public override void Read(string filename)
        {
            FileData f = new FileData(filename);
            byte[] magic = f.read(0xC); ;
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
            throw new NotImplementedException("Yell at jam to implement this");
        }
    }
}
