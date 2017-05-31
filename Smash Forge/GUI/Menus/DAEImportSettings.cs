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
            { "Normals, Tan, Bi-Tan (Float)", 0x3},
            { "Normals (Half Float)", 0x6},
            { "Normals, Tan, Bi-Tan (Half Float)", 0x7}
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

            comboBox1.SelectedIndex = 3;
            comboBox2.SelectedIndex = 2;
        }

        public void Apply(NUD nud)
        {
            Matrix4 rot = Matrix4.CreateRotationX(0.5f * (float)Math.PI);
            float sc = 1f;
            bool hasScale = float.TryParse(scaleTB.Text, out sc);

            bool checkedUVRange = false;
            bool fixUV = false;

            bool warning = false;

            foreach (NUD.Mesh mesh in nud.mesh)
            {
                if (BoneTypes[(string)comboBox2.SelectedItem] == BoneTypes["No Bones"])
                    mesh.boneflag = 0;

                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    if (smoothCB.Checked)
                        poly.SmoothNormals();

                    poly.vertSize = ((poly.vertSize == 0x6 ? 0 : BoneTypes[(string)comboBox2.SelectedItem])) | (VertTypes[(string)comboBox1.SelectedItem]);

                    if(!warning && poly.vertSize == 0x27)
                    {
                        MessageBox.Show("Using \""+ (string)comboBox2.SelectedItem + "\" and \"" + (string)comboBox1.SelectedItem + "\" can make shadows not appear in-game",
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        warning = true;
                    }
                    
                    //if (checkBox1.Checked || checkBox4.Checked || vertcolorCB.Checked || sc != 1f)
                        foreach (NUD.Vertex v in poly.vertices)
                        {

                            if (!checkedUVRange && (Math.Abs(v.tx[0].X) > 4 || Math.Abs(v.tx[0].Y) > 4))
                            {
                                checkedUVRange = true;

                                DialogResult dialogResult = MessageBox.Show("Some UVs are detected to be out of accurate range.\nFix them now?", "Potential UV Problem", MessageBoxButtons.YesNo);
                                if (dialogResult == DialogResult.Yes)
                                    fixUV = true;
                            }

                            if (fixUV)
                            {
                                for (int h = 0; h < v.tx.Count; h++)
                                    v.tx[h] = new Vector2(v.tx[h].X - (int)v.tx[h].X, v.tx[h].Y - (int)v.tx[h].Y);
                            }


                            if (checkBox1.Checked)
                                for (int i = 0; i < v.tx.Count; i++)
                                    v.tx[i] = new Vector2(v.tx[i].X, 1 - v.tx[i].Y);

                            if (vertcolorCB.Checked)
                                v.col = new Vector4(0x7F, 0x7F, 0x7F, 0x7F);

                            if (checkBox4.Checked)
                            {
                                v.pos = Vector3.Transform(v.pos, rot);
                                v.nrm = Vector3.Transform(v.nrm, rot);
                            }
                            if(sc != 1f)
                                v.pos = Vector3.Multiply(v.pos, sc);
                        }
                }
            }

            if (VertTypes[(string)comboBox1.SelectedItem] == 3 || VertTypes[(string)comboBox1.SelectedItem] == 7)
                nud.computeTangentBitangent();

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

        public VBN getVBN()
        {
            VBN v = null;

            if (!vbnFileLabel.Text.Equals(""))
            {
                v = new VBN(vbnFileLabel.Text);
            }else
            {
                if (Runtime.ModelContainers.Count > 0)
                    v = Runtime.ModelContainers[0].vbn;
            }

            return v;
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

        private void vbnButton_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog() { Filter = "Visual Bone Namco (.vbn)|*.vbn|All Files (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    vbnFileLabel.Text = ofd.FileName;
                }
            }
        }
    }
}
