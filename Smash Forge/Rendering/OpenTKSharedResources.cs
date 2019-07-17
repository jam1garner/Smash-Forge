using System;
using System.Runtime.ExceptionServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using SFGraphics.GLObjects.Shaders;


namespace SmashForge.Rendering
{
    static class OpenTkSharedResources
    {
        public enum SharedResourceStatus
        {
            Initialized,
            Failed,
            Uninitialized
        }

        public static SharedResourceStatus SetupStatus { get; private set; } = SharedResourceStatus.Uninitialized;

        // Keep a context around to avoid setting up after making each context.
        public static GameWindow dummyResourceWindow;

        public static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();

        private static DebugProc debugProc;

        [HandleProcessCorruptedStateExceptions]
        public static void InitializeSharedResources()
        {
            // Only setup once. This is checked multiple times to prevent crashes.
            if (SetupStatus == SharedResourceStatus.Initialized)
                return;

            try
            {
                // Make a permanent context to share resources.
                GraphicsContext.ShareContexts = true;
                dummyResourceWindow = CreateGameWindowContext();

                if (Runtime.enableOpenTkDebugOutput)
                    EnableOpenTkDebugOutput();

                RenderTools.LoadTextures();
                GetOpenGlSystemInfo();
                ShaderTools.SetUpShaders();

                SetupStatus = SharedResourceStatus.Initialized;
            }
            catch (AccessViolationException)
            {
                // Context creation failed.
                SetupStatus = SharedResourceStatus.Failed;
            }
        }

        public static void EnableOpenTkDebugOutput()
        {
#if DEBUG
            // This isn't free, so skip this step when not debugging.
            // TODO: Only works with Intel integrated.
            if (SFGraphics.GlUtils.OpenGLExtensions.IsAvailable("GL_KHR_debug"))
            {
                GL.Enable(EnableCap.DebugOutput);
                GL.Enable(EnableCap.DebugOutputSynchronous);
                debugProc = DebugCallback;
                GL.DebugMessageCallback(debugProc, IntPtr.Zero);
                int[] ids = { };
                GL.DebugMessageControl(DebugSourceControl.DontCare, DebugTypeControl.DontCare,
                    DebugSeverityControl.DontCare, 0, ids, true);
            }
#endif
        }

        private static void DebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            string debugMessage = Marshal.PtrToStringAnsi(message, length);
            Debug.WriteLine($"{severity} {type} {debugMessage}");
        }

        public static GameWindow CreateGameWindowContext(int width = 640, int height = 480)
        {
            GraphicsMode mode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 0, 0, ColorFormat.Empty, 1);

            GameWindow gameWindow = new GameWindow(width, height, mode, "", GameWindowFlags.Default)
            {
                Visible = false
            };
            gameWindow.MakeCurrent();
            return gameWindow;
        }

        private static void GetOpenGlSystemInfo()
        {
            Runtime.renderer = GL.GetString(StringName.Renderer);
            Runtime.openGlVersion = GL.GetString(StringName.Version);
            Runtime.glslVersion = GL.GetString(StringName.ShadingLanguageVersion);
        }
    }
}
