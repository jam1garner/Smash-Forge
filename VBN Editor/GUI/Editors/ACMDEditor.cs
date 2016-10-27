using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using WeifenLuo.WinFormsUI.Docking;

namespace VBN_Editor
{
    public partial class ACMDEditor : DockContent
    {
        public ACMDEditor(string filename,ProjectTree parent)
        {
            InitializeComponent();
            openFile(filename);
            parentProject = parent;
        }

        public string fname;
        private ProjectTree parentProject;

        private void openFile(string filename)
        {
            fname = filename;
            Text = Path.GetFileName(filename);
            richTextBox1.Text = File.ReadAllText(filename);
        }

        public void save()
        {
            using (StreamWriter f = new StreamWriter(fname))
            {
                f.Write(Encoding.ASCII.GetBytes(richTextBox1.Text));
                if(Text.EndsWith("*"))
                    Text = Text.Substring(0, Text.Length - 1);
                parentProject.build();
            }
        }

        private void edit(object sender, EventArgs e)
        {
            if (!Text.EndsWith("*"))
                Text += "*";
        }
    }
}
