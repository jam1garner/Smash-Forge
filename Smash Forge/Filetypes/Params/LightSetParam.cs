using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALT.PARAMS;
using OpenTK;

namespace Smash_Forge.Filetypes.Params
{
    class LightSetParam
    {
        private ParamFile paramFile;

        // light_set_param colors are stored internally as RGB
        private Vector3 diffuseColor;
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set
            {
                diffuseColor = value;
                float h;
                float s;
                float v;
                ColorTools.RGB2HSV(diffuseColor.X, diffuseColor.Y, diffuseColor.Z, out h, out s, out v);
                // TODO: Set the param values with the proper HSV values. 
            }
        }



        public LightSetParam(string fileName)
        {
            paramFile = new ParamFile(fileName);
        }


        public void Save(string fileName)
        {
            paramFile.Export(fileName);
        }
    }
}
