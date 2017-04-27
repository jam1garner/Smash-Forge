using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Input;
using System.Security.Cryptography;
using SALT.Scripting.AnimCMD;
using OpenTK.Graphics;
using System.Diagnostics;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Smash_Forge
{
    public partial class VBNViewport : DockContent
    {
        public static int defaulttex = 0;

        public VBNViewport()
        {
            InitializeComponent();
            this.TabText = "Renderer";
            Hitboxes = new SortedList<int, Hitbox>();
            Application.Idle += Application_Idle;
            Runtime.AnimationChanged += Runtime_AnimationChanged;
            timeSinceSelected.Start();
        }

        // Explicitly unsubscribe from the static event to 
        // prevent hanging Garbage Collection.
        ~VBNViewport()
        {
            Runtime.AnimationChanged -= Runtime_AnimationChanged;
            Application.Idle -= Application_Idle;
        }

        public bool _controlLoaded;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                SetupViewPort();
            }
            render = true;
            _controlLoaded = true;
            defaulttex = NUT.loadImage(Smash_Forge.Resources.Resources.DefaultTexture);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!DesignMode && _controlLoaded)
            {
                GL.LoadIdentity();
                GL.Viewport(glControl1.ClientRectangle);


                v = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) * Matrix4.CreatePerspectiveFieldOfView(Runtime.fov, glControl1.Width / (float)glControl1.Height, 1.0f, 500.0f);
            }
        }

        #region Members
        Matrix4 v;
        float rot = 0;
        float x = 0;
        float lookup = 0;
        float height = 0;
        float width = 0;
        float zoom = 0, nzoom = 0;
        float mouseXLast = 0;
        float mouseYLast = 0;
        float mouseSLast = 0;
        bool render = false;
        bool isPlaying = false;
        bool fpsView = false;
        public Stopwatch timeSinceSelected = new Stopwatch();
        Shader shader;
        #endregion

        #region Event Handlers
        private void Application_Idle(object sender, EventArgs e)
        {
            // Fix accessing the control after 
            // the editor has been closed 
            if (Runtime.killWorkspace)
            {
                foreach (ModelContainer n in Runtime.ModelContainers)
                {
                    n.Destroy();
                }
                foreach (NUT n in Runtime.TextureContainers)
                {
                    //n.Destroy();
                }
                Runtime.ModelContainers = new List<ModelContainer>();
                Runtime.TextureContainers = new List<NUT>();
                Runtime.TargetVBN = null;
                Runtime.TargetAnim = null;
                Runtime.TargetLVD = null;
                Runtime.TargetPath = null;
                Runtime.TargetCMR0 = null;
                Runtime.TargetNUD = null;
                Runtime.killWorkspace = false;
                Runtime.Animations = new Dictionary<string, SkelAnimation>();
                MainForm.Instance.lvdList.fillList();
                MainForm.animNode.Nodes.Clear();
                MainForm.Instance.mtaNode.Nodes.Clear();
                MainForm.Instance.meshList.refresh();
                MainForm.Instance.paramEditors = new List<PARAMEditor>();
                MainForm.Instance.lvdEditor.Clear();
                MainForm.Instance.boneTreePanel.Clear();
                string acmdpath = Path.Combine(MainForm.executableDir, "workspace/animcmd/");
                if (Directory.Exists(acmdpath))
                {
                    foreach (string file in Directory.EnumerateFiles(acmdpath))
                        File.Delete(file);
                    Directory.Delete(acmdpath);
                }

                MainForm.Instance.project.fillTree();

                GC.Collect();
            }

            //if (Runtime.ModelContainers.Count > 0)
            //    MainForm.Instance.exportModelToolStripMenuItem.Visible = true;
            //else
            //    MainForm.Instance.exportModelToolStripMenuItem.Visible = false;

            if (this.IsDisposed == true)
                return;

            while (render && _controlLoaded && glControl1.IsIdle)
            {
                if (Smash_Forge.Update.Downloaded)
                    MainForm.Instance.pictureBox1.Image = Resources.Resources.sexy_green_down_arrow;
                
                if (Keyboard.GetState().IsKeyDown(Key.S)
                    && Keyboard.GetState().IsKeyDown(Key.K)
                    && Keyboard.GetState().IsKeyDown(Key.A)
                    && Keyboard.GetState().IsKeyDown(Key.P)
                    && Keyboard.GetState().IsKeyDown(Key.O)
                    && Keyboard.GetState().IsKeyDown(Key.N))
                {
                    DialogResult dialogResult = MessageBox.Show("Activate Skapon?", "Skapon Code", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        foreach (ModelContainer m in Runtime.ModelContainers)
                        {
                            if (m.vbn != null && m.nud == null)
                                m.nud = Skapon.Create(m.vbn);
                        }
                    }
                }

                if (isPlaying)
                {
                    if (nupdFrame.Value < nupdMaxFrame.Value)
                    {
                        nupdFrame.Value++;
                    }
                    else if (nupdFrame.Value == nupdMaxFrame.Value)
                    {
                        if (!checkBox2.Checked)
                        {
                            isPlaying = false;
                            btnPlay.Text = "Play";
                        }
                        else
                        {
                            nupdFrame.Value = 0;
                        }
                    }
                }
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                Render();
                stopWatch.Stop();
                if((1000 / AnimationSpeed) - stopWatch.ElapsedMilliseconds > 0)
                    System.Threading.Thread.Sleep((int)((1000 / AnimationSpeed) - stopWatch.ElapsedMilliseconds));
            }
        }
        private void Runtime_AnimationChanged(object sender, EventArgs e)
        {
            loadAnimation(Runtime.TargetAnim);
        }

        private void btnFirstFrame_Click(object sender, EventArgs e)
        {
            this.nupdFrame.Value = 0;
        }
        private void btnPrevFrame_Click(object sender, EventArgs e)
        {
            if (this.nupdFrame.Value - 1 >= 0)
                this.nupdFrame.Value -= 1;
        }
        private void btnLastFrame_Click(object sender, EventArgs e)
        {
            if (Runtime.TargetAnim != null)
                this.nupdFrame.Value = Runtime.TargetAnim.size() - 1;
        }
        private void btnNextFrame_Click(object sender, EventArgs e)
        {
            if (Runtime.TargetAnim != null)
                this.nupdFrame.Value += 1;
        }
        private void btnPlay_Click(object sender, EventArgs e)
        {
            // If we're already at final frame and we hit play again
            // start from the beginning of the anim
            if (nupdMaxFrame.Value == nupdFrame.Value)
                nupdFrame.Value = 0;

            isPlaying = !isPlaying;
            if (isPlaying)
            {
                btnPlay.Text = "Pause";
            }
            else
            {
                btnPlay.Text = "Play";
            }
        }
        private void nupdFrame_ValueChanged(Object sender, EventArgs e)
        {
            if (Runtime.TargetAnim == null)
                return;

            if (this.nupdFrame.Value >= Runtime.TargetAnim.size())
            {
                this.nupdFrame.Value = 0;
                return;
            }
            if (this.nupdFrame.Value < 0)
            {
                this.nupdFrame.Value = Runtime.TargetAnim.size() - 1;
                return;
            }
            Runtime.TargetAnim.setFrame((int)this.nupdFrame.Value);

            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                Runtime.TargetAnim.setFrame((int)this.nupdFrame.Value);
                if (m.vbn != null)
                    Runtime.TargetAnim.nextFrame(m.vbn);

                if (m.dat_melee != null)
                {
                    Runtime.TargetAnim.setFrame((int)this.nupdFrame.Value);
                    Runtime.TargetAnim.nextFrame(m.dat_melee.bones);
                }
                if (m.bch != null)
                {
                    foreach (BCH.BCH_Model mod in m.bch.models)
                    {
                        Runtime.TargetAnim.setFrame((int)this.nupdFrame.Value);
                        if (mod.skeleton != null)
                            Runtime.TargetAnim.nextFrame(mod.skeleton);
                    }
                }
            }

            Frame = (int)this.nupdFrame.Value;

            if (scr_game != null)
                ProcessFrame();
        }
        private void nupdSpeed_ValueChanged(object sender, EventArgs e)
        {
            AnimationSpeed = (int)nupdFrameRate.Value;
        }

        System.Drawing.Point _LastPoint = System.Drawing.Point.Empty;
        private void glControl1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!freezeCamera)
                if (!fpsView)
                    UpdateMousePosition();
            if (_LastPoint != e.Location) // can add some slack by checking the distance
            {
                dbdistance = 0;
            }
            if(Runtime.TargetVBN != null && freezeCamera)
            {
                ((Bone)MainForm.Instance.boneTreePanel.treeView1.SelectedNode).rot += Quaternion.FromAxisAngle(Vector3.UnitX, 2);
                Runtime.TargetVBN.update();
            }
        }

        public event EventHandler FrameChanged;
        protected virtual void OnFrameChanged(EventArgs e)
        {
            if (FrameChanged != null)
                FrameChanged(this, e);
            //HandleACMD(Runtime.TargetAnimString);
        }
        #endregion

        #region Properties
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal int Frame
        {
            get
            {
                return _frame;
            }
            set
            {
                _frame = value;
                OnFrameChanged(new EventArgs());
            }
        }
        private int _frame = 0;

        [DefaultValue(60)]
        internal int AnimationSpeed { get { return _animSpeed; } set { _animSpeed = value; } }
        private int _animSpeed = 60;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private SortedList<int, Hitbox> Hitboxes { get; set; }
        #endregion

        #region Rendering

        public static string vs = @"#version 330

in vec3 vPosition;
in vec4 vColor;
in vec3 vNormal;
in vec3 vTangent;
in vec3 vBiTangent;
in vec2 vUV;
in vec4 vBone;
in vec4 vWeight;

out vec3 pos;
out vec3 tan;
out vec3 bit;
out vec4 color;
out vec2 texcoord;
out vec3 normal;
out vec3 fragpos;

uniform vec4 colorSamplerUV;
uniform mat4 eyeview;
uniform uint flags;

uniform bones
{
    mat4 transforms[200];
} bones_;

uniform int renderType;


vec4 skin(vec3 po, ivec4 index);
vec3 skinNRM(vec3 po, ivec4 index);
 
