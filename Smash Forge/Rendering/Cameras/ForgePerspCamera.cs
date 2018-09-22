using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Smash_Forge.Rendering
{
    class ForgePerspCamera : ForgeCamera
    {
        public override void UpdateFromMouse()
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
                    RotationXRadians += xAmount * rotateXSpeed;
                    RotationYRadians += yAmount * rotateYSpeed;
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
            }
            catch (Exception)
            {
                // RIP OpenTK...
            }

            UpdateLastMousePosition();
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
