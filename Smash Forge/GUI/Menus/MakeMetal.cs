using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;



namespace SmashForge.Gui.Menus
{
    public partial class MakeMetal : Form
    {
        private Nud nud;

        private int difTexId = 0x40000001;
        private int cubeTexId = 0x10102000;

        private bool preserveNormalMap = true;
        private bool preserveDiffuse = false;

        private float[] minGain = new float[] { 0.3f, 0.3f, 0.3f, 1 };
        private float[] refColor = new float[] { 3f, 3f, 3f, 1 };
        private float[] fresParams = new float[] { 3.7f, 0f, 0f, 1 };
        private float[] fresColor = new float[] { 0.6f, 0.6f, 0.6f, 1 };

        public MakeMetal(Nud nud)
        {
            InitializeComponent();
            this.nud = nud;
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            nud.MakeMetal(difTexId, cubeTexId, minGain, refColor, fresParams, fresColor, preserveDiffuse, preserveNormalMap);
        }

        private void useDifTexCB_CheckedChanged(object sender, EventArgs e)
        {
            preserveDiffuse = !useDifTexCB.Checked;
            difIDTextBox.Enabled = useDifTexCB.Checked;
            label1.Enabled = useDifTexCB.Checked;
        }

        private void prserveNrmRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            preserveNormalMap = true;
        }

        private void noNrmradioButton_CheckedChanged(object sender, EventArgs e)
        {
            preserveNormalMap = false;
        }

        private void difIDTextBox_TextChanged(object sender, EventArgs e)
        {
            difTexId = GuiTools.TryParseTBInt(difIDTextBox, true, true);
        }

        private void Property1X_TextChanged(object sender, EventArgs e)
        {
            refColor[0] = GuiTools.TryParseTBFloat(Property1X);
        }

        private void Property1Y_TextChanged(object sender, EventArgs e)
        {
            refColor[1] = GuiTools.TryParseTBFloat(Property1Y);
        }

        private void Property1Z_TextChanged(object sender, EventArgs e)
        {
            refColor[2] = GuiTools.TryParseTBFloat(Property1Z);
        }

        private void Property1W_TextChanged(object sender, EventArgs e)
        {
            refColor[3] = GuiTools.TryParseTBFloat(Property1W);
        }

        private void Property2X_TextChanged(object sender, EventArgs e)
        {
            fresColor[0] = GuiTools.TryParseTBFloat(Property2X);
        }

        private void Property2Y_TextChanged(object sender, EventArgs e)
        {
            fresColor[1] = GuiTools.TryParseTBFloat(Property2Y);
        }

        private void Property2Z_TextChanged(object sender, EventArgs e)
        {
            fresColor[2] = GuiTools.TryParseTBFloat(Property2Z);
        }

        private void Property2W_TextChanged(object sender, EventArgs e)
        {
            fresColor[3] = GuiTools.TryParseTBFloat(Property2W);
        }

        private void Property3X_TextChanged(object sender, EventArgs e)
        {
            fresParams[0] = GuiTools.TryParseTBFloat(Property3X);
        }

        private void Property3Y_TextChanged(object sender, EventArgs e)
        {
            fresParams[1] = GuiTools.TryParseTBFloat(Property3Y);
        }

        private void Property3Z_TextChanged(object sender, EventArgs e)
        {
            fresParams[2] = GuiTools.TryParseTBFloat(Property3Z);
        }

        private void Property3W_TextChanged(object sender, EventArgs e)
        {
            fresParams[3] = GuiTools.TryParseTBFloat(Property3W);
        }

        private void property4X_TextChanged(object sender, EventArgs e)
        {
            minGain[0] = GuiTools.TryParseTBFloat(Property4X);
        }

        private void Property4Y_TextChanged(object sender, EventArgs e)
        {
            minGain[1] = GuiTools.TryParseTBFloat(Property4Y);
        }

        private void Property4Z_TextChanged(object sender, EventArgs e)
        {
            minGain[2] = GuiTools.TryParseTBFloat(Property4Z);
        }

        private void Property4W_TextChanged(object sender, EventArgs e)
        {
            minGain[3] = GuiTools.TryParseTBFloat(Property4W);
        }

        private void highResradioButton_CheckedChanged(object sender, EventArgs e)
        {
            cubeTexId = 0x10102000;
        }

        private void lowResradioButton_CheckedChanged(object sender, EventArgs e)
        {
            cubeTexId = 0x10101000;
        }
    }
}
