using System;
using MeleeLib.DAT.Melee;
using MeleeLib.DAT.Animation;
using System.Windows.Forms;

namespace SmashForge.Filetypes.Melee
{

    public class MeleeMapModelNode : MeleeNode
    {
        public Map_Model_Group ModelGroup;

        public MeleeMapModelNode(Map_Model_Group ModelGroup)
        {
            Text = "ModelGroup";
            this.ModelGroup = ModelGroup;
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            Nodes.Clear();
            {
                MeleeRootNode n = new MeleeRootNode(ModelGroup.BoneRoot);
                n.Text = "Model";
                Nodes.Add(n);
                n.RefreshDisplay();
            }

            {
                Nodes.Add(new MeleeMapAnimNode(ModelGroup.QuakeRoot) { Text = "StageAnimation" });
            }
        }
    }

    public class MeleeMapAnimNode : MeleeJointAnimationNode
    {
        public Map_Animation_Quake RootBone;

        public MeleeMapAnimNode(Map_Animation_Quake RootBone)
        {
            this.RootBone = RootBone;

            ImageKey = "anim";
            SelectedImageKey = "anim";

            DatAnimation = new DatAnimation();
            float FrameCount = 0;
            foreach(Map_Animation_Quake q in RootBone.GetNodesInOrder())
            {
                if (q.NodeData != null)
                {
                    FrameCount = Math.Max(FrameCount, q.NodeData.FrameCount);
                    DatAnimation.Nodes.Add(q.NodeData);
                }
                else
                {
                    DatAnimation.Nodes.Add(new DatAnimationNode());
                }
            }
            DatAnimation.FrameCount = FrameCount;


            ContextMenu = new ContextMenu();
            
            MenuItem ImportM = new MenuItem("Import");
            ImportM.Click += Import;
            ContextMenu.MenuItems.Add(ImportM);

            MenuItem SaveAsM = new MenuItem("Save As");
            SaveAsM.Click += SaveAs;
            ContextMenu.MenuItems.Add(SaveAsM);
        }

        public override Animation GetAnimation()
        {
            Animation normal = base.GetAnimation();

            int boneIndex = 0;
            foreach (Map_Animation_Quake q in RootBone.GetNodesInOrder())
            {
                if (q.NodeData != null)
                {
                    if (q.NodeData.JOBJ != null)
                    {
                        MeleeJointPath path = new MeleeJointPath(q.NodeData.JOBJ.Path, "Bone_" + boneIndex);
                        normal.Bones.AddRange(path.GetAnimation(q.NodeData.JOBJ).Bones);
                    }
                }
                boneIndex++;
            }
            
            return normal;
        }
    }
    
}
