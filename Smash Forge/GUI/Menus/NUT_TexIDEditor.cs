using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge
{
    public partial class NUT_TexIDEditor : Form
    {

        public static int Running = 0;
        public static int Opened = 1;
        public static int Cancelled = 2;

        public string path = null;

        public int exitStatus = 0; //0 - not done, 1 - one is selected, 2 - cancelled

        public Dictionary<string, int> types = new Dictionary<string, int>()
        {
            {"Nothing", 0x00 },
            {"System Textures", 0x10 },
            {"Effects", 0x15 },
            {"Stage", 0x20 },
            {"Final Smash BGs", 0x27 },
            {"UI", 0x28 },
            {"Stage Editor", 0x2B },
            {"Fighter", 0x40 },
            {"Enemies", 0x42 },
            {"Items", 0x44 },
            {"Assist Trophy", 0x46 },
            {"Pokémon", 0x48 },
            {"Trophies 1", 0x4E },
            {"Trophies 2", 0x4F },
            {"Trophies 3", 0x50 },
            {"Trophies 4", 0x51 },
        };

        public Dictionary<string, int> characters = new Dictionary<string, int>()
        {
            {"Captain Falcon", 0x00 },
            {"King Dedede", 0x01 },
            {"Diddy Kong", 0x02 },
            {"Donkey Kong", 0x03 },
            {"Duck Hunt", 0x04 },
            {"Falco", 0x05 },
            {"Fox", 0x06 },
            {"Mr. Game and Watch", 0x07 },
            {"Ganondorf", 0x08 },
            {"Greninja", 0x30 },
            {"Ike", 0x09},
            {"Kirby", 0x39 },
            {"Kirby Hats ext)", 0x3A },
            {"Kirby Hats ext 2)", 0x3B },
            {"Kirby Hats ext 3)", 0x3C },
            {"Bowser", 0x0B },
            {"Giga Bowser", 0x32 },
            {"Bowser Jr", 0x0A },
            {"Link", 0x0C },
            {"Little Mac", 0x0D },
            {"Giga Mac", 0x33 },
            {"Charizard", 0x0E },
            {"Lucario", 0x0F },
            {"Mega Lucario", 0x34 },
            {"Lucina", 0x2F },
            {"Luigi", 0x10 },
            {"Mario", 0x11 },
            {"Dr. Mario", 0x2E },
            {"Marth", 0x12 },
            {"Meta Knight", 0x13 },
            {"Mii Fighter", 0x14 },
            {"Mii Swordfighter", 0x15 },
            {"Mii Gunner", 0x16 },
            {"Villager", 0x3D },
            {"Villager FS", 0x3E },
            {"Ness", 0x17 },
            {"Pacman", 0x18 },
            {"Palutena", 0x19 },
            {"Peach", 0x1A },
            {"Pikachu", 0x1B },
            {"Pikmin", 0x1C },
            {"Pit", 0x1D },
            {"Dark Pit", 0x31 },
            {"Purin", 0x1E },
            {"Robin", 0x1F },
            {"Robot", 0x20 },
            {"Mega Man", 0x21 },
            {"Rosalina", 0x22 },
            {"Samus", 0x24 },
            {"Sheik", 0x25 },
            {"Shulk", 0x27 },
            {"Sonic", 0x26 },
            {"Zero Suit Samus", 0x28 },
            {"Toon Link", 0x29 },
            {"Wario", 0x2A },
            {"Wario Man", 0x35 },
            {"Wii Fit Trainer", 0x2B },
            {"Yoshi", 0x2C },
            {"Zelda", 0x2D },
            {"Mewtwo", 0x43 },
            {"Ryu", 0x44 },
            {"Lucas", 0x45 },
            {"Roy", 0x46 },
            {"Cloud", 0x47 },
            {"Bayonetta", 0x48 },
            {"Corrin", 0x49 },
            {"Mii Fighter Enemy", 0x36 },
            {"Mii Swordfighter Enemy", 0x37 },
            {"Mii Gunner Enemy", 0x38 }
        };
        
        public NUT_TexIDEditor()
        {
            InitializeComponent();

            typeCB.Items.AddRange(types.Keys.ToArray());
            characterCB.Items.AddRange(characters.Keys.ToArray());
        }

        public NUT nut;

        public void Set(NUT n)
        {
            this.nut = n;
            int hash = n.textures[0].id;
            int type = hash >> 24;
            int chr = (hash >> 16) & 0xFF;
            int slot = (hash >> 8) & 0xFF;

            Console.WriteLine(hash.ToString("x"));

            foreach (KeyValuePair<string, int> p in types)
                if (p.Value == type)
                {
                    typeCB.SelectedItem = p.Key;
                    break;
                }

            foreach (KeyValuePair<string, int> p in characters)
                if (p.Value == chr)
                {
                    characterCB.SelectedItem = p.Key;
                    break;
                }

            slotUD.Value = slot;
        }

        public void Apply()
        {
            Dictionary<int, int> oldtonew = new Dictionary<int, int>();
            int i = 0;
            foreach (NUT.NUD_Texture tex in nut.textures)
            {
                int t = 0;
                int.TryParse(typeTB.Text, out t);
                int chr = 0;
                int.TryParse(charTB.Text, out chr);
                int s = (int)slotUD.Value;

                int newid = ((t & 0xFF) << 24) | ((chr & 0xFF) << 16) | ((s & 0xFF) << 8) | i;
                i++;

                if (!oldtonew.ContainsKey(tex.id))
                {
                    oldtonew.Add(tex.id, newid);
                    nut.draw.Add(newid, nut.draw[tex.id]);
                    nut.draw.Remove(tex.id);
                    tex.id = newid;
                }
            }

            foreach(ModelContainer mc in Runtime.ModelContainers)
            {
                if(mc.nud != null)
                {
                    foreach(NUD.Mesh m in mc.nud.mesh)
                    {
                        foreach(NUD.Polygon p in m.Nodes)
                        {
                            foreach(NUD.Material mat in p.materials)
                            {
                                foreach (NUD.Mat_Texture tex in mat.textures)
                                    if (oldtonew.ContainsKey(tex.hash))
                                        tex.hash = oldtonew[tex.hash];
                            }
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            exitStatus = Opened;
            Close();
        }

        private void NUT_TexIDEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void typeCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            typeTB.Text = types[(string)typeCB.SelectedItem] + "";
        }

        private void characterCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            charTB.Text = characters[(string)characterCB.SelectedItem] + "";
        }
    }
}
