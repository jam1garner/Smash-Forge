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
            nud.GenerateBoundingBoxes();
            Matrix4 rot = Matrix4.CreateRotationX(0.5f * (float)Math.PI);
            float sc = 1f;
            bool hasScale = float.TryParse(scaleTB.Text, out sc);

            bool checkedMeshName = false;
            bool fixMeshName = false;

            bool warning = false;

            foreach (NUD.Mesh mesh in nud.Nodes)
            {
                if (BoneTypes[(string)comboBox2.SelectedItem] == BoneTypes["No Bones"])
                    mesh.boneflag = 0;

                if (!checkedMeshName)
                {
                    checkedMeshName = true;
                    if (mesh.Text.Length > 5)
                    {
                        string sub = mesh.Text.Substring(0, 5);
                        int a = 0;
                        if (sub.StartsWith("_") && sub.EndsWith("_") && int.TryParse(sub.Substring(1, 3), out a))
                            fixMeshName = DialogResult.Yes == MessageBox.Show("Detected mesh names that start with \"_###_\". Would you like to fix this?\nIt is recommended that you select \"Yes\".", "Mesh Name Fix", MessageBoxButtons.YesNo);
                    }
                }
                if (fixMeshName)
                    if (mesh.Text.Length > 5)
                        mesh.Text = mesh.Text.Substring(5, mesh.Text.Length - 5);

                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    if (BoneTypes[(string)comboBox2.SelectedItem] == BoneTypes["No Bones"])
                        poly.polflag = 0;

                    //Smooth normals
                    if (smoothCB.Checked)
                        poly.SmoothNormals();

                    // we only want to calculate new tangents/bitangents for imports
                    // vanilla models have special tangents/bitangents for mirrored normal maps
                    NUD.computeTangentBitangent(poly);

                    poly.vertSize = ((poly.vertSize == 0x6 ? 0 : BoneTypes[(string)comboBox2.SelectedItem])) | (VertTypes[(string)comboBox1.SelectedItem]);

                    if (!warning && poly.vertSize == 0x27)
                    {
                        MessageBox.Show("Using \"" + (string)comboBox2.SelectedItem + "\" and \"" + (string)comboBox1.SelectedItem + "\" can make shadows not appear in-game.",
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        warning = true;
                    }

                    //Use stage material
                    if (stagematCB.Checked)
                    {
                        NUD.Mat_Texture tex = poly.materials[0].textures[0];
                        poly.materials[0].textures.Clear();
                        poly.materials.Clear();

                        NUD.Material m = new NUD.Material();
                        poly.materials.Add(m);
                        m.flags = 0xA2001001;
                        m.RefAlpha = 128;
                        m.cullMode = 1029;

                        m.textures.Clear();
                        m.textures.Add(tex);

                        m.entries.Add("NU_colorSamplerUV", new float[] { 1, 1, 0, 0 });
                        m.entries.Add("NU_diffuseColor", new float[] { 1, 1, 1, 0.5f });
                        m.entries.Add("NU_materialHash", new float[] { BitConverter.ToSingle(new byte[] { 0x12, 0xEE, 0x2A, 0x1B }, 0), 0, 0, 0 });
                    }

                    foreach (NUD.Vertex v in poly.vertices)
                    {
                        //Scroll UVs V by -1
                        if (checkBox2.Checked)
                            for (int i = 0; i < v.uv.Count; i++)
                                v.uv[i] = new Vector2(v.uv[i].X, v.uv[i].Y + 1);

                        //Flip UVs
                        if (flipUVCB.Checked)
                            for (int i = 0; i < v.uv.Count; i++)
                                v.uv[i] = new Vector2(v.uv[i].X, 1 - v.uv[i].Y);

                        //Halve vertex colors
                        if (vertColorDivCB.Checked)
                            for (int i = 0; i < 3; i++)
                                v.col[i] = v.col[i] / 2;

                        //Ignore vertex colors
                        if (vertcolorCB.Checked)
                            v.col = new Vector4(0x7F, 0x7F, 0x7F, 0xFF);

                        //Rotate 90 degrees
                        if (checkBox4.Checked)
                        {
                            v.pos = Vector3.TransformVector(v.pos, rot);
                            v.nrm = Vector3.TransformVector(v.nrm, rot);
                        }

                        //Scale
                        if (sc != 1f)
                            v.pos = Vector3.Multiply(v.pos, sc);
                    }
                }
            }

            //if (VertTypes[(string)comboBox1.SelectedItem] == 3 || VertTypes[(string)comboBox1.SelectedItem] == 7)

            nud.PreRender();

        }

        public VBN getVBN()
        {
            if (!vbnFileLabel.Text.Equals(""))
                return new VBN(vbnFileLabel.Text);
            else
                return null;
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
            if (vbnFileLabel.Text.Equals(""))
            {
                DialogResult dialogResult = MessageBox.Show("You are not using a VBN to import.\nDo you want to generate one?", "Warning", MessageBoxButtons.OKCancel);
                if (dialogResult == DialogResult.OK)
                {
                    exitStatus = Opened;
                    Close();
                }
            }else
            {
                exitStatus = Opened;
                Close();
            }
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
