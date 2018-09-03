using System;
using System.IO;
using System.Windows.Forms;

namespace Smash_Forge
{
    class MaterialXmlBatchExport
    {
        public static void BatchExportNudMaterialXml()
        {
            ExportMaterialXmlFromFolder("*.nud", SaveNudXml);
        }

        public static void BatchExportMeleeDatMaterialXml()
        {
            ExportMaterialXmlFromFolder("*.dat", SaveMeleeDatXml);
        }

        private static void ExportMaterialXmlFromFolder(string searchPattern, Action<string, string, string> saveMaterialXml)
        {
            // Get the source model folder and then the output folder. 
            using (var folderSelect = new FolderSelectDialog())
            {
                folderSelect.Title = "Select Source Directory";
                if (folderSelect.ShowDialog() == DialogResult.OK)
                {
                    using (var outputFolderSelect = new FolderSelectDialog())
                    {
                        outputFolderSelect.Title = "Select Output Directory";
                        if (outputFolderSelect.ShowDialog() == DialogResult.OK)
                        {
                            foreach (string file in Directory.EnumerateFiles(folderSelect.SelectedPath, searchPattern, SearchOption.AllDirectories))
                            {
                                try
                                {
                                    saveMaterialXml(folderSelect.SelectedPath, outputFolderSelect.SelectedPath, file);
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                }
            }
        }

        private static void SaveNudXml(string sourceDir, string outputDir, string file)
        {
            string xmlName = ModelViewport.ConvertDirSeparatorsToUnderscore(file, sourceDir);
            NUD nud = new NUD(file);
            string outputFileName = $"{ outputDir }\\{ xmlName }.xml";
            MaterialXML.ExportMaterialAsXml(nud, outputFileName);
        }

        private static void SaveMeleeDatXml(string sourceDir, string outputDir, string file)
        {
            MeleeDataNode meleeDataNode = new MeleeDataNode(file);
            if (meleeDataNode.DatFile.Roots.Length == 0)
                return;

            var rootNode = new MeleeRootNode(meleeDataNode.DatFile.Roots[0]);
            var doc = rootNode.CreateMaterialXml();

            string xmlName = Path.GetFileNameWithoutExtension(file);
            string outputFileName = $"{ outputDir }\\{ xmlName }.xml";

            doc.Save(outputFileName);
        }
    }
}
