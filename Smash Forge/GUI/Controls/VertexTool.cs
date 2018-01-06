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
    public partial class VertexTool : Form
    {
        public ModelViewport vp;

        public NUD.Vertex SelectedVertex
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
        private NUD.Vertex _selectedVertex;

        private VBN VBN;

        private int SelectedWeight = -1;

        public VertexTool()
        {
            InitializeComponent();
        }

        public void LoadVertexInfo(NUD.Vertex v)
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
            if (v.node.Count > 0)
                boneWeightList.Items.Add(con.VBN.bones[v.node[0] > -1 ? v.node[0] : 0].Text + " _ " + v.weight[0]);
            if (v.node.Count > 1)
                boneWeightList.Items.Add(con.VBN.bones[v.node[1] > -1 ? v.node[1] : 0].Text + " _ " + v.weight[1]);
            if (v.node.Count > 2)
                boneWeightList.Items.Add(con.VBN.bones[v.node[2] > -1 ? v.node[2] : 0].Text + " _ " + v.weight[2]);
            if (v.node.Count > 3)
                boneWeightList.Items.Add(con.VBN.bones[v.node[3] > -1 ? v.node[3] : 0].Text + " _ " + v.weight[3]);
        }

        private void vertexListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (vertexListBox.SelectedIndex >= 0)
            {
                SelectedVertex = ((NUD.Vertex)vertexListBox.SelectedItem);
            }
        }

        private void boneWeightList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateWeights();
        }

        private void UpdateWeights()
        {
            SelectedWeight = boneWeightList.SelectedIndex;
            WeightValue.Value = (decimal)SelectedVertex.weight[SelectedWeight];
        }
    }
}
