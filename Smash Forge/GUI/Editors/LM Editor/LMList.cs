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
        
        public LMList(string fileName = null)
        {
            InitializeComponent();
            if (fileName != null)
            {
                Lumen = new Lumen(fileName);
            }
            treeView1.Nodes.Add(symbolNode);
            treeView1.Nodes.Add(colorNode);

            fillList();

            treeView1.NodeMouseClick += (sender, args) => treeView1.SelectedNode = args.Node;
        }
        public TreeNode symbolNode = new TreeNode("Symbols");
        public TreeNode colorNode = new TreeNode("Colors");
        public TreeNode transformNode = new TreeNode("Transforms");

        public void fillList()
        {
            symbolNode.Nodes.Clear();
            colorNode.Nodes.Clear();
            if(Lumen != null)
            {
                foreach (string s in Lumen.Strings)
                {
                    symbolNode.Nodes.Add(new TreeNode(s) { Text = s });
                }
                foreach (var x in Lumen.Colors)
                {
                    colorNode.Nodes.Add(new TreeNode((x * 255).ToString()));
                }
            }
        }
        public DataTable tbl = new DataTable();
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("Name") { ReadOnly = true });
            tbl.Columns.Add("Value");
            dataGridView1.DataSource = tbl;
            tbl.Rows.Clear();
            try
            {
                if (e.Node.Parent.Text == "Colors")
                {
                    tbl.Rows.Add("Red", Lumen.Colors[e.Node.Index].X * 255);
                    tbl.Rows.Add("Green", Lumen.Colors[e.Node.Index].Y * 255);
                    tbl.Rows.Add("Blue", Lumen.Colors[e.Node.Index].Z * 255);
                    tbl.Rows.Add("Alpha", Lumen.Colors[e.Node.Index].W * 255);
                }
            }
            catch{}
        }
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int indexNum = treeView1.SelectedNode.Index;
            Lumen.AddColor(new OpenTK.Vector4(float.Parse(tbl.Rows[0][1].ToString()) / 255f, float.Parse(tbl.Rows[1][1].ToString()) / 255f, float.Parse(tbl.Rows[2][1].ToString()) / 255f, float.Parse(tbl.Rows[3][1].ToString()) / 255f), indexNum);
            fillList();
        }
    }
}
