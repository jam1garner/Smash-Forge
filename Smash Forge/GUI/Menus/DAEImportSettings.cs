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
    public partial class DAEImportSettings : Form
    {

        public string fname;

        public static int Running = 0;
        public static int Opened = 1;
        public static int Cancelled = 2;

        public int exitStatus = 0; //0 - not done, 1 - one is selected, 2 - cancelled

        public Dictionary<string, int> BoneTypes = new Dictionary<string, int>()
        {
            { "No Bones", 0x00},
            { "Bone Weight Exact(larger filesize)", 0x10},
            { "Bone Weight Approx(smaller filesize)", 0x40}
        };

        public Dictionary<string, int> VertTypes = new Dictionary<string, int>()
        {
            { "No Normals", 0x0},
            { "Normals as Float", 0x1},
            { "Normals as Half Float", 0x6}
        };

        public DAEImportSettings()
        {
            InitializeComponent();
            populate();
        }
        
        public void populate()
        {
            foreach (string key in VertTypes.Keys)
                comboBox1.Items.Add(key);

            foreach (string key in BoneTypes.Keys)
                comboBox2.Items.Add(key);

            comboBox1.SelectedIndex = 2;
            comboBox2.SelectedIndex = 1;
        }

        public void Apply(NUD nud)
        {
            if (checkBox1.Checked)
            {
                foreach(NUD.Mesh mesh in nud.mesh)
                {
                    foreach(NUD.Polygon poly in mesh.polygons)
                    {
                        //poly.vertSize = (BoneTypes[comboBox2.SelectedText]) | (VertTypes[comboBox1.SelectedText]);
                        foreach(NUD.Vertex v in poly.vertices)
                        {
                            for(int i = 0; i < v.tx.Count; i++)
                                v.tx[i] = new Vector2(v.tx[i].X, 1 - v.tx[i].Y);
                        }
                    }
                }
            }

            if (checkBox2.Checked)
            {
                foreach (NUD.Mesh mesh in nud.mesh)
                {
                    if(mesh.name.Length > 4)
                        mesh.name = mesh.name.Substring(5, mesh.name.Length - 5);
                }
            }

            nud.PreRender();
        }

        private void closeButton(object sender, EventArgs e)
        {
            exitStatus = Cancelled;
            Close();
        }

        private void MaterialSelector_Load(object sender, EventArgs e)
        {
            populate();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            exitStatus = Opened;
            Close();
        }
    }
}
