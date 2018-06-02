using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using OpenTK;

namespace Smash_Forge
{
    public partial class LVDEditor : DockContent
    {
        public LVDEditor()
        {
            InitializeComponent();
        }
        private void LVDEditor_Load(object sender, EventArgs e)
        {
            HideAllGroupBoxes();
            physicsMatComboBox.DataSource = Enum.GetValues(typeof(CollisionMatType));
        }

        public LVD LVD;

        public LVDEntry currentEntry;
        private TreeNode currentTreeNode;
        private CollisionMat currentMat;
        private Spawn currentPoint;
        private Bounds currentBounds;

        private DAT.JOBJ currentJobj;
        public DAT datToPrerender = null;

        public void Open(Object obj, TreeNode entryTree)
        {
            ResetUi();

            if (obj is LVDEntry)
            {
                LVDEntry entry = (LVDEntry)obj;
                OpenLvdEntry(entry, entryTree);
            }
            else if (obj is DAT.JOBJ)
            {
                DAT.JOBJ jobj = (DAT.JOBJ)obj;
                OpenDatJObj(jobj);
            }
        }

        private void HideAllGroupBoxes()
        {
            lvdEntryGroup.Visible = false;
            collisionGroup.Visible = false;
            cliffGroup.Visible = false;
            point2dGroup.Visible = false;
            boundsGroup.Visible = false;
            itemSpawnerGroup.Visible = false;
            point3dGroup.Visible = false;
            shapeGroup.Visible = false;
            meleeCollisionGroup.Visible = false;
        }

        public void ResetUi()
        {
            currentEntry = null;
            currentTreeNode = null;
            currentMat = null;
            currentPoint = null;
            currentBounds = null;
            nameTB.Text = "";
            subnameTB.Text = "";
            HideAllGroupBoxes();
        }

        private void OpenDatJObj(DAT.JOBJ jobj)
        {
            currentJobj = jobj;
            jobjX.Value = (Decimal)jobj.pos.X;
            jobjY.Value = (Decimal)jobj.pos.Y;
            jobjZ.Value = (Decimal)jobj.pos.Z;
        }

        private void OpenLvdEntry(LVDEntry entry, TreeNode entryTree)
        {
            lvdEntryGroup.Visible = true;
            currentEntry = entry;
            currentTreeNode = entryTree;

            nameTB.Text = currentEntry.name;
            subnameTB.Text = currentEntry.subname;
            xStartPosUpDown.Value = (decimal)currentEntry.startPos[0];
            yStartPosUpDown.Value = (decimal)currentEntry.startPos[1];
            zStartPosUpDown.Value = (decimal)currentEntry.startPos[2];
            useStartPosCB.Checked = currentEntry.useStartPos;
            string boneNameRigging = currentEntry.boneName;
            if (boneNameRigging.Length == 0)
                boneNameRigging = "None";
            boneRigSelectButton.Text = boneNameRigging;

            if (entry is Collision)
            {
                Collision col = (Collision)entry;
                OpenCollision(col);
            }
            else if (entry is CollisionCliff)
            {
                CollisionCliff cliff = (CollisionCliff)entry;
                OpenCliff(cliff);
            }
            else if (entry is Spawn)
            {
                OpenSpawn(entry);
            }
            else if (entry is Bounds)
            {
                OpenBounds(entry);
            }
            else if (entry is ItemSpawner)
            {
                OpenItemSpawner(entry);

            }
            else if (entry is GeneralPoint)
            {
                GeneralPoint generalPoint = (GeneralPoint)entry;
                OpenGeneralPoint(generalPoint);
            }
            else if (entry is GeneralShape)
            {
                GeneralShape s = (GeneralShape)entry;
                OpenGeneralShape(s);
            }
            else if (entry is DAT.COLL_DATA)
            {
                OpenDatCollData(entry);
            }
        }

