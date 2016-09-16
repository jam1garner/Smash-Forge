using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VBN_Editor
{
    public partial class VBNRebuilder : Form
    {
        VBN vbn = new VBN();

        public VBNRebuilder()
        {
            InitializeComponent();
        }
        
        private TreeNode buildBoneTree(int index)
        {
            List<TreeNode> children = new List<TreeNode>();
            foreach (int i in vbn.bones[index].children)
            {
                children.Add(buildBoneTree(i));
            }
            
            TreeNode temp = new TreeNode(new string(vbn.bones[index].boneName),children.ToArray());

            if (index == 0)
                treeView1.Nodes.Add(temp);

            return temp;

        }

        private void openNUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = "";
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Smash 4 Boneset|*.vbn|All files(*.*)|*.*";
            DialogResult result = save.ShowDialog();

            if(result == DialogResult.OK)
            {
                filename = save.FileName;
                vbn.save(filename);
            }
        }

        private void openVBNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = "";
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Smash 4 Boneset|*.vbn|All files(*.*)|*.*";
            DialogResult result = open.ShowDialog();

            if(result == DialogResult.OK)
            {
                filename = open.FileName;
                vbn = new VBN(filename);
            }
            buildBoneTree(0);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newForm = new Form2 ();
            newForm.ShowDialog();
        }

    }
}
