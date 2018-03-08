using System;
using OpenTK;
using System.IO;

namespace Smash_Forge
{
	public class CHR0
	{

		public static Animation read(FileData d, VBN m){
			d.Endian = Endianness.Big;
			d.seek(0x8);

			int versionNum = d.readInt();

			d.seek(0x10);
			if(versionNum == 4){
				return readAnim(d,m);
			}

			return null;
		}

		public static Animation readAnim(FileData d , VBN m){

			int offset = d.readInt();
			int nameoff = d.readInt();

			d.skip(4);
			int fCount = d.readShort();
			int animDataCount =d.readShort();
			d.skip(8);

			Animation anim = new Animation(d.readString(nameoff, -1));
            anim.FrameCount = fCount;
            
			//anim.setModel(m);

			d.seek(offset);
			int sectionOffset = d.readInt() + offset;
			int size = d.readInt(); // size again 

            for (int i = 0; i < size ; i++){
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

                Animation.KeyNode node = new Animation.KeyNode(d.readString(nameOff, -1));
                anim.Bones.Add(node);
                node.RotType = Animation.RotationType.EULER;

                if (hasS == 1){
					if(Siso == 1){
						float iss = d.readFloat();
                        node.XSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = iss});
                        node.YSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = iss });
                        node.ZSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = iss });
                    }
					else{
                        
						if(SXfixed == 1) node.XSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = d.readFloat() }); else process(d, s_type, pos, node, "SX", false, anim);
						if(SYfixed == 1) node.YSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = d.readFloat() }); else process(d, s_type, pos, node, "SY", false, anim);
						if(SZfixed == 1) node.ZSCA.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = d.readFloat() }); else process(d, s_type, pos, node, "SZ", false, anim);
					}
				}

				if(hasR == 1){
					if(Riso == 1){
						float iss = (float)((d.readFloat ()) * Math.PI / 180f);
                        node.XROT.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = iss });
                        node.YROT.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = iss });
                        node.ZROT.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = iss });
                    }
					else{
                        if (RXfixed == 1) node.XROT.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = (float)(Math.PI / 180f) * (d.readFloat()) }); else process(d, r_type, pos, node, "RX", false, anim);
                        if (RYfixed == 1) node.YROT.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = (float)(Math.PI / 180f) * (d.readFloat()) }); else process(d, r_type, pos, node, "RY", false, anim);
                        if (RZfixed == 1) node.ZROT.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = (float)(Math.PI / 180f) * (d.readFloat()) }); else process(d, r_type, pos, node, "RZ", false, anim);
                    }
				}

				if(hasT == 1){
					if(Tiso == 1){
                        float iss = d.readFloat();
                        node.XPOS.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = iss });
                        node.YPOS.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = iss });
                        node.ZPOS.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = iss });
                    }
					else{
                        if (Xfixed == 1) node.XPOS.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = d.readFloat() }); else process(d, t_type, pos, node, "X", false, anim);
                        if (Yfixed == 1) node.YPOS.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = d.readFloat() }); else process(d, t_type, pos, node, "Y", false, anim);
                        if (Zfixed == 1) node.ZPOS.Keys.Add(new Animation.KeyFrame() { Frame = 0, Value = d.readFloat() }); else process(d, t_type, pos, node, "Z", false, anim);
                    }
				}

				d.seek(temp);
			}

			return anim;
		}

		public static void process(FileData d, int type, int secOff, Animation.KeyNode node, String part, bool debug, Animation a){

			int offset = d.readInt() + secOff;
			int temp = d.pos();
			d.seek(offset);

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

            //a.FrameCount = max;

            float degrad = (float)(Math.PI / 180f);
            if (frame != null)
            {
                for(int i = 0; i < fCount; i++)
                {
                    Animation.KeyFrame f = new Animation.KeyFrame();
                    f.InterType = Animation.InterpolationType.HERMITE;
                    f.Value = step[i];
                    f.Frame = frame[i];
                    f.In = tan[i];
                    switch (part)
                    {
                        case "RX":
                            f.Value = step[i] * degrad;
                            node.XROT.Keys.Add(f);
                            f.Degrees = true;
                            break;
                        case "RY":
                            f.Value = step[i] * degrad;
                            node.YROT.Keys.Add(f);
                            f.Degrees = true;
                            break;
                        case "RZ":
                            f.Value = step[i] * degrad;
                            node.ZROT.Keys.Add(f);
                            f.Degrees = true;
                            break;
                        case "X":
                            node.XPOS.Keys.Add(f);
                            break;
                        case "Y":
                            node.YPOS.Keys.Add(f);
                            break;
                        case "Z":
                            node.ZPOS.Keys.Add(f);
                            break;
                        case "SX":
                            node.XSCA.Keys.Add(f);
                            break;
                        case "SY":
                            node.YSCA.Keys.Add(f);
                            break;
                        case "SZ":
                            node.ZSCA.Keys.Add(f);
                            break;
                    }
                }
            }

			if(type == 0x4)
            {
                float stepb = d.readFloat();
				float base2 = d.readFloat();
				for(int i = 0; i < a.FrameCount; i++){

					float v = base2 + stepb * (d.readByte());

                    Animation.KeyFrame f = new Animation.KeyFrame();
                    f.InterType = Animation.InterpolationType.LINEAR;
                    f.Value = v;
                    f.Frame = i;

                    switch (part)
                    {
                        case "RX":
                            f.Value = v * degrad;
                            node.XROT.Keys.Add(f);
                            break;
                        case "RY":
                            f.Value = v * degrad;
                            node.YROT.Keys.Add(f);
                            break;
                        case "RZ":
                            f.Value = v * degrad;
                            node.ZROT.Keys.Add(f);
                            break;
                        case "X":
                            node.XPOS.Keys.Add(f);
                            break;
                        case "Y":
                            node.YPOS.Keys.Add(f);
                            break;
                        case "Z":
                            node.ZPOS.Keys.Add(f);
                            break;
                        case "SX":
                            node.XSCA.Keys.Add(f);
                            break;
                        case "SY":
                            node.YSCA.Keys.Add(f);
                            break;
                        case "SZ":
                            node.ZSCA.Keys.Add(f);
                            break;
                    }
                }
			}

			if(type == 0x6){
				for(int i = 0; i < a.FrameCount; i++){

					float v = d.readFloat();

                    Animation.KeyFrame f = new Animation.KeyFrame();
                    f.InterType = Animation.InterpolationType.LINEAR;
                    f.Value = v;
                    f.Frame = i;
                    switch (part)
                    {
                        case "RX":
                            f.Value = v * degrad;
                            node.XROT.Keys.Add(f);
                            break;
                        case "RY":
                            f.Value = v * degrad;
                            node.YROT.Keys.Add(f);
                            break;
                        case "RZ":
                            f.Value = v * degrad;
                            node.ZROT.Keys.Add(f);
                            break;
                        case "X":
                            node.XPOS.Keys.Add(f);
                            break;
                        case "Y":
                            node.YPOS.Keys.Add(f);
                            break;
                        case "Z":
                            node.ZPOS.Keys.Add(f);
                            break;
                        case "SX":
                            node.XSCA.Keys.Add(f);
                            break;
                        case "SY":
                            node.YSCA.Keys.Add(f);
                            break;
                        case "SZ":
                            node.ZSCA.Keys.Add(f);
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

