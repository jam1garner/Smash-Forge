using System.Windows.Forms;

namespace SmashForge
{
    public partial class ProgressAlert : Form
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

        public ProgressAlert()
        {
            InitializeComponent();
        }

        private void ProgessAlert_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainForm.Instance.backgroundWorker1.CancelAsync();
        }
    }
}
