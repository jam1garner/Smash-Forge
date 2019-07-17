using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenTK;
using System.Text;

namespace SmashForge
{
    public struct SmdVertex
    {
        public int parent;
        public Vector3 p;
        public Vector3 n;
        public Vector2 uv;
        public int[] bones;
        public float[] weights;
    }

    public class SmdTriangle
    {
        public string material;
        public SmdVertex v1, v2, v3;
    }

    public class Smd
    {
        public VBN bones;
        public Animation animation; // todo
        public List<SmdTriangle> triangles;

        public Smd()
        {
            bones = new VBN();
            triangles = new List<SmdTriangle>();
        }

        public Smd(string fname)
        {
            Read(fname);
        }

        public void Read(string fname)
        {
            StreamReader reader = File.OpenText(fname);
            string line;

            string current = "";

            bones = new VBN();
            triangles = new List<SmdTriangle>();
            Dictionary<int, Bone> boneList = new Dictionary<int, Bone>();

            int time = 0;
            while ((line = reader.ReadLine()) != null)
            {
                line = Regex.Replace(line, @"\s+", " ");
                string[] args = line.Replace(";", "").TrimStart().Split(' ');

                if (args[0].Equals("triangles") || args[0].Equals("end") || args[0].Equals("skeleton") || args[0].Equals("nodes"))
                {
                    current = args[0];
                    continue;
                }

                if (current.Equals("nodes"))
                {
                    int id = int.Parse(args[0]);
                    Bone b = new Bone(bones);
                    b.Text = args[1].Replace('"', ' ').Trim();
                    int s = 2;
                    while (args[s].Contains("\""))
                        b.Text += args[s++];
                    b.parentIndex = int.Parse(args[s]);
                    boneList.Add(id, b);
                }

                if (current.Equals("skeleton"))
                {
                    if (args[0].Contains("time"))
                        time = int.Parse(args[1]);
                    else
                    {
                        if(time == 0)
                        {
                            Bone b = boneList[int.Parse(args[0])];
                            b.position = new float[3];
                            b.rotation = new float[3];
                            b.scale = new float[3];
                            b.position[0] = float.Parse(args[1]);
                            b.position[1] = float.Parse(args[2]);
                            b.position[2] = float.Parse(args[3]);
                            b.rotation[0] = float.Parse(args[4]);
                            b.rotation[1] = float.Parse(args[5]);
                            b.rotation[2] = float.Parse(args[6]);
                            b.scale[0] = 1f;
                            b.scale[1] = 1f;
                            b.scale[2] = 1f;

                            b.pos = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                            b.rot = VBN.FromEulerAngles(float.Parse(args[6]), float.Parse(args[5]), float.Parse(args[4]));

                            bones.bones.Add(b);

                            if(b.parentIndex != -1)
                                b.parentIndex = bones.bones.IndexOf(boneList[b.parentIndex]);
                        }
                    }
                }

                if (current.Equals("triangles"))
                {
                    string meshName = args[0];
                    if (args[0].Equals(""))
                        continue;

                    SmdTriangle t = new SmdTriangle();
                    triangles.Add(t);
                    t.material = meshName;

                    for (int j = 0; j < 3; j++)
                    {
                        line = reader.ReadLine();
                        line = Regex.Replace(line, @"\s+", " ");
                        args = line.Replace(";", "").TrimStart().Split(' ');

                        int parent = int.Parse(args[0]);
                        SmdVertex vert = new SmdVertex();
                        vert.p = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                        vert.n = new Vector3(float.Parse(args[4]), float.Parse(args[5]), float.Parse(args[6]));
                        vert.uv = new Vector2(float.Parse(args[7]), float.Parse(args[8]));
                        vert.bones = new int[0];
                        vert.weights = new float[0];
                        if (args.Length > 9)
                        {
                            int wCount = int.Parse(args[9]);
                            int w = 10;
                            vert.bones = new int[wCount];
                            vert.weights = new float[wCount];
                            for (int i = 0; i < wCount; i++)
                            {
                                vert.bones[i] = (int.Parse(args[w++]));
                                vert.weights[i] = (float.Parse(args[w++]));
                            }
                        }
                        switch (j)
                        {
                            case 0: t.v1 = vert; break;
                            case 1: t.v2 = vert; break;
                            case 2: t.v3 = vert; break;
                        }
                    }
                }
            }
            bones.reset();
        }

