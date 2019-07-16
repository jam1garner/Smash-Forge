using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SALT.PARAMS;
using SALT.Graphics;
using System.Diagnostics;
using System.Globalization;

namespace SmashForge.Rendering.Lights
{
    public class LightMap
    {
        private float scaleX = 1;
        private float scaleY = 1;
        private float scaleZ = 1;

        private int texture_index = 0x10080000;
        private int texture_addr = 0;

        private float posX = 0;
        private float posY = 0;
        private float posZ = 0;

        private float rotX = 0;
        private float rotY = 0;
        private float rotZ = 0;

        private string id = "";

        public LightMap()
        {

        }

        public LightMap(Vector3 scale, int texture_index, int texture_addr, Vector3 pos, float rotX, float rotY, float rotZ, string id)
        {
            this.scaleX = scale.X;
            this.scaleY = scale.Y;
            this.scaleZ = scale.Z;
            this.texture_addr = texture_addr;
            this.texture_index = texture_index;
            this.posX = pos.X;
            this.posY = pos.Y;
            this.posZ = pos.Z;
            this.rotX = rotX;
            this.rotY = rotY;
            this.rotZ = rotZ;
            this.id = id;
        }

        public override string ToString()
        {
            return id;
        }
    }


}
