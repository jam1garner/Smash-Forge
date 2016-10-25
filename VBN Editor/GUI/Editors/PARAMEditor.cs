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
using SALT.PARAMS;

namespace VBN_Editor
{
    public partial class PARAMEditor : DockContent
    {
        public PARAMEditor(string filename)
        {
            InitializeComponent();
            tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("Name") { ReadOnly = true });
            tbl.Columns.Add("Value");
            dataGridView1.DataSource = tbl;
            openParam(filename);
        }

        private ParamFile p;
        private DataTable tbl;

        private void fillTable(int groupNum)
        {
            tbl.Clear();
            if (p.Groups.Count > groupNum)
            {
                int i = 0;
                foreach (ParamEntry val in p.Groups[groupNum].Values)
                {
                    DataRow tempRow = tbl.NewRow();
                    tempRow[0] = i;
                    tempRow[1] = val.Value;
                    tbl.Rows.Add(tempRow);
                    i++;
                }
            }
            
        }

        private void openParam(string f)
        {
            p = new ParamFile(f);
            fillTable(0);
        }
    }
}
