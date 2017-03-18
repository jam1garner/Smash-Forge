using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using System.IO;

using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Smash_Forge
{
    class WAVE
    {
        public static AudioContext con = new AudioContext();
        public static int state;

        public WAVE()
        {
        }

        public static ALFormat GetSoundFormat(int channel, int bits)
        {
            switch (channel)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new Exception("Unsupported WAV format");
            }
        }

        public static void Play(byte[] data, int channelCount, int bps, int sampleRate)
        {
            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += new DoWorkEventHandler(
        delegate (object o, DoWorkEventArgs args)
        {
            BackgroundWorker b = o as BackgroundWorker;
            
                int buffer = AL.GenBuffer();
                int src = AL.GenSource();

                AL.BufferData(buffer, GetSoundFormat(channelCount, bps), data, data.Length, sampleRate);

                AL.Source(src, ALSourcei.Buffer, buffer);
                AL.SourcePlay(src);

                do
                {
                    Thread.Sleep(500);
                    AL.GetSource(src, ALGetSourcei.SourceState, out state);
                } while (state == (int)ALSourceState.Playing);

                AL.SourceStop(src);
                AL.DeleteSource(src);
                AL.DeleteBuffer(buffer);

        });
            bw.RunWorkerAsync();

        }

        public static byte[] FromIDSP(byte[] idsp)
        {
            File.WriteAllBytes("temp.idsp", idsp);
            IntPtr vgm = VGMStreamNative.InitVGMStream("temp.idsp");
            if (vgm == IntPtr.Zero) throw new Exception("Error loading idsp");

            int channelCount = VGMStreamNative.GetVGMStreamChannelCount(vgm);
            int bitsPerFrame = VGMStreamNative.GetVGMStreamFrameSize(vgm);
            int size = VGMStreamNative.GetVGMStreamTotalSamples(vgm);
            int samplerate = VGMStreamNative.GetVGMStreamSampleRate(vgm);

            int total = (int)((samplerate * bitsPerFrame * channelCount * (size / 24576000f)) / 8 * 1024);

            short[] buffer = new short[total];
            VGMStreamNative.RenderVGMStream(buffer, buffer.Length / 2, vgm);

            FileOutput o = new FileOutput();
            o.Endian = Endianness.Little;

            o.writeString("RIFF");
            o.writeInt(0);
            o.writeString("WAVEfmt ");

            o.writeInt(0x10);
            o.writeShort(1);
            o.writeShort(channelCount);
            o.writeInt(samplerate);
            o.writeInt(size);
            o.writeShort(2);
            o.writeShort(0x10);

            o.writeString("data");
            o.writeInt(buffer.Length);

            for (int i = 0; i < buffer.Length / 2; i++)
                o.writeShort(buffer[i]);

            o.writeIntAt(o.size() - 8, 4);

            VGMStreamNative.CloseVGMStream(vgm);
            File.Delete("temp.idsp");
            return o.getBytes();
        }

        public void Read(string fname)
        {
            FileData d = new FileData(fname);
            d.Endian = System.IO.Endianness.Little;

            d.skip(4); // RIFF
            int riffsize = d.readInt();
            d.skip(4); //WAVE

            d.skip(4); //fmt
            int formatsize = d.readInt();
            int audio_format = d.readShort();
            int channelCount = d.readShort();
            int sampleRate = d.readInt();
            int byte_rate = d.readInt();
            int blockalign = d.readShort();
            int bps = d.readShort();

            d.skip(4); // data
            int datasize = d.readInt();


            byte[] data = d.getSection(d.pos(), datasize);

            Play(data, channelCount, bps, sampleRate);
        }

    }
}
