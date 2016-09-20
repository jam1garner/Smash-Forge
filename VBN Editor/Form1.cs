using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace VBN_Editor
{
	public partial class VBNRebuilder : Form
	{
		public VBN vbn;
		public bool vbnSet = false;
		public bool loaded = false;
		public DataTable tbl;
		private bool delete = false;
		private string toDelete;
        private string currentNode;


        public VBNRebuilder()
		{
			InitializeComponent();
		}

		private TreeNode buildBoneTree(int index)
		{
			List<TreeNode> children = new List<TreeNode>();
			foreach (int i in vbn.bones[index].children)
			{
				children.Add(buildBoneTree(i));
			}

			TreeNode temp = new TreeNode(new string(vbn.bones[index].boneName),children.ToArray());

			if (index == 0)
				treeView1.Nodes.Add(temp);

            temp.Expand();
            foreach (TreeNode t in children)
            t.Expand();

            return temp;

		}

		public void treeRefresh()
		{
			treeView1.Nodes.Clear();
			buildBoneTree(0);
		}

		private void openNUDToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string filename = "";
			SaveFileDialog save = new SaveFileDialog();
			save.Filter = "Smash 4 Boneset|*.vbn|All files(*.*)|*.*";
			DialogResult result = save.ShowDialog();

			if(result == DialogResult.OK)
			{
				filename = save.FileName;
				vbn.save(filename);
				//OMO.createOMO (anim, vbn, "C:\\Users\\ploaj_000\\Desktop\\WebGL\\test_outut.omo", -1, -1);
			}
		}

		private void openVBNToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string filename = " ";
			OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Supported Formats|*.vbn;*.mdl0|Smash 4 Boneset|*.vbn|Brawl/Wii Model Format|*.mdl0|All files(*.*)|*.*";
			DialogResult result = open.ShowDialog();

			if(result == DialogResult.OK)
			{
				filename = open.FileName;
                if (filename.EndsWith(".vbn"))
                    vbn = new VBN(filename);
                
                if (filename.EndsWith(".mdl0")) {
                    MDL0Bones mdl0 = new MDL0Bones();
                    vbn = mdl0.GetVBN(new FileData(filename));
                }

                treeRefresh();
                vbnSet = true;
                
				//loadAnimation (ANIM.read ("C:\\Users\\ploaj_000\\Desktop\\WebGL\\Wait1.anim", vbn));
			}
		}

		private void importToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string filename = "";
			OpenFileDialog open = new OpenFileDialog();
			open.Filter = "Supported Formats|*.omo;*.chr0;*.anim|Object Motion|*.omo|Maya Animation|*.anim|Wii Animation|*.chr0|All files(*.*)|*.*";
			DialogResult result = open.ShowDialog();

			if(result == DialogResult.OK && vbn != null)
			{
				filename = open.FileName;
				if(open.FileName.Contains(".omo"))
					loadAnimation(OMO.read (new FileData (filename), vbn));
				if(open.FileName.Contains(".chr0"))
					loadAnimation(CHR0.read (new FileData(filename), vbn));
				if(open.FileName.Contains(".anim"))
					loadAnimation(ANIM.read (filename, vbn));
			}
		}

		private void exportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string filename = "";
			SaveFileDialog save = new SaveFileDialog();
			save.Filter = "Object Motion|*.omo|All files(*.*)|*.*";
			DialogResult result = save.ShowDialog();

			if(result == DialogResult.OK && vbn != null && anim != null)
			{
				filename = save.FileName;
				OMO.createOMO (anim, vbn, filename, -1, -1);
			}
		}


		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var newForm = new Form2 ();
			newForm.ShowDialog();
		}

		private void glControl1_Load(object sender, EventArgs e)
		{
			loaded = true;
			GL.ClearColor(Color.White);
			Application.Idle += AppIdle;
			SetupViewPort();
		}


		// for drawing
		public static Matrix4 scale = Matrix4.CreateScale (new Vector3 (0.5f, 0.5f, 0.5f));
		Matrix4 v;
		float rot = 0;
		float lookup = 0;
		float height = 0;
		float width = 0;
		float zoom = 0;
		float mouseXLast = 0;
		float mouseYLast = 0;
		float mouseSLast = 0;


		// animation
		SkelAnimation anim;
		bool isPlaying = false;

		private void SetupViewPort()
		{
			int h = glControl1.Height;
			int w = glControl1.Width;
			GL.LoadIdentity();
			GL.Viewport(0, 0, w, h);
			v = Matrix4.CreateRotationX (0.2f) *Matrix4.CreateTranslation(0, -5f, -15f) * Matrix4.CreatePerspectiveFieldOfView (1.3f, glControl1.Width / (float)glControl1.Height, 1.0f, 40.0f);

			glControl1.GotFocus += (object sender, EventArgs e) => {
				mouseXLast = OpenTK.Input.Mouse.GetState ().X;
				mouseYLast = OpenTK.Input.Mouse.GetState ().Y;
				zoom = OpenTK.Input.Mouse.GetState ().WheelPrecise;
				mouseSLast = zoom;
			};
		}

		private void Render()
		{
			if (!loaded)
				return;
			// clear the gf buffer
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


			if (anim != null && isPlaying) {
				//anim.nextFrame (vbn);
				this.numericUpDown2.Value = anim.getFrame ();
			}

			// enable depth test for grid...
			GL.Enable (EnableCap.DepthTest);

			// set up the viewport projection and send it to GPU
			GL.MatrixMode(MatrixMode.Projection);

			if (glControl1.Focused) {
				if (OpenTK.Input.Mouse.GetState ().IsButtonDown (OpenTK.Input.MouseButton.Right)) {
					height += 0.025f * (OpenTK.Input.Mouse.GetState ().Y - mouseYLast);
					width += 0.025f * (OpenTK.Input.Mouse.GetState ().X - mouseXLast);
				}
				if (OpenTK.Input.Mouse.GetState ().IsButtonDown (OpenTK.Input.MouseButton.Left)) {
					rot += 0.05f * (OpenTK.Input.Mouse.GetState ().X - mouseXLast);
					lookup += 0.05f * (OpenTK.Input.Mouse.GetState ().Y - mouseYLast);
				}
				v = Matrix4.CreateRotationY (0.5f * rot) * Matrix4.CreateRotationX (0.2f * lookup) * Matrix4.CreateTranslation (5 * width, -5f - 5f * height, -15f + zoom) * Matrix4.CreatePerspectiveFieldOfView (1.3f, glControl1.Width / (float)glControl1.Height, 1.0f, 40.0f);

				mouseXLast = OpenTK.Input.Mouse.GetState ().X;
				mouseYLast = OpenTK.Input.Mouse.GetState ().Y;
				mouseSLast = zoom;
				zoom = OpenTK.Input.Mouse.GetState ().WheelPrecise;
			}

			GL.LoadMatrix(ref v);

			// ready to start drawing model stuff
			GL.MatrixMode(MatrixMode.Modelview); 

			// draw the grid floor first
			drawGridFloor (Matrix4.CreateTranslation(Vector3.Zero));

			// clear the buffer bit so the skeleton will be drawn
			// on top of everything
			GL.Clear(ClearBufferMask.DepthBufferBit);

			// drawing the bones
			if (vbnSet)
			{
				foreach (Bone bone in vbn.bones)
				{
					// first calcuate the point and draw a point
					GL.PointSize (3.5f);
					GL.Color3(Color.Red);
					GL.Begin(PrimitiveType.Points);
					Vector3 pos_c = Vector3.Transform (Vector3.Zero, bone.transform * scale);
					GL.Vertex3 (pos_c);
					GL.End();


					// now draw line between parent 
					GL.Color3(Color.Blue);
					GL.LineWidth(1f);

					GL.Begin(PrimitiveType.Lines);
					if (bone.parentIndex != 0x0FFFFFFF)
					{
						uint i = bone.parentIndex;
						Vector3 pos_p = Vector3.Transform (Vector3.Zero, vbn.bones [(int)i].transform * scale);
						GL.Vertex3 (pos_c);
						GL.Vertex3 (pos_p);
					}
					GL.End();

				}
			}

			glControl1.SwapBuffers();
		}


		public void drawGridFloor(Matrix4 s){

			// Dropping some grid lines
			GL.Color3(Color.Green);
			GL.LineWidth(1f);
			GL.Begin(PrimitiveType.Lines);
			for (var i = -10; i <= 10; i++)
			{
				GL.Vertex3(Vector3.Transform(new Vector3(-10f, 0f, i), s));
				GL.Vertex3(Vector3.Transform(new Vector3(10f, 0f, i), s));
				GL.Vertex3(Vector3.Transform(new Vector3(i, 0f, -10f), s));
				GL.Vertex3(Vector3.Transform(new Vector3(i, 0f, 10f), s));
			}
			GL.End();
		}


		private void glControl1_Paint(object sender, PaintEventArgs e)
		{
			/*if (!loaded)
                return;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            glControl1.SwapBuffers();*/
		}

		private void AppIdle(object sender, EventArgs e)
		{
			while (glControl1.IsIdle)
			{
				Render ();
				System.Threading.Thread.Sleep(1000/60);
				if (delete)
				{
					vbn.deleteBone(toDelete);
					treeRefresh();
					delete = false;
				}

			}
		}


		// Animation Controls-------------------------------------------------------

		// loads a skeletal animation into the viewing system
		public void loadAnimation(SkelAnimation a){
			anim = a;
			this.numericUpDown1.Value = anim.size () - 1;
			anim.nextFrame (vbn);
		}

		// events for controls
		// this is the function that will actually update the skeleton
		private void NumericUpDown1_ValueChanged(Object sender, EventArgs e) {
			if (anim == null)
				return;

			if (this.numericUpDown2.Value >= anim.size ()){
				this.numericUpDown2.Value = 0;
				return;
			}
			if (this.numericUpDown2.Value < 0){
				this.numericUpDown2.Value = anim.size () - 1;
				return;
			}
			anim.setFrame ((int)this.numericUpDown2.Value);
			anim.nextFrame (vbn);
		}

		// play and frame increment buttons
		private void button1_Click(object sender, EventArgs e){
			this.numericUpDown2.Value = 0;
		}
		private void button2_Click(object sender, EventArgs e){
			if(this.numericUpDown2.Value - 1 >= 0)
				this.numericUpDown2.Value -= 1;
		}
		private void button3_Click(object sender, EventArgs e){
            if (anim != null)
                this.numericUpDown2.Value = anim.size() - 1;
		}
		private void button4_Click(object sender, EventArgs e){
            if (anim != null)
                this.numericUpDown2.Value += 1;
		}
		private void button5_Click(object sender, EventArgs e){
			isPlaying = !isPlaying;
			if(isPlaying)
				button5.Text = "Pause";
			else
				button5.Text = "Play";
		}


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
            currentNode = treeView1.SelectedNode.Text;
            textBox1.Text = treeView1.SelectedNode.Text;
			tbl = new DataTable();
			tbl.Columns.Add(new DataColumn("Name") { ReadOnly = true });
			tbl.Columns.Add("Value");
			dataGridView1.DataSource = tbl;
			tbl.Rows.Clear();

			tbl.Rows.Add("Bone Hash",vbn.bone(treeView1.SelectedNode.Text).boneId.ToString("X"));
			tbl.Rows.Add("Bone Type", vbn.bone(treeView1.SelectedNode.Text).boneType);
			tbl.Rows.Add("X Pos", vbn.bone(treeView1.SelectedNode.Text).position[0]);
			tbl.Rows.Add("Y Pos", vbn.bone(treeView1.SelectedNode.Text).position[1]);
			tbl.Rows.Add("Z Pos", vbn.bone(treeView1.SelectedNode.Text).position[2]);
			tbl.Rows.Add("X Rot", vbn.bone(treeView1.SelectedNode.Text).rotation[0]);
			tbl.Rows.Add("Y Rot", vbn.bone(treeView1.SelectedNode.Text).rotation[1]);
			tbl.Rows.Add("Z Rot", vbn.bone(treeView1.SelectedNode.Text).rotation[2]);
			tbl.Rows.Add("X Scale", vbn.bone(treeView1.SelectedNode.Text).scale[0]);
			tbl.Rows.Add("Y Scale", vbn.bone(treeView1.SelectedNode.Text).scale[1]);
			tbl.Rows.Add("Z Scale", vbn.bone(treeView1.SelectedNode.Text).scale[2]);
		}

		private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			Bone editingBone = vbn.bones[vbn.boneIndex(currentNode)];
			editingBone.boneId = (uint)int.Parse(tbl.Rows[0][1].ToString(), System.Globalization.NumberStyles.HexNumber);
			editingBone.boneType = Convert.ToUInt32(tbl.Rows[1][1]);
			vbn.bones[vbn.boneIndex(currentNode)] = editingBone;

			vbn.bone(currentNode).position[0] = Convert.ToSingle(tbl.Rows[2][1]);
			vbn.bone(currentNode).position[1] = Convert.ToSingle(tbl.Rows[3][1]);
			vbn.bone(currentNode).position[2] = Convert.ToSingle(tbl.Rows[4][1]);

			vbn.bone(currentNode).rotation[0] = Convert.ToSingle(tbl.Rows[5][1]);
			vbn.bone(currentNode).rotation[1] = Convert.ToSingle(tbl.Rows[6][1]);
			vbn.bone(currentNode).rotation[2] = Convert.ToSingle(tbl.Rows[7][1]);

			vbn.bone(currentNode).scale[0] = Convert.ToSingle(tbl.Rows[8][1]);
			vbn.bone(currentNode).scale[1] = Convert.ToSingle(tbl.Rows[9][1]);
			vbn.bone(currentNode).scale[2] = Convert.ToSingle(tbl.Rows[10][1]);

			vbn.update ();
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			Bone temp = vbn.bones[vbn.boneIndex(currentNode)];
			temp.boneName = textBox1.Text.ToCharArray();
			vbn.bones[vbn.boneIndex(currentNode)] = temp;
			treeView1.SelectedNode.Text = textBox1.Text;
		}

		private void addBoneToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var newForm = new Form3(this);
			newForm.ShowDialog();
		}

		private bool isAChildOfB(TreeNode a, TreeNode b)
		{
			return (a.Parent != null && (a.Parent == b || isAChildOfB(a.Parent, b)));
		}

		private void treeView1_DragDrop(object sender, DragEventArgs e)
		{
			Point targetPoint = treeView1.PointToClient(new Point(e.X, e.Y));

			TreeNode targetNode = treeView1.GetNodeAt(targetPoint);

			TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

			if (!draggedNode.Equals(targetNode) && targetNode != null && !isAChildOfB(targetNode, draggedNode))
			{
				int oldParent = (int)vbn.bones[vbn.boneIndex(draggedNode.Text)].parentIndex;
				vbn.bones[oldParent].children.Remove(vbn.boneIndex(draggedNode.Text));
				int newParent = vbn.boneIndex(targetNode.Text);
				Bone temp = vbn.bones[vbn.boneIndex(draggedNode.Text)];
				temp.parentIndex = (uint)newParent;
				vbn.bones[vbn.boneIndex(draggedNode.Text)] = temp;
				vbn.bones[newParent].children.Add(vbn.boneIndex(draggedNode.Text));

				draggedNode.Remove();
				targetNode.Nodes.Add(draggedNode);

				targetNode.Expand();
			}
		}

		private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
		{
			DoDragDrop(e.Item, DragDropEffects.Move);
		}

		private void treeView1_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
		}

		private void treeView1_KeyDown(object sender, KeyEventArgs e)
		{
			/*if(e.KeyCode == Keys.Delete)
            {
                delete = true;
                toDelete = treeView1.SelectedNode.Text;
            }*/

			//Deleting is currently broken... gotta find a fix
		}
	}
}
