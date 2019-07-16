using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmashForge.Gui
{
    public partial class MatPropList : UserControl
    {
        public MatPropList()
        {
            InitializeComponent();
        }

        readonly Font CONSOLAS = new Font("Consolas", 11);
        private MatData property;

        private void matPropList_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Draw the background of the ListBox control for each item.
            e.DrawBackground();
            if (e.Index == -1)
                return;
            //Figure out the bounds for the text and color box
            string text = listBox1.Items[e.Index].ToString();
            Rectangle textBounds = e.Bounds;

            // Draw the current item text based on the current Font  
            // and the custom brush settings.
            e.Graphics.DrawString(text, CONSOLAS, Brushes.Black, textBounds, StringFormat.GenericDefault);

            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }

        public void fill(MatData m)
        {
            property = m;
            listBox1.Items.Clear();
            int i = 0, max = (m.frames.Count - 1).ToString().Length;
            foreach (MatData.frame frame in m.frames)
            {
                standardKeyframe s = new standardKeyframe(i++, frame, m.name) { maxLength = max };
                listBox1.Items.Add(s);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                return;

            MatData.frame frame = ((standardKeyframe)listBox1.SelectedItem).data;
            numericUpDown1.Value = (Decimal)frame.values[0];
            numericUpDown2.Value = (Decimal)frame.values[1];
            numericUpDown3.Value = (Decimal)frame.values[2];
            numericUpDown4.Value = (Decimal)frame.values[3];
        }

        private void valueChanged(object sender, EventArgs e)
        {
            foreach (var item in listBox1.SelectedItems)
            {
                if (sender == numericUpDown1)
                    ((standardKeyframe)item).data.values[0] = (float)numericUpDown1.Value;
                if (sender == numericUpDown2)
                    ((standardKeyframe)item).data.values[1] = (float)numericUpDown2.Value;
                if (sender == numericUpDown3)
                    ((standardKeyframe)item).data.values[2] = (float)numericUpDown3.Value;
                if (sender == numericUpDown4)
                    ((standardKeyframe)item).data.values[3] = (float)numericUpDown4.Value;
            }
        }
    }
}
