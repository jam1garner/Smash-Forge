using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeleeLib.DAT;
using MeleeLib.DAT.Script;

namespace Smash_Forge
{
    public class MeleeFighterScriptNode : MeleeNode
    {
        public DatFighterScript Script;

        public MeleeFighterScriptNode(DatFighterScript Script)
        {
            ImageKey = "script";
            SelectedImageKey = "script";
            this.Script = Script;
            Text = Script.Text;
            if (Text.Equals(""))
            {
                Text = "No Name 0x" + Script.Flags.ToString("x");
            }
        }
        
    }

    public class MeleeFighterDataNode : MeleeNode
    {
        private DatFighterData Data;

        public MeleeFighterDataNode(DatFighterData Data)
        {
            this.Data = Data;
            Text = "Animation Scripts";
            Refresh();
        }

        public void Refresh()
        {
            Nodes.Clear();

            foreach(DatFighterScript s in Data.Scripts)
            {
                Nodes.Add(new MeleeFighterScriptNode(s));
            }
        }
    }
}
