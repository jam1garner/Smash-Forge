using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;

namespace Smash_Forge
{
    public class KeyFrame
    {
        public List<KeyNode> nodes = new List<KeyNode>();
        public int frame = 0;
        public int frameRate = 60;

        public void addNode(KeyNode n)
        {
            nodes.Add(n);
        }

        public bool contains(int id)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].id == id)
                    return true;
            }
            return false;
        }

        public KeyNode getNodeid(int id)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].id == id)
                    return nodes[j];
            }
            return null;
        }

        public KeyNode getNode(int i)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].id == i)
                    return nodes[j];
            }

            KeyNode ne = new KeyNode();
            ne.id = i;

            addNode(ne);

            return ne;
        }
    }

    public class KeyNode
    {
        public const int INTERPOLATED = 0;
        public const int CONSTANT = 1;
        public const int KEYFRAME = 2;
        public const int COMPRESSED = 3;

        public int id = -1;
		public uint hash = 0;
        public int t_type, r_type, s_type;
        public Vector3 t, s = new Vector3(1f, 1f, 1f);  // Trans / scale
        public Quaternion r;  // Rotation

        public Vector3 t2, s2, rv, rv2;
        public Quaternion r2;

        public float r_extra;

        public KeyNode()
        {
            t_type = -1;
            r_type = -1;
            s_type = -1;
        }
    }

    public class SkelAnimation
    {

        public object Tag;
        public bool Main = false;
        public List<object> children = new List<object>();
        public List<KeyFrame> frames = new List<KeyFrame>();
        private int frame = 0;

        public SkelAnimation()
        {

        }

        public void addKeyframe(KeyFrame k)
        {
            frames.Add(k);
        }

        public List<int> getNodes(bool fromHash, VBN vbn = null)
        {
            List<int> node = new List<int>();

            foreach (KeyFrame f in frames)
                foreach (KeyNode n in f.nodes)
                {
                    if (fromHash && vbn != null)
                    {
                        foreach (Bone bo in vbn.bones)
                        {
                            if (bo.boneId == n.hash)
                            {
                                if (!node.Contains(vbn.bones.IndexOf(bo)))
                                    node.Add(vbn.bones.IndexOf(bo));
                                break;
                            }
                        }
                    }
                    else
                    if (!node.Contains(n.id) && n.id != -1)
                    {
                        node.Add(n.id);
                    }
                }

            return node;
        }

        public void setFrame(int i)
        {
            frame = i;
        }

        public int getFrame()
        {
            return frame;
        }

        public int size()
        {
            return frames.Count;
        }

        public void nextFrame(VBN vbn)
        {
            if (frame >= frames.Count)
                return;
            
            if (frame == 0 && Main)
            {
                vbn.reset();

                foreach (ModelContainer con in Runtime.ModelContainers)
                {
                    if (con.nud != null && con.mta != null)
                    {
                        con.nud.applyMTA(con.mta, 0);
                    }
                }
            }

            if (children.Count > 0) Main = true;

            foreach (object child in children)
            {
                if(child is SkelAnimation)
                {
                    ((SkelAnimation)child).setFrame(frame);
                    ((SkelAnimation)child).nextFrame(vbn);
                }
                if (child is MTA)
                {
                    foreach(ModelContainer con in Runtime.ModelContainers)
                    {
                        if(con.nud != null)
                        {
                            con.nud.applyMTA(((MTA)child), frame);
                        }
                    }
                }
            }

			KeyFrame key = frames[frame];

            foreach (KeyNode n in key.nodes)
            {
				//if (n.id == -1)
				//	continue;
				
				//if (n.hash == 0) {
                    //continue;
					//n.hash = vbn.bones [n.id].boneId;
				//}

				int id = -1;

				foreach (Bone bo in vbn.bones) {
                    if (bo.boneId == n.hash)
                    {
                        id = vbn.bones.IndexOf(bo);
                        n.id = id;
                        break;
                    }
                }

                if (id == -1)
					continue;

                Bone b = vbn.bones[id];

                if (n.t_type != -1)// !b.isSwingBone)
                {
                    b.pos = n.t;
                }
                // We don't do the same swingBone check on rotation because as of yet
                // I have not seen an example of the rotation data being garbage, and it's
                // actually used properly in the animations - Struz
                if (n.r_type != -1)
                {
                    if (b.Text.Equals("HeadN"))
                    {
                        //Console.WriteLine(b.transform.ExtractRotation().ToString());
                        //Console.WriteLine(VBN.FromEulerAngles(b.rotation[0], b.rotation[1], b.rotation[2]).ToString());
                        //Console.WriteLine(n.r.ToString() + " " + Math.Sqrt(n.r.X* n.r.X + n.r.Y* n.r.Y + n.r.Z* n.r.Z + n.r.W*n.r.W) + " " + n.r.Normalized().ToString());
                    }
                    //Console.WriteLine(new string(b.boneName) + " " + b.rot.ToString() + " " + n.r.ToString() + "\n" + (n.r.X + n.r.Y + n.r.X + n.r.W));
                    b.rot = n.r;
                }
                if (n.s_type != -1)
                {
                    b.sca = n.s;
                }
                else
                    b.sca = new Vector3(b.scale[0], b.scale[1], b.scale[2]);
            }


            frame++;
            if (frame >= frames.Count)
                frame = 0;
            vbn.update();

        }


        public KeyNode getFirstNode(int nodeIndex)
        {
            foreach (KeyFrame k in frames)
                if (k.contains(nodeIndex))
                {
                    return k.getNodeid(nodeIndex);
                }

            return null;
        }


        public int getNodeIndex(string n, VBN vbn)
        {
            for (int i = 0; i < vbn.bones.Count; i++)
            {
                if (vbn.bones[i].Text.Equals(n))
                {
                    return i;
                }
            }
            return -1;
        }

        public KeyNode getNode(int frame, int nodeIndex)
        {
            while (frames.Count <= frame)
                frames.Add(new KeyFrame());

            KeyFrame f = frames[frame];

            return f.getNode(nodeIndex);
        }


        public float getBaseNodeValue(int nid, String type, VBN vbn)
        {
            //UNSAFE: Hacky fix
            if (nid == -1)
                return 0;

            switch (type)
            {
                case "X":
                    return vbn.bones[nid].position[0];
                case "Y":
                    return vbn.bones[nid].position[1];
                case "Z":
                    return vbn.bones[nid].position[2];
                case "RX":
                    return vbn.bones[nid].rotation[0];
                case "RY":
                    return vbn.bones[nid].rotation[1];
                case "RZ":
                    return vbn.bones[nid].rotation[2];
                case "SX":
                    return vbn.bones[nid].scale[0];
                case "SY":
                    return vbn.bones[nid].scale[1];
                case "SZ":
                    return vbn.bones[nid].scale[2];
            }

            return 0;
        }

        /*
		 * TODO: Fix this
		 * the key frame needs to check if it occurs within a time frame AND
		 * it has the node it is looking for
		*/
        public void bakeFramesLinear()
        {

            List<int> nodeids = getNodes(false);
            List<KeyFrame> base_frames = frames;
            frames = new List<KeyFrame>();
            int fCount = 0;

            foreach (KeyFrame k in base_frames)
            {
                if (k.frame > fCount)
                    fCount = k.frame;
            }


            for (int i = 0; i < fCount; i++)
            {
                KeyFrame k = new KeyFrame();
                k.frame = i;
                frames.Add(k);

                // add all the nodes at this frame
                foreach (int id in nodeids)
                {
                    KeyFrame f1 = base_frames[0], f2 = base_frames[0];

                    if (base_frames.Count > 1)
                        for (int j = 0; j < base_frames.Count - 1; j++)
                        {
                            if (base_frames[j].frame <= i && base_frames[j + 1].frame >= i
                                && base_frames[j].contains(id) && base_frames[j + 1].contains(id))
                            {
                                f1 = base_frames[j];
                                f2 = base_frames[j + 1];
                                break;
                            }
                        }

                    // interpolate the values
                    KeyNode n = new KeyNode();
                    n.id = id;

                    KeyNode k1 = f1.getNodeid(id), k2 = f2.getNodeid(id);
                    n.hash = k1.hash;

                    n.t_type = k1.t_type;
                    n.r_type = k1.r_type;
                    n.s_type = k1.s_type;

                    n.t.X = lerp(k1.t.X, k2.t.X, f1.frame, f2.frame, i);
                    n.t.Y = lerp(k1.t.Y, k2.t.Y, f1.frame, f2.frame, i);
                    n.t.Z = lerp(k1.t.Z, k2.t.Z, f1.frame, f2.frame, i);

                    n.r.X = lerp(k1.r.X, k2.r.X, f1.frame, f2.frame, i);
                    n.r.Y = lerp(k1.r.Y, k2.r.Y, f1.frame, f2.frame, i);
                    n.r.Z = lerp(k1.r.Z, k2.r.Z, f1.frame, f2.frame, i);
                    n.r.W = lerp(k1.r.W, k2.r.W, f1.frame, f2.frame, i);

                    //n.s.X = lerp (k1.s.X, k2.s.X, f1.frame, f2.frame, i);
                    //n.s.Y = lerp (k1.s.Y, k2.s.Y, f1.frame, f2.frame, i);
                    //n.s.Z = lerp (k1.s.Z, k2.s.Z, f1.frame, f2.frame, i);


                    k.addNode(n);
                }

            }
        }

        public static float lerp(float av, float bv, float v0, float v1, float t)
        {
            if (v0 == v1) return av;

            float mu = (t - v0) / (v1 - v0);
            return ((av * (1 - mu)) + (bv * mu));
        }
    }
}

