using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Smash_Forge
{
    public partial class AnimListPanel : DockContent
    {
        
        public static ImageList iconList = new ImageList();

        public AnimListPanel()
        {
            InitializeComponent();
            iconList.ImageSize = new Size(24, 24);
            iconList.Images.Add("thesex", Properties.Resources.sexy_green_down_arrow);
            iconList.Images.Add("anim", Properties.Resources.icon_anim);
            iconList.Images.Add("bone", Properties.Resources.icon_bone);
            iconList.Images.Add("frame", Properties.Resources.icon_model);
            treeView1.ImageList = iconList;
        }

        private void lstAnims_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Runtime.TargetAnim = Runtime.Animations[lstAnims.SelectedItem.ToString()];
            //Runtime.TargetAnimString = lstAnims.SelectedItem.ToString();
        }

        private void selectItem(object sender, TreeNodeMouseClickEventArgs e)
        {
            if(e.Node.Level == 1)
            {
                if (e.Node.Parent.Text == "Bone Animations")
                {
                    Runtime.TargetAnimString = e.Node.Text;
                    Runtime.TargetAnim = (Animation)e.Node;// Runtime.Animations[e.Node.Text];
                    
                    //reset mtas
                    foreach(ModelContainer con in Runtime.ModelContainers)
                    {
                        if(con.nud != null && con.mta != null)
                        {
                            con.nud.applyMTA(con.mta, 0);
                            foreach (KeyValuePair<string, MTA> v in Runtime.MaterialAnimations)
                            {
                                if (v.Key.Contains("display"))
                                {
                                    con.nud.applyMTA(v.Value, 0);
                                    break;
                                }
                            }
                        }
                    }

                    Runtime.TargetMTA.Clear();
                    foreach (KeyValuePair<string, MTA> v in Runtime.MaterialAnimations)
                    {
                        if(v.Key.Contains(e.Node.Text.Replace(".omo", "")))
                        {
                            Runtime.TargetMTA.Add(v.Value);
                        }
                    }

                    //MainForm.Instance.viewports[0].loadMTA(Runtime.MaterialAnimations[e.Node.Text]);

                    //Console.WriteLine("Selected Anim " + e.Node.Text);
                }
                else if(e.Node.Parent.Text == "Material Animations")
                {
                    MainForm.Instance.viewports[0].loadMTA(Runtime.MaterialAnimations[e.Node.Text]);
                    //Runtime.TargetMTA = ;
                    Runtime.TargetMTAString = e.Node.Text;
                }
            }
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode is Animation)
            {
                ((Animation)treeView1.SelectedNode).ExpandBones();
            }
        }
    }
}
