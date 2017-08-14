using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALT.PARAMS;

namespace Smash_Forge
{
    public class CharacterParamManager
    {
        public ParamFile param { get; set; }
        public SortedList<int, MoveData> MovesData { get; set; }
        public SortedList<int, ECB> ECBs { get; set; }
        public SortedList<int, Hurtbox> Hurtboxes { get; set; }
        public SortedList<int, LedgeGrabbox> LedgeGrabboxes { get; set; }

        public CharacterParamManager()
        {
            Reset();
        }

        public CharacterParamManager(string file)
        {
            Reset();
            try
            {
                param = new ParamFile(file);

                //Move data (FAF, Intangibility)

                for (int id = 0; id < ((ParamGroup)param.Groups[0]).Chunks.Length; id++)
                {
                    MoveData m = new MoveData();
                    m.Index = id;
                    m.FAF = Convert.ToInt32(((ParamGroup)param.Groups[0])[id][2].Value);
                    m.IntangibilityStart = Convert.ToInt32(((ParamGroup)param.Groups[0])[id][3].Value);
                    m.IntangibilityEnd = Convert.ToInt32(((ParamGroup)param.Groups[0])[id][4].Value);

                    MovesData.Add(id, m);
                }

                //ECB

                for (int id = 0; id < ((ParamGroup)param.Groups[3]).Chunks.Length; id++)
                {
                    ECB ecb = new ECB();
                    ecb.ID = id;
                    ecb.Bone = Convert.ToInt32(((ParamGroup)param.Groups[3])[id][0].Value);
                    ecb.X = Convert.ToSingle(((ParamGroup)param.Groups[3])[id][1].Value);
                    ecb.Y = Convert.ToSingle(((ParamGroup)param.Groups[3])[id][2].Value);
                    ecb.Z = Convert.ToSingle(((ParamGroup)param.Groups[3])[id][3].Value);

                    ECBs.Add(id, ecb);
                }

                //Hurtboxes

                for (int id = 0; id < ((ParamGroup)param.Groups[4]).Chunks.Length; id++)
                {
                    Hurtbox hurtbox = new Hurtbox();
                    hurtbox.ID = id;
                    hurtbox.X = Convert.ToSingle(((ParamGroup)param.Groups[4])[id][0].Value);
                    hurtbox.Y = Convert.ToSingle(((ParamGroup)param.Groups[4])[id][1].Value);
                    hurtbox.Z = Convert.ToSingle(((ParamGroup)param.Groups[4])[id][2].Value);

                    hurtbox.X2 = Convert.ToSingle(((ParamGroup)param.Groups[4])[id][3].Value);
                    hurtbox.Y2 = Convert.ToSingle(((ParamGroup)param.Groups[4])[id][4].Value);
                    hurtbox.Z2 = Convert.ToSingle(((ParamGroup)param.Groups[4])[id][5].Value);

                    hurtbox.Size = Convert.ToSingle(((ParamGroup)param.Groups[4])[id][6].Value);
                    hurtbox.Bone = Convert.ToInt32(((ParamGroup)param.Groups[4])[id][7].Value);
                    hurtbox.Part = Convert.ToInt32(((ParamGroup)param.Groups[4])[id][8].Value);
                    hurtbox.Zone = Convert.ToInt32(((ParamGroup)param.Groups[4])[id][9].Value);

                    if (hurtbox.X == hurtbox.X2 && hurtbox.Y == hurtbox.Y2 && hurtbox.Z == hurtbox.Z2)
                    {
                        // It can't be anything but a sphere. I think some part of the param might
                        // control this so this might be a crude detection method. This fixes Bowser Jr at least.
                        hurtbox.isSphere = true;
                    }

                    Hurtboxes.Add(id, hurtbox);
                }

                //Ledge grabboxes

                for (int id = 0; id < ((ParamGroup)param.Groups[6]).Chunks.Length; id++)
                {
                    LedgeGrabbox l = new LedgeGrabbox();
                    l.ID = id;
                    l.X1 = Convert.ToSingle(((ParamGroup)param.Groups[6])[id][0].Value);
                    l.Y1 = Convert.ToSingle(((ParamGroup)param.Groups[6])[id][1].Value);
                    l.X2 = Convert.ToSingle(((ParamGroup)param.Groups[6])[id][2].Value);
                    l.Y2 = Convert.ToSingle(((ParamGroup)param.Groups[6])[id][3].Value);

                    LedgeGrabboxes.Add(id, l);
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
            Hurtboxes = new SortedList<int, Hurtbox>();
            MovesData = new SortedList<int, MoveData>();
            LedgeGrabboxes = new SortedList<int, LedgeGrabbox>();
            ECBs = new SortedList<int, ECB>();
            param = null;
        }

        public void UnselectHurtboxes()
        {
            Runtime.SelectedHurtboxID = -1;
        }

        public void SaveHurtboxes()
        {
            if (param == null)
                return;

            for (int id = 0; id < ((ParamGroup)param.Groups[4]).Chunks.Length; id++)
            {
                ((ParamGroup)param.Groups[4])[id][0].Value = Convert.ToSingle(Hurtboxes[id].X);
                ((ParamGroup)param.Groups[4])[id][1].Value = Convert.ToSingle(Hurtboxes[id].Y);
                ((ParamGroup)param.Groups[4])[id][2].Value = Convert.ToSingle(Hurtboxes[id].Z);

                ((ParamGroup)param.Groups[4])[id][3].Value = Convert.ToSingle(Hurtboxes[id].X2);
                ((ParamGroup)param.Groups[4])[id][4].Value = Convert.ToSingle(Hurtboxes[id].Y2);
                ((ParamGroup)param.Groups[4])[id][5].Value = Convert.ToSingle(Hurtboxes[id].Z2);

                ((ParamGroup)param.Groups[4])[id][6].Value = Convert.ToSingle(Hurtboxes[id].Size);
                ((ParamGroup)param.Groups[4])[id][7].Value = Convert.ToUInt32(Hurtboxes[id].Bone);
                //((ParamGroup)param.Groups[4])[id][8].Value = Convert.ToUInt32(Hurtboxes[id].Part);
                ((ParamGroup)param.Groups[4])[id][9].Value = Convert.ToUInt32(Hurtboxes[id].Zone);
            }
        }
    }

    public class Hurtbox
    {
        public int ID { get; set; }
        public int Bone { get; set; }
        public float Size { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }
        public float Z2 { get; set; }
        public int Zone { get; set; }
        public bool isSphere { get; set; } = false;

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
        public int FAF { get; set; }
        public int IntangibilityStart { get; set; }
        public int IntangibilityEnd { get; set; }
    }

    public class LedgeGrabbox
    {
        public int ID { get; set; }
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }
    }

    public class ECB
    {
        public int ID { get; set; }
        public int Bone { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}
