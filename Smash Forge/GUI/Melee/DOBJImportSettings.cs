using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MeleeLib.DAT;
using MeleeLib.GCX;

namespace Smash_Forge.GUI.Melee
{
    public partial class DOBJImportSettings : Form
    {
        private MeleeDataObjectNode DOBJ;
        private string ModelSrc;

        public enum ExitStatus
        {
            Running = 0,
            Opened = 1,
            Cancelled = 2
        }

        public ExitStatus exitStatus = ExitStatus.Running;

        public DOBJImportSettings(MeleeDataObjectNode DOBJ)
        {
            InitializeComponent();
            this.DOBJ = DOBJ;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog d = new OpenFileDialog())
            {
                d.Filter += "Source Model|*.smd|" +
                             "All Files (*.*)|*.*";

                if(d.ShowDialog() == DialogResult.OK)
                {
                    ModelSrc = d.FileName;
                }
            }
        }

        private void DOBJImportSettings_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            SMD smd = new SMD(ModelSrc);

            DOBJ.ClearPolygons(null, null);
            DatPolygon p = new DatPolygon();
            p.Flags = 0x0001;
            if (comboBoxBoneType.SelectedIndex == 2) // Rigged - needs to create bonelist
            {
                p.Flags = 0x2001;
                MessageBox.Show("Warning: Rigged not supported yet");
            }
            p.ParentDOBJ = DOBJ.DOBJ;

            List<GXVertex> vert = new List<GXVertex>();
            foreach(SMDTriangle t in smd.Triangles)
            {
                vert.Add(SMDVertexToGXVertex(t.v1));
                vert.Add(SMDVertexToGXVertex(t.v2));
                vert.Add(SMDVertexToGXVertex(t.v3));
            }

            DOBJ.VertsToImport = vert.ToArray();
            
            exitStatus = ExitStatus.Opened;
            Close();
        }

        public GXVertex SMDVertexToGXVertex(SMDVertex v)
        {
            GXVertex o = new GXVertex()
            {
                Pos = new GXVector3(v.P.X, v.P.Y, v.P.Z),
                Nrm = new GXVector3(v.N.X, v.N.Y, v.N.Z),
                TX0 = new GXVector2(v.UV.X, v.UV.Y),
            };
            return o;
        }

        private void DOBJImportSettings_Load(object sender, EventArgs e)
        {

        }
    }
}
