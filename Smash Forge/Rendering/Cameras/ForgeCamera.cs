using OpenTK;

namespace Smash_Forge.Rendering
{
    public abstract class ForgeCamera : SFGraphics.Cameras.Camera
    {
        // Previous mouse state.
        protected float mouseSLast = 0;
        protected float mouseYLast = 0;
        protected float mouseXLast = 0;
        
        // Apply Forge's camera settings.
        protected float zoomSpeed = Runtime.zoomspeed;
        protected float zoomDistanceScale = 0.01f;
        protected float rotateYSpeed = 0.0125f;
        protected float rotateXSpeed = 0.005f;
        protected float zoomMultiplier = Runtime.zoomModifierScale;
        protected float scrollWheelZoomSpeed = 1.75f;
        protected float shiftZoomMultiplier = 2.5f;

        public abstract void UpdateFromMouse();

        public void SetFromBone(Bone b)
        {
            Matrix4 Mat = b.transform.Inverted();
            translationMatrix = Matrix4.CreateTranslation(Mat.ExtractTranslation());
            rotationMatrix = Matrix4.CreateFromQuaternion(Mat.ExtractRotation());
            perspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(FovRadians, RenderWidth / (float)RenderHeight, 1.0f, FarClipPlane);

            modelViewMatrix = Mat;
            mvpMatrix = modelViewMatrix * perspectiveMatrix;
        }
    }
}
