using OpenTK.Graphics.OpenGL;
using SALT.PARAMS;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace SmashForge
{
    public class Runtime
    {
        static Runtime()
        {
            Animations = new Dictionary<string, Animation>();
            OpenedFiles = new SortedList<string, FileBase>();
            MaterialAnimations = new Dictionary<string, MTA>();
            ParamManager = new CharacterParamManager();
            gameAcmdScript = null;
            Animnames = new Dictionary<uint, string>();
        }

        // See https://stackoverflow.com/questions/470690/how-to-automatically-generate-n-distinct-colors
        // for a really good overview of how to use distinct colors.
        public enum DistinctColors : uint
        {
            VividYellow = 0xFFFFB300,
            StrongPurple = 0xFF803E75,
            VividOrange = 0xFFFF6800,
            VeryLightBlue = 0xFFA6BDD7,
            VividRed = 0xFFC10020,
            GrayishYellow = 0xFFCEA262,
            MediumGray = 0xFF817066,

            // The following will not be good for people with defective color vision.
            VividGreen = 0xFF007D34,
            StrongPurplishPink = 0xFFF6768E,
            StrongBlue = 0xFF00538A,
            StrongYellowishPink = 0xFFFF7A5C,
            StrongViolet = 0xFF53377A,
            VividOrangeYellow = 0xFFFF8E00,
            StrongPurplishRed = 0xFFB32851,
            VividGreenishYellow = 0xFFF4C800,
            StrongReddishBrown = 0xFF7F180D,
            VividYellowishGreen = 0xFF93AA00,
            DeepYellowishBrown = 0xFF593315,
            VividReddishOrange = 0xFFF13A13,
            DarkOliveGreen = 0xFF232C16
        }

        public static string marioOdysseyGamePath = "";

        //public static List<ModelContainer> ModelContainers = new List<ModelContainer>();
        public static List<NUT> textureContainers = new List<NUT>();
        public static List<BNTX> bntxList = new List<BNTX>();
        public static List<FTEXContainer> ftexContainerList = new List<FTEXContainer>();

        public static List<NUS3BANK> soundContainers = new List<NUS3BANK>();

        public static SortedList<string, FileBase> OpenedFiles { get; set; }

        public static VBN TargetVbn { get; set; }
        public static LVD TargetLvd { get; set; }
        public static BYAML TargetByaml { get; set; }
        public static PathBin TargetPath { get; set; }
        public static LIGH.LighBin TargetLigh { get; set; }
        public static CMR0 TargetCmr0 { get; set; }
        public static List<MTA> targetMta = new List<MTA>();
        public static HitboxList HitboxList { get; set; }
        public static VariableList VariableViewer { get; set; }

        public static Params.LightSetParam lightSetParam = new Params.LightSetParam();
        public static string lightSetDirectory = "";

        public static ParamFile stprmParam = null;

        public static int SelectedHitboxId { get; set; } = -1;
        public static int SelectedHurtboxId { get; set; } = -1;
        //Hitboxes can be removed halfway on an animation and set again multiple times, this list contains the IDs of the hitboxes that aren't visible
        public static List<int> HiddenHitboxes { get; set; } = new List<int>();

        public enum ViewportModes
        {
            Normal = 0,
            Editvert = 1
        }

        // The messages are annoying when batch rendering.
        public static bool checkNudTexIdOnOpen = true;

        public static bool drawUv;

        // Model Viewport bone display.
        public static bool renderBones = true;
        public static float renderBoneNodeSize = 0.1f;

        public static bool renderLvd = true;

        public static bool enableVSync = false;

        public static bool renderModel = true;
        public static bool renderModelSelection = true;
        public static bool renderModelWireframe;
        public static float wireframeLineWidth = 0.01f;

        public static bool renderCollisions = true;
        public static bool renderCollisionNormals = false;
        public static bool renderHitboxes = true;
        public static bool renderInterpolatedHitboxes = true;
        public static bool renderHitboxAngles = true;
        public static bool renderPath = true;
        public static bool renderRespawns = true;
        public static bool renderSpawns = true;
        public static bool renderItemSpawners = true;
        public static bool renderGeneralPoints = true;
        public static bool renderOtherLvdEntries = true;
        public static bool renderSwagZ = false;
        public static bool renderSwagY = false;
        public static bool renderBoundingSphere;
        public static bool renderHurtboxes = true;
        public static bool renderHurtboxesZone = true;
        public static bool renderEcb = false;
        public static bool renderIndicators = false;
        public static bool renderSpecialBubbles = true;
        public static bool renderLedgeGrabboxes = false;
        public static bool renderReverseLedgeGrabboxes = false;
        public static bool renderTetherLedgeGrabboxes = false;
        public static int hitboxRenderMode = Hitbox.RenderKnockback;
        public static int hitboxAlpha = 130;
        public static int hurtboxAlpha = 80;
        public static Color hitboxAnglesColor = Color.White;
        public static Color hurtboxColor = Color.FromArgb(0x00, 0x53, 0x8A); //Strong blue;
        public static Color hurtboxColorHi = Color.FromArgb(0xFF, 0x8E, 0x00); //Vivid Orange Yellow;
        public static Color hurtboxColorMed = Color.FromArgb(0xF6, 0x76, 0x8E); //Strong Purplish Pink;
        public static Color hurtboxColorLow = Color.FromArgb(0x00, 0x53, 0x8A); //Strong blue;
        public static Color hurtboxColorSelected = Color.FromArgb(0xFF, 0xFF, 0xFF); //White;
        public static Color windboxColor = Color.Blue;
        public static Color grabboxColor = Color.Purple;
        public static Color searchboxColor = Color.DarkOrange;
        public static bool renderHitboxesNoOverlap;
        public static bool loadAndRenderAtkd = false;
        public static string currentAtkd = null;
        public static bool useFrameDuration = true;

        public static Color counterBubbleColor = Color.FromArgb(0x89, 0x89, 0x89);
        public static Color reflectBubbleColor = Color.Cyan;
        public static Color shieldBubbleColor = Color.Red;
        public static Color absorbBubbleColor = Color.SteelBlue;
        public static Color wtSlowdownBubbleColor = Color.FromArgb(0x9a, 0x47, 0x9a);

        public static List<Color> hitboxKnockbackColors = new List<Color>();
        public static readonly List<Color> defaultHitboxKnockbackColors = new List<Color>()
        {
            Color.FromArgb(unchecked((int)DistinctColors.VividGreen)),
            Color.FromArgb(unchecked((int)DistinctColors.VividYellow)),
            Color.FromArgb(unchecked((int)DistinctColors.VividOrange)),
            Color.FromArgb(unchecked((int)DistinctColors.VividRed))     
        };

        public static List<Color> hitboxIdColors = new List<Color>();
        public static readonly List<Color> defaultHitboxIdColors = new List<Color>()
        {
            Color.FromArgb(unchecked((int)DistinctColors.VividYellow)), 
            Color.FromArgb(unchecked((int)DistinctColors.StrongPurple)), 
            Color.FromArgb(unchecked((int)DistinctColors.VividRed)),
            Color.FromArgb(unchecked((int)DistinctColors.GrayishYellow)), 
            Color.FromArgb(unchecked((int)DistinctColors.MediumGray)), 
            Color.FromArgb(unchecked((int)DistinctColors.StrongBlue)), 
            Color.FromArgb(unchecked((int)DistinctColors.DeepYellowishBrown))
        };

        // Floor Grid
        public static bool renderFloor = true;
        public static string floorTexFilePath = "";
        public static TextureWrapMode floorWrap = TextureWrapMode.MirroredRepeat;
        public static float floorSize = 30f;
        public static Color floorColor = Color.Gray;
        public static FloorStyle floorStyle = FloorStyle.WireFrame;
        public static bool renderFloorLines = true;

        // Viewport Background
        public static bool renderBackGround = true;
        public static string backgroundTexFilePath = "";
        public static BackgroundStyle backgroundStyle = BackgroundStyle.Gradient;
        public static Color backgroundGradientTop = Color.FromArgb(255, 26, 26, 26);
        public static Color backgroundGradientBottom = Color.FromArgb(255, 77, 77, 77);
        public static float fov = 0.524f; // default 30 degrees from stage param files
        public static float zoomSpeed = 1.25f;
        public static float zoomModifierScale = 2.0f;
        public static bool cameraLight = false;

        // Post Processing
        public static bool renderBloom = false;
        public static bool usePostProcessing = false;
        public static bool drawModelShadow = false;
        public static float bloomIntensity = 0.25f;
        public static float bloomThreshold = 1.01f;
        public static float bloomTexScale = 0.25f;

        // Toggle Render Passes
        public static bool renderDiffuse = true;
        public static bool renderFresnel = true;
        public static bool renderSpecular = true;
        public static bool renderReflection = true;

        // Render Passes Intensities
        public static float difIntensity = 1.00f;
        public static float spcIntensity = 1.00f;
        public static float frsIntensity = 1.00f;
        public static float refIntensity = 1.00f;
        public static float ambIntensity = 1.00f;

        // Misc Scale Stuff
        public static float modelScale = 1f;
        public static float zScale = 1.0f;

        // Bone Weight Display
        public static int selectedBoneIndex = -1;

        // Polygon ID Maps
        public static bool drawNudColorIdPass = false;

        public static float reflectionHue = 360.0f;
        public static float reflectionSaturation = 0.0f;
        public static float reflectionIntensity = 1.0f;

        public static bool renderFog = true;

        public static float renderDepth = 100000.0f;
        public static bool renderVertColor = true;
        public static bool renderMaterialLighting = true;
        public static bool renderNormalMap = true;
        public static bool useDepthTest = true;
        public static bool drawAreaLightBoundingBoxes = true;
        public static bool renderStageLighting = true;

        public static bool renderBfresPbr = false;

        // Debug Shading
        public static RenderTypes renderType;
        public static bool renderR = true;
        public static bool renderG = true;
        public static bool renderB = true;
        public static bool renderAlpha = true;
        public static UvChannel uvChannel = UvChannel.Channel1;
        public static bool debug1 = false;
        public static bool debug2 = false;

        public static bool enableOpenTkDebugOutput = false;

        // ETC
        public static string fighterDir = "";
        public static string paramDir = "";

        // OpenGL System Information
        public static string renderer = "";
        public static string openGlVersion = "";
        public static string glslVersion = "";

        // Texture creation needs to be delayed until we actually have a context.
        public static bool glTexturesNeedRefreshing = false;

        // This should only be done once for performance reasons.
        public static bool hasRefreshedMatThumbnails = false;

        public enum RenderTypes
        {
            Shaded = 0,
            Normals = 1,
            Lighting = 2,
            DiffuseMap = 3,
            NormalMap = 4,
            VertColor = 5,
            AmbientOcclusion = 6,
            UvCoords = 7,
            UvTestPattern = 8,
            Tangents = 9,
            Bitangents = 10,
            LightSet = 11,
            SelectedBoneWeights = 12
        }

        public enum UvChannel
        {
            Channel1 = 1,
            Channel2 = 2,
            Channel3 = 3
        }

        public enum FloorStyle
        {
            WireFrame = 0,
            UserTexture = 1,
            Solid = 2,
        }

        public enum BackgroundStyle
        {
            Gradient = 0,
            UserTexture = 1,
            Solid = 2,
        }

        public static string TargetAnimString { get; set; }
        public static string TargetMtaString { get; set; }

        public static Dictionary<string, Animation> Animations { get; set; }
        public static Dictionary<string, MTA> MaterialAnimations { get; set; }
        public static MovesetManager Moveset { get; set; }
        public static CharacterParamManager ParamManager { get; set; }
        public static ParamEditor ParamManagerHelper { get; set; }
        public static Dictionary<string, int> ParamMoveNameIdMapping { get; set; }
        public static AcmdPreviewEditor acmdEditor;
        public static ForgeAcmdScript gameAcmdScript;
        public static Dictionary<uint, string> Animnames { get; set; }
        public static int scriptId = -1;


        public static string CanonicalizePath(string path)
        {
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public static void ClearMoveset()
        {
            Moveset = null;
        }
    }
}