vec4 skin(vec3 po, ivec4 index)
{
    vec4 oPos = vec4(po.xyz, 1.0);

    oPos = bones_.transforms[index.x] * vec4(po, 1.0) * vWeight.x;
    oPos += bones_.transforms[index.y] * vec4(po, 1.0) * vWeight.y;
    oPos += bones_.transforms[index.z] * vec4(po, 1.0) * vWeight.z;
    oPos += bones_.transforms[index.w] * vec4(po, 1.0) * vWeight.w;

    return oPos;
}

vec3 skinNRM(vec3 nr, ivec4 index)
{
    vec3 nrmPos = vec3(0);
	
    if(vWeight.x != 0.0) nrmPos = mat3(bones_.transforms[index.x]) * nr * vWeight.x;
    if(vWeight.y != 0.0) nrmPos += mat3(bones_.transforms[index.y]) * nr * vWeight.y;
    if(vWeight.z != 0.0) nrmPos += mat3(bones_.transforms[index.z]) * nr * vWeight.z;
    if(vWeight.w != 0.0) nrmPos += mat3(bones_.transforms[index.w]) * nr * vWeight.w;

    return nrmPos;
}

void main()
{
    vec4 objPos = vec4(vPosition.xyz, 1.0);
    ivec4 bi = ivec4(vBone); 

    if(vBone.x != -1.0) objPos = skin(vPosition, bi);

    objPos = eyeview * vec4(objPos.xyz, 1.0);

    gl_Position = objPos;

    texcoord = vec2((vUV * colorSamplerUV.xy) + colorSamplerUV.zw);
    normal = vec3(0,0,0);
    tan = vTangent;
    bit = vBiTangent;
    color = vec4(vNormal, 1);
    pos = vec3(vPosition * mat3(eyeview));

    if(renderType != 1){

	if(renderType == 2){
        	float normal = dot(vec4(vNormal * mat3(eyeview), 1.0), vec4(0.15,0.15,0.15,1.0)) ;
        	color = vec4(normal, normal, normal, 1);
	}
        else
            color = vColor;

	fragpos = objPos.xyz;

	if(vBone.x != -1.0) 
		normal = normalize((skinNRM(vNormal.xyz, bi)).xyz) ; //  * -1 * mat3(eyeview)
	else
		normal = vNormal ;
    }
}";

        public static string fs = @"#version 330

in vec3 pos;
in vec2 texcoord;
in vec4 color;
in vec3 normal;
in vec3 tan;
in vec3 bit;
in vec3 fragpos;

// Textures 
uniform sampler2D dif;
uniform sampler2D dif2;
uniform sampler2D ramp;
uniform sampler2D nrm;
uniform sampler2D cube;
uniform sampler2D ao;

uniform int hasDif;
uniform int hasDif2;
uniform int hasStage;
uniform int hasCube;
uniform int hasOo;
uniform int hasNrm;
uniform int hasRamp;
uniform int hasRim;

// other textures
uniform samplerCube cmap;

// Da Flags
uniform uint flags;
uniform int isTransparent;

// Properties
uniform vec4 colorSamplerUV;
uniform vec4 colorOffset;
uniform vec4 minGain;

uniform vec4 fresnelColor;
uniform vec4 specularColor;
uniform vec4 specularColorGain;
uniform vec4 diffuseColor;
uniform vec4 colorGain;
uniform vec4 finalColorGain;
uniform vec4 reflectionColor;

// params
uniform vec4 fresnelParams;
uniform vec4 specularParams;
uniform vec4 reflectionParams;

uniform int renderType;
uniform int renderLighting;
uniform int renderVertColor;
uniform int renderNormal;

uniform int renderDiffuse;
uniform int renderSpecular;
uniform int renderFresnel;
uniform int renderReflection;

uniform int useNormalMap;

uniform float gamma = 1.2; 

uniform mat4 eyeview;

const vec3 lightPos = vec3(0,0,-50);
const vec3 specPos = vec3(0,-10,-40);

const uint Glow         = 0x00000080u;
const uint Shadow       = 0x00000040u;
const uint RIM          = 0x00000020u;
const uint UnknownTex   = 0x00000010u;

const uint AOMap        = 0x00000008u;
const uint CubeMap      = 0x00000004u;
const uint NormalMap    = 0x00000002u;
const uint DiffuseMap   = 0x00000001u;

vec3 lerp(float v, vec3 from, vec3 to)
{
    return from + (to - from) * v;
}

vec3 CalcBumpedNormal()
{
    // if no normal map, then return just the normal
    if(hasNrm == 0 || useNormalMap == 0)
	return normal;

    vec3 Normal = normalize(normal);

    //vec3 Tangent = normalize(texture2D(nrm, texcoord).xyz);
    //Tangent = normalize(Tangent - dot(Tangent, Normal) * Normal);
    //vec3 Bitangent = cross(Tangent, Normal);

    vec3 Tangent = tan;
    vec3 Bitangent = bit;
    vec3 BumpMapNormal = texture2D(nrm, texcoord).xyz;
    BumpMapNormal = 2.0 * BumpMapNormal - vec3(1.0, 1.0, 1.0);
    vec3 NewNormal;
    mat3 TBN = mat3(Tangent, Bitangent, Normal);
    NewNormal = TBN * BumpMapNormal;
    NewNormal = normalize(NewNormal);
    return NewNormal;
}

vec4 CalculateDiffuse(vec3 norm)
{
	// diffuse
	vec3 difDir = normalize(-fragpos * mat3(eyeview));  
	float diff = clamp(pow(dot(norm, difDir), 0.15f), 0.5, 2.0);
	vec3 diffuse = vec3(diff);

	if(hasRamp == 1){
		diffuse = texture2D(ramp, vec2(clamp(1-diff, 0.1, 0.9), 0.5f)).rgb;
	}

	diffuse *= diffuseColor.www * diffuseColor.xyz;
	//diffuse = diffuseColor.rgb * (1 / (1.0 + (0.25 * difDir * difDir)));
	if(diffuse == vec3(0,0,0)) diffuse = vec3(1);

	vec3 fin = vec3(0);
	if(renderDiffuse == 1)
		fin += diffuse;

	return vec4(fin.xyz, fresnelColor.a);
}

vec3 CalculateFresnel(vec3 norm){
	vec3 difDir = normalize(-fragpos * mat3(eyeview));  
	float diff = clamp(pow(dot(norm, difDir), 0.5f), 0.4, 1.0);
	vec3 f = (normalize(-fragpos));
	f.z -= 0.8;
	f.y -= 0.2;
	vec3 lightDir = normalize(f * mat3(eyeview));  // vec3(0,-0.3,-0.7)
	vec3 F0 = vec3(0); //fresnelParams.w 
	float cosTheta = dot(lightDir, normal);
	vec3 fresnel = F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0); // 5.0

	float fdiv = min(1.0 + fresnelParams.x / 5.0, 1.0);
	return max(fresnel.xyz*fresnelColor.aaa*fresnelColor.rgb / fdiv, vec3(0.0, 0.0, 0.0)); 
}

vec3 CalculateReflection(vec3 norm){
// reflection
	vec3 cameraPosition = (transpose(mat3(eyeview)) * eyeview[3].xyz);
	cameraPosition.y += 20.0;
	float ratio = 1.0 / 1.0;
	vec3 I = normalize(fragpos - cameraPosition);
    	//vec3 R = refract(I, norm, ratio);
	vec3 R = reflect(I, norm);
	R.y *= -1.0;
    	vec3 refColor = texture(cmap, R).rgb * reflectionColor.rgb;

	return refColor;
}

vec3 CalculateSpecular(vec3 norm){

// specular
	vec3 cameraPosition = (transpose(mat3(eyeview)) * eyeview[3].xyz);
	cameraPosition.y += 20.0;
	vec3 specDir = vec3(0,0,1) * mat3(eyeview);  
	//specDir = normalize(vec3(0,0,1) * mat3(eyeview));  

	vec3 viewDir = normalize(-fragpos); //pos - 
	//vec3 halfDir = normalize(normalize(cameraPosition) + viewDir);
	//float sp = pow(max(dot(halfDir, normalize(norm)), 0.0), 16.0);

	vec3 reflectDir = reflect(-specDir, norm);
	float specDrop = max(specularParams.y / 50.0, 0.1);
	float spec = pow(max(dot(specDir, reflectDir), 0.0), 2.0 + specularParams.z) * specDrop;
	float div = max(specularParams.x/10.0, 1.0);
	vec3 specular = vec3(spec);
	//if(hasRamp==1 && spec>0.05) specular = texture2D(ramp, vec2(spec*spec, 0.5f)).rgb;
	specular *= (specularColor.xyz) * specularColor.www / div;

	if((flags & UnknownTex) > 0u)
	{
		return vec3(spec);
	}
	
	return specular + specular * specularColorGain.xyz;
}

