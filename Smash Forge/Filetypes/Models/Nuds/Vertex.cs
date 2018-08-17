using System.Collections.Generic;
using OpenTK;


namespace Smash_Forge
{
    public partial class NUD
    {
        public class Vertex
        {
            public Vector3 pos = new Vector3(0, 0, 0), nrm = new Vector3(0, 0, 0);
            public Vector4 bitan = new Vector4(0, 0, 0, 1);
            public Vector4 tan = new Vector4(0, 0, 0, 1);
            public Vector4 color = new Vector4(127, 127, 127, 127);
            public List<Vector2> uv = new List<Vector2>();
            public List<int> boneIds = new List<int>();
            public List<float> boneWeights = new List<float>();

            public Vertex()
            {
            }

            public Vertex(float x, float y, float z)
            {
                pos = new Vector3(x, y, z);
            }

            public bool Equals(Vertex p)
            {
                return pos.Equals(p.pos) && nrm.Equals(p.nrm) && new HashSet<Vector2>(uv).SetEquals(p.uv) && color.Equals(p.color)
                    && new HashSet<int>(boneIds).SetEquals(p.boneIds) && new HashSet<float>(boneWeights).SetEquals(p.boneWeights);
            }

            public override string ToString()
            {
                return pos.ToString();
            }
        }
    }
}

