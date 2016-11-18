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
    public partial class MTAEditor : DockContent
    {
        public MTAEditor()
        {
            InitializeComponent();
            tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("Name") { ReadOnly = true });
            tbl.Columns.Add("Value");
            dataGridView1.DataSource = tbl;
        }

        public MTAEditor(MTA m)
        {
            InitializeComponent();
            tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("Name") { ReadOnly = true });
            tbl.Columns.Add("Value");
            dataGridView1.DataSource = tbl;
            loadMTA(m);
        }

        private DataTable tbl;
        private MTA mta;
        private TreeNode headerNode = new TreeNode("Header");
        private TreeNode MatNode = new TreeNode("Material Animation");
        private TreeNode VisNode = new TreeNode("Visibility Animation");

        private void row(string name, object val)
        {
            DataRow tempRow = tbl.NewRow();
            tempRow[0] = name;
            tempRow[1] = val;
            tbl.Rows.Add(tempRow);
        }

        public void tableRefresh()
        {
            tbl.Clear();
            if (treeView1.SelectedNode != null)
            {
                if(treeView1.SelectedNode == headerNode)
                {
                    row("Framerate",mta.frameRate);
                    row("Frame Count", mta.numFrames);
                    row("MTA Type", mta.unknown);
                }
                else if (treeView1.SelectedNode.Parent != null)
                {
                    object tag = treeView1.SelectedNode.Tag;
                    if(tag is MatEntry)
                    {
                        MatEntry temp = (MatEntry)tag;
                        row("Name", temp.name);
                        row("Material Hash", temp.matHash.ToString("X"));
                        row("Second Material Hash", temp.matHash.ToString("X"));
                        row("Has PAT0", Convert.ToInt32(temp.hasPat));
                    }
                    else if(tag is MatData)
                    {
                        MatData temp = (MatData)tag;
                        row("Name", temp.name);
                        foreach (MatData.frame f in temp.frames)
                        {

                        }
                    }
                    else if(tag is PatData)
                    {

                    }
                    else if(tag is VisEntry)
                    {

                    }
                }
            }
        }

        public void loadMTA(MTA m)
        {
            mta = m;
            int i = 0;
            foreach(MatEntry mat in m.matEntries)
            {
                i++;
                TreeNode temp = new TreeNode($"Material {i}") { Tag = mat };
                int j = 0;
                foreach(MatData md in mat.properties)
                {
                    j++;
                    temp.Nodes.Add(new TreeNode($"Property {j}") { Tag = md });
                }

                if (mat.hasPat && mat.pat0 != null)
                    temp.Nodes.Add(new TreeNode("Texture Animation") { Tag = mat.pat0 });

                MatNode.Nodes.Add(temp);
            }

            foreach (VisEntry vis in m.visEntries)
            {
                VisNode.Nodes.Add(new TreeNode($"Visibility {i}") { Tag = vis });
            }
        }

        private void MTAEditor_Load(object sender, EventArgs e)
        {
            treeView1.Nodes.Add(headerNode);
            treeView1.Nodes.Add(MatNode);
            treeView1.Nodes.Add(VisNode);
        }

        private void tableRefresh(object sender, TreeViewEventArgs e)
        {
            tableRefresh();
        }
    }
}
