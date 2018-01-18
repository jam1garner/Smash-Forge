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

namespace Smash_Forge
{
    public partial class NUDMaterialEditor : DockContent
    {
        public NUD.Polygon poly;
        public List<NUD.Material> materials;
        int current = 0;
        public static Dictionary<string, MatParam> propList;

        public void trackchange(object sender, EventArgs e)
        {
            Console.WriteLine(((TrackBar)sender).Value);
        }

        public class MatParam
        {
            public string name = "";
            public string description = "";
            public string[] ps = new string[4];

            // users can still manually enter a value higher than max
            public float max1 = 100.0f;
            public float max2 = 100.0f;
            public float max3 = 100.0f;
            public float max4 = 100.0f;

            public bool useTrackBar = true;
            public List<string> op1, op2, op3, op4;
            public Control control1 = null;
            public Control control2 = null;
            public Control control3 = null;
            public Control control4 = null;

            public MatParam()
            {
            }
            public void trackchange(object sender, EventArgs e)
            {
                Console.WriteLine(((TrackBar)sender).Value);
            }

   
        }

        public static Dictionary<int, string> dstFactor = new Dictionary<int, string>(){
                    { 0x00, "Nothing"},
                    { 0x01, "SourceAlpha"},
                    { 0x02, "One"},
                    { 0x03, "InverseSourceAlpha + SubtractTrue"},
                    { 0x04, "Dummy"},
                };//{ 0x101f, "Invisible"}

        public static Dictionary<int, string> srcFactor = new Dictionary<int, string>(){
                    { 0x00, "Nothing"},
                    { 0x01, "SourceAlpha + CompareBeforeTextureFalse + DepthTestTrue + EnableDepthUpdateTrue"},
                    { 0x03, "SourceAlpha + CompareBeforeTextureTrue + DepthTestTrue + EnableDepthUpdateFalse + MultiplyBy1"},
                    { 0x04, "RasterAlpha + CompareBeforeTextureTrue + DepthTestTrue + EnableDepthUpdateFalse"},
                    { 0x05, "SourceAlpha + CompareBeforeTextureTrue + DepthTestTrue (can also be False) + EnableDepthUpdateFalse + MultiplyBy2"},
                    { 0x07, "SourceAlpha + CompareBeforeTextureTrue + DepthTestFalse + EnableDepthUpdateFalse + ObjectDraw"},
                    { 0x32, "SourceAlpha + CompareBeforeTextureTrue + DepthTestFalse + EnableDepthUpdateFalse + MultiplyBy2"},
                    { 0x33, "SourceAlpha + CompareBeforeTextureTrue + DepthTestFalse + EnableDepthUpdateFalse + MultiplyBy1"}
                };//{ 0x101f, "Invisible"}

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
        }

