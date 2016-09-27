using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;

namespace VBN_Editor
{
	public class ANIM
	{

		private class AnimKey{
			public float input, output;
			public string intan, outtan;
			public float t1, w1;
		}

		private class AnimData{
			public string type, input, output, preInfinity, postInfinity;
			public bool weighted = false;
			public List<AnimKey> keys = new List<AnimKey>();

			public float getValue(int frame){
				AnimKey f1 = null, f2 = null;
				for (int i = 0; i < keys.Count-1; i++) {
					if ((keys [i].input-1 <= frame && keys [i + 1].input-1 >= frame)) {
						f1 = keys [i];
						f2 = keys [i + 1];
						break;
					}
				}
				if (f1 == null) {
					if (keys.Count <= 1) {
						return keys [0].output;
					} else {
						f1 = keys [keys.Count - 2];
						f2 = keys [keys.Count - 1];
					}
				}

				return CHR0.interHermite (frame+1, f1.input, f2.input, weighted ? f1.t1 : 0, weighted ? f2.t1 : 0, f1.output, f2.output);
			}
		}

		private class AnimBone{
			public string name;
			public List<AnimData> atts = new List<AnimData>();
		}

		public static SkelAnimation read(string filename, VBN vbn){
			StreamReader reader = File.OpenText(filename);
			string line;

			bool isHeader = true;

			string angularUnit, linearUnit, timeUnit;
			int startTime = 0;
			int endTime = 0;
			List<AnimBone> bones = new List<AnimBone>();
			AnimBone current;
			AnimData att = new AnimData();
			bool inKeys = false;

			while ((line = reader.ReadLine()) != null) {
				string[] args = line.Replace (";", "").TrimStart().Split (' ');

				if (isHeader) {
					if (args [0].Equals ("anim"))
						isHeader = false;
					else if (args [0].Equals ("angularUnit"))
						angularUnit = args [1];
					else if (args [0].Equals ("endTime"))
						endTime = (int)Math.Ceiling(float.Parse (args [1]));
					else if (args [0].Equals ("startTime"))
						startTime = (int)Math.Ceiling(float.Parse (args [1]));
				}

				if (!isHeader) {

					if (inKeys) {
						if(args[0].Equals("}")){
							inKeys = false;
							continue;
						}
						AnimKey k = new AnimKey ();
						att.keys.Add (k);
						k.input = float.Parse (args [0]);
						k.output = float.Parse (args [1]);
						k.intan = (args [2]);
						k.outtan = (args [3]);
                        if (args.Length > 7 && att.weighted)
                        {
                            k.t1 = float.Parse(args[7]) * (float)(Math.PI / 180f);
                            k.w1 = float.Parse(args[8]);
                        }
                    }

					if (args [0].Equals ("anim")) {
						inKeys = false;
						if (args.Length == 5) {
							//TODO: finish this type
							// can be name of attribute
						}
						if (args.Length == 7) {
							// see of the bone of this attribute exists
							current = null;
							foreach (AnimBone b in bones)
								if (b.name.Equals (args [3])) {
									current = b;
									break;
								}
							if (current == null) {
								current = new AnimBone ();
								bones.Add (current);
							}
							current.name = args [3];

							att = new AnimData ();
							att.type = args [2];
							current.atts.Add (att);

							// row child attribute aren't needed here
						}
					}

					if (args [0].Equals ("input"))
						att.input = args [1];
					if (args [0].Equals ("output"))
						att.output = args [1];
					if (args [0].Equals ("weighted"))
						att.weighted = args [1].Equals("1");
					if (args [0].Equals ("preInfinity"))
						att.preInfinity = args [1];
					if (args [0].Equals ("postInfinity"))
						att.postInfinity = args [1];

					// begining keys section
					if (args [0].Contains ("keys")) {
						inKeys = true;
					}
				}
			}

			SkelAnimation a = new SkelAnimation ();

			for (int i = 0; i < endTime - startTime; i++) {
				KeyFrame key = new KeyFrame ();
				a.addKeyframe (key);

				foreach (AnimBone b in bones) {
					KeyNode n = new KeyNode ();
					n.id = vbn.boneIndex (b.name);
					if (n.id == -1)
						continue;
					foreach (AnimData d in b.atts) {
						if (d.type.Contains ("translate")) {
							n.t_type = KeyNode.INTERPOLATED;
							if (d.type.Contains ("X"))
								n.t.X = d.getValue (i);
							if (d.type.Contains ("Y")) 
								n.t.Y = d.getValue (i);
							if (d.type.Contains ("Z"))
								n.t.Z = d.getValue (i);
						}
						if (d.type.Contains ("rotate")) {
							n.r_type = KeyNode.INTERPOLATED;
							if (d.type.Contains ("X"))
								n.r.X = d.getValue (i) * (float)(Math.PI / 180f);
							if (d.type.Contains ("Y"))
								n.r.Y = d.getValue (i) * (float)(Math.PI / 180f);
							if (d.type.Contains ("Z"))
								n.r.Z = d.getValue (i) * (float)(Math.PI / 180f);
						}
						if (d.type.Contains ("scale")) {
							n.s_type = KeyNode.INTERPOLATED;
							if (d.type.Contains ("X"))
								n.s.X = d.getValue (i);
							if (d.type.Contains ("Y")) 
								n.s.Y = d.getValue (i);
							if (d.type.Contains ("Z"))
								n.s.Z = d.getValue (i);
						}
					}
					key.addNode (n);
				}
			}

			// keynode rotations need caluclation
			foreach(KeyFrame f in a.frames){
				foreach (KeyNode n in f.nodes) {
					n.r = VBN.FromEulerAngles (n.r.Z, n.r.Y, n.r.X);
				}
			}

			return a;
		}

