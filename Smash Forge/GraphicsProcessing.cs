using nQuant;
using SmashForge.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Smash_Forge
{
    public class GifMaker
    {
        public bool makingGif = false;
        public int currentFrame = -1;
        public bool currentFrameCaptured = false;
        public List<Image> keyframes = null;
        public string gifName;
    }

    // Collection of static functions for processing 2D static graphics
    public class GraphicsProcessing
    {
        public static Mutex gdiMutex = new Mutex();

        // For threading use
        public static void ProcessGIF(object stateInformation)
        {
            GifMaker gifMaker = (GifMaker)stateInformation;
            gifMaker.makingGif = false;
            QuantizeImages(gifMaker);
            SaveCompletedAnimationGif(gifMaker);
        }

        public static void QuantizeImages(GifMaker gifMaker)
        {
            var sw = Stopwatch.StartNew();
            List<Image> newKeyframes = new List<Image>();
            var quantizer = new WuQuantizer();

            for (int i = 0; i < gifMaker.keyframes.Count; i++)
            {
                Bitmap frame = (Bitmap)gifMaker.keyframes[i];

                gdiMutex.WaitOne();
                try
                {
                    // Draw frame counter over the top right corner
                    RectangleF rectf = new RectangleF(6, 6, frame.Width, frame.Height);
                    Graphics g = Graphics.FromImage(frame);

                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Near;

                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    Font f = new Font("Helvetica", 26, FontStyle.Bold, GraphicsUnit.Pixel);
                    // Pen is for text outline
                    Pen p = new Pen(ColorTranslator.FromHtml("#000000"), 4);
                    p.LineJoin = LineJoin.Round;

                    GraphicsPath gp = new GraphicsPath();
                    gp.AddString($"Frame: {i + 1}/{gifMaker.keyframes.Count}", f.FontFamily, (int)f.Style, 26, rectf, sf);
                    Brush b = new SolidBrush(Color.White);

                    g.DrawPath(p, gp);
                    g.FillPath(b, gp);
                    g.Flush();
                } finally
                {
                    gdiMutex.ReleaseMutex();
                }

                // Quantize the image
                var quantized = quantizer.QuantizeImage(frame, 10, 70);
                newKeyframes.Add(quantized);
            }
            gifMaker.keyframes = newKeyframes;
            Console.WriteLine($"Quantized GIF keyframes in {sw.ElapsedMilliseconds}ms");
        }

        public static void SaveCompletedAnimationGif(GifMaker gifMaker)
        {
            var sw = Stopwatch.StartNew();
            using (var gif = File.OpenWrite($"{gifMaker.gifName}.gif"))
            using (var encoder = new GifEncoder(gif))
            {
                // 60fps for now, can speed up in gfycat
                // TODO: in the future support a list of FPS and output them all
                // this should be fast since the quantization only happens once
                encoder.FrameDelay = TimeSpan.FromMilliseconds(1000 / 30);
                foreach (Bitmap frame in gifMaker.keyframes)
                {
                    encoder.AddFrame(frame);
                }
            }
            sw.Stop();
            Console.WriteLine($"Encoded GIF in {sw.ElapsedMilliseconds}ms");
        }
    }
}
