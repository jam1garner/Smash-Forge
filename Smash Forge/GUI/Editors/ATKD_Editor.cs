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
    public partial class ATKD_Editor : DockContent
    {
        public ATKD_Editor(ATKD atkd) 
        {
            InitializeComponent();
            this.atkd = atkd;
            tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("ID/Name") { ReadOnly = true });
            tbl.Columns.Add("Unknown");
            tbl.Columns.Add("Start Frame");
            tbl.Columns.Add("End Frame");
            tbl.Columns.Add("X Min");
            tbl.Columns.Add("X Max");
            tbl.Columns.Add("Y Min");
            tbl.Columns.Add("Y Max");
            dataGridView1.DataSource = tbl;
        }

        private DataTable tbl;
        ATKD atkd;

        private void ATKD_Editor_Load(object sender, EventArgs e)
        {
            unknown1.Value = atkd.unknown1;
            unknown2.Value = atkd.unknown2;
            tbl.Clear();
            foreach(ATKD.Entry entry in atkd.entries)
            {
                DataRow row = tbl.NewRow();
                row.ItemArray = new object[] { entry.attackId, entry.unk, entry.start, entry.end, entry.xmin, entry.xmax, entry.ymin, entry.ymax };
                tbl.Rows.Add(row);
            }
        }
    }
}
