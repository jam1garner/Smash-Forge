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
                            f.Endian = Endianness.Little;
                            f.writeInt(0);
                            f.writeInt(0x30 + rom.Length);
                            f.writeInt(0x30);
                            f.writeInt(0x10 + rom.Length);
                            f.writeInt(0x30 + rom.Length);
                            f.writeInt(0);
                            f.writeInt(0x1B + rom.Length);
                            f.writeInt(0x1B + rom.Length);
                            f.WriteString("JAM WAS HERE");
                            f.writeInt(0);
                            f.writeBytes(rom);
                            f.writeHex("3C00000000001000090002080000000100000000000000000000000000000000");
                            f.save(sfd.FileName);
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
                                f.writeByte(rom[i]);
                            f.save(sfd.FileName);
                        }
                    }
                }
            }
        }

        private readonly RemoteAPI api = ApiSource.API;

        private void apiEnabled()
        {
            //grab save state info
        }

        private void createSaveState(object sender, EventArgs e)
        {
            
            ApiSource.initRemoteAPI("localhost", 9999);
            api.addActivateListener(apiEnabled);
        }
    }
}
