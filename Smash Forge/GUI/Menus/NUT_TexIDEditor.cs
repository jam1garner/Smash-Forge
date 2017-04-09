using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge.GUI.Menus
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
            {"Stage", 0x20 },
            {"Trophy", 0x30 },
            {"Character", 0x40 }
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
            {"Greninja", 0x09 },
            {"Ike", 0x0A},
            {"Kirby", 0x09 },
            {"Bowser", 0x0B },
            {"Bowser Jr", 0x0A },
            {"Link", 0x02 },
            {"Little Mac", 0x02 },
            {"Charizard", 0x01 },
            {"Lucario", 0x02 },
            {"Lucina", 0x02 },
            {"Luigi", 0x02 },
            {"Mario", 0x02 },
            {"Dr. Mario", 0x02 },
            {"Marth", 0x02 },
            {"Meta Knight", 0x02 },
            {"MiiFighter", 0x02 },
            {"MiiGunner", 0x02 },
            {"MiiSword", 0x02 },
            {"Villager", 0x02 },
            {"Ness", 0x02 },
            {"Palutena", 0x02 },
            {"Peach", 0x02 },
            {"Pikachu", 0x02 },
            {"Pikmin", 0x02 },
            {"Pit", 0x02 },
            {"Dark Pit", 0x02 },
            {"Purin", 0x02 },
            {"Robot", 0x02 },
            {"Robin", 0x02 },
            {"Mega Man", 0x02 },
            {"Rosalina", 0x02 },
            {"Roy", 0x02 },
            {"Samus", 0x02 },
            {"Sheik", 0x02 },
            {"Shulk", 0x02 },
            {"Sonic", 0x02 },
            {"Zero Suit Samus", 0x02 },
            {"Toon Link", 0x02 },
            {"Wario", 0x02 },
            {"Wario Man", 0x02 },
            {"Wii Fit Trainer", 0x02 },
            {"Yoshi", 0x02 },
            {"Zelda", 0x02 },
            {"Mewtwo", 0x43 },
            {"Ryu", 0x44 },
            {"Lucas", 0x45 },
            {"Roy", 0x46 },
            {"Cloud", 0x47 },
            {"Bayonetta", 0x48 },
            {"Corrin", 0x49 },
        };
        
        public NUT_TexIDEditor()
        {
            InitializeComponent();

            typeCB.Items.AddRange(types.Keys.ToArray());
            characterCB.Items.AddRange(characters.Keys.ToArray());
        }
    }
}
