using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALT.PARAMS;
using System.IO;
using System.Windows.Forms;


namespace Smash_Forge.Params
{
    class ParamTools
    {
        public static object GetParamValue(ParamFile file, int groupIndex, int entryIndex, int valueIndex)
        {
            if (groupIndex > file.Groups.Count)
                return null;

            if ((file.Groups[groupIndex] is ParamGroup))
            {
                // The selected group has multiple entries. Each entry contains EntrySize values. 
                int entrySize = ((ParamGroup)file.Groups[groupIndex]).EntrySize;
                for (int i = 0; i < entrySize; i++)
                {
                    if (i == valueIndex)
                        return file.Groups[groupIndex].Values[(entrySize * entryIndex) + i].Value;
                }
            }

            else if ((file.Groups[groupIndex] is ParamList))
            {
                // The selected group doesn't have any entries, so the entryIndex can be ignored. 
                for (int i = 0; i < file.Groups[groupIndex].Values.Count; i++)
                {
                    if (i == valueIndex)
                    {
                        return file.Groups[groupIndex].Values[i].Value;
                    }
                }
            }

            return null;
        }

        public static void ModifyParamValue(ParamFile file, int groupIndex, int entryIndex, int valueIndex, object newValue)
        {
            if (groupIndex > file.Groups.Count)
                return;

            if ((file.Groups[groupIndex] is ParamGroup))
            {
                // The selected group has multiple entries. Each entry contains EntrySize values. 
                int entrySize = ((ParamGroup)file.Groups[groupIndex]).EntrySize;
                for (int i = 0; i < entrySize; i++)
                {
                    if (i == valueIndex)
                        file.Groups[groupIndex].Values[(entrySize * entryIndex) + i].Value = newValue;
                }
            }

            else if ((file.Groups[groupIndex] is ParamList))
            {
                // The selected group doesn't have any entries, so the entryIndex can be ignored. 
                for (int i = 0; i < file.Groups[groupIndex].Values.Count; i++)
                {
                    if (i == valueIndex)
                    {
                        file.Groups[groupIndex].Values[i].Value = newValue;
                    }
                }
            }
        }

        public static void BatchExportLightSetValues()
        {
            // Get the source model folder and then the output folder. 
            using (var sourceFolderSelect = new FolderSelectDialog())
            {
                sourceFolderSelect.Title = "Stages Directory";
                if (sourceFolderSelect.ShowDialog() == DialogResult.OK)
                {
                    using (var outputFolderSelect = new FolderSelectDialog())
                    {
                        outputFolderSelect.Title = "Output Directory";
                        if (outputFolderSelect.ShowDialog() == DialogResult.OK)
                        {
                            StringBuilder miscCsv = new StringBuilder();
                            StringBuilder lightSetCsv = new StringBuilder();
                            StringBuilder fogCsv = new StringBuilder();
                            StringBuilder unkCsv = new StringBuilder();

                            string[] files = Directory.GetFiles(outputFolderSelect.SelectedPath, "*.bin", SearchOption.AllDirectories);
                            foreach (string file in files)
                            {
                                if (!(file.Contains("light_set")))
                                    continue;

                                ParamFile lightSet;
                                try
                                {
                                    lightSet = new ParamFile(file);
                                }
                                catch (NotImplementedException)
                                {
                                    continue;
                                }

                                // Hardcoding this because all lightsets are structured the same way.
                                // Use basic csv formatting to open in excel, sheets, etc. 
                                string[] directories = file.Split('\\');
                                string stageName = directories[directories.Length - 3]; // get stage folder name

                                AppendMiscValues(miscCsv, stageName, lightSet);
                                AppendLightSetValues(lightSetCsv, stageName, lightSet);
                                AppendFogSetValues(fogCsv, stageName, lightSet);
                                AppendUnknownValues(unkCsv, stageName, lightSet);
                            }

                            SaveLightSetValues(outputFolderSelect, miscCsv, lightSetCsv, fogCsv, unkCsv);
                        }
                    }
                }
            }
        }

        private static void SaveLightSetValues(FolderSelectDialog outputFolderSelect, StringBuilder miscCsv, StringBuilder lightSetCsv, StringBuilder fogCsv, StringBuilder unkCsv)
        {
            string fileName = outputFolderSelect.SelectedPath;
            File.WriteAllText(fileName + "\\misc values.csv", miscCsv.ToString());
            File.WriteAllText(fileName + "\\light_set values.csv", lightSetCsv.ToString());
            File.WriteAllText(fileName + "\\fog_set values.csv", fogCsv.ToString());
            File.WriteAllText(fileName + "\\unk values.csv", unkCsv.ToString());
        }

        private static void AppendUnknownValues(StringBuilder unkCsv, string stageName, SALT.PARAMS.ParamFile lightSet)
        {
            StringBuilder unkValues = new StringBuilder(stageName + ",");
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    var value = GetParamValue(lightSet, 3, i, j);
                    unkValues.Append(value + ",");
                }
            }
            unkCsv.AppendLine(unkValues.ToString());
        }

        private static void AppendFogSetValues(StringBuilder fogCsv, string stageName, SALT.PARAMS.ParamFile lightSet)
        {
            StringBuilder fogValues = new StringBuilder(stageName + ",");
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var value = GetParamValue(lightSet, 2, i, j);
                    fogValues.Append(value + ",");
                }
            }
            fogCsv.AppendLine(fogValues.ToString());
        }

        private static void AppendLightSetValues(StringBuilder lightSetCsv, string stageName, SALT.PARAMS.ParamFile lightSet)
        {
            StringBuilder lightSetValues = new StringBuilder(stageName + ",");
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var value = GetParamValue(lightSet, 1, i, j);
                    lightSetValues.Append(value + ",");
                }
            }
            lightSetCsv.AppendLine(lightSetValues.ToString());
        }

        private static void AppendMiscValues(StringBuilder miscCsv, string stageName, SALT.PARAMS.ParamFile lightSet)
        {
            StringBuilder miscValues = new StringBuilder(stageName + ",");
            for (int i = 0; i < 74; i++)
            {
                var value = GetParamValue(lightSet, 0, 0, i);
                miscValues.Append(value + ",");
            }
            miscCsv.AppendLine(miscValues.ToString());
        }
    }
}
