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

namespace Smash_Forge.GUI.Menus
{
    public partial class OdysseyCostumeSelector : Form
    {
        public List<CostumeData> Costumelist = new List<CostumeData>();

        public List<string> ExcludeFileList = new List<string>(new string[] {
       "Eye","Face", "Head", "HeadTexture","Under","HandL","HandR","HandTexture", "BodyTexture","Hair","2D","Cap","Tail","Ruck",
       "aHakama","Skirt","Shell","PonchoPoncho","PonchoGuitar"
        });

        public OdysseyCostumeSelector()
        {
            InitializeComponent();

            if (Runtime.MarioOdysseyGamePath == "")
            {
                MessageBox.Show("Game path not configured!");

                FolderSelectDialog ofd = new FolderSelectDialog();

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = ofd.SelectedPath;
                    Runtime.MarioOdysseyGamePath = folderPath;
                    Config.Save();
                }
            }

            string[] dirs = Directory.GetFiles($"{Runtime.MarioOdysseyGamePath}\\ObjectData", "Mario*");

            foreach (string dir in dirs)
            {

                string filename = Path.GetFileNameWithoutExtension(dir);

                bool Exluded = ExcludeFileList.Any(filename.Contains);

                if (Exluded == false)
                {
                    CostumeData cd = new CostumeData();

                    listBox1.Items.Add(filename);

                    cd.FullPath = dir;
                    Costumelist.Add(cd);
                }      
            }
        }

        public class CostumeData
        {
            public string FullPath = "";
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {


        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {


        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            MainForm form = (MainForm)this.Owner; // get the owner of this form

            int index = this.listBox1.IndexFromPoint(e.Location);

            if (index != ListBox.NoMatches)
            {
                form.LoadCostumes(Costumelist[index].FullPath);

                this.Close();
            }
        }
    }
}
