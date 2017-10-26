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
    public partial class CameraPosition : Form
    {
        VBNViewport parentViewport;

        public CameraPosition(VBNViewport parentViewport)
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

            this.parentViewport = parentViewport;
            //this.TopMost = true;
        }

        private void CameraPosition_Load(object sender, EventArgs e)
        {
            updatePosition();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            parentViewport.cameraYRotation = Convert.ToSingle(numericHorizontalRadians.Value);
            parentViewport.cameraXRotation = Convert.ToSingle(numericVerticalRadians.Value);
            parentViewport.width = Convert.ToSingle(numericPositionX.Value);
            parentViewport.height = Convert.ToSingle(numericPositionY.Value);
            parentViewport.zoom = Convert.ToSingle(numericZoom.Value);
            parentViewport.UpdateMousePosition();
        }

        // Updates text controls based on parentViewport's current camera position
        public void updatePosition()
        {
            numericHorizontalRadians.Value = Convert.ToDecimal(parentViewport.cameraYRotation);
            numericVerticalRadians.Value = Convert.ToDecimal(parentViewport.cameraXRotation);
            numericPositionX.Value = Convert.ToDecimal(parentViewport.width);
            numericPositionY.Value = Convert.ToDecimal(parentViewport.height);
            numericZoom.Value = Convert.ToDecimal(parentViewport.zoom);

            // derived values
            numericHorizontalDegrees.Value = Convert.ToDecimal(parentViewport.cameraYRotation * (180 / Math.PI));
            numericVerticalDegrees.Value = Convert.ToDecimal(parentViewport.cameraXRotation * (180 / Math.PI));
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
    }
}
