using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gif.Components
{
    public partial class GIFSettings : Form
    {
        public bool OK { get; set; } = false;
        public int StartFrame { get; set; } = 1;
        public int EndFrame { get; set; } = 1;
        public float ScaleFactor { get; set; } = 1;
        public int Quality { get; set; } = 1;
        public bool Repeat { get; set; } = false;

        public GIFSettings(int MaxFrame)
        {
            InitializeComponent();

            nupdMaxFrame.Maximum = MaxFrame;
            nupdMaxFrame.Value = MaxFrame;

            btStart.Select();

        }

        private void start_Click(object sender, EventArgs e)
        {
            OK = true;

            StartFrame = (int)nupdInitialFrame.Value;
            EndFrame = (int)nupdMaxFrame.Value;
            ScaleFactor = (float)nupdScaleFactor.Value;
            Quality = tbQuality.Value;
            Repeat = cbRepeat.Checked;

            this.Close();

        }
    }
}
