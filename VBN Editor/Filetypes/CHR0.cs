using System;
using OpenTK;

namespace VBN_Editor
{
	public class CHR0
	{

		public static SkelAnimation read(FileData d, VBN m){
			d.littleEndian = false;
			d.seek(0x8);

			int versionNum = d.readInt();

			d.seek(0x10);
			if(versionNum == 4){
				return readAnim(d,m);
			}

			return null;
		}

		public static SkelAnimation readAnim(FileData d , VBN m){

			int offset = d.readInt();
			int nameoff = d.readInt();

			d.skip(4);
			int fCount = d.readShort();
			int animDataCount =d.readShort();
			d.skip(8);

			SkelAnimation anim = new SkelAnimation();
			//anim.setModel(m);

			d.seek(offset);
			int sectionOffset = d.readInt() + offset;
			int size = d.readInt(); // size again 

			for(int i = 0; i < size ; i++){
				//			System.out.print(d.readShort()); // id
				d.skip(4); // id and unknown
				d.readShort(); //left
				d.readShort(); //right
				int nameOffset = d.readInt() + offset;
				int dataOffset = d.readInt() + offset;
				if(dataOffset == offset){
					i--;
					continue;
					//				d.skip(8);
					//				nameOffset = d.readInt() + 4;
					//				dataOffset = d.readInt() + offset;
				}


				int temp = d.pos();

				d.seek(dataOffset);

				int pos = d.pos();
				int nameOff = d.readInt() + sectionOffset + (d.pos() - sectionOffset) - 4;
				int flags = d.readInt();

				int t_type = (flags>>0x1e)&0x3;
				int r_type = (flags>>0x1b)&0x7;
				int s_type = (flags>>0x19)&0x3;

				int hasT = (flags>>0x18)&0x1;
				int hasR = (flags>>0x17)&0x1;
				int hasS = (flags>>0x16)&0x1;

				int Zfixed = (flags>>0x15)&0x1;
				int Yfixed = (flags>>0x14)&0x1;
				int Xfixed = (flags>>0x13)&0x1;

				int RZfixed = (flags>>0x12)&0x1;
				int RYfixed = (flags>>0x11)&0x1;
				int RXfixed = (flags>>0x10)&0x1;

				int SZfixed = (flags>>0xf)&0x1;
				int SYfixed = (flags>>0xe)&0x1;
				int SXfixed = (flags>>0xd)&0x1;

				int Tiso = (flags>>0x6)&0x1;
				int Riso = (flags>>0x5)&0x1;
				int Siso = (flags>>0x4)&0x1;


				if(hasS == 1){
					if(Siso == 1){
						//System.out.println("S is ISO");

						int nid = anim.getNodeIndex(d.readString(nameOff, -1), m);
						KeyNode node = anim.getNode(0, nid);
						node.s_type = 1;
						float iss = d.readFloat();
						node.s = new OpenTK.Vector3(iss, iss, iss);
					}
					else{
						int nid = anim.getNodeIndex(d.readString(nameOff, -1), m);
						KeyNode node = anim.getNode(0, nid);
						node.s_type = 1;

						//					System.out.println("Scale: " + SXfixed + " " + SYfixed + " " + SZfixed + " " + s_type);
						node.s = new OpenTK.Vector3(-99,-99,-99);
						if(SXfixed == 1) node.s.X = d.readFloat(); else process(d, s_type, pos, anim, "SX", nid,false, m);
						if(SYfixed == 1) node.s.Y = d.readFloat(); else process(d, s_type, pos, anim, "SY", nid,false, m);
						if(SZfixed == 1) node.s.Z = d.readFloat(); else process(d, s_type, pos, anim, "SZ", nid,false, m);
					}
				}

				if(hasR == 1){
					if(Riso == 1){
						//System.out.println("R is ISO");

						int nid = anim.getNodeIndex(d.readString(nameOff, -1), m);
						KeyNode node = anim.getNode(0, nid);
						node.r_type = 1;
						float iss = (float)((d.readFloat ()) * Math.PI / 180f);
						node.r = VBN.FromEulerAngles(iss, iss, iss);
					}
					else{

						int nid = anim.getNodeIndex(d.readString(nameOff, -1), m);
						KeyNode node = anim.getNode(0, nid);

						//				System.out.println("Rot: " + RXfixed + " " + RYfixed + " " + RZfixed);

						node.r = new OpenTK.Quaternion(-99,-99,-99,0);

						if(RXfixed == 1) node.r.X = (float) (Math.PI / 180f) * (d.readFloat()); else process(d, r_type, pos, anim, "RX", nid, false, m);
						if(RYfixed == 1) node.r.Y = (float) (Math.PI / 180f) * (d.readFloat()); else process(d, r_type, pos, anim, "RY", nid, false, m);
						if(RZfixed == 1) node.r.Z = (float) (Math.PI / 180f) * (d.readFloat()); else process(d, r_type, pos, anim, "RZ", nid, false, m);
					}
				}

				if(hasT == 1){
					if(Tiso == 1){
						//System.out.println("T is ISO");

						int nid = anim.getNodeIndex(d.readString(nameOff, -1), m);
						KeyNode node = anim.getNode(0, nid);
						node.t_type = 1;
						float iss = d.readFloat();
						node.t = new OpenTK.Vector3(iss, iss, iss);
					}
					else{
						int nid = anim.getNodeIndex(d.readString(nameOff, -1), m);
						KeyNode node = anim.getNode(0, nid);
						node.t_type = 1;

						//					System.out.println("Trans: " + Xfixed + " " + Yfixed + " " + Zfixed);

						node.t = new OpenTK.Vector3(-99,-99,-99);
						if(Xfixed == 1) node.t.X = d.readFloat(); else process(d, t_type, pos, anim, "X", nid,false, m);
						if(Yfixed == 1) node.t.Y = d.readFloat(); else process(d, t_type, pos, anim, "Y", nid,false, m);
						if(Zfixed == 1) node.t.Z = d.readFloat(); else process(d, t_type, pos, anim, "Z", nid,false, m);
					}
				}

				d.seek(temp);
			}

			// keynode rotations need caluclation
			foreach(KeyFrame f in anim.frames){
				foreach (KeyNode n in f.nodes) {
					n.r = VBN.FromEulerAngles (n.r.Z, n.r.Y, n.r.X);
				}
			}
			//anim.calcMax();

			return anim;
		}

