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
using SALT.PARAMS;
using System.Text.RegularExpressions;
using System.IO;

namespace Smash_Forge
{
    public partial class PARAMEditor : DockContent
    {
        public PARAMEditor(string filename)
        {
            InitializeComponent();
            tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("Name") { ReadOnly = true });
            tbl.Columns.Add("Value");
            dataGridView1.DataSource = tbl;
            labels = new IniLabels(filename);
            openParam(filename);
        }

        private IniLabels labels;
        private ParamFile p;
        private DataTable tbl;
        private int[] currentEntry = new int[2];
        private bool notSaved = false;

        private string getEntryName(int i, int j, int k)
        {
            foreach (IniLabels.Label label in labels.labels)
                if (label.Type == IniLabels.Label.type.Value && (label.group == -1 || label.group == i) && (label.entry == -1 || label.entry == j) && label.value == k)
                    return label.name;
            return $"{k}";
        }

        private void fillTable(int groupNum, int entryNum)
        {
            currentEntry[0] = groupNum;
            currentEntry[1] = entryNum;
            tbl.Clear();
            if (p.Groups.Count > groupNum)
            {
                if (!(p.Groups[groupNum] is ParamGroup))
                {
                    int i = 0;
                    foreach (ParamEntry val in p.Groups[groupNum].Values)
                    {
                        DataRow tempRow = tbl.NewRow();
                        tempRow[0] = getEntryName(groupNum, entryNum, i);
                        tempRow[1] = val.Value;
                        tbl.Rows.Add(tempRow);
                        i++;
                    }
                }
                else
                {
                    int entrySize = ((ParamGroup)p.Groups[groupNum]).EntrySize;
                    for (int j = 0; j < entrySize; j++)
                    {
                        DataRow tempRow = tbl.NewRow();
                        tempRow[0] = getEntryName(groupNum, entryNum, j);
                        tempRow[1] = p.Groups[groupNum].Values[entrySize * entryNum + j].Value;
                        tbl.Rows.Add(tempRow);
                    }
                }
            }
        }

        private string getGroupName(int i)
        {
            foreach(IniLabels.Label label in labels.labels)
                if(label.Type == IniLabels.Label.type.Group && label.group == i)
                    return label.name;
            return "Group [" + (i + 1) + "]";
        }

        private string getEntryName(int i, int j)
        {
            foreach(IniLabels.Label label in labels.labels)
                if (label.Type == IniLabels.Label.type.Entry && (label.group == -1 || label.group == i) && label.entry == j)
                    return label.name;
            return "Entry [" + j + "]";
        }

        private void openParam(string f)
        {
            p = new ParamFile(f);
            for(int i = 0; i < p.Groups.Count; i++)
            {
                if (p.Groups[i] is ParamGroup) {
                    TreeNode[] children = new TreeNode[((ParamGroup)p.Groups[i]).EntryCount];
                    for (int j = 0; j < ((ParamGroup)p.Groups[i]).EntryCount; j++)
                    {
                        TreeNode child = new TreeNode(getEntryName(i, j));
                        int[] temp1 = new int[2];
                        temp1[0] = i;
                        temp1[1] = j;
                        child.Tag = temp1;
                        children[j] = child;
                    }
                    TreeNode parent = new TreeNode(getGroupName(i), children);
                    int[] temp = new int[2];
                    temp[0] = i;
                    temp[1] = 0;
                    parent.Tag = temp;
                    treeView1.Nodes.Add(parent);
                }
                else
                {
                    int[] temp = { i, 0 };
                    treeView1.Nodes.Add(new TreeNode(getGroupName(i)) { Tag = temp });
                }
            }
            fillTable(0,0);
        }

        private void select(object sender, TreeViewEventArgs e)
        {
            fillTable(((int[])e.Node.Tag)[0], ((int[])e.Node.Tag)[1]);
        }

