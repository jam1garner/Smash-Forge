using System;
using System.Collections.Generic;
using System.IO;

namespace SmashForge
{
    public class NUS3BANK : FileBase
    {
        public override Endianness Endian
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public NusProp prop;
        public NusBinf binf;
        public NusGrp grp;
        public NusDton dton;
        public NUS_TONE tone;

        public override string ToString()
        {
            return binf.name;
        }

        public NUS3BANK()
        {
            prop = new NusProp();
            binf = new NusBinf();
            grp = new NusGrp();
            dton = new NusDton();
            tone = new NUS_TONE();
        }

        public override void Read(string filename)
        {
            FileData d = new FileData(filename);
            d.endian = Endianness.Little;

            if (d.Magic().Equals("3SUN"))
                throw new Exception("Not a valid nus3bank");

            d.Seek(4);
            int filesize = d.ReadInt();

            d.Skip(8); // BANKTOC 
            int headerSize = 0x14 + d.ReadInt();
            int secCount = d.ReadInt();

            for (int i = 0; i < secCount; i++)
            {
                string magic = d.ReadString(d.Pos(), 4);
                d.Skip(4);
                int size = d.ReadInt();

                int temp = d.Pos();
                d.Seek(headerSize);
                Console.WriteLine(magic + " " + d.Pos().ToString("x"));
                if (magic.Equals("PROP"))
                    prop.Read(d);
                if (magic.Equals("BINF"))
                    binf.Read(d);
                if (magic.Equals("GRP "))
                    grp.Read(d);
                if (magic.Equals("DTON"))
                    dton.Read(d);
                if (magic.Equals("TONE"))
                    tone.Read(d);
                if (magic.Equals("PACK"))
                    tone.ReadPACK(d);
                headerSize += size + 8;
                d.Seek(temp);
            }

        }

        public override byte[] Rebuild()
        {
            FileOutput o = new FileOutput();
            o.endian = Endianness.Little;

            FileOutput d = new FileOutput();
            d.endian = Endianness.Little;

            o.WriteString("NUS3");
            o.WriteInt(0);

            o.WriteString("BANKTOC ");
            o.WriteInt(0x3C);
            o.WriteInt(0x07);

            // write each section
            o.WriteString("PROP");
            o.WriteInt(prop.Rebuild(d));

            o.WriteString("BINF");
            o.WriteInt(binf.Rebuild(d));

            o.WriteString("GRP ");
            o.WriteInt(grp.Rebuild(d));

            o.WriteString("DTON");
            o.WriteInt(dton.Rebuild(d));

            o.WriteString("TONE");
            o.WriteInt(tone.Rebuild(d));

            o.WriteString("JUNK");
            o.WriteInt(4);
            //d.writeString("JUNK");
            d.WriteInt(4);
            d.WriteInt(0);

            o.WriteString("PACK");
            o.WriteInt(0);

            o.WriteOutput(d);

            o.WriteIntAt(o.Size(), 4);

            // something extra with bgm??

            return o.GetBytes();
        }


        // class system
        public class NusProp
        {
            public string project = "DefaultProject";
            public string timestamp = "2014/10/06 03:02:28";
            public int unk1 = 0xF1;
            public int unk2 = 0x3;
            public int unk3 = 0x8;

            public void Read(FileData d)
            {
                d.Skip(8);// magic and section size
                d.Skip(4); // 0 padding?
                unk1 = d.ReadInt();
                d.Skip(2); //0
                unk2 = d.ReadUShort();

                int ssize = d.ReadByte();
                project = d.ReadString(d.Pos(), ssize - 1);
                d.Skip(ssize - 1);
                d.Skip(6);
                unk3 = d.ReadUShort();
                d.Align(4);
                ssize = d.ReadByte();
                timestamp = d.ReadString(d.Pos(), ssize - 1);
                d.Skip(ssize - 1);
                d.Skip(4);
            }

