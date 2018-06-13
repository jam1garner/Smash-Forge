using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml;
using OpenTK.Graphics.OpenGL;
using SALT.PARAMS;
using SFGraphics.GLObjects.Shaders;


namespace Smash_Forge
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

        public static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();

        public static List<NUT> TextureContainers = new List<NUT>();
        public static List<NUS3BANK> SoundContainers = new List<NUS3BANK>();

        public static Dictionary<string, Rendering.Lights.AreaLight> areaLights = new Dictionary<string, Rendering.Lights.AreaLight>();

        public static SortedList<string, FileBase> OpenedFiles { get; set; }

        public static VBN TargetVBN { get; set; }
        public static NUD TargetNUD { get; set; }
        public static LVD TargetLVD { get; set; }
        public static PathBin TargetPath { get; set; }
        public static LIGH.LighBin TargetLigh { get; set; }
        public static CMR0 TargetCMR0 { get; set; }
        public static List<MTA> TargetMTA = new List<MTA>();
        public static Object LVDSelection { get; set; }
        public static HitboxList hitboxList { get; set; }
        public static VariableList variableViewer { get; set; }

        public static Params.LightSetParam lightSetParam = new Params.LightSetParam();
        public static string lightSetDirectory = "";

        public static ParamFile stprmParam = null;

        public static int SelectedHitboxID { get; set; } = -1;
        public static int SelectedHurtboxID { get; set; } = -1;
        //Hitboxes can be removed halfway on an animation and set again multiple times, this list contains the IDs of the hitboxes that aren't visible
        public static List<int> HiddenHitboxes { get; set; } = new List<int>();

        public enum ViewportModes
        {
            NORMAL = 0,
            EDITVERT = 1
        }

        public static ViewportModes ViewportMode = ViewportModes.EDITVERT;

        public static float RenderLineSize = 2;
        public static bool renderLVD = true;

        public static bool renderModel = true;
        public static bool renderModelSelection = true;
        public static bool renderModelWireframe;
        public static float wireframeLineWidth = 0.01f;

        // The messages are annoying when batch rendering.
        public static bool checkNudTexIdOnOpen = true;

        public static bool renderBones = true;
        public static bool renderCollisions = true;
        public static bool renderCollisionNormals = false;
        public static bool renderHitboxes = true;
        public static bool renderInterpolatedHitboxes = true;
        public static bool renderHitboxesColorByKb;
        public static bool renderHitboxAngles = true;
        public static bool renderPath = true;
        public static bool renderRespawns = true;
        public static bool renderSpawns = true;
        public static bool renderItemSpawners = true;
        public static bool renderGeneralPoints = true;
        public static bool renderOtherLVDEntries = true;
        public static bool renderSwagZ = false;
        public static bool renderSwagY = false;
        public static bool renderBoundingBox;
        public static bool renderHurtboxes = true;
        public static bool renderHurtboxesZone = true;
        public static bool renderECB = false;
        public static bool renderIndicators = false;
        public static bool renderSpecialBubbles = true;
        public static bool renderLedgeGrabboxes = false;
        public static bool renderReverseLedgeGrabboxes = false;
        public static bool renderTetherLedgeGrabboxes = false;
        public static int hitboxRenderMode = Hitbox.RENDER_KNOCKBACK;
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
        public static bool useFrameDuration = true;
        public static bool useFAFasAnimationLength = false;

        public static Color counterBubbleColor = Color.FromArgb(0x89, 0x89, 0x89);
        public static Color reflectBubbleColor = Color.Cyan;
        public static Color shieldBubbleColor = Color.Red;
        public static Color absorbBubbleColor = Color.SteelBlue;
        public static Color wtSlowdownBubbleColor = Color.FromArgb(0x9a, 0x47, 0x9a);

        public static List<Color> hitboxKnockbackColors = new List<Color>();
        public static readonly List<Color> defaultHitboxKnockbackColors = new List<Color>()
        {
            ColorTools.ColorFromUint((uint)ColorTools.DistinctColors.VividGreen),
            ColorTools.ColorFromUint((uint)ColorTools.DistinctColors.VividYellow),
            ColorTools.ColorFromUint((uint)ColorTools.DistinctColors.VividOrange),
            ColorTools.ColorFromUint((uint)ColorTools.DistinctColors.VividRed)     
        };

        public static List<Color> hitboxIdColors = new List<Color>();
        public static readonly List<Color> defaultHitboxIdColors = new List<Color>()
        {
            ColorTools.ColorFromUint((uint)ColorTools.DistinctColors.VividYellow), 
            ColorTools.ColorFromUint((uint)ColorTools.DistinctColors.StrongPurple), 
            ColorTools.ColorFromUint((uint)ColorTools.DistinctColors.VividRed),
            ColorTools.ColorFromUint((uint)ColorTools.DistinctColors.GrayishYellow), 
            ColorTools.ColorFromUint((uint)ColorTools.DistinctColors.MediumGray), 
            ColorTools.ColorFromUint((uint)ColorTools.DistinctColors.StrongBlue), 
            ColorTools.ColorFromUint((uint)ColorTools.DistinctColors.DeepYellowishBrown)
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
        public static float zoomspeed = 1.25f;
        public static float zoomModifierScale = 2.0f;
        public static bool cameraLight = true;

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
        public static float spcIntentensity = 1.00f;
        public static float frsIntensity = 1.00f;
        public static float refIntensity = 1.00f;
        public static float ambItensity = 1.00f;

        // Misc Scale Stuff
        public static float modelScale = 1f;
        public static float zScale = 1.0f;

        // Bone Weight Display
        public static int selectedBoneIndex = -1;

        // Polygon ID Maps
        public static bool drawNudColorIdPass = false;

        public static bool drawUv = false;

        public static float specularHue = 360.0f;
        public static float specularSaturation = 0.0f;
        public static float specularIntensity = 0.65f;
        public static float specularRotX = 0.0f;
        public static float specularRotY = 0.0f;
        public static float specularRotZ = 0.0f;

        public static float reflectionHue = 360.0f;
        public static float reflectionSaturation = 0.0f;
        public static float reflectionIntensity = 1.0f;

        public static bool renderStageLight1 = true;
        public static bool renderStageLight2 = true;
        public static bool renderStageLight3 = true;
        public static bool renderStageLight4 = true;

        public static bool renderFog = true;

        public static float renderDepth = 100000.0f;
        public static bool renderVertColor = true;
        public static bool renderMaterialLighting = true;
        public static bool renderNormalMap = true;
        public static bool useDepthTest = true;
        public static bool drawAreaLightBoundingBoxes = true;
        public static bool renderStageLighting = true;

        // Debug Shading
        public static RenderTypes renderType;
        public static bool renderR = true;
        public static bool renderG = true;
        public static bool renderB = true;
        public static bool renderAlpha = true;
        public static UVChannel uvChannel = UVChannel.Channel1;
        public static bool useDebugShading = false;
        public static bool debug1 = false;
        public static bool debug2 = false;

        // ETC
        public static string fighterDir = "";
        public static string paramDir = "";

        // OpenGL System Information
        public static string renderer = "";
        public static string openGLVersion = "";
        public static string GLSLVersion = "";

        // Texture creation needs to be delayed until we actually have a context.
        public static bool glTexturesNeedRefreshing = false;

        public enum RenderTypes
        {
            Shaded = 0,
            Normals = 1,
            Lighting = 2,
            DiffuseMap = 3,
            NormalMap = 4,
            VertColor = 5,
            AmbientOcclusion = 6,
            UVCoords = 7,
            UVTestPattern = 8,
            Tangents = 9,
            Bitangents = 10,
            LightSet = 11,
            SelectedBoneWeights = 12
        }

        public enum UVChannel
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
        public static string TargetMTAString { get; set; }

        public static Dictionary<string, Animation> Animations { get; set; }
        public static Dictionary<string, MTA> MaterialAnimations { get; set; }
        public static MovesetManager Moveset { get; set; }
        public static CharacterParamManager ParamManager { get; set; }
        public static PARAMEditor ParamManagerHelper { get; set; }
        public static Dictionary<string, int> ParamMoveNameIdMapping { get; set; }
        public static ACMDPreviewEditor acmdEditor;
        public static ForgeACMDScript gameAcmdScript;
        public static Dictionary<uint, string> Animnames { get; set; }
        public static int scriptId = -1;


        public static string CanonicalizePath(string path)
        {
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public static void clearMoveset()
        {
            Moveset = null;
        }

        public static bool killWorkspace { get; set; }

        // Make sure subscribers unsubscribe or this will prevent garbage collection!
        public static event EventHandler AnimationChanged;
        private static void OnAnimationChanged()
        {
            if (AnimationChanged != null && !killWorkspace)
                AnimationChanged(typeof(Runtime), EventArgs.Empty);
        }
    }
}
