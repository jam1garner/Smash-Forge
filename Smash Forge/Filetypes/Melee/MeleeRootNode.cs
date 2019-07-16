using MeleeLib.DAT;
using MeleeLib.DAT.Animation;
using MeleeLib.DAT.Helpers;
using MeleeLib.DAT.MatAnim;
using MeleeLib.DAT.Melee;
using MeleeLib.GCX;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.Cameras;
using SFGraphics.GLObjects.Shaders;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SFGraphics.GLObjects.BufferObjects;
using SFGenericModel.RenderState;
using SmashForge.Filetypes.Melee.Utils;
using SmashForge.Rendering;

namespace SmashForge.Filetypes.Melee
{
    public class MeleeRootNode : MeleeNode
    {
        public DATRoot Root;

        TreeNode Skeleton = new TreeNode("Joints") { SelectedImageKey = "folder", ImageKey = "folder" };
        TreeNode DataObjects = new TreeNode("Data Objects") { SelectedImageKey = "folder", ImageKey = "folder" };
        TreeNode MatAnims = new TreeNode("Material Animations") { SelectedImageKey = "folder", ImageKey = "folder" };
        TreeNode JointAnims = new TreeNode("Joint Animations") { SelectedImageKey = "dat", ImageKey = "dat" };

        public VBN RenderBones;
        private BufferObject bonesUbo;

        public MeleeRootNode(DATRoot Root)
        {
            this.Root = Root;
            Text = Root.Text;

            SelectedImageKey = "folder";
            ImageKey = "folder";

            Checked = true;

            ContextMenu = new ContextMenu();

            MenuItem exportAs = new MenuItem("Export As");
            exportAs.Click += ExportAs;
            ContextMenu.MenuItems.Add(exportAs);

            MenuItem exportMaterialXml = new MenuItem("Export Material XML");
            exportMaterialXml.Click += ExportMaterialXml_Click;
            ContextMenu.MenuItems.Add(exportMaterialXml);

            DataObjects.ContextMenu = new ContextMenu();

            MenuItem removeTex = new MenuItem("Remove Unused Textures");
            removeTex.Click += RemoveAllTextures;
            DataObjects.ContextMenu.MenuItems.Add(removeTex);
        }

        public void RemoveAllTextures(object sender, EventArgs args)
        {
            // grab textures used by material animations
            List<byte[]> MaterialAnims = new List<byte[]>();
            foreach(DATRoot r in Root.ParentDAT.Roots)
            {
                foreach (DatMatAnim anim in r.MatAnims)
                {
                    foreach (DatMatAnimGroup g in anim.Groups)
                    {
                        foreach (DatMatAnimData d in g.TextureData)
                        {
                            foreach (DatMatAnimTextureData t in d.Textures)
                            {
                                MaterialAnims.Add(t.Data);
                            }
                        }
                    }
                }
            }
            if (MaterialAnims.Count > 0)
                MessageBox.Show("Warning: Models with Material Animations may freeze in-game");

            // clear textures that aren't part of material animation and don't have polygons
            foreach(MeleeDataObjectNode n in DataObjects.Nodes)
            {
                DatDOBJ o = n.DOBJ;
                if(n.DOBJ.Polygons.Count == 0)
                {
                    foreach (DatTexture t in o.Material.Textures)
                    {
                        if(t.ImageData == null || !MaterialAnims.Contains(t.ImageData.Data))
                        {
                            o.Material.RemoveTexture(t);
                        }
                        else
                            break;
                    }
                    if(o.Material.Textures.Length == 0)
                        o.Material.Flags = (int)(o.Material.Flags & 0xFFFFF00F);
                }
            }
        }

        private void ExportMaterialXml_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "XML Material|*.xml";
                DialogResult result = sfd.ShowDialog();

