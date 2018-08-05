using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Syroot.NintenTools.Bfres;
using System.IO;
using Syroot.NintenTools.Yaz0;


namespace Smash_Forge
{
    public partial class BFRES : TreeNode
    {
        public class FVIS
        {
            public static AnimationGroupNode ThisAnimation;

            public void Read(Syroot.NintenTools.NSW.Bfres.ResFile b, AnimationGroupNode ThisAnimation, ModelContainer modelContainer)
            {

                TreeNode BoneVISAnimation = new TreeNode() { Text = "Bone Visual Animations" };
                ThisAnimation.Nodes.Add(BoneVISAnimation);

                int i = 0;
                foreach (Syroot.NintenTools.NSW.Bfres.VisibilityAnim vis in b.BoneVisibilityAnims)
                {
                    modelContainer.BFRES_MTA = new BFRES.MTA();

                    ReadVIS(modelContainer.BFRES_MTA, vis);

                    BoneVISAnimation.Nodes.Add(modelContainer.BFRES_MTA);

                    i++;
                }
            }
            public void ReadVIS(BFRES.MTA mta, Syroot.NintenTools.NSW.Bfres.VisibilityAnim vis)
            {
                mta.Text = vis.Name;
                mta.FrameCount = (uint)vis.FrameCount;

                int boneindx = 0;
                if (vis.Names != null)
                {
                    foreach (string nm in vis.Names) //Loop through every bone. Not all have base and curve data
                    {
                        BFRES.MatAnimEntry bone = new BFRES.MatAnimEntry();
                        bone.Text = vis.Names[boneindx];



                        if (boneindx < vis.BaseDataList.Length)
                        {
                            BFRES.MatAnimData md = new BFRES.MatAnimData();

                            bool bas = vis.BaseDataList[boneindx];
                            md.VIS_State = bas;
                            md.Frame = 0;

                            bone.matCurves.Add(md);
                        }


                        if (vis.Curves.Count != 0)
                        {
                            if (boneindx < vis.Curves.Count)
                            {
                                Syroot.NintenTools.NSW.Bfres.AnimCurve cr = vis.Curves[boneindx];


                                int frm = 0;
                                foreach (bool bn in cr.KeyStepBoolData)
                                {
                                    BFRES.MatAnimData md = new BFRES.MatAnimData();
                                    md.Frame = (int)cr.Frames[frm];
                                    md.VIS_State = bn;

                                    bone.matCurves.Add(md);
                                    frm++;
                                }
                            }
                        }
                        mta.matEntries.Add(bone);

                        boneindx++;
                    }
                }
            }
        }
    }
}

