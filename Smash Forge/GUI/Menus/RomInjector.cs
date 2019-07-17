using System;
using System.Windows.Forms;
using System.IO;
using Nintaco;

namespace SmashForge
{
    public partial class ROM_Injector : Form
    {
        public ROM_Injector()
        {
            InitializeComponent();
        }

        private void injectRom(object sender, EventArgs e)
        {
            using(OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select NES ROM";
                if(ofd.ShowDialog() == DialogResult.OK)
                {
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Title = "Select Save Location";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            byte[] rom = File.ReadAllBytes(ofd.FileName);
                            FileOutput f = new FileOutput();
                            f.endian = Endianness.Little;
                            f.WriteInt(0);
                            f.WriteInt(0x30 + rom.Length);
                            f.WriteInt(0x30);
                            f.WriteInt(0x10 + rom.Length);
                            f.WriteInt(0x30 + rom.Length);
                            f.WriteInt(0);
                            f.WriteInt(0x1B + rom.Length);
                            f.WriteInt(0x1B + rom.Length);
                            f.WriteString("JAM WAS HERE");
                            f.WriteInt(0);
                            f.WriteBytes(rom);
                            f.WriteHex("3C00000000001000090002080000000100000000000000000000000000000000");
                            f.Save(sfd.FileName);
                        }
                    }
                }
            }
        }

        private void extractRom(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select NES ROM";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Title = "Select Save Location";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            byte[] rom = File.ReadAllBytes(ofd.FileName);
                            FileOutput f = new FileOutput();
                            for (int i = 0x30; i < rom.Length - 0x20; i++)
                                f.WriteByte(rom[i]);
                            f.Save(sfd.FileName);
                        }
                    }
                }
            }
        }

        private readonly RemoteApi api = ApiSource.api;

        private void apiEnabled()
        {
            //grab save state info
        }

        private void createSaveState(object sender, EventArgs e)
        {
            
            ApiSource.InitRemoteApi("localhost", 9999);
            api.AddActivateListener(apiEnabled);
        }
    }
}