        public NUDMaterialEditor(NUD.Polygon p)
        {
            InitializeComponent();
            this.poly = p;
            this.materials = p.materials;
            Init();
            FillForm();
            matsComboBox.SelectedIndex = 0;
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
                                case "param1": matParam.ps[0] = args[1]; break;
                                case "param2": matParam.ps[1] = args[1]; break;
                                case "param3": matParam.ps[2] = args[1]; break;
                                case "param4": matParam.ps[3] = args[1]; break;
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

        private void NUDMaterialEditor_Load(object sender, EventArgs e)
        {
        }

        private void UpdateTrackBarFromParam(MatParam param)
        {
        
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if(glControl1 != null)
                RenderTexture();
        }

        public void Init()
        {
            matsComboBox.Items.Clear();
            for (int i = 0; i < materials.Count; i++)
            {
                matsComboBox.Items.Add("Material_" + i);
            }

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
                    AlphaTestComboBox.Items.Add(AlphaTest[i]);
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

        public void FillForm()
        {
            NUD.Material mat = materials[current];

            flagsTB.Text = mat.Flags.ToString("X") + "";
            srcTB.Text = mat.srcFactor + "";
            dstTB.Text = mat.dstFactor + "";
            AlphaTestComboBox.SelectedItem = AlphaTest[mat.AlphaTest];
            AlphaFuncComboBox.SelectedItem = AlphaFunc[mat.AlphaFunc];

            refAlphaTB.Text = mat.RefAlpha + "";
            cullModeTB.Text = mat.cullMode + "";
            zBufferTB.Text = mat.zBufferOffset + "";

            mysteryCB.Checked = mat.unkownWater != 0;

            texturesListView.Items.Clear();
            
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
            foreach (string s in mat.entries.Keys)
            {
                propertiesListView.Items.Add(s);
            }
            if(propertiesListView.Items.Count > 0)
                propertiesListView.SelectedIndices.Add(0);
        }
        
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            current = matsComboBox.SelectedIndex;
            FillForm();
            matsComboBox.SelectedIndex = current;
        }

        #region DST
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in srcFactor.Keys)
                if (srcFactor[i].Equals(srcComboBox.SelectedItem))
                {
                    srcTB.Text = i + "";
                    break;
                }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            setValue(srcTB, srcComboBox, srcFactor, out materials[current].srcFactor);
        }
        #endregion

        #region SRC
        private void comboBox3_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            foreach (int i in dstFactor.Keys)
                if (dstFactor[i].Equals(dstComboBox.SelectedItem))
                {
                    dstTB.Text = i + "";
                    break;
                }
        }

        private void textBox4_TextChanged_1(object sender, EventArgs e)
        {
            setValue(dstTB, dstComboBox, dstFactor, out materials[current].dstFactor);
        }
        #endregion

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

        #region CULL
        private void comboBox6_SelectionChangeCommitted(object sender, EventArgs e)
        {
            foreach (int i in cullmode.Keys)
                if (cullmode[i].Equals(cullModeComboBox.SelectedItem))
                {
                    cullModeTB.Text = i + "";
                    break;
                }
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            setValue(cullModeTB, cullModeComboBox, cullmode, out materials[current].cullMode);
        }
        #endregion

        #region alpha function
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in AlphaTest.Keys)
                if (AlphaTest[i].Equals(AlphaTestComboBox.SelectedItem))
                {
                    alphaTestTB.Text = i + "";
                    break;
                }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(alphaTestTB.Text, out materials[current].AlphaTest);
        }
        
        private void AlphaFuncCB_SelectedIndexChanged(object sender, EventArgs e)
        {

            foreach (int i in AlphaFunc.Keys)
                if (AlphaFunc[i].Equals(AlphaFuncComboBox.SelectedItem))
                {
                    Console.WriteLine(AlphaFunc[i] + " " + i);
                    alphaFuncTB.Text = i + "";
                    materials[current].AlphaFunc = i;
                    break;
                }
        }
        
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(alphaFuncTB.Text, out materials[current].AlphaFunc);
        }



        #endregion

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (mysteryCB.Checked)
                materials[current].unkownWater = 0x3A83126f;
            else
                materials[current].unkownWater = 0; ;
        }

        #region Textures
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = 0;
            if (texturesListView.SelectedItems.Count > 0)
            {
                index = texturesListView.Items.IndexOf(texturesListView.SelectedItems[0]);
                tableLayoutPanel2.Enabled = true;
                textBox10.Enabled = true;
            }
            else
            {
                tableLayoutPanel2.Enabled = false;
                textBox10.Enabled = false;
            }
            if(index >= materials[current].textures.Count)
            {
                MessageBox.Show("Texture doesn't exist");
                return;
            }
            NUD.Mat_Texture tex = materials[current].textures[index];
            textBox10.Text = tex.hash.ToString("X");

            mapModeComboBox.SelectedItem = mapmode[tex.MapMode];
            wrapXComboBox.SelectedItem = wrapmode[tex.WrapMode1];
            wrapYComboBox.SelectedItem = wrapmode[tex.WrapMode2];
            minFilterComboBox.SelectedItem = minfilter[tex.minFilter];
            magFilterComboBox.SelectedItem = magfilter[tex.magFilter];
            mipDetailComboBox.SelectedItem = mip[tex.mipDetail];
            RenderTexture();
            RenderTextureAlpha();
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            uint f = 0;
            if (uint.TryParse(flagsTB.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out f))// && listView1.SelectedIndices.Count > 0
            {
                materials[current].Flags = f;
                flagsTB.BackColor = Color.White;

                // Clear the texture list to prevent displaying duplicates
                texturesListView.Clear();
                FillForm();
            }
            else
                flagsTB.BackColor = Color.Red;
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            int f = -1;
            int.TryParse(textBox10.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out f);
            if (f != -1 && texturesListView.SelectedIndices.Count > 0)
                materials[current].textures[texturesListView.SelectedIndices[0]].hash = f;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            int n = -1;
            int.TryParse(refAlphaTB.Text, out n);
            if (n != -1)
            {
                materials[current].RefAlpha = n;
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
                materials[current].zBufferOffset = n;
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
                    materials[current].textures[texturesListView.SelectedIndices[0]].MapMode = i;
                    break;
                }
        }

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in wrapmode.Keys)
                if (wrapmode[i].Equals(wrapXComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        materials[current].textures[texturesListView.SelectedIndices[0]].WrapMode1 = i;
                    break;
                }
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in wrapmode.Keys)
                if (wrapmode[i].Equals(wrapYComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        materials[current].textures[texturesListView.SelectedIndices[0]].WrapMode2 = i;
                    break;
                }
        }

        private void comboBox11_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in minfilter.Keys)
                if (minfilter[i].Equals(minFilterComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        materials[current].textures[texturesListView.SelectedIndices[0]].minFilter = i;
                    break;
                }
        }

        private void comboBox12_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in magfilter.Keys)
                if (magfilter[i].Equals(magFilterComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        materials[current].textures[texturesListView.SelectedIndices[0]].magFilter = i;
                    break;
                }
        }

        private void comboBox13_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in mip.Keys)
                if (mip[i].Equals(mipDetailComboBox.SelectedItem))
                {
                    if (texturesListView.SelectedItems.Count > 0)
                        materials[current].textures[texturesListView.SelectedIndices[0]].mipDetail = i;
                    break;
                }
        }
        #endregion

        #region Properties
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (propertiesListView.SelectedIndices.Count > 0)
                matPropertyNameTB.Text = materials[current].entries.Keys.ElementAt(propertiesListView.SelectedIndices[0]);
            if (matPropertyNameTB.Text.Equals("NU_materialHash"))
            {
                
                param1TB.Text = BitConverter.ToInt32(BitConverter.GetBytes(materials[current].entries[matPropertyNameTB.Text][0]), 0).ToString("X");
                param2TB.Text = materials[current].entries[matPropertyNameTB.Text][1] + "";
                param3TB.Text = materials[current].entries[matPropertyNameTB.Text][2] + "";
                param4TB.Text = materials[current].entries[matPropertyNameTB.Text][3] + "";
            }
            else
            {
                param1TB.Text = materials[current].entries[matPropertyNameTB.Text][0] + "";
                param2TB.Text = materials[current].entries[matPropertyNameTB.Text][1] + "";
                param3TB.Text = materials[current].entries[matPropertyNameTB.Text][2] + "";
                param4TB.Text = materials[current].entries[matPropertyNameTB.Text][3] + "";
            }
        }

        private void param1TB_TextChanged(object sender, EventArgs e)
        {
            if (matPropertyNameTB.Text.Equals("NU_materialHash"))
            {
                int f = -1;
                int.TryParse(param1TB.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out f);
                if (f != -1 && propertiesListView.SelectedItems.Count > 0)
                    materials[current].entries[propertiesListView.SelectedItems[0].Text][0] = BitConverter.ToSingle(BitConverter.GetBytes(f), 0);
            }
            else
            {
                float f = -1;
                float.TryParse(param1TB.Text, out f);
                if (f != -1 && propertiesListView.SelectedItems.Count > 0)
                {
                    materials[current].entries[propertiesListView.SelectedItems[0].Text][0] = f;

                    // udpate trackbar. should clean this up later
                    MatParam labels = null;
                    propList.TryGetValue(matPropertyNameTB.Text, out labels);
                    float max = 1;
                    if (labels != null)
                    {
                        max = labels.max1;
                    }

                    // clamp slider value to maximum value
                    int newSliderValue = (int)((f * (float)param1TrackBar.Maximum) / max);
                    newSliderValue = Math.Min(newSliderValue, param1TrackBar.Maximum);
                    newSliderValue = Math.Max(newSliderValue, 0);
                    param1TrackBar.Value = newSliderValue;
                }
            }
            updateButton();
        }

        private void param2TB_TextChanged(object sender, EventArgs e)
        {
            float f = -1;
            float.TryParse(param2TB.Text, out f);
            if (f != -1 && propertiesListView.SelectedItems.Count > 0)
            {
                materials[current].entries[propertiesListView.SelectedItems[0].Text][1] = f;

                // udpate trackbar
                MatParam labels = null;
                propList.TryGetValue(matPropertyNameTB.Text, out labels);
                float max = 1;
                if (labels != null)
                {
                    max = labels.max2;
                }

                // clamp slider values. values outside range can still be entered in text box
                int newSliderValue = (int)((f * (float)param2TrackBar.Maximum) / max);
                newSliderValue = Math.Min(newSliderValue, param2TrackBar.Maximum);
                newSliderValue = Math.Max(newSliderValue, 0);
                param2TrackBar.Value = newSliderValue;
            }
            updateButton();
        }

        private void param3TB_TextChanged(object sender, EventArgs e)
        {
            float f = -1;
            float.TryParse(param3TB.Text, out f);
            if (f != -1 && propertiesListView.SelectedItems.Count > 0)
            {
                materials[current].entries[propertiesListView.SelectedItems[0].Text][2] = f;

                // udpate trackbar
                MatParam labels = null;
                propList.TryGetValue(matPropertyNameTB.Text, out labels);
                float max = 1;
                if (labels != null)
                {
                    max = labels.max3;
                }

                // clamp slider values. values outside range can still be entered in text box
                int newSliderValue = (int)((f * (float)param2TrackBar.Maximum) / max);
                newSliderValue = Math.Min(newSliderValue, param3TrackBar.Maximum);
                newSliderValue = Math.Max(newSliderValue, 0);
                param3TrackBar.Value = newSliderValue;
            }
            updateButton();
        }

        private void param4TB_TextChanged(object sender, EventArgs e)
        {
            float f = -1;
            float.TryParse(param4TB.Text, out f);
            if (f != -1 && propertiesListView.SelectedItems.Count > 0)
            {
                materials[current].entries[propertiesListView.SelectedItems[0].Text][3] = f;

                // udpate trackbar
                MatParam labels = null;
                propList.TryGetValue(matPropertyNameTB.Text, out labels);
                float max = 1;
                if (labels != null)
                {
                    max = labels.max4;
                }

                // clamp slider values. values outside range can still be entered in text box
                int newSliderValue = (int)((f * (float)param4TrackBar.Maximum) / max);
                newSliderValue = Math.Min(newSliderValue, param3TrackBar.Maximum);
                newSliderValue = Math.Max(newSliderValue, 0);
                param4TrackBar.Value = newSliderValue;
            }
        }
        #endregion

        // property change
        private void matPropertyTB_TextChanged(object sender, EventArgs e)
        {
            MatParam labels = null;
            propList.TryGetValue(matPropertyNameTB.Text, out labels);
            descriptionLabel.Text = "Description:\n";
            tableLayoutPanel1.Controls.Remove(tableLayoutPanel1.GetControlFromPosition(2, 0));
            tableLayoutPanel1.Controls.Remove(tableLayoutPanel1.GetControlFromPosition(2, 1));
            tableLayoutPanel1.Controls.Remove(tableLayoutPanel1.GetControlFromPosition(2, 2));
            tableLayoutPanel1.Controls.Remove(tableLayoutPanel1.GetControlFromPosition(2, 3));
            if (labels != null)
            {
                descriptionLabel.Text += labels.description;
                label20.Text = labels.ps[0].Equals("") ? "Param1" : labels.ps[0];
                label21.Text = labels.ps[1].Equals("") ? "Param2" : labels.ps[1];
                label22.Text = labels.ps[2].Equals("") ? "Param3" : labels.ps[2];
                label23.Text = labels.ps[3].Equals("") ? "Param4" : labels.ps[3];
                if(labels.control1 != null)
                    paramGB.Controls.Add(labels.control1, 2, 0);
                if (labels.control2 != null)
                    paramGB.Controls.Add(labels.control2, 2, 1);
                if (labels.control3 != null)
                    paramGB.Controls.Add(labels.control3, 2, 2);
                if (labels.control4 != null)
                    paramGB.Controls.Add(labels.control4, 2, 3);

                // not all material properties need a trackbar
                param1TrackBar.Enabled = labels.useTrackBar;
                param2TrackBar.Enabled = labels.useTrackBar;
                param3TrackBar.Enabled = labels.useTrackBar;
                param4TrackBar.Enabled = labels.useTrackBar;
            } else
            {
                label20.Text = "Param1";
                label21.Text = "Param2";
                label22.Text = "Param3";
                label23.Text = "Param4";
            }
        }

        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            addMatPropertyButton.Enabled = true;

            if (materials[current].entries.ContainsKey(matPropertyComboBox.Text))
            {
                addMatPropertyButton.Enabled = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!matPropertyComboBox.Text.Equals(""))
            {
                materials[current].entries.Add(matPropertyComboBox.Text, new float[] { 0, 0, 0, 0 });
                FillForm();
                addMatPropertyButton.Enabled = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if(propertiesListView.SelectedItems.Count > 0)
            {
                materials[current].entries.Remove(propertiesListView.SelectedItems[0].Text);
                FillForm();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (materials[current].textures.Count < 4)
            {
                materials[current].textures.Add(NUD.Polygon.makeDefault());
                FillForm();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (texturesListView.SelectedItems.Count > 0 && materials[current].textures.Count > 1)
            {
                materials[current].textures.RemoveAt(texturesListView.Items.IndexOf(texturesListView.SelectedItems[0]));
                FillForm();
            }
        }

        //Saving Mat
        private void button1_Click(object sender, EventArgs e)
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
        private void button2_Click(object sender, EventArgs e)
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

                // store all the tex IDs for each texture type before changing material
                int diffuseID = poly.materials[0].diffuse1ID;
                int nrmID = poly.materials[0].normalID;

                // are these variables necessary?
                int diffuse1ID = poly.materials[0].diffuse1ID;
                int diffuse2ID = poly.materials[0].diffuse2ID;
                int diffuse3ID = poly.materials[0].diffuse3ID;
                int normalID = poly.materials[0].normalID;
                int rampID = poly.materials[0].rampID;
                int dummyRampID = poly.materials[0].dummyRampID;
                int sphereMapID = poly.materials[0].sphereMapID;
                int aoMapID = poly.materials[0].aoMapID;
                int stageMapID = poly.materials[0].stageMapID;
                int cubeMapID = poly.materials[0].cubeMapID;

                poly.materials = NUD.readMaterial(matFile, pol, soff);

                // might be a cleaner way to do this
                int count = 0;
                if (poly.materials[0].hasDiffuse && count < poly.materials[0].textures.Count)
                {
                    poly.materials[0].textures[count].hash = diffuse1ID;
                    count++;
                }
                if (poly.materials[0].hasDiffuse2 && count < poly.materials[0].textures.Count)
                {
                    poly.materials[0].textures[count].hash = diffuse2ID;
                    count++;
                }
                if (poly.materials[0].hasDiffuse3 && count < poly.materials[0].textures.Count)
                {
                    poly.materials[0].textures[count].hash = diffuse3ID;
                    count++;
                }
                if (poly.materials[0].hasStageMap && count < poly.materials[0].textures.Count)
                {
                    // don't preserve stageMap ID
                    count++;
                }
                if (poly.materials[0].hasCubeMap && count < poly.materials[0].textures.Count)
                {
                    poly.materials[0].textures[count].hash = cubeMapID;
                    count++;
                }
                if (poly.materials[0].hasSphereMap && count < poly.materials[0].textures.Count)
                {
                    poly.materials[0].textures[count].hash = sphereMapID;
                    count++;
                }
                if (poly.materials[0].hasAoMap && count < poly.materials[0].textures.Count)
                {
                    poly.materials[0].textures[count].hash = aoMapID;
                    count++;
                }
                if (poly.materials[0].hasNormalMap && count < poly.materials[0].textures.Count)
                {
                    poly.materials[0].textures[count].hash = normalID;
                    count++;
                }
                if (poly.materials[0].hasRamp && count < poly.materials[0].textures.Count)
                {
                    poly.materials[0].textures[count].hash = rampID;
                    count++;
                }
                if (poly.materials[0].hasDummyRamp && count < poly.materials[0].textures.Count)
                {
                    // dummy ramp should almost always be 0x10080000
                    count++;
                }

                materials = poly.materials;
                Console.WriteLine(materials.Count);
                current = 0;
                Init();
                FillForm();
            }
       
        }

        private void RenderTexture()
        {
            if (!tabControl1.SelectedTab.Text.Equals("Textures")) return;

            glControl1.MakeCurrent();
            GL.Viewport(glControl1.ClientRectangle);
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Enable(EnableCap.Texture2D);

            NUT_Texture tex = null;
            int texture = 0;
            if (materials[current].entries.ContainsKey("NU_materialHash") && texturesListView.SelectedIndices.Count > 0)
            {
                int hash = materials[current].textures[texturesListView.SelectedIndices[0]].hash;

                foreach (NUT n in Runtime.TextureContainers)
                    if (n.draw.ContainsKey(hash))
                    {
                        n.getTextureByID(hash, out tex);
                        texture = n.draw[hash];
                        break;
                    }
            }
     
            RenderTools.DrawTexturedQuad(texture, 1, 1, true, true, true, false, false, false);

            if (!Runtime.shaders["Texture"].hasCheckedCompilation())
            {
                Runtime.shaders["Texture"].displayCompilationWarning("Texture");
            }

            glControl1.SwapBuffers();
        }


        private void RenderTextureAlpha()
        {
            if (!tabControl1.SelectedTab.Text.Equals("Textures")) return;
            glControl2.MakeCurrent();
            GL.Viewport(glControl2.ClientRectangle);
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Enable(EnableCap.Texture2D);

            NUT_Texture tex = null;
            int texture = 0;
            if (materials[current].entries.ContainsKey("NU_materialHash") && texturesListView.SelectedIndices.Count > 0)
            {
                int hash = materials[current].textures[texturesListView.SelectedIndices[0]].hash;

                foreach (NUT n in Runtime.TextureContainers)
                    if (n.draw.ContainsKey(hash))
                    {
                        n.getTextureByID(hash, out tex);
                        texture = n.draw[hash];
                        break;
                    }
            }
            float h = 1f, w = 1f;
            if (tex != null)
            {
                float texureRatioW = tex.Width / tex.Height;
                float widthPre = texureRatioW * glControl2.Height;
                w = glControl2.Width / widthPre;
                if (texureRatioW > glControl2.AspectRatio)
                {
                    w = 1f;
                    float texureRatioH = tex.Height / tex.Width;
                    float HeightPre = texureRatioH * glControl2.Width;
                    h = glControl2.Height / HeightPre;
                }
            }
            if (float.IsInfinity(h)) h = 1;
            Console.WriteLine(w + " " + h);

            RenderTools.DrawTexturedQuad(texture, 1, 1, false, false, false, true, true, false);
            glControl2.SwapBuffers();
        }


        private void glControl1_Click(object sender, EventArgs e)
        {
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (!matPropertyComboBox.Text.Equals(""))
            {
                if(!materials[current].entries.ContainsKey(matPropertyComboBox.Text))
                    materials[current].entries.Add(matPropertyComboBox.Text, new float[] { 0,0,0,0 });
                FillForm();
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if(materials[current].textures.Count < 4)
            {
                materials[current].textures.Add(NUD.Polygon.makeDefault());
                FillForm();
            }
        }

        private void listView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 'd' && texturesListView.SelectedIndices.Count > 0)
            {
                if(materials[current].textures.Count > 1)
                {
                    materials[current].textures.RemoveAt(texturesListView.SelectedIndices[0]);
                    FillForm();
                }
            }
        }

        private void listView2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar == 'd') && propertiesListView.SelectedIndices.Count > 0)
            {
                if (materials[current].entries.Count > 1)
                {
                    materials[current].entries.Remove(propertiesListView.SelectedItems[0].Text);
                    FillForm();
                }
            }
        }

        private void NUDMaterialEditor_Scroll(object sender, ScrollEventArgs e)
        {
            RenderTexture();
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void updateButton()
        {
            try
            {
                colorSelect.BackColor = Color.FromArgb(255,
                    Clamp(materials[current].entries[propertiesListView.SelectedItems[0].Text][0] * 255),
                    Clamp(materials[current].entries[propertiesListView.SelectedItems[0].Text][1] * 255),
                    Clamp(materials[current].entries[propertiesListView.SelectedItems[0].Text][2] * 255));
            }
            catch (ArgumentOutOfRangeException)
            {

            }

        }

        public int Clamp(float i)
        {
            if (i > 255)
                return 255;
            if (i < 0)
                return 0;
            return (int)i;
        }

        private void colorSelect_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = colorSelect.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                param1TB.Text = colorDialog1.Color.R / 255f + "";
                param2TB.Text = colorDialog1.Color.G / 255f + "";
                param3TB.Text = colorDialog1.Color.B / 255f + "";
                
            }
        }

        private void listView2_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) && propertiesListView.SelectedIndices.Count > 0)
            {
                if (materials[current].textures.Count > 1)
                {
                    materials[current].entries.Remove(propertiesListView.SelectedItems[0].Text);
                    FillForm();
                }
            }
        }

        private void sphereMapCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[current].hasSphereMap = sphereMapCB.Checked;
            FillForm();
        }

        private void shadowCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[current].hasShadow = shadowCB.Checked;
            FillForm();
        }

        private void GlowCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[current].glow = GlowCB.Checked;
            FillForm();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RenderTexture();
            FillForm();
        }

        private void NUDMaterialEditor_Paint(object sender, PaintEventArgs e)
        {
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {

            RenderTexture();
        }

        private void glControl2_Paint(object sender, PaintEventArgs e)
        {

            RenderTextureAlpha();
        }

        private void listView2_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete)
            {
                NUD.Material mat = materials[current];
                foreach (var property in propertiesListView.SelectedItems)
                {
                    mat.entries.Remove(property.ToString());
                }
                e.Handled = true;
            }
        }

        private void diffuseCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[current].hasDiffuse = diffuseCB.Checked;
            FillForm();
        }

        private void dummy_rampCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[current].hasDummyRamp = dummy_rampCB.Checked;
            FillForm();
        }

        private void diffuse2CB_CheckedChanged(object sender, EventArgs e)
        {
            materials[current].hasDiffuse2 = diffuse2CB.Checked;
            FillForm();
        }

        private void normalCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[current].hasNormalMap = normalCB.Checked;
            FillForm();
        }

        private void sphere_mapCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[current].hasSphereMap = sphere_mapCB.Checked;
            FillForm();
        }

        private void rampCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[current].hasRamp = rampCB.Checked;
            FillForm();
        }

        private void AOCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[current].hasAoMap = AOCB.Checked;
            FillForm();
        }

        private void stageMapCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[current].hasStageMap = stageMapCB.Checked;
            FillForm();
        }

        private void cubemapCB_CheckedChanged(object sender, EventArgs e)
        {
            materials[current].hasCubeMap = cubemapCB.Checked;
            FillForm();
        }

        private void param1TrackBar_Scroll(object sender, EventArgs e)
        {
            MatParam labels = null;
            propList.TryGetValue(matPropertyNameTB.Text, out labels);
            
            if (labels != null)
            {
                param1TB.Text = ((float)param1TrackBar.Value * labels.max1 / (float)param1TrackBar.Maximum) + "";
            }
        }

        private void param2TrackBar_Scroll(object sender, EventArgs e)
        {
            MatParam labels = null;
            propList.TryGetValue(matPropertyNameTB.Text, out labels);

            if (labels != null)
            {
                param2TB.Text = ((float)param2TrackBar.Value * labels.max2 / (float)param2TrackBar.Maximum) + "";
            }
        }

        private void param3TrackBar_Scroll(object sender, EventArgs e)
        {
            MatParam labels = null;
            propList.TryGetValue(matPropertyNameTB.Text, out labels);

            if (labels != null)
            {
                param3TB.Text = ((float)param3TrackBar.Value * labels.max3 / (float)param3TrackBar.Maximum) + "";
            }
        }

        private void param4TrackBar_Scroll(object sender, EventArgs e)
        {
            MatParam labels = null;
            propList.TryGetValue(matPropertyNameTB.Text, out labels);

            if (labels != null)
            {
                param4TB.Text = ((float)param4TrackBar.Value * labels.max4 / (float)param4TrackBar.Maximum) + "";
            }
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void addMaterialButton_Click(object sender, EventArgs e)
        {
            NUD.Material mat = new NUD.Material(); // beginner's mat
            mat.Flags = 0x94010161;
            mat.cullMode = 0x0405;
            mat.entries.Add("NU_colorSamplerUV", new float[] { 1, 1, 0, 0 });
            mat.entries.Add("NU_fresnelColor", new float[] { 1, 1, 1, 1 });
            mat.entries.Add("NU_blinkColor", new float[] { 0, 0, 0, 0 });
            mat.entries.Add("NU_aoMinGain", new float[] { 0, 0, 0, 0 });
            mat.entries.Add("NU_lightMapColorOffset", new float[] { 0, 0, 0, 0 });
            mat.entries.Add("NU_fresnelParams", new float[] { 1, 0, 0, 0 });
            mat.entries.Add("NU_alphaBlendParams", new float[] { 0, 0, 0, 0 });
            mat.entries.Add("NU_materialHash", new float[] { FileData.toFloat(0x7E538F65), 0, 0, 0 });

            mat.textures.Add(NUD.Polygon.makeDefault()); // diffuse
            mat.textures.Add(NUD.Polygon.makeDefault()); // dummy ramp

            materials.Add(mat);

            FillForm();

            matsComboBox.Items.Clear();
            for (int i = 0; i < materials.Count; i++)
            {
                matsComboBox.Items.Add("Material_" + i);
            }
        }
    }
}
