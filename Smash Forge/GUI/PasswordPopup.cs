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
    public partial class PasswordPopup : Form
    {
        public PasswordPopup()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PasswordPopup_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainForm.Instance.password = textBox1.Text;
        }
    }
}
