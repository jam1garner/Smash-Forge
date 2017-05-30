using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smash_Forge
{
    class TriangleTools
    {
        public static List<int> fromTriangleStrip(List<int> input)
        {
            List<int> newFace = new List<int>();

            for (int i = 0; i < input.Count - 2; i++)
            {
                if (i % 2 == 1)
                {
                    newFace.Add(input[i + 0]);
                    newFace.Add(input[i + 1]);
                    newFace.Add(input[i + 2]);
                }
                else
                {
                    newFace.Add(input[i]);
                    newFace.Add(input[i + 2]);
                    newFace.Add(input[i + 1]);
                }
            }
            Console.WriteLine(input.Count + " " + (input.Count / 3f));

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
