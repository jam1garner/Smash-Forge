using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge
{
    public partial class ProgessAlert : Form
    {
        public int ProgressValue
        {
            set {
                progressBar1.Value = value;
                if (value >= 100)
                    Close();
                progressBar1.Refresh();
            }
        }
        public string Message
        {
            set {
                ProgressLabel.Text = value;
                ProgressLabel.Refresh();
            }
        }

        public ProgessAlert()
        {
            InitializeComponent();
        }

        private void ProgessAlert_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainForm.Instance.backgroundWorker1.CancelAsync();
        }
    }
}
