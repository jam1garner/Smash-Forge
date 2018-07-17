using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Smash_Forge
{
    public partial class BYAMLList :  DockContent
    {
        public BYAMLList()
        {
            InitializeComponent();
        }

        public BYAML TargetBYAML;
        public BYAMLEditor BYAMLEditor;


        public void fillList()
        {
            if (TargetBYAML != null)
            {
                Console.WriteLine("Adding items to list");
                treeView1.Nodes.Add(TargetBYAML.Areas);
                treeView1.Nodes.Add(TargetBYAML.Clips);
                treeView1.Nodes.Add(TargetBYAML.ClipAreas);
                treeView1.Nodes.Add(TargetBYAML.ClipPatterns);
                treeView1.Nodes.Add(TargetBYAML.EnemyPaths);
                treeView1.Nodes.Add(TargetBYAML.FirstCurve);
                treeView1.Nodes.Add(TargetBYAML.GliderPaths);
                treeView1.Nodes.Add(TargetBYAML.GravityCamPaths);
                treeView1.Nodes.Add(TargetBYAML.GravityPaths);
                treeView1.Nodes.Add(TargetBYAML.IntroCameras);
                treeView1.Nodes.Add(TargetBYAML.ItemPaths);
                treeView1.Nodes.Add(TargetBYAML.JugemPaths);
                treeView1.Nodes.Add(TargetBYAML.LapPaths);
                treeView1.Nodes.Add(TargetBYAML.Objs);
                treeView1.Nodes.Add(TargetBYAML.ObjPaths);
                treeView1.Nodes.Add(TargetBYAML.Paths);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!(treeView1.SelectedNode is BYAML.PathPoint))
                return;

            BYAML.PathPoint pt = (BYAML.PathPoint)treeView1.SelectedNode;

            if (e.Node.Level != 0)
            {
                BYAMLEditor.Open(pt);
            }
        }
    }
}
