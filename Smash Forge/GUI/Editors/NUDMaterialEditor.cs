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
        public List<NUD.Material> material;
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
            public int min = -1, max = -1;
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

            public void CreateTrackBar(int param)
            {
                Control control = new TrackBar();
                if (param == 1) { control1 = control;} 
                if (param == 2) control2 = control;
                if (param == 3) control3 = control;
                if (param == 4) control4 = control;
                control = new TrackBar();
                control.Dock = DockStyle.Fill;
                control.MaximumSize = new Size(control.MaximumSize.Width, 16);
                ((TrackBar)control).Minimum = 0;
                ((TrackBar)control).Maximum = 100;
                ((TrackBar)control).TickStyle = TickStyle.Both;
                ((TrackBar)control).TickFrequency = 5;
                ((TrackBar)control).ValueChanged += trackchange;
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
                    { 0x0205, "Cull Front"},
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
            this.material = p.materials;
            Init();
            FillForm();
            comboBox1.SelectedIndex = 0;
        }

        public void InitPropList()
        {
            propList = new Dictionary<string, MatParam>();
            if (File.Exists("param_labels\\material_params.ini"))
            {
                try
                {
                    MatParam p = new MatParam();
                    using (StreamReader sr = new StreamReader("param_labels\\material_params.ini"))
                    {
                        while (!sr.EndOfStream)
                        {
                            string[] args = sr.ReadLine().Split('=');
                            string line = args[0];
                            switch (line)
                            {
                                case "[Param]": if (!p.name.Equals("") && !propList.ContainsKey(p.name)) propList.Add(p.name, p); p = new MatParam(); break;
                                case "name": p.name = args[1]; Console.WriteLine(p.name); break;
                                case "description": p.description = args[1]; break;
                                case "param1": p.ps[0] = args[1]; break;
                                case "param2": p.ps[1] = args[1]; break;
                                case "param3": p.ps[2] = args[1]; break;
                                case "param4": p.ps[3] = args[1]; break;
                            }
                        }
                    }
                    if (!p.name.Equals("") && !propList.ContainsKey(p.name)) propList.Add(p.name, p);
                }
                catch (Exception)
                {
                }
            }
        }

        private void NUDMaterialEditor_Load(object sender, EventArgs e)
        {
            /*// Create a timer with a two second interval.
            System.Timers.Timer aTimer = new System.Timers.Timer((1f / 60) * 1000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;*/
        }


        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if(glControl1 != null)
                RenderTexture();
        }

        public void Init()
        {
            comboBox1.Items.Clear();
            for (int i = 0; i < material.Count; i++)
            {
                comboBox1.Items.Add("Material_" + i);
            }

            tableLayoutPanel2.Enabled = false;

            comboBox7.Items.Clear();
            if (propList == null) InitPropList();
            foreach (string s in propList.Keys)
                comboBox7.Items.Add(s);

            if (comboBox10.Items.Count == 0)
            {
                foreach (int i in srcFactor.Keys)
                    comboBox2.Items.Add(srcFactor[i]);
                foreach (int i in dstFactor.Keys)
                    comboBox3.Items.Add(dstFactor[i]);
                foreach (int i in cullmode.Keys)
                    comboBox6.Items.Add(cullmode[i]);
                foreach (int i in AlphaTest.Keys)
                    AlphaTestCB.Items.Add(AlphaTest[i]);
                foreach (int i in AlphaFunc.Keys)
                    AlphaFuncCB.Items.Add(AlphaFunc[i]);

                foreach (int i in wrapmode.Keys)
                {
                    comboBox10.Items.Add(wrapmode[i]);
                    comboBox8.Items.Add(wrapmode[i]);
                }
                foreach (int i in mapmode.Keys)
                    comboBox9.Items.Add(mapmode[i]);
                foreach (int i in minfilter.Keys)
                    comboBox11.Items.Add(minfilter[i]);
                foreach (int i in magfilter.Keys)
                    comboBox12.Items.Add(magfilter[i]);
                foreach (int i in mip.Keys)
                    comboBox13.Items.Add(mip[i]);
            }
        }

        public void FillForm()
        {
            NUD.Material mat = material[current];

            //mat.flags = mat.RebuildFlags();
            textBox1.Text = mat.flags.ToString("X") + "";
            textBox3.Text = mat.srcFactor + "";
            textBox4.Text = mat.dstFactor + "";
            AlphaTestCB.SelectedItem = AlphaTest[mat.AlphaTest];
            AlphaFuncCB.SelectedItem = AlphaFunc[mat.AlphaFunc];
            textBox6.Text = mat.RefAlpha + "";
            textBox7.Text = mat.cullMode + "";
            textBox8.Text = mat.zBufferOffset + "";

            checkBox1.Checked = mat.unkownWater != 0;

            listView1.Items.Clear();
            
            shadowCB.Checked = mat.hasShadow;
            GlowCB.Checked = mat.glow;
            //sphereMapCB.Checked = mat.useSphereMap;
            dummy_rampCB.Checked = mat.useDummyRamp;
            AOCB.Checked = mat.aomap;
            diffuseCB.Checked = mat.diffuse;
            diffuse2CB.Checked = mat.diffuse2;
            normalCB.Checked = mat.normalmap;
            sphere_mapCB.Checked = mat.useSphereMap;
            cubemapCB.Checked = mat.cubemap;
            stageMapCB.Checked = mat.stagemap;
            rampCB.Checked = mat.ramp;

   
            if (mat.diffuse) listView1.Items.Add("Diffuse");
            if (mat.diffuse2) listView1.Items.Add("Diffuse2");
            if (mat.diffuse3) listView1.Items.Add("Diffuse3");
            if (mat.stagemap) listView1.Items.Add("StageMap");
            if (mat.cubemap) listView1.Items.Add("Cubemap");
            if (mat.spheremap) listView1.Items.Add("SphereMap");
            //if (mat.diffuse2) listView1.Items.Add("Diffuse2");
            if (mat.aomap) listView1.Items.Add("AO Map");
            if (mat.normalmap) listView1.Items.Add("NormalMap");
            if (mat.ramp) listView1.Items.Add("Ramp");
            if (mat.useDummyRamp) listView1.Items.Add("Dummy Ramp");

            while (listView1.Items.Count > mat.textures.Count)
                listView1.Items.RemoveAt(1);

            listView2.Items.Clear();
            listView2.View = View.List;
            foreach (string s in mat.entries.Keys)
            {
                listView2.Items.Add(s);
            }
            if(listView2.Items.Count > 0)
                listView2.SelectedIndices.Add(0);
        }
        
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            current = comboBox1.SelectedIndex;
            FillForm();
            comboBox1.SelectedIndex = current;
        }

        #region DST
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in srcFactor.Keys)
                if (srcFactor[i].Equals(comboBox2.SelectedItem))
                {
                    textBox3.Text = i + "";
                    break;
                }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            setValue(textBox3, comboBox2, srcFactor, out material[current].srcFactor);
        }
        #endregion

        #region SRC
        private void comboBox3_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            foreach (int i in dstFactor.Keys)
                if (dstFactor[i].Equals(comboBox3.SelectedItem))
                {
                    textBox4.Text = i + "";
                    break;
                }
        }

        private void textBox4_TextChanged_1(object sender, EventArgs e)
        {
            setValue(textBox4, comboBox3, dstFactor, out material[current].dstFactor);
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
                if (cullmode[i].Equals(comboBox6.SelectedItem))
                {
                    textBox7.Text = i + "";
                    break;
                }
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            setValue(textBox7, comboBox6, cullmode, out material[current].cullMode);
        }
        #endregion

        #region alpha function
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in AlphaTest.Keys)
                if (AlphaTest[i].Equals(AlphaTestCB.SelectedItem))
                {
                    textBox5.Text = i + "";
                    break;
                }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(textBox5.Text, out material[current].AlphaTest);
        }
        
        private void AlphaFuncCB_SelectedIndexChanged(object sender, EventArgs e)
        {

            foreach (int i in AlphaFunc.Keys)
                if (AlphaFunc[i].Equals(AlphaFuncCB.SelectedItem))
                {
                    Console.WriteLine(AlphaFunc[i] + " " + i);
                    textBox2.Text = i + "";
                    material[current].AlphaFunc = i;
                    break;
                }
        }
        
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(textBox2.Text, out material[current].AlphaFunc);
        }



        #endregion

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                material[current].unkownWater = 0x3A83126f;
            else
                material[current].unkownWater = 0; ;
        }

        #region Textures
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = 0;
            if (listView1.SelectedItems.Count > 0)
            {
                index = listView1.Items.IndexOf(listView1.SelectedItems[0]);
                tableLayoutPanel2.Enabled = true;
                textBox10.Enabled = true;
            }
            else
            {
                tableLayoutPanel2.Enabled = false;
                textBox10.Enabled = false;
            }
            if(index >= material[current].textures.Count)
            {
                MessageBox.Show("Texture doesn't exist");
                return;
            }
            NUD.Mat_Texture tex = material[current].textures[index];
            textBox10.Text = tex.hash.ToString("X");

            comboBox9.SelectedItem = mapmode[tex.MapMode];
            comboBox10.SelectedItem = wrapmode[tex.WrapMode1];
            comboBox8.SelectedItem = wrapmode[tex.WrapMode2];
            comboBox11.SelectedItem = minfilter[tex.minFilter];
            comboBox12.SelectedItem = magfilter[tex.magFilter];
            comboBox13.SelectedItem = mip[tex.mipDetail];
            RenderTexture();
            RenderTextureAlpha();
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            uint f = 0;
            if (uint.TryParse(textBox1.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out f))// && listView1.SelectedIndices.Count > 0
            {
                material[current].flags = f;
                textBox1.BackColor = Color.White;
            }
            else
                textBox1.BackColor = Color.Red;
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            int f = -1;
            int.TryParse(textBox10.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out f);
            if (f != -1 && listView1.SelectedIndices.Count > 0)
                material[current].textures[listView1.SelectedIndices[0]].hash = f;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            int n = -1;
            int.TryParse(textBox6.Text, out n);
            if (n != -1)
            {
                material[current].RefAlpha = n;
            } else
            {
                textBox6.Text = "0";
            }
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            int n = -1;
            int.TryParse(textBox8.Text, out n);
            if (n != -1)
            {
                material[current].zBufferOffset = n;
            }
            else
            {
                textBox8.Text = "0";
            }
        }

        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in mapmode.Keys)
                if (mapmode[i].Equals(comboBox9.SelectedItem))
                {
                    material[current].textures[listView1.SelectedIndices[0]].MapMode = i;
                    break;
                }
        }

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in wrapmode.Keys)
                if (wrapmode[i].Equals(comboBox10.SelectedItem))
                {
                    if (listView1.SelectedItems.Count > 0)
                        material[current].textures[listView1.SelectedIndices[0]].WrapMode1 = i;
                    break;
                }
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in wrapmode.Keys)
                if (wrapmode[i].Equals(comboBox8.SelectedItem))
                {
                    if (listView1.SelectedItems.Count > 0)
                        material[current].textures[listView1.SelectedIndices[0]].WrapMode2 = i;
                    break;
                }
        }

        private void comboBox11_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in minfilter.Keys)
                if (minfilter[i].Equals(comboBox11.SelectedItem))
                {
                    if (listView1.SelectedItems.Count > 0)
                        material[current].textures[listView1.SelectedIndices[0]].minFilter = i;
                    break;
                }
        }

        private void comboBox12_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in magfilter.Keys)
                if (magfilter[i].Equals(comboBox12.SelectedItem))
                {
                    if (listView1.SelectedItems.Count > 0)
                        material[current].textures[listView1.SelectedIndices[0]].magFilter = i;
                    break;
                }
        }

        private void comboBox13_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (int i in mip.Keys)
                if (mip[i].Equals(comboBox13.SelectedItem))
                {
                    if (listView1.SelectedItems.Count > 0)
                        material[current].textures[listView1.SelectedIndices[0]].mipDetail = i;
                    break;
                }
        }
        #endregion

        #region Properties
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedIndices.Count > 0)
                textBox11.Text = material[current].entries.Keys.ElementAt(listView2.SelectedIndices[0]);
            if (textBox11.Text.Equals("NU_materialHash"))
            {
                
                textBox12.Text = BitConverter.ToInt32(BitConverter.GetBytes(material[current].entries[textBox11.Text][0]), 0).ToString("X");
                textBox13.Text = material[current].entries[textBox11.Text][1] + "";
                textBox14.Text = material[current].entries[textBox11.Text][2] + "";
                textBox15.Text = material[current].entries[textBox11.Text][3] + "";
            }
            else
            {
                textBox12.Text = material[current].entries[textBox11.Text][0] + "";
                textBox13.Text = material[current].entries[textBox11.Text][1] + "";
                textBox14.Text = material[current].entries[textBox11.Text][2] + "";
                textBox15.Text = material[current].entries[textBox11.Text][3] + "";
            }
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            if (textBox11.Text.Equals("NU_materialHash"))
            {
                int f = -1;
                int.TryParse(textBox12.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out f);
                if (f != -1 && listView2.SelectedItems.Count > 0)
                    material[current].entries[listView2.SelectedItems[0].Text][0] = BitConverter.ToSingle(BitConverter.GetBytes(f), 0);
            }
            else
            {
                float f = -1;
                float.TryParse(textBox12.Text, out f);
                if (f != -1 && listView2.SelectedItems.Count > 0)
                    material[current].entries[listView2.SelectedItems[0].Text][0] = f;
            }
            updateButton();
        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            float f = -1;
            float.TryParse(textBox13.Text, out f);
            if (f != -1 && listView2.SelectedItems.Count > 0)
                material[current].entries[listView2.SelectedItems[0].Text][1] = f;
            updateButton();
        }

        private void textBox14_TextChanged(object sender, EventArgs e)
        {
            float f = -1;
            float.TryParse(textBox14.Text, out f);
            if (f != -1 && listView2.SelectedItems.Count > 0)
                material[current].entries[listView2.SelectedItems[0].Text][2] = f;
            updateButton();
        }

        private void textBox15_TextChanged(object sender, EventArgs e)
        {
            float f = -1;
            float.TryParse(textBox15.Text, out f);
            if (f != -1 && listView2.SelectedItems.Count > 0)
                material[current].entries[listView2.SelectedItems[0].Text][3] = f;
        }
        #endregion

        // property change
        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            MatParam labels = null;
            propList.TryGetValue(textBox11.Text, out labels);
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
            button4.Enabled = true;

            if (material[current].entries.ContainsKey(comboBox7.Text))
            {
                button4.Enabled = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!comboBox7.Text.Equals(""))
            {
                material[current].entries.Add(comboBox7.Text, new float[] { 0, 0, 0, 0 });
                FillForm();
                button4.Enabled = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if(listView2.SelectedItems.Count > 0)
            {
                material[current].entries.Remove(listView2.SelectedItems[0].Text);
                FillForm();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (material[current].textures.Count < 4)
            {
                material[current].textures.Add(NUD.Polygon.makeDefault());
                FillForm();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0 && material[current].textures.Count > 1)
            {
                material[current].textures.RemoveAt(listView1.Items.IndexOf(listView1.SelectedItems[0]));
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

                        int[] c = NUD.writeMaterial(m, material, s);

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
            MaterialSelector m = new MaterialSelector();
            m.ShowDialog();
            if (m.exitStatus == MaterialSelector.Opened)
            {
                FileData f = new FileData(m.path);

                int soff = f.readInt();

                NUD._s_Poly pol = new NUD._s_Poly()
                {
                    texprop1 = f.readInt(),
                    texprop2 = f.readInt(),
                    texprop3 = f.readInt(),
                    texprop4 = f.readInt()
                };

                int texid = poly.materials[0].textures[0].hash;
                poly.materials = NUD.readMaterial(f, pol, soff);
                poly.materials[0].textures[0].hash = texid;
                material = poly.materials;
                Console.WriteLine(material.Count);
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

            NUT.NUD_Texture tex = null;
            int texture = 0;
            if (material[current].entries.ContainsKey("NU_materialHash") && listView1.SelectedIndices.Count > 0)
            {
                int hash = material[current].textures[listView1.SelectedIndices[0]].hash;

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
                float texureRatioW = tex.width / tex.height;
                float widthPre = texureRatioW * glControl1.Height;
                w = glControl1.Width / widthPre;
                if (texureRatioW > glControl1.AspectRatio)
                {
                    w = 1f;
                    float texureRatioH = tex.height / tex.width;
                    float HeightPre = texureRatioH * glControl1.Width;
                    h = glControl1.Height / HeightPre;
                }
            }
            if (float.IsInfinity(h)) h = 1;
            Console.WriteLine(w + " " + h);

            RenderTools.DrawTexturedQuad(texture, 1, 1, true, true, true, false, false, false);

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

            NUT.NUD_Texture tex = null;
            int texture = 0;
            if (material[current].entries.ContainsKey("NU_materialHash") && listView1.SelectedIndices.Count > 0)
            {
                int hash = material[current].textures[listView1.SelectedIndices[0]].hash;

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
                float texureRatioW = tex.width / tex.height;
                float widthPre = texureRatioW * glControl2.Height;
                w = glControl2.Width / widthPre;
                if (texureRatioW > glControl2.AspectRatio)
                {
                    w = 1f;
                    float texureRatioH = tex.height / tex.width;
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
            if (!comboBox7.Text.Equals(""))
            {
                if(!material[current].entries.ContainsKey(comboBox7.Text))
                    material[current].entries.Add(comboBox7.Text, new float[] { 0,0,0,0 });
                FillForm();
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if(material[current].textures.Count < 4)
            {
                material[current].textures.Add(NUD.Polygon.makeDefault());
                FillForm();
            }
        }

        private void listView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 'd' && listView1.SelectedIndices.Count > 0)
            {
                if(material[current].textures.Count > 1)
                {
                    material[current].textures.RemoveAt(listView1.SelectedIndices[0]);
                    FillForm();
                }
            }
        }

        private void listView2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar == 'd') && listView2.SelectedIndices.Count > 0)
            {
                if (material[current].textures.Count > 1)
                {
                    material[current].entries.Remove(listView2.SelectedItems[0].Text);
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
            colorSelect.BackColor = Color.FromArgb(255,
                Clamp(material[current].entries[listView2.SelectedItems[0].Text][0] * 255),
                Clamp(material[current].entries[listView2.SelectedItems[0].Text][1] * 255),
                Clamp(material[current].entries[listView2.SelectedItems[0].Text][2] * 255));
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
                textBox12.Text = colorDialog1.Color.R / 255f + "";
                textBox13.Text = colorDialog1.Color.G / 255f + "";
                textBox14.Text = colorDialog1.Color.B / 255f + "";
                
            }
        }

        private void listView2_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) && listView2.SelectedIndices.Count > 0)
            {
                if (material[current].textures.Count > 1)
                {
                    material[current].entries.Remove(listView2.SelectedItems[0].Text);
                    FillForm();
                }
            }
        }

        private void rimLightCB_CheckedChanged(object sender, EventArgs e)
        {
            material[current].UseDummyRamp = dummyRampCB.Checked;
            FillForm();
        }

        private void sphereMapCB_CheckedChanged(object sender, EventArgs e)
        {
            material[current].UseSphereMap = sphereMapCB.Checked;
            FillForm();
        }

        private void shadowCB_CheckedChanged(object sender, EventArgs e)
        {
            material[current].hasShadow = shadowCB.Checked;
            FillForm();
        }

        private void GlowCB_CheckedChanged(object sender, EventArgs e)
        {
            material[current].glow = GlowCB.Checked;
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
                NUD.Material mat = material[current];
                foreach (var property in listView2.SelectedItems)
                {
                    mat.entries.Remove(property.ToString());
                }
                e.Handled = true;
            }
        }

        private void diffuseCB_CheckedChanged(object sender, EventArgs e)
        {
            material[current].diffuse = diffuseCB.Checked;
            FillForm();
        }

        private void dummy_rampCB_CheckedChanged(object sender, EventArgs e)
        {
            material[current].UseDummyRamp = dummy_rampCB.Checked;
            FillForm();
        }

        private void diffuse2CB_CheckedChanged(object sender, EventArgs e)
        {
            material[current].diffuse2 = diffuse2CB.Checked;
            FillForm();
        }

        private void normalCB_CheckedChanged(object sender, EventArgs e)
        {
            material[current].normalmap = normalCB.Checked;
            FillForm();
        }

        private void sphere_mapCB_CheckedChanged(object sender, EventArgs e)
        {
            material[current].useSphereMap = sphere_mapCB.Checked;
            FillForm();
        }

        private void rampCB_CheckedChanged(object sender, EventArgs e)
        {
            material[current].ramp = rampCB.Checked;
            FillForm();
        }

        private void AOCB_CheckedChanged(object sender, EventArgs e)
        {
            material[current].aomap = AOCB.Checked;
            FillForm();
        }

        private void stageMapCB_CheckedChanged(object sender, EventArgs e)
        {
            material[current].stagemap = stageMapCB.Checked;
            FillForm();
        }

        private void cubemapCB_CheckedChanged(object sender, EventArgs e)
        {
            material[current].cubemap = cubemapCB.Checked;
            FillForm();
        }
    }
}
