using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBN_Editor
{
    public abstract class FileBase
    {
        public abstract Endianness Endian { get; set; }

        public abstract byte[] GetBytes();
        public abstract void Read(string filename);
        public abstract void Rebuild();

        public void Save(string filename)
        {
            Rebuild();

            var Data = GetBytes();
            if (Data.Length <= 0)
                throw new Exception("Warning: Data was empty!");

            File.WriteAllBytes(filename, Data);
        }
    }
}
