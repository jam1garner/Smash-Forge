using System.Windows.Forms;
using SALT.Graphics;

namespace SmashForge.Gui.Menus
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
