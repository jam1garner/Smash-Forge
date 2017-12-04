using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge.GUI.Menus
{
    public partial class CameraSettings : Form
    {
        public CameraSettings()
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

            depthSlider.Value = Math.Min((int)Runtime.renderDepth, depthSlider.Maximum);
            fovSlider.Value = (int)(Camera.viewportCamera.fov * 180.0f / Math.PI);
            fovTB.Text = (int)(Camera.viewportCamera.fov * 180.0f / Math.PI) + "";
            renderDepthTB.Text = Runtime.renderDepth + "";
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

            Camera.viewportCamera.setPosition(new OpenTK.Vector3(width, height, zoom));
            Camera.viewportCamera.setRotX(xRotation);
            Camera.viewportCamera.setRotY(yRotation);
            Camera.viewportCamera.Update();
        }

        // Updates text controls based on parentViewport's current camera position
        public void updatePosition()
        {
            OpenTK.Vector3 pos = Camera.viewportCamera.getPosition();

            numericHorizontalRadians.Value = Convert.ToDecimal(Camera.viewportCamera.getRotY());
            numericVerticalRadians.Value = Convert.ToDecimal(Camera.viewportCamera.getRotX());
            numericPositionX.Value = Convert.ToDecimal(pos.X);
            numericPositionY.Value = Convert.ToDecimal(pos.Y);
            numericZoom.Value = Convert.ToDecimal(pos.Z);

            // derived values
            numericHorizontalDegrees.Value = Convert.ToDecimal(Camera.viewportCamera.getRotY() * (180 / Math.PI));
            numericVerticalDegrees.Value = Convert.ToDecimal(Camera.viewportCamera.getRotX() * (180 / Math.PI));
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
            float i = 0;
            if (float.TryParse(renderDepthTB.Text, out i))
            {
                renderDepthTB.BackColor = Color.White;
                Runtime.renderDepth = i;
            }
            else
                renderDepthTB.BackColor = Color.Red;

            // update trackbar
            int newSliderValue = (int)(i);
            newSliderValue = Math.Min(newSliderValue, depthSlider.Maximum);
            newSliderValue = Math.Max(newSliderValue, depthSlider.Minimum);
            depthSlider.Value = newSliderValue;
        }

        private void fovTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(fovTB.Text, out i))
            {
                fovTB.BackColor = Color.White;
                Camera.viewportCamera.fov = i * (float)Math.PI / 180.0f;
            }
            else
                fovTB.BackColor = Color.Red;

            // update trackbar
            int newSliderValue = (int)(i);
            newSliderValue = Math.Min(newSliderValue, fovSlider.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            fovSlider.Value = newSliderValue;
        }
    }
}
