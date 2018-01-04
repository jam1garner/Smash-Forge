using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Smash_Forge
{
    public class Bone : TreeNode
    {
        public VBN vbnParent;
        public UInt32 boneType;

        public enum BoneType
        {
            Normal = 0,
            UNK,
            Helper,
            Swing
        }

        public UInt32 boneId;
        public float[] position = new float[] { 0, 0, 0 };
        public float[] rotation = new float[] { 0, 0, 0 };
        public float[] scale = new float[] { 1,1,1};
        
        public Vector3 pos = Vector3.Zero, sca = new Vector3(1f, 1f, 1f);
        public Quaternion rot = Quaternion.FromMatrix(Matrix3.Zero);
        public Matrix4 transform, invert;

        public bool isSwingBone = false;

        public int parentIndex
        {
            set
            {
                if (Parent != null) Parent.Nodes.Remove(this);
                if(value > -1 && value < vbnParent.bones.Count)
                {
                    vbnParent.bones[value].Nodes.Add(this);
                }
            }

            get
            {
                if (Parent == null)
                    return -1;
                return vbnParent.bones.IndexOf((Bone)Parent);
            }
        }

        public Bone(VBN v)
        {
            vbnParent = v;
            ImageKey = "bone";
            SelectedImageKey = "bone";
        }

        public List<Bone> GetChildren()
        {
            List<Bone> l = new List<Bone>();
            foreach (Bone b in vbnParent.bones)
                if(b.Parent == this)
                    l.Add(b);
            return l;
        }

        public override string ToString()
        {
            return Text;
        }

        public static float rot90 = (float)(90*(Math.PI / 180));

        public int CheckControl(Ray r)
        {
            /*
            Vector3 pos_c = Vector3.Transform(Vector3.Zero, transform);
            if (RenderTools.intersectCircle(pos_c, 2f, 30, r.p1, r.p2))
                return 1;
            
            */
            return -1;
        }

        public void Draw()
        {
            Vector3 pos_c = Vector3.Transform(Vector3.Zero, transform);
            // first calcuate the point and draw a point
            if (IsSelected)
            {
                /*GL.Color3(Color.Red);
                RenderTools.drawCircleOutline(pos_c, 2f, 30, Matrix4.CreateRotationX(0));
                GL.Color3(Color.Green);
                RenderTools.drawCircleOutline(pos_c, 2f, 30, Matrix4.CreateRotationX(rot90));
                GL.Color3(Color.Gold);
                RenderTools.drawCircleOutline(pos_c, 2f, 30, Matrix4.CreateRotationY(rot90));*/
                GL.Color3(Color.Red);
            }
            else
                GL.Color3(Color.GreenYellow);

            RenderTools.drawCube(pos_c, .1f);

            // now draw line between parent 
            GL.Color3(Color.LightBlue);
            GL.LineWidth(2f);

            GL.Begin(PrimitiveType.Lines);
            if (Parent != null && Parent is Bone)
            {
                Vector3 pos_p = Vector3.Transform(Vector3.Zero, ((Bone)Parent).transform);
                GL.Vertex3(pos_c);
                GL.Color3(Color.Blue);
                GL.Vertex3(pos_p);
            }
            GL.End();
        }
    }

    public class HelperBone
    {
        public void Read(FileData f)
        {
            f.Endian = Endianness.Little;
            f.seek(4);
            int count = f.readInt();
            f.skip(12);
            int dataCount = f.readInt();
            int boneCount = f.readInt();
            int hashCount = f.readInt();
            int hashOffset = f.readInt() + 0x28;
            f.skip(4);

            int pos = f.pos();
            f.seek(hashOffset);

            csvHashes csv = new csvHashes(Path.Combine(MainForm.executableDir, "hashTable.csv"));
            List<string> bonename = new List<string>();

            for (int i = 0; i < hashCount; i++)
            {
                uint hash = (uint)f.readInt();
                Console.WriteLine(csv.ids[hash]);
                bonename.Add(csv.ids[hash]);
            }

            f.seek(pos);
            Console.WriteLine("Count " + count);

            for (int i = 0; i < dataCount; i++)
            {
                Console.WriteLine("Bone " + i + " start at " + f.pos().ToString("x"));
                // 3 sections
                int secLength = f.readInt();
                int someCount = f.readInt(); // usually 2?

                for(int sec = 0; sec < 5; sec++)
                {
                    int size = f.readInt();
                    int id = f.readInt();
                    Console.WriteLine(id + ":\t" + size.ToString("x"));
                    for (int j = 0; j < ((size - 1) / 4) - 1; j++)
                    {

                        if(id == 4)
                        {
                            int b1 = (short)f.readShort();
                            int b2 = (short)f.readShort();
                            Console.Write("\t" + (b1==-1?b1 + "" : bonename[b1]) + " " + b2 + "\t");
                        }
                        else
                        if (id == 5)
                        {
                            int b1 = (short)f.readShort();
                            int b2 = (short)f.readShort();
                            Console.Write("\t" + (b1 == -1 ? b1 + "" : bonename[b1]) + " " + (b2 == -1 ? b2 + "" : bonename[b2]) + "\t");
                        }
                        else
                            Console.Write("\t" + (f.readShort() / (id==7?(float)0xffff:1)) + " " + (f.readShort() / (id == 7 ? (float)0xffff : 1)) + "\t");
                    }
                    Console.WriteLine();
                }

                f.skip(8);
            }

            Console.WriteLine("0x" + f.pos().ToString("X"));
            f.skip(8);
            int hashSize = f.readInt();
            int unk = f.readInt();


            
        }
    }

    public class VBN : FileBase
    {
        public VBN()
        {
            Text = "model.vbn";
            ImageKey = "skeleton";
            SelectedImageKey = "skeleton";

            ContextMenu = new ContextMenu();

            MenuItem OpenEdit = new MenuItem("Open Editor");
            OpenEdit.Click += OpenEditor;
            ContextMenu.MenuItems.Add(OpenEdit);

            MenuItem save = new MenuItem("Save As");
            ContextMenu.MenuItems.Add(save);
            save.Click += Save;

            ResetNodes();
        }

        public VBN(string filename) : this()
        {
            FilePath = filename;
            Read(filename);
        }

        public override Endianness Endian { get; set; }

        public string FilePath = "";
        public Int16 unk_1 = 2, unk_2 = 1;
        public UInt32 totalBoneCount;
        public UInt32[] boneCountPerType = new UInt32[4];
        public List<Bone> bones = new List<Bone>();
        
        public SB SwingBones {
            get 
            {
                if (_swingBones == null)
                    _swingBones = new SB();
                return _swingBones;
            }
            set
            {
                _swingBones = value;
                ResetNodes();
            }
        }
        private SB _swingBones;
        public JTB JointTable
        {
            get
            {
                if (_jointTable == null)
                    _jointTable = new JTB();
                return _jointTable;
            }
            set
            {
                _jointTable = value;
                ResetNodes();
            }
        }
        private JTB _jointTable;

        private TreeNode RootNode = new TreeNode() { Text = "Bones" };

        #region Events

        BoneTreePanel Editor;

        private void OpenEditor(object sender, EventArgs args)
        {
            RootNode.Nodes.Clear();
            if (Editor == null || Editor.IsDisposed)
            {
                Editor = new BoneTreePanel(this);
                Editor.FilePath = FilePath;
                Editor.Text = Parent.Text + "\\" + Text;
                MainForm.Instance.AddDockedControl(Editor);
            }
            else
            {
                Editor.BringToFront();
            }
        }

        public void ResetNodes()
        {
            Nodes.Clear();
            
            Nodes.Add(RootNode);
            Nodes.Add(SwingBones);
        }

        public void Save(object sender, EventArgs args)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Visual Bones Namco (.vbn)|*.vbn|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(sfd.FileName, Rebuild());
                }
            }
        }

        #endregion

        public Bone getBone(String name)
        {
            foreach (Bone bo in bones)
                if (bo.Text.Equals(name))
                    return bo;
            return null;
        }
        public Bone getBone(uint hash)
        {
            foreach (Bone bo in bones)
                if (bo.boneId == hash)
                    return bo;
            return null;
        }

        public List<Bone> getBoneTreeOrderol()
        {
            List<Bone> bone = new List<Bone>();
            Queue<Bone> q = new Queue<Bone>();

            q.Enqueue(bones[0]);

            while (q.Count > 0)
            {
                Bone b = q.Dequeue();
                foreach (Bone bo in b.GetChildren())
                    q.Enqueue(bo);
                bone.Add(b);
            }
            return bone;
        }

        public List<Bone> getBoneTreeOrder()
        {
            if (bones.Count == 0)
                return null;
            List<Bone> bone = new List<Bone>();
            Queue<Bone> q = new Queue<Bone>();

            queueBones(bones[0], q);

            while (q.Count > 0)
            {
                bone.Add(q.Dequeue());
            }
            return bone;
        }

        public void queueBones(Bone b, Queue<Bone> q)
        {
            q.Enqueue(b);
            foreach (Bone c in b.GetChildren())
                queueBones(c, q);
        }

        public static Quaternion FromEulerAngles(float z, float y, float x)
        {
            {
                Quaternion xRotation = Quaternion.FromAxisAngle(Vector3.UnitX, x);
                Quaternion yRotation = Quaternion.FromAxisAngle(Vector3.UnitY, y);
                Quaternion zRotation = Quaternion.FromAxisAngle(Vector3.UnitZ, z);

                Quaternion q = (zRotation * yRotation * xRotation);

                if (q.W < 0)
                    q *= -1;

                //return xRotation * yRotation * zRotation;
                return q;
            }
        }

        private bool Updated = false;
        public void update(bool reset = false)
        {
            Updated = true;
            List<Bone> nodesToProcess = new List<Bone>();
            // Add all root nodes from the VBN
            foreach (Bone b in bones)
                if (b.Parent == null)
                    nodesToProcess.Add(b);

            // some special processing for the root bones before we start
            foreach (Bone b in nodesToProcess)
            {
                b.transform = Matrix4.CreateScale(b.sca) * Matrix4.CreateFromQuaternion(b.rot) * Matrix4.CreateTranslation(b.pos);
                // scale down the model in its entirety only when mid-animation (i.e. reset == false)
                if (!reset && Runtime.model_scale != 1) b.transform *= Matrix4.CreateScale(Runtime.model_scale);
            }

            // Process as a tree from the root node's children and beyond. These
            // all use the same processing, unlike the root nodes.
            int numRootNodes = nodesToProcess.Count;
            for (int i = 0; i < numRootNodes; i++)
            {
                nodesToProcess.AddRange(nodesToProcess[0].GetChildren());
                nodesToProcess.RemoveAt(0);
            }
            while (nodesToProcess.Count > 0)
            {
                // DFS
                Bone currentBone = nodesToProcess[0];
                nodesToProcess.RemoveAt(0);
                nodesToProcess.AddRange(currentBone.GetChildren());

                // Process this node
                currentBone.transform = Matrix4.CreateScale(currentBone.sca) * Matrix4.CreateFromQuaternion(currentBone.rot) * Matrix4.CreateTranslation(currentBone.pos);
                if (currentBone.Parent != null)
                {
                    currentBone.transform = currentBone.transform * ((Bone)currentBone.Parent).transform;
                }
            }
        }

        //public void updateOld(bool reset = false)
        //{
        //    for (int i = 0; i < bones.Count; i++)
        //    {
        //        bones[i].transform = Matrix4.CreateScale(bones[i].sca) * Matrix4.CreateFromQuaternion(bones[i].rot) * Matrix4.CreateTranslation(bones[i].pos);

        //        // Scale down the model only when in animations (e.g. reset == false)
        //        if (i == 0 && !reset && Runtime.model_scale != 1) bones[i].transform *= Matrix4.CreateScale(Runtime.model_scale);

        //        if (bones[i].Parent !=null)
        //        {
        //            bones[i].transform = bones[i].transform * bones[(int)bones[i].parentIndex].transform;
        //        }
        //    }
        //}

        public void reset(bool Main = true)
        {
            //if(Main)
            {
                /*RootNode.Nodes.Clear();
                if (bones.Count > 0 && bones[0].Parent == null)
                    RootNode.Nodes.Add(bones[0]);*/
            }

            ExpandAll();
            for (int i = 0; i < bones.Count; i++)
            {
                bones[i].pos = new Vector3(bones[i].position[0], bones[i].position[1], bones[i].position[2]);
                bones[i].rot = (FromEulerAngles(bones[i].rotation[2], bones[i].rotation[1], bones[i].rotation[0]));
                bones[i].sca = new Vector3(bones[i].scale[0], bones[i].scale[1], bones[i].scale[2]);
            }
            update(true);
            for (int i = 0; i < bones.Count; i++)
            {
                try{
                    bones[i].invert = Matrix4.Invert(bones[i].transform);
                } catch (InvalidOperationException){
                    bones[i].invert = Matrix4.Zero;
                }
            }
            if(Runtime.model_scale != 1f) update();
        }

        public override void Read(string filename)
        {
            FileData file = new FileData(filename);
            if (file != null)
            {
                file.Endian = Endianness.Little;
                Endian = Endianness.Little;
                string magic = file.readString(0, 4);
                if (magic == "VBN ")
                {
                    file.Endian = Endianness.Big;
                    Endian = Endianness.Big;
                }

                file.seek(4);

                unk_1 = (short)file.readShort();
                unk_2 = (short)file.readShort();
                totalBoneCount = (UInt32)file.readInt();
                boneCountPerType[0] = (UInt32)file.readInt();
                boneCountPerType[1] = (UInt32)file.readInt();
                boneCountPerType[2] = (UInt32)file.readInt();
                boneCountPerType[3] = (UInt32)file.readInt();

                int[] pi = new int[totalBoneCount];
                for (int i = 0; i < totalBoneCount; i++)
                {
                    Bone temp = new Bone(this);
                    temp.Text = file.readString(file.pos(), -1);
                    file.skip(64);
                    temp.boneType = (UInt32)file.readInt();
                    pi[i] = file.readInt();
                    temp.boneId = (UInt32)file.readInt();
                    temp.position = new float[3];
                    temp.rotation = new float[3];
                    temp.scale = new float[3];
                    //temp.isSwingBone = temp.Text.Contains("__swing");
                    bones.Add(temp);
                }

                for (int i = 0; i < bones.Count; i++)
                {
                    bones[i].position[0] = file.readFloat();
                    bones[i].position[1] = file.readFloat();
                    bones[i].position[2] = file.readFloat();
                    bones[i].rotation[0] = file.readFloat();
                    bones[i].rotation[1] = file.readFloat();
                    bones[i].rotation[2] = file.readFloat();
                    bones[i].scale[0] = file.readFloat();
                    bones[i].scale[1] = file.readFloat();
                    bones[i].scale[2] = file.readFloat();
                    Bone temp = bones[i];
                    temp.parentIndex = pi[i];
                    //Debug.Write(temp.parentIndex);
                    //if (temp.parentIndex != 0x0FFFFFFF && temp.parentIndex > -1)
                    //    bones[temp.parentIndex].children.Add(i);
                    bones[i] = temp;
                }
                reset();
            }
        }

        public override byte[] Rebuild()
        {
            FileOutput file = new FileOutput();
            if (file != null)
            {
                if (Endian == Endianness.Little) {
                    file.Endian = Endianness.Little;
                    file.writeString(" NBV");
                    file.writeShort(0x02);
                    file.writeShort(0x01);
                }
                else if (Endian == Endianness.Big) {
                    file.Endian = Endianness.Big;
                    file.writeString("VBN ");
                    file.writeShort(0x01);
                    file.writeShort(0x02);
                }

                
                file.writeInt(bones.Count);
                if (boneCountPerType[0] == 0)
                    boneCountPerType[0] = (uint)bones.Count;
                for (int i = 0; i < 4; i++)
                    file.writeInt((int)boneCountPerType[i]);

                for (int i = 0; i < bones.Count; i++)
                {
                    file.writeString(bones[i].Text);
                    for (int j = 0; j < 64 - bones[i].Text.Length; j++)
                        file.writeByte(0);
                    file.writeInt((int)bones[i].boneType);
                    if(bones[i].parentIndex == -1)
                        file.writeInt(0x0FFFFFFF);
                    else
                        file.writeInt(bones[i].parentIndex);
                    file.writeInt((int)bones[i].boneId);
                }

                for (int i = 0; i < bones.Count; i++)
                {
                    file.writeFloat(bones[i].position[0]);
                    file.writeFloat(bones[i].position[1]);
                    file.writeFloat(bones[i].position[2]);
                    file.writeFloat(bones[i].rotation[0]);
                    file.writeFloat(bones[i].rotation[1]);
                    file.writeFloat(bones[i].rotation[2]);
                    file.writeFloat(bones[i].scale[0]);
                    file.writeFloat(bones[i].scale[1]);
                    file.writeFloat(bones[i].scale[2]);
                }
            }
            return file.getBytes();
        }

        /*public void readJointTable(string fname)
        {
            FileData d = new FileData(fname);
            d.Endian = Endianness.Big;

            int tableSize = 2;

            int table1 = d.readShort();

            if (table1 * 2 + 2 >= d.size())
                tableSize = 1;

            int table2 = -1;
            if (tableSize != 1)
                table2 = d.readShort();

            //if (table2 == 0)
            //    d.seek(d.pos() - 2);

            List<int> t1 = new List<int>();

            for (int i = 0; i < table1; i++)
                t1.Add(d.readShort());

            jointTable.Clear();
            jointTable.Add(t1);

            if (tableSize != 1)
            {
                List<int> t2 = new List<int>();
                for (int i = 0; i < table2; i++)
                    t2.Add(d.readShort());
                jointTable.Add(t2);
            }
        }*/

        public Bone bone(string name)
        {
            foreach (Bone b in bones)
            {
                if (b.Text.Equals(name))
                {
                    return b;
                }
            }
            throw new Exception("No bone of char[] name");
        }

        public int boneIndex(string name)
        {
            for (int i = 0; i < bones.Count; i++)
            {
                if (bones[i].Text.Equals(name))
                {
                    return i;
                }
            }

            return -1;
            //throw new Exception("No bone of char[] name");
        }

        public int getJTBIndex(string name)
        {
            int index = -1;
            int vbnIndex = boneIndex(name);
            if(JointTable != null)
            {
                for(int i = 0; i < JointTable.Tables.Count; i++)
                {
                    for(int j = 0; j < JointTable.Tables[i].Count; j++)
                    {
                        if(JointTable.Tables[i][j] == vbnIndex)
                        {
                            // Note that some bones appear twice in the joint tables
                            // and this function will only find the first occurrence.
                            index = j + (i * 1000);
                            return index;
                        }
                    }
                }
            }
            return index;
        }

        public void deleteBone(int index)
        {
            boneCountPerType[bones[index].boneType]--;
            totalBoneCount--;
            List<Bone> children = bones[index].GetChildren();
            bones.RemoveAt(index);
            foreach (Bone b in children)
                deleteBone(bones.IndexOf(b));
        }

        public void deleteBone(string name)
        {
            deleteBone(boneIndex(name));
        }

        public float[] f = null;
        public Matrix4[] bonemat = { };
        public Matrix4[] bonematIT = { };

        public Matrix4[] getShaderMatrix()
        {
            if (Updated)
            {
                Updated = false;
                if (bonemat.Length != bones.Count)
                    bonemat = new Matrix4[bones.Count];

                for (int i = 0; i < bones.Count; i++)
                {
                    bonemat[i] = bones[i].invert * bones[i].transform;
                    //bonematIT[i] = bones[i].invert * bones[i].transform;
                    //bonematIT[i].Invert();
                    //bonematIT[i].Transpose();
                }
            }

            return bonemat;
        }

        private static string charsToString(char[] c)
        {
            string boneNameRigging = "";
            foreach (char b in c)
                if (b != (char)0)
                    boneNameRigging += b;
            return boneNameRigging;
        }

        public static string BoneNameFromHash(uint boneHash)
        {
            /*foreach (ModelContainer m in Runtime.ModelContainers)
                if (m.VBN != null)
                    foreach (Bone b in m.VBN.bones)
                        if (b.boneId == boneHash)
                            return b.Text;*/

            /*csvHashes csv = new csvHashes(Path.Combine(MainForm.executableDir, "hashTable.csv"));
            for (int i = 0; i < csv.ids.Count; i++)
                if (csv.ids[i] == boneHash)
                    return csv.names[i]+" (From hashTable.csv)";*/

            return $"[Bonehash {boneHash.ToString("X")}]";
        }

        public Bone GetBone(uint boneHash)
        {
            if(boneHash == 3449071621)
                return null;
            foreach (Bone b in bones)
                if (b.boneId == boneHash)
                    return b;
            //MessageBox.Show("Open the VBN before editing the SB");
            return null;
        }

        public bool essentialComparison(VBN compareTo)
        {
            // Because I don't want to override == just for a cursory bone comparison
            if (this.bones.Count != compareTo.bones.Count)
                return false;

            for (int i = 0; i < this.bones.Count; i++)
            {
                if (this.bones[i].Name != compareTo.bones[i].Name)
                    return false;
                if (this.bones[i].pos != compareTo.bones[i].pos)
                    return false;
            }
            return true;
        }
    }

    public class SB : FileBase
    {
        public override Endianness Endian
        {
            get;
            set;
        }

        public class SBEntry
        {
            public uint hash = 3449071621;
            public float param1_1, param2_1;
            public int param1_2, param1_3, param2_2, param2_3;
            public float rx1, rx2, ry1, ry2, rz1, rz2;
            public uint[] boneHashes = new uint[8] { 3449071621, 3449071621, 3449071621, 3449071621, 3449071621, 3449071621, 3449071621, 3449071621 };
            public float[] unks1 = new float[4], unks2 = new float[5];
            public float factor;
            public int[] ints = new int[4];

            public override string ToString()
            {
                return VBN.BoneNameFromHash(hash);
            }
        }

        public string FilePath;
        public List<SBEntry> bones = new List<SBEntry>();

        public SB()
        {
            ImageKey = "skeleton";
            SelectedImageKey = "skeleton";
            Text = "model.sb";

            ContextMenu = new ContextMenu();
            MenuItem OpenEdit = new MenuItem("Open Editor");
            OpenEdit.Click += OpenEditor;
            ContextMenu.MenuItems.Add(OpenEdit);
        }

        public void TryGetEntry(uint hash, out SBEntry entry)
        {
            entry = null;
            foreach(SBEntry sb in bones)
                if (sb.hash == hash)
                    entry = sb;
        }

        public void OpenEditor(object sender, EventArgs args)
        {
            SwagEditor swagEditor = new SwagEditor(this);
            swagEditor.Text = Path.GetFileName(FilePath);
            swagEditor.FilePath = FilePath;
            MainForm.Instance.AddDockedControl(swagEditor);
        }

        public override void Read(string filename)
        {
            FileData d = new FileData(filename);
            FilePath = filename;
            d.Endian = Endianness.Little; // characters are little
            d.seek(8); // skip magic and version?
            int count = d.readInt(); // entry count

            for(int i = 0; i < count; i++)
            {
                SBEntry sb = new SBEntry()
                {
                    hash = (uint)d.readInt(),
                    param1_1 = d.readFloat(),
                    param1_2 = d.readInt(),
                    param1_3 = d.readInt(),
                    param2_1 = d.readFloat(),
                    param2_2 = d.readInt(),
                    param2_3 = d.readInt(),
                    rx1 = d.readFloat(),
                    rx2 = d.readFloat(),
                    ry1 = d.readFloat(),
                    ry2 = d.readFloat(),
                    rz1 = d.readFloat(),
                    rz2 = d.readFloat()
                };

                for (int j = 0; j < 8; j++)
                    sb.boneHashes[j] = (uint)d.readInt();

                for (int j = 0; j < 4; j++)
                    sb.unks1[j] = d.readFloat();

                for (int j = 0; j < 5; j++)
                    sb.unks2[j] = d.readFloat();

                sb.factor = d.readFloat();

                for (int j = 0; j < 4; j++)
                    sb.ints[j] = d.readInt();

                bones.Add(sb);

                /*Console.WriteLine(sb.hash.ToString("x"));
                Console.WriteLine(d.readFloat() + " " + d.readInt() + " " + d.readInt());
                Console.WriteLine(d.readFloat() + " " + d.readInt() + " " + d.readInt());

                //28 floats?
                Console.WriteLine(d.readFloat() + " " + d.readFloat());
                Console.WriteLine(d.readFloat() + " " + d.readFloat());
                Console.WriteLine(d.readFloat() + " " + d.readFloat());
                Console.WriteLine(d.readFloat() + " " + d.readFloat() + " " + d.readFloat() + " " + d.readFloat());
                Console.WriteLine(d.readFloat() + " " + d.readFloat() + " " + d.readFloat() + " " + d.readFloat());

                Console.WriteLine(d.readFloat() + " " + d.readFloat() + " " + d.readFloat() + " " + d.readFloat());
                Console.WriteLine(d.readFloat() + " " + d.readFloat() + " " + d.readFloat() + " " + d.readFloat());

                Console.WriteLine(d.readFloat() + " " + d.readFloat());
                Console.WriteLine(d.readInt() +  " " + d.readInt());
                Console.WriteLine(d.readInt() + " " + d.readInt());
                Console.WriteLine();*/
            }
        }

        public override byte[] Rebuild()
        {
            FileOutput o = new FileOutput();
            o.Endian = Endianness.Little;

            o.writeString(" BWS");
            o.writeShort(0x05);
            o.writeShort(0x01);
            o.writeInt(bones.Count);

            foreach(SBEntry s in bones)
            {
                o.writeInt((int)s.hash);
                o.writeFloat(s.param1_1);
                o.writeInt(s.param1_2);
                o.writeInt(s.param1_3);
                o.writeFloat(s.param2_1);
                o.writeInt(s.param2_2);
                o.writeInt(s.param2_3);
                o.writeFloat(s.rx1);
                o.writeFloat(s.rx2);
                o.writeFloat(s.ry1);
                o.writeFloat(s.ry2);
                o.writeFloat(s.rz1);
                o.writeFloat(s.rz2);

                for (int j = 0; j < 8; j++)
                    o.writeInt((int)s.boneHashes[j]);

                for (int j = 0; j < 4; j++)
                    o.writeFloat(s.unks1[j]);

                for (int j = 0; j < 5; j++)
                    o.writeFloat(s.unks2[j]);

                o.writeFloat(s.factor);

                for (int j = 0; j < 4; j++)
                    o.writeInt(s.ints[j]);
            }

            return o.getBytes();
        }
    }
}

namespace Smash_Forge
{
    public class csvHashes
    {
        public Dictionary<string, uint> names = new Dictionary<string, uint>();
        public Dictionary<uint, string> ids = new Dictionary<uint, string>();

        public csvHashes(string filename)
        {
            var reader = new StreamReader(File.OpenRead(filename));

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                names.Add(values[0], Convert.ToUInt32(values[1]));
                ids.Add(Convert.ToUInt32(values[1]), values[0]);
            }
        }
    }
}