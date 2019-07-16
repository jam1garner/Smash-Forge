//From lioncash
//https://github.com/lioncash/vgmstreamSharp

using System;
using System.Runtime.InteropServices;

namespace SmashForge
{
    /// <summary>
    /// Class for VGMStream native calls.
    /// </summary>
    public static class VGMStreamNative
    {
        private static bool _VGMStreamLoaded = false;
        public static bool VGMStreamLoaded { get { return _VGMStreamLoaded; } }

        #region Interface methods

        /// <summary>
        /// Initialize VGMStream.
        /// </summary>
        /// <param name="filename">The file to open in VGMStream.</param>
        /// <returns>An IntPtr to a usable VGMSTREAM or NULL upon failure.</returns>
        public static IntPtr InitVGMStream(string filename)
        {
            try
            {
                _VGMStreamLoaded = true;
                return init_vgmstream(filename);
            }
            catch (Exception e)
            {
                _VGMStreamLoaded = false;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Resets a given VGMSTREAM to the beginning of its stream.
        /// </summary>
        /// <param name="vgmstream">The VGMSTREAM to reset.</param>
        public static void ResetVGMStream(IntPtr vgmstream)
        {
            reset_vgmstream(vgmstream);
        }

        /// <summary>
        /// Allocate a VGMSTREAM and channel stuff.
        /// </summary>
        /// <param name="channelCount">Number of channels.</param>
        /// <param name="looped">Whether or not it's looped.</param>
        /// <returns>An IntPtr to a usable VGMSTREAM, or null upon failure.</returns>
        public static IntPtr AllocateVGMStream(int channelCount, bool looped)
        {
            return allocate_vgmstream(channelCount, looped ? 1 : 0);
        }

        /// <summary>
        /// Deallocates and closes a VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">The VGMSTREAM to close.</param>
        public static void CloseVGMStream(IntPtr vgmstream)
        {
            close_vgmstream(vgmstream);
        }

        /// <summary>
        /// Calculate the number of samples to be played based on looping parameters.
        /// </summary>
        /// <param name="loopTimes">Number of times to loop.</param>
        /// <param name="fadeSeconds">Number of seconds the fade will start at.</param>
        /// <param name="fadeDelaySeconds">Number of seconds until the fade finished.</param>
        /// <param name="vgmstream">VGMSTREAM to calculate all of this for.</param>
        /// <returns>The number of samples to be played based on looping parameter.</returns>
        public static int GetVGMStreamPlaySamples(double loopTimes, double fadeSeconds, double fadeDelaySeconds, IntPtr vgmstream)
        {
            return get_vgmstream_play_samples(loopTimes, fadeSeconds, fadeDelaySeconds, vgmstream);
        }

        /// <summary>
        /// Generates sampleCount number of samples into the given buffer.
        /// </summary>
        /// <param name="buffer">The buffer to hold all of the samples.</param>
        /// <param name="sampleCount">The number of samples to generate into the buffer.</param>
        /// <param name="vgmstream">The VGMSTREAM to render samples of.</param>
        public static void RenderVGMStream(short[] buffer, int sampleCount, IntPtr vgmstream)
        {
            render_vgmstream(buffer, sampleCount, vgmstream);
        }

        /// <summary>
        /// Get the samples per frame for the given VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">The VGMSTREAM to get the samples per frame of.</param>
        /// <returns>The samples per frame for the given VGMSTREAM.</returns>
        public static int GetVGMStreamSamplesPerFrame(IntPtr vgmstream)
        {
            return get_vgmstream_samples_per_frame(vgmstream);
        }

        /// <summary>
        /// Gets the number of bytes per frame for the given VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">The VGMSTREAM to get the number of bytes per frame of.</param>
        /// <returns>The number of bytes per frame for the given VGMSTREAM.</returns>
        public static int GetVGMStreamFrameSize(IntPtr vgmstream)
        {
            return get_vgmstream_frame_size(vgmstream);
        }

        /// <summary>
        /// Gets the number of channels used by a given VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">VGMSTREAM to get the channel count of.</param>
        /// <returns>The number of channels used by this vgmstream.</returns>
        public static int GetVGMStreamChannelCount(IntPtr vgmstream)
        {
            return get_vgmstream_channel_count(vgmstream);
        }

        /// <summary>
        /// Gets the sample rate used by a given VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">The VGMSTREAM to get the sample rate of.</param>
        /// <returns>The sample rate used by a given VGMSTREAM.</returns>
        public static int GetVGMStreamSampleRate(IntPtr vgmstream)
        {
            return get_vgmstream_samplerate(vgmstream);
        }

        /// <summary>
        /// Checks whether or not the given VGMSTREAM should be looped or not.
        /// </summary>
        /// <param name="vgmstream">The VGMSTREAM to check</param>
        /// <returns>1 if the VGMSTREAM should be looped. 0 if the VGMSTREAM doesn't loop.</returns>
        public static int GetVGMStreamIsLooped(IntPtr vgmstream)
        {
            return get_vgmstream_is_looped(vgmstream);
        }

        public static string[] GetVGMStreamInfo(IntPtr vgmstream)
        {
            byte[] array = new byte[400];
            char[] split = new char[] { '\n' };
            describe_vgmstream(vgmstream, array, 400);
            string info = System.Text.Encoding.ASCII.GetString(array).TrimEnd('\0');
            return info.Split(split, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Gets the loop start sample used by a given VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">VGMSTREAM to get the channel count of.</param>
        /// <returns>The loop start sample used by this vgmstream.</returns>
        public static int GetVGMStreamLoopStartSample(IntPtr vgmstream)
        {
            return get_vgmstream_loop_startsample(vgmstream);
        }

        /// <summary>
        /// Gets the loop end sample used by a given VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">VGMSTREAM to get the channel count of.</param>
        /// <returns>The loop end sample used by this vgmstream.</returns>
        public static int GetVGMStreamLoopEndSample(IntPtr vgmstream)
        {
            return get_vgmstream_loop_endsample(vgmstream);
        }

        /// <summary>
        /// Gets the total samples used by a given VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">VGMSTREAM to get the channel count of.</param>
        /// <returns>The total samples used by this vgmstream.</returns>
        public static int GetVGMStreamTotalSamples(IntPtr vgmstream)
        {
            return get_vgmstream_totalsamples(vgmstream);
        }
        #endregion


        #region P/Invoke Methods

        private const string DllName = "lib\\libvgmstream.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr init_vgmstream(string filename);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void reset_vgmstream(IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr allocate_vgmstream(int channel_count, int looped);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void close_vgmstream(IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_vgmstream_play_samples(double looptimes, double fadeseconds, double fadedelayseconds, IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void render_vgmstream([In, Out] short[] buffer, int sample_count, IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_vgmstream_samples_per_frame(IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_vgmstream_frame_size(IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_vgmstream_samples_per_shortframe(IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_vgmstream_shortframe_size(IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_vgmstream_channel_count(IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_vgmstream_samplerate(IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_vgmstream_loop_startsample(IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_vgmstream_loop_endsample(IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_vgmstream_totalsamples(IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int get_vgmstream_is_looped(IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void decode_vgmstream(IntPtr vgmstream, int samples_written, int samples_to_do, [In, Out] short[] buffer);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int vgmstream_samples_to_do(int samples_this_block, int samples_per_frame, IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int vgmstream_do_loop(IntPtr vgmstream);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int describe_vgmstream(IntPtr vgmstream, [In, Out] byte[] desc, int length);

        #endregion
    }
}