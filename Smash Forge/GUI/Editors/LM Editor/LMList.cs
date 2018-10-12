using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Smash_Forge
{
    public partial class LMList : DockContent
    {
        public Lumen Lumen;
        
        public LMList(string fileName = null)
        {
            InitializeComponent();
            if (fileName != null)
            {
                Lumen = new Lumen(fileName);
            }
            treeView1.Nodes.Add(symbolNode);
            treeView1.Nodes.Add(colorNode);
            treeView1.Nodes.Add(transformNode);
            treeView1.Nodes.Add(positionsNode);
            treeView1.Nodes.Add(boundsNode);
            treeView1.Nodes.Add(atlasNode);


            fillList();

            treeView1.NodeMouseClick += (sender, args) => treeView1.SelectedNode = args.Node;
        }
        public TreeNode symbolNode = new TreeNode("Symbols");
        public TreeNode colorNode = new TreeNode("Colors");
        public TreeNode transformNode = new TreeNode("Transforms");
        public TreeNode positionsNode = new TreeNode("Positions");
        public TreeNode boundsNode = new TreeNode("Bounds");
        public TreeNode atlasNode = new TreeNode("Atlases");


        public void fillList()
        {
            symbolNode.Nodes.Clear();
            colorNode.Nodes.Clear();
            transformNode.Nodes.Clear();
            positionsNode.Nodes.Clear();

            if(Lumen != null)
            {
                foreach (string s in Lumen.Strings)
                {
                    symbolNode.Nodes.Add(new TreeNode(s) { Text = s });
                }
                foreach (var x in Lumen.Colors)
                {
                    colorNode.Nodes.Add(new TreeNode((x * 255).ToString()));
                }
                for (int i = 0; i < Lumen.Transforms.Count; i++)
                {
                    transformNode.Nodes.Add(new TreeNode("Transform 0x" + i.ToString("X")));
                }
                for (int i = 0; i < Lumen.Positions.Count; i++)
                {
                    positionsNode.Nodes.Add(new TreeNode("Position 0x" + i.ToString("X")));
                }
                for (int i = 0; i < Lumen.Bounds.Count; i++)
                {
                    boundsNode.Nodes.Add(new TreeNode("Bounds 0x" + i.ToString("X")));
                }
                for (int i = 0; i < Lumen.Atlases.Count; i++)
                {
                    atlasNode.Nodes.Add(new TreeNode("Atlas 0x" + i.ToString("X")));
                }
            }
        }
        public DataTable tbl = new DataTable();
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("Name") { ReadOnly = true });
            tbl.Columns.Add("Value");
            dataGridView1.DataSource = tbl;
            tbl.Rows.Clear();
            try
            {
                if (e.Node.Parent.Text == "Colors")
                {
                    tbl.Rows.Add("Red", Lumen.Colors[e.Node.Index].X * 255);
                    tbl.Rows.Add("Green", Lumen.Colors[e.Node.Index].Y * 255);
                    tbl.Rows.Add("Blue", Lumen.Colors[e.Node.Index].Z * 255);
                    tbl.Rows.Add("Alpha", Lumen.Colors[e.Node.Index].W * 255);
                }
                else if (e.Node.Parent.Text == "Transforms")
                {
                    tbl.Rows.Add("X-Scale", Lumen.Transforms[e.Node.Index].Row0[0]);
                    tbl.Rows.Add("X-Skew", Lumen.Transforms[e.Node.Index].Row1[0]);
                    tbl.Rows.Add("X-Transform", Lumen.Transforms[e.Node.Index].Row3[0]);
                    tbl.Rows.Add("Y-Scale", Lumen.Transforms[e.Node.Index].Row1[1]);
                    tbl.Rows.Add("Y-Skew", Lumen.Transforms[e.Node.Index].Row0[1]);
                    tbl.Rows.Add("Y-Transform", Lumen.Transforms[e.Node.Index].Row3[1]);
                }
                else if (e.Node.Parent.Text == "Positions")
                {
                    tbl.Rows.Add("X", Lumen.Positions[e.Node.Index][0]);
                    tbl.Rows.Add("Y", Lumen.Positions[e.Node.Index][1]);
                }
                else if (e.Node.Parent.Text == "Bounds")
                {
                    tbl.Rows.Add("Top", Lumen.Bounds[e.Node.Index].TopLeft.X);
                    tbl.Rows.Add("Left", Lumen.Bounds[e.Node.Index].TopLeft.Y);
                    tbl.Rows.Add("Bottom", Lumen.Bounds[e.Node.Index].BottomRight.X);
                    tbl.Rows.Add("Right", Lumen.Bounds[e.Node.Index].BottomRight.Y);
                }
                else if (e.Node.Parent.Text == "Atlases")
                {
                    tbl.Rows.Add("Texture ID", Lumen.Atlases[e.Node.Index].id);
                    tbl.Rows.Add("Name ID", Lumen.Atlases[e.Node.Index].nameId);
                    tbl.Rows.Add("Width", Lumen.Atlases[e.Node.Index].width);
                    tbl.Rows.Add("Height", Lumen.Atlases[e.Node.Index].height);
                }
                else if (e.Node.Parent.Text == "Unk")
                {

                }
            }
            catch{}
        }
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int indexNum = treeView1.SelectedNode.Index;
            try
            {
                if (treeView1.SelectedNode.Parent.Text == "Colors")
                {
                    Lumen.ReplaceColor(new OpenTK.Vector4(float.Parse(tbl.Rows[0][1].ToString()) / 255f, float.Parse(tbl.Rows[1][1].ToString()) / 255f, float.Parse(tbl.Rows[2][1].ToString()) / 255f, float.Parse(tbl.Rows[3][1].ToString()) / 255f), indexNum);
                    treeView1.SelectedNode.Text = (Lumen.Colors[indexNum] * 255).ToString();
                }
                else if (treeView1.SelectedNode.Parent.Text == "Transforms")
                {
                    Lumen.ReplaceTransform(new OpenTK.Matrix4(
                        float.Parse(tbl.Rows[0][1].ToString()), float.Parse(tbl.Rows[3][1].ToString()), 0, 0,
                        float.Parse(tbl.Rows[1][1].ToString()), float.Parse(tbl.Rows[4][1].ToString()), 0, 0,
                        0, 0, 1, 0,
                        float.Parse(tbl.Rows[2][1].ToString()), float.Parse(tbl.Rows[5][1].ToString()), 0, 1), indexNum
                        );
                }
                else if (treeView1.SelectedNode.Parent.Text == "Positions")
                {
                    Lumen.ReplacePosition(new OpenTK.Vector2(float.Parse(tbl.Rows[0][1].ToString()), float.Parse(tbl.Rows[1][1].ToString())), indexNum);
                }
                else if (treeView1.SelectedNode.Parent.Text == "Bounds")
                {
                    Lumen.ReplaceBound(new Lumen.Rect(float.Parse(tbl.Rows[0][1].ToString()), float.Parse(tbl.Rows[1][1].ToString()), float.Parse(tbl.Rows[2][1].ToString()), float.Parse(tbl.Rows[3][1].ToString())), indexNum);
                }
                else if (treeView1.SelectedNode.Parent.Text == "Atlases")
                {
                    Lumen.TextureAtlas atlas = new Lumen.TextureAtlas();

                    atlas.id = int.Parse(tbl.Rows[0][1].ToString());
                    atlas.nameId = int.Parse(tbl.Rows[1][1].ToString());
                    atlas.width = float.Parse(tbl.Rows[2][1].ToString());
                    atlas.height = float.Parse(tbl.Rows[3][1].ToString());
 
                    Lumen.ReplaceAtlas(atlas, indexNum);
                }
                else if (treeView1.SelectedNode.Parent.Text == "unk")
                {

                }
            }
            catch
            {
                MessageBox.Show("Incorrect format", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
