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
using SALT.Moveset.AnimCMD;
using System.Text.RegularExpressions;

namespace Smash_Forge.GUI.Editors
{
    public partial class HurtboxList : DockContent
    {
        private DataTable hurtboxData = new DataTable();

        public HurtboxList()
        {
            InitializeComponent();
            refresh();
        }

        public void refresh()
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
                hurtboxData.Rows.Add("Zone", hurtbox.Zone == Hurtbox.LW_ZONE ? "Low" : hurtbox.Zone == Hurtbox.HI_ZONE ? "High" : "Mid");

                hurtbox.Selected = true;
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
                float X = hurtbox.X, Y = hurtbox.Y, Z = hurtbox.Z, X2 = hurtbox.X2, Y2 = hurtbox.Y2, Z2 = hurtbox.Z2, Size = hurtbox.Size;
                string Zone = hurtbox.Zone == Hurtbox.LW_ZONE ? "Low" : hurtbox.Zone == Hurtbox.HI_ZONE ? "High" : "Mid";

                int.TryParse(hurtboxData.Rows[0][1].ToString(), out bone);

                float.TryParse(hurtboxData.Rows[1][1].ToString(), out Size);
                float.TryParse(hurtboxData.Rows[2][1].ToString(), out X);
                float.TryParse(hurtboxData.Rows[3][1].ToString(), out Y);
                float.TryParse(hurtboxData.Rows[4][1].ToString(), out Z);
                float.TryParse(hurtboxData.Rows[5][1].ToString(), out X2);
                float.TryParse(hurtboxData.Rows[6][1].ToString(), out Y2);
                float.TryParse(hurtboxData.Rows[7][1].ToString(), out Z2);

                Zone = hurtboxData.Rows[8][1].ToString();

                hurtbox.Bone = bone;
                hurtbox.Size = Size;
                hurtbox.X = X;
                hurtbox.Y = Y;
                hurtbox.Z = Z;
                hurtbox.X2 = X2;
                hurtbox.Y2 = Y2;
                hurtbox.Z2 = Z2;

                if (Zone == "Low")
                    hurtbox.Zone = Hurtbox.LW_ZONE;
                else if (Zone == "Mid")
                    hurtbox.Zone = Hurtbox.N_ZONE;
                else if (Zone == "High")
                    hurtbox.Zone = Hurtbox.HI_ZONE;

                if (hurtbox.X == hurtbox.X2 && hurtbox.Y == hurtbox.Y2 && hurtbox.Z == hurtbox.Z2)
                    hurtbox.isSphere = true;

                Runtime.ParamManager.SaveHurtboxes();

            }
        }
    }
}
