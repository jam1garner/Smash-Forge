using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SmashForge
{
    public class Animation : TreeNode
    {
        public float frame = 0;
        public int frameCount = 0;

        public List<object> children = new List<object>();

        public List<KeyNode> bones = new List<KeyNode>();

        public Animation(string name)
        {
            Text = name;
            ImageKey = "anim";
            SelectedImageKey = "anim";

            ContextMenu cm = new ContextMenu();

            MenuItem export = new MenuItem("Export As");
            export.Click += SaveAs;
            cm.MenuItems.Add(export);

            MenuItem replace = new MenuItem("Replace");
            replace.Click += ReplaceAnimation;
            cm.MenuItems.Add(replace);

            MenuItem remove = new MenuItem("Remove");
            remove.Click += RemoveAnimation;
            cm.MenuItems.Add(remove);

            ContextMenu = cm;
        }

        #region Events

        public void RemoveAnimation(object sender, EventArgs args)
        {
            if (Parent != null)
                Parent.Nodes.Remove(this);
            else
            {
                MainForm.Instance.animList.treeView1.Nodes.Remove(this);
            }
        }

        public void ReplaceAnimation(object sender, EventArgs args)
        {
            using (OpenFileDialog of = new OpenFileDialog())
            {
                of.ShowDialog();

                foreach (string filename in of.FileNames)
                {
                    if (filename.EndsWith(".omo"))
                    {
                        Animation a = OMOOld.read(new FileData(filename));
                        a.Text = filename;
                        ReplaceMe(a);
                    }
                    if (filename.EndsWith(".smd"))
                    {
                        Animation a = new Animation(filename.Replace(".smd", ""));
                        Smd.Read(filename, a, Runtime.TargetVbn);
                        ReplaceMe(a);
                    }
                    if (filename.EndsWith(".chr0"))
                    {
                        Animation a = (CHR0.read(new FileData(filename), Runtime.TargetVbn));
                        ReplaceMe(a);
                    }
                    if (filename.EndsWith(".anim"))
                    {
                        Animation a = (ANIM.read(filename, Runtime.TargetVbn));
                        ReplaceMe(a);
                    }
                }
            }
        }

        public void ReplaceMe(Animation a)
        {
            Tag = null;
            Nodes.Clear();
            bones.Clear();
            children.Clear();

            bones = a.bones;

            frameCount = a.frameCount;
        }

        public void SaveAs(object sender, EventArgs args)
        {
            if (Runtime.TargetVbn == null)
            {
                MessageBox.Show("You must have a bone-set (VBN) selected to save animations.");
                return;
            }
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Supported Files (.omo, .anim, .smd)|*.omo;*.anim;*.smd|" +
                             "Maya Anim (.anim)|*.anim|" +
                             "Object Motion (.omo)|*.omo|" +
                             "Source Animation (.smd)|*.smd|" +
                             "All Files (*.*)|*.*";

                sfd.DefaultExt = "smd"; //Set a default extension to prevent crashing if not specified by user
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    sfd.FileName = sfd.FileName;

                    if (sfd.FileName.EndsWith(".anim"))
                    {
                        if (Tag is AnimTrack)
                            ((AnimTrack)Tag).CreateAnim(sfd.FileName, Runtime.TargetVbn);
                        else
                            ANIM.CreateANIM(sfd.FileName, this, Runtime.TargetVbn);

                    }

                    if (sfd.FileName.EndsWith(".omo"))
                    {
                        if (Tag is FileData)
                        {
                            FileOutput o = new FileOutput();
                            o.WriteBytes(((FileData)Tag).GetSection(0,
                                ((FileData)Tag).Size()));
                            o.Save(sfd.FileName);
                        }
                        else
                            File.WriteAllBytes(sfd.FileName, OMOOld.CreateOMOFromAnimation(this, Runtime.TargetVbn));

                    }


                    if (sfd.FileName.EndsWith(".smd"))
                    {
                        Smd.Save(this, Runtime.TargetVbn, sfd.FileName);
                    }

                }
            }
        }

        #endregion

        public enum InterpolationType
        {
            Linear = 0,
            Constant,
            Hermite,
            Step,
        };

        public enum BoneType
        {
            Normal = 0,
            Swag,
            Helper
        }

        public enum RotationType
        {
            Euler = 0,
            Quaternion
        }


        public class KeyNode : TreeNode
        {
            public int hash = -1;
            public BoneType type = BoneType.Normal;

            public KeyGroup xpos = new KeyGroup() { Text = "XPOS" };
            public KeyGroup ypos = new KeyGroup() { Text = "YPOS" };
            public KeyGroup zpos = new KeyGroup() { Text = "ZPOS" };

            public RotationType rotType = RotationType.Quaternion;
            public KeyGroup xrot = new KeyGroup() { Text = "XROT" };
            public KeyGroup yrot = new KeyGroup() { Text = "YROT" };
            public KeyGroup zrot = new KeyGroup() { Text = "ZROT" };
            public KeyGroup wrot = new KeyGroup() { Text = "WROT" };

            public KeyGroup xsca = new KeyGroup() { Text = "XSCA" };
            public KeyGroup ysca = new KeyGroup() { Text = "YSCA" };
            public KeyGroup zsca = new KeyGroup() { Text = "ZSCA" };

            public KeyNode(string bname)
            {
                Text = bname;
                if (bname != null && bname.Equals("")) Text = hash.ToString("x");
                ImageKey = "bone";
                SelectedImageKey = "bone";
            }

            public void SetKeyFromBone(float frame, Bone bone)
            {
                Vector3 rot = ANIM.quattoeul(bone.rot);
                if (rot.X != bone.rotation[0] || rot.Y != bone.rotation[1] || rot.Z != bone.rotation[2])
                {
                    xrot.GetKeyFrame(frame).Value = bone.rot.X;
                    yrot.GetKeyFrame(frame).Value = bone.rot.Y;
                    zrot.GetKeyFrame(frame).Value = bone.rot.Z;
                    wrot.GetKeyFrame(frame).Value = bone.rot.W;
                }
                if (bone.pos.X != bone.position[0] || bone.pos.Y != bone.position[1] || bone.pos.Z != bone.position[2])
                {
                    xpos.GetKeyFrame(frame).Value = bone.pos.X;
                    ypos.GetKeyFrame(frame).Value = bone.pos.Y;
                    zpos.GetKeyFrame(frame).Value = bone.pos.Z;
                }
                if (bone.sca.X != bone.scale[0] || bone.sca.Y != bone.scale[1] || bone.sca.Z != bone.scale[2])
                {
                    xsca.GetKeyFrame(frame).Value = bone.sca.X;
                    ysca.GetKeyFrame(frame).Value = bone.sca.Y;
                    zsca.GetKeyFrame(frame).Value = bone.sca.Z;
                }
            }

            public void ExpandNodes()
            {
                xpos.Text = "XPOS";
                ypos.Text = "YPOS";
                zpos.Text = "ZPOS";
                xrot.Text = "XROT";
                yrot.Text = "YROT";
                zrot.Text = "ZROT";
                wrot.Text = "WROT";
                xsca.Text = "XSCA";
                ysca.Text = "YSCA";
                zsca.Text = "ZSCA";
                Nodes.Clear();
                Nodes.Add(xpos);
                Nodes.Add(ypos);
                Nodes.Add(zpos);
                Nodes.Add(xrot);
                Nodes.Add(yrot);
                Nodes.Add(zrot);
                Nodes.Add(wrot);
                Nodes.Add(xsca);
                Nodes.Add(ysca);
                Nodes.Add(zsca);
            }
        }

        public class KeyGroup : TreeNode
        {
            public bool HasAnimation()
            {
                return keys.Count > 0;
            }

            public List<KeyFrame> keys = new List<KeyFrame>();
            public float FrameCount
            {
                get
                {
                    float fc = 0;
                    foreach (KeyFrame k in keys)
                        if (k.Frame > fc) fc = k.Frame;
                    return fc;
                }
            }

            public KeyFrame GetKeyFrame(float frame)
            {
                KeyFrame key = null;
                int i;
                for (i = 0; i < keys.Count; i++)
                {
                    if (keys[i].Frame == frame)
                    {
                        key = keys[i];
                        break;
                    }
                    if (keys[i].Frame > frame)
                    {
                        break;
                    }
                }

                if (key == null)
                {
                    key = new KeyFrame();
                    key.Frame = frame;
                    keys.Insert(i, key);
                }

                return key;
            }

            int lastFound = 0;
            float lastFrame;
            public float GetValue(float frame)
            {
                KeyFrame k1 = (KeyFrame)keys[0], k2 = (KeyFrame)keys[0];
                int i = 0;
                if (frame < lastFrame)
                    lastFound = 0;
                for (i = lastFound; i < keys.Count; i++)
                {
                    lastFound = i % (keys.Count);
                    KeyFrame k = keys[lastFound];
                    if (k.Frame < frame)
                    {
                        k1 = k;
                    }
                    else
                    {
                        k2 = k;
                        break;
                    }
                }
                lastFound -= 1;
                if (lastFound < 0)
                    lastFound = 0;
                if (lastFound >= keys.Count - 2)
                    lastFound = 0;
                lastFrame = frame;

                if (k1.interType == InterpolationType.Constant)
                    return k1.Value;
                if (k1.interType == InterpolationType.Step)
                    return k1.Value;
                if (k1.interType == InterpolationType.Linear)
                {
                    return Lerp(k1.Value, k2.Value, k1.Frame, k2.Frame, frame);
                }
                if (k1.interType == InterpolationType.Hermite)
                {
                    float val = Hermite(frame, k1.Frame, k2.Frame, k1.In, k1.Out != -1 ? k1.Out : k2.In, k1.Value, k2.Value) * (k1.degrees ? (float)Math.PI / 180 : 1);
                    if (Parent != null && Text.Equals("XROT"))
                        Console.WriteLine(Text + " " + k1.Value + " " + k2.Value + " " + k1.Frame + " " + k2.Frame + " " + (val * 180 / (float)Math.PI));
                    if (float.IsNaN(val)) val = k1.value;

                    return val;//k1.Out != -1 ? k1.Out : 
                }

                return k1.Value;
            }

            public KeyFrame[] GetFrame(float frame)
            {
                if (keys.Count == 0) return null;
                KeyFrame k1 = (KeyFrame)keys[0], k2 = (KeyFrame)keys[0];
                foreach (KeyFrame k in keys)
                {
                    if (k.Frame < frame)
                    {
                        k1 = k;
                    }
                    else
                    {
                        k2 = k;
                        break;
                    }
                }

                return new KeyFrame[] { k1, k2 };
            }

            public void ExpandNodes()
            {
                Nodes.Clear();
                foreach (KeyFrame v in keys)
                {
                    Nodes.Add(v.GetNode());
                }
            }
        }

        public class KeyFrame
        {
            public float Value
            {
                get { if (degrees) return value * 180 / (float)Math.PI; else return value; }
                set { this.value = value; }//Text = _frame + " : " + _value; }
            }
            public float value;
            public float Frame
            {
                get { return frame; }
                set { frame = value; }//Text = _frame + " : " + _value; }
            }
            public string text;
            public float frame;
            public float In = 0, Out = -1;
            public bool weighted = false;
            public bool degrees = false; // Use Degrees
            public InterpolationType interType = InterpolationType.Linear;

            public KeyFrame(float value, float frame)
            {
                Value = value;
                Frame = frame;
            }

            public KeyFrame()
            {

            }

            public TreeNode GetNode()
            {
                TreeNode t = new TreeNode();
                t.Text = Frame + " : " + Value + (In != 0 ? " " + In.ToString() : "");
                t.Tag = this;
                return t;
            }

            public override string ToString()
            {
                return Frame + " " + Value;
            }
        }

        public void SetFrame(float frame)
        {
            this.frame = frame;
        }

        public int Size()
        {
            return frameCount;
        }

        public void NextFrame(VBN skeleton, bool isChild = false)
        {
            if (frame >= frameCount) return;

            if (frame == 0 && !isChild)
                skeleton.reset();

            foreach (object child in children)
            {
                if (child is Animation)
                {
                    ((Animation)child).SetFrame(frame);
                    ((Animation)child).NextFrame(skeleton, isChild: true);
                }
                if (child is MTA)
                {
                    //foreach (ModelContainer con in Runtime.ModelContainers)
                    {
                        if (((ModelContainer)skeleton.Parent).NUD != null)
                        {
                            ((ModelContainer)skeleton.Parent).NUD.ApplyMta(((MTA)child), (int)frame);
                        }
                    }
                }
                if (child is BFRES.MTA) //For BFRES
                {
                    {
                        if (((ModelContainer)skeleton.Parent).Bfres != null)
                        {
                            ((ModelContainer)skeleton.Parent).Bfres.ApplyMta(((BFRES.MTA)child), (int)frame);
                        }
                    }
                }
            }

            bool updated = false; // no need to update skeleton of animations that didn't change
            foreach (KeyNode node in bones)
            {
                // Get Skeleton Node
                Bone b = null;
                if (node.hash == -1)
                    b = skeleton.getBone(node.Text);
                else
                    b = skeleton.GetBone((uint)node.hash);
                if (b == null) continue;
                updated = true;

                if (node.xpos.HasAnimation() && b.boneType != 3)
                    b.pos.X = node.xpos.GetValue(frame);
                if (node.ypos.HasAnimation() && b.boneType != 3)
                    b.pos.Y = node.ypos.GetValue(frame);
                if (node.zpos.HasAnimation() && b.boneType != 3)
                    b.pos.Z = node.zpos.GetValue(frame);

                if (node.xsca.HasAnimation())
                    b.sca.X = node.xsca.GetValue(frame);
                else b.sca.X = 1;
                if (node.ysca.HasAnimation())
                    b.sca.Y = node.ysca.GetValue(frame);
                else b.sca.Y = 1;
                if (node.zsca.HasAnimation())
                    b.sca.Z = node.zsca.GetValue(frame);
                else b.sca.Z = 1;


                if (node.xrot.HasAnimation() || node.yrot.HasAnimation() || node.zrot.HasAnimation())
                {
                    if (node.rotType == RotationType.Quaternion)
                    {
                        KeyFrame[] x = node.xrot.GetFrame(frame);
                        KeyFrame[] y = node.yrot.GetFrame(frame);
                        KeyFrame[] z = node.zrot.GetFrame(frame);
                        KeyFrame[] w = node.wrot.GetFrame(frame);
                        Quaternion q1 = new Quaternion(x[0].Value, y[0].Value, z[0].Value, w[0].Value);
                        Quaternion q2 = new Quaternion(x[1].Value, y[1].Value, z[1].Value, w[1].Value);
                        if (x[0].Frame == frame)
                            b.rot = q1;
                        else
                        if (x[1].Frame == frame)
                            b.rot = q2;
                        else
                            b.rot = Quaternion.Slerp(q1, q2, (frame - x[0].Frame) / (x[1].Frame - x[0].Frame));
                    }
                    else
                    if (node.rotType == RotationType.Euler)
                    {
                        float x = node.xrot.HasAnimation() ? node.xrot.GetValue(frame) : b.rotation[0];
                        float y = node.yrot.HasAnimation() ? node.yrot.GetValue(frame) : b.rotation[1];
                        float z = node.zrot.HasAnimation() ? node.zrot.GetValue(frame) : b.rotation[2];
                        b.rot = EulerToQuat(z, y, x);
                    }
                }
            }
            frame += 1f;
            if (frame >= frameCount)
            {
                frame = 0;
            }

            if (!isChild && updated)
            {
                skeleton.update();
            }
        }

        public void ExpandBones()
        {
            Nodes.Clear();
            foreach (var v in bones)
                Nodes.Add(v);
        }

        public bool HasBone(string name)
        {
            foreach (var v in bones)
                if (v.Text.Equals(name))
                    return true;
            return false;
        }

        public KeyNode GetBone(string name)
        {
            foreach (var v in bones)
                if (v.Text.Equals(name))
                    return v;
            return null;
        }

        #region  Interpolation


        public static float Hermite(float frame, float frame1, float frame2, float outslope, float inslope, float val1, float val2)
        {
            /*float offset = frame - frame1;
            float span = frame2 - frame1;
            if (offset == 0) return val1;
            if (offset == span) return val2;

            float diff = val2 - val1;

            float time = offset / span;
            
            //bool prevDouble = prevframe1 >= 0 && prevframe1 == frame1 - 1;
            //bool nextDouble = next._next._index >= 0 && next._next._index == next._index + 1;
            bool oneApart = frame2 == frame1 + 1;
            
            float tan = outslope, nextTan = inslope;
            if (oneApart)
                tan = (val2 - val1) / (frame2 - frame1);
            //if (oneApart)
                nextTan = (val2 - val1) / (frame2 - frame1);

            float inv = time - 1.0f; //-1 to 0
            return val1
                + (offset * inv * ((inv * tan) + (time * nextTan)))
                + ((time * time) * (3.0f - 2.0f * time) * diff);*/

            if (frame == frame1) return val1;
            if (frame == frame2) return val2;

            float distance = frame - frame1;
            float invDuration = 1f / (frame2 - frame1);
            float t = distance * invDuration;
            float t1 = t - 1f;
            return (val1 + ((((val1 - val2) * ((2f * t) - 3f)) * t) * t)) + ((distance * t1) * ((t1 * outslope) + (t * inslope)));
        }

        public static float Lerp(float av, float bv, float v0, float v1, float t)
        {
            if (v0 == v1) return av;

            if (t == v0) return av;
            if (t == v1) return bv;


            float mu = (t - v0) / (v1 - v0);
            return ((av * (1 - mu)) + (bv * mu));
        }

        public static Quaternion Slerp(Vector4 v0, Vector4 v1, double t)
        {
            v0.Normalize();
            v1.Normalize();

            double dot = Vector4.Dot(v0, v1);

            const double dotThreshold = 0.9995;
            if (Math.Abs(dot) > dotThreshold)
            {
                Vector4 result = v0 + new Vector4((float)t) * (v1 - v0);
                result.Normalize();
                return new Quaternion(result.Xyz, result.W);
            }
            if (dot < 0.0f)
            {
                v1 = -v1;
                dot = -dot;
            }

            if (dot < -1) dot = -1;
            if (dot > 1) dot = 1;
            double theta0 = Math.Acos(dot);  // theta_0 = angle between input vectors
            double theta = theta0 * t;    // theta = angle between v0 and result 

            Vector4 v2 = v1 - v0 * new Vector4((float)dot);
            v2.Normalize();              // { v0, v2 } is now an orthonormal basis

            Vector4 res = v0 * new Vector4((float)Math.Cos(theta)) + v2 * new Vector4((float)Math.Sign(theta));
            return new Quaternion(res.Xyz, res.W);
        }

        public static Quaternion EulerToQuat(float z, float y, float x)
        {
            {
                Quaternion xRotation = Quaternion.FromAxisAngle(Vector3.UnitX, x);
                Quaternion yRotation = Quaternion.FromAxisAngle(Vector3.UnitY, y);
                Quaternion zRotation = Quaternion.FromAxisAngle(Vector3.UnitZ, z);

                Quaternion q = (zRotation * yRotation * xRotation);

                if (q.W < 0)
                    q *= -1;

                //return xRotation * yRotation * zRotation;
                return q;
            }
        }

        #endregion

    }
}
