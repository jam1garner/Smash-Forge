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
using System.Windows.Input;
using System.Diagnostics;
using Smash_Forge.Rendering;

namespace Smash_Forge
{
    public partial class NUDMaterialEditor : DockContent
    {
        public NUD.Polygon poly;
        public List<NUD.Material> materials;
        int currentMatIndex = 0;
        public static Dictionary<string, MatParam> propList;

        public class MatParam
        {
            public string name = "";
            public string description = "";
            public string[] paramLabels = new string[4];

            // Users can still manually enter a value higher than max.
            public float max1 = 10.0f;
            public float max2 = 10.0f;
            public float max3 = 10.0f;
            public float max4 = 10.0f;

            public bool useTrackBar = true;
        }

        public static Dictionary<int, string> dstFactor = new Dictionary<int, string>(){
                    { 0x00, "Nothing"},
                    { 0x01, "SourceAlpha"},
                    { 0x02, "One"},
                    { 0x03, "InverseSourceAlpha + SubtractTrue"},
                    { 0x04, "Dummy"},
                };

        public static Dictionary<int, string> srcFactor = new Dictionary<int, string>(){
                    { 0x00, "Nothing"},
                    { 0x01, "SourceAlpha + CompareBeforeTextureFalse + DepthTestTrue + EnableDepthUpdateTrue"},
                    { 0x03, "SourceAlpha + CompareBeforeTextureTrue + DepthTestTrue + EnableDepthUpdateFalse + MultiplyBy1"},
                    { 0x04, "RasterAlpha + CompareBeforeTextureTrue + DepthTestTrue + EnableDepthUpdateFalse"},
                    { 0x05, "SourceAlpha + CompareBeforeTextureTrue + DepthTestTrue (can also be False) + EnableDepthUpdateFalse + MultiplyBy2"},
                    { 0x07, "SourceAlpha + CompareBeforeTextureTrue + DepthTestFalse + EnableDepthUpdateFalse + ObjectDraw"},
                    { 0x32, "SourceAlpha + CompareBeforeTextureTrue + DepthTestFalse + EnableDepthUpdateFalse + MultiplyBy2"},
                    { 0x33, "SourceAlpha + CompareBeforeTextureTrue + DepthTestFalse + EnableDepthUpdateFalse + MultiplyBy1"}
                };

        public static Dictionary<int, string> cullmode = new Dictionary<int, string>(){
                    { 0x0000, "Cull None"},
                    { 0x0404, "Cull Outside"},
                    { 0x0405, "Cull Inside"}
                };

        public static Dictionary<int, string> AlphaTest = new Dictionary<int, string>(){
                    { 0x00, "Alpha Test Disabled"},
                    { 0x02, "Alpha Test Enabled"},
                };

        public static Dictionary<int, string> AlphaFunc = new Dictionary<int, string>(){
                    { 0x00, "Never"},
                    { 0x04, "Lequal Ref Alpha + ??"},
                    { 0x06, "Lequal Ref Alpha + ???"}
                };

        public static Dictionary<int, string> mapmode = new Dictionary<int, string>(){
                    { 0x00, "TexCoord"},
                    { 0x1D00, "EnvCamera"},
                    { 0x1E00, "Projection"},
                    { 0x1ECD, "EnvLight"},
                    { 0x1F00, "EnvSpec"}
                };
        public static Dictionary<int, string> minfilter = new Dictionary<int, string>(){
                    { 0x00, "Linear_Mipmap_Linear"},
                    { 0x01, "Nearest"},
                    { 0x02, "Linear"},
                    { 0x03, "Nearest_Mipmap_Linear"}
                };
        public static Dictionary<int, string> magfilter = new Dictionary<int, string>(){
                    { 0x00, "???"},
                    { 0x01, "Nearest"},
                    { 0x02, "Linear"}
                };
        public static Dictionary<int, string> wrapmode = new Dictionary<int, string>(){
                    { 0x01, "Repeat"},
                    { 0x02, "Mirror"},
                    { 0x03, "Clamp"}
                };
        public static Dictionary<int, string> mip = new Dictionary<int, string>(){
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
            RenderTools.Setup();
        }

