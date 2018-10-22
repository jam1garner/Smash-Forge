using System.IO;

namespace Smash_Forge.Rendering
{
    public static class BatchRendering
    {
        public static void OpenNud(string file, ModelViewport viewport)
        {
            MainForm.Instance.OpenNud(file, "", viewport);
        }

        public static  void OpenMeleeDat(string file, ModelViewport viewport)
        {
            byte[] data = File.ReadAllBytes(file);
            MainForm.Instance.OpenMeleeDat(data, file, "", viewport);
        }

        public static void OpenBfres(string file, ModelViewport viewport)
        {
            MainForm.Instance.OpenBfres(MainForm.GetUncompressedSzsSbfresData(file), file, "", viewport);

            string nameNoExtension = Path.GetFileNameWithoutExtension(file);
            string textureFileName = Path.GetDirectoryName(file) + "\\" + $"{nameNoExtension}.Tex1.sbfres";

            if (File.Exists(textureFileName))
                MainForm.Instance.OpenBfres(MainForm.GetUncompressedSzsSbfresData(textureFileName), textureFileName, "", viewport);
        }

    }
}
