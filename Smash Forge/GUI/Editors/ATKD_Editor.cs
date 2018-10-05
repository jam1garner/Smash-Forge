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
    public partial class ATKD_Editor : EditorBase
    {
        public ATKD_Editor(string atkdPath) 
        {
            InitializeComponent();
            filePath = atkdPath;
            atkd = new ATKD().Read(filePath);
            tbl = new DataTable();
            //tbl.Columns.Add(new DataColumn("ID/Name") { ReadOnly = true });
            tbl.Columns.Add("ID");
            tbl.Columns.Add("Start Frame");
            tbl.Columns.Add("End Frame");
            tbl.Columns.Add("X Min");
            tbl.Columns.Add("X Max");
            tbl.Columns.Add("Y Min");
            tbl.Columns.Add("Y Max");
            dataGridView.DataSource = tbl;
        }

        private DataTable tbl;
        string filePath;
        ATKD atkd;

        private void ATKD_Editor_Load(object sender, EventArgs e)
        {
            CmnSubactions_UpDownBox.Value = atkd.commonSubactions;
            UnqSubactions_UpDownBox.Value = atkd.uniqueSubactions;
            tbl.Clear();
            foreach(ATKD.Entry entry in atkd.entries)
            {
                DataRow row = tbl.NewRow();
                row.ItemArray = new object[] { entry.attackId, entry.start, entry.end, entry.xmin, entry.xmax, entry.ymin, entry.ymax };
                tbl.Rows.Add(row);
            }
        }

        public override void Save()
        {
            atkd.Save(filePath);
        }

        public override void SaveAs()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "AI attack data|attack_data.bin";

                if (sfd.ShowDialog() == DialogResult.OK)
                    atkd.Save(sfd.FileName);
            }
        }

        private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int row = e.RowIndex;
            int column = e.ColumnIndex;
            ushort shValue;
            float flValue;
            
            if (ushort.TryParse((string)tbl.Rows[row].ItemArray[column], out shValue))
            {
                switch (column)
                {
                    case 0:
                        atkd.entries[row].attackId = shValue;
                        break;
                    case 1:
                        atkd.entries[row].start = shValue;
                        break;
                    case 2:
                        atkd.entries[row].end = shValue;
                        break;
                    case 3:
                        atkd.entries[row].xmin = shValue;
                        break;
                    case 4:
                        atkd.entries[row].xmin = shValue;
                        break;
                    case 5:
                        atkd.entries[row].ymin = shValue;
                        break;
                    case 6:
                        atkd.entries[row].ymax = shValue;
                        break;
                }
            }
            else
            {
                flValue = float.Parse((string)tbl.Rows[row].ItemArray[column]);
                if (column < 3 && flValue < 0)
                    flValue = 0;
                
                switch (column)
                {
                    case 0:
                        atkd.entries[row].attackId = (ushort)flValue;
                        tbl.Rows[row][column] = (ushort)flValue;
                        break;
                    case 1:
                        atkd.entries[row].start = (ushort)flValue;
                        tbl.Rows[row][column] = (ushort)flValue;
                        break;
                    case 2:
                        atkd.entries[row].end = (ushort)flValue;
                        tbl.Rows[row][column] = (ushort)flValue;
                        break;
                    case 3:
                        atkd.entries[row].xmin = flValue;
                        break;
                    case 4:
                        atkd.entries[row].xmin = flValue;
                        break;
                    case 5:
                        atkd.entries[row].ymin = flValue;
                        break;
                    case 6:
                        atkd.entries[row].ymax = flValue;
                        break;
                }
            }
        }
    }
}