        private void OpenDatCollData(LVDEntry entry)
        {
            meleeCollisionGroup.Visible = true;
            meleeVerts.Nodes.Clear();
            meleeLinks.Nodes.Clear();
            meleePolygons.Nodes.Clear();

            int i = 0;
            foreach (Vector2 vert in ((DAT.COLL_DATA)entry).vertices)
                meleeVerts.Nodes.Add(new TreeNode($"Vertex {i++}") { Tag = vert });

            int j = 0;
            foreach (DAT.COLL_DATA.Link link in ((DAT.COLL_DATA)entry).links)
                meleeLinks.Nodes.Add(new TreeNode($"Link {j++}") { Tag = link });

            int k = 0;
            foreach (DAT.COLL_DATA.AreaTableEntry ate in ((DAT.COLL_DATA)entry).areaTable)
                meleePolygons.Nodes.Add(new TreeNode($"Polygon {k++}") { Tag = ate });
        }

        private void OpenGeneralShape(GeneralShape shape)
        {
            shapeGroup.Visible = true;

            rectLowerX.Value = (decimal)shape.x1;
            rectLowerY.Value = (decimal)shape.y1;
            rectUpperX.Value = (decimal)shape.x2;
            rectUpperY.Value = (decimal)shape.y2;

            treeViewPath.Nodes.Clear();
            for (int i = 0; i < shape.points.Count; ++i)
                treeViewPath.Nodes.Add(new TreeNode());
            renamePathTreeview();

            /*if (shape.type == (int)LVDShapeType.Point)
            {
                point2dGroup.Visible = true;
                xPointUpDown.Value = (decimal)shape.x1;
                yPointUpDown.Value = (decimal)shape.y1;
            }
            else if (shape.type == (int)LVDShapeType.Rectangle)
            {
                shapeGroup.Visible = true;
                currentGeneralRect = shape;
                rectUpperX.Value = (Decimal)shape.x2;
                rectUpperY.Value = (Decimal)shape.y2;
                rectLowerX.Value = (Decimal)shape.x1;
                rectLowerY.Value = (Decimal)shape.y1;
            }
            else if (shape.type == (int)LVDShapeType.Path)
            {
                pathGroup.Visible = true;
                currentGeneralPath = shape;
                treeViewPath.Nodes.Clear();
                int j = 0;
                foreach (Vector2 v in shape.points)
                    treeViewPath.Nodes.Add(new TreeNode($"Point {++j} ({v.X},{v.Y})") { Tag = v });
            }*/
        }

        private void OpenGeneralPoint(GeneralPoint point)
        {
            point3dGroup.Visible = true;
            pointShapeXUpDown.Value = (Decimal)point.x;
            pointShapeYUpDown.Value = (Decimal)point.y;
            pointShapeZUpDown.Value = (Decimal)point.z;
        }

        private void OpenItemSpawner(LVDEntry entry)
        {
            itemSpawnerGroup.Visible = true;
            ItemSpawner spawner = (ItemSpawner)entry;
            itemSpawnSectionTreeView.Nodes.Clear();
            int i = 1;
            foreach (LVDShape section in spawner.sections)
                itemSpawnSectionTreeView.Nodes.Add(new TreeNode($"Section {i++}") { Tag = section });
        }

        private void OpenBounds(LVDEntry entry)
        {
            boundsGroup.Visible = true;
            currentBounds = (Bounds)entry;
            topValUpDown.Value = (decimal)currentBounds.top;
            rightValUpDown.Value = (decimal)currentBounds.right;
            leftVal.Value = (decimal)currentBounds.left;
            bottomVal.Value = (decimal)currentBounds.bottom;
        }

        private void OpenSpawn(LVDEntry entry)
        {
            point2dGroup.Visible = true;
            currentPoint = (Spawn)entry;
            xPointUpDown.Value = (decimal)((Spawn)entry).x;
            yPointUpDown.Value = (decimal)((Spawn)entry).y;
        }

        private void OpenCliff(CollisionCliff cliff)
        {
            cliffGroup.Visible = true;

            cliffPosXUpDown.Value = (decimal)cliff.pos.X;
            cliffPosYUpDown.Value = (decimal)cliff.pos.Y;
            cliffAngleUpDown.Value = (decimal)cliff.angle;
            cliffLineIndexUpDown.Maximum = ((Collision)currentTreeNode.Parent.Tag).materials.Count;
            cliffLineIndexUpDown.Value = cliff.lineIndex + 1;
        }

