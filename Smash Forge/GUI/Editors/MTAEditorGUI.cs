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
    public partial class MTAEditorGUI : DockContent
    {
        public MTAEditorGUI(MTA mta)
        {
            InitializeComponent();
            this.mta = mta;
        }

        private void MTAEditorGUI_Load(object sender, EventArgs e)
        {
            treeView1.Nodes.Add(headerNode);
            treeView1.Nodes.Add(matNode);
            treeView1.Nodes.Add(visNode);
            foreach(MatEntry m in mta.matEntries)
            {
                TreeNode mNode = new TreeNode(m.name) { Tag = m };
                matNode.Nodes.Add(mNode);
                foreach(MatData md in m.properties)
                    mNode.Nodes.Add(new TreeNode(md.name) { Tag = md });
            }
            foreach (VisEntry v in mta.visEntries)
                visNode.Nodes.Add(new TreeNode(v.name) { Tag = v });
        }

        readonly Font CONSOLAS = new Font("Consolas", 11);
        private MTA mta;
        private TreeNode matNode = new TreeNode("Materials");
        private TreeNode visNode = new TreeNode("Visibility");
        private TreeNode headerNode = new TreeNode("Header");

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
            if(textBounds.Width > 20)
            {
                textBounds.Width -= 20;
            }
            
            Rectangle colorBounds = new Rectangle();
            colorBounds.X = textBounds.X + (int)e.Graphics.MeasureString(text, CONSOLAS).Width + 5;
            colorBounds.Y = textBounds.Y + 1;
            colorBounds.Height = textBounds.Height - 2;
            colorBounds.Width = 20;
            SolidBrush b = new SolidBrush(((colorKeyframe)colorAnimList.Items[e.Index]).color);

            // Draw the current item text based on the current Font  
            // and the custom brush settings.
            e.Graphics.DrawString(text, CONSOLAS, Brushes.Black, textBounds, StringFormat.GenericDefault);
            
            //Draw the color rectangle
            e.Graphics.FillRectangle(b, colorBounds);
            e.Graphics.DrawRectangle(new Pen(Color.Black), colorBounds);

            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if(e.Node.Parent.Parent == matNode)
            {
                //If they selected a material property

            }
            else if(e.Node.Parent == matNode)
            {
                //If they selected a material header

            }
            else if(e.Node == headerNode)
            {
                //If they selected the header node

            }
        }
    }

    public class colorKeyframe
    {
        public Color color;
        public int frame;
        public int maxLength;

        public colorKeyframe(int frame, Color color)
        {
            this.color = color;
            this.frame = frame;
        }

        public override string ToString()
        {
            return $"[{frame.ToString().PadLeft(maxLength)}] R:{color.R.ToString().PadLeft(3)} G:{color.G.ToString().PadLeft(3)} B:{color.B.ToString().PadLeft(3)} A:{color.A.ToString().PadLeft(3)}";
        }
    }
}
