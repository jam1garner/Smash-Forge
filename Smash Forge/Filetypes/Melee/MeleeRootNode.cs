using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using MeleeLib.DAT;
using MeleeLib.DAT.Animation;
using MeleeLib.DAT.MatAnim;
using MeleeLib.DAT.Helpers;
using SFGraphics.Cameras;
using Smash_Forge.Rendering;
using SFGraphics.GLObjects.Shaders;

namespace Smash_Forge
{
    public class MeleeRootNode : TreeNode
    {
        DATRoot Root;

        TreeNode Skeleton = new TreeNode("Joints") { SelectedImageKey = "folder", ImageKey = "folder" };
        TreeNode DataObjects = new TreeNode("Data Objects") { SelectedImageKey = "folder", ImageKey = "folder" };
        TreeNode MatAnims = new TreeNode("Material Animations") { SelectedImageKey = "folder", ImageKey = "folder" };
        TreeNode JointAnims = new TreeNode("Joint Animations") { SelectedImageKey = "folder", ImageKey = "folder" };

        public VBN RenderBones;
        public Matrix4[] BoneTransforms;

        public MeleeRootNode(DATRoot Root)
        {
            this.Root = Root;
            Text = Root.Text;

            SelectedImageKey = "folder";
            ImageKey = "folder";
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


            if (Root.GetJOBJinOrder().Length > 0)
                Nodes.Add(Skeleton);

            int i = 0;
            RenderBones = new VBN();
            List<DatJOBJ> JOBJS = new List<DatJOBJ>();
            foreach (DatJOBJ j in Root.GetJOBJinOrder())
            {
                Skeleton.Nodes.Add(new MeleeJointNode(j) { Text = "Bone_" + i++ });
                Bone b = new Bone(RenderBones);
                b.position = new float[] { j.TX, j.TY, j.TZ };
                b.rotation = new float[] { j.RX, j.RY, j.RZ };
                b.scale = new float[] { j.SX, j.SY, j.SZ };
                if (j.Parent != null)
                {
                    b.parentIndex = JOBJS.IndexOf(j.Parent);
                }
                JOBJS.Add(j);
                RenderBones.bones.Add(b);
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

            i = 0;
            foreach (DatDOBJ d in Root.GetDataObjects())
            {
                MeleeDataObjectNode n = new MeleeDataObjectNode(d) { Text = "DataObject" + i++ };
                DataObjects.Nodes.Add(n);
                n.RefreshRenderMeshes();
                n.BonePosition = Vector3.TransformPosition(Vector3.Zero, BoneTransforms[JOBJS.IndexOf(n.DOBJ.Parent)]);
            }

            if (Root.MatAnims.Count > 0)
            {
                Nodes.Add(MatAnims);

                foreach (DatMatAnim anim in Root.MatAnims)
                    MatAnims.Nodes.Add(new MeleeMaterialAnimationNode(anim));

            }

            if (Root.Animations.Count > 0)
            {
                Nodes.Add(JointAnims);

                foreach (DatAnimation anim in Root.Animations)
                    JointAnims.Nodes.Add(new MeleeJointAnimationNode(anim));
            }
        }

        public void Render(Camera c)
        {
            Shader shader = OpenTKSharedResources.shaders["DAT"];
            shader.UseProgram();

            Matrix4 mvpMatrix = c.MvpMatrix;
            shader.SetMatrix4x4("mvpMatrix", ref mvpMatrix);

            //Matrix4[] BoneTransforms = RenderBones.GetShaderMatrices();
            if (BoneTransforms.Length > 0)
                GL.UniformMatrix4(GL.GetUniformLocation(shader.Id, "bones"), BoneTransforms.Length, false, ref BoneTransforms[0].Row0.X);

            foreach (MeleeDataObjectNode n in DataObjects.Nodes)
            {
                n.Render(c, shader);
            }

            GL.UseProgram(0);
            GL.Disable(EnableCap.DepthTest);

            RenderTools.DrawVBN(RenderBones);
        }
    }
}
