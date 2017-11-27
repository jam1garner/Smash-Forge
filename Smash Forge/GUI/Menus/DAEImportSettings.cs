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

            comboBox1.SelectedIndex = 2;
            comboBox2.SelectedIndex = 1;
        }

        public void Apply(NUD nud)
        {
            Matrix4 rot = Matrix4.CreateRotationX(0.5f * (float)Math.PI);
            float sc = 1f;
            bool hasScale = float.TryParse(scaleTB.Text, out sc);

            bool checkedUVRange = false;
            bool fixUV = false;

            bool warning = false;

            foreach (NUD.Mesh mesh in nud.meshes)
            {
                if (BoneTypes[(string)comboBox2.SelectedItem] == BoneTypes["No Bones"])
                    mesh.boneflag = 0;

                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    if (BoneTypes[(string)comboBox2.SelectedItem] == BoneTypes["No Bones"])
                        poly.polflag = 0;

                    if (smoothCB.Checked)
                        poly.SmoothNormals();

                    // we only want to calculate new tangents/bitangents for imports
                    // vanilla models have special tangents/bitangents for mirrored normal maps
                    NUD.computeTangentBitangent(poly);

                    poly.vertSize = ((poly.vertSize == 0x6 ? 0 : BoneTypes[(string)comboBox2.SelectedItem])) | (VertTypes[(string)comboBox1.SelectedItem]);

                    if (!warning && poly.vertSize == 0x27)
                    {
                        MessageBox.Show("Using \"" + (string)comboBox2.SelectedItem + "\" and \"" + (string)comboBox1.SelectedItem + "\" can make shadows not appear in-game",
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        warning = true;
                    }

                    if (stagematCB.Checked)
                    {
                        // change to stage material
                        NUD.Mat_Texture tex = poly.materials[0].textures[0];
                        poly.materials[0].textures.Clear();
                        poly.materials.Clear();

                        NUD.Material m = new NUD.Material();
                        poly.materials.Add(m);
                        m.flags = 0xA2011001;
                        m.RefAlpha = 128;
                        m.cullMode = 1029;

                        m.textures.Clear();
                        m.textures.Add(tex);

                        m.entries.Add("NU_colorSamplerUV", new float[] { 1, 1, 0, 0 });
                        m.entries.Add("NU_diffuseColor", new float[] { 1, 1, 1, 0.5f });
                        m.entries.Add("NU_materialHash", new float[] { BitConverter.ToSingle(new byte[] { 0x12, 0xEE, 0x2A, 0x1B }, 0), 0, 0, 0 });
                    }

                    //if (checkBox1.Checked || checkBox4.Checked || vertcolorCB.Checked || sc != 1f)
                    foreach (NUD.Vertex v in poly.vertices)
                    {

                        if (!checkedUVRange && v.uv.Count > 0 && (Math.Abs(v.uv[0].X) > 4 || Math.Abs(v.uv[0].Y) > 4))
                        {
                            checkedUVRange = true;

                            DialogResult dialogResult = MessageBox.Show("Some UVs are detected to be out of accurate range.\nFix them now?", "Potential UV Problem", MessageBoxButtons.YesNo);
                            if (dialogResult == DialogResult.Yes)
                                fixUV = true;
                        }

                        if (fixUV)
                        {
                            for (int h = 0; h < v.uv.Count; h++)
                                v.uv[h] = new Vector2(v.uv[h].X - (int)v.uv[h].X, v.uv[h].Y - (int)v.uv[h].Y);
                        }


                        if (flipUVCB.Checked)
                            for (int i = 0; i < v.uv.Count; i++)
                                v.uv[i] = new Vector2(v.uv[i].X, 1 - v.uv[i].Y);

                        if (vertColorDivCB.Checked)
                            v.col = v.col / 2;

                        if (vertcolorCB.Checked)
                            v.col = new Vector4(0x7F, 0x7F, 0x7F, 0x7F);

                        if (checkBox4.Checked)
                        {
                            v.pos = Vector3.Transform(v.pos, rot);
                            v.nrm = Vector3.Transform(v.nrm, rot);
                        }
                        if (sc != 1f)
                            v.pos = Vector3.Multiply(v.pos, sc);
                    }
                }
            }

            //if (VertTypes[(string)comboBox1.SelectedItem] == 3 || VertTypes[(string)comboBox1.SelectedItem] == 7)
           

            if (checkBox2.Checked)
            {
                foreach (NUD.Mesh mesh in nud.meshes)
                {
                    if (mesh.Text.Length > 5)
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

        private void vertColorDivCB_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
