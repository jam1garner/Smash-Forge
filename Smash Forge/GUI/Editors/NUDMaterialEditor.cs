using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;
using OpenTK.Graphics.OpenGL;
using System.Timers;
using System.Diagnostics;
using Smash_Forge.Rendering;
using SFGraphics.Tools;


namespace Smash_Forge
{
    public partial class NUDMaterialEditor : DockContent
    {
        public NUD.Polygon currentPolygon;
        public List<NUD.Material> currentMaterialList;
        int currentMatIndex = 0;
        string currentPropertyName = "";

        public static Dictionary<string, Params.MatParam> materialParamList = new Dictionary<string, Params.MatParam>();

        // Set to false while using the sliders to avoid a loop of scroll and text changed events.
        // Set to true when focus on the slider is lost (ex. clicking on text box).
        bool enableParam1SliderUpdates = true;
        bool enableParam2SliderUpdates = true;
        bool enableParam3SliderUpdates = true;
        bool enableParam4SliderUpdates = true;

        public static Dictionary<int, string> dstFactor = new Dictionary<int, string>()
        {
            { 0x00, "Nothing"},
            { 0x01, "SourceAlpha"},
            { 0x02, "One"},
            { 0x03, "InverseSourceAlpha + SubtractTrue"},
            { 0x04, "Dummy"},
        };

        public static Dictionary<int, string> srcFactor = new Dictionary<int, string>()
        {
            { 0x00, "Nothing"},
            { 0x01, "SourceAlpha + CompareBeforeTextureFalse + DepthTestTrue + EnableDepthUpdateTrue"},
            { 0x04, "RasterAlpha + CompareBeforeTextureTrue + DepthTestTrue + EnableDepthUpdateFalse"},
            { 0x32, "SourceAlpha + CompareBeforeTextureTrue + DepthTestFalse + EnableDepthUpdateFalse + MultiplyBy2"},
            { 0x33, "SourceAlpha + CompareBeforeTextureTrue + DepthTestFalse + EnableDepthUpdateFalse + MultiplyBy1"}
        };

        public static Dictionary<int, string> cullModeByMatValue = new Dictionary<int, string>()
        {
            { 0x000, "Cull None"},
            { 0x404, "Cull Outside"},
            { 0x405, "Cull Inside"}
        };

        public static Dictionary<string, int> matValueByCullModeName = new Dictionary<string, int>()
        {
            { "Cull None", 0x000 },
            { "Cull Outside", 0x404 },
            { "Cull Inside", 0x405 }
        };

        public static Dictionary<int, string> alphaTestByMatValue = new Dictionary<int, string>()
        {
            { 0x00, "Alpha Test Disabled"},
            { 0x02, "Alpha Test Enabled"},
        };

        public static Dictionary<int, string> alphaFuncByMatValue = new Dictionary<int, string>()
        {
            { 0x00, "Never"},
            { 0x04, "Lequal Ref Alpha + ??"},
            { 0x06, "Lequal Ref Alpha + ???"}
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
            { 0x03, "4 mip levels, trilinear off, anisotropic off"},
            { 0x04, "4 mip levels, trilinear off, anisotropic on"},
            { 0x05, "4 mip levels, trilinear on, anisotropic off"},
            { 0x06, "4 mip levels, trilinear on, anisotropic on"}
        };

        public NUDMaterialEditor()
        {
            InitializeComponent();
        }

        public NUDMaterialEditor(NUD.Polygon p) : this()
        {
            currentPolygon = p;
            currentMaterialList = p.materials;
            Init();
            FillForm();
            matsComboBox.SelectedIndex = 0;

            // The dummy textures will be used later. 
            RenderTools.SetUpOpenTkRendering();

            // Only happens once.
            UpdateMaterialThumbnails();
        }

        private static void UpdateMaterialThumbnails()
        {
            // Update the material thumbnails.
            if (!Runtime.hasRefreshedMatThumbnails && Runtime.shaders["NudSphere"].ProgramCreatedSuccessfully())
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

            tableLayoutPanel2.Enabled = false;

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
            NUD.Material mat = currentMaterialList[currentMatIndex];

            InitializeComboBoxes(mat);
            InitializeTextBoxes(mat);
            InitializeCheckBoxes(mat);
            InitializeTextureListView(mat);
            InitializePropertiesListView(mat);
        }

