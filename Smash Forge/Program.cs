using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic.ApplicationServices;

namespace Smash_Forge
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string[] args = Environment.GetCommandLineArgs();
            //If the update has been installed and there is an update for the updater then run it
            if (Directory.Exists(Path.Combine(Application.StartupPath, "new_updater/")))
            {
                Directory.Delete(Path.Combine(Application.StartupPath, "updater/"), true);
                Directory.Move(Path.Combine(Application.StartupPath, "new_updater/"), Path.Combine(Application.StartupPath, "updater/"));
            }
            SingleInstanceController controller = new SingleInstanceController();
            controller.Run(args);
            /*MainForm.Instance.filesToOpen = args;
            Application.StartupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            Application.Run(MainForm.Instance);*/
        }

        public class SingleInstanceController : WindowsFormsApplicationBase
        {
            public SingleInstanceController()
            {
                IsSingleInstance = true;
                StartupNextInstance += this_StartupNextInstance;
                Startup += OnStart;
            }

            private void OnStart(object sender, StartupEventArgs e)
            {
                List<string> args = new List<string>();
                foreach (string arg in e.CommandLine)
                    args.Add(arg);
                Smash_Forge.MainForm.Instance.filesToOpen = args.ToArray();
            }

            void this_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
            {
                e.BringToForeground = true;
                Smash_Forge.MainForm form = MainForm as Smash_Forge.MainForm;
                Smash_Forge.MainForm.Instance.filesToOpen = e.CommandLine.ToArray();
                Smash_Forge.MainForm.Instance.openFiles();
            }

            protected override void OnCreateMainForm()
            {
                MainForm = Smash_Forge.MainForm.Instance;
            }
        }
    }
}
