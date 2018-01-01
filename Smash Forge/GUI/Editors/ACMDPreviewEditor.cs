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
using SALT.Moveset.AnimCMD;
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

        public ModelViewport Owner;

        public ACMDPreviewEditor()
        {
            InitializeComponent();
        }

        public void SetManualScript(uint crc)
        {
            bool changed = false;
            if (cb_section.Text.Equals("GAME") && Owner.MovesetManager.Game.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.DecompileCommands((ACMDScript)Owner.MovesetManager.Game.Scripts[crc]);
                changed = true;
            }
            if (cb_section.Text.Equals("SOUND") && Owner.MovesetManager.Sound.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.DecompileCommands((ACMDScript)Owner.MovesetManager.Sound.Scripts[crc]);
                changed = true;
            }
            if (cb_section.Text.Equals("EXPRESSION") && Owner.MovesetManager.Expression.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.DecompileCommands((ACMDScript)Owner.MovesetManager.Expression.Scripts[crc]);
                changed = true;
            }
            if (cb_section.Text.Equals("EFFECT") && Owner.MovesetManager.Effect.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.DecompileCommands((ACMDScript)Owner.MovesetManager.Effect.Scripts[crc]);
                changed = true;
            }

            if (Owner.MovesetManager.ScriptsHashList.Contains(crc))
                Owner.scriptId = Owner.MovesetManager.ScriptsHashList.IndexOf(crc);

            if (Owner.MovesetManager.Game.Scripts.ContainsKey(crc))
            {
                Owner.ACMDScript = new ForgeACMDScript((ACMDScript)Owner.MovesetManager.Game.Scripts[crc]);
                //if (Runtime.vbnViewport != null && Runtime.TargetAnim != null)
                //    Runtime.vbnViewport.setAnimMaxFrames(Runtime.TargetAnim);
            }

            if (changed)
            {
                this.crc = crc;
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
            if (cb_section.Text.Equals("GAME") && Owner.MovesetManager.Game.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.DecompileCommands((ACMDScript)Owner.MovesetManager.Game.Scripts[crc]);
                changed = true;
            }
            if (cb_section.Text.Equals("SOUND") && Owner.MovesetManager.Sound.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.DecompileCommands((ACMDScript)Owner.MovesetManager.Sound.Scripts[crc]);
                changed = true;
            }
            if (cb_section.Text.Equals("EXPRESSION") && Owner.MovesetManager.Expression.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.DecompileCommands((ACMDScript)Owner.MovesetManager.Expression.Scripts[crc]);
                changed = true;
            }
            if (cb_section.Text.Equals("EFFECT") && Owner.MovesetManager.Effect.Scripts.ContainsKey(crc))
            {
                richTextBox1.Text = ACMDDecompiler.DecompileCommands((ACMDScript)Owner.MovesetManager.Effect.Scripts[crc]);
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
            if(Owner.MovesetManager != null && cb_section.SelectedIndex >= 0)
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

                    SortedList<uint, SALT.Moveset.IScript> scriptList = null;
                    if (cb_section.Text.Equals("GAME"))
                        scriptList = Owner.MovesetManager.Game.Scripts;
                    else if (cb_section.Text.Equals("SOUND"))
                        scriptList = Owner.MovesetManager.Sound.Scripts;
                    else if (cb_section.Text.Equals("EXPRESSION"))
                        scriptList = Owner.MovesetManager.Expression.Scripts;
                    else if (cb_section.Text.Equals("EFFECT"))
                        scriptList = Owner.MovesetManager.Effect.Scripts;

                    //Update script if it already exists
                    if (scriptList.ContainsKey(crc))
                        scriptList[crc] = script;

                    if (cb_section.Text.Equals("GAME"))
                    {
                        Owner.ACMDScript = new ForgeACMDScript(script);
                        Owner.ACMDScript.processToFrame(0);
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
            if (Owner.MovesetManager != null)
            {
                //Get unique crc
                crcs = Owner.MovesetManager.ScriptsHashList.Distinct().ToList();
                for (int i = 0; i < crcs.Count; i++)
                {
                    string s = "";
                    //if (Runtime.Animnames.ContainsKey(crcs[i]))
                    //    s = Runtime.Animnames[crcs[i]] + " - ";

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
