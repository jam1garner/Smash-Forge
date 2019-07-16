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
    public partial class BfresMeshEditor : Form
    {
        public BFRES.Mesh Mesh = null;
        public BFRES.FMDL_Model Model = null;
        public BFRES bfres = null;

        public BfresMeshEditor(BFRES.Mesh p, BFRES.FMDL_Model mdl, BFRES b)
        {
            InitializeComponent();

            button1.Enabled = false;
            textBox1.Text = p.Text;
            VertCountlabel1.Text = VertCountlabel1.Text + " " + p.vertices.Count.ToString();
            PolyCountlabel3.Text = PolyCountlabel3.Text + " " + p.lodMeshes[p.DisplayLODIndex].displayFaceSize.ToString();
            skinCountLabel.Text = skinCountLabel.Text + " " + p.VertexSkinCount.ToString();
            label3.Text = label3.Text + " " + mdl.skeleton.bones[p.boneIndx].ToString();

            Mesh = p;
            Model = mdl;
            bfres = b;


            foreach (var lod in p.lodMeshes)
            {
                LODcomboBox3.Items.Add(lod);
            }
            LODcomboBox3.SelectedIndex = p.DisplayLODIndex;

            List<string> attributes = new List<string>();
            foreach (Syroot.NintenTools.NSW.Bfres.GFX.AttribFormat attr in Enum.GetValues(typeof(Syroot.NintenTools.NSW.Bfres.GFX.AttribFormat)))
            {
                attributes.Add(attr.ToString());
            }
            attributes.Sort();
            foreach (string att in attributes)
            {
                comboBox2.Items.Add(att);
            }


            int Height = 2;
            foreach (BFRES.Mesh.VertexAttribute att in p.vertexAttributes)
            {
                comboBox1.Items.Add(att);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void LODcomboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            BFRES.Mesh.LOD_Mesh msh = (BFRES.Mesh.LOD_Mesh)LODcomboBox3.SelectedItem;
            Mesh.DisplayLODIndex = LODcomboBox3.SelectedIndex;

            bfres.UpdateRenderMeshes();

            PolyCountlabel3.Text = "Poly Count " + Mesh.lodMeshes[Mesh.DisplayLODIndex].displayFaceSize.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
      //      if (textBox1.TextLength > 0)
        //        Mesh.Text = textBox1.Text;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MeshBoneList mb = new MeshBoneList();
            mb.SetMeshBoneList(((BFRES.FMDL_Model)Mesh.Parent), Mesh); //Fmdl stores node array
            mb.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BFRES.Mesh.VertexAttribute attrb = (BFRES.Mesh.VertexAttribute)comboBox1.SelectedItem;
            BufferList buff = new BufferList();
            buff.SetVertexBufferList(Mesh, attrb.Name, Model);
            buff.Show();
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            BFRES.Mesh.VertexAttribute attrb = (BFRES.Mesh.VertexAttribute)comboBox1.SelectedItem;
            button1.Enabled = true;
            comboBox2.SelectedItem = attrb.Format.ToString();
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

  /*      private void button6_Click(object sender, EventArgs e)
        {
            foreach (Bone bn in Model.skeleton.bones)
            {
                BFRES.DefaultBonePos defbn = new BFRES.DefaultBonePos();
                defbn.pos = bn.pos;
                defbn.rot = new OpenTK.Vector3(bn.rotation[0], bn.rotation[1], bn.rotation[2]);
                defbn.scale = bn.sca;
                defbn.Name = bn.Text;          
            }

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter =
                      "Supported Formats|*.smd;|" +
                      "All files(*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                SMD.toBFRES(ofd.FileName);
                foreach (var bone in BFRES.HackyBoneDiffList)
                {
                    Console.WriteLine(bone.pos);
                    Console.WriteLine(bone.rot);
                    Console.WriteLine(bone.scale);
                }
            }
        }*/
    }
}
