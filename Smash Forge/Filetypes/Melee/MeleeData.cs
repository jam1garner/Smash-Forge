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
using MeleeLib.DAT.Animation;
using SFGraphics.Cameras;

namespace Smash_Forge
{
    public class MeleeDataNode : MeleeNode
    {
        public DATFile DatFile;

        private bool hasCreatedRenderMeshes = false;
        private string fileName; // temp for injecting script

        public MeleeDataNode(string fname)
        {
            fileName = fname;
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
                            //MessageBox.Show("Cannot save this dat type yet");
                            // gonna have to inject this one for now
                            FileData d = new FileData(fileName);
                            d.Endian = Endianness.Big;
                            FileOutput o = new FileOutput();

                            if (DatFile.Roots.Length > 1)
                                foreach (DatAnimation a in DatFile.Roots[1].Animations)
                                {
                                    DatFighterScript script = null;
                                    foreach (DatFighterScript s in DatFile.Roots[0].FighterData[0].Scripts)
                                    {
                                        if (s.Text.Equals(a.Text))
                                        {
                                            script = s;
                                            break;
                                        }
                                    }
                                    // Create Animation
                                    MeleeJointAnimationNode n = new MeleeJointAnimationNode(a);
                                    Compiler.Compile(n.GetAsDATFile(), "temp.dat");
                                    if (script != null)
                                        script.AnimationOffset = o.size();
                                    o.writeBytes(File.ReadAllBytes("temp.dat"));
                                    if (script != null)
                                        script.AnimationSize = o.size() - script.AnimationOffset;
                                    o.align(0x20, 0xFF);
                                }
                            if(File.Exists("temp.dat"))
                                File.Delete("temp.dat");
                            bool CanExpand = false;
                            foreach (DatFighterScript s in DatFile.Roots[0].FighterData[0].Scripts)
                            {
                                if (DatFile.Roots.Length > 1)
                                {
                                    d.writeInt(s.Offset + 4, s.AnimationOffset);
                                    d.writeInt(s.Offset + 8, s.AnimationSize);
                                }
                                List<byte> newSection = new List<byte>();
                                foreach(SubAction sub in s.SubActions)
                                {
                                    
                                    newSection.AddRange(sub.Data);
                                    if(MeleeCMD.GetActionName((byte)(sub.Data[0] >> 2)).Equals("Subroutine")
                                        || MeleeCMD.GetActionName((byte)(sub.Data[0] >> 2)).Equals("Goto"))
                                    {
                                        newSection[newSection.Count - 1] -= 0x20;
                                    }
                                }
                                newSection.AddRange(new byte[] { 0, 0, 0, 0});
                                
                                if(newSection.Count > s.SubActionSize && !CanExpand)
                                {
                                    DialogResult dialogResult = MessageBox.Show("Expand the file?\n(Warning incomplete)", "Error: Not enough space", MessageBoxButtons.YesNo);
                                    if (dialogResult != DialogResult.Yes)
                                    {
                                        CanExpand = true;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Failed saving the file");
                                        return;
                                    }
                                }
                                else if (newSection.Count > s.SubActionSize && CanExpand)
                                {
                                    d.writeInt(s.Offset + 12, d.size() - 0x20);
                                    d.writeBytesAt(d.size(), newSection.ToArray());
                                }
                                else
                                {
                                    d.writeInt(s.Offset + 12, s.SubActionOffset - 0x20);
                                    d.writeBytesAt(s.SubActionOffset, newSection.ToArray());
                                }
                                Console.WriteLine(s.SubActionOffset.ToString("x") + " " + s.SubActionSize.ToString("x") + " " + newSection.Count.ToString("x"));
                            }
                            if (DatFile.Roots.Length > 1)
                                o.save(sfd.FileName.Replace(".dat", "AJ.dat"));
                            d.writeInt(0, d.size());
                            File.WriteAllBytes(sfd.FileName, d.getSection(0, d.eof()));
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
            DialogResult dialogResult = MessageBox.Show("Import Animations?", "Animation Import", MessageBoxButtons.YesNo);
            if (dialogResult != DialogResult.Yes)
            {
                return;
            }
            FileData d = new FileData(fname);

            //Extract All Animations from the AJ file
            DATRoot r = new DATRoot();
            r.Text = Path.GetFileNameWithoutExtension(fname);
            DatFile.AddRoot(r);

            FileData f = new FileData(fname);
            while(f.pos() < f.eof())
            {
                int size = f.readInt();
                byte[] data = f.getSection(f.pos()-4, size);
                f.skip(size - 4);
                f.align(0x20);
                DATFile datfile = Decompiler.Decompile(data);
                datfile.Roots[0].Animations[0].Text = datfile.Roots[0].Text;
                r.Animations.Add(datfile.Roots[0].Animations[0]);
            }

            /*foreach (DATRoot root in DatFile.Roots)
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
            }*/
        }
    }
    
}