		public static void process(FileData d, int type, int secOff, SkelAnimation anim, String part, int nid, bool debug, VBN vbn){

			int offset = d.readInt() + secOff;
			int temp = d.pos();
			d.seek(offset);
			//		System.out.println(d.pos());

			int max = 0;
			int fCount = -1;
			float scale = 0;
			float[] frame = null, step = null, tan = null;

			if(type == 0x1){

				fCount = d.readShort();
				d.skip(2);
				scale = d.readFloat();
				float stepb = d.readFloat();
				float base2 = d.readFloat();

				frame = new float[fCount];
				step = new float[fCount];
				tan = new float[fCount];

				for(int i = 0; i < fCount ; i++){
					frame[i] = d.readByte();
					int th = d.readThree();
					step[i] = base2 + ((th>>12) & 0xfff) * stepb;
					tan[i] = (FileData.sign12Bit(th & 0xfff) / 32f);

					if(frame[i] > max){
						max = (int)frame[i];
					}
				}
			}

			if(type == 0x2){
				//if(debug)
					//System.out.println(part + "\tInterpolated 6\t" + Integer.toHexString(offset));

				fCount = d.readShort();
				d.skip(2);
				scale = d.readFloat();
				float stepb = d.readFloat();
				float base2 = d.readFloat();

				frame = new float[fCount];
				step = new float[fCount];
				tan = new float[fCount];

				for(int i = 0; i < fCount ; i++){
					frame[i] = d.readShort() / 32f;
					step[i] = base2 + d.readShort() * stepb;
					tan[i] = ((short)d.readShort() / 256f);

					if(frame[i] > max){
						max = (int)frame[i];
					}
				}
			}

			if(type == 0x3){
				//if(debug)
					//System.out.println(part + "\tInterpolated 12 " + Integer.toHexString(offset));

				fCount = d.readShort();
				d.skip(2);
				scale = d.readFloat();

				frame = new float[fCount];
				step = new float[fCount];
				tan = new float[fCount];

				for(int i = 0; i < fCount ; i++){
					frame[i] = d.readFloat();
					step[i] = d.readFloat();
					tan[i] = d.readFloat();

					if(frame[i] > max){
						max = (int)frame[i];
					}
				}
			}

			if(frame != null){
				generateInter(anim, max, nid, part, frame, tan, step, vbn);
			}

			if(type == 0x4){

				//if(debug)
					//System.out.println(part + "\tLkin 1 " + Integer.toHexString(offset) + " " + anim.size());
				float stepb = d.readFloat();
				float base2 = d.readFloat();
				for(int i = 0; i < anim.size() ; i++){
					KeyNode n = anim.getNode(i, nid);

					if(part.Contains("R"))
						n.r_type = 1;
					else
						if(part.Contains("S"))
							n.s_type = 1;
						else
							n.t_type = 1;

					float v = base2 + stepb * (d.readByte());
					//				float f = d.readFloat();
					//				System.out.println(stepb + " " + base + " " + (byte)d.readByte());

					switch(part){
					case "RX":
						n.r.X = (float)(Math.PI/180f) * (v);
						break;
					case "RY":
						n.r.Y = (float)(Math.PI/180f) * (v);
						break;
					case "RZ":
						n.r.Z = (float) (Math.PI/180f) * (v);
						break;
					case "X":
						n.t.X = v;
						break;
					case "Y":
						n.t.Y = v;
						break;
					case "Z":
						n.t.Z = v;
						break;
					case "SX":
						n.s.X = v;
						break;
					case "SY":
						n.s.Y = v;
						break;
					case "SZ":
						n.s.Z = v;
						break;
					}
				}

				//			System.out.println(d.pos());
			}

			if(type == 0x6){
				//if(debug)
					//System.out.println(part + "\tLin 4");
				for(int i = 0; i < anim.size() ; i++){
					KeyNode n = anim.getNode(i, nid);

					if(part.Contains("R"))
						n.r_type = 1;
					else
						if(part.Contains("S"))
							n.s_type = 1;
						else
							n.t_type = 1;

					float v = d.readFloat();

					switch(part){
					case "RX":
						n.r.X = (float) (Math.PI / 180) * (v);
						break;
					case "RY":
						n.r.Y = (float) (Math.PI / 180) * (v);
						break;
					case "RZ":
						n.r.Z = (float) (Math.PI / 180) * (v);
						break;
					case "X":
						n.t.X = v;
						break;
					case "Y":
						n.t.Y = v;
						break;
					case "Z":
						n.t.Z = v;
						break;
					case "SX":
						n.s.X = v;
						break;
					case "SY":
						n.s.Y = v;
						break;
					case "SZ":
						n.s.Z = v;
						break;
					}
				}
			}

			d.seek(temp);
		}

