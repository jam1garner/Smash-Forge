using SmashForge.Gui.Menus;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace SmashForge
{
    public partial class LmList : EditorBase
    {
        public Lumen lumen;
        public NUT nut;

        public LmList(string fileName = null)
        {
            InitializeComponent();
            if (fileName != null)
            {
                lumen = new Lumen(fileName);
            }
            treeView1.Nodes.Add(symbolNode);
            treeView1.Nodes.Add(colorNode);
            treeView1.Nodes.Add(transformNode);
            treeView1.Nodes.Add(positionsNode);
            treeView1.Nodes.Add(boundsNode);
            treeView1.Nodes.Add(atlasNode);
            treeView1.Nodes.Add(shapesNode);
            treeView1.Nodes.Add(spritesNode);
            treeView1.Nodes.Add(textsNode);


            FillList();

            treeView1.NodeMouseClick += (sender, args) => treeView1.SelectedNode = args.Node;
        }
        public TreeNode symbolNode = new TreeNode("Symbols");
        public TreeNode colorNode = new TreeNode("Colors");
        public TreeNode transformNode = new TreeNode("Transforms");
        public TreeNode positionsNode = new TreeNode("Positions");
        public TreeNode boundsNode = new TreeNode("Bounds");
        public TreeNode atlasNode = new TreeNode("Atlases");
        public TreeNode shapesNode = new TreeNode("Shapes");
        public TreeNode spritesNode = new TreeNode("Sprites");
        public TreeNode textsNode = new TreeNode("Texts");

        public void FillList()
        {
            symbolNode.Nodes.Clear();
            colorNode.Nodes.Clear();
            transformNode.Nodes.Clear();
            positionsNode.Nodes.Clear();
            boundsNode.Nodes.Clear();
            atlasNode.Nodes.Clear();
            shapesNode.Nodes.Clear();
            spritesNode.Nodes.Clear();
            textsNode.Nodes.Clear();


            if (lumen != null)
            {
                foreach (string s in lumen.Strings)
                {
                    symbolNode.Nodes.Add(new TreeNode(s) { Text = s });
                }
                foreach (var x in lumen.Colors)
                {
                    TreeNode ColorText = new TreeNode((x * 255).ToString());
                    ColorText.BackColor = System.Drawing.Color.FromArgb((int)(x.W * 255), (int)(x.X * 255), (int)(x.Y * 255), (int)(x.Z * 255));
                    ColorText.ForeColor = System.Drawing.Color.FromArgb(ColorText.BackColor.ToArgb() ^ 0xffffff);
                    colorNode.Nodes.Add(ColorText);
                }
                for (int i = 0; i < lumen.Transforms.Count; i++)
                {
                    transformNode.Nodes.Add(new TreeNode("Transform 0x" + i.ToString("X")));
                }
                for (int i = 0; i < lumen.Positions.Count; i++)
                {
                    positionsNode.Nodes.Add(new TreeNode("Position 0x" + i.ToString("X")));
                }
                for (int i = 0; i < lumen.Bounds.Count; i++)
                {
                    boundsNode.Nodes.Add(new TreeNode("Bounds 0x" + i.ToString("X")));
                }
                for (int i = 0; i < lumen.Atlases.Count; i++)
                {
                    atlasNode.Nodes.Add(new TreeNode("Atlas 0x" + i.ToString("X")));
                }
                for (int i = 0; i < lumen.Shapes.Count; i++)
                {
                    TreeNode newNode = new TreeNode("Shape 0x" + i.ToString("X"));
                    for (int j = 0; j < lumen.Shapes[i].numGraphics; j++)
                    {
                        newNode.Nodes.Add("Graphic 0x" + j.ToString("X"));
                    }
                    shapesNode.Nodes.Add(newNode);
                }
                for (int i = 0; i < lumen.Sprites.Count; i++)
                {
                    TreeNode spriteNode = new TreeNode("Sprite 0x" + i.ToString("X"));
                    TreeNode labelNode = new TreeNode("Frame Labels");
                    TreeNode showNode = new TreeNode("Show Frames");
                    TreeNode keyNode = new TreeNode("Key Frames");

                    for (int j = 0; j < lumen.Sprites[i].labels.Count; j++)
                    {
                        labelNode.Nodes.Add("Label 0x" + j.ToString("X"));
                    }

                    for (int j = 0; j < lumen.Sprites[i].Frames.Count; j++)
                    {
                        showNode.Nodes.Add("Show Frame 0x" + j.ToString("X"));
                    }

                    for (int j = 0; j < lumen.Sprites[i].Keyframes.Count; j++)
                    {
                        keyNode.Nodes.Add("Key Frame 0x" + j.ToString("X"));
                    }

                    spriteNode.Nodes.Add(labelNode);
                    spriteNode.Nodes.Add(showNode);
                    spriteNode.Nodes.Add(keyNode);

                    spritesNode.Nodes.Add(spriteNode);
                }
                for (int i = 0; i < lumen.Texts.Count; i++)
                {
                    textsNode.Nodes.Add("Text 0x" + i.ToString("X"));
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
                if (e.Node.Parent.Text == "Symbols")
                {
                    tbl.Rows.Add("String", lumen.Strings[e.Node.Index]);
                }
                else if (e.Node.Parent.Text == "Colors")
                {
                    tbl.Rows.Add("Red", lumen.Colors[e.Node.Index].X * 255);
                    tbl.Rows.Add("Green", lumen.Colors[e.Node.Index].Y * 255);
                    tbl.Rows.Add("Blue", lumen.Colors[e.Node.Index].Z * 255);
                    tbl.Rows.Add("Alpha", lumen.Colors[e.Node.Index].W * 255);
                }
                else if (e.Node.Parent.Text == "Transforms")
                {
                    tbl.Rows.Add("X-Scale", lumen.Transforms[e.Node.Index].Row0[0]);
                    tbl.Rows.Add("X-Skew", lumen.Transforms[e.Node.Index].Row1[0]);
                    tbl.Rows.Add("X-Transform", lumen.Transforms[e.Node.Index].Row3[0]);
                    tbl.Rows.Add("Y-Scale", lumen.Transforms[e.Node.Index].Row1[1]);
                    tbl.Rows.Add("Y-Skew", lumen.Transforms[e.Node.Index].Row0[1]);
                    tbl.Rows.Add("Y-Transform", lumen.Transforms[e.Node.Index].Row3[1]);
                }
                else if (e.Node.Parent.Text == "Positions")
                {
                    tbl.Rows.Add("X", lumen.Positions[e.Node.Index][0]);
                    tbl.Rows.Add("Y", lumen.Positions[e.Node.Index][1]);
                }
                else if (e.Node.Parent.Text == "Bounds")
                {
                    tbl.Rows.Add("Top", lumen.Bounds[e.Node.Index].TopLeft.X);
                    tbl.Rows.Add("Left", lumen.Bounds[e.Node.Index].TopLeft.Y);
                    tbl.Rows.Add("Bottom", lumen.Bounds[e.Node.Index].BottomRight.X);
                    tbl.Rows.Add("Right", lumen.Bounds[e.Node.Index].BottomRight.Y);
                }
                else if (e.Node.Parent.Text == "Atlases")
                {
                    tbl.Rows.Add("Texture ID", lumen.Atlases[e.Node.Index].id);
                    tbl.Rows.Add("Name ID", lumen.Atlases[e.Node.Index].nameId);
                    tbl.Rows.Add("Width", lumen.Atlases[e.Node.Index].width);
                    tbl.Rows.Add("Height", lumen.Atlases[e.Node.Index].height);
                }
                else if (e.Node.Text.StartsWith("Graphic 0x"))
                {
                    string filename = (Path.GetDirectoryName(lumen.Filename) + "\\img-" + (e.Node.Index.ToString("00000")) + ".nut");
                    nut = new NUT(filename);
                    nut.RefreshGlTexturesByHashId();
                    LmuvViewer uvViewer = new LmuvViewer(lumen, nut, e.Node.Parent.Index, e.Node.Index);
                    uvViewer.Show();
                }
                else if (e.Node.Parent.Text == "Texts")
                {
                    tbl.Rows.Add("Character ID", lumen.Texts[e.Node.Index].CharacterId);
                    tbl.Rows.Add("unk 1", lumen.Texts[e.Node.Index].unk1);
                    tbl.Rows.Add("Placeholder Text ID", lumen.Texts[e.Node.Index].placeholderTextId);
                    tbl.Rows.Add("unk 2", lumen.Texts[e.Node.Index].unk2);
                    tbl.Rows.Add("Stroke Color ID", lumen.Texts[e.Node.Index].strokeColorId);
                    tbl.Rows.Add("unk 3", lumen.Texts[e.Node.Index].unk3);
                    tbl.Rows.Add("unk 4", lumen.Texts[e.Node.Index].unk3);
                    tbl.Rows.Add("unk 5", lumen.Texts[e.Node.Index].unk3);
                    tbl.Rows.Add("Text Alignment", lumen.Texts[e.Node.Index].alignment);
                    tbl.Rows.Add("unk 6", lumen.Texts[e.Node.Index].unk3);
                    tbl.Rows.Add("unk 7", lumen.Texts[e.Node.Index].unk3);
                    tbl.Rows.Add("unk 8", lumen.Texts[e.Node.Index].unk3);
                    tbl.Rows.Add("Size", lumen.Texts[e.Node.Index].size);
                    tbl.Rows.Add("unk 9", lumen.Texts[e.Node.Index].unk3);
                    tbl.Rows.Add("unk 10", lumen.Texts[e.Node.Index].unk10);
                    tbl.Rows.Add("unk 11", lumen.Texts[e.Node.Index].unk11);
                    tbl.Rows.Add("unk 12", lumen.Texts[e.Node.Index].unk12);
                }
                else if (e.Node.Parent.Text == "Unk")
                {

                }
            }
            catch { }
        }
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Edited = true;
            int indexNum = treeView1.SelectedNode.Index;
            try
            {
                if (treeView1.SelectedNode.Parent.Text == "Symbols")
                {
                    lumen.Strings[indexNum] = tbl.Rows[0][1].ToString();
                }
                else if (treeView1.SelectedNode.Parent.Text == "Colors")
                {
                    lumen.ReplaceColor(new OpenTK.Vector4(float.Parse(tbl.Rows[0][1].ToString()) / 255f, float.Parse(tbl.Rows[1][1].ToString()) / 255f, float.Parse(tbl.Rows[2][1].ToString()) / 255f, float.Parse(tbl.Rows[3][1].ToString()) / 255f), indexNum);
                    treeView1.SelectedNode.Text = (lumen.Colors[indexNum] * 255).ToString();
                }
                else if (treeView1.SelectedNode.Parent.Text == "Transforms")
                {
                    lumen.ReplaceTransform(new OpenTK.Matrix4(
                        float.Parse(tbl.Rows[0][1].ToString()), float.Parse(tbl.Rows[3][1].ToString()), 0, 0,
                        float.Parse(tbl.Rows[1][1].ToString()), float.Parse(tbl.Rows[4][1].ToString()), 0, 0,
                        0, 0, 1, 0,
                        float.Parse(tbl.Rows[2][1].ToString()), float.Parse(tbl.Rows[5][1].ToString()), 0, 1), indexNum
                        );
                }
                else if (treeView1.SelectedNode.Parent.Text == "Positions")
                {
                    lumen.ReplacePosition(new OpenTK.Vector2(float.Parse(tbl.Rows[0][1].ToString()), float.Parse(tbl.Rows[1][1].ToString())), indexNum);
                }
                else if (treeView1.SelectedNode.Parent.Text == "Bounds")
                {
                    lumen.ReplaceBound(new Lumen.Rect(float.Parse(tbl.Rows[0][1].ToString()), float.Parse(tbl.Rows[1][1].ToString()), float.Parse(tbl.Rows[2][1].ToString()), float.Parse(tbl.Rows[3][1].ToString())), indexNum);
                }
                else if (treeView1.SelectedNode.Parent.Text == "Atlases")
                {
                    Lumen.TextureAtlas atlas = new Lumen.TextureAtlas();

                    atlas.id = int.Parse(tbl.Rows[0][1].ToString());
                    atlas.nameId = int.Parse(tbl.Rows[1][1].ToString());
                    atlas.width = float.Parse(tbl.Rows[2][1].ToString());
                    atlas.height = float.Parse(tbl.Rows[3][1].ToString());

                    lumen.ReplaceAtlas(atlas, indexNum);
                }
                else if (treeView1.SelectedNode.Parent.Text == "Texts")
                {
                    Lumen.DynamicText text = new Lumen.DynamicText();

                    text.CharacterId = int.Parse(tbl.Rows[0][1].ToString());
                    text.unk1 = int.Parse(tbl.Rows[1][1].ToString());
                    text.placeholderTextId = int.Parse(tbl.Rows[2][1].ToString());
                    text.unk2 = int.Parse(tbl.Rows[3][1].ToString());
                    text.strokeColorId = int.Parse(tbl.Rows[4][1].ToString());
                    text.unk3 = int.Parse(tbl.Rows[5][1].ToString());
                    text.unk4 = int.Parse(tbl.Rows[6][1].ToString());
                    text.unk5 = int.Parse(tbl.Rows[7][1].ToString());
                    text.alignment = (Lumen.TextAlignment)int.Parse(tbl.Rows[8][1].ToString());
                    text.unk6 = short.Parse(tbl.Rows[9][1].ToString());
                    text.unk7 = int.Parse(tbl.Rows[10][1].ToString());
                    text.unk8 = int.Parse(tbl.Rows[11][1].ToString());
                    text.size = int.Parse(tbl.Rows[12][1].ToString());
                    text.unk9 = int.Parse(tbl.Rows[13][1].ToString());
                    text.unk10 = int.Parse(tbl.Rows[14][1].ToString());
                    text.unk11 = int.Parse(tbl.Rows[15][1].ToString());
                    text.unk12 = int.Parse(tbl.Rows[16][1].ToString());
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
        public override void Save()
        {
            FileOutput o = new FileOutput();
            byte[] n = lumen.Rebuild();
            o.WriteBytes(n);
            o.Save(lumen.Filename);
            Edited = false;
        }
    }
}
