using System;
using System.Windows.Forms;

namespace SmashForge
{
    public partial class BoneButton : Button
    {
        public BoneButton()
        {
            this.Click += new EventHandler(Clicked);
            InitializeComponent();
        }

        private void Clicked(object sender, EventArgs e)
        {
            BoneRiggingSelector brs;
            if (bone == null)
                brs = new BoneRiggingSelector();
            else
                brs = new BoneRiggingSelector(bone.Text);

            brs.ModelContainers.Add(new ModelContainer() { VBN = vbn});
            brs.CurrentBone = bone;
            brs.ShowDialog();
            if (!brs.Cancelled)
            {
                if (brs.SelectedNone)
                {
                    bone = null;
                    boneId = noBone;
                }
                else
                {
                    if (brs.CurrentBone == null)
                    {
                        MessageBox.Show("Please select a bone or hit 'None'/'Cancel'", "Bone Selector issue",
                            MessageBoxButtons.OK);
                        boneId = noBone;
                    }
                    else
                    {
                        bone = brs.CurrentBone;
                        boneId = bone.boneId;
                    }
                }
                OnBoneChanged(new EventArgs());
            }
            if (bone == null)
                Text = "None";
            else
                Text = bone.Text;
        }

        public VBN vbn;
        private Bone bone = null;
        public uint noBone = 3449071621;
        public uint boneId;

        public event EventHandler BoneChanged;

        protected virtual void OnBoneChanged(EventArgs e)
        {
            if (BoneChanged != null)
                BoneChanged(this, e);
        }

        private static string CharsToString(char[] c)
        {
            string boneNameRigging = "";
            foreach (char b in c)
                if (b != (char)0)
                    boneNameRigging += b;
            return boneNameRigging;
        }

        public void SetBone(Bone bone)
        {
            if (bone == null)
            {
                boneId = noBone;
                Text = "None";
                return;
            }
            this.bone = bone;
            Text = bone.Text;
            boneId = bone.boneId;
        }
    }
}
