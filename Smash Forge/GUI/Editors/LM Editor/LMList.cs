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
        private TreeView treeView1;
        ContextMenu SymbolCM;
        public LMList()
        {
            InitializeComponent();
            treeView1.Nodes.Add(symbolNode);

            {
                symbolNode.ContextMenu = new ContextMenu();
            }
        }
        public TreeNode symbolNode = new TreeNode("Symbols");
        public void fillList()
        {
            symbolNode.Nodes.Clear();

            if (Lumen != null)
            {
                foreach (string s in Lumen.Strings)
                {
                    TreeNode newNode = new TreeNode(s) { Tag = s, ContextMenu = SymbolCM };
                    symbolNode.Nodes.Add(newNode);
                }
            }
        }

        private void InitializeComponent()
        {
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(285, 262);
            this.treeView1.TabIndex = 0;
            // 
            // LMList
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.treeView1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "LMList";
            this.ResumeLayout(false);

        }
    }


    
}
