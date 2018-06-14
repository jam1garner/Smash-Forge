using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Smash_Forge.Rendering
{
    public class Camera
    {
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

        private float rotationXRadians = 0;
        public float RotationXRadians
        {
            get { return rotationXRadians; }
            set
            {
                rotationXRadians = value;
                UpdateMatrices();
            }
        }

        public float RotationXDegrees
        {
            get { return (float)(rotationXRadians * 180.0f / Math.PI); }
            set
            {
                // Only store radians internally.
                rotationXRadians = (float)(value / 180.0f * Math.PI);
                UpdateMatrices();
            }
        }

        private float rotationYRadians = 0;
        public float RotationYRadians
        {
            get { return rotationYRadians; }
            set
            {
                rotationYRadians = value;
                UpdateMatrices();
            }
        }

        public float RotationYDegrees
        {
            get { return (float)(rotationYRadians * 180.0f / Math.PI); }
            set
            {
                // Only store radians internally.
                rotationYRadians = (float)(value / 180.0f * Math.PI);
                UpdateMatrices();
            }
        }

        private float farClipPlane = 100000;
        public float FarClipPlane
        {
            get { return farClipPlane; }
            set
            {
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

        // Matrices shouldn't be changed directly.
        // To change the rotation matrix, set the rotation values, for example.
        protected Matrix4 modelViewMatrix = Matrix4.Identity;
        public Matrix4 ModelViewMatrix { get { return modelViewMatrix; } }

        protected Matrix4 mvpMatrix = Matrix4.Identity;
        public Matrix4 MvpMatrix { get { return mvpMatrix; } }

        protected Matrix4 rotationMatrix = Matrix4.Identity;
        public Matrix4 RotationMatrix { get { return rotationMatrix; } }

        protected Matrix4 translationMatrix = Matrix4.Identity;
        public Matrix4 TranslationMatrix { get { return translationMatrix; } }

        protected Matrix4 perspectiveMatrix = Matrix4.Identity;
        public Matrix4 PerspectiveMatrix { get { return perspectiveMatrix; } }

        // Camera control settings. 
        public float mouseTranslateSpeed = 0.5f;

        public float zoomMultiplier = Runtime.zoomModifierScale;
        public float zoomSpeed = Runtime.zoomspeed;
        public float scrollWheelZoomSpeed = 1.75f;
        public float shiftZoomMultiplier = 2.5f;
        public float zoomDistanceScale = 0.01f;

        public float rotateYSpeed = 0.0125f;
        public float rotateXSpeed = 0.005f;

        public Camera()
        {

        }

        public Camera(Vector3 position, float rotX, float rotY, int renderWidth = 1, int renderHeight = 1)
        {
            Position = position;
            this.renderHeight = renderHeight;
            this.renderWidth = renderWidth;
            RotationXRadians = rotX;
            RotationYRadians = rotY;
        }

        public void Rotate(float xAmount, float yAmount)
        {
            rotationYRadians += rotateYSpeed * xAmount;
            rotationXRadians += rotateXSpeed * yAmount;
        }

        public void Pan(float xAmount, float yAmount, bool scaleByDistanceToOrigin = true)
        {
            // Find the change in normalized screen coordinates.
            float deltaX = xAmount / renderWidth;
            float deltaY = yAmount / renderHeight;

            if (scaleByDistanceToOrigin)
            {
                // Translate the camera based on the distance from the origin and field of view.
                // Objects will "follow" the mouse while panning.
                position.Y += deltaY * ((float)Math.Sin(fovRadians) * position.Length);
                position.X += deltaX * ((float)Math.Sin(fovRadians) * position.Length);
            }
            else
            {
                // Regular panning.
                position.Y += deltaY;
                position.X += deltaX;
            }        
        }

        public void Zoom(float amount, bool scaleByDistanceToOrigin = true)
        {
            // Increase zoom speed when zooming out. 
            float zoomscale = zoomSpeed;
            if (scaleByDistanceToOrigin)
                zoomscale *= Math.Abs(position.Z) * zoomDistanceScale;

            position.Z += amount * zoomscale;
        }

        public void UpdateMatrices()
        {
            translationMatrix = Matrix4.CreateTranslation(position.X, -position.Y, position.Z);
            rotationMatrix = Matrix4.CreateRotationY(rotationYRadians) * Matrix4.CreateRotationX(rotationXRadians);
            perspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(fovRadians, renderWidth / (float)renderHeight, nearClipPlane, farClipPlane);

            modelViewMatrix = rotationMatrix * translationMatrix;
            mvpMatrix = modelViewMatrix * perspectiveMatrix;
        }

        public void ResetToDefaultPosition()
        {
            position = new Vector3(0, 10, -80);
            rotationXRadians = 0;
            rotationYRadians = 0;
        }

        public void FrameBoundingSphere(Vector3 center, float radius)
        {
            // Calculate a right triangle using the bounding sphere radius as the height and the fov as the angle.
            // The distance is the base of the triangle. 
            float distance = radius / (float)Math.Tan(fovRadians / 2.0f);

            float offset = 10 / fovRadians;
            rotationXRadians = 0;
            rotationYRadians = 0;
            position.X = -center.X;
            position.Y = center.Y;
            position.Z = -1 * (distance + offset);
        }
    }
}
