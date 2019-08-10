using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;

namespace SmashForge
{
    public partial class DAEImportSettings : Form
    {
        public bool ShouldImportCollada { get; private set; }

        private string vbnFilePath;

        private readonly Dictionary<string, int> boneTypes = new Dictionary<string, int>()
        {
            { "None", (int)Nud.Polygon.BoneTypes.NoBones},
            { "Float", (int)Nud.Polygon.BoneTypes.Float},
            { "Half Float", (int)Nud.Polygon.BoneTypes.HalfFloat},
            { "Byte", (int)Nud.Polygon.BoneTypes.Byte}
        };

        private readonly Dictionary<string, int> vertexTypes = new Dictionary<string, int>()
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
        }

        public void Populate()
        {
            vertColorComboBox.SelectedIndex = 0;

            vertTypeComboBox.BeginUpdate();
            boneTypeComboBox.BeginUpdate();

            foreach (string key in vertexTypes.Keys)
                vertTypeComboBox.Items.Add(key);

            foreach (string key in boneTypes.Keys)
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
                Scale = GuiTools.TryParseTBFloat(scaleTB)
            };

            var task = Task.Run(() => Collada.DaetoNudAsync(fileName, container, options));
            task.Wait();

            bool checkedMeshName = false;
            bool fixMeshName = false;

            bool hasShownShadowWarning = false;

            foreach (Nud.Mesh mesh in container.NUD.Nodes)
            {
                if (boneTypes[(string)boneTypeComboBox.SelectedItem] == boneTypes["None"])
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
                    if (boneTypes[(string)boneTypeComboBox.SelectedItem] == boneTypes["None"])
                        poly.polflag = 0;

                    // Set the vertex size before tangent/bitangent calculations.
                    if (poly.vertSize == (int)Nud.Polygon.VertexTypes.NormalsHalfFloat) // what is this supposed to mean?
                        poly.vertSize = 0;
                    else
                        poly.vertSize = boneTypes[(string)boneTypeComboBox.SelectedItem] | vertexTypes[(string)vertTypeComboBox.SelectedItem];

                    poly.CalculateTangentBitangent();

                    int vertSizeShadowWarning = (int)Nud.Polygon.BoneTypes.HalfFloat | (int)Nud.Polygon.VertexTypes.NormalsTanBiTanHalfFloat;
                    if (!hasShownShadowWarning && poly.vertSize == vertSizeShadowWarning)
                    {
                        MessageBox.Show("Using \"" + (string)boneTypeComboBox.SelectedItem + "\" and \"" + (string)vertTypeComboBox.SelectedItem + "\" can make shadows not appear in-game.",
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        hasShownShadowWarning = true;
                    }

                    switch (vertColorComboBox.SelectedIndex)
                    {
                        case 0:
                            // Preserve existing colors.
                            break;
                        case 1:
                            poly.SetVertexColor(new Vector4(127, 127, 127, 127));
                            break;
                        case 2:
                            DivideVertexColorsBy2(poly);
                            break;
                    }
                }
            }

            // Wait until after the model is rotated to generate bounding spheres.
            container.NUD.GenerateBoundingSpheres();
        }

        private static void DivideVertexColorsBy2(Nud.Polygon poly)
        {
            foreach (Nud.Vertex v in poly.vertices)
            {
                for (int i = 0; i < 3; i++)
                    v.color[i] = v.color[i] / 2.0f;
            }
        }

        public VBN GetVBN()
        {
            if (!string.IsNullOrEmpty(vbnFilePath))
                return new VBN(vbnFilePath);

            return null;
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(vbnFilePath))
            {
                DialogResult dialogResult = MessageBox.Show("You are not using a VBN to import.\nDo you want to generate one?", "Warning", MessageBoxButtons.OKCancel);
                if (dialogResult == DialogResult.OK)
                {
                    ShouldImportCollada = true;
                }
            }
            else
            {
                ShouldImportCollada = true;
            }

            Close();
        }

        private void openVbnButton_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "Visual Bone Namco (.vbn)|*.vbn|All Files (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    vbnFilePath = ofd.FileName;
                }
            }
        }
    }
}
