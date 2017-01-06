using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            MainForm.executableDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
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
                Smash_Forge.MainForm.Instance.filesToOpen = args.ToArray();
            }

            void this_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
            {
                e.BringToForeground = true;
                Smash_Forge.MainForm form = MainForm as Smash_Forge.MainForm;
                foreach (string arg in e.CommandLine)
                    Smash_Forge.MainForm.Instance.openFile(arg);
            }

            protected override void OnCreateMainForm()
            {
                MainForm = Smash_Forge.MainForm.Instance;
            }
        }
    }
}
