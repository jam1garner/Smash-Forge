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
            { "None", (int)NUD.Polygon.BoneTypes.NoBones},
            { "Float", (int)NUD.Polygon.BoneTypes.Float},
            { "Half Float", (int)NUD.Polygon.BoneTypes.HalfFloat},
            { "Byte", (int)NUD.Polygon.BoneTypes.Byte}
        };

        public Dictionary<string, int> VertexTypes = new Dictionary<string, int>()
        {
            { "No Normals", (int)NUD.Polygon.VertexTypes.NoNormals},
            { "Normals (Float)", (int)NUD.Polygon.VertexTypes.NormalsFloat},
            { "Normals, Tan, Bi-Tan (Float)", (int)NUD.Polygon.VertexTypes.NormalsTanBiTanFloat},
            { "Normals (Half Float)", (int)NUD.Polygon.VertexTypes.NormalsHalfFloat},
            { "Normals, Tan, Bi-Tan (Half Float)", (int)NUD.Polygon.VertexTypes.NormalsTanBiTanHalfFloat}
        };

        public DAEImportSettings()
        {
            InitializeComponent();
            Populate();

            transUvVerticalCB.Checked = true;
        }
        
        public void Populate()
        {
            vertTypeComboBox.BeginUpdate();
            boneTypeComboBox.BeginUpdate();

            foreach (string key in VertexTypes.Keys)
                vertTypeComboBox.Items.Add(key);

            foreach (string key in BoneTypes.Keys)
                boneTypeComboBox.Items.Add(key);

            vertTypeComboBox.SelectedIndex = 4;
            boneTypeComboBox.SelectedIndex = 3;

            vertTypeComboBox.EndUpdate();
            boneTypeComboBox.EndUpdate();
        }

        public void Apply(NUD nud)
        {
            nud.GenerateBoundingBoxes();
            Matrix4 rotXBy90 = Matrix4.CreateRotationX(0.5f * (float)Math.PI);
            float scale = 1f;
            bool hasScale = float.TryParse(scaleTB.Text, out scale);

            bool checkedMeshName = false;
            bool fixMeshName = false;

            bool hasShownShadowWarning = false;

            foreach (NUD.Mesh mesh in nud.Nodes)
            {
                if (BoneTypes[(string)boneTypeComboBox.SelectedItem] == BoneTypes["None"])
                    mesh.boneflag = 0;

                if (!checkedMeshName)
                {
                    checkedMeshName = true;
                    if (Collada.HasInitialUnderscoreId(mesh.Text))
                        fixMeshName = DialogResult.Yes == MessageBox.Show("Detected mesh names that start with \"_###_\". Would you like to fix this?\nIt is recommended that you select \"Yes\".", "Mesh Name Fix", MessageBoxButtons.YesNo);
                }

                if (fixMeshName)
                    mesh.Text = Collada.RemoveInitialUnderscoreId(mesh.Text);

                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    if (BoneTypes[(string)boneTypeComboBox.SelectedItem] == BoneTypes["None"])
                        poly.polflag = 0;

                    if (smoothNrmCB.Checked)
                        poly.SmoothNormals();

                    // Set the vertex size before tangent/bitangent calculations.
                    if (poly.vertSize == (int)NUD.Polygon.VertexTypes.NormalsHalfFloat) // what is this supposed to mean?
                        poly.vertSize = 0;
                    else
                        poly.vertSize = BoneTypes[(string)boneTypeComboBox.SelectedItem] | VertexTypes[(string)vertTypeComboBox.SelectedItem];

                    poly.CalculateTangentBitangent();          

                    int vertSizeShadowWarning = (int)NUD.Polygon.BoneTypes.HalfFloat | (int)NUD.Polygon.VertexTypes.NormalsTanBiTanHalfFloat;
                    if (!hasShownShadowWarning && poly.vertSize == vertSizeShadowWarning)
                    {
                        MessageBox.Show("Using \"" + (string)boneTypeComboBox.SelectedItem + "\" and \"" + (string)vertTypeComboBox.SelectedItem + "\" can make shadows not appear in-game.",
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        hasShownShadowWarning = true;
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
                                v.color[i] = v.color[i] / 2;

                        // Set vertex colors to white. 
                        if (vertcolorCB.Checked)
                            v.color = new Vector4(127, 127, 127, 127);

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
