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
    public partial class DAT_TreeView : DockContent
    {
        public DAT_TreeView()
        {
            InitializeComponent();
            refresh();
        }

        private DAT dat;

        public void setDAT(DAT d)
        {
            dat = d;
            refresh();
        }

        public void refresh()
        {
            treeView1.Nodes.Clear();

            if (dat != null)
            {
                treeView1.Nodes.AddRange(dat.tree.ToArray());
            }
        }
    }
}
