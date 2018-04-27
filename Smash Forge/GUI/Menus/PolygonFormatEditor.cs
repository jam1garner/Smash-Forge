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
            Close();
        }
    }
}
