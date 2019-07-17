using System.Windows.Forms;
using System.IO;

namespace SmashForge
{
    public partial class Nus3BankEditor : Form
    {
        private NUS3BANK selected;
        private FileSystemWatcher fw;
        //private Dictionary<NUT.NUD_Texture, string> fileFromTexture = new Dictionary<NUT.NUD_Texture, string>();
        //private Dictionary<string, NUT.NUD_Texture> textureFromFile = new Dictionary<string, NUT.NUD_Texture>();
        private bool dontModify;

        public Nus3BankEditor()
        {
            InitializeComponent();
            FillForm();
            soundListBox.ContextMenuStrip = contextMenuStrip1;
        }

        public void FillForm()
        {
            bankListBox.Items.Clear();
            soundListBox.Items.Clear();
            foreach (NUS3BANK n in Runtime.soundContainers)
            {
                bankListBox.Items.Add(n);
            }
        }

        public void SelectBank(NUS3BANK b)
        {
            selected = b;

            soundListBox.Items.Clear();
            foreach (NUS3BANK.NUS_TONE.ToneMeta meta in b.tone.tones)
            {
                soundListBox.Items.Add(meta);
            }
        }

        private void bankListBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (bankListBox.SelectedIndex >= 0)
            {
                SelectBank((NUS3BANK)bankListBox.SelectedItem);
            }
        }

        private void soundListBox_DoubleClick(object sender, System.EventArgs e)
        {
            // Play the sound

            if (soundListBox.SelectedIndex >= 0)
            {
                ((NUS3BANK.NUS_TONE.ToneMeta)soundListBox.SelectedItem).Play();
            }

        }

        private void soundListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                soundListBox.SelectedIndex = soundListBox.IndexFromPoint(e.Location);
                if (soundListBox.SelectedIndex != -1)
                {
                    contextMenuStrip1.Show();
                }
            }
        }

        private void iDSPToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            NUS3BANK.NUS_TONE.ToneMeta meta = null;
            if (selected != null && soundListBox.SelectedIndex >= 0)
            {
                meta = (NUS3BANK.NUS_TONE.ToneMeta)soundListBox.SelectedItem;
            }
            if (meta != null)
                using (var sfd = new SaveFileDialog())
            {
                sfd.FileName = meta.name;
                sfd.Filter = "IDSP (.idsp)|*.idsp|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".idsp") && meta != null)
                    {
                        File.WriteAllBytes(sfd.FileName, meta.idsp);
                    }
                }
            }
        }

        private void wAVToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            NUS3BANK.NUS_TONE.ToneMeta meta = null;
            if (selected != null && soundListBox.SelectedIndex >= 0)
            {
                meta = (NUS3BANK.NUS_TONE.ToneMeta)soundListBox.SelectedItem;
            }
            if (meta != null)
            using (var sfd = new SaveFileDialog())
            {
                sfd.FileName = meta.name;
                sfd.Filter = "WAVE (.wav)|*.wav|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".wav") && meta != null)
                    {
                        File.WriteAllBytes(sfd.FileName, WAVE.FromIDSP(meta.idsp));
                    }
                }
            }
        }
    }
}
