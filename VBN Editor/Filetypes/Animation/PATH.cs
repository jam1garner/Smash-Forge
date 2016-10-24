using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBN_Editor
{
    public struct pathFrame
    {
        public float x, y, z, qx, qy, qz, qw;
    }

    public class PathBin
    {
        public List<pathFrame> frames = new List<pathFrame>();

        public PathBin()
        {

        }

        public void read(FileData f)
        {
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
                frames.Add(temp);
            }
        }
    }
}
