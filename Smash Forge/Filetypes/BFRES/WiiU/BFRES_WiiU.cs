using OpenTK;
using OpenTK.Graphics.OpenGL;
using Syroot.NintenTools.Bfres;
using Syroot.NintenTools.Bfres.GX2;
using Syroot.NintenTools.Bfres.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Syroot.NintenTools.Yaz0;

namespace Smash_Forge
{
    public class BFRES_WiiU : FileBase
    {
        public override Endianness Endian { get; set; }

    
        public Dictionary<string, FTEX> textures = new Dictionary<string, FTEX>();
        public int FSKACount;

        public List<BFRES.FMDL_Model> models = new List<BFRES.FMDL_Model>();

        public TreeNode Models = new TreeNode() { Text = "Models", Checked = true };
        public TreeNode SkeletalAnim = new TreeNode() { Text = "Skeletal Animations" };
        public TreeNode Textures = new TreeNode() { Text = "Textures" };
        public TreeNode Shaderparam = new TreeNode() { Text = "Shader Param Animations" };
        public TreeNode Coloranim = new TreeNode() { Text = "Color Animations" };
        public TreeNode TextureSRT = new TreeNode() { Text = "Texture STR Animations" };
        public TreeNode TexturePat = new TreeNode() { Text = "Texture Pattern Animations" };
        public TreeNode Bonevisabilty = new TreeNode() { Text = "Bone Visabilty" };
        public TreeNode VisualAnim = new TreeNode() { Text = "Visual Animations" };
        public TreeNode ShapeAnim = new TreeNode() { Text = "Shape Animations" };
        public TreeNode SceneAnim = new TreeNode() { Text = "Scene Animations" };
        public TreeNode Embedded = new TreeNode() { Text = "Embedded Files" };

        public static int sign10Bit(int i)
        {
            if (((i >> 9) & 0x1) == 1)
            {
                i = ~i;
                i = i & 0x3FF;
                i += 1;
                i *= -1;
            }

            return i;
        }


        public string path = "";

        public ResFile TargetWiiUBFRES;

 
        private const string TEMP_FILE = "temp.bfres";

        public override void Read(string filename)
        {
            FileData f = new FileData(filename);
  
            
        }
        public override byte[] Rebuild()
        {
            throw new Exception("Unsupported atm :(");
        }
    }
}