        public NUDMaterialEditor(NUD.Polygon p)
        {
            InitializeComponent();
            this.poly = p;
            this.materials = p.materials;
            Init();
            FillForm();
            matsComboBox.SelectedIndex = 0;

            // The dummy textures will be used later. 
            RenderTools.Setup();
        }

        public void InitPropList()
        {
            propList = new Dictionary<string, MatParam>();
            if (File.Exists("param_labels\\material_params.ini"))
            {
                try
                {
                    MatParam matParam = new MatParam();
                    using (StreamReader sr = new StreamReader("param_labels\\material_params.ini"))
                    {
                        while (!sr.EndOfStream)
                        {
                            string[] args = sr.ReadLine().Split('=');
                            string line = args[0];
                            switch (line)
                            {
                                case "[Param]": if (!matParam.name.Equals("") && !propList.ContainsKey(matParam.name)) propList.Add(matParam.name, matParam); matParam = new MatParam(); break;
                                case "name": matParam.name = args[1]; Console.WriteLine(matParam.name); break;
                                case "description": matParam.description = args[1]; break;
                                case "param1": matParam.paramLabels[0] = args[1]; break;
                                case "param2": matParam.paramLabels[1] = args[1]; break;
                                case "param3": matParam.paramLabels[2] = args[1]; break;
                                case "param4": matParam.paramLabels[3] = args[1]; break;
                                case "max1": float.TryParse(args[1], out matParam.max1); break;
                                case "max2": float.TryParse(args[1], out matParam.max2); break;
                                case "max3": float.TryParse(args[1], out matParam.max3); break;
                                case "max4": float.TryParse(args[1], out matParam.max4); break;
                                case "useTrackBar": bool.TryParse(args[1], out matParam.useTrackBar); break;
                            }
                        }
                    }
                    if (!matParam.name.Equals("") && !propList.ContainsKey(matParam.name)) propList.Add(matParam.name, matParam);
                }
                catch (Exception)
                {
                }
            }
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
            if (propList == null) InitPropList();
            foreach (string s in propList.Keys)
                matPropertyComboBox.Items.Add(s);

            if (wrapXComboBox.Items.Count == 0)
            {
                foreach (int i in srcFactor.Keys)
                    srcComboBox.Items.Add(srcFactor[i]);
                foreach (int i in dstFactor.Keys)
                    dstComboBox.Items.Add(dstFactor[i]);
                foreach (int i in cullmode.Keys)
                    cullModeComboBox.Items.Add(cullmode[i]);
                foreach (int i in AlphaTest.Keys)
                    alphaTestComboBox.Items.Add(AlphaTest[i]);
                foreach (int i in AlphaFunc.Keys)
                    AlphaFuncComboBox.Items.Add(AlphaFunc[i]);

                foreach (int i in wrapmode.Keys)
                {
                    wrapXComboBox.Items.Add(wrapmode[i]);
                    wrapYComboBox.Items.Add(wrapmode[i]);
                }
                foreach (int i in mapmode.Keys)
                    mapModeComboBox.Items.Add(mapmode[i]);
                foreach (int i in minfilter.Keys)
                    minFilterComboBox.Items.Add(minfilter[i]);
                foreach (int i in magfilter.Keys)
                    magFilterComboBox.Items.Add(magfilter[i]);
                foreach (int i in mip.Keys)
                    mipDetailComboBox.Items.Add(mip[i]);
            }
        }

        private void UpdateMatComboBox()
        {
            matsComboBox.Items.Clear();
            for (int i = 0; i < materials.Count; i++)
            {
                matsComboBox.Items.Add("Material_" + i);
            }
        }

