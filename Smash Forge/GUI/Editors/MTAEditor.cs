using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Smash_Forge
{
    public partial class MTAEditor : DockContent
    {
        public MTAEditor(MTA Mta)
        {
            InitializeComponent();
            mta = Mta;
        }

        public MTA mta;

        private void MTAEditor_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = mta.Decompile();
        }
    }
}
