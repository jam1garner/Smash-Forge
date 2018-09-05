using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MeleeLib.DAT;
using MeleeLib.GCX;
using OpenTK;

namespace Smash_Forge.GUI.Melee
{
    public partial class DOBJImportSettings : Form
    {
        private MeleeDataObjectNode DOBJ;
        private string ModelSrc;

        public enum ExitStatus
        {
            Running = 0,
            Opened = 1,
            Cancelled = 2
        }

        public ExitStatus exitStatus = ExitStatus.Running;

        public DOBJImportSettings(MeleeDataObjectNode DOBJ)
        {
            InitializeComponent();
            this.DOBJ = DOBJ;

            foreach(GXAttribGroup group in DOBJ.GetRoot().Root.Attributes)
            {
                listBox1.Items.Add(group.Attributes.Count + "_Group");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog d = new OpenFileDialog())
            {
                d.Filter += "Source Model|*.smd|" +
                             "All Files (*.*)|*.*";

                if(d.ShowDialog() == DialogResult.OK)
                {
                    ModelSrc = d.FileName;
                    button1.Text = System.IO.Path.GetFileName(ModelSrc);
                }
            }
        }

        private void DOBJImportSettings_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            SMD smd = new SMD(ModelSrc);

            DatJOBJ[] Bones = DOBJ.GetRoot().Root.GetJOBJinOrder();
            VBN RenderBones = DOBJ.GetRoot().RenderBones;
            VBN ImportedBones = smd.Bones;

            DOBJ.ClearPolygons(null, null);

            GXAttribGroup AttrGroup = null;
            if (listBox1.SelectedIndex != -1)
                AttrGroup = DOBJ.GetRoot().Root.Attributes[listBox1.SelectedIndex];
            else
            {
                MessageBox.Show("Please select an attribute group");
                return;
            }
            int Flags = 0x8001;
            if (comboBoxBoneType.SelectedIndex == 2) // Rigged - needs to create bonelist
            {
                Flags = 0xA001;

                MessageBox.Show("Warning: Rigged not supported yet");
            }

            DatPolygon p = new DatPolygon();
            p.AttributeGroup = AttrGroup;
            p.Flags = Flags;
            p.ParentDOBJ = DOBJ.DOBJ;

            DOBJ.VertsToImport = new List<GXVertex[]>();
            List<GXVertex> vert = new List<GXVertex>();
            foreach (SMDTriangle t in smd.Triangles)
            {
                if (t.v1.Bones.Length == 0 || t.v2.Bones.Length == 0 || t.v3.Bones.Length == 0)
                    continue;

                List<DatBoneWeight> bwl1 = CreateWeightList(t.v1.Bones, t.v1.Weights, Bones, ImportedBones);
                List<DatBoneWeight> bwl2 = CreateWeightList(t.v2.Bones, t.v2.Weights, Bones, ImportedBones);
                List<DatBoneWeight> bwl3 = CreateWeightList(t.v3.Bones, t.v3.Weights, Bones, ImportedBones);
                int bid1 = GetWeightListIndex(p.BoneWeightList, bwl1);
                int bid2 = GetWeightListIndex(p.BoneWeightList, bwl2);
                int bid3 = GetWeightListIndex(p.BoneWeightList, bwl3);

                int wc = p.BoneWeightList.Count;
                if (bid1 == -1) wc++;
                if (bid2 == -1) wc++;
                if (bid3 == -1) wc++;
                if(wc >= 10) // need new polygon
                {
                    DOBJ.VertsToImport.Add(vert.ToArray());
                    vert.Clear();
                    p = new DatPolygon();
                    p.AttributeGroup = AttrGroup;
                    p.Flags = Flags;
                    p.ParentDOBJ = DOBJ.DOBJ;
                }

                bid1 = GetWeightListIndex(p.BoneWeightList, bwl1);
                bid2 = GetWeightListIndex(p.BoneWeightList, bwl2);
                bid3 = GetWeightListIndex(p.BoneWeightList, bwl3);
                if (bid1 == -1) p.BoneWeightList.Add(bwl1);
                if (bid2 == -1) p.BoneWeightList.Add(bwl2);
                if (bid3 == -1) p.BoneWeightList.Add(bwl3);
                bid1 = GetWeightListIndex(p.BoneWeightList, bwl1);
                bid2 = GetWeightListIndex(p.BoneWeightList, bwl2);
                bid3 = GetWeightListIndex(p.BoneWeightList, bwl3);

                GXVertex v = SMDVertexToGXVertex(t.v1);
                v.PMXID = GetWeightListIndex(p.BoneWeightList, bwl1);
                v.Pos = RigVertex(t.v1.P, RenderBones, p.BoneWeightList[v.PMXID/3], Bones);
               
                GXVertex v2 = SMDVertexToGXVertex(t.v2);
                v2.PMXID = GetWeightListIndex(p.BoneWeightList, CreateWeightList(t.v2.Bones, t.v2.Weights, Bones, ImportedBones));
                v2.Pos = RigVertex(t.v2.P, RenderBones, p.BoneWeightList[v2.PMXID / 3], Bones);
                
                GXVertex v3 = SMDVertexToGXVertex(t.v3);
                v3.PMXID = GetWeightListIndex(p.BoneWeightList, CreateWeightList(t.v3.Bones, t.v3.Weights, Bones, ImportedBones));
                v3.Pos = RigVertex(t.v3.P, RenderBones, p.BoneWeightList[v3.PMXID / 3], Bones);
               
                vert.Add(v);
                vert.Add(v2);
                vert.Add(v3);
            }

