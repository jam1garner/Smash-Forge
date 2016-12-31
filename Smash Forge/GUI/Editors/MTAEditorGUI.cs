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
            colorAnimList.Visible = false;
            matPropList.Visible = false;
            if (e.Node == null)
                return;
            if (e.Node == headerNode)
            {
                //If they selected the header node

            }
            if (e.Node.Parent == null)
                return;
            if (e.Node.Parent == matNode)
            {
                //If they selected a material header

            }
            if (e.Node.Parent.Parent == null)
                return;
            if (e.Node.Parent.Parent == matNode)
            {
                //If they selected a material property
                MatData m = (MatData)e.Node.Tag;
                if(m.name == "effColorGain" || m.name == "colorGain" || m.name == "aoColorGain" || m.name == "finalColorGain")
                {
                    //Display as color animation
                    colorAnimList.Visible = true;
                    colorAnimList.Items.Clear();
                    int i = 0, max = (m.frames.Count - 1).ToString().Length;
                    foreach (MatData.frame frame in m.frames)
                    {
                        Color c = Color.FromArgb((int)(frame.values[0] * 255), (int)(frame.values[1] * 255), (int)(frame.values[2] * 255), (int)(frame.values[3] * 255));
                        colorKeyframe ck = new colorKeyframe(i++, c) { maxLength = max };
                        colorAnimList.Items.Add(ck);
                    }
                }
                else
                {
                    matPropList.Visible = true;
                    matPropList.Items.Clear();
                    int i = 0, max = (m.frames.Count - 1).ToString().Length;
                    foreach (MatData.frame frame in m.frames)
                    {
                        standardKeyframe s = new standardKeyframe(i++, frame, m.name) { maxLength = max };
                        matPropList.Items.Add(s);
                    }
                }
            }
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node == null || e.Node.Parent == null)
                return;
            if (e.Node.Parent.Parent == matNode)
            {
                //If they selected a material property
                MatData m = (MatData)e.Node.Tag;
                Console.WriteLine(m.name);
                m.name = e.Label;
                e.Node.Text = e.Label;
                Console.WriteLine(m.name);
            }
            treeView1_AfterSelect(treeView1, new TreeViewEventArgs(e.Node));
        }

        private void colorAnimList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void matPropList_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Draw the background of the ListBox control for each item.
            e.DrawBackground();
            if (e.Index == -1)
                return;
            //Figure out the bounds for the text and color box
            string text = matPropList.Items[e.Index].ToString();
            Rectangle textBounds = e.Bounds;

            // Draw the current item text based on the current Font  
            // and the custom brush settings.
            e.Graphics.DrawString(text, CONSOLAS, Brushes.Black, textBounds, StringFormat.GenericDefault);

            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
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

    public class standardKeyframe
    {
        public int frame;
        public MatData.frame data;
        public string type;
        public int maxLength;

        public standardKeyframe(int frame, MatData.frame data, string type)
        {
            this.frame = frame;
            this.data = data;
            this.type = type;
        }

        public override string ToString()
        {
            if (type.Equals("NU_colorSamplerUV"))
                return $"[{frame.ToString().PadLeft(maxLength)}] X:{data.values[2].ToString("#0.000").PadLeft(6)} Y:{data.values[3].ToString("#0.000").PadLeft(6)} W:{data.values[0].ToString("#0.000").PadLeft(6)} H:{data.values[1].ToString("#0.000").PadLeft(6)}";

            string r = $"[{frame.ToString().PadLeft(maxLength)}]";
            int i = 64;
            foreach (float value in data.values)
                r += $" {(char)++i}:{value.ToString("#0.000").PadLeft(6)}";
            return r;
        }
    }
}
