using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Smash_Forge
{
    public partial class SwagEditor : DockContent
    {
        public SwagEditor(SB swag)
        {
            InitializeComponent();
            this.swag = swag;
            buttons = new BoneButton[] {boneButton2, boneButton3, boneButton4, boneButton5, boneButton6, boneButton7, boneButton8, boneButton9};
            boneButton1.BoneChanged += new EventHandler(BoneChange);
            foreach(BoneButton b in buttons)
                b.BoneChanged += new EventHandler(BoneChange);
        }

        private SB swag;
        private BoneButton[] buttons;
        private bool dontChange = false;

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dontChange = true;
            SB.SBEntry sbEntry = (SB.SBEntry) listBox1.SelectedItem;
            boneButton1.SetBone(VBN.GetBone(sbEntry.hash));
            for (int i = 0; i < buttons.Length; i++)
            {
                Bone bone = VBN.GetBone(sbEntry.boneHashes[i]);
                buttons[i].SetBone(bone);
            }
            xMin.Value = (Decimal)sbEntry.rx1;
            xMax.Value = (Decimal)sbEntry.rx2;
            yMin.Value = (Decimal)sbEntry.ry1;
            yMax.Value = (Decimal)sbEntry.ry2;
            zMin.Value = (Decimal)sbEntry.rz1;
            zMax.Value = (Decimal)sbEntry.rz2;
            weightBox.Value = (Decimal) sbEntry.factor;
            dontChange = false;
        }

        private void SwagEditor_Load(object sender, EventArgs e)
        {
            foreach (SB.SBEntry swagEntry in swag.bones.Values)
                listBox1.Items.Add(swagEntry);
            if (listBox1.Items.Count >= 1)
                listBox1.SelectedItem = listBox1.Items[0];
        }

        private void BoneChange(object sender, EventArgs e)
        {
            ((SB.SBEntry)listBox1.SelectedItem).hash = boneButton1.boneId;
            for (int i = 0; i < 8; i++)
                ((SB.SBEntry) listBox1.SelectedItem).boneHashes[i] = buttons[i].boneId;
        }

        public void save()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Swag Bone Physics (.sb)|*.sb";

                if (sfd.ShowDialog() == DialogResult.OK)
                    swag.Save(sfd.FileName);
            }
        }

        private void valueChanged(object sender, EventArgs e)
        {
            if (!dontChange)
            {
                SB.SBEntry sbEntry = (SB.SBEntry)listBox1.SelectedItem;
                sbEntry.rx1 = (float) xMin.Value;
                sbEntry.rx2 = (float) xMax.Value;
                sbEntry.ry1 = (float) yMin.Value;
                sbEntry.ry2 = (float) yMax.Value;
                sbEntry.rz1 = (float) zMin.Value;
                sbEntry.rz2 = (float) zMax.Value;
                sbEntry.factor = (float) weightBox.Value;
            }
        }
    }
}
