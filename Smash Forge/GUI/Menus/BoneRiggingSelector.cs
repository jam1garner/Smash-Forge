using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge
{
    public partial class BoneRiggingSelector : Form
    {
        public BoneRiggingSelector(LVDEditor.StringWrapper s)
        {
            InitializeComponent();
            str = s;
            initialValue = str.data;
            textBox1.Text = charsToString(initialValue);
            currentValue = initialValue;
        }

        private static string charsToString(char[] c)
        {
            string boneNameRigging = "";
            foreach (char b in c)
                if (b != (char)0)
                    boneNameRigging += b;
            return boneNameRigging;
        }

        public LVDEditor.StringWrapper str;
        public char[] initialValue;
        public char[] currentValue;
        public short boneIndex = -1;
        public bool Cancelled = false;
        public bool SelectedNone = false;
        public Bone CurrentBone = null;

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            textBox1.Text = e.Node.Text;
            currentValue = e.Node.Text.ToArray();
            List<char> newValue = new List<char>(currentValue);
            while (newValue.Count < 0x40)
                newValue.Add((char)0);
            currentValue = newValue.ToArray();
            CurrentBone = (Bone) ((object[]) e.Node.Tag)[1];
            boneIndex = (short)((VBN)((object[]) e.Node.Tag)[0]).bones.IndexOf(CurrentBone);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            str.data = initialValue;
            Cancelled = true;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            str.data = currentValue;
            if (str.data.Length != 0x40)
                throw new IndexOutOfRangeException("Wrong number of characters in bone name\nSomething must have gone terribly wrong");
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            str.data = new char[0x40];
            boneIndex = -1;
            CurrentBone = null;
            SelectedNone = true;
            Close();
        }

        private void BoneRiggingSelector_Load(object sender, EventArgs e)
        {
            //List<ModelContainer> m = Runtime.ModelContainers;
            //Console.WriteLine($"Model Count: {Runtime.ModelContainers.Count}");
            treeView1.Nodes.Clear();
            List<VBN> alreadyUsedVbns = new List<VBN>();
            foreach(ModelContainer model in Runtime.ModelContainers)
            {
                if (!alreadyUsedVbns.Contains(model.vbn) && model.vbn != null)
                {
                    alreadyUsedVbns.Add(model.vbn);
                    foreach (Bone b in model.vbn.bones)
                    {
                        object[] objs = { model.vbn, b };
                        TreeNode temp = new TreeNode(b.Text) {Tag = objs};
                        treeView1.Nodes.Add(temp);
                        if (str.data == b.Text.ToCharArray())
                            treeView1.SelectedNode = temp;
                    }
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            List<char> newValue = new List<char>();
            foreach (char c in textBox1.Text)
                newValue.Add(c);
            while (newValue.Count < 0x40)
                newValue.Add((char)0);
            currentValue = newValue.ToArray();
        }
    }
}
