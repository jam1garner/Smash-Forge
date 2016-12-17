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
    public partial class LVDList : DockContent
    {
        public LVDList()
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
        public TreeNode enemyNode = new TreeNode("Enemy Spawners");

        public void fillList()
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

            if(Runtime.TargetLVD != null)
            {
                foreach (Collision c in Runtime.TargetLVD.collisions)
                {
                    collisionNode.Nodes.Add(new TreeNode(c.name) { Tag = c });
                }

                foreach (Point c in Runtime.TargetLVD.spawns)
                {
                    spawnNode.Nodes.Add(new TreeNode(c.name) { Tag = c });
                }

                foreach (Point c in Runtime.TargetLVD.respawns)
                {
                    respawnNode.Nodes.Add(new TreeNode(c.name) { Tag = c });
                }

                foreach (Bounds c in Runtime.TargetLVD.cameraBounds)
                {
                    camNode.Nodes.Add(new TreeNode(c.name) { Tag = c });
                }

                foreach (Bounds c in Runtime.TargetLVD.blastzones)
                {
                    deathNode.Nodes.Add(new TreeNode(c.name) { Tag = c });
                }

                foreach (ItemSpawner c in Runtime.TargetLVD.items)
                {
                    itemNode.Nodes.Add(new TreeNode(c.name) { Tag = c });
                }

                foreach (Point c in Runtime.TargetLVD.generalPoints)
                {
                    pointNode.Nodes.Add(new TreeNode(c.name) { Tag = c });
                }

                foreach (LVDGeneralShape s in Runtime.TargetLVD.generalShapes)
                {
                    shapeNode.Nodes.Add(new TreeNode(s.name) { Tag = s });
                }

                foreach (Sphere c in Runtime.TargetLVD.damageSpheres)
                {
                    hurtNode.Nodes.Add(new TreeNode(c.name) { Tag = c });
                }

                foreach (Capsule c in Runtime.TargetLVD.damageCapsules)
                {
                    hurtNode.Nodes.Add(new TreeNode(c.name) { Tag = c });
                }

                foreach (EnemyGenerator c in Runtime.TargetLVD.enemySpawns)
                {
                    enemyNode.Nodes.Add(new TreeNode(c.name) { Tag = c });
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if(e.Node.Level != 0)
            {
                MainForm.Instance.lvdEditor.open((LVDEntry)e.Node.Tag, e.Node);
            }
        }
    }
}
