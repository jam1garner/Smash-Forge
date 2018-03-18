using SALT.Moveset;
using SALT.Moveset.AnimCMD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

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

        public MovesetManager Moveset
        {
            get
            {
                if(MainForm.dockPanel.ActiveContent is ModelViewport)
                {
                    return ((ModelViewport)MainForm.dockPanel.ActiveContent).MovesetManager;
                }
                return null;
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

        // Script variables
        public List<uint> ActiveFlags { get; set; }
        public SortedList<uint, bool> IfVariableList { get; set; } = new SortedList<uint, bool>(); //Variables that have a bit flag
        public SortedList<uint, int> IfVariableValueList { get; set; } = new SortedList<uint, int>(); //Variables that have a value
        public SortedList<uint, List<int>> VariableValueList { get; set; } = new SortedList<uint, List<int>>(); //List of values these variables can have


        // Script processing helpers
        public int currentGameFrame { get; set; }   // In-game frame # for the move. One in-game frame can skip multiple animation frames.
        public double currentFrame { get; set; }     // Script frame, can be fractional due to frameSpeed values
        public double currentAnimationFrame { get; set; }     // Script frame, can be fractional due to frameSpeed values
        public double frameSpeed { get; set; }       // Set by Set_Frame_Duration. How many in-game frames maps to an animation frame right now.
        public List<float> animationFrames;           // Used to track historical animationFrames so that the right one can be returned
        public float animationFrame                   // The current animation frame to display in-game
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
        public static double Epsilon = 0.001;

        public int scriptCommandIndex { get; set; }
        private int loopStart;
        private int loopIterations;
        private double framesSinceSyncTimerStarted;
        // To track state through subscripts
        public Stack<ScriptState> scriptStates { get; set; }

        //Conditionals
        public int TrueFalseSkipLength = 0;
        public bool readTrue = true;

        //Ledge grab
        public bool LedgeGrabDisallowed = false;
        public bool FrontLedgeGrabAllowed = false;
        public bool ReverseLedgeGrabAllowed = false;

        public ForgeACMDScript(ACMDScript script)
        {
            this.script = script;
            Reset();


            //Get all If events variables for BitVariableList
            IfVariableList = new SortedList<uint, bool>();
            IfVariableValueList = new SortedList<uint, int>();
            VariableValueList = new SortedList<uint, List<int>>();

            if (script != null)
            {
                getIfs();

                foreach(var pair in VariableValueList)
                {
                    if (!pair.Value.Contains(-1))
                        pair.Value.Add(-1); //There are some scripts that have false blocks so putting this value for those (One of peach scripts has -1 but only true blocks)
                }

                if (Runtime.variableViewer != null)
                    Runtime.variableViewer.Initialize();

            }
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
            ActiveFlags        = new List<uint>();

            //Reset hidden hitboxes so all hitboxes are visible
            Runtime.HiddenHitboxes.Clear();

            currentGameFrame   = 0;
            currentFrame       = 0;
            currentAnimationFrame = 0;
            frameSpeed         = 1;
            animationFrames    = new List<float>();
            incrementFrameValue = false;  // First frame has weird stuff going on with this

            scriptCommandIndex = 0;
            loopStart          = 0;
            loopIterations     = 0;
            framesSinceSyncTimerStarted = -1;
            subscripts         = new List<ACMDScript>();
            scriptStates       = new Stack<ScriptState>();

            TrueFalseSkipLength = 0;
            LedgeGrabDisallowed = false;
            FrontLedgeGrabAllowed = false;
            ReverseLedgeGrabAllowed = false;
        }

        public void addOrOverwriteHitbox(int id, Hitbox newHitbox)
        {
            newHitbox.ID = id;
            if (Hitboxes.ContainsKey(id))
                Hitboxes[id] = newHitbox;
            else
                Hitboxes.Add(id, newHitbox);
        }

        // Get the total number of in-game frames this move goes for, including
        // modified frame speed
        public int calculateTotalFrames(int totalAnimFrames)
        {
            ForgeACMDScript tempScript = new ForgeACMDScript(this.script);

            // Calculate frames
            int gameFrame = -1;
            while (tempScript.animationFrame < totalAnimFrames)
            {
                gameFrame++;
                tempScript.processToFrame(gameFrame);
            }
            return gameFrame;
        }

        public int calculateFAF(int FAF)
        {
            ForgeACMDScript tempScript = new ForgeACMDScript(this.script);

            int gameFrame = -1;
            while (tempScript.currentFrame < FAF)
            {
                gameFrame++;
                tempScript.processToFrame(gameFrame);
            }

            return tempScript.currentGameFrame;
        }

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
                // Theory: actual animation frame keeps processing regardless
                currentAnimationFrame += frameSpeed;

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
                    //Console.WriteLine($"END ACMD FRAME: gameFrame={currentGameFrame} animationFrame={animationFrame} currentFrame={currentFrame} currentAnimationFrame={currentAnimationFrame}");
                }
                int roundedAnimFrame = roundAnimationFrame(currentAnimationFrame);
                roundedAnimFrame -= 1;
                animationFrames.Add((float)currentAnimationFrame - 1);
                currentGameFrame += 1;
                //Console.WriteLine($"END GAME FRAME: gameFrame={currentGameFrame} animationFrame={animationFrame} currentFrame={currentFrame} currentAnimationFrame={currentAnimationFrame}");
            }

            // Avoid weird interpolations showing if going back a frame
            // Ultimately we want to tie this to the animation eventually so that
            // we aren't relying on the render loop to calculate hitbox positions.
            if (wasReset)
                LastHitboxes.Clear();

            if (Runtime.hitboxList != null)
                Runtime.hitboxList.refresh();
            if (Runtime.variableViewer != null)
                Runtime.variableViewer.refresh();
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
            if(TrueFalseSkipLength > 0)
            {
                //Skip command since its on a true/false block and the variable is set to the other value
                TrueFalseSkipLength -= cmd.Parameters.Count + 1;
                return true;
            }
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
                            frameSpeed = 1 / (float)cmd.Parameters[0];
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

                    if (Moveset == null) break;
                    if (Moveset.Game.Scripts.ContainsKey(crc)) //TODO:
                    {
                        subscripts.Add((ACMDScript)Moveset.Game.Scripts[crc]);

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
                        newHitbox.Part = (int)cmd.Parameters[1];
                        newHitbox.Bone = (int)cmd.Parameters[2];
                        newHitbox.Damage = (float)cmd.Parameters[3];
                        newHitbox.Angle = (int)cmd.Parameters[4];
                        newHitbox.KnockbackGrowth = (int)cmd.Parameters[5];
                        newHitbox.WeightBasedKnockback = (int)cmd.Parameters[6];
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
                        newHitbox.Part = (int)cmd.Parameters[1];
                        newHitbox.Extended = true;
                        newHitbox.Bone = (int)cmd.Parameters[2];
                        newHitbox.Damage = (float)cmd.Parameters[3];
                        newHitbox.Angle = (int)cmd.Parameters[4];
                        newHitbox.KnockbackGrowth = (int)cmd.Parameters[5];
                        newHitbox.WeightBasedKnockback = (int)cmd.Parameters[6];
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
                        newHitbox.Part = (int)cmd.Parameters[1];
                        newHitbox.Bone = (int)cmd.Parameters[2];
                        newHitbox.Damage = (float)cmd.Parameters[3];
                        newHitbox.Angle = (int)cmd.Parameters[4];
                        newHitbox.KnockbackGrowth = (int)cmd.Parameters[5];
                        newHitbox.WeightBasedKnockback = (int)cmd.Parameters[6];
                        newHitbox.KnockbackBase = (int)cmd.Parameters[7];
                        newHitbox.Size = (float)cmd.Parameters[8];
                        newHitbox.X = (float)cmd.Parameters[9];
                        newHitbox.Y = (float)cmd.Parameters[10];
                        newHitbox.Z = (float)cmd.Parameters[11];
                        newHitbox.FacingRestriction = (int)cmd.Parameters[34];
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
                        newHitbox.Part = (int)cmd.Parameters[1];
                        newHitbox.Bone = (int)cmd.Parameters[2];
                        newHitbox.Damage = (float)cmd.Parameters[3];
                        newHitbox.Angle = (int)cmd.Parameters[4];
                        newHitbox.KnockbackGrowth = (int)cmd.Parameters[5];
                        newHitbox.WeightBasedKnockback = (int)cmd.Parameters[6];
                        newHitbox.KnockbackBase = (int)cmd.Parameters[7];
                        newHitbox.Size = (float)cmd.Parameters[8];
                        newHitbox.X = (float)cmd.Parameters[9];
                        newHitbox.Y = (float)cmd.Parameters[10];
                        newHitbox.Z = (float)cmd.Parameters[11];
                        newHitbox.FacingRestriction = (int)cmd.Parameters[34];
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
                        newHitbox.Part = (int)cmd.Parameters[1];
                        newHitbox.Bone = (int)cmd.Parameters[2];
                        newHitbox.Damage = (float)cmd.Parameters[3];
                        newHitbox.Angle = (int)cmd.Parameters[4];
                        newHitbox.KnockbackGrowth = (int)cmd.Parameters[5];
                        newHitbox.WeightBasedKnockback = (int)cmd.Parameters[6];
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
                        newHitbox.Part = (int)cmd.Parameters[1];
                        newHitbox.Bone = (int)cmd.Parameters[2];
                        newHitbox.Damage = (float)cmd.Parameters[3];
                        newHitbox.Angle = (int)cmd.Parameters[4];
                        newHitbox.KnockbackGrowth = (int)cmd.Parameters[5];
                        newHitbox.WeightBasedKnockback = (int)cmd.Parameters[6];
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
                    Hitboxes.RemoveAt(Hitboxes.Keys.Max());
                    break;
                case 0x684264C9: // Change_Hitbox_Size
                    {
                        int id = (int)cmd.Parameters[0];
                        float size = (float)cmd.Parameters[1];
                        Hitbox hitboxToChange = null;
                        if (Hitboxes.TryGetValue(id, out hitboxToChange))
                            hitboxToChange.Size = size;
                        else
                            Console.WriteLine($"Could not find hitbox with ID={id} to change size");
                        break;
                    }
                case 0xDFA4517A: // Change_Hitbox_Damage
                    {
                        int id = (int)cmd.Parameters[0];
                        float damage = (float)cmd.Parameters[1];
                        Hitbox hitboxToChange = null;
                        if (Hitboxes.TryGetValue(id, out hitboxToChange))
                            hitboxToChange.Damage = damage;
                        else
                            Console.WriteLine($"Could not find hitbox with ID={id} to change damage");
                        break;
                    }
                case 0x1AFDA8E6: // Move_Hitbox
                    {
                        int id = (int)cmd.Parameters[0];
                        Hitbox hitboxToChange = null;
                        if (Hitboxes.TryGetValue(id, out hitboxToChange))
                        {
                            hitboxToChange.Bone = (int)cmd.Parameters[1];
                            hitboxToChange.X = (float)cmd.Parameters[2];
                            hitboxToChange.Y = (float)cmd.Parameters[3];
                            hitboxToChange.Z = (float)cmd.Parameters[4];
                        }
                        else
                            Console.WriteLine($"Could not find hitbox with ID={id} to move position");
                        break;
                    }
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
                case 0x661B30DA: // Bit_Variable_Set
                    {
                        uint flag = (uint)((int)cmd.Parameters[0]);
                        if (!ActiveFlags.Contains(flag))
                            ActiveFlags.Add(flag);
                        break;
                    }
                case 0x17232732: // Bit_Variable_Clear
                    {
                        uint flag = (uint)((int)cmd.Parameters[0]);
                        ActiveFlags.Remove(flag);
                        break;
                    }
                case 0xA21BC6EA: //If_Bit_Is_Set
                case 0x34BCD3F7: //If_Compare
                    {
                        if (IfVariableList.ContainsKey((uint)(int)cmd.Parameters[0]))
                            readTrue = IfVariableList[(uint)(int)cmd.Parameters[0]];
                        break;
                    }
                case 0x477705C2: //unk_477705C2 If_Compare2?
                    {
                        if (IfVariableValueList.ContainsKey((uint)(int)cmd.Parameters[0]))
                            readTrue = IfVariableValueList[(uint)(int)cmd.Parameters[0]] == (int)cmd.Parameters[2];
                        break;
                    }
                case 0xA5BD4F32: // TRUE
                    {
                        if (!readTrue)
                        {
                            TrueFalseSkipLength = (int)cmd.Parameters[0] - 4;
                            if (TrueFalseSkipLength == 0)
                                TrueFalseSkipLength = 1;
                        }
                        break;
                    }
                case 0x895B9275: // FALSE
                    {
                        if (readTrue)
                        {
                            TrueFalseSkipLength = (int)cmd.Parameters[0] - 2;
                            if (TrueFalseSkipLength == 0)
                                TrueFalseSkipLength = 1;
                        }
                        break;
                    }
                case 0x0F39EC70: // Allow/Disallow Ledgegrab
                    {
                        int type = (int)cmd.Parameters[0];
                        switch (type)
                        {
                            case 0:
                                LedgeGrabDisallowed = true;
                                FrontLedgeGrabAllowed = false;
                                ReverseLedgeGrabAllowed = false;
                                break;
                            case 1:
                                LedgeGrabDisallowed = false;
                                FrontLedgeGrabAllowed = true;
                                break;
                            case 2:
                                LedgeGrabDisallowed = false;
                                ReverseLedgeGrabAllowed = true;
                                break;
                        }
                        break;
                    }

            }
            return true;
        }

        public void getIfs()
        {
            scriptCommandIndex = 0;
            while (true)
            {
                // Subscripts take precedence in processing, most deep one first
                if (subscripts.Count > 0)
                {
                    if (scriptCommandIndex < subscripts[subscripts.Count - 1].Count)
                        processAndGetIfs(subscripts[subscripts.Count - 1][scriptCommandIndex]);
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
                        processAndGetIfs(script[scriptCommandIndex]);
                    else
                        // No script commands left to process, regardless of frame
                        break;
                }
                scriptCommandIndex++;
            }
            scriptCommandIndex = 0;
        }

        public void processAndGetIfs(ICommand cmd)
        {
            switch (cmd.Ident)
            {
                case 0x34BCD3F7: //If_Compare
                case 0xA21BC6EA: //If_Bit_Is_Set
                    {
                        if (!IfVariableList.ContainsKey((uint)(int)cmd.Parameters[0]))
                            IfVariableList.Add((uint)(int)cmd.Parameters[0], true);
                        break;
                    }
                case 0x477705C2: //unk_477705C2 If_Compare2?
                    {
                        if (!IfVariableValueList.ContainsKey((uint)(int)cmd.Parameters[0]))
                        {
                            IfVariableValueList.Add((uint)(int)cmd.Parameters[0], (int)cmd.Parameters[2]);
                            VariableValueList.Add((uint)(int)cmd.Parameters[0], new List<int>() { (int)cmd.Parameters[2] });
                        }
                        else
                        {
                            if (!VariableValueList[(uint)(int)cmd.Parameters[0]].Contains((int)cmd.Parameters[2]))
                                VariableValueList[(uint)(int)cmd.Parameters[0]].Add((int)cmd.Parameters[2]);
                        }
                        break;
                    }

                case 0x9126EBA2: // Subroutine: call another script
                case 0xFA1BC28A: // Subroutine1: call another script
                    // Try and load the other script. If we can't, then just keep going as per normal
                    uint crc = (uint)int.Parse(cmd.Parameters[0] + "");
                    if (Moveset == null) break;
                    if (Moveset.Game.Scripts.ContainsKey(crc) && crc != this.script.AnimationCRC)
                    {
                        subscripts.Add((ACMDScript)Moveset.Game.Scripts[crc]);

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
            }
        }

        private static int roundAnimationFrame(double animFrame)
        {
            // Modified version of Math.Round to try an approximate in-game behaviour
            return (int)Math.Floor(animFrame + 0.4);
        }

        #region Rendering

        public void Render(VBN Skeleton)
        {
            if (!Runtime.renderHitboxes || Skeleton == null)
                return;

            if (Hitboxes.Count <= 0)
                return;

            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            foreach (var pair in Hitboxes)
            {
                var h = pair.Value;

                if (Runtime.HiddenHitboxes.Contains(h.ID))
                    continue;

                Bone b = getBone(h.Bone, Skeleton);
                h.va = Vector3.TransformPosition(new Vector3(h.X, h.Y, h.Z), b.transform.ClearScale());

                // Draw angle marker
                if (Runtime.renderHitboxAngles)
                    RenderHitboxAngles(h, Skeleton);

                GL.Color4(h.GetDisplayColor());

                // Draw everything to the stencil buffer
                Rendering.RenderTools.beginTopLevelStencil();
                if (!h.IsSphere())
                {
                    h.va2 = new Vector3(h.X2, h.Y2, h.Z2);
                    if (h.Bone != -1) h.va2 = Vector3.TransformPosition(h.va2, b.transform.ClearScale());
                    Rendering.RenderTools.DrawCylinder(h.va, h.va2, h.Size);
                }
                else
                {
                    Rendering.RenderTools.drawSphere(h.va, h.Size, 30);
                }

                // n factorial (n!) algorithm (NOT EFFICIENT) to draw subsequent hitboxes around each other.
                // Will work fine for the low amounts of hitboxes in smash4.
                if (Runtime.renderHitboxesNoOverlap)
                {
                    // Remove the stencil for the already drawn hitboxes
                    Rendering.RenderTools.beginTopLevelAntiStencil();
                    foreach (var pair2 in Hitboxes.Reverse())
                    {
                        if (pair2.Key == pair.Key)
                            break;  // this only works because the list is sorted
                        var h2 = pair2.Value;

                        if (!Runtime.HiddenHitboxes.Contains(h2.ID))
                        {

                            Bone b2 = getBone(h2.Bone, Skeleton);
                            var va = Vector3.TransformPosition(new Vector3(h2.X, h2.Y, h2.Z), b2.transform.ClearScale());
                            if (!h2.IsSphere())
                            {
                                var va2 = new Vector3(h2.X2, h2.Y2, h2.Z2);
                                if (h2.Bone != -1) va2 = Vector3.TransformPosition(va2, b2.transform.ClearScale());
                                Rendering.RenderTools.DrawCylinder(va, va2, h2.Size);
                            }
                            else
                            {
                                Rendering.RenderTools.drawSphere(va, h2.Size, 30);
                            }
                        }
                    }
                }

                if (Runtime.SelectedHitboxID == h.ID)
                {
                    GL.Color4(Color.FromArgb(Runtime.hurtboxAlpha, Runtime.hurtboxColorSelected));
                    if (!h.IsSphere())
                    {
                        Rendering.RenderTools.DrawWireframeCylinder(h.va, h.va2, h.Size);
                    }
                    else
                    {
                        Rendering.RenderTools.drawWireframeSphere(h.va, h.Size, 10);
                    }
                }

                // End stenciling and draw over all the stenciled bits
                Rendering.RenderTools.endTopLevelStencilAndDraw();
            }
            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
        }
        
        public void RenderHitboxAngles(Hitbox h, VBN vbn)
        {
            //Angle marker changes direction depending on conditions:
            //If hitbox is behind transN bone (as if the person being hit is located at the hitbox center)
            //If facing-restriction parameter of hitboxes is set to certain values (3 = forward, 4 = backward)
            float transN_Z = getBone(0, vbn).pos.Z;
            int facingRestr = h.FacingRestriction;
            int direction = 1;

            //check facing restriction values first, then check position
            if (facingRestr == 3)
                direction = 1;
            else if (facingRestr == 4)
                direction = -1;
            else if (h.va.Z < transN_Z)
                direction = -1;

            GL.LineWidth(5f);
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(Runtime.hitboxAnglesColor);
            if (h.Angle <= 360)
            {
                //draws a straight line segment of the hitbox angle for angles <= 360
                GL.Vertex3(h.va);
                GL.Vertex3(h.va + Vector3.Transform(new Vector3(0, 0, h.Size), Matrix3.CreateRotationX((float)(90 * (1 - direction) - direction * h.Angle) * ((float)Math.PI / 180f))));
            }
            else if (h.Angle == 361)
            {
                //draws a "+" sign for 361 angles (Sakurai angles)
                GL.Vertex3(h.va - new Vector3(0, 0, h.Size / 2));
                GL.Vertex3(h.va + new Vector3(0, 0, h.Size / 2));
                GL.Vertex3(h.va - new Vector3(0, h.Size / 2, 0));
                GL.Vertex3(h.va + new Vector3(0, h.Size / 2, 0));
            }
            else
            {
                //draws a "x" sign for >361 angles (autolink angles)
                GL.Vertex3(h.va - Vector3.Transform(new Vector3(0, 0, h.Size / 2), Matrix3.CreateRotationX(45f * (float)Math.PI / 180f)));
                GL.Vertex3(h.va + Vector3.Transform(new Vector3(0, 0, h.Size / 2), Matrix3.CreateRotationX(45f * (float)Math.PI / 180f)));
                GL.Vertex3(h.va - Vector3.Transform(new Vector3(0, 0, h.Size / 2), Matrix3.CreateRotationX(-45f * (float)Math.PI / 180f)));
                GL.Vertex3(h.va + Vector3.Transform(new Vector3(0, 0, h.Size / 2), Matrix3.CreateRotationX(-45f * (float)Math.PI / 180f)));
            }
            GL.End();
        }

        public static Tuple<int, int> translateScriptBoneId(int boneId)
        {
            int jtbIndex = 0;
            while (boneId >= 1000)
            {
                boneId -= 1000;
                jtbIndex++;  // look in a different joint table
            }
            return Tuple.Create(boneId, jtbIndex);
        }

        public static Bone getBone(int bone, VBN VBN)
        {
            Tuple<int, int> boneInfo = translateScriptBoneId(bone);
            int bid = boneInfo.Item1;
            int jtbIndex = boneInfo.Item2;

            Bone b = new Bone(null);

            if (bone != -1)
            {
                // ModelContainers should store Hitbox data or have them linked since it will use last
                // modelcontainer bone for hitbox display (which might not be the character model).
                // This is especially important for the future when importing weapons for some moves.
                if (VBN != null)
                {
                    try //Try used to avoid bone not found issue that crashes the application
                    {
                        if (VBN.JointTable.Tables.Count < 1)
                            b = VBN.bones[bid];
                        else
                        {
                            if (jtbIndex == 0)
                            {
                                // Special rule for table 0, index 0 is *always* TransN, and index 1 counts as index 0
                                if (bid <= 0)
                                {
                                    b = VBN.bones.Find(item => item.Name == "TransN");
                                    if (b == null)
                                        b = VBN.bones[0];
                                }
                                else  // Index 2 counts as index 1, etc
                                    b = VBN.bones[VBN.JointTable.Tables[jtbIndex][bid - 1]];
                            }
                            else if (jtbIndex < VBN.JointTable.Tables.Count)
                            {
                                // Extra joint tables don't have the TransN rule
                                b = VBN.bones[VBN.JointTable.Tables[jtbIndex][bid]];
                            }
                            else
                            {
                                //If there is no jointTable but bone is >1000 then don't look into a another joint table
                                //This makes some weapons like Luma have hitboxes visualized
                                //b = m.vbn.bones[bid];
                                b = VBN.bones[VBN.JointTable.Tables[VBN.JointTable.Tables.Count - 1][bid]];
                            }
                        }
                    }
                    catch { }
                }
            }
            return b;
        }

        #endregion
    }
}
