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
    public partial class Form3 : Form
    {
        public VBNRebuilder otherForm;

        public Form3(VBNRebuilder mainForm)
        {
            otherForm = mainForm;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Bone temp = new Bone();
            temp.boneName = textBox1.Text.ToCharArray();
            temp.boneId = (uint)int.Parse(textBox2.Text, System.Globalization.NumberStyles.HexNumber);
            temp.boneType = Convert.ToUInt32(textBox3.Text);
            temp.parentIndex = 0;
            temp.children = new List<int>();
            temp.position = new float[] {0f,0f,0f};
            temp.rotation = new float[] { 0f,0f,0f};
            temp.scale = new float[] { 0f,0f,0f};
            otherForm.vbn.bones.Add(temp);
            otherForm.vbn.totalBoneCount++;
            otherForm.vbn.boneCountPerType[temp.boneType]++;
            otherForm.vbn.bones[0].children.Add((int)otherForm.vbn.totalBoneCount-1);
            otherForm.treeRefresh();
            Close();
        }
    }
}
