using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Forge_Updater
{
    internal class Program
    {
        private static Release[] releases;
        private static string executableDir;
        private static string forgeDir;

        private static int Main(string[] args)
        {
            try
            {
                executableDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                forgeDir = Path.GetDirectoryName(executableDir);
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage:\n\nForgeUpdater.exe [options]\n\nOptions:\n* Download latest release     -d\n* Download latest nightly     -dn\n* Install downloaded release  -i\n* Latest release info       -info\n* Restart Forge              -r");
                    return 0;
                }
                var client = new GitHubClient(new ProductHeaderValue("forge-updater"));
                GetReleases(client).Wait();

                bool foundRelease = false;
                string alreadyDownloaded = "";
                if (File.Exists(Path.Combine(executableDir, "currentRelease\\version.txt")))
                    alreadyDownloaded = File.ReadAllText(Path.Combine(executableDir, "currentRelease\\version.txt"));

                foreach (string arg in args)
                {
                    if (arg.Equals("-info"))
                    {
                        foreach (Release latest in releases)
                        {
                            Console.WriteLine($"Name: {latest.Name}\nDescription:\n{latest.Body}");
                            Console.WriteLine($"URL: {latest.Assets[0].BrowserDownloadUrl}");
                            Console.WriteLine($"Upload Date: {latest.Assets[0].UpdatedAt}");
                            break;
                        }
                    }
                    if (arg.Equals("-d"))
                    {
                        foreach (Release latest in releases)
                        {
                            if (!latest.Prerelease)
                            {
                                if (!foundRelease)
                                {
                                    Console.WriteLine($"Name: {latest.Name}\nDescription:\n{latest.Body}");
                                    Console.WriteLine($"URL: {latest.Assets[0].BrowserDownloadUrl}");
                                    Console.WriteLine($"Upload Date: {latest.Assets[0].UpdatedAt}");
                                    if (!latest.Assets[0].UpdatedAt.ToString().Equals(alreadyDownloaded))
                                    {
                                        int code = DownloadRelease(latest.Assets[0].BrowserDownloadUrl, "currentRelease", latest.Assets[0].UpdatedAt.ToString());
                                        if (code != 0)
                                            return code;
                                    }
                                    foundRelease = true;
                                }
                            }
                        }
                    }
                    else if (arg.Equals("-dn"))
                    {
                        foreach (Release latest in releases)
                        {
                            if (!foundRelease)
                            {
                                Console.WriteLine($"Name: {latest.Name}\nDescription:\n{latest.Body}");
                                Console.WriteLine($"URL: {latest.Assets[0].BrowserDownloadUrl}");
                                Console.WriteLine($"Upload Date: {latest.Assets[0].UpdatedAt}");
                                if (!latest.Assets[0].UpdatedAt.ToString().Equals(alreadyDownloaded))
                                {
                                    int code = DownloadRelease(latest.Assets[0].BrowserDownloadUrl, "currentRelease", latest.Assets[0].UpdatedAt.ToString());
                                    if (code != 0)
                                        return code;
                                }
                                foundRelease = true;
                            }
                        }
                    }
                    else if (arg.Equals("-i"))
                    {
                        foreach (string dir in Directory.GetDirectories("currentRelease/"))
                        {
                            string dirName = new DirectoryInfo(dir).Name;
                            if (!dirName.Equals("updater") && !dirName.Equals("materials") && !dirName.Equals("param_labels"))
                            {
                                if (Directory.Exists(Path.Combine(forgeDir, dirName + @"\")))
                                    Directory.Delete(Path.Combine(forgeDir, dirName + @"\"), true);
                                Directory.Move(dir, Path.Combine(forgeDir, dirName + @"\"));
                            }
                            else if(dirName.Equals("updater"))
                            {
                                Directory.Move(dir, Path.Combine(forgeDir, @"new_updater\"));
                            }
                            else if(dirName.Equals("param_labels"))
                            {
                                foreach(string file in Directory.EnumerateFiles(dir))
                                {
                                    string copyPath = Path.Combine(forgeDir, "param_labels/", Path.GetFileName(file));
                                    File.Copy(file, copyPath, true);
                                }
                            }
                            else if (dirName.Equals("materials"))
                            {
                                foreach(string folder in Directory.EnumerateDirectories(dir))
                                {
                                    if (!Directory.Exists(Path.Combine(forgeDir, dirName + @"\", folder + @"\")))
                                        Directory.CreateDirectory(Path.Combine(forgeDir, dirName + @"\", folder + @"\"));
                                    foreach(string file in Directory.EnumerateFiles(folder))
                                        File.Copy(file, Path.Combine(Path.Combine(forgeDir, dirName + @"\", folder + @"\"), Path.GetFileName(file)), true);
                                    
                                }
                            }
                        }
                        foreach (string file in Directory.GetFiles("currentRelease/"))
                        {
                            if (File.Exists(Path.Combine(forgeDir, Path.GetFileName(file))))
                                File.Delete(Path.Combine(forgeDir, Path.GetFileName(file)));
                            File.Move(file, Path.Combine(forgeDir, Path.GetFileName(file)));
                        }
                    }
                }

                

                foreach (string arg in args)
                {
                    if (arg.Equals("-r"))
                    {
                        Thread.Sleep(3000);
                        System.Diagnostics.Process.Start(Path.Combine(forgeDir, "Smash Forge.exe"));
                    }
                }

                return 0;
            }
            catch
            {
                return 1;
            }
            
        }

        private static async Task GetReleases(GitHubClient client)
        {
            List<Release> releases = new List<Release>();
            foreach (Release r in await client.Repository.Release.GetAll("jam1garner", "Smash-Forge"))
                releases.Add(r);
            Program.releases = releases.ToArray();
        }

        private static int DownloadRelease(string downloadUrl, string downloadName, string versionTime)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(downloadUrl, downloadName + ".zip");
                }
                if (Directory.Exists(downloadName + "/"))
                    Directory.Delete(downloadName + "/", true);
                ZipFile.ExtractToDirectory(downloadName + ".zip", downloadName + "/");
                string versionTxt = Path.Combine(Path.GetFullPath(downloadName + "/"), "version.txt");
                File.WriteAllText(versionTxt, versionTime);
                return 0;
            }
            catch
            {
                return -1;
            }
        }
    }
}
