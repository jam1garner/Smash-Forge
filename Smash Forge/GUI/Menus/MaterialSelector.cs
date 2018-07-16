using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Smash_Forge
{
    public partial class MaterialSelector : Form
    {
        public enum ExitStatus
        {
            Running,
            Opened,
            Cancelled
        }

        public string path = null;
        private ImageList matPresetRenders = new ImageList();
        public ExitStatus exitStatus = ExitStatus.Running;

        public MaterialSelector()
        {
            InitializeComponent();
            matPresetRenders.ImageSize = new Size(64, 64);
            matPresetRenders.ColorDepth = ColorDepth.Depth32Bit;
            AddPresetImagesFromFiles();

            materialPresetTreeView.ImageList = matPresetRenders;
        }

        public void PopulateTreeNodes()
        {
            if (Directory.Exists(MainForm.executableDir + "\\materials"))
            {
                foreach (string folder in Directory.EnumerateDirectories(Path.GetFullPath(MainForm.executableDir + "\\materials")))
                {
                    DirectoryInfo dir = new DirectoryInfo(folder);
               
                    // The presets are split up into a character and stage categories.
                    TreeNode folderNode = new TreeNode(dir.Name) { Tag = "folder" };
                    if (dir.Name == "Character Mats")
                    {
                        folderNode.ImageKey = "Character";
                        folderNode.SelectedImageKey = "Character";
                    }
                    if (dir.Name == "Stage Mats")
                    {
                        folderNode.ImageKey = "Stage";
                        folderNode.SelectedImageKey = "Stage";
                    }

                    foreach (string file in Directory.EnumerateFiles(folder))
                    {
                        if (Path.GetExtension(file) == ".nmt")
                        {
                            // The name of the material should match the name of the preview image.
                            string filename = Path.GetFileNameWithoutExtension(file);
                            TreeNode node = new TreeNode(filename) { Tag = file };
                            if (matPresetRenders.Images.ContainsKey(filename))
                            {
                                node.ImageKey = filename;
                                node.SelectedImageKey = filename;

                            }
                            folderNode.Nodes.Add(node);
                        }
                    }
                    if (dir.Name != "Preview Images")
                        materialPresetTreeView.Nodes.Add(folderNode);
                }
            }
            materialPresetTreeView.Refresh();
        }

        public void AddPresetImagesFromFiles()
        {
            // Wait for thumbnail generation to finish to avoid file concurrency issues.
            if (Rendering.MaterialPreviewRendering.RenderAllPresetsToFiles != null)
                Rendering.MaterialPreviewRendering.RenderAllPresetsToFiles.Wait();

            // Load all of the preview images from the preview images folder.
            if (Directory.Exists(MainForm.executableDir + "//Preview Images"))
            {
                // Load the folder icons.
                if (File.Exists(MainForm.executableDir + "//Preview Images//Dummy.png"))
                    matPresetRenders.Images.Add("Dummy", Image.FromFile(MainForm.executableDir + "//Preview Images//Dummy.png"));
                if (File.Exists(MainForm.executableDir + "//Preview Images//Character.png"))
                    matPresetRenders.Images.Add("Character", Image.FromFile(MainForm.executableDir + "//Preview Images//Character.png"));
                if (File.Exists(MainForm.executableDir + "//Preview Images//Stage.png"))
                    matPresetRenders.Images.Add("Stage", Image.FromFile(MainForm.executableDir + "//Preview Images//Stage.png"));

                // Load the preset images.
                foreach (string file in Directory.EnumerateFiles(MainForm.executableDir + "//Preview Images"))
                {
                    if (Path.GetExtension(file) == ".png")
                    {
                        string filename = Path.GetFileNameWithoutExtension(file);
                        matPresetRenders.Images.Add(filename, Image.FromFile(file));
                    }
                }
            }
        }

        private void OpenButton()
        {
            if (materialPresetTreeView.SelectedNode != null && ((string)materialPresetTreeView.SelectedNode.Tag).EndsWith(".nmt"))
            {
                path = ((string)materialPresetTreeView.SelectedNode.Tag);
                exitStatus = ExitStatus.Opened;
                Close();
            }
        }

        private void openButton(object sender, EventArgs e)
        {
            OpenButton();
        }

        private void closeButton(object sender, EventArgs e)
        {
            exitStatus = ExitStatus.Cancelled;
            Close();
        }

        private void MaterialSelector_Load(object sender, EventArgs e)
        {
            PopulateTreeNodes();
        }

        private void materialPresetTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OpenButton();
                e.Handled = true;
            }
        }
    }
}
