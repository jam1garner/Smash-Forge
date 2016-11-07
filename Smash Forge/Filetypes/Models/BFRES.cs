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
    class BFRES
    {
        public List<FMDL_Model> models = new List<FMDL_Model>();
        public Dictionary<string, FTEX_Texture> textures = new Dictionary<string, FTEX_Texture>();
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


            //FMDLs -Models-
            f.seek(FMDLOffset + 0x18);
            for(int i = 0;i< FMDLCount; i++)
            {

                FMDL_Model model = new FMDL_Model();
                f.skip(0xC);
                int offset = readOffset(f);
                int NextFMDL = f.pos();
                f.seek(offset);
                FMDLheader fmdl_info = new FMDLheader
                {
                    FMDL = f.readString(f.pos(), 4),
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
                    f.seek(readOffset(f));
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

                f.seek(fmdl_info.fsklOff);
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
                f.seek(fmdl_info.fshpIndx + 0x18);
                for(int shp = 0;shp< fmdl_info.fshpCount; shp++)
                {
                    f.skip(0xC);
                    int FSHPOffset = readOffset(f);
                    int rtn = f.pos();
                    f.seek(FSHPOffset + 4);
                    FSHPH fshp_info = new FSHPH
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
                    };
                    f.seek(rtn);
                    FSHPArr.Add(fshp_info);
                }

                f.seek(fskl_info.boneArrOff);
                for(int bn = 0;bn< fskl_info.boneArrCount; bn++)
                {
                    Bone bone = new Smash_Forge.Bone();
                    bone.boneName = f.readString(readOffset(f), -1).ToCharArray();
                    bone.boneId = (uint)f.readShort();
                    int parIndx1 = f.readShort();
                    int parIndx2 = f.readShort();
                    int parIndx3 = f.readShort();
                    int parIndx4 = f.readShort();

                    f.skip(6);
                    bone.scale = new float[3];
                    bone.rotation = new float[3];
                    bone.position = new float[3];
                    bone.scale[0] = f.readFloat();
                    bone.scale[1] = f.readFloat();
                    bone.scale[2] = f.readFloat();
                    bone.rotation[0] = f.readFloat();
                    bone.rotation[1] = f.readFloat();
                    bone.rotation[2] = f.readFloat();
                    f.skip(4);
                    bone.position[0] = f.readFloat();
                    bone.position[1] = f.readFloat();
                    bone.position[2] = f.readFloat();
                    f.skip(4);

                    if (parIndx1 > -1)
                        bone.children.Add(parIndx1);
                    if (parIndx2 > -1)
                        bone.children.Add(parIndx2);
                    if (parIndx3 > -1)
                        bone.children.Add(parIndx2);
                    if (parIndx4 > -1)
                        bone.children.Add(parIndx3);


                    model.skeleton.bones.Add(bone);

                    if (highVersion<4)
                        f.skip(0x30);
                }
                model.skeleton.reset();


                //MeshTime!!
                for(int m = 0;m < FSHPArr.Count; m++)
                { 
                    model.name = f.readString(FSHPArr[m].polyNameOff, -1);

                    List<attdata> AttrArr = new List<attdata>();
                    f.seek(FVTXArr[FSHPArr[m].fvtxIndx].attArrOff);
                    for(int att = 0;att< FVTXArr[FSHPArr[m].fvtxIndx].attCount; att++)
                    {
                        string AttType = f.readString(FSHPArr[m].polyNameOff, -1);
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
                    Vertex vert = new Vertex();
                    if (BuffArr.Count > 1) { 
                        for(int attr = 0;attr < AttrArr.Count; attr++)
                    {
                            
                            f.seek(((BuffArr[AttrArr[attr].buffIndx].dataOffset) + (AttrArr[attr].buffOff)));
                            for(int v = 0;v < FVTXArr[FSHPArr[m].fvtxIndx].vertCount; v++)
                            {
                                vert = new Vertex();
                                int VertStart = f.pos();
                                //f.seek(AttrArr[attr].buffOff + f.pos()); -Need to see if buffOff is on larger than 1 BuffArr first
                                
                                switch (AttrArr[attr].attName)
                                {
                                    case "_p0":
                                        if (AttrArr[attr].vertType == 2063)
                                            vert.pos = new Vector3 {X=f.readHalfFloat(),Y=f.readHalfFloat(), Z=f.readHalfFloat()};
                                        if (AttrArr[attr].vertType == 2065)
                                            vert.pos = new Vector3 { X = f.readFloat(), Y = f.readFloat(), Z = f.readFloat() };
                                        break;
                                    case "_c0":
                                        if (AttrArr[attr].vertType == 2063)
                                            vert.col = new Vector4 { X = (int)f.readHalfFloat()*255, Y = (int)f.readHalfFloat() * 255, Z = (int)f.readHalfFloat() * 255 , W = (int)f.readHalfFloat() * 255 };
                                        if (AttrArr[attr].vertType == 2067)
                                            vert.col = new Vector4 { X = (int)f.readFloat() * 255, Y = (int)f.readFloat() * 255, Z = (int)f.readFloat() * 255, W = (int)f.readFloat() * 255 };
                                        if (AttrArr[attr].vertType == 10)
                                            vert.col = new Vector4 { X = f.readByte(), Y = f.readByte(), Z = f.readByte(), W = f.readByte()};
                                        break;
                                    case "_u0":
                                    case "color":
                                    case "_u1":
                                    case "_u2":
                                    case "_u3":
                                        if (AttrArr[attr].vertType == 4 || AttrArr[attr].vertType == 516)
                                            vert.tx.Add(new Vector2 {X = ((float)f.readByte()) / 255, Y = ((float)f.readByte())/255});
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
                                        if (AttrArr[attr].vertType == 256)
                                            vert.node.Add(Node_Array[f.readByte()]);
                                        if (AttrArr[attr].vertType == 260)
                                        {
                                            vert.node.Add(Node_Array[f.readByte()]);
                                            vert.node.Add(Node_Array[f.readByte()]);
                                        }
                                        if (AttrArr[attr].vertType == 266)
                                        {
                                            vert.node.Add(Node_Array[f.readByte()]);
                                            vert.node.Add(Node_Array[f.readByte()]);
                                            vert.node.Add(Node_Array[f.readByte()]);
                                            vert.node.Add(Node_Array[f.readByte()]);
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

                                f.seek(BuffArr[AttrArr[attr].buffIndx].strideSize + f.pos());
                            }
                            model.vertices.Add(vert);

                            List<int> faces = new List<int>();

                            f.seek(FSHPArr[m].lodMdlOff + 4);
                            int faceType = f.readInt();
                            f.skip(0xC);
                            f.seek(readOffset(f) + 4);
                            int FaceCount = f.readInt();
                            f.skip(0xC);
                            f.seek(readOffset(f));
                            if (faceType == 4)
                                FaceCount = FaceCount / 6;
                            if (faceType == 9)
                                FaceCount = FaceCount / 12;
                            for (int face = 0; face < FaceCount; face++)
                            {
                                for(int sF = 0; sF < 3; sF++) { 
                                if (faceType == 4)
                                    faces.Add(f.readShort());
                                if (faceType == 9)
                                    faces.Add(f.readInt());
                                }
                                model.faces.Add(faces);
                            }
                        }

                    }
                }
                models.Add(model);
                f.seek(NextFMDL);
            }

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
        public class FTEX_Texture
        {
            public int width, height, type;
            public byte[] data;
            // for display only
            public int display = 0;
        }
        public class Vertex
        {
            public Vector3 pos = new Vector3(0, 0, 0), nrm = new Vector3(0, 0, 0);
            public Vector4 col = new Vector4(127, 127, 127, 127);
            public List<Vector2> tx = new List<Vector2>();
            public List<int> node = new List<int>();
            public List<float> weight = new List<float>();
        }
        public class FMDL_Model
        {

            public VBN skeleton = new VBN();
            public List<List<int>> faces = new List<List<int>>();
            public List<Vertex> vertices = new List<Vertex>();
            
            public string name;

            public bool isVisible = true;
        }


    }
}
