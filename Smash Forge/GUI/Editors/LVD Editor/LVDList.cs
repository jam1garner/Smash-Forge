using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace SmashForge
{
    public partial class LvdList : DockContent
    {
        public LVD targetLvd;
        public LvdEditor lvdEditor;

        ContextMenu elementCm;
        ContextMenu collisionCm;
        public LvdList()
        {
            InitializeComponent();
            treeView1.Nodes.Add(collisionNode);
            treeView1.Nodes.Add(spawnNode);
            treeView1.Nodes.Add(respawnNode);
            treeView1.Nodes.Add(camNode);
            treeView1.Nodes.Add(deathNode);
            treeView1.Nodes.Add(itemNode);
            treeView1.Nodes.Add(shapeNode);
            treeView1.Nodes.Add(pointNode);
            treeView1.Nodes.Add(hurtNode);
            treeView1.Nodes.Add(enemyNode);

            //---------------------------------------------
            {
                collisionNode.ContextMenu = new ContextMenu();

                MenuItem addCollision = new MenuItem("Add New Collision");
                addCollision.Click += delegate
                {
                    targetLvd.collisions.Add(new Collision() { Name = "COL_00_NewCollision", Subname = "00_NewCollision" });
                    FillList();
                };
                collisionNode.ContextMenu.MenuItems.Add(addCollision);

                MenuItem genCliffs = new MenuItem("Regenerate Cliffs");
                genCliffs.Click += delegate
                {
                    foreach (Collision c in targetLvd.collisions)
                    {
                        LVD.GenerateCliffs(c);
                    }
                    FillList();
                };
                collisionNode.ContextMenu.MenuItems.Add(genCliffs);
            }
            //---------------------------------------------
            {
                spawnNode.ContextMenu = new ContextMenu();
                MenuItem add = new MenuItem("Add New Spawn");
                add.Click += delegate
                {
                    string newNum = (targetLvd.spawns.Count + 1).ToString().PadLeft(2, '0');
                    targetLvd.spawns.Add(new Spawn() { Name = "START_00_P" + newNum, Subname = "00_P" + newNum });
                    FillList();
                };
                spawnNode.ContextMenu.MenuItems.Add(add);
            }
            //---------------------------------------------
            {
                TreeNode node = respawnNode;
                node.ContextMenu = new ContextMenu();
                MenuItem add = new MenuItem("Add New Respawn");
                add.Click += delegate
                {
                    string newNum = (targetLvd.respawns.Count + 1).ToString().PadLeft(2, '0');
                    targetLvd.respawns.Add(new Spawn() { Name = "RESTART_00_P" + newNum, Subname = "00_P" + newNum });
                    FillList();
                };
                node.ContextMenu.MenuItems.Add(add);
            }
            //---------------------------------------------
            {
                TreeNode node = camNode;
                node.ContextMenu = new ContextMenu();
                MenuItem add = new MenuItem("Add New Camera Bounds");
                add.Click += delegate
                {
                    targetLvd.cameraBounds.Add(new Bounds() { Name = "CAMERA_00", Subname = "00" });
                    FillList();
                };
                node.ContextMenu.MenuItems.Add(add);
            }
            //---------------------------------------------
            {
                TreeNode node = deathNode;
                node.ContextMenu = new ContextMenu();
                MenuItem add = new MenuItem("Add New Blastzones");
                add.Click += delegate
                {
                    targetLvd.blastzones.Add(new Bounds() { Name = "DEATH_00", Subname = "00" });
                    FillList();
                };
                node.ContextMenu.MenuItems.Add(add);
            }
            //---------------------------------------------
            {
                TreeNode node = itemNode;
                node.ContextMenu = new ContextMenu();
                MenuItem add = new MenuItem("Add New Item Spawner");
                add.Click += delegate
                {
                    targetLvd.itemSpawns.Add(new ItemSpawner() { Name = "ItemPopup_NEW", Subname = "00_Item" });
                    FillList();
                };
                node.ContextMenu.MenuItems.Add(add);
            }
            //---------------------------------------------
            {
                TreeNode node = shapeNode;
                node.ContextMenu = new ContextMenu();

                MenuItem addPoint = new MenuItem("Add New General Shape (Point)");
                addPoint.Click += delegate
                {
                    targetLvd.generalShapes.Add(new GeneralShape(1) { Name = "GeneralPoint_NEW", Subname = "00_NEW"});
                    FillList();
                };
                node.ContextMenu.MenuItems.Add(addPoint);

                MenuItem addCircle = new MenuItem("Add New General Shape (Circle)");
                addCircle.Click += delegate
                {
                    targetLvd.generalShapes.Add(new GeneralShape(2) { Name = "GeneralCircle_NEW", Subname = "00_NEW"});
                    FillList();
                };
                node.ContextMenu.MenuItems.Add(addCircle);

                MenuItem addRect = new MenuItem("Add New General Shape (Rectangle)");
                addRect.Click += delegate
                {
                    targetLvd.generalShapes.Add(new GeneralShape(3) { Name = "GeneralRect_NEW", Subname = "00_NEW"});
                    FillList();
                };
                node.ContextMenu.MenuItems.Add(addRect);

                MenuItem addPath = new MenuItem("Add New General Shape (Path)");
                addPath.Click += delegate
                {
                    targetLvd.generalShapes.Add(new GeneralShape(4) { Name = "GeneralPath_NEW", Subname = "00_NEW"});
                    FillList();
                };
                node.ContextMenu.MenuItems.Add(addPath);
            }
            //---------------------------------------------
            {
                TreeNode node = pointNode;
                node.ContextMenu = new ContextMenu();
                MenuItem add = new MenuItem("Add New General Point");
                add.Click += delegate
                {
                    targetLvd.generalPoints.Add(new GeneralPoint() { Name = "GeneralPoint3D_NEW", Subname = "00_NEW" });
                    FillList();
                };
                node.ContextMenu.MenuItems.Add(add);
            }
            //---------------------------------------------
            {
                TreeNode node = hurtNode;
                node.ContextMenu = new ContextMenu();

                MenuItem addSphere = new MenuItem("Add New Damage Sphere");
                addSphere.Click += delegate
                {
                    targetLvd.damageShapes.Add(new DamageShape() { Name = "DamageeSphere_00_NEW", Subname = "00_NEW", type = 2 });
                    FillList();
                };
                node.ContextMenu.MenuItems.Add(addSphere);

                MenuItem addCapsule = new MenuItem("Add New Damage Capsule");
                addCapsule.Click += delegate
                {
                    targetLvd.damageShapes.Add(new DamageShape() { Name = "DamageeCapsule_00_NEW", Subname = "00_NEW", type = 3 });
                    FillList();
                };
                node.ContextMenu.MenuItems.Add(addCapsule);
            }
            //---------------------------------------------
            {
                TreeNode node = enemyNode;
                node.ContextMenu = new ContextMenu();
                MenuItem add = new MenuItem("Add New Enemy Generator");
                add.Click += delegate
                {
                    targetLvd.enemyGenerators.Add(new EnemyGenerator() { Name = "EnemyGenerator_NEW", Subname = "00_NEW" });
                    FillList();
                };
                node.ContextMenu.MenuItems.Add(add);
            }

            {
                elementCm = new ContextMenu();

                MenuItem delete = new MenuItem("Delete Entry");
                delete.Click += delegate
                {
                    DeleteSelected();
                };
                elementCm.MenuItems.Add(delete);
            }

            {
                collisionCm = new ContextMenu();

                MenuItem delete = new MenuItem("Delete Entry");
                delete.Click += delegate
                {
                    DeleteSelected();
                };
                collisionCm.MenuItems.Add(delete);

                MenuItem genPassthru = new MenuItem("Regenerate Passthrough Angles");
                genPassthru.Click += delegate
                {
                    if (treeView1.SelectedNode != null && treeView1.SelectedNode.Tag is Collision)
                    {
                        LVD.GeneratePassthroughs((Collision)treeView1.SelectedNode.Tag, true);
                    }
                };
                collisionCm.MenuItems.Add(genPassthru);

                MenuItem flipNormals = new MenuItem("Flip Passthrough Angles");
                flipNormals.Click += delegate
                {
                    if (treeView1.SelectedNode != null && treeView1.SelectedNode.Tag is Collision)
                    {
                        LVD.FlipPassthroughs((Collision)treeView1.SelectedNode.Tag);
                    }
                };
                collisionCm.MenuItems.Add(flipNormals);

                MenuItem genCliffs = new MenuItem("Regenerate Cliffs");
                genCliffs.Click += delegate
                {
                    if (treeView1.SelectedNode != null && treeView1.SelectedNode.Tag is Collision)
                    {
                        LVD.GenerateCliffs((Collision)treeView1.SelectedNode.Tag);
                    }
                    FillList();
                };
                collisionCm.MenuItems.Add(genCliffs);
            }

            treeView1.NodeMouseClick += (sender, args) => treeView1.SelectedNode = args.Node;
        }

        public TreeNode collisionNode = new TreeNode("Collisions");
        public TreeNode spawnNode = new TreeNode("Spawns");
        public TreeNode respawnNode = new TreeNode("Respawns");
        public TreeNode camNode = new TreeNode("Camera Bounds");
        public TreeNode deathNode = new TreeNode("Death Bounds");
        public TreeNode itemNode = new TreeNode("Item Spawners");
        public TreeNode shapeNode = new TreeNode("General Shapes");
        public TreeNode pointNode = new TreeNode("General Points");
        public TreeNode hurtNode = new TreeNode("Hurtboxes");
        public TreeNode enemyNode = new TreeNode("Enemy Generators");

        public void FillList()
        {
            collisionNode.Nodes.Clear();
            spawnNode.Nodes.Clear();
            respawnNode.Nodes.Clear();
            camNode.Nodes.Clear();
            deathNode.Nodes.Clear();
            itemNode.Nodes.Clear();
            pointNode.Nodes.Clear();
            hurtNode.Nodes.Clear();
            enemyNode.Nodes.Clear();
            shapeNode.Nodes.Clear();

            if(targetLvd != null)
            {
                foreach (Collision c in targetLvd.collisions)
                {
                    TreeNode newNode = new TreeNode(c.Name) { Tag = c, ContextMenu = collisionCm };
                    foreach (CollisionCliff d in c.cliffs)
                    {
                        newNode.Nodes.Add(new TreeNode(d.Name) { Tag = d, ContextMenu = elementCm });
                    }
                    collisionNode.Nodes.Add(newNode);
                }

                foreach (Spawn c in targetLvd.spawns)
                {
                    spawnNode.Nodes.Add(new TreeNode(c.Name) { Tag = c, ContextMenu = elementCm });
                }

                foreach (Spawn c in targetLvd.respawns)
                {
                    respawnNode.Nodes.Add(new TreeNode(c.Name) { Tag = c, ContextMenu = elementCm });
                }

                foreach (Bounds c in targetLvd.cameraBounds)
                {
                    camNode.Nodes.Add(new TreeNode(c.Name) { Tag = c, ContextMenu = elementCm });
                }

                foreach (Bounds c in targetLvd.blastzones)
                {
                    deathNode.Nodes.Add(new TreeNode(c.Name) { Tag = c, ContextMenu = elementCm });
                }

                foreach (ItemSpawner c in targetLvd.itemSpawns)
                {
                    itemNode.Nodes.Add(new TreeNode(c.Name) { Tag = c, ContextMenu = elementCm });
                }

                foreach (GeneralPoint c in targetLvd.generalPoints)
                {
                    pointNode.Nodes.Add(new TreeNode(c.Name) { Tag = c, ContextMenu = elementCm });
                }

                foreach (GeneralShape c in targetLvd.generalShapes)
                {
                    shapeNode.Nodes.Add(new TreeNode(c.Name) { Tag = c, ContextMenu = elementCm });
                }

                foreach (DamageShape c in targetLvd.damageShapes)
                {
                    hurtNode.Nodes.Add(new TreeNode(c.Name) { Tag = c, ContextMenu = elementCm });
                }

                foreach (EnemyGenerator c in targetLvd.enemyGenerators)
                {
                    enemyNode.Nodes.Add(new TreeNode(c.Name) { Tag = c, ContextMenu = elementCm });
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level != 0)
            {
                targetLvd.LVDSelection = e.Node.Tag;
                //MainForm.Instance.viewports[0].timeSinceSelected.Restart();
                lvdEditor.Open((LvdEntry)e.Node.Tag, e.Node);
            }
        }

        public void DeleteNode(TreeNode treeNode)
        {
            if (!(treeNode.Tag is LvdEntry))
                return;
            LvdEntry entry = (LvdEntry)treeNode.Tag;

            if (entry is Collision)
                targetLvd.collisions.Remove((Collision)entry);
            if (entry is CollisionCliff)
                targetLvd.collisions[treeView1.SelectedNode.Parent.Index].cliffs.Remove((CollisionCliff)entry);
            if (entry is Spawn)
            {
                targetLvd.respawns.Remove((Spawn)entry);
                targetLvd.spawns.Remove((Spawn)entry);
            }
            if (entry is Bounds)
            {
                targetLvd.blastzones.Remove((Bounds)entry);
                targetLvd.cameraBounds.Remove((Bounds)entry);
            }
            if (entry is DamageShape)
                targetLvd.damageShapes.Remove((DamageShape)entry);
            if (entry is EnemyGenerator)
                targetLvd.enemyGenerators.Remove((EnemyGenerator)entry);
            if (entry is GeneralShape)
                targetLvd.generalShapes.Remove((GeneralShape)entry);
            if (entry is ItemSpawner)
                targetLvd.itemSpawns.Remove((ItemSpawner)entry);
            if (entry is GeneralPoint)
                targetLvd.generalPoints.Remove((GeneralPoint)entry);

            treeView1.Nodes.Remove(treeNode);
        }

        public void DeleteSelected()
        {
            if (treeView1.SelectedNode == null || !(treeView1.SelectedNode.Tag is LvdEntry))
                return;

            DeleteNode(treeView1.SelectedNode);
        }

        private void treeView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'd')
            {
                DialogResult r =
                        MessageBox.Show(
                            "Are you sure you want to delete this entry?\nThis cannot be undone.",
                            "Delete Selected Entry", MessageBoxButtons.YesNo);
                if (r == DialogResult.Yes)
                {
                    DeleteSelected();
                }
            }
        }
    }
}
