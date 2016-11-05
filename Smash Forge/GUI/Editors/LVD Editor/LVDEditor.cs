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

        public void open(LVDEntry entry)
        {
            currentEntry = entry;
            collisionGroup.Enabled = false;
            name.Text = currentEntry.name;
            subname.Text = currentEntry.subname;
            if (entry is Collision)
            {
                Collision col = (Collision)entry;
                collisionGroup.Enabled = true;
                flag1.Checked = col.flag1;
                flag2.Checked = col.flag2;
                flag3.Checked = col.flag3;
                flag4.Checked = col.flag4;
                for (int i = 0; i < col.verts.Count; i++)
                    vertices.Nodes.Add(new TreeNode($"Vertex {i}") { Tag = col.verts[i] });
                for (int i = 0; i < col.normals.Count; i++)
                    lines.Nodes.Add(new TreeNode($"Line {i}") { Tag = col.normals[i] });
            }
        }
    }
}
