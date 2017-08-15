using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Gif.Components
{
    public partial class GIFProgress : Form
    {
        private bool cancelled = false;
        private bool finished = false;

        public GIFProgress(List<Bitmap> images, string filename, int AnimationSpeed, bool Repeat, int Quality)
        {
            InitializeComponent();
            progressBar1.Value = 0;
            progressBar1.Maximum = images.Count;

            this.Text = $"Encoding {Path.GetFileNameWithoutExtension(filename)}";

            new Task(() =>
            {
                AnimatedGifEncoder gifEncoder = new AnimatedGifEncoder();
                gifEncoder.Start(filename);
                gifEncoder.SetDelay(1000 / AnimationSpeed);
                gifEncoder.SetRepeat(Repeat ? 0 : 1);
                gifEncoder.SetQuality(Quality);
                foreach (Bitmap b in images)
                {
                    if (cancelled)
                        break;

                    gifEncoder.AddFrame(b);
                    b.Dispose();
                    if (!this.IsDisposed)
                        this.Invoke((Action)(() =>
                        {
                            if (!cancelled)
                                progressBar1.Value++;
                        }));
                }
                gifEncoder.Finish();
                if (cancelled)
                {
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }
                }
                gifEncoder = null;
                finished = true;
                if (!this.IsDisposed)
                    this.Invoke((Action)(() => this.Close()));
            }).Start();
        }

        private void GIFProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!finished)
            {
                cancelled = true;
            }
        }
    }
}
