using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.GLObjectManagement;
using SFGraphics.GLObjects.Textures;
using SFGraphics.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Timers;
using System.Windows.Forms;
using SmashForge.Filetypes.Models.Nuds;
using SmashForge.Rendering;
using SmashForge.Rendering.Meshes;
using WeifenLuo.WinFormsUI.Docking;

namespace SmashForge
{
    public partial class NudMaterialEditor : DockContent
    {
        public static Dictionary<int, string> cullModeByMatValue = new Dictionary<int, string>()
        {
            { 0x000, "Cull None"},
            { 0x404, "Cull Outside"},
            { 0x405, "Cull Inside"},

            { 0x001, "Cull None (Pokkén)"},
            { 0x003, "Cull Outside (Pokkén)"},
            { 0x002, "Cull Inside (Pokkén)"}
        };

        public static Dictionary<string, int> matValueByCullModeName = new Dictionary<string, int>()
        {
            { "Cull None", 0x000 },
            { "Cull Outside", 0x404 },
            { "Cull Inside", 0x405 },

            { "Cull None (Pokkén)", 0x001 },
            { "Cull Outside (Pokkén)", 0x003 },
            { "Cull Inside (Pokkén)", 0x002 }
        };

        public static Dictionary<int, string> alphaFuncByMatValue = new Dictionary<int, string>()
        {
            { 0x00, "Never"},
            { 0x04, "Gequal Ref Alpha"},
            { 0x06, "Gequal Ref Alpha + ???"}
        };

        public static Dictionary<int, string> mapModeByMatValue = new Dictionary<int, string>()
        {
            { 0x00, "TexCoord"},
            { 0x1D00, "EnvCamera"},
            { 0x1E00, "Projection"},
            { 0x1ECD, "EnvLight"},
            { 0x1F00, "EnvSpec"}
        };

        public static Dictionary<int, string> minFilterByMatValue = new Dictionary<int, string>()
        {
            { 0x00, "Linear_Mipmap_Linear"},
            { 0x01, "Nearest"},
            { 0x02, "Linear"},
            { 0x03, "Nearest_Mipmap_Linear"}
        };

        public static Dictionary<int, string> magFilterByMatValue = new Dictionary<int, string>()
        {
            { 0x00, "???"},
            { 0x01, "Nearest"},
            { 0x02, "Linear"}
        };

        public static Dictionary<int, string> wrapModeByMatValue = new Dictionary<int, string>()
        {
            { 0x01, "Repeat"},
            { 0x02, "Mirror"},
            { 0x03, "Clamp"}
        };

        public static Dictionary<int, string> mipDetailByMatValue = new Dictionary<int, string>()
        {
            { 0x01, "1 mip level, anisotropic off"},
            { 0x02, "1 mip level, anisotropic off 2"},
            { 0x03, "4 mip levels"},
            { 0x04, "4 mip levels, anisotropic"},
            { 0x05, "4 mip levels, trilinear"},
            { 0x06, "4 mip levels, trilinear, anisotropic"}
        };

        public static Dictionary<string, Params.MatParam> materialParamList = new Dictionary<string, Params.MatParam>();

        public Nud.Polygon currentPolygon;
        public List<Nud.Material> currentMaterialList;

        private int currentMatIndex = 0;
        private string currentPropertyName = "";
        private ImageList textureThumbnails = new ImageList()
        {
            ColorDepth = ColorDepth.Depth32Bit,
            ImageSize = new Size(64, 64)
        };

        // Set to false while using the sliders to avoid a loop of scroll and text changed events.
        // Set to true when focus on the slider is lost (ex. clicking on text box).
        private bool enableParam1SliderUpdates = true;
        private bool enableParam2SliderUpdates = true;
        private bool enableParam3SliderUpdates = true;
        private bool enableParam4SliderUpdates = true;

        private NudMaterialEditor()
        {
            InitializeComponent();
        }

        public NudMaterialEditor(Nud.Polygon p) : this()
        {
            currentPolygon = p;
            currentMaterialList = p.materials;
            Init();
            FillForm();
            ResizeGlControlsToMaxSquareSize(glControlTableLayout);
            matsComboBox.SelectedIndex = 0;
            texturesListView.LargeImageList = textureThumbnails;

            RefreshTexturesImageList();

            // The dummy textures will be used later. 
            OpenTkSharedResources.InitializeSharedResources();
            if (OpenTkSharedResources.SetupStatus == OpenTkSharedResources.SharedResourceStatus.Initialized)
            {
                // Only happens once.
                UpdateMaterialThumbnails();
            }
        }

        private void RefreshTexturesImageList()
        {
            textureThumbnails.Images.Clear();
            AddTextureThumbnails(textureThumbnails);
        }

