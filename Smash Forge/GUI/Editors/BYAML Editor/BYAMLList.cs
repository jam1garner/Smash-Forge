using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace SmashForge
{
    public partial class ByamlList :  DockContent
    {
        public ByamlList()
        {
            InitializeComponent();
        }

        public BYAML targetByaml;
        public ByamlEditor byamlEditor;


        public void FillList()
        {
            if (targetByaml != null)
            {
                Console.WriteLine("Adding items to list");
                treeView1.Nodes.Add(targetByaml.Areas);
                treeView1.Nodes.Add(targetByaml.Clips);
                treeView1.Nodes.Add(targetByaml.ClipAreas);
                treeView1.Nodes.Add(targetByaml.ClipPatterns);
                treeView1.Nodes.Add(targetByaml.EnemyPaths);
                treeView1.Nodes.Add(targetByaml.FirstCurve);
                treeView1.Nodes.Add(targetByaml.GliderPaths);
                treeView1.Nodes.Add(targetByaml.GravityCamPaths);
                treeView1.Nodes.Add(targetByaml.GravityPaths);
                treeView1.Nodes.Add(targetByaml.IntroCameras);
                treeView1.Nodes.Add(targetByaml.ItemPaths);
                treeView1.Nodes.Add(targetByaml.JugemPaths);
                treeView1.Nodes.Add(targetByaml.LapPaths);
                treeView1.Nodes.Add(targetByaml.Objs);
                treeView1.Nodes.Add(targetByaml.ObjPaths);
                treeView1.Nodes.Add(targetByaml.Paths);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!(treeView1.SelectedNode is BYAML.PathPoint))
                return;

            BYAML.PathPoint pt = (BYAML.PathPoint)treeView1.SelectedNode;

            if (e.Node.Level != 0)
            {
                byamlEditor.Open(pt);
            }
        }
    }
}
