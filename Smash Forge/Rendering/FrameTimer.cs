using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace Smash_Forge
{
    public class FrameTimer : Stopwatch
    {
        private long totalRenderTime = 0;
        private long renderedFrameCount = 0;
        private int duration = 60;
        private double averageRenderTime = 0;

        public double getAverageRenderTime()
        {
            totalRenderTime += ElapsedMilliseconds;
            Reset();
            renderedFrameCount += 1;
            if (renderedFrameCount > 90)
            {
                // calculate an average render time and reset count
                averageRenderTime = (double)totalRenderTime / renderedFrameCount;
                renderedFrameCount = 0;
                totalRenderTime = 0;
                Reset();
            }

            return averageRenderTime;
        }
    }
}
