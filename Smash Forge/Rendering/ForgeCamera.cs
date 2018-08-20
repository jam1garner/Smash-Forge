using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Smash_Forge.Rendering
{
    public class ForgeCamera : SFGraphics.Cameras.Camera
    {
        // Previous mouse state.
        private float mouseSLast = 0;
        private float mouseYLast = 0;
        private float mouseXLast = 0;

        // Apply Forge's camera settings.
        private float zoomSpeed = Runtime.zoomspeed;
        private float zoomDistanceScale = 0.01f;
        private float rotateYSpeed = 0.0125f;
        private float rotateXSpeed = 0.005f;
        private float zoomMultiplier = Runtime.zoomModifierScale;
        private float scrollWheelZoomSpeed = 1.75f;
        private float shiftZoomMultiplier = 2.5f;

        public ForgeCamera()
        {
            NearClipPlane = 0.1f;
        }

        public void SetFromBone(Bone b)
        {
            Matrix4 Mat = b.transform.Inverted();
            translationMatrix = Matrix4.CreateTranslation(Mat.ExtractTranslation());
            rotationMatrix = Matrix4.CreateFromQuaternion(Mat.ExtractRotation());
            perspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(FovRadians, renderWidth / (float)renderHeight, 1.0f, FarClipPlane);

            modelViewMatrix = Mat;
            mvpMatrix = modelViewMatrix * perspectiveMatrix;
        }

        public void UpdateFromMouse()
        {
            try
            {
                OpenTK.Input.MouseState mouseState = OpenTK.Input.Mouse.GetState();
                OpenTK.Input.KeyboardState keyboardState = OpenTK.Input.Keyboard.GetState();

                if (OpenTK.Input.Mouse.GetState().RightButton == OpenTK.Input.ButtonState.Pressed)
                {
                    float xAmount = OpenTK.Input.Mouse.GetState().X - mouseXLast;
                    float yAmount = (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                    Pan(xAmount, yAmount, true);
                }

                if (mouseState.LeftButton == OpenTK.Input.ButtonState.Pressed)
                {
                    // Dragging left/right rotates around the y-axis.
                    // Dragging up/down rotates around the x-axis.
                    float xAmount = (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                    float yAmount = OpenTK.Input.Mouse.GetState().X - mouseXLast;
                    Rotate(xAmount * rotateXSpeed, yAmount * rotateYSpeed);
                }

                // Holding shift changes zoom speed.
                float zoomAmount = zoomSpeed * zoomDistanceScale;
                if (keyboardState.IsKeyDown(OpenTK.Input.Key.ShiftLeft) || OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ShiftRight))
                    zoomAmount *= shiftZoomMultiplier;

                // Zooms in or out with arrow keys.
                if (keyboardState.IsKeyDown(OpenTK.Input.Key.Down))
                    Zoom(-zoomAmount, true);
                else if (keyboardState.IsKeyDown(OpenTK.Input.Key.Up))
                    Zoom(zoomAmount, true);

                // Scroll wheel zooms in or out.
                float scrollZoomAmount = (mouseState.WheelPrecise - mouseSLast) * scrollWheelZoomSpeed;
                Zoom(scrollZoomAmount * zoomAmount, true);

                UpdateLastMousePosition();
            }
            catch (Exception)
            {
                // RIP OpenTK...
            }

            UpdateMatrices();
        }

        private void UpdateLastMousePosition()
        {
            mouseXLast = OpenTK.Input.Mouse.GetState().X;
            mouseYLast = OpenTK.Input.Mouse.GetState().Y;
            mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;
        }
    }
}
