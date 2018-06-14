using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Smash_Forge.Rendering;

namespace Smash_Forge.GUI.Menus
{
    public partial class CameraSettings : Form
    {
        public Camera camera;

        public string vbnFilePath;
        public string omoFilePath;
        public VBN vbn;
        public Animation animation;

        public CameraSettings(Camera c)
        {
            InitializeComponent();

            camera = c;

            // Initialize control values.
            SetNumericUpDownMaxMinValues();
            SetNumericUpDownValues();
            depthSlider.Value = Math.Min((int)camera.FarClipPlane, depthSlider.Maximum);
            fovSlider.Value = (int)(camera.FovDegrees);
            fovTB.Text = (int)(camera.FovDegrees) + "";
            renderDepthTB.Text = camera.FarClipPlane + "";
        }

        private void SetNumericUpDownMaxMinValues()
        {
            numericHorizontalRadians.Maximum = Decimal.MaxValue;
            numericHorizontalRadians.Minimum = Decimal.MinValue;
            numericHorizontalDegrees.Maximum = Decimal.MaxValue;
            numericHorizontalDegrees.Minimum = Decimal.MinValue;

            numericVerticalRadians.Maximum = Decimal.MaxValue;
            numericVerticalRadians.Minimum = Decimal.MinValue;
            numericVerticalDegrees.Maximum = Decimal.MaxValue;
            numericVerticalDegrees.Minimum = Decimal.MinValue;

            numericPositionX.Maximum = Decimal.MaxValue;
            numericPositionX.Minimum = Decimal.MinValue;
            numericPositionY.Maximum = Decimal.MaxValue;
            numericPositionY.Minimum = Decimal.MinValue;

            numericZoom.Maximum = Decimal.MaxValue;
            numericZoom.Minimum = Decimal.MinValue;
        }

        private void CameraPosition_Load(object sender, EventArgs e)
        {
            SetNumericUpDownValues();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            float yRotation = Convert.ToSingle(numericHorizontalRadians.Value);
            float xRotation = Convert.ToSingle(numericVerticalRadians.Value);
            float width = Convert.ToSingle(numericPositionX.Value);
            float height = Convert.ToSingle(numericPositionY.Value);
            float zoom = Convert.ToSingle(numericZoom.Value);

            camera.Position = new OpenTK.Vector3(width, height, zoom);
            camera.RotationXRadians = xRotation;
            camera.RotationYRadians = yRotation;
        }

        public void SetNumericUpDownValues()
        {
            // Rotation
            numericHorizontalRadians.Value = Convert.ToDecimal(camera.RotationYRadians);
            numericVerticalRadians.Value = Convert.ToDecimal(camera.RotationXRadians);
            numericHorizontalDegrees.Value = Convert.ToDecimal(camera.RotationYDegrees);
            numericVerticalDegrees.Value = Convert.ToDecimal(camera.RotationXDegrees);

            // Position
            numericPositionX.Value = Convert.ToDecimal(camera.Position.X);
            numericPositionY.Value = Convert.ToDecimal(camera.Position.Y);
            numericZoom.Value = Convert.ToDecimal(camera.Position.Z);
        }

        private void numericHorizontalDegrees_ValueChanged(object sender, EventArgs e)
        {
            // The Camera class handles the angle conversions.
            camera.RotationYDegrees = Convert.ToSingle(numericHorizontalDegrees.Value);
            numericHorizontalRadians.Value = Convert.ToDecimal(camera.RotationYRadians);
        }

        private void numericVerticalDegrees_ValueChanged(object sender, EventArgs e)
        {
            // The Camera class handles the angle conversions.
            camera.RotationXDegrees = Convert.ToSingle(numericVerticalDegrees.Value);
            numericVerticalRadians.Value = Convert.ToDecimal(camera.RotationXRadians);
        }

        private void numericHorizontalRadians_ValueChanged(object sender, EventArgs e)
        {
            // The Camera class handles the angle conversions.
            camera.RotationYRadians = Convert.ToSingle(numericHorizontalRadians.Value);
            numericHorizontalDegrees.Value = Convert.ToDecimal(camera.RotationYDegrees);
        }

        private void numericVerticalRadians_ValueChanged(object sender, EventArgs e)
        {
            // The Camera class handles the angle conversions.
            camera.RotationXRadians = Convert.ToSingle(numericVerticalRadians.Value);
            numericVerticalDegrees.Value = Convert.ToDecimal(camera.RotationXDegrees);
        }

        private void fovSlider_Scroll(object sender, EventArgs e)
        {
            fovTB.Text = fovSlider.Value + "";
        }

        private void depthSlider_Scroll(object sender, EventArgs e)
        {
            renderDepthTB.Text = depthSlider.Value + "";
        }

        private void renderDepthTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(renderDepthTB);
            // The far clip plane can't come before the near clip plane.
            if (value <= camera.NearClipPlane)
                value = camera.NearClipPlane + 0.001f;
            camera.FarClipPlane = value;

            GuiTools.UpdateTrackBarFromValue(value, depthSlider, 0, depthSlider.Maximum);
        }

        private void fovTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(fovTB);
            camera.FovDegrees = value;

            GuiTools.UpdateTrackBarFromValue(value, fovSlider, 0, fovSlider.Maximum);
        }

        private void btnLoadAnim_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Camera Animation (.omo)|*.omo";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    omoFilePath = ofd.FileName;
                    animation = OMOOld.read(new FileData(omoFilePath));
                    lbAnimation.Text = omoFilePath;
                }
                else
                {
                    omoFilePath = "";
                    animation = null;
                    lbAnimation.Text = omoFilePath;
                }
            }
        }

        private void btnLoadVBN_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Namco Bones (.vbn)|*.vbn";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    vbnFilePath = ofd.FileName;
                    vbn = new VBN(vbnFilePath);
                    lbVBN.Text = vbnFilePath;
                }else
                {
                    vbnFilePath = "";
                    vbn = null;
                    lbVBN.Text = vbnFilePath;
                }
            }
        }

        public void ApplyCameraAnimation(ForgeCamera Cam, int frame)
        {
            if (useCameraAnimation.Checked)
            {
                if(vbn != null && animation != null)
                {
                    animation.SetFrame(frame);
                    animation.NextFrame(vbn);

                    if(vbn.bones.Count > 0)
                        Cam.SetFromBone(vbn.bones[0]);
                }
            }
        }

        private void numericPositionX_ValueChanged(object sender, EventArgs e)
        {
            float positionX = Convert.ToSingle(numericPositionX.Value);
            camera.Position = new OpenTK.Vector3(positionX, camera.Position.Y, camera.Position.Z);
        }

        private void numericPositionY_ValueChanged(object sender, EventArgs e)
        {
            float positionY = Convert.ToSingle(numericPositionY.Value);
            camera.Position = new OpenTK.Vector3(camera.Position.X, positionY, camera.Position.Z);
        }

        private void numericZoom_ValueChanged(object sender, EventArgs e)
        {
            float positionZ = Convert.ToSingle(numericZoom.Value);
            camera.Position = new OpenTK.Vector3(camera.Position.X, camera.Position.Y, positionZ);
        }
    }
}
