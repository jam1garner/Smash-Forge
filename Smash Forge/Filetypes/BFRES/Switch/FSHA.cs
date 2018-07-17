using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Syroot.NintenTools.NSW.Bfres;
using System.IO;
using Syroot.NintenTools.Yaz0;


namespace Smash_Forge
{

    public class FSHA
    {
        public static AnimationGroupNode ThisAnimation;
        public static int FrameCount;

        public List<BFRES.MTA> matanims = new List<BFRES.MTA>();



        public void Read(ResFile b, AnimationGroupNode ThisAnimation, ModelContainer modelContainer)
        {
            Console.WriteLine("Reading Shape Animations ...");

            TreeNode ShapeAnimation = new TreeNode() { Text = "Shape Animations" };
            ThisAnimation.Nodes.Add(ShapeAnimation);

            int i = 0;
            foreach (ShapeAnim fsha in b.ShapeAnims)
            {
                modelContainer.BFRES_MTA = new BFRES.MTA();

                PerShapeAnim perAnim = new PerShapeAnim(modelContainer.BFRES_MTA, fsha);

                ShapeAnimation.Nodes.Add(modelContainer.BFRES_MTA);
            }
        }
    }

    public class PerShapeAnim
    {
        public PerShapeAnim(BFRES.MTA mta, ShapeAnim vis)
        {

            mta.Text = vis.Name;
            mta.ImageKey = "mesh";
            mta.SelectedImageKey = "mesh";

            mta.FrameCount = (uint)vis.FrameCount;


            foreach (VertexShapeAnim vtxanim in vis.VertexShapeAnims)
            {
                BFRES.MatAnimEntry mat = new BFRES.MatAnimEntry();

                mat.ImageKey = "bone";
                mat.SelectedImageKey = "bone";


                //First set the data then iterpolate
                mat.Text = vtxanim.Name;
                foreach (AnimCurve cr in vtxanim.Curves)
                {
                    mat.Interpolate(cr);
                }
                mta.matEntries.Add(mat);

                for (int Frame = 0; Frame < vis.FrameCount; Frame++)
                {
                    foreach (BFRES.MatAnimData track in mat.matCurves)
                    {

                        BFRES.AnimKey left = track.GetLeft(Frame);
                        BFRES.AnimKey right = track.GetRight(Frame);



                        track.Value = Animation.Hermite(Frame, left.frame, right.frame, 0, 0, left.unk1, right.unk1);
                    }
                }
            }
        }
    }
}

