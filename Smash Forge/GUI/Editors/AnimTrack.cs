using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Smash_Forge
{
    public partial class AnimTrack : DockContent
    {
        public DAT_Animation anim;
        Frame[] frames;

        public AnimTrack()
        {
            InitializeComponent();
        }

        public AnimTrack(DAT_Animation anim)
        {
            InitializeComponent();
            this.anim = anim;
            PopulateBoneList();
        }

        private class Frame
        {
            float frame;
            public float x=-99, y = -99, z = -99, rx = -99, ry = -99, rz = -99, sx = -99, sy = -99, sz = -99;

            public Frame(int frame)
            {
                this.frame = frame+1;
            }

            public override string ToString()
            {
                string s = "[";
                s += Format(frame + "", " ");
                s += "](";
                s += Format(sx == -99 ? "" : sx + "", "_");
                s += ",";
                s += Format(sy == -99 ? "" : sy + "", "_");
                s += ",";
                s += Format(sz == -99 ? "" : sz + "", "_");
                s += ")(";
                s += Format(rx == -99 ? "" : rx + "", "_");
                s += ",";
                s += Format(ry == -99 ? "" : ry + "", "_");
                s += ",";
                s += Format(rz == -99 ? "" : rz + "", "_");
                s += ")(";
                s += Format(x == -99 ? "" : x + "", "_");
                s += ",";
                s += Format(y == -99 ? "" : y + "", "_");
                s += ",";
                s += Format(z == -99 ? "" : z + "", "_");
                s += ")";

                return s;
            }

            private string Format(string f, string p)
            {
                string s = "";
                string n = f + "";
                if (n.Length > 5)
                {
                    n = n.Substring(0, 5);
                }

                for (int i = 0; i < 5 - n.Length; i++)
                    s += p;

                s += n;

                return s;
            }
        }

        public void PopulateBoneList()
        {
            for(int i = 0; i < 0x32; i++)
            {
                TreeNode t = new TreeNode();
                t.Text = "Bone_" + i;
                t.Tag = i;
                treeView1.Nodes.Add(t);
            }
        }

        public void Generate(int boneid)
        {
            Frame[] frames = new Frame[anim.frameCount+1];
            listBox1.Items.Clear();
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = new Frame(i);
            }

            int x = 0, y = 0, z = 0, rx = 0, ry = 0, rz = 0, sx = 0, sy = 0, sz = 0;

            foreach (DAT_Animation.DATAnimTrack track in anim.nodes[boneid])
                foreach (DAT_Animation.KeyNode node in track.keys)
                {
                    if (node.frame >= frames.Length)
                        continue;
                    try
                    {
                        switch (track.type)
                        {
                            case DAT_Animation.AnimType.XPOS: frames[x].x = node.value; x += (int)node.frame; break;
                            case DAT_Animation.AnimType.YPOS: frames[y].y = node.value; y += (int)node.frame; break;
                            case DAT_Animation.AnimType.ZPOS: frames[z].z = node.value; z += (int)node.frame; break;
                            case DAT_Animation.AnimType.XROT: frames[rx].rx = node.value * 180 / (float)Math.PI; rx += (int)node.frame; break;
                            case DAT_Animation.AnimType.YROT: frames[ry].ry = node.value * 180 / (float)Math.PI; ry += (int)node.frame; break;
                            case DAT_Animation.AnimType.ZROT: frames[rz].rz = node.value * 180 / (float)Math.PI; rz += (int)node.frame; break;
                            case DAT_Animation.AnimType.XSCA: frames[sx].sx = node.value; sx += (int)node.frame; break;
                            case DAT_Animation.AnimType.YSCA: frames[sy].sy = node.value; sy += (int)node.frame; break;
                            case DAT_Animation.AnimType.ZSCA: frames[sz].sz = node.value; sz += (int)node.frame; break;
                        }
                    }
                    catch (IndexOutOfRangeException e)
                    {

                    }
                }

            for (int i = 0; i < frames.Length; i++)
            {
                listBox1.Items.Add(frames[i]);
            }
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
        }

        private void treeView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Generate((int)treeView1.SelectedNode.Tag);
            }
        }

        public SkelAnimation BakeToSkel(VBN skel)
        {
            SkelAnimation a = new SkelAnimation();

            for (int i = 1; i < anim.frameCount+1; i++)
                a.frames.Add(new KeyFrame());

            for (int i = 0; i < anim.nodes.Count; i++)
            {
                Frame[] frames = Bake(i);

                int fr = 0;
                foreach(Frame f in frames)
                {
                    if(fr == 0)
                    {
                        fr++;
                        continue;
                    }
                    if (i >= skel.bones.Count)
                        continue;

                    KeyFrame frame = a.frames[fr-1];
                    KeyNode node = new KeyNode();
                    frame.nodes.Add(node);

                    node.hash = skel.bones[i].boneId;

                    node.t_type = 1;
                    node.r_type = 1;
                    node.s_type = 1;

                    node.t.X = f.x != -99 ? f.x : skel.bones[i].position[0];
                    node.t.Y = f.y != -99 ? f.y : skel.bones[i].position[1];
                    node.t.Z = f.z != -99 ? f.z : skel.bones[i].position[2];

                    node.r.X = f.rx != -99 ? f.rx * (float)Math.PI / 180 : skel.bones[i].rotation[0];
                    node.r.Y = f.ry != -99 ? f.ry * (float)Math.PI / 180 : skel.bones[i].rotation[1];
                    node.r.Z = f.rz != -99 ? f.rz * (float)Math.PI / 180 : skel.bones[i].rotation[2];

                    node.s.X = f.sx != -99 ? f.sx : skel.bones[i].scale[0];
                    node.s.Y = f.sy != -99 ? f.sy : skel.bones[i].scale[1];
                    node.s.Z = f.sz != -99 ? f.sz : skel.bones[i].scale[2];

                    node.r = VBN.FromEulerAngles(node.r.Z, node.r.Y, node.r.X);

                    fr++;
                }
            }

            return a;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            frames = Bake((int)treeView1.SelectedNode.Tag);
            listBox1.Items.Clear();
            for (int i = 0; i < frames.Length; i++)
            {
                listBox1.Items.Add(frames[i]);
            }
            Render();
        }

        public void Render()
        {
            glControl1.MakeCurrent();

            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            float max = 0, min = 99;

            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i].rx > max) max = frames[i].rx;
                if (frames[i].rx < min) min = frames[i].rx;
            }

            max -= min;
            GL.PointSize(5f);
            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex2(-1, 0);
            GL.Vertex2(1, 0);
            GL.End();

            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.LineStrip);
            for(int i = 0;i < frames.Length; i++)
            {
                GL.Vertex2((float)i / frames.Length * 2 - 1, ((frames[i].rx-min)/max)*2 - 1);
            }
            GL.End();

            GL.Color3(Color.Black);
            GL.Begin(PrimitiveType.Lines);
            for (int i = 0; i < frames.Length; i++)
            {
                GL.Vertex2((float)i / frames.Length * 2 - 1, 0f-1);
                GL.Vertex2((float)i / frames.Length * 2 - 1, 0.2f-1);
            }
            GL.End();

            glControl1.SwapBuffers();
        }

        private Frame[] Bake(int bone)
        {
            Frame[] frames = new Frame[anim.frameCount];
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = new Frame(i);
            }

            foreach (DAT_Animation.DATAnimTrack track in anim.nodes[bone])
            {
                int f = 0, ni = 0, currentFrame = 0, nextFrame = 0;
                DAT_Animation.KeyNode node = track.keys[ni++];
                nextFrame = (int)node.frame;
                float cvalue = node.value, ctan = node.tan;
                while (f < anim.frameCount)
                    try
                    {
                        // calculate value
                        float nvalue = -99, ntan = -99;
                        for(int j = ni; j < track.keys.Count; j++)
                        {
                            //if (track.keys[j].interpolationType != DAT_Animation.InterpolationType.HermiteCurve)
                            {
                                if (track.keys[j].interpolationType == DAT_Animation.InterpolationType.HermiteCurve)
                                    ctan = track.keys[j].tan;
                                else
                                if (track.keys[j].tan != -99 && ntan == -99)
                                    ntan = track.keys[j].tan;
                                if (track.keys[j].value != -99 && nvalue == -99)
                                    nvalue = track.keys[j].value;
                                if (nvalue != -99 && ntan != -99)
                                    break;
                            }
                        }
                        if (nvalue == -99)
                            nvalue = track.keys[0].value;
                        if (ntan == -99)
                            ntan = 0;
                        if (ctan == -99)
                            ctan = 0;

                        float value = 0;
                        //Console.WriteLine(track.type + " " + node.interpolationType);
                        switch (node.interpolationType)
                        {
                            case DAT_Animation.InterpolationType.Hermite:
                                {
                                    cvalue = node.value;
                                    //value = Interpolate(f - currentFrame, nextFrame - currentFrame, cvalue, nvalue, ctan, ntan);
                                    value = CHR0.interHermite(f, currentFrame, nextFrame+1, ctan, ntan, cvalue, nvalue);
                                }
                                break;
                            case DAT_Animation.InterpolationType.HermiteValue:
                                {
                                    cvalue = node.value;
                                    ctan = 0;
                                    //value = Interpolate(f - currentFrame, nextFrame - currentFrame, cvalue, nvalue, ctan, ntan);
                                    value = CHR0.interHermite(f, currentFrame, nextFrame+1, 0, 0, cvalue, nvalue);
                                }
                                break;
                           case DAT_Animation.InterpolationType.Step:
                               {
                                   value = node.value;
                               }
                               break;
                            case DAT_Animation.InterpolationType.Linear:
                                {
                                    cvalue = node.value;
                                    value = CHR0.lerp(cvalue, nvalue, currentFrame, nextFrame, f);
                                }
                                break;
                            default:
                                Console.WriteLine(node.interpolationType);
                                break;
                        }

                        if (float.IsNaN(value) || f == currentFrame)
                            value = node.value;

                        switch (track.type)
                        {
                            case DAT_Animation.AnimType.XROT:
                                //Console.WriteLine(ctan + " " + ntan + " " + node.value + " " + nvalue + " " + f + " " + currentFrame + " " + nextFrame);
                                frames[f].rx = value * 180 / (float)Math.PI;
                                break;
                            case DAT_Animation.AnimType.YROT:
                                //Console.WriteLine(ctan + " " + ntan + " " + cvalue + " " + nvalue + " " + f + " " + currentFrame + " " + nextFrame);

                                frames[f].ry = value * 180 / (float)Math.PI;
                                break;
                            case DAT_Animation.AnimType.ZROT:
                                //Console.WriteLine(ctan + " " + ntan + " " + cvalue + " " + nvalue + " " + f + " " + currentFrame + " " + nextFrame);
                                frames[f].rz = value * 180 / (float)Math.PI; 
                                break;
                            case DAT_Animation.AnimType.XPOS: frames[f].x = value; break;
                            case DAT_Animation.AnimType.YPOS: frames[f].y = value; break;
                            case DAT_Animation.AnimType.ZPOS: frames[f].z = value; break;
                            case DAT_Animation.AnimType.XSCA: frames[f].sx = value; break;
                            case DAT_Animation.AnimType.YSCA: frames[f].sy = value; break;
                            case DAT_Animation.AnimType.ZSCA: frames[f].sz = value; break;
                        }
                        
                        f++;
                        if (f > nextFrame)
                        {

                            if (ni >= track.keys.Count)
                            {
                                node = track.keys[0];
                                continue;
                            }
                            node = track.keys[ni++];
                            while (node.interpolationType == DAT_Animation.InterpolationType.HermiteCurve)
                            {
                                node = track.keys[ni++];
                            }
                            currentFrame = f;
                            nextFrame += (int)node.frame;

                            switch (node.interpolationType)
                            {
                                case DAT_Animation.InterpolationType.Hermite:
                                    {
                                        ctan = node.tan;
                                    }
                                    break;
                            }
                        }
                    }
                    catch (IndexOutOfRangeException ex)
                    {

                    }
            }
            return frames;
        }

        public float Interpolate(float offset, float span, float _value, float _nvalue, float tan, float ntan)
        {
            //Return this value if no offset from this keyframe
            if (offset == 0)
                return _value;

            //Return next value if offset is to the next keyframe
            if (offset == span)
                return _nvalue;

            //Get the difference in values
            float diff = _nvalue - _value;

            //Calculate a percentage from this keyframe to the next
            float time = offset / span; //Normalized, 0 to 1

            //bool prevDouble = _prev._index >= 0 && _prev._index == _index - 1;
            //bool nextDouble = next._next._index >= 0 && next._next._index == next._index + 1;
            //bool oneApart = _next._index == _index + 1;
            
            //if (prevDouble || oneApart)
            //    tan = (next._value - _value) / (next._index - _index);
            //if (nextDouble || oneApart)
            //    nextTan = (next._value - _value) / (next._index - _index);

            //Interpolate using a hermite curve
            float inv = time - 1.0f; //-1 to 0
            return _value
                + (offset * inv * ((inv * tan) + (time * ntan)))
                + ((time * time) * (3.0f - 2.0f * time) * diff);
        }

        public Animation toAnimation(VBN vbn)
        {
            Animation animation = new Animation(anim.Name);
            animation.FrameCount = anim.frameCount;

            int i = 0;
            foreach (Bone b in vbn.bones)
            {
                i = vbn.boneIndex(b.Text);

                if (i < anim.nodes.Count)
                {
                    List<DAT_Animation.DATAnimTrack> tracks = anim.nodes[i];
                    
                    Animation.KeyNode node = new Animation.KeyNode(b.Text);
                    node.RotType = Animation.RotationType.EULER;

                    foreach (DAT_Animation.DATAnimTrack track in tracks)
                    {
                        switch (track.type)
                        {
                            case DAT_Animation.AnimType.XPOS:
                                node.XPOS = CreateKeyGroup(i, track, false);
                                break;
                            case DAT_Animation.AnimType.YPOS:
                                node.YPOS = CreateKeyGroup(i, track, false);
                                break;
                            case DAT_Animation.AnimType.ZPOS:
                                node.ZPOS = CreateKeyGroup(i, track, false);
                                break;
                            case DAT_Animation.AnimType.XROT:
                                node.XROT = CreateKeyGroup(i, track, false);
                                break;
                            case DAT_Animation.AnimType.YROT:
                                node.YROT = CreateKeyGroup(i, track, false);
                                break;
                            case DAT_Animation.AnimType.ZROT:
                                node.ZROT = CreateKeyGroup(i, track, false);
                                break;
                            case DAT_Animation.AnimType.XSCA:
                                node.XSCA = CreateKeyGroup(i, track, false);
                                break;
                            case DAT_Animation.AnimType.YSCA:
                                node.YSCA = CreateKeyGroup(i, track, false);
                                break;
                            case DAT_Animation.AnimType.ZSCA:
                                node.ZSCA = CreateKeyGroup(i, track, false);
                                break;
                        }
                    }

                    if(node.XSCA.HasAnimation() || node.YSCA.HasAnimation() || node.ZSCA.HasAnimation()
                        || node.XPOS.HasAnimation() || node.YPOS.HasAnimation() || node.ZPOS.HasAnimation()
                        || node.XROT.HasAnimation() || node.YROT.HasAnimation() || node.ZROT.HasAnimation())
                        animation.Bones.Add(node);
                }
            }

            return animation;
        }

        public void createANIM(string fname, VBN vbn)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@fname))
            {
                file.WriteLine("animVersion 1.1;");
                file.WriteLine("mayaVersion 2014 x64;\ntimeUnit ntscf;\nlinearUnit cm;\nangularUnit deg;\nstartTime 1;\nendTime " + (anim.frameCount+1) + ";");
                
                int i = 0;

                // writing node attributes
                foreach (Bone b in vbn.getBoneTreeOrder())
                {
                    i = vbn.boneIndex(b.Text);

                    if (i < anim.nodes.Count)
                    {
                        // write the bone attributes
                        // count the attributes
                        List<DAT_Animation.DATAnimTrack> tracks = anim.nodes[i];

                        int tracknum = 0;
                        if (tracks.Count == 0)
                            file.WriteLine("anim " + b.Text + " 0 0 0;");
                               
                        foreach (DAT_Animation.DATAnimTrack track in tracks)
                        {
                            switch (track.type)
                            {
                                case DAT_Animation.AnimType.XPOS:
                                    file.WriteLine("anim translate.translateX translateX " + b.Text + " 0 0 " + (tracknum++) + ";");
                                    WriteAnimKey(file, i, track, false);
                                    break;
                                case DAT_Animation.AnimType.YPOS:
                                    file.WriteLine("anim translate.translateY translateY " + b.Text + " 0 0 " + (tracknum++) + ";");
                                    WriteAnimKey(file, i, track, false);
                                    break;
                                case DAT_Animation.AnimType.ZPOS:
                                    file.WriteLine("anim translate.translateZ translateZ " + b.Text + " 0 0 " + (tracknum++) + ";");
                                    WriteAnimKey(file, i, track, false);
                                    break;
                                case DAT_Animation.AnimType.XROT:
                                    file.WriteLine("anim rotate.rotateX rotateX " + b.Text + " 0 0 " + (tracknum++) + ";");
                                    WriteAnimKey(file, i, track, true);
                                    break;
                                case DAT_Animation.AnimType.YROT:
                                    file.WriteLine("anim rotate.rotateY rotateY " + b.Text + " 0 0 " + (tracknum++) + ";");
                                    WriteAnimKey(file, i, track, true);
                                    break;
                                case DAT_Animation.AnimType.ZROT:
                                    file.WriteLine("anim rotate.rotateZ rotateZ " + b.Text + " 0 0 " + (tracknum++) + ";");
                                    WriteAnimKey(file, i, track, true);
                                    break;
                                case DAT_Animation.AnimType.XSCA:
                                    file.WriteLine("anim scale.scaleX scaleX " + b.Text + " 0 0 " + (tracknum++) + ";");
                                    WriteAnimKey(file, i, track, false);
                                    break;
                                case DAT_Animation.AnimType.YSCA:
                                    file.WriteLine("anim scale.scaleY scaleY " + b.Text + " 0 0 " + (tracknum++) + ";");
                                    WriteAnimKey(file, i, track, false);
                                    break;
                                case DAT_Animation.AnimType.ZSCA:
                                    file.WriteLine("anim scale.scaleZ scaleZ " + b.Text + " 0 0 " + (tracknum++) + ";");
                                    WriteAnimKey(file, i, track, false);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        file.WriteLine("anim " + b.Text + " 0 0 0;");
                    }
                }
            }
        }

        private static void WriteAnimKey(StreamWriter file, int i, DAT_Animation.DATAnimTrack track, bool rotation)
        {

            file.WriteLine("animData {\n input time;\n output linear;\n weighted 1;\n preInfinity constant;\n postInfinity constant;\n keys {");

            int size = track.keys.Count;

            int time = 0;
            float cvalue = 0, ctan = 0;

            for (int f = 0; f < size; f++)
            {
                while (track.keys[f].interpolationType == DAT_Animation.InterpolationType.HermiteCurve)
                    f++;

                DAT_Animation.KeyNode no = track.keys[f];
                DAT_Animation.KeyNode curve = track.keys[f+1 >= track.keys.Count ? 0: f+1];

                if (curve.interpolationType == DAT_Animation.InterpolationType.HermiteCurve)
                {
                    cvalue = no.value;
                    ctan = curve.tan;
                    file.WriteLine(" " + (time + 1) + " {0:N6} fixed fixed 1 1 0 {1:N6} 1 {2:N6} 1;", cvalue * (rotation ? 180 / (float)Math.PI : 1), no.tan, ctan);
                }
                else
                switch (no.interpolationType)
                {
                    case DAT_Animation.InterpolationType.Hermite:
                        {
                            cvalue = no.value;
                            ctan = no.tan;
                            file.WriteLine(" " + (time + 1) + " {0:N6} fixed fixed 1 1 0 {1:N6} 1 {2:N6} 1;", cvalue * (rotation ? 180 / (float)Math.PI : 1), ctan, ctan);
                        }
                        break;
                    case DAT_Animation.InterpolationType.HermiteValue:
                        {
                            cvalue = no.value;
                            ctan = 0;
                            file.WriteLine(" " + (time + 1) + " {0:N6} fixed fixed 1 1 0 {1:N6} 1 {2:N6} 1;", cvalue * (rotation ? 180 / (float)Math.PI : 1), ctan, ctan);
                        }
                        break;
                    case DAT_Animation.InterpolationType.Step:
                        {
                            cvalue = no.value;
                            file.WriteLine(" " + (time + 1) + " {0:N6} fixed fixed 1 1 0 0 1 0 1;", cvalue * (rotation ? 180 / (float)Math.PI : 1));
                        }
                        break;
                    case DAT_Animation.InterpolationType.Linear:
                        {
                            cvalue = no.value;
                            file.WriteLine(" " + (time + 1) + " {0:N6} linear linear 1 1 0", cvalue * (rotation ? 180 / (float)Math.PI : 1));
                        }
                        break;
                }

                time += (int)no.frame;
            }

            file.WriteLine(" }");
            file.WriteLine("}");
        }

        private static Animation.KeyGroup CreateKeyGroup(int i, DAT_Animation.DATAnimTrack track, bool rotation)
        {
            Animation.KeyGroup group = new Animation.KeyGroup();

            int size = track.keys.Count;

            int time = 0;
            float cvalue = 0, ctan = 0;

            for (int f = 0; f < size; f++)
            {
                while (track.keys[f].interpolationType == DAT_Animation.InterpolationType.HermiteCurve)
                    f++;

                DAT_Animation.KeyNode no = track.keys[f];
                DAT_Animation.KeyNode curve = track.keys[f + 1 >= track.keys.Count ? 0 : f + 1];

                if (curve.interpolationType == DAT_Animation.InterpolationType.HermiteCurve)
                {
                    cvalue = no.value;
                    ctan = curve.tan;
                    group.Keys.Add(new Animation.KeyFrame() { Weighted = true, Frame = time, Value = cvalue, In = no.tan, Out = ctan, InterType = Animation.InterpolationType.HERMITE});
                    //file.WriteLine(" " + (time + 1) + " {0:N6} fixed fixed 1 1 0 {1:N6} 1 {2:N6} 1;", cvalue * (rotation ? 180 / (float)Math.PI : 1), no.tan, ctan);
                }
                else
                    switch (no.interpolationType)
                    {
                        case DAT_Animation.InterpolationType.Hermite:
                            {
                                cvalue = no.value;
                                ctan = no.tan;
                                group.Keys.Add(new Animation.KeyFrame() { Weighted = true, Frame = time, Value = cvalue, In = ctan, Out = ctan, InterType = Animation.InterpolationType.HERMITE });
                                //file.WriteLine(" " + (time + 1) + " {0:N6} fixed fixed 1 1 0 {1:N6} 1 {2:N6} 1;", cvalue * (rotation ? 180 / (float)Math.PI : 1), ctan, ctan);
                            }
                            break;
                        case DAT_Animation.InterpolationType.HermiteValue:
                            {
                                cvalue = no.value;
                                ctan = 0;
                                group.Keys.Add(new Animation.KeyFrame() { Weighted = true, Frame = time, Value = cvalue, In = ctan, Out = ctan, InterType = Animation.InterpolationType.HERMITE });
                                //file.WriteLine(" " + (time + 1) + " {0:N6} fixed fixed 1 1 0 {1:N6} 1 {2:N6} 1;", cvalue * (rotation ? 180 / (float)Math.PI : 1), ctan, ctan);
                            }
                            break;
                        case DAT_Animation.InterpolationType.Step:
                            {
                                cvalue = no.value;
                                group.Keys.Add(new Animation.KeyFrame() { Weighted = false, Frame = time, Value = cvalue, InterType = Animation.InterpolationType.STEP });
                                //file.WriteLine(" " + (time + 1) + " {0:N6} fixed fixed 1 1 0 0 1 0 1;", cvalue * (rotation ? 180 / (float)Math.PI : 1));
                            }
                            break;
                        case DAT_Animation.InterpolationType.Linear:
                            {
                                cvalue = no.value;
                                group.Keys.Add(new Animation.KeyFrame() { Weighted = false, Frame = time, Value = cvalue, InterType = Animation.InterpolationType.LINEAR });
                                //file.WriteLine(" " + (time + 1) + " {0:N6} linear linear 1 1 0", cvalue * (rotation ? 180 / (float)Math.PI : 1));
                            }
                            break;
                    }

                time += (int)no.frame;
            }
            return group;
        }
    }
}
