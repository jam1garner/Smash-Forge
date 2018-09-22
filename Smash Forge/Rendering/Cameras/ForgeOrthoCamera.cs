using OpenTK;

namespace Smash_Forge.Rendering
{
    class ForgeOrthoCamera : ForgeCamera
    {
        public override void UpdateFromMouse()
        {

        }

        protected override void UpdatePerspectiveMatrix()
        {
            perspectiveMatrix = Matrix4.CreateOrthographicOffCenter(-100, 100, -100, 100, 0.1f, 1000);
        }

        protected override void UpdateRotationMatrix()
        {
            rotationMatrix = Matrix4.Identity;
        }
    }
}
