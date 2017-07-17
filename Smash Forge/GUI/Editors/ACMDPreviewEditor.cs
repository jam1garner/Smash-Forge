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
using SALT.Scripting.AnimCMD;
using System.Text.RegularExpressions;

namespace Smash_Forge
{
    public partial class ACMDPreviewEditor : DockContent
    {
        public uint crc;
        public bool set = false;
        public bool manualCrc = false;
        private bool ignoreTextChangedEvent = false;
        private Dictionary<uint, string> crcDict = new Dictionary<uint, string>();

        public ACMDPreviewEditor()
        {
            InitializeComponent();
        }

        public void SetManualScript(uint crc)
        {
            bool changed = false;
            if (cb_section.Text.Equals("GAME") && Runtime.Moveset.Game.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.Decompile((ACMDScript)Runtime.Moveset.Game.Scripts[crc]);
                changed = true;
            }
            if (cb_section.Text.Equals("SOUND") && Runtime.Moveset.Sound.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.Decompile((ACMDScript)Runtime.Moveset.Sound.Scripts[crc]);
                changed = true;
            }
            if (cb_section.Text.Equals("EXPRESSION") && Runtime.Moveset.Expression.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.Decompile((ACMDScript)Runtime.Moveset.Expression.Scripts[crc]);
                changed = true;
            }
            if (cb_section.Text.Equals("EFFECT") && Runtime.Moveset.Effect.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.Decompile((ACMDScript)Runtime.Moveset.Effect.Scripts[crc]);
                changed = true;
            }

            this.crc = crc;
            if (Runtime.Moveset.ScriptsHashList.Contains(crc))
                Runtime.scriptId = Runtime.Moveset.ScriptsHashList.IndexOf(crc);

            if (Runtime.Moveset.Game.Scripts.ContainsKey(crc))
            {
                Runtime.gameAcmdScript = new ForgeACMDScript((ACMDScript)Runtime.Moveset.Game.Scripts[crc]);
            }

            if (changed)
            {
                set = true;

                //HighlightSyntax();
                //Update game script
                
            }
            else
            {
                richTextBox1.Text = "";
            }
        }

        public void SetAnimation(uint crc)
        {
            //If manually set the crc ignore viewport handleACMD
            if (manualCrc)
                return;

            //Activate flag to ignore selectedIndexChanged event when updating Text
            ignoreTextChangedEvent = true;
            if (crcDict.ContainsKey(crc))
                cb_crc.Text = crcDict[crc];

            bool changed = false;
            if (cb_section.Text.Equals("GAME") && Runtime.Moveset.Game.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.Decompile((ACMDScript)Runtime.Moveset.Game.Scripts[crc]);
                changed = true;
            }
            if (cb_section.Text.Equals("SOUND") && Runtime.Moveset.Sound.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.Decompile((ACMDScript)Runtime.Moveset.Sound.Scripts[crc]);
                changed = true;
            }
            if (cb_section.Text.Equals("EXPRESSION") && Runtime.Moveset.Expression.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.Decompile((ACMDScript)Runtime.Moveset.Expression.Scripts[crc]);
                changed = true;
            }
            if (cb_section.Text.Equals("EFFECT") && Runtime.Moveset.Effect.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.Decompile((ACMDScript)Runtime.Moveset.Effect.Scripts[crc]);
                changed = true;
            }
            if (changed)
            {
                set = true;
                this.crc = crc;

                //HighlightSyntax();
            }
            ignoreTextChangedEvent = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(Runtime.Moveset != null && cb_section.SelectedIndex >= 0)
            {
                // need to split into lines
                string[] line = richTextBox1.Text.Split('\n');
                ACMDScript script = new ACMDScript(crc);
                int index = 0;
                try
                {
                    foreach (string str in line)
                    {
                        if (str.Equals("")) continue;
                        ACMDCompiler.CompileSingleCommand(str); // try to compile
                        index++;
                    }
                    foreach (ACMDCommand s in ACMDCompiler.CompileCommands(line))
                    {
                        script.Add(s);
                        index++;
                    }

                    //Update script if it already exists
                    if (Runtime.Moveset.Game.Scripts.ContainsKey(crc))
                        Runtime.Moveset.Game.Scripts[crc] = script;

                    if (manualCrc)
                    {
                        //Crc was set manually, update gameScript to process script
                        Runtime.gameAcmdScript = new ForgeACMDScript(script);
                        Runtime.gameAcmdScript.processToFrame(0);
                    }
                } catch (Exception)
                {
                    HighlightLine(richTextBox1, index, Color.Red);
                }
            }
        }

        public void HighlightSyntax()
        {
            RichTextBox rtb = richTextBox1;
            rtb.SelectAll();
            rtb.SelectionBackColor = rtb.BackColor;
            rtb.SelectionColor = rtb.ForeColor;
            HighlightRegex(richTextBox1, "\\w+\\(", Color.Blue, 1);
            HighlightRegex(richTextBox1, "\\w+=", Color.Teal, 1);
            HighlightRegex(richTextBox1, "0x\\w+", Color.Green, 0);

            // check parathesis?
        }

        public void HighlightRegex(RichTextBox rtb, string reg, Color c, int ignore)
        {
            Regex regExp = new Regex(reg);
            foreach (Match match in regExp.Matches(rtb.Text))
            {
                rtb.Select(match.Index, match.Length-ignore);
                rtb.SelectionColor = c;
            }
        }

        public void HighlightLine(RichTextBox richTextBox, int index, Color color)
        {
            richTextBox.SelectAll();
            richTextBox.SelectionBackColor = richTextBox.BackColor;
            var lines = richTextBox.Lines;
            if (index < 0 || index >= lines.Length)
                return;
            var start = richTextBox.GetFirstCharIndexFromLine(index);  
            var length = lines[index].Length;
            richTextBox.Select(start, length);                 
            richTextBox.SelectionBackColor = color;
        }

        private void cb_section_TextUpdate(object sender, EventArgs e)
        {
            if(set)
                SetAnimation(crc);
        }

        private void updateSelection(object sender, EventArgs e)
        {
            if(set)
                SetAnimation(crc);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //HighlightSyntax();
        }

        public void updateCrcList()
        {
            cb_crc.Items.Clear();
            crcDict.Clear();
            List<uint> crcs = new List<uint>();
            List<string> list = new List<string>();
            if (Runtime.Moveset != null)
            {
                //Get unique crc
                crcs = Runtime.Moveset.ScriptsHashList.Distinct().ToList();
                for (int i = 0; i < crcs.Count; i++)
                {
                    string s = "";
                    if (Runtime.Animnames.ContainsKey(crcs[i]))
                        s = Runtime.Animnames[crcs[i]] + " - ";

                    s += $"0x{crcs[i].ToString("X8")}";

                    if (!crcDict.ContainsKey(crcs[i]))
                        crcDict.Add(crcs[i], s);

                    cb_crc.Items.Add(s);
                }
            }
        }

        private void updateCrc(object sender, EventArgs e)
        {
            //Ignore if this was done by setAnmation
            if (ignoreTextChangedEvent)
                return;

            manualCrc = true;
            string temp = cb_crc.Text.Substring(cb_crc.Text.Length - 8);
            uint ncrc = uint.Parse(cb_crc.Text.Substring(cb_crc.Text.Length-8), System.Globalization.NumberStyles.HexNumber);
            if (ncrc != crc)
                SetManualScript(ncrc);
        }
    }
}
