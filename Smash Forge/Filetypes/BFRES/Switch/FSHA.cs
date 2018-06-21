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



        public void Read(string filename, BFRES bfres, AnimationGroupNode ThisAnimation, ModelContainer modelContainer)
        {
            Console.WriteLine("Reading Shape Animations ...");

            ResFile b = new ResFile(filename);

            ThisAnimation.Text = "Shape Animations";



            TreeNode dummy = new TreeNode() { Text = "Animation Set" };

            int i = 0;
            foreach (ShapeAnim fsha in b.ShapeAnims)
            {
                modelContainer.BFRES_MTA = new BFRES.MTA();

                PerShapeAnim perAnim = new PerShapeAnim(modelContainer.BFRES_MTA, fsha);

                ThisAnimation.Nodes.Add(modelContainer.BFRES_MTA);
            }
        }
    }

    public class PerShapeAnim
    {
        public PerShapeAnim(BFRES.MTA mta, ShapeAnim vis)
        {

            mta.Text = vis.Name;

            mta.FrameCount = (uint)vis.FrameCount;


            foreach (VertexShapeAnim vtxanim in vis.VertexShapeAnims)
            {
                BFRES.MatAnimEntry mat = new BFRES.MatAnimEntry();

                mat.Text = vtxanim.Name;

                int CurCurve = 0;
                foreach (AnimCurve cr in vtxanim.Curves)
                {
                    for (int i = 0; i < (ushort)cr.Frames.Length; i++)
                    {
                        BFRES.MatAnimData md = new BFRES.MatAnimData();


                        
                        mat.matCurves.Add(md);
                    }
                    CurCurve++;
                }

                mta.matEntries.Add(mat);
            }
        }


        public class FSHANode
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
            public List<FSHATrack> tracks = new List<FSHATrack>();

            public FSHANode(MaterialAnimData md)
            {
                Text = md.Name;



                foreach (AnimCurve tr in md.Curves)
                {
                    FSHATrack t = new FSHATrack();
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
                        t.keys.Add(new FSHAKey()
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
        public class FSHATrack
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
            public List<FSHAKey> keys = new List<FSHAKey>();

            public int offset;

            public FSHAKey GetLeft(int frame)
            {
                FSHAKey prev = keys[0];

                for (int i = 0; i < keys.Count - 1; i++)
                {
                    FSHAKey key = keys[i];
                    if (key.frame > frame && prev.frame <= frame)
                        break;
                    prev = key;
                }

                return prev;
            }
            public FSHAKey GetRight(int frame)
            {
                FSHAKey cur = keys[0];
                FSHAKey prev = keys[0];

                for (int i = 1; i < keys.Count; i++)
                {
                    FSHAKey key = keys[i];
                    cur = key;
                    if (key.frame > frame && prev.frame <= frame)
                        break;
                    prev = key;
                }

                return cur;
            }
        }
        public class FSHAKey
        {
            public int frame;
            public float unk1, unk2, unk3, unk4;

            public int offset;
        }
    }
}

