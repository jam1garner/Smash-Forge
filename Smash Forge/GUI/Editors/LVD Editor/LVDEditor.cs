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
    public partial class LVDEditor : DockContent
    {
        public LVDEditor()
        {
            InitializeComponent();
        }

        private LVDEntry currentEntry;
        private Vector2D currentVert;
        private Vector2D currentNormal;
        private CollisionMat currentMat;

        enum mat : byte
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

        public void open(LVDEntry entry)
        {
            currentEntry = entry;
            collisionGroup.Visible = false;
            name.Text = currentEntry.name;
            subname.Text = currentEntry.subname;
            if (entry is Collision)
            {
                Collision col = (Collision)entry;
                collisionGroup.Visible = true;
                flag1.Checked = col.flag1;
                flag2.Checked = col.flag2;
                flag3.Checked = col.flag3;
                flag4.Checked = col.flag4;
                for (int i = 0; i < col.verts.Count; i++)
                    vertices.Nodes.Add(new TreeNode($"Vertex {i}") { Tag = col.verts[i] });
                for (int i = 0; i < col.normals.Count; i++)
                {
                    object[] temp = { col.normals[i], col.materials[i] };
                    lines.Nodes.Add(new TreeNode($"Line {i}") { Tag = temp });
                }
                    
            }
        }

        private void vertices_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currentVert = (Vector2D)e.Node.Tag;
            xtext.Text = $"{currentVert.x}";
            ytext.Text = $"{currentVert.y}";
        }

        private void lines_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currentNormal = (Vector2D)((object[])e.Node.Tag)[0];
            currentMat = (CollisionMat)((object[])e.Node.Tag)[1];
            leftLedge.Checked = currentMat.leftLedge();
            rightLedge.Checked = currentMat.rightLedge();
            noWallJump.Checked = currentMat.noWallJump();
            comboBox1.Text = Enum.GetName(typeof(mat), currentMat.physics());
            passthroughAngle.Value = (decimal)(Math.Atan2(currentNormal.y, currentNormal.x) * 180.0 / Math.PI);
        }
    }
}
