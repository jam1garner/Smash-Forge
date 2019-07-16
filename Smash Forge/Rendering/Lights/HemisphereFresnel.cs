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
    public class HemisphereFresnel
    {
        public LightColor groundColor = new LightColor();
        public LightColor skyColor = new LightColor();

        // There are two lights with different x-axis rotations.
        public float skyAngle = 0;
        public float groundAngle = 0;

        public string id = "";

        public HemisphereFresnel()
        {

        }

        public HemisphereFresnel(Vector3 groundHsv, Vector3 skyHsv, float skyAngle, float groundAngle, string id)
        {
            groundColor.H = groundHsv.X;
            groundColor.S = groundHsv.Y;
            groundColor.V = groundHsv.Z;
            skyColor.H = skyHsv.X;
            skyColor.S = skyHsv.Y;
            skyColor.V = skyHsv.Z;

            this.skyAngle = skyAngle;
            this.groundAngle = groundAngle;
            this.id = id;
        }

        public Vector3 getSkyDirection()
        {
            Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, skyAngle * ((float)Math.PI / 180f));
            Vector3 direction = Vector3.TransformVector(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
            return direction;
        }

        public Vector3 getGroundDirection()
        {
            Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, groundAngle * ((float)Math.PI / 180f));
            Vector3 direction = Vector3.TransformVector(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
            return direction;
        }

        public override string ToString()
        {
            return id;
        }
    }
}
