using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALT.Scripting.AnimCMD;
using System.IO;
using System.Security.Cryptography;
using OpenTK;

namespace Smash_Forge
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

        public acmd_frame CommandsAtFrame(string animation, int frame)
        {
            var _commandsGame = new List<ACMDCommand>();
            var _commandsEffect = new List<ACMDCommand>();
            var _commandsSound = new List<ACMDCommand>();
            var _commandsExpression = new List<ACMDCommand>();
            var _hash = Crc32.Compute(animation.ToLower());

            var info = new acmd_frame();
            info.fnum = frame;

            if (Game != null)
            {
                if (Game.Scripts.ContainsKey(_hash))
                {
                    info.Game = CommandsAtFrame((ACMDScript)Game.Scripts[_hash], frame);
                }
            }
            if (Effect != null)
            {
                if (Effect.Scripts.ContainsKey(_hash))
                {
                    info.Effect = CommandsAtFrame((ACMDScript)Effect.Scripts[_hash], frame);
                }
            }
            if (Sound != null)
            {
                if (Sound.Scripts.ContainsKey(_hash))
                {
                    info.Sound = CommandsAtFrame((ACMDScript)Sound.Scripts[_hash], frame);
                }
            }
            if (Expression != null)
            {
                if (Expression.Scripts.ContainsKey(_hash))
                {
                    info.Expression = CommandsAtFrame((ACMDScript)Expression.Scripts[_hash], frame);
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
    public class acmd_frame
    {
        public acmd_frame()
        {
            ActiveHitboxes = new Dictionary<int, Hitbox>();
        }

        public int fnum;
        public Dictionary<int, Hitbox> ActiveHitboxes { get; set; }

        public ACMDCommand[] Game;
        public ACMDCommand[] Effect;
        public ACMDCommand[] Sound;
        public ACMDCommand[] Expression;
    }

    public class Hitbox
    {
        public float Damage { get; set; }
        public float Angle { get; set; }
        public float KnockbackGrowth { get; set; }
        public float KnockbackBase { get; set; }

        public int Type { get; set; }

        public bool Extended { get; set; }
        public bool Ignore_Throw { get; set; }

        public const int HITBOX = 0;
        public const int GRABBOX = 1;
        public const int WINDBOX = 2;
        public const int SEARCHBOX = 3;

        public int Bone { get; set; }
        public float Size { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }
        public float Z2 { get; set; }

        // Stuff for interpolation, set during rendering
        // These are *post transform*
        public Vector3 va, va2;
    }
}
