using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using WeifenLuo.WinFormsUI.Docking;

namespace Smash_Forge
{
    public partial class MainForm : Form
    {
        public static MainForm Instance;

        public MainForm()
        {
            InitializeComponent();
            Instance = this;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            foreach (var vp in viewports)
                AddDockedControl(vp);

            animationsWindowToolStripMenuItem.Checked =
            boneTreeToolStripMenuItem.Checked = true;

            AddDockedControl(leftPanel);
            AddDockedControl(rightPanel);
            AddDockedControl(lvdEditor);
            AddDockedControl(project);
            AddDockedControl(lvdList);

            rightPanel.treeView1.Nodes.Add(animNode);
            rightPanel.treeView1.Nodes.Add(mtaNode);

            Runtime.renderBones = true;
            Runtime.renderLVD = true;
            Runtime.renderFloor = true;
            Runtime.renderHitboxes = true;
            Runtime.renderModel = true;
            Runtime.renderPath = true;
            Runtime.renderCollisions = true;
            Runtime.renderGeneralPoints = true;
            Runtime.renderItemSpawners = true;
            Runtime.renderSpawns = true;
            Runtime.renderRespawns = true;
            Runtime.renderOtherLVDEntries = true;
        }

        private void MainForm_Close(object sender, EventArgs e)
        {
            if (Runtime.TargetNUD != null)
                Runtime.TargetNUD.Destroy();

            foreach (ModelContainer n in Runtime.ModelContainers)
            {
                n.Destroy();
            }
            foreach (NUT n in Runtime.TextureContainers)
            {
                n.Destroy();
            }
        }

        public void AddDockedControl(DockContent content)
        {
            if (dockPanel1.DocumentStyle == DocumentStyle.SystemMdi)
            {
                content.MdiParent = this;
                content.Show();
            }
            else
                content.Show(dockPanel1);
        }

        #region Members
        public AnimListPanel rightPanel = new AnimListPanel() { ShowHint = DockState.DockRight };
        public BoneTreePanel leftPanel = new BoneTreePanel() { ShowHint = DockState.DockLeft };
        public TreeNode animNode = new TreeNode("Bone Animations");
        public TreeNode mtaNode = new TreeNode("Material Animations");
        public ProjectTree project = new ProjectTree() { ShowHint = DockState.DockLeft };
        public LVDList lvdList = new LVDList() { ShowHint = DockState.DockLeft };
        public LVDEditor lvdEditor = new LVDEditor() { ShowHint = DockState.DockRight };
        public List<PARAMEditor> paramEditors = new List<PARAMEditor>() { };
        public List<ACMDEditor> ACMDEditors = new List<ACMDEditor>() { };
        public MeshList meshList = new MeshList() { ShowHint = DockState.DockRight };
        public List<VBNViewport> viewports = new List<VBNViewport>() { new VBNViewport() }; // Default viewport
        #endregion

        #region ToolStripMenu
        private void openNUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PARAMEditor currentParam = null;
            ACMDEditor currentACMD = null;
            foreach(PARAMEditor p in paramEditors)
            {
                if(p.ContainsFocus)
                {
                    currentParam = p;
                }
            }
            foreach(ACMDEditor a in ACMDEditors)
            {
                if (a.ContainsFocus)
                {
                    currentACMD = a;
                }
            }
            if(currentParam != null)
            {
                currentParam.saveAs();
            }
            else
            if(currentACMD != null)
            {
                currentACMD.save();
            }
            else
            {
                string filename = "";
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Supported Filetypes (VBN,LVD)|*.vbn;*.lvd|Smash 4 Boneset|*.vbn|All files(*.*)|*.*";
                DialogResult result = save.ShowDialog();

                if (result == DialogResult.OK)
                {
                    filename = save.FileName;
                    if(filename.EndsWith(".vbn"))
                        Runtime.TargetVBN.Save(filename);
                    if (filename.EndsWith(".lvd") && Runtime.TargetLVD != null)
                        File.WriteAllBytes(filename, Runtime.TargetLVD.Rebuild());
                    //OMO.createOMO (anim, vbn, "C:\\Users\\ploaj_000\\Desktop\\WebGL\\test_outut.omo", -1, -1);
                }
            }
        }

