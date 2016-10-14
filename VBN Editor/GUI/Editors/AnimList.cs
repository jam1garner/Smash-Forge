using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace VBN_Editor
{
    public partial class AnimListPanel : DockContent
    {
        public AnimListPanel()
        {
            InitializeComponent();
        }

        private void lstAnims_SelectedIndexChanged(object sender, EventArgs e)
        {
            Runtime.TargetAnim = Runtime.Animations[lstAnims.SelectedItem.ToString()];
        }
    }
}
