using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smash_Forge
{
    class DiscordSettings
    {
        public enum ImageKeyMode
        {
            Default,
            UserPicked,
            LastFileOpened
        }

        public enum DescriptionMode
        {
            Default,
            UserNamed,
            LastFileOpened
        }

        // Fields to be saved between runs
        public static ImageKeyMode imageKeyMode;
        public static string userPickedImageKey;
        public static DescriptionMode descriptionMode;
        public static string userNamedMod;
        public static bool showTimeElapsed;
        
        
    }
}
