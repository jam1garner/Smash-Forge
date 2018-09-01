using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MeleeLib.DAT;
using MeleeLib.IO;
using MeleeLib.DAT.Helpers;
using SFGraphics.Cameras;

namespace Smash_Forge
{
    public class MeleeDataNode : TreeNode
    {
        public DATFile DatFile;

        private bool hasCreatedRenderMeshes = false;

        public MeleeDataNode(string fname)
        {
            DatFile = Decompiler.Decompile(File.ReadAllBytes(fname));
            Text = "HAL DAT FILE";

            ImageKey = "dat";
            SelectedImageKey = "dat";

            ContextMenu = new ContextMenu();
            MenuItem Save = new MenuItem();

            MenuItem Export = new MenuItem("Save As");
            Export.Click += SaveAs;
            ContextMenu.MenuItems.Add(Export);

            MenuItem Recompile = new MenuItem("Recompile");
            Recompile.Click += RecompileVertices;
            ContextMenu.MenuItems.Add(Recompile);
        }

        public void SaveAs(object sender, EventArgs args)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "HAL DAT|*.dat|" +
                             "All Files (*.*)|*.*";

                sfd.DefaultExt = "dat";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    Compiler.Compile(DatFile, sfd.FileName);

                }
            }
        }


        public void RecompileVertices(object sender, EventArgs args)
        {
            RecompileVertices();
        }

        public void RefreshDisplay()
        {
            Nodes.Clear();
            foreach(DATRoot root in DatFile.Roots)
            {
                MeleeRootNode ro = new MeleeRootNode(root);
                Nodes.Add(ro);
                ro.RefreshDisplay();
            }
        }

        public void RecompileVertices()
        {
            GXVertexCompressor compressor = new GXVertexCompressor(DatFile);
            GXVertexDecompressor decompressor = new GXVertexDecompressor(DatFile);

            foreach (MeleeRootNode root in Nodes)
            {
                root.RecompileVertices(decompressor, compressor);
            }

            DatFile.DataBuffer = compressor.GetBuffer();
            RefreshDisplay();
        }

        public void Render(Camera c)
        {
            if (!hasCreatedRenderMeshes)
            {
                RefreshDisplay();
                hasCreatedRenderMeshes = true;
            }

            foreach (MeleeRootNode n in Nodes)
            {
                n.Render(c);
            }
        }
    }
    
}
