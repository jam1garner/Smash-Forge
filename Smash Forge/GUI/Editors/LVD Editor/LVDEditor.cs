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
    public partial class LVDEditor : DockContent
    {
        public LVDEditor()
        {
            InitializeComponent();
        }

        private LVDEntry currentEntry;

        public void open(LVDEntry entry)
        {
            currentEntry = entry;
            if(entry is Collision)
            {

            }
        }
    }
}
