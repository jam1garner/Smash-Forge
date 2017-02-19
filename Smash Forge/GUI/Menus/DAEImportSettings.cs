using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;

namespace Smash_Forge
{
    public partial class DAEImportSettings : Form
    {

        public string fname;

        public static int Running = 0;
        public static int Opened = 1;
        public static int Cancelled = 2;

        public int exitStatus = 0; //0 - not done, 1 - one is selected, 2 - cancelled

        public Dictionary<string, int> BoneTypes = new Dictionary<string, int>()
        {
            { "No Bones", 0x00},
            { "Bone Weight (Float)", 0x10},
            { "Bone Weight (Half Float)", 0x20},
            { "Bone Weight (Byte)", 0x40}
        };

        public Dictionary<string, int> VertTypes = new Dictionary<string, int>()
        {
            { "No Normals", 0x0},
            { "Normals (Float)", 0x1},
            { "Normals (Half Float)", 0x6}
        };

        public DAEImportSettings()
        {
            InitializeComponent();
            populate();
        }
        
        public void populate()
        {
            foreach (string key in VertTypes.Keys)
                comboBox1.Items.Add(key);

            foreach (string key in BoneTypes.Keys)
                comboBox2.Items.Add(key);

            comboBox1.SelectedIndex = 2;
            comboBox2.SelectedIndex = 1;
        }

        public void Apply(NUD nud)
        {
            Matrix4 rot = Matrix4.CreateRotationX(0.5f * (float)Math.PI);
            foreach (NUD.Mesh mesh in nud.mesh)
            {
                foreach (NUD.Polygon poly in mesh.polygons)
                {
                    poly.vertSize = (BoneTypes[(string)comboBox2.SelectedItem]) | (VertTypes[(string)comboBox1.SelectedItem]);

                    if (checkBox1.Checked || checkBox4.Checked)
                        foreach (NUD.Vertex v in poly.vertices)
                        {
                            if (checkBox1.Checked)
                                for (int i = 0; i < v.tx.Count; i++)
                                    v.tx[i] = new Vector2(v.tx[i].X, 1 - v.tx[i].Y);

                            if (checkBox4.Checked)
                            {
                                v.pos = Vector3.Transform(v.pos, rot);
                                v.nrm = Vector3.Transform(v.nrm, rot);
                            }
                        }
                }
            }

            if (checkBox2.Checked)
            {
                foreach (NUD.Mesh mesh in nud.mesh)
                {
                    if(mesh.Text.Length > 5)
                        mesh.Text = mesh.Text.Substring(5, mesh.Text.Length - 5);
                }
            }

            nud.PreRender();
        }

        private void closeButton(object sender, EventArgs e)
        {
            exitStatus = Cancelled;
            Close();
        }

        private void MaterialSelector_Load(object sender, EventArgs e)
        {
            populate();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            exitStatus = Opened;
            Close();
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
