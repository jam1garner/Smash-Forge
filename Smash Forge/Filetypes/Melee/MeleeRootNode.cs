using MeleeLib.DAT;
using MeleeLib.DAT.Animation;
using MeleeLib.DAT.Helpers;
using MeleeLib.DAT.MatAnim;
using MeleeLib.GCX;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.Cameras;
using SFGraphics.GLObjects.Shaders;
using Smash_Forge.Filetypes.Melee;
using Smash_Forge.Rendering;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Smash_Forge
{
    public class MeleeRootNode : MeleeNode
    {
        public DATRoot Root;

        TreeNode Skeleton = new TreeNode("Joints") { SelectedImageKey = "folder", ImageKey = "folder" };
        TreeNode DataObjects = new TreeNode("Data Objects") { SelectedImageKey = "folder", ImageKey = "folder" };
        TreeNode MatAnims = new TreeNode("Material Animations") { SelectedImageKey = "folder", ImageKey = "folder" };
        TreeNode JointAnims = new TreeNode("Joint Animations") { SelectedImageKey = "dat", ImageKey = "dat" };

        public VBN RenderBones;
        public Matrix4[] BoneTransforms;

        public MeleeRootNode(DATRoot Root)
        {
            this.Root = Root;
            Text = Root.Text;

            SelectedImageKey = "folder";
            ImageKey = "folder";

            ContextMenu = new ContextMenu();

            MenuItem exportAs = new MenuItem("Export As");
            exportAs.Click += ExportAs;
            ContextMenu.MenuItems.Add(exportAs);

            MenuItem exportMaterialXml = new MenuItem("Export Material XML");
            exportMaterialXml.Click += ExportMaterialXml_Click;
            ContextMenu.MenuItems.Add(exportMaterialXml);

            DataObjects.ContextMenu = new ContextMenu();

            MenuItem removeTex = new MenuItem("Remove All Textures");
            removeTex.Click += RemoveAllTextures;
            DataObjects.ContextMenu.MenuItems.Add(removeTex);
        }

        public void RemoveAllTextures(object sender, EventArgs args)
        {
            foreach(MeleeDataObjectNode n in DataObjects.Nodes)
            {
                DatDOBJ o = n.DOBJ;
                foreach (DatTexture t in o.Material.Textures)
                {
                    o.Material.RemoveTexture(t);
                }
                o.Material.Flags = (int)(o.Material.Flags & 0xFFFFF00F);
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
                    GXVertexDecompressor decom = new GXVertexDecompressor(((MeleeDataNode)Parent).DatFile);
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

        public void RecompileVertices(GXVertexDecompressor decompressor, GXVertexCompressor compressor)
        {
            foreach(MeleeDataObjectNode n in DataObjects.Nodes)
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


            // Bones--------------------------------------
            if (Root.GetJOBJinOrder().Length > 0)
                Nodes.Add(Skeleton);

            int i = 0;
            RenderBones = new VBN();
            List<DatJOBJ> JOBJS = new List<DatJOBJ>();
            foreach (DatJOBJ j in Root.GetJOBJinOrder())
            {
                Bone b = new Bone(RenderBones);
                b.Text = "Bone_" + (i);
                b.position = new float[] { j.TX, j.TY, j.TZ };
                b.rotation = new float[] { j.RX, j.RY, j.RZ };
                b.scale = new float[] { j.SX, j.SY, j.SZ };
                if (j.Parent != null)
                {
                    b.parentIndex = JOBJS.IndexOf(j.Parent);
                }
                JOBJS.Add(j);
                RenderBones.bones.Add(b);
                Skeleton.Nodes.Add(new MeleeJointNode(j) { Text = "Bone_" + i++, RenderBone = b});
            }
            RenderBones.reset();
            BoneTransforms = new Matrix4[RenderBones.bones.Count];
            i = 0;
            foreach (Bone b in RenderBones.bones)
            {
                BoneTransforms[i++] = b.transform;
            }

            if (Root.GetDataObjects().Length > 0)
            {
                Nodes.Add(DataObjects);
                SelectedImageKey = "model";
                ImageKey = "model";
            }

            // Data Objects--------------------------------------
            i = 0;
            foreach (DatDOBJ d in Root.GetDataObjects())
            {
                MeleeDataObjectNode n = new MeleeDataObjectNode(d) { Text = "DataObject" + i++ };
                DataObjects.Nodes.Add(n);
                n.RefreshRendering();
                n.BoneIndex = JOBJS.IndexOf(n.DOBJ.Parent);
                n.Checked = false;
                if (((MeleeDataNode)Parent).LodModels.Contains((byte)(i - 1)))
                {
                    n.Checked = true;
                }
                else
                    n.Text += "Low";
                //n.BonePosition = Vector3.TransformPosition(Vector3.Zero, BoneTransforms[JOBJS.IndexOf(n.DOBJ.Parent)]);
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

        public void Render(Camera c)
        {
            Shader shader = OpenTKSharedResources.shaders["Dat"];
            if (Runtime.renderType != Runtime.RenderTypes.Shaded)
                shader = OpenTKSharedResources.shaders["DatDebug"];
            shader.UseProgram();

            SetSharedUniforms(c, shader);

            if (BoneTransforms.Length > 0)
                GL.UniformMatrix4(GL.GetUniformLocation(shader.Id, "bones"), BoneTransforms.Length, false, ref BoneTransforms[0].Row0.X);

            Matrix4[] binds = RenderBones.GetShaderMatrices();
            if(binds.Length > 0)
            GL.UniformMatrix4(GL.GetUniformLocation(shader.Id, "binds"), binds.Length, false, ref binds[0].Row0.X);

            foreach (MeleeDataObjectNode n in DataObjects.Nodes)
            {
                n.Render(c, shader);
            }

            GL.UseProgram(0);
            GL.PushAttrib(AttribMask.DepthBufferBit);
            GL.Disable(EnableCap.DepthTest);

            if (Runtime.renderBones)
                RenderTools.DrawVBN(RenderBones);
            GL.PopAttrib();
        }

        private static void SetSharedUniforms(Camera c, Shader shader)
        {
            Matrix4 mvpMatrix = c.MvpMatrix;
            shader.SetMatrix4x4("mvpMatrix", ref mvpMatrix);

            Matrix4 sphereMatrix = c.ModelViewMatrix;
            sphereMatrix.Invert();
            sphereMatrix.Transpose();
            shader.SetMatrix4x4("sphereMatrix", ref sphereMatrix);

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
    }
}
