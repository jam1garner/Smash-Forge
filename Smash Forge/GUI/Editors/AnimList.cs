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
    }
}
