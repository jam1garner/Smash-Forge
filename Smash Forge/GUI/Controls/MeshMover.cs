using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;

namespace Smash_Forge
{
    public partial class MeshMover : Form
    {
        public NUD.Mesh mesh;
        public int prevValue = 5;

        public MeshMover()
        {
            InitializeComponent();
            
            posIncBox.Text = "0.5";
            rotIncBox.Text = "90";
            scaIncBox.Text = "0.5";
        }

        public new void MouseUp(object sender, MouseEventArgs e)
        {
            prevValue = 5;
            posXTB.Value = 5;
            posYTB.Value = 5;
            posZTB.Value = 5;
            rotXTB.Value = 5;
            rotYTB.Value = 5;
            rotZTB.Value = 5;
            scaXTB.Value = 5;
        }

        public void Move(int type, float move)
        {
            // move mesh over
            foreach (NUD.Polygon p in mesh.Nodes)
            {
                foreach (NUD.Vertex v in p.vertices)
                {
                    switch (type)
                    {
                        case 1: v.pos.X += move; break;
                        case 2: v.pos.Y += move; break;
                        case 3: v.pos.Z += move; break;
                        case 4: v.pos = Vector3.Transform(v.pos, Matrix4.CreateRotationX(move * ((float)Math.PI / 180))); break;
                        case 5: v.pos = Vector3.Transform(v.pos, Matrix4.CreateRotationY(move * ((float)Math.PI / 180))); break;
                        case 6: v.pos = Vector3.Transform(v.pos, Matrix4.CreateRotationZ(move * ((float)Math.PI / 180))); break;
                        case 7: v.pos = Vector3.Multiply(v.pos, move); break;
                    }
                    
                }
                p.PreRender();
            }
        }

        private void posXTB_ValueChanged(object sender, EventArgs e)
        {
            float amt = 0;
            if (float.TryParse(posIncBox.Text, out amt))
            {
                float move = (posXTB.Value - prevValue) * amt;
                Move(1, move);
                prevValue = posXTB.Value;
            }
        }

        private void posYTB_ValueChanged(object sender, EventArgs e)
        {
            float amt = 0;
            if (float.TryParse(posIncBox.Text, out amt))
            {
                float move = (posYTB.Value - prevValue) * amt;
                Move(2, move);
                prevValue = posYTB.Value;
            }
        }

        private void posZTB_ValueChanged(object sender, EventArgs e)
        {
            float amt = 0;
            if (float.TryParse(posIncBox.Text, out amt))
            {
                float move = (posZTB.Value - prevValue) * amt;
                Move(3, move);
                prevValue = posZTB.Value;
            }
        }

        private void rotXTB_ValueChanged(object sender, EventArgs e)
        {
            float amt = 0;
            if (float.TryParse(rotIncBox.Text, out amt))
            {
                float move = (rotXTB.Value - prevValue) * amt;
                Move(4, move);
                prevValue = rotXTB.Value;
            }
        }

        private void rotYTB_ValueChanged(object sender, EventArgs e)
        {
            float amt = 0;
            if (float.TryParse(rotIncBox.Text, out amt))
            {
                float move = (rotYTB.Value - prevValue) * amt;
                Move(5, move);
                prevValue = rotYTB.Value;
            }
        }

        private void rotZTB_ValueChanged(object sender, EventArgs e)
        {
            float amt = 0;
            if (float.TryParse(rotIncBox.Text, out amt))
            {
                float move = (rotZTB.Value - prevValue) * amt;
                Move(6, move);
                prevValue = rotZTB.Value;
            }
        }

        private void scaXTB_ValueChanged(object sender, EventArgs e)
        {
            float amt = 0;
            if (float.TryParse(scaIncBox.Text, out amt))
            {
                float move = 1 + ((scaXTB.Value - prevValue) * amt);
                Move(7, move);
                prevValue = scaXTB.Value;
            }
        }
    }
}
