using SALT.Scripting;
using SALT.Scripting.AnimCMD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smash_Forge
{
    public class ForgeACMDScript
    {
        public class ScriptState
        {
            public int commandIndex;
            public int loopStart;
            public int loopIterations;

            public ScriptState(int commandIndex, int loopStart, int loopIterations)
            {
                this.commandIndex = commandIndex;
                this.loopStart = loopStart;
                this.loopIterations = loopIterations;
            }
        }

        public int scriptId { get; set; }
        public ACMDScript script { get; set; }
        public List<ACMDScript> subscripts { get; set; }  // For subscript processing, shares currentFrame between all scripts

        // Data processed by script
        public SortedList<int, Hitbox> Hitboxes { get; set; }
        // For interpolation
        public SortedList<int, Hitbox> LastHitboxes { get; set; }
        // Intangibility/Invincibility
        public List<int> IntangibleBones { get; set; }
        public List<int> InvincibleBones { get; set; }
        public bool BodyIntangible { get; set; }
        public bool BodyInvincible { get; set; }
        public bool SuperArmor { get; set; }

        // Script processing helpers
        public int currentGameFrame { get; set; }   // In-game frame # for the move. One in-game frame can skip multiple animation frames.
        public double currentFrame { get; set; }     // Script frame, can be fractional due to frameSpeed values
        public double frameSpeed { get; set; }       // Set by Set_Frame_Duration. How many in-game frames maps to an animation frame right now.
        public List<int> animationFrames;           // Used to track historical animationFrames so that the right one can be returned
        public int animationFrame                   // The current animation frame to display in-game
        { 
            // This getter exists solely to match up what animation frame the ACMD
            // results should be seen on as it would appear in-game.
            get
            {
                if (animationFrames.Count == 0)
                    return 0;
                return animationFrames[animationFrames.Count - 1];
            }
        }
        public bool incrementFrameValue;   // Whether to increment the frame value next frame
        public bool adjustAnimFrames;      // Fixup for anim frames, only happens when frameSpeed <= 1 on first frame

        // Trying to emulate float comparison with the smash engine
        public static double Epsilon = 0.000000000001;

        public int scriptCommandIndex { get; set; }
        private int loopStart;
        private int loopIterations;
        private double framesSinceSyncTimerStarted;
        // To track state through subscripts
        public Stack<ScriptState> scriptStates { get; set; }

        public ForgeACMDScript(ACMDScript script)
        {
            this.script = script;
            Reset();
        }

        public void Reset()
        {
            Hitboxes           = new SortedList<int, Hitbox>(Comparer<int>.Create((x, y) => y.CompareTo(x)));
            LastHitboxes       = new SortedList<int, Hitbox>(Comparer<int>.Create((x, y) => y.CompareTo(x)));
            InvincibleBones    = new List<int>();
            IntangibleBones    = new List<int>();
            BodyIntangible     = false;
            BodyInvincible     = false;
            SuperArmor         = false;

            currentGameFrame   = 0;
            currentFrame       = 0;
            frameSpeed         = 1;
            animationFrames    = new List<int>();
            incrementFrameValue = false;  // First frame has weird stuff going on with this

            scriptCommandIndex = 0;
            loopStart          = 0;
            loopIterations     = 0;
            framesSinceSyncTimerStarted = -1;
            subscripts         = new List<ACMDScript>();
            scriptStates       = new Stack<ScriptState>();
        }

        public void addOrOverwriteHitbox(int id, Hitbox newHitbox)
        {
            newHitbox.ID = id;
            if (Hitboxes.ContainsKey(id))
                Hitboxes[id] = newHitbox;
            else
                Hitboxes.Add(id, newHitbox);
        }

        //public void processToInGameFrame(int frame)
        public void processToFrame(int frame)
        {
            if (script == null)
                return;

            bool wasReset = false;
            if (frame < currentGameFrame)
            {
                // Reset script to frame 0 and process from there
                Reset();
                wasReset = true;
            }

            bool keepProcessing;
            while (currentGameFrame <= frame)
            {
                updateHitboxes(true);  // once per in-game frame

                // ACMD subsystem stuff now
                if (incrementFrameValue)
                {
                    currentFrame += frameSpeed;
                }

                keepProcessing = true;
                while (keepProcessing)
                {
                    keepProcessing = false;

                    processFrame();
                    // Weird checks done only on the first frame
                    if (currentFrame < 1)
                    {
                        currentFrame += frameSpeed;
                        if (currentFrame > 1)
                        {
                            // Do not increment frame value next frame, defer processing to next frame
                            incrementFrameValue = false;
                            keepProcessing = false;
                        }
                        else
                        {
                            // Increment frame value next frame, process ACMD at least one more time in this in-game frame
                            incrementFrameValue = true;
                            keepProcessing = true;
                            adjustAnimFrames = true;  // Fixup for animation frames
                        }
                    }
                    else  // Every other frame
                    {
                        incrementFrameValue = true;
                        keepProcessing = false;
                    }
                    //Console.WriteLine($"END ACMD FRAME: gameFrame={currentGameFrame} animationFrame={animationFrame} currentFrame={currentFrame}");
                }
                int roundedAnimFrame = roundAnimationFrame(currentFrame);
                //if (adjustAnimFrames)
                roundedAnimFrame -= 1;
                animationFrames.Add(roundedAnimFrame);
                currentGameFrame += 1;
                //Console.WriteLine($"END GAME FRAME: gameFrame={currentGameFrame} animationFrame={animationFrame} currentFrame={currentFrame}");
            }

            // Avoid weird interpolations showing if going back a frame
            // Ultimately we want to tie this to the animation eventually so that
            // we aren't relying on the render loop to calculate hitbox positions.
            if (wasReset)
                LastHitboxes.Clear();
        }

        public void processFrame()
        {
            bool continueProcessing = true;
            while (continueProcessing)
            {
                // Subscripts take precedence in processing, most deep one first
                if (subscripts.Count > 0)
                {
                    if (scriptCommandIndex < subscripts[subscripts.Count - 1].Count)
                        continueProcessing = processCommand(subscripts[subscripts.Count - 1][scriptCommandIndex]);
                    else
                    {
                        // We finished the subscript, pop it off and keep processing the parent
                        ScriptState prevState = scriptStates.Pop();
                        scriptCommandIndex = prevState.commandIndex;
                        loopStart = prevState.loopStart;
                        loopIterations = prevState.loopIterations;
                        subscripts.RemoveAt(subscripts.Count - 1);
                    }
                }
                else
                {
                    if (scriptCommandIndex < script.Count)
                        continueProcessing = processCommand(script[scriptCommandIndex]);
                    else
                        // No script commands left to process, regardless of frame
                        break;
                }
                if (continueProcessing)
                    scriptCommandIndex++;
            }
        }

        public void updateHitboxes(bool updateInterpolation)
        {
            // Store the last frame's hitboxes for interpolation reasons
            LastHitboxes = new SortedList<int, Hitbox>(Comparer<int>.Create((x, y) => y.CompareTo(x)));
            foreach (KeyValuePair<int, Hitbox> kvp in Hitboxes)
                LastHitboxes.Add(kvp.Key, (Hitbox)kvp.Value.Clone());
        }

        public static bool doubleEquals(double d1, double d2)
        {
            return (Math.Abs(d1 - d2) < Epsilon);
        }

        /// <summary>
        /// Process the next ACMD command in the queue.
        /// </summary>
        /// <returns>True if we should keep processing, False to stop 
        /// for the rest of the frame.</returns>
        public bool processCommand(ICommand cmd)
        {
            Hitbox newHitbox = null;
            switch (cmd.Ident)
            {
                case 0x42ACFE7D: // Asynchronous Timer, exact animation frame on which next command should run
                    {
                        float continueOnFrame = (float)cmd.Parameters[0];
                        // A way of saying strictly greater than or equal to (>=) with better precision
                        if (doubleEquals(currentFrame, continueOnFrame) || currentFrame > continueOnFrame)
                            return true;  // Timer finished, keep processing ACMD commands
                        return false;  // Timer hasn't finished yet
                    }
                case 0x4B7B6E51: // Synchronous Timer, number of animation frames to wait until next command should run
                    {
                        // Formula from ASM is: stopAfterFrames - ((is_active * frameSpeed) + framesSinceSyncTimerStarted) > 0
                        // THEN the timer has finished.

                        if (framesSinceSyncTimerStarted < 0)
                            // Interesting to note that the first encounter sets the frames to 0
                            // in the game's code, while subsequent timer encounters increment it.
                            // Thus a Synchronous_Timer(Frames=0) would always instantly finish.
                            framesSinceSyncTimerStarted = 0;
                        else
                            framesSinceSyncTimerStarted += frameSpeed;

                        float stopAfterFrames = (int)(float)cmd.Parameters[0];
                        double framesPassed = stopAfterFrames - framesSinceSyncTimerStarted;
                        // A way of saying strictly greater than (>) with better precision
                        if (!doubleEquals(framesPassed, 0d) && framesPassed > 0)
                            return false;  // Timer hasn't finished yet

                        // Timer finished, keep processing ACMD commands
                        framesSinceSyncTimerStarted = -1;
                        return true;
                    }
                case 0x7172A764: // Set_Frame_Duration, sets Frame_Speed such that Arg0 ACMD frames are processed per in-game frame
                    {
                        if (Runtime.useFrameDuration)
                            frameSpeed = 1 / (float)cmd.Parameters[0];
                        break;
                    }
                case 0xB2E91D0C: // Used in bayo scripts to set the Frame_Speed to a specific value
                    {
                        if (Runtime.useFrameDuration)
                            frameSpeed = (float)cmd.Parameters[0];
                        break;
                    }
                case 0xA546845C: // Frame_Speed_Multiplier, sets Frame_Speed = Arg0
                    {
                        if (Runtime.useFrameDuration)
                            frameSpeed = (float)cmd.Parameters[0];
                        break;
                    }
                case 0x9126EBA2: // Subroutine: call another script
                case 0xFA1BC28A: // Subroutine1: call another script
                    // Try and load the other script. If we can't, then just keep going as per normal
                    uint crc = (uint)int.Parse(cmd.Parameters[0] + "");
                    if (Runtime.Moveset.Game.Scripts.ContainsKey(crc))
                    {
                        subscripts.Add((ACMDScript)Runtime.Moveset.Game.Scripts[crc]);

                        // Store the return scriptCommandIndex
                        scriptStates.Push(new ScriptState(
                            scriptCommandIndex,
                            loopStart,
                            loopIterations
                        ));

                        // Start fresh in the new script
                        scriptCommandIndex = -1; // This is incremented immediately in the containing loop hence the -1
                        loopStart = 0;
                        loopIterations = 0;
                    }
                    break;
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
                        Hitboxes.RemoveAt((int)cmd.Parameters[0]);
                    }
                    break;
                case 0x7698BB42: // deactivate previous hitbox
                    Hitboxes.RemoveAt(Hitboxes.Keys.Max());
                    break;
                case 0xEB375E3: // Set Loop
                    loopIterations = int.Parse(cmd.Parameters[0] + "") - 1;
                    loopStart = scriptCommandIndex;
                    break;
                case 0x38A3EC78: // goto
                    if (loopIterations > 0)
                    {
                        scriptCommandIndex = loopStart;
                        loopIterations -= 1;
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
                        switch (state)
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
                case 0x2A25155D: // Set_armor - super armor
                    {
                        int state = (int)cmd.Parameters[0];
                        if (state == 1)
                            SuperArmor = true;
                        else
                            SuperArmor = false;
                        break;
                    }

            }
            return true;
        }

        private static int roundAnimationFrame(double animFrame)
        {
            // Modified version of Math.Round to try an approximate in-game behaviour
            return (int)Math.Floor(animFrame + 0.4);
        }
    }
}
