using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALT.PARAMS;
using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SmashForge
{
    public class CharacterParamManager
    {
        private List<CsvSpecialBubble> SpecialBubbleData { get; set; }

        public string Character { get; set; }
        public ParamFile Param { get; set; }
        public SortedList<int, MoveData> MovesData { get; set; }
        public SortedList<int, Ecb> EcBs { get; set; }
        public SortedList<int, Hurtbox> Hurtboxes { get; set; }
        public SortedList<int, LedgeGrabBox> LedgeGrabboxes { get; set; }

        public SortedList<int, SpecialBubble> SpecialBubbles { get; set; }

        public CharacterParamManager()
        {
            Reset();
        }

        public CharacterParamManager(string file, string character = null)
        {
            Reset();
            
            try
            {
                Param = new ParamFile(file);

                if (character != null)
                    this.Character = character;
                else
                    this.Character = Path.GetFileNameWithoutExtension(file.Replace("fighter_param_vl_", ""));

                //Move data (FAF, Intangibility)

                for (int id = 0; id < ((ParamGroup)Param.Groups[0]).Chunks.Length; id++)
                {
                    MoveData m = new MoveData();
                    m.Index = id;
                    m.Faf = Convert.ToInt32(((ParamGroup)Param.Groups[0])[id][2].Value);
                    m.IntangibilityStart = Convert.ToInt32(((ParamGroup)Param.Groups[0])[id][3].Value);
                    m.IntangibilityEnd = Convert.ToInt32(((ParamGroup)Param.Groups[0])[id][4].Value);

                    MovesData.Add(id, m);
                }

                //ECB

                for (int id = 0; id < ((ParamGroup)Param.Groups[3]).Chunks.Length; id++)
                {
                    Ecb ecb = new Ecb();
                    ecb.Id = id;
                    ecb.Bone = Convert.ToInt32(((ParamGroup)Param.Groups[3])[id][0].Value);
                    ecb.X = Convert.ToSingle(((ParamGroup)Param.Groups[3])[id][1].Value);
                    ecb.Y = Convert.ToSingle(((ParamGroup)Param.Groups[3])[id][2].Value);
                    ecb.Z = Convert.ToSingle(((ParamGroup)Param.Groups[3])[id][3].Value);

                    EcBs.Add(id, ecb);
                }

                //Hurtboxes

                for (int id = 0; id < ((ParamGroup)Param.Groups[4]).Chunks.Length; id++)
                {
                    Hurtbox hurtbox = new Hurtbox();
                    hurtbox.Id = id;
                    hurtbox.X = Convert.ToSingle(((ParamGroup)Param.Groups[4])[id][0].Value);
                    hurtbox.Y = Convert.ToSingle(((ParamGroup)Param.Groups[4])[id][1].Value);
                    hurtbox.Z = Convert.ToSingle(((ParamGroup)Param.Groups[4])[id][2].Value);

                    hurtbox.X2 = Convert.ToSingle(((ParamGroup)Param.Groups[4])[id][3].Value);
                    hurtbox.Y2 = Convert.ToSingle(((ParamGroup)Param.Groups[4])[id][4].Value);
                    hurtbox.Z2 = Convert.ToSingle(((ParamGroup)Param.Groups[4])[id][5].Value);

                    hurtbox.Size = Convert.ToSingle(((ParamGroup)Param.Groups[4])[id][6].Value);
                    hurtbox.Bone = Convert.ToInt32(((ParamGroup)Param.Groups[4])[id][7].Value);
                    hurtbox.Part = Convert.ToInt32(((ParamGroup)Param.Groups[4])[id][8].Value);
                    hurtbox.Zone = Convert.ToInt32(((ParamGroup)Param.Groups[4])[id][9].Value);

                    if (hurtbox.X == hurtbox.X2 && hurtbox.Y == hurtbox.Y2 && hurtbox.Z == hurtbox.Z2)
                    {
                        // It can't be anything but a sphere. I think some part of the param might
                        // control this so this might be a crude detection method. This fixes Bowser Jr at least.
                        hurtbox.IsSphere = true;
                    }

                    Hurtboxes.Add(id, hurtbox);
                }

                //Ledge grabboxes

                for (int id = 0; id < ((ParamGroup)Param.Groups[6]).Chunks.Length; id++)
                {
                    LedgeGrabBox l = new LedgeGrabBox();
                    l.Id = id;
                    l.Z1 = Convert.ToSingle(((ParamGroup)Param.Groups[6])[id][0].Value);
                    l.Y1 = Convert.ToSingle(((ParamGroup)Param.Groups[6])[id][1].Value);
                    l.Z2 = Convert.ToSingle(((ParamGroup)Param.Groups[6])[id][2].Value);
                    l.Y2 = Convert.ToSingle(((ParamGroup)Param.Groups[6])[id][3].Value);

                    LedgeGrabboxes.Add(id, l);
                }

                //Special Bubbles, these are used in certain moves as trigger/reflect/absorb bubbles and shields such as counters or reflectors

                //Read the data from the csv file that contains the param group, entry and values as well as the character and animation
                foreach(CsvSpecialBubble sb in SpecialBubbleData)
                {
                    if(sb.Character == this.Character)
                    {
                        try
                        {
                            SpecialBubbles.Add(sb.Id, new SpecialBubble()
                            {
                                Animations = sb.Animation.ToLower().Split('|').ToList(),
                                type = (SpecialBubble.BubbleType)sb.Type,
                                X = sb.X != -1 ? Convert.ToSingle(((ParamGroup)Param.Groups[sb.ParamGroup-1])[sb.ParamEntry][sb.X].Value) : 0,
                                Y = sb.Y != -1 ? Convert.ToSingle(((ParamGroup)Param.Groups[sb.ParamGroup - 1])[sb.ParamEntry][sb.Y].Value) : 0,
                                Z = sb.Z != -1 ? Convert.ToSingle(((ParamGroup)Param.Groups[sb.ParamGroup - 1])[sb.ParamEntry][sb.Z].Value) : 0,
                                X2 = sb.X2 != -1 ? Convert.ToSingle(((ParamGroup)Param.Groups[sb.ParamGroup - 1])[sb.ParamEntry][sb.X2].Value) : 0,
                                Y2 = sb.Y2 != -1 ? Convert.ToSingle(((ParamGroup)Param.Groups[sb.ParamGroup - 1])[sb.ParamEntry][sb.Y2].Value) : 0,
                                Z2 = sb.Z2 != -1 ? Convert.ToSingle(((ParamGroup)Param.Groups[sb.ParamGroup - 1])[sb.ParamEntry][sb.Z2].Value) : 0,
                                Size = sb.Size != -1 ? Convert.ToSingle(((ParamGroup)Param.Groups[sb.ParamGroup - 1])[sb.ParamEntry][sb.Size].Value) : 0,
                                Bone = sb.SetBone ? sb.Bone :(sb.Bone != -1 ? Convert.ToInt32(((ParamGroup)Param.Groups[sb.ParamGroup - 1])[sb.ParamEntry][sb.Bone].Value) : 0),
                                Id = sb.Id,
                                StartFrame = sb.StartFrame,
                                EndFrame = sb.EndFrame
                            });
                        }catch
                        {
                            //Probably wrong params, ignore it... also don't reset hurtboxes and other stuff if something goes wrong here
                        }
                    }
                }

            }
            catch
            {
                //Some error occurred (Invalid file probably)
                //Reset lists
                Reset();
            }
        }

        public void Reset()
        {
            if (SpecialBubbleData == null)
                SpecialBubbleData = CsvSpecialBubble.Read(Path.Combine(MainForm.executableDir, "characterSpecialBubbles.csv"));

            Hurtboxes = new SortedList<int, Hurtbox>();
            MovesData = new SortedList<int, MoveData>();
            LedgeGrabboxes = new SortedList<int, LedgeGrabBox>();
            EcBs = new SortedList<int, Ecb>();
            Param = null;
            SpecialBubbles = new SortedList<int, SpecialBubble>();
        }

        public void UnselectHurtboxes()
        {
            Runtime.SelectedHurtboxID = -1;
        }

        public void SaveHurtboxes()
        {
            if (Param == null)
                return;

            for (int id = 0; id < ((ParamGroup)Param.Groups[4]).Chunks.Length; id++)
            {
                ((ParamGroup)Param.Groups[4])[id][0].Value = Convert.ToSingle(Hurtboxes[id].X);
                ((ParamGroup)Param.Groups[4])[id][1].Value = Convert.ToSingle(Hurtboxes[id].Y);
                ((ParamGroup)Param.Groups[4])[id][2].Value = Convert.ToSingle(Hurtboxes[id].Z);

                ((ParamGroup)Param.Groups[4])[id][3].Value = Convert.ToSingle(Hurtboxes[id].X2);
                ((ParamGroup)Param.Groups[4])[id][4].Value = Convert.ToSingle(Hurtboxes[id].Y2);
                ((ParamGroup)Param.Groups[4])[id][5].Value = Convert.ToSingle(Hurtboxes[id].Z2);

                ((ParamGroup)Param.Groups[4])[id][6].Value = Convert.ToSingle(Hurtboxes[id].Size);
                ((ParamGroup)Param.Groups[4])[id][7].Value = Convert.ToUInt32(Hurtboxes[id].Bone);
                //((ParamGroup)param.Groups[4])[id][8].Value = Convert.ToUInt32(Hurtboxes[id].Part);
                ((ParamGroup)Param.Groups[4])[id][9].Value = Convert.ToUInt32(Hurtboxes[id].Zone);
            }
        }

        public void RenderHurtboxes(float frame, int scriptId, ForgeACMDScript acmdScript, VBN vbn)
        {
            if (Hurtboxes.Count > 0)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.Blend);

                if (acmdScript != null)
                {
                    if (acmdScript.BodyIntangible)
                        return;
                }

                if (scriptId != -1)
                    if (frame + 1 >= MovesData[scriptId].IntangibilityStart && frame + 1 < MovesData[scriptId].IntangibilityEnd)
                        return;

                foreach (var pair in Hurtboxes)
                {
                    var h = pair.Value;
                    if (!h.Visible)
                        continue;

                    var va = new Vector3(h.X, h.Y, h.Z);
                    Bone b = ForgeACMDScript.getBone(h.Bone, vbn);
                    if (b == null) continue; 

                    if (acmdScript != null)
                    {
                        if (acmdScript.IntangibleBones.Contains(h.Bone))
                            continue;
                    }

                    //va = Vector3.Transform(va, b.transform);

                    GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Runtime.hurtboxColor));

                    if (Runtime.renderHurtboxesZone)
                    {
                        switch (h.Zone)
                        {
                            case Hurtbox.LW_ZONE:
                                GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Runtime.hurtboxColorLow));
                                break;
                            case Hurtbox.N_ZONE:
                                GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Runtime.hurtboxColorMed));
                                break;
                            case Hurtbox.HI_ZONE:
                                GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Runtime.hurtboxColorHi));
                                break;
                        }
                    }

                    if (acmdScript != null)
                    {
                        if (acmdScript.SuperArmor)
                            GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, 0x73, 0x0a, 0x43));

                        if (acmdScript.BodyInvincible)
                            GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Color.White));

                        if (acmdScript.InvincibleBones.Contains(h.Bone))
                            GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Color.White));
                    }

                    var va2 = new Vector3(h.X2, h.Y2, h.Z2);

                    //if (h.Bone != -1)va2 = Vector3.Transform(va2, b.transform);

                    if (h.IsSphere)
                    {
                        Rendering.ShapeDrawing.drawSphereTransformedVisible(va, h.Size, 30, b.transform);
                    }
                    else
                    {
                        Rendering.ShapeDrawing.drawReducedCylinderTransformed(va, va2, h.Size, b.transform);
                    }
                    if (Runtime.SelectedHurtboxID == h.Id)
                    {
                        GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Runtime.hurtboxColorSelected));
                        if (h.IsSphere)
                        {
                            Rendering.ShapeDrawing.drawWireframeSphereTransformedVisible(va, h.Size, 20, b.transform);
                        }
                        else
                        {
                            Rendering.ShapeDrawing.drawWireframeCylinderTransformed(va, va2, h.Size, b.transform);
                        }
                    }
                }

                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.DepthTest);
            }
        }

        public static Dictionary<string, int> fighterId = new Dictionary<string, int>
        {
            {"miifighter", 0},
            {"miiswordsman", 1},
            {"miigunner", 2},
            {"mario", 3},
            {"donkey", 4},
            {"link", 5},
            {"samus", 6},
            {"yoshi", 7},
            {"kirby", 8},
            {"fox", 9},
            {"pikachu", 10},
            {"luigi", 11},
            {"captain", 12},
            {"ness", 13},
            {"peach", 14},
            {"koopa", 15},
            {"zelda", 16},
            {"sheik", 17},
            {"marth", 18},
            {"gamewatch", 19},
            {"ganon", 20},
            {"falco", 21},
            {"wario", 22},
            {"metaknight", 23},
            {"pit", 24},
            {"szerosuit", 25},
            {"pikmin", 26},
            {"diddy", 27},
            {"dedede", 28},
            {"ike", 29},
            {"lucario", 30},
            {"robot", 31},
            {"toonlink", 32},
            {"lizardon", 33},
            {"sonic", 34},
            {"purin", 35},
            {"mariod", 36},
            {"lucina", 37},
            {"pitb", 38},
            {"rosetta", 39},
            {"wiifit", 40},
            {"littlemac", 41},
            {"murabito", 42},
            {"palutena", 43},
            {"reflet", 44},
            {"duckhunt", 45},
            {"koopajr", 46},
            {"shulk", 47},
            {"gekkouga", 48},
            {"pacman", 49},
            {"rockman", 50},
            {"mewtwo", 51},
            {"ryu", 52},
            {"lucas", 53},
            {"roy", 54},
            {"cloud", 55},
            {"bayonetta", 56},
            {"kamui", 57},
            {"koopag", 58},
            {"warioman", 59},
            {"littlemacg", 60},
            {"lucariom", 61},
            {"miienemyf", 62},
            {"miienemys", 63},
            {"miienemyg", 64},
        };
    }

    public class Hurtbox
    {
        public int Id { get; set; }
        public int Bone { get; set; }
        public float Size { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }
        public float Z2 { get; set; }
        public int Zone { get; set; }
        public bool IsSphere { get; set; } = false;

        public const int LW_ZONE = 0;
        public const int N_ZONE = 1;
        public const int HI_ZONE = 2;

        public int Part { get; set; }

        //Forge vars
        public bool Visible { get; set; } = true;
    }

    public class MoveData
    {
        public int Index { get; set; }
        public int Faf { get; set; }
        public int IntangibilityStart { get; set; }
        public int IntangibilityEnd { get; set; }
    }

    public class LedgeGrabBox
    {
        public int Id { get; set; }
        public float Z1 { get; set; }
        public float Y1 { get; set; }
        public float Z2 { get; set; }
        public float Y2 { get; set; }

        public bool Tether
        {
            get
            {
                //Characters can have tether ledge grabboxes on various IDs so just check those that are very big (all normal grabboxes have values lower than 20)
                return Z2 > 20 || Y2 > 20; 
            }
        }
    }

    public class Ecb
    {
        public int Id { get; set; }
        public int Bone { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class SpecialBubble
    {
        public enum BubbleType
        {
            COUNTER,
            REFLECT,
            ABSORB,
            SHIELD,
            WT_SLOWDOWN,
            OTHER = -1
        }

        public List<string> Animations { get; set; }
        public int Id { get; set; }
        public int Bone { get; set; } = 0;
        public float Size { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }
        public float Z2 { get; set; }
        public bool IsSphere {
            get
            {
                return X2 == 0 && Y2 == 0 && Z2 == 0;
            }
        }

        public BubbleType type;

        public Color Color
        {
            get
            {
                switch (type)
                {
                    case BubbleType.COUNTER:
                        return Runtime.counterBubbleColor;
                    case BubbleType.REFLECT:
                        return Runtime.reflectBubbleColor;
                    case BubbleType.ABSORB:
                        return Runtime.absorbBubbleColor;
                    case BubbleType.SHIELD:
                        return Runtime.shieldBubbleColor;
                    case BubbleType.WT_SLOWDOWN:
                        return Runtime.wtSlowdownBubbleColor;
                }
                return Color.FromArgb(0x32, 0x32, 0x32);
            }
        }

        public int StartFrame { get; set; }
        public int EndFrame { get; set; }
    }

    class CsvSpecialBubble
    {
        public string Character { get; set; }
        public string Animation { get; set; }

        public int ParamGroup { get; set; }
        public int ParamEntry { get; set; }

        public int Id { get; set; }

        //If value == -1 then set it to 0
        public int Bone { get; set; } = 0;
        public int Size { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }
        public int Z2 { get; set; }

        public int Type { get; set; }

        //Manual frames since they vary their activation: Defensive_Collision, Bit_Variable_Set, WT activation
        public int StartFrame { get; set; }
        public int EndFrame { get; set; }

        public bool SetBone { get; set; } //Bone has an ! at the start, this means bone value is given on csv and it isn't on params nor default 0

        public static List<CsvSpecialBubble> Read(string filename)
        {
            StreamReader reader = new StreamReader(File.OpenRead(filename));
            bool firstLine = false;

            List<CsvSpecialBubble> list = new List<CsvSpecialBubble>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                if (!firstLine)
                {
                    firstLine = true;
                    continue;
                }

                string[] values = line.Split(',');

                list.Add(new CsvSpecialBubble()
                {
                    Character = values[0],
                    Animation = values[1],
                    Id = Convert.ToInt32(values[2]),
                    ParamGroup = Convert.ToInt32(values[3]),
                    ParamEntry = Convert.ToInt32(values[4]),
                    Bone = Convert.ToInt32(values[5].Replace("!", "")),
                    X = Convert.ToInt32(values[6]),
                    Y = Convert.ToInt32(values[7]),
                    Z = Convert.ToInt32(values[8]),
                    X2 = Convert.ToInt32(values[9]),
                    Y2 = Convert.ToInt32(values[10]),
                    Z2 = Convert.ToInt32(values[11]),
                    Size = Convert.ToInt32(values[12]),
                    Type = Convert.ToInt32(values[13]),
                    StartFrame = Convert.ToInt32(values[14]),
                    EndFrame = Convert.ToInt32(values[15]), //-1 = Active until end of animation

                    SetBone = values[5].Contains("!")

                });
            }
            return list;
        }
    }
}