            DOBJ.VertsToImport.Add(vert.ToArray());
            Console.WriteLine(DOBJ.DOBJ.Polygons.Count);
            
            exitStatus = ExitStatus.Opened;
            Close();
        }

        private GXVector3 RigVertex(Vector3 P, VBN RenderBones, List<DatBoneWeight> Weight, DatJOBJ[] jobjs)
        {
            if(Weight.Count == 1)
            {
                int i;
                for (i = 0; i < jobjs.Length; i++)
                    if (jobjs[i] == Weight[0].jobj)
                        break;
                P = Vector3.TransformPosition(P, RenderBones.bones[i].transform.Inverted());
            }
            
            return new GXVector3(P.X, P.Y, P.Z);
        }

        private int GetWeightListIndex(List<List<DatBoneWeight>> WeightList, List<DatBoneWeight> Weights)
        {
            for(int i = 0; i < WeightList.Count; i++)
            {
                if (WeightList[i].Count != Weights.Count) continue;

                bool success = true;
                for(int j = 0; j < Weights.Count; j++)
                {
                    if (WeightList[i][j].jobj != Weights[j].jobj
                        ||WeightList[i][j].Weight != Weights[j].Weight)
                    {
                        success = false;
                        break;
                    }
                }
                if (success)
                    return i * 3;
            }
            
            return -1;
        }

        private List<DatBoneWeight> CreateWeightList(int[] bid, float[] weight, DatJOBJ[] Bones, VBN ImportedBones)
        {
            List<DatBoneWeight> Weight = new List<DatBoneWeight>();
            if (bid.Length != weight.Length)
                return Weight;
            for(int i = 0; i < weight.Length; i++)
            {
                DatBoneWeight w = new DatBoneWeight();
                w.jobj = GetJOBJFromIndex(Bones, ImportedBones, bid[i]);
                if (w.jobj == null) throw new Exception("Can't be null");
                w.Weight = weight[i];
                Weight.Add(w);
            }
            return Weight;
        }

        private DatJOBJ GetJOBJFromIndex(DatJOBJ[] jobs, VBN ImportedBones, int ImportedIndex)
        {
            Bone b = ImportedBones.bones[ImportedIndex];
            
            for(int i = 0; i < jobs.Length; i++)
            {
                if(b.Text.Equals("Bone_" + i))
                {
                    return jobs[i];
                }
            }
            if(jobs.Length > 0)
                return jobs[0];

            return null;
        }

        private GXVertex SMDVertexToGXVertex(SMDVertex v)
        {
            GXVertex o = new GXVertex()
            {
                Pos = new GXVector3(v.P.X, v.P.Y, v.P.Z),
                Nrm = new GXVector3(v.N.X, v.N.Y, v.N.Z),
                TX0 = new GXVector2(v.UV.X, v.UV.Y),
            };
            return o;
        }

        private void DOBJImportSettings_Load(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            if (listBox1.SelectedIndex == -1) return; 
            foreach(GXAttr a in DOBJ.GetRoot().Root.Attributes[listBox1.SelectedIndex].Attributes)
            {
                listBox2.Items.Add(a.Name.ToString() + " " + a.AttributeType);
            }
        }
    }
}
