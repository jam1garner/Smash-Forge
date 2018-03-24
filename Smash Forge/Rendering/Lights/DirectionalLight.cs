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

namespace Smash_Forge.Rendering.Lights
{

    public class DirectionalLight
    {
        public LightColor diffuseColor = new LightColor();
        public LightColor ambientColor = new LightColor();

        // in degrees (converted to radians for calcultions)
        public float rotX = 0.0f;
        public float rotY = 0.0f;
        public float rotZ = 0.0f;

        public Vector3 direction = new Vector3(0f, 0f, 1f);

        public string id = "";
        public bool enabled = true;

        public DirectionalLight(Vector3 diffuseHsv, Vector3 ambientHsv, float rotX, float rotY, float rotZ, string id)
        {
            // calculate light color
            diffuseColor.H = diffuseHsv.X;
            diffuseColor.S = diffuseHsv.Y;
            diffuseColor.V = diffuseHsv.Z;
            ambientColor.H = ambientHsv.X;
            ambientColor.S = ambientHsv.Y;
            ambientColor.V = ambientHsv.Z;

            // calculate light vector
            this.rotX = rotX;
            this.rotY = rotY;
            this.rotZ = rotZ;
            UpdateDirection();

            this.id = id;
        }

        public DirectionalLight(Vector3 diffuseHsv, Vector3 lightDirection, string id)
        {
            diffuseColor.H = diffuseHsv.X;
            diffuseColor.S = diffuseHsv.Y;
            diffuseColor.V = diffuseHsv.Z;

            direction = lightDirection;

            this.id = id;
        }

        public DirectionalLight()
        {

        }

        private void UpdateDirection()
        {
            // calculate light vector from 3 rotation angles
            Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, rotX * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitY, rotY * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitZ, rotZ * ((float)Math.PI / 180f));

            direction = Vector3.TransformVector(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
        }

        public override string ToString()
        {
            return id;
        }
    }

}
