using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmashForge
{
    public partial class BoneRiggingSelector : Form
    {
        public string currentValue = "";
        public Bone CurrentBone = null;
        public short boneIndex = -1;

        public List<ModelContainer> ModelContainers = new List<ModelContainer>();

        public bool Cancelled = false;
        public bool SelectedNone = false;

        public BoneRiggingSelector()
        {
            InitializeComponent();
        }
        public BoneRiggingSelector(string s) : this()
        {
            currentValue = String.Copy(s);
        }
        public BoneRiggingSelector(short i) : this()
        {
            boneIndex = i;
        }

        private void BoneRiggingSelector_Load(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            List<VBN> alreadyUsedVbns = new List<VBN>();
            foreach (ModelContainer model in ModelContainers)
            {
                if (model.VBN == null || alreadyUsedVbns.Contains(model.VBN))
                    continue;

                alreadyUsedVbns.Add(model.VBN);
                foreach (Bone b in model.VBN.bones)
                {
                    object[] objs = { model.VBN, b };
                    TreeNode temp = new TreeNode(b.Text) {Tag = objs};
                    treeView1.Nodes.Add(temp);
                }
            }

            //Set the initial selection based on if bone index, bone name, or neither, was provided and matches one in the list.
            //I'd like to have no node selected if neither was provided or matches one in the list,
            //but the treeview automatically selects the first node on initialization if the selection is null and the list of nodes isn't empty.
            treeView1.SelectedNode = null;
            textBox1.Text = "";
            if (boneIndex > -1 && boneIndex < treeView1.Nodes.Count)
                treeView1.SelectedNode = treeView1.Nodes[boneIndex];
            else if (!String.IsNullOrEmpty(currentValue))
            {
                foreach (TreeNode node in treeView1.Nodes)
                {
                    if (node.Text == currentValue)
                    {
                        treeView1.SelectedNode = node;
                        break;
                    }
                }
                if (treeView1.SelectedNode == null)
                {
                    textBox1.Text = String.Copy(currentValue);
                }
            }
        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currentValue = String.Copy(e.Node.Text);
            textBox1.Text = String.Copy(currentValue);
            CurrentBone = (Bone) ((object[]) e.Node.Tag)[1];
            boneIndex = (short)((VBN)((object[]) e.Node.Tag)[0]).bones.IndexOf(CurrentBone);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (currentValue == textBox1.Text)
                return;
            currentValue = String.Copy(textBox1.Text);
            CurrentBone = null;
            boneIndex = -1;

            treeView1.SelectedNode = null;
            foreach (TreeNode node in treeView1.Nodes)
            {
                if (node.Text == currentValue)
                {
                    //This will trigger AfterSelect
                    treeView1.SelectedNode = node;
                    break;
                }
            }
        }

        //Button: Cancel
        private void button3_Click(object sender, EventArgs e)
        {
            Cancelled = true;
            Close();
        }

        //Button: Select Bone
        private void button2_Click(object sender, EventArgs e)
        {
            if (currentValue.Length > 0x40)
                throw new IndexOutOfRangeException("Wrong number of characters in bone name\nSomething must have gone terribly wrong");
            currentValue = currentValue.PadRight(0x40, (char)0);
            Close();
        }

        //Button: No Bone
        private void button1_Click(object sender, EventArgs e)
        {
            currentValue = new string(new char[0x40]);
            CurrentBone = null;
            boneIndex = -1;

            SelectedNone = true;
            Close();
        }

    }
}
