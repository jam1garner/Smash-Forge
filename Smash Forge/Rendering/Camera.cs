using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SALT.PARAMS;
using Smash_Forge.Params;

namespace Smash_Forge.Rendering
{
    public class Camera
    {
        // Values from camera controls or stprm.bin.
        private Vector3 position = new Vector3(0, 10, -80);
        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;
                UpdateMatrices();
            }
        }

        private float fovRadians = 0.524f;
        public float FovRadians
        {
            get { return fovRadians; }
            set
            {
                fovRadians = value;
                UpdateMatrices();
            }
        }

        public float FovDegrees
        {
            get { return (float)(fovRadians * 180.0f / Math.PI); }
            set
            {
                // Only store radians internally.
                fovRadians = (float)(value / 180.0f * Math.PI);
                UpdateMatrices();
            }
        }

        private float rotationX = 0;
        public float RotationX
        {
            get { return rotationX; }
            set
            {
                rotationX = value;
                UpdateMatrices();
            }
        }

        private float rotationY = 0;
        public float RotationY
        {
            get { return rotationY; }
            set
            {
                rotationY = value;
                UpdateMatrices();
            }
        }

        private float farClipPlane = 100000;
        public float FarClipPlane
        {
            get { return farClipPlane; }
            set
            {
                // The far clip plane and near clip plane can't be swapped.
                farClipPlane = value;
                UpdateMatrices();
            }
        }

        private float nearClipPlane = 1;
        public float NearClipPlane
        {
            get { return nearClipPlane; }
            set
            {
                nearClipPlane = value;
                UpdateMatrices();
            }
        }

        public int renderWidth = 1;
        public int renderHeight = 1;

        public Vector3 scale = new Vector3(1);

        // Matrices shouldn't be changed directly.
        // To change the rotation matrix, set the rotation values, for example.
        private Matrix4 modelViewMatrix = Matrix4.Identity;
        public Matrix4 ModelViewMatrix { get { return modelViewMatrix; } }

        private Matrix4 mvpMatrix = Matrix4.Identity;
        public Matrix4 MvpMatrix { get { return mvpMatrix; } }

        private Matrix4 rotationMatrix = Matrix4.Identity;
        public Matrix4 RotationMatrix { get { return rotationMatrix; } }

        private Matrix4 translationMatrix = Matrix4.Identity;
        public Matrix4 TranslationMatrix { get { return translationMatrix; } }

        private Matrix4 perspectiveMatrix = Matrix4.Identity;
        public Matrix4 PerspectiveMatrix { get { return perspectiveMatrix; } }

        // Camera control settings. 
        public float zoomMultiplier = Runtime.zoomModifierScale; 
        public float zoomSpeed = Runtime.zoomspeed;
        public float mouseTranslateSpeed = 0.5f;
        public float scrollWheelZoomSpeed = 1.75f;
        public float shiftZoomMultiplier = 2.5f;

        // Previous mouse state.
        private float mouseSLast = 0;
        private float mouseYLast = 0;
        private float mouseXLast = 0;

        public Camera()
        {

        }

        public Camera(Vector3 position, float rotX, float rotY, int renderWidth = 1, int renderHeight = 1)
        {
            Position = position;
            this.renderHeight = renderHeight;
            this.renderWidth = renderWidth;
            RotationX = rotX;
            RotationY = rotY;
        }

        public void SetFromBone(Bone b)
        {
            Matrix4 Mat = b.transform.Inverted();
            translationMatrix = Matrix4.CreateTranslation(Mat.ExtractTranslation());
            rotationMatrix = Matrix4.CreateFromQuaternion(Mat.ExtractRotation());
            perspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(fovRadians, renderWidth / (float)renderHeight, 1.0f, farClipPlane);

            modelViewMatrix = Mat;
            mvpMatrix = modelViewMatrix * perspectiveMatrix;
        }

        public void Update()
        {
            try
            {
                OpenTK.Input.Mouse.GetState();

                Pan();
                Rotate();
                Zoom();

                UpdateLastMousePosition();
            }
            catch (Exception)
            {
                // RIP OpenTK...
            }

            UpdateMatrices();
        }

        private void Rotate()
        {
            if ((OpenTK.Input.Mouse.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed))
            {
                rotationY += 0.0125f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
                rotationX += 0.005f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
            }
        }

        private void Pan()
        {
            if ((OpenTK.Input.Mouse.GetState().RightButton == OpenTK.Input.ButtonState.Pressed))
            {
                // Find the change in normalized screen coordinates.
                float deltaYNormalized = (OpenTK.Input.Mouse.GetState().Y - mouseYLast) / renderHeight;
                float deltaXNormalized = (OpenTK.Input.Mouse.GetState().X - mouseXLast) / renderWidth;

                // Translate the camera based on the distance from the origin and field of view.
                // Objects will "follow" the mouse while panning.
                position.Y += deltaYNormalized * ((float)Math.Sin(fovRadians) * position.Length);
                position.X += deltaXNormalized * ((float)Math.Sin(fovRadians) * position.Length);
            }
        }

        private void Zoom()
        {
            // Increase zoom speed when zooming out. 
            float zoomDistanceScale = 0.01f;
            float zoomscale = zoomSpeed * Math.Abs(position.Z) * zoomDistanceScale;

            // Holding shift changes zoom speed.
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ShiftLeft) || OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ShiftRight))
                zoomscale *= shiftZoomMultiplier;

            // Zooms in or out with arrow keys.
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.Down))
                position.Z -= 1 * zoomscale;
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.Up))
                position.Z += 1 * zoomscale;

            // Scroll wheel zooms in or out.
            position.Z += (OpenTK.Input.Mouse.GetState().WheelPrecise - mouseSLast) * zoomscale * scrollWheelZoomSpeed;
        }

        private void UpdateMatrices()
        {
            translationMatrix = Matrix4.CreateTranslation(position.X, -position.Y, position.Z);
            rotationMatrix = Matrix4.CreateRotationY(rotationY) * Matrix4.CreateRotationX(rotationX);
            perspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(fovRadians, renderWidth / (float)renderHeight, nearClipPlane, farClipPlane);

            modelViewMatrix = rotationMatrix * translationMatrix;
            mvpMatrix = modelViewMatrix * perspectiveMatrix * Matrix4.CreateScale(scale);
        }

        private void UpdateLastMousePosition()
        {
            mouseXLast = OpenTK.Input.Mouse.GetState().X;
            mouseYLast = OpenTK.Input.Mouse.GetState().Y;
            mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;
        }

        public void SetValuesFromStprm(ParamFile stprm)
        {
            if (stprm == null)
                return;
            float newFov = (float)ParamTools.GetParamValue(stprm, 0, 0, 6);
            fovRadians = newFov * ((float)Math.PI / 180.0f); 
            farClipPlane = (float)ParamTools.GetParamValue(stprm, 0, 0, 77);           
        }

        public void ResetToDefaultPosition()
        {
            position = new Vector3(0, 10, -80);
            rotationX = 0;
            rotationY = 0;
        }

        public void FrameBoundingSphere(Vector3 center, float radius)
        {
            // Calculate a right triangle using the bounding sphere radius as the height and the fov as the angle.
            // The distance is the base of the triangle. 
            float distance = radius / (float)Math.Tan(fovRadians / 2.0f);

            float offset = 10 / fovRadians;
            rotationX = 0;
            rotationY = 0;
            position.X = -center.X;
            position.Y = center.Y;
            position.Z = -1 * (distance + offset);
        }
    }
}