		public static void generateInter(SkelAnimation anim, int max, int nid, String part, float[] frame, float[] tan, float[] step, VBN vbn){
			int in2 = 0;
			int out2 = 1;

			float degrad = (float) (Math.PI / 180f);

			for(int i = 0; i < max ; i++){
				KeyNode n = anim.getNode(i, nid);
				if(part.Contains("R"))
					n.r_type = 1;
				else
					if(part.Contains("S"))
						n.s_type = 1;
					else
						n.t_type = 1;

				if(i > frame[out2]){
					in2++;
					out2++;
				}
				float inv = frame[in2];
				float tanin = tan[in2];
				float stepin = step[in2];

				if(frame[0] > i){
					inv = 0;
					tanin = 0;
					stepin = anim.getBaseNodeValue(nid, part, vbn);
					out2 = 0;
					in2 = 0;
				}

				if(frame[0] == i && out2 == 0){
					out2 = 1;
					in2 = 0;
				}

				switch(part){
				case "RX":
					n.r.X = interHermite(i, inv, frame[out2], tanin, tan[out2], stepin, step[out2]) * degrad;
					break;
				case "RY":
					n.r.Y = interHermite(i, inv, frame[out2], tanin, tan[out2], stepin, step[out2]) * degrad;
					break;
				case "RZ":
					n.r.Z = interHermite(i, inv, frame[out2], tanin, tan[out2], stepin, step[out2]) * degrad;
					break;
				case "X":
					n.t.X = interHermite(i, inv, frame[out2], tanin, tan[out2], stepin, step[out2]);
					break;
				case "Y":
					n.t.Y = interHermite(i, inv, frame[out2], tanin, tan[out2], stepin, step[out2]);
					break;
				case "Z":
					n.t.Z = interHermite(i, inv, frame[out2], tanin, tan[out2], stepin, step[out2]);
					break;
				case "SX":
					n.s.X = interHermite(i, inv, frame[out2], tanin, tan[out2], stepin, step[out2]);
					break;
				case "SY":
					n.s.Y = interHermite(i, inv, frame[out2], tanin, tan[out2], stepin, step[out2]);
					break;
				case "SZ":
					n.s.Z = interHermite(i, inv, frame[out2], tanin, tan[out2], stepin, step[out2]);
					break;
				}
			}
		}

		public static float interHermite(float frame, float frame1, float frame2, float outslope, float inslope, float val1, float val2){
			float distance = frame - frame1;
			float invDuration = 1f / (frame2 - frame1);
			float t = distance * invDuration;
			float t1 = t - 1f;
			return (val1 + ((((val1 - val2) * ((2f * t) - 3f)) * t) * t)) + ((distance * t1) * ((t1 * outslope) + (t * inslope)));
		}

		public static float lerp(float av, float bv, float v0, float v1, float t) {
			if (v0 == v1) return av;

			float mu = (t - v0) / (v1 - v0);
			return ((av * (1 - mu)) + (bv * mu));
		}

	}
}

