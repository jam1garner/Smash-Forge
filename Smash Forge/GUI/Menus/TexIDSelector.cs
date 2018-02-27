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
    public partial class TexIdSelector : Form
    {
        public enum ExitStatus
        {
            NotDone = 0,
            Opened = 1
        }

        public ExitStatus exitStatus = ExitStatus.NotDone;

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
        
        public TexIdSelector()
        {
            InitializeComponent();

            typeComboBox.Items.AddRange(types.Keys.ToArray());
            characterComboBox.Items.AddRange(characters.Keys.ToArray());
        }

        public void Set(int originalHash)
        {
            int type = originalHash >> 24;
            int chr = (originalHash >> 16) & 0xFF;
            int slot = (originalHash >> 8) & 0xFF;

            foreach (KeyValuePair<string, int> typeKeyValuePair in types)
                if (typeKeyValuePair.Value == type)
                {
                    typeComboBox.SelectedItem = typeKeyValuePair.Key;
                    break;
                }

            foreach (KeyValuePair<string, int> charKeyValuePair in characters)
                if (charKeyValuePair.Value == chr)
                {
                    characterComboBox.SelectedItem = charKeyValuePair.Key;
                    break;
                }

            slotUD.Value = slot;
        }

        public int getNewTexId()
        {
            int type = 0;
            int.TryParse(typeTB.Text, out type);

            int character = 0;
            int.TryParse(charTB.Text, out character);

            int slot = (int)slotUD.Value;

            return ((type & 0xFF) << 24) | ((character & 0xFF) << 16) | ((slot & 0xFF) << 8);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            exitStatus = ExitStatus.Opened;
            Close();
        }

        private void NUT_TexIDEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void typeCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            typeTB.Text = types[(string)typeComboBox.SelectedItem] + "";
        }

        private void characterCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            charTB.Text = characters[(string)characterComboBox.SelectedItem] + "";
        }
    }
}
