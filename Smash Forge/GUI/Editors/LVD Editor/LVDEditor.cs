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
using OpenTK;

namespace Smash_Forge
{
    public partial class LVDEditor : DockContent
    {
        public class StringWrapper
        {
            public char[] data;
        } 

        public LVDEditor()
        {
            InitializeComponent();
        }

        public LVDEntry currentEntry;
        private Vector2D currentVert;
        private Vector2D currentNormal;
        private CollisionMat currentMat;
        private TreeNode currentTreeNode;
        private Point currentPoint;
        private Bounds currentBounds;
        private Section currentItemSection;
        private GeneralPoint currentGeneralPoint;
        private GeneralRect currentGeneralRect;
        private GeneralPath currentGeneralPath;

        enum materialTypes : byte
        {
            Iron = 0x06,
            Snow = 0x0d,
            Ice = 0x0c,
            Wood = 0x04,
            LargeBubbles = 0x15,
            Hurt = 0x1f,
            Brick = 0x00,
            Stone2 = 0x18,
            Metal2 = 0x1b,
            Water = 0x0a,
            Bubbles = 0x0b,
            Clouds = 0x16,
            Ice2 = 0x10,
            NebuIron = 0x05,
            Danbouru = 0x11,
            Rock = 0x01,
            Gamewatch = 0x0f,
            Grass = 0x02,
            SnowIce = 0x0e,
            Fence = 0x08,
            Soil = 0x03,
            Sand = 0x1c,
            MasterFortress = 0x09,
            Carpet = 0x07
        }

        public void open(LVDEntry entry, TreeNode entryTree)
        {
            currentTreeNode = entryTree;
            currentEntry = entry;
            collisionGroup.Visible = false;
            pointGroup.Visible = false;
            boundGroup.Visible = false;
            itemSpawnerGroup.Visible = false;
            generalPointShapeBox.Visible = false;
            rectangleGroup.Visible = false;
            pathGroup.Visible = false;
            name.Text = currentEntry.name;
            subname.Text = currentEntry.subname;
            if (entry is Collision)
            {
                Collision col = (Collision)entry;
                collisionGroup.Visible = true;
                xStart.Value = (decimal)col.startPos[0];
                yStart.Value = (decimal)col.startPos[1];
                zStart.Value = (decimal)col.startPos[2];
                flag1.Checked = col.useStartPos;
                flag2.Checked = col.flag2;
                flag3.Checked = col.flag3;
                flag4.Checked = col.flag4;
                vertices.Nodes.Clear();
                string boneNameRigging = "";
                foreach (char b in col.unk4)
                    if (b != (char)0)
                        boneNameRigging += b;
                if (boneNameRigging.Length == 0)
                    boneNameRigging = "None";
                button3.Text = boneNameRigging; 
                for (int i = 0; i < col.verts.Count; i++)
                    vertices.Nodes.Add(new TreeNode($"Vertex {i+1} ({col.verts[i].x},{col.verts[i].y})") { Tag = col.verts[i] });
                lines.Nodes.Clear();
                for (int i = 0; i < col.normals.Count; i++)
                {
                    object[] temp = { col.normals[i], col.materials[i] };
                    lines.Nodes.Add(new TreeNode($"Line {i+1}") { Tag = temp });
                }
            }
            else if(entry is Point)
            {
                pointGroup.Visible = true;
                currentPoint = (Point)entry;
                xPoint.Value = (decimal)((Point)entry).x;
                yPoint.Value = (decimal)((Point)entry).y;
            }
            else if(entry is Bounds)
            {
                boundGroup.Visible = true;
                currentBounds = (Bounds)entry;
                topVal.Value = (decimal)currentBounds.top;
                rightVal.Value = (decimal)currentBounds.right;
                leftVal.Value = (decimal)currentBounds.left;
                bottomVal.Value = (decimal)currentBounds.bottom;
            }
            else if(entry is ItemSpawner)
            {
                itemSpawnerGroup.Visible = true;
                ItemSpawner spawner = (ItemSpawner)entry;
                treeView1.Nodes.Clear();
                int i = 1;
                foreach(Section section in spawner.sections)
                    treeView1.Nodes.Add(new TreeNode($"Section {i++}") { Tag = section });

            }
            else if(entry is GeneralPoint)
            {
                generalPointShapeBox.Visible = true;
                GeneralPoint p = (GeneralPoint)entry;
                currentGeneralPoint = p;
                pointShapeX.Value = (Decimal)p.x;
                pointShapeX.Value = (Decimal)p.y;
            }
            else if(entry is GeneralRect)
            {
                rectangleGroup.Visible = true;
                GeneralRect r = (GeneralRect)entry;
                currentGeneralRect = r;
                rectUpperX.Value = (Decimal)r.x2;
                rectUpperY.Value = (Decimal)r.y2;
                rectLowerX.Value = (Decimal)r.x1;
                rectLowerY.Value = (Decimal)r.y1;
            }
            else if(entry is GeneralPath)
            {
                pathGroup.Visible = true;
                GeneralPath p = (GeneralPath)entry;
                currentGeneralPath = p;
                treeViewPath.Nodes.Clear();
                int j = 0;
                foreach(Vector2D v in p.points)
                    treeViewPath.Nodes.Add(new TreeNode($"Point {++j} ({v.x},{v.y})") { Tag = v });
            }
        }

