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

namespace SmashForge
{
    public partial class DatStageList : DockContent
    {
        private DAT dat;

        public DatStageList(DAT dat)
        {
            InitializeComponent();
            this.dat = dat;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            MainForm.Instance.lvdEditor.Open((LVDEntry)e.Node.Tag, new TreeNode());
        }

        private void DAT_stage_list_Load(object sender, EventArgs e)
        {
            if (dat.collisions != null)
                treeView1.Nodes.Add(new TreeNode("Collisions") { Tag = dat.collisions });

            if (dat.blastzones != null)
                treeView1.Nodes.Add(new TreeNode("Blastzones") { Tag = dat.blastzones });

            if (dat.cameraBounds != null)
                treeView1.Nodes.Add(new TreeNode("Camera Bounds") { Tag = dat.cameraBounds });

            if (dat.spawns != null)
            {
                int spawnNum = 0;
                foreach (Point spawn in dat.spawns)
                    treeView1.Nodes.Add(new TreeNode($"Spawn {spawnNum++}") { Tag = spawn });
            }

            if (dat.respawns != null)
            {
                int spawnNum = 0;
                foreach (Point spawn in dat.respawns)
                    treeView1.Nodes.Add(new TreeNode($"Respawn {spawnNum++}") { Tag = spawn });
            }

            if (dat.itemSpawns != null)
            {
                int spawnNum = 0;
                foreach (Point spawn in dat.itemSpawns)
                    treeView1.Nodes.Add(new TreeNode($"Item Spawner {spawnNum++}") { Tag = spawn });
            }

            if (dat.targets != null)
            {
                int spawnNum = 0;
                foreach (Point target in dat.targets)
                    treeView1.Nodes.Add(new TreeNode($"Target {spawnNum++}") { Tag = target });
            }
        }
    }
}
