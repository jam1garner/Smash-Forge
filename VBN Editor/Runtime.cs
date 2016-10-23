using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBN_Editor
{
    public class Runtime
    {
        static Runtime()
        {
            Animations = new Dictionary<string, SkelAnimation>();
        }

        public static List<ModelContainer> ModelContainers = new List<ModelContainer>();
        public static List<NUT> TextureContainers = new List<NUT>();

        public static VBN TargetVBN { get; set; }
        public static NUD TargetNUD { get; set; }
        public static LVD TargetLVD { get; set; }
        public static PathBin TargetPath { get; set; }
        public static CMR0 TargetCMR0 { get; set; }
        public static SkelAnimation TargetAnim { get { return _targetAnim; } set { _targetAnim = value; OnAnimationChanged(); } }
        private static SkelAnimation _targetAnim;

        public static bool renderLVD { get; set; }
        public static bool renderModel { get; set; }
        public static bool renderBones { get; set; }
        public static bool renderCollisions { get; set; }
        public static bool renderHitboxes { get; set; }
        public static bool renderFloor { get; set; }
        public static bool renderPath { get; set; }
        public static bool renderRespawns { get; set; }
        public static bool renderSpawns { get; set; }
        public static bool renderItemSpawners { get; set; }
        public static bool renderGeneralPoints { get; set; }

        public static string TargetAnimString { get; set; }

        public static Dictionary<string, SkelAnimation> Animations { get; set; }
        public static MovesetManager Moveset { get; set; }

        public static void StartupFromConfig(string config)
        {
            throw new NotImplementedException();
        }

        public static bool killWorkspace { get; set; }
        // Make sure subscribers unsubscribe or this
        // will prevent garbage collection!
        public static event EventHandler AnimationChanged;
        private static void OnAnimationChanged()
        {
            if (AnimationChanged != null && !killWorkspace)
                AnimationChanged(typeof(Runtime), EventArgs.Empty);
        }
    }
}