        private void vertices_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currentVert = (Vector2D)e.Node.Tag;
            Runtime.LVDSelection = currentVert;
            MainForm.Instance.viewports[0].timeSinceSelected.Restart();
            xVert.Value = (decimal)currentVert.x;
            yVert.Value = (decimal)currentVert.y;
        }

        private void lines_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currentNormal = (Vector2D)((object[])e.Node.Tag)[0];
            Runtime.LVDSelection = currentNormal;
            MainForm.Instance.viewports[0].timeSinceSelected.Restart();
            currentMat = (CollisionMat)((object[])e.Node.Tag)[1];
            leftLedge.Checked = currentMat.getFlag(6);
            rightLedge.Checked = currentMat.getFlag(7);
            noWallJump.Checked = currentMat.getFlag(5);
            comboBox1.Text = Enum.GetName(typeof(materialTypes), currentMat.getPhysics());
            passthroughAngle.Value = (decimal)(Math.Atan2(currentNormal.y, currentNormal.x) * 180.0 / Math.PI);
        }

        private void flagChange(object sender, EventArgs e)
        {
            if(sender == flag1)
                ((Collision)currentEntry).useStartPos = flag1.Checked;
            if (sender == flag2)
                ((Collision)currentEntry).flag2 = flag2.Checked;
            if (sender == flag3)
                ((Collision)currentEntry).flag3 = flag3.Checked;
            if (sender == flag4)
                ((Collision)currentEntry).flag4 = flag4.Checked;
        }

        private void changePos(object sender, EventArgs e)
        {
            if(sender == xVert)
                currentVert.x = (float)xVert.Value;
            if(sender == yVert)
                currentVert.y = (float)yVert.Value;
            vertices.SelectedNode.Text = $"Vertex {vertices.SelectedNode.Index + 1} ({currentVert.x},{currentVert.y})";
        }

        private void nameChange(object sender, EventArgs e)
        {
            if (currentEntry == null)
                return;
            if (sender == name)
            {
                currentEntry.name = name.Text;
                currentTreeNode.Text = name.Text;
            }
                
            if (sender == subname)
                currentEntry.subname = subname.Text;
        }

        private void passthroughAngle_ValueChanged(object sender, EventArgs e)
        {
            double theta = (double)((NumericUpDown)sender).Value;
            currentNormal.x = (float)Math.Cos(theta * Math.PI / 180.0f);
            currentNormal.y = (float)Math.Sin(theta * Math.PI / 180.0f);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
             currentMat.setPhysics((byte)Enum.Parse(typeof(materialTypes), comboBox1.Text));
        }

        private void changeStart(object sender, EventArgs e)
        {
            if (sender == xStart)
                ((Collision)currentEntry).startPos[0] = (float)xStart.Value;
            if (sender == yStart)
                ((Collision)currentEntry).startPos[1] = (float)yStart.Value;
            if (sender == zStart)
                ((Collision)currentEntry).startPos[2] = (float)zStart.Value;
        }

        private void lineFlagChange(object sender, EventArgs e)
        {
            if (sender == rightLedge)
                currentMat.setFlag(7, ((CheckBox)sender).Checked);
            if (sender == leftLedge)
                currentMat.setFlag(6, ((CheckBox)sender).Checked);
            if (sender == noWallJump)
                currentMat.setFlag(5, ((CheckBox)sender).Checked);
        }

        private void LVDEditor_Load(object sender, EventArgs e)
        {
            collisionGroup.Visible = false;
            pointGroup.Visible = false;
            boundGroup.Visible = false;
        }

        private void pointMoved(object sender, EventArgs e)
        {
            if (sender == xPoint)
                currentPoint.x = (float)xPoint.Value;
            if (sender == yPoint)
                currentPoint.y = (float)yPoint.Value;
            //Console.WriteLine("(" + currentPoint.x + "," + currentPoint.y + ")");
        }

        private void boundsChanged(object sender, EventArgs e)
        {
            if (sender == topVal)
                currentBounds.top = (float)topVal.Value;
            if (sender == bottomVal)
                currentBounds.bottom = (float)bottomVal.Value;
            if (sender == leftVal)
                currentBounds.left = (float)leftVal.Value;
            if (sender == rightVal)
                currentBounds.right = (float)rightVal.Value;
        }

        private void addVert(object sender, EventArgs e)
        {
            if(vertices.SelectedNode == null || (vertices.SelectedNode != null && vertices.SelectedNode.Index == vertices.Nodes.Count - 1))
            {
                Vector2D newVert;
                if (vertices.SelectedNode != null)
                    newVert = new Vector2D() { x = currentVert.x, y = currentVert.y };
                else
                    newVert = new Vector2D();
                ((Collision)currentEntry).verts.Add(newVert);
                vertices.Nodes.Add(new TreeNode("New Vertex") { Tag = newVert });
                if (((Collision)currentEntry).verts.Count > ((Collision)currentEntry).normals.Count + 1)
                {
                    CollisionMat newMat = new CollisionMat();
                    object[] t = { new Vector2D() { x = 1, y = 0 }, new CollisionMat() };
                    ((Collision)currentEntry).materials.Add((CollisionMat)t[1]);
                    ((Collision)currentEntry).normals.Add((Vector2D)t[0]);
                    lines.Nodes.Add(new TreeNode("New line") { Tag = t });
                }
            }
            else
            {
                Vector2D newVert = new Vector2D() { x = currentVert.x, y = currentVert.y };
                ((Collision)currentEntry).verts.Insert(vertices.SelectedNode.Index, newVert);
                vertices.Nodes.Insert(vertices.SelectedNode.Index, new TreeNode("New Vertex") { Tag = newVert });
                if (((Collision)currentEntry).verts.Count > ((Collision)currentEntry).normals.Count + 1)
                {
                    object[] t = { new Vector2D() { x = 1, y = 0 }, new CollisionMat() };
                    ((Collision)currentEntry).materials.Insert(vertices.SelectedNode.Index, (CollisionMat)t[1]);
                    ((Collision)currentEntry).normals.Insert(vertices.SelectedNode.Index, (Vector2D)t[0]);
                    lines.Nodes.Insert(vertices.SelectedNode.Index, new TreeNode("New line") { Tag = t });
                }
            }

            renumber();
        }

        private void removeVert(object sender, EventArgs e)
        {
            ((Collision)currentEntry).verts.RemoveAt(vertices.SelectedNode.Index);
            vertices.Nodes.RemoveAt(vertices.SelectedNode.Index);

            if (((Collision)currentEntry).verts.Count - 1 == vertices.SelectedNode.Index && ((Collision)currentEntry).normals.Count != 0)
            {
                ((Collision)currentEntry).normals.RemoveAt(vertices.SelectedNode.Index - 1);
            }
            else if (((Collision)currentEntry).verts.Count - 1 != vertices.SelectedNode.Index)
            {
                ((Collision)currentEntry).normals.RemoveAt(vertices.SelectedNode.Index);
            }

            if (((Collision)currentEntry).verts.Count - 1 == vertices.SelectedNode.Index && ((Collision)currentEntry).materials.Count != 0)
            {
                ((Collision)currentEntry).materials.RemoveAt(vertices.SelectedNode.Index - 1);
                lines.Nodes.RemoveAt(vertices.SelectedNode.Index - 1);
            }   
            else if (((Collision)currentEntry).verts.Count - 1 != vertices.SelectedNode.Index)
            {
                ((Collision)currentEntry).materials.RemoveAt(vertices.SelectedNode.Index);
                lines.Nodes.RemoveAt(vertices.SelectedNode.Index);
            }

            renumber();
        }

        private void renumber()
        {
            for(int i = 0; i < vertices.Nodes.Count; i++)
                vertices.Nodes[i].Text = $"Vertex {i + 1} ({((Collision)currentEntry).verts[i].x},{((Collision)currentEntry).verts[i].y})";
            for(int i = 0; i < lines.Nodes.Count; i++)
                lines.Nodes[i].Text = $"Line {i + 1}";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Open bone selector for collision rigging
            StringWrapper str = new StringWrapper() { data = ((Collision) currentEntry).unk4 };
            BoneRiggingSelector bs = new BoneRiggingSelector(str);
            bs.ShowDialog();
            ((Collision)currentEntry).unk4 = str.data;
            string boneNameRigging = "";
            foreach (char b in ((Collision)currentEntry).unk4)
                if (b != (char)0)
                    boneNameRigging += b;
            if (boneNameRigging.Length == 0)
                boneNameRigging = "None";
            button3.Text = boneNameRigging;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //selecting something in the sections tab of the item spawner editor
            Section section = (Section)e.Node.Tag;
            treeView2.Nodes.Clear();
            currentItemSection = section;
            int i = 1;
            foreach (Vector2D v in section.points)
                treeView2.Nodes.Add(new TreeNode($"Point {i++} ({v.x},{v.y})") { Tag = v });
        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //selecting something in the vertices tab of the item spawner editor
            numericUpDown2.Value = (Decimal)((Vector2D)e.Node.Tag).x;
            numericUpDown1.Value = (Decimal)((Vector2D)e.Node.Tag).y;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //Add section
            Section section = new Section();
            
            TreeNode node = new TreeNode($"Section {treeView1.Nodes.Count + 1}") { Tag = section };
            ((ItemSpawner)currentEntry).sections.Add(section);
            treeView1.Nodes.Add(node);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //remove section
            Section section = (Section)treeView1.SelectedNode.Tag;
            TreeNode node = treeView1.SelectedNode;
            ((ItemSpawner)currentEntry).sections.Remove(section);
            treeView1.Nodes.Remove(node);
            int i = 1;
            foreach (TreeNode n in treeView1.Nodes)
                n.Text = $"Section {i++}";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //Add item spawner vertex
            Vector2D v = new Vector2D();
            if(treeView2.SelectedNode == null)
            {
                treeView2.Nodes.Add(new TreeNode("temp") { Tag = v });
                currentItemSection.points.Add(v);
            }
            else
            {
                int index = treeView2.SelectedNode.Index;
                treeView2.Nodes.Insert(index + 1, new TreeNode("temp") { Tag = v });
                currentItemSection.points.Insert(index + 1, v);
            }
            int i = 1;
            foreach (TreeNode t in treeView2.Nodes)
                t.Text = $"Point {i++} ({((Vector2D)t.Tag).x},{((Vector2D)t.Tag).y})";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //Delete item spawner vertex
            if(treeView2.SelectedNode != null)
            {
                Vector2D v = (Vector2D)treeView2.SelectedNode.Tag;
                currentItemSection.points.Remove(v);
                treeView2.Nodes.Remove(treeView2.SelectedNode);
                int i = 1;
                foreach (TreeNode t in treeView2.Nodes)
                    t.Text = $"Point {i++} ({((Vector2D)t.Tag).x},{((Vector2D)t.Tag).y})";
            }
        }

        private void changeItemVertPosition(object sender, EventArgs e)
        {
            //changed either X or Y pos of item spawner vertex
            if (sender == numericUpDown2)
                ((Vector2D)treeView2.SelectedNode.Tag).x = (float)numericUpDown2.Value;
            if(sender == numericUpDown1)
                ((Vector2D)treeView2.SelectedNode.Tag).y = (float)numericUpDown1.Value;
            treeView2.SelectedNode.Text = $"Point {treeView2.SelectedNode.Index + 1} ({((Vector2D)treeView2.SelectedNode.Tag).x},{((Vector2D)treeView2.SelectedNode.Tag).y})";
        }

        private void pointShape_ValueChanged(object sender, EventArgs e)
        {
            //General point editing
            if (sender == pointShapeX)
                currentGeneralPoint.x = (float)pointShapeX.Value;
            if (sender == pointShapeY)
                currentGeneralPoint.y = (float)pointShapeY.Value;
        }

        private void rectValueChanged(object sender, EventArgs e)
        {
            GeneralRect r = currentGeneralRect;
            if(sender == rectUpperX)
                r.x2 = (float)rectUpperX.Value;
            if (sender == rectUpperY)
                r.y2 = (float)rectUpperY.Value;
            if (sender == rectLowerX)
                r.x1 = (float)rectLowerX.Value;
            if (sender == rectLowerY)
                r.y1 = (float)rectLowerY.Value;
        }

        private void treeViewPath_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //select vertex in path
            Vector2D v = (Vector2D)e.Node.Tag;
            pathNodeX.Value = (Decimal)v.x;
            pathNodeY.Value = (Decimal)v.y;
        }

        private void pathValueChanged(object sender, EventArgs e)
        {
            Vector2D v = (Vector2D)treeViewPath.SelectedNode.Tag;
            if (sender == pathNodeX)
                v.x = (float)pathNodeX.Value;
            if (sender == pathNodeY)
                v.y = (float)pathNodeY.Value;
            renamePathTreeview();
        }

        private void renamePathTreeview()
        {
            int i = 0;
            foreach(TreeNode t in treeViewPath.Nodes)
            {
                Vector2D v = (Vector2D)t.Tag;
                t.Text = $"Point {++i} ({v.x},{v.y})";
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //Add path point
            Vector2D newPoint = new Vector2D();
            currentGeneralPath.points.Add(newPoint);
            treeViewPath.Nodes.Add(new TreeNode("") { Tag = newPoint });
            renamePathTreeview();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //Remove path point
            if (treeViewPath.SelectedNode == null)
                treeViewPath.SelectedNode = treeViewPath.Nodes[0];
            Vector2D v = (Vector2D)treeViewPath.SelectedNode.Tag;
            treeViewPath.Nodes.Remove(treeViewPath.SelectedNode);
            currentGeneralPath.points.Remove(v);
            renamePathTreeview();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //This was code to attempt to guess passthroughs for a collision if you are reading this DO NOT USE THIS
            Collision c = (Collision)currentEntry;
            Bounds collisionBounds = new Bounds() { top = -1000000, bottom = 1000000, left = 1000000, right = -1000000 };
            foreach (Vector2D v in c.verts)
            {
                if (v.y > collisionBounds.top)
                    collisionBounds.top = v.y;
                if (v.y < collisionBounds.bottom)
                    collisionBounds.bottom = v.y;
                if (v.x > collisionBounds.right)
                    collisionBounds.right = v.x;
                if (v.x < collisionBounds.left)
                    collisionBounds.left = v.x;
            }
            Vector2d centerPoint = new Vector2d();
            centerPoint.X = ((collisionBounds.right - collisionBounds.left) / 2) + collisionBounds.left;
            centerPoint.Y = ((collisionBounds.top - collisionBounds.bottom) / 2) + collisionBounds.bottom;
            for (int i = 0; i < c.verts.Count - 1; i++)
            {
                Vector2d midpoint = new Vector2d();
                midpoint.X = ((c.verts[i].x - c.verts[i + 1].x) / 2) + c.verts[i + 1].x;
                midpoint.Y = ((c.verts[i].y - c.verts[i + 1].y) / 2) + c.verts[i + 1].y;
                Vector2d normal = Vector2d.Normalize(Vector2d.Subtract(midpoint, centerPoint));
                if(c.normals.Count > i)
                {
                    c.normals[i].x = (float)normal.X;
                    c.normals[i].x = (float)normal.Y;
                }
            }
        }

        public void Clear()
        {
            currentEntry = null;
            currentVert = null;
            currentNormal = null;
            currentMat = null;
            currentTreeNode = null;
            currentPoint = null;
            currentBounds = null;
            currentItemSection = null;
            currentGeneralPoint = null;
            currentGeneralRect = null;
            currentGeneralPath = null;
            name.Text = "";
            subname.Text = "";
            collisionGroup.Visible = false;
            pointGroup.Visible = false;
            boundGroup.Visible = false;
            itemSpawnerGroup.Visible = false;
            generalPointShapeBox.Visible = false;
            rectangleGroup.Visible = false;
            pathGroup.Visible = false;
        }
    }
}
