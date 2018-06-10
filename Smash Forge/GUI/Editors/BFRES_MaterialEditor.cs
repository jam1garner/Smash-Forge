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
    public partial class BFRES_MaterialEditor : Form
    {
        public BFRES.Mesh poly;
        public BFRES.MaterialData mat;

        public BFRES_MaterialEditor(BFRES.Mesh p)
        {
            InitializeComponent();
            this.poly = p;

            Console.WriteLine("Material Editor");
            Console.WriteLine(p.Text);

            mat = p.material;
            textBox1.Text = mat.Name;

 
        }

        private void RenderTexture(bool justRenderAlpha = false)
        {
            if (!tabControl1.SelectedTab.Text.Equals("Textures"))
                return;

            // Get the selected NUT texture.
            NutTexture nutTexture = null;
            int displayTexture = 0;
            if (listBox1.SelectedIndices.Count > 0)
            {
                int hash = poly.texHashs[listBox1.SelectedIndices[0]];

                foreach (NUT n in Runtime.TextureContainers)
                {
                    if (n.draw.ContainsKey(hash))
                    {
                        n.getTextureByID(hash, out nutTexture);
                        displayTexture = n.draw[hash];
                        break;
                    }
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            poly.Text = textBox1.Text;
        }

        private void BFRES_MaterialEditor_Load(object sender, EventArgs e)
        {

        }

        private void glControl1_Load(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabTextureMaps_Click(object sender, EventArgs e)
        {

        }
    }
}