            public int Rebuild(FileOutput o)
            {
                o.WriteString("PROP");
                int sizeoff = o.Size();
                o.WriteInt(0);
                int size = o.Size();
                o.WriteInt(0);
                o.WriteInt(unk1);
                o.WriteShort(0);
                o.WriteShort(unk2);

                o.WriteByte(project.Length + 1);
                o.WriteString(project);
                o.WriteByte(0);
                o.WriteByte(0);
                o.WriteByte(0);
                o.WriteByte(0);
                o.Align(4);
                o.WriteShort(unk3);
                o.Align(4);


                o.WriteByte(timestamp.Length + 1);
                o.WriteString(timestamp);
                o.WriteByte(0);
                o.WriteByte(0);
                o.WriteByte(0);
                o.WriteByte(0);
                o.Align(4);

                size = o.Size() - size;
                o.WriteIntAt(size, sizeoff);
                return size;
            }
        }

        public class NusBinf
        {
            public int unk1 = 3;
            public int flag = 0x05;
            public string name = "snd_bgm_CRS01_Menu";

            public void Read(FileData d)
            {
                d.Skip(12);
                unk1 = d.ReadInt();

                int s = d.ReadByte();
                name = d.ReadString(d.Pos(), s - 1);
                d.Skip(s);
                d.Align(4);
                flag = d.ReadInt();
            }

            public int Rebuild(FileOutput o)
            {
                o.WriteString("BINF");
                int sizeoff = o.Size();
                o.WriteInt(0);
                int size = o.Size();

                o.WriteInt(0);
                o.WriteInt(unk1);

                o.WriteByte(name.Length + 1);
                o.WriteString(name);
                o.WriteByte(0);
                o.Align(4);
                o.WriteInt(flag);

                size = o.Size() - size;
                o.WriteIntAt(size, sizeoff);
                return size;

            }
        }

        public class NusGrp
        {
            public List<string> names = new List<string>();

            public NusGrp()
            {
                //TODO: Set up default
            }

            public void Read(FileData d)
            {
                names.Clear();
                d.Skip(8);// magic and section size
                int c1 = d.ReadInt();

                int start = d.Pos();
                for (int i = 0; i < c1; i++)
                {
                    int offset = d.ReadInt();
                    int size = d.ReadInt();

                    int temp = d.Pos();
                    d.Seek(start + offset);

                    d.ReadInt();
                    int s = (sbyte)d.ReadByte();
                    names.Add(d.ReadString(d.Pos(), -1));
                    d.Skip(s);
                    d.Align(4);


                    d.Seek(temp);
                }
            }

            public int Rebuild(FileOutput o)
            {
                o.WriteString("GRP ");
                int sizeoff = o.Size();
                o.WriteInt(0);
                int size = o.Size();

                o.WriteInt(names.Count);

                int start = names.Count * 8 + 4;
                FileOutput name = new FileOutput();
                name.endian = Endianness.Little;

                int c = 0;
                foreach (string na in names)
                {
                    o.WriteInt(start + name.Size());

                    int ns = name.Pos();
                    name.WriteInt(1);
                    name.WriteByte(na.Length == 0 ? 0xFF : na.Length + 1);
                    name.WriteString(na);
                    name.WriteByte(0);
                    name.Align(4);
                    if (c != names.Count - 1)
                        name.WriteInt(0); // padding
                    else
                        ns -= 4;
                    c++;

                    o.WriteInt(name.Pos() - ns);
                }
                o.WriteInt(0);
                o.WriteOutput(name);

                size = o.Size() - size;
                o.WriteIntAt(size, sizeoff);
                return size;
            }
        }

        public class NusDton
        {
            public class ToneDes
            {
                public int hash, unk1 = 0x20C;
                public string name;
                public float[] data = new float[]
                {

                };

                public void Read(FileData d)
                {
                    hash = d.ReadInt();
                    unk1 = d.ReadInt();

                    int s = d.ReadByte();
                    name = d.ReadString(d.Pos(), s - 1);
                    d.Skip(s);
                    d.Align(4);
                    data = new float[0x2c];
                    for (int i = 0; i < 0x2c; i++)
                    {
                        data[i] = d.ReadFloat();
                    }
                }

