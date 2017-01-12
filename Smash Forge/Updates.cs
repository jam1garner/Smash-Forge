using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace Smash_Forge
{
    class Update
    {
        static Release[] releases;
        public static bool includeNightlies = true;

        public static void CheckLatest()
        {
            try
            {
                var client = new GitHubClient(new ProductHeaderValue("forge-updater"));
                Console.WriteLine("Github client made");
                GetReleases(client).Wait();
                Console.WriteLine("Releases obtained");
                foreach (Release latest in releases)
                {
                    if ((includeNightlies || !latest.Prerelease) && latest.Assets.Count > 0)
                    {
                        Console.WriteLine("Found latest release");
                        string version;
                        try
                        {
                            version = File.ReadAllText(Path.Combine(MainForm.executableDir, "version.txt"));
                        }
                        catch (System.IO.FileNotFoundException)
                        {
                            File.WriteAllText(Path.Combine(MainForm.executableDir, "version.txt"), "");
                            version = "";
                        }
                        if (!version.Equals(latest.Assets[0].UpdatedAt.ToString()))
                        {
                            ProcessStartInfo p = new ProcessStartInfo();
                            p.WindowStyle = ProcessWindowStyle.Hidden;
                            p.CreateNoWindow = true;
                            p.FileName = Path.Combine(MainForm.executableDir, "updater/ForgeUpdater.exe");
                            if (includeNightlies)
                                p.Arguments = "-dn";
                            else
                                p.Arguments = "-d";
                            Process process = new Process();
                            process.StartInfo = p;
                            Console.WriteLine("Downloading...");
                            process.Start();
                            process.WaitForExit();
                            if (process.ExitCode != 0)
                                throw new TimeoutException();
                            Console.WriteLine("Finished downloading");
                            MainForm.Instance.pictureBox1.Visible = true;
                        }
                        break;
                    }
                }
            }
            catch
            {
                Console.WriteLine("Failed to get latest update");
            }
        }

        static async Task GetReleases(GitHubClient client)
        {
            List<Release> Releases = new List<Release>();
            foreach (Release r in await client.Repository.Release.GetAll("jam1garner", "Smash-Forge"))
                Releases.Add(r);
            releases = Releases.ToArray();
        }
    }
}
