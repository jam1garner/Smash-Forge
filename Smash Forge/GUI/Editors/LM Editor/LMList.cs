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
    public partial class LMList : DockContent
    {
        public Lumen Lumen;
        
        public LMList()
        {
            InitializeComponent();
            treeView1.Nodes.Add("Testing");
            symbolNode.Nodes.Add("");

            treeView1.Nodes.Add(symbolNode);

            treeView1.NodeMouseClick += (sender, args) => treeView1.SelectedNode = args.Node;
        }
        public TreeNode symbolNode = new TreeNode("Symbols");
        public void fillList()
        {
            symbolNode.Nodes.Clear();
            if(Lumen != null)
            {
                foreach (string s in Lumen.Strings)
                {
                    symbolNode.Nodes.Add(new TreeNode(s) { Text = s });
                }
            }
        }
    }
}
