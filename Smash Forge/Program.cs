using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic.ApplicationServices;

namespace SmashForge
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
            MainForm.executableDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            //If the update has been installed and there is an update for the updater then run it
            if (Directory.Exists(Path.Combine(MainForm.executableDir, "new_updater/")))
            {
                Directory.Delete(Path.Combine(MainForm.executableDir, "updater/"), true);
                Directory.Move(Path.Combine(MainForm.executableDir, "new_updater/"), Path.Combine(MainForm.executableDir, "updater/"));
            }

            OpenTK.Toolkit.Init(new OpenTK.ToolkitOptions() { Backend = OpenTK.PlatformBackend.PreferNative });

            SingleInstanceController controller = new SingleInstanceController();
            controller.Run(args);
            /*MainForm.Instance.filesToOpen = args;
            MainForm.executableDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
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
                SmashForge.MainForm.Instance.filesToOpen = args.ToArray();
            }

            void this_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
            {
                e.BringToForeground = true;
                MainForm form = MainForm as MainForm;
                SmashForge.MainForm.Instance.filesToOpen = e.CommandLine.ToArray();
                SmashForge.MainForm.Instance.OpenFiles();
            }

            protected override void OnCreateMainForm()
            {
                MainForm = SmashForge.MainForm.Instance;
            }
        }
    }
}
