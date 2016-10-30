using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smash_Forge
{
    //self.nameOffset = uint32(mta)
    //        self.matHash = uint32(mta)
    //        self.materialPropCount = uint32(mta)
    //        self.materialDPos = uint32(mta)
    //        self.hasPat = ord(mta.read(1))
    //        mta.seek(3,1)
    //        self.patOffset = uint32(mta)
    //        self.name2Offset = uint32(mta)
    //        self.matHash2 = uint32(mta)
    //        mta.seek(self.nameOffset)
    //        self.name = nullEndString(mta)
    //        if self.name2Offset != 0:
    //            mta.seek(self.name2Offset)
    //            self.name2 = nullEndString(mta)
    //        self.patData = None
    //        if self.hasPat:
    //            mta.seek(self.patOffset)
    //            self.patDataPos = uint32(mta)
    //            if self.patDataPos != 0:
    //                mta.seek(self.patDataPos)
    //                self.patData = PatData(mta)
    //        mta.seek(self.materialDPos)
    //        self.mtaProps = []
    //        if self.materialPropCount > 0:
    //            self.matDataPos = []
    //            for i in range(self.materialPropCount):
    //                self.matDataPos.append(uint32(mta))
    //            for off in self.matDataPos:
    //                mta.seek(off)
    //                self.mtaProps.append(MatData(mta))
    public class PatData
    {
        public struct keyframe
        {
            public int frameNum;
            public int texId;
        }

        public int defaultTexId;
        public int unknown;
        public List<keyframe> keyframes = new List<keyframe>();

        public PatData() { }

        public int getTexId(int frame)
        {
            int lastTexId = defaultTexId;
            for(int i = 0; i < keyframes.Count; i++)
            {
                if (frame < keyframes[i].frameNum)
                    return lastTexId;
                else
                    lastTexId = keyframes[i].texId;
            }
            return lastTexId;
        }

        public void read(FileData f)
        {
            keyframe temp;
            defaultTexId = f.readInt();
            int keyframeCount = f.readInt();
            int keyframeOffset = f.readInt();
            f.skip(4);//numFramesMinusOne (again)
            unknown = f.readInt();
            if(keyframeOffset != f.eof())
            {
                f.seek(keyframeOffset);
                for(int i = 0;i < keyframeCount;i++) {
                    temp.texId = f.readInt();
                    temp.frameNum = f.readInt();
                    keyframes.Add(temp);
                }
            } 
        }
    }

    public class MatData
    {
        public struct frame
        {
            public int size;
            public float[] values;
        }

        public string name;
        public List<frame> frames = new List<frame>();
        public int unknown, unknown2, unknown3;

        public MatData(){ }

        public void read(FileData f)
        {
            int nameOff = f.readInt();
            unknown = f.readInt();
            int valueCount = f.readInt();
            int frameCount = f.readInt();
            unknown2 = f.readShort();
            unknown3 = f.readShort();
            int dataOff = f.readInt();
            f.seek(nameOff);
            name = f.readString();
            f.seek(dataOff);
            for(int i = 0; i < frameCount; i++)
            {
                frame temp;
                temp.size = valueCount;
                temp.values = new float[valueCount];
                for (int j = 0; j < valueCount; j++)
                    temp.values[j] = f.readFloat();
            }
        }
    }

    public class MatEntry
    {
        public int matHash;
        public int matHash2;
        public bool hasPat;
        public string name;
        public string name2;
        public PatData pat0 = new PatData();
        public List<MatData> properties = new List<MatData>();

        public MatEntry() { }

        public void read(FileData f)
        {
            int nameOffset = f.readInt();
            matHash = f.readInt();
            int propertyCount = f.readInt();
            int propertyPos = f.readInt();
            hasPat = (0 != f.readByte());
            f.skip(3);
            int patOffset = f.readInt();
            int secondNameOff = f.readInt();
            matHash2 = f.readInt();
            if(secondNameOff != 0)
            {
                f.seek(secondNameOff);
                name2 = f.readString();
            }

            if (hasPat)
            {
                f.seek(patOffset);
                int patDataPos = f.readInt();
                if (patDataPos != 0)
                {
                    f.seek(patDataPos);
                    pat0.read(f);
                }
            }
            f.seek(propertyPos);
            for(int i = 0; i < propertyCount; i++)
            {
                int propOffset = f.readInt();
                int returnPos = f.pos();
                f.seek(propOffset);
                MatData temp = new MatData();
                temp.read(f);
                properties.Add(temp);
                f.seek(returnPos);
            }
            
        }
    }

    public class VisEntry
    {
        public struct frame
        {
            public short frameNum;
            public byte state;
            public byte unknown;
        }

        int unk1;
        short unk2;
        int frameCount;
        public string name;
        List<frame> frames = new List<frame>();

        public VisEntry() { }

        public void read(FileData f)
        {
            int nameOff = f.readInt();
            unk1 = f.readInt();
            int dataOff = f.readInt();
            f.seek(nameOff);
            name = f.readString();
            f.seek(dataOff);
            frameCount = f.readInt();
            unk2 = (short)f.readShort();
            short keyframeCount = (short)f.readShort();
            int keyframeOffset = f.readInt();
            f.seek(keyframeOffset);
            frame tempFrame;
            for (int i = 0; i < keyframeCount; i++)
            {
                tempFrame.frameNum = (short)f.readShort();
                tempFrame.state = (byte)f.readByte();
                tempFrame.unknown = (byte)f.readByte();
                frames.Add(tempFrame);
                tempFrame = new frame();
            }
        }

        public int getState(int frame){
            int state = -1;
            foreach (frame f in frames)
            {
                if (f.frameNum > frame)
                {
                    break;
                }
                state = f.state;
            }
            return state;
        }
    }

    public class MTA
    {
        public uint unknown;
        public uint numFrames;
        public uint frameRate;
        public List<MatEntry> matEntries = new List<MatEntry>();
        public List<VisEntry> visEntries = new List<VisEntry>();

        public MTA() { }
        
        public void read(FileData f)
        {
            f.Endian = System.IO.Endianness.Big;
            f.seek(4);
            unknown = (uint)f.readInt();
            numFrames = (uint)f.readInt();
            f.skip(8);
            frameRate = (uint)f.readInt();
            int matCount = f.readInt();
            int matOffset = f.readInt();
            int visCount = f.readInt();
            int visOffset = f.readInt();
            int returnPos;
            f.seek(matOffset);
            for (int i = 0;i < matCount; i++)
            {
                returnPos = f.pos()+4;
                f.seek(f.readInt());
                MatEntry tempMatEntry = new MatEntry();
                tempMatEntry.read(f);
                matEntries.Add(tempMatEntry);
                f.seek(returnPos);
            }
            f.seek(visOffset);
            for (int i = 0; i < visCount; i++)
            {
                returnPos = f.pos() + 4;
                f.seek(f.readInt());
                VisEntry tempVisEntry = new VisEntry();
                tempVisEntry.read(f);
                visEntries.Add(tempVisEntry);
                f.seek(returnPos);
            }
        }
    }
    
}
