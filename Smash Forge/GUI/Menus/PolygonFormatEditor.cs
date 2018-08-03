using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge
{
    public partial class PolygonFormatEditor : Form
    {
        private Dictionary<string, int> WeightTypes = new Dictionary<string, int>()
        {
            { "None", (int)NUD.Polygon.BoneTypes.NoBones},
            { "Float", (int)NUD.Polygon.BoneTypes.Float},
            { "Half Float", (int)NUD.Polygon.BoneTypes.HalfFloat},
            { "Byte", (int)NUD.Polygon.BoneTypes.Byte}
        };
        private Dictionary<string, int> NormalTypes = new Dictionary<string, int>()
        {
            { "No Normals", (int)NUD.Polygon.VertexTypes.NoNormals},
            { "Normals (Float)", (int)NUD.Polygon.VertexTypes.NormalsFloat},
            { "Normals, Tan, Bi-Tan (Float)", (int)NUD.Polygon.VertexTypes.NormalsTanBiTanFloat},
            { "Normals (Half Float)", (int)NUD.Polygon.VertexTypes.NormalsHalfFloat},
            { "Normals, Tan, Bi-Tan (Half Float)", (int)NUD.Polygon.VertexTypes.NormalsTanBiTanHalfFloat}
        };
        private PolygonFormatEditor()
        {
            InitializeComponent();

            weightTypeComboBox.BeginUpdate();
            normalTypeComboBox.BeginUpdate();
            weightTypeComboBox.Items.Clear();
            normalTypeComboBox.Items.Clear();
            weightTypeComboBox.Items.AddRange(WeightTypes.Keys.ToArray());
            normalTypeComboBox.Items.AddRange(NormalTypes.Keys.ToArray());
            weightTypeComboBox.SelectedIndex = 0;
            normalTypeComboBox.SelectedIndex = 0;
            weightTypeComboBox.EndUpdate();
            normalTypeComboBox.EndUpdate();

            uvCountUpDown.Value = 0;
            vertexColorCB.Checked = true;
        }

        private NUD.Polygon poly;
        public PolygonFormatEditor(NUD.Polygon poly) : this()
        {
            this.poly = poly;

            int weightType = poly.vertSize & 0xF0;
            int normalType = poly.vertSize & 0x0F;
            foreach (string key in WeightTypes.Keys)
            {
                if (weightType == WeightTypes[key])
                {
                    weightTypeComboBox.SelectedIndex = weightTypeComboBox.FindStringExact(key);
                    break;
                }
            }
            foreach (string key in NormalTypes.Keys)
            {
                if (normalType == NormalTypes[key])
                {
                    normalTypeComboBox.SelectedIndex = normalTypeComboBox.FindStringExact(key);
                    break;
                }
            }

            int uvCount = poly.UVSize >> 4;
            int colorType = poly.UVSize & 0x0F;
            uvCountUpDown.Value = uvCount;
            vertexColorCB.Checked = colorType != 0;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            int weightType = WeightTypes[(string)weightTypeComboBox.SelectedItem];
            int normalType = NormalTypes[(string)normalTypeComboBox.SelectedItem];
            poly.vertSize = weightType | normalType;
            foreach (NUD.Vertex v in poly.vertices)
            {
                if (weightType == (int)NUD.Polygon.BoneTypes.Float)
                {
                    for (int i = 0; i < v.boneIds.Count; ++i)
                        v.boneIds[i] = (int)v.boneIds[i];
                    for (int i = 0; i < v.boneWeights.Count; ++i)
                        v.boneWeights[i] = (float)v.boneWeights[i];
                }
                else if (weightType == (int)NUD.Polygon.BoneTypes.HalfFloat)
                {
                    for (int i = 0; i < v.boneIds.Count; ++i)
                        v.boneIds[i] = (int)(short)v.boneIds[i];
                    for (int i = 0; i < v.boneWeights.Count; ++i)
                        v.boneWeights[i] = (float)v.boneWeights[i];
                }
                else if (weightType == (int)NUD.Polygon.BoneTypes.Byte)
                {
                    for (int i = 0; i < v.boneIds.Count; ++i)
                        v.boneIds[i] = (int)(byte)v.boneIds[i];
                    for (int i = 0; i < v.boneWeights.Count; ++i)
                        v.boneWeights[i] = ((float)((byte)(v.boneWeights[i] * 255))) / 255;
                }
            }

            int uvCount = poly.UVSize >> 4;
            int colorType = poly.UVSize & 0x0F;

            while (uvCountUpDown.Value > uvCount)
            {
                foreach (NUD.Vertex v in poly.vertices)
                {
                    if (uvCount > 0)
                        v.uv.Add(new OpenTK.Vector2(v.uv[0].X, v.uv[0].Y));
                    else
                        v.uv.Add(new OpenTK.Vector2(0, 0));
                }
                ++uvCount;
            }
            while (uvCountUpDown.Value < uvCount)
            {
                foreach (NUD.Vertex v in poly.vertices)
                {
                    v.uv.RemoveAt(uvCount - 1);
                }
                --uvCount;
            }

            if (vertexColorCB.Checked)
            {
                if (colorType == 0x0)
                {
                    colorType = 0x2;
                    foreach (NUD.Vertex v in poly.vertices)
                        v.color = new OpenTK.Vector4(127, 127, 127, 127);
                }
            }
            else
            {
                if (colorType != 0x0)
                {
                    colorType = 0x0;
                    foreach (NUD.Vertex v in poly.vertices)
                        v.color = new OpenTK.Vector4(127, 127, 127, 127);
                }
            }

            poly.UVSize = ((uvCount & 0xF) << 4) | (colorType & 0xF);

            Close();
        }
    }
}
