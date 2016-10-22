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

        public static VBN TargetVBN { get; set; }
        public static NUD TargetNUD { get; set; }
        public static LVD TargetLVD { get; set; }
        public static PathBin TargetPath { get; set; }
        public static SkelAnimation TargetAnim { get { return _targetAnim; } set { _targetAnim = value; OnAnimationChanged(); } }
        private static SkelAnimation _targetAnim;

        public static string TargetAnimString { get; set; }

        public static Dictionary<string, SkelAnimation> Animations { get; set; }
        public static MovesetManager Moveset { get; set; }

        public static void StartupFromConfig(string config)
        {
            throw new NotImplementedException();
        }

        // Make sure subscribers unsubscribe or this
        // will prevent garbage collection!
        public static event EventHandler AnimationChanged;
        private static void OnAnimationChanged()
        {
            if (AnimationChanged != null)
                AnimationChanged(typeof(Runtime), EventArgs.Empty);
        }
    }
}