        private void edit(object sender, DataGridViewCellEventArgs e)
        {
            IParamCollection i = p.Groups[currentEntry[0]];
            if (i is ParamGroup)
            {
                ParamGroup pg = (ParamGroup)i;
                switch(pg.Values[pg.EntrySize * currentEntry[1] + e.RowIndex].Type)
                {
                    case ParamType.str:
                        pg.Values[pg.EntrySize * currentEntry[1] + e.RowIndex].Value = tbl.Rows[e.RowIndex][1];
                        break;
                    case ParamType.s8:
                        pg.Values[pg.EntrySize * currentEntry[1] + e.RowIndex].Value = Convert.ToByte(tbl.Rows[e.RowIndex][1]);
                        break;
                    case ParamType.u8:
                        pg.Values[pg.EntrySize * currentEntry[1] + e.RowIndex].Value = Convert.ToByte(tbl.Rows[e.RowIndex][1]);
                        break;
                    case ParamType.s16:
                        pg.Values[pg.EntrySize * currentEntry[1] + e.RowIndex].Value = Convert.ToInt16(tbl.Rows[e.RowIndex][1]);
                        break;
                    case ParamType.u16:
                        pg.Values[pg.EntrySize * currentEntry[1] + e.RowIndex].Value = Convert.ToUInt16(tbl.Rows[e.RowIndex][1]);
                        break;
                    case ParamType.s32:
                        pg.Values[pg.EntrySize * currentEntry[1] + e.RowIndex].Value = Convert.ToInt32(tbl.Rows[e.RowIndex][1]);
                        break;
                    case ParamType.u32:
                        pg.Values[pg.EntrySize * currentEntry[1] + e.RowIndex].Value = Convert.ToUInt32(tbl.Rows[e.RowIndex][1]);
                        break;
                    case ParamType.f32:
                        pg.Values[pg.EntrySize * currentEntry[1] + e.RowIndex].Value = Convert.ToSingle(tbl.Rows[e.RowIndex][1]);
                        break;
                }
                //pg.Values[pg.EntrySize * currentEntry[1] + e.RowIndex].Value = tbl.Rows[e.RowIndex][1];
            }
            else
            {
                switch (i.Values[e.RowIndex].Type)
                {
                    case ParamType.str:
                        i.Values[e.RowIndex].Value = tbl.Rows[e.RowIndex][1];
                        break;
                    case ParamType.s8:
                        i.Values[e.RowIndex].Value = Convert.ToByte(tbl.Rows[e.RowIndex][1]);
                        break;
                    case ParamType.u8:
                        i.Values[e.RowIndex].Value = Convert.ToByte(tbl.Rows[e.RowIndex][1]);
                        break;
                    case ParamType.s16:
                        i.Values[e.RowIndex].Value = Convert.ToInt16(tbl.Rows[e.RowIndex][1]);
                        break;
                    case ParamType.u16:
                        i.Values[e.RowIndex].Value = Convert.ToUInt16(tbl.Rows[e.RowIndex][1]);
                        break;
                    case ParamType.s32:
                        i.Values[e.RowIndex].Value = Convert.ToInt32(tbl.Rows[e.RowIndex][1]);
                        break;
                    case ParamType.u32:
                        i.Values[e.RowIndex].Value = Convert.ToUInt32(tbl.Rows[e.RowIndex][1]);
                        break;
                    case ParamType.f32:
                        i.Values[e.RowIndex].Value = Convert.ToSingle(tbl.Rows[e.RowIndex][1]);
                        break;
                }
                //i.Values[e.RowIndex].Value = tbl.Rows[e.RowIndex][1];
            }
            if (!notSaved)
            {
                Text += "*";
                notSaved = true;
            }
        }

        private void save()
        {
            p.Export(p.Filepath);
            notSaved = false;
            if (Text.EndsWith("*"))
                Text = Text.Substring(0, Text.Length - 1);
        }

        public void saveAs()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Paramter Files (.bin)|*.bin|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string filename = sfd.FileName;
                    p.Export(filename);
                    notSaved = false;
                    if (Text.EndsWith("*"))
                        Text = Text.Substring(0, Text.Length - 1);
                }
            }

        }

        public class IniLabels
        {
            public class Label
            {
                public enum type
                {
                    Group,
                    Entry,
                    Value
                }
                public type Type;
                public string name;
                public int group = -1;
                public int entry = -1;
                public int value = -1;
            }

            public List<Label> labels = new List<Label>();

            public IniLabels(string filename)
            {
                List<string> labelPaths = new List<string>();
                string noExtension = Path.GetFileNameWithoutExtension(filename);
                if (Directory.Exists(Path.Combine(MainForm.executableDir, "param_labels\\")))
                {
                    foreach (string file in Directory.GetFiles(Path.Combine(MainForm.executableDir, "param_labels\\")))
                    {
                        string[] ini = File.ReadAllLines(file);
                        if (ini.Length > 0 && ini[0].StartsWith("name"))
                        {
                            string name = ini[0].Split('=')[1];
                            if ((new Regex(name)).IsMatch(noExtension))
                                labelPaths.Add(file);
                        }
                    }
                }
                string testPath = Path.Combine(MainForm.executableDir, Path.Combine("param_labels\\", Path.ChangeExtension(Path.GetFileName(filename), ".ini")));
                if (File.Exists(testPath))
                    labelPaths.Add(testPath);

                foreach(string labelPath in labelPaths)
                {
                    string[] ini = File.ReadAllLines(labelPath);
                    for (int i = 0; i < ini.Length; i++)
                    {
                        if(ini[i].Length > 0 && ini[i][0] == '[' && ini[i].IndexOf(']') != -1)
                        {
                            Label label = new Label();
                            string name = ini[i].Substring(1, ini[i].IndexOf(']') - 1);
                            Label.type temp;
                            if(Enum.TryParse(name, out temp))
                            {
                                label.Type = temp;
                                for (int j = 1; j <= 4 && i + j < ini.Length; j++)
                                {
                                    if (ini[i + j].Length > 0 && ini[i + j][0] == '[')
                                        break;
                                    if (ini[i + j].StartsWith("name"))
                                        label.name = ini[i + j].Split('=')[1].Trim();
                                    if (ini[i + j].StartsWith("group"))
                                        label.group = int.Parse(ini[i + j].Split('=')[1].Trim()) - 1;
                                    if (ini[i + j].StartsWith("entry"))
                                        label.entry = int.Parse(ini[i + j].Split('=')[1].Trim());
                                    if (ini[i + j].StartsWith("value"))
                                        label.value = int.Parse(ini[i + j].Split('=')[1].Trim());
                                }
                                labels.Add(label);
                            }
                        }
                    }
                }
            }
        }
    }
}
