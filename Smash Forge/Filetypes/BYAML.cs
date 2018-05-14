using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace Smash_Forge
{
    public class BYAML
    {
        public dynamic byaml_file;

        public BYAML(string fname)
        {
            Read(fname);
        }

        public void Read(string FileName)
        {
          //  byaml_file = ByamlFile.Load(FileName);
        }
        public void Rebuild(string fileName)
        {
           // byaml_file = byaml_file.Save(fileName);
        }




    }
}
