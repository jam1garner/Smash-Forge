using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SALT.PARAMS;

namespace Smash_Forge
{
    public class Camera
    {
        public static Camera viewportCamera = new Camera();

        private Vector3 position = new Vector3(0, 10, -80);
        private float cameraXRotation = 0;
        private float cameraYRotation = 0;
        private int renderWidth = 1;
        private int renderHeight = 1;
        private float farClipPlane = 10000;
        public float fov = 0.524f;

        private Matrix4 modelViewMatrix = Matrix4.Identity;
        private Matrix4 mvpMatrix = Matrix4.Identity;
        private Matrix4 projectionMatrix = Matrix4.Identity;

        private float zoomMultiplier = Runtime.zoomModifierScale; 
        private float zoomSpeed = Runtime.zoomspeed;
        private float mouseTranslateSpeed = 0.050f;
        private float scrollWheelZoomSpeed = 1.75f;
        private float shiftZoomMultiplier = 2.5f;
        public float mouseSLast, mouseYLast, mouseXLast;

        public Camera()
        { 

        }

        public Camera(Vector3 position, float rotX, float rotY, int renderWidth, int renderHeight)
        {
            this.position = position;
            this.renderHeight = renderHeight;
            this.renderWidth = renderWidth;
            cameraXRotation = rotX;
            cameraYRotation = rotY;
        }

        public Matrix4 getModelViewMatrix()
        {
            return modelViewMatrix;
        }

        public Matrix4 getMVPMatrix()
        {
            return mvpMatrix;
        }

        public Vector3 getPosition()
        {
            return position;
        }

        public float getFarClipPlane()
        {
            return farClipPlane;
        }

        public void setFarClipPlane(float farClip)
        {
            farClipPlane = farClip;
        }

        public void setRenderWidth(int width)
        {
            renderWidth = width;
        }

        public void setRenderHeight(int height)
        {
            renderHeight = height;
        }

        public float getRotX()
        {
            return cameraXRotation;
        }

        public float getRotY()
        {
            return cameraYRotation;
        }

        public void setRotX(float angle)
        {
            cameraXRotation = angle;
        }

        public void setRotY(float angle)
        {
            cameraYRotation = angle;
        }

        public void Update()
        {
            // left click drag to rotate. right click drag to pan
            if ((OpenTK.Input.Mouse.GetState().RightButton == OpenTK.Input.ButtonState.Pressed))
            {
                position.Y += mouseTranslateSpeed * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                position.X += mouseTranslateSpeed * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
            }
            if ((OpenTK.Input.Mouse.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed))
            {
                cameraYRotation += 0.0125f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
                cameraXRotation += 0.005f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
            }

            float zoomscale = zoomSpeed;

            // hold shift to change zoom speed
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ShiftLeft) || OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ShiftRight))
                zoomscale *= shiftZoomMultiplier;

            // zoom in or out with arrow keys
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.Down))
                position.Z -= 1 * zoomscale;
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.Up))
                position.Z += 1 * zoomscale;

            TrackMouse();

            // scrollwheel to zoom in our out
            position.Z += (OpenTK.Input.Mouse.GetState().WheelPrecise - mouseSLast) * zoomscale * scrollWheelZoomSpeed;

            UpdateMatrices();
        }

        public void setPosition(Vector3 newPosition)
        {
            this.position = newPosition;
        }

        private void UpdateMatrices()
        {
            Matrix4 translation = Matrix4.CreateTranslation(position.X, -position.Y, position.Z);
            Matrix4 rotation = Matrix4.CreateRotationY(cameraYRotation) * Matrix4.CreateRotationX(cameraXRotation);
            Matrix4 perspFOV = Matrix4.CreatePerspectiveFieldOfView(fov, renderWidth / (float)renderHeight, 1.0f, farClipPlane);

            // need to fix these
            modelViewMatrix = Matrix4.CreatePerspectiveFieldOfView(fov, renderWidth / (float)renderHeight,
                1.0f, Runtime.renderDepth) * Matrix4.CreateRotationY(cameraYRotation) * Matrix4.CreateRotationX(cameraXRotation);

            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(fov, renderWidth / (float)renderHeight,
                1.0f, Runtime.renderDepth);
            mvpMatrix = rotation * translation * perspFOV;
        }

        public void TrackMouse()
        {
            this.mouseXLast = OpenTK.Input.Mouse.GetState().X;
            this.mouseYLast = OpenTK.Input.Mouse.GetState().Y;
            this.mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;
        }

        public static void SetCameraFromSTPRM(ParamFile stprm)
        {
            if (stprm != null)
            {
                float fov = (float)RenderTools.GetValueFromParamFile(stprm, 0, 0, 6);
                Runtime.fov = fov * ((float)Math.PI / 180.0f);
                Runtime.renderDepth = (float)RenderTools.GetValueFromParamFile(stprm, 0, 0, 77);
            }
        }
    }
}
