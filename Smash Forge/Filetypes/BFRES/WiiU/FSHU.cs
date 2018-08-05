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
        public class FSHU
        {
            public static AnimationGroupNode ThisAnimation;
            public static int FrameCount;

            public List<MTA> matanims = new List<MTA>();

            public void Read(ResFile b, AnimationGroupNode ThisAnimation, ModelContainer modelContainer)
            {
                Console.WriteLine("Reading Shader Animations ...");

                TreeNode TexAnimation = new TreeNode() { Text = "Shader Animations" };
                ThisAnimation.Nodes.Add(TexAnimation);

                TreeNode dummy = new TreeNode() { Text = "Animation Set" };

                foreach (ShaderParamAnim shd in b.ColorAnims.Values)
                {
                    modelContainer.BFRES_MTA = new MTA();
                    BFRES_FSHU FSHU = new BFRES_FSHU(modelContainer.BFRES_MTA, shd, b);
                    TexAnimation.Nodes.Add(modelContainer.BFRES_MTA);
                }
                foreach (ShaderParamAnim shd in b.ShaderParamAnims.Values)
                {
                    modelContainer.BFRES_MTA = new MTA();
                    BFRES_FSHU FSHU = new BFRES_FSHU(modelContainer.BFRES_MTA, shd, b);
                    TexAnimation.Nodes.Add(modelContainer.BFRES_MTA);
                }
                foreach (ShaderParamAnim shd in b.TexSrtAnims.Values)
                {
                    modelContainer.BFRES_MTA = new MTA();
                    BFRES_FSHU FSHU = new BFRES_FSHU(modelContainer.BFRES_MTA, shd, b);
                    TexAnimation.Nodes.Add(modelContainer.BFRES_MTA);
                }
            }
        }

        public class BFRES_FSHU
        {
            public BFRES_FSHU(MTA mta, ShaderParamAnim fshu, ResFile b)
            {

                mta.Text = fshu.Name;

                mta.FrameCount = (uint)fshu.FrameCount;

                foreach (ShaderParamMatAnim matanim in fshu.ShaderParamMatAnims)
                {
                    MatAnimEntry mat = new MatAnimEntry();

                    mat.Text = matanim.Name;
                    Console.WriteLine($"MatAnim = {mat.Text}");
                    Console.WriteLine($"Curve Count = {matanim.Curves.Count}");

                    if (matanim.Curves.Count == 0)
                    {

                    }

                    //First set the data then iterpolate
                    foreach (AnimCurve cr in matanim.Curves)
                    {
                        mat.InterpolateWU(cr);
                    }
                    mta.matEntries.Add(mat);



                    for (int Frame = 0; Frame < fshu.FrameCount; Frame++)
                    {
                        foreach (MatAnimData track in mat.matCurves)
                        {
                            AnimKey left = track.GetLeft(Frame);
                            AnimKey right = track.GetRight(Frame);

                            track.Value = Animation.Hermite(Frame, left.frame, right.frame, 0, 0, left.unk1, right.unk1);
                        }
                    }

                    int CurCurve = 0;
                    for (int Frame = 0; Frame < fshu.FrameCount; Frame++)
                    {
                        foreach (MatAnimData track in mat.matCurves)
                        {

                            //This works like this
                            //Each param has their own info. While this loop through each curve determine which data is which
                            //Set the param name. Then detemine the data in between begin curve and total count
                            //Example. Begin curve starts at 0. Count may be 3 for RGB values
                            //Then for next curve would start at 3 and so on
                            //For color I simply use the values starting from RGBA
                            //Then for the next param i subtract the start value to reset the index back to 0
                            foreach (ParamAnimInfo inf in matanim.ParamAnimInfos)
                            {
                                track.shaderParamName = inf.Name;

                                if (inf.BeginCurve >= CurCurve)
                                {
                                    if (inf.FloatCurveCount >= CurCurve)
                                    {
                                        int ColorIndex = CurCurve - inf.BeginCurve;

                                        track.AnimColorType = (MatAnimData.ColorType)ColorIndex;
                                    }
                                }
                            }
                        }
                        CurCurve++;
                    }

                    mta.matEntries.Add(mat);
                }
            }
        }
    }
}