        private void openDAE(string fname, ModelContainer con)
        {
            COLLADA model = COLLADA.Load(fname);

            NUD n = new NUD();
            if (con.vbn == null)
                return;
            VBN vbn = con.vbn;
            con.nud = n;

            // Iterate on libraries
            foreach (var item in model.Items)
            {
                var geometries = item as library_geometries;
                if (geometries != null)
                {
                    // Iterate on geomerty in library_geometries 
                    foreach (var geom in geometries.geometry)
                    {
                        var mesh = geom.Item as mesh;
                        if (mesh == null)
                            continue;

                        NUD.Mesh n_mesh = new NUD.Mesh();
                        n.mesh.Add(n_mesh);
                        n_mesh.name = geom.name;

                        Dictionary<string, double[]> sources = new Dictionary<string, double[]>();
                        Dictionary<string, string> semantic = new Dictionary<string, string>();

                        // Dump source[] for geom
                        foreach (var source in mesh.source)
                        {
                            var float_array = source.Item as float_array;
                            if (float_array == null)
                                continue;
                            sources.Add(source.id, float_array.Values);
                        }
                        {
                            var inputs = mesh.vertices.input;
                            foreach (var input in inputs)
                            {
                                semantic.Add(input.semantic, input.source);
                            }
                        }
                        // Dump Items[] for geom
                        foreach (var meshItem in mesh.Items)
                        {
                            if (meshItem is vertices)
                            {
                                var vertices = meshItem as vertices;
                                var inputs = vertices.input;
                                foreach (var input in inputs)
                                    semantic.Add(input.semantic, input.source);                             
                            }
                            else if (meshItem is triangles)
                            {
                                var triangles = meshItem as triangles;
                                var inputs = triangles.input;

                                NUD.Polygon poly = new NUD.Polygon();
                                poly.setDefaultMaterial();
                                n_mesh.polygons.Add(poly);
                                string[] ps = triangles.p.Split(' ');
                                int vCount = 0;
                                for (int i = 0; i < ps.Length; i++)
                                {
                                    poly.faces.Add(int.Parse(ps[i]));
                                    if (int.Parse(ps[i]) > vCount)
                                        vCount = int.Parse(ps[i]);
                                }

                                for (int i = 0; i < vCount + 1; i++)
                                {
                                    NUD.Vertex v = new NUD.Vertex();

                                    // iterate semantics
                                    foreach (string s in semantic.Keys)
                                    {
                                        string src;
                                        double[] bank;
                                        semantic.TryGetValue(s, out src);
                                        src = src.Replace("#", "");
                                        sources.TryGetValue(src, out bank);
                                        switch (s)
                                        {
                                            case "POSITION":
                                                v.pos.X = (float)bank[i * 3 + 0];
                                                v.pos.Y = (float)bank[i * 3 + 1];
                                                v.pos.Z = (float)bank[i * 3 + 2];
                                                break;
                                            case "NORMAL":
                                                v.nrm.X = (float)bank[i * 3 + 0];
                                                v.nrm.Y = (float)bank[i * 3 + 1];
                                                v.nrm.Z = (float)bank[i * 3 + 2];
                                                break;
                                            case "COLOR":
                                                v.col.X = (float)bank[i * 3 + 0] * 255;
                                                v.col.Y = (float)bank[i * 3 + 1] * 255;
                                                v.col.Z = (float)bank[i * 3 + 2] * 255;
                                                v.col.W = (float)bank[i * 3 + 2] * 255;
                                                break;
                                            case "TEXCOORD":
                                                v.tx.Add(new OpenTK.Vector2((float)bank[i * 2 + 0], (float)bank[i * 2 + 1]));
                                                break;
                                        }
                                    }
                                    poly.AddVertex(v);
                                }
                            }
                        }
                    }
                }

                var controllers = item as library_controllers;
                if (controllers != null)
                {
                    int cid = 0;
                    // Iterate on controllers in library_controllers 
                    foreach (var cont in controllers.controller)
                    {
                        var control = cont as controller;
                        //if (control == null)
                        //    continue;

                        var skin = control.Item as skin;

                        string[] boneNames = null;
                        Dictionary<string, double[]> sources = new Dictionary<string, double[]>();
                        Dictionary<string, string> semantic = new Dictionary<string, string>();

                        // Dump source[] for geom
                        foreach (var source in skin.source)
                        {
                            var float_array = source.Item as float_array;
                            if (float_array != null)
                            {
                                sources.Add(source.id, float_array.Values);
                            }
                            var name_array = source.Item as Name_array;
                            if (name_array != null)
                            {
                                boneNames = name_array._Text_.Split(' ');
                            }
                        }
                        {
                            var inputs = skin.joints.input;
                            foreach (var input in inputs)
                            {
                                if(input.semantic.Equals(""))
                                semantic.Add(input.semantic, input.source);
                            }
                        }
                        // Dump Items[] for geom
                        NUD.Mesh m = n.mesh[cid];
                        List<NUD.Vertex> v = m.polygons[0].vertices;
                        string[] vcount = skin.vertex_weights.vcount.Split(' ');
                        string[] vi = skin.vertex_weights.v.Split(' ');
                        int pos = 0;

                        for (int i = 0; i < (int)skin.vertex_weights.count; i++)
                        {
                            NUD.Vertex vert = v[i];

                            for (int j = 0; j < int.Parse(vcount[i]); j++)
                                foreach (var sem in skin.vertex_weights.input)
                                {
                                    switch (sem.semantic)
                                    {
                                        case "JOINT":
                                            // find joint name in vbn
                                            int ind = int.Parse(vi[pos++]);
                                            //Console.WriteLine(boneNames[ind].Substring(5));
                                            //Console.WriteLine(boneNames[ind].Substring(5));
                                            vert.node.Add(vbn.boneIndex(boneNames[ind].Substring(6)));
                                            break;
                                        case "WEIGHT":
                                            // find weight int weight list
                                            double[] weight;
                                            sources.TryGetValue(sem.source.Replace("#", ""), out weight);
                                            vert.weight.Add((float)weight[int.Parse(vi[pos++])]);
                                            break;
                                    }
                                }

                        }
                        cid++;
                    }
                }
            }

            n.PreRender();
            File.WriteAllBytes("C:\\s\\Smash\\extract\\data\\fighter\\murabito\\isa.nud",n.Rebuild());
        }

