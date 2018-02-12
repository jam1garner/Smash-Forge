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
using System.Diagnostics;

namespace Smash_Forge
{
    public partial class DAEImportSettings : Form
    {

        public string fileName;

        public enum ExitStatus
        {
            Running = 0,
            Opened = 1,
            Cancelled = 2
        }

        public ExitStatus exitStatus = ExitStatus.Running; 

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
            Populate();
        }
        
        public void Populate()
        {
            foreach (string key in VertTypes.Keys)
                vertTypeComboBox.Items.Add(key);

            foreach (string key in BoneTypes.Keys)
                boneTypeComboBox.Items.Add(key);

            vertTypeComboBox.SelectedIndex = 2;
            boneTypeComboBox.SelectedIndex = 1;
        }

        public void Apply(NUD nud)
        {
            nud.GenerateBoundingBoxes();
            Matrix4 rotXBy90 = Matrix4.CreateRotationX(0.5f * (float)Math.PI);
            float scale = 1f;
            bool hasScale = float.TryParse(scaleTB.Text, out scale);

            bool checkedMeshName = false;
            bool fixMeshName = false;

            bool warning = false;

            foreach (NUD.Mesh mesh in nud.Nodes)
            {
                if (BoneTypes[(string)boneTypeComboBox.SelectedItem] == BoneTypes["No Bones"])
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
                    if (BoneTypes[(string)boneTypeComboBox.SelectedItem] == BoneTypes["No Bones"])
                        poly.polflag = 0;

                    if (smoothNrmCB.Checked)
                        poly.SmoothNormals();

                    // Set the vertex size before tangent/bitangent calculations.
                    poly.vertSize = ((poly.vertSize == 0x6 ? 0 : BoneTypes[(string)boneTypeComboBox.SelectedItem])) | (VertTypes[(string)vertTypeComboBox.SelectedItem]);
                    poly.CalculateTangentBitangent();          

                    if (!warning && poly.vertSize == 0x27)
                    {
                        MessageBox.Show("Using \"" + (string)boneTypeComboBox.SelectedItem + "\" and \"" + (string)vertTypeComboBox.SelectedItem + "\" can make shadows not appear in-game.",
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        warning = true;
                    }

                    if (stageMatCB.Checked)
                    {
                        poly.materials.Clear();
                        poly.materials.Add(NUD.Material.GetStageDefault());
                    }

                    foreach (NUD.Vertex v in poly.vertices)
                    {
                        //Scroll UVs V by -1
                        if (transUvVerticalCB.Checked)
                            for (int i = 0; i < v.uv.Count; i++)
                                v.uv[i] = new Vector2(v.uv[i].X, v.uv[i].Y + 1);

                        // Flip UVs
                        if (flipUVCB.Checked)
                            for (int i = 0; i < v.uv.Count; i++)
                                v.uv[i] = new Vector2(v.uv[i].X, 1 - v.uv[i].Y);

                        // Halve vertex colors
                        if (vertColorDivCB.Checked)
                            for (int i = 0; i < 3; i++)
                                v.col[i] = v.col[i] / 2;

                        // Set vertex colors to white. 
                        if (vertcolorCB.Checked)
                            v.col = new Vector4(127, 127, 127, 255);

                        // Rotate 90 degrees.
                        if (rotate90CB.Checked)
                        {
                            v.pos = Vector3.TransformPosition(v.pos, rotXBy90);
                            v.nrm = Vector3.TransformNormal(v.nrm, rotXBy90);
                        }

                        // Scale.
                        if (scale != 1f)
                            v.pos = Vector3.Multiply(v.pos, scale);
                    }
                }
            }

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
            exitStatus = ExitStatus.Cancelled;
            Close();
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            if (vbnFileLabel.Text.Equals(""))
            {
                DialogResult dialogResult = MessageBox.Show("You are not using a VBN to import.\nDo you want to generate one?", "Warning", MessageBoxButtons.OKCancel);
                if (dialogResult == DialogResult.OK)
                {
                    exitStatus = ExitStatus.Opened;
                    Close();
                }
            }else
            {
                exitStatus = ExitStatus.Opened;
                Close();
            }
        }

        private void openVbnButton_Click(object sender, EventArgs e)
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