        private void InitializeComboBoxes(NUD.Material mat)
        {
            alphaFuncComboBox.SelectedItem = alphaFuncByMatValue[mat.alphaFunction];
            cullModeComboBox.SelectedItem = cullModeByMatValue[mat.cullMode];
        }

        private void InitializeCheckBoxes(NUD.Material mat)
        {
            shadowCB.Checked = mat.hasShadow;
            GlowCB.Checked = mat.glow;

            alphaTestCB.Checked = mat.alphaTest == 0x2;
            // Enable/disable extra controls.
            alphaTestCB_CheckedChanged(null, null);
        }

        private void InitializeTextBoxes(NUD.Material mat)
        {
            flagsTB.Text = mat.Flags.ToString("X");
            srcTB.Text = mat.srcFactor + "";
            dstTB.Text = mat.dstFactor + "";
            refAlphaTB.Text = mat.RefAlpha + "";
            zBufferTB.Text = mat.zBufferOffset + "";
        }

        private void InitializePropertiesListView(NUD.Material mat)
        {
            propertiesListView.Items.Clear();
            propertiesListView.View = View.List;
            foreach (string propertyName in mat.entries.Keys)
            {
                propertiesListView.Items.Add(propertyName);
            }

            // Select the first property.
            if (propertiesListView.Items.Count > 0)
                propertiesListView.SelectedIndices.Add(0);
        }

        private void InitializeTextureListView(NUD.Material mat)
        {
            texturesListView.Items.Clear();

            // Jigglypuff has weird eyes.
            if ((mat.Flags & 0xFFFFFFFF) == 0x9AE11163)
            {
                texturesListView.Items.Add("Diffuse");
                texturesListView.Items.Add("Diffuse2");
                texturesListView.Items.Add("NormalMap");
            }
            else if ((mat.Flags & 0xFFFFFFFF) == 0x92F01101)
            {
                // These flags are even weirder. 
                texturesListView.Items.Add("Diffuse");
                texturesListView.Items.Add("Diffuse2");
                if (currentMatIndex == 0)
                {
                    // The second material doesn't have these textures.
                    // The texture are probably shared with the first material.
                    texturesListView.Items.Add("Ramp");
                    texturesListView.Items.Add("DummyRamp");
                }
            }
            else
            {
                // The order of the textures is critical.
                if (mat.hasDiffuse)
                    texturesListView.Items.Add("Diffuse");
                if (mat.hasSphereMap)
                    texturesListView.Items.Add("SphereMap");
                if (mat.hasDiffuse2)
                    texturesListView.Items.Add("Diffuse2");
                if (mat.hasDiffuse3)
                    texturesListView.Items.Add("Diffuse3");
                if (mat.hasStageMap)
                    texturesListView.Items.Add("StageMap");
                if (mat.hasCubeMap)
                    texturesListView.Items.Add("Cubemap");
                if (mat.hasAoMap)
                    texturesListView.Items.Add("AO Map");
                if (mat.hasNormalMap)
                    texturesListView.Items.Add("NormalMap");
                if (mat.hasRamp)
                    texturesListView.Items.Add("Ramp");
                if (mat.hasDummyRamp)
                    texturesListView.Items.Add("Dummy Ramp");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentMatIndex = matsComboBox.SelectedIndex;
            FillForm();
        }

        private void srcTB_TextChanged(object sender, EventArgs e)
        {
            int value = GuiTools.TryParseTBInt(srcTB);
            if (value != -1)
                currentMaterialList[currentMatIndex].srcFactor = value;
        }

        private void dstTB_TextChanged(object sender, EventArgs e)
        {
            int value = GuiTools.TryParseTBInt(dstTB);
            if (value != -1)
                currentMaterialList[currentMatIndex].dstFactor = value;
        }

        public void SetValue(TextBox textBox, ComboBox combobox, Dictionary<int, string> dict, out int materialValue)
        {
            materialValue = -1;
            int.TryParse(textBox.Text, out materialValue);
            if (materialValue != -1)
            {
                string descriptionKey = "";
                dict.TryGetValue(materialValue, out descriptionKey);
                if (descriptionKey != "")
                    combobox.Text = descriptionKey;
            }
            else
                textBox.Text = "0";
        }

        private void cullModeComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {

        }

        private void AlphaFuncCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in alphaFuncByMatValue.Keys)
            {
                if (alphaFuncByMatValue[i].Equals(alphaFuncComboBox.SelectedItem))
                {
                    currentMaterialList[currentMatIndex].alphaFunction = i;
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
                tableLayoutPanel2.Enabled = true;
                textureIDTB.Enabled = true;
            }
            else
            {
                tableLayoutPanel2.Enabled = false;
                textureIDTB.Enabled = false;
            }
            if(index >= currentMaterialList[currentMatIndex].textures.Count)
            {
                MessageBox.Show("Texture doesn't exist");
                return;
            }
            NUD.MatTexture tex = currentMaterialList[currentMatIndex].textures[index];
            textureIDTB.Text = tex.hash.ToString("X");

            mapModeComboBox.SelectedItem = mapModeByMatValue[tex.mapMode];
            wrapXComboBox.SelectedItem = wrapModeByMatValue[tex.wrapModeS];
            wrapYComboBox.SelectedItem = wrapModeByMatValue[tex.wrapModeT];
            minFilterComboBox.SelectedItem = minFilterByMatValue[tex.minFilter];
            magFilterComboBox.SelectedItem = magFilterByMatValue[tex.magFilter];
            mipDetailComboBox.SelectedItem = mipDetailByMatValue[tex.mipDetail];
            RenderTexture();
            RenderTexture(true);
        }
        private void flagsTB_TextChanged(object sender, EventArgs e)
        {
            uint f = 0;
            if (uint.TryParse(flagsTB.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out f))// && listView1.SelectedIndices.Count > 0
            {
                currentMaterialList[currentMatIndex].Flags = f;
                flagsTB.BackColor = Color.White;

                // Clear the texture list to prevent displaying duplicates
                texturesListView.Clear();
                FillForm();
            }
            else
                flagsTB.BackColor = Color.Red;
        }