        private void openNud(string filename)
        {
            string[] files = Directory.GetFiles(System.IO.Path.GetDirectoryName(filename));

            string pnud = "";
            string pnut = "";
            string pjtb = "";
            string pvbn = "";
            string pmta = "";
            List<string> pacs = new List<string>();

            foreach (string s in files)
            {
                if (s.EndsWith(".nud"))
                    pnud = s;
                if (s.EndsWith(".nut"))
                    pnut = s;
                if (s.EndsWith(".vbn"))
                    pvbn = s;
                if (s.EndsWith(".jtb"))
                    pjtb = s;
                if (s.EndsWith(".mta"))
                    pmta = s;
                if (s.EndsWith(".pac"))
                    pacs.Add(s);
            }

            ModelContainer model = new ModelContainer();

            if (!pvbn.Equals(""))
            {
                model.vbn = new VBN(pvbn);
                Runtime.TargetVBN = model.vbn;
                if (!pjtb.Equals(""))
                    model.vbn.readJointTable(pjtb);

            }

            if (!pnut.Equals(""))
            {
                NUT nut = new NUT(new FileData(pnut));
                Runtime.TextureContainers.Add(nut);
            }

            if (!pnud.Equals(""))
            {
                model.nud = new NUD(pnud);

                //AddDockedControl(new NUDMaterialEditor(model.nud.mesh[0].polygons[0].materials));

                foreach (string s in pacs)
                {
                    PAC p = new PAC();
                    p.Read(s);
                    byte[] data;
                    p.Files.TryGetValue("default.mta", out data);
                    if (data != null) { 
                        MTA m = new MTA();
                        m.read(new FileData(data));
                        model.nud.applyMTA(m, 0);
                    }
                }
            }

            if (!pmta.Equals(""))
            {
                try
                {
                    model.mta = new MTA();
                    model.mta.Read(pmta);
                    viewports[0].loadMTA(model.mta);
                }
                catch (EndOfStreamException)
                {
                    model.mta = null;
                }
            }

            Runtime.ModelContainers.Add(model);
            meshList.refresh();
        }

