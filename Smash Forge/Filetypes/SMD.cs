using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenTK;

namespace Smash_Forge
{
    public class SMD
    {

        public static void read(string fname, Animation a, VBN v)
        {
            StreamReader reader = File.OpenText(fname);
            string line;

            string current = "";
            bool readBones = false;
            int frame = 0, prevframe = 0;
            KeyFrame k = new KeyFrame();

            VBN vbn = v;
            if (v.bones.Count == 0)
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
                    a.Bones.Add(node);
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
                    bone.RotType = Animation.RotationType.EULER;

                    Animation.KeyFrame n = new Animation.KeyFrame();
                    n.Value = float.Parse(args[1]);
                    n.Frame = frame;
                    bone.XPOS.Nodes.Add(n);

                    n = new Animation.KeyFrame();
                    n.Value = float.Parse(args[2]);
                    n.Frame = frame;
                    bone.YPOS.Nodes.Add(n);

                    n = new Animation.KeyFrame();
                    n.Value = float.Parse(args[3]);
                    n.Frame = frame;
                    bone.ZPOS.Nodes.Add(n);

                    n = new Animation.KeyFrame();
                    n.Value = float.Parse(args[4]);
                    n.Frame = frame;
                    bone.XROT.Nodes.Add(n);

                    n = new Animation.KeyFrame();
                    n.Value = float.Parse(args[5]);
                    n.Frame = frame;
                    bone.YROT.Nodes.Add(n);

                    n = new Animation.KeyFrame();
                    n.Value = float.Parse(args[6]);
                    n.Frame = frame;
                    bone.ZROT.Nodes.Add(n);
                }
            }

            a.FrameCount = frame;

            v.boneCountPerType[0] = (uint)vbn.bones.Count;
            v.update();
        }

        public static NUD toNUD(string fname)
        {
            StreamReader reader = File.OpenText(fname);
            string line;

            string current = "";

            NUD nud = new NUD();

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
                        NUD.Vertex vert = new NUD.Vertex();
                        vert.pos = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                        vert.nrm = new Vector3(float.Parse(args[4]), float.Parse(args[5]), float.Parse(args[6]));
                        vert.uv.Add(new Vector2(float.Parse(args[7]), float.Parse(args[8])));
                        int wCount = int.Parse(args[9]);
                        int w = 10;
                        for (int i = 0; i < wCount; i++)
                        {
                            vert.node.Add(int.Parse(args[w++]));
                            vert.weight.Add(float.Parse(args[w++]));
                        }

                        NUD.Mesh mes = null;
                        foreach (NUD.Mesh m in nud.meshes)
                        {
                            if (m.Text.Equals(meshName))
                            {
                                mes = m;
                            }
                        }
                        if (mes == null)
                        {
                            mes = new NUD.Mesh();
                            mes.Text = meshName;
                            nud.meshes.Add(mes);
                        }
                        if (mes.Nodes.Count == 0)
                        {
                            NUD.Polygon poly = new NUD.Polygon();
                            poly.setDefaultMaterial();
                            mes.Nodes.Add(poly);
                        }
                        bool found = false;
                        foreach (NUD.Vertex nv in ((NUD.Polygon)mes.Nodes[0]).vertices)
                        {
                            if (nv.pos.Equals(vert.pos))
                            {
                                ((NUD.Polygon)mes.Nodes[0]).faces.Add(((NUD.Polygon)mes.Nodes[0]).vertices.IndexOf(nv));
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            ((NUD.Polygon)mes.Nodes[0]).faces.Add(((NUD.Polygon)mes.Nodes[0]).vertices.Count);
                            ((NUD.Polygon)mes.Nodes[0]).vertices.Add(vert);
                        }
                    }
                }
            }

            nud.PreRender();
            return nud;
        }

        public static void Save(Animation anim, VBN Skeleton, String Fname)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@Fname))
            {
                file.WriteLine("version 1");

                file.WriteLine("nodes");
                foreach(Bone b in Skeleton.bones)
                {
                    file.WriteLine(Skeleton.bones.IndexOf(b) + " \"" + b.Text + "\" " + b.parentIndex);
                }
                file.WriteLine("end");
                
                file.WriteLine("skeleton");
                anim.SetFrame(0);
                for(int i = 0; i <= anim.FrameCount; i++)
                {
                    anim.NextFrame(Skeleton);
                    
                    file.WriteLine("time " + i);

                    foreach (Animation.KeyNode sb in anim.Bones)
                    {
                        Bone b = Skeleton.getBone(sb.Text);
                        if (b == null) continue;
                        Vector3 eul = ANIM.quattoeul(b.rot);
                        file.WriteLine(Skeleton.bones.IndexOf(b) + " " + b.pos.X + " " + b.pos.Y + " " + b.pos.Z + " " + eul.X + " " + eul.Y + " " + eul.Z);
                    }

                }
                file.WriteLine("end");

                file.Close();
            }
        }
    }
}