        public void FillForm()
        {
            NUD.Material mat = materials[currentMatIndex];

            alphaTestComboBox.SelectedItem = AlphaTest[mat.AlphaTest];
            AlphaFuncComboBox.SelectedItem = AlphaFunc[mat.AlphaFunc];

            flagsTB.Text = mat.Flags.ToString("X");

            srcTB.Text = mat.srcFactor + "";
            dstTB.Text = mat.dstFactor + "";
            refAlphaTB.Text = mat.RefAlpha + "";
            cullModeTB.Text = mat.cullMode + "";
            zBufferTB.Text = mat.zBufferOffset + "";
           
            shadowCB.Checked = mat.hasShadow;
            GlowCB.Checked = mat.glow;
            dummy_rampCB.Checked = mat.hasDummyRamp;
            AOCB.Checked = mat.hasAoMap;
            diffuseCB.Checked = mat.hasDiffuse;
            diffuse2CB.Checked = mat.hasDiffuse2;
            normalCB.Checked = mat.hasNormalMap;
            sphere_mapCB.Checked = mat.hasSphereMap;
            cubemapCB.Checked = mat.hasCubeMap;
            stageMapCB.Checked = mat.hasStageMap;
            rampCB.Checked = mat.hasRamp;
            mysteryCB.Checked = mat.unkownWater != 0;

            texturesListView.Items.Clear();

            if (mat.hasDiffuse) texturesListView.Items.Add("Diffuse");
            if (mat.hasSphereMap) texturesListView.Items.Add("SphereMap");
            if (mat.hasDiffuse2) texturesListView.Items.Add("Diffuse2");
            if (mat.hasDiffuse3) texturesListView.Items.Add("Diffuse3");
            if (mat.hasStageMap) texturesListView.Items.Add("StageMap");
            if (mat.hasCubeMap) texturesListView.Items.Add("Cubemap");
            if (mat.hasAoMap) texturesListView.Items.Add("AO Map");
            if (mat.hasNormalMap) texturesListView.Items.Add("NormalMap");
            if (mat.hasRamp) texturesListView.Items.Add("Ramp");
            if (mat.hasDummyRamp) texturesListView.Items.Add("Dummy Ramp");

            propertiesListView.Items.Clear();
            propertiesListView.View = View.List;
            foreach (string propertyName in mat.entries.Keys)
            {
                propertiesListView.Items.Add(propertyName);
            }
            if(propertiesListView.Items.Count > 0)
                propertiesListView.SelectedIndices.Add(0);
        }
        
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentMatIndex = matsComboBox.SelectedIndex;
            FillForm();
            matsComboBox.SelectedIndex = currentMatIndex;
        }

