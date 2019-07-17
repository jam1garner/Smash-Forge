using System;
using System.Data;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace SmashForge
{
    public partial class HurtboxList : DockContent
    {
        private DataTable hurtboxData = new DataTable();

        public HurtboxList()
        {
            InitializeComponent();
            Refresh();
        }

        public void Refresh()
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            if(Runtime.ParamManager.Hurtboxes.Count > 0)
            {
                int i = 0;
                foreach(Hurtbox h in Runtime.ParamManager.Hurtboxes.Values)
                {
                    TreeNode node = new TreeNode($"Hurtbox {i}") { Tag = $"{i}", Checked = true };

                    treeView1.Nodes.Add(node);

                    i++;
                }
            }
            treeView1.EndUpdate();
            hurtboxData = new DataTable();
            dataGridView1.DataSource = hurtboxData;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode.Index > -1) {
                Runtime.ParamManager.UnselectHurtboxes();
                hurtboxData = new DataTable();
                dataGridView1.DataSource = hurtboxData;

                Hurtbox hurtbox = Runtime.ParamManager.Hurtboxes[treeView1.SelectedNode.Index];

                hurtboxData.Columns.Add(new DataColumn("Name") { ReadOnly = true });
                hurtboxData.Columns.Add("Value");
                hurtboxData.Rows.Add("Bone", hurtbox.Bone);
                hurtboxData.Rows.Add("Size", hurtbox.Size);
                hurtboxData.Rows.Add("X Pos", hurtbox.X);
                hurtboxData.Rows.Add("Y Pos", hurtbox.Y);
                hurtboxData.Rows.Add("Z Pos", hurtbox.Z);
                hurtboxData.Rows.Add("X Stretch", hurtbox.X2);
                hurtboxData.Rows.Add("Y Stretch", hurtbox.Y2);
                hurtboxData.Rows.Add("Z Stretch", hurtbox.Z2);
                hurtboxData.Rows.Add("Zone", hurtbox.Zone == Hurtbox.LwZone ? "Low" : hurtbox.Zone == Hurtbox.HiZone ? "High" : "Mid");

                Runtime.SelectedHurtboxId = hurtbox.Id;
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Index > -1)
            {
                Runtime.ParamManager.Hurtboxes[e.Node.Index].Visible = e.Node.Checked;
                Runtime.ParamManager.UnselectHurtboxes();
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (treeView1.SelectedNode.Index > -1)
            {
                Hurtbox hurtbox = Runtime.ParamManager.Hurtboxes[treeView1.SelectedNode.Index];

                int bone = hurtbox.Bone;
                float x = hurtbox.X, y = hurtbox.Y, z = hurtbox.Z, x2 = hurtbox.X2, y2 = hurtbox.Y2, z2 = hurtbox.Z2, size = hurtbox.Size;
                string zone = hurtbox.Zone == Hurtbox.LwZone ? "Low" : hurtbox.Zone == Hurtbox.HiZone ? "High" : "Mid";

                int.TryParse(hurtboxData.Rows[0][1].ToString(), out bone);

                float.TryParse(hurtboxData.Rows[1][1].ToString(), out size);
                float.TryParse(hurtboxData.Rows[2][1].ToString(), out x);
                float.TryParse(hurtboxData.Rows[3][1].ToString(), out y);
                float.TryParse(hurtboxData.Rows[4][1].ToString(), out z);
                float.TryParse(hurtboxData.Rows[5][1].ToString(), out x2);
                float.TryParse(hurtboxData.Rows[6][1].ToString(), out y2);
                float.TryParse(hurtboxData.Rows[7][1].ToString(), out z2);

                zone = hurtboxData.Rows[8][1].ToString();

                hurtbox.Bone = bone;
                hurtbox.Size = size;
                hurtbox.X = x;
                hurtbox.Y = y;
                hurtbox.Z = z;
                hurtbox.X2 = x2;
                hurtbox.Y2 = y2;
                hurtbox.Z2 = z2;

                if (zone == "Low")
                    hurtbox.Zone = Hurtbox.LwZone;
                else if (zone == "Mid")
                    hurtbox.Zone = Hurtbox.NZone;
                else if (zone == "High")
                    hurtbox.Zone = Hurtbox.HiZone;

                if (hurtbox.X == hurtbox.X2 && hurtbox.Y == hurtbox.Y2 && hurtbox.Z == hurtbox.Z2)
                    hurtbox.IsSphere = true;

                Runtime.ParamManager.SaveHurtboxes();

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Runtime.ParamManager.UnselectHurtboxes();
        }
    }
}
