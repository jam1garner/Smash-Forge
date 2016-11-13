using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Smash_Forge
{
    public class FMDL
    {
        public Header HDR = new Header();
        public class Header
        {
            public int magic;
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
            public void Read(FileData f)
            {

            }
        }

        public class FVTX {
            public Header HDR = new Header();
            public List<attdata> AttrArr = new List<attdata>();
            public List<buffData> BuffArr = new List<buffData>();
            public List<List<Vector4>> Data = new List<List<Vector4>>();

            public class Header
            {
                public int magic = 0x46565458;//FVTX
                public int attCount;
                public int buffCount;
                public int sectIndx;
                public int vertCount;
                public int u1;
                public int attArrOff;
                public int attIndxOff;
                public int buffArrOff;
                public int padding;

                public void Read(FileData f)
                {

                    magic = f.readInt();
                    attCount = f.readByte();
                    buffCount = f.readByte();
                    sectIndx = f.readShort();
                    vertCount = f.readInt();
                    u1 = f.readInt();
                    attArrOff = f.readOffset();
                    attIndxOff = f.readOffset();
                    buffArrOff = f.readOffset();
                    padding = f.readInt();

                }
            }
            public class attdata
            {
                public string name;
                public int buffIndx;
                public int buffOff;
                public int vertType;
                public void Read(FileData f)
                {
                    name = f.readString(f.readOffset(), -1);
                    buffIndx = f.readByte();
                    f.skip(1);
                    buffOff = f.readShort();
                    vertType = f.readInt();
                }
            }
            public class buffData
            {
                public int u1 = 0;
                public int size;
                public int u2 = 0;
                public int stride;
                public int u3 = 1;
                public int u4 = 0;
                public int offset;
                public void Read(FileData f)
                {
                    u1 = f.readInt();
                    size = f.readInt();
                    u2 = f.readInt();
                    stride = f.readShort();
                    u3 = f.readShort();
                    u4 = f.readInt();
                    offset = f.readInt();
                }
            }
            public void Read(FileData f)
            {
                HDR.Read(f);
                f.seek(HDR.attArrOff);
                for(int att = 0; HDR.attCount > att; att++)
                {
                    attdata attData = new attdata();
                    attData.Read(f);
                    AttrArr.Add(attData);
                }
                f.seek(HDR.buffArrOff);
                for (int att = 0; HDR.buffCount > att; att++)
                {
                    attdata attData = new attdata();
                    attData.Read(f);
                    AttrArr.Add(attData);
                }
                for(int attr = 0;attr < HDR.attCount;attr++)
                {
                    List<Vector4> attData = new List<Vector4>();
                    for (int v = 0; v < HDR.vertCount; v++)
                    {
                        Vector4 vec = new Vector4(0, 0, 0, 0);
                        f.seek(((BuffArr[AttrArr[attr].buffIndx].offset) + (AttrArr[attr].buffOff) + (BuffArr[AttrArr[attr].buffIndx].stride * v)));
                        switch (AttrArr[attr].vertType)
                        {
                            case 0x100:
                                vec = new Vector4(f.readByte(), 0, 0, 0);
                                break;
                            case 0x4:
                            case 0x104:
                            case 0x204:
                                vec = new Vector4(f.readByte(), f.readByte(),0,0);
                                break;
                            case 0x7:
                            case 0x207:
                                vec = new Vector4(f.readShort(), f.readShort(), 0, 0);
                                break;
                            case 0x20A:
                            case 0x10A:
                            case 0xA:
                                vec = new Vector4(f.readByte(), f.readByte(), f.readByte(), f.readByte());
                                break;
                            case 0x20B:
                                int baseVALUE = f.readInt();
                                vec = new Vector4(((baseVALUE & 0x3FC00000) >> 22) / 511, ((baseVALUE & 0x000FF000) >> 12) / 511, ((baseVALUE & 0x000003FC) >> 2) / 511,0);
                                break;
                            case 0x808:
                                vec = new Vector4(f.readHalfFloat(), f.readHalfFloat(), 0, 0);
                                break;
                            case 0x80D:
                                vec = new Vector4(f.readFloat(), f.readFloat(), 0, 0);
                                break;
                            case 0x80F:
                                vec = new Vector4(f.readHalfFloat(), f.readHalfFloat(), f.readHalfFloat(), f.readHalfFloat());
                                break;
                            case 0x811:
                                vec = new Vector4(f.readFloat(), f.readFloat(),f.readFloat(), 0);
                                break;
                            case 0x813:
                                vec = new Vector4(f.readFloat(), f.readFloat(), f.readFloat(), f.readFloat());
                                break;
                        }
                        attData.Add(vec);
                    }
                    Data.Add(attData);
                }

            }
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
        public class FSKL
        {
            public class Header
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
    }
}
