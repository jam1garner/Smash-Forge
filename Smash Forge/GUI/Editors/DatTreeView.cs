using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace SmashForge
{
    public partial class DatTreeView : DockContent
    {
        public DatTreeView()
        {
            InitializeComponent();
            Refresh();
        }

        private DAT dat;

        public void SetDat(DAT d)
        {
            dat = d;
            Refresh();
        }

        public void Refresh()
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
                MainForm.Instance.lvdEditor.Open(jobj, new TreeNode());
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
