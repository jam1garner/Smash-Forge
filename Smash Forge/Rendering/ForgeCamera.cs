using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Smash_Forge.Rendering
{
    public class ForgeCamera : Camera
    {
        // Previous mouse state.
        private float mouseSLast = 0;
        private float mouseYLast = 0;
        private float mouseXLast = 0;

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
                    float xAmount = OpenTK.Input.Mouse.GetState().X - mouseXLast;
                    float yAmount = (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                    Rotate(xAmount, yAmount);
                }

                //Zoom(mouseState, keyboardState);
                // Holding shift changes zoom speed.
                float zoomAmount = 1;
                if (keyboardState.IsKeyDown(OpenTK.Input.Key.ShiftLeft) || OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ShiftRight))
                    zoomAmount *= shiftZoomMultiplier;

                // Zooms in or out with arrow keys.
                if (keyboardState.IsKeyDown(OpenTK.Input.Key.Down))
                    Zoom(-zoomAmount, true);
                else if (keyboardState.IsKeyDown(OpenTK.Input.Key.Up))
                    Zoom(zoomAmount, true);

                // Scroll wheel zooms in or out.
                float scrollZoomAmount = (mouseState.WheelPrecise - mouseSLast) * scrollWheelZoomSpeed;
                Zoom(scrollZoomAmount, true);

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