                public int Rebuild(FileOutput o)
                {
                    int size = o.Size();
                    o.WriteInt(hash);
                    o.WriteInt(unk1);
                    o.WriteByte(name.Length + 1);
                    o.WriteString(name);
                    o.WriteByte(0);
                    o.Align(4);

                    // write data
                    foreach (float f in data)
                        o.WriteFloat(f);

                    return o.Size() - size;
                }
            }

            List<ToneDes> destone = new List<ToneDes>();

            public NusDton()
            {
                //TODO: Setup default
            }

            public void Read(FileData d)
            {
                destone.Clear();
                d.Skip(8);// magic and section size
                int count = d.ReadInt();
                int start = d.Pos();

                for (int i = 0; i < count; i++)
                {
                    int offset = d.ReadInt();
                    int size = d.ReadInt();

                    int temp = d.Pos();
                    d.Seek(start + offset);
                    ToneDes des = new ToneDes();
                    des.Read(d);
                    destone.Add(des);

                    d.Seek(temp);
                }
            }

            public int Rebuild(FileOutput o)
            {
                o.WriteString("DTON");
                int sizeoff = o.Size();
                o.WriteInt(0);
                int size = o.Size();

                o.WriteInt(destone.Count);

                FileOutput dat = new FileOutput();
                dat.endian = Endianness.Little;

                int start = destone.Count * 8 + 4;

                for (int i = 0; i < destone.Count; i++)
                {
                    o.WriteInt(start + dat.Size());
                    o.WriteInt(destone[i].Rebuild(dat) + 4);

                    if (i != destone.Count - 1)
                        dat.WriteInt(0);
                }

                o.WriteInt(0);
                o.WriteOutput(dat);

                size = o.Size() - size;
                o.WriteIntAt(size, sizeoff);
                return size;
            }
        }

        public class NUS_TONE
        {

            public class ToneMeta
            {
                public int hash;
                public int unk1;
                public string name;
                public int id;

                public int offset, size;
                public byte[] idsp;

                public float[] param = new float[12];
                public int[] offsets;
                public float[] unkvalues;
                public int[] unkending;
                public int[] end = new int[9];

                public ToneMeta()
                {

                }

                public override string ToString()
                {
                    return name;
                }

                public void Play()
                {
                    // this cannot be very fast .-.
                    // if anyone know how to pass the file via a byte array then that would be great...
                    File.WriteAllBytes("temp.idsp", idsp);
                    Console.WriteLine("here");
                    IntPtr vgm = VGMStreamNative.InitVGMStream("temp.idsp");
                    if (vgm == IntPtr.Zero) throw new Exception("Error loading idsp");

                    Console.WriteLine("here");
                    int channelCount = VGMStreamNative.GetVGMStreamChannelCount(vgm);
                    int bitsPerFrame = VGMStreamNative.GetVGMStreamFrameSize(vgm);
                    int size = VGMStreamNative.GetVGMStreamTotalSamples(vgm);
                    int samplerate = VGMStreamNative.GetVGMStreamSampleRate(vgm);
                    Console.WriteLine("here");

                    int total = (int)((samplerate * bitsPerFrame * channelCount * (size / 24576000f)) / 8 * 1024);

                    //Console.WriteLine(channelCount + " " + bitsPerFrame + " " + size + " " + samplerate + " " + total.ToString("x"));

                    short[] buffer = new short[total];

                    VGMStreamNative.RenderVGMStream(buffer, buffer.Length / 2, vgm);
                    Console.WriteLine("here");

                    FileOutput o = new FileOutput();
                    o.endian = Endianness.Little;
                    for (int i = 0; i < buffer.Length / 2; i++)
                        o.WriteShort(buffer[i]);
                    o.Save("test.wav");
                    Console.WriteLine("here");
                    WAVE.Play(o.GetBytes(), VGMStreamNative.GetVGMStreamChannelCount(vgm), VGMStreamNative.GetVGMStreamSamplesPerFrame(vgm), VGMStreamNative.GetVGMStreamSampleRate(vgm));

                    VGMStreamNative.CloseVGMStream(vgm);
                    File.Delete("temp.idsp");
                }

