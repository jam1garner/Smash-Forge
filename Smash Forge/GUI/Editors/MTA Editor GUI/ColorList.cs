using System;
using System.Drawing;
using System.Windows.Forms;

namespace SmashForge
{
    public partial class ColorList : UserControl
    {
        public ColorList()
        {
            InitializeComponent();
        }

        readonly Font consolas = new Font("Consolas", 11);

        public void Fill(MatData m)
        {
            //Display as color animation
            colorAnimList.Visible = true;
            colorAnimList.Items.Clear();
            int i = 0, max = (m.frames.Count - 1).ToString().Length;
            foreach (MatData.frame frame in m.frames)
            {
                Color c = Color.FromArgb((int)(frame.values[0] * 255), (int)(frame.values[1] * 255), (int)(frame.values[2] * 255), (int)(frame.values[3] * 255));
                ColorKeyframe ck = new ColorKeyframe(i++, c) { maxLength = max };
                colorAnimList.Items.Add(ck);
            }
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Draw the background of the ListBox control for each item.
            e.DrawBackground();
            if (e.Index == -1)
                return;
            //Figure out the bounds for the text and color box
            string text = colorAnimList.Items[e.Index].ToString();
            Rectangle textBounds = e.Bounds;
            Console.WriteLine(textBounds.Width);
            if (textBounds.Width > 20)
            {
                textBounds.Width -= 20;
            }

            Rectangle colorBounds = new Rectangle();
            colorBounds.X = textBounds.X + (int)e.Graphics.MeasureString(text, consolas).Width + 5;
            colorBounds.Y = textBounds.Y + 1;
            colorBounds.Height = textBounds.Height - 2;
            colorBounds.Width = 20;
            SolidBrush b = new SolidBrush(((ColorKeyframe)colorAnimList.Items[e.Index]).color);

            // Draw the current item text based on the current Font  
            // and the custom brush settings.
            e.Graphics.DrawString(text, consolas, Brushes.Black, textBounds, StringFormat.GenericDefault);

            //Draw the color rectangle
            e.Graphics.FillRectangle(b, colorBounds);
            e.Graphics.DrawRectangle(new Pen(Color.Black), colorBounds);

            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }
    }
}
