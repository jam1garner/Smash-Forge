using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using WeifenLuo.WinFormsUI.Docking;

namespace SmashForge
{
    public partial class MoiEditor : DockContent
    {
        public MoiEditor(MOI moi)
        {
            InitializeComponent();
            this.moi = moi;
        }

        private MOI moi;

        private void MOIEditor_Load(object sender, EventArgs e)
        {
            foreach (MOI.Entry entry in moi.entries)
                listBox1.Items.Add(entry);
            foreach (MOI.Entry entry in moi.otherEntries)
                listBox2.Items.Add(entry);
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index == -1)
                return;
            e.Graphics.DrawString($"Entry {e.Index+1}",e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            MOI.Entry entry = (MOI.Entry)listBox1.SelectedItem;
            n1.Value = entry.values[0];
            n2.Value = entry.values[1];
            n3.Value = entry.values[2];
            n4.Value = entry.values[3];
            n5.Value = entry.values[4];
            n6.Value = entry.values[5];
            n7.Value = entry.values[6];
            n8.Value = entry.values[7];
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            MOI.Entry entry = (MOI.Entry)listBox2.SelectedItem;
            u1.Value = entry.values[0];
            u2.Value = entry.values[1];
        }



        private void button1_Click(object sender, EventArgs e)
        {
            using(SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Model Order Index (.moi)|*.moi";
                
                if(sfd.ShowDialog() == DialogResult.OK)
                {
                    byte[] rebuildMoi = moi.Rebuild();
                    File.WriteAllBytes(sfd.FileName, rebuildMoi);
                }
            }
        }

        private void ChangedValue(object sender, EventArgs e)
        {
            MOI.Entry entry;
            if (sender == u1 || sender == u2)
                entry = (MOI.Entry)listBox2.SelectedItem;
            else
                entry = (MOI.Entry)listBox1.SelectedItem;

            if(sender == n1)
                entry.values[0] = (int)n1.Value;
            if (sender == n2)
                entry.values[1] = (int)n2.Value;
            if (sender == n3)
                entry.values[2] = (int)n3.Value;
            if (sender == n4)
                entry.values[3] = (int)n4.Value;
            if (sender == n5)
                entry.values[4] = (int)n5.Value;
            if (sender == n6)
                entry.values[5] = (int)n6.Value;
            if (sender == n7)
                entry.values[6] = (int)n7.Value;
            if (sender == n8)
                entry.values[7] = (int)n8.Value;

            if (sender == u1)
                entry.values[0] = (int)u1.Value;
            if (sender == u2)
                entry.values[1] = (int)u2.Value;
        }
    }
}
