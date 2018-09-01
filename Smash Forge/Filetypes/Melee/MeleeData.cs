using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MeleeLib.DAT;
using MeleeLib.IO;
using SFGraphics.Cameras;

namespace Smash_Forge
{
    public class MeleeDataNode : TreeNode
    {
        public DATFile DatFile;

        private bool hasCreatedRenderMeshes = false;

        public MeleeDataNode(string fname)
        {
            DatFile = Decompiler.Decompile(File.ReadAllBytes(fname));
            Text = "HAL DAT FILE";
        }

        public void RefreshDisplay()
        {
            Nodes.Clear();
            foreach(DATRoot root in DatFile.Roots)
            {
                MeleeRootNode ro = new MeleeRootNode(root);
                Nodes.Add(ro);
                ro.RefreshDisplay();
            }
        }

        public void Render(Camera c)
        {
            if (!hasCreatedRenderMeshes)
            {
                RefreshDisplay();
                hasCreatedRenderMeshes = true;
            }

            foreach (MeleeRootNode n in Nodes)
            {
                n.Render(c);
            }
        }
    }
    
}
