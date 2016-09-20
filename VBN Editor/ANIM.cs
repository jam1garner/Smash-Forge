using System;
using System.IO;
using System.Collections.Generic;

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
					if (keys [i].input-1 <= frame && keys [i + 1].input-1 >= frame) {
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
						endTime = int.Parse (args [1]);
					else if (args [0].Equals ("startTime"))
						startTime = int.Parse (args [1]);
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
						if (att.weighted) {
							k.t1 = float.Parse (args [7]) * (float)(Math.PI / 180f);
							k.w1 = float.Parse (args [8]);
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
					if (b.name.Equals ("HipN"))
						Console.WriteLine (i + " " + n.t);
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
	}
}

