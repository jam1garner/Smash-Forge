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
    public partial class AddBone : Form
    {
        public MainForm otherForm;

        public AddBone(MainForm mainForm)
        {
            otherForm = mainForm;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
			if (textBox1.Text.Equals (""))
				return;

            Bone temp = new Bone();
			temp.boneName = textBox1.Text.ToCharArray();
			if(!textBox2.Text.Equals(""))
            	temp.boneId = (uint)int.Parse(textBox2.Text, System.Globalization.NumberStyles.HexNumber);

			if(!textBox3.Text.Equals(""))
				temp.boneType = Convert.ToUInt32(textBox3.Text);
			
            if (Runtime.TargetVBN.bones.Count > 0)
                temp.parentIndex = 0;
            else
                temp.parentIndex = 0x0FFFFFFF;
            temp.children = new List<int>();
            temp.position = new float[] {0f,0f,0f};
            temp.rotation = new float[] { 0f,0f,0f};
            temp.scale = new float[] { 1f,1f,1f};
            Runtime.TargetVBN.bones.Add(temp);
            Runtime.TargetVBN.totalBoneCount++;
            Runtime.TargetVBN.boneCountPerType[temp.boneType]++;
            Runtime.TargetVBN.updateChildren();
            Runtime.TargetVBN.reset();
            otherForm.leftPanel.treeRefresh();
            Close();
        }
    }
}
