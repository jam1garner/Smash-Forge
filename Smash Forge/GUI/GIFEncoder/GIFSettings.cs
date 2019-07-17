using System;
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
        public bool StoreFrames { get; set; } = true;
        public bool ClearFrames { get; set; } = false;

        public GIFSettings(int MaxFrame, float ScaleFactor, bool HasStoredFrames)
        {
            InitializeComponent();

            this.ScaleFactor = ScaleFactor;
            nupdScaleFactor.Value = (decimal)ScaleFactor;

            if (HasStoredFrames)
            {
                nupdScaleFactor.Enabled = false;
                btClearFrames.Visible = true;
            }

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
            StoreFrames = cbStoreFrames.Checked;            

            this.Close();

        }

        private void cbStoreFrames_CheckedChanged(object sender, EventArgs e)
        {
            cbRepeat.Enabled = !cbStoreFrames.Checked;
            tbQuality.Enabled = !cbStoreFrames.Checked;
        }

        private void btClearFrames_Click(object sender, EventArgs e)
        {
            ClearFrames = true;
            btClearFrames.Visible = false;
            nupdScaleFactor.Enabled = true;
        }
    }
}
