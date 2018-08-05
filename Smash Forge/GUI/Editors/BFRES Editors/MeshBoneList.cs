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

namespace Smash_Forge
{
    public partial class MeshBoneList : Form
    {
        public MeshBoneList()
        {
            InitializeComponent();
        }
        public BFRES.Mesh msh;
        public BFRES.FMDL_Model fmdl;
        public BFRES bfres;
        public bool isSingleBinding = false;

        public void SetMeshBoneList(BFRES.FMDL_Model mdl, BFRES.Mesh m, bool sb = false)
        {
            listView1.Items.Clear();

            //This list is used for both viewing and also single binding for certain instances
            isSingleBinding = sb;
            msh = m;
            fmdl = mdl;

            foreach (var bn in m.BoneIndexList)
            {
                listView1.Items.Add(bn.Value.ToString()).SubItems.Add(bn.Key);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
         
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (isSingleBinding == true)
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    int bi = 0;
                    foreach (int indx in fmdl.Node_Array)
                    {
                        if (fmdl.skeleton.bones[indx].Text == listView1.SelectedItems[0].SubItems[1].Text)
                        {
                            foreach (BFRES.Vertex v in msh.vertices)
                            {
                                v.boneIds.Add(bi);
                                v.boneWeights.Add(1);
                            }
                            Close();
                        }
                        bi++;
                    }
                }
            }
        }
    }
}