        private void OpenCollision(Collision col)
        {
            collisionGroup.Visible = true;
            flag1CB.Checked = col.flag1;
            rigCollisionCB.Checked = col.flag2;
            flag3CB.Checked = col.flag3;
            dropThroughCB.Checked = col.flag4;
            verticesTreeView.Nodes.Clear();
            for (int i = 0; i < col.verts.Count; i++)
                verticesTreeView.Nodes.Add(new TreeNode($"Vertex {i + 1} ({col.verts[i].X},{col.verts[i].Y})") { Tag = col.verts[i] });
            linesTreeView.Nodes.Clear();
            for (int i = 0; i < col.normals.Count; i++)
            {
                object[] temp = { col.normals[i], col.materials[i] };
                linesTreeView.Nodes.Add(new TreeNode($"Line {i + 1}") { Tag = temp });
            }
        }

        private void vertices_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Find the selected vert and update the position numeric control.
            int selectedIndex = verticesTreeView.SelectedNode.Index;
            Collision collision = (Collision)currentEntry;
            if (selectedIndex >= collision.verts.Count)
                return;

            // Used for the blinking during rendering.
            LVD.LVDSelection = collision.verts[selectedIndex];

            vertXPosUpDown.Value = (decimal)collision.verts[selectedIndex].X;
            vertYPosUpDown.Value = (decimal)collision.verts[selectedIndex].Y;
        }

        private void lines_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Find the currently selected normal.
            int selectedIndex = linesTreeView.SelectedNode.Index;
            Collision collision = (Collision)currentEntry;
            if (selectedIndex >= collision.normals.Count)
                return;

            LVD.LVDSelection = collision.normals[selectedIndex];

            currentMat = (CollisionMat)((object[])e.Node.Tag)[1];
            leftLedgeCB.Checked = currentMat.getFlag(6);
            rightLedgeCB.Checked = currentMat.getFlag(7);
            noWallJumpCB.Checked = currentMat.getFlag(4);
            physicsMatComboBox.SelectedItem = (CollisionMatType)currentMat.physics;

