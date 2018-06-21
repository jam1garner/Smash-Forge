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

    public class FVIS
    {
        private const string TEMP_FILE = "temp.bfres";

        public static AnimationGroupNode ThisAnimation;

        public static AnimationGroupNode Read(string filename, ResFile TargetWiiUBFRES, BFRES bfres)
        {
            string path = filename;

            FileData f = new FileData(filename);


            int Magic = f.readInt();

            if (Magic == 0x59617A30) //YAZO compressed
            {
                using (FileStream input = new FileStream(path, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Yaz0Compression.Decompress(path, TEMP_FILE);

                    path = TEMP_FILE;
                }
            }

            f = new FileData(path);

            f.seek(0);

            f.Endian = Endianness.Little;

            Console.WriteLine("Reading Animations ...");

            f.seek(4); // magic check
            int SwitchCheck = f.readInt(); //Switch version only has padded magic
            f.skip(4);


            //    SwitchAnim2WiiU(path); //Hacky auto convert switch anims to wii u


            Syroot.NintenTools.NSW.Bfres.ResFile b = new Syroot.NintenTools.NSW.Bfres.ResFile(path);

            AnimationGroupNode ThisAnimation = new AnimationGroupNode() { Text = "Bone Visual Animations" };

            TreeNode dummy = new TreeNode() { Text = "Animation Set" };

            int i = 0;
            foreach (Syroot.NintenTools.NSW.Bfres.VisibilityAnim vis in b.BoneVisibilityAnims)
            {

                Animation a = new Animation(vis.Name);

                ThisAnimation.Nodes.Add(a);

                a.FrameCount = vis.FrameCount;
                i++;

                int boneindx = 0;
                if (vis.Names != null)
                {
                    foreach (string nm in vis.Names) //Loop through every bone. Not all have base and curve data
                    {
                        Animation.KeyNode bone = new Animation.KeyNode("");
                        a.Bones.Add(bone);
                        bone.Text = vis.Names[boneindx];



                        if (boneindx < vis.BaseDataList.Length)
                        {
                            bool bas = vis.BaseDataList[boneindx];

                            if (bas == true)
                            {
                                bone.XSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = 1 });
                                bone.YSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = 1 });
                                bone.ZSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = 1 });

                            }
                            else
                            {
                                bone.XSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = 0 });
                                bone.YSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = 0 });
                                bone.ZSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = 0 });
                            }
                        }


                        if (vis.Curves.Count != 0)
                        {
                            if (boneindx < vis.Curves.Count)
                            {
                                Syroot.NintenTools.NSW.Bfres.AnimCurve cr = vis.Curves[boneindx];


                                int frm = 0;
                                foreach (bool bn in cr.KeyStepBoolData)
                                {
                                    Animation.KeyFrame frame = new Animation.KeyFrame();
                                    frame.InterType = Animation.InterpolationType.STEP;
                                    frame.Frame = cr.Frames[frm];



                                    switch (bn)
                                    {
                                        case true:
                                            frame.Value = 1; bone.XSCA.Keys.Add(frame);
                                            frame.Value = 1; bone.YSCA.Keys.Add(frame);
                                            frame.Value = 1; bone.ZSCA.Keys.Add(frame);
                                            break;
                                        case false:
                                            frame.Value = 0; bone.XSCA.Keys.Add(frame);
                                            frame.Value = 0; bone.YSCA.Keys.Add(frame);
                                            frame.Value = 0; bone.ZSCA.Keys.Add(frame);
                                            break;
                                    }
                                    frm++;
                                }
                            }
                        }

                        boneindx++;
                    }
                }
            
            }
            return ThisAnimation;
        }

    }
}

