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
    public partial class PARAMEditor : EditorBase
    {
        private static IniLabels labelDB;
        static PARAMEditor()
        {
            labelDB = new IniLabels();
        }

        private ParamFile p;
        private DataTable tbl;
        private int[] currentEntry = new int[2];
        private IniLabels.LabelFile labelFile;

        public PARAMEditor(string filename)
        {
            InitializeComponent();

            tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("Name") { ReadOnly = true });
            tbl.Columns.Add(new DataColumn("Type") { ReadOnly = true });
            tbl.Columns.Add("Value");
            dataGridView1.DataSource = tbl;
            labelFile = labelDB.GetLabels(Path.GetFileNameWithoutExtension(filename));
            openParam(filename);
            Edited = false;
        }

        public enum Columns : int
        {
            Name,
            Type,
            Value
        };

        private string getValueName(int i, int j, int k)
        {
            if (labelFile != null)
                foreach (IniLabels.Label label in labelFile.labels)
                    if (label.type == IniLabels.Label.LabelType.Value && (label.group == -1 || label.group == i) && (label.entry == -1 || label.entry == j) && label.value == k)
                        return label.name;
            return $"{k}";
        }

        private string getValueToolTip(int i, int j, int k)
        {
            if (labelFile != null)
                foreach (IniLabels.Label label in labelFile.labels)
                    if (label.type == IniLabels.Label.LabelType.Value && (label.group == -1 || label.group == i) && (label.entry == -1 || label.entry == j) && label.value == k)
                        return label.description;
            return null;
        }

        private static string getValueTypeString(ParamEntry pvalue)
        {
            switch (pvalue.Type)
            {
                case ParamType.s8:
                    return "int8";
                case ParamType.u8:
                    return "uint8";
                case ParamType.s16:
                    return "int16";
                case ParamType.u16:
                    return "uint16";
                case ParamType.s32:
                    return "int32";
                case ParamType.u32:
                    return "uint32";
                case ParamType.f32:
                    return "float";
                case ParamType.str:
                    return "string";
                default:
                    return "";
            }
        }

        private int getEntrySize(int groupNum)
        {
            int entrySize = -1;
            if (p.Groups[groupNum] is ParamGroup)
            {
                entrySize = ((ParamGroup)p.Groups[groupNum]).EntrySize;
            }
            else
            {
                if (labelFile != null)
                    foreach (IniLabels.Label label in labelFile.labels)
                        if (label.type == IniLabels.Label.LabelType.Group && label.group == groupNum && label.entries != -1)
                            entrySize = (int)((p.Groups[groupNum].Values.ToArray().Length / label.entries) + 0.5);
            }
            return entrySize;
        }

        private void fillTable(int groupNum, int entryNum)
        {
            currentEntry[0] = groupNum;
            currentEntry[1] = entryNum;
            tbl.Clear();
            if (p.Groups.Count > groupNum)
            {
                int entrySize = getEntrySize(groupNum);
                int count;
                if (entrySize == -1)
                {
                    count = p.Groups[groupNum].Values.Count;
                    entrySize = 0;
                }
                else
                {
                    count = entrySize;
                }

                for (int i = 0; i < count; i++)
                {
                    ParamEntry val = p.Groups[groupNum].Values[(entrySize * entryNum) + i];

                    DataRow tempRow = tbl.NewRow();
                    tempRow[(int)Columns.Name] = getValueName(groupNum, entryNum, i);
                    tempRow[(int)Columns.Type] = getValueTypeString(val);
                    tempRow[(int)Columns.Value] = val.Value;
                    string toolTip = getValueToolTip(groupNum, entryNum, i);
                    tbl.Rows.Add(tempRow);
                    //The Cells property only exists through the dataGridView, so the tooltip can only be set after adding the row
                    if (toolTip != null)
                        dataGridView1.Rows[i].Cells[0].ToolTipText = toolTip;
                }
            }
            //I couldn't find any way to manually set a width for a DataColumn, so this will have to do
            dataGridView1.AutoResizeColumn((int)Columns.Type, System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells);
        }

        private string getGroupName(int i)
        {
            if (labelFile != null)
                foreach (IniLabels.Label label in labelFile.labels)
                    if (label.type == IniLabels.Label.LabelType.Group && label.group == i)
                        return label.name;
            return "Group [" + (i + 1) + "]";
        }

        private string getEntryName(int i, int j)
        {
            if (labelFile != null)
                foreach (IniLabels.Label label in labelFile.labels)
                    if (label.type == IniLabels.Label.LabelType.Entry && (label.group == -1 || label.group == i) && label.entry == j)
                        return label.name;
            return "Entry [" + j + "]";
        }

        // Helper method for drawing some move intangibility
        public Dictionary<string, int> getMoveNameIdMapping()
        {
            Dictionary<string, int> moveNameIdMapping = new Dictionary<string, int>();

            for (int j = 0; j < ((ParamGroup)p.Groups[0]).EntryCount; j++)
                if (labelFile != null)
                    foreach (IniLabels.Label label in labelFile.labels)
                        if (label.type == IniLabels.Label.LabelType.Entry && label.group == 0 && label.entry == j)
                            moveNameIdMapping[label.name.Split(' ')[0]] = j;
            return moveNameIdMapping;
        }

        private void openParam(string f)
        {
            p = new ParamFile(f);
            for (int i = 0; i < p.Groups.Count; i++)
            {
                if (p.Groups[i] is ParamGroup)
                {
                    TreeNode[] children = new TreeNode[((ParamGroup)p.Groups[i]).EntryCount];
                    for (int j = 0; j < ((ParamGroup)p.Groups[i]).EntryCount; j++)
                    {
                        TreeNode child = new TreeNode(getEntryName(i, j));
                        children[j] = child;
                    }
                    TreeNode parent = new TreeNode(getGroupName(i), children);
                    treeView1.Nodes.Add(parent);
                }
                else
                {
                    int entryCount = -1;
                    if (labelFile != null)
                        foreach (IniLabels.Label label in labelFile.labels)
                            if (label.type == IniLabels.Label.LabelType.Group && label.group == i && label.entries != -1)
                                entryCount = label.entries;
                    if (entryCount == -1)
                    {
                        treeView1.Nodes.Add(new TreeNode(getGroupName(i)));
                    }
                    else
                    {
                        TreeNode[] children = new TreeNode[entryCount];
                        for (int j = 0; j < entryCount; j++)
                        {
                            TreeNode child = new TreeNode(getEntryName(i, j));
                            children[j] = child;
                        }
                        TreeNode parent = new TreeNode(getGroupName(i), children);
                        treeView1.Nodes.Add(parent);
                    }
                }
            }
            treeView1.SelectedNode = null;
        }

        private void select(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 0)
                fillTable(e.Node.Index, 0);
            else
                fillTable(e.Node.Parent.Index, e.Node.Index);
        }

        private void edit(object sender, DataGridViewCellEventArgs e)
        {
            int entrySize = getEntrySize(currentEntry[0]);

            ParamEntry pvalue;
            if (entrySize == -1)
                pvalue = p.Groups[currentEntry[0]].Values[e.RowIndex];
            else
                pvalue = p.Groups[currentEntry[0]].Values[entrySize * currentEntry[1] + e.RowIndex];

            try
            {
                switch (pvalue.Type)
                {
                    case ParamType.s8:
                        pvalue.Value = Convert.ToSByte(tbl.Rows[e.RowIndex][(int)Columns.Value]);
                        break;
                    case ParamType.u8:
                        pvalue.Value = Convert.ToByte(tbl.Rows[e.RowIndex][(int)Columns.Value]);
                        break;
                    case ParamType.s16:
                        pvalue.Value = Convert.ToInt16(tbl.Rows[e.RowIndex][(int)Columns.Value]);
                        break;
                    case ParamType.u16:
                        pvalue.Value = Convert.ToUInt16(tbl.Rows[e.RowIndex][(int)Columns.Value]);
                        break;
                    case ParamType.s32:
                        pvalue.Value = Convert.ToInt32(tbl.Rows[e.RowIndex][(int)Columns.Value]);
                        break;
                    case ParamType.u32:
                        pvalue.Value = Convert.ToUInt32(tbl.Rows[e.RowIndex][(int)Columns.Value]);
                        break;
                    case ParamType.f32:
                        pvalue.Value = Convert.ToSingle(tbl.Rows[e.RowIndex][(int)Columns.Value]);
                        break;
                    case ParamType.str:
                        pvalue.Value = tbl.Rows[e.RowIndex][(int)Columns.Value];
                        break;
                }

                Edited = true;
            }
            catch (Exception) //Would be better to specifically catch FormatException and OverflowException here
            {
            }
            //If an invalid value was entered, revert it. If a valid value was entered, ensure that the table displays the actual new value (e.g. for floats).
            tbl.Rows[e.RowIndex][(int)Columns.Value] = pvalue.Value;
        }

        public override void Save()
        {
            p.Export(p.Filepath);
            Edited = false;
        }

        public override void SaveAs()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Parameter Files (.bin)|*.bin|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string filename = sfd.FileName;
                    p.Export(filename);
                    Edited = false;
                }
            }
        }

        public class IniLabels
        {
            public class Label
            {
                public enum LabelType
                {
                    Group,
                    Entry,
                    Value
                }
                public LabelType type;
                public string name;
                public int group = -1;
                public int entry = -1;
                public int value = -1;
                public int entries = -1;
                public string description = null;
            }

            public class LabelFile
            {
                public string filePath;
                public List<string> fileNames;
                public List<Label> labels;

                public LabelFile(string filePath)
                {
                    this.filePath = filePath;
                    fileNames = new List<string>();
                    labels = new List<Label>();

                    string[] ini = File.ReadAllLines(filePath);

                    Label currentLabel = null;
                    for (int i = 0; i < ini.Length; i++)
                    {
                        if (ini[i].Length <= 0)
                            continue;

                        //Comment
                        if (ini[i][0] == ';')
                        {
                            continue;
                        }

                        //Section
                        if (ini[i][0] == '[')
                        {
                            int closeBracket = ini[i].IndexOf(']');
                            if (closeBracket <= 1)
                                continue;
                            currentLabel = null;
                            string typeName = ini[i].Substring(1, closeBracket - 1);
                            Label.LabelType tempType;
                            //If we recognize the section type, start a new label
                            if (Enum.TryParse(typeName, out tempType))
                            {
                                currentLabel = new Label();
                                labels.Add(currentLabel);
                                currentLabel.type = tempType;
                            }
                            //Else skip lines until the next section
                            else
                            {
                                while (++i < ini.Length && ini[i][0] != '[')
                                {}
                                --i;
                            }
                            continue;
                        }

                        //Key
                        {
                            int splitIndex = ini[i].IndexOf('=');
                            if (splitIndex <= 0)
                                continue;
                            string key = ini[i].Substring(0, splitIndex).ToLowerInvariant(); //Not case-sensitive
                            string val = ini[i].Substring(splitIndex + 1);

                            if (currentLabel == null)
                            {
                                if (key == "name")
                                    fileNames.Add(val);
                            }
                            else
                            {
                                if (key == "name")
                                    currentLabel.name = val;
                                else if (key == "group")
                                    currentLabel.group = int.Parse(val) - 1;
                                else if (key == "entry")
                                    currentLabel.entry = int.Parse(val);
                                else if (key == "value")
                                    currentLabel.value = int.Parse(val);
                                else if (key == "entries")
                                    currentLabel.entries = int.Parse(val);
                                else if (key == "desc")
                                    currentLabel.description = val;
                            }
                        }
                    }
                    //If no name keys were defined outside of a valid section, use the file name as the only name
                    if (fileNames.Count == 0)
                        fileNames.Add(Path.GetFileNameWithoutExtension(filePath));
                }
            }

            public Dictionary<string, string> fileLookup;

            //We want to read every label file initially, so that each label file can be assigned to each file name that it lists, in the fileLookup dictionary.
            //Loading of labels for actual use can then be done by calls to GetLabels.
            public IniLabels()
            {
                fileLookup = new Dictionary<string, string>();
                string labelDirectory = Path.Combine(MainForm.executableDir, "param_labels\\");
                if (Directory.Exists(labelDirectory))
                {
                    string[] files = Directory.GetFiles(labelDirectory);
                    foreach (string filePath in files)
                    {
                        if (Path.GetExtension(filePath).ToLowerInvariant() == ".ini")
                        {
                            LabelFile lf = new LabelFile(filePath);
                            foreach (string name in lf.fileNames)
                            {
                                fileLookup[name] = filePath;
                            }
                        }
                    }
                }
            }
            public LabelFile GetLabels(string fileName)
            {
                //First check if exact name exists, then check with regex
                if (fileLookup.ContainsKey(fileName))
                    return new LabelFile(fileLookup[fileName]);
                else
                    foreach (string key in fileLookup.Keys)
                        if ((new Regex(key)).IsMatch(fileName))
                            return new LabelFile(fileLookup[key]);
                return null;
            }
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = treeView1.GetNodeAt(e.Location);
                if (treeView1.SelectedNode != null)
                {
                    contextMenuStrip1.Show(this, e.X, e.Y);
                }
                
            }
        }

        private TreeNode duplicateEntry(TreeNode node)
        {
            int groupNum = node.Parent.Index;
            int entryNum = node.Index;
            int entrySize = ((ParamGroup)p.Groups[groupNum]).EntrySize;
            for (int j = 0; j < entrySize; j++)
                p.Groups[groupNum].Add(new ParamEntry(p.Groups[groupNum].Values[entrySize * entryNum + j].Value, p.Groups[groupNum].Values[entrySize * entryNum + j].Type));

            ((ParamGroup)p.Groups[groupNum]).EntryCount++;
            ((ParamGroup)p.Groups[groupNum]).Chunk();

            TreeNode temp2 = new TreeNode();
            temp2.Text = getEntryName(groupNum, ((ParamGroup)p.Groups[groupNum]).EntryCount - 1);
            node.Parent.Nodes.Add(temp2);

            //fillTable(groupNum, ((ParamGroup)p.Groups[groupNum]).EntryCount - 1);
            Edited = true;

            return temp2;
        }

        private void duplicateSelectedEntry()
        {
            if (treeView1.SelectedNode == null || treeView1.SelectedNode.Level == 0)
                return;
            treeView1.SelectedNode = duplicateEntry(treeView1.SelectedNode);
        }

        private void duplicateEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            duplicateSelectedEntry();
        }

        private void deleteEntry(TreeNode node)
        {
            int groupNum = node.Parent.Index;
            int entryNum = node.Index;
            int entrySize = ((ParamGroup)p.Groups[groupNum]).EntrySize;
            for (int j = 0; j < entrySize; j++)
                p.Groups[groupNum].Values.RemoveAt(entrySize * entryNum);
            
            ((ParamGroup)p.Groups[groupNum]).EntryCount--;
            ((ParamGroup)p.Groups[groupNum]).Chunk();

            treeView1.Nodes.Remove(node);

            Edited = true;
        }

        private void deleteSelectedEntry()
        {
            if (treeView1.SelectedNode == null || treeView1.SelectedNode.Level == 0)
                return;
            TreeNode prev = treeView1.SelectedNode.PrevNode;
            TreeNode next = treeView1.SelectedNode.NextNode;
            deleteEntry(treeView1.SelectedNode);
            if (prev != null)
                treeView1.SelectedNode = prev;
            else
                treeView1.SelectedNode = next;
        }

        private void deleteEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteSelectedEntry();
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (treeView1.SelectedNode == null || treeView1.SelectedNode.Level == 0)
            {
                return;
            }

            if (e.Control)
            {
                if (e.KeyCode == Keys.Up && treeView1.SelectedNode.Parent.FirstNode != treeView1.SelectedNode)
                {
                    int groupNum = treeView1.SelectedNode.Parent.Index;
                    int entryNum = treeView1.SelectedNode.Index;
                    int entrySize = ((ParamGroup)p.Groups[groupNum]).EntrySize;
                    for (int j = 0; j < entrySize; j++)
                    {
                        p.Groups[groupNum].Values.Insert(entrySize * (entryNum - 1) + j, p.Groups[groupNum].Values[entrySize * entryNum + j]);
                        p.Groups[groupNum].Values.RemoveAt(entrySize * entryNum + j + 1);
                    }
                    fillTable(groupNum, entryNum - 1);
                    treeView1.SelectedNode = treeView1.SelectedNode.PrevNode;
                }
                else if (e.KeyCode == Keys.Down && treeView1.SelectedNode.Parent.LastNode != treeView1.SelectedNode)
                {
                    int groupNum = treeView1.SelectedNode.Parent.Index;
                    int entryNum = treeView1.SelectedNode.Index;
                    int entrySize = ((ParamGroup)p.Groups[groupNum]).EntrySize;
                    for (int j = 0; j < entrySize; j++)
                    {
                        p.Groups[groupNum].Values.Insert(entrySize * (entryNum + 2), p.Groups[groupNum].Values[entrySize * entryNum]);
                        p.Groups[groupNum].Values.RemoveAt(entrySize * entryNum);
                    }
                    fillTable(groupNum, entryNum + 1);
                    treeView1.SelectedNode = treeView1.SelectedNode.NextNode;
                }
                else if (e.KeyCode == Keys.D)
                {
                    duplicateSelectedEntry();
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                deleteSelectedEntry();
            }
        }
    }
}
