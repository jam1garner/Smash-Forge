using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Smash_Forge
{
    public class BFRES
    {
        public List<string> stringContainer = new List<string>();
        public List<FMDL_Model> models = new List<FMDL_Model>();
        public Dictionary<string, FTEX> textures = new Dictionary<string, FTEX>();
        public int readOffset(FileData f)
        { 
            return f.pos() + f.readInt();
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
        Vector3[] vertdata, nrmdata;
        int[] facedata;
        Vector4[] bonedata, coldata, weightdata;

        public BFRES()
        {
            GL.GenBuffers(1, out vbo_position);
            GL.GenBuffers(1, out vbo_color);
            GL.GenBuffers(1, out vbo_nrm);
            GL.GenBuffers(1, out vbo_uv);
            GL.GenBuffers(1, out vbo_bone);
            GL.GenBuffers(1, out vbo_weight);
            GL.GenBuffers(1, out ibo_elements);
        }
        public void Destroy()
        {
            GL.DeleteBuffer(vbo_position);
            GL.DeleteBuffer(vbo_color);
            GL.DeleteBuffer(vbo_nrm);
            GL.DeleteBuffer(vbo_uv);
            GL.DeleteBuffer(vbo_weight);
            GL.DeleteBuffer(vbo_bone);
        }
        public void PreRender()
        {
            List<Vector3> vert = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector4> col = new List<Vector4>();
            List<Vector3> nrm = new List<Vector3>();
            List<Vector4> bone = new List<Vector4>();
            List<Vector4> weight = new List<Vector4>();
            List<int> face = new List<int>();

            int i = 0;
            foreach (FMDL_Model fmdl in models)
            {
                foreach(Mesh m in fmdl.poly) { 
                Console.WriteLine(m.vertices.Count);
                Console.WriteLine(m.faces.Count);
                if (m.faces.Count <= 3)
                    continue;
                foreach (Vertex v in m.vertices)
                {
                    vert.Add(v.pos);
                    col.Add(v.col);
                    nrm.Add(v.nrm);

                        if (v.tx.Count > 0)
                            uv.Add(v.tx[0]);
                        else
                            uv.Add(new Vector2(0, 0));

                        while (v.node.Count < 4)
                    {
                        v.node.Add(0);
                        v.weight.Add(0);
                    }
                    bone.Add(new Vector4(-1, 0, 0, 0));
                    weight.Add(new Vector4(-1, 0, 0, 0));
                    //bone.Add(new Vector4(v.node[0], v.node[1], v.node[2], v.node[3]));
                    //weight.Add(new Vector4(v.weight[0], v.weight[1], v.weight[2], v.weight[3]));
                }

                foreach (List<int> l in m.faces)
                    {
                        //face.AddRange(l);
                        // rearrange faces
                        int[] ia = l.ToArray();
                        for (int j = 0; j < ia.Length; j++)
                        {
                            ia[j] += i;
                        }
                        face.AddRange(ia);
                    }
                    i += m.vertices.Count;
                }
            }

            vertdata = vert.ToArray();
            coldata = col.ToArray();
            nrmdata = nrm.ToArray();
            uvdata = uv.ToArray();
            facedata = face.ToArray();
            bonedata = bone.ToArray();
            weightdata = weight.ToArray();

        }
        public void Render(Shader shader)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);
            GL.BufferData<Vector4>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector4.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("vColor"), 4, VertexAttribPointerType.Float, false, 0, 0);

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

            int indiceat = 0;
            foreach (FMDL_Model fmdl in models)
            {
                foreach (Mesh m in fmdl.poly)
                {
                    GL.Uniform4(shader.getAttribute("colorSamplerUV"), new Vector4(1, 1, 0, 0));

                    if (m.texHashs.Count > 0 ) { 
                    GL.BindTexture(TextureTarget.Texture2D, m.texHashs[0]);
                    GL.Uniform1(shader.getAttribute("tex"), 0);
                }
                    //GL.BindTexture(TextureTarget.Texture2D, m.texId);
                    //GL.Uniform1(shader.getAttribute("tex"), 0);

                    foreach (List<int> l in m.faces)
                {
                    if (fmdl.isVisible)
                        GL.DrawElements(PrimitiveType.Triangles, l.Count, DrawElementsType.UnsignedInt, indiceat * sizeof(int));

                    indiceat += l.Count;
                }
                }
            }
        }

        public  void Read(string filename)
        {
            FileData f = new FileData(filename);
            f.Endian = Endianness.Big;
            f.seek(0);

            f.seek(4);
            int highVersion = f.readByte();
            int lowVersion = f.readByte();
            int overallVerstion = f.readShort();
            short BOM = (short)f.readShort();
            f.skip(6);
            int fileAlignment = f.readInt();
            string name = f.readString(readOffset(f),-1);
            int strTblLenth = f.readInt();
            int strTblOffset = readOffset(f);

            int FMDLOffset = readOffset(f);
            int FTEXOffset = readOffset(f);
            int FSKAOffset = readOffset(f);
            int FSHU0Offset = readOffset(f);
            int FSHU1Offset = readOffset(f);
            int FSHU2Offset = readOffset(f);
            int FTXPOffset = readOffset(f);
            int FVIS0Offset = readOffset(f);
            int FVIS1Offset = readOffset(f);
            int FSHAOffset = readOffset(f);
            int FSCNOffset = readOffset(f);
            int EMBOffset = readOffset(f);

            int FMDLCount = f.readShort();
            int FTEXCount = f.readShort();
            int FSKACount = f.readShort();
            int FSHU0Count = f.readShort();
            int FSHU1Count = f.readShort();
            int FSHU2Count = f.readShort();
            int FTXPCount = f.readShort();
            int FVIS0Count = f.readShort();
            int FVIS1Count = f.readShort();
            int FSHACount = f.readShort();
            int FSCNCount = f.readShort();
            int EMBCount = f.readShort();

                //FTEX -Texures-
                f.seek(FTEXOffset + 0x18);
            for (int i = 0; i < FTEXCount; i++)
            {
                f.skip(0x8);
                string TextureName = f.readString(readOffset(f), -1);
                int offset = readOffset(f);
                int NextFTEX = f.pos();
                f.seek(offset + 8);
                FTEX texture = new FTEX();
                texture.ReadFTEX(f);
                textures.Add(TextureName, texture);
                f.seek(NextFTEX);
            }

                //FMDLs -Models-
                f.seek(FMDLOffset + 0x18);
            for(int i = 0;i< FMDLCount; i++)
            {

                FMDL_Model model = new FMDL_Model();
                

                f.skip(0xC);
                int offset = readOffset(f);
                int NextFMDL = f.pos();
                f.seek(offset+4);
                FMDLheader fmdl_info = new FMDLheader
                {
                    name = f.readString(readOffset(f), -1),
                    eofString = readOffset(f),
                    fsklOff = readOffset(f),
                    fvtxArrOff = readOffset(f),
                    fshpIndx = readOffset(f),
                    fmatIndx = readOffset(f),
                    paramOff = readOffset(f),
                    fvtxCount = f.readShort(),
                    fshpCount = f.readShort(),
                    fmatCount = f.readShort(),
                    paramCount = f.readShort(),

                };

                List<FVTXH> FVTXArr = new List<FVTXH>();
                f.seek(fmdl_info.fvtxArrOff);
                for(int vtx = 0;vtx < fmdl_info.fvtxCount; vtx++)
                {
                    f.skip(4);
                    FVTXArr.Add(new FVTXH
                    {
                        attCount = f.readByte(),
                        buffCount = f.readByte(),
                        sectIndx = f.readShort(),
                        vertCount = f.readInt(),
                        u1 = f.readShort(),
                        u2 = f.readShort(),
                        attArrOff = readOffset(f),
                        attIndxOff = readOffset(f),
                        buffArrOff = readOffset(f)
                    });
                    f.skip(4);
                }


                f.seek(fmdl_info.fmatIndx + 0x18);
                List<FMATH> FMATheaders = new List<FMATH>();
                for (int mat = 0;mat< fmdl_info.fmatCount; mat++)
                {
                    f.skip(8);
                    string FMATNameOffset = f.readString(readOffset(f), -1);
                    int rtn = f.pos() + 4;
                    f.seek(readOffset(f) + 4);
                    FMATH fmat_info = new FMATH
                    {
                        name = FMATNameOffset,
                        matOff = readOffset(f),
                        u1 = f.readInt(),
                        sectIndx = f.readShort(),
                        rendParamCount = f.readShort(),
                        texSelCount = f.readByte(),
                        texAttSelCount = f.readByte(),
                        matParamCount = f.readShort(),
                        matParamSize = f.readInt(),
                        u2 = f.readInt(),
                        rendParamIndx = readOffset(f),
                        unkMatOff = readOffset(f),
                        shadeOff = readOffset(f),
                        texSelOff = readOffset(f),
                        texAttSelOff = readOffset(f),
                        texAttIndxOff = readOffset(f),
                        matParamArrOff = readOffset(f),
                        matParamIndxOff = readOffset(f),
                        matParamOff = readOffset(f),
                        shadParamIndxOff = readOffset(f)
                    };
                    f.seek(rtn);
                    FMATheaders.Add(fmat_info);
                }

                f.seek(fmdl_info.fsklOff + 6);
                FSKLH fskl_info = new FSKLH
                {
                    fsklType = f.readShort(),
                    boneArrCount = f.readShort(),
                    invIndxArrCount = f.readShort(),
                    exIndxCount = f.readShort(),
                    u1 = f.readShort(),
                    boneIndxOff = readOffset(f),
                    boneArrOff = readOffset(f),
                    invIndxArrOff = readOffset(f),
                    invMatrArrOff = readOffset(f)
                };

                List<int> Node_Array = new List<int>();
                f.seek(fskl_info.invIndxArrOff);
                for(int nodes = 0; nodes < fskl_info.invIndxArrCount; nodes++)
                {
                    Node_Array.Add(f.readShort());
                }

                List<FSHPH> FSHPArr = new List<FSHPH>();
                f.seek(fmdl_info.fshpIndx + 24);
                for(int shp = 0;shp< fmdl_info.fshpCount; shp++)
                {
                    f.skip(12);
                    int rtn2 = f.pos() + 4;
                    f.seek(readOffset(f) + 4);
                    FSHPArr.Add(new FSHPH
                    {
                        polyNameOff = readOffset(f),
                        u1 = f.readInt(),
                        fvtxIndx = f.readShort(),
                        fmatIndx = f.readShort(),
                        fsklIndx = f.readShort(),
                        sectIndx = f.readShort(),
                        fsklIndxArrCount = f.readShort(),
                        matrFlag = f.readByte(),
                        lodMdlCount = f.readByte(),
                        visGrpCount = f.readInt(),
                        u3 = f.readInt(),
                        fvtxOff = readOffset(f),
                        lodMdlOff = readOffset(f),
                        fsklIndxArrOff = readOffset(f),
                        u4 = f.readInt(),
                        visGrpNodeOff = readOffset(f),
                        visGrpRangeOff = readOffset(f),
                        visGrpIndxOff = readOffset(f)
                    });
                    f.seek(rtn2);
                }

                f.seek(fskl_info.boneArrOff);
                for(int bn = 0;bn< fskl_info.boneArrCount; bn++)
                {
                    Bone bone = new Smash_Forge.Bone();
                    bone.boneName = f.readString(readOffset(f), -1).ToCharArray();
                    bone.boneId = (uint)f.readShort();
                    int parIndx1 = (short)f.readShort();
                    int parIndx2 = f.readShort();
                    int parIndx3 = f.readShort();
                    int parIndx4 = f.readShort();
                    bone.parentIndex = parIndx1;

                    f.skip(6);
                    bone.scale = new float[3];
                    bone.rotation = new float[4];
                    bone.position = new float[3];
                    bone.scale[0] = f.readFloat();
                    bone.scale[1] = f.readFloat();
                    bone.scale[2] = f.readFloat();
                    bone.rotation[0] = f.readFloat();
                    bone.rotation[1] = f.readFloat();
                    bone.rotation[2] = f.readFloat();
                    bone.rotation[3] = f.readFloat();
                    bone.position[0] = f.readFloat();
                    bone.position[1] = f.readFloat();
                    bone.position[2] = f.readFloat();
                    f.skip(4);

                    model.skeleton.bones.Add(bone);

                    if (lowVersion<4)
                        f.skip(48);
                }
                model.skeleton.reset();


                //MeshTime!!
                for(int m = 0;m < FSHPArr.Count; m++)
                {
                    Mesh poly = new Mesh();
                    

                    poly.name = f.readString(FSHPArr[m].polyNameOff, -1);

                    List<attdata> AttrArr = new List<attdata>();
                    f.seek(FVTXArr[FSHPArr[m].fvtxIndx].attArrOff);
                    for(int att = 0;att< FVTXArr[FSHPArr[m].fvtxIndx].attCount; att++)
                    {
                        string AttType = f.readString(readOffset(f), -1);
                        int buffIndx = f.readByte();
                        f.skip(1);
                        int buffOff = f.readShort();
                        int vertType = f.readInt();
                        AttrArr.Add(new attdata {attName = AttType,buffIndx = buffIndx,buffOff = buffOff, vertType = vertType });
                    }


                    List<buffData> BuffArr = new List<buffData>();
                    f.seek(FVTXArr[FSHPArr[m].fvtxIndx].buffArrOff);
                    for(int buf = 0; buf < FVTXArr[FSHPArr[m].fvtxIndx].buffCount; buf++)
                    {
                        buffData data = new buffData();
                        f.skip(4);
                        data.buffSize = f.readInt();
                        f.skip(4);
                        data.strideSize = f.readShort();
                        f.skip(6);
                        data.dataOffset = readOffset(f);
                        BuffArr.Add(data);
                    }
                    for (int v = 0; v < FVTXArr[FSHPArr[m].fvtxIndx].vertCount; v++)
                    {
                        Vertex vert = new Vertex();
                        for (int attr = 0; attr < AttrArr.Count; attr++)
                        {
                            f.seek(((BuffArr[AttrArr[attr].buffIndx].dataOffset) + (AttrArr[attr].buffOff) + (BuffArr[AttrArr[attr].buffIndx].strideSize * v)));
                            switch (AttrArr[attr].attName)
                            {
                                case "_p0":
                                    if (AttrArr[attr].vertType == 2063)
                                        vert.pos = new Vector3 { X = f.readHalfFloat(), Y = f.readHalfFloat(), Z = f.readHalfFloat() };
                                    if (AttrArr[attr].vertType == 2065)
                                        vert.pos = new Vector3 { X = f.readFloat(), Y = f.readFloat(), Z = f.readFloat() };
                                    break;
                                case "_c0":
                                    if (AttrArr[attr].vertType == 2063)
                                        vert.col = new Vector4 { X = f.readHalfFloat(), Y = f.readHalfFloat(), Z = f.readHalfFloat(), W = f.readHalfFloat() };
                                    if (AttrArr[attr].vertType == 2067)
                                        vert.col = new Vector4 { X = f.readFloat(), Y = f.readFloat(), Z = f.readFloat(), W = f.readFloat() };
                                    if (AttrArr[attr].vertType == 10)
                                        vert.col = new Vector4 { X = f.readByte()/127 , Y = f.readByte() / 127, Z = f.readByte() / 127, W = f.readByte() / 127 };
                                    break;
                                case "_n0":
                                    if (AttrArr[attr].vertType == 0x20b)
                                    {
                                        uint normVal = (uint)f.readInt();
                                        //Thanks RayKoopa!
                                        vert.nrm = new Vector3 { X = ((normVal & 0x3FC00000) >> 22) / 511,Y = ((normVal & 0x000FF000) >> 12) / 511,Z = ((normVal & 0x000003FC) >> 2) / 511 };
                                    }
                                    break;
                                case "_u0":
                                case "color":
                                case "_u1":
                                case "_u2":
                                case "_u3":
                                    if (AttrArr[attr].vertType == 4 || AttrArr[attr].vertType == 516)
                                        vert.tx.Add(new Vector2 { X = ((float)f.readByte()) / 255, Y = ((float)f.readByte()) / 255 });
                                    if (AttrArr[attr].vertType == 7)
                                        vert.tx.Add(new Vector2 { X = ((float)f.readShort()) / 65535, Y = ((float)f.readShort()) / 65535 });
                                    if (AttrArr[attr].vertType == 519)
                                        vert.tx.Add(new Vector2 { X = ((float)f.readShort()) / 32767, Y = ((float)f.readShort()) / 32767 });
                                    if (AttrArr[attr].vertType == 2056)
                                        vert.tx.Add(new Vector2 { X = f.readHalfFloat(), Y = f.readHalfFloat() });
                                    if (AttrArr[attr].vertType == 2061)
                                        vert.tx.Add(new Vector2 { X = f.readFloat(), Y = f.readFloat() });
                                    break;
                                case "_i0":
                                    if (AttrArr[attr].vertType == 256) { 
                                        vert.node.Add(f.readByte());
                                        vert.weight.Add((float)1.0);
                                    }
                                    if (AttrArr[attr].vertType == 260)
                                    {
                                        vert.node.Add(f.readByte());
                                        vert.node.Add(f.readByte());
                                    }
                                    if (AttrArr[attr].vertType == 266)
                                    {
                                        vert.node.Add(f.readByte());
                                        vert.node.Add(f.readByte());
                                        vert.node.Add(f.readByte());
                                        vert.node.Add(f.readByte());
                                    }
                                    break;
                                case "_w0":
                                    if (AttrArr[attr].vertType == 4)
                                    {
                                        vert.weight.Add(((float)f.readByte()) / 255);
                                        vert.weight.Add(((float)f.readByte()) / 255);
                                    }
                                    if (AttrArr[attr].vertType == 10)
                                    {
                                        vert.weight.Add(((float)f.readByte()) / 255);
                                        vert.weight.Add(((float)f.readByte()) / 255);
                                        vert.weight.Add(((float)f.readByte()) / 255);
                                        vert.weight.Add(((float)f.readByte()) / 255);
                                    }
                                    break;
                            }
                        }
                        poly.vertices.Add(vert);
                    }

                    f.seek(FSHPArr[m].lodMdlOff + 4);
                    int faceType = f.readInt();
                    f.skip(0xC);
                    int indxBuffOff = readOffset(f) + 4;
                    int elmSkip = f.readInt();
                    f.seek(indxBuffOff);
                    int FaceCount = f.readInt();
                    f.skip(0xC);
                    f.seek(readOffset(f));
                    if (faceType == 4)
                        FaceCount = FaceCount / 6;
                    if (faceType == 9)
                        FaceCount = FaceCount / 12;
                    for (int face = 0; face < FaceCount; face++)
                    {
                        if (faceType == 4)
                            poly.faces.Add(new List<int> { elmSkip + f.readShort()  , elmSkip + f.readShort() , elmSkip + f.readShort()  });
                        else if (faceType == 9)
                            poly.faces.Add(new List<int> { elmSkip + f.readInt(), elmSkip + f.readInt(), elmSkip + f.readInt() });
                        else
                            Console.Write("UnkFaceFormat");

                    }

                    f.seek(FMATheaders[FSHPArr[m].fmatIndx].texSelOff);
                    for(int tex = 0; FMATheaders[FSHPArr[m].fmatIndx].texAttSelCount > tex; tex++)
                    {
                        string TextureName = f.readString(readOffset(f), -1);
                        poly.texHashs.Add(textures[TextureName].texture.display);
                        f.skip(4);
                    }
                    model.poly.Add(poly);
                }
                models.Add(model);
                f.seek(NextFMDL);
            }
            PreRender();

        }

        public class FMDLheader
        {
            public string FMDL;
            public string name;
            public int eofString;
            public int fsklOff;
            public int fvtxArrOff;
            public int fshpIndx;
            public int fmatIndx;
            public int paramOff;
            public int fvtxCount;
            public int fshpCount;
            public int fmatCount;
            public int paramCount;
        }
        public class FVTXH
        {
            public int attCount;
            public int buffCount;
            public int sectIndx;
            public int vertCount;
            public int u1;
            public int u2;
            public int attArrOff;
            public int attIndxOff;
            public int buffArrOff;
        }
        public class FMATH
        {
            public string name;
            public int matOff;
            public int u1;
            public int sectIndx;
            public int rendParamCount;
            public int texSelCount;
            public int texAttSelCount;
            public int matParamCount;
            public int matParamSize;
            public int u2;
            public int rendParamIndx;
            public int unkMatOff;
            public int shadeOff;
            public int texSelOff;
            public int texAttSelOff;
            public int texAttIndxOff;
            public int matParamArrOff;
            public int matParamIndxOff;
            public int matParamOff;
            public int shadParamIndxOff;
        }
        public class FSKLH
        {
            public int fsklType;
            public int boneArrCount;
            public int invIndxArrCount;
            public int exIndxCount;
            public int u1;
            public int boneIndxOff;
            public int boneArrOff;
            public int invIndxArrOff;
            public int invMatrArrOff;
        }
        public class FSHPH
        {
            public int polyNameOff;
            public int u1;
            public int fvtxIndx;
            public int fmatIndx;
            public int fsklIndx;
            public int sectIndx;
            public int fsklIndxArrCount;
            public int matrFlag;
            public int lodMdlCount;
            public int visGrpCount;
            public int u3;
            public int fvtxOff;
            public int lodMdlOff;
            public int fsklIndxArrOff;
            public int u4;
            public int visGrpNodeOff;
            public int visGrpRangeOff;
            public int visGrpIndxOff;
        }
        public class attdata
        {
            public string attName;
            public int buffIndx;
            public int buffOff;
            public int vertType;
        }
        public class buffData
        {
            public int buffSize;
            public int strideSize;
            public int dataOffset;
        }
        public class lodmdl
        {
            public int u1;
            public int faceType;
            public int dCount;
            public int visGrpCount;
            public int u3;
            public int visGrpOff;
            public int indxBuffOff;
            public int elmSkip;
        }
        public class Vertex
        {
            public Vector3 pos = new Vector3(0, 0, 0), nrm = new Vector3(0, 0, 0);
            public Vector4 col = new Vector4(2, 2, 2, 1);
            public List<Vector2> tx = new List<Vector2>();
            public List<int> node = new List<int>();
            public List<float> weight = new List<float>();
        }
        public class Mesh
        {
            public List<List<int>> faces = new List<List<int>>();
            public List<Vertex> vertices = new List<Vertex>();
            public List<int> texHashs = new List<int>();
            public string name;
        }
        public class FMDL_Model
        {

            public VBN skeleton = new VBN();
            public List<Mesh> poly = new List<Mesh>();
            public bool isVisible = true;
        }
        


        public void readFSKA()
        {

        }


    }
}
