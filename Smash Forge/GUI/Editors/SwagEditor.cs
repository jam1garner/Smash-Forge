using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SmashForge
{
    public partial class SwagEditor : EditorBase
    {
        public SwagEditor(SB swag)
        {
            InitializeComponent();
            this.Swag = swag;
            buttons = new BoneButton[] {boneButton2, boneButton3, boneButton4, boneButton5, boneButton6, boneButton7, boneButton8, boneButton9};
            boneButton1.BoneChanged += new EventHandler(BoneChange);
            foreach(BoneButton b in buttons)
                b.BoneChanged += new EventHandler(BoneChange);

            FilePath = "";
            Text = "New Swing Bone";
            Edited = false;
        }

        private SB Swag { get { return swag; } set { swag = value; vbn = ((VBN)swag.Parent); } }
        private SB swag;
        
        private VBN vbn;
        private BoneButton[] buttons;
        private bool dontChange = false;

        public override void Save()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                SaveAs();
                return;
            }
            FileOutput o = new FileOutput();
            byte[] n = Swag.Rebuild();
            o.WriteBytes(n);
            o.Save(FilePath);
            Edited = false;
        }

        public override void SaveAs()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Physics Bones (.sb)|*.sb|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".sb"))
                    {
                        FilePath = sfd.FileName;
                        Save();
                    }
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dontChange = true;
            SB.SBEntry sbEntry = (SB.SBEntry)listBox1.SelectedItem;
            if (sbEntry == null)
                return;
            boneButton1.vbn = vbn;
            boneButton1.SetBone(vbn.GetBone(sbEntry.hash));
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].vbn = vbn;
                Bone bone = vbn.GetBone(sbEntry.boneHashes[i]);
                buttons[i].SetBone(bone);
            }
            xMin.Value = (Decimal)sbEntry.rx1;
            xMax.Value = (Decimal)sbEntry.rx2;
            yMin.Value = (Decimal)sbEntry.ry1;
            yMax.Value = (Decimal)sbEntry.ry2;
            zMin.Value = (Decimal)sbEntry.rz1;
            zMax.Value = (Decimal)sbEntry.rz2;
            weightBox.Value = (Decimal)sbEntry.factor;
            numericUpDown1.Value = (Decimal)sbEntry.param1_1;
            numericUpDown2.Value = (Decimal)sbEntry.param1_2;
            numericUpDown3.Value = (Decimal)sbEntry.param1_3;
            numericUpDown4.Value = (Decimal)sbEntry.param2_1;
            numericUpDown5.Value = (Decimal)sbEntry.param2_2;
            numericUpDown6.Value = (Decimal)sbEntry.param2_3;
            numericUpDown7.Value = (Decimal)sbEntry.unks1[0];
            numericUpDown8.Value = (Decimal)sbEntry.unks1[1];
            numericUpDown9.Value = (Decimal)sbEntry.unks1[2];
            numericUpDown10.Value = (Decimal)sbEntry.unks1[3];
            numericUpDown11.Value = (Decimal)sbEntry.unks2[0];
            numericUpDown12.Value = (Decimal)sbEntry.unks2[1];
            numericUpDown13.Value = (Decimal)sbEntry.unks2[2];
            numericUpDown14.Value = (Decimal)sbEntry.unks2[3];
            numericUpDown15.Value = (Decimal)sbEntry.unks2[4];
            numericUpDown16.Value = (Decimal)sbEntry.unks2[5];
            numericUpDown17.Value = (Decimal)sbEntry.ints[0];
            numericUpDown18.Value = (Decimal)sbEntry.ints[1];
            numericUpDown19.Value = (Decimal)sbEntry.ints[2];
            dontChange = false;
        }

        private void SwagEditor_Load(object sender, EventArgs e)
        {
            foreach (SB.SBEntry swagEntry in Swag.bones)
                listBox1.Items.Add(swagEntry);
            if (listBox1.Items.Count >= 1)
                listBox1.SelectedItem = listBox1.Items[0];
        }

        private void BoneChange(object sender, EventArgs e)
        {
            SB.SBEntry sbEntry = (SB.SBEntry) listBox1.SelectedItem;
            if (sbEntry == null)
                return;
            sbEntry.hash = boneButton1.boneId;
            for (int i = 0; i < 8; i++)
                sbEntry.boneHashes[i] = buttons[i].boneId;
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            if (!dontChange)
            {
                SB.SBEntry sbEntry = (SB.SBEntry)listBox1.SelectedItem;
                if (sbEntry == null)
                    return;
                sbEntry.rx1 = (float)xMin.Value;
                sbEntry.rx2 = (float)xMax.Value;
                sbEntry.ry1 = (float)yMin.Value;
                sbEntry.ry2 = (float)yMax.Value;
                sbEntry.rz1 = (float)zMin.Value;
                sbEntry.rz2 = (float)zMax.Value;
                sbEntry.factor = (float)weightBox.Value;
                sbEntry.param1_1 = (float)numericUpDown1.Value;
                sbEntry.param1_2 = (int)numericUpDown2.Value;
                sbEntry.param1_3 = (int)numericUpDown3.Value;
                sbEntry.param2_1 = (float)numericUpDown4.Value;
                sbEntry.param2_2 = (int)numericUpDown5.Value;
                sbEntry.param2_3 = (int)numericUpDown6.Value;
                sbEntry.unks1[0] = (float)numericUpDown7.Value;
                sbEntry.unks1[1] = (float)numericUpDown8.Value;
                sbEntry.unks1[2] = (float)numericUpDown9.Value;
                sbEntry.unks1[3] = (float)numericUpDown10.Value;
                sbEntry.unks2[0] = (float)numericUpDown11.Value;
                sbEntry.unks2[1] = (float)numericUpDown12.Value;
                sbEntry.unks2[2] = (float)numericUpDown13.Value;
                sbEntry.unks2[3] = (float)numericUpDown14.Value;
                sbEntry.unks2[4] = (float)numericUpDown15.Value;
                sbEntry.unks2[5] = (float)numericUpDown16.Value;
                sbEntry.ints[0] = (int)numericUpDown17.Value;
                sbEntry.ints[1] = (int)numericUpDown18.Value;
                sbEntry.ints[2] = (int)numericUpDown19.Value;
            }
        }

        private void addEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SB.SBEntry newEntry = new SB.SBEntry();
            Swag.bones.Add(newEntry);
            listBox1.Items.Add(newEntry);
            Edited = true;
        }

        private void removeEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Swag.bones.Remove((SB.SBEntry)listBox1.SelectedItem);
            listBox1.Items.Remove(listBox1.SelectedItem);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (listBox1.SelectedItem == null)
                removeEntryToolStripMenuItem.Visible = false;
            else
                removeEntryToolStripMenuItem.Visible = true;
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }
    }
}
