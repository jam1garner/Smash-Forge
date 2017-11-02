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



namespace Smash_Forge.GUI.Menus
{
    public partial class MakeMetal : Form
    {
        private NUD nud;

        private int difTexID = 0x40000001;
        private bool preserveNormalMap = true;
        private bool preserveDiffuse = false;

        private float[] minGain = new float[] { 0.3f, 0.3f, 0.3f, 1 };
        private float[] refColor = new float[] { 3f, 3f, 3f, 1 };
        private float[] fresParams = new float[] { 3.7f, 0f, 0f, 1 };
        private float[] fresColor = new float[] { 0.6f, 0.6f, 0.6f, 1 };

        public MakeMetal(NUD nud)
        {
            InitializeComponent();
            this.nud = nud;
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            nud.MakeMetal(difTexID, preserveDiffuse, preserveNormalMap, minGain, refColor, fresParams, fresColor);
        }

        private void useDifTexCB_CheckedChanged(object sender, EventArgs e)
        {
            preserveDiffuse = !useDifTexCB.Checked;
            difIDTextBox.Enabled = useDifTexCB.Checked;
            label1.Enabled = useDifTexCB.Checked;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            preserveNormalMap = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            preserveNormalMap = false;
        }

        private void difIDTextBox_TextChanged(object sender, EventArgs e)
        {
            int f = 0;
            if (int.TryParse(difIDTextBox.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out f))
            {
                difTexID = f;
                difIDTextBox.BackColor = Color.White;
            }
            else
                difIDTextBox.BackColor = Color.Red;
        }

        private void Property1X_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property1X.Text, out i))
            {
                Property1X.BackColor = Color.White;
                refColor[0] = i;
            }
            else
                Property1X.BackColor = Color.Red;
        }

        private void Property1Y_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property1Y.Text, out i))
            {
                Property1Y.BackColor = Color.White;
                refColor[1] = i;
            }
            else
                Property1Y.BackColor = Color.Red;
        }

        private void Property1Z_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property1Z.Text, out i))
            {
                Property1Z.BackColor = Color.White;
                refColor[2] = i;
            }
            else
                Property1Z.BackColor = Color.Red;
        }

        private void Property1W_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property1W.Text, out i))
            {
                Property1W.BackColor = Color.White;
                refColor[3] = i;
            }
            else
                Property1W.BackColor = Color.Red;
        }

        private void Property2X_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property2X.Text, out i))
            {
                Property2X.BackColor = Color.White;
                fresColor[0] = i;
            }
            else
                Property2X.BackColor = Color.Red;
        }

        private void Property2Y_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property2Y.Text, out i))
            {
                Property2Y.BackColor = Color.White;
                fresColor[1] = i;
            }
            else
                Property2Y.BackColor = Color.Red;
        }

        private void Property2Z_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property2Z.Text, out i))
            {
                Property2Z.BackColor = Color.White;
                fresColor[2] = i;
            }
            else
                Property2Z.BackColor = Color.Red;
        }

        private void Property2W_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property2W.Text, out i))
            {
                Property2W.BackColor = Color.White;
                fresColor[3] = i;
            }
            else
                Property2W.BackColor = Color.Red;
        }

        private void Property3X_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property3X.Text, out i))
            {
                Property3X.BackColor = Color.White;
                fresParams[0] = i;
            }
            else
                Property3X.BackColor = Color.Red;
        }

        private void Property3Y_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property3Y.Text, out i))
            {
                Property3Y.BackColor = Color.White;
                fresParams[1] = i;
            }
            else
                Property3Y.BackColor = Color.Red;
        }

        private void Property3Z_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property3Z.Text, out i))
            {
                Property3Z.BackColor = Color.White;
                fresParams[2] = i;
            }
            else
                Property3Z.BackColor = Color.Red;
        }

        private void Property3W_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property3W.Text, out i))
            {
                Property3W.BackColor = Color.White;
                fresParams[3] = i;
            }
            else
                Property3W.BackColor = Color.Red;
        }

        private void textBox14_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property4X.Text, out i))
            {
                Property4X.BackColor = Color.White;
                minGain[0] = i;
            }
            else
                Property4X.BackColor = Color.Red;
        }

        private void Property4Y_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property4Y.Text, out i))
            {
                Property4Y.BackColor = Color.White;
                minGain[1] = i;
            }
            else
                Property4Y.BackColor = Color.Red;
        }

        private void Property4Z_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property4Z.Text, out i))
            {
                Property4Z.BackColor = Color.White;
                minGain[2] = i;
            }
            else
                Property4Z.BackColor = Color.Red;
        }

        private void Property4W_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(Property4W.Text, out i))
            {
                Property4W.BackColor = Color.White;
                minGain[3] = i;
            }
            else
                Property4W.BackColor = Color.Red;
        }
    }
}
