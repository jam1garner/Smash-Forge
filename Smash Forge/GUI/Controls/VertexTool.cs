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
        public List<NUD.Vertex> vertices = new List<NUD.Vertex>();

        public VertexTool()
        {
            InitializeComponent();
        }

        public void refresh()
        {
            // grab contcainer
            vertexListBox.Items.Clear();

            ModelContainer con = vp.draw[0];
            foreach(NUD.Mesh mesh in con.nud.mesh)
            {
                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    for(int i = 0; i < poly.selectedVerts.Length; i++)
                    {
                        if (poly.selectedVerts[i] > 0)
                        {
                            vertexListBox.Items.Add(poly.vertices[i]);
                        }
                    }
                }
            }
        }

        public void LoadVertexInfo(NUD.Vertex v)
        {
            ModelContainer con = vp.draw[0];
            boneWeightList.Items.Clear();
            if (con.vbn == null) return;
            if (v.node.Count > 0)
                boneWeightList.Items.Add(con.vbn.bones[v.node[0] > -1 ? v.node[0] : 0].Text + " _ " + v.weight[0]);
            if (v.node.Count > 1)
                boneWeightList.Items.Add(con.vbn.bones[v.node[1] > -1 ? v.node[1] : 0].Text + " _ " + v.weight[1]);
            if (v.node.Count > 2)
                boneWeightList.Items.Add(con.vbn.bones[v.node[2] > -1 ? v.node[2] : 0].Text + " _ " + v.weight[2]);
            if (v.node.Count > 3)
                boneWeightList.Items.Add(con.vbn.bones[v.node[3] > -1 ? v.node[3] : 0].Text + " _ " + v.weight[3]);
        }

        private void vertexListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(vertexListBox.SelectedIndex >= 0)
            {
                ModelContainer con = vp.draw[0];
                NUD.Vertex v = (NUD.Vertex)vertexListBox.SelectedItem;
                SelectVertex(v);
                LoadVertexInfo(v);
            }
        }

        private void SelectVertex(NUD.Vertex v)
        {
            ModelContainer con = vp.draw[0];
            foreach (NUD.Mesh mesh in con.nud.mesh)
            {
                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    for (int i = 0; i < poly.vertices.Count; i++)
                    {
                        if (poly.selectedVerts[i] < 0)
                            poly.selectedVerts[i] = 1;
                        if (poly.vertices[i] == v)
                        {
                            poly.selectedVerts[i] = -1;
                            return;
                        }
                    }
                }
            }
        }
    }
}