		public static void createANIM(string fname, SkelAnimation a, VBN vbn){
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(@fname))
			{
				file.WriteLine ("animVersion 1.1;");
				file.WriteLine ("mayaVersion 2014 x64;\ntimeUnit ntscf;\nlinearUnit cm;\nangularUnit deg;\nstartTime 1;\nendTime "+(a.size())+";");


				List<int> nodes = a.getNodes ();
				int i = 0;

				// writing node attributes
				foreach (Bone b in vbn.getBoneTreeOrder()) {
					i = vbn.boneIndex (new string(b.boneName));
						
					if (nodes.Contains (i)) {
						// write the bone attributes
						// count the attributes
						KeyNode n = a.getNode (0, i);
						int ac = 0;
						if (n.t_type != -1) {
							file.WriteLine ("anim translate.translateX translateX " + new string (b.boneName) + " 0 0 " + (ac++) + ";");
							writeKey(file, a, i, "translateX", n.t_type);
							file.WriteLine ("}");
							file.WriteLine ("anim translate.translateY translateY " + new string (b.boneName) + " 0 0 " + (ac++) + ";");
							writeKey(file, a, i, "translateY", n.t_type);
							file.WriteLine ("}");
							file.WriteLine ("anim translate.translateZ translateZ " + new string (b.boneName) + " 0 0 " + (ac++) + ";");
							writeKey(file, a, i, "translateZ", n.t_type);
							file.WriteLine ("}");
						}
						if (n.r_type != -1) {
							file.WriteLine ("anim rotate.rotateX rotateX " + new string (b.boneName) + " 0 0 " + (ac++) + ";");
							writeKey(file, a, i, "rotateX", n.r_type);
							file.WriteLine ("}");
							file.WriteLine ("anim rotate.rotateY rotateY " + new string (b.boneName) + " 0 0 " + (ac++) + ";");
							writeKey(file, a, i, "rotateY", n.r_type);
							file.WriteLine ("}");
							file.WriteLine ("anim rotate.rotateZ rotateZ " + new string (b.boneName) + " 0 0 " + (ac++) + ";");
							writeKey(file, a, i, "rotateZ", n.r_type);
							file.WriteLine ("}");
						}
						if (n.s_type != -1) {
							file.WriteLine ("anim scale.scaleX scaleX " + new string (b.boneName) + " 0 0 " + (ac++) + ";");
							writeKey(file, a, i, "scaleX", n.s_type);
							file.WriteLine ("}");
							file.WriteLine ("anim scale.scaleY scaleY " + new string (b.boneName) + " 0 0 " + (ac++) + ";");
							writeKey(file, a, i, "scaleY", n.s_type);
							file.WriteLine ("}");
							file.WriteLine ("anim scale.scaleZ scaleZ " + new string (b.boneName) + " 0 0 " + (ac++) + ";");
							writeKey(file, a, i, "scaleZ", n.s_type);
							file.WriteLine ("}");
						}
					} else {
						file.WriteLine ("anim " + new string (b.boneName) + " 0 0 0;");
					}
				}
			}
		}

