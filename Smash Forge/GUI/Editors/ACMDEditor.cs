using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using WeifenLuo.WinFormsUI.Docking;

namespace SmashForge
{
    public partial class AcmdEditor : DockContent
    {
        public AcmdEditor(string filename,ProjectTree parent)
        {
            InitializeComponent();
            OpenFile(filename);
            parentProject = parent;
        }

        public string fname;
        private ProjectTree parentProject;

        private void OpenFile(string filename)
        {
            fname = filename;
            Text = Path.GetFileName(filename);
            richTextBox1.Text = File.ReadAllText(filename);
        }

        public void Save()
        {
            using (StreamWriter f = new StreamWriter(fname))
            {
                f.Write(Encoding.ASCII.GetBytes(richTextBox1.Text));
                if(Text.EndsWith("*"))
                    Text = Text.Substring(0, Text.Length - 1);
                parentProject.Build();
            }
        }

        private void Edit(object sender, EventArgs e)
        {
            if (!Text.EndsWith("*"))
                Text += "*";
        }

        private void ACMDEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainForm.Instance.acmdEditors.Remove(this);
        }
    }
}
