using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge.GUI.Editors
{
    public partial class BFRES_MeshEditor : Form
    {
        public BFRES_MeshEditor(BFRES.Mesh p)
        {
            InitializeComponent();

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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            BFRES.Mesh.VertexAttribute attrb = (BFRES.Mesh.VertexAttribute)comboBox1.SelectedItem;
 
            comboBox2.SelectedItem = attrb.Format.ToString();

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
