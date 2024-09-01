using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SmashForge
{
    public partial class PolygonFormatEditor : Form
    {
        private Dictionary<string, int> WeightTypes = new Dictionary<string, int>()
        {
            { "None", (int)Nud.Polygon.BoneTypes.NoBones},
            { "Byte", (int)Nud.Polygon.BoneTypes.Byte},
            { "Half Float", (int)Nud.Polygon.BoneTypes.HalfFloat},
            { "Float", (int)Nud.Polygon.BoneTypes.Float}
        };
        private Dictionary<string, int> NormalTypes = new Dictionary<string, int>()
        {
            { "No Normals", (int)Nud.Polygon.VertexTypes.NoNormals},
            { "Normals (Half Float)", (int)Nud.Polygon.VertexTypes.NormalsHalfFloat},
            { "Normals, Tan, Bi-Tan (Half Float)", (int)Nud.Polygon.VertexTypes.NormalsTanBiTanHalfFloat},
            { "Normals (Float)", (int)Nud.Polygon.VertexTypes.NormalsFloat},
            { "Normals, Tan, Bi-Tan (Float)", (int)Nud.Polygon.VertexTypes.NormalsTanBiTanFloat}
        };
        private Dictionary<string, int> ColorTypes = new Dictionary<string, int>()
        {
            { "None", (int)Nud.Polygon.VertexColorTypes.None},
            { "Byte", (int)Nud.Polygon.VertexColorTypes.Byte},
            { "Half Float", (int)Nud.Polygon.VertexColorTypes.HalfFloat}
        };
        private Dictionary<string, int> UVTypes = new Dictionary<string, int>()
        {
            { "Half Float", (int)Nud.Polygon.UVTypes.HalfFloat},
            { "Float", (int)Nud.Polygon.UVTypes.Float}
        };
        private Dictionary<string, int> FaceTypes = new Dictionary<string, int>()
        {
            { "Triangles", (int)Nud.Polygon.PrimitiveTypes.Triangles},
            { "Triangle Strips", (int)Nud.Polygon.PrimitiveTypes.TriangleStrip}
        };
        private PolygonFormatEditor()
        {
            InitializeComponent();

            weightTypeComboBox.BeginUpdate();
            normalTypeComboBox.BeginUpdate();
            colorTypeComboBox.BeginUpdate();
            uvTypeComboBox.BeginUpdate();
            faceTypeComboBox.BeginUpdate();

            weightTypeComboBox.Items.Clear();
            normalTypeComboBox.Items.Clear();
            colorTypeComboBox.Items.Clear();
            uvTypeComboBox.Items.Clear();
            faceTypeComboBox.Items.Clear();

            weightTypeComboBox.Items.AddRange(WeightTypes.Keys.ToArray());
            normalTypeComboBox.Items.AddRange(NormalTypes.Keys.ToArray());
            colorTypeComboBox.Items.AddRange(ColorTypes.Keys.ToArray());
            uvTypeComboBox.Items.AddRange(UVTypes.Keys.ToArray());
            faceTypeComboBox.Items.AddRange(FaceTypes.Keys.ToArray());

            weightTypeComboBox.SelectedIndex = 0;
            normalTypeComboBox.SelectedIndex = 0;
            colorTypeComboBox.SelectedIndex = 0;
            uvTypeComboBox.SelectedIndex = 0;
            faceTypeComboBox.SelectedIndex = 0;

            weightTypeComboBox.EndUpdate();
            normalTypeComboBox.EndUpdate();
            colorTypeComboBox.EndUpdate();
            uvTypeComboBox.EndUpdate();
            faceTypeComboBox.EndUpdate();

            uvCountUpDown.Value = 0;
        }

        private static int SetComboBox(System.Windows.Forms.ComboBox comboBox, Dictionary<string, int> types, int value)
        {
            foreach (string key in types.Keys)
            {
                if (value == types[key])
                {
                    comboBox.SelectedIndex = comboBox.FindStringExact(key);
                    break;
                }
            }
            return comboBox.SelectedIndex;
        }

        private Nud.Polygon poly;
        public PolygonFormatEditor(Nud.Polygon poly) : this()
        {
            this.poly = poly;

            SetComboBox(weightTypeComboBox, WeightTypes, poly.boneType);
            SetComboBox(normalTypeComboBox, NormalTypes, poly.normalType);
            SetComboBox(colorTypeComboBox, ColorTypes, poly.colorType);
            SetComboBox(uvTypeComboBox, UVTypes, poly.uvType);
            SetComboBox(faceTypeComboBox, FaceTypes, poly.strip);

            uvCountUpDown.Value = poly.uvCount;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            int weightType = WeightTypes[(string)weightTypeComboBox.SelectedItem];
            int normalType = NormalTypes[(string)normalTypeComboBox.SelectedItem];
            int colorType = ColorTypes[(string)colorTypeComboBox.SelectedItem];
            int uvType = UVTypes[(string)uvTypeComboBox.SelectedItem];
            int uvCount = (int)uvCountUpDown.Value;
            int faceType = FaceTypes[(string)faceTypeComboBox.SelectedItem];
            foreach (Nud.Vertex v in poly.vertices)
            {
                if (weightType == (int)Nud.Polygon.BoneTypes.HalfFloat)
                {
                    for (int i = 0; i < v.boneIds.Count; i++)
                        v.boneIds[i] = (int)(short)v.boneIds[i];
                    for (int i = 0; i < v.boneWeights.Count; i++)
                        v.boneWeights[i] = FileData.ToFloat(FileData.FromFloat(v.boneWeights[i]));
                }
                else if (weightType == (int)Nud.Polygon.BoneTypes.Byte)
                {
                    for (int i = 0; i < v.boneIds.Count; i++)
                        v.boneIds[i] = (int)(byte)v.boneIds[i];
                    for (int i = 0; i < v.boneWeights.Count; i++)
                        v.boneWeights[i] = ((float)((byte)(v.boneWeights[i] * 255))) / 255;
                }

                while (v.uv.Count < uvCount)
                {
                    if (v.uv.Count > 0)
                        v.uv.Add(new OpenTK.Vector2(v.uv[0].X, v.uv[0].Y));
                    else
                        v.uv.Add(new OpenTK.Vector2(0, 0));
                }
                while (v.uv.Count > uvCount)
                {
                    v.uv.RemoveAt(v.uv.Count - 1);
                }

                if (uvType == (int)Nud.Polygon.UVTypes.HalfFloat)
                {
                    for (int i = 0; i < v.uv.Count; i++)
                        v.uv[i] = new OpenTK.Vector2(FileData.ToFloat(FileData.FromFloat(v.uv[i].X)), FileData.ToFloat(FileData.FromFloat(v.uv[i].Y)));
                }

                if (colorType == (int)Nud.Polygon.VertexColorTypes.None)
                {
                    v.color = new OpenTK.Vector4(127, 127, 127, 127);
                }
            }
            if (faceType == (int)Nud.Polygon.PrimitiveTypes.Triangles)
            {
                poly.vertexIndices = poly.GetTriangles();
            }
            else if (faceType == (int)Nud.Polygon.PrimitiveTypes.TriangleStrip)
            {
                poly.vertexIndices = poly.GetTriangleStrip();
            }

            poly.boneType = weightType;
            poly.normalType = normalType;
            poly.colorType = colorType;
            poly.uvType = uvType;
            poly.uvCount = uvCount;
            poly.strip = faceType;

            Close();
        }
    }
}
