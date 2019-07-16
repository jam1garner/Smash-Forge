using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmashForge
{
    public partial class VertexTool : Form
    {
        private Nud.Vertex _selectedVertex;
        public ModelViewport vp;
        private VBN VBN;
        
        private int SelectedWeight = -1;

        public Nud.Vertex SelectedVertex
        {
            get
            {
                return _selectedVertex;
            }
            set
            {
                _selectedVertex = value;
                LoadVertexInfo(_selectedVertex);
            }
        }

        public VertexTool()
        {
            InitializeComponent();
        }

        public void LoadVertexInfo(Nud.Vertex v)
        {
            boneWeightList.Items.Clear();
            if (v == null)
            {
                return;
            }
            if (!(vp.draw[0] is ModelContainer)) return;
            ModelContainer con = (ModelContainer)vp.draw[0];
            if (con.VBN == null) return;
            VBN = con.VBN;
            if (v.boneIds.Count > 0)
                boneWeightList.Items.Add(con.VBN.bones[v.boneIds[0] > -1 ? v.boneIds[0] : 0].Text + " _ " + v.boneWeights[0]);
            if (v.boneIds.Count > 1)
                boneWeightList.Items.Add(con.VBN.bones[v.boneIds[1] > -1 ? v.boneIds[1] : 0].Text + " _ " + v.boneWeights[1]);
            if (v.boneIds.Count > 2)
                boneWeightList.Items.Add(con.VBN.bones[v.boneIds[2] > -1 ? v.boneIds[2] : 0].Text + " _ " + v.boneWeights[2]);
            if (v.boneIds.Count > 3)
                boneWeightList.Items.Add(con.VBN.bones[v.boneIds[3] > -1 ? v.boneIds[3] : 0].Text + " _ " + v.boneWeights[3]);
        }

        private void vertexListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (vertexListBox.SelectedIndex >= 0)
            {
                SelectedVertex = ((Nud.Vertex)vertexListBox.SelectedItem);
            }
        }

        private void boneWeightList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateWeights();
        }

        private void UpdateWeights()
        {
            SelectedWeight = boneWeightList.SelectedIndex;
            WeightValue.Value = (decimal)SelectedVertex.boneWeights[SelectedWeight];
        }
    }
}
