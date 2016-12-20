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

        LVDEditor.StringWrapper str;
        char[] initialValue;
        char[] currentValue;

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            textBox1.Text = e.Node.Text;
            currentValue = e.Node.Text.ToArray();
            List<char> newValue = new List<char>(currentValue);
            while (newValue.Count < 0x40)
                newValue.Add((char)0);
            currentValue = newValue.ToArray();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            str.data = initialValue;
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
            Close();
        }

        private void BoneRiggingSelector_Load(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            foreach(ModelContainer model in Runtime.ModelContainers)
            {
                foreach(Bone b in model.vbn.bones)
                {
                    treeView1.Nodes.Add(new TreeNode(new string(b.boneName)) { Tag = b.boneName });
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
