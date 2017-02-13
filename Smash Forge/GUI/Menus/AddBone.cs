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
        public AddBone(Bone parentBone = null)
        {
            InitializeComponent();
            this.parent = parentBone;
        }

        private Bone parent = null;

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.ToLower().Equals("make pichu plz"))
            {
                using (FolderBrowserDialog ofd = new FolderBrowserDialog())
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        Pichu.MakePichu(ofd.SelectedPath);
                    }
                }
                Close();
                return;
            }
            else if (textBox1.Text.Equals("THRoW"))
            {
                //If you are reading this he kidnapped me and made me do it
                System.Diagnostics.Process.Start("https://twitter.com/realheroofwinds");
                Close();
                return;
            }

            if (textBox1.Text.Equals (""))
				return;

            Bone temp = new Bone(Runtime.TargetVBN);
			temp.boneName = textBox1.Text.ToCharArray();
			if(!textBox2.Text.Equals(""))
            	temp.boneId = (uint)int.Parse(textBox2.Text, System.Globalization.NumberStyles.HexNumber);

			if(!textBox3.Text.Equals(""))
				temp.boneType = Convert.ToUInt32(textBox3.Text);

            if (Runtime.TargetVBN == null)
                Runtime.TargetVBN = new VBN();

            if (Runtime.TargetVBN.bones.Count > 0)
                temp.ParentBone = parent;
            temp.position = new float[] {0f,0f,0f};
            temp.rotation = new float[] { 0f,0f,0f};
            temp.scale = new float[] { 1f,1f,1f};
            Runtime.TargetVBN.bones.Add(temp);
            Runtime.TargetVBN.totalBoneCount++;
            Runtime.TargetVBN.boneCountPerType[temp.boneType]++;
            Runtime.TargetVBN.reset();
            MainForm.Instance.boneTreePanel.treeRefresh();
            Close();
        }
    }
}
