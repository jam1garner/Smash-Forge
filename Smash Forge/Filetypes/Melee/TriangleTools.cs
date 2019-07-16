using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmashForge
{
    class TriangleTools
    {
        public static List<int> fromTriangleStrip(List<int> input)
        {
            List<int> newFace = new List<int>();

            int t1 = 0, t2 = 0, t3 = 0;
            for (int i = 0; i < input.Count - 2; i++)
            {
                if (i % 2 == 1)
                {
                    t1=(input[i + 0]);
                    t2=(input[i + 1]);
                    t3=(input[i + 2]);
                }
                else
                {
                    t1=(input[i]);
                    t2=(input[i + 2]);
                    t3=(input[i + 1]);
                }
                if (t1 == t2)
                    continue;
                if (t2 == t3)
                    continue;
                if (t3 == t1)
                    continue;
                newFace.Add(t1);
                newFace.Add(t2);
                newFace.Add(t3);
            }

            return newFace;
        }

        public static List<int> fromQuad(List<int> input)
        {
            List<int> newFace = new List<int>();

            for (int i = 0; i < input.Count / 4; i++)
            {
                newFace.Add(input[i * 4 + 2]);
                newFace.Add(input[i * 4 + 1]);
                newFace.Add(input[i * 4 + 0]);
                newFace.Add(input[i * 4 + 0]);
                newFace.Add(input[i * 4 + 3]);
                newFace.Add(input[i * 4 + 2]);
            }
                return newFace;
        }
    }
}
