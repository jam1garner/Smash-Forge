using System;
using System.IO;
using System.IO.Compression;
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
            Dictionary<string, string> files = new Dictionary<string, string>();
            ZipArchive zip = ZipFile.OpenRead("lib\\Skapon.zip");

            Random random = new Random();
            int randomNumber = random.Next(0, 0xFFFFFF);

            NUT nut = new NUT();
            foreach (ZipArchiveEntry e in zip.Entries)
            {
                byte[] b;
                using (BinaryReader br = new BinaryReader(e.Open()))
                {
                    b = br.ReadBytes((int)e.Length);
                }
                var stream = new StreamReader(new MemoryStream(b));
                string s = stream.ReadToEnd();
                files.Add(e.Name, s);

                if (e.Name.EndsWith(".dds"))
                {
                    NUT.NUD_Texture tex = new DDS(new FileData(b)).toNUT_Texture();
                    nut.textures.Add(tex);
                    tex.id = 0x40000000 + randomNumber;
                    nut.draw.Add(tex.id, NUT.loadImage(tex));
                }
            }

            NUD nud = new NUD();
            NUD.Mesh head = new NUD.Mesh();
            nud.mesh.Add(head);
            head.Text = "Skapon";

            head.Nodes.Add(setToBone(scale(readPoly(files["head.obj"]), 1, 1, 1), vbn.bones[vbn.boneIndex("HeadN")], vbn));
            head.Nodes.Add(setToBone(scale(readPoly(files["body.obj"]), 1, 1, 1), vbn.bones[vbn.boneIndex("BustN")], vbn));
            head.Nodes.Add(setToBone(scale(readPoly(files["hand.obj"]), 1, 1, 1), vbn.bones[vbn.boneIndex("RHandN")], vbn));
            head.Nodes.Add(setToBone(scale(readPoly(files["hand.obj"]), -1, -1, 1), vbn.bones[vbn.boneIndex("LHandN")], vbn));
            head.Nodes.Add(setToBone(scale(readPoly(files["foot.obj"]), 1, 1, 1), vbn.bones[vbn.boneIndex("RFootJ")], vbn));
            head.Nodes.Add(setToBone(scale(readPoly(files["foot.obj"]), -1, -1, -1), vbn.bones[vbn.boneIndex("LFootJ")], vbn));

            foreach (NUD.Polygon p in head.Nodes)
            {
                p.materials[0].textures[0].hash = 0x40000000 + randomNumber;
            }

            nud.PreRender();

            return nud;
        }

        public static NUD.Polygon scale(NUD.Polygon poly, float sx, float sy, float sz)
        {
            foreach (NUD.Vertex v in poly.vertices)
            {
                v.pos.X = v.pos.X * sx;
                v.pos.Y = v.pos.Y * sy;
                v.pos.Z = v.pos.Z * sz;

                if (sx == -1) v.pos = Vector3.Transform(v.pos, Matrix4.CreateRotationX((float)Math.PI));
            }
            return poly;
        }

        public static NUD.Polygon setToBone(NUD.Polygon poly, Bone b, VBN vbn)
        {
            foreach (NUD.Vertex v in poly.vertices)
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
                        poly.faces.Add(int.Parse(args[1].Split('/')[0]) - 1);
                        poly.faces.Add(int.Parse(args[2].Split('/')[0]) - 1);
                        poly.faces.Add(int.Parse(args[3].Split('/')[0]) - 1);
                        break;
                }
            }

            return poly;
        }

    }

    public class Pichu
    {

        private static Dictionary<int, string> bonematch = new Dictionary<int, string>()
        {
            { 0, "TransN"},
            { 1, "RotN"},
{2, "YRotN"},
{3, "HipN"},
{4, "WaistN"},
{5, "BustN"},
{6, "???1"},
{7, "LShoulderN"},
{8, "LShoulderJ"},
{9, "???2"},
{10, "LArmJ"},
{11, "LHandN"},
{12, "LIndex1N"},
{13, "LThumbN"},
{14, "LHaveN"},
{15, "NeckN"},
{16, "HeadN"},
{17, "MouthN"},
{18, "LMimiN"},
{19, "LMimiNb"},
{20, "Nose"},
{21, "RMimiN"},
{22, "RMimiNb"},
{23, "RShoulderN"},
{24, "RShoulderJ"},
{25, "???3"},
{26, "RArmJ"},
{27, "RHandN"},
{28, "RIndex1N"},
{29, "RThumbN"},
{30, "RHaveN"},
{31, "LLegJ"},
{32, "LKneeJ"},
{33, "LKneeJ2"},
{34, "LFootJ"},
{35, "LFootJ2"},
{36, "LToeN"},
{37, "RLegJ"},
{38, "RKneeJ"},
{39, "RKneeJ2"},
{40, "RFootJ"},
{41, "RFootJ2"},
{42, "RToeN"},
{43, "TailN"},
{44, "ThrowN"},
{45, "???4"}
        };

        public static List<string> boneOrder = new List<string>()
        {
            "TransN",
            "RotN",
            "HipN",
            "LLegJ",
            "LKneeJ",
            "LFootJ",
            "LToeN",
            "RLegJ",
            "RKneeJ",
            "RFootJ",
            "RToeN",
            "WaistN",
            "BustN",
            "LShoulderN",
            "LShoulderJ",
            "LArmJ",
            "LHandN",
            "RShoulderN",
            "RShoulderJ",
            "RArmJ",
            "RHandN",
            "NeckN",
            "HeadN",
            "RHaveN",
            "LHaveN",
            "ThrowN"
        };

        public static void MakePichu(string path = "C:\\Pichu\\")
        {
            if (!path.EndsWith("\\"))
                path += "\\";
            DAT dat = new DAT();
            dat.Read(new FileData(path + "PlPcNr.dat"));
            dat.PreRender();

            dat.ExportTextures(path, 0x401B1000);

            BoneNameFix(dat.bones);

            // model--------------------------------------------------------
            ModelContainer converted = dat.wrapToNUD();
            NUD nud = converted.nud;
            float sca = 0.6f;


            removeLowPolyNr(nud);
            nud.PreRender();

            Runtime.ModelContainers.Add(converted);
            //-------------------------------------------------

            Runtime.TargetVBN = converted.vbn;

            MainForm.HashMatch();

            Dictionary<string, SkelAnimation> anims = DAT_Animation.LoadAJ(path + "PlPcAJ.dat", converted.vbn);

            //ArrangeBones(converted.vbn, converted.nud);

            // note bone 40 - 51 is disabled for pika

            foreach (string an in anims.Keys)
            {
                effectiveScale(anims[an], Matrix4.CreateTranslation(0, 0, 0) * Matrix4.CreateScale(sca, sca, sca));
            }
            effectiveScale(converted.nud, converted.vbn, Matrix4.CreateTranslation(0, 0, 0) * Matrix4.CreateScale(sca, sca, sca));

            Directory.CreateDirectory(path + "build\\model\\body\\c00\\");
            nud.Save(path + "build\\model\\body\\c00\\model.nud");
            converted.vbn.Endian = Endianness.Little;
            converted.vbn.Save(path + "build\\model\\body\\c00\\model.vbn");


            PAC org = new PAC();
            PAC npac = new PAC();
            org.Read(path + "main.pac");
            foreach (string key in org.Files.Keys)
            {
                byte[] d = org.Files[key];

                foreach (string an in anims.Keys)
                {
                    string name = an.Replace("PlyPichu5K_Share_ACTION_", "").Replace("_figatree", "");
                    if (key.Contains(name))
                    {
                        Console.WriteLine("Matched " + name + " with " + key);

                        if (!anims[an].getNodes(true).Contains(0) && !key.Contains("Cliff"))
                        {
                            KeyNode node = anims[an].getNode(0, 0);
                            node.t_type = 1;
                        }
                        d = OMOOld.createOMO(anims[an], converted.vbn);
                        break;
                    }
                }

                npac.Files.Add(key, d);
            }
            Directory.CreateDirectory(path + "build\\motion\\");
            npac.Save(path + "build\\motion\\main.pac");

            /*FileOutput omo = new FileOutput();
            converted.vbn.reset();
            converted.vbn.totalBoneCount = (uint)converted.vbn.bones.Count;
            omo.writeBytes(OMO.createOMO(anims["PlyPichu5K_Share_ACTION_Wait1_figatree"], converted.vbn));
            omo.save(path + "PlyPichu5K_Share_ACTION_Wait1_figatree.omo");*/

        }

        public static void removeLowPolyNr(NUD n)
        {
            List<NUD.Mesh> toRemove = new List<NUD.Mesh>();

            for (int i = 15; i <= 32; i++)
                toRemove.Add(n.mesh[i]);

            foreach (NUD.Mesh m in toRemove)
                n.mesh.Remove(m);
        }


        public static void effectiveScale(SkelAnimation anim, Matrix4 sca)
        {
            foreach (KeyFrame frame in anim.frames)
            {
                foreach (KeyNode node in frame.nodes)
                {
                    if (node.t_type != -1)
                    {
                        Vector3 pos = Vector3.Transform(node.t, sca);
                        if (node.t.X != -99) node.t.X = pos.X;
                        if (node.t.Y != -99) node.t.Y = pos.Y;
                        if (node.t.Z != -99) node.t.Z = pos.Z;
                    }
                }
            }
        }
        public static void effectiveScale(DAT_Animation anim, Matrix4 sca)
        {
            foreach (List<DAT_Animation.DATAnimTrack> track in anim.nodes)
            {
                foreach (DAT_Animation.DATAnimTrack track2 in track)
                {
                    if (track2.type == DAT_Animation.AnimType.XPOS
                        || track2.type == DAT_Animation.AnimType.YPOS
                        || track2.type == DAT_Animation.AnimType.ZPOS)
                        foreach (DAT_Animation.KeyNode node in track2.keys)
                        {
                            node.value = Vector3.Transform(new Vector3(node.value, 0, 0), sca).X;
                        }
                }
            }
        }
        public static void effectiveScale(NUD nud, VBN vbn, Matrix4 sca)
        {
            foreach (Bone b in vbn.bones)
            {
                Vector3 pos = Vector3.Transform(new Vector3(b.position[0], b.position[1], b.position[2]), sca);
                b.position[0] = pos.X;
                b.position[1] = pos.Y;
                b.position[2] = pos.Z;
            }

            vbn.reset();

            foreach (NUD.Mesh mesh in nud.mesh)
            {
                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    foreach (NUD.Vertex v in poly.vertices)
                    {
                        v.pos = Vector3.Transform(v.pos, sca);
                    }
                }
            }
            nud.PreRender();
        }

        public static void ArrangeBones(VBN vbn, NUD nud)
        {
            Dictionary<int, int> boneReorder = new Dictionary<int, int>();
            int i = 0;
            for (i = 0; i < boneOrder.Count; i++)
            {
                int j = 0;
                for (j = 0; j < vbn.bones.Count; j++)
                    if (vbn.bones[j].Text.Equals(boneOrder[i]))
                        break;

                boneReorder.Add(j, i);
            }
            // get rest of the bones
            for (int j = 0; j < vbn.bones.Count; j++)
            {
                if (!boneReorder.Keys.Contains(j))
                {
                    boneReorder.Add(j, i++);
                }
            }

            // reorder vbn
            Bone[] nList = new Bone[vbn.bones.Count];
            foreach (int k in boneReorder.Keys)
            {
                nList[boneReorder[k]] = vbn.bones[k];
                //if (new string(vbn.bones[k].boneName).Equals("RotN")) vbn.bones[k].parentIndex = 0;
                if (vbn.bones[k].parentIndex != -1 && vbn.bones[k].parentIndex != 0x0FFFFFFF)
                {
                    vbn.bones[k].parentIndex = boneReorder[vbn.bones[k].parentIndex];
                }
            }
            vbn.bones.Clear();
            vbn.bones.AddRange(nList);
            vbn.reset();

            // now fix the nud

            foreach (NUD.Mesh mesh in nud.mesh)
            {
                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    foreach (NUD.Vertex v in poly.vertices)
                    {
                        for (int k = 0; k < v.node.Count; k++)
                        {
                            v.node[k] = boneReorder[v.node[k]];
                        }
                    }
                }
            }
            nud.PreRender();
        }

        public static void BoneNameFix(VBN vbn)
        {
            foreach (int key in bonematch.Keys)
            {
                vbn.bones[key].Text = bonematch[key];
            }
        }

    }
}
