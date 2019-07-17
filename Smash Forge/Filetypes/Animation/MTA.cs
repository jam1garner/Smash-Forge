using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SmashForge
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
            defaultTexId = f.ReadInt();
            int keyframeCount = f.ReadInt();
            int keyframeOffset = f.ReadInt();
            frameCount = f.ReadInt() + 1;
            unknown = f.ReadInt();
            if(keyframeOffset != f.Eof())
            {
                f.Seek(keyframeOffset);
                for(int i = 0; i < keyframeCount; i++)
                {
                    temp.texId = f.ReadInt();
                    temp.frameNum = f.ReadInt();
                    keyframes.Add(temp);
                }
            } 
        }

        public byte[] Rebuild(int pos)
        {
            FileOutput f = new FileOutput();
            f.endian = Endianness.Big;

            f.WriteInt(pos + f.Pos() + 0x8);
            f.WriteInt(0);
            f.WriteInt(defaultTexId);
            f.WriteInt(keyframes.Count);
            f.WriteInt(pos + f.Pos() + 0x1C);
            f.WriteInt(frameCount - 1);
            f.WriteInt(unknown);
            f.WriteBytes(new byte[0x10]);
            foreach(keyframe k in keyframes)
            {
                f.WriteInt(k.texId);
                f.WriteInt(k.frameNum);
            }

            return f.GetBytes();
        }
    }

    public class MatData
    {
        public enum AnimType : ushort
        {
            NONE = 0,
            FULL = 1,
            CONST = 2,
        }

        public class frame
        {
            //public int size;
            public float[] values;
        }

        public string name;
        public List<frame> frames = new List<frame>();
        public int unknown, unknown2;
        public ushort animType;
        public int valueCount;

        public MatData(){ }

        public void read(FileData f)
        {
            int nameOff = f.ReadInt();
            unknown = f.ReadInt();
            valueCount = f.ReadInt();
            int frameCount = f.ReadInt();
            unknown2 = f.ReadUShort();
            animType = f.ReadUShort();
            int dataOff = f.ReadInt();
            f.Seek(nameOff);
            name = f.ReadString();
            f.Seek(dataOff);
            for(int i = 0; i < frameCount; i++)
            {
                frame temp = new frame();
                //temp.size = valueCount;
                temp.values = new float[valueCount];
                for (int j = 0; j < valueCount; j++)
                    temp.values[j] = f.ReadFloat();
                frames.Add(temp);
            }
        }

        public byte[] Rebuild(int pos)
        {
            FileOutput f = new FileOutput();
            f.endian = Endianness.Big;

            f.WriteInt(pos + f.Pos() + 0x20);
            f.WriteInt(unknown);
            f.WriteInt(valueCount);
            f.WriteInt(frames.Count);
            f.WriteShort(unknown2);
            f.WriteShort(animType);
            int position = pos + f.Pos() + 0xC + name.Length + 1;
            while (position % 0x10 != 0)
                position++;

            f.WriteInt(position);
            f.WriteBytes(new byte[8]);
            f.WriteString(name);
            f.WriteByte(0);
            while ((pos + f.Pos()) % 0x10 != 0)
                f.WriteByte(0);

            foreach (frame fr in frames)
                for (int i = 0; i < valueCount; i++)
                    f.WriteFloat(fr.values[i]);
            f.WriteBytes(new byte[0x10]);

            return f.GetBytes();
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
            int nameOffset = f.ReadInt();
            matHash = f.ReadInt();
            int propertyCount = f.ReadInt();
            int propertyPos = f.ReadInt();
            hasPat = (0 != f.ReadByte());
            f.Skip(3);
            int patOffset = f.ReadInt();
            int secondNameOff = f.ReadInt();
            matHash2 = f.ReadInt();

            f.Seek(nameOffset);
            name = f.ReadString();

            if(secondNameOff != 0)
            {
                f.Seek(secondNameOff);
                name2 = f.ReadString();
            }

            if (hasPat)
            {
                f.Seek(patOffset);
                int patDataPos = f.ReadInt();
                if (patDataPos != 0)
                {
                    f.Seek(patDataPos);
                    pat0.read(f);
                }
            }
            f.Seek(propertyPos);
            for(int i = 0; i < propertyCount; i++)
            {
                int propOffset = f.ReadInt();
                int returnPos = f.Pos();
                f.Seek(propOffset);
                MatData temp = new MatData();
                temp.read(f);
                properties.Add(temp);
                f.Seek(returnPos);
            }
        }

        public byte[] Rebuild(int pos)
        {
            FileOutput f = new FileOutput();
            f.endian = Endianness.Big;

            f.WriteInt(pos + f.Pos() + 0x20);
            f.WriteInt(matHash);
            f.WriteInt(properties.Count);
            int nameOffset = pos + f.Pos() + 0x15 + name.Length;
            while (nameOffset % 4 != 0)
                nameOffset++;
            f.WriteInt(nameOffset);
            f.WriteFlag(hasPat);
            f.WriteBytes(new byte[3]);
            //Write all the mat data into a buffer (g) then write pat offset
            int pos2 = pos + f.Pos() + 4;
            FileOutput g = new FileOutput();
            g.endian = Endianness.Big;

            if (matHash2 != 0)
            {
                g.WriteInt(pos2 + g.Pos() + 0x8);
                g.WriteInt(matHash);
            }
            else
            {
                g.WriteBytes(new byte[8]);
            }

            g.WriteString(name);
            g.WriteByte(0);
            while ((pos2 + g.Pos()) % 0x10 != 0)
                g.WriteByte(0);

            int position = pos2 + g.Pos() + properties.Count * 4;
            while (position % 16 != 0)
                position++;

            List<byte[]> builtProperties = new List<byte[]>();
            foreach (MatData prop in properties)
            {
                g.WriteInt(position);
                byte[] b = prop.Rebuild(position);
                builtProperties.Add(b);
                position += b.Length;
                while (position % 16 != 0)
                    position++;
            }

            while ((pos2 + g.Pos()) % 16 != 0)
                g.WriteByte(0);

            foreach (byte[] b in builtProperties)
            {
                g.WriteBytes(b);
                while ((pos2 + g.Pos()) % 16 != 0)
                    g.WriteByte(0);
            }

            f.WriteInt(pos2 + g.Pos());
            f.WriteBytes(g.GetBytes());
            if(hasPat)
                f.WriteBytes(pat0.Rebuild(f.Pos()));

            return f.GetBytes();
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
            int nameOff = f.ReadInt();
            unk1 = (f.ReadInt() != 0);
            int dataOff = f.ReadInt();
            f.Seek(nameOff);
            name = f.ReadString();
            f.Seek(dataOff);
            frameCount = f.ReadInt();
            unk2 = (f.ReadUShort() != 0);
            short keyframeCount = f.ReadShort();
            int keyframeOffset = f.ReadInt();
            f.Seek(keyframeOffset);
            frame tempFrame;
            for (int i = 0; i < keyframeCount; i++)
            {
                tempFrame.frameNum = f.ReadShort();
                tempFrame.state = f.ReadByte();
                tempFrame.unknown = f.ReadByte();
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
            f.endian = Endianness.Big;

            f.WriteInt(pos + f.Pos() + 0x20);
            f.WriteInt(Convert.ToInt32(unk1));
            int offset = pos + f.Pos() + 0x18;
            offset += name.Length + 1;
            while (offset % 16 != 0)
                offset++;
            offset += 0x10;
            f.WriteInt(offset);
            f.WriteBytes(new byte[0x14]);
            f.WriteString(name);
            f.WriteByte(0);
            while ((pos + f.Pos()) % 16 != 0)
                f.WriteByte(0);
            f.WriteBytes(new byte[0x10]);
            f.WriteInt(frameCount);
            f.WriteShort(Convert.ToInt32(unk2));
            f.WriteShort(frames.Count);
            f.WriteInt(pos + f.Pos() + 0x18);
            f.WriteBytes(new byte[0x14]);
            foreach(frame keyframe in frames)
            {
                f.WriteShort(keyframe.frameNum);
                f.WriteByte(keyframe.state);
                f.WriteByte(keyframe.unknown);
            }

            return f.GetBytes();
        }
    }

    public class MTA : FileBase
    {
        public uint unknown;
        public uint frameCount;
        public uint startFrame;
        public uint endFrame;
        public uint frameRate;

        public List<MatEntry> matEntries = new List<MatEntry>();
        public List<VisEntry> visEntries = new List<VisEntry>();

        MtaEditor Editor;

        public MTA()
        {
            Text = "model.mta";
            Endian = Endianness.Big;
            ImageKey = "image";
            SelectedImageKey = "image";

            unknown = 0;
            frameCount = 0;
            startFrame = 0;
            endFrame = 0;
            frameRate = 60;

            ContextMenu = new ContextMenu();

            MenuItem OpenEdit = new MenuItem("Open Editor");
            OpenEdit.Click += OpenEditor;
            ContextMenu.MenuItems.Add(OpenEdit);

            /*MenuItem save = new MenuItem("Save As");
            ContextMenu.MenuItems.Add(save);
            save.Click += Save;*/
        }

        public MTA(string Name) : base()
        {
            Text = Name;
        }

        private void OpenEditor(object sender, EventArgs args)
        {
            if (Editor == null || Editor.IsDisposed)
            {
                Editor = new MtaEditor(this);
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
            f.endian = Endian;
            if (f.Size() < 4)
                return;

            f.Seek(4);
            unknown = f.ReadUInt();
            frameCount = f.ReadUInt();
            startFrame = f.ReadUInt();
            endFrame = f.ReadUInt();
            frameRate = f.ReadUInt();
            int matCount = f.ReadInt();
            int matOffset = f.ReadInt();
            int visCount = f.ReadInt();
            int visOffset = f.ReadInt();
            int returnPos;
            f.Seek(matOffset);
            for (int i = 0; i < matCount; i++)
            {
                returnPos = f.Pos() + 4;
                f.Seek(f.ReadInt());
                MatEntry tempMatEntry = new MatEntry();
                tempMatEntry.read(f);
                matEntries.Add(tempMatEntry);
                f.Seek(returnPos);
            }
            f.Seek(visOffset);
            for (int i = 0; i < visCount; i++)
            {
                returnPos = f.Pos() + 4;
                f.Seek(f.ReadInt());
                VisEntry tempVisEntry = new VisEntry();
                tempVisEntry.read(f);
                visEntries.Add(tempVisEntry);
                f.Seek(returnPos);
            }
        }

        public override byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.endian = Endianness.Big;

            f.WriteString("MTA4");
            f.WriteUInt(unknown);
            f.WriteUInt(frameCount);
            f.WriteUInt(startFrame);
            f.WriteUInt(endFrame);
            f.WriteUInt(frameRate);

            f.WriteInt(matEntries.Count);
            if (matEntries.Count > 0)
                f.WriteInt(0x38);
            else
                f.WriteInt(0);
            f.WriteInt(visEntries.Count);
            if (visEntries.Count > 0)
                f.WriteInt(0x38 + 4 * matEntries.Count);
            else
                f.WriteInt(0);
            for (int i = 0; i < 0x10; i++)
                f.WriteByte(0);

            List<byte[]> matEntriesBuilt = new List<byte[]>();
            List<byte[]> visEntriesBuilt = new List<byte[]>();

            int position = 0x38 + matEntries.Count + visEntries.Count;
            while (position % 0x10 != 0)
                position++;

            foreach (MatEntry m in matEntries)
            {
                byte[] b = m.Rebuild(position);
                matEntriesBuilt.Add(b);
                f.WriteInt(position);
                position += b.Length;
                while (position % 0x10 != 0)
                    position++;
            }

            foreach (VisEntry v in visEntries)
            {
                byte[] b = v.Rebuild(position);
                matEntriesBuilt.Add(b);
                f.WriteInt(position);
                position += b.Length;
                while (position % 0x10 != 0)
                    position++;
            }

            while (f.Pos() % 0x10 != 0)
                f.WriteByte(0);

            foreach(byte[] b in matEntriesBuilt)
            {
                f.WriteBytes(b);
                while (f.Pos() % 0x10 != 0)
                    f.WriteByte(0);
            }

            foreach (byte[] b in visEntriesBuilt)
            {
                f.WriteBytes(b);
                while (f.Pos() % 0x10 != 0)
                    f.WriteByte(0);
            }

            return f.GetBytes();
        }

        public string Decompile()
        {
            string f = "";

            f += "Header\n";
            f += $"Header_Unknown,{unknown}\n";
            f += $"Frame Count,{frameCount}\n";
            f += $"Frame Rate,{frameRate}\n";
            foreach(MatEntry matEntry in matEntries)
            {
                f += "--------------------------------------\n";
                f += "Material\n";
                f += matEntry.name + '\n';
                f += $"Material Hash,{matEntry.matHash.ToString("X")}\n";
                f += $"Has PAT0,{matEntry.hasPat}\n";
                if (matEntry.matHash2 != 0)
                    f += $"Second Material Hash,{matEntry.matHash2.ToString("X")}\n";
                f += "###\n";

                foreach(MatData matProp in matEntry.properties)
                {
                    f += "Material Property\n";
                    f += matProp.name + '\n';
                    f += $"MatProp_Unk1,{matProp.unknown}\n";
                    f += $"MatProp_Unk2,{matProp.unknown2}\n";
                    f += $"Animation Type,{matProp.animType}\n";
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
            matEntries.Clear();
            visEntries.Clear();
            Nodes.Clear();
            unknown = Convert.ToUInt32(f[1].Split(',')[1]);
            frameCount = Convert.ToUInt32(f[2].Split(',')[1]);
            startFrame = 0;
            endFrame = frameCount - 1;
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
                                    md.animType = Convert.ToUInt16(f[l++].Split(',')[1]);
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
