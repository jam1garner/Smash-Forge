using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            if (args.Length > 0)
            {
                MainForm.Instance.WorkspaceManager = new WorkspaceManager(MainForm.Instance.project);
                MainForm.Instance.WorkspaceManager.TargetProject = args[0];
            }
            Application.Run(MainForm.Instance);

        }
    }
}