                public void Read(FileData d)
                {
                    hash = d.ReadInt();
                    unk1 = d.ReadInt();

                    int s = d.ReadByte();
                    name = d.ReadString(d.Pos(), -1);
                    d.Skip(s);
                    d.Align(4);

                    d.Skip(8);

                    offset = d.ReadInt();
                    size = d.ReadInt();

                    for (int i = 0; i < param.Length; i++)
                        param[i] = d.ReadFloat();

                    offsets = new int[d.ReadInt()];
                    for (int i = 0; i < offsets.Length; i++)
                        offsets[i] = d.ReadInt();

                    unkvalues = new float[d.ReadInt()];
                    for (int i = 0; i < unkvalues.Length; i++)
                    {
                        unkvalues[d.ReadInt()] = d.ReadFloat();
                    }

                    List<int> une = new List<int>();

                    while (true)
                    {
                        int i = d.ReadInt();
                        une.Add(i);
                        if (i == -1)
                            break;
                    }
                    unkending = une.ToArray();

                    end = new int[3 + (int)Math.Ceiling((double)((unk1 >> 8) & 0xFF) / 4)];

                    for (int i = 0; i < end.Length; i++)
                        end[i] = d.ReadInt();

                    //Console.WriteLine(id + " " + name + " " + offset.ToString("x"));

                }

                public int Rebuild(FileOutput o)
                {
                    int size = o.Size();

                    o.WriteInt(hash);
                    o.WriteInt(unk1);

                    o.WriteByte(name.Length + 1);
                    o.WriteString(name);
                    o.WriteByte(0);
                    o.Align(4);

                    o.WriteInt(0);
                    o.WriteInt(8);

                    o.WriteInt(offset);
                    o.WriteInt(this.size);

                    // write data
                    foreach (float f in param)
                        o.WriteFloat(f);

                    o.WriteInt(offsets.Length);
                    foreach (int f in offsets)
                        o.WriteInt(f);

                    o.WriteInt(unkvalues.Length);
                    int v = 0;
                    foreach (float f in unkvalues)
                    {
                        o.WriteInt(v++);
                        o.WriteFloat(f);
                    }

                    foreach (int f in unkending)
                        o.WriteInt(f);

                    foreach (int f in end)
                        o.WriteInt(f);

                    return o.Size() - size;
                }
            }

            public List<ToneMeta> tones = new List<ToneMeta>();

            public void Read(FileData d)
            {
                d.Skip(8);// magic and section size
                int count = d.ReadInt();

                int start = d.Pos();
                for (int i = 0; i < count; i++)
                {
                    int offset = d.ReadInt();
                    int size = d.ReadInt();

                    int temp = d.Pos();
                    d.Seek(offset + start);

                    ToneMeta meta = new ToneMeta();
                    meta.Read(d);
                    tones.Add(meta);

                    d.Seek(temp);
                }
            }

            public int Rebuild(FileOutput o)
            {
                o.WriteString("TONE");
                int sizeoff = o.Size();
                o.WriteInt(0);
                int size = o.Size();

                o.WriteInt(tones.Count);

                FileOutput dat = new FileOutput();
                dat.endian = Endianness.Little;

                int start = tones.Count * 8 + 4;

                for (int i = 0; i < tones.Count; i++)
                {
                    o.WriteInt(start + dat.Size());
                    o.WriteInt(tones[i].Rebuild(dat));
                }

                o.WriteInt(0);
                o.WriteOutput(dat);

                size = o.Size() - size - 4;
                o.WriteIntAt(size, sizeoff);
                return size;
            }


            public void ReadPACK(FileData d)
            {
                int start = d.Pos();

                foreach (ToneMeta meta in tones)
                {
                    meta.idsp = d.GetSection(start + meta.offset + 8, meta.size);
                }
            }

            public int RebuildPACK(FileOutput o)
            {
                return 0;
            }
        }

    }
}
