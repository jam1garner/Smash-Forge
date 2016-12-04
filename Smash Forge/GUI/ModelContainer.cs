using System;
using System.Collections.Generic;

namespace Smash_Forge
{
    public class ModelContainer
    {
        public string name = "";
        public NUD nud;
        public VBN vbn;
        public MTA mta;

        public BCH bch;

        public DAT dat_melee;

        public static Dictionary<string, SkelAnimation> Animations { get; set; }
        public static MovesetManager Moveset { get; set; }

        public ModelContainer()
        {
        }

        /*
         * This method is for clearing all the GL stuff
         * Don't want wasted buffers :>
         * */
        public void Destroy()
        {
            if(nud != null)
                nud.Destroy();
        }
    }
}

