using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace Smash_Forge
{
    class MaterialXmlBatchExport
    {
        public static void ExportAllMaterialsFromFolder()
        {
            // Get the source model folder and then the output folder. 
            using (var folderSelect = new FolderSelectDialog())
            {
                if (folderSelect.ShowDialog() == DialogResult.OK)
                {
                    string[] files = Directory.GetFiles(folderSelect.SelectedPath, "*.nud", SearchOption.AllDirectories);

                    using (var outputFolderSelect = new FolderSelectDialog())
                    {
                        if (outputFolderSelect.ShowDialog() == DialogResult.OK)
                        {
                            for (int i = 0; i < files.Length; i++)
                            {
                                string xmlName = ModelViewport.ConvertDirSeparatorsToUnderscore(files[i], folderSelect.SelectedPath);
                                NUD nud = new NUD(files[i]);
                                string outputFileName = outputFolderSelect.SelectedPath + "\\" + xmlName + ".xml";
                                MaterialXML.ExportMaterialAsXml(nud, outputFileName);
                            }
                        }
                    }
                }
            }
        }
    }
}