        private void srcComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in srcFactor.Keys)
                if (srcFactor[i].Equals(srcComboBox.SelectedItem))
                {
                    srcTB.Text = i + "";
                    break;
                }
        }

        private void srcTB_TextChanged(object sender, EventArgs e)
        {
            setValue(srcTB, srcComboBox, srcFactor, out materials[currentMatIndex].srcFactor);
        }

        private void dstComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in dstFactor.Keys)
                if (dstFactor[i].Equals(dstComboBox.SelectedItem))
                {
                    dstTB.Text = i + "";
                    break;
                }
        }

        private void dstTB_TextChanged(object sender, EventArgs e)
        {
            setValue(dstTB, dstComboBox, dstFactor, out materials[currentMatIndex].dstFactor);
        }

        public void setValue(TextBox tb, ComboBox cb, Dictionary<int, string> dict, out int n)
        {
            n = -1;
            int.TryParse(tb.Text, out n);
            if (n != -1)
            {
                string o = "";
                dict.TryGetValue(n, out o);
                if (o != "")
                    cb.Text = o;
            }
            else
                tb.Text = "0";
        }

        private void cullModeComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            foreach (int i in cullmode.Keys)
                if (cullmode[i].Equals(cullModeComboBox.SelectedItem))
                {
                    cullModeTB.Text = i + "";
                    break;
                }
        }

        private void cullModeTB_TextChanged(object sender, EventArgs e)
        {
            setValue(cullModeTB, cullModeComboBox, cullmode, out materials[currentMatIndex].cullMode);
        }

        private void alphaTestComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in AlphaTest.Keys)
                if (AlphaTest[i].Equals(alphaTestComboBox.SelectedItem))
                {
                    alphaTestTB.Text = i + "";
                    break;
                }
        }

        private void alphaTestTB_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(alphaTestTB.Text, out materials[currentMatIndex].AlphaTest);
        }
        
        private void AlphaFuncCB_SelectedIndexChanged(object sender, EventArgs e)
        {

            foreach (int i in AlphaFunc.Keys)
                if (AlphaFunc[i].Equals(AlphaFuncComboBox.SelectedItem))
                {
                    Console.WriteLine(AlphaFunc[i] + " " + i);
                    alphaFuncTB.Text = i + "";
                    materials[currentMatIndex].AlphaFunc = i;
                    break;
                }
        }
        
        private void alphaFuncTB_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(alphaFuncTB.Text, out materials[currentMatIndex].AlphaFunc);
        }

        private void mysteryCB_CheckedChanged(object sender, EventArgs e)
        {
            if (mysteryCB.Checked)
                materials[currentMatIndex].unkownWater = 0x3A83126f;
            else
                materials[currentMatIndex].unkownWater = 0; ;
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
            if(index >= materials[currentMatIndex].textures.Count)
            {
                MessageBox.Show("Texture doesn't exist");
                return;
            }
            NUD.MatTexture tex = materials[currentMatIndex].textures[index];
            textureIDTB.Text = tex.hash.ToString("X");

            mapModeComboBox.SelectedItem = mapmode[tex.mapMode];
            wrapXComboBox.SelectedItem = wrapmode[tex.wrapModeS];
            wrapYComboBox.SelectedItem = wrapmode[tex.wrapModeT];
            minFilterComboBox.SelectedItem = minfilter[tex.minFilter];
            magFilterComboBox.SelectedItem = magfilter[tex.magFilter];
            mipDetailComboBox.SelectedItem = mip[tex.mipDetail];
            RenderTexture();
            RenderTexture(true);
        }
        private void flagsTB_TextChanged(object sender, EventArgs e)
        {
            uint f = 0;
            if (uint.TryParse(flagsTB.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out f))// && listView1.SelectedIndices.Count > 0
            {
                materials[currentMatIndex].Flags = f;
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
                materials[currentMatIndex].textures[texturesListView.SelectedIndices[0]].hash = f;

            // Update the texture color channels.
            RenderTexture();
            RenderTexture(true);
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            int n = -1;
            int.TryParse(refAlphaTB.Text, out n);
            if (n != -1)
            {
                materials[currentMatIndex].RefAlpha = n;
            } else
            {
                refAlphaTB.Text = "0";
            }
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            int n = -1;
            int.TryParse(zBufferTB.Text, out n);
            if (n != -1)
            {
                materials[currentMatIndex].zBufferOffset = n;
            }
            else
            {
                zBufferTB.Text = "0";
            }
        }

        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in mapmode.Keys)
                if (mapmode[i].Equals(mapModeComboBox.SelectedItem))
                {
                    materials[currentMatIndex].textures[texturesListView.SelectedIndices[0]].mapMode = i;
                    break;
                }
        }

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in wrapmode.Keys)
                if (wrapmode[i].Equals(wrapXComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        materials[currentMatIndex].textures[texturesListView.SelectedIndices[0]].wrapModeS = i;
                    break;
                }
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in wrapmode.Keys)
                if (wrapmode[i].Equals(wrapYComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        materials[currentMatIndex].textures[texturesListView.SelectedIndices[0]].wrapModeT = i;
                    break;
                }
        }

        private void comboBox11_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in minfilter.Keys)
                if (minfilter[i].Equals(minFilterComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        materials[currentMatIndex].textures[texturesListView.SelectedIndices[0]].minFilter = i;
                    break;
                }
        }

        private void comboBox12_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in magfilter.Keys)
                if (magfilter[i].Equals(magFilterComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        materials[currentMatIndex].textures[texturesListView.SelectedIndices[0]].magFilter = i;
                    break;
                }
        }

        private void comboBox13_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in mip.Keys)
                if (mip[i].Equals(mipDetailComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        materials[currentMatIndex].textures[texturesListView.SelectedIndices[0]].mipDetail = i;
                    break;
                }
        }

        #region Properties
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (propertiesListView.SelectedIndices.Count > 0)
                matPropertyNameTB.Text = materials[currentMatIndex].entries.Keys.ElementAt(propertiesListView.SelectedIndices[0]);
            if (matPropertyNameTB.Text.Equals("NU_materialHash"))
            {          
                param1TB.Text = BitConverter.ToInt32(BitConverter.GetBytes(materials[currentMatIndex].entries[matPropertyNameTB.Text][0]), 0).ToString("X");
                param2TB.Text = materials[currentMatIndex].entries[matPropertyNameTB.Text][1] + "";
                param3TB.Text = materials[currentMatIndex].entries[matPropertyNameTB.Text][2] + "";
                param4TB.Text = materials[currentMatIndex].entries[matPropertyNameTB.Text][3] + "";
            }
            else
            {
                param1TB.Text = materials[currentMatIndex].entries[matPropertyNameTB.Text][0] + "";
                param2TB.Text = materials[currentMatIndex].entries[matPropertyNameTB.Text][1] + "";
                param3TB.Text = materials[currentMatIndex].entries[matPropertyNameTB.Text][2] + "";
                param4TB.Text = materials[currentMatIndex].entries[matPropertyNameTB.Text][3] + "";
            }
        }

        private void param1TB_TextChanged(object sender, EventArgs e)
        {
            if (matPropertyNameTB.Text.Equals("NU_materialHash"))
            {
                int f = GuiTools.TryParseTBInt(param1TB, true);
                if (f != -1 && propertiesListView.SelectedItems.Count > 0)
                    materials[currentMatIndex].entries[propertiesListView.SelectedItems[0].Text][0] = BitConverter.ToSingle(BitConverter.GetBytes(f), 0);
            }
            else
            {
                float f = -1;
                float.TryParse(param1TB.Text, out f);
                if (f != -1 && propertiesListView.SelectedItems.Count > 0)
                {
                    materials[currentMatIndex].entries[propertiesListView.SelectedItems[0].Text][0] = f;

                    // Update trackbar.
                    MatParam labels = null;
                    propList.TryGetValue(matPropertyNameTB.Text, out labels);
                    float max = 1;
                    if (labels != null)
                    {
                        max = labels.max1;
                    }

                    GuiTools.UpdateTrackBarFromValue(f, param1TrackBar, 0, max);
                }
            }
            UpdateButtonColor();
        }

        private void param2TB_TextChanged(object sender, EventArgs e)
        {
            float f = -1;
            float.TryParse(param2TB.Text, out f);
            if (f != -1 && propertiesListView.SelectedItems.Count > 0)
            {
                materials[currentMatIndex].entries[propertiesListView.SelectedItems[0].Text][1] = f;

                // update trackbar
                MatParam labels = null;
                propList.TryGetValue(matPropertyNameTB.Text, out labels);
                float max = 1;
                if (labels != null)
                {
                    max = labels.max2;
                }

                GuiTools.UpdateTrackBarFromValue(f, param2TrackBar, 0, max);
            }
            UpdateButtonColor();
        }

        private void param3TB_TextChanged(object sender, EventArgs e)
        {
            float f = -1;
            float.TryParse(param3TB.Text, out f);
            if (f != -1 && propertiesListView.SelectedItems.Count > 0)
            {
                materials[currentMatIndex].entries[propertiesListView.SelectedItems[0].Text][2] = f;

                // update trackbar
                MatParam labels = null;
                propList.TryGetValue(matPropertyNameTB.Text, out labels);
                float max = 1;
                if (labels != null)
                {
                    max = labels.max3;
                }

                GuiTools.UpdateTrackBarFromValue(f, param3TrackBar, 0, max);
            }
            UpdateButtonColor();
        }

        private void param4TB_TextChanged(object sender, EventArgs e)
        {
            float f = -1;
            float.TryParse(param4TB.Text, out f);
            if (f != -1 && propertiesListView.SelectedItems.Count > 0)
            {
                // Set the param value for the selected property.
                string matPropertyKey = propertiesListView.SelectedItems[0].Text;
                materials[currentMatIndex].entries[matPropertyKey][3] = f;

                // Update trackbar
                MatParam labels = null;
                propList.TryGetValue(matPropertyNameTB.Text, out labels);
                float max = 1;
                if (labels != null)
                {
                    max = labels.max4;
                }

                GuiTools.UpdateTrackBarFromValue(f, param4TrackBar, 0, max);
            }
        }
        #endregion

        // property change
        private void matPropertyTB_TextChanged(object sender, EventArgs e)
        {
            MatParam matParams = null;
            propList.TryGetValue(matPropertyNameTB.Text, out matParams);
            descriptionLabel.Text = "Description:\n";
            tableLayoutPanel1.Controls.Remove(tableLayoutPanel1.GetControlFromPosition(2, 0));
            tableLayoutPanel1.Controls.Remove(tableLayoutPanel1.GetControlFromPosition(2, 1));
            tableLayoutPanel1.Controls.Remove(tableLayoutPanel1.GetControlFromPosition(2, 2));
            tableLayoutPanel1.Controls.Remove(tableLayoutPanel1.GetControlFromPosition(2, 3));
            if (matParams != null)
            {
                descriptionLabel.Text += matParams.description;
                param1Label.Text = matParams.paramLabels[0].Equals("") ? "Param1" : matParams.paramLabels[0];
                param2Label.Text = matParams.paramLabels[1].Equals("") ? "Param2" : matParams.paramLabels[1];
                param3Label.Text = matParams.paramLabels[2].Equals("") ? "Param3" : matParams.paramLabels[2];
                param4Label.Text = matParams.paramLabels[3].Equals("") ? "Param4" : matParams.paramLabels[3];

                // Not all material properties need a trackbar (Ex: NU_materialHash).
                param1TrackBar.Enabled = matParams.useTrackBar;
                param2TrackBar.Enabled = matParams.useTrackBar;
                param3TrackBar.Enabled = matParams.useTrackBar;
                param4TrackBar.Enabled = matParams.useTrackBar;
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
            if (materials[currentMatIndex].entries.ContainsKey(matPropertyComboBox.Text))
            {
                addMatPropertyButton.Enabled = false;
            }
        }

        private void addMatPropertyButton_Click(object sender, EventArgs e)
        {
            if (!matPropertyComboBox.Text.Equals(""))
            {
                materials[currentMatIndex].entries.Add(matPropertyComboBox.Text, new float[] { 0, 0, 0, 0 });
                FillForm();
                addMatPropertyButton.Enabled = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (materials[currentMatIndex].textures.Count < 4)
            {
                materials[currentMatIndex].textures.Add(NUD.MatTexture.getDefault());
                FillForm();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (texturesListView.SelectedItems.Count > 0 && materials[currentMatIndex].textures.Count > 1)
            {
                materials[currentMatIndex].textures.RemoveAt(texturesListView.Items.IndexOf(texturesListView.SelectedItems[0]));
                FillForm();
            }
        }

        //Saving Mat
        private void savePresetButton_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Namco Material (NMT)|*.nmt|" +
                             "All files(*.*)|*.*";

                sfd.InitialDirectory = Path.Combine(MainForm.executableDir,"materials\\");
                Console.WriteLine(sfd.InitialDirectory);
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    sfd.FileName = sfd.FileName;
                    sfd.RestoreDirectory = true;

                    if (sfd.FileName.EndsWith(".nmt"))
                    {
                        FileOutput m = new FileOutput();
                        FileOutput s = new FileOutput();

                        int[] c = NUD.writeMaterial(m, materials, s);

                        FileOutput fin = new FileOutput();
                        
                        fin.writeInt(0);

                        fin.writeInt(20 + c[0]);
                        for (int i = 1; i < 4; i++)
                        {
                            fin.writeInt(c[i] == c[i-1] ? 0 : 20 + c[i]);
                        }

                        for (int i = 0; i < 4 - c.Length; i++)
                            fin.writeInt(0);
                        
                        fin.writeOutput(m);
                        fin.align(32, 0xFF);
                        fin.writeIntAt(fin.size(), 0);
                        fin.writeOutput(s);
                        fin.save(sfd.FileName);
                    }
                }
            }
        }

        // Loading Mat
        private void loadPresetButton_Click(object sender, EventArgs e)
        {
            MaterialSelector matSelector = new MaterialSelector();
            matSelector.ShowDialog();
            if (matSelector.exitStatus == MaterialSelector.Opened)
            {
                FileData matFile = new FileData(matSelector.path);

                int soff = matFile.readInt();

                NUD._s_Poly pol = new NUD._s_Poly()
                {
                    texprop1 = matFile.readInt(),
                    texprop2 = matFile.readInt(),
                    texprop3 = matFile.readInt(),
                    texprop4 = matFile.readInt()
                };

                // Store the original material to preserve Tex IDs. 
                NUD.Material original = poly.materials[0].Clone();

                poly.materials = NUD.readMaterial(matFile, pol, soff);

                // Copy the old Tex IDs. 
                poly.materials[0].CopyTextureIds(original);

                materials = poly.materials;
                currentMatIndex = 0;
                Init();
                FillForm();
            }

        }

        private void RenderTexture(bool justRenderAlpha = false)
        {
            if (!tabControl1.SelectedTab.Text.Equals("Textures"))
                return;

            // Get the selected NUT texture.
            NUT_Texture nutTexture = null;
            int displayTexture = 0;
            if (materials[currentMatIndex].entries.ContainsKey("NU_materialHash") && texturesListView.SelectedIndices.Count > 0)
            {
                int hash = materials[currentMatIndex].textures[texturesListView.SelectedIndices[0]].hash;

                // Display dummy textures from resources. 
                if (hash == 0x10080000)
                    displayTexture = RenderTools.dummyRamp;

                foreach (NUT n in Runtime.TextureContainers)
                {
                    if (n.draw.ContainsKey(hash))
                    {
                        n.getTextureByID(hash, out nutTexture);
                        displayTexture = n.draw[hash];
                        break;
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

            if (!Runtime.shaders["Texture"].hasCheckedCompilation())
            {
                Runtime.shaders["Texture"].displayCompilationWarning("Texture");
            }
        }

        private void listView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 'd' && texturesListView.SelectedIndices.Count > 0)
            {
                if(materials[currentMatIndex].textures.Count > 1)
                {
                    materials[currentMatIndex].textures.RemoveAt(texturesListView.SelectedIndices[0]);
                    FillForm();
                }
            }
        }

        private void listView2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar == 'd') && propertiesListView.SelectedIndices.Count > 0)
            {
                if (materials[currentMatIndex].entries.Count > 1)
                {
                    materials[currentMatIndex].entries.Remove(propertiesListView.SelectedItems[0].Text);
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
                    ColorTools.FloatToIntClamp(materials[currentMatIndex].entries[selectedMatPropKey][0]),
                    ColorTools.FloatToIntClamp(materials[currentMatIndex].entries[selectedMatPropKey][1]),
                    ColorTools.FloatToIntClamp(materials[currentMatIndex].entries[selectedMatPropKey][2]));
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

        private void listView2_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) && propertiesListView.SelectedIndices.Count > 0)
            {
                if (materials[currentMatIndex].textures.Count > 1)
                {
                    materials[currentMatIndex].entries.Remove(propertiesListView.SelectedItems[0].Text);
                    FillForm();
                }
            }
        }

        private void sphereMapCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[currentMatIndex].hasSphereMap = sphereMapCB.Checked;
            FillForm();
        }

        private void shadowCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[currentMatIndex].hasShadow = shadowCB.Checked;
            FillForm();
        }

        private void GlowCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[currentMatIndex].glow = GlowCB.Checked;
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

        private void listView2_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete)
            {
                NUD.Material mat = materials[currentMatIndex];
                foreach (var property in propertiesListView.SelectedItems)
                {
                    mat.entries.Remove(property.ToString());
                }
                e.Handled = true;
            }
        }

        private void diffuseCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[currentMatIndex].hasDiffuse = diffuseCB.Checked;
            FillForm();
        }

        private void dummy_rampCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[currentMatIndex].hasDummyRamp = dummy_rampCB.Checked;
            FillForm();
        }

        private void diffuse2CB_CheckedChanged(object sender, EventArgs e)
        {
            materials[currentMatIndex].hasDiffuse2 = diffuse2CB.Checked;
            FillForm();
        }

        private void normalCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[currentMatIndex].hasNormalMap = normalCB.Checked;
            FillForm();
        }

        private void sphere_mapCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[currentMatIndex].hasSphereMap = sphere_mapCB.Checked;
            FillForm();
        }

        private void rampCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[currentMatIndex].hasRamp = rampCB.Checked;
            FillForm();
        }

        private void AOCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[currentMatIndex].hasAoMap = AOCB.Checked;
            FillForm();
        }

        private void stageMapCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[currentMatIndex].hasStageMap = stageMapCB.Checked;
            FillForm();
        }

        private void cubemapCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[currentMatIndex].hasCubeMap = cubemapCB.Checked;
            FillForm();
        }

        private void param1TrackBar_Scroll(object sender, EventArgs e)
        {
            MatParam labels = null;
            propList.TryGetValue(matPropertyNameTB.Text, out labels);
            
            if (labels != null)
            {
                param1TB.Text = GuiTools.GetTrackBarValue(param1TrackBar, labels.max1).ToString();
            }
        }

        private void param2TrackBar_Scroll(object sender, EventArgs e)
        {
            MatParam labels = null;
            propList.TryGetValue(matPropertyNameTB.Text, out labels);

            if (labels != null)
            {
                param2TB.Text = GuiTools.GetTrackBarValue(param2TrackBar, labels.max2).ToString();
            }
        }

        private void param3TrackBar_Scroll(object sender, EventArgs e)
        {
            MatParam labels = null;
            propList.TryGetValue(matPropertyNameTB.Text, out labels);

            if (labels != null)
            {
                param3TB.Text = GuiTools.GetTrackBarValue(param3TrackBar, labels.max3).ToString();
            }
        }

        private void param4TrackBar_Scroll(object sender, EventArgs e)
        {
            MatParam labels = null;
            propList.TryGetValue(matPropertyNameTB.Text, out labels);

            if (labels != null)
            {
                param4TB.Text = GuiTools.GetTrackBarValue(param4TrackBar, labels.max4).ToString();
            }
        }

        private void addMaterialButton_Click(object sender, EventArgs e)
        {
            // Can only have two materials.
            if (materials.Count < 2)
                materials.Add(NUD.Material.GetDefault());

            FillForm();
            UpdateMatComboBox();
        }
    }
}
