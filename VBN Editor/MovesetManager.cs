using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALT.Scripting.AnimCMD;
using System.IO;

namespace VBN_Editor
{
    class MovesetManager
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
                    Endian = Game.Endian;
                }
                if (filename == "sound.bin")
                {
                    Sound = new ACMDFile(file);
                    Endian = Game.Endian;
                }
                if (filename == "expression.bin")
                {
                    Expression = new ACMDFile(file);
                    Endian = Game.Endian;
                }

            }
            if (File.Exists(mtablePath))
                MotionTable = new MTable(mtablePath, Endian);
        }

        public Endianness Endian { get; set; }
        public MTable MotionTable { get; set; }

        public ACMDFile Game { get; set; }
        public ACMDFile Effect { get; set; }
        public ACMDFile Sound { get; set; }
        public ACMDFile Expression { get; set; }

        private ACMDScript TargetScript { get; set; }

        public ACMDCommand[] CommandsAtFrame(int frame, string animation)
        {
            int curFrame = 0;
            var commands = new List<ACMDCommand>();
            // Only do game.bin scripts for now
           // if ((Game?.Scripts).ContainsKey(System.Security.Cryptography.Crc32.Compute(animation)))
                foreach (ACMDCommand cmd in TargetScript.Commands)
                {
                    if (curFrame < frame)
                    {
                        if (cmd.Ident == 0x42ACFE7D)
                        {
                            curFrame = (int)cmd.Parameters[0];
                        }
                        else if (cmd.Ident == 0x4B7B6E51)
                        {
                            curFrame++;
                        }
                        continue;
                    }
                    else if (curFrame > frame)
                    {
                        return commands.ToArray();
                    }

                    commands.Add(cmd);
                }
            return commands.ToArray();
        }
    }
}
