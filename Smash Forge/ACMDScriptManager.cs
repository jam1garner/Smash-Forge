using SALT.Scripting;
using SALT.Scripting.AnimCMD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smash_Forge
{
    // For processing ACMD files and relaying the state to the GUI
    public class ACMDScriptManager
    {
        public ACMDScript script { get; set; }
        public SortedList<int, Hitbox> Hitboxes { get; set; }
        // For interpolation
        public SortedList<int, Hitbox> LastHitboxes { get; set; }
        // Intangibility/Invincibility
        public List<int> IntangibleBones { get; set; }
        public List<int> InvincibleBones { get; set; }
        public bool BodyIntangible { get; set; }
        public bool BodyInvincible { get; set; }
        public bool FAFReached { get; set; }
        public int currentFrame { get; set; }

        // How fast the current frame is playing in-game. Used to marry up
        // hitboxes to the animation properly. Essentially
        // numAnimationFramesPerRealFrame = (1/currentFrameDuration)
        public float currentFrameDuration { get; set; }
        // What the current frame is modified by various currentFrameDuration instructions
        public float realCurrentFrame { get; set; }
        public int floorRealCurrentFrame { get; set; } // to save computation

        // TODO: these may need to be passed down for proper subscript parsing
        // if any subscripts are used in loops anywhere.
        private int setLoop;
        private int iterations;

        public ACMDScriptManager()
        {
            Reset(hardReset: true);
        }

        public ACMDScriptManager(ACMDScript script)
        {
            Reset();
            this.script = script;
        }

        public void Reset(ACMDScript script = null, bool hardReset = false)
        {
            // Don't reset on the same script
            if (script == this.script && !hardReset)
        public void Reset(ACMDScript script = null, int scriptId = -1, bool hardReset = false)
            // Don't reset on the same script
            if (script == this.script && !hardReset)
                return;

            Hitboxes = new SortedList<int, Hitbox>(Comparer<int>.Create((x, y) => y.CompareTo(x)));
            LastHitboxes = new SortedList<int, Hitbox>(Comparer<int>.Create((x, y) => y.CompareTo(x)));
            InvincibleBones = new List<int>();
            IntangibleBones = new List<int>();
            BodyIntangible = false;
            BodyInvincible = false;
            FAFReached = false;
            resetFrame();

            currentFrame = 0;
            this.script = script;

            setLoop = 0;
            iterations = 0;
        }

        // Reset the animation frame data
        public void resetFrame()
        {
            currentFrameDuration = 1.0f;
            realCurrentFrame = -1f;  // starts at same as halt for proper comparison
            floorRealCurrentFrame = 0;
        }

        public void addOrOverwriteHitbox(int id, Hitbox newHitbox)
        {
            newHitbox.ID = id;
            if (Hitboxes.ContainsKey(id))
            {
                Hitboxes[id] = newHitbox;
            }
            else
            {
                Hitboxes.Add(id, newHitbox);
            }
        }

        public float processScriptCommandsAtCurrentFrame(ICommand cmd, float halt, ref int scriptCommandIndex)
        {
            Hitbox newHitbox = null;
            switch (cmd.Ident)
            {
                case 0x42ACFE7D: // Asynchronous Timer (specific frame start for next commands), unaffected by Frame_Duration
                    {
                        int frame = (int)(float)cmd.Parameters[0];
                        halt = frame >= halt + 2 ? frame - 2 : halt;
                        break;
                    }
                case 0x4B7B6E51: // Synchronous Timer (relative frame start for next commands), unaffected by Frame_Duration
                    {
                        halt = Math.Max(halt, realCurrentFrame);
                        //halt += (float)cmd.Parameters[0] * (1 / currentFrameDuration);
                        halt += (int)(float)cmd.Parameters[0];
                        break;
                    }
                case 0x7172A764: // Set_Frame_Duration (how fast the current frame is playing)
                    {
                        currentFrameDuration = (float)cmd.Parameters[0];
                        break;
                    }
                case 0xB738EABD: // hitbox 
                    {
                        newHitbox = new Hitbox();
                        int id = (int)cmd.Parameters[0];
                        newHitbox.Type = Hitbox.HITBOX;
                        newHitbox.Bone = (int)cmd.Parameters[2];
                        newHitbox.Damage = (float)cmd.Parameters[3];
                        newHitbox.Angle = (int)cmd.Parameters[4];
                        newHitbox.KnockbackGrowth = (int)cmd.Parameters[5];
                        //FKB = (float)cmd.Parameters[6]
                        newHitbox.KnockbackBase = (int)cmd.Parameters[7];
                        newHitbox.Size = (float)cmd.Parameters[8];
                        newHitbox.X = (float)cmd.Parameters[9];
                        newHitbox.Y = (float)cmd.Parameters[10];
                        newHitbox.Z = (float)cmd.Parameters[11];
                        addOrOverwriteHitbox(id, newHitbox);
                        break;
                    }
                case 0x2988D50F: // Extended hitbox
                    {
                        newHitbox = new Hitbox();
                        int id = (int)cmd.Parameters[0];
                        newHitbox.Type = Hitbox.HITBOX;
                        newHitbox.Extended = true;
                        newHitbox.Bone = (int)cmd.Parameters[2];
                        newHitbox.Damage = (float)cmd.Parameters[3];
                        newHitbox.Angle = (int)cmd.Parameters[4];
                        newHitbox.KnockbackGrowth = (int)cmd.Parameters[5];
                        //FKB = (float)cmd.Parameters[6]
                        newHitbox.KnockbackBase = (int)cmd.Parameters[7];
                        newHitbox.Size = (float)cmd.Parameters[8];
                        newHitbox.X = (float)cmd.Parameters[9];
                        newHitbox.Y = (float)cmd.Parameters[10];
                        newHitbox.Z = (float)cmd.Parameters[11];
                        newHitbox.X2 = (float)cmd.Parameters[24];
                        newHitbox.Y2 = (float)cmd.Parameters[25];
                        newHitbox.Z2 = (float)cmd.Parameters[26];
                        addOrOverwriteHitbox(id, newHitbox);
                        break;
                    }
                case 0x14FCC7E4: // special hitbox
                    {
                        newHitbox = new Hitbox();
                        int id = (int)cmd.Parameters[0];
                        newHitbox.Type = Hitbox.HITBOX;
                        newHitbox.Bone = (int)cmd.Parameters[2];
                        newHitbox.Damage = (float)cmd.Parameters[3];
                        newHitbox.Angle = (int)cmd.Parameters[4];
                        newHitbox.KnockbackGrowth = (int)cmd.Parameters[5];
                        //FKB = (float)cmd.Parameters[6]
                        newHitbox.KnockbackBase = (int)cmd.Parameters[7];
                        newHitbox.Size = (float)cmd.Parameters[8];
                        newHitbox.X = (float)cmd.Parameters[9];
                        newHitbox.Y = (float)cmd.Parameters[10];
                        newHitbox.Z = (float)cmd.Parameters[11];
                        if (cmd.Parameters.Count > 39)
                        {
                            if ((int)cmd.Parameters[39] == 1)
                            {
                                newHitbox.Type = Hitbox.WINDBOX;
                            }
                        }
                        addOrOverwriteHitbox(id, newHitbox);
                        break;
                    }
                case 0x7075DC5A: // Extended special hitbox
                    {
                        newHitbox = new Hitbox();
                        int id = (int)cmd.Parameters[0];
                        newHitbox.Type = Hitbox.HITBOX;
                        newHitbox.Extended = true;
                        newHitbox.Bone = (int)cmd.Parameters[2];
                        newHitbox.Damage = (float)cmd.Parameters[3];
                        newHitbox.Angle = (int)cmd.Parameters[4];
                        newHitbox.KnockbackGrowth = (int)cmd.Parameters[5];
                        //FKB = (float)cmd.Parameters[6]
                        newHitbox.KnockbackBase = (int)cmd.Parameters[7];
                        newHitbox.Size = (float)cmd.Parameters[8];
                        newHitbox.X = (float)cmd.Parameters[9];
                        newHitbox.Y = (float)cmd.Parameters[10];
                        newHitbox.Z = (float)cmd.Parameters[11];
                        if ((int)cmd.Parameters[39] == 1)
                        {
                            newHitbox.Type = Hitbox.WINDBOX;
                        }
                        newHitbox.X2 = (float)cmd.Parameters[40];
                        newHitbox.Y2 = (float)cmd.Parameters[41];
                        newHitbox.Z2 = (float)cmd.Parameters[42];
                        addOrOverwriteHitbox(id, newHitbox);
                        break;
                    }
                case 0xCC7CC705: // collateral hitbox (ignored by character being thrown)
                    {
                        newHitbox = new Hitbox();
                        int id = (int)cmd.Parameters[0];
                        newHitbox.Type = Hitbox.HITBOX;
                        newHitbox.Bone = (int)cmd.Parameters[2];
                        newHitbox.Damage = (float)cmd.Parameters[3];
                        newHitbox.Angle = (int)cmd.Parameters[4];
                        newHitbox.KnockbackGrowth = (int)cmd.Parameters[5];
                        //FKB = (float)cmd.Parameters[6]
                        newHitbox.KnockbackBase = (int)cmd.Parameters[7];
                        newHitbox.Size = (float)cmd.Parameters[8];
                        newHitbox.X = (float)cmd.Parameters[9];
                        newHitbox.Y = (float)cmd.Parameters[10];
                        newHitbox.Z = (float)cmd.Parameters[11];

                        newHitbox.Ignore_Throw = true;

                        addOrOverwriteHitbox(id, newHitbox);
                        break;
                    }
                case 0xED67D5DA: // Extended collateral hitbox (ignored by character being thrown)
                    {
                        newHitbox = new Hitbox();
                        int id = (int)cmd.Parameters[0];
                        newHitbox.Type = Hitbox.HITBOX;
                        newHitbox.Extended = true;
                        newHitbox.Bone = (int)cmd.Parameters[2];
                        newHitbox.Damage = (float)cmd.Parameters[3];
                        newHitbox.Angle = (int)cmd.Parameters[4];
                        newHitbox.KnockbackGrowth = (int)cmd.Parameters[5];
                        //FKB = (float)cmd.Parameters[6]
                        newHitbox.KnockbackBase = (int)cmd.Parameters[7];
                        newHitbox.Size = (float)cmd.Parameters[8];
                        newHitbox.X = (float)cmd.Parameters[9];
                        newHitbox.Y = (float)cmd.Parameters[10];
                        newHitbox.Z = (float)cmd.Parameters[11];
                        newHitbox.X2 = (float)cmd.Parameters[24];
                        newHitbox.Y2 = (float)cmd.Parameters[25];
                        newHitbox.Z2 = (float)cmd.Parameters[26];

                        newHitbox.Ignore_Throw = true;

                        addOrOverwriteHitbox(id, newHitbox);
                        break;
                    }
                case 0x9245E1A8: // clear all hitboxes
                    Hitboxes.Clear();
                    break;
                case 0xFF379EB6: // delete hitbox
                    if (Hitboxes.ContainsKey((int)cmd.Parameters[0]))
                    {
                        Hitboxes.Remove((int)cmd.Parameters[0]);
                    }
                    break;
                case 0x7698BB42: // deactivate previous hitbox
                    Hitboxes.Remove(Hitboxes.Keys.Max());
                    break;
                case 0xEB375E3: // Set Loop
                    iterations = int.Parse(cmd.Parameters[0] + "") - 1;
                    setLoop = scriptCommandIndex;
                    break;
                case 0x38A3EC78: // goto
                    if (iterations > 0)
                    {
                        // Can fail if a subscript has a goto with no loop starter
                        scriptCommandIndex = setLoop;
                        iterations -= 1;
                    }
                    break;

                case 0x7B48FE1C: //Extended Grabbox
                case 0x1EAF840C: //Grabbox 2 (most command grabs)
                case 0x548F2D4C: //Grabbox (used in tether grabs)
                case 0xEF787D43: //Extended Grabbox 2 (Mega Man's grab)
                case 0x323FB9D4: //Special Grabbox (Pikmin's grab)
                    {
                        newHitbox = new Hitbox();
                        int id = (int)cmd.Parameters[0];
                        newHitbox.Type = Hitbox.GRABBOX;
                        newHitbox.Bone = int.Parse(cmd.Parameters[1] + "");
                        newHitbox.Size = (float)cmd.Parameters[2];
                        newHitbox.X = (float)cmd.Parameters[3];
                        newHitbox.Y = (float)cmd.Parameters[4];
                        newHitbox.Z = (float)cmd.Parameters[5];

                        if (cmd.Parameters.Count > 10)
                        {
                            newHitbox.X2 = float.Parse(cmd.Parameters[8] + "");
                            newHitbox.Y2 = float.Parse(cmd.Parameters[9] + "");
                            newHitbox.Z2 = float.Parse(cmd.Parameters[10] + "");
                            newHitbox.Extended = true;
                        }

                        addOrOverwriteHitbox(id, newHitbox);
                        break;
                    }
                case 0xF3A464AC: // Terminate_Grab_Collisions
                    {
                        List<int> toDelete = new List<int>();
                        foreach (KeyValuePair<int, Hitbox> kvp in Hitboxes)
                        {
                            if (kvp.Value.Type == Hitbox.GRABBOX)
                                toDelete.Add(kvp.Key);
                        }
                        foreach (int index in toDelete)
                            Hitboxes.Remove(index);
                        break;
                    }
                case 0x2F08F54F: // Delete_Catch_Collision by ID
                    int idToDelete = (int)cmd.Parameters[0];
                    if (Hitboxes[idToDelete].Type == Hitbox.GRABBOX)
                        Hitboxes.Remove(idToDelete);
                    break;
                case 0x44081C21: //SEARCH
                    {
                        newHitbox = new Hitbox();
                        int id = (int)cmd.Parameters[0];
                        if (Hitboxes.ContainsKey(id))
                            Hitboxes.Remove(id);
                        newHitbox.Type = Hitbox.SEARCHBOX;
                        newHitbox.Bone = (int)cmd.Parameters[2];

                        newHitbox.Size = (float)cmd.Parameters[3];
                        newHitbox.X = (float)cmd.Parameters[4];
                        newHitbox.Y = (float)cmd.Parameters[5];
                        newHitbox.Z = (float)cmd.Parameters[6];
                        addOrOverwriteHitbox(id, newHitbox);
                        break;
                    }
                case 0xCD0C1CC9: //Bat Within (it clears WT SEARCH event)
                case 0x98203AF6: //SRH_CLEAR_ALL
                    {
                        List<int> toDelete = new List<int>();
                        foreach (KeyValuePair<int, Hitbox> kvp in Hitboxes)
                        {
                            if (kvp.Value.Type == Hitbox.SEARCHBOX)
                                toDelete.Add(kvp.Key);
                        }
                        foreach (int index in toDelete)
                            Hitboxes.Remove(index);
                        break;
                    }
                case 0xFA1BC28A: //External Subroutine: call another script
                case 0x9126EBA2: //Subroutine
                    halt = processSubscriptCommandsAtCurrentFrame((uint)int.Parse(cmd.Parameters[0] + ""), halt, scriptCommandIndex);
                    break;
                case 0xFAA85333:
                    break;
                case 0x321297B0:
                    break;
                case 0x7640AEEB:
                    break;
                case 0xA5BD4F32: // TRUE
                    break;
                case 0x895B9275: // FALSE
                    break;
                case 0xF13BFE8D: //Bone collision state (intangibility/invincibility)
                    {
                        int bone = (int)cmd.Parameters[0];
                        int state = (int)cmd.Parameters[1];
                        switch(state)
                        {
                            case 2:
                                IntangibleBones.Remove(bone);
                                InvincibleBones.Remove(bone);
                                IntangibleBones.Add(bone);
                                break;
                            case 1:
                                IntangibleBones.Remove(bone);
                                InvincibleBones.Remove(bone);
                                InvincibleBones.Add(bone);
                                break;
                            default:
                                IntangibleBones.Remove(bone);
                                InvincibleBones.Remove(bone);
                                break;
                        }
                        break;
                    }
                case 0xCEDC237E: //Undo Bone collision state
                    {
                        int state = (int)cmd.Parameters[0];
                        IntangibleBones.Clear();
                        InvincibleBones.Clear();
                        break;
                    }
                case 0xF0D25BDA: //Body_Collision (Full intangibility/invincibility)
                    {
                        int state = (int)cmd.Parameters[0];
                        switch (state)
                        {
                            case 2:
                                BodyInvincible = false;
                                BodyIntangible = true;
                                break;
                            case 1:
                                BodyInvincible = true;
                                BodyIntangible = false;
                                break;
                            default:
                                BodyInvincible = false;
                                BodyIntangible = false;
                                break;
                        }
                        break;
                    }

            }
            return halt;
        }

        public void processScript()
        {
            LastHitboxes = Hitboxes;
            // Hitboxes stored in descending order because lower ID hitboxes take priority
            // over higher ID ones. Thus, drawing the lower IDs on top makes sense.
            Hitboxes = new SortedList<int, Hitbox>(Comparer<int>.Create((x, y) => y.CompareTo(x)));
            InvincibleBones = new List<int>();
            IntangibleBones = new List<int>();
            // The next frame the script halts at for execution. Only modified
            // by timer commands.
            float halt = -1;
            int scriptCommandIndex = 0;
            ICommand cmd = script[scriptCommandIndex];
            //ProcessANMCMD_SOUND();

            resetFrame();
            for (int frame = 0; frame <= currentFrame; frame++)
            {
                // This block protects us from animating the model too early back in VBNViewport
                // and from adding to the frame on frame 0
                //if (frame > 0)
                //{

                //}
                while (halt <= realCurrentFrame)
                {
                    halt = processScriptCommandsAtCurrentFrame(cmd, halt, ref scriptCommandIndex);

                    scriptCommandIndex++;
                    if (scriptCommandIndex >= script.Count)
                        break;
                    else
                        cmd = script[scriptCommandIndex];

                    // If the next command is beyond our current anim frame
                    //if (halt > floorRealCurrentFrame)
                    //    break;
                }
                realCurrentFrame += (1 / currentFrameDuration);
                floorRealCurrentFrame = (int)Math.Floor(realCurrentFrame);
            }
            //while (halt < frame)
            //{
            //    halt = processScriptCommandsAtCurrentFrame(cmd, halt, ref scriptCommandIndex);

            //    scriptCommandIndex++;
            //    if (scriptCommandIndex >= script.Count)
            //        break;
            //    else
            //        cmd = script[scriptCommandIndex];

            //    // If the next command is beyond our current anim frame
            //    if (halt > currentFrame)
            //        break;
            //}
        }

        public float processSubscriptCommandsAtCurrentFrame(uint crc, float halt, int scriptCommandIndex)
        {
            // Try and load the other script, if we can't just keep going
            ACMDScript subscript;
            if (Runtime.Moveset.Game.Scripts.ContainsKey(crc))
                subscript = (ACMDScript)Runtime.Moveset.Game.Scripts[crc];
            else
                return halt;

            int subscriptCommandIndex = 0;
            ICommand cmd = subscript[subscriptCommandIndex];
            while (halt < currentFrame)
            {
                halt = processScriptCommandsAtCurrentFrame(cmd, halt, ref subscriptCommandIndex);

                subscriptCommandIndex++;
                if (subscriptCommandIndex >= subscript.Count)
                    break;
                else
                    cmd = subscript[subscriptCommandIndex];

                // If the next command is beyond our current anim frame
                if (halt > currentFrame)
                    break;
            }

            return halt;
        }

        public void processAnimationParams(string animname)
        {
            if (Runtime.ParamMoveNameIdMapping == null)
                return;

            int moveId;
            if (!Runtime.ParamMoveNameIdMapping.TryGetValue(animname, out moveId))
                return;

            MoveData moveData;
            if (!Runtime.ParamManager.MovesData.TryGetValue(moveId, out moveData))
                return;

            // Forge uses 0-index while game data uses 1-index, so convert here
            if (currentFrame >= moveData.IntangibilityStart - 1 && currentFrame < moveData.IntangibilityEnd - 1)
                this.BodyIntangible = true;
            else
                this.BodyIntangible = false;

            if (currentFrame >= moveData.FAF)
                this.FAFReached = true;
            else
                this.FAFReached = false;
        }
    }
}
