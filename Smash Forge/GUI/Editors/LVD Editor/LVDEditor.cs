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
        public LVD LVD;

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
        private Spawn currentPoint;
        private Bounds currentBounds;
        private LVDShape currentItemSection;
        private GeneralPoint currentGeneralPoint;
        private GeneralShape currentGeneralRect;
        private GeneralShape currentGeneralPath;
        private DAT.JOBJ currentJobj;
        public DAT datToPrerender = null;

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

        public void open(Object obj, TreeNode entryTree)
        {
            lvdEntryGroup.Visible = false;
            collisionGroup.Visible = false;
            cliffGroup.Visible = false;
            point2dGroup.Visible = false;
            boundsGroup.Visible = false;
            itemSpawnerGroup.Visible = false;
            point3dGroup.Visible = false;
            rectangleGroup.Visible = false;
            pathGroup.Visible = false;
            meleeCollisionGroup.Visible = false;
            if (obj is LVDEntry)
            {
                LVDEntry entry = (LVDEntry)obj;
                lvdEntryGroup.Visible = true;
                currentTreeNode = entryTree;
                currentEntry = entry;

                name.Text = currentEntry.name;
                subname.Text = currentEntry.subname;
                xStart.Value = (decimal)currentEntry.startPos[0];
                yStart.Value = (decimal)currentEntry.startPos[1];
                zStart.Value = (decimal)currentEntry.startPos[2];
                useStartPos.Checked = currentEntry.useStartPos;
                string boneNameRigging = currentEntry.BoneName;
                if (boneNameRigging.Length == 0)
                    boneNameRigging = "None";
                button3.Text = boneNameRigging;

                if (entry is Collision)
                {
                    Collision col = (Collision)entry;
                    collisionGroup.Visible = true;
                    flag1.Checked = col.flag1;
                    flag2.Checked = col.flag2;
                    flag3.Checked = col.flag3;
                    flag4.Checked = col.flag4;
                    vertices.Nodes.Clear();
                    for (int i = 0; i < col.verts.Count; i++)
                        vertices.Nodes.Add(new TreeNode($"Vertex {i + 1} ({col.verts[i].x},{col.verts[i].y})") { Tag = col.verts[i] });
                    lines.Nodes.Clear();
                    for (int i = 0; i < col.normals.Count; i++)
                    {
                        object[] temp = { col.normals[i], col.materials[i] };
                        lines.Nodes.Add(new TreeNode($"Line {i + 1}") { Tag = temp });
                    }
                }
                else if (entry is CollisionCliff)
                {
                    CollisionCliff cliff = (CollisionCliff)entry;
                    cliffGroup.Visible = true;

                    cliffPosX.Value = (decimal)cliff.pos.x;
                    cliffPosY.Value = (decimal)cliff.pos.y;
                    cliffAngle.Value = (decimal)cliff.angle;
                    cliffLineIndex.Maximum = ((Collision)currentTreeNode.Parent.Tag).materials.Count;
                    cliffLineIndex.Value = cliff.lineIndex + 1;
                }
                else if (entry is Spawn)
                {
                    point2dGroup.Visible = true;
                    currentPoint = (Spawn)entry;
                    xPoint.Value = (decimal)((Spawn)entry).x;
                    yPoint.Value = (decimal)((Spawn)entry).y;
                }
                else if (entry is Bounds)
                {
                    boundsGroup.Visible = true;
                    currentBounds = (Bounds)entry;
                    topVal.Value = (decimal)currentBounds.top;
                    rightVal.Value = (decimal)currentBounds.right;
                    leftVal.Value = (decimal)currentBounds.left;
                    bottomVal.Value = (decimal)currentBounds.bottom;
                }
                else if (entry is ItemSpawner)
                {
                    itemSpawnerGroup.Visible = true;
                    ItemSpawner spawner = (ItemSpawner)entry;
                    treeView1.Nodes.Clear();
                    int i = 1;
                    foreach (LVDShape section in spawner.sections)
                        treeView1.Nodes.Add(new TreeNode($"Section {i++}") { Tag = section });

                }
                else if (entry is GeneralPoint)
                {
                    point3dGroup.Visible = true;
                    GeneralPoint p = (GeneralPoint)entry;
                    currentGeneralPoint = p;
                    pointShapeX.Value = (Decimal)p.x;
                    pointShapeY.Value = (Decimal)p.y;
                    pointShapeZ.Value = (Decimal)p.z;
                }
                else if (entry is GeneralShape)
                {
                    GeneralShape s = (GeneralShape)entry;
                    if (s.type == 1)
                    {
                        point2dGroup.Visible = true;
                        xPoint.Value = (decimal)s.x1;
                        yPoint.Value = (decimal)s.y1;
                    }
                    else if (s.type == 3)
                    {
                        rectangleGroup.Visible = true;
                        currentGeneralRect = s;
                        rectUpperX.Value = (Decimal)s.x2;
                        rectUpperY.Value = (Decimal)s.y2;
                        rectLowerX.Value = (Decimal)s.x1;
                        rectLowerY.Value = (Decimal)s.y1;
                    }
                    else if (s.type == 4)
                    {
                        pathGroup.Visible = true;
                        currentGeneralPath = s;
                        treeViewPath.Nodes.Clear();
                        int j = 0;
                        foreach (Vector2D v in s.points)
                            treeViewPath.Nodes.Add(new TreeNode($"Point {++j} ({v.x},{v.y})") { Tag = v });
                    }
                }
                else if (entry is DAT.COLL_DATA)
                {
                    meleeCollisionGroup.Visible = true;
                    meleeVerts.Nodes.Clear();
                    meleeLinks.Nodes.Clear();
                    meleePolygons.Nodes.Clear();
                    int i = 0;
                    foreach (Vector2D vert in ((DAT.COLL_DATA)entry).vertices)
                        meleeVerts.Nodes.Add(new TreeNode($"Vertex {i++}") { Tag = vert });
                    i = 0;
                    foreach (DAT.COLL_DATA.Link link in ((DAT.COLL_DATA)entry).links)
                        meleeLinks.Nodes.Add(new TreeNode($"Link {i++}") { Tag = link });
                    i = 0;
                    foreach (DAT.COLL_DATA.AreaTableEntry ate in ((DAT.COLL_DATA)entry).areaTable)
                        meleePolygons.Nodes.Add(new TreeNode($"Polygon {i++}") { Tag = ate });
                }
            }
            else if(obj is DAT.JOBJ)
            {
                DAT.JOBJ jobj = (DAT.JOBJ)obj;
                currentJobj = jobj;
                jobjX.Value = (Decimal)jobj.pos.X;
                jobjY.Value = (Decimal)jobj.pos.Y;
                jobjZ.Value = (Decimal)jobj.pos.Z;
            }
        }

        private void vertices_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currentVert = (Vector2D)e.Node.Tag;
            LVD.LVDSelection = currentVert;
            MainForm.Instance.viewports[0].timeSinceSelected.Restart();
            xVert.Value = (decimal)currentVert.x;
            yVert.Value = (decimal)currentVert.y;
        }

        private void lines_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currentNormal = (Vector2D)((object[])e.Node.Tag)[0];
            LVD.LVDSelection = currentNormal;
            MainForm.Instance.viewports[0].timeSinceSelected.Restart();
            currentMat = (CollisionMat)((object[])e.Node.Tag)[1];
            leftLedge.Checked = currentMat.getFlag(6);
            rightLedge.Checked = currentMat.getFlag(7);
            noWallJump.Checked = currentMat.getFlag(4);
            comboBox1.Text = Enum.GetName(typeof(materialTypes), currentMat.physics);
            passthroughAngle.Value = (decimal)(Math.Atan2(currentNormal.y, currentNormal.x) * 180.0 / Math.PI);
        }

        private void flagChange(object sender, EventArgs e)
        {
            if(sender == flag1)
                ((Collision)currentEntry).flag1 = flag1.Checked;
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
             currentMat.physics = ((byte)Enum.Parse(typeof(materialTypes), comboBox1.Text));
        }

        private void changeStart(object sender, EventArgs e)
        {
            if (sender == useStartPos)
                currentEntry.useStartPos = useStartPos.Checked;
            if (sender == xStart)
                currentEntry.startPos[0] = (float)xStart.Value;
            if (sender == yStart)
                currentEntry.startPos[1] = (float)yStart.Value;
            if (sender == zStart)
                currentEntry.startPos[2] = (float)zStart.Value;
        }

        private void lineFlagChange(object sender, EventArgs e)
        {
            if (sender == rightLedge)
                currentMat.setFlag(7, ((CheckBox)sender).Checked);
            if (sender == leftLedge)
                currentMat.setFlag(6, ((CheckBox)sender).Checked);
            if (sender == noWallJump)
                currentMat.setFlag(4, ((CheckBox)sender).Checked);
        }

        private void LVDEditor_Load(object sender, EventArgs e)
        {
            lvdEntryGroup.Visible = false;
            collisionGroup.Visible = false;
            point2dGroup.Visible = false;
            boundsGroup.Visible = false;
        }

        private void cliff_ValueChanged(object sender, EventArgs e)
        {
            CollisionCliff cliff = (CollisionCliff)currentEntry;

            if (sender == cliffPosX)
                cliff.pos.x = (float)cliffPosX.Value;
            if (sender == cliffPosY)
                cliff.pos.y = (float)cliffPosY.Value;
            if (sender == cliffAngle)
                cliff.angle = (float)cliffAngle.Value;
            if (sender == cliffLineIndex)
                cliff.lineIndex = (int)cliffLineIndex.Value - 1;
        }

        private void pointMoved(object sender, EventArgs e)
        {
            if (currentEntry is Spawn)
            {
                if (sender == xPoint)
                    currentPoint.x = (float)xPoint.Value;
                if (sender == yPoint)
                    currentPoint.y = (float)yPoint.Value;
            }
            else if (currentEntry is GeneralShape)
            {
                if (sender == xPoint)
                    ((GeneralShape)currentEntry).x1 = (float)xPoint.Value;
                if (sender == yPoint)
                    ((GeneralShape)currentEntry).y1 = (float)yPoint.Value;
            }
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
            Collision col = (Collision)currentEntry;
            int index = (vertices.SelectedNode == null) ? col.verts.Count : vertices.SelectedNode.Index + 1;

            Vector2D newVert;
            if (vertices.SelectedNode == null)
                newVert = new Vector2D();
            else
                newVert = new Vector2D(currentVert.x, currentVert.y);
            col.verts.Insert(index, newVert);

            TreeNode newNode = new TreeNode("New Vertex") { Tag = newVert };
            vertices.Nodes.Insert(index, newNode);
            if (vertices.SelectedNode == null)
                vertices.SelectedNode = newNode;

            index--;
            if (col.verts.Count > col.normals.Count + 1)
            {
                object[] temp = { new Vector2D(1, 0), new CollisionMat() };
                col.normals.Insert(index, (Vector2D)temp[0]);
                col.materials.Insert(index, (CollisionMat)temp[1]);
                lines.Nodes.Insert(index, new TreeNode("New Line") { Tag = temp });
            }

            renumber();
        }

        private void removeVert(object sender, EventArgs e)
        {
            Collision col = (Collision)currentEntry;
            int vertCount = col.verts.Count;
            if (vertCount == 0)
                return;
            int index = (vertices.SelectedNode == null) ? col.verts.Count - 1 : vertices.SelectedNode.Index;

            col.verts.RemoveAt(index);
            vertices.Nodes.RemoveAt(index);

            index = (index == vertCount - 1) ? index - 1 : index;
            if (col.normals.Count > 0)
                col.normals.RemoveAt(index);
            if (col.materials.Count > 0)
                col.materials.RemoveAt(index);
            if (lines.Nodes.Count > 0)
                lines.Nodes.RemoveAt(index);

            for (int i = 0; i < col.cliffs.Count; i++)
            {
                if (col.cliffs[i].lineIndex == index)
                {
                    col.cliffs.RemoveAt(i);
                    currentTreeNode.Nodes.RemoveAt(i);
                    continue;
                }
                if (col.cliffs[i].lineIndex > index)
                    col.cliffs[i].lineIndex--;
                while (col.cliffs[i].lineIndex >= col.materials.Count && col.cliffs[i].lineIndex > 0)
                    col.cliffs[i].lineIndex--;
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
            StringWrapper str = new StringWrapper() { data = currentEntry.boneName };
            BoneRiggingSelector bs = new BoneRiggingSelector(str);
            bs.ShowDialog();
            currentEntry.boneName = str.data;
            string boneNameRigging = currentEntry.BoneName;
            if (boneNameRigging.Length == 0)
                boneNameRigging = "None";
            button3.Text = boneNameRigging;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //selecting something in the sections tab of the item spawner editor
            LVDShape section = (LVDShape)e.Node.Tag;
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
            LVDShape section = new LVDShape(4);
            
            TreeNode node = new TreeNode($"Section {treeView1.Nodes.Count + 1}") { Tag = section };
            ((ItemSpawner)currentEntry).sections.Add(section);
            treeView1.Nodes.Add(node);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //remove section
            LVDShape section = (LVDShape)treeView1.SelectedNode.Tag;
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
            if (sender == pointShapeZ)
                currentGeneralPoint.z = (float)pointShapeZ.Value;
        }

        private void rectValueChanged(object sender, EventArgs e)
        {
            GeneralShape r = currentGeneralRect;
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
            // //How about this code?
            Collision c = (Collision)currentEntry;
            for (int i = 0; i < c.verts.Count - 1; i++)
            {
                decimal lineAngle = (decimal)(Math.Atan2(c.verts[i].x-c.verts[i+1].x, c.verts[i].y-c.verts[i+1].y) * 180/Math.PI);
                double theta = (double)(lineAngle+90);
                c.normals[i].x = (float)Math.Cos(theta * Math.PI / 180.0f);
                c.normals[i].y = (float)Math.Sin(theta * Math.PI / 180.0f);
            }
            
            // //Original code
            /*
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
            */
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
            lvdEntryGroup.Visible = false;
            collisionGroup.Visible = false;
            cliffGroup.Visible = false;
            point2dGroup.Visible = false;
            boundsGroup.Visible = false;
            itemSpawnerGroup.Visible = false;
            point3dGroup.Visible = false;
            rectangleGroup.Visible = false;
            pathGroup.Visible = false;
        }

        #region meleeCollisions
        private DAT.COLL_DATA.AreaTableEntry currentAreaTableEntry;
        private DAT.COLL_DATA.Link currentLink;
        private Vector2D currentMeleeVert;

        private void updateVertexPosition(object sender, EventArgs e)
        {
            if (currentMeleeVert == null)
                return;
            if (sender == meleeX)
                currentMeleeVert.x = (float)meleeX.Value;
            if (sender == meleeY)
                currentMeleeVert.y = (float)meleeY.Value;
        }

        private void linkVertUpdate(object sender, EventArgs e)
        {
            if (currentLink == null)
                return;
            if (sender == vertStart)
                currentLink.vertexIndices[0] = (int)vertStart.Value;
            if (sender == vertEnd)
                currentLink.vertexIndices[1] = (int)vertEnd.Value;
            if (sender == linkBefore)
                currentLink.connectors[0] = (int)linkBefore.Value;
            if (sender == linkAfter)
                currentLink.connectors[1] = (int)linkAfter.Value;
            if (currentLink.connectors[0] == -1)
                currentLink.connectors[0] = ushort.MaxValue;
            if (currentLink.connectors[1] == -1)
                currentLink.connectors[1] = ushort.MaxValue;
        }

        private int changeFlag(int original, int mask, bool newValue)
        {
            bool isSet = (original & mask) != 0;
            if (newValue)
                original |= mask;
            else
                original &= (byte)~mask;
            return original;
        }

        private void linkPropertyUpdate(object sender, EventArgs e)
        {
            if (currentLink == null)
                return;
            if (sender == leftWall)
                currentLink.collisionAngle = changeFlag(currentLink.collisionAngle, 4, leftWall.Checked);
            if (sender == rightWall)
                currentLink.collisionAngle = changeFlag(currentLink.collisionAngle, 8, rightWall.Checked);
            if (sender == floor)
                currentLink.collisionAngle = changeFlag(currentLink.collisionAngle, 1, floor.Checked);
            if (sender == ceiling)
                currentLink.collisionAngle = changeFlag(currentLink.collisionAngle, 2, ceiling.Checked);
            if (sender == ledge)
                currentLink.flags = (byte)changeFlag(currentLink.flags, 2, ledge.Checked);
            if (sender == meleeDropThrough)
                currentLink.flags = (byte)changeFlag(currentLink.flags, 1, meleeDropThrough.Checked);
        }

        private void polygonRangeChange(object sender, EventArgs e)
        {
            if (currentAreaTableEntry == null)
                return;
            if (sender == polyStart)
                currentAreaTableEntry.idxLowestSpot = (ushort)polyStart.Value;
            if (sender == polyEnd)
                currentAreaTableEntry.nbLinks = (ushort)((polyEnd.Value - currentAreaTableEntry.idxLowestSpot) + 1);
        }

        private void selectItem(object sender, TreeViewEventArgs e)
        {
            if(sender == meleeVerts)
            {
                currentMeleeVert = (Vector2D)meleeVerts.SelectedNode.Tag;
                meleeX.Value = (Decimal)currentMeleeVert.x;
                meleeY.Value = (Decimal)currentMeleeVert.y;
            }
            else if(sender == meleeLinks)
            {
                currentLink = (DAT.COLL_DATA.Link)meleeLinks.SelectedNode.Tag;
                vertStart.Value = currentLink.vertexIndices[0];
                vertEnd.Value = currentLink.vertexIndices[1];
                if (currentLink.connectors[0] != ushort.MaxValue)
                    linkBefore.Value = currentLink.connectors[0];
                else
                    linkBefore.Value = -1;
                if (currentLink.connectors[1] != ushort.MaxValue)
                    linkAfter.Value = currentLink.connectors[1];
                else
                    linkAfter.Value = -1;
                leftWall.Checked = ((currentLink.collisionAngle & 4) != 0);
                rightWall.Checked = ((currentLink.collisionAngle & 8) != 0);
                floor.Checked = ((currentLink.collisionAngle & 1) != 0);
                ceiling.Checked = ((currentLink.collisionAngle & 2) != 0);
                ledge.Checked = ((currentLink.flags & 2) != 0);
                meleeDropThrough.Checked = ((currentLink.flags & 1) != 0);
                comboBox2.Text = Enum.GetName(typeof(materialTypes), currentLink.material);
            }
            else if(sender == meleePolygons)
            {
                currentAreaTableEntry = (DAT.COLL_DATA.AreaTableEntry)meleePolygons.SelectedNode.Tag;
                polyStart.Value = currentAreaTableEntry.idxLowestSpot;
                polyEnd.Value = currentAreaTableEntry.idxLowestSpot + currentAreaTableEntry.nbLinks - 1;
                Console.WriteLine(meleePolygons.SelectedNode.Text + $" - ({currentAreaTableEntry.xBotLeftCorner},{currentAreaTableEntry.yBotLeftCorner}),({currentAreaTableEntry.xTopRightCorner},{currentAreaTableEntry.yTopRightCorner})");
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentLink == null)
                return;
            currentLink.material = (byte)Enum.Parse(typeof(materialTypes), comboBox2.Text);
        }
        #endregion

        private void meleeAddVert_Click(object sender, EventArgs e)
        {
            Vector2D vert = new Vector2D() { x = (float)meleeX.Value, y = (float)meleeY.Value };
            DAT.COLL_DATA coll = (DAT.COLL_DATA)currentEntry;
            coll.vertices.Add(vert);
            meleeVerts.Nodes.Add(new TreeNode($"Vertex {meleeVerts.Nodes.Count}") { Tag = vert });
        }

        private void meleeSubtractVert_Click(object sender, EventArgs e)
        {
            DAT.COLL_DATA coll = (DAT.COLL_DATA)currentEntry;
            Vector2D vert = (Vector2D)meleeVerts.SelectedNode.Tag;
            coll.vertices.Remove(vert);
            int index = meleeVerts.SelectedNode.Index;
            meleeVerts.Nodes.Remove(meleeVerts.SelectedNode);
            for(int i = index; i < meleeVerts.Nodes.Count; i++)
                meleeVerts.Nodes[i].Text = $"Vertex {i}";
        }

        private void meleeAddLink_Click(object sender, EventArgs e)
        {
            DAT.COLL_DATA.Link link = new DAT.COLL_DATA.Link();
            link.connectors = new int[]{ 0xFFFF, 0xFFFF };
            link.vertexIndices = new int[2];
            DAT.COLL_DATA coll = (DAT.COLL_DATA)currentEntry;
            coll.links.Add(link);
            meleeLinks.Nodes.Add(new TreeNode($"Link {meleeLinks.Nodes.Count}") { Tag = link });
        }

        private void meleeSubtractLink_Click(object sender, EventArgs e)
        {
            DAT.COLL_DATA coll = (DAT.COLL_DATA)currentEntry;
            DAT.COLL_DATA.Link link = (DAT.COLL_DATA.Link)meleeLinks.SelectedNode.Tag;
            coll.links.Remove(link);
            int index = meleeLinks.SelectedNode.Index;
            meleeLinks.Nodes.Remove(meleeLinks.SelectedNode);
            for (int i = index; i < meleeLinks.Nodes.Count; i++)
                meleeLinks.Nodes[i].Text = $"Link {i}";
        }

        private void polygonAdd_Click(object sender, EventArgs e)
        {
            DAT.COLL_DATA.AreaTableEntry ate = new DAT.COLL_DATA.AreaTableEntry();
            ate.nbLinks = 1;
            DAT.COLL_DATA coll = (DAT.COLL_DATA)currentEntry;
            coll.areaTable.Add(ate);
            meleePolygons.Nodes.Add(new TreeNode($"Polygon {meleePolygons.Nodes.Count}") { Tag = ate });
        }

        private void polygonSubtract_Click(object sender, EventArgs e)
        {
            DAT.COLL_DATA coll = (DAT.COLL_DATA)currentEntry;
            DAT.COLL_DATA.AreaTableEntry link = (DAT.COLL_DATA.AreaTableEntry)meleePolygons.SelectedNode.Tag;
            coll.areaTable.Remove(link);
            int index = meleePolygons.SelectedNode.Index;
            meleePolygons.Nodes.Remove(meleePolygons.SelectedNode);
            for (int i = index; i < meleePolygons.Nodes.Count; i++)
                meleePolygons.Nodes[i].Text = $"Polygon {i}";
        }

        private void jobjPostionUpdate(object sender, EventArgs e)
        {
            if (sender == jobjX)
            {
                float xdelta = (float)jobjX.Value - currentJobj.pos.X;
                jobjTranslate(currentJobj, new Vector3(xdelta, 0, 0), true);
                if (datToPrerender != null)
                    datToPrerender.PreRender();
            }
            if (sender == jobjY)
            {
                float ydelta = (float)jobjY.Value - currentJobj.pos.Y;
                jobjTranslate(currentJobj, new Vector3(0, ydelta, 0), true);
                if (datToPrerender != null)
                    datToPrerender.PreRender();
            }
            if (sender == jobjZ)
            {
                float zdelta = (float)jobjZ.Value - currentJobj.pos.Z;
                jobjTranslate(currentJobj, new Vector3(0, 0, zdelta), true);
                if (datToPrerender != null)
                    datToPrerender.PreRender();
            }
            
        }

        private void jobjTranslate(DAT.JOBJ jobj, Vector3 posTransform, bool applyTransform = false)
        {
            if (applyTransform)
            {
                jobj.pos += posTransform;
                jobj.transform = Matrix4.Add(jobj.transform, Matrix4.CreateTranslation(posTransform));
                jobj.inverseTransform = Matrix4.Invert(jobj.transform);
            }

            foreach (TreeNode node in jobj.node.Nodes)
            {
                if(node.Tag is DAT.JOBJ)
                    jobjTranslate((DAT.JOBJ)node.Tag, posTransform);
                if (node.Tag is DAT.DOBJ)
                    dobjTranslate((DAT.DOBJ)node.Tag, posTransform);
            }
        }

        private void dobjTranslate(DAT.DOBJ dobj, Vector3 posTransform)
        {
            foreach (DAT.POBJ pobj in dobj.polygons)
                foreach (DAT.POBJ.DisplayObject disp in pobj.display)
                    if(disp.verts != null)
                        foreach (DAT.Vertex vert in disp.verts)
                            vert.pos += posTransform;
        }
    }
}