                if (result == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".xml"))
                    {
                        var doc = DatMaterialXml.CreateMaterialXml(this);
                        doc.Save(sfd.FileName);
                    }
                }
            }
        }

        public void ExportAs(object sender, EventArgs args)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Source Model|*.smd|" +
                             "All Files (*.*)|*.*";

                sfd.DefaultExt = "smd";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    GXVertexDecompressor decom = new GXVertexDecompressor(Root);
                    SMD smd = new SMD();
                    smd.Bones = RenderBones;

                    foreach(MeleeDataObjectNode n in DataObjects.Nodes)
                    {
                        int[] ind;
                        List<GXVertex> verts;
                        n.GetVerticesAsTriangles(out ind, out verts);

                        for(int i = 0; i < ind.Length; i+=3)
                        {
                            SMDTriangle t = new SMDTriangle();
                            t.Material = "defaultmaterial";
                            t.v1 = GXVertexToSMDVertex(verts[ind[i]]);
                            t.v2 = GXVertexToSMDVertex(verts[ind[i+1]]);
                            t.v3 = GXVertexToSMDVertex(verts[ind[i+2]]);
                            smd.Triangles.Add(t);
                        }
                    }

                    smd.Save(sfd.FileName);
                }
            }
        }

        public SMDVertex GXVertexToSMDVertex(GXVertex v)
        {
            SMDVertex smdv = new SMDVertex()
            {
                Parent = 0,
                P = new Vector3(v.Pos.X, v.Pos.Y, v.Pos.Z),
                N = new Vector3(v.Nrm.X, v.Nrm.Y, v.Nrm.Z),
                UV = new Vector2(v.TX0.X, v.TX0.Y),
            };
            smdv.Bones = v.N;
            smdv.Weights = v.W;
            return smdv;
        }

        public void RecompileVertices(GXVertexCompressor compressor)
        {
            GXVertexDecompressor decompressor = new GXVertexDecompressor(Root);
            foreach (MeleeDataObjectNode n in DataObjects.Nodes)
            {
                n.RecompileVertices(decompressor, compressor);
            }
        }

        public void RefreshDisplay()
        {
            Nodes.Clear();
            Skeleton.Nodes.Clear();
            DataObjects.Nodes.Clear();
            MatAnims.Nodes.Clear();
            JointAnims.Nodes.Clear();

            // Stage Stuff
            if(Root.Map_Head != null)
            {
                foreach (Map_Model_Group g in Root.Map_Head.ModelObjects)
                    Nodes.Add(new MeleeMapModelNode(g));
            }

            // Bones--------------------------------------
            if (Root.GetJOBJinOrder().Length > 0)
                Nodes.Add(Skeleton);

            int boneIndex = 0;
            RenderBones = new VBN();
            List<DatJOBJ> JOBJS = new List<DatJOBJ>();
            foreach (DatJOBJ j in Root.GetJOBJinOrder())
            {
                Bone b = new Bone(RenderBones);
                b.Text = "Bone_" + (boneIndex);
                b.position = new float[] { j.TX, j.TY, j.TZ };
                b.rotation = new float[] { j.RX, j.RY, j.RZ };
                b.scale = new float[] { j.SX, j.SY, j.SZ };
                if (j.Parent != null)
                {
                    b.parentIndex = JOBJS.IndexOf(j.Parent);
                }
                JOBJS.Add(j);
                RenderBones.bones.Add(b);
                Skeleton.Nodes.Add(new MeleeJointNode(j) { Text = "Bone_" + boneIndex++, RenderBone = b});
            }
            RenderBones.reset();

            if (Root.GetDataObjects().Length > 0)
            {
                Nodes.Add(DataObjects);
                SelectedImageKey = "model";
                ImageKey = "model";
            }

            // Data Objects--------------------------------------
            DatDOBJ[] dataObjects = Root.GetDataObjects();
            for (int i = 0; i < dataObjects.Length; i++)
            {
                DatDOBJ d = dataObjects[i];

                MeleeDataObjectNode n = new MeleeDataObjectNode(d) { Text = $"DataObject {i}" };
                DataObjects.Nodes.Add(n);
                n.RefreshRendering();
                n.BoneIndex = JOBJS.IndexOf(n.DOBJ.Parent);

                n.Checked = true;

                if(Parent is MeleeDataNode)
                {
                    MeleeDataNode parentNode = (MeleeDataNode)Parent;
                    if ((parentNode.LodModels.Count > 0) && !parentNode.LodModels.Contains((byte)(i)))
                    {
                        n.Checked = false;
                        n.Text += "Low";
                    }
                }
            }

            // MaterialAnimation--------------------------------------
            if (Root.MatAnims.Count > 0)
            {
                Nodes.Add(MatAnims);

                foreach (DatMatAnim anim in Root.MatAnims)
                    MatAnims.Nodes.Add(new MeleeMaterialAnimationNode(anim));

            }

            // Animation--------------------------------------
            if (Root.Animations.Count > 0)
            {
                Nodes.Add(JointAnims);

                foreach (DatAnimation anim in Root.Animations)
                    JointAnims.Nodes.Add(new MeleeJointAnimationNode(anim));
            }

            // Scripts--------------------------------------
            foreach (MeleeLib.DAT.Script.DatFighterData r in Root.FighterData)
            {
                Nodes.Add(new MeleeFighterDataNode(r));
            }
        }

        public void Render(Shader shader, Camera c)
        {
            if (!Checked)
                return;

            // Bones use immediate mode for drawing.
            if (Runtime.renderBones)
                shader.UseProgram();

            if (bonesUbo == null)
                bonesUbo = new BufferObject(BufferTarget.UniformBuffer);

            GL.UniformBlockBinding(shader.Id, shader.GetUniformBlockIndex("Bones"), 0);
            bonesUbo.BindBase(BufferRangeTarget.UniformBuffer, 0);

            Matrix4[] matrices = RenderBones.GetShaderMatricesNoInverse();
            Matrix4[] binds = RenderBones.GetShaderMatrices();

            int mat4Size = 16 * 4;
            bonesUbo.SetCapacity(400 * mat4Size, BufferUsageHint.DynamicDraw);
            bonesUbo.SetSubData(matrices, 0);
            bonesUbo.SetSubData(binds, 200 * mat4Size);

            var previousRenderSettings = new RenderSettings();

            foreach (MeleeDataObjectNode n in DataObjects.Nodes)
            {
                var newRenderSettings = n.GetRenderSettings();
                GLRenderSettings.SetRenderSettings(newRenderSettings, previousRenderSettings);
                previousRenderSettings = newRenderSettings;

                n.Render(c, shader);
            }

            if (Runtime.renderBones)
            {
                GL.UseProgram(0);
                GL.PushAttrib(AttribMask.DepthBufferBit);
                GL.Disable(EnableCap.DepthTest);

                RenderTools.DrawVBN(RenderBones);
                GL.PopAttrib();
            }
        }
    }
}