void
main()
{
	// if the renderer wants to show something other than textures
    if(renderType == 1 || renderType == 2 || renderType == 3){
        gl_FragColor = color;
    } else {

	// calcuate final color by mixing with vertex color
	vec4 fincol = vec4(0);

	if(hasDif == 1){
		fincol = texture2D(dif, texcoord);

		if(hasDif2 == 1){
			fincol = texture2D(dif2, texcoord);
			//fincol = mix(fincol, texture2D(nrm, texcoord), texture2D(nrm, texcoord).a);
			fincol = mix(fincol, texture2D(dif, texcoord), texture2D(dif, texcoord).a);
			fincol.a = 1.0;
		}
	}

	float a = fincol.a;

	if(renderVertColor == 1) fincol = fincol * color;

	// color gains
	fincol = (colorOffset + fincol * colorGain);

	// diffuse specular and fresnel
	vec3 norm = CalcBumpedNormal();
        if(renderNormal == 1){
		if(renderLighting == 1){
			// ambient occlusion
			vec4 difc = CalculateDiffuse(norm);
			if(renderSpecular == 1) difc.xyz += CalculateSpecular(norm);
			if(renderReflection == 1) difc.xyz += CalculateReflection(norm);
			vec4 ao = ((minGain + texture2D(nrm, texcoord).aaaa));
			fincol *= ao;
			fincol *= difc.rgba;
			//if(flags & UnknownTex) a = difc.a;
			if(renderFresnel == 1) fincol.xyz += CalculateFresnel(norm) * texture2D(dif, texcoord).rgb;
		}
    		else
    		{
        		// old lighting
            		float normal = dot(vec4(normal * mat3(eyeview), 1.0), vec4(0.15,0.15,0.15,1.0)) ;
            		fincol *= normal;
			vec4 ao = ((minGain + texture2D(nrm, texcoord).aaaa));
			fincol *= ao;
        	}
    	}

    fincol = fincol * (finalColorGain);

	// correct alpha
	fincol.a = a;
	if(isTransparent == 0) fincol.a *= color.a;
	fincol.a *= finalColorGain.a;
	
	// gamma correction
	fincol.rgb = pow(fincol.rgb, vec3(1.0/gamma));

    	gl_FragColor = fincol;
    }
}";
        
        public int ubo_bones, ubo_bonesIT;
        public static int cubeTex;

        private void SetupViewPort()
        {
            cubeTex = RenderTools.LoadCubeMap();

            if (shader != null)
                GL.DeleteShader(shader.programID);

            if (shader == null)
            {
                GL.GenBuffers(1, out ubo_bones);
                GL.GenBuffers(1, out ubo_bonesIT);

                shader = new Shader();

                {
                    shader.vertexShader(vs);
                    shader.fragmentShader(fs);
                }
            }

            int h = Height - groupBox2.ClientRectangle.Top;
            int w = Width;
            GL.LoadIdentity();
            GL.Viewport(glControl1.ClientRectangle);
            v = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) 
                * Matrix4.CreatePerspectiveFieldOfView(Runtime.fov, glControl1.Width / (float)glControl1.Height, 1.0f, Runtime.renderDepth);
            //v2 = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom);

        }


        int cf = 0;
        Color back1 = Color.FromArgb((255 << 24) | (26 << 16) | (26 << 8) | (26));
        Color back2 = Color.FromArgb((255 << 24) | (77 << 16) | (77 << 8) | (77));
        //Matrix4 v2;

        public void Render()
        {
            if (!render)
                return;

            glControl1.MakeCurrent();

            GL.Viewport(glControl1.ClientRectangle);

            //GL.ClearColor(back1);
            // Push all attributes so we don't have to clean up later
            GL.PushAttrib(AttribMask.AllAttribBits);
            // clear the gf buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            if (Runtime.renderBackGround)
            {
                GL.Begin(PrimitiveType.Quads);
                GL.Color3(back1);
                GL.Vertex2(1.0, 1.0);
                GL.Vertex2(-1.0, 1.0);
                GL.Color3(back2);
                GL.Vertex2(-1.0, -1.0);
                GL.Vertex2(1.0, -1.0);
                GL.End();
            }
            //RenderTools.RenderCubeMap(v);

            //GL.DepthFunc(DepthFunction.Never);
            GL.Enable(EnableCap.DepthTest);
            GL.ClearDepth(1.0);

            // set up the viewport projection and send it to GPU
            GL.MatrixMode(MatrixMode.Projection);

            if (IsMouseOverViewport() && glControl1.Focused && !freezeCamera)
            {
                if (fpsView)
                    FPSCamera();
                else
                    UpdateMousePosition();
            }
            mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;

            SetCameraAnimation();

            GL.LoadMatrix(ref v);
            // ready to start drawing model stuff
            GL.MatrixMode(MatrixMode.Modelview);
            
            GL.UseProgram(0);

            // drawing floor---------------------------
            if (Runtime.renderFloor)
                RenderTools.drawFloor(Matrix4.CreateTranslation(Vector3.Zero));


            //GL.Enable(EnableCap.LineSmooth); // This is Optional 
            GL.Enable(EnableCap.Normalize);  // These is critical to have
            GL.Enable(EnableCap.RescaleNormal);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Gequal, 0.1f);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            GL.Enable(EnableCap.LineSmooth);

            GL.Enable(EnableCap.StencilTest);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);


            // draw models
            if (Runtime.renderModel)
                DrawModels();

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.DepthFunc(DepthFunction.Less);
            GL.AlphaFunc(AlphaFunction.Gequal, 0.1f);
            GL.Disable(EnableCap.CullFace);

            GL.UseProgram(0);
            // draw path.bin
            if (Runtime.renderPath)
                DrawPathDisplay();
            // clear the buffer bit so the skeleton 
            // will be drawn on top of everything
            GL.Clear(ClearBufferMask.DepthBufferBit);
            // draw lvd
            if (Runtime.renderLVD)
                DrawLVD();
            // drawing the bones
            DrawBones();

            /*GL.LineWidth(3f);
            GL.Color3(freezeCamera ? Color.Yellow : Color.White);
            RenderTools.drawCircle(new Vector3(6, 6, 6), 5, 30);

            Cursor.Current = freezeCamera ? Cursors.VSplit : Cursors.Default;*/

            // Clean up
            GL.PopAttrib();
            glControl1.SwapBuffers();
        }

        public void UpdateMousePosition()
        {
            float zoomscale = 1;

            if ((OpenTK.Input.Mouse.GetState().RightButton == OpenTK.Input.ButtonState.Pressed))
            {
                height += 0.025f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                width += 0.025f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
            }
            if ((OpenTK.Input.Mouse.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed))
            {
                rot += 0.025f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
                lookup += 0.025f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
            }

            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ShiftLeft) || OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ShiftRight))
                zoomscale = 6;

            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.Down))
                zoom -= 1 * zoomscale;
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.Up))
                zoom += 1 * zoomscale;

            mouseXLast = OpenTK.Input.Mouse.GetState().X;
            mouseYLast = OpenTK.Input.Mouse.GetState().Y;

            zoom += (OpenTK.Input.Mouse.GetState().WheelPrecise - mouseSLast) * zoomscale;

            v = Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) * Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) 
                * Matrix4.CreatePerspectiveFieldOfView(Runtime.fov, glControl1.Width / (float)glControl1.Height, 1.0f, Runtime.renderDepth);
            //v2 = Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) * Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup);
        }
        public bool IsMouseOverViewport()
        {
            if (glControl1.ClientRectangle.Contains(PointToClient(Cursor.Position)))
                return true;
            else
                return false;
        }

        private void SetCameraAnimation()
        {
            if (Runtime.TargetPath != null && checkBox1.Checked)
            {
                if (cf >= Runtime.TargetPath.Frames.Count)
                    cf = 0;
                pathFrame f = Runtime.TargetPath.Frames[cf];
                v = (Matrix4.CreateTranslation(f.x, f.y, f.z) * Matrix4.CreateFromQuaternion(new Quaternion(f.qx, f.qy, f.qz, f.qw))).Inverted() 
                    * Matrix4.CreatePerspectiveFieldOfView(Runtime.fov, glControl1.Width / (float)glControl1.Height, 1.0f, 90000.0f);
                cf++;
            }
            else if (Runtime.TargetCMR0 != null && checkBox1.Checked)
            {
                if (cf >= Runtime.TargetCMR0.frames.Count)
                    cf = 0;
                Matrix4 m = Runtime.TargetCMR0.frames[cf].Inverted();
                v = Matrix4.CreateTranslation(m.M14, m.M24, m.M34) * Matrix4.CreateFromQuaternion(m.ExtractRotation()) 
                    * Matrix4.CreatePerspectiveFieldOfView(Runtime.fov, glControl1.Width / (float)glControl1.Height, 1.0f, 90000.0f);
                cf++;
            }
        }

        private void DrawModels()
        {
            // Bounding Box Render
            if(Runtime.renderBoundingBox)
            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if (m.nud != null)
                {
                    RenderTools.drawCubeWireframe(new Vector3(m.nud.param[0], m.nud.param[1], m.nud.param[2]), m.nud.param[3]);
                    foreach (NUD.Mesh mesh in m.nud.mesh)
                    {
                        if(mesh.Checked)
                            RenderTools.drawCubeWireframe(new Vector3(mesh.bbox[0], mesh.bbox[1], mesh.bbox[2]), mesh.bbox[3]);
                    }
                }
            }

            shader = Runtime.shaders["NUD"];
            
            GL.UseProgram(shader.programID);
            int rt = (int)Runtime.renderType;
            if(rt == 0)
            {
                if (Runtime.renderNormals)
                    rt = rt | (0x10);
                if (Runtime.renderVertColor)
                    rt = rt | (0x20);
            }
            GL.Uniform1(shader.getAttribute("renderType"), rt);
            GL.Uniform1(shader.getAttribute("renderLighting"), Runtime.renderLighting ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderVertColor"), Runtime.renderVertColor ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderNormal"), Runtime.renderNormals ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderDiffuse"), Runtime.renderDiffuse ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderFresnel"), Runtime.renderFresnel ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderSpecular"), Runtime.renderSpecular ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderReflection"), Runtime.renderReflection ? 1 : 0);
            GL.Uniform1(shader.getAttribute("useNormalMap"), Runtime.useNormalMap ? 1 : 0);

            GL.Uniform1(shader.getAttribute("gamma"), Runtime.gamma);

            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if (m.bch != null)
                {
                    if (m.bch.mbn != null)
                    {
                        m.bch.mbn.Render(v);
                    }
                }

                if (m.dat_melee != null)
                {
                    m.dat_melee.Render(v);
                }

                if (m.nud != null)
                {
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.TextureCubeMap, cubeTex);
                    GL.Uniform1(shader.getAttribute("cmap"), 2);
                    GL.UniformMatrix4(shader.getAttribute("eyeview"), false, ref v);
                    //GL.UniformMatrix4(shader.getAttribute("modelview"), false, ref v2);

                    if (m.vbn != null)
                    {
                        Matrix4[] f = m.vbn.getShaderMatrix();
                        /*int shad = shader.getAttribute("bones");
                        GL.UniformMatrix4(shad, f.Length, false, ref f[0].Row0.X);*/
                        //int shadi = shader.getAttribute("boneIT");
                        //GL.UniformMatrix4(shadi, f.Length, false, ref m.vbn.bonematIT[0].Row0.X);

                        int maxUniformBlockSize = GL.GetInteger(GetPName.MaxUniformBlockSize);
                        int boneCount = m.vbn.bones.Count;
                        int dataSize = boneCount * Vector4.SizeInBytes * 4;

                        GL.BindBuffer(BufferTarget.UniformBuffer, ubo_bones);
                        GL.BufferData(BufferTarget.UniformBuffer, (IntPtr)(dataSize), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                        GL.BindBuffer(BufferTarget.UniformBuffer, 0);

                        var blockIndex = GL.GetUniformBlockIndex(shader.programID, "bones");
                        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, blockIndex, ubo_bones);

                        //GL.BindBuffer(BufferTarget.UniformBuffer, ubo_bonesIT);
                        //GL.BufferData(BufferTarget.UniformBuffer, (IntPtr)(dataSize), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                        //GL.BindBuffer(BufferTarget.UniformBuffer, 0);
                        //blockIndex = GL.GetUniformBlockIndex(shader.programID, "bonesIT");
                        //GL.BindBufferBase(BufferRangeTarget.UniformBuffer, blockIndex, ubo_bonesIT);

                        if (f.Length > 0)
                        {
                            GL.BindBuffer(BufferTarget.UniformBuffer, ubo_bones);
                            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, (IntPtr)(f.Length * Vector4.SizeInBytes * 4), f);

                            //GL.BindBuffer(BufferTarget.UniformBuffer, ubo_bonesIT);
                            //GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)(f.Length * Vector4.SizeInBytes * 4), (IntPtr)(f.Length * Vector4.SizeInBytes * 4), m.vbn.bonematIT);
                        }
                    }

                    shader.enableAttrib();
                    m.nud.clearMTA();//Clear animated materials

                    if (m.mta != null)
                        m.nud.applyMTA(m.mta, (int)nupdFrame.Value);//Apply base mta
                    if (Runtime.TargetMTA != null)
                        m.nud.applyMTA(Runtime.TargetMTA, (int)nupdFrame.Value);//Apply additional mta (can override base)

                    m.nud.Render(shader);
                    shader.disableAttrib();

                    //m.nud.DrawPoints(v, m.vbn);
                }
            }
        }

        private void DrawBones()
        {
            if (Runtime.ModelContainers.Count > 0)
            {
                // Render the hitboxes
                if (!string.IsNullOrEmpty(Runtime.TargetAnimString))
                    HandleACMD(Runtime.TargetAnimString.Substring(3));

                if (Runtime.renderHitboxes)
                    RenderHitboxes();

                if (Runtime.renderHurtboxes)
                    RenderHurtboxes();

                foreach (ModelContainer m in Runtime.ModelContainers)
                {
                    RenderTools.DrawVBN(m.vbn);
                    if (m.bch != null)
                    {
                        RenderTools.DrawVBN(m.bch.models[0].skeleton);
                    }

                    if (m.dat_melee != null)
                    {
                        RenderTools.DrawVBN(m.dat_melee.bones);
                    }
                }
            }
        }

        public void DrawVBNDiamond(VBN vbn)
        {
            if (vbn != null && Runtime.renderBones)
            {
                foreach (Bone bone in vbn.bones)
                {
                    // first calcuate the point and draw a point
                    GL.Color3(Color.DarkGray);
                    GL.PointSize(1f);

                    Vector3 pos_c = Vector3.Transform(Vector3.Zero, bone.transform);

                    GL.Begin(PrimitiveType.LineLoop);
                    GL.Vertex3(new Vector3(pos_c.X - 0.1f, pos_c.Y, pos_c.Z - 0.1f));
                    GL.Vertex3(new Vector3(pos_c.X + 0.1f, pos_c.Y, pos_c.Z - 0.1f));
                    GL.Vertex3(new Vector3(pos_c.X + 0.1f, pos_c.Y, pos_c.Z + 0.1f));
                    GL.Vertex3(new Vector3(pos_c.X - 0.1f, pos_c.Y, pos_c.Z + 0.1f));
                    GL.End();

                    Vector3 pos_p = pos_c;
                    if (bone.parentIndex != 0x0FFFFFFF && bone.parentIndex != -1)
                    {
                        int i = bone.parentIndex;
                        pos_p = Vector3.Transform(Vector3.Zero, vbn.bones[i].transform);
                    }

                    GL.Color3(Color.Gray);
                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex3(new Vector3(pos_c.X - 0.1f, pos_c.Y, pos_c.Z - 0.1f));
                    GL.Vertex3(new Vector3(pos_c.X, pos_c.Y + 0.25f, pos_c.Z));
                    GL.Vertex3(new Vector3(pos_c.X + 0.1f, pos_c.Y, pos_c.Z - 0.1f));
                    GL.Vertex3(new Vector3(pos_c.X, pos_c.Y + 0.25f, pos_c.Z));
                    GL.Vertex3(new Vector3(pos_c.X + 0.1f, pos_c.Y, pos_c.Z + 0.1f));
                    GL.Vertex3(new Vector3(pos_c.X, pos_c.Y + 0.25f, pos_c.Z));
                    GL.Vertex3(new Vector3(pos_c.X - 0.1f, pos_c.Y, pos_c.Z + 0.1f));
                    GL.Vertex3(new Vector3(pos_c.X, pos_c.Y + 0.25f, pos_c.Z));
                    GL.Vertex3(new Vector3(pos_c.X - 0.1f, pos_c.Y, pos_c.Z - 0.1f));
                    GL.Vertex3(new Vector3(pos_c.X, pos_c.Y - 0.25f, pos_c.Z));
                    GL.Vertex3(new Vector3(pos_c.X + 0.1f, pos_c.Y, pos_c.Z - 0.1f));
                    GL.Vertex3(new Vector3(pos_c.X, pos_c.Y - 0.25f, pos_c.Z));
                    GL.Vertex3(new Vector3(pos_c.X + 0.1f, pos_c.Y, pos_c.Z + 0.1f));
                    GL.Vertex3(new Vector3(pos_c.X, pos_c.Y - 0.25f, pos_c.Z));
                    GL.Vertex3(new Vector3(pos_c.X - 0.1f, pos_c.Y, pos_c.Z + 0.1f));
                    GL.Vertex3(new Vector3(pos_c.X, pos_c.Y - 0.25f, pos_c.Z));


                    GL.Vertex3(new Vector3(pos_c.X - 0.1f, pos_c.Y, pos_c.Z - 0.1f));
                    GL.Vertex3(pos_p);
                    GL.Vertex3(new Vector3(pos_c.X + 0.1f, pos_c.Y, pos_c.Z - 0.1f));
                    GL.Vertex3(pos_p);
                    GL.Vertex3(new Vector3(pos_c.X + 0.1f, pos_c.Y, pos_c.Z + 0.1f));
                    GL.Vertex3(pos_p);
                    GL.Vertex3(new Vector3(pos_c.X - 0.1f, pos_c.Y, pos_c.Z + 0.1f));
                    GL.Vertex3(pos_p);

                    GL.End();
                }
            }
        }
        
        private static Color getLinkColor(DAT.COLL_DATA.Link link)
        {
            if ((link.flags & 1) != 0)
                return Color.FromArgb(128, Color.Yellow);
            if ((link.collisionAngle & 4) + (link.collisionAngle & 8) != 0)
                return Color.FromArgb(128, Color.Lime);
            if ((link.collisionAngle & 2) != 0)
                return Color.FromArgb(128, Color.Red);

            return Color.FromArgb(128, Color.DarkCyan);
        }

        public static Color invertColor(Color color)
        {
            return Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);
        }

        public void DrawSpawn(Point s, bool isRespawn)
        {
            GL.Color4(Color.FromArgb(100, Color.Blue));
            GL.Begin(PrimitiveType.QuadStrip);
            GL.Vertex3(s.x - 3f, s.y, 0f);
            GL.Vertex3(s.x + 3f, s.y, 0f);
            GL.Vertex3(s.x - 3f, s.y + 10f, 0f);
            GL.Vertex3(s.x + 3f, s.y + 10f, 0f);
            GL.End();

            //Draw respawn platform
            if (isRespawn)
            {
                GL.Color4(Color.FromArgb(200, Color.Gray));
                GL.Begin(PrimitiveType.Triangles);
                GL.Vertex3(s.x - 5, s.y, 0);
                GL.Vertex3(s.x + 5, s.y, 0);
                GL.Vertex3(s.x, s.y, 5);

                GL.Vertex3(s.x - 5, s.y, 0);
                GL.Vertex3(s.x + 5, s.y, 0);
                GL.Vertex3(s.x, s.y, -5);

                GL.Vertex3(s.x - 5, s.y, 0);
                GL.Vertex3(s.x, s.y - 5, 0);
                GL.Vertex3(s.x, s.y, 5);

                GL.Vertex3(s.x + 5, s.y, 0);
                GL.Vertex3(s.x, s.y - 5, 0);
                GL.Vertex3(s.x, s.y, -5);

                GL.Vertex3(s.x + 5, s.y, 0);
                GL.Vertex3(s.x, s.y - 5, 0);
                GL.Vertex3(s.x, s.y, 5);

                GL.Vertex3(s.x - 5, s.y, 0);
                GL.Vertex3(s.x, s.y - 5, 0);
                GL.Vertex3(s.x, s.y, -5);
                GL.End();

                GL.Color4(Color.FromArgb(200, Color.Black));
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(s.x - 5, s.y, 0);
                GL.Vertex3(s.x, s.y - 5, 0);
                GL.Vertex3(s.x + 5, s.y, 0);
                GL.Vertex3(s.x, s.y - 5, 0);

                GL.Vertex3(s.x, s.y, -5);
                GL.Vertex3(s.x, s.y - 5, 0);
                GL.Vertex3(s.x, s.y, 5);
                GL.Vertex3(s.x, s.y - 5, 0);

                GL.Vertex3(s.x, s.y, -5);
                GL.Vertex3(s.x + 5, s.y, 0);
                GL.Vertex3(s.x, s.y, -5);
                GL.Vertex3(s.x - 5, s.y, 0);

                GL.Vertex3(s.x, s.y, 5);
                GL.Vertex3(s.x + 5, s.y, 0);
                GL.Vertex3(s.x, s.y, 5);
                GL.Vertex3(s.x - 5, s.y, 0);

                GL.End();
            }
        }

        public void DrawLVD()
        {
            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                // JAM FIIIIIIXXXXXED IIIIIIIT
                if (m.dat_melee != null && m.dat_melee.collisions != null)
                {
                    List<int> ledges = new List<int>();
                    foreach (DAT.COLL_DATA.Link link in m.dat_melee.collisions.links)
                    {
                        GL.Begin(PrimitiveType.Quads);
                        GL.Color4(getLinkColor(link));
                        Vector2D vi = m.dat_melee.collisions.vertices[link.vertexIndices[0]];
                        GL.Vertex3(vi.x, vi.y, 5);
                        GL.Vertex3(vi.x, vi.y, -5);
                        vi = m.dat_melee.collisions.vertices[link.vertexIndices[1]];
                        GL.Vertex3(vi.x, vi.y, -5);
                        GL.Vertex3(vi.x, vi.y, 5);
                        GL.End();
                        if ((link.flags & 2) != 0)
                        {
                            ledges.Add(link.vertexIndices[0]);
                            ledges.Add(link.vertexIndices[1]);
                        }
                    }
                    GL.LineWidth(4);
                    for (int i = 0; i < m.dat_melee.collisions.vertices.Count; i++)
                    {
                        Vector2D vi = m.dat_melee.collisions.vertices[i];
                        if (ledges.Contains(i))
                            GL.Color3(Color.Purple);
                        else
                            GL.Color3(Color.Tomato);
                        GL.Begin(PrimitiveType.Lines);
                        GL.Vertex3(vi.x, vi.y, 5);
                        GL.Vertex3(vi.x, vi.y, -5);
                        GL.End();
                    }

                    /*GL.Color4(Color.FromArgb(128, Color.Purple));
                    foreach(DAT.COLL_DATA.AreaTableEntry entry in m.dat_melee.collisions.areaTable)
                    {
                        GL.Begin(PrimitiveType.QuadStrip);
                        GL.Vertex3(entry.xBotLeftCorner, entry.yBotLeftCorner, 0);
                        GL.Vertex3(entry.xTopRightCorner, entry.yBotLeftCorner, 0);
                        GL.Vertex3(entry.xBotLeftCorner, entry.yTopRightCorner, 0);
                        GL.Vertex3(entry.xTopRightCorner, entry.yTopRightCorner, 0);
                        GL.End();
                        //Console.WriteLine($"{entry.xBotLeftCorner},{entry.yBotLeftCorner},{entry.xTopRightCorner},{entry.yTopRightCorner}");
                    }*/
                }

                if(m.dat_melee != null && m.dat_melee.blastzones != null)
                {
                    Bounds b = m.dat_melee.blastzones;
                    GL.Color3(Color.Red);
                    GL.Begin(PrimitiveType.LineLoop);
                    GL.Vertex3(b.left, b.top, 0);
                    GL.Vertex3(b.right, b.top, 0);
                    GL.Vertex3(b.right, b.bottom, 0);
                    GL.Vertex3(b.left, b.bottom, 0);
                    GL.End();
                }

                if (m.dat_melee != null && m.dat_melee.cameraBounds != null)
                {
                    Bounds b = m.dat_melee.cameraBounds;
                    GL.Color3(Color.Blue);
                    GL.Begin(PrimitiveType.LineLoop);
                    GL.Vertex3(b.left, b.top, 0);
                    GL.Vertex3(b.right, b.top, 0);
                    GL.Vertex3(b.right, b.bottom, 0);
                    GL.Vertex3(b.left, b.bottom, 0);
                    GL.End();
                }

                if (m.dat_melee != null && m.dat_melee.respawns != null)
                    foreach (Point r in m.dat_melee.respawns)
                        DrawSpawn(r, true);

                if (m.dat_melee != null && m.dat_melee.spawns != null)
                    foreach (Point r in m.dat_melee.spawns)
                        DrawSpawn(r, false);

                GL.Color4(Color.FromArgb(200, Color.Fuchsia));
                if (m.dat_melee != null && m.dat_melee.itemSpawns != null)
                    foreach (Point r in m.dat_melee.itemSpawns)
                        RenderTools.drawCubeWireframe(new Vector3(r.x, r.y, 0), 3);
            }

            if (Runtime.TargetLVD != null)
            {
                if (Runtime.renderCollisions)
                {
                    Color color;
                    GL.LineWidth(4);
                    Matrix4 transform = Matrix4.Identity;
                    foreach (Collision c in Runtime.TargetLVD.collisions)
                    {
                        bool colSelected = (Runtime.LVDSelection == c);
                        float addX = 0, addY = 0, addZ = 0;
                        if (c.useStartPos)
                        {
                            addX = c.startPos[0];
                            addY = c.startPos[1];
                            addZ = c.startPos[2];
                        }
                        if (c.flag2)
                        {
                            //Flag2 == rigged collision
                            ModelContainer riggedModel = null;
                            Bone riggedBone = null;
                            foreach (ModelContainer m in Runtime.ModelContainers)
                            {
                                if (m.name.Equals(c.subname))
                                {
                                    riggedModel = m;
                                    if (m.vbn != null)
                                    {
                                        foreach(Bone b in m.vbn.bones)
                                        {
                                            if (b.Text.Equals(new string(c.unk4)))
                                            {
                                                riggedBone = b;
                                            }
                                        }
                                    }
                                }
                            }
                            if(riggedModel != null)
                            {
                                if(riggedBone == null && riggedModel.vbn != null && riggedModel.vbn.bones.Count > 0)
                                {
                                    riggedBone = riggedModel.vbn.bones[0];
                                }
                                if (riggedBone != null)
                                    transform = riggedBone.invert * riggedBone.transform;
                            }
                        }

                        for(int i = 0; i < c.verts.Count - 1; i++)
                        {
                            Vector3 v1Pos = Vector3.Transform(new Vector3(c.verts[i].x + addX, c.verts[i].y + addY, addZ + 5), transform);
                            Vector3 v1Neg = Vector3.Transform(new Vector3(c.verts[i].x + addX, c.verts[i].y + addY, addZ - 5), transform);
                            Vector3 v1Zero = Vector3.Transform(new Vector3(c.verts[i].x + addX, c.verts[i].y + addY, addZ), transform);
                            Vector3 v2Pos = Vector3.Transform(new Vector3(c.verts[i + 1].x + addX, c.verts[i + 1].y + addY, addZ + 5), transform);
                            Vector3 v2Neg = Vector3.Transform(new Vector3(c.verts[i + 1].x + addX, c.verts[i + 1].y + addY, addZ - 5), transform);
                            Vector3 v2Zero = Vector3.Transform(new Vector3(c.verts[i + 1].x + addX, c.verts[i + 1].y + addY, addZ), transform);

                            GL.Begin(PrimitiveType.Quads);
                            if(c.normals.Count > i)
                            {
                                if (Runtime.renderCollisionNormals)
                                {
                                    Vector3 v = Vector3.Add(Vector3.Divide(Vector3.Subtract(v1Zero, v2Zero), 2), v2Zero);
                                    GL.End();
                                    GL.Begin(PrimitiveType.Lines);
                                    GL.Color3(Color.Blue);
                                    GL.Vertex3(v);
                                    GL.Vertex3(v.X + (c.normals[i].x * 5), v.Y + (c.normals[i].y * 5), v.Z);
                                    GL.End();
                                    GL.Begin(PrimitiveType.Quads);
                                }
                                
				                if(c.flag4)
				                    color = Color.FromArgb(128, Color.Yellow);
                                else if (Math.Abs(c.normals[i].x) > Math.Abs(c.normals[i].y))
                                    color = Color.FromArgb(128, Color.Lime);
                                else if(c.normals[i].y < 0)
                                    color = Color.FromArgb(128, Color.Red);
                                else
				                    color = Color.FromArgb(128, Color.Cyan);

                                if ((colSelected || Runtime.LVDSelection == c.normals[i]) && ((int)((timeSinceSelected.ElapsedMilliseconds % 1000) / 500) == 0))
                                    color = invertColor(color);

                                GL.Color4(color);
                            }
                            else
                            {
                                GL.Color4(Color.FromArgb(128, Color.Gray));
                            }
                            GL.Vertex3(v1Pos);
                            GL.Vertex3(v1Neg);
                            GL.Vertex3(v2Neg);
                            GL.Vertex3(v2Pos);
                            GL.End();

                            GL.Begin(PrimitiveType.Lines);
                            if (c.materials.Count > i)
                            {
                                if (c.materials[i].getFlag(6) || (i > 0 && c.materials[i - 1].getFlag(7)))
                                    color = Color.Purple;
                                else
                                    color = Color.Orange;

                                if ((colSelected || Runtime.LVDSelection == c.verts[i]) && ((int)((timeSinceSelected.ElapsedMilliseconds % 1000) / 500) == 0))
                                    color = invertColor(color);
                                GL.Color4(color);
                            }
                            else
                            {
                                GL.Color4(Color.Gray);
                            }
                            GL.Vertex3(v1Pos);
                            GL.Vertex3(v1Neg);

                            if (i == c.verts.Count - 2)
                            {
                                if (c.materials.Count > i)
                                {
                                    if (c.materials[i].getFlag(7))
                                        color = Color.Purple;
                                    else
                                        color = Color.Orange;

                                    if (Runtime.LVDSelection == c.verts[i+1] && ((int)((timeSinceSelected.ElapsedMilliseconds % 1000) / 500) == 0))
                                        color = invertColor(color);
                                    GL.Color4(color);
                                }
                                else
                                {
                                    GL.Color4(Color.Gray);
                                }
                                GL.Vertex3(v2Pos);
                                GL.Vertex3(v2Neg);
                            }
                            GL.End();
                        }
                    }
                }

                if (Runtime.renderItemSpawners)
                {
                    foreach (ItemSpawner c in Runtime.TargetLVD.items)
                    {
                        foreach (Section s in c.sections)
                        {
                            // draw the item spawn quads
                            GL.LineWidth(2);

                            // draw outside borders
                            GL.Color3(Color.Black);
                            GL.Begin(PrimitiveType.LineStrip);
                            foreach (Vector2D vi in s.points)
                            {
                                GL.Vertex3(vi.x, vi.y, 5);
                            }
                            GL.End();
                            GL.Begin(PrimitiveType.LineStrip);
                            foreach (Vector2D vi in s.points)
                            {
                                GL.Vertex3(vi.x, vi.y, -5);
                            }
                            GL.End();


                            // draw vertices
                            GL.Color3(Color.White);
                            GL.Begin(PrimitiveType.Lines);
                            foreach (Vector2D vi in s.points)
                            {
                                GL.Vertex3(vi.x, vi.y, 5);
                                GL.Vertex3(vi.x, vi.y, -5);
                            }
                            GL.End();
                        }
                    }
                }

                if (Runtime.renderSpawns)
                {
                    foreach (Point s in Runtime.TargetLVD.spawns)
                    {
                        DrawSpawn(s, false);
                    }
                }

                if (Runtime.renderRespawns)
                {
                    foreach (Point s in Runtime.TargetLVD.respawns)
                    {
                        DrawSpawn(s,true);
                    }
                }

                if (Runtime.renderGeneralPoints)
                {
                    foreach (Point g in Runtime.TargetLVD.generalPoints)
                    {
                        GL.Color4(Color.FromArgb(200, Color.Fuchsia));
                        RenderTools.drawCubeWireframe(new Vector3(g.x, g.y, 0), 3);
                    }
                    
                    foreach (LVDGeneralShape shape in Runtime.TargetLVD.generalShapes)
                    {
                        if(shape is GeneralPoint)
                        {
                            GeneralPoint g = (GeneralPoint)shape;
                            GL.Color4(Color.FromArgb(200, Color.Fuchsia));
                            RenderTools.drawCubeWireframe(new Vector3(g.x, g.y, 0), 3);
                        }
                        if(shape is GeneralRect)
                        {
                            GeneralRect b = (GeneralRect)shape;
                            GL.Color4(Color.FromArgb(200, Color.Fuchsia));
                            GL.Begin(PrimitiveType.LineLoop);
                            GL.Vertex3(b.x1, b.y1, 0);
                            GL.Vertex3(b.x2, b.y1, 0);
                            GL.Vertex3(b.x2, b.y2, 0);
                            GL.Vertex3(b.x1, b.y2, 0);
                            GL.End();
                        }
                        if(shape is GeneralPath)
                        {
                            List<Vector2D> p = ((GeneralPath)shape).points;
                            GL.Color4(Color.FromArgb(200, Color.Fuchsia));
                            GL.Begin(PrimitiveType.LineStrip);
                            foreach(Vector2D point in p)
                                GL.Vertex3(point.x, point.y, 0);
                            GL.End();
                        }
                    }
                }

                if (Runtime.renderOtherLVDEntries)
                {
                    GL.Color4(Color.FromArgb(128, Color.Yellow));
                    foreach (Sphere s in Runtime.TargetLVD.damageSpheres)
                    {
                        RenderTools.drawSphere(new Vector3(s.x, s.y, s.z), s.radius, 24);
                    }

                    foreach (Capsule c in Runtime.TargetLVD.damageCapsules)
                    {
                        RenderTools.drawCylinder(new Vector3(c.x, c.y, c.z), new Vector3(c.x + c.dx, c.y + c.dy, c.z + c.dz), c.r);
                    }

                    GL.Color4(Color.FromArgb(128, Color.Red));
                    foreach (Bounds b in Runtime.TargetLVD.blastzones)
                    {
                        GL.Begin(PrimitiveType.LineLoop);
                        GL.Vertex3(b.left, b.top, 0);
                        GL.Vertex3(b.right, b.top, 0);
                        GL.Vertex3(b.right, b.bottom, 0);
                        GL.Vertex3(b.left, b.bottom, 0);
                        GL.End();
                    }

                    GL.Color4(Color.FromArgb(128, Color.Blue));
                    foreach (Bounds b in Runtime.TargetLVD.cameraBounds)
                    {
                        GL.Begin(PrimitiveType.LineLoop);
                        GL.Vertex3(b.left, b.top, 0);
                        GL.Vertex3(b.right, b.top, 0);
                        GL.Vertex3(b.right, b.bottom, 0);
                        GL.Vertex3(b.left, b.bottom, 0);
                        GL.End();
                    }
                }
            }
        }

        private void DrawPathDisplay()
        {
            if (Runtime.TargetPath != null && !checkBox1.Checked)
            {
                GL.Color3(Color.Yellow);
                GL.LineWidth(2);
                for (int i = 1; i < Runtime.TargetPath.Frames.Count; i++)
                {
                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex3(Runtime.TargetPath.Frames[i].x, Runtime.TargetPath.Frames[i].y, Runtime.TargetPath.Frames[i].z);
                    GL.Vertex3(Runtime.TargetPath.Frames[i - 1].x, Runtime.TargetPath.Frames[i - 1].y, Runtime.TargetPath.Frames[i - 1].z);
                    GL.End();
                }
            }

            if (Runtime.TargetCMR0 != null && !checkBox1.Checked)
            {
                GL.Color3(Color.Yellow);
                GL.LineWidth(2);
                for (int i = 1; i < Runtime.TargetCMR0.frames.Count; i++)
                {
                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex3(Runtime.TargetCMR0.frames[i].M14, Runtime.TargetCMR0.frames[i].M24, Runtime.TargetCMR0.frames[i].M34);
                    GL.Vertex3(Runtime.TargetCMR0.frames[i - 1].M14, Runtime.TargetCMR0.frames[i - 1].M24, Runtime.TargetCMR0.frames[i - 1].M34);
                    GL.End();
                }
            }
        }

        public void RenderHitboxes()
        {
            if (Hitboxes.Count > 0)
            {
                GL.Color4(Color.FromArgb(85, Color.Red));
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.Blend);

                foreach (var pair in Hitboxes)
                {
                    var h = pair.Value;
                    var va = new Vector3(h.X, h.Y, h.Z);

                    int bid = h.Bone;
                    int gr = 0;
                    if (bid > 1000)
                    {
                        /*while (bid >= 1000)
                        {
                            bid -= 1000;
                            gr++;
                        }
                        gr = gr % 2;
                        //bid = bid >> 6;
                        Console.WriteLine(h.Bone + " " + gr + " " + bid);*/
                        bid >>= 8;
                    }

                    Bone b = new Bone(null);

                    if (h.Bone != -1)
                    {
                        foreach (ModelContainer m in Runtime.ModelContainers)
                        {
                            //ModelContainers should store Hitbox data or have them linked since it will use last modelcontainer bone for hitbox display (which might not be the character model)
                            if (m.vbn != null)
                            {
                                try //Try used to avoid bone not found issue that crashes the application
                                {
                                    if (m.vbn.jointTable.Count < 1)
                                        b = m.vbn.bones[bid];
                                    else
                                    {
                                        b = m.vbn.bones[m.vbn.jointTable[gr][bid]];
                                    }
                                }
                                catch
                                {
                                    //Console.WriteLine($"Hitbox: Bone not found {h.Bone}");
                                }
                            }
                        }
                    }

                    va = Vector3.Transform(va, b.transform.ClearScale());


                    // Draw angle marker
                    /*GL.LineWidth(7f);
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color3(Color.Black);
                    GL.Vertex3(va);
                    GL.Vertex3(va + Vector3.Transform(new Vector3(0,0,h.Size), Matrix4.CreateRotationX(-h.Angle * ((float)Math.PI / 180f))));
                    GL.End();*/

                    GL.Color4(Color.FromArgb(85, Color.Red));

                    switch (h.Type)
                    {
                        case Hitbox.HITBOX:
                            GL.Color4(Color.FromArgb(85, Color.Red));
                            break;
                        case Hitbox.GRABBOX:
                            GL.Color4(Color.FromArgb(85, Color.Purple));
                            break;
                        case Hitbox.WINDBOX:
                            GL.Color4(Color.FromArgb(85, Color.Blue));
                            break;
                    }

                    GL.DepthMask(false);
                    if (h.Extended)
                    {
                        var va2 = new Vector3(h.X2, h.Y2, h.Z2);

                        if (h.Bone != -1)
                            va2 = Vector3.Transform(va2, b.transform.ClearScale());

                        RenderTools.drawCylinder(va, va2, h.Size);
                    }
                    else
                    {
                        RenderTools.drawSphere(va, h.Size, 30);
                    }
                }

                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.DepthTest);
            }
        }

        public void RenderHurtboxes()
        {
            if (Runtime.ParamManager.Hurtboxes.Count > 0)
            {
                GL.Color4(Color.FromArgb(50, Color.Green));
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.Blend);

                foreach (var pair in Runtime.ParamManager.Hurtboxes)
                {
                    var h = pair.Value;
                    var va = new Vector3(h.X, h.Y, h.Z);

                    int bid = h.Bone;
                    int gr = 0;
                    if (bid > 1000)
                    {
                        /*while (bid >= 1000)
                        {
                            bid -= 1000;
                            gr++;
                        }
                        gr = gr % 2;
                        //bid = bid >> 6;
                        Console.WriteLine(h.Bone + " " + gr + " " + bid);*/
                        bid >>= 8;
                    }

                    Bone b = new Bone(null);

                    if (h.Bone != -1)
                    {
                        foreach (ModelContainer m in Runtime.ModelContainers)
                        {
                            if (m.vbn != null)
                            {
                                try //Try used to avoid bone not found issue that crashes the application
                                {
                                    if (m.vbn.jointTable.Count < 1)
                                        b = m.vbn.bones[bid];
                                    else
                                    {
                                        b = m.vbn.bones[m.vbn.jointTable[gr][bid]];
                                    }
                                }
                                catch
                                {
                                    //Console.WriteLine($"Hurtbox: Bone not found {h.Bone}");
                                }
                            }

                        }
                    }

                    va = Vector3.Transform(va, b.transform.ClearScale());

                    GL.Color4(Color.FromArgb(50, Color.Green));

                    if (Runtime.renderHurtboxesZone)
                    {
                        switch (h.Zone)
                        {
                            case Hurtbox.LW_ZONE:
                                GL.Color4(Color.FromArgb(50, Color.Aqua));
                                break;
                            case Hurtbox.N_ZONE:
                                GL.Color4(Color.FromArgb(50, Color.Green));
                                break;
                            case Hurtbox.HI_ZONE:
                                GL.Color4(Color.FromArgb(50, Color.Orange));
                                break;
                        }
                    }

                    GL.DepthMask(false);
                    var va2 = new Vector3(h.X2, h.Y2, h.Z2);

                    if (h.Bone != -1)
                        va2 = Vector3.Transform(va2, b.transform.ClearScale());

                    RenderTools.drawReducedSidesCylinder(va, va2, h.Size);
                }

                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.DepthTest);
            }
        }

        ACMDScript scr_game;
        ACMDScript scr_sound;

        public void ProcessFrame()
        {
            Hitboxes.Clear();
            int halt = 0;
            int e = 0;
            int setLoop = 0;
            int iterations = 0;
            var cmd = scr_game[e];
            //ProcessANMCMD_SOUND();

            while (halt < Frame)
            {
                switch (cmd.Ident)
                {
                    case 0x42ACFE7D: // Asynchronous Timer
                        {
                            halt = (int)(float)cmd.Parameters[0] - 2;
                            break;
                        }
                    case 0x4B7B6E51: // Synchronous Timer
                        {
                            halt += (int)(float)cmd.Parameters[0];
                            break;
                        }
                    case 0xB738EABD: // hitbox 
                        {
                            Hitbox h = new Hitbox();
                            int id = (int)cmd.Parameters[0];
                            if (Hitboxes.ContainsKey(id))
                                Hitboxes.Remove(id);
                            h.Type = Hitbox.HITBOX;
                            h.Bone = ((int)cmd.Parameters[2] - 1).Clamp(0, int.MaxValue);
                            h.Damage = (float)cmd.Parameters[3];
                            h.Angle = (int)cmd.Parameters[4];
                            h.KnockbackGrowth = (int)cmd.Parameters[5];
                            //FKB = (float)cmd.Parameters[6]
                            h.KnockbackBase = (int)cmd.Parameters[7];
                            h.Size = (float)cmd.Parameters[8];
                            h.X = (float)cmd.Parameters[9];
                            h.Y = (float)cmd.Parameters[10];
                            h.Z = (float)cmd.Parameters[11];
                            Hitboxes.Add(id, h);
                            break;
                        }
                    case 0x2988D50F: // Extended hitbox
                        {
                            Hitbox h = new Hitbox();
                            int id = (int)cmd.Parameters[0];
                            if (Hitboxes.ContainsKey(id))
                                Hitboxes.Remove(id);
                            h.Type = Hitbox.HITBOX;
                            h.Extended = true;
                            h.Bone = ((int)cmd.Parameters[2] - 1).Clamp(0, int.MaxValue);
                            h.Damage = (float)cmd.Parameters[3];
                            h.Angle = (int)cmd.Parameters[4];
                            h.KnockbackGrowth = (int)cmd.Parameters[5];
                            //FKB = (float)cmd.Parameters[6]
                            h.KnockbackBase = (int)cmd.Parameters[7];
                            h.Size = (float)cmd.Parameters[8];
                            h.X = (float)cmd.Parameters[9];
                            h.Y = (float)cmd.Parameters[10];
                            h.Z = (float)cmd.Parameters[11];
                            h.X2 = (float)cmd.Parameters[24];
                            h.Y2 = (float)cmd.Parameters[25];
                            h.Z2 = (float)cmd.Parameters[26];
                            Hitboxes.Add(id, h);
                            break;
                        }
                    case 0x14FCC7E4: // special hitbox
                        {
                            Hitbox h = new Hitbox();
                            int id = (int)cmd.Parameters[0];
                            if (Hitboxes.ContainsKey(id))
                                Hitboxes.Remove(id);
                            h.Type = Hitbox.HITBOX;
                            h.Bone = ((int)cmd.Parameters[2] - 1).Clamp(0, int.MaxValue);
                            h.Damage = (float)cmd.Parameters[3];
                            h.Angle = (int)cmd.Parameters[4];
                            h.KnockbackGrowth = (int)cmd.Parameters[5];
                            //FKB = (float)cmd.Parameters[6]
                            h.KnockbackBase = (int)cmd.Parameters[7];
                            h.Size = (float)cmd.Parameters[8];
                            h.X = (float)cmd.Parameters[9];
                            h.Y = (float)cmd.Parameters[10];
                            h.Z = (float)cmd.Parameters[11];
                            if (cmd.Parameters.Count > 39)
                            {
                                if ((int)cmd.Parameters[39] == 1)
                                {
                                    h.Type = Hitbox.WINDBOX;
                                }
                            }
                            Hitboxes.Add(id, h);
                            break;
                        }
                    case 0x7075DC5A: // Extended special hitbox
                        {
                            Hitbox h = new Hitbox();
                            int id = (int)cmd.Parameters[0];
                            if (Hitboxes.ContainsKey(id))
                                Hitboxes.Remove(id);
                            h.Type = Hitbox.HITBOX;
                            h.Extended = true;
                            h.Bone = ((int)cmd.Parameters[2] - 1).Clamp(0, int.MaxValue);
                            h.Damage = (float)cmd.Parameters[3];
                            h.Angle = (int)cmd.Parameters[4];
                            h.KnockbackGrowth = (int)cmd.Parameters[5];
                            //FKB = (float)cmd.Parameters[6]
                            h.KnockbackBase = (int)cmd.Parameters[7];
                            h.Size = (float)cmd.Parameters[8];
                            h.X = (float)cmd.Parameters[9];
                            h.Y = (float)cmd.Parameters[10];
                            h.Z = (float)cmd.Parameters[11];
                            if ((int)cmd.Parameters[39] == 1)
                            {
                                h.Type = Hitbox.WINDBOX;
                            }
                            h.X2 = (float)cmd.Parameters[40];
                            h.Y2 = (float)cmd.Parameters[41];
                            h.Z2 = (float)cmd.Parameters[42];
                            Hitboxes.Add(id, h);
                            break;
                        }
                    case 0x9245E1A8: // clear all hitboxes
                        Hitboxes.Clear();
                        break;
                    case 0xFF379EB6: // delete hitbox
                        if (Hitboxes.ContainsKey((int)cmd.Parameters[0]))
                        {
                            Hitboxes.Remove((int)cmd.Parameters[0]);
                        }
                        break;
                    case 0x7698BB42: // deactivate previous hitbox
                        Hitboxes.Remove(Hitboxes.Keys.Max());
                        break;
                    case 0xEB375E3: // Set Loop
                        iterations = int.Parse(cmd.Parameters[0] + "") - 1;
                        setLoop = e;
                        break;
                    case 0x38A3EC78: // goto
                        if (iterations > 0)
                        {
                            e = setLoop;
                            iterations -= 1;
                        }
                        break;

                    case 0x7B48FE1C: // grabbox 
                        {
                            Hitbox h = new Hitbox();
                            int id = (int)cmd.Parameters[0];
                            h.Type = Hitbox.GRABBOX;
                            h.Bone = (int.Parse(cmd.Parameters[1] + "") - 1).Clamp(0, int.MaxValue);
                            h.Size = (float)cmd.Parameters[2];
                            h.X = (float)cmd.Parameters[3];
                            h.Y = (float)cmd.Parameters[4];
                            h.Z = (float)cmd.Parameters[5];

                            if (cmd.Parameters.Count > 8)
                            {
                                h.X2 = float.Parse(cmd.Parameters[8] + "");
                                h.Y2 = float.Parse(cmd.Parameters[9] + "");
                                h.Z2 = float.Parse(cmd.Parameters[10] + "");
                                h.Extended = true;
                            }

                            Hitboxes.Add(id, h);
                            break;
                        }
                    case 0x548F2D4C: //"Special grabbox" (used in tether grabs) Requires changing SALT's PARAM_FORMAT event values to float to work
                        {
                            //PARAM_SYNTAX "ID,Bone,Size,X,Y,Z,Action,Air/Ground"

                            Hitbox h = new Hitbox();
                            int id = (int)cmd.Parameters[0];
                            h.Type = Hitbox.GRABBOX;
                            h.Bone = (int.Parse(cmd.Parameters[1] + "")-1).Clamp(0, int.MaxValue);
                            h.Size = (float)cmd.Parameters[2];
                            h.X = (float)cmd.Parameters[3];
                            h.Y = (float)cmd.Parameters[4];
                            h.Z = (float)cmd.Parameters[5];

                            Hitboxes.Add(id, h);
                            break;
                        }
                    case 0xF3A464AC: // Terminate_Grab_Collisions
                        List<Hitbox> toDelete = new List<Hitbox>();
                        for (int i = 0; i < Hitboxes.Count; i++)
                        {
                            if (Hitboxes.Values[i].Type == Hitbox.GRABBOX)
                                toDelete.Add(Hitboxes.Values[i]);
                        }
                        foreach (Hitbox h in toDelete)
                            Hitboxes.Remove(Hitboxes.IndexOfValue(h));
                        break;
                    case 0xFAA85333:
                        break;
                    case 0x321297B0:
                        break;
                    case 0xED67D5DA:
                        break;
                    case 0x7640AEEB:
                        break;

                }

                e++;
                if (e >= scr_game.Count)
                    break;
                else
                    cmd = scr_game[e];

                if (halt > Frame)
                    break;
            }
        }

        int lastFrame = 0;
        bool enterFrame = false;
        public void ProcessANMCMD_SOUND()
        {
            enterFrame = Frame != lastFrame;

            if (enterFrame)
            {
                int halt = 0;
                int e = 0;
                var cmd = scr_sound[e];

                List<int> soundtoplay = new List<int>();

                while (halt < Frame)
                {
                    switch (cmd.Ident)
                    {
                        case 0x42ACFE7D: // Asynchronous Timer
                            {
                                soundtoplay.Clear();
                                halt = (int)(float)cmd.Parameters[0] - 2;
                                break;
                            }
                        case 0x4B7B6E51: // Synchronous Timer
                            {
                                soundtoplay.Clear();
                                halt += (int)(float)cmd.Parameters[0];
                                break;
                            }
                        case 0xE5751F0A: // play sound 
                            {
                                if (Runtime.SoundContainers.Count > 0)
                                {
                                    soundtoplay.Add((int)cmd.Parameters[0] & 0xFF);
                                }
                                break;
                            }

                    }

                    e++;
                    if (e >= scr_sound.Count)
                        break;
                    else
                        cmd = scr_sound[e];

                    if (halt > Frame)
                        break;
                }

                foreach (int s in soundtoplay)
                {
                    //Runtime.SoundContainers[0].tone.tones[s].Play();
                }
            }

            lastFrame = Frame;

            
        }

        public void HandleACMD(string animname)
        {
            //Console.WriteLine("Handling " + animname);
            var crc = Crc32.Compute(animname.Replace(".omo", "").ToLower());

            if (Runtime.Moveset == null)
            {
                scr_game = null;
                return;
            }

            if (!Runtime.Moveset.Game.Scripts.ContainsKey(crc))
            {
                scr_game = null;
                return;
            }

            //Console.WriteLine("Handling " + animname);
            scr_game = (ACMDScript)Runtime.Moveset.Game.Scripts[crc];
            //scr_sound = (ACMDScript)Runtime.Moveset.Sound.Scripts[crc];
            if (Runtime.acmdEditor.crc != crc)
                Runtime.acmdEditor.SetAnimation(crc);
        }

        #endregion

        public void SetFrame(int frame)
        {
            Runtime.TargetAnim.setFrame(frame);
            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if (m.vbn != null)
                    Runtime.TargetAnim.nextFrame(m.vbn);

                if (m.dat_melee != null)
                {
                    Runtime.TargetAnim.setFrame((int)this.nupdFrame.Value);
                    Runtime.TargetAnim.nextFrame(m.dat_melee.bones);
                }
            }
        }
        public void loadAnimation(SkelAnimation a)
        {
            a.setFrame(0);
            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if (m.vbn != null)
                    Runtime.TargetAnim.nextFrame(m.vbn);
            }
            nupdMaxFrame.Value = a.size() > 1 ? a.size() - 1 : a.size();
            nupdFrame.Value = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            rot = 0;
            lookup = 0;
            height = 0;
            width = 0;
            zoom = 0;
            nzoom = 0;
            mouseXLast = 0;
            mouseYLast = 0;
            mouseSLast = 0;
            UpdateMousePosition();
        }

        public void loadMTA(MTA m)
        {
            Console.WriteLine("MTA Loaded");
            Frame = 0;
            nupdFrame.Value = 0;
            nupdMaxFrame.Value = m.numFrames;
            //Console.WriteLine(m.numFrames);
            Runtime.TargetMTA = m;
        }

        private void VBNViewport_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == 'i')
            {
                //GL.DeleteProgram(shader.programID);
                //shader = new Shader();
                //shader.vertexShader(File.ReadAllText("vert.txt"));
                //shader.fragmentShader(File.ReadAllText("frag.txt"));
            }
            if (e.KeyChar == 'r')
            {
                /*int fb;
                GL.GenFramebuffers(1, out fb);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fb);

                int rt;
                GL.GenTextures(1, out rt);
                GL.BindTexture(TextureTarget.Texture2D, rt);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 1024, 768, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr)0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                int dep;
                GL.GenRenderbuffers(1, out dep);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, dep);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, 1024, 768);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, dep);

                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, rt, 0);

                DrawBuffersEnum[] drawBuffers = new DrawBuffersEnum[] { DrawBuffersEnum .ColorAttachment0};
                GL.DrawBuffers(1, drawBuffers);

                if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                    MessageBox.Show("Error Rendering");*/

                //GL.BindFramebuffer(FramebufferTarget.Framebuffer, fb);
                //GL.Viewport(0, 0, 1024, 768);


                // GL.MatrixMode(MatrixMode.Projection);
                //GL.LoadIdentity();
                /*GL.Begin(PrimitiveType.Quads);
                GL.Color3(back1);
                GL.Vertex2(1.0, 1.0);
                GL.Vertex2(-1.0, 1.0);
                GL.Color3(back2);
                GL.Vertex2(-1.0, -1.0);
                GL.Vertex2(1.0, -1.0);
                GL.End();*/

                /*GL.Enable(EnableCap.Normalize);  
                GL.Enable(EnableCap.RescaleNormal);

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Less);

                GL.Enable(EnableCap.AlphaTest);
                GL.AlphaFunc(AlphaFunction.Gequal, 0.1f);

                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Front);

                GL.Enable(EnableCap.LineSmooth);*/

                //GL.LoadMatrix(ref v);
                // set up the viewport projection and send it to GPU
                //GL.PushAttrib(AttribMask.AllAttribBits);
                //GL.Enable(EnableCap.DepthTest);
                //GL.DepthFunc(DepthFunction.Less);
                //GL.MatrixMode(MatrixMode.Modelview);

                //GL.UseProgram(0);
                // drawing floor---------------------------
                //if (Runtime.renderFloor)RenderTools.drawFloor(Matrix4.CreateTranslation(Vector3.Zero));

                // draw models
                //if (Runtime.renderModel)
                //    DrawModels();

                int width = glControl1.Width;
                int height = glControl1.Height;

                byte[] pixels = new byte[width*height*4];
                GL.ReadPixels(0,0,width, height, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
                File.WriteAllBytes("test.bin", pixels);
                
                Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

                byte[] np = new byte[width*height * 4];
                // need to flip and rearrange the data
                for(int h = 0; h < height; h++)
                    for(int w = 0;  w < width; w++)
                    {
                        np[(w + (bmp.Height - h - 1) * bmp.Width) * 4 + 2] = pixels[(w + h * bmp.Width) * 4 + 0];
                        np[(w + (bmp.Height - h - 1) * bmp.Width) * 4 + 1] = pixels[(w + h * bmp.Width) * 4 + 1];
                        np[(w + (bmp.Height - h - 1) * bmp.Width) * 4 + 0] = pixels[(w + h * bmp.Width) * 4 + 2];
                        np[(w + (bmp.Height - h - 1) * bmp.Width) * 4 + 3] = pixels[(w + h * bmp.Width) * 4 + 3];
                    }

                Marshal.Copy(np, 0, bmpData.Scan0, np.Length);
                bmp.UnlockBits(bmpData);

                Console.WriteLine("Saving");
                bmp.Save("Render.png");
                //GL.DeleteFramebuffer(fb);
                //GL.DeleteTexture(rt);
                //GL.DeleteTexture(dep);

                //GL.PopAttrib();

                //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
            if (e.KeyChar == 'f')
            {
                fpsView = !fpsView;

                // I need to convert the camera stuff...
                if (fpsView)
                {
                    //Console.WriteLine(trans);
                    /*Quaternion rot = v.ExtractRotation();
                    Vector3 tr = Vector3.Transform(Vector3.Zero, v.Inverted());
                    width = tr.X / 5f;
                    height = (tr.Y+5f) / -5f;
                    zoom = -(tr.Z + 15f);*/
            }
            else
                {
                }
            }
        }

        System.Drawing.Point mouseDownPos = new System.Drawing.Point();
        Vector3 p1 = Vector3.Zero, p2 = Vector3.Zero;
        public int dbdistance = 0;
        bool freezeCamera = false;
        private void glControl1_DoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            float mouse_x = this.PointToClient(Cursor.Position).X;
            float mouse_y = this.PointToClient(Cursor.Position).Y;

            float x = (2.0f * mouse_x) / glControl1.Width - 1.0f;
            float y = 1.0f - (2.0f * mouse_y) / glControl1.Height;
            Vector4 va = Vector4.Transform(new Vector4(x, y, -1.0f, 1.0f), v.Inverted());
            Vector4 vb = Vector4.Transform(new Vector4(x, y, 1.0f, 1.0f), v.Inverted());

            p1 = va.Xyz;
            p2 = p1 - (va - (va + vb)).Xyz * 100;

            SortedList<double, Bone> selected = new SortedList<double, Bone>(new DuplicateKeyComparer<double>());

            foreach (ModelContainer con in Runtime.ModelContainers)
            {
                if (con.vbn != null)
                {
                    foreach (Bone b in con.vbn.bones)
                    {
                        Vector3 closest = Vector3.Zero;
                        Vector3 cen = b.transform.ExtractTranslation();
                        if (!selected.Values.Contains(b) && RenderTools.CheckSphereHit(cen, .3f, p1, p2, out closest))
                        {
                            double dis = Math.Pow(closest.X - p1.X, 2) + Math.Pow(closest.Y - p1.Y, 2) + Math.Pow(closest.Z - p1.Z, 2);
                            selected.Add(dis, b);
                        }
                    }
                }
            }

            /*Console.WriteLine(dbdistance + " " + selected.Count);
            foreach(var v in selected)
            {
                Console.WriteLine(new String(v.Value.boneName));
            }*/

            if (selected.Count > dbdistance)
            {
                if(Runtime.TargetVBN.bones.Contains(selected.Values.ElementAt(dbdistance)));
                    MainForm.Instance.boneTreePanel.treeView1.SelectedNode = selected.Values.ElementAt(dbdistance);
            }

            dbdistance += 1;
            if (dbdistance >= selected.Count) dbdistance = 0;
            _LastPoint = e.Location;
        }

        private void glControl1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mouseDownPos = this.PointToClient(Cursor.Position);
        }

        public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
        {
            #region IComparer<TKey> Members

            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1;  
                else
                    return result;
            }

            #endregion
        }

        private void glControl1_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(mouseDownPos == this.PointToClient(Cursor.Position) && 1==2)
            {
                float mouse_x = this.PointToClient(Cursor.Position).X;
                float mouse_y = this.PointToClient(Cursor.Position).Y;

                float x = (2.0f * mouse_x) / glControl1.Width - 1.0f;
                float y = 1.0f - (2.0f * mouse_y) / glControl1.Height;
                Vector4 va = Vector4.Transform(new Vector4(x, y, -1.0f, 1.0f), v.Inverted());
                Vector4 vb = Vector4.Transform(new Vector4(x, y, 1.0f, 1.0f), v.Inverted());

                p1 = va.Xyz;
                p2 = p1 - (va - (va + vb)).Xyz * 100;

                freezeCamera = (RenderTools.intersectCircle(new Vector3(6, 6, 6), 5, 90, p1, p2));
            }
        }

        public void FPSCamera()
        {
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.A))
                x += 0.2f;
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.D))
                x -= 0.2f;
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.W))
                zoom += 0.2f;
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.S))
                zoom -= 0.2f;

            rot += 0.025f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
            lookup += 0.025f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);

            mouseXLast = OpenTK.Input.Mouse.GetState().X;
            mouseYLast = OpenTK.Input.Mouse.GetState().Y;

            v = Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) * Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup) 
                * Matrix4.CreatePerspectiveFieldOfView(Runtime.fov, glControl1.Width / (float)glControl1.Height, 1.0f, Runtime.renderDepth);
            //v2 = Matrix4.CreateTranslation(5 * width, -5f - 5f * height, -15f + zoom) * Matrix4.CreateRotationY(0.5f * rot) * Matrix4.CreateRotationX(0.2f * lookup);
        }
    }
}
