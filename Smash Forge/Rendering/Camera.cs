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
        public Vector3 position = new Vector3(0, 10, -80);
        public float rotX = 0;
        public float rotY = 0;
        public float fovRadians = 0.524f;
        public float renderDepth = 100000;

        public int renderWidth = 1;
        public int renderHeight = 1;

        // Matrices for rendering.
        public Matrix4 modelViewMatrix = Matrix4.Identity;
        public Matrix4 mvpMatrix = Matrix4.Identity;
        public Matrix4 projectionMatrix = Matrix4.Identity;
        public Matrix4 billboardMatrix = Matrix4.Identity;
        public Matrix4 billboardYMatrix = Matrix4.Identity;
        public Matrix4 rotationMatrix = Matrix4.Identity;
        public Matrix4 translation = Matrix4.Identity;
        public Matrix4 perspFov = Matrix4.Identity;

        // Camera control settings. 
        public float zoomMultiplier = Runtime.zoomModifierScale; 
        public float zoomSpeed = Runtime.zoomspeed;
        public float mouseTranslateSpeed = 0.050f;
        public float scrollWheelZoomSpeed = 1.75f;
        public float shiftZoomMultiplier = 2.5f;
        public float mouseSLast = 0;
        public float mouseYLast = 0;
        public float mouseXLast = 0;

        public Camera()
        {

        }

        public Camera(Vector3 position, float rotX, float rotY, int renderWidth = 1, int renderHeight = 1)
        {
            this.position = position;
            this.renderHeight = renderHeight;
            this.renderWidth = renderWidth;
            this.rotX = rotX;
            this.rotY = rotY;
        }

        public void SetFromBone(Bone b)
        {
            Matrix4 Mat = b.transform.Inverted();
            translation = Matrix4.CreateTranslation(Mat.ExtractTranslation());
            rotationMatrix = Matrix4.CreateFromQuaternion(Mat.ExtractRotation());
            perspFov = Matrix4.CreatePerspectiveFieldOfView(fovRadians, renderWidth / (float)renderHeight, 1.0f, renderDepth);

            modelViewMatrix = Mat;
            mvpMatrix = modelViewMatrix * perspFov;
            billboardMatrix = translation * perspFov;
            billboardYMatrix = Matrix4.CreateRotationX(rotX) * translation * perspFov;
        }

        public void Update()
        {
            try
            {
                OpenTK.Input.Mouse.GetState();

                // left click drag to rotate. right click drag to pan
                if ((OpenTK.Input.Mouse.GetState().RightButton == OpenTK.Input.ButtonState.Pressed))
                {
                    position.Y += mouseTranslateSpeed * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                    position.X += mouseTranslateSpeed * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
                }
                if ((OpenTK.Input.Mouse.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed))
                {
                    rotY += 0.0125f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
                    rotX += 0.005f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                }

                float zoomscale = zoomSpeed;

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

                // Update the mouse values. 
                TrackMouse();
            }
            catch (Exception)
            {
                // RIP OpenTK...
            }

            UpdateMatrices();
        }

        private void UpdateMatrices()
        {
            translation = Matrix4.CreateTranslation(position.X, -position.Y, position.Z);
            rotationMatrix = Matrix4.CreateRotationY(rotY) * Matrix4.CreateRotationX(rotX);
            perspFov = Matrix4.CreatePerspectiveFieldOfView(fovRadians, renderWidth / (float)renderHeight, 1.0f, renderDepth);

            modelViewMatrix = rotationMatrix * translation;
            mvpMatrix = modelViewMatrix * perspFov;
            billboardMatrix = translation * perspFov;
            billboardYMatrix = Matrix4.CreateRotationX(rotX) * translation * perspFov;
        }

        public void TrackMouse()
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
            renderDepth = (float)ParamTools.GetParamValue(stprm, 0, 0, 77);           
        }

        public void ResetPositionRotation()
        {
            position = new Vector3(0, 10, -80);
            rotX = 0;
            rotY = 0;
        }

        public void FrameSelection(Vector3 center, float radius)
        {
            // Calculate a right triangle using the bounding box radius as the height and the fov as the angle.
            // The distance is the base of the triangle. 
            float distance = radius / (float)Math.Tan(fovRadians / 2.0f);

            float offset = 10 / fovRadians;
            rotX = 0;
            rotY = 0;
            position.X = -center.X;
            position.Y = center.Y;
            position.Z = -1 * (distance + offset);
        }
    }
}
