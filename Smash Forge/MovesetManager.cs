using System;
using System.Collections.Generic;
using System.Linq;
using SALT.Moveset.AnimCMD;
using System.IO;
using System.Security.Cryptography;
using OpenTK;
using System.Drawing;

namespace SmashForge
{
    public class MovesetManager
    {
        public MovesetManager(string mtablePath)
        {
            var directory = Path.GetDirectoryName(mtablePath);
            string[] files = Directory.EnumerateFiles(directory, "*.bin").ToArray();

            if (files.Length == 0)
                return;

            foreach (string file in files)
            {
                var filename = Path.GetFileName(file);

                if (filename == "game.bin")
                {
                    Game = new ACMDFile(file);
                    Endian = Game.Endian;
                }
                if (filename == "effect.bin")
                {
                    Effect = new ACMDFile(file);
                    Endian = Effect.Endian;
                }
                if (filename == "sound.bin")
                {
                    Sound = new ACMDFile(file);
                    Endian = Sound.Endian;
                }
                if (filename == "expression.bin")
                {
                    Expression = new ACMDFile(file);
                    Endian = Expression.Endian;
                }

            }
            if (File.Exists(mtablePath))
                MotionTable = new MTable(mtablePath, Endian);

            ScriptsHashList = new List<uint>();
            if (MotionTable != null)
                ScriptsHashList.AddRange(MotionTable.ToList());

        }

        public Dictionary<int, Hitbox> Hitboxes { get; set; }

        public Endianness Endian { get; set; }
        public MTable MotionTable { get; set; }

        public ACMDFile Game { get; set; }
        public ACMDFile Effect { get; set; }
        public ACMDFile Sound { get; set; }
        public ACMDFile Expression { get; set; }

        public List<uint> ScriptsHashList { get; set; }

        public void Save(String fname)
        {
            string path = Path.GetDirectoryName(fname) + "\\";
            MotionTable.Export(fname);
            Game.Export(path + "game.bin");
            Effect.Export(path + "effect.bin");
            Sound.Export(path + "sound.bin");
            Expression.Export(path + "expression.bin");
        }

        public AcmdFrame CommandsAtFrame(string animation, int frame)
        {
            var commandsGame = new List<ACMDCommand>();
            var commandsEffect = new List<ACMDCommand>();
            var commandsSound = new List<ACMDCommand>();
            var commandsExpression = new List<ACMDCommand>();
            var hash = Crc32.Compute(animation.ToLower());

            var info = new AcmdFrame();
            info.fnum = frame;

            if (Game != null)
            {
                if (Game.Scripts.ContainsKey(hash))
                {
                    info.game = CommandsAtFrame((ACMDScript)Game.Scripts[hash], frame);
                }
            }
            if (Effect != null)
            {
                if (Effect.Scripts.ContainsKey(hash))
                {
                    info.effect = CommandsAtFrame((ACMDScript)Effect.Scripts[hash], frame);
                }
            }
            if (Sound != null)
            {
                if (Sound.Scripts.ContainsKey(hash))
                {
                    info.sound = CommandsAtFrame((ACMDScript)Sound.Scripts[hash], frame);
                }
            }
            if (Expression != null)
            {
                if (Expression.Scripts.ContainsKey(hash))
                {
                    info.expression = CommandsAtFrame((ACMDScript)Expression.Scripts[hash], frame);
                }
            }
            return info;
        }
        public ACMDCommand[] CommandsAtFrame(ACMDScript script, int frame)
        {
            int curFrame = 0;
            var commands = new List<ACMDCommand>();

            // Only do game.bin scripts for now
            if (Game == null)
                return null;

            foreach (ACMDCommand cmd in script.Commands)
            {
                if (cmd.Ident == 0x42ACFE7D)
                {
                    curFrame = (int)(float)cmd.Parameters[0];
                    continue;
                }
                else if (cmd.Ident == 0x4B7B6E51)
                {
                    curFrame++;
                    continue;
                }

                else if (curFrame == frame)
                {
                    commands.Add(cmd);
                }
                else if (curFrame > frame)
                {
                    return commands.ToArray();
                }
            }
            return commands.ToArray();
        }
    }
    public class AcmdFrame
    {
        public AcmdFrame()
        {
            ActiveHitboxes = new Dictionary<int, Hitbox>();
        }

        public int fnum;
        public Dictionary<int, Hitbox> ActiveHitboxes { get; set; }

        public ACMDCommand[] game;
        public ACMDCommand[] effect;
        public ACMDCommand[] sound;
        public ACMDCommand[] expression;
    }

    public class Hitbox : ICloneable
    {
        public int Id { get; set; }

