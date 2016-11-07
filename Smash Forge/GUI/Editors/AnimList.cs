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
        public AnimListPanel()
        {
            InitializeComponent();
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
                    Runtime.TargetAnim = Runtime.Animations[e.Node.Text];
                    Runtime.TargetAnimString = e.Node.Text;
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
    }
}
