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
using System.IO;

namespace Smash_Forge
{
    public partial class MTAEditor : EditorBase
    {
        public MTAEditor(MTA Mta)
        {
            InitializeComponent();
            mta = Mta;
        }

        public MTA mta;

        private void MTAEditor_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = mta.Decompile();
        }

        public override void Save()
        {
            if (FilePath.Equals(""))
            {
                SaveAs();
                return;
            }
            FileOutput o = new FileOutput();
            mta.Compile(new List<string>(richTextBox1.Text.Split('\n')));
            byte[] n = mta.Rebuild();
            o.writeBytes(n);
            o.save(FilePath);
            Edited = false;
        }

        public override void SaveAs()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Material Animation (.mta)|*.mta|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".mta"))
                    {
                        FilePath = sfd.FileName;
                        Save();
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //try
            //{
            mta = new MTA();
            mta.Compile(new List<string>(richTextBox1.Text.Split('\n')));
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Material Animation (.mta)|*.mta|" +
                                "All Files (*.*)|*.*";
                    
                if(sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(sfd.FileName,mta.Rebuild());
                }
            }
            //}
            /*catch (Exception ex)
            {
                throw;
                Console.WriteLine(ex.ToString());
                MessageBox.Show("Failed to build MTA, make sure your formatting is correct", "MTA Build Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }*/
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mta = new MTA();
            mta.Compile(new List<string>(richTextBox1.Text.Split('\n')));
            MainForm.Instance.viewports[0].loadMTA(mta);
        }
    }
}