        public float Damage { get; set; }
        public int Angle { get; set; }
        public float KnockbackGrowth { get; set; }
        public float KnockbackBase { get; set; }
        public float WeightBasedKnockback { get; set; }

        public int Type { get; set; }

        public bool Extended { get; set; }
        public bool IgnoreThrow { get; set; }

        public const int HitboxValue = 0;
        public const int Grabbox = 1;
        public const int Windbox = 2;
        public const int Searchbox = 3;

        public const int RenderNormal = 0;
        public const int RenderKnockback = 1;
        public const int RenderId = 2;

        public int Part { get; set; } = 0;
        public int Bone { get; set; }
        public float Size { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public int FacingRestriction { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }
        public float Z2 { get; set; }

        // Stuff for interpolation, set during rendering
        // These are *post transform*
        public Vector3 va, va2;

        public Object Clone()
        {
            Hitbox h = (Hitbox)this.MemberwiseClone();
            h.va = new Vector3(va);
            h.va2 = new Vector3(va2);
            return h;
        }

        public bool IsSphere()
        {
            if (Extended)
                return (X2 == X && Y2 == Y && Z2 == Z);
            return true;
        }

        // Just a quick way to generalise this for all cases without opponent weight being involved
        public float GetSimplifiedKnockback(float damage, float knockbackBase,
            float knockbackGrowth, float targetPercent)
        {
            return (((damage + targetPercent) / 10) + (((targetPercent + damage) * damage) / 20)) *
                (knockbackGrowth / 100) + knockbackBase;
        }

        // These values were chosen to capture the middle 70% of hitboxes. Outliers
        // are just pulled in to the max/min values.
        public static readonly float kbUpperThreshold = 210;
        public static readonly float kbLowerThreshold = 100;
        public int GetKnockbackBucket(float knockback)
        {
            float bucketRange = (kbUpperThreshold - kbLowerThreshold) / Runtime.hitboxKnockbackColors.Count;
            if (knockback < kbLowerThreshold) knockback = kbLowerThreshold;
            if (knockback > kbUpperThreshold) knockback = kbUpperThreshold - 0.001f;
            return (int)Math.Floor((knockback - kbLowerThreshold) / bucketRange);
        }

        public Color GetRegularDisplayColor()
        {
            if (Runtime.hitboxRenderMode == RenderKnockback)
            {
                // Chooses different color from distinctColours depending on knockback
                // or things like spike angle
                if (Angle > 245 && Angle < 295)
                    return Color.FromArgb(0xFF, Color.Black);
                float kb = GetSimplifiedKnockback(Damage, KnockbackBase, KnockbackGrowth, 160);
                return Runtime.hitboxKnockbackColors[GetKnockbackBucket(kb)];
            }
            else
            {
                if (Id < Runtime.hitboxIdColors.Count)
                    return Runtime.hitboxIdColors[Id];
                return Runtime.hitboxIdColors[0];  // confusing sure, but better than a runtime error
            }
        }

        // The color to fill a Hitbox with when displaying
        public Color GetDisplayColor()
        {
            Color color;
            switch (Type)
            {
                case Hitbox.HitboxValue:
                    if (IgnoreThrow)
                        color = Color.FromArgb(Runtime.hitboxAlpha, 0x59, 0x33, 0x15); // Deep yellowish brown
                    else
                        if (Runtime.hitboxRenderMode == Hitbox.RenderNormal)
                            color = Color.FromArgb(Runtime.hitboxAlpha, Color.Red);
                        else
                            color = Color.FromArgb(Runtime.hitboxAlpha, GetRegularDisplayColor());
                    break;
                case Hitbox.Grabbox:
                    color = Color.FromArgb(Runtime.hitboxAlpha, Runtime.grabboxColor);
                    break;
                case Hitbox.Windbox:
                    color = Color.FromArgb(Runtime.hitboxAlpha, Runtime.windboxColor);
                    break;
                case Hitbox.Searchbox:
                    color = Color.FromArgb(Runtime.hitboxAlpha, Runtime.searchboxColor);
                    break;
                default:
                    color = Color.FromArgb(Runtime.hitboxAlpha, Color.FloralWhite);
                    break;
            }
            return color;
        }

        public string GetHitboxType()
        {
            switch (Type)
            {
                case Hitbox.HitboxValue:
                    return "Hitbox";
                case Hitbox.Grabbox:
                    return "Grabbox";
                case Hitbox.Searchbox:
                    return "Searchbox";
                case Hitbox.Windbox:
                    return "Windbox";
                default:
                    return "Hitbox";
            }
        }
    }
}
