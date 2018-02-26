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

        //public static List<ModelContainer> ModelContainers = new List<ModelContainer>();
        public static List<NUT> TextureContainers = new List<NUT>();
        public static List<NUS3BANK> SoundContainers = new List<NUS3BANK>();

        public static Dictionary<string, Rendering.Lights.AreaLight> areaLights = new Dictionary<string, Rendering.Lights.AreaLight>();

        public static SortedList<string, FileBase> OpenedFiles { get; set; }

        public static VBN TargetVBN { get; set; }
        public static NUD TargetNUD { get; set; }
        public static LVD TargetLVD { get; set; }
        public static PathBin TargetPath { get; set; }
        public static LighBin TargetLigh { get; set; }
        public static CMR0 TargetCMR0 { get; set; }
        public static List<MTA> TargetMTA = new List<MTA>();
        public static Object LVDSelection { get; set; }
        //public static Animation TargetAnim { get { return _targetAnim; } set { _targetAnim = value; OnAnimationChanged(); } }
        //private static Animation _targetAnim;
        public static HitboxList hitboxList { get; set; }
        public static VariableList variableViewer { get; set; }

        public static ParamFile lightSetParam = null;
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
        public static bool renderLVD;
        public static bool renderModel;
        public static bool renderModelSelection = true;
        public static bool renderModelWireframe;
        public static bool renderBones;
        public static bool renderCollisions;
        public static bool renderCollisionNormals;
        public static bool renderHitboxes;
        public static bool renderInterpolatedHitboxes;
        public static bool renderHitboxesColorByKb;
        public static bool renderHitboxAngles;
        public static bool renderFloor;
        public static bool renderBackGround;
        public static bool renderPath;
        public static bool renderRespawns;
        public static bool renderSpawns;
        public static bool renderItemSpawners;
        public static bool renderGeneralPoints;
        public static bool renderOtherLVDEntries;
        public static bool renderSwagZ;
        public static bool renderSwagY;
        public static bool renderBoundingBox;
        public static bool renderHurtboxes;
        public static bool renderHurtboxesZone;
        public static bool renderECB;
        public static bool renderIndicators;
        public static bool renderSpecialBubbles;
        public static bool renderLedgeGrabboxes;
        public static bool renderReverseLedgeGrabboxes;
        public static bool renderTetherLedgeGrabboxes;
        public static int hitboxRenderMode;
        public static int hitboxAlpha;
        public static int hurtboxAlpha;
        public static Color hitboxAnglesColor;
        public static Color hurtboxColor;
        public static Color hurtboxColorHi;
        public static Color hurtboxColorMed;
        public static Color hurtboxColorLow;
        public static Color hurtboxColorSelected;
        public static Color windboxColor;
        public static Color grabboxColor;
        public static Color searchboxColor;
        public static bool renderHitboxesNoOverlap;
        public static bool useFrameDuration = true;
        public static bool useFAFasAnimationLength = false;

        public static Color counterBubbleColor;
        public static Color reflectBubbleColor;
        public static Color shieldBubbleColor;
        public static Color absorbBubbleColor;
        public static Color wtSlowdownBubbleColor;

        // See https://stackoverflow.com/questions/470690/how-to-automatically-generate-n-distinct-colors
        // for a really good overview of how to use distinct colors.
        //UIntToColor(0xFFFFB300), //Vivid Yellow
        //UIntToColor(0xFF803E75), //Strong Purple
        //UIntToColor(0xFFFF6800), //Vivid Orange
        //UIntToColor(0xFFA6BDD7), //Very Light Blue
        //UIntToColor(0xFFC10020), //Vivid Red
        //UIntToColor(0xFFCEA262), //Grayish Yellow
        //UIntToColor(0xFF817066), //Medium Gray

        ////The following will not be good for people with defective color vision
        //UIntToColor(0xFF007D34), //Vivid Green
        //UIntToColor(0xFFF6768E), //Strong Purplish Pink
        //UIntToColor(0xFF00538A), //Strong Blue
        //UIntToColor(0xFFFF7A5C), //Strong Yellowish Pink
        //UIntToColor(0xFF53377A), //Strong Violet
        //UIntToColor(0xFFFF8E00), //Vivid Orange Yellow
        //UIntToColor(0xFFB32851), //Strong Purplish Red
        //UIntToColor(0xFFF4C800), //Vivid Greenish Yellow
        //UIntToColor(0xFF7F180D), //Strong Reddish Brown
        //UIntToColor(0xFF93AA00), //Vivid Yellowish Green
        //UIntToColor(0xFF593315), //Deep Yellowish Brown
        //UIntToColor(0xFFF13A13), //Vivid Reddish Orange
        //UIntToColor(0xFF232C16), //Dark Olive Green
        public static List<Color> hitboxKnockbackColors;
        public static readonly List<Color> defaultHitboxKnockbackColors = new List<Color>()
        {
            Color.FromArgb(0xFF, 0x00, 0x7D, 0x34), // Vivid green
            Color.FromArgb(0xFF, 0xFF, 0xB3, 0x0),    // Vivid yellow
            Color.FromArgb(0xFF, 0xFF, 0x68, 0x00),   // Vivid orange
            Color.FromArgb(0xFF, 0xC1, 0x0, 0x20),    // Vivid red
        };
        public static List<Color> hitboxIdColors;
        public static readonly List<Color> defaultHitboxIdColors = new List<Color>()
        {
            Color.FromArgb(0xFF, 0xFF, 0xB3, 0x00), // Vivid yellow
            Color.FromArgb(0xFF, 0x80, 0x3E, 0x75), // Strong purple
            Color.FromArgb(0xFF, 0xC1, 0x00, 0x20), // Vivid red
            Color.FromArgb(0xFF, 0xCE, 0xA2, 0x62), // Grayish yellow
            Color.FromArgb(0xFF, 0x81, 0x70, 0x66), // Medium gray
            Color.FromArgb(0xFF, 0x00, 0x53, 0x8A), // Strong blue
            Color.FromArgb(0xFF, 0x59, 0x33, 0x15), // Deep yellowish brown
        };

        public static string FloorURL = "";
        public static TextureWrapMode floorWrap = TextureWrapMode.MirroredRepeat;
        public static float floorSize = 30f;
        public static Color floorColor = Color.Gray;
        public static FloorStyle floorStyle = FloorStyle.Normal;
        public static bool renderFloorLines = true;

        public static Color back1 = Color.FromArgb((255 << 24) | (26 << 16) | (26 << 8) | (26));
        public static Color back2 = Color.FromArgb((255 << 24) | (77 << 16) | (77 << 8) | (77));
        public static float fov = 0.524f; // default angle in radians from stage param files
        public static float zoomspeed = 1.0f;
        public static float zoomModifierScale = 2.0f;
        public static bool cameraLight = false;

        public static bool drawQuadBlur = false;
        public static bool drawQuadFinalOutput = false;
        public static bool drawModelShadow = false;

        public static bool renderDiffuse = true;
        public static bool renderFresnel = true;
        public static bool renderSpecular = true;
        public static bool renderReflection = true;

        public static float difIntensity = 1.00f;
        public static float spcIntentensity = 1.00f;
        public static float frsIntensity = 1.00f;
        public static float refIntensity = 1.00f;
        public static float ambItensity = 1.00f;
        public static float model_scale = 1f;
        public static float zScale = 1.0f;

        public static int selectedBoneIndex = -1;

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
        public static string paramDir;

        // OpenGL System Information
        public static string renderer = "";
        public static string openGLVersion = "";
        public static string GLSLVersion = "";
        public static bool useLegacyShaders = false;

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
            Normal = 0,
            Textured = 1,
            UserTexture = 2,
            Solid = 3,
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
            //acmdEditor.updateCrcList();
        }

        public static void StartupFromConfig(string config)
        {
            int temp;
            if (!File.Exists(config)) SaveConfig();
            else
            {
                // Read Config

                XmlDocument doc = new XmlDocument();
                doc.Load(config);
                Queue<XmlNode> que = new Queue<XmlNode>();

                foreach (XmlNode node in doc.ChildNodes)
                    que.Enqueue(node);

                while (que.Count > 0)
                {
                    XmlNode node = que.Dequeue();

                    foreach (XmlNode n in node.ChildNodes)
                        que.Enqueue(n);

                    switch (node.Name)
                    {
                        case "texture":
                            if (node.ParentNode != null)
                            {
                                switch (node.ParentNode.Name)
                                {
                                    case "floor":
                                        if (File.Exists(node.InnerText) && node.InnerText.ToLower().EndsWith(".png"))
                                        {
                                            FloorURL = node.InnerText;
                                            Rendering.RenderTools.floorTexture = NUT.loadImage(new Bitmap(node.InnerText));
                                            floorStyle = FloorStyle.UserTexture;
                                        }
                                        break;
                                }
                            }
                            break;
                        case "texture_wrap":
                            if (node.ParentNode != null)
                            {
                                switch (node.ParentNode.Name)
                                {
                                    case "floor":Enum.TryParse(node.InnerText, out floorWrap); break;
                                }
                            }
                            break;
                        case "default_texture":
                            if (File.Exists(node.InnerText) && node.InnerText.ToLower().EndsWith(".png"))
                            {
                                Rendering.RenderTools.defaultTex = NUT.loadImage(new Bitmap(node.InnerText));
                            }
                            break;
                        case "size":
                            if (node.ParentNode != null)
                            {
                                switch (node.ParentNode.Name)
                                {
                                    case "floor": float.TryParse(node.InnerText, out floorSize); break;
                                }
                            }
                            break;
                        case "guide_lines": bool.TryParse(node.InnerText, out renderFloorLines); break;
                        case "zoom_speed": float.TryParse(node.InnerText, out zoomspeed); break;
                        case "zoom_modifier_multiplier": float.TryParse(node.InnerText, out zoomModifierScale); break;
                        case "render_depth": float.TryParse(node.InnerText, out renderDepth); break;
                        case "fov": float.TryParse(node.InnerText, out fov); break;
                        case "back_gradient_top": try { back1 = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                        case "back_gradient_bottom": try { back2 = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;

                        case "type": if (node.ParentNode != null && node.ParentNode.Name.Equals("RENDERSETTINGS")) Enum.TryParse(node.InnerText, out renderType); break;
                        case "OpenGL_2.10": bool.TryParse(node.InnerText, out useLegacyShaders); break;
                        case "camera_light": bool.TryParse(node.InnerText, out cameraLight); break;
                        case "use_normal_map": bool.TryParse(node.InnerText, out renderNormalMap); break;
                        case "render_vertex_color": bool.TryParse(node.InnerText, out renderVertColor); break;
                        case "render_alpha": bool.TryParse(node.InnerText, out renderAlpha); break;
                        case "render_diffuse": bool.TryParse(node.InnerText, out renderDiffuse); break;
                        case "render_specular": bool.TryParse(node.InnerText, out renderSpecular); break;
                        case "render_fresnel": bool.TryParse(node.InnerText, out renderFresnel); break;
                        case "render_reflection": bool.TryParse(node.InnerText, out renderReflection); break;

                        case "render_selection": bool.TryParse(node.InnerText, out renderModelSelection); break;
                        case "render_wireframe": bool.TryParse(node.InnerText, out renderModelWireframe); break;
                        case "render_bones": bool.TryParse(node.InnerText, out renderBones); break;
                        case "render_collisions": bool.TryParse(node.InnerText, out renderCollisions); break;
                        case "render_collision_normals": bool.TryParse(node.InnerText, out renderCollisionNormals); break;
                        case "render_hitboxes": bool.TryParse(node.InnerText, out renderHitboxes); break;
                        case "render_interpolated_hitboxes": bool.TryParse(node.InnerText, out renderInterpolatedHitboxes); break;
                        case "render_hitboxes_no_overlap": bool.TryParse(node.InnerText, out renderHitboxesNoOverlap); break;
                        case "render_hitboxes_mode": int.TryParse(node.InnerText, out hitboxRenderMode); break;
                        case "render_hitbox_angles": bool.TryParse(node.InnerText, out renderHitboxAngles); break;
                        case "render_hurtboxes": bool.TryParse(node.InnerText, out renderHurtboxes); break;
                        case "render_hurtboxes_zone": bool.TryParse(node.InnerText, out renderHurtboxesZone); break;
                        case "render_ECB": bool.TryParse(node.InnerText, out renderECB); break;
                        case "render_special_bubbles": bool.TryParse(node.InnerText, out renderSpecialBubbles); break;
                        case "render_ledge_grabboxes": bool.TryParse(node.InnerText, out renderLedgeGrabboxes); break;
                        case "render_reverse_ledge_grabboxes": bool.TryParse(node.InnerText, out renderReverseLedgeGrabboxes); break;
                        case "render_tether_ledge_grabboxes": bool.TryParse(node.InnerText, out renderTetherLedgeGrabboxes); break;
                        case "render_bounding_boxes": bool.TryParse(node.InnerText, out renderBoundingBox); break;
                        case "render_path": bool.TryParse(node.InnerText, out renderPath); break;
                        case "render_respawns": bool.TryParse(node.InnerText, out renderRespawns); break;
                        case "render_spawns": bool.TryParse(node.InnerText, out renderSpawns); break;
                        case "render_item_spawners": bool.TryParse(node.InnerText, out renderItemSpawners); break;
                        case "render_general_points": bool.TryParse(node.InnerText, out renderGeneralPoints); break;
                        case "render_otherLVDEntries": bool.TryParse(node.InnerText, out renderOtherLVDEntries); break;
                        case "render_swag": bool.TryParse(node.InnerText, out renderSwagY); break;
                        case "render_swagZ": bool.TryParse(node.InnerText, out renderSwagZ); break;
                        case "fighter_dir": fighterDir = node.InnerText; break;
                        case "param_dir": paramDir = node.InnerText; break;
                        case "render_indicators": bool.TryParse(node.InnerText, out renderIndicators); break;
                        case "hitbox_alpha": int.TryParse(node.InnerText, out hitboxAlpha); break;
                        case "hurtbox_alpha": int.TryParse(node.InnerText, out hurtboxAlpha); break;
                        case "hitbox_angles_color": try { Runtime.hitboxAnglesColor = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                        case "hurtbox_color": try { Runtime.hurtboxColor = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                        case "hurtbox_color_hi": try { Runtime.hurtboxColorHi = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                        case "hurtbox_color_med": try { Runtime.hurtboxColorMed = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                        case "hurtbox_color_low": try { Runtime.hurtboxColorLow = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                        case "hurtbox_color_selected": try { Runtime.hurtboxColorSelected = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                        case "windbox_color": try { Runtime.windboxColor = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                        case "grabbox_color": try { Runtime.grabboxColor = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                        case "searchbox_color": try { Runtime.searchboxColor = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                        case "counterBubble_color": try { Runtime.counterBubbleColor = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                        case "reflectBubble_color": try { Runtime.reflectBubbleColor = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                        case "shieldBubble_color": try { Runtime.shieldBubbleColor = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                        case "absorbBubble_color": try { Runtime.absorbBubbleColor = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                        case "wtSlowdownBubble_color": try { Runtime.wtSlowdownBubbleColor = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;

                        //Discord Stuff
                        case "image_key_mode": int.TryParse(node.InnerText, out temp); DiscordSettings.imageKeyMode = (DiscordSettings.ImageKeyMode)temp; break;
                        case "user_image_key": DiscordSettings.userPickedImageKey = node.InnerText; break;
                        case "user_mod_name": DiscordSettings.userNamedMod = node.InnerText; break;
                        case "use_user_mod_name": bool.TryParse(node.InnerText, out DiscordSettings.useUserModName); break;
                        case "show_current_window": bool.TryParse(node.InnerText, out DiscordSettings.showCurrentWindow); break;
                        case "show_time_elapsed": bool.TryParse(node.InnerText, out DiscordSettings.showTimeElapsed); break;
                        
                        case "enabled":
                            if (node.ParentNode != null)
                            {
                                switch (node.ParentNode.Name)
                                {
                                    case "diffuse": bool.TryParse(node.InnerText, out renderDiffuse); break;
                                    case "specular": bool.TryParse(node.InnerText, out renderSpecular); break;
                                    case "fresnel": bool.TryParse(node.InnerText, out renderFresnel); break;
                                    case "reflection": bool.TryParse(node.InnerText, out renderReflection); break;
                                    case "floor": bool.TryParse(node.InnerText, out renderFloor); break;
                                    case "lighting": bool.TryParse(node.InnerText, out renderMaterialLighting); break;
                                    case "render_model": bool.TryParse(node.InnerText, out renderModel); break;
                                    case "render_LVD": bool.TryParse(node.InnerText, out renderLVD); break;
                                }
                            }
                            break;
                        case "color":
                            if (node.ParentNode != null)
                            {
                                switch (node.ParentNode.Name)
                                {
                                    case "floor": try { floorColor = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                                    case "hitbox_kb_colors": try { hitboxKnockbackColors.Add(ColorTranslator.FromHtml(node.InnerText)); } catch (Exception) { } break;
                                    case "hitbox_id_colors": try { hitboxIdColors.Add(ColorTranslator.FromHtml(node.InnerText)); } catch (Exception) { } break;
                                }
                            }
                            break;
                        case "style":
                            if (node.ParentNode != null)
                            {
                                switch (node.ParentNode.Name)
                                {
                                    case "floor": Enum.TryParse(node.InnerText, out floorStyle); break;
                                }
                            }
                            break;
                        default:
                            Console.WriteLine(node.Name);
                            break;
                    }
                }
                EnsureHitboxColors();
            }
        }

        public static void EnsureHitboxColors()
        {
            if (Runtime.hitboxKnockbackColors.Count <= 0)
                Runtime.hitboxKnockbackColors = new List<Color>(Runtime.defaultHitboxKnockbackColors);
            if (Runtime.hitboxIdColors.Count <= 0)
                Runtime.hitboxIdColors = new List<Color>(Runtime.defaultHitboxIdColors);
        }

        public static void SaveConfig()
        {
            EnsureHitboxColors();

            XmlDocument doc = new XmlDocument();

            string comment = @"
                Config ENUMS

                floor style
                -Normal
                -Solid
                -Textured

                for setting floor texture 
                <texture>(texture location)</texture>

                render type
                -Texture
                -Normals
                -NormalsBnW
                -VertColor

                for changing default texure
                <default_texture>(texture location)</default_texture>";

            XmlComment com = doc.CreateComment(comment);

            XmlNode mainNode = doc.CreateElement("FORGECONFIG");
            doc.AppendChild(mainNode);
            mainNode.AppendChild(com);

            XmlNode viewportNode = doc.CreateElement("VIEWPORT");
            mainNode.AppendChild(viewportNode);
            {
                XmlNode node = doc.CreateElement("floor");
                viewportNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", renderFloor.ToString()));
                node.AppendChild(createNode(doc, "style", floorStyle.ToString()));
                node.AppendChild(createNode(doc, "color", ColorTranslator.ToHtml(floorColor)));
                node.AppendChild(createNode(doc, "size", floorSize.ToString()));
                if(floorStyle == FloorStyle.UserTexture)
                    node.AppendChild(createNode(doc, "texture", FloorURL));
            }
            
            viewportNode.AppendChild(createNode(doc, "zoom_speed", zoomspeed.ToString()));
            viewportNode.AppendChild(createNode(doc, "zoom_modifier_multiplier", zoomModifierScale.ToString()));
            viewportNode.AppendChild(createNode(doc, "fov", fov.ToString()));
            viewportNode.AppendChild(createNode(doc, "render_depth", renderDepth.ToString()));
            viewportNode.AppendChild(createNode(doc, "render_background", renderBackGround.ToString()));
            viewportNode.AppendChild(createNode(doc, "back_gradient_top", ColorTranslator.ToHtml(back1)));
            viewportNode.AppendChild(createNode(doc, "back_gradient_bottom", ColorTranslator.ToHtml(back2)));

            XmlNode renderNode = doc.CreateElement("RENDERSETTINGS");
            mainNode.AppendChild(renderNode);
            renderNode.AppendChild(createNode(doc, "type",renderType.ToString()));
            renderNode.AppendChild(createNode(doc, "render_vertex_color", renderVertColor.ToString()));
            renderNode.AppendChild(createNode(doc, "render_alpha", renderAlpha.ToString()));
            renderNode.AppendChild(createNode(doc, "camera_light", cameraLight.ToString()));
            renderNode.AppendChild(createNode(doc, "OpenGL_2.10", useLegacyShaders.ToString()));
            renderNode.AppendChild(createNode(doc, "use_normal_map", renderNormalMap.ToString()));

            {
                XmlNode node = doc.CreateElement("lighting");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", renderMaterialLighting.ToString()));
                node.AppendChild(createNode(doc, "render_diffuse", renderDiffuse.ToString()));
                node.AppendChild(createNode(doc, "render_specular", renderSpecular.ToString()));
                node.AppendChild(createNode(doc, "render_fresnel", renderFresnel.ToString()));
                node.AppendChild(createNode(doc, "render_reflection", renderReflection.ToString()));
            }
            {
                XmlNode node = doc.CreateElement("diffuse");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", renderDiffuse.ToString()));
                node.AppendChild(createNode(doc, "intensity", difIntensity.ToString()));
            }
            {
                XmlNode node = doc.CreateElement("specular");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", renderSpecular.ToString()));
                node.AppendChild(createNode(doc, "intensity", spcIntentensity.ToString()));
            }
            {
                XmlNode node = doc.CreateElement("fresnel");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", renderFresnel.ToString()));
                node.AppendChild(createNode(doc, "intensity", frsIntensity.ToString()));
            }
            {
                XmlNode node = doc.CreateElement("reflection");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", renderReflection.ToString()));
                node.AppendChild(createNode(doc, "intensity", refIntensity.ToString()));
            }
            {
                XmlNode node = doc.CreateElement("ambient");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "intensity", ambItensity.ToString()));
            }

            {
                XmlNode node = doc.CreateElement("render_model");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", renderModel.ToString()));
                node.AppendChild(createNode(doc, "render_selection", renderModelSelection.ToString()));
                node.AppendChild(createNode(doc, "render_wireframe", renderModelWireframe.ToString()));
                node.AppendChild(createNode(doc, "render_bones", renderBones.ToString()));
                node.AppendChild(createNode(doc, "render_bounding_boxes", renderBoundingBox.ToString()));
            }

            renderNode.AppendChild(createNode(doc, "render_ECB", renderECB.ToString()));
            renderNode.AppendChild(createNode(doc, "render_hurtboxes", renderHurtboxes.ToString()));
            renderNode.AppendChild(createNode(doc, "render_hurtboxes_zone", renderHurtboxesZone.ToString()));
            renderNode.AppendChild(createNode(doc, "render_hitboxes", renderHitboxes.ToString()));
            renderNode.AppendChild(createNode(doc, "render_interpolated_hitboxes", renderInterpolatedHitboxes.ToString()));
            renderNode.AppendChild(createNode(doc, "render_hitboxes_no_overlap", renderHitboxesNoOverlap.ToString()));
            renderNode.AppendChild(createNode(doc, "render_hitboxes_mode", hitboxRenderMode.ToString()));
            renderNode.AppendChild(createNode(doc, "render_hitbox_angles", renderHitboxAngles.ToString()));
            renderNode.AppendChild(createNode(doc, "render_special_bubbles", renderSpecialBubbles.ToString()));
            renderNode.AppendChild(createNode(doc, "render_ledge_grabboxes", renderLedgeGrabboxes.ToString()));
            renderNode.AppendChild(createNode(doc, "render_reverse_ledge_grabboxes", renderReverseLedgeGrabboxes.ToString()));
            renderNode.AppendChild(createNode(doc, "render_tether_ledge_grabboxes", renderTetherLedgeGrabboxes.ToString()));
            renderNode.AppendChild(createNode(doc, "hitbox_alpha", hitboxAlpha.ToString()));
            renderNode.AppendChild(createNode(doc, "hurtbox_alpha", hurtboxAlpha.ToString()));
            renderNode.AppendChild(createNode(doc, "hurtbox_color", System.Drawing.ColorTranslator.ToHtml(hurtboxColor)));
            renderNode.AppendChild(createNode(doc, "hurtbox_color_hi", System.Drawing.ColorTranslator.ToHtml(hurtboxColorHi)));
            renderNode.AppendChild(createNode(doc, "hurtbox_color_med", System.Drawing.ColorTranslator.ToHtml(hurtboxColorMed)));
            renderNode.AppendChild(createNode(doc, "hurtbox_color_low", System.Drawing.ColorTranslator.ToHtml(hurtboxColorLow)));
            renderNode.AppendChild(createNode(doc, "hurtbox_color_selected", System.Drawing.ColorTranslator.ToHtml(hurtboxColorSelected)));
            renderNode.AppendChild(createNode(doc, "hitbox_angles_color", System.Drawing.ColorTranslator.ToHtml(hitboxAnglesColor)));
            renderNode.AppendChild(createNode(doc, "windbox_color", System.Drawing.ColorTranslator.ToHtml(windboxColor)));
            renderNode.AppendChild(createNode(doc, "grabbox_color", System.Drawing.ColorTranslator.ToHtml(grabboxColor)));
            renderNode.AppendChild(createNode(doc, "searchbox_color", System.Drawing.ColorTranslator.ToHtml(searchboxColor)));
            renderNode.AppendChild(createNode(doc, "counterBubble_color", System.Drawing.ColorTranslator.ToHtml(counterBubbleColor)));
            renderNode.AppendChild(createNode(doc, "reflectBubble_color", System.Drawing.ColorTranslator.ToHtml(reflectBubbleColor)));
            renderNode.AppendChild(createNode(doc, "shieldBubble_color", System.Drawing.ColorTranslator.ToHtml(shieldBubbleColor)));
            renderNode.AppendChild(createNode(doc, "absorbBubble_color", System.Drawing.ColorTranslator.ToHtml(absorbBubbleColor)));
            renderNode.AppendChild(createNode(doc, "wtSlowdownBubble_color", System.Drawing.ColorTranslator.ToHtml(wtSlowdownBubbleColor)));
            {
                XmlNode node = doc.CreateElement("hitbox_kb_colors");
                renderNode.AppendChild(node);
                foreach (Color c in Runtime.hitboxKnockbackColors)
                    node.AppendChild(createNode(doc, "color", System.Drawing.ColorTranslator.ToHtml(c)));
            }
            {
                XmlNode node = doc.CreateElement("hitbox_id_colors");
                renderNode.AppendChild(node);
                foreach (Color c in Runtime.hitboxIdColors)
                    node.AppendChild(createNode(doc, "color", System.Drawing.ColorTranslator.ToHtml(c)));
            }

            renderNode.AppendChild(createNode(doc, "render_path", renderPath.ToString()));
            renderNode.AppendChild(createNode(doc, "render_indicators", renderIndicators.ToString()));
            {
                XmlNode node = doc.CreateElement("render_LVD");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", renderLVD.ToString()));
                node.AppendChild(createNode(doc, "render_collisions", renderCollisions.ToString()));
                node.AppendChild(createNode(doc, "render_collision_normals", renderCollisionNormals.ToString()));

                node.AppendChild(createNode(doc, "render_respawns", renderRespawns.ToString()));
                node.AppendChild(createNode(doc, "render_spawns", renderSpawns.ToString()));
                node.AppendChild(createNode(doc, "render_item_spawners", renderItemSpawners.ToString()));
                node.AppendChild(createNode(doc, "render_general_points", renderGeneralPoints.ToString()));
                node.AppendChild(createNode(doc, "render_otherLVDEntries", renderOtherLVDEntries.ToString()));
                node.AppendChild(createNode(doc, "render_swag", renderSwagY.ToString()));
                node.AppendChild(createNode(doc, "render_swagZ", renderSwagZ.ToString()));
            }
            {
                XmlNode etcNode = doc.CreateElement("ETC");
                mainNode.AppendChild(etcNode);
                etcNode.AppendChild(createNode(doc, "param_dir", paramDir));
            }

            //Discord settings
            {
                XmlNode discordNode = doc.CreateElement("DISCORDSETTINGS");
                discordNode.AppendChild(createNode(doc, "image_key_mode", ((int)DiscordSettings.imageKeyMode).ToString()));
                discordNode.AppendChild(createNode(doc, "user_image_key", DiscordSettings.userPickedImageKey));
                discordNode.AppendChild(createNode(doc, "use_user_mod_name", DiscordSettings.useUserModName.ToString()));
                discordNode.AppendChild(createNode(doc, "user_mod_name", DiscordSettings.userNamedMod));
                discordNode.AppendChild(createNode(doc, "show_current_window", DiscordSettings.showCurrentWindow.ToString()));
                discordNode.AppendChild(createNode(doc, "show_time_elapsed", DiscordSettings.showTimeElapsed.ToString()));
                mainNode.AppendChild(discordNode);
            }

            doc.Save(MainForm.executableDir + "\\config.xml");
        }

        public static XmlNode createNode(XmlDocument doc, string el, string v)
        {
            XmlNode floorstyle = doc.CreateElement(el);
            floorstyle.InnerText = v;
            return floorstyle;
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
