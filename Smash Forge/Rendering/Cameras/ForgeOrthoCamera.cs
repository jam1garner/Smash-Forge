using OpenTK;
using System;

namespace SmashForge.Rendering
{
    class ForgeOrthoCamera : ForgeCamera
    {
        private readonly float leftInitial = -100;
        private readonly float rightInitial = 100;
        private readonly float bottomInitial = -100;
        private readonly float topInitial = 100;
        private float scale = 1;

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
                scale -= scrollZoomAmount * zoomAmount;
                scale = Math.Max(scale, 0);
            }
            catch (Exception)
            {
                // RIP OpenTK...
            }

            UpdateLastMousePosition();
        }

        private void UpdateLastMousePosition()
        {
            mouseXLast = OpenTK.Input.Mouse.GetState().X;
            mouseYLast = OpenTK.Input.Mouse.GetState().Y;
            mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;
        }

        protected override void UpdatePerspectiveMatrix()
        {
            float aspectRatio = (float)RenderWidth / RenderHeight;

            float left = (leftInitial * aspectRatio) * scale;

            float right = (rightInitial * aspectRatio) * scale;

            float bottom = (bottomInitial) * scale;

            float top = (topInitial) * scale;

            perspectiveMatrix = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, NearClipPlane, FarClipPlane * scale);
        }

        protected override void UpdateRotationMatrix()
        {
            rotationMatrix = Matrix4.Identity;
        }
    }
}
