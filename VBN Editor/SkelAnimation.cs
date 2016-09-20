using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;

namespace VBN_Editor
{
	public class KeyFrame {
		public List<KeyNode> nodes = new List<KeyNode>();
		public int frame = 0;

		public void addNode(KeyNode n){
			nodes.Add (n);
		}
			
		public bool contains(int id){
			for(int j = 0; j < nodes.Count ; j++){
				if(nodes[j].id == id)
					return true;
			}
			return false;
		}

		public KeyNode getNodeid(int id){
			for(int j = 0; j < nodes.Count ; j++){
				if(nodes[j].id == id)
					return nodes[j];
			}
			return null;
		}

		public KeyNode getNode(int i){
			for(int j = 0; j < nodes.Count ; j++){
				if(nodes[j].id == i)
					return nodes[j];
			}

			KeyNode ne = new KeyNode();
			ne.id = i;

			addNode(ne);

			return ne;
		}

	}

	public class KeyNode {
		public const int INTERPOLATED = 0;
		public const int CONSTANT = 1;

		public int id;
		public int t_type, r_type, s_type;
		public Vector3 t, s;
		public Quaternion r;

		public Vector3 t2, s2, rv, rv2;
		public Quaternion r2;

		public KeyNode(){
			t_type = -1;
			r_type = -1;
			s_type = -1;
		}
	}

	public class SkelAnimation
	{

		public List<KeyFrame> frames = new List<KeyFrame>();
		private int frame = 0;

		public SkelAnimation ()
		{

		}

		public void addKeyframe(KeyFrame k){
			frames.Add (k);
		}

		public List<int> getNodes(){
			List<int> node = new List<int>();

			foreach(KeyFrame f in frames)
				foreach(KeyNode n in f.nodes)
					if(!node.Contains(n.id) && n.id != -1){
						node.Add(n.id);
					}

			return node;
		}

		public void setFrame(int i){
			frame = i;
		}

		public int getFrame(){
			return frame;
		}

		public int size(){
			return frames.Count;
		}

		public void nextFrame(VBN vbn){

			KeyFrame key = frames [frame];

			foreach (KeyNode n in key.nodes) {
				if (n.id == -1)
					continue;
				Bone b = vbn.bones [n.id];

				if (n.t_type != -1) {
					b.pos = n.t;
				}
				if (n.r_type != -1) {
					b.rot = n.r;
				}
				if (n.s_type != -1) {
					//b.sca = n.s;
				}
			}

			vbn.update ();

			frame++;
			if (frame >= frames.Count)
				frame = 0;

		}	


		public KeyNode getFirstNode(int nodeIndex){
			foreach(KeyFrame k in frames)
				if(k.contains(nodeIndex)){
					return k.getNodeid(nodeIndex);
				}

			return null;
		}


		public int getNodeIndex(string n, VBN vbn){
			for(int i = 0 ;i < vbn.bones.Count ; i++){
				if(new string(vbn.bones[i].boneName).Equals(n)){
					return i;
				}
			}
			return -1;
		}

		public KeyNode getNode(int frame, int nodeIndex){
			while(frames.Count <= frame)
				frames.Add(new KeyFrame());

			KeyFrame f = frames[frame];

			return f.getNode(nodeIndex);
		}


		public float getBaseNodeValue(int nid, String type, VBN vbn){
			switch(type){
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



		public void nextFrameNoRender(VBN vbn){

			KeyFrame key = frames [frame];

			foreach (KeyNode n in key.nodes) {
				if (n.id == -1)
					continue;
				Bone b = vbn.bones [n.id];

				if (n.t_type != -1) {
					b.pos = n.t;
				}
				if (n.r_type != -1) {
					b.rot = n.r;
				}
				if (n.s_type != -1) {
					//b.sca = n.s;
				}
			}

			frame++;
			if (frame >= frames.Count)
				frame = 0;

		}

	}
}