            passthroughAngleUpDown.Value = (decimal)(Math.Atan2(collision.normals[selectedIndex].Y, collision.normals[selectedIndex].X) * 180.0 / Math.PI);
        }

        private void flagChange(object sender, EventArgs e)
        {
            if(sender == flag1CB)
                ((Collision)currentEntry).flag1 = flag1CB.Checked;
            if (sender == rigCollisionCB)
                ((Collision)currentEntry).flag2 = rigCollisionCB.Checked;
            if (sender == flag3CB)
                ((Collision)currentEntry).flag3 = flag3CB.Checked;
            if (sender == dropThroughCB)
                ((Collision)currentEntry).flag4 = dropThroughCB.Checked;
        }

        private void ChangeCollisionVertPos(object sender, EventArgs e)
        {
            Debug.WriteLine(currentEntry.GetType());
            if (currentEntry is Collision)
            {
                // Find which vert of the collision is selected.
                int selectedIndex = verticesTreeView.SelectedNode.Index;
                Collision collision = (Collision)currentEntry;
                if (selectedIndex >= collision.verts.Count)
                    return;

                // Update the collision vert's position.
                if (sender == vertXPosUpDown)
                    collision.verts[selectedIndex] = new Vector2((float)vertXPosUpDown.Value, collision.verts[selectedIndex].Y);
                if (sender == vertYPosUpDown)
                    collision.verts[selectedIndex] = new Vector2(collision.verts[selectedIndex].X, (float)vertYPosUpDown.Value);

                // Verts are named using the index and their position.
                string name = $"Vertex {verticesTreeView.SelectedNode.Index + 1} ({collision.verts[selectedIndex].X},{collision.verts[selectedIndex].Y})";
                verticesTreeView.SelectedNode.Text = name;
            }
        }

        private void nameChange(object sender, EventArgs e)
        {
            if (currentEntry == null)
                return;
            if (sender == nameTB)
            {
                currentEntry.name = nameTB.Text;
                currentTreeNode.Text = nameTB.Text;
            }
                
            if (sender == subnameTB)
                currentEntry.subname = subnameTB.Text;
        }

        private void passthroughAngle_ValueChanged(object sender, EventArgs e)
        {
            // Find the currently selected normal.
            int selectedIndex = linesTreeView.SelectedNode.Index;
            Collision collision = (Collision)currentEntry;
            if (selectedIndex >= collision.normals.Count)
                return;

            double theta = (double)((NumericUpDown)sender).Value;
            collision.normals[selectedIndex] = new Vector2((float)Math.Cos(theta * Math.PI / 180.0f), (float)Math.Sin(theta * Math.PI / 180.0f));
        }

        private void physicsMatComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentMat == null)
                return;
            currentMat.physics = ((byte)Enum.Parse(typeof(CollisionMatType), physicsMatComboBox.Text));
        }

        private void changeStart(object sender, EventArgs e)
        {
            if (sender == useStartPosCB)
                currentEntry.useStartPos = useStartPosCB.Checked;
            if (sender == xStartPosUpDown)
                currentEntry.startPos[0] = (float)xStartPosUpDown.Value;
            if (sender == yStartPosUpDown)
                currentEntry.startPos[1] = (float)yStartPosUpDown.Value;
            if (sender == zStartPosUpDown)
                currentEntry.startPos[2] = (float)zStartPosUpDown.Value;
        }

        private void lineFlagChange(object sender, EventArgs e)
        {
            if (currentMat == null)
                return;

            if (sender == noWallJumpCB)
                currentMat.setFlag(4, ((CheckBox)sender).Checked);
            if (sender == leftLedgeCB)
                currentMat.setFlag(6, ((CheckBox)sender).Checked);
            if (sender == rightLedgeCB)
                currentMat.setFlag(7, ((CheckBox)sender).Checked);
        }

        private void cliff_ValueChanged(object sender, EventArgs e)
        {
            CollisionCliff cliff = (CollisionCliff)currentEntry;

            if (sender == cliffPosXUpDown)
                cliff.pos.X = (float)cliffPosXUpDown.Value;
            if (sender == cliffPosYUpDown)
                cliff.pos.Y = (float)cliffPosYUpDown.Value;
            if (sender == cliffAngleUpDown)
                cliff.angle = (float)cliffAngleUpDown.Value;
            if (sender == cliffLineIndexUpDown)
                cliff.lineIndex = (int)cliffLineIndexUpDown.Value - 1;
        }

        private void pointMoved(object sender, EventArgs e)
        {
            if (currentEntry is Spawn)
            {
                if (sender == xPointUpDown)
                    currentPoint.x = (float)xPointUpDown.Value;
                if (sender == yPointUpDown)
                    currentPoint.y = (float)yPointUpDown.Value;
            }
            else if (currentEntry is GeneralShape)
            {
                if (sender == xPointUpDown)
                    ((GeneralShape)currentEntry).x1 = (float)xPointUpDown.Value;
                if (sender == yPointUpDown)
                    ((GeneralShape)currentEntry).y1 = (float)yPointUpDown.Value;
            }
        }

        private void boundsChanged(object sender, EventArgs e)
        {
            if (sender == topValUpDown)
                currentBounds.top = (float)topValUpDown.Value;
            if (sender == bottomVal)
                currentBounds.bottom = (float)bottomVal.Value;
            if (sender == leftVal)
                currentBounds.left = (float)leftVal.Value;
            if (sender == rightValUpDown)
                currentBounds.right = (float)rightValUpDown.Value;
        }

        private void addVertButtonClicked(object sender, EventArgs e)
        {
            Collision col = (Collision)currentEntry;

            // Add a new vertex to the collision. Duplicates the currently selected vertex.
            int index = (verticesTreeView.SelectedNode == null) ? -1 : verticesTreeView.SelectedNode.Index;
            Vector2 newVert = new Vector2();
            if (index == -1)
            {
                index = col.verts.Count;
            }
            else
            {
                newVert = new Vector2(col.verts[index].X, col.verts[index].Y);
                ++index;
            }
            col.verts.Insert(index, newVert);

            // Add the new vert to the tree view.
            TreeNode newNode = new TreeNode("New Vertex") {};
            verticesTreeView.Nodes.Insert(index, newNode);
            if (verticesTreeView.SelectedNode == null)
                verticesTreeView.SelectedNode = newNode;

            index--;
            if (col.verts.Count > col.normals.Count + 1)
            {
                object[] temp = { new Vector2(1, 0), new CollisionMat() };
                col.normals.Insert(index, (Vector2)temp[0]);
                col.materials.Insert(index, (CollisionMat)temp[1]);
                linesTreeView.Nodes.Insert(index, new TreeNode("New Line") { Tag = temp });
            }

            UpdateVertexNumbers();
        }

        private void removeVertButtonClicked(object sender, EventArgs e)
        {
            Collision col = (Collision)currentEntry;
            int vertCount = col.verts.Count;
            if (vertCount == 0)
                return;
            int index = (verticesTreeView.SelectedNode == null) ? col.verts.Count - 1 : verticesTreeView.SelectedNode.Index;

            col.verts.RemoveAt(index);
            verticesTreeView.Nodes.RemoveAt(index);

            index = (index == vertCount - 1) ? index - 1 : index;
            if (col.normals.Count > 0)
                col.normals.RemoveAt(index);
            if (col.materials.Count > 0)
                col.materials.RemoveAt(index);
            if (linesTreeView.Nodes.Count > 0)
                linesTreeView.Nodes.RemoveAt(index);

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

            UpdateVertexNumbers();
        }

        private void UpdateVertexNumbers()
        {
            for(int i = 0; i < verticesTreeView.Nodes.Count; i++)
                verticesTreeView.Nodes[i].Text = $"Vertex {i + 1} ({((Collision)currentEntry).verts[i].X},{((Collision)currentEntry).verts[i].Y})";
            for(int i = 0; i < linesTreeView.Nodes.Count; i++)
                linesTreeView.Nodes[i].Text = $"Line {i + 1}";
        }

        //Open bone selector for object rigging
        private void boneRigSelectButton_Click(object sender, EventArgs e)
        {
            BoneRiggingSelector brs = new BoneRiggingSelector(currentEntry.boneName);
            brs.ModelContainers = MainForm.Instance.GetActiveViewport().meshList.GetModelContainers();
            brs.ShowDialog();
            if (!brs.Cancelled)
                currentEntry.boneName = brs.currentValue;

            string boneNameRigging = currentEntry.boneName;
            if (boneNameRigging.Length == 0)
                boneNameRigging = "None";
            boneRigSelectButton.Text = boneNameRigging;
        }

        //selecting something in the sections tab of the item spawner editor
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            LVDShape section = (LVDShape)e.Node.Tag;
            itemSpawnVertTreeView.Nodes.Clear();
            int i = 1;
            foreach (Vector2 v in section.points)
                itemSpawnVertTreeView.Nodes.Add(new TreeNode($"Point {i++} ({v.X},{v.Y})") { Tag = v });
        }

        //selecting something in the vertices tab of the item spawner editor
        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            LVDShape shape = (LVDShape)itemSpawnSectionTreeView.SelectedNode.Tag;
            int selectionIndex = itemSpawnVertTreeView.SelectedNode.Index;
            Vector2 vert = shape.points[selectionIndex];

            numericUpDown2.Value = (decimal)vert.X;
            numericUpDown1.Value = (decimal)vert.Y;
        }

        //Add section
        private void addSectionButton_Click(object sender, EventArgs e)
        {
            LVDShape section = new LVDShape(4);
            
            TreeNode node = new TreeNode($"Section {itemSpawnSectionTreeView.Nodes.Count + 1}") { Tag = section };
            ((ItemSpawner)currentEntry).sections.Add(section);
            itemSpawnSectionTreeView.Nodes.Add(node);
        }

        //Remove section
        private void removeSectionButton_Click(object sender, EventArgs e)
        {
            LVDShape section = (LVDShape)itemSpawnSectionTreeView.SelectedNode.Tag;
            TreeNode node = itemSpawnSectionTreeView.SelectedNode;
            ((ItemSpawner)currentEntry).sections.Remove(section);
            itemSpawnSectionTreeView.Nodes.Remove(node);
            int i = 1;
            foreach (TreeNode n in itemSpawnSectionTreeView.Nodes)
                n.Text = $"Section {i++}";
        }

        //Add item spawner vertex
        private void addItemSpawnButton_Click(object sender, EventArgs e)
        {
            LVDShape shape = (LVDShape)itemSpawnSectionTreeView.SelectedNode.Tag;

            int selectionIndex;
            if (itemSpawnVertTreeView.SelectedNode != null)
                selectionIndex = itemSpawnVertTreeView.SelectedNode.Index + 1;
            else
                selectionIndex = shape.points.Count;

            shape.points.Insert(selectionIndex, new Vector2());
            itemSpawnVertTreeView.Nodes.Insert(selectionIndex, new TreeNode());

            renameItemSpawnVertTreeview();
        }

        //Delete item spawner vertex
        private void removeItemSpawnButton_Click(object sender, EventArgs e)
        {
            LVDShape shape = (LVDShape)itemSpawnSectionTreeView.SelectedNode.Tag;
            if (shape.points.Count == 0)
                return;

            int selectionIndex;
            if (itemSpawnVertTreeView.SelectedNode != null)
                selectionIndex = itemSpawnVertTreeView.SelectedNode.Index;
            else
                selectionIndex = shape.points.Count - 1;
            
            shape.points.RemoveAt(selectionIndex);
            itemSpawnVertTreeView.Nodes.RemoveAt(selectionIndex);

            renameItemSpawnVertTreeview();
        }

        //changed either X or Y pos of item spawner vertex
        private void changeItemVertPosition(object sender, EventArgs e)
        {
            LVDShape shape = (LVDShape)itemSpawnSectionTreeView.SelectedNode.Tag;
            int selectionIndex = itemSpawnVertTreeView.SelectedNode.Index;
            Vector2 vert = shape.points[selectionIndex];

            if (sender == numericUpDown2)
                vert.X = (float)numericUpDown2.Value;
            if(sender == numericUpDown1)
                vert.Y = (float)numericUpDown1.Value;

            shape.points[selectionIndex] = vert;
            renameItemSpawnVertTreeview();
        }

        private void renameItemSpawnVertTreeview()
        {
            LVDShape shape = (LVDShape)itemSpawnSectionTreeView.SelectedNode.Tag;

            for (int i = 0; i < shape.points.Count; ++i)
                itemSpawnVertTreeView.Nodes[i].Text = $"Point {i + 1} ({shape.points[i].X},{shape.points[i].Y})";
        }

        //General point editing
        private void pointShape_ValueChanged(object sender, EventArgs e)
        {
            GeneralPoint point = (GeneralPoint)currentEntry;

            if (sender == pointShapeXUpDown)
                point.x = (float)pointShapeXUpDown.Value;
            if (sender == pointShapeYUpDown)
                point.y = (float)pointShapeYUpDown.Value;
            if (sender == pointShapeZUpDown)
                point.z = (float)pointShapeZUpDown.Value;
        }

        private void rectValueChanged(object sender, EventArgs e)
        {
            GeneralShape shape = (GeneralShape)currentEntry;

            if (sender == rectLowerX)
                shape.x1 = (float)rectLowerX.Value;
            if (sender == rectLowerY)
                shape.y1 = (float)rectLowerY.Value;
            if(sender == rectUpperX)
                shape.x2 = (float)rectUpperX.Value;
            if (sender == rectUpperY)
                shape.y2 = (float)rectUpperY.Value;
        }

        private void treeViewPath_AfterSelect(object sender, TreeViewEventArgs e)
        {
            GeneralShape shape = (GeneralShape)currentEntry;
            int selectionIndex = treeViewPath.SelectedNode.Index;
            Vector2 vert = shape.points[selectionIndex];

            pathNodeX.Value = (decimal)vert.X;
            pathNodeY.Value = (decimal)vert.Y;
        }

        private void pathValueChanged(object sender, EventArgs e)
        {
            GeneralShape shape = (GeneralShape)currentEntry;
            int selectionIndex = treeViewPath.SelectedNode.Index;
            Vector2 vert = shape.points[selectionIndex];

            if (sender == pathNodeX)
                vert.X = (float)pathNodeX.Value;
            if (sender == pathNodeY)
                vert.Y = (float)pathNodeY.Value;

            shape.points[selectionIndex] = vert;
            renamePathTreeview();
        }

        private void renamePathTreeview()
        {
            GeneralShape shape = (GeneralShape)currentEntry;

            for (int i = 0; i < shape.points.Count; ++i)
                treeViewPath.Nodes[i].Text = $"Point {i + 1} ({shape.points[i].X},{shape.points[i].Y})";
        }

        //Add path point
        private void addPathPointButton_Click(object sender, EventArgs e)
        {
            GeneralShape shape = (GeneralShape)currentEntry;

            int selectionIndex;
            if (treeViewPath.SelectedNode != null)
                selectionIndex = treeViewPath.SelectedNode.Index + 1;
            else
                selectionIndex = shape.points.Count;

            shape.points.Insert(selectionIndex, new Vector2());
            treeViewPath.Nodes.Insert(selectionIndex, new TreeNode());
            renamePathTreeview();
        }

        //Remove path point
        private void removePathPointButton_Click(object sender, EventArgs e)
        {
            GeneralShape shape = (GeneralShape)currentEntry;
            if (shape.points.Count == 0)
                return;

            int selectionIndex;
            if (treeViewPath.SelectedNode != null)
                selectionIndex = treeViewPath.SelectedNode.Index;
            else
                selectionIndex = shape.points.Count - 1;

            shape.points.RemoveAt(selectionIndex);
            treeViewPath.Nodes.RemoveAt(selectionIndex);
            renamePathTreeview();
        }

        #region meleeCollisions
        private DAT.COLL_DATA.AreaTableEntry currentAreaTableEntry;
        private DAT.COLL_DATA.Link currentLink;
        private Vector2 currentMeleeVert;

        private void updateVertexPosition(object sender, EventArgs e)
        {
            if (currentMeleeVert == null)
                return;
            if (sender == meleeX)
                currentMeleeVert.X = (float)meleeX.Value;
            if (sender == meleeY)
                currentMeleeVert.Y = (float)meleeY.Value;
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
                currentMeleeVert = (Vector2)meleeVerts.SelectedNode.Tag;
                meleeX.Value = (Decimal)currentMeleeVert.X;
                meleeY.Value = (Decimal)currentMeleeVert.Y;
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
                comboBox2.Text = Enum.GetName(typeof(CollisionMatType), currentLink.material);
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
            currentLink.material = (byte)Enum.Parse(typeof(CollisionMatType), comboBox2.Text);
        }
        #endregion

        private void meleeAddVert_Click(object sender, EventArgs e)
        {
            Vector2 vert = new Vector2() { X = (float)meleeX.Value, Y = (float)meleeY.Value };
            DAT.COLL_DATA coll = (DAT.COLL_DATA)currentEntry;
            coll.vertices.Add(vert);
            meleeVerts.Nodes.Add(new TreeNode($"Vertex {meleeVerts.Nodes.Count}") { Tag = vert });
        }

        private void meleeSubtractVert_Click(object sender, EventArgs e)
        {
            DAT.COLL_DATA coll = (DAT.COLL_DATA)currentEntry;
            Vector2 vert = (Vector2)meleeVerts.SelectedNode.Tag;
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
