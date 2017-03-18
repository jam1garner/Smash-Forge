using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Smash_Forge
{
    class WAVE
    {
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

            using (AudioContext con = new AudioContext())
            {
                int buffer = AL.GenBuffer();
                int src = AL.GenSource();
                int state;

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
            }
        }

    }
}
