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
    public partial class HitboxList : DockContent
    {
        private DataTable hitboxData;

        public HitboxList()
        {
            InitializeComponent();
        }

        public void refresh()
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            if (Runtime.gameAcmdScript != null)
            {
                foreach (Hitbox h in Runtime.gameAcmdScript.Hitboxes.Values)
                {
                    TreeNode node = new TreeNode($"Hitbox {h.ID}") { Tag = h.ID, Checked = true };

                    treeView1.Nodes.Add(node);
                }
            }
            treeView1.EndUpdate();
            hitboxData = new DataTable();
            dataGridView1.DataSource = hitboxData;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode.Index > -1)
            {
                hitboxData = new DataTable();
                dataGridView1.DataSource = hitboxData;

                Hitbox hitbox = Runtime.gameAcmdScript.Hitboxes[(int)treeView1.SelectedNode.Tag];

                hitboxData.Columns.Add(new DataColumn("Name") { ReadOnly = true });
                hitboxData.Columns.Add("Value");
                hitboxData.Rows.Add("Type", hitbox.GetHitboxType());
                hitboxData.Rows.Add("ID", hitbox.ID);
                if (hitbox.Type == Hitbox.HITBOX || hitbox.Type == Hitbox.WINDBOX)
                    hitboxData.Rows.Add("Part", hitbox.Part);
                hitboxData.Rows.Add("Bone", hitbox.Bone);
                hitboxData.Rows.Add("Damage", hitbox.Damage);
                hitboxData.Rows.Add("BKB", hitbox.KnockbackBase);
                hitboxData.Rows.Add("WBKB", hitbox.WeightBasedKnockback);
                hitboxData.Rows.Add("KBG", hitbox.KnockbackGrowth);
                
                hitboxData.Rows.Add("Size", hitbox.Size);
                hitboxData.Rows.Add("X Pos", hitbox.X);
                hitboxData.Rows.Add("Y Pos", hitbox.Y);
                hitboxData.Rows.Add("Z Pos", hitbox.Z);
                if (hitbox.Extended)
                {
                    hitboxData.Rows.Add("X Stretch", hitbox.X2);
                    hitboxData.Rows.Add("Y Stretch", hitbox.Y2);
                    hitboxData.Rows.Add("Z Stretch", hitbox.Z2);
                }

                Runtime.SelectedHitboxID = hitbox.ID;
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Index > -1)
            {
                if (!e.Node.Checked)
                    Runtime.HiddenHitboxes.Add((int)e.Node.Tag);
                else
                    Runtime.HiddenHitboxes.Remove((int)e.Node.Tag);
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (treeView1.SelectedNode.Index > -1)
            {
                

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Runtime.SelectedHitboxID = -1;
        }
    }
}
