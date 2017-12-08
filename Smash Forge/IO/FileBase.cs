using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge
{
    public abstract class FileBase : TreeNode
    {
        public abstract Endianness Endian { get; set; }

        public bool Renderable = false;

        public abstract void Read(string filename);
        public abstract byte[] Rebuild();

        public void Save(string filename)
        {
            var Data = Rebuild();
            if (Data.Length <= 0)
                throw new Exception("Warning: Data was empty!");

            File.WriteAllBytes(filename, Data);
        }
    }
}
