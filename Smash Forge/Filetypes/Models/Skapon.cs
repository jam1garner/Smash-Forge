using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Smash_Forge
{
    class Skapon
    {

        // I'm completely totally serious

        public static NUD Create(VBN vbn)
        {
            NUD nud = new NUD();
            NUD.Mesh head = new NUD.Mesh();
            nud.mesh.Add(head);
            head.name = "Skapon";

            NUT nut = new NUT();
            NUT.NUD_Texture tex = new DDS(new FileData("Skapon//tex.dds")).toNUT_Texture();
            nut.textures.Add(tex);
            Random random = new Random();
            int randomNumber = random.Next(0, 0xFFFFFF);

            tex.id = 0x40000000 + randomNumber;
            nut.draw.Add(tex.id, NUT.loadImage(tex));

            head.polygons.Add(setToBone(scale(readPoly(File.ReadAllText("Skapon//head.obj")), 1, 1, 1), vbn.bones[vbn.boneIndex("HeadN")], vbn));
            head.polygons.Add(setToBone(scale(readPoly(File.ReadAllText("Skapon//body.obj")), 1, 1, 1), vbn.bones[vbn.boneIndex("BustN")], vbn));
            head.polygons.Add(setToBone(scale(readPoly(File.ReadAllText("Skapon//hand.obj")), 1, 1, 1), vbn.bones[vbn.boneIndex("RHandN")], vbn));
            head.polygons.Add(setToBone(scale(readPoly(File.ReadAllText("Skapon//hand.obj")), -1, -1, 1), vbn.bones[vbn.boneIndex("LHandN")], vbn));
            head.polygons.Add(setToBone(scale(readPoly(File.ReadAllText("Skapon//foot.obj")), 1, 1, 1), vbn.bones[vbn.boneIndex("RFootJ")], vbn));
            head.polygons.Add(setToBone(scale(readPoly(File.ReadAllText("Skapon//foot.obj")), -1, -1, -1), vbn.bones[vbn.boneIndex("LFootJ")], vbn));

            foreach(NUD.Polygon p in head.polygons)
            {
                p.materials[0].textures[0].hash = tex.id;
            }

            return nud;
        }

        public static NUD.Polygon scale(NUD.Polygon poly, float sx, float sy, float sz)
        {
            foreach (NUD.Vertex v in poly.vertices)
            {
                v.pos.X = v.pos.X * sx;
                v.pos.Y = v.pos.Y * sy;
                v.pos.Z = v.pos.Z * sz;

                if(sx == -1)v.pos = Vector3.Transform(v.pos, Matrix4.CreateRotationX((float)Math.PI));
            }
            return poly;
        }

        public static NUD.Polygon setToBone(NUD.Polygon poly, Bone b, VBN vbn)
        {
            foreach(NUD.Vertex v in poly.vertices)
            {
                v.node.Clear();
                v.node.Add(vbn.bones.IndexOf(b));

                Vector3 newpos = Vector3.Transform(Vector3.Zero, b.transform);
                v.pos += newpos;
            }

            return poly;
        }

        public static NUD.Polygon readPoly(string input)
        {
            NUD.Polygon poly = new NUD.Polygon();
            poly.setDefaultMaterial();

            string[] lines = input.Replace("  ", " ").Split('\n');

            int vi = 0;
            NUD.Vertex v;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] args = lines[i].Split(' ');
                switch (args[0])
                {
                    case "v":
                        v = new NUD.Vertex();
                        v.pos.X = float.Parse(args[1]);
                        v.pos.Y = float.Parse(args[2]);
                        v.pos.Z = float.Parse(args[3]);
                        v.node.Add(-1);
                        v.weight.Add(1);
                        poly.vertices.Add(v);
                        break;
                    case "vt":
                        v = poly.vertices[vi++];
                        v.tx.Add(new Vector2(float.Parse(args[1]), float.Parse(args[2])));
                        break;
                    case "f":
                        poly.faces.Add(int.Parse(args[1].Split('/')[0])-1);
                        poly.faces.Add(int.Parse(args[2].Split('/')[0])-1);
                        poly.faces.Add(int.Parse(args[3].Split('/')[0])-1);
                        break;
                }
            }

            return poly;
        }
        
    }
}