		private static void writeKey(StreamWriter file, SkelAnimation a, int i, string type, int tt){

			file.WriteLine ("animData {\n input time;\n output linear;\n weighted 1;\n preInfinity constant;\n postInfinity constant;\n keys {");

			int size = a.size ();
			if (tt == KeyNode.CONSTANT) 
				size = 1;

			for (int f = 0; f < size; f++) {
				KeyNode node = a.getNode (f, i);

				float v = 0;

				switch (type) {
				case "translateX":
					v = node.t.X;
					break;
				case "translateY":
					v = node.t.Y;
					break;
				case "translateZ":
					v = node.t.Z;
					break;	
				case "rotateX":
					v = quattoeul(node.r).X * (float)(180f /  Math.PI);
					break;	
				case "rotateY":
					v = quattoeul(node.r).Y * (float)(180f /  Math.PI);
					break;	
				case "rotateZ":
					v = quattoeul(node.r).Z * (float)(180f /  Math.PI);
					break;	
				case "scaleX":
					v = node.s.X;
					break;
				case "scaleY":
					v = node.s.Y;
					break;
				case "scaleZ":
					v = node.s.Z;
					break;
				}

				file.WriteLine (" " + (f+1) + " {0:N6} fixed fixed 1 1 0 0 1 0 1;", v);
			}

			file.WriteLine (" }");
		}

		public static Vector3 quattoeul(Quaternion q){
			float sqw = q.W * q.W;
			float sqx = q.X * q.X;
			float sqy = q.Y * q.Y;
			float sqz = q.Z * q.Z;

			float normal = (float)Math.Sqrt (sqw + sqx + sqy + sqz);
			float pole_result = (q.X * q.Z) + (q.Y * q.W);

			if (pole_result > (0.5 * normal)){
				float ry = (float)Math.PI / 2;
				float rz = 0;
				float rx = 2 * (float)Math.Atan2(q.X, q.W);
				return new Vector3(rx, ry, rz);
			}
			if (pole_result < (-0.5 * normal)){
				float ry = (float)Math.PI/2;
				float rz = 0;
				float rx = -2 * (float)Math.Atan2(q.X, q.W);
				return new Vector3(rx, ry, rz);
			}

			float r11 = 2*(q.X*q.Y + q.W*q.Z);
			float r12 = sqw + sqx - sqy - sqz;
			float r21 = -2*(q.X*q.Z - q.W*q.Y);
			float r31 = 2*(q.Y*q.Z + q.W*q.X);
			float r32 = sqw - sqx - sqy + sqz;

			float frx = (float)Math.Atan2( r31, r32 );
			float fry = (float)Math.Asin ( r21 );
			float frz = (float)Math.Atan2( r11, r12 );
			return new Vector3(frx, fry, frz);
		}
	}
}