        private void AddTextureThumbnails(ImageList imageList)
        {
            // Reuse the same context to avoid CPU bottlenecks.
            using (OpenTK.GameWindow gameWindow = OpenTkSharedResources.CreateGameWindowContext(64, 64))
            {
                Nud.Material mat = currentMaterialList[currentMatIndex];
                RenderMaterialTexturesAddToImageList(imageList, mat);
            }
        }

        private static void RenderMaterialTexturesAddToImageList(ImageList imageList, Nud.Material mat)
        {
            // Shaders weren't initialized.
            if (OpenTkSharedResources.SetupStatus != OpenTkSharedResources.SharedResourceStatus.Initialized)
                return;

            // Generate thumbnails for all textures in case the material's texture IDs are changed.
            foreach (NUT nut in Runtime.textureContainers)
            {
                foreach (var texture in nut.glTexByHashId)
                {
                    if (!(nut.glTexByHashId[texture.Key] is Texture2D))
                        continue;

                    Bitmap bitmap = TextureToBitmap.RenderBitmapUseExistingContext((Texture2D)nut.glTexByHashId[texture.Key], 64, 64);
                    imageList.Images.Add(texture.Key.ToString("X"), bitmap);

                    // StackOverflow makes the bad exceptions go away.
                    var dummy = imageList.Handle;
                    bitmap.Dispose();
                }      
            }
        }

        private static ImageList CreateImageList()
        {
            ImageList imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageSize = new Size(64, 64);
            return imageList;
        }

        private static void UpdateMaterialThumbnails()
        {
            // Update the material thumbnails.
            if (!Runtime.hasRefreshedMatThumbnails)
            {
                // If it didn't work the first time, it probably won't work again.
                MaterialPreviewRendering.RenderMaterialPresetPreviewsToFilesThreaded();
            }
        }

        public void InitMaterialParamList()
        {
            materialParamList = Params.MaterialParamTools.GetMatParamsFromFile();

            // Only allow for selecting known properties to add to the material.
            foreach (string s in materialParamList.Keys)
                matPropertyComboBox.Items.Add(s);
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if(texRgbGlControl != null)
                RenderTexture();
        }

        public void Init()
        {
            UpdateMatComboBox();

            texParamsTableLayout.Enabled = false;

            matPropertyComboBox.Items.Clear();

            if (materialParamList.Count == 0)
                InitMaterialParamList();

            // HACK: ???
            if (wrapXComboBox.Items.Count == 0)
            {               
                foreach (int i in cullModeByMatValue.Keys)
                    cullModeComboBox.Items.Add(cullModeByMatValue[i]);
                foreach (int i in alphaFuncByMatValue.Keys)
                    alphaFuncComboBox.Items.Add(alphaFuncByMatValue[i]);

                foreach (int i in wrapModeByMatValue.Keys)
                {
                    wrapXComboBox.Items.Add(wrapModeByMatValue[i]);
                    wrapYComboBox.Items.Add(wrapModeByMatValue[i]);
                }
                foreach (int i in mapModeByMatValue.Keys)
                    mapModeComboBox.Items.Add(mapModeByMatValue[i]);
                foreach (int i in minFilterByMatValue.Keys)
                    minFilterComboBox.Items.Add(minFilterByMatValue[i]);
                foreach (int i in magFilterByMatValue.Keys)
                    magFilterComboBox.Items.Add(magFilterByMatValue[i]);
                foreach (int i in mipDetailByMatValue.Keys)
                    mipDetailComboBox.Items.Add(mipDetailByMatValue[i]);
            }
        }

        private void UpdateMatComboBox()
        {
            matsComboBox.Items.Clear();
            for (int i = 0; i < currentMaterialList.Count; i++)
            {
                matsComboBox.Items.Add("Material_" + i);
            }
        }

        public void FillForm()
        {
            Nud.Material mat = currentMaterialList[currentMatIndex];

            InitializeComboBoxes(mat);
            InitializeTextBoxes(mat);
            InitializeCheckBoxes(mat);
            InitializeTextureListView(mat);
            InitializePropertiesListView(mat);
        }

        private void InitializeComboBoxes(Nud.Material mat)
        {
            alphaFuncComboBox.SelectedItem = alphaFuncByMatValue[mat.AlphaFunction];
            cullModeComboBox.SelectedItem = cullModeByMatValue[mat.CullMode];
        }

        private void InitializeCheckBoxes(Nud.Material mat)
        {
            shadowCB.Checked = mat.HasShadow;
            GlowCB.Checked = mat.Glow;

            alphaTestCB.Checked = mat.AlphaTest == (int)NudEnums.AlphaTest.Enabled;
            // Enable/disable extra controls.
            alphaTestCB_CheckedChanged(null, null);
        }

