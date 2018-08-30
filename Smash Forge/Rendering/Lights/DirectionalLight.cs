using OpenTK;
using SFGraphics.Utils;

namespace Smash_Forge.Rendering.Lights
{

    public class DirectionalLight
    {
        public LightColor diffuseColor = new LightColor();
        public LightColor ambientColor = new LightColor();

        public float RotationXDegrees
        {
            get { return rotationXDegrees; }
            set
            {
                rotationXDegrees = value;
                UpdateDirection();
            }
        }
        private float rotationXDegrees = 0.0f;

        public float RotationYDegrees
        {
            get { return rotationYDegrees; }
            set
            {
                rotationYDegrees = value;
                UpdateDirection();
            }
        }
        private float rotationYDegrees = 0.0f;

        public float RotationZDegrees
        {
            get { return rotationZDegrees; }
            set
            {
                rotationZDegrees = value;
                UpdateDirection();
            }
        }
        private float rotationZDegrees = 0.0f;

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
            this.RotationXDegrees = rotX;
            this.RotationYDegrees = rotY;
            this.RotationZDegrees = rotZ;
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
            Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, (float)VectorTools.GetRadians(RotationXDegrees))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitY, RotationYDegrees * (float)VectorTools.GetRadians(RotationYDegrees))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitZ, RotationZDegrees * (float)VectorTools.GetRadians(RotationZDegrees));

            direction = Vector3.TransformVector(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
        }

        public override string ToString()
        {
            return id;
        }
    }

}
