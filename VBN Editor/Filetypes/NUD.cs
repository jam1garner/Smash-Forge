using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace VBN_Editor
{
	public class NUD
	{

		public class Vertex{
			public Vector3 pos = new Vector3(0,0,0), nrm = new Vector3(0,0,0);
			public Vector3 col = new Vector3(1, 1, 1);
			public Vector2 tx = new Vector2(0,0);
			public List<int> node = new List<int>();
			public List<float> weight = new List<float>();

			public Vertex(){
			}

			public Vertex(float x, float y, float z){
				pos = new Vector3(x, y, z);
			}
		}

		public class Polygon{
			public List<Vertex> vertices = new List<Vertex>();
			public List<int> faces = new List<int>();

			// Material
			public int dif = -1;
			public bool isVisible = true;

			// for nud stuff
			public int vertSize = 0x46; // defaults to a basic bone weighted vertex format

			public void AddVertex(Vertex v){
				vertices.Add (v);
			}
		}

		// typically a mesh will just have 1 polygon
		// but you can just use the mesh class without polygons
		public class Mesh{
			public string name;
			public List<Polygon> polygons = new List<Polygon>();

			public void addVertex (Vertex v){
				if (polygons.Count == 0) 
					polygons.Add (new Polygon());

				polygons [0].AddVertex (v);
			}
		}


		// gl buffer objects
		int vbo_position;
		int vbo_color;
		int vbo_nrm;
		int vbo_uv;
		int vbo_weight;
		int vbo_bone;
		int ibo_elements;

		Vector2[] uvdata;
		Vector3[] vertdata, coldata, nrmdata;
		int[] facedata;
		Vector4[] bonedata, weightdata;

		public const int SMASH = 0;
		public const int POKKEN = 1;
		public int type = SMASH;

		public List<Mesh> mesh = new List<Mesh>();
		public NUT nut;
		int[] textureIds;

		public NUD (string fname)
		{
			nut = new NUT (new FileData("C:\\s\\Smash\\extract\\data\\fighter\\bayonetta\\model.nut"));

			GL.GenBuffers(1, out vbo_position);
			GL.GenBuffers(1, out vbo_color);
			GL.GenBuffers(1, out vbo_nrm);
			GL.GenBuffers(1, out vbo_uv);
			GL.GenBuffers(1, out vbo_bone);
			GL.GenBuffers(1, out vbo_weight);
			GL.GenBuffers(1, out ibo_elements);

			read (new FileData(fname));

			PreRender ();
		}

		public void Destroy (){
			GL.DeleteBuffer (vbo_position);
			GL.DeleteBuffer (vbo_color);
			GL.DeleteBuffer (vbo_nrm);
			GL.DeleteBuffer (vbo_uv);
			GL.DeleteBuffer (vbo_weight);
			GL.DeleteBuffer (vbo_bone);
		}

		/*
		 * Not sure if update is needed here
		*/
		private void PreRender(){
			List<Vector3> vert = new List<Vector3> ();
			List<Vector2> uv = new List<Vector2> ();
			List<Vector3> col = new List<Vector3> ();
			List<Vector3> nrm = new List<Vector3> ();
			List<Vector4> bone = new List<Vector4> ();
			List<Vector4> weight = new List<Vector4> ();
			List<int> face = new List<int> ();

			int i = 0;

			foreach(Mesh m in mesh){
				foreach(Polygon p in m.polygons){
					if (p.faces.Count <= 3)
						continue;
					foreach(Vertex v in p.vertices){
						vert.Add (v.pos);
						col.Add (v.col);
						nrm.Add (v.nrm);
						uv.Add (v.tx);
						while (v.node.Count < 4) {
							v.node.Add (0);
							v.weight.Add (0);
						}
						bone.Add (new Vector4(v.node[0], v.node[1], v.node[2], v.node[3]));
						weight.Add (new Vector4(v.weight[0], v.weight[1], v.weight[2], v.weight[3]));
					}

					// rearrange faces
					int[] ia = p.faces.ToArray ();
					for (int j = 0; j < ia.Length; j++) {
						ia [j] += i;
					}
					face.AddRange (ia);
					i += p.vertices.Count;
				}
			}

			vertdata = vert.ToArray ();
			coldata = col.ToArray ();
			nrmdata = nrm.ToArray ();
			uvdata = uv.ToArray ();
			facedata = face.ToArray ();
			bonedata = bone.ToArray ();
			weightdata = weight.ToArray ();

		}

		public void Render(Shader shader){

			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
			GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
			GL.VertexAttribPointer(shader.getAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);
			GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
			GL.VertexAttribPointer(shader.getAttribute("vColor"), 3, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_nrm);
			GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(nrmdata.Length * Vector3.SizeInBytes), nrmdata, BufferUsageHint.StaticDraw);
			GL.VertexAttribPointer(shader.getAttribute("vNormal"), 3, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_uv);
			GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(uvdata.Length * Vector2.SizeInBytes), uvdata, BufferUsageHint.StaticDraw);
			GL.VertexAttribPointer(shader.getAttribute("vUV"), 2, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_bone);
			GL.BufferData<Vector4>(BufferTarget.ArrayBuffer, (IntPtr)(bonedata.Length * Vector4.SizeInBytes), bonedata, BufferUsageHint.StaticDraw);
			GL.VertexAttribPointer(shader.getAttribute("vBone"), 4, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_weight);
			GL.BufferData<Vector4>(BufferTarget.ArrayBuffer, (IntPtr)(weightdata.Length * Vector4.SizeInBytes), weightdata, BufferUsageHint.StaticDraw);
			GL.VertexAttribPointer(shader.getAttribute("vWeight"), 4, VertexAttribPointerType.Float, false, 0, 0);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(facedata.Length * sizeof(int)), facedata, BufferUsageHint.StaticDraw);


			mesh [0].polygons [0].isVisible = false;


			int indiceat = 0;
			foreach (Mesh m in mesh) {
				foreach (Polygon p in m.polygons) {
					if (p.faces.Count <= 3)
						continue;

					if (p.dif != -1) {
						GL.BindTexture(TextureTarget.Texture2D, p.dif);
						GL.Uniform1(shader.getAttribute("tex"), 0);
					}

					if(p.isVisible)
						GL.DrawElements (BeginMode.Triangles, p.faces.Count, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
					indiceat += p.faces.Count;
				}
			}

			//GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
		}



		//----------------------------------------------------------
		// texture
		public static int loadImage(Bitmap image)
		{
			int texID = GL.GenTexture();

			GL.BindTexture(TextureTarget.Texture2D, texID);
			BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
				ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

			image.UnlockBits(data);

			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

			return texID;
		}


		//------------------------------------------------------------------------------------------------------------------------
		/*
		 * Reads the contents of the nud file into this class
		 * Not all info will be saved, so the file will be different on export
		 */
		//------------------------------------------------------------------------------------------------------------------------
		public void read(FileData d){
			d.Endian = System.IO.Endianness.Big;
			d.seek (0);

			// read header
			d.seek (10); // magic filesize and unknown short
			int polysets = d.readShort();
			d.skip(2); // unknown
			d.skip(2);	// somethingsets
			int polyClumpStart = d.readInt() + 0x30;
			int polyClumpSize = d.readInt();
			int vertClumpStart = polyClumpStart + polyClumpSize;
			int vertClumpSize = d.readInt();
			int vertaddClumpStart = vertClumpStart + vertClumpSize;
			int vertaddClumpSize = d.readInt();
			int nameStart = vertaddClumpStart + vertaddClumpSize;
			d.skip (16); // some floats

			// object descriptors

			Object[] obj = new Object[polysets];
			for(int i = 0 ; i < polysets ; i++){
				d.skip (32);
				int temp = d.pos() + 4;
				d.seek (nameStart + d.readInt());
				obj[i].name = (d.readString());
				// read name string
				d.seek (temp);
				obj [i].id = d.readInt ();
				obj [i].singlebind = d.readShort ();
				obj [i].polyamt = d.readShort ();
				obj [i].positionb = d.readInt ();
			}

			// reading polygon data
			foreach(var o in obj){

				Mesh m = new Mesh();
				m.name = o.name;
				mesh.Add (m);

				for (int i = 0; i < o.polyamt; i++) {
					Poly p = new Poly ();

					p.polyStart = d.readInt () + polyClumpStart;
					p.vertStart = d.readInt () + vertClumpStart;
					p.verAddStart = d.readInt () + vertaddClumpStart;
					p.vertamt = d.readShort ();
					p.vertSize = d.readByte ();
					p.UVSize = d.readByte ();
					p.texprop1 = d.readInt ();
					p.texprop2 = d.readInt ();
					p.texprop3 = d.readInt ();
					p.texprop4 = d.readInt ();
					p.polyamt = d.readShort ();
					p.polsize = d.readByte ();
					p.polflag = d.readByte ();
					d.skip (0xC);

					int temp = d.pos ();

					// read vertex
					m.polygons.Add(readVertex (d, p, o));

					// temp tex id;
					d.seek(p.texprop1 + 0x20);
					int texid = -1;
					nut.draw.TryGetValue (d.readInt(), out texid);
					m.polygons [m.polygons.Count - 1].dif = texid;

					d.seek (temp);
				}
			}
		}

		// HELPERS FOR READING
		/*private struct header{
			char[] magic;
			public int fileSize;
			public short unknown;
			public int polySetCount;
		}*/
		private struct Object{
			public int id;
			//public int polynamestart;
			public int singlebind;
			public int polyamt;
			public int positionb;
			public string name;
		}

		private struct Poly{
			public int polyStart;
			public int vertStart;
			public int verAddStart;
			public int vertamt;
			public int vertSize;
			public int UVSize;
			public int polyamt;
			public int polsize;
			public int polflag;
			public int texprop1;
			public int texprop2;
			public int texprop3;
			public int texprop4;
		}


		//VERTEX TYPES----------------------------------------------------------------------------------------

		private static Polygon readVertex(FileData d, NUD.Poly p, NUD.Object o){

			Polygon m = new Polygon();

			readVertex (d, p, o, m, p.vertSize);

			// faces
			d.seek(p.polyStart);

			if(p.polsize == 0x40){
				for(int x = 0 ; x < p.polyamt / 3 ; x++){
					m.faces.Add (d.readShort());
					m.faces.Add (d.readShort());
					m.faces.Add (d.readShort());
				}
			}

			if (p.polsize == 0x00 || p.polsize == 0x04) {
				int faceCount = p.polyamt;
				int faceStart = d.pos();
				int verStart = (faceCount * 2) + faceStart;

				int startDirection = 1;
				int f1 = d.readShort();
				int f2 = d.readShort();
				int faceDirection = startDirection;
				int f3;
				do {
					f3 = d.readShort();
					if (f3 == 0xFFFF) {
						f1 = d.readShort();
						f2 = d.readShort();
						faceDirection = startDirection;
					} else {
						faceDirection *= -1;
						if ((f1 != f2) && (f2 != f3) && (f3 != f1)) {
							if (faceDirection > 0) {
								m.faces.Add (f3);
								m.faces.Add (f2);
								m.faces.Add (f1);
							} else {
								m.faces.Add (f2);
								m.faces.Add (f3);
								m.faces.Add (f1);
							}
						}
						f1 = f2;
						f2 = f3;
					}
				} while (d.pos() != (verStart));

			}

			return m;
		}

		//VERTEX TYPES----------------------------------------------------------------------------------------
		private static void readUV(FileData d, Poly p, NUD.Object o, Polygon m, Vertex[] v){

			int uvCount = (p.UVSize >> 4);
			int uvType = (p.UVSize) & 0xF;

			for (int i = 0; i < p.vertamt; i++) {
				v[i] = new Vertex();
				if (uvType == 0x2) {
					v[i].col =  new Vector3 (d.readByte ()/127f, d.readByte ()/127f, d.readByte ()/127f);
					d.skip (1);
					v [i].tx = new Vector2 (d.readHalfFloat(), d.readHalfFloat());
				} else
					Console.WriteLine ("No uv found");
			}
		}




		private static void readVertex (FileData d, Poly p, Object o, Polygon m, int size){
			int weight = size >> 4;
			int nrm = size & 0xF;

			Vertex[] v = new Vertex[p.vertamt];

			d.seek (p.vertStart);
			if (weight > 0) {
				readUV (d, p, o, m, v);
				d.seek (p.verAddStart);
			} else {
				for (int i = 0; i < p.vertamt; i++) {
					v [i] = new Vertex ();
				}
			}


			for (int i = 0; i < p.vertamt; i++) {
				v[i].pos.X = d.readFloat();
				v[i].pos.Y = d.readFloat();
				v[i].pos.Z = d.readFloat();

				v[i].nrm.X = d.readHalfFloat();
				v[i].nrm.Y = d.readHalfFloat();
				v[i].nrm.Z = d.readHalfFloat();
				d.skip(2); // n1?

				if(nrm == 7)
					d.skip(16); // bn and tan half floats

				if (weight == 0) {
					if(p.UVSize >= 18){
						v[i].col.X = (int) d.readByte() / 0xFF;
						v[i].col.Y = (int) d.readByte() / 0xFF;
						v[i].col.Z = (int) d.readByte() / 0xFF;
						d.skip (1);
						//v.a = (int) (d.readByte());
					}

					v [i].tx = new Vector2 (d.readHalfFloat(), d.readHalfFloat());

					// UV layers
					d.skip (4 * ((p.UVSize>>4) - 1));
				}

				if (weight == 4) {
					v [i].node.Add (d.readByte ());
					v [i].node.Add (d.readByte ());
					v [i].node.Add (d.readByte ());
					v [i].node.Add (d.readByte ());
					v [i].weight.Add ((float)d.readByte () / 255f);
					v [i].weight.Add ((float)d.readByte () / 255f);
					v [i].weight.Add ((float)d.readByte () / 255f);
					v [i].weight.Add ((float)d.readByte () / 255f);
				} else if (weight == 0) {
					v [i].node.Add (o.singlebind);
					v [i].weight.Add (1);
				}
			}

			foreach(Vertex vi in v)
				m.vertices.Add(vi);
		}


		// Creating---------------------------------------------------------
		public void saveNUD(String fname, VBN vbn){
			FileOutput d = new FileOutput(); // data
			d.littleEndian = true;

			// mesh optimize

			int vertexType = 0x46;
			if(vbn.bones.Count == 0)
				vertexType = 0x06;
			int UVType = 0x12;

			d.writeString("NDP3");
			d.writeInt(0); //FileSize
			d.writeShort(0x200); //  version num
			d.writeShort(mesh.Count); // polysets
			d.writeShort(2); // type
			d.writeShort(vbn.bones.Count - 1); // Number of bones

			d.writeInt(0); // polyClump start
			d.writeInt(0); // polyClump size
			d.writeInt(0); // vertexClumpsize
			d.writeInt(0); // vertexaddcump size

			// some floats.. TODO: I dunno what these are for
			d.writeFloat(0);
			d.writeFloat(0);
			d.writeFloat(0);
			d.writeFloat(0);

			// other sections....
			FileOutput obj = new FileOutput(); // data
			obj.littleEndian = true;
			FileOutput tex = new FileOutput(); // data
			tex.littleEndian = true;

			FileOutput poly = new FileOutput(); // data
			poly.littleEndian = true;
			FileOutput vert = new FileOutput(); // data
			vert.littleEndian = true;
			FileOutput vertadd = new FileOutput(); // data
			vertadd.littleEndian = true;

			FileOutput str = new FileOutput(); // data
			str.littleEndian = true;


			// obj descriptor

			FileOutput tempstring = new FileOutput(); // data
			for(int i = 0; i < mesh.Count ; i++){
				str.writeString(mesh[i].name);
				str.writeByte(0);
				str.align(16);
			}

			int polyCount = 0; // counting number of poly
			foreach (Mesh m in mesh)
				polyCount += m.polygons.Count;

			for(int i = 0; i < mesh.Count ; i++){

				// more floats TODO: I dunno what these are for
				d.writeFloat(0);
				d.writeFloat(0);
				d.writeFloat(0);
				d.writeFloat(0);

				d.writeFloat(0);
				d.writeFloat(0);
				d.writeFloat(0);
				d.writeFloat(0);

				d.writeInt(tempstring.size());

				// TODO: Write String here
				tempstring.writeString(mesh[i].name);
				tempstring.writeByte(0);
				tempstring.align(16);

				d.writeInt(0x04); // ID
				d.writeShort(-1); // Single Bind 
				d.writeShort(mesh[i].polygons.Count); // poly count
				d.writeInt(obj.size() + 0x30 + mesh.Count * 0x30); // position start for obj

				// write obj info here...
				for (int k = 0; k < mesh[i].polygons.Count; k++) {

					obj.writeInt(poly.size());
					obj.writeInt(vert.size());
					obj.writeInt(vertadd.size());
					obj.writeShort(mesh[i].polygons[k].vertices.Count);
					obj.writeByte(vertexType); // type of vert

					int maxUV = 1; // TODO: multi uv stuff  mesh[i].polygons[k].maxUV() + 

					obj.writeByte((maxUV << 4)|2); // type of UV 0x12 for vertex color

					obj.writeInt(tex.size() + 0x30 + mesh.Count * 0x30 + polyCount * 0x30); // Tex properties... This is tex offset
					obj.writeInt(0);// TODO: perhaps figure out??
					obj.writeInt(0);
					obj.writeInt(0);

					obj.writeShort(mesh[i].polygons[k].faces.Count); // polyamt
					obj.writeByte(0x40); // polysize 0x04 is strips and 0x40 is easy
					// :D
					obj.writeByte(0x04); // polyflag

					obj.writeInt(0); // idk, nothing padding??
					obj.writeInt(0);
					obj.writeInt(0);

					// Write the texture.... TODO: skip for now
					// MATERIAL SECTION TODO:---------------------------------------------------------------------------------

					FileOutput te = new FileOutput();
					te.littleEndian = true;

					uint testflags = 0x9A013063;
					te.writeInt((int)testflags); // flags 9A013063
					te.writeInt(0);
					te.writeInt(1); // number of tex
					te.writeInt(0x00010204);
					te.writeInt(0x405);
					te.writeInt(0);
					te.writeInt(0);
					te.writeInt(0);

					te.writeInt(0 + i); // texid
					te.writeInt(0);
					te.writeInt(0);
					te.writeInt(0x01010302);
					te.writeByte(0x2);// 6 when not ending
					te.writeByte(0);
					te.writeShort(0);
					te.writeInt(0);

					// write properties
					// color sample UV
					int propCount = 0;
					te.writeInt(propCount-- > 0 ? 0x20 : 0x00);
					te.writeInt(str.size());
					te.writeInt(4);
					te.writeInt(0);
					te.writeFloat(1f);
					te.writeFloat(1f);
					te.writeFloat(0f);
					te.writeFloat(0f);
					str.writeString("NU_colorSamplerUV");
					str.writeByte(0);
					str.align(16);

					tex.writeOutput(te);

					// Actually Create Texture
					// TODO:

					// Write the poly...
					foreach (int face in mesh[i].polygons[k].faces)
						poly.writeShort(face);

					// Write the vertex....

					if (vertexType == 0x06) {
						foreach (Vertex v in mesh[i].polygons[k].vertices) {
							vert.writeFloat(v.pos.X);
							vert.writeFloat(v.pos.Y);
							vert.writeFloat(v.pos.Z);

							vert.writeHalfFloat(v.nrm.X);
							vert.writeHalfFloat(v.nrm.Y);
							vert.writeHalfFloat(v.nrm.Z);
							vert.writeHalfFloat(1);

							vert.writeByte((int)(v.col.X * 127));
							vert.writeByte((int)(v.col.Y * 127));
							vert.writeByte((int)(v.col.Z * 127));
							vert.writeByte(255 / 2);

							vert.writeHalfFloat(v.tx.X);
							vert.writeHalfFloat(v.tx.Y);

							// TODO: Multiuv
							/*for(int l = 0 ; l < maxUV-1 ; l++){
								vert.writeHalfFloat(v.uvx.get(l));
								vert.writeHalfFloat(v.uvy.get(l));
							}*/
						}
					} else if (vertexType == 0x46) {
						foreach (Vertex v in mesh[i].polygons[k].vertices) {
							vert.writeByte((int)(v.col.X  * 127)); // +1 for stages
							vert.writeByte((int)(v.col.Y  * 127));
							vert.writeByte((int)(v.col.Z  * 127));
							vert.writeByte(255 / 2);

							vert.writeHalfFloat(v.tx.X);
							vert.writeHalfFloat(v.tx.Y);
							// TODO: MultiUV
							/*for(int l = 0 ; l < maxUV-1 ; l++){
								vert.writeHalfFloat(v.uvx.get(l));
								vert.writeHalfFloat(v.uvy.get(l));
							}*/
						}

						// Write the Vertex add??

						foreach (Vertex v in mesh[i].polygons[k].vertices) {
							vertadd.writeFloat(v.pos.X);
							vertadd.writeFloat(v.pos.Y);
							vertadd.writeFloat(v.pos.Z);

							vertadd.writeHalfFloat(v.nrm.X);
							vertadd.writeHalfFloat(v.nrm.Y);
							vertadd.writeHalfFloat(v.nrm.Z);
							vertadd.writeHalfFloat(1f);

							// weight cannot go over 4 here anyway...
							/*int vsize = v.node.size();
							if (vsize > 4) {
								//System.out.println("Weight Problem!!!");
								vsize = 4;
								float scalefactor = (1 - (v.weight.get(0)
									+ v.weight.get(1) + v.weight.get(2) + v.weight
									.get(3))) / 4;
								v.weight.set(0, v.weight.get(0) + scalefactor);
								v.weight.set(1, v.weight.get(1) + scalefactor);
								v.weight.set(2, v.weight.get(2) + scalefactor);
								v.weight.set(3, v.weight.get(3) + scalefactor);
							}*/

							for (int j = 0; j < v.node.Count; j++)
								vertadd.writeByte(v.node[j]);

							for (int j = v.node.Count; j < v.node.Count + (4 - v.node.Count); j++)
								vertadd.writeByte(0);

							for (int j = 0; j < v.node.Count; j++)
								vertadd.writeByte((int) (v.weight[j] * 255f));

							for (int j = v.node.Count; j < v.node.Count + (4 - v.node.Count); j++)
								vertadd.writeByte(0);
						}
					}

				}
			}

			//
			d.writeOutput(obj);
			d.writeOutput(tex);

			d.writeIntAt(d.size() - 0x30, 0x10);
			d.writeIntAt(poly.size(), 0x14);
			d.writeIntAt(vert.size(), 0x18);
			d.writeIntAt(vertadd.size(), 0x1c);

			d.writeOutput(poly);

			int s = d.size();
			d.align(16);
			s = d.size() - s;
			d.writeIntAt(poly.size() + s, 0x14);

			d.writeOutput(vert);

			s = d.size();
			d.align(16);
			s = d.size() - s;
			d.writeIntAt(vert.size() + s, 0x18);

			d.writeOutput(vertadd);

			s = d.size();
			d.align(16);
			s = d.size() - s;
			d.writeIntAt(vertadd.size() + s, 0x1c);

			d.writeOutput(str);

			d.writeIntAt(d.size(), 0x4);

			d.save(fname);
		}

	}
}

