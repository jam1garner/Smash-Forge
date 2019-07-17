using MeleeLib.DAT;
using MeleeLib.DAT.Animation;
using MeleeLib.DAT.Helpers;
using MeleeLib.DAT.Script;
using MeleeLib.IO;
using SFGenericModel.RenderState;
using SFGraphics.Cameras;
using SFGraphics.GLObjects.Shaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using SmashForge.Rendering;

namespace SmashForge.Filetypes.Melee
{
    public class MeleeDataNode : MeleeNode
    {
        public DATFile DatFile;

        private bool hasCreatedRenderMeshes = false;
        private string fileName; // temp for injecting script


        // For display only
        public List<byte> LodModels = new List<byte>();

        public MeleeDataNode(string fname)
        {
            fileName = fname;
            DatFile = Decompiler.Decompile(File.ReadAllBytes(fname));
            if(Path.GetFileNameWithoutExtension(fname).StartsWith("Pl") && File.Exists(fname.Substring(0, fname.Length - 6) + ".dat"))
            {
                DialogResult dialogResult = MessageBox.Show("Mark LOD Models?\nUseful for easier importing", "Import LOD Information", MessageBoxButtons.YesNo);
                if (dialogResult != DialogResult.Yes)
                {
                    return;
                }
                // Read it
                FileData d = new FileData(fname.Substring(0, fname.Length - 6) + ".dat");
                d.endian = Endianness.Big;
                d.Seek(0x24);
                d.Seek(d.ReadInt() + 0x20);
                int off;
                while((off = d.ReadInt()) != 0)
                {
                    int temp = d.Pos();
                    d.Seek(off + 0x20);
                    int Count = d.ReadInt();
                    int Offset = d.ReadInt() + 0x20;

                    for(int i =0; i < Count; i++)
                    {
                        d.Seek(Offset + i * 8);
                        int c = d.ReadInt();
                        int o = d.ReadInt() + 0x20;
                        LodModels.AddRange(d.GetSection(o, c));
                    }
                    d.Seek(temp);
                    break;
                }
            }

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

            /*if(LodModels.Count > 0)
            {
                MenuItem Import = new MenuItem("Import High Poly From File");
                Import.Click += SaveAs;
                ContextMenu.MenuItems.Add(Import);
            }*/
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
                            d.endian = Endianness.Big;
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
                                        script.AnimationOffset = o.Size();
                                    o.WriteBytes(File.ReadAllBytes("temp.dat"));
                                    if (script != null)
                                        script.AnimationSize = o.Size() - script.AnimationOffset;
                                    o.Align(0x20, 0xFF);
                                }
                            if(File.Exists("temp.dat"))
                                File.Delete("temp.dat");
                            bool CanExpand = false;
                            foreach (DatFighterScript s in DatFile.Roots[0].FighterData[0].Scripts)
                            {
                                if (DatFile.Roots.Length > 1)
                                {
                                    d.WriteInt(s.Offset + 4, s.AnimationOffset);
                                    d.WriteInt(s.Offset + 8, s.AnimationSize);
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
                                    d.WriteInt(s.Offset + 12, d.Size() - 0x20);
                                    d.WriteBytesAt(d.Size(), newSection.ToArray());
                                }
                                else
                                {
                                    d.WriteInt(s.Offset + 12, s.SubActionOffset - 0x20);
                                    d.WriteBytesAt(s.SubActionOffset, newSection.ToArray());
                                }
                                Console.WriteLine(s.SubActionOffset.ToString("x") + " " + s.SubActionSize.ToString("x") + " " + newSection.Count.ToString("x"));
                            }
                            if (DatFile.Roots.Length > 1)
                                o.Save(sfd.FileName.Replace(".dat", "AJ.dat"));
                            d.WriteInt(0, d.Size());
                            File.WriteAllBytes(sfd.FileName, d.GetSection(0, d.Eof()));
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
            GXVertexCompressor compressor = new GXVertexCompressor();

            foreach (MeleeRootNode root in Nodes)
            {
                root.RecompileVertices(compressor);
            }

            compressor.CompileChanges();
            
            RefreshDisplay();
        }

        public void Render(Camera c)
        {
            if (!hasCreatedRenderMeshes)
            {
                RefreshDisplay();
                hasCreatedRenderMeshes = true;
            }

            // Only initialize this once to improve frame rates.
            Shader shader = OpenTkSharedResources.shaders["Dat"];
            if (Runtime.renderType != Runtime.RenderTypes.Shaded)
                shader = OpenTkSharedResources.shaders["DatDebug"];
            shader.UseProgram();

            // TODO: Why is this flipped?
            GLRenderSettings.SetFaceCulling(new FaceCullingSettings(false, OpenTK.Graphics.OpenGL.CullFaceMode.Front));

            SetSharedUniforms(c, shader);

            // Melee roots can be deeper into the structure.
            List<MeleeRootNode> Nodes = GetAllRoots();
            foreach (MeleeRootNode n in Nodes)
            {
                n.Render(shader, c);
            }
        }

        private static void SetSharedUniforms(Camera c, Shader shader)
        {
            shader.SetMatrix4x4("mvpMatrix", c.MvpMatrix);

            OpenTK.Matrix4 sphereMatrix = c.ModelViewMatrix;
            sphereMatrix.Invert();
            sphereMatrix.Transpose();
            shader.SetMatrix4x4("sphereMatrix", sphereMatrix);

            shader.SetInt("renderType", (int)Runtime.renderType);

            shader.SetTexture("UVTestPattern", RenderTools.uvTestPattern, 10);

            shader.SetBoolToInt("renderR", Runtime.renderR);
            shader.SetBoolToInt("renderG", Runtime.renderG);
            shader.SetBoolToInt("renderB", Runtime.renderB);
            shader.SetBoolToInt("renderAlpha", Runtime.renderAlpha);
            bool alphaOverride = Runtime.renderAlpha && !Runtime.renderR && !Runtime.renderG && !Runtime.renderB;
            shader.SetBoolToInt("alphaOverride", alphaOverride);

            shader.SetBoolToInt("renderNormalMap", Runtime.renderNormalMap);

            shader.SetBoolToInt("renderDiffuse", Runtime.renderDiffuse);
            shader.SetBoolToInt("renderSpecular", Runtime.renderSpecular);
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
            while(f.Pos() < f.Eof())
            {
                int size = f.ReadInt();
                byte[] data = f.GetSection(f.Pos()-4, size);
                f.Skip(size - 4);
                f.Align(0x20);
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
