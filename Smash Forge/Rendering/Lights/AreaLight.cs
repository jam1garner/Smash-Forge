using OpenTK;

namespace SmashForge.Rendering.Lights
{
    public class AreaLight : DirectionalLight
    {
        // ambient color
        public float groundR = 1.0f;
        public float groundG = 1.0f;
        public float groundB = 1.0f;

        // diffuse color
        public float skyR = 1.0f;
        public float skyG = 1.0f;
        public float skyB = 1.0f;

        // size
        public float scaleX = 1.0f;
        public float scaleY = 1.0f;
        public float scaleZ = 1.0f;

        // position of the center of the region
        public float positionX = 0.0f;
        public float positionY = 0.0f;
        public float positionZ = 0.0f;

        // How should "non directional" area lights work?
        public bool nonDirectional = false;

        public AreaLight(string areaLightId)
        {
            id = areaLightId;
        }

        public AreaLight(string areaLightId, Vector3 groundColor, Vector3 skyColor, Vector3 position, Vector3 scale, float rotX, float rotY, float rotZ)
        {
            id = areaLightId;

            positionX = position.X;
            positionY = position.Y;
            positionZ = position.Z;

            scaleX = scale.X;
            scaleY = scale.Y;
            scaleZ = scale.Z;

            skyR = skyColor.X;
            skyG = skyColor.Y;
            skyB = skyColor.Z;

            groundR = groundColor.X;
            groundB = groundColor.Y;
            groundG = groundColor.Z;
        }
    }

}
