using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeleeLib.DAT;
using MeleeLib.DAT.Script;
using System.Windows.Forms;
using SmashForge.Gui.Melee;

namespace SmashForge.Filetypes.Melee
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

            ContextMenu = new ContextMenu();

            MenuItem Edit = new MenuItem("Edit");
            Edit.Click += OpenEditor;
            ContextMenu.MenuItems.Add(Edit);
        }

        public void OpenEditor(object sender, EventArgs args)
        {
            MeleeCMDEditor editor = new MeleeCMDEditor(Script);
            editor.Show();
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
