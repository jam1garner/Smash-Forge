using MeleeLib.DAT;
using MeleeLib.GCX;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SmashForge.Filetypes.Melee;

namespace SmashForge.Gui.Melee
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

        private class BoneNode : TreeNode
        {
            public DatJOBJ JOBJ;
            public BoneNode(DatJOBJ j)
            {
                JOBJ = j;
            }
        }

        public ExitStatus exitStatus = ExitStatus.Running;

        public DOBJImportSettings(MeleeDataObjectNode DOBJ)
        {
            InitializeComponent();
            this.DOBJ = DOBJ;
            
            foreach (GXAttribGroup group in DOBJ.GetRoot().Root.Attributes)
            {
                listBox1.Items.Add(group.Attributes.Count + "_Group");
            }

            int i = 0;
            foreach(DatJOBJ b in DOBJ.GetRoot().Root.GetJOBJinOrder())
            {
                CBBone.Items.Add(new BoneNode(b) { Text = "Bone_" + i++ });
            }
            CBBone.SelectedIndex = 0;
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
            Smd smd = new Smd(ModelSrc);

            DatJOBJ[] Bones = DOBJ.GetRoot().Root.GetJOBJinOrder();
            VBN RenderBones = DOBJ.GetRoot().RenderBones;
            RenderBones.reset();
            RenderBones.update();
            VBN ImportedBones = smd.bones;

            GXAttribGroup AttrGroup = null;
            if (listBox1.SelectedIndex != -1)
                AttrGroup = DOBJ.GetRoot().Root.Attributes[listBox1.SelectedIndex];
            else
            {
                MessageBox.Show("Please select an attribute group");
                return;
            }

            DOBJ.ClearPolygons(null, null);

            int Flags = 0x8000;
            if (comboBoxBoneType.SelectedIndex == 0)
            {
                MessageBox.Show("Warning: no binds");
            }
            if (comboBoxBoneType.SelectedIndex == 1)
            {
                Flags = 0xA001;
            }
            if (comboBoxBoneType.SelectedIndex == 2) // Rigged - needs to create bonelist
            {
                Flags = 0xA001;
            }

            DatPolygon p = new DatPolygon();
            p.AttributeGroup = AttrGroup;
            p.Flags = Flags;
            p.ParentDOBJ = DOBJ.DOBJ;

            DOBJ.VertsToImport = new List<GXVertex[]>();
            List<GXVertex> vert = new List<GXVertex>();
            DatJOBJ parent = DOBJ.DOBJ.Parent;

            if (comboBoxBoneType.SelectedIndex == 1)
                ImportedBones = RenderBones;
            foreach (SmdTriangle t in smd.triangles)
            {
                if(comboBoxBoneType.SelectedIndex == 1)
                {
                    // single bind
                    t.v1.bones = new int[] { CBBone.SelectedIndex };
                    t.v1.weights = new float[] { 1 };
                    t.v2.bones = new int[] { CBBone.SelectedIndex };
                    t.v2.weights = new float[] { 1 };
                    t.v3.bones = new int[] { CBBone.SelectedIndex };
                    t.v3.weights = new float[] { 1 };
                }
                List<DatBoneWeight> bwl1 = CreateWeightList(t.v1.bones, t.v1.weights, Bones, ImportedBones);
                List<DatBoneWeight> bwl2 = CreateWeightList(t.v2.bones, t.v2.weights, Bones, ImportedBones);
                List<DatBoneWeight> bwl3 = CreateWeightList(t.v3.bones, t.v3.weights, Bones, ImportedBones);
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
                RigVertex(ref v, RenderBones, p.BoneWeightList[v.PMXID/3], Bones, parent);
               
                GXVertex v2 = SMDVertexToGXVertex(t.v2);
                v2.PMXID = GetWeightListIndex(p.BoneWeightList, CreateWeightList(t.v2.bones, t.v2.weights, Bones, ImportedBones));
                RigVertex(ref v2, RenderBones, p.BoneWeightList[v2.PMXID / 3], Bones, parent);
                
                GXVertex v3 = SMDVertexToGXVertex(t.v3);
                v3.PMXID = GetWeightListIndex(p.BoneWeightList, CreateWeightList(t.v3.bones, t.v3.weights, Bones, ImportedBones));
                RigVertex(ref v3, RenderBones, p.BoneWeightList[v3.PMXID / 3], Bones, parent);
               
                vert.Add(v);
                vert.Add(v2);
                vert.Add(v3);
            }

            DOBJ.VertsToImport.Add(vert.ToArray());
            
            exitStatus = ExitStatus.Opened;
            Close();
        }

        private void RigVertex(ref GXVertex P, VBN RenderBones, List<DatBoneWeight> Weight, DatJOBJ[] jobjs, DatJOBJ Parent = null)
        {
            if(Weight.Count == 1)
            {
                int i;
                for (i = 0; i < jobjs.Length; i++)
                    if (jobjs[i] == Weight[0].jobj)
                        break;
                Vector3 NewP = Vector3.TransformPosition(new Vector3(P.Pos.X, P.Pos.Y, P.Pos.Z), RenderBones.bones[i].transform.Inverted());
                P.Pos.X = NewP.X; P.Pos.Y = NewP.Y; P.Pos.Z = NewP.Z;
                NewP = Vector3.TransformNormal(new Vector3(P.Nrm.X, P.Nrm.Y, P.Nrm.Z), RenderBones.bones[i].transform.Inverted());
                P.Nrm.X = NewP.X; P.Nrm.Y = NewP.Y; P.Nrm.Z = NewP.Z;
            }
            if (Weight.Count == 0 && Parent != null)
            {
                int i;
                for (i = 0; i < jobjs.Length; i++)
                    if (jobjs[i] == Parent)
                        break;
                Vector3 NewP = Vector3.TransformPosition(new Vector3(P.Pos.X, P.Pos.Y, P.Pos.Z), RenderBones.bones[i].transform.Inverted());
                P.Pos.X = NewP.X; P.Pos.Y = NewP.Y; P.Pos.Z = NewP.Z;
                NewP = Vector3.TransformNormal(new Vector3(P.Nrm.X, P.Nrm.Y, P.Nrm.Z), RenderBones.bones[i].transform.Inverted());
                P.Nrm.X = NewP.X; P.Nrm.Y = NewP.Y; P.Nrm.Z = NewP.Z;
            }
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

        private GXVertex SMDVertexToGXVertex(SmdVertex v)
        {
            GXVertex o = new GXVertex()
            {
                Pos = new GXVector3(v.p.X, v.p.Y, v.p.Z),
                Nrm = new GXVector3(v.n.X, v.n.Y, v.n.Z),
                TX0 = new GXVector2(v.uv.X, v.uv.Y),
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

        private void comboBoxBoneType_SelectedIndexChanged(object sender, EventArgs e)
        {
            CBBone.Enabled = comboBoxBoneType.SelectedIndex == 1;
        }
    }
}
