using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MeleeLib.DAT.Script;

namespace Smash_Forge.GUI.Melee
{
    public partial class MeleeCMDEditor : Form
    {
        private DatFighterScript Script;

        public MeleeCMDEditor(DatFighterScript Script)
        {
            InitializeComponent();
            this.Script = Script;
            foreach(SubAction a in Script.SubActions)
            {
                richTextBox1.Text += MeleeCMD.DecompileSubAction(a) + "\n";
            }
        }

        // from https://www.c-sharpcorner.com/blogs/creating-line-numbers-for-richtextbox-in-c-sharp
        /*public void AddLineNumbers()
        {
            // create & set Point pt to (0,0)    
            Point pt = new Point(0, 0);
            // get First Index & First Line from richTextBox1    
            int First_Index = richTextBox1.GetCharIndexFromPosition(pt);
            int First_Line = richTextBox1.GetLineFromCharIndex(First_Index);
            // set X & Y coordinates of Point pt to ClientRectangle Width & Height respectively    
            pt.X = ClientRectangle.Width;
            pt.Y = ClientRectangle.Height;
            // get Last Index & Last Line from richTextBox1    
            int Last_Index = richTextBox1.GetCharIndexFromPosition(pt);
            int Last_Line = richTextBox1.GetLineFromCharIndex(Last_Index);
            // set Center alignment to LineNumberTextBox    
            LineNumberTextBox.SelectionAlignment = HorizontalAlignment.Center;
            // set LineNumberTextBox text to null & width to getWidth() function value    
            LineNumberTextBox.Text = "";
            LineNumberTextBox.Width = getWidth();
            // now add each line number to LineNumberTextBox upto last line    
            for (int i = First_Line; i <= Last_Line + 2; i++)
            {
                LineNumberTextBox.Text += i + 1 + "\n";
            }
        }*/
        
        public void HighlightLine(RichTextBox richTextBox, int index, Color color)
        {
            ClearHighlights(richTextBox);
            var lines = richTextBox.Lines;
            if (index < 0 || index >= lines.Length)
                return;
            var start = richTextBox.GetFirstCharIndexFromLine(index);  // Get the 1st char index of the appended text
            var length = lines[index].Length;
            richTextBox.Select(start, length);                 // Select from there to the end
            richTextBox.SelectionBackColor = color;
        }

        public void ClearHighlights(RichTextBox richTextBox)
        {
            richTextBox.SelectAll();
            richTextBox.SelectionBackColor = richTextBox.BackColor;
        }

        private void buttonCompile_Click(object sender, EventArgs e)
        {
            string[] actionsrc = richTextBox1.Text.Split('\n');
            ClearHighlights(richTextBox1);

            List<SubAction> actions = new List<SubAction>();
            int line = 0;
            foreach(string a in actionsrc)
            {
                if (a.Equals("")) continue;
                CompileError err;
                SubAction comp = null;
                try
                {
                    comp = MeleeCMD.CompileCommand(a, out err);
                }
                catch (Exception)
                {
                    err = CompileError.Syntax;
                };

                switch (err)
                {
                    case CompileError.None:
                        actions.Add(comp);
                        break;
                    case CompileError.Syntax:
                        HighlightLine(richTextBox1, line, Color.PaleVioletRed);
                        MessageBox.Show("Syntax error on line " + line);
                        return;
                    case CompileError.ParameterCount:
                        HighlightLine(richTextBox1, line, Color.PaleVioletRed);
                        MessageBox.Show("Too many or too few parameters on line " + line);
                        return;
                    case CompileError.UnknownCommand:
                        HighlightLine(richTextBox1, line, Color.PaleVioletRed);
                        MessageBox.Show("Unknown Command on line " + line);
                        return;
                }
                line++;
            }
            MessageBox.Show("Compiled Successfully");
            Script.SubActions = actions;
        }
    }
}
