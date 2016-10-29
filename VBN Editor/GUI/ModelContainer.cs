using System;
using System.Collections.Generic;

namespace VBN_Editor
{
    public class ModelContainer
    {
        public string name = "";
        public NUD nud;
        public VBN vbn;

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

