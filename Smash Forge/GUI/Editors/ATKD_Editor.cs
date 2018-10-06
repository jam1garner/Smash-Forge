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
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge
{
    public partial class ATKD_Editor : EditorBase
    {
        public ATKD_Editor(string atkdPath) 
        {
            InitializeComponent();
            filePath = atkdPath;
            atkd = new ATKD().Read(filePath);
            InitDataGridTable();
        }
        public ATKD_Editor(string atkdPath, ModelViewport mvp)
        {
            InitializeComponent();
            filePath = atkdPath;
            atkd = new ATKD().Read(filePath);
            InitDataGridTable();
            this.mvp = mvp;
        }
        private void InitDataGridTable()
        {
            tbl = new DataTable();
            //tbl.Columns.Add(new DataColumn("ID/Name") { ReadOnly = true });
            tbl.Columns.Add("ID");
            tbl.Columns.Add("Start Frame");
            tbl.Columns.Add("Last Frame");
            tbl.Columns.Add("X Min");
            tbl.Columns.Add("X Max");
            tbl.Columns.Add("Y Min");
            tbl.Columns.Add("Y Max");
            dataGridView.DataSource = tbl;
        }

        DataTable tbl;
        string filePath;
        ATKD atkd;
        //when the ATKD Editor is loaded with 'Open Character' it links the editor and viewport together
        //this allows rendering dynamically every time an edit occurs
        //and lets us use viewport resources such as frame and current subaction
        ModelViewport mvp;

        private void ATKD_Editor_Load(object sender, EventArgs e)
        {
            CmnSubactions_UpDownBox.Value = atkd.commonSubactions;
            UnqSubactions_UpDownBox.Value = atkd.uniqueSubactions;
            tbl.Clear();
            foreach(ATKD.Entry entry in atkd.entries)
            {
                DataRow row = tbl.NewRow();
                row.ItemArray = new object[] { entry.subaction, entry.startFrame, entry.lastFrame, entry.xmin, entry.xmax, entry.ymin, entry.ymax };
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
                        atkd.entries[row].subaction = shValue;
                        break;
                    case 1:
                        atkd.entries[row].startFrame = shValue;
                        break;
                    case 2:
                        atkd.entries[row].lastFrame = shValue;
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
                        tbl.Rows[row][column] = atkd.entries[row].subaction = (ushort)flValue;
                        break;
                    case 1:
                        tbl.Rows[row][column] = atkd.entries[row].startFrame = (ushort)flValue;
                        break;
                    case 2:
                        tbl.Rows[row][column] = atkd.entries[row].lastFrame = (ushort)flValue;
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

        public void Viewport_Render(VBN Skeleton)
        {
            if (Skeleton == null || mvp.CurrentAnimation == null || mvp.acmdScript == null)
                return;
            
            Vector3 position = Skeleton.getBone("TransN").pos;
            int subactionID = mvp.scriptId;
            ATKD.Entry entry = atkd.entries.Find(e => e.subaction == subactionID);
            float frame = mvp.acmdScript.animationFrame;

            if (entry == null)
                return;

            if (frame < entry.startFrame || frame > entry.lastFrame) return;
            
            GL.Color4(Color.MediumVioletRed);
            GL.LineWidth(2);
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(0, position.Y + entry.ymin, position.Z + entry.xmin);
            GL.Vertex3(0, position.Y + entry.ymin, position.Z + entry.xmax);
            GL.Vertex3(0, position.Y + entry.ymax, position.Z + entry.xmax);
            GL.Vertex3(0, position.Y + entry.ymax, position.Z + entry.xmin);
            GL.End();
        }

        public void ViewportEvent_SetXY(ushort subaction, float xmin, float xmax, float ymin, float ymax)
        {
            int entryIndex;
            if ((entryIndex = atkd.entries.FindIndex(i => i.subaction == subaction)) >= 0)
            {
                tbl.Rows[entryIndex][3] = atkd.entries[entryIndex].xmin = xmin;
                tbl.Rows[entryIndex][4] = atkd.entries[entryIndex].xmax = xmax;
                tbl.Rows[entryIndex][5] = atkd.entries[entryIndex].ymin = ymin;
                tbl.Rows[entryIndex][6] = atkd.entries[entryIndex].ymax = ymax;
            }
        }
    }
}