        private void textureIDTB_TextChanged(object sender, EventArgs e)
        {
            int f = -1;
            int.TryParse(textureIDTB.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out f);
            if (f != -1 && texturesListView.SelectedIndices.Count > 0)
                currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].hash = f;

            // Update the texture color channels.
            RenderTexture();
            RenderTexture(true);
        }

        private void refAlphaTB_TextChanged(object sender, EventArgs e)
        {
            int n = -1;
            int.TryParse(refAlphaTB.Text, out n);
            if (n != -1)
            {
                currentMaterialList[currentMatIndex].RefAlpha = n;
            }
            else
            {
                refAlphaTB.Text = "0";
            }
        }

        private void zBufferTB_TextChanged(object sender, EventArgs e)
        {
            int n = -1;
            int.TryParse(zBufferTB.Text, out n);
            if (n != -1)
            {
                currentMaterialList[currentMatIndex].zBufferOffset = n;
            }
            else
            {
                zBufferTB.Text = "0";
            }
        }

        private void mapModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in mapModeByMatValue.Keys)
                if (mapModeByMatValue[i].Equals(mapModeComboBox.SelectedItem))
                {
                    currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].mapMode = i;
                    break;
                }
        }

        private void wrapXComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in wrapModeByMatValue.Keys)
                if (wrapModeByMatValue[i].Equals(wrapXComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].wrapModeS = i;
                    break;
                }
        }

        private void wrapYComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in wrapModeByMatValue.Keys)
                if (wrapModeByMatValue[i].Equals(wrapYComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].wrapModeT = i;
                    break;
                }
        }

        private void minFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in minFilterByMatValue.Keys)
                if (minFilterByMatValue[i].Equals(minFilterComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].minFilter = i;
                    break;
                }
        }

        private void comboBox12_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in magFilterByMatValue.Keys)
                if (magFilterByMatValue[i].Equals(magFilterComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].magFilter = i;
                    break;
                }
        }

        private void comboBox13_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in mipDetailByMatValue.Keys)
                if (mipDetailByMatValue[i].Equals(mipDetailComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].mipDetail = i;
                    break;
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
            if (e.KeyCode == Keys.Delete)
            {
                NUD.Material mat = currentMaterialList[currentMatIndex];
                foreach (ListViewItem property in propertiesListView.SelectedItems)
                {
                    mat.entries.Remove(property.Text);
                }
                FillForm();
                e.Handled = true;
            }
        }

        private void SetParamTextBoxValues(string propertyName)
        {
            if (propertyNameLabel.Text.Contains("NU_materialHash"))
            {
                int materialHash = BitConverter.ToInt32(BitConverter.GetBytes(currentMaterialList[currentMatIndex].entries[propertyName][0]), 0);
                param1TB.Text = materialHash.ToString("X");
                param2TB.Text = currentMaterialList[currentMatIndex].entries[propertyName][1] + "";
                param3TB.Text = currentMaterialList[currentMatIndex].entries[propertyName][2] + "";
                param4TB.Text = currentMaterialList[currentMatIndex].entries[propertyName][3] + "";
            }
            else
            {
                param1TB.Text = currentMaterialList[currentMatIndex].entries[propertyName][0] + "";
                param2TB.Text = currentMaterialList[currentMatIndex].entries[propertyName][1] + "";
                param3TB.Text = currentMaterialList[currentMatIndex].entries[propertyName][2] + "";
                param4TB.Text = currentMaterialList[currentMatIndex].entries[propertyName][3] + "";
            }
        }

        private static float GetMatParamMax(string propertyName)
        {
            Params.MatParam labels = null;
            materialParamList.TryGetValue(propertyName, out labels);
            if (labels != null)
            {
                return labels.max1;
            }

            return 1;
        }

        private void param1TB_TextChanged(object sender, EventArgs e)
        {
            string propertyName = propertiesListView.SelectedItems[0].Text;
            if (propertyName == "NU_materialHash")
            {
                ParseMaterialHashTBText();
            }
            else
            {
                float value = GuiTools.TryParseTBFloat(param1TB);
                currentMaterialList[currentMatIndex].entries[propertyName][0] = value;

                float max = GetMatParamMax(propertyName);
                if (enableParam1SliderUpdates)
                    GuiTools.UpdateTrackBarFromValue(value, param1TrackBar, 0, max);
            }

            UpdateButtonColor();
        }

        private void ParseMaterialHashTBText()
        {
            int hash = GuiTools.TryParseTBInt(param1TB, true);
            if (hash != -1 && propertiesListView.SelectedItems.Count > 0)
                currentMaterialList[currentMatIndex].entries[propertiesListView.SelectedItems[0].Text][0] = BitConverter.ToSingle(BitConverter.GetBytes(hash), 0);
        }

        private void param2TB_TextChanged(object sender, EventArgs e)
        {
            string propertyName = propertiesListView.SelectedItems[0].Text;
            float value = GuiTools.TryParseTBFloat(param2TB);
            currentMaterialList[currentMatIndex].entries[propertyName][1] = value;

            float max = GetMatParamMax(propertyName);
            if (enableParam2SliderUpdates)
                GuiTools.UpdateTrackBarFromValue(value, param2TrackBar, 0, max);

            UpdateButtonColor();
        }

        private void param3TB_TextChanged(object sender, EventArgs e)
        {
            string propertyName = propertiesListView.SelectedItems[0].Text;
            float value = GuiTools.TryParseTBFloat(param3TB);
            currentMaterialList[currentMatIndex].entries[propertyName][2] = value;

            float max = GetMatParamMax(propertyName);
            if (enableParam3SliderUpdates)
                GuiTools.UpdateTrackBarFromValue(value, param3TrackBar, 0, max);

            UpdateButtonColor();
        }

        private void param4TB_TextChanged(object sender, EventArgs e)
        {
            string propertyName = propertiesListView.SelectedItems[0].Text;
            float value = GuiTools.TryParseTBFloat(param4TB);
            currentMaterialList[currentMatIndex].entries[propertyName][3] = value;

            float max = GetMatParamMax(propertyName);
            if (enableParam4SliderUpdates)
                GuiTools.UpdateTrackBarFromValue(value, param4TrackBar, 0, max);

            UpdateButtonColor();
        }

        // property change
        private void matPropertyTB_TextChanged(object sender, EventArgs e)
        {
            Params.MatParam matParam = null;

            // Try and find the name of the property that is selected in the listview.
            string propertyName = "";
            if (propertiesListView.SelectedIndices.Count > 0)
                propertyName = currentMaterialList[currentMatIndex].entries.Keys.ElementAt(propertiesListView.SelectedIndices[0]);

            materialParamList.TryGetValue(propertyName, out matParam);

            srcDstTableLayout.Controls.Remove(srcDstTableLayout.GetControlFromPosition(2, 0));
            srcDstTableLayout.Controls.Remove(srcDstTableLayout.GetControlFromPosition(2, 1));
            srcDstTableLayout.Controls.Remove(srcDstTableLayout.GetControlFromPosition(2, 2));
            srcDstTableLayout.Controls.Remove(srcDstTableLayout.GetControlFromPosition(2, 3));

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
            if (currentMaterialList[currentMatIndex].entries.ContainsKey(matPropertyComboBox.Text))
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
                currentMaterialList[currentMatIndex].entries.Add(matPropertyComboBox.Text, new float[] { 0, 0, 0, 0 });
                FillForm();
                addMatPropertyButton.Enabled = false;
            }
        }

        private void RemoveSelectedProperty()
        {
            // Check if the property exists first.
            string propertyName = propertiesListView.SelectedItems[0].Text;
            if (currentMaterialList[currentMatIndex].entries.ContainsKey(propertyName))
            {
                currentMaterialList[currentMatIndex].entries.Remove(propertyName);
                FillForm();
                addMatPropertyButton.Enabled = true; // The property can be added again.
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (currentMaterialList[currentMatIndex].textures.Count < 4)
            {
                currentMaterialList[currentMatIndex].textures.Add(NUD.MatTexture.GetDefault());
                FillForm();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (texturesListView.SelectedItems.Count > 0 && currentMaterialList[currentMatIndex].textures.Count > 1)
            {
                currentMaterialList[currentMatIndex].textures.RemoveAt(texturesListView.Items.IndexOf(texturesListView.SelectedItems[0]));
                FillForm();
            }
        }

        private void loadPresetButton_Click(object sender, EventArgs e)
        {
            MaterialSelector matSelector = new MaterialSelector();
            matSelector.ShowDialog();
            if (matSelector.exitStatus == MaterialSelector.ExitStatus.Opened)
            {
                List<NUD.Material> presetMaterials = ReadMaterialListFromPreset(matSelector.path);

                // Store the original material to preserve Tex IDs. 
                NUD.Material original = currentPolygon.materials[0].Clone();
                currentPolygon.materials = presetMaterials;

                // Copy the old Tex IDs. 
                currentPolygon.materials[0].CopyTextureIds(original);

                currentMaterialList = currentPolygon.materials;
                currentMatIndex = 0;
                Init();
                FillForm();
            }
        }

        public static List<NUD.Material> ReadMaterialListFromPreset(string file)
        {
            FileData matFile = new FileData(file);
            int soff = matFile.readInt();

            NUD.PolyData pol = new NUD.PolyData()
            {
                texprop1 = matFile.readInt(),
                texprop2 = matFile.readInt(),
                texprop3 = matFile.readInt(),
                texprop4 = matFile.readInt()
            };

            List<NUD.Material> presetMaterials = NUD.ReadMaterials(matFile, pol, soff);
            return presetMaterials;
        }

        private void RenderTexture(bool justRenderAlpha = false)
        {
            if (!tabControl1.SelectedTab.Text.Equals("Textures"))
                return;

            if (!Runtime.shaders["Texture"].ProgramCreatedSuccessfully())
                return;

            // Get the selected NUT texture.
            NutTexture nutTexture = null;
            int displayTexture = 0;
            if (currentMaterialList[currentMatIndex].entries.ContainsKey("NU_materialHash") && texturesListView.SelectedIndices.Count > 0)
            {
                int hash = currentMaterialList[currentMatIndex].textures[texturesListView.SelectedIndices[0]].hash;

                // Display dummy textures from resources. 
                if (Enum.IsDefined(typeof(NUD.DummyTextures), hash))
                    displayTexture = RenderTools.dummyTextures[(NUD.DummyTextures)hash].Id;
                else
                {
                    foreach (NUT n in Runtime.TextureContainers)
                    {
                        if (n.glTexByHashId.ContainsKey(hash))
                        {
                            n.getTextureByID(hash, out nutTexture);
                            displayTexture = n.glTexByHashId[hash].Id;
                            break;
                        }
                    }
                }
            }

            if (justRenderAlpha)
            {
                texAlphaGlControl.MakeCurrent();
                GL.Viewport(texAlphaGlControl.ClientRectangle);
                RenderTools.DrawTexturedQuad(displayTexture, 1, 1, false, false, false, true);
                texAlphaGlControl.SwapBuffers();
            }
            else
            {
                texRgbGlControl.MakeCurrent();
                GL.Viewport(texRgbGlControl.ClientRectangle);
                RenderTools.DrawTexturedQuad(displayTexture, 1, 1);
                texRgbGlControl.SwapBuffers();
            }
        }

        private void texturesListView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 'd' && texturesListView.SelectedIndices.Count > 0)
            {
                if(currentMaterialList[currentMatIndex].textures.Count > 1)
                {
                    currentMaterialList[currentMatIndex].textures.RemoveAt(texturesListView.SelectedIndices[0]);
                    FillForm();
                }
            }
        }

        private void NUDMaterialEditor_Scroll(object sender, ScrollEventArgs e)
        {
            RenderTexture();
        }

        private void UpdateButtonColor()
        {
            try
            {
                string selectedMatPropKey = propertiesListView.SelectedItems[0].Text;
                colorSelect.BackColor = Color.FromArgb(255,
                    ColorTools.FloatToIntClamp(currentMaterialList[currentMatIndex].entries[selectedMatPropKey][0]),
                    ColorTools.FloatToIntClamp(currentMaterialList[currentMatIndex].entries[selectedMatPropKey][1]),
                    ColorTools.FloatToIntClamp(currentMaterialList[currentMatIndex].entries[selectedMatPropKey][2]));
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
            currentMaterialList[currentMatIndex].hasSphereMap = sphereMapCB.Checked;
            FillForm();
        }

        private void shadowCB_CheckedChanged(object sender, EventArgs e)
        {
            currentMaterialList[currentMatIndex].hasShadow = shadowCB.Checked;
            FillForm();
        }

        private void GlowCB_CheckedChanged(object sender, EventArgs e)
        {
            currentMaterialList[currentMatIndex].glow = GlowCB.Checked;
            FillForm();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RenderTexture();
            FillForm();

            // Nothing is currently selected, so don't display the tex ID.
            textureIDTB.Text = "";
        }

        private void texRgbGlControl_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture();
        }

        private void texAlphaGlControl_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture(true);
        }

        private void param1TrackBar_Scroll(object sender, EventArgs e)
        {
            Params.MatParam labels = null;
            materialParamList.TryGetValue(currentPropertyName, out labels);
            
            if (labels != null)
            {
                enableParam1SliderUpdates = false;
                param1TB.Text = GuiTools.GetTrackBarValue(param1TrackBar, 0, labels.max1).ToString();
            }
        }

        private void param2TrackBar_Scroll(object sender, EventArgs e)
        {
            Params.MatParam labels = null;
            materialParamList.TryGetValue(currentPropertyName, out labels);

            if (labels != null)
            {
                enableParam2SliderUpdates = false;
                param2TB.Text = GuiTools.GetTrackBarValue(param2TrackBar, 0, labels.max2).ToString();
            }
        }

        private void param3TrackBar_Scroll(object sender, EventArgs e)
        {
            Params.MatParam labels = null;
            materialParamList.TryGetValue(currentPropertyName, out labels);

            if (labels != null)
            {
                enableParam3SliderUpdates = false;
                param3TB.Text = GuiTools.GetTrackBarValue(param3TrackBar, 0, labels.max3).ToString();
            }
        }

        private void param4TrackBar_Scroll(object sender, EventArgs e)
        {
            Params.MatParam labels = null;
            materialParamList.TryGetValue(currentPropertyName, out labels);

            if (labels != null)
            {
                enableParam4SliderUpdates = false;
                param4TB.Text = GuiTools.GetTrackBarValue(param4TrackBar, 0, labels.max4).ToString();
            }
        }

        private void addMaterialButton_Click(object sender, EventArgs e)
        {
            // Can only have two materials.
            if (currentMaterialList.Count < 2)
            {
                currentMaterialList.Add(NUD.Material.GetDefault());
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
            currentMaterialList[currentMatIndex].alphaTest = alphaTestCB.Checked ? 0x2 : 0x0;

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

            int[] c = NUD.WriteMaterial(m, currentMaterialList, s);

            FileOutput fin = new FileOutput();

            fin.writeInt(0);

            fin.writeInt(20 + c[0]);
            for (int i = 1; i < 4; i++)
            {
                fin.writeInt(c[i] == c[i - 1] ? 0 : 20 + c[i]);
            }

            for (int i = 0; i < 4 - c.Length; i++)
                fin.writeInt(0);

            fin.writeOutput(m);
            fin.align(32, 0xFF);
            fin.writeIntAt(fin.size(), 0);
            fin.writeOutput(s);
            fin.save(fileName);
        }

        private void cullModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            NUD.Material mat = currentMaterialList[currentMatIndex];
            if (matValueByCullModeName.ContainsKey(cullModeComboBox.SelectedItem.ToString()))
                mat.cullMode = matValueByCullModeName[cullModeComboBox.SelectedItem.ToString()];
        }
    }
}
