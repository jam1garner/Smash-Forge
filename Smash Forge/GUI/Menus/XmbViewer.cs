using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SALT.Graphics;

namespace Smash_Forge.GUI.Menus
{
    public partial class XmbViewer : Form
    {
        public XmbViewer(XMBFile xmb)
        {
            InitializeComponent();

            GetXmbDump(xmb);
        }

        private void GetXmbDump(XMBFile xmb)
        {
            foreach (XMBEntry entry in xmb.Entries)
            {
                textBox1.Text += entry.deserialize();
            }

            // Deselect the text that was added.
            textBox1.SelectionLength = 0;
            textBox1.Select(0, 0);
        }
    }
}
