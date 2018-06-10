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

        public List<MatAnimEntry> matanims = new List<MatAnimEntry>();

        public class MatAnimEntry
        {
           public string Name;
        }

        public AnimationGroupNode Read(string filename, BFRES bfres)
        {
            Console.WriteLine("Reading Material Animations ...");

            ResFile b = new ResFile(filename);

            AnimationGroupNode ThisAnimation = new AnimationGroupNode() { Text = "Material Animations" };

            TreeNode dummy = new TreeNode() { Text = "Animation Set" };

            int i = 0;
            foreach (MaterialAnim vis in b.MaterialAnims)
            {
                Animation a = new Animation(vis.Name);

                ThisAnimation.Nodes.Add(a);

                a.FrameCount = vis.FrameCount;
                i++;
                foreach (MaterialAnimData matanim in vis.MaterialAnimDataList)
                {
                    MatAnimEntry mat = new MatAnimEntry();

                    mat.Name = matanim.Name;


                    foreach (AnimCurve cr in matanim.Curves)
                    {
                        TreeNode TCurveAnim = new TreeNode() { Text = "Curve" };

                    }

                    matanims.Add(mat);
                }
              
            }
            return ThisAnimation;
        }

    }
}

