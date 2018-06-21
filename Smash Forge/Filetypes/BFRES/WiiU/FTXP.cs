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

    public class FTXP
    {
        public static AnimationGroupNode ThisAnimation;
        public static int FrameCount;

        public List<BFRES.MTA> matanims = new List<BFRES.MTA>();



        public void Read(string filename, BFRES bfres, AnimationGroupNode ThisAnimation, ModelContainer modelContainer)
        {
            Console.WriteLine("Reading Textue Pattern Animations ...");

            ResFile b = new ResFile(filename);

            ThisAnimation.Text = "Textue Pattern Animations";

            TreeNode dummy = new TreeNode() { Text = "Animation Set" };

            int i = 0;
            foreach (TexPatternAnim tex in b.TexPatternAnims.Values)
            {
                modelContainer.BFRES_MTA = new BFRES.MTA();

                BFRES_FVTX FVTX = new BFRES_FVTX(modelContainer.BFRES_MTA, tex);


                ThisAnimation.Nodes.Add(modelContainer.BFRES_MTA);
            }
        }
    }

    public class BFRES_FVTX
    {
    

        public BFRES_FVTX(BFRES.MTA mta, TexPatternAnim tex)
        {
           
            mta.Text = tex.Name;

            mta.FrameCount = (uint)tex.FrameCount;


            if (tex.TextureRefs != null)
            {
                foreach (var tx in tex.TextureRefs)
                {
                    mta.Pat0.Add(tx.Key);
                }
            }

            foreach (TexPatternMatAnim matanim in tex.TexPatternMatAnims)
            {
                BFRES.MatAnimEntry mat = new BFRES.MatAnimEntry();

                mat.Text = matanim.Name;

                if (matanim.Curves.Count == 0)
                {
                    int CurTex = 0;
                    foreach (PatternAnimInfo inf in matanim.PatternAnimInfos)
                    {
                        if (tex.TextureRefs != null)
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

                        if (tex.TextureRefs != null)
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

