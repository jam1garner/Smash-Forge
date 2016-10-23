using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VBN_Editor.GUI
{
    public partial class RenderSettings : Form
    {
        public RenderSettings()
        {
            InitializeComponent();
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            //Disable all the checkboxes for LVD
            checkChanged();
            checkBox7.Enabled = checkBox6.Checked;
            checkBox8.Enabled = checkBox6.Checked;
            checkBox9.Enabled = checkBox6.Checked;
            checkBox10.Enabled = checkBox6.Checked;
            checkBox11.Enabled = checkBox6.Checked;
        }

        private void checkChanged()
        {
            Runtime.renderModel = checkBox1.Checked;
            Runtime.renderBones = checkBox2.Checked;
            Runtime.renderHitboxes = checkBox3.Checked;
            Runtime.renderPath = checkBox4.Checked;
            Runtime.renderFloor = checkBox5.Checked;
            Runtime.renderLVD = checkBox6.Checked;
            Runtime.renderCollisions = checkBox7.Checked;
            Runtime.renderSpawns = checkBox8.Checked;
            Runtime.renderRespawns = checkBox9.Checked;
            Runtime.renderItemSpawners = checkBox10.Checked;
            Runtime.renderGeneralPoints = checkBox11.Checked;
        }

        private void checkChanged(object sender, EventArgs e)
        {
            checkChanged();
        }
    }
}