        public void Save(string fileName)
        {
            StringBuilder o = new StringBuilder();

            o.AppendLine("version 1");

            if(bones != null)
            {
                o.AppendLine("nodes");
                for(int i = 0; i < bones.bones.Count; i++)
                    o.AppendLine("  " + i + " \"" + bones.bones[i].Text + "\" " + bones.bones[i].parentIndex);
                o.AppendLine("end");

                o.AppendLine("skeleton");
                o.AppendLine("time 0");
                for (int i = 0; i < bones.bones.Count; i++)
                {
                    Bone b = bones.bones[i];
                    o.AppendFormat("{0} {1} {2} {3} {4} {5} {6}\n", i, b.position[0], b.position[1], b.position[2], b.rotation[0], b.rotation[1], b.rotation[2]);
                }
                o.AppendLine("end");
            }

            if(triangles != null)
            {
                o.AppendLine("triangles");
                foreach(SmdTriangle tri in triangles)
                {
                    o.AppendLine(tri.material);
                    WriteVertex(o, tri.v1);
                    WriteVertex(o, tri.v2);
                    WriteVertex(o, tri.v3);
                }
                o.AppendLine("end");
            }

            File.WriteAllText(fileName, o.ToString());
        }

        private void WriteVertex(StringBuilder o, SmdVertex v)
        {
            o.AppendFormat("{0} {1} {2} {3} {4} {5} {6} {7} {8} ",
                        v.parent,
                        v.p.X, v.p.Y, v.p.Z,
                        v.n.X, v.n.Y, v.n.Z,
                        v.uv.X, v.uv.Y);
            if(v.weights == null)
            {
                o.AppendLine("0");
            }
            else
            {
                string weights = v.weights.Length + "";
                for (int i = 0; i < v.weights.Length; i++)
                {
                    weights += " " + v.bones[i] + " " + v.weights[i];
                }
                o.AppendLine(weights);
            }
        }

        public static void ToBfres(string fname)
        {
            StreamReader reader = File.OpenText(fname);
            string line;
            string current = "";
            bool readBones = false;
            int frame = 0, prevframe = 0;
            string text = "";

            readBones = true;

            while ((line = reader.ReadLine()) != null)
            {
                line = Regex.Replace(line, @"\s+", " ");
                string[] args = line.Replace(";", "").TrimStart().Split(' ');

                if (args[0].Equals("nodes") || args[0].Equals("skeleton") || args[0].Equals("end") || args[0].Equals("time"))
                {
                    current = args[0];
                    if (args.Length > 1)
                    {
                        prevframe = frame;
                        frame = int.Parse(args[1]);
                    }
                    continue;
                }
                if (current.Equals("nodes"))
                {
                    text = args[1].Replace("\"", "");
                }
                if (current.Equals("time"))
                {
    
                }
            }
        }

