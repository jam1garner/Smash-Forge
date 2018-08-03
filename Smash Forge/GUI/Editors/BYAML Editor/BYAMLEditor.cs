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
using OpenTK;
using Smash_Forge.Rendering;

namespace Smash_Forge
{
    public partial class BYAMLEditor : DockContent
    {
        public BYAML TargetBYAML;
        public BYAML.PathPoint currentEntry;

        public BYAMLEditor()
        {
            InitializeComponent();

        
        }

        public SortedList<double, BYAML.PathPoint> GetBYAMPointSelection(Ray ray)
        {
            SortedList<double, BYAML.PathPoint> selected = new SortedList<double, BYAML.PathPoint>(new DuplicateKeyComparer<double>());
            if (TargetBYAML != null)
            {
                Vector3 closest = Vector3.Zero;
                foreach (BYAML.GenericPathGroup path in TargetBYAML.EnemyPaths.Nodes)
                {
                    foreach (BYAML.PathPoint pt in path.Nodes)
                    {
                        if (ray.CheckSphereHit(pt.translate, 44, out closest))
                            selected.Add(ray.Distance(closest), pt);
                    }
                }

            }
            return selected;
        }

        public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
        {
            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1;
                else
                    return result;
            }
        }

        public void Open(BYAML.PathPoint obj)
        {
            currentEntry = obj;

            textBox1.Text = currentEntry.Text;

            if (currentEntry != null)
            {
                numericUpDown2.Value = (decimal)currentEntry.translate.X;
                numericUpDown3.Value = (decimal)currentEntry.translate.Y;
                numericUpDown1.Value = (decimal)currentEntry.translate.Z;

                propertyGrid1.SelectedObject = currentEntry;
            }
        }

        Stack<float> undoList1 = new Stack<float>();
        Stack<float> undoList2 = new Stack<float>();
        Stack<float> undoList3 = new Stack<float>();

        private void numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (currentEntry != null)
            {
                undoList2.Push((float)numericUpDown2.Value);
                undoList3.Push((float)numericUpDown3.Value);
                undoList1.Push((float)numericUpDown1.Value);

                if (sender == numericUpDown2)
                    currentEntry.translate.X = (float)numericUpDown2.Value;
                if (sender == numericUpDown3)
                    currentEntry.translate.Y = (float)numericUpDown3.Value;
                if (sender == numericUpDown1)
                    currentEntry.translate.Z = (float)numericUpDown1.Value;
            }
        }

        private void numericUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z && (e.Control))
            {
                if (undoList2.Count > 0)
                    numericUpDown2.Value = (decimal)undoList2.Pop();
                if (undoList1.Count > 0)
                    numericUpDown1.Value = (decimal)undoList1.Pop();
                if (undoList3.Count > 0)
                    numericUpDown3.Value = (decimal)undoList3.Pop();
            }
        }
    }
}
