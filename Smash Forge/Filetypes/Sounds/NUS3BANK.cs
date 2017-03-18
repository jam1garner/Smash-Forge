using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smash_Forge
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

        public NUS_PROP prop;
        public NUS_BINF binf;
        public NUS_GRP grp;
        public NUS_DTON dton;
        public NUS_TONE tone;

        public override string ToString()
        {
            return binf.name;
        }

        public NUS3BANK()
        {
            prop = new NUS_PROP();
            binf = new NUS_BINF();
            grp = new NUS_GRP();
            dton = new NUS_DTON();
            tone = new NUS_TONE();
        }

        public override void Read(string filename)
        {
            FileData d = new FileData(filename);
            d.Endian = Endianness.Little;

            if (d.Magic().Equals("3SUN"))
                throw new Exception("Not a valid nus3bank");

            d.seek(4);
            int filesize = d.readInt();

            d.skip(8); // BANKTOC 
            int headerSize = 0x14 + d.readInt();
            int secCount = d.readInt();

            for(int i = 0; i < secCount; i++)
            {
                string magic = d.readString(d.pos(), 4);
                d.skip(4);
                int size = d.readInt();

                int temp = d.pos();
                d.seek(headerSize);
                Console.WriteLine(magic + " " + d.pos().ToString("x"));
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
                d.seek(temp);
            }
            
        }

        public override byte[] Rebuild()
        {
            FileOutput o = new FileOutput();
            o.Endian = Endianness.Little;

            FileOutput d = new FileOutput();
            d.Endian = Endianness.Little;

            o.writeString("NUS3");
            o.writeInt(0);

            o.writeString("BANKTOC ");
            o.writeInt(0x3C);
            o.writeInt(0x07);

            // write each section
            o.writeString("PROP");
            o.writeInt(prop.Rebuild(d));

            o.writeString("BINF");
            o.writeInt(binf.Rebuild(d));

            o.writeString("GRP ");
            o.writeInt(grp.Rebuild(d));

            o.writeString("DTON");
            o.writeInt(dton.Rebuild(d));

            o.writeString("TONE");
            o.writeInt(tone.Rebuild(d));

            o.writeString("JUNK");
            o.writeInt(4);
            //d.writeString("JUNK");
            d.writeInt(4);
            d.writeInt(0);

            o.writeString("PACK");
            o.writeInt(0);

            o.writeOutput(d);
            
            o.writeIntAt(o.size(), 4);

            // something extra with bgm??

            return o.getBytes();
        }


        // class system
        public class NUS_PROP
        {
            public string project = "DefaultProject";
            public string timestamp = "2014/10/06 03:02:28";
            public int unk1 = 0xF1;
            public int unk2 = 0x3;
            public int unk3 = 0x8;

            public void Read(FileData d)
            {
                d.skip(8);// magic and section size
                d.skip(4); // 0 padding?
                unk1 = d.readInt();
                d.skip(2); //0
                unk2 = d.readShort();

                int ssize = d.readByte();
                project = d.readString(d.pos(), ssize-1);
                d.skip(ssize-1);
                d.skip(6);
                unk3 = d.readShort();
                d.align(4);
                ssize = d.readByte();
                timestamp = d.readString(d.pos(), ssize-1);
                d.skip(ssize - 1);
                d.skip(4);
            }

            public int Rebuild(FileOutput o)
            {
                o.writeString("PROP");
                int sizeoff = o.size();
                o.writeInt(0);
                int size = o.size();
                o.writeInt(0);
                o.writeInt(unk1);
                o.writeShort(0);
                o.writeShort(unk2);

                o.writeByte(project.Length + 1);
                o.writeString(project);
                o.writeByte(0);
                o.writeByte(0);
                o.writeByte(0);
                o.writeByte(0);
                o.align(4);
                o.writeShort(unk3);
                o.align(4);


                o.writeByte(timestamp.Length + 1);
                o.writeString(timestamp);
                o.writeByte(0);
                o.writeByte(0);
                o.writeByte(0);
                o.writeByte(0);
                o.align(4);

                size = o.size() - size;
                o.writeIntAt(size, sizeoff);
                return size;
            }
        }

        public class NUS_BINF
        {
            public int unk1 = 3;
            public int flag = 0x05;
            public string name = "snd_bgm_CRS01_Menu";

            public void Read(FileData d)
            {
                d.skip(12);
                unk1 = d.readInt();

                int s = d.readByte();
                name = d.readString(d.pos(), s - 1);
                d.skip(s);
                d.align(4);
                flag = d.readInt();
            }

            public int Rebuild(FileOutput o)
            {
                o.writeString("BINF");
                int sizeoff = o.size();
                o.writeInt(0);
                int size = o.size();

                o.writeInt(0);
                o.writeInt(unk1);

                o.writeByte(name.Length + 1);
                o.writeString(name);
                o.writeByte(0);
                o.align(4);
                o.writeInt(flag);

                size = o.size() - size;
                o.writeIntAt(size, sizeoff);
                return size;

            }
        }

        public class NUS_GRP
        {
            public List<string> names = new List<string>();

            public NUS_GRP()
            {
                //TODO: Set up default
            }

            public void Read(FileData d)
            {
                names.Clear();
                d.skip(8);// magic and section size
                int c1 = d.readInt();

                int start = d.pos();
                for (int i = 0; i < c1; i++)
                {
                    int offset = d.readInt();
                    int size = d.readInt();

                    int temp = d.pos();
                    d.seek(start + offset);

                    d.readInt();
                    int s = (sbyte)d.readByte();
                    names.Add(d.readString(d.pos(), -1));
                    d.skip(s);
                    d.align(4);
                    

                    d.seek(temp);
                }
            }

            public int Rebuild(FileOutput o)
            {
                o.writeString("GRP ");
                int sizeoff = o.size();
                o.writeInt(0);
                int size = o.size();

                o.writeInt(names.Count);

                int start = names.Count * 8 + 4;
                FileOutput name = new FileOutput();
                name.Endian = Endianness.Little;

                int c = 0;
                foreach(string na in names)
                {
                    o.writeInt(start + name.size());

                    int ns = name.pos();
                    name.writeInt(1);
                    name.writeByte(na.Length == 0 ? 0xFF : na.Length + 1);
                    name.writeString(na);
                    name.writeByte(0);
                    name.align(4);
                    if (c != names.Count - 1)
                        name.writeInt(0); // padding
                    else
                        ns -= 4;
                    c++;

                    o.writeInt(name.pos() - ns);
                }
                o.writeInt(0);
                o.writeOutput(name);

                size = o.size() - size;
                o.writeIntAt(size, sizeoff);
                return size;
            }
        }

        public class NUS_DTON
        {
            public class TONE_DES
            {
                public int hash, unk1 = 0x20C;
                public string name;
                public float[] data = new float[]
                {

                };

                public void Read(FileData d)
                {
                    hash = d.readInt();
                    unk1 = d.readInt();

                    int s = d.readByte();
                    name = d.readString(d.pos(), s - 1);
                    d.skip(s);
                    d.align(4);
                    data = new float[0x2c];
                    for(int i =0; i < 0x2c; i++)
                    {
                        data[i] = d.readFloat();
                    }
                }
                
                public int Rebuild(FileOutput o)
                {
                    int size = o.size();
                    o.writeInt(hash);
                    o.writeInt(unk1);
                    o.writeByte(name.Length+1);
                    o.writeString(name);
                    o.writeByte(0);
                    o.align(4);

                    // write data
                    foreach (float f in data)
                        o.writeFloat(f);

                    return o.size() - size;
                }
            }

            List<TONE_DES> destone = new List<TONE_DES>();

            public NUS_DTON()
            {
                //TODO: Setup default
            }

            public void Read(FileData d)
            {
                destone.Clear();
                d.skip(8);// magic and section size
                int count = d.readInt();
                int start = d.pos();

                for(int i = 0; i < count; i++)
                {
                    int offset = d.readInt();
                    int size = d.readInt();

                    int temp = d.pos();
                    d.seek(start + offset);
                    TONE_DES des = new TONE_DES();
                    des.Read(d);
                    destone.Add(des);

                    d.seek(temp);
                }
            }

            public int Rebuild(FileOutput o)
            {
                o.writeString("DTON");
                int sizeoff = o.size();
                o.writeInt(0);
                int size = o.size();

                o.writeInt(destone.Count);

                FileOutput dat = new FileOutput();
                dat.Endian = Endianness.Little;

                int start = destone.Count * 8 + 4;
                
                for (int i = 0; i < destone.Count; i++)
                {
                    o.writeInt(start + dat.size());
                    o.writeInt(destone[i].Rebuild(dat) + 4);

                    if (i != destone.Count - 1)
                        dat.writeInt(0);
                }

                o.writeInt(0);
                o.writeOutput(dat);

                size = o.size() - size;
                o.writeIntAt(size, sizeoff);
                return size;
            }
        }

        public class NUS_TONE
        {

            public class TONE_META
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

                public TONE_META()
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
                    IntPtr vgm = VGMStreamNative.InitVGMStream("temp.idsp");
                    if (vgm == IntPtr.Zero) throw new Exception("Error loading idsp");

                    int channelCount = VGMStreamNative.GetVGMStreamChannelCount(vgm);
                    int bitsPerFrame = VGMStreamNative.GetVGMStreamFrameSize(vgm);
                    int size = VGMStreamNative.GetVGMStreamTotalSamples(vgm);
                    int samplerate = VGMStreamNative.GetVGMStreamSampleRate(vgm);
                    
                    int total = (int)((samplerate * bitsPerFrame * channelCount * (size / 24576000f)) / 8 * 1024);
                    
                    //Console.WriteLine(channelCount + " " + bitsPerFrame + " " + size + " " + samplerate + " " + total.ToString("x"));
                
                    short[] buffer = new short[total];
                    
                    VGMStreamNative.RenderVGMStream(buffer, buffer.Length / 2, vgm);

                    FileOutput o = new FileOutput();
                    o.Endian = Endianness.Little;
                    for(int i = 0; i < buffer.Length / 2; i++)
                        o.writeShort(buffer[i]);

                    WAVE.Play(o.getBytes(), VGMStreamNative.GetVGMStreamChannelCount(vgm), VGMStreamNative.GetVGMStreamSamplesPerFrame(vgm), VGMStreamNative.GetVGMStreamSampleRate(vgm));

                    VGMStreamNative.CloseVGMStream(vgm);
                    File.Delete("temp.idsp");
                }

                public void Read(FileData d)
                {
                    hash = d.readInt();
                    unk1 = d.readInt();

                    int s = d.readByte();
                    name = d.readString(d.pos(), -1);
                    d.skip(s);
                    d.align(4);

                    d.skip(8);

                    offset = d.readInt();
                    size = d.readInt();

                    for (int i = 0; i < param.Length; i++)
                        param[i] = d.readFloat();

                    offsets = new int[d.readInt()];
                    for (int i = 0; i < offsets.Length; i++)
                        offsets[i] = d.readInt();

                    unkvalues = new float[d.readInt()];
                    for (int i = 0; i < unkvalues.Length; i++)
                    {
                        unkvalues[d.readInt()] = d.readFloat();
                    }
                    
                    List<int> une = new List<int>();

                    while(true)
                    {
                        int i = d.readInt();
                        une.Add(i);
                        if (i == -1)
                            break;
                    }
                    unkending = une.ToArray();

                    end = new int[3 + (int)Math.Ceiling((double)((unk1>>8)&0xFF)/4)];

                    for (int i = 0; i < end.Length; i++)
                        end[i] = d.readInt();

                    //Console.WriteLine(id + " " + name + " " + offset.ToString("x"));

                }

                public int Rebuild(FileOutput o)
                {
                    int size = o.size();

                    o.writeInt(hash);
                    o.writeInt(unk1);

                    o.writeByte(name.Length + 1);
                    o.writeString(name);
                    o.writeByte(0);
                    o.align(4);

                    o.writeInt(0);
                    o.writeInt(8);
                    
                    o.writeInt(offset);
                    o.writeInt(this.size);

                    // write data
                    foreach (float f in param)
                        o.writeFloat(f);

                    o.writeInt(offsets.Length);
                    foreach (int f in offsets)
                        o.writeInt(f);

                    o.writeInt(unkvalues.Length);
                    int v = 0;
                    foreach (float f in unkvalues)
                    {
                        o.writeInt(v++);
                        o.writeFloat(f);
                    }

                    foreach (int f in unkending)
                        o.writeInt(f);

                    foreach (int f in end)
                        o.writeInt(f);

                    return o.size() - size;
                }
            }

            public List<TONE_META> tones = new List<TONE_META>();

            public void Read(FileData d)
            {
                d.skip(8);// magic and section size
                int count = d.readInt();

                int start = d.pos();
                for(int i = 0; i < count; i++)
                {
                    int offset = d.readInt();
                    int size = d.readInt();

                    int temp = d.pos();
                    d.seek(offset + start);

                    TONE_META meta = new TONE_META();
                    meta.Read(d);
                    tones.Add(meta);

                    d.seek(temp);
                }
            }

            public int Rebuild(FileOutput o)
            {
                o.writeString("TONE");
                int sizeoff = o.size();
                o.writeInt(0);
                int size = o.size();

                o.writeInt(tones.Count);
                
                FileOutput dat = new FileOutput();
                dat.Endian = Endianness.Little;

                int start = tones.Count * 8 + 4;

                for (int i = 0; i < tones.Count; i++)
                {
                    o.writeInt(start + dat.size());
                    o.writeInt(tones[i].Rebuild(dat) );
                }

                o.writeInt(0);
                o.writeOutput(dat);

                size = o.size() - size - 4;
                o.writeIntAt(size, sizeoff);
                return size;
            }


            public void ReadPACK(FileData d)
            {
                int start = d.pos();

                foreach (TONE_META meta in tones)
                {
                    meta.idsp = d.getSection(start + meta.offset + 8, meta.size);
                }
            }

            public int RebuildPACK(FileOutput o)
            {
                return 0;
            }
        }

    }
}
