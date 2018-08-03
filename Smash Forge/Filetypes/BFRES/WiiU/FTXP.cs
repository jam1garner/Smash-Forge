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
        public class FTXP
        {
            public static AnimationGroupNode ThisAnimation;
            public static int FrameCount;

            public List<MTA> matanims = new List<MTA>();

            public void Read(ResFile b, AnimationGroupNode ThisAnimation, ModelContainer modelContainer)
            {
                Console.WriteLine("Reading Textue Pattern Animations ...");

                TreeNode TexAnimation = new TreeNode() { Text = "Textue Pattern Animations" };
                ThisAnimation.Nodes.Add(TexAnimation);

                TreeNode dummy = new TreeNode() { Text = "Animation Set" };

                foreach (TexPatternAnim tex in b.TexPatternAnims.Values)
                {
                    modelContainer.BFRES_MTA = new MTA();

                    BFRES_FVTX FVTX = new BFRES_FVTX(modelContainer.BFRES_MTA, tex, b);


                    TexAnimation.Nodes.Add(modelContainer.BFRES_MTA);
                }
            }
        }

        public class BFRES_FVTX
        {


            public BFRES_FVTX(BFRES.MTA mta, TexPatternAnim tex, ResFile b)
            {

                mta.Text = tex.Name;

                mta.FrameCount = (uint)tex.FrameCount;


                if (tex.TextureRefs != null || tex.TextureRefNames != null)
                {
                    if (b.Version >= 0x03040000)
                    {
                        foreach (var tx in tex.TextureRefs)
                        {
                            mta.Pat0.Add(tx.Key);
                        }
                    }
                    else
                    {
                        foreach (var tx in tex.TextureRefNames)
                        {
                            mta.Pat0.Add(tx.Name);
                            Console.WriteLine(tx.Name);
                        }
                    }

                }

                foreach (TexPatternMatAnim matanim in tex.TexPatternMatAnims)
                {
                    BFRES.MatAnimEntry mat = new BFRES.MatAnimEntry();

                    mat.Text = matanim.Name;
                    Console.WriteLine($"MatAnim = {mat.Text}");
                    Console.WriteLine($"Curve Count = {matanim.Curves.Count}");

                    if (matanim.Curves.Count == 0)
                    {
                        int CurTex = 0;
                        foreach (PatternAnimInfo inf in matanim.PatternAnimInfos)
                        {
                            if (tex.TextureRefs != null || tex.TextureRefNames != null)
                            {
                                BFRES.MatAnimData md = new BFRES.MatAnimData();

                                md.Pat0Tex = mta.Pat0[CurTex];
                                md.SamplerName = inf.Name;
                                md.Frame = 0;

                                mat.matCurves.Add(md);
                            }
                            CurTex++;
                        }
                    }

                    int CurCurve = 0;
                    foreach (AnimCurve cr in matanim.Curves)
                    {
                        for (int i = 0; i < (ushort)cr.Frames.Length; i++)
                        {
                            BFRES.MatAnimData md = new BFRES.MatAnimData();

                            foreach (PatternAnimInfo inf in matanim.PatternAnimInfos)
                            {
                                if (inf.CurveIndex == CurCurve)
                                {
                                    md.SamplerName = inf.Name;
                                }
                            }

                            if (tex.TextureRefs != null || tex.TextureRefNames != null)
                            {
                                if (cr.KeyType == AnimCurveKeyType.SByte)
                                {
                                    md.CurveIndex = CurCurve;

                                    if (cr.Scale != 0)
                                    {
                                        int test = (int)cr.Keys[i, 0];
                                        float key = cr.Offset + test * cr.Scale;
                                        md.Pat0Tex = (mta.Pat0[(int)key]);
                                        md.Frame = (int)cr.Frames[i];

                                    }
                                    else
                                    {
                                        int test = (int)cr.Keys[i, 0];
                                        int key = cr.Offset + test;
                                        md.Pat0Tex = (mta.Pat0[(int)key]);
                                        md.Frame = (int)cr.Frames[i];
                                        Console.WriteLine($"{md.Frame} {md.Pat0Tex}");

                                    }
                                }
                            }
                            mat.matCurves.Add(md);
                        }
                        CurCurve++;
                    }

                    foreach (BFRES.MatAnimData md in mat.matCurves)
                    {
                        Console.WriteLine($"At frame {md.Frame} show {md.Pat0Tex} {md.SamplerName}");
                    }

                    mta.matEntries.Add(mat);
                }
            }
        }
    }
}

