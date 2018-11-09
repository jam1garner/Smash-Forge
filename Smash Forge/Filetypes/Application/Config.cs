using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Xml;
using SFGraphics.GLObjects.Textures;

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
                                        Runtime.floorTexFilePath = node.InnerText;
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
                            Rendering.RenderTools.defaultTex = new Texture2D();
                            Rendering.RenderTools.defaultTex.LoadImageData(new Bitmap(node.InnerText));
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
                    case "enable_vSync":
                        bool.TryParse(node.InnerText, out Runtime.enableVSync);
                        break;
                    case "fov":
                        float.TryParse(node.InnerText, out Runtime.fov);
                        break;
                    case "back_gradient_top":
                        TryParseHexColor(node, ref Runtime.backgroundGradientTop);
                        break;
                    case "back_gradient_bottom":
                        TryParseHexColor(node, ref Runtime.backgroundGradientBottom);
                        break;
                    case "type":
                        if (node.ParentNode != null && node.ParentNode.Name.Equals("RENDERSETTINGS"))
                            Enum.TryParse(node.InnerText, out Runtime.renderType);
                        break;
                    case "bone_node_size":
                        float.TryParse(node.InnerText, out Runtime.renderBoneNodeSize);
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
                    case "render_bounding_spheres":
                        bool.TryParse(node.InnerText, out Runtime.renderBoundingSphere);
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
                    case "reander_physicallyBasedRendering":
                        bool.TryParse(node.InnerText, out Runtime.renderBfresPbr);
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
                        TryParseHexColor(node, ref Runtime.hitboxAnglesColor);
                        break;
                    case "hurtbox_color":
                        TryParseHexColor(node, ref Runtime.hurtboxColor);
                        break;
                    case "hurtbox_color_hi":
                        TryParseHexColor(node, ref Runtime.hurtboxColorHi);
                        break;
                    case "hurtbox_color_med":
                        TryParseHexColor(node, ref Runtime.hurtboxColorMed);
                        break;
                    case "hurtbox_color_low":
                        TryParseHexColor(node, ref Runtime.hurtboxColorLow);
                        break;
                    case "hurtbox_color_selected":
                        TryParseHexColor(node, ref Runtime.hurtboxColorSelected);
                        break;
                    case "windbox_color":
                        TryParseHexColor(node, ref Runtime.windboxColor);
                        break;
                    case "grabbox_color":
                        TryParseHexColor(node, ref Runtime.grabboxColor);
                        break;
                    case "searchbox_color":
                        TryParseHexColor(node, ref Runtime.searchboxColor);
                        break;
                    case "counterBubble_color":
                        TryParseHexColor(node, ref Runtime.counterBubbleColor);
                        break;
                    case "reflectBubble_color":
                        TryParseHexColor(node, ref Runtime.reflectBubbleColor);
                        break;
                    case "shieldBubble_color":
                        TryParseHexColor(node, ref Runtime.shieldBubbleColor);
                        break;
                    case "absorbBubble_color":
                        TryParseHexColor(node, ref Runtime.absorbBubbleColor);
                        break;
                    case "wtSlowdownBubble_color":
                        TryParseHexColor(node, ref Runtime.wtSlowdownBubbleColor);
                        break;
                    case "loadAndRenderATKD":
                        bool.TryParse(node.InnerText, out Runtime.LoadAndRenderATKD);
                        break;

                    //Discord Stuff
                    case "discord_enabled":
                        bool.TryParse(node.InnerText, out DiscordSettings.enabled);
                        break;
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
                    case "GamePath":
                        Runtime.MarioOdysseyGamePath = (node.InnerText);
                        break;
                }
            }
        }

        private static bool TryParseHexColor(XmlNode node, ref Color color)
        {
            try
            {
                color = ColorTranslator.FromHtml(node.InnerText);
                return true;
            }
            catch (Exception)
            {
                // Invalid hex format.
                return false;
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

            Floor Style:
            -Normal
            -Solid
            -Textured

            Setting the Floor Texture (must be .png):
            <texture>(texture location)</texture>

            Render Type:
            -Shaded
            -Normals
            -Lighting
            -Diffuse Maps
            -Normal Maps
            -Vertex Color
            -Ambient Occlusion
            -UV Coords
            -UV Test Pattern
            -Tangents
            -Bitangents
            -Light Set
            -Bone Weights
                
            Changing the Default Texture:
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
            viewportNode.AppendChild(createNode(doc, "enable_vSync", Runtime.enableVSync.ToString()));

            AppendBackgroundSettings(doc, viewportNode);
            AppendOdysseyCostumeEditor(doc, viewportNode);

            AppendRenderSettings(doc, mainNode);

            XmlNode paramDirNode = doc.CreateElement("ETC");
            mainNode.AppendChild(paramDirNode);
            paramDirNode.AppendChild(createNode(doc, "param_dir", Runtime.paramDir));

            AppendDiscordSettings(doc, mainNode);
            return doc;
        }

        private static void AppendBackgroundSettings(XmlDocument doc, XmlNode parentNode)
        {
            parentNode.AppendChild(createNode(doc, "render_background", Runtime.renderBackGround.ToString()));
            parentNode.AppendChild(createNode(doc, "back_gradient_top", ColorTranslator.ToHtml(Runtime.backgroundGradientTop)));
            parentNode.AppendChild(createNode(doc, "back_gradient_bottom", ColorTranslator.ToHtml(Runtime.backgroundGradientBottom)));
        }
        private static void AppendOdysseyCostumeEditor(XmlDocument doc, XmlNode parentNode)
        {
            parentNode.AppendChild(createNode(doc, "GamePath", Runtime.MarioOdysseyGamePath.ToString()));
        }

        private static void AppendRenderSettings(XmlDocument doc, XmlNode parentNode)
        {
            XmlNode renderSettingsNode = doc.CreateElement("RENDERSETTINGS");
            parentNode.AppendChild(renderSettingsNode);
            renderSettingsNode.AppendChild(createNode(doc, "type", Runtime.renderType.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_vertex_color", Runtime.renderVertColor.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_alpha", Runtime.renderAlpha.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "camera_light", Runtime.cameraLight.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "use_normal_map", Runtime.renderNormalMap.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "bone_node_size", Runtime.renderBoneNodeSize.ToString()));

            AppendMaterialLightingSettings(doc, renderSettingsNode);
            AppendModelRenderSettings(doc, renderSettingsNode);
            AppendHitBoxHurtBoxRenderSettings(doc, renderSettingsNode);
            AppendLvdRenderSettings(doc, renderSettingsNode);

            renderSettingsNode.AppendChild(createNode(doc, "render_path", Runtime.renderPath.ToString()));
            renderSettingsNode.AppendChild(createNode(doc, "render_indicators", Runtime.renderIndicators.ToString()));
        }

        private static void AppendModelRenderSettings(XmlDocument doc, XmlNode parentNode)
        {
            XmlNode renderModelNode = doc.CreateElement("render_model");
            parentNode.AppendChild(renderModelNode);
            renderModelNode.AppendChild(createNode(doc, "enabled", Runtime.renderModel.ToString()));
            renderModelNode.AppendChild(createNode(doc, "render_selection", Runtime.renderModelSelection.ToString()));
            renderModelNode.AppendChild(createNode(doc, "render_wireframe", Runtime.renderModelWireframe.ToString()));
            renderModelNode.AppendChild(createNode(doc, "render_bones", Runtime.renderBones.ToString()));
            renderModelNode.AppendChild(createNode(doc, "render_bounding_spheres", Runtime.renderBoundingSphere.ToString()));
        }

        private static void AppendMaterialLightingSettings(XmlDocument doc, XmlNode parentNode)
        {
            XmlNode lightingNode = doc.CreateElement("lighting");
            parentNode.AppendChild(lightingNode);
            lightingNode.AppendChild(createNode(doc, "enabled", Runtime.renderMaterialLighting.ToString()));
            lightingNode.AppendChild(createNode(doc, "render_diffuse", Runtime.renderDiffuse.ToString()));
            lightingNode.AppendChild(createNode(doc, "render_specular", Runtime.renderSpecular.ToString()));
            lightingNode.AppendChild(createNode(doc, "render_fresnel", Runtime.renderFresnel.ToString()));
            lightingNode.AppendChild(createNode(doc, "render_reflection", Runtime.renderReflection.ToString()));

            XmlNode diffuseNode = doc.CreateElement("diffuse");
            parentNode.AppendChild(diffuseNode);
            diffuseNode.AppendChild(createNode(doc, "enabled", Runtime.renderDiffuse.ToString()));
            diffuseNode.AppendChild(createNode(doc, "intensity", Runtime.difIntensity.ToString()));

            XmlNode specularNode = doc.CreateElement("specular");
            parentNode.AppendChild(specularNode);
            specularNode.AppendChild(createNode(doc, "enabled", Runtime.renderSpecular.ToString()));
            specularNode.AppendChild(createNode(doc, "intensity", Runtime.spcIntentensity.ToString()));

            XmlNode fresnelNode = doc.CreateElement("fresnel");
            parentNode.AppendChild(fresnelNode);
            fresnelNode.AppendChild(createNode(doc, "enabled", Runtime.renderFresnel.ToString()));
            fresnelNode.AppendChild(createNode(doc, "intensity", Runtime.frsIntensity.ToString()));

            XmlNode reflectionNode = doc.CreateElement("reflection");
            parentNode.AppendChild(reflectionNode);
            reflectionNode.AppendChild(createNode(doc, "enabled", Runtime.renderReflection.ToString()));
            reflectionNode.AppendChild(createNode(doc, "intensity", Runtime.refIntensity.ToString()));

            XmlNode ambientNode = doc.CreateElement("ambient");
            parentNode.AppendChild(ambientNode);
            ambientNode.AppendChild(createNode(doc, "intensity", Runtime.ambItensity.ToString()));
        }

        private static void AppendHitBoxHurtBoxRenderSettings(XmlDocument doc, XmlNode parentNode)
        {
            parentNode.AppendChild(createNode(doc, "render_ECB", Runtime.renderECB.ToString()));
            parentNode.AppendChild(createNode(doc, "render_hurtboxes", Runtime.renderHurtboxes.ToString()));
            parentNode.AppendChild(createNode(doc, "render_hurtboxes_zone", Runtime.renderHurtboxesZone.ToString()));
            parentNode.AppendChild(createNode(doc, "render_hitboxes", Runtime.renderHitboxes.ToString()));
            parentNode.AppendChild(createNode(doc, "render_interpolated_hitboxes", Runtime.renderInterpolatedHitboxes.ToString()));
            parentNode.AppendChild(createNode(doc, "render_hitboxes_no_overlap", Runtime.renderHitboxesNoOverlap.ToString()));
            parentNode.AppendChild(createNode(doc, "render_hitboxes_mode", Runtime.hitboxRenderMode.ToString()));
            parentNode.AppendChild(createNode(doc, "render_hitbox_angles", Runtime.renderHitboxAngles.ToString()));
            parentNode.AppendChild(createNode(doc, "render_special_bubbles", Runtime.renderSpecialBubbles.ToString()));
            parentNode.AppendChild(createNode(doc, "render_ledge_grabboxes", Runtime.renderLedgeGrabboxes.ToString()));
            parentNode.AppendChild(createNode(doc, "render_reverse_ledge_grabboxes", Runtime.renderReverseLedgeGrabboxes.ToString()));
            parentNode.AppendChild(createNode(doc, "render_tether_ledge_grabboxes", Runtime.renderTetherLedgeGrabboxes.ToString()));
            parentNode.AppendChild(createNode(doc, "hitbox_alpha", Runtime.hitboxAlpha.ToString()));
            parentNode.AppendChild(createNode(doc, "hurtbox_alpha", Runtime.hurtboxAlpha.ToString()));
            parentNode.AppendChild(createNode(doc, "hurtbox_color", ColorTranslator.ToHtml(Runtime.hurtboxColor)));
            parentNode.AppendChild(createNode(doc, "hurtbox_color_hi", ColorTranslator.ToHtml(Runtime.hurtboxColorHi)));
            parentNode.AppendChild(createNode(doc, "hurtbox_color_med", ColorTranslator.ToHtml(Runtime.hurtboxColorMed)));
            parentNode.AppendChild(createNode(doc, "hurtbox_color_low", ColorTranslator.ToHtml(Runtime.hurtboxColorLow)));
            parentNode.AppendChild(createNode(doc, "hurtbox_color_selected", ColorTranslator.ToHtml(Runtime.hurtboxColorSelected)));
            parentNode.AppendChild(createNode(doc, "hitbox_angles_color", ColorTranslator.ToHtml(Runtime.hitboxAnglesColor)));
            parentNode.AppendChild(createNode(doc, "windbox_color", ColorTranslator.ToHtml(Runtime.windboxColor)));
            parentNode.AppendChild(createNode(doc, "grabbox_color", ColorTranslator.ToHtml(Runtime.grabboxColor)));
            parentNode.AppendChild(createNode(doc, "searchbox_color", ColorTranslator.ToHtml(Runtime.searchboxColor)));
            parentNode.AppendChild(createNode(doc, "counterBubble_color", ColorTranslator.ToHtml(Runtime.counterBubbleColor)));
            parentNode.AppendChild(createNode(doc, "reflectBubble_color", ColorTranslator.ToHtml(Runtime.reflectBubbleColor)));
            parentNode.AppendChild(createNode(doc, "shieldBubble_color", ColorTranslator.ToHtml(Runtime.shieldBubbleColor)));
            parentNode.AppendChild(createNode(doc, "absorbBubble_color", ColorTranslator.ToHtml(Runtime.absorbBubbleColor)));
            parentNode.AppendChild(createNode(doc, "wtSlowdownBubble_color", ColorTranslator.ToHtml(Runtime.wtSlowdownBubbleColor)));
            parentNode.AppendChild(createNode(doc, "loadAndRenderATKD", Runtime.LoadAndRenderATKD.ToString()));

            XmlNode hitboxKbColorNode = doc.CreateElement("hitbox_kb_colors");
            parentNode.AppendChild(hitboxKbColorNode);
            foreach (Color color in Runtime.hitboxKnockbackColors)
                hitboxKbColorNode.AppendChild(createNode(doc, "color", ColorTranslator.ToHtml(color)));

            XmlNode hitboxIdColorNode = doc.CreateElement("hitbox_id_colors");
            parentNode.AppendChild(hitboxIdColorNode);
            foreach (Color color in Runtime.hitboxIdColors)
                hitboxIdColorNode.AppendChild(createNode(doc, "color", ColorTranslator.ToHtml(color)));
        }

        private static void AppendFloorSettings(XmlDocument doc, XmlNode parentNode)
        {
            XmlNode floorNode = doc.CreateElement("floor");
            parentNode.AppendChild(floorNode);
            floorNode.AppendChild(createNode(doc, "enabled", Runtime.renderFloor.ToString()));
            floorNode.AppendChild(createNode(doc, "style", Runtime.floorStyle.ToString()));
            floorNode.AppendChild(createNode(doc, "color", ColorTranslator.ToHtml(Runtime.floorColor)));
            floorNode.AppendChild(createNode(doc, "size", Runtime.floorSize.ToString()));
            if (Runtime.floorStyle == Runtime.FloorStyle.UserTexture)
                floorNode.AppendChild(createNode(doc, "texture", Runtime.floorTexFilePath));
        }

        private static void AppendLvdRenderSettings(XmlDocument doc, XmlNode parentNode)
        {
            XmlNode lvdRenderSettingsNode = doc.CreateElement("render_LVD");
            parentNode.AppendChild(lvdRenderSettingsNode);
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
            lvdRenderSettingsNode.AppendChild(createNode(doc, "reander_physicallyBasedRendering", Runtime.renderBfresPbr.ToString()));
        }

        private static void AppendDiscordSettings(XmlDocument doc, XmlNode parentNode)
        {
            XmlNode discordNode = doc.CreateElement("DISCORDSETTINGS");
            discordNode.AppendChild(createNode(doc, "discord_enabled", DiscordSettings.enabled.ToString()));
            discordNode.AppendChild(createNode(doc, "image_key_mode", ((int)DiscordSettings.imageKeyMode).ToString()));
            discordNode.AppendChild(createNode(doc, "user_image_key", DiscordSettings.userPickedImageKey));
            discordNode.AppendChild(createNode(doc, "use_user_mod_name", DiscordSettings.useUserModName.ToString()));
            discordNode.AppendChild(createNode(doc, "user_mod_name", DiscordSettings.userNamedMod));
            discordNode.AppendChild(createNode(doc, "show_current_window", DiscordSettings.showCurrentWindow.ToString()));
            discordNode.AppendChild(createNode(doc, "show_time_elapsed", DiscordSettings.showTimeElapsed.ToString()));
            parentNode.AppendChild(discordNode);
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

