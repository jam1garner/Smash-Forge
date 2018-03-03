using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Smash_Forge.Rendering
{
    class BatchRenderTools
    {
        public static void LoadNextVbn(string fileName, ModelContainer modelContainer)
        {
            // Not all models have a vbn.
            if (File.Exists(fileName.Replace("nud", "vbn")))
                modelContainer.VBN = new VBN(fileName.Replace("nud", "vbn"));
        }

        public static void LoadNextPacs(string fileName, ModelContainer modelContainer)
        {
            // Read pacs to hide meshes.
            string[] pacs = Directory.GetFiles(fileName.Replace("model.nud", ""), "*.pac");
            foreach (string s in pacs)
            {
                PAC p = new PAC();
                p.Read(s);
                byte[] data;
                if (p.Files.TryGetValue("display", out data))
                {
                    MTA m = new MTA();
                    m.read(new FileData(data));
                    modelContainer.NUD.applyMTA(m, 0);
                }
                if (p.Files.TryGetValue("default.mta", out data))
                {
                    MTA m = new MTA();
                    m.read(new FileData(data));
                    modelContainer.NUD.applyMTA(m, 0);
                }
            }
        }

        public static void LoadNextNud(string fileName, ModelContainer modelContainer)
        {
            // Free memory used by OpenTK.
            modelContainer.Destroy();
            modelContainer.NUD = new NUD(fileName);
        }

        public static void LoadNextNut(string fileName, ModelContainer modelContainer)
        {
            Runtime.TextureContainers.Clear();
            try
            {
                NUT newNut = new NUT(fileName.Replace("nud", "nut"));
                Runtime.TextureContainers.Add(newNut);

                // Free memory used by OpenTK.
                modelContainer.NUT.Destroy();

                modelContainer.NUT = newNut;
            }
            catch (Exception e)
            {
                // A few nuts still don't open properly, so just skip them.
                Debug.WriteLine(e.Message);
            }
        }
    }
}
