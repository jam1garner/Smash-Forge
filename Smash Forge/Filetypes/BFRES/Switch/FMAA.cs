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

    public class FMAA
    {
        public static AnimationGroupNode ThisAnimation;
        public static int FrameCount;

        public List<BFRES.MTA> matanims = new List<BFRES.MTA>();



        public void Read(ResFile b, AnimationGroupNode ThisAnimation, ModelContainer modelContainer)
        {
            Console.WriteLine("Reading Material Animations ...");

            TreeNode MaterialAnimation = new TreeNode() { Text = "Material Animations" };
            ThisAnimation.Nodes.Add(MaterialAnimation);

            foreach (MaterialAnim vis in b.MaterialAnims)
            {
                modelContainer.BFRES_MTA = new BFRES.MTA();

                PerMatAnim perAnim = new PerMatAnim(modelContainer.BFRES_MTA, vis);

                MaterialAnimation.Nodes.Add(modelContainer.BFRES_MTA);
            }
        }
    }

    public class PerMatAnim
    {


        public PerMatAnim(BFRES.MTA mta, MaterialAnim vis)
        {

            mta.Text = vis.Name;

            mta.FrameCount = (uint)vis.FrameCount;


            if (vis.TextureNames != null)
            {
                foreach (string tex in vis.TextureNames)
                {
                    mta.Pat0.Add(tex);
                }
            }

            foreach (MaterialAnimData matanim in vis.MaterialAnimDataList)
            {
                BFRES.MatAnimEntry mat = new BFRES.MatAnimEntry();

                mat.Text = matanim.Name;



                if (matanim.Curves.Count == 0)
                {
                    int CurTex = 0;
                    foreach (TexturePatternAnimInfo inf in matanim.TexturePatternAnimInfos)
                    {

                        if (vis.TextureNames != null)
                        {
                            BFRES.MatAnimData md = new BFRES.MatAnimData();


                            //Switch doesn't have base values? I can't find any like wii u did so i'll just have this look though each texture map.
                            //Some have multiple maps but one texture (Yoshi in Odyssey) so this checks the texture count
                            if (CurTex + 1 <= vis.TextureNames.Count)
                            {
                                md.Pat0Tex = mta.Pat0[CurTex];
                            }
                            else
                            {
                                md.Pat0Tex = mta.Pat0[0];
                            }              

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

                        foreach (TexturePatternAnimInfo inf in matanim.TexturePatternAnimInfos)
                        {
                            if (inf.CurveIndex == CurCurve)
                            {
                                md.SamplerName = inf.Name;
                            }
                        }
                        
                        //Set pat0 data if texture list exists
                        if (vis.TextureNames != null)
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

                mta.matEntries.Add(mat);

            }
        }


        public class FMAANode
        {
            public int flags;
            public int flags2;
            public int stride;
            public int BeginRotate;
            public int BeginTranslate;
            public long offBase;
            public int trackCount;
            public int trackFlag;
            public long offTrack;
            public string Text;

            public Vector3 sca, rot, pos;
            public List<FMAATrack> tracks = new List<FMAATrack>();

            public FMAANode(MaterialAnimData md)
            {
                Text = md.Name;



                foreach (AnimCurve tr in md.Curves)
                {
                    FMAATrack t = new FMAATrack();
                    t.flag = (int)tr.AnimDataOffset;
                    tracks.Add(t);



                    //     Console.WriteLine("Flag = " + (int)tr.AnimDataOffset + " Offset = " + tr.Offset + "  Scale = " + tr.Scale);


                    //   Console.WriteLine();

                    float tanscale = tr.Delta;
                    if (tanscale == 0)
                        tanscale = 1;

                    for (int i = 0; i < (ushort)tr.Frames.Length; i++)
                    {
                        if (tr.CurveType == Syroot.NintenTools.NSW.Bfres.AnimCurveType.Cubic)
                        {
                            int framedata = (int)tr.Frames[i];
                            float keydata = tr.Offset + ((tr.Keys[i, 0] * tr.Scale));
                            float keydata2 = tr.Offset + ((tr.Keys[i, 1] * tr.Scale));
                            float keydata3 = tr.Offset + ((tr.Keys[i, 2] * tr.Scale));
                            float keydata4 = tr.Offset + ((tr.Keys[i, 3] * tr.Scale));
                            //    Console.WriteLine($"{framedata} {keydata} {keydata2} {keydata3} {keydata4} ");
                            //     Console.WriteLine($"Raw Data = " + tr.Keys[i, 0]);

                        }
                        if (tr.KeyType == AnimCurveKeyType.Int16)
                        {

                        }
                        else if (tr.KeyType == AnimCurveKeyType.Single)
                        {

                        }
                        else if (tr.KeyType == AnimCurveKeyType.SByte)
                        {

                        }
                        t.keys.Add(new FMAAKey()
                        {
                            frame = (int)tr.Frames[i],
                            unk1 = tr.Offset + ((tr.Keys[i, 0] * tr.Scale)),
                            unk2 = tr.Offset + ((tr.Keys[i, 1] * tr.Scale)),
                            unk3 = tr.Offset + ((tr.Keys[i, 2] * tr.Scale)),
                            unk4 = tr.Offset + ((tr.Keys[i, 3] * tr.Scale)),
                        });
                    }
                }
            }
        }
        public class FMAATrack
        {
            public short type;
            public short keyCount;
            public int flag;
            public int unk2;
            public int padding1;
            public int padding2;
            public int padding3;
            public float frameCount;
            public float scale, init, unkf3;
            public long offtolastKeys, offtolastData;
            public List<FMAAKey> keys = new List<FMAAKey>();

            public int offset;

            public FMAAKey GetLeft(int frame)
            {
                FMAAKey prev = keys[0];

                for (int i = 0; i < keys.Count - 1; i++)
                {
                    FMAAKey key = keys[i];
                    if (key.frame > frame && prev.frame <= frame)
                        break;
                    prev = key;
                }

                return prev;
            }
            public FMAAKey GetRight(int frame)
            {
                FMAAKey cur = keys[0];
                FMAAKey prev = keys[0];

                for (int i = 1; i < keys.Count; i++)
                {
                    FMAAKey key = keys[i];
                    cur = key;
                    if (key.frame > frame && prev.frame <= frame)
                        break;
                    prev = key;
                }

                return cur;
            }
        }
        public class FMAAKey
        {
            public int frame;
            public float unk1, unk2, unk3, unk4;

            public int offset;
        }
    }
}

