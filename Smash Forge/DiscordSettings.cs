using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeifenLuo.WinFormsUI.Docking;

namespace SmashForge
{
    class DiscordSettings
    {
        public enum ImageKeyMode
        {
            Default,
            UserPicked,
            LastFileOpened
        }

        // Fields to be saved between runs
        public static bool enabled = true;
        public static ImageKeyMode imageKeyMode;
        public static string userPickedImageKey = "forge";
        public static bool useUserModName;
        public static string userNamedMod = "ModNameHere";
        public static bool showCurrentWindow;
        public static bool showTimeElapsed;
        public static DiscordController discordController;

        public static void Update()
        {
            if (!enabled)
            {
                DiscordRpc.Shutdown();
                return;
            }
                
            if (imageKeyMode == ImageKeyMode.Default)
            {
                discordController.presence = new DiscordRpc.RichPresence()
                {
                    smallImageKey = "",
                    smallImageText = "",
                    largeImageKey = "forge",
                    largeImageText = ""
                };
            }
            else if (imageKeyMode == ImageKeyMode.UserPicked)
            {
                discordController.presence = new DiscordRpc.RichPresence()
                {
                    smallImageKey = "",
                    smallImageText = "",
                    largeImageKey = userPickedImageKey,
                    largeImageText = ""
                };
            }
            else
            {
                string key = "file";
                if (lastFileOpened.EndsWith("vbn"))
                    key = "vbn";
                if (lastFileOpened.EndsWith("nud"))
                    key = "nud";
                if (lastFileOpened.EndsWith("nut"))
                    key = "nut";
                if (lastFileOpened.EndsWith("omo") || lastFileOpened.EndsWith("anim") || lastFileOpened.EndsWith("pac"))
                    key = "big_icon_anim";

                discordController.presence = new DiscordRpc.RichPresence()
                {
                    smallImageKey = "",
                    smallImageText = "",
                    largeImageKey = key,
                    largeImageText = ""
                };
            }

            if (!useUserModName)
                discordController.presence.state = "Working on a mod";
            else
                discordController.presence.state = $"Working on {userNamedMod}";

            if (showCurrentWindow)
            {
                if (MainForm.dockPanel.ActiveContent != null)
                {
                    string tabName = ((DockContent)MainForm.dockPanel.ActiveContent).Text;
                    discordController.presence.details = $"{tabName}";
                }
            }

            if (showTimeElapsed)
                discordController.presence.startTimestamp = startTime;
            DiscordRpc.UpdatePresence(discordController.presence);
        }

        //Temporary, don't save to config
        public static string lastFileOpened = null;
        public static long startTime;
    }
}
