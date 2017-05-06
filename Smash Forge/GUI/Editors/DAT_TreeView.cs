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

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if(e.Node.Tag is DAT.JOBJ)
            {
                DAT.JOBJ jobj = (DAT.JOBJ)e.Node.Tag;
                MainForm.Instance.lvdEditor.open(jobj, new TreeNode());
            }
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            if(treeView1.SelectedNode != null && treeView1.SelectedNode.Text == "coll_data")
            {
                Console.WriteLine("coll_data");
                //Open DAT collision editor
            }
        }
    }
}