        private void InitializeTextBoxes(Nud.Material mat)
        {
            flagsTB.Text = mat.Flags.ToString("X");
            srcTB.Text = mat.SrcFactor + "";
            dstTB.Text = mat.DstFactor + "";
            refAlphaTB.Text = mat.RefAlpha + "";
            zBufferTB.Text = mat.ZBufferOffset + "";
        }

        private void InitializePropertiesListView(Nud.Material mat)
        {
            propertiesListView.Items.Clear();
            propertiesListView.View = View.List;
            foreach (string propertyName in mat.PropertyNames)
            {
                propertiesListView.Items.Add(propertyName);
            }

            // Select the first property.
            if (propertiesListView.Items.Count > 0)
                propertiesListView.SelectedIndices.Add(0);
        }

        private void InitializeTextureListView(Nud.Material mat)
        {
            texturesListView.Items.Clear();

            // Jigglypuff has weird eyes.
            if ((mat.Flags & 0xFFFFFFFF) == 0x9AE11163)
            {
                texturesListView.Items.Add("Diffuse", mat.Diffuse1Id.ToString("X"));
                texturesListView.Items.Add("Diffuse2", mat.Diffuse2Id.ToString("X"));
                texturesListView.Items.Add("NormalMap", mat.NormalId.ToString("X"));
            }
            else if ((mat.Flags & 0xFFFFFFFF) == 0x92F01101)
            {
                // These flags are even weirder. 
                texturesListView.Items.Add("Diffuse", mat.Diffuse1Id.ToString("X"));
                texturesListView.Items.Add("Diffuse2", mat.Diffuse2Id.ToString("X"));
                if (currentMatIndex == 0)
                {
                    // The second material doesn't have these textures.
                    // The texture are probably shared with the first material.
                    texturesListView.Items.Add("Ramp", mat.RampId.ToString("X"));
                    texturesListView.Items.Add("DummyRamp", mat.DummyRampId.ToString("X"));
                }
            }
            else
            {
                // The order of the textures is critical.
                if (mat.HasDiffuse)
                    texturesListView.Items.Add("Diffuse", mat.Diffuse1Id.ToString("X"));
                if (mat.HasSphereMap)
                    texturesListView.Items.Add("SphereMap", mat.SphereMapId.ToString("X"));
                if (mat.HasDiffuse2)
                    texturesListView.Items.Add("Diffuse2", mat.Diffuse2Id.ToString("X"));
                if (mat.HasDiffuse3)
                    texturesListView.Items.Add("Diffuse3", mat.Diffuse3Id.ToString("X"));
                if (mat.HasStageMap)
                    texturesListView.Items.Add("StageMap", mat.StageMapId.ToString("X"));
                if (mat.HasCubeMap)
                    texturesListView.Items.Add("Cubemap", mat.CubeMapId.ToString("X"));
                if (mat.HasAoMap)
                    texturesListView.Items.Add("AOMap", mat.AoMapId.ToString("X"));
                if (mat.HasNormalMap)
                    texturesListView.Items.Add("NormalMap", mat.NormalId.ToString("X"));
                if (mat.HasRamp)
                    texturesListView.Items.Add("Ramp", mat.RampId.ToString("X"));
                if (mat.HasDummyRamp)
                    texturesListView.Items.Add("DummyRamp", mat.DummyRampId.ToString("X"));
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentMatIndex = matsComboBox.SelectedIndex;
            RefreshTexturesImageList();
            FillForm();
        }

        private void srcTB_TextChanged(object sender, EventArgs e)
        {
            int value = GuiTools.TryParseTBInt(srcTB);
            if (value != -1)
                currentMaterialList[currentMatIndex].SrcFactor = value;
        }

        private void dstTB_TextChanged(object sender, EventArgs e)
        {
            int value = GuiTools.TryParseTBInt(dstTB);
            if (value != -1)
                currentMaterialList[currentMatIndex].DstFactor = value;
        }

        private void AlphaFuncCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in alphaFuncByMatValue.Keys)
            {
                if (alphaFuncByMatValue[i].Equals(alphaFuncComboBox.SelectedItem))
                {
                    currentMaterialList[currentMatIndex].AlphaFunction = i;
                    break;
                }
            }
        }
        
        private void texturesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = 0;
            if (texturesListView.SelectedItems.Count > 0)
            {
                index = texturesListView.Items.IndexOf(texturesListView.SelectedItems[0]);
                texParamsTableLayout.Enabled = true;
                textureIdTB.Enabled = true;
            }
            else
            {
                texParamsTableLayout.Enabled = false;
                textureIdTB.Enabled = false;
            }

            UpdateSelectedTextureControlValues(index);

