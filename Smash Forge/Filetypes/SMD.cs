using System;
using System.IO;
using System.Text.RegularExpressions;
using OpenTK;

namespace Smash_Forge
{
	public class SMD
	{

		public static void read(string fname, SkelAnimation a, VBN v){
			StreamReader reader = File.OpenText(fname);
			string line;

			string current = "";
			bool readBones = false;
			int frame = 0, prevframe = 0;
			KeyFrame k = new KeyFrame();

			VBN vbn = v;
			if (v.bones.Count == 0) {
				readBones = true;
			} else
				vbn = new VBN ();

			while ((line = reader.ReadLine ()) != null) {
				line = Regex.Replace (line, @"\s+", " ");
				string[] args = line.Replace (";", "").TrimStart().Split (' ');

				if (args [0].Equals ("nodes") || args [0].Equals ("skeleton") || args [0].Equals ("end") || args [0].Equals ("time")) {
					current = args [0];
					if (args.Length > 1){
						prevframe = frame;
						frame = int.Parse (args [1]);

						/*if (frame != prevframe + 1) {
							Console.WriteLine ("Needs interpolation " + frame);
						}*/

						k = new KeyFrame ();
						k.frame = frame;
						a.addKeyframe (k);
					}
					continue;
				}

				if (current.Equals ("nodes")) {
					Bone b = new Bone ();
					b.boneName = args [1].Replace ("\"", "").ToCharArray();
					b.parentIndex = int.Parse(args [2]);
					b.children = new System.Collections.Generic.List<int> ();
					vbn.totalBoneCount++;
					vbn.bones.Add (b);
				}

				if (current.Equals ("time")) {
					KeyNode n = new KeyNode ();
					n.id = v.boneIndex (new string(vbn.bones[int.Parse(args[0])].boneName));
					if (n.id == -1) {
						continue;
					}

					// only if it finds the node
					k.addNode (n);

					// reading the skeleton if this isn't an animation
					if (readBones && frame == 0) {
						Bone b = vbn.bones [n.id];
						b.position = new float[3];
						b.rotation = new float[3];
						b.scale = new float[3];
						b.position [0] = float.Parse (args[1]);
						b.position [1] = float.Parse (args[2]);
						b.position [2] = float.Parse (args[3]);
						b.rotation [0] = float.Parse (args[4]);
						b.rotation [1] = float.Parse (args[5]);
						b.rotation [2] = float.Parse (args[6]);
						b.scale [0] = 1f;
						b.scale [1] = 1f;
						b.scale [2] = 1f;

						b.pos = new Vector3 (float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
						b.rot = VBN.FromEulerAngles (float.Parse(args[6]), float.Parse(args[5]), float.Parse(args[4]));

						if(b.parentIndex!=-1)
							vbn.bones [b.parentIndex].children.Add (int.Parse(args[0]));
					}

					n.t_type = KeyNode.INTERPOLATED;
					n.t = new Vector3 (float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
					n.r_type = KeyNode.INTERPOLATED;
					n.r = VBN.FromEulerAngles (float.Parse(args[6]), float.Parse(args[5]), float.Parse(args[4]));
				}
			}

			v.boneCountPerType [0] = (uint)vbn.bones.Count;
			v.update ();
			a.bakeFramesLinear ();
		}

	}
}

