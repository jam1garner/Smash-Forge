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

namespace SmashForge
{
    public partial class BufferList : Form
    {
        public BufferList()
        {
            InitializeComponent();
        }
        public string AttName;
        public BFRES.Mesh msh;
        public BFRES.FMDL_Model model;

        public void SetFaceBufferList()
        {

        }
        public void SetVertexBufferList(BFRES.Mesh m, string attName, BFRES.FMDL_Model mdl)
        {
            listView1.Items.Clear();

            //Set public variables so i can use it for other things
            AttName = attName;
            model = mdl; //For updating meshes
            msh = m; //For updating meshes

            foreach (BFRES.Vertex v in msh.vertices)
            {
                if (attName == "_p0")
                {
                    var item = new ListViewItem(v.pos.X.ToString());
                    item.SubItems.Add(v.pos.Y.ToString());
                    item.SubItems.Add(v.pos.Z.ToString());
                    listView1.Items.Add(item);
                }
                if (attName == "_p1")
                {
                    var item = new ListViewItem(v.pos1.X.ToString());
                    item.SubItems.Add(v.pos1.Y.ToString());
                    item.SubItems.Add(v.pos1.Z.ToString());
                    listView1.Items.Add(item);
                }
                if (attName == "_p2")
                {
                    var item = new ListViewItem(v.pos2.X.ToString());
                    item.SubItems.Add(v.pos2.Y.ToString());
                    item.SubItems.Add(v.pos2.Z.ToString());
                    listView1.Items.Add(item);
                }
                if (attName == "_n0")
                {
                    var item = new ListViewItem(v.nrm.X.ToString());
                    item.SubItems.Add(v.nrm.Y.ToString());
                    item.SubItems.Add(v.nrm.Z.ToString());
                    listView1.Items.Add(item);
                }
                if (attName == "_u0")
                {
                    var item = new ListViewItem(v.uv0.X.ToString());
                    item.SubItems.Add(v.uv0.Y.ToString());
                    listView1.Items.Add(item);
                }
                if (attName == "_u1")
                {
                    var item = new ListViewItem(v.uv1.X.ToString());
                    item.SubItems.Add(v.uv1.Y.ToString());
                    listView1.Items.Add(item);
                }
                if (attName == "_u2")
                {
                    var item = new ListViewItem(v.uv2.X.ToString());
                    item.SubItems.Add(v.uv2.Y.ToString());
                    listView1.Items.Add(item);
                }
                if (attName == "_w0")
                {
                    float X = v.boneWeights.Count > 0 ? v.boneWeights[0] : 0;
                    float Y = v.boneWeights.Count > 1 ? v.boneWeights[1] : 0;
                    float Z = v.boneWeights.Count > 2 ? v.boneWeights[2] : 0;
                    float W = v.boneWeights.Count > 3 ? v.boneWeights[3] : 0;

                    var item = new ListViewItem(X.ToString());
                    item.SubItems.Add(Y.ToString());
                    item.SubItems.Add(Z.ToString());
                    item.SubItems.Add(W.ToString());
                    listView1.Items.Add(item);
                }
                if (attName == "_i0")
                {
                    string X = "";
                    string Y = "";
                    string Z = "";
                    string W = "";

                    float iX = v.boneIds.Count > 0 ? v.boneIds[0] : 0;
                    float iY = v.boneIds.Count > 1 ? v.boneIds[1] : 0;
                    float iZ = v.boneIds.Count > 2 ? v.boneIds[2] : 0;
                    float iW = v.boneIds.Count > 3 ? v.boneIds[3] : 0;

                    //Set the bone in the skeleton of it's fmdl. 
                    try
                    {
                        if (iX != 0)
                            X = model.skeleton.bones[model.Node_Array[(int)iX]].Text;
                        if (iY != 0)
                            Y = model.skeleton.bones[model.Node_Array[(int)iY]].Text;
                        if (iZ != 0)
                            Z = model.skeleton.bones[model.Node_Array[(int)iZ]].Text;
                        if (iW != 0)
                            W = model.skeleton.bones[model.Node_Array[(int)iW]].Text;
                    }
                    catch
                    {
                        X = iX.ToString();
                        Y = iY.ToString();
                        Z = iZ.ToString();
                        W = iW.ToString();
                    }


                    var item = new ListViewItem(X);
                    item.SubItems.Add(Y);
                    item.SubItems.Add(Z);
                    item.SubItems.Add(W);
                    listView1.Items.Add(item);
                }
                if (attName == "_b0")
                {
                    var item = new ListViewItem(v.bitan.X.ToString());
                    item.SubItems.Add(v.bitan.Y.ToString());
                    item.SubItems.Add(v.bitan.Z.ToString());
                    item.SubItems.Add(v.bitan.W.ToString());
                    listView1.Items.Add(item);
                }
                if (attName == "_t0")
                {
                    var item = new ListViewItem(v.tan.X.ToString());
                    item.SubItems.Add(v.tan.Y.ToString());
                    item.SubItems.Add(v.tan.Z.ToString());
                    item.SubItems.Add(v.tan.W.ToString());
                    listView1.Items.Add(item);
                }
                if (attName == "_c0")
                {
                    Color SetColor = Color.White;

                    int someIntX = (int)Math.Ceiling(v.col.X * 255);
                    int someIntY = (int)Math.Ceiling(v.col.Y * 255);
                    int someIntZ = (int)Math.Ceiling(v.col.Z * 255);
                    int someIntW = (int)Math.Ceiling(v.col.W * 255);

                    SetColor = Color.FromArgb(
                255,
                someIntX,
                someIntY,
                someIntZ
                );


                    var item = new ListViewItem(v.col.X.ToString());
                    item.BackColor = SetColor;
                    item.SubItems.Add(v.col.Y.ToString());
                    item.SubItems.Add(v.col.Z.ToString());
                    item.SubItems.Add(v.col.W.ToString());
                    listView1.Items.Add(item);
                }
            }

          
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
         
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (AttName == "_c0")
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    ColorDialog clr = new ColorDialog();
                    if (clr.ShowDialog() == DialogResult.OK)
                    {
                        for (int i = 0; i < listView1.SelectedItems.Count; i++)
                        {
                            listView1.SelectedItems[i].BackColor = clr.Color;

                            
                            float X = (float)clr.Color.R / 255;
                            float Y = (float)clr.Color.G / 255;
                            float Z = (float)clr.Color.B / 255;
                            float W = (float)clr.Color.A / 255;

                            int indx = listView1.SelectedIndices[i];
                            listView1.SelectedItems[0].Text = X.ToString();
                            listView1.SelectedItems[0].SubItems[0].Text = Y.ToString();
                            listView1.SelectedItems[0].SubItems[1].Text = Z.ToString();
                            listView1.SelectedItems[0].SubItems[2].Text = W.ToString();

                            msh.vertices[indx].col = new Vector4(X, Y, Z, W);
                        }
                    }
                }
            }
        }
    }
}