            RenderTexture();
            RenderTexture(true);
        }

        private void UpdateSelectedTextureControlValues(int index)
        {
            Nud.MatTexture tex = currentMaterialList[currentMatIndex].textures[index];
            textureIdTB.Text = tex.hash.ToString("X");

            mapModeComboBox.SelectedItem = mapModeByMatValue[tex.mapMode];
            wrapXComboBox.SelectedItem = wrapModeByMatValue[tex.wrapModeS];
            wrapYComboBox.SelectedItem = wrapModeByMatValue[tex.wrapModeT];
            minFilterComboBox.SelectedItem = minFilterByMatValue[tex.minFilter];
            magFilterComboBox.SelectedItem = magFilterByMatValue[tex.magFilter];
            mipDetailComboBox.SelectedItem = mipDetailByMatValue[tex.mipDetail];
        }

        private void flagsTB_TextChanged(object sender, EventArgs e)
        {
            currentMaterialList[currentMatIndex].Flags = GuiTools.TryParseTBUint(flagsTB, true);

            // Clear the texture list to prevent displaying duplicates
            texturesListView.Clear();
            FillForm();
        }

        private void textureIdTB_TextChanged(object sender, EventArgs e)
        {
            int value = GuiTools.TryParseTBInt(textureIdTB, true);
            if (value != -1 && texturesListView.SelectedIndices.Count > 0)
                currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].hash = value;

            // Update the texture color channels.
            RenderTexture();
            RenderTexture(true);

            if (texturesListView.SelectedItems.Count > 0 && value != -1)
            {
                texturesListView.SelectedItems[0].ImageKey = value.ToString("X");
            }
        }

        private void refAlphaTB_TextChanged(object sender, EventArgs e)
        {
            int value = GuiTools.TryParseTBInt(refAlphaTB);
            if (value != -1)
                currentMaterialList[currentMatIndex].RefAlpha = value;
        }

        private void zBufferTB_TextChanged(object sender, EventArgs e)
        {
            int value = GuiTools.TryParseTBInt(zBufferTB);
            if (value != -1)
                currentMaterialList[currentMatIndex].ZBufferOffset = value;
        }

        private void mapModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (texturesListView.SelectedIndices.Count == 0)
                return;

            foreach (int i in mapModeByMatValue.Keys)
            {
                if (mapModeByMatValue[i].Equals(mapModeComboBox.SelectedItem))
                {
                    currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].mapMode = i;
                    break;
                }
            }
        }

        private void wrapXComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (texturesListView.SelectedIndices.Count == 0)
                return;

            foreach (int i in wrapModeByMatValue.Keys)
            {
                if (wrapModeByMatValue[i].Equals(wrapXComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].wrapModeS = i;
                    break;
                }
            }
        }

        private void wrapYComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (texturesListView.SelectedIndices.Count == 0)
                return;

            foreach (int i in wrapModeByMatValue.Keys)
            {
                if (wrapModeByMatValue[i].Equals(wrapYComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].wrapModeT = i;
                    break;
                }
            }
        }

        private void minFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (texturesListView.SelectedIndices.Count == 0)
                return;

            foreach (int i in minFilterByMatValue.Keys)
            {
                if (minFilterByMatValue[i].Equals(minFilterComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].minFilter = i;
                    break;
                }
            }
        }

        private void magFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (texturesListView.SelectedIndices.Count == 0)
                return;

            foreach (int i in magFilterByMatValue.Keys)
            {
                if (magFilterByMatValue[i].Equals(magFilterComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].magFilter = i;
                    break;
                }
            }
        }

        private void mipDetailComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (texturesListView.SelectedIndices.Count == 0)
                return;

            foreach (int i in mipDetailByMatValue.Keys)
            {
                if (mipDetailByMatValue[i].Equals(mipDetailComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].mipDetail = i;
                    break;
                }
            }
        }

        private void SetPropertyLabelText(string propertyName)
        {
            propertyNameLabel.Text = propertyName;
        }

        private void propertiesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (propertiesListView.SelectedIndices.Count == 0)
                return;

            // Try and find the name of the property that is selected in the listview.
            currentPropertyName = propertiesListView.SelectedItems[0].Text;
            Params.MatParam matParam;
            materialParamList.TryGetValue(currentPropertyName, out matParam);

            SetPropertyLabelText(currentPropertyName);
            SetParamTextBoxValues(currentPropertyName);
            SetParamLabelsAndToolTips(matParam);
        }

        private void propertiesListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.D)
            {
                Nud.Material mat = currentMaterialList[currentMatIndex];
                foreach (ListViewItem property in propertiesListView.SelectedItems)
                {
                    mat.RemoveProperty(property.Text);
                }
                FillForm();
                e.Handled = true;
            }
        }

        private void SetParamTextBoxValues(string propertyName)
        {
            if (propertyNameLabel.Text.Contains("NU_materialHash"))
            {
                int materialHash = BitConverter.ToInt32(BitConverter.GetBytes(currentMaterialList[currentMatIndex].GetPropertyValues(propertyName)[0]), 0);
                param1TB.Text = materialHash.ToString("X");
                param2TB.Text = currentMaterialList[currentMatIndex].GetPropertyValues(propertyName)[1] + "";
                param3TB.Text = currentMaterialList[currentMatIndex].GetPropertyValues(propertyName)[2] + "";
                param4TB.Text = currentMaterialList[currentMatIndex].GetPropertyValues(propertyName)[3] + "";
            }
            else
            {
                param1TB.Text = currentMaterialList[currentMatIndex].GetPropertyValues(propertyName)[0] + "";
                param2TB.Text = currentMaterialList[currentMatIndex].GetPropertyValues(propertyName)[1] + "";
                param3TB.Text = currentMaterialList[currentMatIndex].GetPropertyValues(propertyName)[2] + "";
                param4TB.Text = currentMaterialList[currentMatIndex].GetPropertyValues(propertyName)[3] + "";
            }
        }

        private static float GetMatParamMax(string propertyName, int index)
        {
            Params.MatParam labels = null;
            materialParamList.TryGetValue(propertyName, out labels);
            if (labels != null)
            {
                return labels.maxValues[index];
            }

            return 1;
        }

        private void param1TB_TextChanged(object sender, EventArgs e)
        {
            string propertyName = propertiesListView.SelectedItems[0].Text;
            if (propertyName == "NU_materialHash")
            {
                ParseMaterialHashTbText();
            }
            else
            {
                float value = GuiTools.TryParseTBFloat(param1TB);
                currentMaterialList[currentMatIndex].UpdateProperty(propertyName, value, 0);

                float max = GetMatParamMax(propertyName, 0);
                if (enableParam1SliderUpdates)
                    GuiTools.UpdateTrackBarFromValue(value, param1TrackBar, 0, max);
            }

            UpdatePropertyButtonColor();
        }

        private void ParseMaterialHashTbText()
        {
            int hash = GuiTools.TryParseTBInt(param1TB, true);
            if (hash != -1 && propertiesListView.SelectedItems.Count > 0)
                currentMaterialList[currentMatIndex].UpdateProperty(propertiesListView.SelectedItems[0].Text, BitConverter.ToSingle(BitConverter.GetBytes(hash), 0), 0);
        }

        private void param2TB_TextChanged(object sender, EventArgs e)
        {
            string propertyName = propertiesListView.SelectedItems[0].Text;
            float value = GuiTools.TryParseTBFloat(param2TB);
            currentMaterialList[currentMatIndex].UpdateProperty(propertyName, value, 1);

            float max = GetMatParamMax(propertyName, 1);
            if (enableParam2SliderUpdates)
                GuiTools.UpdateTrackBarFromValue(value, param2TrackBar, 0, max);

            UpdatePropertyButtonColor();
        }

        private void param3TB_TextChanged(object sender, EventArgs e)
        {
            string propertyName = propertiesListView.SelectedItems[0].Text;
            float value = GuiTools.TryParseTBFloat(param3TB);
            currentMaterialList[currentMatIndex].UpdateProperty(propertyName, value, 2);

            float max = GetMatParamMax(propertyName, 2);
            if (enableParam3SliderUpdates)
                GuiTools.UpdateTrackBarFromValue(value, param3TrackBar, 0, max);

            UpdatePropertyButtonColor();
        }

        private void param4TB_TextChanged(object sender, EventArgs e)
        {
            string propertyName = propertiesListView.SelectedItems[0].Text;
            float value = GuiTools.TryParseTBFloat(param4TB);
            currentMaterialList[currentMatIndex].UpdateProperty(propertyName, value, 3);

            float max = GetMatParamMax(propertyName, 3);
            if (enableParam4SliderUpdates)
                GuiTools.UpdateTrackBarFromValue(value, param4TrackBar, 0, max);

            UpdatePropertyButtonColor();
        }

        private void SetParamLabelsAndToolTips(Params.MatParam matParam)
        {
            if (matParam != null)
            {
                toolTip1.SetToolTip(propertyNameLabel, matParam.generalDescription);
                toolTip1.SetToolTip(param1Label, matParam.param1Description);
                toolTip1.SetToolTip(param2Label, matParam.param2Description);
                toolTip1.SetToolTip(param3Label, matParam.param3Description);
                toolTip1.SetToolTip(param4Label, matParam.param4Description);

                param1Label.Text = matParam.paramLabels[0].Equals("") ? "Param1" : matParam.paramLabels[0];
                param2Label.Text = matParam.paramLabels[1].Equals("") ? "Param2" : matParam.paramLabels[1];
                param3Label.Text = matParam.paramLabels[2].Equals("") ? "Param3" : matParam.paramLabels[2];
                param4Label.Text = matParam.paramLabels[3].Equals("") ? "Param4" : matParam.paramLabels[3];

                // Not all material properties need a trackbar (Ex: NU_materialHash).
                param1TrackBar.Enabled = matParam.useTrackBar;
                param2TrackBar.Enabled = matParam.useTrackBar;
                param3TrackBar.Enabled = matParam.useTrackBar;
                param4TrackBar.Enabled = matParam.useTrackBar;
            }
            else
            {
                param1Label.Text = "Param1";
                param2Label.Text = "Param2";
                param3Label.Text = "Param3";
                param4Label.Text = "Param4";
            }
        }

        private void matPropertyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Prevent adding duplicate material properties.
            addMatPropertyButton.Enabled = true;
            if (currentMaterialList[currentMatIndex].HasProperty(matPropertyComboBox.Text))
            {
                addMatPropertyButton.Enabled = false;
            }
        }

        private void addMatPropertyButton_Click(object sender, EventArgs e)
        {
            AddPropertyFromComboBox();
        }

        private void deleteMatPropertyButton_Click(object sender, EventArgs e)
        {
            RemoveSelectedProperty();
        }

        private void AddPropertyFromComboBox()
        {
            if (!matPropertyComboBox.Text.Equals(""))
            {
                currentMaterialList[currentMatIndex].UpdateProperty(matPropertyComboBox.Text, new float[] { 0, 0, 0, 0 });
                FillForm();
                addMatPropertyButton.Enabled = false;
            }
        }

        private void RemoveSelectedProperty()
        {
            // Check if the property exists first.
            string propertyName = propertiesListView.SelectedItems[0].Text;
            if (currentMaterialList[currentMatIndex].HasProperty(propertyName))
            {
                currentMaterialList[currentMatIndex].RemoveProperty(propertyName);
                FillForm();
                addMatPropertyButton.Enabled = true; // The property can be added again.
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (currentMaterialList[currentMatIndex].textures.Count < 4)
            {
                currentMaterialList[currentMatIndex].textures.Add(Nud.MatTexture.GetDefault());
                FillForm();
            }
        }

        private void loadPresetButton_Click(object sender, EventArgs e)
        {
            MaterialSelector matSelector = new MaterialSelector();
            matSelector.ShowDialog();
            if (matSelector.exitStatus == MaterialSelector.ExitStatus.Opened)
            {
                List<Nud.Material> presetMaterials = ReadMaterialListFromPreset(matSelector.path);

                // Store the original material to preserve Tex IDs. 
                Nud.Material original = currentPolygon.materials[0].Clone();
                currentPolygon.materials = presetMaterials;

                // Copy the old Tex IDs. 
                currentPolygon.materials[0].CopyTextureIds(original);

                currentMaterialList = currentPolygon.materials;
                currentMatIndex = 0;
                Init();
                FillForm();
            }
        }

        public static List<Nud.Material> ReadMaterialListFromPreset(string file)
        {
            FileData matFile = new FileData(file);
            int soff = matFile.ReadInt();

            Nud.PolyData pol = new Nud.PolyData()
            {
                texprop1 = matFile.ReadInt(),
                texprop2 = matFile.ReadInt(),
                texprop3 = matFile.ReadInt(),
                texprop4 = matFile.ReadInt()
            };

            List<Nud.Material> presetMaterials = Nud.ReadMaterials(matFile, pol, soff);
            return presetMaterials;
        }

        private void RenderTexture(bool justRenderAlpha = false)
        {
            if (OpenTkSharedResources.SetupStatus != OpenTkSharedResources.SharedResourceStatus.Initialized)
                return;

            if (!tabControl1.SelectedTab.Text.Equals("Textures"))
                return;

            if (!OpenTkSharedResources.shaders["Texture"].LinkStatusIsOk)
                return;

            // Get the selected NUT texture.
            NutTexture nutTexture = null;
            Texture displayTexture = null;
            if (currentMaterialList[currentMatIndex].HasProperty("NU_materialHash") && texturesListView.SelectedIndices.Count > 0)
            {
                int hash = currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].hash;

                // Display dummy textures from resources. 
                if (Enum.IsDefined(typeof(NudEnums.DummyTexture), hash))
                    displayTexture = RenderTools.dummyTextures[(NudEnums.DummyTexture)hash];
                else
                {
                    foreach (NUT n in Runtime.textureContainers)
                    {
                        if (n.glTexByHashId.ContainsKey(hash))
                        {
                            n.getTextureByID(hash, out nutTexture);
                            displayTexture = n.glTexByHashId[hash];
                            break;
                        }
                    }
                }
            }

            // We can't share these vaos across both contexts.
            if (justRenderAlpha)
            {
                texAlphaGlControl.MakeCurrent();

                Mesh3D screenTriangle = ScreenDrawing.CreateScreenTriangle();

                GL.Viewport(texAlphaGlControl.ClientRectangle);
                if (displayTexture != null)
                    ScreenDrawing.DrawTexturedQuad(displayTexture, 1, 1, screenTriangle, false, false, false, true);
                texAlphaGlControl.SwapBuffers();
            }
            else
            {
                texRgbGlControl.MakeCurrent();

                Mesh3D screenTriangle = ScreenDrawing.CreateScreenTriangle();

                GL.Viewport(texRgbGlControl.ClientRectangle);
                if (displayTexture != null)
                    ScreenDrawing.DrawTexturedQuad(displayTexture, 1, 1, screenTriangle);
                texRgbGlControl.SwapBuffers();
            }
        }

        private void NUDMaterialEditor_Scroll(object sender, ScrollEventArgs e)
        {
            RenderTexture();
        }

        private void UpdatePropertyButtonColor()
        {
            try
            {
                string selectedMatPropKey = propertiesListView.SelectedItems[0].Text;
                float[] values = currentMaterialList[currentMatIndex].GetPropertyValues(selectedMatPropKey);
                colorSelect.BackColor = ColorUtils.GetColor(values[0], values[1], values[2]);
            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }

        private void colorSelect_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = colorSelect.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                // Convert the colors to float.
                param1TB.Text = colorDialog1.Color.R / 255f + "";
                param2TB.Text = colorDialog1.Color.G / 255f + "";
                param3TB.Text = colorDialog1.Color.B / 255f + "";
            }
        }

        private void sphereMapCB_CheckedChanged(object sender, EventArgs e)
        {
            currentMaterialList[currentMatIndex].HasSphereMap = sphereMapCB.Checked;
            FillForm();
        }

        private void shadowCB_CheckedChanged(object sender, EventArgs e)
        {
            currentMaterialList[currentMatIndex].HasShadow = shadowCB.Checked;
            FillForm();
        }

        private void GlowCB_CheckedChanged(object sender, EventArgs e)
        {
            currentMaterialList[currentMatIndex].Glow = GlowCB.Checked;
            FillForm();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RenderTexture();
            FillForm();

            // Nothing is currently selected, so don't display the tex ID.
            textureIdTB.Text = "";
        }

        private void texRgbGlControl_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture();
            GLObjectManager.DeleteUnusedGLObjects();
        }

        private void texAlphaGlControl_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture(true);
            GLObjectManager.DeleteUnusedGLObjects();
        }

        private void param1TrackBar_Scroll(object sender, EventArgs e)
        {
            Params.MatParam labels = null;
            materialParamList.TryGetValue(currentPropertyName, out labels);
            
            if (labels != null)
            {
                enableParam1SliderUpdates = false;
                param1TB.Text = GuiTools.GetTrackBarValue(param1TrackBar, 0, labels.maxValues[0]).ToString();
            }
        }

        private void param2TrackBar_Scroll(object sender, EventArgs e)
        {
            Params.MatParam labels = null;
            materialParamList.TryGetValue(currentPropertyName, out labels);

            if (labels != null)
            {
                enableParam2SliderUpdates = false;
                param2TB.Text = GuiTools.GetTrackBarValue(param2TrackBar, 0, labels.maxValues[1]).ToString();
            }
        }

        private void param3TrackBar_Scroll(object sender, EventArgs e)
        {
            Params.MatParam labels = null;
            materialParamList.TryGetValue(currentPropertyName, out labels);

            if (labels != null)
            {
                enableParam3SliderUpdates = false;
                param3TB.Text = GuiTools.GetTrackBarValue(param3TrackBar, 0, labels.maxValues[2]).ToString();
            }
        }

        private void param4TrackBar_Scroll(object sender, EventArgs e)
        {
            Params.MatParam labels = null;
            materialParamList.TryGetValue(currentPropertyName, out labels);

            if (labels != null)
            {
                enableParam4SliderUpdates = false;
                param4TB.Text = GuiTools.GetTrackBarValue(param4TrackBar, 0, labels.maxValues[3]).ToString();
            }
        }

        private void addMaterialButton_Click(object sender, EventArgs e)
        {
            // Can only have two materials.
            if (currentMaterialList.Count < 2)
            {
                currentMaterialList.Add(Nud.Material.GetDefault());
                currentMatIndex = 1;
                FillForm();
                UpdateMatComboBox();
            }
        }

        private void deleteMaterialButton_Click(object sender, EventArgs e)
        {
            // Don't allow removing all materials.
            if (currentMaterialList.Count > 1)
            {
                currentMaterialList.RemoveAt(matsComboBox.SelectedIndex);
                currentMatIndex = 0; // The last material has been removed.
                FillForm();
                UpdateMatComboBox();
            }
        }

        private void param1TrackBar_Leave(object sender, EventArgs e)
        {
            enableParam1SliderUpdates = true;
        }

        private void param2TrackBar_Leave(object sender, EventArgs e)
        {
            enableParam2SliderUpdates = true;
        }

        private void param3TrackBar_Leave(object sender, EventArgs e)
        {
            enableParam3SliderUpdates = true;
        }

        private void param4TrackBar_Leave(object sender, EventArgs e)
        {
            enableParam4SliderUpdates = true;
        }

        private void alphaTestCB_CheckedChanged(object sender, EventArgs e)
        {
            if (alphaTestCB.Checked)
                currentMaterialList[currentMatIndex].AlphaTest = (int)NudEnums.AlphaTest.Enabled;
            else
                currentMaterialList[currentMatIndex].AlphaTest = (int)NudEnums.AlphaTest.Disabled;

            // Only enable extra settings when alpha testing is enabled.
            alphaFuncRefPanel.Visible = alphaTestCB.Checked;

            // Force all flow layouts to rescale children.
            GuiTools.ScaleControlsHorizontallyToLayoutWidth(generalFlowLayout);
        }

        private void flagsButton_Click(object sender, EventArgs e)
        {
            flagsPanel.Visible = !flagsPanel.Visible;
        }

        private void alphaTestButton_Click(object sender, EventArgs e)
        {
            alphaTestPanel.Visible = !alphaTestPanel.Visible;
        }

        private void alphaBlendButton_Click(object sender, EventArgs e)
        {
            alphaBlendPanel.Visible = !alphaBlendPanel.Visible;
        }

        private void miscButton_Click(object sender, EventArgs e)
        {
            miscPanel.Visible = !miscPanel.Visible;
        }

        private void flowLayout_Resize(object sender, EventArgs e)
        {
            GuiTools.ScaleControlsHorizontallyToLayoutWidth((FlowLayoutPanel)sender);
        }

        private void savePresetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Namco Material (NMT)|*.nmt";
                sfd.InitialDirectory = Path.Combine(MainForm.executableDir, "materials\\");

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    sfd.FileName = sfd.FileName;
                    sfd.RestoreDirectory = true;

                    if (sfd.FileName.EndsWith(".nmt"))
                        SaveMaterialToFile(sfd.FileName);
                }
            }
        }

        private void SaveMaterialToFile(string fileName)
        {
            FileOutput m = new FileOutput();
            FileOutput s = new FileOutput();

            int[] c = Nud.WriteMaterial(m, currentMaterialList, s);

            FileOutput fin = new FileOutput();

            fin.WriteInt(0);

            fin.WriteInt(20 + c[0]);
            for (int i = 1; i < 4; i++)
            {
                fin.WriteInt(c[i] == c[i - 1] ? 0 : 20 + c[i]);
            }

            for (int i = 0; i < 4 - c.Length; i++)
                fin.WriteInt(0);

            fin.WriteOutput(m);
            fin.Align(32, 0xFF);
            fin.WriteIntAt(fin.Size(), 0);
            fin.WriteOutput(s);
            fin.Save(fileName);
        }

        private void cullModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Nud.Material mat = currentMaterialList[currentMatIndex];
            if (matValueByCullModeName.ContainsKey(cullModeComboBox.SelectedItem.ToString()))
                mat.CullMode = matValueByCullModeName[cullModeComboBox.SelectedItem.ToString()];
        }

        private void glControlTableLayout_Resize(object sender, EventArgs e)
        {
            ResizeGlControlsToMaxSquareSize(glControlTableLayout);
        }

        private void ResizeGlControlsToMaxSquareSize(Control container)
        {
            // Scale the glControls to the maximum size that still preserves the square aspect ratio.
            int sideLength = Math.Min(container.Width / 2, container.Height);
            texRgbGlControl.Width = sideLength;
            texRgbGlControl.Height = sideLength;
            texAlphaGlControl.Width = sideLength;
            texAlphaGlControl.Height = sideLength;
        }

        private void texturesListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Gui.Menus.TextureSelector textureSelector = new Gui.Menus.TextureSelector();
            textureSelector.ShowDialog();

            // Updating the text box will update the material texture.
            if (textureSelector.TextureId != -1)
            {
                textureIdTB.Text = textureSelector.TextureId.ToString("X");
            }
        }
    }
}
