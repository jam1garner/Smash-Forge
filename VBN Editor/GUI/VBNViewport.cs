using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Security.Cryptography;
using SALT.Scripting.AnimCMD;

namespace VBN_Editor
{
    public partial class VBNViewport : GLControl
    {
        public VBNViewport()
        {
            InitializeComponent();
            Hitboxes = new SortedList<int, Hitbox>();
        }

        public bool _controlLoaded;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                GL.ClearColor(Color.AliceBlue);
                SetupViewPort();
            }
            render = true;
            _controlLoaded = true;
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            int h = Height;
            int w = Width;
            if (!DesignMode && _controlLoaded)
            {
                GL.LoadIdentity();
                GL.Viewport(0, 0, w, h);
                v = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1.0f, 40.0f);
            }
        }

        public event EventHandler FrameChanged;
        protected virtual void OnFrameChanged(EventArgs e)
        {
            //FrameChanged.Invoke(this, e);
            //HandleACMD(AnimName);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        internal int Frame
        {
            get
            {
                return _frame;
            }
            set
            {
                _frame = value;
                OnFrameChanged(new EventArgs());
            }
        }
        private int _frame = 0;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        internal MovesetManager Moveset { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        private SortedList<int, Hitbox> Hitboxes { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        internal VBN TargetVBN { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        internal SkelAnimation TargetAnim { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        internal string AnimName { get; set; }

        public void SetTarget(string animName, SkelAnimation animation, VBN boneset)
        {
            TargetVBN = boneset;
            TargetAnim = animation;
            AnimName = animName;
        }
        public void SetFrame(int frame)
        {
            TargetAnim.setFrame(frame);
            TargetAnim.nextFrame(TargetVBN);
        }

        Matrix4 v;
        float rot = 0;
        float lookup = 0;
        float height = 0;
        float width = 0;
        float zoom = 0;
        float mouseXLast = 0;
        float mouseYLast = 0;
        float mouseSLast = 0;
        bool render = false;

		// opentk shader stuffs
		Shader shader;

        private void SetupViewPort()
        {
			shader = new Shader();

			if (File.Exists ("vs.glsl")) {
				shader.vertexShader ("vs.glsl");
				shader.fragmentShader ("fr.glsl");

				shader.addAttribute ("vPosition", false);
				shader.addAttribute ("vColor", false);
				shader.addAttribute ("vNormal", false);
				shader.addAttribute ("vUV", false);
				shader.addAttribute ("vBone", false);
				shader.addAttribute ("vWeight", false);

				shader.addAttribute ("tex", true);
				shader.addAttribute ("modelview", true);
				shader.addAttribute ("bones", true);
			}


            int h = Height;
            int w = Width;
            GL.LoadIdentity();
            GL.Viewport(0, 0, w, h);
            v = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1.0f, 40.0f);

            GotFocus += (object sender, EventArgs e) =>
            {
                mouseXLast = OpenTK.Input.Mouse.GetState().X;
                mouseYLast = OpenTK.Input.Mouse.GetState().Y;
                zoom = OpenTK.Input.Mouse.GetState().WheelPrecise;
                mouseSLast = zoom;
            };
        }
        private bool MouseIsOverViewport()
        {
            if (ClientRectangle.Contains(PointToClient(Cursor.Position)))
            {
                return true;
            }
            return false;
        }

		public void Render(VBN skeleton, NUD model)
        {
            if (!render)
                return;

            // Push all attributes so we don't have to clean up later
            GL.PushAttrib(AttribMask.AllAttribBits);

            // clear the gf buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.LineSmooth); // This is Optional 
            GL.Enable(EnableCap.Normalize);  // These is critical to have
            GL.Enable(EnableCap.RescaleNormal);

            // set up the viewport projection and send it to GPU
            GL.MatrixMode(MatrixMode.Projection);
            GL.ShadeModel(ShadingModel.Flat);

            // Poll the mouse for changes
            UpdateMousePosition();

            GL.LoadMatrix(ref v);

            // ready to start drawing model stuff
            GL.MatrixMode(MatrixMode.Modelview);

			// draw models
			if (model != null && skeleton != null) {
				GL.UseProgram (shader.programID);

				GL.UniformMatrix4 (shader.getAttribute("modelview"), false, ref v);

				float[] f = skeleton.getShaderMatrix ();

				GL.UniformMatrix4 (shader.getAttribute("bone"), f.Length, false, f);

				shader.enableAttrib ();
				model.Render (shader);
				shader.disableAttrib ();
			}


			GL.UseProgram (0);

            // draw the grid floor first
            drawFloor(Matrix4.CreateTranslation(Vector3.Zero));

            // clear the buffer bit so the skeleton 
            // will be drawn on top of everything
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // drawing the bones
            if (TargetVBN != null)
            {
                // Render the hitboxes
                if (!string.IsNullOrEmpty(AnimName))
                    HandleACMD(AnimName);
                RenderHitboxes();

                foreach (Bone bone in TargetVBN.bones)
                {
                    // first calcuate the point and draw a point
                    GL.Color3(Color.GreenYellow);

                    Vector3 pos_c = Vector3.Transform(Vector3.Zero, bone.transform/* * scale*/);
                    drawCube(pos_c, .085f);

                    // now draw line between parent 
                    GL.Color3(Color.Blue);
                    GL.LineWidth(1f);

                    GL.Begin(PrimitiveType.Lines);
                    if (bone.parentIndex != 0x0FFFFFFF && bone.parentIndex != -1)
                    {
                        int i = bone.parentIndex;
                        Vector3 pos_p = Vector3.Transform(Vector3.Zero, TargetVBN.bones[i].transform/* * scale*/);
                        GL.Vertex3(pos_c);
                        GL.Vertex3(pos_p);
                    }
                    GL.End();
                }
            }

            // Clean up
            GL.PopAttrib();
            SwapBuffers();
        }
        public void UpdateMousePosition()
        {
            if (MouseIsOverViewport())
            {
                if (OpenTK.Input.Mouse.GetState().IsButtonDown(OpenTK.Input.MouseButton.Right))
                {
                    height += 0.025f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                    width += 0.025f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
                    height = clampControl(height);
                    width = clampControl(width);
                }
                if (OpenTK.Input.Mouse.GetState().IsButtonDown(OpenTK.Input.MouseButton.Left))
                {
                    rot += 0.025f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
                    lookup += 0.025f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                }
                v = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1.0f, 80.0f);

                mouseXLast = OpenTK.Input.Mouse.GetState().X;
                mouseYLast = OpenTK.Input.Mouse.GetState().Y;

                zoom += OpenTK.Input.Mouse.GetState().WheelPrecise - mouseSLast;
                mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;
            }
        }
        public void RenderHitboxes()
        {
            if (Hitboxes.Count > 0)
            {
                GL.Color4(Color.FromArgb(85, Color.Red));
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.Blend);

                foreach (var pair in Hitboxes)
                {
                    var h = pair.Value;
                    if (Frame < h.EndFrame && Frame >= h.StartFrame)
                    {
                        var va = Vector3.Transform(new Vector3(h.X, h.Y, h.Z), TargetVBN.bones[h.Bone].transform.ClearScale());

                        GL.DepthMask(false);
                        drawSphere(va, h.Size, 30);
                    }
                }

                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.DepthTest);
            }
        }

        public void HandleACMD(string animname)
        {
            var crc = Crc32.Compute(animname.ToLower());

            if (Moveset == null)
                return;

            if (!Moveset.Game.Scripts.ContainsKey(crc))
                return;

            int frame = 0;
            foreach (var cmd in (ACMDScript)Moveset.Game.Scripts[crc])
            {
                switch (cmd.Ident)
                {
                    case 0x4B7B6E51: // Synchronous Timer
                        frame += (int)(float)cmd.Parameters[0];
                        break;
                    case 0x42ACFE7D: // Asynchronous Timer
                        frame = (int)(float)cmd.Parameters[0];
                        break;
                    case 0xB738EABD: // hitbox commands
                    case 0xFAA85333:
                    case 0x321297B0:
                    case 0x2988D50F:
                    case 0xED67D5DA:
                    case 0x14FCC7E4:
                    case 0x7640AEEB:
                    case 0x7075DC5A:
                        {
                            Hitbox h = new Hitbox();
                            h.StartFrame = frame;
                            int id = (int)cmd.Parameters[0];
                            h.Bone = ((int)cmd.Parameters[2] - 1).Clamp(0, int.MaxValue);
                            h.Damage = (float)cmd.Parameters[3];
                            h.Angle = (int)cmd.Parameters[4];
                            h.KnockbackGrowth = (int)cmd.Parameters[5];
                            //FKB = (float)cmd.Parameters[6]
                            h.KnockbackBase = (int)cmd.Parameters[7];
                            h.Size = (float)cmd.Parameters[8];
                            h.X = (float)cmd.Parameters[9];
                            h.Y = (float)cmd.Parameters[10];
                            h.Z = (float)cmd.Parameters[11];
                            // I don't really know how the game handles hitboxes
                            // that use the same id, so i'm just gonna assume one
                            // replaces the other.
                            if (!Hitboxes.ContainsKey(id))
                                Hitboxes.Add(id, h);
                            else
                                Hitboxes[id] = h;
                            break;
                        }
                    case 0x9245E1A8: // clear all hitboxes
                        if (Hitboxes.Count > 0)
                            foreach (var hitbox in Hitboxes)
                                hitbox.Value.EndFrame = frame;
                        break;
                    case 0xFF379EB6: // delete hitbox
                        if (Hitboxes.ContainsKey((int)cmd.Parameters[0]))
                        {
                            Hitboxes[(int)cmd.Parameters[0]].EndFrame = frame;
                        }
                        break;
                }
            }
        }
        private float clampControl(float f)
        {
            if (f < -5)
                f = -5;
            if (f > 5)
                f = 5;
            return f;
        }
        public void drawFloor(Matrix4 s)
        {
            // Draw floor plane
            GL.Color3(Color.LightGray);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(-20f, 0f, -20f);
            GL.Vertex3(20f, 0f, -20f);
            GL.Vertex3(20f, 0f, 20f);
            GL.Vertex3(-20f, 0f, 20f);
            GL.End();
            // Draw grid over it

            GL.Color3(Color.DimGray);
            GL.LineWidth(1f);
            GL.Begin(PrimitiveType.Lines);
            for (var i = -10; i <= 10; i++)
            {
                GL.Vertex3(Vector3.Transform(new Vector3(-10f*2, 0f, i*2), s));
                GL.Vertex3(Vector3.Transform(new Vector3(10f*2, 0f, i*2), s));
                GL.Vertex3(Vector3.Transform(new Vector3(i*2, 0f, -10f*2), s));
                GL.Vertex3(Vector3.Transform(new Vector3(i*2, 0f, 10f*2), s));
            }
            GL.End();

            GL.Enable(EnableCap.DepthTest);
        }

        public void drawCircle(float x, float y, float z, float radius, uint precision)
        {
            drawCircle(new Vector3(x, y, z), radius, precision);
        }
        public void drawCircle(Vector3 center, float radius, uint precision)
        {
            float theta = 2.0f * (float)Math.PI / precision;
            float cosine = (float)Math.Cos(theta);
            float sine = (float)Math.Sin(theta);

            float x = radius;
            float y = 0;

            GL.Begin(PrimitiveType.TriangleFan);
            for (int i = 0; i < precision; i++)
            {
                GL.Vertex2(x + center.X, y + center.Y);

                //apply the rotation matrix
                var temp = x;
                x = cosine * x - sine * y;
                y = sine * temp + cosine * y;
            }
            GL.End();
        }

        public void drawCube(Vector3 center, float size)
        {
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);

            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);

            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);
            GL.End();
        }
        public void drawCubeWireframe(Vector3 center, float size)
        {
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);

            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);

            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);
            GL.End();
        }

        // Taken from Brawllib render TKContext.cs
        public static void drawSphere(Vector3 center, float radius, uint precision)
        {

            if (radius < 0.0f)
                radius = -radius;

            if (radius == 0.0f)
                throw new DivideByZeroException("DrawSphere: Radius cannot be zero.");

            if (precision == 0)
                throw new DivideByZeroException("DrawSphere: Precision of 8 or greater is required.");

            float halfPI = (float)(Math.PI * 0.5);
            float oneThroughPrecision = 1.0f / precision;
            float twoPIThroughPrecision = (float)(Math.PI * 2.0 * oneThroughPrecision);

            float theta1, theta2, theta3;
            Vector3 norm = new Vector3(), pos = new Vector3();

            for (uint j = 0; j < precision / 2; j++)
            {
                theta1 = (j * twoPIThroughPrecision) - halfPI;
                theta2 = ((j + 1) * twoPIThroughPrecision) - halfPI;

                GL.Begin(PrimitiveType.TriangleStrip);
                for (uint i = 0; i <= precision; i++)
                {
                    theta3 = i * twoPIThroughPrecision;

                    norm.X = (float)(Math.Cos(theta2) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta2);
                    norm.Z = (float)(Math.Cos(theta2) * Math.Sin(theta3));
                    pos.X = center.X + radius * norm.X;
                    pos.Y = center.Y + radius * norm.Y;
                    pos.Z = center.Z + radius * norm.Z;

                    GL.Normal3(norm.X, norm.Y, norm.Z);
                    GL.TexCoord2(i * oneThroughPrecision, 2.0f * (j + 1) * oneThroughPrecision);
                    GL.Vertex3(pos.X, pos.Y, pos.Z);

                    norm.X = (float)(Math.Cos(theta1) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta1);
                    norm.Z = (float)(Math.Cos(theta1) * Math.Sin(theta3));
                    pos.X = center.X + radius * norm.X;
                    pos.Y = center.Y + radius * norm.Y;
                    pos.Z = center.Z + radius * norm.Z;

                    GL.Normal3(norm.X, norm.Y, norm.Z);
                    GL.TexCoord2(i * oneThroughPrecision, 2.0f * j * oneThroughPrecision);
                    GL.Vertex3(pos.X, pos.Y, pos.Z);
                }
                GL.End();
            }
        }
    }
}
