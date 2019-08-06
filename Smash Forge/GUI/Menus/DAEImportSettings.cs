using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;

namespace SmashForge
{
    public partial class DAEImportSettings : Form
    {
        public enum ExitStatus
        {
            Running = 0,
            Opened = 1,
            Cancelled = 2
        }

        public ExitStatus exitStatus = ExitStatus.Running; 

        public Dictionary<string, int> BoneTypes = new Dictionary<string, int>()
        {
            { "None", (int)Nud.Polygon.BoneTypes.NoBones},
            { "Float", (int)Nud.Polygon.BoneTypes.Float},
            { "Half Float", (int)Nud.Polygon.BoneTypes.HalfFloat},
            { "Byte", (int)Nud.Polygon.BoneTypes.Byte}
        };

        public Dictionary<string, int> VertexTypes = new Dictionary<string, int>()
        {
            { "No Normals", (int)Nud.Polygon.VertexTypes.NoNormals},
            { "Normals (Float)", (int)Nud.Polygon.VertexTypes.NormalsFloat},
            { "Normals, Tan, Bi-Tan (Float)", (int)Nud.Polygon.VertexTypes.NormalsTanBiTanFloat},
            { "Normals (Half Float)", (int)Nud.Polygon.VertexTypes.NormalsHalfFloat},
            { "Normals, Tan, Bi-Tan (Half Float)", (int)Nud.Polygon.VertexTypes.NormalsTanBiTanHalfFloat}
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

        public void DaeToNud(string fileName, ModelContainer container)
        {
            // TODO: Scale may be invalid if parsing the text box fails.
            var options = new ColladaPostProcessOptions
            {
                RotateX90 = rotate90CB.Checked,
                FlipUvs = flipUVCB.Checked,
                SmoothNormals = smoothNrmCB.Checked,
                TranslateUvs = transUvVerticalCB.Checked,
                Scale = GuiTools.TryParseTBFloat(scaleTB)
            };

            var task = Task.Run(() => Collada.DaetoNudAsync(fileName, container, options));
            task.Wait();

            bool checkedMeshName = false;
            bool fixMeshName = false;

            bool hasShownShadowWarning = false;

            foreach (Nud.Mesh mesh in container.NUD.Nodes)
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

                foreach (Nud.Polygon poly in mesh.Nodes)
                {
                    if (BoneTypes[(string)boneTypeComboBox.SelectedItem] == BoneTypes["None"])
                        poly.polflag = 0;

                    if (smoothNrmCB.Checked)
                        poly.SmoothNormals();

                    // Set the vertex size before tangent/bitangent calculations.
                    if (poly.vertSize == (int)Nud.Polygon.VertexTypes.NormalsHalfFloat) // what is this supposed to mean?
                        poly.vertSize = 0;
                    else
                        poly.vertSize = BoneTypes[(string)boneTypeComboBox.SelectedItem] | VertexTypes[(string)vertTypeComboBox.SelectedItem];

                    poly.CalculateTangentBitangent();          

                    int vertSizeShadowWarning = (int)Nud.Polygon.BoneTypes.HalfFloat | (int)Nud.Polygon.VertexTypes.NormalsTanBiTanHalfFloat;
                    if (!hasShownShadowWarning && poly.vertSize == vertSizeShadowWarning)
                    {
                        MessageBox.Show("Using \"" + (string)boneTypeComboBox.SelectedItem + "\" and \"" + (string)vertTypeComboBox.SelectedItem + "\" can make shadows not appear in-game.",
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        hasShownShadowWarning = true;
                    }

                    if (stageMatCB.Checked)
                    {
                        poly.materials.Clear();
                        poly.materials.Add(Nud.Material.GetStageDefault());
                    }

                    foreach (Nud.Vertex v in poly.vertices)
                    {
                        //Scroll UVs V by -1
                        if (transUvVerticalCB.Checked)
                            for (int i = 0; i < v.uv.Count; i++)
                                v.uv[i] = new Vector2(v.uv[i].X, v.uv[i].Y + 1);

                        // Halve vertex colors
                        if (vertColorDivCB.Checked)
                            for (int i = 0; i < 3; i++)
                                v.color[i] = v.color[i] / 2;

                        // Set vertex colors to white. 
                        if (vertcolorCB.Checked)
                            v.color = new Vector4(127, 127, 127, 127);
                    }
                }
            }

            // Wait until after the model is rotated.
            container.NUD.GenerateBoundingSpheres();
        }

        public VBN GetVBN()
        {
            if (!vbnFileLabel.Text.Equals(""))
                return new VBN(vbnFileLabel.Text);
            else
                return null;
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