        private void openVBNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Supported Formats(.vbn, .mdl0, .smd, .nud, .lvd)|*.vbn;*.mdl0;*.smd;*.lvd;*.nud;*.mtable|" +
                             "Smash 4 Boneset (.vbn)|*.vbn|" +
                             "Namco Model (.nud)|*.nud|" +
                             "Smash 4 Level Data (.lvd)|*.lvd|" +
                             "NW4R Model (.mdl0)|*.mdl0|" +
                             "Source Model (.SMD)|*.smd|" +
                             "All files(*.*)|*.*";

                // "Namco Universal Data Folder (.NUD)|*.nud|" +

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileName.EndsWith(".vbn"))
                        Runtime.TargetVBN = new VBN(ofd.FileName);

                    if (ofd.FileName.EndsWith(".lvd"))
                    {
                        Runtime.TargetLVD = new LVD(ofd.FileName);
                        lvdList.fillList();
                    }

                    if (ofd.FileName.EndsWith(".mtable"))
                    {
                        project.openACMD(ofd.FileName);
                    }

                    if (ofd.FileName.EndsWith("path.bin"))
                    {
                        Runtime.TargetPath = new PathBin(ofd.FileName);
                    }
                    else
                    if (ofd.FileName.EndsWith(".bin"))
                    {
                        //Note to whoever is readin this: 
                        //Eventually we need to look at the magic here (and also make all .bins look at magic)
                        //Runtime.TargetCMR0 = new CMR0();
                        //Runtime.TargetCMR0.read(new FileData(ofd.FileName));
                        PARAMEditor p = new PARAMEditor(ofd.FileName) { ShowHint = DockState.Document };
                        p.Text = Path.GetFileName(ofd.FileName);
                        AddDockedControl(p);
                        paramEditors.Add(p);
                    }

                    if (ofd.FileName.EndsWith(".mdl0"))
                    {
                        MDL0Bones mdl0 = new MDL0Bones();
                        Runtime.TargetVBN = mdl0.GetVBN(new FileData(ofd.FileName));
                    }

                    if (ofd.FileName.EndsWith(".smd"))
                    {
                        Runtime.TargetVBN = new VBN();
                        SMD.read(ofd.FileName, new SkelAnimation(), Runtime.TargetVBN);
                    }
                    //Viewport.Runtime.TargetVBN = Runtime.TargetVBN;


                    if (ofd.FileName.EndsWith(".dae"))
                    {
                        openDAE(ofd.FileName, Runtime.ModelContainers[0]);
                    }

                    /*if (ofd.FileName.EndsWith(".mbn"))
                    {
                        MBN m = new MBN();
                        m.Read(ofd.FileName);
                        ModelContainer con = new ModelContainer();
                        BCH b = new BCH();
                        b.mbn = m;
                        b.Read("C:\\s\\Smash\\extract\\data\\fighter\\lucas\\Ness3DS - h00\\normal.bch");
                        con.bch = b;
                        m.mesh.RemoveAt(m.mesh.Count - 1);
                        m.mesh.RemoveAt(m.mesh.Count - 2);
                        m.Save("C:\\s\\Smash\\extract\\data\\fighter\\lucas\\Ness3DS - h00\\rebuild.mbn");
                        Runtime.ModelContainers.Add(con);
                        //m.Save("C:\\s\\Smash\\extract\\data\\fighter\\lucas\\Ness3DS - h00\\test.mbn");
                        /*NUD n = m.toNUD();
                        n.PreRender();
                        n.Save("C:\\s\\Smash\\extract\\data\\fighter\\lucas\\Ness3DS - h00\\mbn.nud");
                    }*/

                    /*if (ofd.FileName.EndsWith(".bch"))
                    {
                        ModelContainer con = new ModelContainer();
                        BCH b = new BCH();
                        b.Read(ofd.FileName);
                        con.bch = b;
                        Runtime.ModelContainers.Add(con);
                    }*/

                    if (ofd.FileName.EndsWith(".nud"))
                    {
                        openNud(ofd.FileName);
                        //File.WriteAllBytes("C:\\s\\Smash\\extract\\data\\fighter\\lucas\\model_new.nud",Runtime.ModelContainers[0].nud.Rebuild());
                        /*PAC p = new PAC();
                        p.Read("C:\\s\\Smash\\extract\\data\\fighter\\lucas\\model\\body\\c00\\material_anime_face.pac");
                        byte[] data;
                        p.Files.TryGetValue ("display.mta", out data);
                        MTA m = new MTA();
                        m.read(new FileData(data));
                        Runtime.TargetNUD.applyMTA (m, 0);


                        p = new PAC();
                        p.Read("C:\\s\\Smash\\extract\\data\\fighter\\lucas\\model\\body\\c00\\material_anime_eyelid.pac");
                        p.Files.TryGetValue ("display.mta", out data);
                        m = new MTA();
                        m.read(new FileData(data));
                        Runtime.TargetNUD.applyMTA (m, 0);*/


                    }

                    if (Runtime.TargetVBN != null)
                    {
                        ModelContainer m = new ModelContainer();
                        m.name = new DirectoryInfo(ofd.FileName).Name;
                        m.vbn = Runtime.TargetVBN;
                        Runtime.ModelContainers.Add(m);

                        leftPanel.treeRefresh();
                        if (Runtime.TargetVBN.Endian == Endianness.Little)
                        {
                            radioButton2.Checked = true;
                        }
                        else
                        {
                            radioButton1.Checked = true;
                        }
                    }
                    else
                    {
                        foreach (ModelContainer m in Runtime.ModelContainers)
                        {
                            if (m.vbn != null)
                            {
                                Runtime.TargetVBN = Runtime.ModelContainers[0].vbn;
                                break;
                            }
                        }
                    }
                    project.fillTree();
                }
            }
        }

        private void addMaterialAnimation(string name,MTA m)
        {
            Runtime.MaterialAnimations.Add(name, m);
            MainForm.Instance.viewports[0].loadMTA(m);
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Supported Formats|*.omo;*.anim;*.chr0;*.smd;*.mta;*.pac|" +
                             "Object Motion|*.omo|" +
                             "Maya Animation|*.anim|" +
                             "NW4R Animation|*.chr0|" +
                             "Source Animation (SMD)|*.smd|" +
                             "Smash 4 Material Animation (MTA)|*.mta|"+
                             "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    //Runtime.Animations.Clear();
                    if (ofd.FileName.EndsWith(".mta"))
                    {
                        MTA mta = new MTA();
                        try
                        {
                            mta.Read(ofd.FileName);
                            Runtime.MaterialAnimations.Add(ofd.FileName, mta);
                            mtaNode.Nodes.Add(ofd.FileName);
                            MainForm.Instance.viewports[0].loadMTA(mta);
                            Runtime.TargetMTAString = ofd.FileName;
                        }
                        catch (EndOfStreamException)
                        {
                            mta = null;
                        }
                    }
                    else if (ofd.FileName.EndsWith(".smd"))
                    {
                        var anim = new SkelAnimation();
                        if (Runtime.TargetVBN == null)
                            Runtime.TargetVBN = new VBN();
                        SMD.read(ofd.FileName, anim, Runtime.TargetVBN);
                        leftPanel.treeRefresh();
                        Runtime.Animations.Add(ofd.FileName, anim);
                        animNode.Nodes.Add(ofd.FileName);
                    }
                    else if (ofd.FileName.EndsWith(".pac"))
                    {
                        PAC p = new PAC();
                        p.Read(ofd.FileName);

                        foreach (var pair in p.Files)
                        {
                            if (pair.Key.EndsWith(".omo"))
                            {
                                var anim = OMO.read(new FileData(pair.Value));
                                string AnimName = Regex.Match(pair.Key, @"([A-Z][0-9][0-9])(.*)").Groups[0].ToString();
                                //AnimName = pair.Key;
                                AnimName = AnimName.Remove(AnimName.Length - 4);
                                AnimName = AnimName.Insert(3, "_");
                                if (!string.IsNullOrEmpty(AnimName))
                                {
                                    animNode.Nodes.Add(AnimName);
                                    Runtime.Animations.Add(AnimName, anim);
                                }
                                else
                                {
                                    animNode.Nodes.Add(pair.Key);
                                    Runtime.Animations.Add(pair.Key, anim);
                                }
                            }
                            else if (pair.Key.EndsWith(".mta"))
                            {
                                MTA mta = new MTA();
                                try
                                {
                                    mta.read(new FileData(pair.Value));
                                    Runtime.MaterialAnimations.Add(pair.Key, mta);
                                    mtaNode.Nodes.Add(pair.Key);
                                }
                                catch (EndOfStreamException)
                                {
                                    mta = null;
                                }
                            }
                        }
                    }
                    //if (Runtime.TargetVBN.bones.Count > 0)
                    //{
                    if (ofd.FileName.EndsWith(".omo"))
                    {
                        Runtime.Animations.Add(ofd.FileName, OMO.read(new FileData(ofd.FileName)));
                        animNode.Nodes.Add(ofd.FileName);
                    }
                    if (ofd.FileName.EndsWith(".chr0"))
                    {
                        Runtime.Animations.Add(ofd.FileName, CHR0.read(new FileData(ofd.FileName), Runtime.TargetVBN));
                        animNode.Nodes.Add(ofd.FileName);
                    }
                    if (ofd.FileName.EndsWith(".anim"))
                    {
                        Runtime.Animations.Add(ofd.FileName, ANIM.read(ofd.FileName, Runtime.TargetVBN));
                        animNode.Nodes.Add(ofd.FileName);
                    }
                    //}
                }
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (Runtime.TargetVBN == null)
            {
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Supported Files (.omo, .anim, .pac)|*.omo;*.anim;*.pac|" +
                             "Object Motion (.omo)|*.omo|" +
                             "Maya Anim (.anim)|*.anim|" +
                             "OMO Pack (.pac)|*.pac|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    sfd.FileName = sfd.FileName;

                    if (sfd.FileName.EndsWith(".anim"))
                        ANIM.createANIM(sfd.FileName, Runtime.TargetAnim, Runtime.TargetVBN);

                    if (sfd.FileName.EndsWith(".omo"))
                        OMO.createOMO(Runtime.TargetAnim, Runtime.TargetVBN, sfd.FileName);

                    if (sfd.FileName.EndsWith(".pac"))
                    {
                        var pac = new PAC();
                        foreach (var anim in Runtime.Animations)
                        {
                            var bytes = OMO.createOMO(anim.Value, Runtime.TargetVBN);
                            pac.Files.Add(anim.Key, bytes);
                        }
                        pac.Save(sfd.FileName);
                    }
                }
            }
        }
        private void hashMatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            csvHashes csv = new csvHashes("hashTable.csv");
            foreach (ModelContainer m in Runtime.ModelContainers)
            {
                if (m.vbn != null)
                {
                    foreach (Bone bone in m.vbn.bones)
                    {
                        for (int i = 0; i < csv.names.Count; i++)
                        {
                            if (csv.names[i] == new string(bone.boneName))
                            {
                                bone.boneId = csv.ids[i];
                            }
                        }
                    }
                }
                if (m.bch != null)
                {
                    foreach (BCH.BCH_Model mod in m.bch.models)
                    {
                        foreach (Bone bone in mod.skeleton.bones)
                        {
                            for (int i = 0; i < csv.names.Count; i++)
                            {
                                if (csv.names[i] == new string(bone.boneName))
                                {
                                    bone.boneId = csv.ids[i];
                                }
                            }
                        }
                    }
                }
            }
            //leftPanel.treeRefresh();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var abt = new About())
            {
                abt.ShowDialog();
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            animNode.Nodes.Clear();
            mtaNode.Nodes.Clear();
            Runtime.Animations.Clear();
            Runtime.MaterialAnimations.Clear();
            Runtime.TargetVBN.reset();
        }

        private void importToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog() { Filter = "Motion Table (.mtable)|*.mtable|All Files (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Runtime.Moveset = new MovesetManager(ofd.FileName);
                }
            }
        }
        private void animationPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rightPanel.Show(dockPanel1);
        }
        private void viewportWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //var vp = new VBNViewport();
            //viewports.Add(vp);
            //AddDockedControl(vp);
        }

        private void animationsWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (animationsWindowToolStripMenuItem.Checked)
                rightPanel.Show(dockPanel1);
            else
                rightPanel.Hide();
        }

        private void boneTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (boneTreeToolStripMenuItem.Checked)
                leftPanel.Show(dockPanel1);
            else
                leftPanel.Hide();
        }
        #endregion

        private void addBoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newForm = new AddBone(this);
            newForm.ShowDialog();
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.TargetVBN.Endian = Endianness.Big;
            Runtime.TargetVBN.unk_1 = 1;
            Runtime.TargetVBN.unk_2 = 2;
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.TargetVBN.Endian = Endianness.Little;
            Runtime.TargetVBN.unk_1 = 2;
            Runtime.TargetVBN.unk_2 = 1;
        }

        public void openMats(NUD.Polygon poly, string name)
        {
            AddDockedControl(new NUDMaterialEditor(poly.materials) { ShowHint = DockState.Float, Text = name});
            
        }

        private void openStageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new FolderSelectDialog())
            {
                ofd.Title = "Stage Folder";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string stagePath = ofd.SelectedPath;
                    string modelPath = stagePath + "\\model\\";
                    string paramPath = stagePath + "\\param\\";
                    string animationPath = stagePath + "\\animation\\";
                    List<string> nuds = new List<string>();

                    if (Directory.Exists(modelPath))
                    {
                        foreach (string d in Directory.GetDirectories(modelPath))
                        {
                            foreach (string f in Directory.GetFiles(d))
                            {
                                if (f.EndsWith(".nud"))
                                {
                                    //Console.WriteLine(f);
                                    openNud(f);
                                }
                            }
                        }
                    }

                    if (Directory.Exists(paramPath))
                    {
                        Runtime.TargetLVD = null;
                        foreach (string f in Directory.GetFiles(paramPath))
                        {
                            if (f.EndsWith(".lvd") && Runtime.TargetLVD != null)
                            {
                                Runtime.TargetLVD = new LVD(f);
                                lvdList.fillList();
                            }
                        }
                    }

                    if (Directory.Exists(animationPath))
                    {
                        foreach (string d in Directory.GetDirectories(animationPath))
                        {
                            foreach (string f in Directory.GetFiles(d))
                            {
                                if (f.EndsWith(".omo"))
                                {
                                    Runtime.Animations.Add(f, OMO.read(new FileData(f)));
                                    animNode.Nodes.Add(f);
                                }
                                else if (f.EndsWith("path.bin"))
                                {
                                    Runtime.TargetPath = new PathBin();
                                    Runtime.TargetPath.Read(f);
                                }
                            }
                        }
                    }
                }
            }
        }
        private void clearWorkspaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Runtime.killWorkspace = true;
        }
        private void renderSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            using (var rndr = new GUI.RenderSettings())
            {
                rndr.ShowDialog();
            }
        }
        private void meshListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            meshList.refresh();
            AddDockedControl(meshList);
        }
        private void projectTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (project.DockState == DockState.Unknown)
                project = new ProjectTree();
            else
                project.Focus();
        }

        private void openCharacterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new FolderSelectDialog())
            {
                ofd.Title = "Character Folder";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    project.openACMD($"{ofd.SelectedPath}\\script\\animcmd\\body\\motion.mtable", $"{ofd.SelectedPath}\\motion");
                }
            }
        }
    }
}
