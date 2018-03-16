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
                    case "guide_lines":
                        bool.TryParse(node.InnerText, out Runtime.renderFloorLines);
                        break;
                    case "zoom_speed":
                        float.TryParse(node.InnerText, out Runtime.zoomspeed);
                        break;
                    case "zoom_modifier_multiplier":
                        float.TryParse(node.InnerText, out Runtime.zoomModifierScale);
                        break;
                    case "render_depth":
                        float.TryParse(node.InnerText, out Runtime.renderDepth);
                        break;
                    case "fov":
                        float.TryParse(node.InnerText, out Runtime.fov);
                        break;
                    case "back_gradient_top":
                        try
                        {
                            Runtime.backgroundGradientTop = ColorTranslator.FromHtml(node.InnerText);
                        }
                        catch (Exception)
                        {

                        }
                        break;
                    case "back_gradient_bottom":
                        try
                        {
                            Runtime.backgroundGradientBottom = ColorTranslator.FromHtml(node.InnerText);
                        }
                        catch (Exception)
                        {

                        }
                        break;
                    case "type":
                        if (node.ParentNode != null && node.ParentNode.Name.Equals("RENDERSETTINGS"))
                            Enum.TryParse(node.InnerText, out Runtime.renderType);
                        break;
                    case "OpenGL_2.10":
                        bool.TryParse(node.InnerText, out Runtime.useLegacyShaders);
                        break;
                    case "camera_light":
                        bool.TryParse(node.InnerText, out Runtime.cameraLight);
                        break;
                    case "use_normal_map":
                        bool.TryParse(node.InnerText, out Runtime.renderNormalMap);
                        break;
                    case "render_vertex_color":
                        bool.TryParse(node.InnerText, out Runtime.renderVertColor);
                        break;
                    case "render_alpha":
                        bool.TryParse(node.InnerText, out Runtime.renderAlpha);
                        break;
                    case "render_diffuse":
                        bool.TryParse(node.InnerText, out Runtime.renderDiffuse);
                        break;
                    case "render_specular":
                        bool.TryParse(node.InnerText, out Runtime.renderSpecular);
                        break;
                    case "render_fresnel":
                        bool.TryParse(node.InnerText, out Runtime.renderFresnel);
                        break;
                    case "render_reflection":
                        bool.TryParse(node.InnerText, out Runtime.renderReflection);
                        break;
                    case "render_selection":
                        bool.TryParse(node.InnerText, out Runtime.renderModelSelection);
                        break;
                    case "render_wireframe":
                        bool.TryParse(node.InnerText, out Runtime.renderModelWireframe);
                        break;
                    case "render_bones":
                        bool.TryParse(node.InnerText, out Runtime.renderBones);
                        break;
                    case "render_collisions":
                        bool.TryParse(node.InnerText, out Runtime.renderCollisions);
                        break;
                    case "render_collision_normals":
                        bool.TryParse(node.InnerText, out Runtime.renderCollisionNormals);
                        break;
                    case "render_hitboxes":
                        bool.TryParse(node.InnerText, out Runtime.renderHitboxes);
                        break;
                    case "render_interpolated_hitboxes":
                        bool.TryParse(node.InnerText, out Runtime.renderInterpolatedHitboxes);
                        break;
                    case "render_hitboxes_no_overlap":
                        bool.TryParse(node.InnerText, out Runtime.renderHitboxesNoOverlap);
                        break;
                    case "render_hitboxes_mode":
                        int.TryParse(node.InnerText, out Runtime.hitboxRenderMode);
                        break;
                    case "render_hitbox_angles":
                        bool.TryParse(node.InnerText, out Runtime.renderHitboxAngles);
                        break;
                    case "render_hurtboxes":
                        bool.TryParse(node.InnerText, out Runtime.renderHurtboxes);
                        break;
                    case "render_hurtboxes_zone":
                        bool.TryParse(node.InnerText, out Runtime.renderHurtboxesZone);
                        break;
                    case "render_ECB":
                        bool.TryParse(node.InnerText, out Runtime.renderECB);
                        break;
                    case "render_special_bubbles":
                        bool.TryParse(node.InnerText, out Runtime.renderSpecialBubbles);
                        break;
                    case "render_ledge_grabboxes":
                        bool.TryParse(node.InnerText, out Runtime.renderLedgeGrabboxes);
                        break;
                    case "render_reverse_ledge_grabboxes":
                        bool.TryParse(node.InnerText, out Runtime.renderReverseLedgeGrabboxes);
                        break;
                    case "render_tether_ledge_grabboxes":
                        bool.TryParse(node.InnerText, out Runtime.renderTetherLedgeGrabboxes);
                        break;
                    case "render_bounding_boxes":
                        bool.TryParse(node.InnerText, out Runtime.renderBoundingBox);
                        break;
                    case "render_path":
                        bool.TryParse(node.InnerText, out Runtime.renderPath);
                        break;
                    case "render_respawns":
                        bool.TryParse(node.InnerText, out Runtime.renderRespawns);
                        break;
                    case "render_spawns":
                        bool.TryParse(node.InnerText, out Runtime.renderSpawns);
                        break;
                    case "render_item_spawners":
                        bool.TryParse(node.InnerText, out Runtime.renderItemSpawners);
                        break;
                    case "render_general_points":
                        bool.TryParse(node.InnerText, out Runtime.renderGeneralPoints);
                        break;
                    case "render_otherLVDEntries":
                        bool.TryParse(node.InnerText, out Runtime.renderOtherLVDEntries);
                        break;
                    case "render_swag":
                        bool.TryParse(node.InnerText, out Runtime.renderSwagY);
                        break;
                    case "render_swagZ":
                        bool.TryParse(node.InnerText, out Runtime.renderSwagZ);
                        break;
                    case "fighter_dir":
                        Runtime.fighterDir = node.InnerText;
                        break;
                    case "param_dir":
                        Runtime.paramDir = node.InnerText;
                        break;
                    case "render_indicators":
                        bool.TryParse(node.InnerText, out Runtime.renderIndicators);
                        break;
                    case "hitbox_alpha":
                        int.TryParse(node.InnerText, out Runtime.hitboxAlpha);
                        break;
                    case "hurtbox_alpha":
                        int.TryParse(node.InnerText, out Runtime.hurtboxAlpha);
                        break;
                    case "hitbox_angles_color":
                        try
                        {
                            Runtime.hitboxAnglesColor = ColorTranslator.FromHtml(node.InnerText);
                        }
                        catch (Exception)
                        {

                        }
                        break;
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
                    case "image_key_mode":
                        int.TryParse(node.InnerText, out discordImageKey);
                        DiscordSettings.imageKeyMode = (DiscordSettings.ImageKeyMode)discordImageKey;
                        break;
                    case "user_image_key":
                        DiscordSettings.userPickedImageKey = node.InnerText;
                        break;
                    case "user_mod_name":
                        DiscordSettings.userNamedMod = node.InnerText;
                        break;
                    case "use_user_mod_name":
                        bool.TryParse(node.InnerText, out DiscordSettings.useUserModName);
                        break;
                    case "show_current_window":
                        bool.TryParse(node.InnerText, out DiscordSettings.showCurrentWindow);
                        break;
                    case "show_time_elapsed":
                        bool.TryParse(node.InnerText, out DiscordSettings.showTimeElapsed);
                        break;

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
                }
            }
        }

        public static void Save()
        {
            EnsureHitboxColors();
            XmlDocument doc = CreateXmlFromSettings();
            doc.Save(MainForm.executableDir + "\\config.xml");
        }

        private static XmlDocument CreateXmlFromSettings()
        {
            XmlDocument doc = new XmlDocument();

            XmlNode mainNode = doc.CreateElement("FORGECONFIG");
            doc.AppendChild(mainNode);

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

            XmlComment enumComment = doc.CreateComment(comment);
            mainNode.AppendChild(enumComment);

            XmlNode viewportNode = doc.CreateElement("VIEWPORT");
            mainNode.AppendChild(viewportNode);

            AppendFloorSettings(doc, viewportNode);

            viewportNode.AppendChild(createNode(doc, "zoom_speed", Runtime.zoomspeed.ToString()));
            viewportNode.AppendChild(createNode(doc, "zoom_modifier_multiplier", Runtime.zoomModifierScale.ToString()));
            viewportNode.AppendChild(createNode(doc, "fov", Runtime.fov.ToString()));
            viewportNode.AppendChild(createNode(doc, "render_depth", Runtime.renderDepth.ToString()));
            viewportNode.AppendChild(createNode(doc, "render_background", Runtime.renderBackGround.ToString()));
            viewportNode.AppendChild(createNode(doc, "back_gradient_top", ColorTranslator.ToHtml(Runtime.backgroundGradientTop)));
            viewportNode.AppendChild(createNode(doc, "back_gradient_bottom", ColorTranslator.ToHtml(Runtime.backgroundGradientBottom)));

            XmlNode renderSettingsNode = doc.CreateElement("RENDERSETTINGS");
            mainNode.AppendChild(renderSettingsNode);
            renderSettingsNode.AppendChild(createNode(doc, "type", Runtime.renderType.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_vertex_color", Runtime.renderVertColor.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_alpha", Runtime.renderAlpha.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "camera_light", Runtime.cameraLight.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "OpenGL_2.10", Runtime.useLegacyShaders.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "use_normal_map", Runtime.renderNormalMap.ToString()));

            XmlNode lightingNode = doc.CreateElement("lighting");
            renderSettingsNode.AppendChild(lightingNode);
            lightingNode.AppendChild(createNode(doc, "enabled", Runtime.renderMaterialLighting.ToString()));
            lightingNode.AppendChild(createNode(doc, "render_diffuse", Runtime.renderDiffuse.ToString()));
            lightingNode.AppendChild(createNode(doc, "render_specular", Runtime.renderSpecular.ToString()));
            lightingNode.AppendChild(createNode(doc, "render_fresnel", Runtime.renderFresnel.ToString()));
            lightingNode.AppendChild(createNode(doc, "render_reflection", Runtime.renderReflection.ToString()));

            XmlNode diffuseNode = doc.CreateElement("diffuse");
            renderSettingsNode.AppendChild(diffuseNode);
            diffuseNode.AppendChild(createNode(doc, "enabled", Runtime.renderDiffuse.ToString()));
            diffuseNode.AppendChild(createNode(doc, "intensity", Runtime.difIntensity.ToString()));

            XmlNode specularNode = doc.CreateElement("specular");
            renderSettingsNode.AppendChild(specularNode);
            specularNode.AppendChild(createNode(doc, "enabled", Runtime.renderSpecular.ToString()));
            specularNode.AppendChild(createNode(doc, "intensity", Runtime.spcIntentensity.ToString()));

            XmlNode fresnelNode = doc.CreateElement("fresnel");
            renderSettingsNode.AppendChild(fresnelNode);
            fresnelNode.AppendChild(createNode(doc, "enabled", Runtime.renderFresnel.ToString()));
            fresnelNode.AppendChild(createNode(doc, "intensity", Runtime.frsIntensity.ToString()));

            XmlNode reflectionNode = doc.CreateElement("reflection");
            renderSettingsNode.AppendChild(reflectionNode);
            reflectionNode.AppendChild(createNode(doc, "enabled", Runtime.renderReflection.ToString()));
            reflectionNode.AppendChild(createNode(doc, "intensity", Runtime.refIntensity.ToString()));

            XmlNode ambientNode = doc.CreateElement("ambient");
            renderSettingsNode.AppendChild(ambientNode);
            ambientNode.AppendChild(createNode(doc, "intensity", Runtime.ambItensity.ToString()));

            XmlNode renderModelNode = doc.CreateElement("render_model");
            renderSettingsNode.AppendChild(renderModelNode);
            renderModelNode.AppendChild(createNode(doc, "enabled", Runtime.renderModel.ToString()));
            renderModelNode.AppendChild(createNode(doc, "render_selection", Runtime.renderModelSelection.ToString()));
            renderModelNode.AppendChild(createNode(doc, "render_wireframe", Runtime.renderModelWireframe.ToString()));
            renderModelNode.AppendChild(createNode(doc, "render_bones", Runtime.renderBones.ToString()));
            renderModelNode.AppendChild(createNode(doc, "render_bounding_boxes", Runtime.renderBoundingBox.ToString()));

            renderSettingsNode.AppendChild(createNode(doc, "render_ECB", Runtime.renderECB.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_hurtboxes", Runtime.renderHurtboxes.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_hurtboxes_zone", Runtime.renderHurtboxesZone.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_hitboxes", Runtime.renderHitboxes.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_interpolated_hitboxes", Runtime.renderInterpolatedHitboxes.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_hitboxes_no_overlap", Runtime.renderHitboxesNoOverlap.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_hitboxes_mode", Runtime.hitboxRenderMode.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_hitbox_angles", Runtime.renderHitboxAngles.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_special_bubbles", Runtime.renderSpecialBubbles.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_ledge_grabboxes", Runtime.renderLedgeGrabboxes.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_reverse_ledge_grabboxes", Runtime.renderReverseLedgeGrabboxes.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_tether_ledge_grabboxes", Runtime.renderTetherLedgeGrabboxes.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "hitbox_alpha", Runtime.hitboxAlpha.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "hurtbox_alpha", Runtime.hurtboxAlpha.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "hurtbox_color", ColorTranslator.ToHtml(Runtime.hurtboxColor)));
            renderSettingsNode.AppendChild(createNode(doc, "hurtbox_color_hi", ColorTranslator.ToHtml(Runtime.hurtboxColorHi)));
            renderSettingsNode.AppendChild(createNode(doc, "hurtbox_color_med", ColorTranslator.ToHtml(Runtime.hurtboxColorMed)));
            renderSettingsNode.AppendChild(createNode(doc, "hurtbox_color_low", ColorTranslator.ToHtml(Runtime.hurtboxColorLow)));
            renderSettingsNode.AppendChild(createNode(doc, "hurtbox_color_selected", ColorTranslator.ToHtml(Runtime.hurtboxColorSelected)));
            renderSettingsNode.AppendChild(createNode(doc, "hitbox_angles_color", ColorTranslator.ToHtml(Runtime.hitboxAnglesColor)));
            renderSettingsNode.AppendChild(createNode(doc, "windbox_color", ColorTranslator.ToHtml(Runtime.windboxColor)));
            renderSettingsNode.AppendChild(createNode(doc, "grabbox_color", ColorTranslator.ToHtml(Runtime.grabboxColor)));
            renderSettingsNode.AppendChild(createNode(doc, "searchbox_color", ColorTranslator.ToHtml(Runtime.searchboxColor)));
            renderSettingsNode.AppendChild(createNode(doc, "counterBubble_color", ColorTranslator.ToHtml(Runtime.counterBubbleColor)));
            renderSettingsNode.AppendChild(createNode(doc, "reflectBubble_color", ColorTranslator.ToHtml(Runtime.reflectBubbleColor)));
            renderSettingsNode.AppendChild(createNode(doc, "shieldBubble_color", ColorTranslator.ToHtml(Runtime.shieldBubbleColor)));
            renderSettingsNode.AppendChild(createNode(doc, "absorbBubble_color", ColorTranslator.ToHtml(Runtime.absorbBubbleColor)));
            renderSettingsNode.AppendChild(createNode(doc, "wtSlowdownBubble_color", ColorTranslator.ToHtml(Runtime.wtSlowdownBubbleColor)));
            {
                XmlNode node = doc.CreateElement("hitbox_kb_colors");
                renderSettingsNode.AppendChild(node);
                foreach (Color c in Runtime.hitboxKnockbackColors)
                    node.AppendChild(createNode(doc, "color", ColorTranslator.ToHtml(c)));
            }
            {
                XmlNode node = doc.CreateElement("hitbox_id_colors");
                renderSettingsNode.AppendChild(node);
                foreach (Color c in Runtime.hitboxIdColors)
                    node.AppendChild(createNode(doc, "color", ColorTranslator.ToHtml(c)));
            }

            renderSettingsNode.AppendChild(createNode(doc, "render_path", Runtime.renderPath.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_indicators", Runtime.renderIndicators.ToString()));

            AppendLvdRenderSettings(doc, renderSettingsNode);

            XmlNode paramDirNode = doc.CreateElement("ETC");
            mainNode.AppendChild(paramDirNode);
            paramDirNode.AppendChild(createNode(doc, "param_dir", Runtime.paramDir));

            AppendDiscordSettings(doc, mainNode);
            return doc;
        }

        private static void AppendFloorSettings(XmlDocument doc, XmlNode viewportNode)
        {
            XmlNode floorNode = doc.CreateElement("floor");
            viewportNode.AppendChild(floorNode);
            floorNode.AppendChild(createNode(doc, "enabled", Runtime.renderFloor.ToString()));
            floorNode.AppendChild(createNode(doc, "style", Runtime.floorStyle.ToString()));
            floorNode.AppendChild(createNode(doc, "color", ColorTranslator.ToHtml(Runtime.floorColor)));
            floorNode.AppendChild(createNode(doc, "size", Runtime.floorSize.ToString()));
            if (Runtime.floorStyle == Runtime.FloorStyle.UserTexture)
                floorNode.AppendChild(createNode(doc, "texture", Runtime.FloorURL));
        }

        private static void AppendLvdRenderSettings(XmlDocument doc, XmlNode renderSettingsNode)
        {
            XmlNode lvdRenderSettingsNode = doc.CreateElement("render_LVD");
            renderSettingsNode.AppendChild(lvdRenderSettingsNode);
            lvdRenderSettingsNode.AppendChild(createNode(doc, "enabled", Runtime.renderLVD.ToString()));
            lvdRenderSettingsNode.AppendChild(createNode(doc, "render_collisions", Runtime.renderCollisions.ToString()));
            lvdRenderSettingsNode.AppendChild(createNode(doc, "render_collision_normals", Runtime.renderCollisionNormals.ToString()));
            lvdRenderSettingsNode.AppendChild(createNode(doc, "render_respawns", Runtime.renderRespawns.ToString()));
            lvdRenderSettingsNode.AppendChild(createNode(doc, "render_spawns", Runtime.renderSpawns.ToString()));
            lvdRenderSettingsNode.AppendChild(createNode(doc, "render_item_spawners", Runtime.renderItemSpawners.ToString()));
            lvdRenderSettingsNode.AppendChild(createNode(doc, "render_general_points", Runtime.renderGeneralPoints.ToString()));
            lvdRenderSettingsNode.AppendChild(createNode(doc, "render_otherLVDEntries", Runtime.renderOtherLVDEntries.ToString()));
            lvdRenderSettingsNode.AppendChild(createNode(doc, "render_swag", Runtime.renderSwagY.ToString()));
            lvdRenderSettingsNode.AppendChild(createNode(doc, "render_swagZ", Runtime.renderSwagZ.ToString()));
        }

        private static void AppendDiscordSettings(XmlDocument doc, XmlNode mainNode)
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

