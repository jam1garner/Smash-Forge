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
        public Camera Camera;

        public string VBNFile;
        public string OMOFile;
        public VBN VBN;
        public Animation Anim;

        public CameraSettings(Camera c)
        {
            InitializeComponent();
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

            Camera = c;
            depthSlider.Value = Math.Min((int)Camera.renderDepth, depthSlider.Maximum);
            fovSlider.Value = (int)(Camera.fovRadians * 180.0f / Math.PI);
            fovTB.Text = (int)(Camera.fovRadians * 180.0f / Math.PI) + "";
            renderDepthTB.Text = Camera.renderDepth + "";
            updatePosition();
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

            Camera.position = new OpenTK.Vector3(width, height, zoom);
            Camera.rotX = xRotation;
            Camera.rotY = yRotation;
            Camera.Update();
        }

        // Updates text controls based on parentViewport's current camera position
        public void updatePosition()
        {
            OpenTK.Vector3 pos = Camera.position;

            numericHorizontalRadians.Value = Convert.ToDecimal(Camera.rotY);
            numericVerticalRadians.Value = Convert.ToDecimal(Camera.rotX);
            numericPositionX.Value = Convert.ToDecimal(pos.X);
            numericPositionY.Value = Convert.ToDecimal(pos.Y);
            numericZoom.Value = Convert.ToDecimal(pos.Z);

            // derived values
            numericHorizontalDegrees.Value = Convert.ToDecimal(Camera.rotY * (180 / Math.PI));
            numericVerticalDegrees.Value = Convert.ToDecimal(Camera.rotX * (180 / Math.PI));

            //buttonApply_Click(null, null);
        }

        private void numericHorizontalDegrees_ValueChanged(object sender, EventArgs e)
        {
            numericHorizontalRadians.Value = Convert.ToDecimal(Convert.ToSingle(numericHorizontalDegrees.Value) * (Math.PI / 180));
            //buttonApply_Click(null, null);
        }

        private void numericVerticalDegrees_ValueChanged(object sender, EventArgs e)
        {
            numericVerticalRadians.Value = Convert.ToDecimal(Convert.ToSingle(numericVerticalDegrees.Value) * (Math.PI / 180));
            //buttonApply_Click(null, null);
        }

        private void numericHorizontalRadians_ValueChanged(object sender, EventArgs e)
        {
            numericHorizontalDegrees.Value = Convert.ToDecimal(Convert.ToSingle(numericHorizontalRadians.Value) * (180 / Math.PI));
            //buttonApply_Click(null, null);
        }

        private void numericVerticalRadians_ValueChanged(object sender, EventArgs e)
        {
            numericVerticalDegrees.Value = Convert.ToDecimal(Convert.ToSingle(numericVerticalRadians.Value) * (180 / Math.PI));
            //buttonApply_Click(null, null);
        }

        private void fovSlider_Scroll(object sender, EventArgs e)
        {
            fovTB.Text = fovSlider.Value + "";
            //buttonApply_Click(null, null);
        }

        private void depthSlider_Scroll(object sender, EventArgs e)
        {
            renderDepthTB.Text = depthSlider.Value + "";
            //buttonApply_Click(null, null);
        }

        private void renderDepthTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(renderDepthTB.Text, out i))
            {
                renderDepthTB.BackColor = Color.White;
                Camera.renderDepth = i;
            }
            else
                renderDepthTB.BackColor = Color.Red;

            // update trackbar
            int newSliderValue = (int)(i);
            newSliderValue = Math.Min(newSliderValue, depthSlider.Maximum);
            newSliderValue = Math.Max(newSliderValue, depthSlider.Minimum);
            depthSlider.Value = newSliderValue;
            //buttonApply_Click(null, null);
        }

        private void fovTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(fovTB.Text, out i))
            {
                fovTB.BackColor = Color.White;
                Camera.fovRadians = i * (float)Math.PI / 180.0f;
            }
            else
                fovTB.BackColor = Color.Red;

            // update trackbar
            int newSliderValue = (int)(i);
            newSliderValue = Math.Min(newSliderValue, fovSlider.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            fovSlider.Value = newSliderValue;
            //buttonApply_Click(null, null);
        }

        private void btnLoadAnim_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Camera Animation (.omo)|*.omo";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    OMOFile = ofd.FileName;
                    Anim = OMOOld.read(new FileData(OMOFile));
                    lbAnimation.Text = OMOFile;
                }
                else
                {
                    OMOFile = "";
                    Anim = null;
                    lbAnimation.Text = OMOFile;
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
                    VBNFile = ofd.FileName;
                    VBN = new VBN(VBNFile);
                    lbVBN.Text = VBNFile;
                }else
                {
                    VBNFile = "";
                    VBN = null;
                    lbVBN.Text = VBNFile;
                }
            }
        }

        public void ApplyCameraAnimation(Camera Cam, int frame)
        {
            if (useCameraAnimation.Checked)
            {
                if(VBN != null && Anim != null)
                {
                    Anim.SetFrame(frame);
                    Anim.NextFrame(VBN);

                    if(VBN.bones.Count > 0)
                        Cam.SetFromBone(VBN.bones[0]);
                }
            }
        }

        private void FreeCamToggle_CheckedChanged(object sender, EventArgs e)
        {
            if (FreeCamToggle.Checked)
            {
                Camera.FreeCam();
            }
        }
    }
}
