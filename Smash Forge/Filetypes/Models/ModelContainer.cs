using System;
using System.Collections.Generic;
using SALT.Graphics;
using System.Windows.Forms;

namespace Smash_Forge
{
    public class ModelContainer : TreeNode
    {
        public NUD NUD {
            get
            {
                return nud;
            }
            set
            {
                nud = value;
                Refresh();
            }
        }
        private NUD nud;
        public NUT NUT
        {
            get
            {
                return nut;
            }
            set
            {
                nut = value;
                Refresh();
            }
        }
        private NUT nut;
        public VBN VBN
        {
            get
            {
                return vbn;
            }
            set
            {
                vbn = value;
                Refresh();
            }
        }
        private VBN vbn;
        public MTA mta;
        public MOI moi;
        public XMBFile xmb;

        public BCH bch;

        public DAT dat_melee;

        public static Dictionary<string, SkelAnimation> Animations { get; set; }
        public static MovesetManager Moveset { get; set; }

        public ModelContainer()
        {
            ImageKey = "folder";
            SelectedImageKey = "folder";
        }

        public void Refresh()
        {
            Nodes.Clear();
            if (nud != null) Nodes.Add(nud);
            if (nut != null) Nodes.Add(nut);
            if (vbn != null) Nodes.Add(vbn);
        }

        /*
         * This method is for clearing all the GL stuff
         * Don't want wasted buffers :>
         * */
        public void Destroy()
        {
            if(NUD != null)
                NUD.Destroy();
        }
    }
}

