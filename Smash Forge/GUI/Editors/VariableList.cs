using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace SmashForge
{
    public partial class VariableList : DockContent
    {
        public VariableList()
        {
            InitializeComponent();
        }

        private bool variableValuesSet = false;

        public void Refresh()
        {
            if (Runtime.gameAcmdScript != null)
            {
                listBox1.BeginUpdate();
                listBox1.Items.Clear();
                foreach (var flag in Runtime.gameAcmdScript.ActiveFlags)
                {
                    listBox1.Items.Add(flag.ToString());
                }
                if (Runtime.gameAcmdScript.ledgeGrabDisallowed)
                    listBox1.Items.Add("Ledge grab disallowed");
                if (Runtime.gameAcmdScript.frontLedgeGrabAllowed)
                    listBox1.Items.Add("Front Ledge grab allowed");
                if (Runtime.gameAcmdScript.reverseLedgeGrabAllowed)
                    listBox1.Items.Add("Reverse Ledge grab allowed");

                listBox1.EndUpdate();

                treeView1.BeginUpdate();
                treeView1.Nodes.Clear();
                if (Runtime.gameAcmdScript != null)
                {
                    foreach (var pair in Runtime.gameAcmdScript.IfVariableList)
                    {
                        TreeNode node = new TreeNode(pair.Key.ToString()) { Tag = pair.Key, Checked = pair.Value };

                        treeView1.Nodes.Add(node);
                    }
                }
                treeView1.EndUpdate();

                if (!variableValuesSet)
                {
                    flowLayoutPanel1.Controls.Clear();
                    foreach (var pair in Runtime.gameAcmdScript.VariableValueList)
                    {
                        Label label = new Label() { Text = pair.Key.ToString() };
                        ComboBox comboBox = new ComboBox() { Tag = pair.Key };
                        flowLayoutPanel1.Controls.Add(label);
                        flowLayoutPanel1.Controls.Add(comboBox);
                        label.Update();
                        foreach (var value in pair.Value)
                            comboBox.Items.Add(value);

                        label.Width = 800;

                        comboBox.SelectedItem = Runtime.gameAcmdScript.IfVariableValueList[pair.Key];

                        comboBox.SelectedIndexChanged += (sender, e) =>
                        {
                            Runtime.gameAcmdScript.IfVariableValueList[(SALT.Moveset.FighterVariable)comboBox.Tag] = (int)comboBox.SelectedItem;
                        };
                    }
                    variableValuesSet = true;
                }
            }
            else
            {
                flowLayoutPanel1.Controls.Clear();
                treeView1.Nodes.Clear();
                listBox1.Items.Clear();
            }
        }

        public void Initialize()
        {
            variableValuesSet = false;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode.Index > -1)
            {
                
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Index > -1)
                Runtime.gameAcmdScript.IfVariableList[(SALT.Moveset.FighterVariable)e.Node.Tag] = e.Node.Checked;
        }

    }
}
