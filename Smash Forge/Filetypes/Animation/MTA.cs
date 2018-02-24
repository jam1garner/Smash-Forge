using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace Smash_Forge
{
    public class PatData
    {
        public struct keyframe
        {
            public int frameNum;
            public int texId;
        }

        public int defaultTexId;
        public int unknown;
        public int frameCount;
        public List<keyframe> keyframes = new List<keyframe>();

        public PatData() { }

        public int getTexId(int frame)
        {
            frame = frame % frameCount;
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
            frameCount = f.readInt() + 1;
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

        public byte[] Rebuild(int pos)
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            f.writeInt(pos + f.pos() + 0x8);
            f.writeInt(0);
            f.writeInt(defaultTexId);
            f.writeInt(keyframes.Count);
            f.writeInt(pos + f.pos() + 0x1C);
            f.writeInt(frameCount - 1);
            f.writeInt(unknown);
            f.writeBytes(new byte[0x10]);
            foreach(keyframe k in keyframes)
            {
                f.writeInt(k.texId);
                f.writeInt(k.frameNum);
            }

            return f.getBytes();
        }
    }

    public class MatData
    {
        public class frame
        {
            //public int size;
            public float[] values;
        }

        public string name;
        public List<frame> frames = new List<frame>();
        public int unknown, unknown2, unknown3;
        public int valueCount;

        public MatData(){ }

        public void read(FileData f)
        {
            int nameOff = f.readInt();
            unknown = f.readInt();
            valueCount = f.readInt();
            int frameCount = f.readInt();
            unknown2 = f.readShort();
            unknown3 = f.readShort();
            int dataOff = f.readInt();
            f.seek(nameOff);
            name = f.readString();
            f.seek(dataOff);
            for(int i = 0; i < frameCount; i++)
            {
                frame temp = new frame();
                //temp.size = valueCount;
                temp.values = new float[valueCount];
                for (int j = 0; j < valueCount; j++)
                    temp.values[j] = f.readFloat();
                frames.Add(temp);
            }
        }

        public byte[] Rebuild(int pos)
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            f.writeInt(pos + f.pos() + 0x20);
            f.writeInt(unknown);
            f.writeInt(valueCount);
            f.writeInt(frames.Count);
            f.writeShort(unknown2);
            f.writeShort(unknown3);
            int position = pos + f.pos() + 0xC + name.Length + 1;
            while (position % 0x10 != 0)
                position++;

            f.writeInt(position);
            f.writeBytes(new byte[8]);
            f.writeString(name);
            f.writeByte(0);
            while ((pos + f.pos()) % 0x10 != 0)
                f.writeByte(0);

            foreach (frame fr in frames)
                for (int i = 0; i < valueCount; i++)
                    f.writeFloat(fr.values[i]);
            f.writeBytes(new byte[0x10]);

            return f.getBytes();
        }
    }

    public class MatEntry : TreeNode
    {
        public int matHash;
        public int matHash2;
        public bool hasPat;
        public string name;
        public string name2;
        public PatData pat0 = new PatData();
        public List<MatData> properties = new List<MatData>();

        public MatEntry()
        {
            ImageKey = "image";
            SelectedImageKey = "image";
        }

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

            f.seek(nameOffset);
            name = f.readString();

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

        public byte[] Rebuild(int pos)
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            f.writeInt(pos + f.pos() + 0x20);
            f.writeInt(matHash);
            f.writeInt(properties.Count);
            int nameOffset = pos + f.pos() + 0x15 + name.Length;
            while (nameOffset % 4 != 0)
                nameOffset++;
            f.writeInt(nameOffset);
            f.writeFlag(hasPat);
            f.writeBytes(new byte[3]);
            //Write all the mat data into a buffer (g) then write pat offset
            int pos2 = pos + f.pos() + 4;
            FileOutput g = new FileOutput();
            g.Endian = Endianness.Big;

            if (matHash2 != 0)
            {
                g.writeInt(pos2 + g.pos() + 0x8);
                g.writeInt(matHash);
            }
            else
            {
                g.writeBytes(new byte[8]);
            }

            g.writeString(name);
            g.writeByte(0);
            while ((pos2 + g.pos()) % 0x10 != 0)
                g.writeByte(0);

            int position = pos2 + g.pos() + properties.Count * 4;
            while (position % 16 != 0)
                position++;

            List<byte[]> builtProperties = new List<byte[]>();
            foreach (MatData prop in properties)
            {
                g.writeInt(position);
                byte[] b = prop.Rebuild(position);
                builtProperties.Add(b);
                position += b.Length;
                while (position % 16 != 0)
                    position++;
            }

            while ((pos2 + g.pos()) % 16 != 0)
                g.writeByte(0);

            foreach (byte[] b in builtProperties)
            {
                g.writeBytes(b);
                while ((pos2 + g.pos()) % 16 != 0)
                    g.writeByte(0);
            }

            f.writeInt(pos2 + g.pos());
            f.writeBytes(g.getBytes());
            if(hasPat)
                f.writeBytes(pat0.Rebuild(f.pos()));

            return f.getBytes();
        }
    }

    public class VisEntry : TreeNode
    {
        public struct frame
        {
            public short frameNum;
            public byte state;
            public byte unknown;
        }

        public bool unk1;
        public bool unk2;
        public int frameCount;
        public string name;
        public List<frame> frames = new List<frame>();

        public VisEntry()
        {
            ImageKey = "image";
            SelectedImageKey = "image";
        }

        public void read(FileData f)
        {
            int nameOff = f.readInt();
            unk1 = (f.readInt() != 0);
            int dataOff = f.readInt();
            f.seek(nameOff);
            name = f.readString();
            f.seek(dataOff);
            frameCount = f.readInt();
            unk2 = (f.readShort() != 0);
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

        public byte[] Rebuild(int pos)
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            f.writeInt(pos + f.pos() + 0x20);
            f.writeInt(Convert.ToInt32(unk1));
            int offset = pos + f.pos() + 0x18;
            offset += name.Length + 1;
            while (offset % 16 != 0)
                offset++;
            offset += 0x10;
            f.writeInt(offset);
            f.writeBytes(new byte[0x14]);
            f.writeString(name);
            f.writeByte(0);
            while ((pos + f.pos()) % 16 != 0)
                f.writeByte(0);
            f.writeBytes(new byte[0x10]);
            f.writeInt(frameCount);
            f.writeShort(Convert.ToInt32(unk2));
            f.writeShort(frames.Count);
            f.writeInt(pos + f.pos() + 0x18);
            f.writeBytes(new byte[0x14]);
            foreach(frame keyframe in frames)
            {
                f.writeShort(keyframe.frameNum);
                f.writeByte(keyframe.state);
                f.writeByte(keyframe.unknown);
            }

            return f.getBytes();
        }
    }

    public class MTA : FileBase
    {
        public uint unknown;
        public uint numFrames;
        public uint frameRate;
        public List<MatEntry> matEntries = new List<MatEntry>();
        public List<VisEntry> visEntries = new List<VisEntry>();
        
        MTAEditor Editor;

        public MTA()
        {
            Text = "model.mta";
            Endian = Endianness.Big;
            ImageKey = "image";
            SelectedImageKey = "image";


            ContextMenu = new ContextMenu();

            MenuItem OpenEdit = new MenuItem("Open Editor");
            OpenEdit.Click += OpenEditor;
            ContextMenu.MenuItems.Add(OpenEdit);

            /*MenuItem save = new MenuItem("Save As");
            ContextMenu.MenuItems.Add(save);
            save.Click += Save;*/
        }


        private void OpenEditor(object sender, EventArgs args)
        {
            if (Editor == null || Editor.IsDisposed)
            {
                Editor = new MTAEditor(this);
                //Editor.FilePath = FilePath;
                Editor.Text = Parent.Text + "\\" + Text;
                MainForm.Instance.AddDockedControl(Editor);
            }
            else
            {
                Editor.BringToFront();
            }
        }

        public void ExpandNodes()
        {
            Nodes.Clear();
            TreeNode mat = new TreeNode();
            TreeNode vis = new TreeNode();
            foreach (MatEntry e in matEntries)
            {
                e.Text = e.name;
                mat.Nodes.Add(e);
            }
            foreach (VisEntry e in visEntries)
            {
                e.Text = e.name;
                vis.Nodes.Add(e);
            }
            Nodes.Add(mat);
            Nodes.Add(vis);
        }

        public override Endianness Endian { get; set; }

        public override void Read(string filename)
        {
            read(new FileData(filename));
        }

        public void read(FileData f)
        {
            f.Endian = Endian;
            if (f.size() < 4)
                return;
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
            for (int i = 0; i < matCount; i++)
            {
                returnPos = f.pos() + 4;
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

        public override byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            f.writeString("MTA4");
            f.writeInt((int)unknown);
            f.writeInt((int)numFrames);
            f.writeInt(0);
            f.writeInt((int)numFrames - 1);
            f.writeInt((int)frameRate);
            f.writeInt(matEntries.Count);
            if (matEntries.Count > 0)
                f.writeInt(0x38);
            else
                f.writeInt(0);
            f.writeInt(visEntries.Count);
            if (visEntries.Count > 0)
                f.writeInt(0x38 + 4 * matEntries.Count);
            else
                f.writeInt(0);
            for (int i = 0; i < 0x10; i++)
                f.writeByte(0);

            List<byte[]> matEntriesBuilt = new List<byte[]>();
            List<byte[]> visEntriesBuilt = new List<byte[]>();

            int position = 0x38 + matEntries.Count + visEntries.Count;
            while (position % 0x10 != 0)
                position++;

            foreach (MatEntry m in matEntries)
            {
                byte[] b = m.Rebuild(position);
                matEntriesBuilt.Add(b);
                f.writeInt(position);
                position += b.Length;
                while (position % 0x10 != 0)
                    position++;
            }

            foreach (VisEntry v in visEntries)
            {
                byte[] b = v.Rebuild(position);
                matEntriesBuilt.Add(b);
                f.writeInt(position);
                position += b.Length;
                while (position % 0x10 != 0)
                    position++;
            }

            while (f.pos() % 0x10 != 0)
                f.writeByte(0);

            foreach(byte[] b in matEntriesBuilt)
            {
                f.writeBytes(b);
                while (f.pos() % 0x10 != 0)
                    f.writeByte(0);
            }

            foreach (byte[] b in visEntriesBuilt)
            {
                f.writeBytes(b);
                while (f.pos() % 0x10 != 0)
                    f.writeByte(0);
            }

            return f.getBytes();
        }

        public string Decompile()
        {
            string f = "";

            f += "Header\n";
            f += $"Header_Unknown,{unknown}\n";
            f += $"Frame Count,{numFrames}\n";
            f += $"Frame rate,{frameRate}\n";
            foreach(MatEntry matEntry in matEntries)
            {
                f += "--------------------------------------\n";
                f += "Material\n";
                f += matEntry.name + '\n';
                f += $"Material Hash,{matEntry.matHash.ToString("X")}\n";
                f += $"Has PAT0,{matEntry.hasPat}\n";
                if (matEntry.matHash2 != 0)
                    f += $"Second Material Hash,{matEntry.matHash2.ToString("X")}";
                f += "###\n";

                foreach(MatData matProp in matEntry.properties)
                {
                    f += "Material Property\n";
                    f += matProp.name + '\n';
                    f += $"MatProp_Unk1,{matProp.unknown}\n";
                    f += $"MatProp_Unk2,{matProp.unknown2}\n";
                    f += $"MatProp_Unk3,{matProp.unknown3}\n";
                    f += "Compile Type (Baked or Keyed),Baked\n";
                    foreach(MatData.frame frame in matProp.frames)
                    {
                        int i = 0;
                        foreach(float value in frame.values)
                        {
                            f += value;
                            if (i == frame.values.Length - 1)
                                f += '\n';
                            else
                                f += ',';
                            i++;
                        }
                    }
                    f += "###\n";
                    
                }
                if (matEntry.pat0 != null)
                {
                    f += "PAT0\n";
                    f += $"Default TexId,{matEntry.pat0.defaultTexId.ToString("X")}\n";
                    f += $"Keyframe Count,{matEntry.pat0.keyframes.Count}\n";
                    f += $"PAT0_Unkown,{matEntry.pat0.unknown}\n";
                    foreach (PatData.keyframe keyframe in matEntry.pat0.keyframes)
                        f += $"frameNum,{keyframe.frameNum},texId,{keyframe.texId.ToString("X")}\n";
                }
            }

            foreach(VisEntry visEntry in visEntries)
            {
                f += "--------------------------------------\n";
                f += "VIS0\n";
                f += visEntry.name + '\n';
                f += $"Frame Count,{visEntry.frameCount}\n";
                f += $"Keyframe Count,{visEntry.frames.Count}\n";
                f += $"Is Constant,{visEntry.unk1}\n";
                f += $"Constant Value,{visEntry.unk2}\n";
                foreach (VisEntry.frame frame in visEntry.frames)
                    f += $"Frame,{frame.frameNum},State,{frame.state},unknown,{frame.unknown}\n";
            }
            f += "\n";

            return f;
        }

        public void Compile(List<string> f)
        {
            Nodes.Clear();
            unknown = Convert.ToUInt32(f[1].Split(',')[1]);
            numFrames = Convert.ToUInt32(f[2].Split(',')[1]);
            frameRate = Convert.ToUInt32(f[3].Split(',')[1]);
            int l = 3;
            try
            {
                while (l < f.Count)
                {
                    if (f[l++].StartsWith("---"))
                    {
                        if (l >= f.Count)
                            break;
                        if (f[l].StartsWith("Material"))
                        {
                            l++;
                            MatEntry m = new MatEntry();
                            m.name = f[l];
                            l++;
                            m.matHash = Convert.ToInt32(f[l].Split(',')[1], 16);
                            l++;
                            m.hasPat = Convert.ToBoolean(f[l].Split(',')[1]);
                            l++;
                            if (!f[l].StartsWith("###"))
                            {
                                m.matHash2 = Convert.ToInt32(f[l].Split(',')[1], 16);
                                l++;
                            }
                            while (l < f.Count - 1 && (f[l].StartsWith("###") || string.IsNullOrWhiteSpace(f[l])))
                            {
                                if (f[++l].StartsWith("Material Property"))
                                {
                                    l++;
                                    MatData md = new MatData();
                                    md.name = f[l++];
                                    md.unknown = Convert.ToInt32(f[l++].Split(',')[1]);
                                    md.unknown2 = Convert.ToInt32(f[l++].Split(',')[1]);
                                    md.unknown3 = Convert.ToInt32(f[l++].Split(',')[1]);
                                    bool keyed = (f[l++].Split(',')[1].Equals("Keyed") || f[l].Split(',')[1].Equals("keyed"));
                                    int lastFrame = 0;
                                    MatData.frame lastKeyframe = null;
                                    List<MatData.frame> frames = new List<MatData.frame>();
                                    while (l < f.Count && !f[l].StartsWith("---") && !f[l].StartsWith("###"))
                                    {
                                        if (keyed)
                                        {
                                            int currentFrame = frames.Count, fnum = Convert.ToInt32(f[l].Split(',')[0]);
                                            MatData.frame tempFrame = new MatData.frame();
                                            tempFrame.values = new float[f[l].Split(',').Length - 1];
                                            int i = 0;
                                            foreach (string value in (new List<string>(f[l].Split(',')).GetRange(1, f[l].Split(',').Length - 1)))
                                                tempFrame.values[i++] = Convert.ToSingle(value);
                                            if (lastKeyframe == null)
                                            {
                                                while (currentFrame <= fnum)
                                                {
                                                    frames.Add(tempFrame);
                                                    currentFrame = frames.Count;
                                                }
                                            }
                                            else
                                            {
                                                while (currentFrame <= fnum)
                                                {
                                                    List<float> thisFrame = new List<float>();
                                                    for (int k = 0; k < lastKeyframe.values.Length; k++)
                                                    {
                                                        float slope = (tempFrame.values[k] - lastKeyframe.values[k]) / (float)(fnum - lastFrame);
                                                        thisFrame.Add(lastKeyframe.values[k] + (slope * (currentFrame - lastFrame)));
                                                    }
                                                    frames.Add(new MatData.frame() { values = thisFrame.ToArray() });
                                                    currentFrame = frames.Count;
                                                }
                                            }
                                            lastFrame = fnum;
                                            lastKeyframe = tempFrame;
                                        }
                                        else
                                        {
                                            float[] values = new float[f[l].Split(',').Length];
                                            int i = 0;
                                            foreach (string value in f[l].Split(','))
                                                values[i++] = Convert.ToSingle(value);

                                            frames.Add(new MatData.frame() { values = values });
                                        }
                                        l++;
                                    }
                                    md.frames = frames;
                                    if (md.frames.Count > 0)
                                        md.valueCount = md.frames[0].values.Length;
                                    m.properties.Add(md);
                                }
                                else if (f[l + 1].StartsWith("PAT0"))
                                {
                                    l += 2;
                                    PatData p = new PatData();
                                    p.defaultTexId = Convert.ToInt32(f[l++].Split(',')[1]);
                                    int keyFrameCount = Convert.ToInt32(f[l++].Split(',')[1]);
                                    p.unknown = Convert.ToInt32(f[l++].Split(',')[1]);
                                    for (int i = 0; i < keyFrameCount; i++)
                                        p.keyframes.Add(new PatData.keyframe() { frameNum = Convert.ToInt32(f[l].Split(',')[1]), texId = Convert.ToInt32(f[l++].Split(',')[3], 16) });
                                    m.pat0 = p;
                                }

                            }
                            //while (l < f.Count && !f[l].StartsWith("---"))
                            //    l++;

                            matEntries.Add(m);
                        }
                        else if (f[l].StartsWith("VIS0"))
                        {
                            l++;
                            VisEntry v = new VisEntry();
                            v.name = f[l++];
                            v.frameCount = Convert.ToInt32(f[l++].Split(',')[1]);
                            int keyframeCount = Convert.ToInt32(f[l++].Split(',')[1]);
                            v.unk1 = Convert.ToBoolean(f[l++].Split(',')[1]);
                            v.unk2 = Convert.ToBoolean(f[l++].Split(',')[1]);
                            for (int i = 0; i < keyframeCount; i++)
                                v.frames.Add(new VisEntry.frame() { frameNum = Convert.ToInt16(f[l].Split(',')[1]), state = Convert.ToByte(f[l].Split(',')[3]), unknown = Convert.ToByte(f[l++].Split(',')[1]) });
                            visEntries.Add(v);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                MessageBox.Show($"Failed to build MTA\nError on line {l}\n{ex.ToString()}", "MTA Build Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //throw;
            }
        }
    }
}