        public static void Read(string fname, Animation a, VBN v)
        {
            StreamReader reader = File.OpenText(fname);
            string line;

            string current = "";
            bool readBones = false;
            int frame = 0, prevframe = 0;
            KeyFrame k = new KeyFrame();

            VBN vbn = v;
            if (v != null && v.bones.Count == 0)
            {
                readBones = true;
            }
            else
                vbn = new VBN();

            while ((line = reader.ReadLine()) != null)
            {
                line = Regex.Replace(line, @"\s+", " ");
                string[] args = line.Replace(";", "").TrimStart().Split(' ');

                if (args[0].Equals("nodes") || args[0].Equals("skeleton") || args[0].Equals("end") || args[0].Equals("time"))
                {
                    current = args[0];
                    if (args.Length > 1)
                    {
                        prevframe = frame;
                        frame = int.Parse(args[1]);

                        /*if (frame != prevframe + 1) {
							Console.WriteLine ("Needs interpolation " + frame);
						}*/

                        k = new KeyFrame();
                        k.frame = frame;
                        //a.addKeyframe(k);
                    }
                    continue;
                }

                if (current.Equals("nodes"))
                {
                    Bone b = new Bone(vbn);
                    b.Text = args[1].Replace("\"", "");
                    b.parentIndex = int.Parse(args[2]);
                    //b.children = new System.Collections.Generic.List<int> ();
                    vbn.totalBoneCount++;
                    vbn.bones.Add(b);
                    Animation.KeyNode node = new Animation.KeyNode(b.Text);
                    a.bones.Add(node);
                }

                if (current.Equals("time"))
                {
                    //Animation.KeyFrame n = new Animation.KeyFrame();
                    /*n.id = v.boneIndex(vbn.bones[int.Parse(args[0])].Text);
                    if (n.id == -1)
                    {
                        continue;
                    }
                    else
                        n.hash = v.bones[n.id].boneId;*/

                    // only if it finds the node
                    //k.addNode(n);

                    // reading the skeleton if this isn't an animation
                    if (readBones && frame == 0)
                    {
                        Bone b = vbn.bones[int.Parse(args[0])];
                        b.position = new float[3];
                        b.rotation = new float[3];
                        b.scale = new float[3];
                        b.position[0] = float.Parse(args[1]);
                        b.position[1] = float.Parse(args[2]);
                        b.position[2] = float.Parse(args[3]);
                        b.rotation[0] = float.Parse(args[4]);
                        b.rotation[1] = float.Parse(args[5]);
                        b.rotation[2] = float.Parse(args[6]);
                        b.scale[0] = 1f;
                        b.scale[1] = 1f;
                        b.scale[2] = 1f;

                        b.pos = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                        b.rot = VBN.FromEulerAngles(float.Parse(args[6]), float.Parse(args[5]), float.Parse(args[4]));

                        if(b.parentIndex!=-1)
                        	vbn.bones [b.parentIndex].Nodes.Add (b);
                    }
                    Animation.KeyNode bone = a.GetBone(vbn.bones[int.Parse(args[0])].Text);
                    bone.rotType = Animation.RotationType.Euler;

                    Animation.KeyFrame n = new Animation.KeyFrame();
                    n.Value = float.Parse(args[1]);
                    n.Frame = frame;
                    bone.xpos.keys.Add(n);

                    n = new Animation.KeyFrame();
                    n.Value = float.Parse(args[2]);
                    n.Frame = frame;
                    bone.ypos.keys.Add(n);

                    n = new Animation.KeyFrame();
                    n.Value = float.Parse(args[3]);
                    n.Frame = frame;
                    bone.zpos.keys.Add(n);

                    n = new Animation.KeyFrame();
                    n.Value = float.Parse(args[4]);
                    n.Frame = frame;
                    bone.xrot.keys.Add(n);

                    n = new Animation.KeyFrame();
                    n.Value = float.Parse(args[5]);
                    n.Frame = frame;
                    bone.yrot.keys.Add(n);

                    n = new Animation.KeyFrame();
                    n.Value = float.Parse(args[6]);
                    n.Frame = frame;
                    bone.zrot.keys.Add(n);

                    if(args.Length > 7)
                    {
                        n = new Animation.KeyFrame();
                        n.Value = float.Parse(args[7]);
                        n.Frame = frame;
                        bone.xsca.keys.Add(n);

                        n = new Animation.KeyFrame();
                        n.Value = float.Parse(args[8]);
                        n.Frame = frame;
                        bone.ysca.keys.Add(n);

                        n = new Animation.KeyFrame();
                        n.Value = float.Parse(args[9]);
                        n.Frame = frame;
                        bone.zsca.keys.Add(n);
                    }
                }
            }

            a.frameCount = frame;

            vbn.boneCountPerType[0] = (uint)vbn.bones.Count;
            vbn.update();
        }

