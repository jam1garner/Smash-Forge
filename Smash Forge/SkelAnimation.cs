using System;
using System.Collections.Generic;
using OpenTK;

// Obselete as f
// Will probably remove eventually
// Still used for some converting...

namespace SmashForge
{
    public class KeyFrame
    {
        public List<KeyNode> nodes = new List<KeyNode>();
        public int frame = 0;
        public int frameRate = 60;

        public void AddNode(KeyNode n)
        {
            nodes.Add(n);
        }

        public bool Contains(int id)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].id == id)
                    return true;
            }
            return false;
        }

        public KeyNode GetNodeid(int id)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].id == id)
                    return nodes[j];
            }
            return null;
        }

        public KeyNode GetNode(int i)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].id == i)
                    return nodes[j];
            }

            KeyNode ne = new KeyNode();
            ne.id = i;

            AddNode(ne);

            return ne;
        }
    }

    public class KeyNode
    {
        public const int Interpolated = 0;
        public const int Constant = 1;
        public const int Keyframe = 2;
        public const int Compressed = 3;

        public int id = -1;
		public uint hash = 0;
        public int tType, rType, sType;
        public Vector3 t, s = new Vector3(1f, 1f, 1f);  // Trans / scale
        public Quaternion r;  // Rotation

        public Vector3 t2, s2, rv, rv2;
        public Quaternion r2;

        public float rExtra;

        public KeyNode()
        {
            tType = -1;
            rType = -1;
            sType = -1;
        }
    }

    public class SkelAnimation
    {

        public object tag;
        public List<object> children = new List<object>();
        public List<KeyFrame> frames = new List<KeyFrame>();
        private int frame = 0;

        public SkelAnimation()
        {

        }

        public void AddKeyframe(KeyFrame k)
        {
            frames.Add(k);
        }

        public List<int> GetNodes(bool fromHash, VBN vbn = null)
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

        public void SetFrame(int i)
        {
            frame = i;
        }

        public int GetFrame()
        {
            return frame;
        }

        public int Size()
        {
            return frames.Count;
        }

        public void NextFrame(VBN vbn, bool isChild = false)
        {
            if (frame >= frames.Count)
                return;
            
            if (frame == 0 && (!isChild || children.Count > 0))
            {
                vbn.reset();

                /*foreach (ModelContainer con in Runtime.ModelContainers)
                {
                    if (con.NUD != null && con.mta != null)
                    {
                        con.NUD.applyMTA(con.mta, 0);
                    }
                }*/
            }

            foreach (object child in children)
            {
                if(child is SkelAnimation)
                {
                    ((SkelAnimation)child).SetFrame(frame);
                    ((SkelAnimation)child).NextFrame(vbn, isChild: true);
                }
                if (child is MTA)
                {
                    /*foreach(ModelContainer con in Runtime.ModelContainers)
                    {
                        if(con.NUD != null)
                        {
                            con.NUD.applyMTA(((MTA)child), frame);
                        }
                    }*/
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

                if (n.tType != -1)// !b.isSwingBone)
                {
                    b.pos = n.t;
                }
                // We don't do the same swingBone check on rotation because as of yet
                // I have not seen an example of the rotation data being garbage, and it's
                // actually used properly in the animations - Struz
                if (n.rType != -1)
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
                if (n.sType != -1)
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


        public KeyNode GetFirstNode(int nodeIndex)
        {
            foreach (KeyFrame k in frames)
                if (k.Contains(nodeIndex))
                {
                    return k.GetNodeid(nodeIndex);
                }

            return null;
        }


        public int GetNodeIndex(string n, VBN vbn)
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

        public KeyNode GetNode(int frame, int nodeIndex)
        {
            while (frames.Count <= frame)
                frames.Add(new KeyFrame());

            KeyFrame f = frames[frame];

            return f.GetNode(nodeIndex);
        }


        public float GetBaseNodeValue(int nid, String type, VBN vbn)
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
        public void BakeFramesLinear()
        {

            List<int> nodeids = GetNodes(false);
            List<KeyFrame> baseFrames = frames;
            frames = new List<KeyFrame>();
            int fCount = 0;

            foreach (KeyFrame k in baseFrames)
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
                    KeyFrame f1 = baseFrames[0], f2 = baseFrames[0];

                    if (baseFrames.Count > 1)
                        for (int j = 0; j < baseFrames.Count - 1; j++)
                        {
                            if (baseFrames[j].frame <= i && baseFrames[j + 1].frame >= i
                                && baseFrames[j].Contains(id) && baseFrames[j + 1].Contains(id))
                            {
                                f1 = baseFrames[j];
                                f2 = baseFrames[j + 1];
                                break;
                            }
                        }

                    // interpolate the values
                    KeyNode n = new KeyNode();
                    n.id = id;

                    KeyNode k1 = f1.GetNodeid(id), k2 = f2.GetNodeid(id);
                    n.hash = k1.hash;

                    n.tType = k1.tType;
                    n.rType = k1.rType;
                    n.sType = k1.sType;

                    n.t.X = Lerp(k1.t.X, k2.t.X, f1.frame, f2.frame, i);
                    n.t.Y = Lerp(k1.t.Y, k2.t.Y, f1.frame, f2.frame, i);
                    n.t.Z = Lerp(k1.t.Z, k2.t.Z, f1.frame, f2.frame, i);

                    n.r.X = Lerp(k1.r.X, k2.r.X, f1.frame, f2.frame, i);
                    n.r.Y = Lerp(k1.r.Y, k2.r.Y, f1.frame, f2.frame, i);
                    n.r.Z = Lerp(k1.r.Z, k2.r.Z, f1.frame, f2.frame, i);
                    n.r.W = Lerp(k1.r.W, k2.r.W, f1.frame, f2.frame, i);

                    //n.s.X = lerp (k1.s.X, k2.s.X, f1.frame, f2.frame, i);
                    //n.s.Y = lerp (k1.s.Y, k2.s.Y, f1.frame, f2.frame, i);
                    //n.s.Z = lerp (k1.s.Z, k2.s.Z, f1.frame, f2.frame, i);


                    k.AddNode(n);
                }

            }
        }

        public static float Lerp(float av, float bv, float v0, float v1, float t)
        {
            if (v0 == v1) return av;

            float mu = (t - v0) / (v1 - v0);
            return ((av * (1 - mu)) + (bv * mu));
        }
    }
}

