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

            SetNumericUpDownMaxMinValues();

            camera = c;
            depthSlider.Value = Math.Min((int)camera.FarClipPlane, depthSlider.Maximum);
            fovSlider.Value = (int)(camera.FovDegrees);
            fovTB.Text = (int)(camera.FovDegrees) + "";
            renderDepthTB.Text = camera.FarClipPlane + "";
            updatePosition();
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
            updatePosition();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        { 
            float yRotation = Convert.ToSingle(numericHorizontalRadians.Value);
            float xRotation = Convert.ToSingle(numericVerticalRadians.Value);
            float width = Convert.ToSingle(numericPositionX.Value);
            float height = Convert.ToSingle(numericPositionY.Value);
            float zoom = Convert.ToSingle(numericZoom.Value);

            camera.Position = new OpenTK.Vector3(width, height, zoom);
            camera.RotationX = xRotation;
            camera.RotationY = yRotation;
            camera.Update();
        }

        // Updates text controls based on parentViewport's current camera position
        public void updatePosition()
        {
            OpenTK.Vector3 pos = camera.Position;

            numericHorizontalRadians.Value = Convert.ToDecimal(camera.RotationY);
            numericVerticalRadians.Value = Convert.ToDecimal(camera.RotationX);
            numericPositionX.Value = Convert.ToDecimal(pos.X);
            numericPositionY.Value = Convert.ToDecimal(pos.Y);
            numericZoom.Value = Convert.ToDecimal(pos.Z);

            // derived values
            numericHorizontalDegrees.Value = Convert.ToDecimal(camera.RotationY * (180 / Math.PI));
            numericVerticalDegrees.Value = Convert.ToDecimal(camera.RotationX * (180 / Math.PI));
        }

        private void numericHorizontalDegrees_ValueChanged(object sender, EventArgs e)
        {
            numericHorizontalRadians.Value = Convert.ToDecimal(Convert.ToSingle(numericHorizontalDegrees.Value) * (Math.PI / 180));
        }

        private void numericVerticalDegrees_ValueChanged(object sender, EventArgs e)
        {
            numericVerticalRadians.Value = Convert.ToDecimal(Convert.ToSingle(numericVerticalDegrees.Value) * (Math.PI / 180));
        }

        private void numericHorizontalRadians_ValueChanged(object sender, EventArgs e)
        {
            numericHorizontalDegrees.Value = Convert.ToDecimal(Convert.ToSingle(numericHorizontalRadians.Value) * (180 / Math.PI));
        }

        private void numericVerticalRadians_ValueChanged(object sender, EventArgs e)
        {
            numericVerticalDegrees.Value = Convert.ToDecimal(Convert.ToSingle(numericVerticalRadians.Value) * (180 / Math.PI));
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

            // update trackbar
            GuiTools.UpdateTrackBarFromValue(value, depthSlider, 0, depthSlider.Maximum);
            /*int newSliderValue = (int)(value);
            newSliderValue = Math.Min(newSliderValue, depthSlider.Maximum);
            newSliderValue = Math.Max(newSliderValue, depthSlider.Minimum);
            depthSlider.Value = newSliderValue;*/
        }

        private void fovTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(fovTB);
            camera.FovDegrees = value;

            // update trackbar
            int newSliderValue = (int)(value);
            newSliderValue = Math.Min(newSliderValue, fovSlider.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            fovSlider.Value = newSliderValue;
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

        public void ApplyCameraAnimation(Camera Cam, int frame)
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
    }
}
