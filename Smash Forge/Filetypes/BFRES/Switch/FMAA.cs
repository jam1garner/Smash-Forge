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

        public List<BFRES_MTA> matanims = new List<BFRES_MTA>();



        public void Read(string filename, BFRES bfres, AnimationGroupNode ThisAnimation, ModelContainer modelContainer)
        {
            Console.WriteLine("Reading Material Animations ...");

            ResFile b = new ResFile(filename);

            ThisAnimation.Text = "Material Animations" ;

    

            TreeNode dummy = new TreeNode() { Text = "Animation Set" };

            int i = 0;
            foreach (MaterialAnim vis in b.MaterialAnims)
            {
                modelContainer.BFRES_MTA = new BFRES_MTA(vis);


                ThisAnimation.Nodes.Add(modelContainer.BFRES_MTA);
            }
        }
    }

    public class BFRES_MTA : TreeNode
    {
        public uint FrameCount;

        public List<MatAnimEntry> matEntries = new List<MatAnimEntry>();

        

        public void ExpandNodes()
        {
            Nodes.Clear();
            TreeNode mat = new TreeNode();
            foreach (MatAnimEntry e in matEntries)
            {
                mat.Text = e.Name;
                mat.Nodes.Add(e);
            }
            Nodes.Add(mat);
        }

        public BFRES_MTA(MaterialAnim vis)
        {
            ImageKey = "image";
            SelectedImageKey = "image";


            Text = vis.Name;

            FrameCount = (uint)vis.FrameCount;

            foreach (MaterialAnimData matanim in vis.MaterialAnimDataList)
            {
                MatAnimEntry mat = new MatAnimEntry();

                FMAANode matnode = new FMAANode(matanim);

                mat.Text = matanim.Name;

                for (int Frame = 0; Frame < vis.FrameCount; Frame++)
                {
                    int CurTrack = 0;
                    foreach (FMAATrack track in matnode.tracks)
                    {
                        MatAnimData md = new MatAnimData();

                        float value;

                        if (matanim.Curves[CurTrack].CurveType == AnimCurveType.Cubic)
                        {
                            FMAAKey left = track.GetLeft(Frame);
                            FMAAKey right = track.GetRight(Frame);

                            value = Animation.Hermite(Frame, left.frame, right.frame, 0, 0, left.unk1, right.unk1);

                            md.keys.Add(value);
                        }


                        mat.matCurves.Add(md);
                        CurTrack++;
                    }
                }

                matEntries.Add(mat);
            }
        }

        public class MatAnimEntry : TreeNode
        {
            public List<MatAnimData> matCurves = new List<MatAnimData>();

        }
        public class MatAnimData
        {
            public class frame
            {
                //public int size;
                public float[] values;
            }

            public List<float> keys = new List<float>();

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

