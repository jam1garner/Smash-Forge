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
            if (otherForm.vbn.bones.Count > 0)
                temp.parentIndex = 0;
            else
                temp.parentIndex = 0x0FFFFFFF;
            temp.children = new List<int>();
            temp.position = new float[] {0f,0f,0f};
            temp.rotation = new float[] { 0f,0f,0f};
            temp.scale = new float[] { 1f,1f,1f};
            otherForm.vbn.bones.Add(temp);
            otherForm.vbn.totalBoneCount++;
            otherForm.vbn.boneCountPerType[temp.boneType]++;
            otherForm.vbn.updateChildren();
            otherForm.vbn.reset();
            otherForm.vbnSet = true;
            otherForm.treeRefresh();
            Close();
        }
    }
}