        public static Nud ToNud(string fname)
        {
            StreamReader reader = File.OpenText(fname);
            string line;

            string current = "";

            Nud nud = new Nud();

            while ((line = reader.ReadLine()) != null)
            {
                line = Regex.Replace(line, @"\s+", " ");
                string[] args = line.Replace(";", "").TrimStart().Split(' ');

                if (args[0].Equals("triangles") || args[0].Equals("end"))
                {
                    current = args[0];
                    continue;
                }

                if (current.Equals("triangles"))
                {
                    string meshName = args[0];
                    if (args[0].Equals(""))
                        continue;
                    for (int j = 0; j < 3; j++)
                    {
                        line = reader.ReadLine();
                        line = Regex.Replace(line, @"\s+", " ");
                        args = line.Replace(";", "").TrimStart().Split(' ');
                        // read triangle strip
                        int parent = int.Parse(args[0]);
                        Nud.Vertex vert = new Nud.Vertex();
                        vert.pos = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                        vert.nrm = new Vector3(float.Parse(args[4]), float.Parse(args[5]), float.Parse(args[6]));
                        vert.uv.Add(new Vector2(float.Parse(args[7]), float.Parse(args[8])));
                        int wCount = int.Parse(args[9]);
                        int w = 10;
                        for (int i = 0; i < wCount; i++)
                        {
                            vert.boneIds.Add(int.Parse(args[w++]));
                            vert.boneWeights.Add(float.Parse(args[w++]));
                        }

                        Nud.Mesh mes = null;
                        foreach (Nud.Mesh m in nud.Nodes)
                        {
                            if (m.Text.Equals(meshName))
                            {
                                mes = m;
                            }
                        }
                        if (mes == null)
                        {
                            mes = new Nud.Mesh();
                            mes.Text = meshName;
                            nud.Nodes.Add(mes);
                        }
                        if (mes.Nodes.Count == 0)
                        {
                            Nud.Polygon poly = new Nud.Polygon();
                            poly.AddDefaultMaterial();
                            mes.Nodes.Add(poly);
                        }
                        {
                            ((Nud.Polygon)mes.Nodes[0]).vertexIndices.Add(((Nud.Polygon)mes.Nodes[0]).vertices.Count);
                            ((Nud.Polygon)mes.Nodes[0]).vertices.Add(vert);
                        }
                    }
                }
            }

            nud.OptimizeFileSize();
            nud.UpdateRenderMeshes();
            return nud;
        }

        public static void Save(Animation anim, VBN skeleton, String fname)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fname))
            {
                file.WriteLine("version 1");

                file.WriteLine("nodes");
                foreach(Bone b in skeleton.bones)
                {
                    file.WriteLine(skeleton.bones.IndexOf(b) + " \"" + b.Text + "\" " + b.parentIndex);
                }
                file.WriteLine("end");
                
                file.WriteLine("skeleton");
                anim.SetFrame(0);
                for(int i = 0; i <= anim.frameCount; i++)
                {
                    anim.NextFrame(skeleton);
                    
                    file.WriteLine("time " + i);

                    foreach (Animation.KeyNode sb in anim.bones)
                    {
                        Bone b = skeleton.getBone(sb.Text);
                        if (b == null) continue;
                        Vector3 eul = ANIM.quattoeul(b.rot);
                        file.WriteLine(skeleton.bones.IndexOf(b) + " " + b.pos.X + " " + b.pos.Y + " " + b.pos.Z + " " + eul.X + " " + eul.Y + " " + eul.Z);
                    }

                }
                file.WriteLine("end");

                file.Close();
            }
        }
    }
}

