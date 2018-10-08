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
            tbl.Columns.Add(new DataColumn("ID") { ReadOnly = true });
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
        Color renderColor = Color.MediumVioletRed;
        public bool isRendered = false;
        public int selectedPart = 0;

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
            dataGridView.AutoResizeColumns();
        }

        private void ATKD_Editor_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (mvp != null)
            {
                mvp.atkdEditor = null;
                Runtime.currentATKD = null;
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

        private ATKD.Entry FindEntry(int id)
        {
            if (id < 0) return null;
            return atkd.entries.Find(e => e.subaction == id);
        }
        private DataRow FindDataRow(int id)
        {
            if (id < 0) return null;
            foreach (DataRow r in tbl.Rows)
            {
                if (r[0] != null && ushort.Parse((string)r[0]) == id)
                    return r;
            }
            return null;
        }
        private DataGridViewRow FindDataGridViewRow(int id)
        {
            if (id < 0) return null;
            foreach (DataGridViewRow r in dataGridView.Rows)
            {
                if (r.Cells[0].Value != null && ushort.Parse((string)r.Cells[0].Value) == id)
                    return r;
            }
            return null;
        }

        private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int row = e.RowIndex;
            int column = e.ColumnIndex;
            ATKD.Entry entry = FindEntry((int)tbl.Rows[row][0]);
            ushort shValue;
            float flValue;
            
            if (ushort.TryParse((string)tbl.Rows[row][column], out shValue))
            {
                switch (column)
                {
                    case 0:
                        entry.subaction = shValue;
                        break;
                    case 1:
                        entry.startFrame = shValue;
                        break;
                    case 2:
                        entry.lastFrame = shValue;
                        break;
                    case 3:
                        entry.xmin = shValue;
                        break;
                    case 4:
                        entry.xmax = shValue;
                        break;
                    case 5:
                        entry.ymin = shValue;
                        break;
                    case 6:
                        entry.ymax = shValue;
                        break;
                }
            }
            else
            {
                flValue = float.Parse((string)tbl.Rows[row][column]);
                if (column < 3 && flValue < 0)
                    flValue = 0;
                
                switch (column)
                {
                    case 0:
                        tbl.Rows[row][column] = entry.subaction = (ushort)flValue;
                        break;
                    case 1:
                        tbl.Rows[row][column] = entry.startFrame = (ushort)flValue;
                        break;
                    case 2:
                        tbl.Rows[row][column] = entry.lastFrame = (ushort)flValue;
                        break;
                    case 3:
                        entry.xmin = flValue;
                        break;
                    case 4:
                        entry.xmax = flValue;
                        break;
                    case 5:
                        entry.ymin = flValue;
                        break;
                    case 6:
                        entry.ymax = flValue;
                        break;
                }
            }
        }

        public void Viewport_Render(VBN Skeleton)
        {
            if (Skeleton == null || mvp.CurrentAnimation == null || mvp.acmdScript == null)
                return;

            ATKD.Entry entry = FindEntry(mvp.scriptId);
            float frame = mvp.acmdScript.animationFrame;

            if (entry == null)
                return;
            if (frame < entry.startFrame || frame > entry.lastFrame)
                return;

            GL.Color4(renderColor);
            GL.LineWidth(2);
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(0, entry.ymin, entry.xmin);
            GL.Vertex3(0, entry.ymax, entry.xmin);
            GL.Vertex3(0, entry.ymax, entry.xmax);
            GL.Vertex3(0, entry.ymin, entry.xmax);
            GL.End();
            isRendered = true;
        }

        public void ViewportEvent_SetSelection(float projX, float projY)
        {
            RectangleSelectionPart selection = RectangleSelectionPart.None;
            ATKD.Entry entry = FindEntry(mvp.scriptId);
            if (entry != null)
            {
                float xmax = entry.xmax;
                float xmin = entry.xmin;
                float ymax = entry.ymax;
                float ymin = entry.ymin;
                float delta = 0.5f;

                if (projX < xmax + delta && projX > xmin - delta && projY < ymax + delta && projY > ymin - delta)
                {
                    if (projX > xmax - delta && projX < xmax + delta)
                        selection |= RectangleSelectionPart.Right;
                    if (projX > xmin - delta && projX < xmin + delta)
                        selection |= RectangleSelectionPart.Left;
                    if (projY > ymax - delta && projY < ymax + delta)
                        selection |= RectangleSelectionPart.Top;
                    if (projY > ymin - delta && projY < ymin + delta)
                        selection |= RectangleSelectionPart.Bottom;
                }
            }
            selectedPart = (int)selection;
            if (selectedPart > 0)
                renderColor = Color.PaleVioletRed;
            else
                renderColor = Color.MediumVioletRed;
        }
        public void ViewportEvent_SetSelectedXY(float projX, float projY)
        {
            if (selectedPart == 0) return;
            ATKD.Entry entry = FindEntry(mvp.scriptId);
            DataRow row = FindDataRow(mvp.scriptId);
            if (entry != null && row != null)
            {
                float delta = 0.5f;
                
                if ((selectedPart & (int)RectangleSelectionPart.Right) > 0)
                {
                    float min = entry.xmin + delta;
                    if (projX > min)
                        row[4] = entry.xmax = projX;
                    else
                        row[4] = entry.xmax = min;
                }
                else if ((selectedPart & (int)RectangleSelectionPart.Left) > 0)
                {
                    float max = entry.xmax - delta;
                    if (projX < max)
                        row[3] = entry.xmin = projX;
                    else
                        row[3] = entry.xmin = max;
                }

                if ((selectedPart & (int)RectangleSelectionPart.Top) > 0)
                {
                    float min = entry.ymin + delta;
                    if (projY > min)
                        row[4] = entry.ymax = projY;
                    else
                        row[4] = entry.ymax = min;
                }
                else if ((selectedPart & (int)RectangleSelectionPart.Bottom) > 0)
                {
                    float max = entry.ymax - delta;
                    if (projY < max)
                        row[3] = entry.ymin = projY;
                    else
                        row[3] = entry.ymin = max;
                }
            }
        }
        public void ViewportEvent_SetSelectedSubaction()
        {
            DataGridViewRow row = FindDataGridViewRow(mvp.scriptId);
            if (row != null)
                dataGridView.CurrentCell = row.Cells[0];
        }

        public enum RectangleSelectionPart
        {
            None = 0x0,
            Right = 0x1,
            Left = 0x2,
            Top = 0x4,
            Bottom = 0x8
        }
    }
}
