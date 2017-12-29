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
        public AddBone(Bone parentBone = null, VBN vbn = null)
        {
            InitializeComponent();
            this.vbn = vbn;
            this.parent = parentBone;
        }

        private VBN vbn;
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

            Bone temp = new Bone(vbn);
			temp.Text = textBox1.Text;
			if(!textBox2.Text.Equals(""))
            	temp.boneId = (uint)int.Parse(textBox2.Text, System.Globalization.NumberStyles.HexNumber);

			if(!textBox3.Text.Equals(""))
				temp.boneType = Convert.ToUInt32(textBox3.Text);

            if (parent != null)
                parent.Nodes.Add(temp);
            //if (Runtime.TargetVBN.bones.Count > 0)
            //    temp.ParentBone = parent;

            temp.position = new float[] {0f,0f,0f};
            temp.rotation = new float[] { 0f,0f,0f};
            temp.scale = new float[] { 1f,1f,1f};
            vbn.bones.Add(temp);
            vbn.totalBoneCount++;
            vbn.boneCountPerType[temp.boneType]++;
            vbn.reset();
            //MainForm.Instance.boneTreePanel.treeRefresh();
            Close();
        }
    }
}
