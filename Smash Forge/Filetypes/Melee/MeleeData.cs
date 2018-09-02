using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MeleeLib.DAT;
using MeleeLib.IO;
using MeleeLib.DAT.Script;
using MeleeLib.DAT.Helpers;
using SFGraphics.Cameras;

namespace Smash_Forge
{
    public class MeleeDataNode : MeleeNode
    {
        public DATFile DatFile;

        private bool hasCreatedRenderMeshes = false;

        public MeleeDataNode(string fname)
        {
            DatFile = Decompiler.Decompile(File.ReadAllBytes(fname));

            ImageKey = "dat";
            SelectedImageKey = "dat";

            ContextMenu = new ContextMenu();
            MenuItem Save = new MenuItem();

            MenuItem Export = new MenuItem("Save As");
            Export.Click += SaveAs;
            ContextMenu.MenuItems.Add(Export);

            MenuItem Recompile = new MenuItem("Update Vertices");
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
                    if (DatFile.Roots.Length > 0)
                    {
                        if (DatFile.Roots[0].FighterData.Count > 0)
                        {
                            MessageBox.Show("Cannot save this dat type yet");
                            return;
                        }
                    }
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

        public void LoadPlayerAJ(string fname)
        {
            FileData d = new FileData(fname);

            //Extract All Animations from the AJ file
            DATRoot r = new DATRoot();
            r.Text = Path.GetFileNameWithoutExtension(fname);
            DatFile.AddRoot(r);

            foreach (DATRoot root in DatFile.Roots)
            {
                if(root.FighterData.Count > 0)
                foreach (DatFighterScript script in root.FighterData[0].Scripts)
                {
                    if(script.AnimationOffset != 0)
                    {
                        DATFile datfile = Decompiler.Decompile(d.getSection(script.AnimationOffset, script.AnimationSize));
                        datfile.Roots[0].Animations[0].Text = script.Text;
                        r.Animations.Add(datfile.Roots[0].Animations[0]);
                    }
                }
            }
        }
    }
    
}
