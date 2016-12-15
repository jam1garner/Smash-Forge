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

namespace Smash_Forge
{
    public partial class AnimTrack : DockContent
    {
        public DAT_Animation anim;

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
                s += Format(frame, " ");
                s += "](";
                s += Format(sx, "_");
                s += ",";
                s += Format(sy, "_");
                s += ",";
                s += Format(sz, "_");
                s += ")(";
                s += Format(rx, "_");
                s += ",";
                s += Format(ry, "_");
                s += ",";
                s += Format(rz, "_");
                s += ")(";
                s += Format(x, "_");
                s += ",";
                s += Format(y, "_");
                s += ",";
                s += Format(z, "_");
                s += ")";

                return s;
            }

            private string Format(float f, string p)
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

            foreach (DAT_Animation.AnimTrack track in anim.nodes[boneid])
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
                //Generate((int)treeView1.SelectedNode.Tag);
            }
        }

        public SkelAnimation BakeToSkel(VBN skel)
        {
            SkelAnimation a = new SkelAnimation();

            for (int i = 0; i < anim.frameCount+1; i++)
                a.frames.Add(new KeyFrame());

            for (int i = 0; i < anim.nodes.Count; i++)
            {
                Frame[] frames = Bake(i);

                int fr = 0;
                foreach(Frame f in frames)
                {
                    KeyFrame frame = a.frames[fr];
                    KeyNode node = new KeyNode();
                    frame.nodes.Add(node);

                    node.hash = skel.bones[i].boneId;

                    node.t_type = 1;
                    node.r_type = 1;
                    node.s_type = 1;

                    node.t.X = f.x != -99 ? f.x : skel.bones[i].position[0];
                    node.t.Y = f.y != -99 ? f.y : skel.bones[i].position[1];
                    node.t.Z = f.z != -99 ? f.z : skel.bones[i].position[2];

                    node.r.X = f.rx != -99 ? f.rx * (float)Math.PI/180 : skel.bones[i].rotation[0];
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
            Frame[] frames = Bake((int)treeView1.SelectedNode.Tag);
            listBox1.Items.Clear();
            for (int i = 0; i < frames.Length; i++)
            {
                listBox1.Items.Add(frames[i]);
            }

        }

        private Frame[] Bake(int bone)
        {
            Frame[] frames = new Frame[anim.frameCount + 1];
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = new Frame(i);
            }

            foreach (DAT_Animation.AnimTrack track in anim.nodes[bone])
            {
                int f = 0, ni = 0, currentFrame = 0, nextFrame = 0;
                DAT_Animation.KeyNode node = track.keys[ni++];
                nextFrame = (int)node.frame;
                float cvalue = node.value, ctan = node.tan;
                while (f < anim.frameCount && ni < track.keys.Count - 1)
                    try
                    {
                        // calculate value
                        DAT_Animation.KeyNode next;
                        if (ni >= track.keys.Count)
                            next = track.keys[0];
                        else
                            next = track.keys[ni];

                        float value = 0;

                        switch (node.interpolationType)
                        {
                            case DAT_Animation.InterpolationType.Hermite:
                                {
                                    value = CHR0.interHermite(f, currentFrame, nextFrame, next.tan, node.tan, node.value, next.value);
                                }
                                break;
                            case DAT_Animation.InterpolationType.HermiteValue:
                                {
                                    value = CHR0.interHermite(f, currentFrame, nextFrame, 0, next.tan, node.value, next.value);
                                }
                                break;
                            /*case DAT_Animation.InterpolationType.HermiteCurve:
                               {
                                   value = CHR0.interHermite(f, currentFrame, nextFrame, next.tan, node.tan, cvalue, next.value);
                               }
                               break;
                           case DAT_Animation.InterpolationType.Step:
                               {
                                   value = node.value;
                               }
                               break;*/
                            default:
                                Console.WriteLine(node.interpolationType);
                                break;
                        }

                        if (f > nextFrame)
                        {
                            node = track.keys[ni++];
                            currentFrame = f;
                            nextFrame += (int)node.frame;
                            cvalue = node.value;
                            ctan = node.tan;
                        }

                        switch (track.type)
                        {
                            case DAT_Animation.AnimType.XROT:
                                //Console.WriteLine(node.tan + " " + next.tan + " " + node.value + " " + next.value + " " + f + " " + currentFrame + " " + nextFrame);
                                frames[f].rx = value * 180 / (float)Math.PI;
                                break;
                            case DAT_Animation.AnimType.YROT:
                                //Console.WriteLine(node.tan + " " + next.tan + " " + node.value + " " + next.value + " " + f + " " + currentFrame + " " + nextFrame);

                                frames[f].ry = value * 180 / (float)Math.PI;
                                break;
                            case DAT_Animation.AnimType.ZROT: frames[f].rz = value * 180 / (float)Math.PI; break;
                            case DAT_Animation.AnimType.XPOS: frames[f].x = value; break;
                            case DAT_Animation.AnimType.YPOS: frames[f].y = value; break;
                            case DAT_Animation.AnimType.ZPOS: frames[f].z = value; break;
                            case DAT_Animation.AnimType.XSCA: frames[f].sx = value; break;
                            case DAT_Animation.AnimType.YSCA: frames[f].sy = value; break;
                            case DAT_Animation.AnimType.ZSCA: frames[f].sz = value; break;
                        }

                        f++;
                    }
                    catch (IndexOutOfRangeException ex)
                    {

                    }
            }
            return frames;
        }
    }
}
