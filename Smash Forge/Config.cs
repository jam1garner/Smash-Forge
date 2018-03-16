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
    class Config
    {
        public static void StartupFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Save();
                return;
            }

            // Read Config
            ReadConfigFromFile(fileName);
            EnsureHitboxColors();

        }

        private static void ReadConfigFromFile(string fileName)
        {
            int discordImageKey;
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
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
                                        Runtime.FloorURL = node.InnerText;
                                        Rendering.RenderTools.floorTexture = NUT.loadImage(new Bitmap(node.InnerText));
                                        Runtime.floorStyle = Runtime.FloorStyle.UserTexture;
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
                                case "floor": Enum.TryParse(node.InnerText, out Runtime.floorWrap); break;
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
                                case "floor": float.TryParse(node.InnerText, out Runtime.floorSize); break;
                            }
                        }
                        break;
                    case "guide_lines": bool.TryParse(node.InnerText, out Runtime.renderFloorLines); break;
                    case "zoom_speed": float.TryParse(node.InnerText, out Runtime.zoomspeed); break;
                    case "zoom_modifier_multiplier": float.TryParse(node.InnerText, out Runtime.zoomModifierScale); break;
                    case "render_depth": float.TryParse(node.InnerText, out Runtime.renderDepth); break;
                    case "fov": float.TryParse(node.InnerText, out Runtime.fov); break;
                    case "back_gradient_top": try { Runtime.back1 = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                    case "back_gradient_bottom": try { Runtime.back2 = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;

                    case "type": if (node.ParentNode != null && node.ParentNode.Name.Equals("RENDERSETTINGS")) Enum.TryParse(node.InnerText, out Runtime.renderType); break;
                    case "OpenGL_2.10": bool.TryParse(node.InnerText, out Runtime.useLegacyShaders); break;
                    case "camera_light": bool.TryParse(node.InnerText, out Runtime.cameraLight); break;
                    case "use_normal_map": bool.TryParse(node.InnerText, out Runtime.renderNormalMap); break;
                    case "render_vertex_color": bool.TryParse(node.InnerText, out Runtime.renderVertColor); break;
                    case "render_alpha": bool.TryParse(node.InnerText, out Runtime.renderAlpha); break;
                    case "render_diffuse": bool.TryParse(node.InnerText, out Runtime.renderDiffuse); break;
                    case "render_specular": bool.TryParse(node.InnerText, out Runtime.renderSpecular); break;
                    case "render_fresnel": bool.TryParse(node.InnerText, out Runtime.renderFresnel); break;
                    case "render_reflection": bool.TryParse(node.InnerText, out Runtime.renderReflection); break;

                    case "render_selection": bool.TryParse(node.InnerText, out Runtime.renderModelSelection); break;
                    case "render_wireframe": bool.TryParse(node.InnerText, out Runtime.renderModelWireframe); break;
                    case "render_bones": bool.TryParse(node.InnerText, out Runtime.renderBones); break;
                    case "render_collisions": bool.TryParse(node.InnerText, out Runtime.renderCollisions); break;
                    case "render_collision_normals": bool.TryParse(node.InnerText, out Runtime.renderCollisionNormals); break;
                    case "render_hitboxes": bool.TryParse(node.InnerText, out Runtime.renderHitboxes); break;
                    case "render_interpolated_hitboxes": bool.TryParse(node.InnerText, out Runtime.renderInterpolatedHitboxes); break;
                    case "render_hitboxes_no_overlap": bool.TryParse(node.InnerText, out Runtime.renderHitboxesNoOverlap); break;
                    case "render_hitboxes_mode": int.TryParse(node.InnerText, out Runtime.hitboxRenderMode); break;
                    case "render_hitbox_angles": bool.TryParse(node.InnerText, out Runtime.renderHitboxAngles); break;
                    case "render_hurtboxes": bool.TryParse(node.InnerText, out Runtime.renderHurtboxes); break;
                    case "render_hurtboxes_zone": bool.TryParse(node.InnerText, out Runtime.renderHurtboxesZone); break;
                    case "render_ECB": bool.TryParse(node.InnerText, out Runtime.renderECB); break;
                    case "render_special_bubbles": bool.TryParse(node.InnerText, out Runtime.renderSpecialBubbles); break;
                    case "render_ledge_grabboxes": bool.TryParse(node.InnerText, out Runtime.renderLedgeGrabboxes); break;
                    case "render_reverse_ledge_grabboxes": bool.TryParse(node.InnerText, out Runtime.renderReverseLedgeGrabboxes); break;
                    case "render_tether_ledge_grabboxes": bool.TryParse(node.InnerText, out Runtime.renderTetherLedgeGrabboxes); break;
                    case "render_bounding_boxes": bool.TryParse(node.InnerText, out Runtime.renderBoundingBox); break;
                    case "render_path": bool.TryParse(node.InnerText, out Runtime.renderPath); break;
                    case "render_respawns": bool.TryParse(node.InnerText, out Runtime.renderRespawns); break;
                    case "render_spawns": bool.TryParse(node.InnerText, out Runtime.renderSpawns); break;
                    case "render_item_spawners": bool.TryParse(node.InnerText, out Runtime.renderItemSpawners); break;
                    case "render_general_points": bool.TryParse(node.InnerText, out Runtime.renderGeneralPoints); break;
                    case "render_otherLVDEntries": bool.TryParse(node.InnerText, out Runtime.renderOtherLVDEntries); break;
                    case "render_swag": bool.TryParse(node.InnerText, out Runtime.renderSwagY); break;
                    case "render_swagZ": bool.TryParse(node.InnerText, out Runtime.renderSwagZ); break;
                    case "fighter_dir": Runtime.fighterDir = node.InnerText; break;
                    case "param_dir": Runtime.paramDir = node.InnerText; break;
                    case "render_indicators": bool.TryParse(node.InnerText, out Runtime.renderIndicators); break;
                    case "hitbox_alpha": int.TryParse(node.InnerText, out Runtime.hitboxAlpha); break;
                    case "hurtbox_alpha": int.TryParse(node.InnerText, out Runtime.hurtboxAlpha); break;
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
                    case "image_key_mode": int.TryParse(node.InnerText, out discordImageKey); DiscordSettings.imageKeyMode = (DiscordSettings.ImageKeyMode)discordImageKey; break;
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
                                case "diffuse": bool.TryParse(node.InnerText, out Runtime.renderDiffuse); break;
                                case "specular": bool.TryParse(node.InnerText, out Runtime.renderSpecular); break;
                                case "fresnel": bool.TryParse(node.InnerText, out Runtime.renderFresnel); break;
                                case "reflection": bool.TryParse(node.InnerText, out Runtime.renderReflection); break;
                                case "floor": bool.TryParse(node.InnerText, out Runtime.renderFloor); break;
                                case "lighting": bool.TryParse(node.InnerText, out Runtime.renderMaterialLighting); break;
                                case "render_model": bool.TryParse(node.InnerText, out Runtime.renderModel); break;
                                case "render_LVD": bool.TryParse(node.InnerText, out Runtime.renderLVD); break;
                            }
                        }
                        break;
                    case "color":
                        if (node.ParentNode != null)
                        {
                            switch (node.ParentNode.Name)
                            {
                                case "floor": try { Runtime.floorColor = ColorTranslator.FromHtml(node.InnerText); } catch (Exception) { } break;
                                case "hitbox_kb_colors": try { Runtime.hitboxKnockbackColors.Add(ColorTranslator.FromHtml(node.InnerText)); } catch (Exception) { } break;
                                case "hitbox_id_colors": try { Runtime.hitboxIdColors.Add(ColorTranslator.FromHtml(node.InnerText)); } catch (Exception) { } break;
                            }
                        }
                        break;
                    case "style":
                        if (node.ParentNode != null)
                        {
                            switch (node.ParentNode.Name)
                            {
                                case "floor": Enum.TryParse(node.InnerText, out Runtime.floorStyle); break;
                            }
                        }
                        break;
                    default:
                        Console.WriteLine(node.Name);
                        break;
                }
            }
        }

        public static void Save()
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
                node.AppendChild(createNode(doc, "enabled", Runtime.renderFloor.ToString()));
                node.AppendChild(createNode(doc, "style", Runtime.floorStyle.ToString()));
                node.AppendChild(createNode(doc, "color", ColorTranslator.ToHtml(Runtime.floorColor)));
                node.AppendChild(createNode(doc, "size", Runtime.floorSize.ToString()));
                if (Runtime.floorStyle == Runtime.FloorStyle.UserTexture)
                    node.AppendChild(createNode(doc, "texture", Runtime.FloorURL));
            }

            viewportNode.AppendChild(createNode(doc, "zoom_speed", Runtime.zoomspeed.ToString()));
            viewportNode.AppendChild(createNode(doc, "zoom_modifier_multiplier", Runtime.zoomModifierScale.ToString()));
            viewportNode.AppendChild(createNode(doc, "fov", Runtime.fov.ToString()));
            viewportNode.AppendChild(createNode(doc, "render_depth", Runtime.renderDepth.ToString()));
            viewportNode.AppendChild(createNode(doc, "render_background", Runtime.renderBackGround.ToString()));
            viewportNode.AppendChild(createNode(doc, "back_gradient_top", ColorTranslator.ToHtml(Runtime.back1)));
            viewportNode.AppendChild(createNode(doc, "back_gradient_bottom", ColorTranslator.ToHtml(Runtime.back2)));

            XmlNode renderNode = doc.CreateElement("RENDERSETTINGS");
            mainNode.AppendChild(renderNode);
            renderNode.AppendChild(createNode(doc, "type", Runtime.renderType.ToString()));
            renderNode.AppendChild(createNode(doc, "render_vertex_color", Runtime.renderVertColor.ToString()));
            renderNode.AppendChild(createNode(doc, "render_alpha", Runtime.renderAlpha.ToString()));
            renderNode.AppendChild(createNode(doc, "camera_light", Runtime.cameraLight.ToString()));
            renderNode.AppendChild(createNode(doc, "OpenGL_2.10", Runtime.useLegacyShaders.ToString()));
            renderNode.AppendChild(createNode(doc, "use_normal_map", Runtime.renderNormalMap.ToString()));

            {
                XmlNode node = doc.CreateElement("lighting");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", Runtime.renderMaterialLighting.ToString()));
                node.AppendChild(createNode(doc, "render_diffuse", Runtime.renderDiffuse.ToString()));
                node.AppendChild(createNode(doc, "render_specular", Runtime.renderSpecular.ToString()));
                node.AppendChild(createNode(doc, "render_fresnel", Runtime.renderFresnel.ToString()));
                node.AppendChild(createNode(doc, "render_reflection", Runtime.renderReflection.ToString()));
            }
            {
                XmlNode node = doc.CreateElement("diffuse");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", Runtime.renderDiffuse.ToString()));
                node.AppendChild(createNode(doc, "intensity", Runtime.difIntensity.ToString()));
            }
            {
                XmlNode node = doc.CreateElement("specular");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", Runtime.renderSpecular.ToString()));
                node.AppendChild(createNode(doc, "intensity", Runtime.spcIntentensity.ToString()));
            }
            {
                XmlNode node = doc.CreateElement("fresnel");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", Runtime.renderFresnel.ToString()));
                node.AppendChild(createNode(doc, "intensity", Runtime.frsIntensity.ToString()));
            }
            {
                XmlNode node = doc.CreateElement("reflection");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", Runtime.renderReflection.ToString()));
                node.AppendChild(createNode(doc, "intensity", Runtime.refIntensity.ToString()));
            }
            {
                XmlNode node = doc.CreateElement("ambient");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "intensity", Runtime.ambItensity.ToString()));
            }

            {
                XmlNode node = doc.CreateElement("render_model");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", Runtime.renderModel.ToString()));
                node.AppendChild(createNode(doc, "render_selection", Runtime.renderModelSelection.ToString()));
                node.AppendChild(createNode(doc, "render_wireframe", Runtime.renderModelWireframe.ToString()));
                node.AppendChild(createNode(doc, "render_bones", Runtime.renderBones.ToString()));
                node.AppendChild(createNode(doc, "render_bounding_boxes", Runtime.renderBoundingBox.ToString()));
            }

            renderNode.AppendChild(createNode(doc, "render_ECB", Runtime.renderECB.ToString()));
            renderNode.AppendChild(createNode(doc, "render_hurtboxes", Runtime.renderHurtboxes.ToString()));
            renderNode.AppendChild(createNode(doc, "render_hurtboxes_zone", Runtime.renderHurtboxesZone.ToString()));
            renderNode.AppendChild(createNode(doc, "render_hitboxes", Runtime.renderHitboxes.ToString()));
            renderNode.AppendChild(createNode(doc, "render_interpolated_hitboxes", Runtime.renderInterpolatedHitboxes.ToString()));
            renderNode.AppendChild(createNode(doc, "render_hitboxes_no_overlap", Runtime.renderHitboxesNoOverlap.ToString()));
            renderNode.AppendChild(createNode(doc, "render_hitboxes_mode", Runtime.hitboxRenderMode.ToString()));
            renderNode.AppendChild(createNode(doc, "render_hitbox_angles", Runtime.renderHitboxAngles.ToString()));
            renderNode.AppendChild(createNode(doc, "render_special_bubbles", Runtime.renderSpecialBubbles.ToString()));
            renderNode.AppendChild(createNode(doc, "render_ledge_grabboxes", Runtime.renderLedgeGrabboxes.ToString()));
            renderNode.AppendChild(createNode(doc, "render_reverse_ledge_grabboxes", Runtime.renderReverseLedgeGrabboxes.ToString()));
            renderNode.AppendChild(createNode(doc, "render_tether_ledge_grabboxes", Runtime.renderTetherLedgeGrabboxes.ToString()));
            renderNode.AppendChild(createNode(doc, "hitbox_alpha", Runtime.hitboxAlpha.ToString()));
            renderNode.AppendChild(createNode(doc, "hurtbox_alpha", Runtime.hurtboxAlpha.ToString()));
            renderNode.AppendChild(createNode(doc, "hurtbox_color", ColorTranslator.ToHtml(Runtime.hurtboxColor)));
            renderNode.AppendChild(createNode(doc, "hurtbox_color_hi", ColorTranslator.ToHtml(Runtime.hurtboxColorHi)));
            renderNode.AppendChild(createNode(doc, "hurtbox_color_med", ColorTranslator.ToHtml(Runtime.hurtboxColorMed)));
            renderNode.AppendChild(createNode(doc, "hurtbox_color_low", ColorTranslator.ToHtml(Runtime.hurtboxColorLow)));
            renderNode.AppendChild(createNode(doc, "hurtbox_color_selected", ColorTranslator.ToHtml(Runtime.hurtboxColorSelected)));
            renderNode.AppendChild(createNode(doc, "hitbox_angles_color", ColorTranslator.ToHtml(Runtime.hitboxAnglesColor)));
            renderNode.AppendChild(createNode(doc, "windbox_color", ColorTranslator.ToHtml(Runtime.windboxColor)));
            renderNode.AppendChild(createNode(doc, "grabbox_color", ColorTranslator.ToHtml(Runtime.grabboxColor)));
            renderNode.AppendChild(createNode(doc, "searchbox_color", ColorTranslator.ToHtml(Runtime.searchboxColor)));
            renderNode.AppendChild(createNode(doc, "counterBubble_color", ColorTranslator.ToHtml(Runtime.counterBubbleColor)));
            renderNode.AppendChild(createNode(doc, "reflectBubble_color", ColorTranslator.ToHtml(Runtime.reflectBubbleColor)));
            renderNode.AppendChild(createNode(doc, "shieldBubble_color", ColorTranslator.ToHtml(Runtime.shieldBubbleColor)));
            renderNode.AppendChild(createNode(doc, "absorbBubble_color", ColorTranslator.ToHtml(Runtime.absorbBubbleColor)));
            renderNode.AppendChild(createNode(doc, "wtSlowdownBubble_color", ColorTranslator.ToHtml(Runtime.wtSlowdownBubbleColor)));
            {
                XmlNode node = doc.CreateElement("hitbox_kb_colors");
                renderNode.AppendChild(node);
                foreach (Color c in Runtime.hitboxKnockbackColors)
                    node.AppendChild(createNode(doc, "color", ColorTranslator.ToHtml(c)));
            }
            {
                XmlNode node = doc.CreateElement("hitbox_id_colors");
                renderNode.AppendChild(node);
                foreach (Color c in Runtime.hitboxIdColors)
                    node.AppendChild(createNode(doc, "color", ColorTranslator.ToHtml(c)));
            }

            renderNode.AppendChild(createNode(doc, "render_path", Runtime.renderPath.ToString()));
            renderNode.AppendChild(createNode(doc, "render_indicators", Runtime.renderIndicators.ToString()));
            {
                XmlNode node = doc.CreateElement("render_LVD");
                renderNode.AppendChild(node);
                node.AppendChild(createNode(doc, "enabled", Runtime.renderLVD.ToString()));
                node.AppendChild(createNode(doc, "render_collisions", Runtime.renderCollisions.ToString()));
                node.AppendChild(createNode(doc, "render_collision_normals", Runtime.renderCollisionNormals.ToString()));

                node.AppendChild(createNode(doc, "render_respawns", Runtime.renderRespawns.ToString()));
                node.AppendChild(createNode(doc, "render_spawns", Runtime.renderSpawns.ToString()));
                node.AppendChild(createNode(doc, "render_item_spawners", Runtime.renderItemSpawners.ToString()));
                node.AppendChild(createNode(doc, "render_general_points", Runtime.renderGeneralPoints.ToString()));
                node.AppendChild(createNode(doc, "render_otherLVDEntries", Runtime.renderOtherLVDEntries.ToString()));
                node.AppendChild(createNode(doc, "render_swag", Runtime.renderSwagY.ToString()));
                node.AppendChild(createNode(doc, "render_swagZ", Runtime.renderSwagZ.ToString()));
            }
            {
                XmlNode etcNode = doc.CreateElement("ETC");
                mainNode.AppendChild(etcNode);
                etcNode.AppendChild(createNode(doc, "param_dir", Runtime.paramDir));
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

        public static void EnsureHitboxColors()
        {
            if (Runtime.hitboxKnockbackColors.Count <= 0)
                Runtime.hitboxKnockbackColors = new List<Color>(Runtime.defaultHitboxKnockbackColors);
            if (Runtime.hitboxIdColors.Count <= 0)
                Runtime.hitboxIdColors = new List<Color>(Runtime.defaultHitboxIdColors);
        }
    }
}

