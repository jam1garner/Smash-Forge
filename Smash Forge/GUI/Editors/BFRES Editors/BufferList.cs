using System;
using System.Drawing;
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
        public string attName;
        public BFRES.Mesh msh;
        public BFRES.FMDL_Model model;

        public void SetFaceBufferList()
        {

        }
        public void SetVertexBufferList(BFRES.Mesh m, string attName, BFRES.FMDL_Model mdl)
        {
            listView1.Items.Clear();

            //Set public variables so i can use it for other things
            this.attName = attName;
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
                    float x = v.boneWeights.Count > 0 ? v.boneWeights[0] : 0;
                    float y = v.boneWeights.Count > 1 ? v.boneWeights[1] : 0;
                    float z = v.boneWeights.Count > 2 ? v.boneWeights[2] : 0;
                    float w = v.boneWeights.Count > 3 ? v.boneWeights[3] : 0;

                    var item = new ListViewItem(x.ToString());
                    item.SubItems.Add(y.ToString());
                    item.SubItems.Add(z.ToString());
                    item.SubItems.Add(w.ToString());
                    listView1.Items.Add(item);
                }
                if (attName == "_i0")
                {
                    string x = "";
                    string y = "";
                    string z = "";
                    string w = "";

                    float iX = v.boneIds.Count > 0 ? v.boneIds[0] : 0;
                    float iY = v.boneIds.Count > 1 ? v.boneIds[1] : 0;
                    float iZ = v.boneIds.Count > 2 ? v.boneIds[2] : 0;
                    float iW = v.boneIds.Count > 3 ? v.boneIds[3] : 0;

                    //Set the bone in the skeleton of it's fmdl. 
                    try
                    {
                        if (iX != 0)
                            x = model.skeleton.bones[model.Node_Array[(int)iX]].Text;
                        if (iY != 0)
                            y = model.skeleton.bones[model.Node_Array[(int)iY]].Text;
                        if (iZ != 0)
                            z = model.skeleton.bones[model.Node_Array[(int)iZ]].Text;
                        if (iW != 0)
                            w = model.skeleton.bones[model.Node_Array[(int)iW]].Text;
                    }
                    catch
                    {
                        x = iX.ToString();
                        y = iY.ToString();
                        z = iZ.ToString();
                        w = iW.ToString();
                    }


                    var item = new ListViewItem(x);
                    item.SubItems.Add(y);
                    item.SubItems.Add(z);
                    item.SubItems.Add(w);
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
                    Color setColor = Color.White;

                    int someIntX = (int)Math.Ceiling(v.col.X * 255);
                    int someIntY = (int)Math.Ceiling(v.col.Y * 255);
                    int someIntZ = (int)Math.Ceiling(v.col.Z * 255);
                    int someIntW = (int)Math.Ceiling(v.col.W * 255);

                    setColor = Color.FromArgb(
                255,
                someIntX,
                someIntY,
                someIntZ
                );


                    var item = new ListViewItem(v.col.X.ToString());
                    item.BackColor = setColor;
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
            if (attName == "_c0")
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    ColorDialog clr = new ColorDialog();
                    if (clr.ShowDialog() == DialogResult.OK)
                    {
                        for (int i = 0; i < listView1.SelectedItems.Count; i++)
                        {
                            listView1.SelectedItems[i].BackColor = clr.Color;

                            
                            float x = (float)clr.Color.R / 255;
                            float y = (float)clr.Color.G / 255;
                            float z = (float)clr.Color.B / 255;
                            float w = (float)clr.Color.A / 255;

                            int indx = listView1.SelectedIndices[i];
                            listView1.SelectedItems[0].Text = x.ToString();
                            listView1.SelectedItems[0].SubItems[0].Text = y.ToString();
                            listView1.SelectedItems[0].SubItems[1].Text = z.ToString();
                            listView1.SelectedItems[0].SubItems[2].Text = w.ToString();

                            msh.vertices[indx].col = new Vector4(x, y, z, w);
                        }
                    }
                }
            }
        }
    }
}
