using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Windows.Forms;

namespace Smash_Forge
{
    public class NUD : FileBase
    {
        public NUD()
        {
            GL.GenBuffers(1, out vbo_position);
            GL.GenBuffers(1, out ibo_elements);
            GL.GenBuffers(1, out ubo_bones);
        }
        public NUD(string fname) : this()
        {
            Read(fname);
            PreRender();
        }

        // gl buffer objects
        int vbo_position;
        int ibo_elements;
        int ubo_bones;

        int[] facedata;
        dVertex[] vertdata;

        public const int SMASH = 0;
        public const int POKKEN = 1;
        public int type = SMASH;
        public int boneCount = 0;
        public bool hasBones = false;
        public List<Mesh> mesh = new List<Mesh>();
        public float[] param = new float[4];

        public override Endianness Endian { get; set; }

        #region Rendering
        public void Destroy()
        {
            GL.DeleteBuffer(vbo_position);
            GL.DeleteBuffer(ibo_elements);
            GL.DeleteBuffer(ubo_bones);

            vertdata = null;
            facedata = null;
            mesh.Clear();
        }

        public enum TextureFlags
        {
            Glow = 0x00000080,
            Shadow = 0x00000040,
            RIM = 0x00000020,
            UnknownTex = 0x00000010,
            AOMap = 0x00000008,
            CubeMap = 0x00000004,
            NormalMap = 0x00000002,
            DiffuseMap = 0x00000001
        }

        /*
         * Not sure if update is needed here
        */
        public void PreRender()
        {
            List<dVertex> vert = new List<dVertex>();

            for (int mes = mesh.Count - 1; mes >= 0; mes--)
            {
                Mesh m = mesh[mes];
                for (int pol = m.polygons.Count - 1; pol >= 0; pol--)
                {
                    Polygon p = m.polygons[pol];
                    if (p.faces.Count <= 3)
                        continue;
                    foreach (Vertex v in p.vertices)
                    {
                        dVertex nv = new dVertex()
                        {
                            pos = v.pos,
                            nrm = v.nrm,
                            col = Endian == Endianness.Little ? new Vector4(1f, 1f, 1f, 1f) : v.col / 0x7F,
                            tx0 = v.tx.Count > 0 ? v.tx[0] : new Vector2(0, 0),
                            node = new Vector4(v.node.Count > 0 ? v.node[0] : -1,
                            v.node.Count > 1 ? v.node[1] : -1, 
                            v.node.Count > 2 ? v.node[2] : -1, 
                            v.node.Count > 3 ? v.node[3] : -1),
                            weight = new Vector4(v.weight.Count > 0 ? v.weight[0] : 0,
                            v.weight.Count > 1 ? v.weight[1] : 0,
                            v.weight.Count > 2 ? v.weight[2] : 0,
                            v.weight.Count > 3 ? v.weight[3] : 0),

                        };

                        p.isTransparent = false;
                        if (v.col.Z < 0x7F)
                            p.isTransparent = true;
                        if(p.materials[0].srcFactor > 1)
                            p.isTransparent = true;

                        vert.Add(nv);
                    }
                    p.vertdata = vert.ToArray();
                    vert = new List<dVertex>();

                    // rearrange faces
                    p.display = p.getDisplayFace().ToArray();
                }
            }
        }

        public void Render(Matrix4 view, VBN vbn)
        {
            // Bounding Box Render
            if (Runtime.renderBoundingBox)
            {
                GL.UseProgram(0);
                RenderTools.drawCubeWireframe(new Vector3(param[0], param[1], param[2]), param[3]);
                foreach (NUD.Mesh mesh in mesh)
                {
                    if (mesh.Checked)
                        RenderTools.drawCubeWireframe(new Vector3(mesh.bbox[0], mesh.bbox[1], mesh.bbox[2]), mesh.bbox[3]);
                }
            }

            Shader shader = Runtime.shaders["NUD"];

            GL.UseProgram(shader.programID);
            int rt = (int)Runtime.renderType;
            if (rt == 0)
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

            {

                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.TextureCubeMap, RenderTools.cubeTex);
                GL.Uniform1(shader.getAttribute("cmap"), 2);
                GL.UniformMatrix4(shader.getAttribute("eyeview"), false, ref view);

                if (vbn != null)
                {
                    Matrix4[] f = vbn.getShaderMatrix();

                    int maxUniformBlockSize = GL.GetInteger(GetPName.MaxUniformBlockSize);
                    int boneCount = vbn.bones.Count;
                    int dataSize = boneCount * Vector4.SizeInBytes * 4;

                    GL.BindBuffer(BufferTarget.UniformBuffer, ubo_bones);
                    GL.BufferData(BufferTarget.UniformBuffer, (IntPtr)(dataSize), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    GL.BindBuffer(BufferTarget.UniformBuffer, 0);

                    var blockIndex = GL.GetUniformBlockIndex(shader.programID, "bones");
                    GL.BindBufferBase(BufferRangeTarget.UniformBuffer, blockIndex, ubo_bones);

                    if (f.Length > 0)
                    {
                        GL.BindBuffer(BufferTarget.UniformBuffer, ubo_bones);
                        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, (IntPtr)(f.Length * Vector4.SizeInBytes * 4), f);
                    }
                }

                shader.enableAttrib();
                Render(shader);
                shader.disableAttrib();
            }
        }

        public void Render(Shader shader)
        {
            // create lists...
            // first draw opaque
            // then sort transparent by distance to camera
            // 

            List<Polygon> opaque = new List<Polygon>();
            List<Polygon> trans = new List<Polygon>();

            /*SortedList<float, Mesh> transorder = new SortedList<float, Mesh>();
            foreach(Mesh m in mesh)
            {
                // get nearest box point
                float pos = Vector4.Transform(new Vector4(m.bbox[0], m.bbox[1], m.bbox[2], 1.0f), view).Z;
                if (!transorder.ContainsKey((pos+m.bbox[3])*-1))
                    transorder.Add((pos + m.bbox[3])*-1, m);
            }*/

            foreach (Mesh m in mesh)
            {
                for (int pol = m.polygons.Count - 1; pol >= 0; pol--)
                {
                    Polygon p = m.polygons[m.polygons.Count - 1 - pol];

                    if (p.isTransparent)
                    {
                        trans.Add(p);
                        continue;
                    }
                    int hash = p.materials[0].textures[0].hash;
                    NUT.NUD_Texture tex = null;
                    foreach (NUT nut in Runtime.TextureContainers)
                    {
                        if(nut.getTextureByID(hash, out tex))
                        {
                            if (tex.getNutFormat() == 2)
                                trans.Add(p);
                            else
                                opaque.Add(p);
                            break;
                        }
                    }
                    if (tex == null)
                        opaque.Add(p);
                }
            }
            
            //GL.Enable(EnableCap.PrimitiveRestartFixedIndex);

            foreach (Polygon p in opaque)
                if (((Mesh)p.Parent).Checked)
                    DrawPolygon(p, shader);

            foreach (Polygon p in trans)
                if(((Mesh)p.Parent).Checked)
                    DrawPolygon(p, shader);
        }

        private void DrawPolygon(Polygon p, Shader shader)
        {
            if (p.faces.Count <= 3)
                return;

            //foreach (Material mat in p.materials)
            {
                Material mat = p.materials[0];
                GL.Uniform1(shader.getAttribute("flags"), mat.flags);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, VBNViewport.defaulttex);
                GL.Uniform1(shader.getAttribute("tex"), 0);

                GL.ActiveTexture(TextureUnit.Texture0 + 1);
                GL.BindTexture(TextureTarget.Texture2D, VBNViewport.defaulttex);
                GL.Uniform1(shader.getAttribute("nrm"), 1);

                GL.ActiveTexture(TextureUnit.Texture0 + 2);
                GL.BindTexture(TextureTarget.Texture2D, VBNViewport.defaulttex);
                GL.Uniform1(shader.getAttribute("rim"), 2);

                GL.ActiveTexture(TextureUnit.Texture0 + 3);
                GL.BindTexture(TextureTarget.Texture2D, VBNViewport.defaulttex);
                GL.Uniform1(shader.getAttribute("cube"), 3);

                int[] locs = new int[4] { 3, 3, 3, 3 };
                int li = 0;
                if ((mat.flags & (uint)TextureFlags.DiffuseMap) > 0)
                    locs[li++] = 0;

                if ((mat.flags & (uint)TextureFlags.RIM) > 0)
                {
                    // as a stage map...
                    if ((mat.flags & (uint)TextureFlags.AOMap) > 0)
                        locs[li++] = 2;
                }
                else
                    if ((mat.flags & (uint)TextureFlags.CubeMap) > 0)
                    locs[li++] = 3;

                // TODO: This is a specular shine map thing, I'm putting it as rim for now
                if ((mat.flags & (uint)NUD.TextureFlags.UnknownTex) > 0)
                    locs[li++] = 2;

                if ((mat.flags & (uint)TextureFlags.NormalMap) > 0)
                    locs[li++] = 1;
                else if ((mat.flags & (uint)TextureFlags.RIM) > 0)
                {
                    locs[li++] = 1; // second diffuse
                }

                if ((mat.flags & (uint)TextureFlags.RIM) > 0)
                {
                    // as rim lighting
                    if ((mat.flags & (uint)TextureFlags.CubeMap) > 0)
                        locs[li++] = 3;
                }
                else
                    if ((mat.flags & (uint)TextureFlags.AOMap) > 0)
                    locs[li++] = 2;

                int texid;
                bool success;
                for (int i = 0; i < mat.textures.Count; i++)
                {
                    if (i > locs.Length) break;
                    foreach (NUT nut in Runtime.TextureContainers)
                    {
                        success = nut.draw.TryGetValue(mat.textures[i].hash, out texid);
                        if (success)
                        {
                            GL.ActiveTexture(TextureUnit.Texture0 + locs[i]);
                            GL.BindTexture(TextureTarget.Texture2D, texid);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapmode[mat.textures[i].WrapMode1]);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapmode[mat.textures[i].WrapMode2]);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)wrapmode[mat.textures[i].minFilter]);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)wrapmode[mat.textures[i].magFilter]);
                            //GL.BindTexture(TextureTarget.Texture2D, 0);
                            break;
                        }
                    }
                }


                float[] ao;
                mat.entries.TryGetValue("NU_aoMinGain", out ao);
                if (ao == null) ao = new float[] { 0, 0, 0, 0 };
                Vector4 aoo = new Vector4(ao[0], ao[1], ao[2], ao[3]);
                GL.Uniform4(shader.getAttribute("minGain"), aoo);

                {
                    float[] pa;
                    mat.entries.TryGetValue("NU_colorSamplerUV", out pa);
                    if (pa == null) pa = new float[] { 1, 1, 0, 0 };
                    GL.Uniform4(shader.getAttribute("colorSamplerUV"), pa[0], pa[1], pa[2], pa[3]);
                }
                {
                    float[] pa;
                    mat.entries.TryGetValue("NU_colorGain", out pa);
                    if (pa == null) pa = new float[] { 1, 1, 1, 1 };
                    GL.Uniform4(shader.getAttribute("colorGain"), pa[0], pa[1], pa[2], pa[3]);
                }
                {
                    float[] pa;
                    mat.entries.TryGetValue("NU_colorOffset", out pa);
                    if (pa == null) pa = new float[] { 0, 0, 0, 0 };
                    GL.Uniform4(shader.getAttribute("colorOffset"), pa[0], pa[1], pa[2], pa[3]);
                }
                {
                    float[] pa;
                    mat.entries.TryGetValue("NU_diffuseColor", out pa);
                    if (pa == null) pa = new float[] { 1, 1, 1, 1 };
                    GL.Uniform4(shader.getAttribute("diffuseColor"), pa[0], pa[1], pa[2], pa[3]);
                }
                {
                    float[] pa;
                    mat.entries.TryGetValue("NU_specularColor", out pa);
                    if (pa == null) pa = new float[] { 0, 0, 0, 0 };
                    GL.Uniform4(shader.getAttribute("specularColor"), pa[0], pa[1], pa[2], pa[3]);
                }
                {
                    float[] pa;
                    mat.entries.TryGetValue("NU_specularColorGain", out pa);
                    if (pa == null) pa = new float[] { 1, 1, 1, 1 };
                    GL.Uniform4(shader.getAttribute("specularColorGain"), pa[0], pa[1], pa[2], pa[3]);
                }
                {
                    float[] pa;
                    mat.entries.TryGetValue("NU_specularParams", out pa);
                    if (pa == null) pa = new float[] { 0, 0, 0, 0 };
                    GL.Uniform4(shader.getAttribute("specularParams"), pa[0], pa[1], pa[2], pa[3]);
                }
                {
                    float[] pa;
                    mat.entries.TryGetValue("NU_fresnelColor", out pa);
                    if (pa == null) pa = new float[] { 0, 0, 0, 0 };
                    GL.Uniform4(shader.getAttribute("fresnelColor"), pa[0], pa[1], pa[2], pa[3]);
                }
                {
                    float[] pa;
                    mat.entries.TryGetValue("NU_fresnelParams", out pa);
                    if (pa == null) pa = new float[] { 0, 0, 0, 0 };
                    GL.Uniform4(shader.getAttribute("fresnelParams"), pa[0], pa[1], pa[2], pa[3]);
                }
                {
                    float[] pa;
                    mat.entries.TryGetValue("NU_reflectionColor", out pa);
                    if (pa == null) pa = new float[] { 0, 0, 0, 1 };
                    GL.Uniform4(shader.getAttribute("reflectionColor"), pa[0], pa[1], pa[2], pa[3]);
                }
                {
                    float[] pa;
                    mat.entries.TryGetValue("NU_reflectionParams", out pa);
                    if (pa == null) pa = new float[] { 0, 0, 0, 1 };
                    GL.Uniform4(shader.getAttribute("reflectionParams"), pa[0], pa[1], pa[2], pa[3]);
                }

                GL.Enable(EnableCap.Blend);
                if (mat.srcFactor == 5)
                {
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                }

                GL.BlendFunc(srcFactor.Keys.Contains(mat.srcFactor) ? srcFactor[mat.srcFactor] : BlendingFactorSrc.SrcAlpha, 
                    dstFactor.Keys.Contains(mat.dstFactor) ? dstFactor[mat.dstFactor] : BlendingFactorDest.OneMinusSrcAlpha);

                /*GL.Enable(EnableCap.AlphaTest);

                GL.AlphaFunc(AlphaFunction.Gequal, 0.1f);
                switch (mat.alphaFunc){
                    case 0:
                        GL.AlphaFunc(AlphaFunction.Gequal, 128 / 255f);
                        break;
                }
                /*switch (mat.ref1)
                {
                    case 4:
                        GL.AlphaFunc(AlphaFunction.Lequal, 128 / 255f);
                        break;
                    case 6:
                        GL.AlphaFunc(AlphaFunction.Lequal, 255 / 255f);
                        break;
                }*/

                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Front);
                switch (mat.cullMode)
                {
                    case 0:
                        GL.Disable(EnableCap.CullFace);
                        break;
                    case 2:
                        GL.CullFace(CullFaceMode.Front);
                        break;
                    case 4:
                        GL.CullFace(CullFaceMode.Back);
                        break;
                }

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
                GL.BufferData<dVertex>(BufferTarget.ArrayBuffer, (IntPtr)(p.vertdata.Length * dVertex.Size), p.vertdata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shader.getAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, dVertex.Size, 0);
                GL.VertexAttribPointer(shader.getAttribute("vNormal"), 3, VertexAttribPointerType.Float, false, dVertex.Size, 12);
                GL.VertexAttribPointer(shader.getAttribute("vUV"), 2, VertexAttribPointerType.Float, false, dVertex.Size, 24);
                GL.VertexAttribPointer(shader.getAttribute("vColor"), 4, VertexAttribPointerType.Float, false, dVertex.Size, 32);
                GL.VertexAttribPointer(shader.getAttribute("vBone"), 4, VertexAttribPointerType.Float, false, dVertex.Size, 48);
                GL.VertexAttribPointer(shader.getAttribute("vWeight"), 4, VertexAttribPointerType.Float, false, dVertex.Size, 64);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(p.display.Length * sizeof(int)), p.display, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                if (p.Checked)
                {
                    //(p.strip >> 4) == 4 ? PrimitiveType.Triangles : PrimitiveType.TriangleStrip
                    GL.DrawElements(PrimitiveType.Triangles, p.displayFaceSize, DrawElementsType.UnsignedInt, 0);
                }
            }
        }

        Dictionary<int, BlendingFactorDest> dstFactor = new Dictionary<int, BlendingFactorDest>(){
                    { 0x01, BlendingFactorDest.OneMinusSrcAlpha},
                    { 0x02, BlendingFactorDest.One},
                    { 0x03, BlendingFactorDest.OneMinusSrcAlpha},
                    { 0x04, BlendingFactorDest.OneMinusConstantAlpha},
                    { 0x05, BlendingFactorDest.ConstantAlpha},
                    { 0x07, BlendingFactorDest.DstAlpha}
                };

        Dictionary<int, BlendingFactorSrc> srcFactor = new Dictionary<int, BlendingFactorSrc>(){
                    { 0x01, BlendingFactorSrc.SrcAlpha},
                    { 0x02, BlendingFactorSrc.SrcAlpha},
                    { 0x03, BlendingFactorSrc.DstAlpha},
                    { 0x04, BlendingFactorSrc.SrcAlpha},
                    { 0x0a, BlendingFactorSrc.Zero}
                };

        Dictionary<int, TextureWrapMode> wrapmode = new Dictionary<int, TextureWrapMode>(){
                    { 0x01, TextureWrapMode.Repeat},
                    { 0x02, TextureWrapMode.MirroredRepeat},
                    { 0x03, TextureWrapMode.Clamp}
        };
        #endregion

        #region MTA
        public void clearMTA()
        {
            foreach (Mesh me in mesh)
            {
                foreach (Polygon p in me.polygons)
                {
                    foreach (Material ma in p.materials)
                    {
                        ma.anims.Clear();
                    }
                }
            }
        }

        public void applyMTA(MTA m, int frame)
        {
            foreach (MatEntry mat in m.matEntries)
            {
                foreach (Mesh me in mesh)
                {
                    foreach(Polygon p in me.polygons)
                    {
                        foreach (Material ma in p.materials)
                        {
                            float[] matHashFloat;
                            ma.entries.TryGetValue("NU_materialHash", out matHashFloat);
                            if (matHashFloat != null) {

                                byte[] bytes = new byte[4];
                                Buffer.BlockCopy(matHashFloat, 0, bytes, 0, 4);
                                int matHash = BitConverter.ToInt32(bytes, 0);

                                if (matHash == mat.matHash || matHash == mat.matHash2)
                                {
                                    //Console.WriteLine("MTA mat hash match");
                                    if (mat.hasPat)
                                    {
                                        ma.displayTexId = mat.pat0.getTexId((int)((frame * 60 / m.frameRate) % m.numFrames));
                                        //Console.WriteLine("PAT0 TexID - " + ma.displayTexId);
                                    }

                                    foreach(MatData md in mat.properties)
                                    {
                                        //Console.WriteLine("Frame - "+frame+" "+md.name);
                                        
                                        if (md.frames.Count > 0)
                                        {
                                            if (ma.anims.ContainsKey(md.name))
                                                ma.anims[md.name] = md.frames[(int)((frame * 60 / m.frameRate) % (m.numFrames))].values;
                                            else
                                                if(md.frames.Count > (int)((frame * 60 / m.frameRate) % (m.numFrames)))
                                                    ma.anims.Add(md.name, md.frames[(int)((frame * 60 / m.frameRate) % (m.numFrames))].values);
                                            //Console.WriteLine(""+md.frames[frame % md.frames.Count].values[0]+"," + md.frames[frame % md.frames.Count].values[1] + "," + md.frames[frame % md.frames.Count].values[2] + "," + md.frames[frame % md.frames.Count].values[3]);
                                        }
                                            
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (VisEntry e in m.visEntries)
            {
                int state = e.getState(frame);
                foreach (Mesh me in mesh)
                {
                    if (me.Text.Equals(e.name))
                    {
                        //Console.WriteLine("Set " + me.name + " to " + state);
                        if (state == 0)
                        {
                            me.Checked = false;
                        }
                        else
                        {
                            me.Checked = true;
                        }
                        break;
                    }
                }
            }

        }
        #endregion

        #region Reading
        //------------------------------------------------------------------------------------------------------------------------
        /*
         * Reads the contents of the nud file into this class
         */
        //------------------------------------------------------------------------------------------------------------------------
        // HELPERS FOR READING
        private struct _s_Object
        {
            public int id;
            //public int polynamestart;
            public int singlebind;
            public int polyamt;
            public int positionb;
            public string name;
        }

        public struct _s_Poly
        {
            public int polyStart;
            public int vertStart;
            public int verAddStart;
            public int vertamt;
            public int vertSize;
            public int UVSize;
            public int polyamt;
            public int polsize;
            public int polflag;
            public int texprop1;
            public int texprop2;
            public int texprop3;
            public int texprop4;
        }

        public override void Read(string filename)
        {
            FileData d = new FileData(filename);
            d.Endian = Endianness.Big;
            d.seek(0);

            // read header
            string magic = d.readString(0, 4);

            if (magic.Equals("NDWD"))
                d.Endian = Endianness.Little;

            Endian = d.Endian;

            d.seek(0xA);
            int polysets = d.readShort();
            boneCount = d.readShort();
            d.skip(2);  // somethingsets
            int polyClumpStart = d.readInt() + 0x30;
            int polyClumpSize = d.readInt();
            int vertClumpStart = polyClumpStart + polyClumpSize;
            int vertClumpSize = d.readInt();
            int vertaddClumpStart = vertClumpStart + vertClumpSize;
            int vertaddClumpSize = d.readInt();
            int nameStart = vertaddClumpStart + vertaddClumpSize;
            param[0] = d.readFloat();
            param[1] = d.readFloat();
            param[2] = d.readFloat();
            param[3] = d.readFloat();

            // object descriptors

            _s_Object[] obj = new _s_Object[polysets];
            List<float[]> unknown = new List<float[]>();
            int[] boneflags = new int[polysets];
            for (int i = 0; i < polysets; i++)
            {
                float[] un = new float[8];
                un[0] = d.readFloat();
                un[1] = d.readFloat();
                un[2] = d.readFloat();
                un[3] = d.readFloat();
                un[4] = d.readFloat();
                un[5] = d.readFloat();
                un[6] = d.readFloat();
                un[7] = d.readFloat();
                unknown.Add(un);
                int temp = d.pos() + 4;
                d.seek(nameStart + d.readInt());
                obj[i].name = (d.readString());
                // read name string
                d.seek(temp);
                boneflags[i] = d.readInt();
                obj[i].singlebind = d.readShort();
                obj[i].polyamt = d.readShort();
                obj[i].positionb = d.readInt();
            }

            // reading polygon data
            int mi = 0;
            foreach (var o in obj)
            {
                Mesh m = new Mesh();
                //Console.WriteLine($"{o.name} singlebind: {o.singlebind}");
                m.Text = o.name;
                mesh.Add(m);
                m.boneflag = boneflags[mi];
                m.singlebind = (short)o.singlebind;
                m.bbox = unknown[mi++];

                for (int i = 0; i < o.polyamt; i++)
                {
                    _s_Poly p = new _s_Poly();

                    p.polyStart = d.readInt() + polyClumpStart;
                    p.vertStart = d.readInt() + vertClumpStart;
                    p.verAddStart = d.readInt() + vertaddClumpStart;
                    p.vertamt = d.readShort();
                    p.vertSize = d.readByte();
                    p.UVSize = d.readByte();
                    p.texprop1 = d.readInt();
                    p.texprop2 = d.readInt();
                    p.texprop3 = d.readInt();
                    p.texprop4 = d.readInt();
                    p.polyamt = d.readShort();
                    p.polsize = d.readByte();
                    p.polflag = d.readByte();
                    d.skip(0xC);

                    int temp = d.pos();

                    // read vertex
                    Polygon pol = readVertex(d, p, o);
                    m.polygons.Add(pol);

                    pol.materials = readMaterial(d, p, nameStart);

                    d.seek(temp);
                }
            }
        }

        //VERTEX TYPES----------------------------------------------------------------------------------------

        public static List<Material> readMaterial(FileData d, _s_Poly p, int nameOffset)
        {
            int propoff = p.texprop1;
            List<Material> mats = new List<Material>();

            while (propoff != 0)
            {
                d.seek(propoff);

                Material m = new Material();
                mats.Add(m);

                m.flags = (uint)d.readInt();
                d.skip(4);

                
                m.srcFactor = d.readShort();
                int propCount = d.readShort();
                m.dstFactor = d.readShort();
                m.ref1 = d.readByte();
                m.ref0 = d.readByte();

                d.skip(1); // unknown
                m.drawPriority = d.readByte();
                m.cullMode = d.readByte();
                m.unknown1 = d.readByte();
                d.skip(4); // padding
                m.unkownWater = d.readInt();
                m.zBufferOffset = d.readInt();

                for (int i = 0; i < propCount; i++)
                {
                    Mat_Texture tex = new Mat_Texture();
                    tex.hash = d.readInt();
                    d.skip(6); // padding?
                    tex.MapMode = d.readShort();
                    tex.WrapMode1 = d.readByte();
                    tex.WrapMode2 = d.readByte();
                    tex.minFilter = d.readByte();
                    tex.magFilter = d.readByte();
                    tex.mipDetail = d.readByte();
                    tex.unknown = d.readByte();
                    d.skip(6);
                    m.textures.Add(tex);
                }

                int head = 0x20;

                if(d.Endian != Endianness.Little)
                while (head != 0)
                {
                    head = d.readInt();
                    int nameStart = d.readInt();

                    string name = d.readString(nameOffset + nameStart, -1);

                    int pos = d.pos();
                    int c = d.readInt();
                    d.skip(4);
                    float[] f = new float[c];
                    for (int i = 0; i < c; i++)
                    {
                        f[i] = d.readFloat();
                    }

                    m.entries.Add(name, f);

                    d.seek(pos);

                    if (head == 0)
                        d.skip(0x20 - 8);
                    else
                        d.skip(head - 8);
                }

                if (propoff == p.texprop1)
                    propoff = p.texprop2;
                else
                    if (propoff == p.texprop2)
                        propoff = p.texprop3;
                    else
                        if (propoff == p.texprop3)
                            propoff = p.texprop4;
            }

            return mats;
        }

        private static Polygon readVertex(FileData d, _s_Poly p, _s_Object o)
        {
            Polygon m = new Polygon();
            m.vertSize = p.vertSize;
            m.UVSize = p.UVSize;
            m.polflag = p.polflag;
            m.strip = p.polsize;

            readVertex(d, p, o, m);

            // faces
            d.seek(p.polyStart);

            for (int x = 0; x < p.polyamt; x++)
            {
                m.faces.Add(d.readShort());
            }

            return m;
        }

        //VERTEX TYPES----------------------------------------------------------------------------------------
        private static void readUV(FileData d, _s_Poly p, _s_Object o, Polygon m, Vertex[] v)
        {
            int uvCount = (p.UVSize >> 4);
            int uvType = (p.UVSize) & 0xF;

            for (int i = 0; i < p.vertamt; i++)
            {
                v[i] = new Vertex();
                if (uvType == 0x0)
                {
                    for (int j = 0; j < uvCount; j++)
                        v[i].tx.Add(new Vector2(d.readHalfFloat(), d.readHalfFloat()));
                }
                else
                    if (uvType == 0x2)
                    {
                        v[i].col = new Vector4(d.readByte(), d.readByte(), d.readByte(), d.readByte());
                        for (int j = 0; j < uvCount; j++)
                            v[i].tx.Add(new Vector2(d.readHalfFloat(), d.readHalfFloat()));
                }
                else
                    if (uvType == 0x4)
                {
                    v[i].col = new Vector4(d.readHalfFloat() * 0xFF, d.readHalfFloat() * 0xFF, d.readHalfFloat() * 0xFF, d.readHalfFloat() * 0xFF);
                    for (int j = 0; j < uvCount; j++)
                        v[i].tx.Add(new Vector2(d.readHalfFloat(), d.readHalfFloat()));
                }
                else
                        throw new NotImplementedException("UV type not supported " + uvType);
            }
        }

        private static void readVertex(FileData d, _s_Poly p, _s_Object o, Polygon m)
        {
            int weight = p.vertSize >> 4;
            int nrm = p.vertSize & 0xF;

            Vertex[] v = new Vertex[p.vertamt];

            d.seek(p.vertStart);

            if (weight > 0)
            {
                readUV(d, p, o, m, v);
                d.seek(p.verAddStart);
            }
            else
            {
                for (int i = 0; i < p.vertamt; i++)
                {
                    v[i] = new Vertex();
                }
            }

            Debug.WriteLine(p.UVSize.ToString("x") + " " + p.vertSize.ToString("x") + " " + d.pos().ToString("x"));

            for (int i = 0; i < p.vertamt; i++)
            {
                if (nrm != 8)
                {
                    v[i].pos.X = d.readFloat();
                    v[i].pos.Y = d.readFloat();
                    v[i].pos.Z = d.readFloat();
                }

                if (nrm == 1)
                {
                    v[i].nrm.X = d.readFloat();
                    v[i].nrm.Y = d.readFloat();
                    v[i].nrm.Z = d.readFloat();
                    d.skip(4); // n1?
                    d.skip(4); // r1?
                } else if (nrm == 2)
                {
                    v[i].nrm.X = d.readFloat();
                    v[i].nrm.Y = d.readFloat();
                    v[i].nrm.Z = d.readFloat();
                    d.skip(4); // n1?
                    d.skip(12); // r1?
                    d.skip(12); // r1?
                    d.skip(12); // r1?
                } else if (nrm == 3)
                {
                    d.skip(4); 
                    v[i].nrm.X = d.readFloat();
                    v[i].nrm.Y = d.readFloat();
                    v[i].nrm.Z = d.readFloat();
                    d.skip(4); 
                    d.skip(32); 
                }
                else if (nrm == 6)
                {
                    v[i].nrm.X = d.readHalfFloat();
                    v[i].nrm.Y = d.readHalfFloat();
                    v[i].nrm.Z = d.readHalfFloat();
                    d.skip(2); // n1?
                } else if (nrm == 7)
                {
                    v[i].nrm.X = d.readHalfFloat();
                    v[i].nrm.Y = d.readHalfFloat();
                    v[i].nrm.Z = d.readHalfFloat();
                    d.skip(2); // n1?
                    v[i].bitan.X = d.readHalfFloat();
                    v[i].bitan.Y = d.readHalfFloat();
                    v[i].bitan.Z = d.readHalfFloat();
                    v[i].bitan.W = d.readHalfFloat();
                    v[i].tan.X = d.readHalfFloat();
                    v[i].tan.Y = d.readHalfFloat();
                    v[i].tan.Z = d.readHalfFloat();
                    v[i].tan.W = d.readHalfFloat();
                } else
                    d.skip(4);

                if (weight == 0)
                {
                    if (p.UVSize >= 18)
                    {
                        v[i].col.X = (int)d.readByte();
                        v[i].col.Y = (int)d.readByte();
                        v[i].col.Z = (int)d.readByte();
                        v[i].col.W = (int)d.readByte();
                        //v.a = (int) (d.readByte());
                    }

                    for (int j = 0; j < (p.UVSize >> 4); j++)
                        v[i].tx.Add(new Vector2(d.readHalfFloat(), d.readHalfFloat()));

                    // UV layers
                    //d.skip(4 * ((p.UVSize >> 4) - 1));
                }

                if (weight == 1)
                {
                    v[i].node.Add(d.readInt());
                    v[i].node.Add(d.readInt());
                    v[i].node.Add(d.readInt());
                    v[i].node.Add(d.readInt());
                    v[i].weight.Add(d.readFloat());
                    v[i].weight.Add(d.readFloat());
                    v[i].weight.Add(d.readFloat());
                    v[i].weight.Add(d.readFloat());
                }
                else if (weight == 2)
                {
                    v[i].node.Add(d.readShort());
                    v[i].node.Add(d.readShort());
                    v[i].node.Add(d.readShort());
                    v[i].node.Add(d.readShort());
                    v[i].weight.Add(d.readHalfFloat());
                    v[i].weight.Add(d.readHalfFloat());
                    v[i].weight.Add(d.readHalfFloat());
                    v[i].weight.Add(d.readHalfFloat());
                }
                else if (weight == 4)
                {
                    v[i].node.Add(d.readByte());
                    v[i].node.Add(d.readByte());
                    v[i].node.Add(d.readByte());
                    v[i].node.Add(d.readByte());
                    v[i].weight.Add((float)d.readByte() / 255f);
                    v[i].weight.Add((float)d.readByte() / 255f);
                    v[i].weight.Add((float)d.readByte() / 255f);
                    v[i].weight.Add((float)d.readByte() / 255f);
                }
                else if (weight == 0)
                {
                    v[i].node.Add((short)o.singlebind);
                    v[i].weight.Add(1);
                }
            }

            foreach (Vertex vi in v)
                m.vertices.Add(vi);
        }
        #endregion

        #region Building
        public override byte[] Rebuild()
        {
            FileOutput d = new FileOutput(); // data
            d.Endian = Endianness.Big;

            // mesh optimize

            d.writeString("NDP3");
            d.writeInt(0); //FileSize
            d.writeShort(0x200); //  version num
            d.writeShort(mesh.Count); // polysets

            foreach (ModelContainer con in Runtime.ModelContainers)
            {
                if (con.nud == this && con.vbn!=null)
                    boneCount = con.vbn.bones.Count;
            }

            d.writeShort(boneCount == 0 ? 0 : 2); // type
            d.writeShort(boneCount == 0 ? boneCount : boneCount - 1); // Number of bones

            d.writeInt(0); // polyClump start
            d.writeInt(0); // polyClump size
            d.writeInt(0); // vertexClumpsize
            d.writeInt(0); // vertexaddcump size
            
            d.writeFloat(param[0]);
            d.writeFloat(param[1]);
            d.writeFloat(param[2]);
            d.writeFloat(param[3]);

            // other sections....
            FileOutput obj = new FileOutput(); // data
            obj.Endian = Endianness.Big;
            FileOutput tex = new FileOutput(); // data
            tex.Endian = Endianness.Big;

            FileOutput poly = new FileOutput(); // data
            poly.Endian = Endianness.Big;
            FileOutput vert = new FileOutput(); // data
            vert.Endian = Endianness.Big;
            FileOutput vertadd = new FileOutput(); // data
            vertadd.Endian = Endianness.Big;

            FileOutput str = new FileOutput(); // data
            str.Endian = Endianness.Big;


            // obj descriptor

            FileOutput tempstring = new FileOutput(); // data
            for (int i = 0; i < mesh.Count; i++)
            {
                str.writeString(mesh[i].Text);
                str.writeByte(0);
                str.align(16);
            }

            int polyCount = 0; // counting number of poly
            foreach (Mesh m in mesh)
                polyCount += m.polygons.Count;

            for (int i = 0; i < mesh.Count; i++)
            {
                foreach (float f in mesh[i].bbox)
                    d.writeFloat(f);

                d.writeInt(tempstring.size());

                tempstring.writeString(mesh[i].Text);
                tempstring.writeByte(0);
                tempstring.align(16);

                d.writeInt(mesh[i].boneflag); // ID
                d.writeShort(mesh[i].singlebind); // Single Bind 
                d.writeShort(mesh[i].polygons.Count); // poly count
                d.writeInt(obj.size() + 0x30 + mesh.Count * 0x30); // position start for obj

                // write obj info here...
                for (int k = 0; k < mesh[i].polygons.Count; k++)
                {
                    obj.writeInt(poly.size());
                    obj.writeInt(vert.size());
                    obj.writeInt(mesh[i].polygons[k].vertSize >> 4 > 0 ? vertadd.size() : 0);
                    obj.writeShort(mesh[i].polygons[k].vertices.Count);
                    obj.writeByte(mesh[i].polygons[k].vertSize); // type of vert

                    int maxUV = mesh[i].polygons[k].vertices[0].tx.Count; // TODO: multi uv stuff  mesh[i].polygons[k].maxUV() + 

                    obj.writeByte(mesh[i].polygons[k].UVSize); 

                    // MATERIAL SECTION 

                    FileOutput te = new FileOutput();
                    te.Endian = Endianness.Big;

                    int[] texoff = writeMaterial(tex, mesh[i].polygons[k].materials, str);
                    //tex.writeOutput(te);

                    //obj.writeInt(tex.size() + 0x30 + mesh.Count * 0x30 + polyCount * 0x30); // Tex properties... This is tex offset
                    obj.writeInt(texoff[0] + 0x30 + mesh.Count * 0x30 + polyCount * 0x30);
                    obj.writeInt(texoff[1] > 0 ? texoff[1] + 0x30 + mesh.Count * 0x30 + polyCount * 0x30 : 0);
                    obj.writeInt(texoff[2] > 0 ? texoff[2] + 0x30 + mesh.Count * 0x30 + polyCount * 0x30 : 0);
                    obj.writeInt(texoff[3] > 0 ? texoff[3] + 0x30 + mesh.Count * 0x30 + polyCount * 0x30 : 0);

                    obj.writeShort(mesh[i].polygons[k].faces.Count); // polyamt
                    obj.writeByte(mesh[i].polygons[k].strip); // polysize 0x04 is strips and 0x40 is easy
                    // :D
                    obj.writeByte(mesh[i].polygons[k].polflag); // polyflag

                    obj.writeInt(0); // idk, nothing padding??
                    obj.writeInt(0);
                    obj.writeInt(0);

                    // Write the poly...
                    foreach (int face in mesh[i].polygons[k].faces)
                        poly.writeShort(face);

                    // Write the vertex....

                    writeVertex(vert, vertadd, mesh[i].polygons[k]);
                    vertadd.align(4, 0x0);
                }
            }

            //
            d.writeOutput(obj);
            d.writeOutput(tex);
            d.align(16);

            d.writeIntAt(d.size() - 0x30, 0x10);
            d.writeIntAt(poly.size(), 0x14);
            d.writeIntAt(vert.size(), 0x18);
            d.writeIntAt(vertadd.size(), 0x1c);

            d.writeOutput(poly);

            int s = d.size();
            d.align(16);
            s = d.size() - s;
            d.writeIntAt(poly.size() + s, 0x14);

            d.writeOutput(vert);

            s = d.size();
            d.align(16);
            s = d.size() - s;
            d.writeIntAt(vert.size() + s, 0x18);

            d.writeOutput(vertadd);

            s = d.size();
            d.align(16);
            s = d.size() - s;
            d.writeIntAt(vertadd.size() + s, 0x1c);

            d.writeOutput(str);

            d.writeIntAt(d.size(), 0x4);

            return d.getBytes();
        }

        private static void writeUV(FileOutput d, Polygon m)
        {
            for (int i = 0; i < m.vertices.Count; i++)
            {
                if ((m.UVSize & 0xF) == 0x2)
                {
                    d.writeByte((int)m.vertices[i].col.X);
                    d.writeByte((int)m.vertices[i].col.Y);
                    d.writeByte((int)m.vertices[i].col.Z);
                    d.writeByte((int)m.vertices[i].col.W);
                    for (int j = 0; j < m.vertices[i].tx.Count; j++)
                    {
                        d.writeHalfFloat(m.vertices[i].tx[j].X);
                        d.writeHalfFloat(m.vertices[i].tx[j].Y);
                    }
                }
                else
                    throw new NotImplementedException("Unsupported UV format");
            }
        }

        private static void writeVertex(FileOutput d, FileOutput add, Polygon m)
        {
            int weight = m.vertSize >> 4;
            int nrm = m.vertSize & 0xF;
            
            if (weight > 0)
            {
                writeUV(d, m);
                d = add;
            }

            for (int i = 0; i < m.vertices.Count; i++)
            {
                Vertex v = m.vertices[i];
                d.writeFloat(v.pos.X);
                d.writeFloat(v.pos.Y);
                d.writeFloat(v.pos.Z);
                
                if (nrm > 1)
                {
                    d.writeHalfFloat(v.nrm.X);
                    d.writeHalfFloat(v.nrm.Y);
                    d.writeHalfFloat(v.nrm.Z);
                    d.writeHalfFloat(1);
                }
                else if (nrm == 1)
                {
                    d.writeFloat(v.nrm.X);
                    d.writeFloat(v.nrm.Y);
                    d.writeFloat(v.nrm.Z);
                    d.writeFloat(1);
                    d.writeFloat(1);
                }
                else
                    d.writeInt(0);

                if (nrm == 7)
                {
                    // bn and tan half floats
                    d.writeHalfFloat(m.vertices[i].bitan.X);
                    d.writeHalfFloat(m.vertices[i].bitan.Y);
                    d.writeHalfFloat(m.vertices[i].bitan.Z);
                    d.writeHalfFloat(m.vertices[i].bitan.W);
                    d.writeHalfFloat(m.vertices[i].tan.X);
                    d.writeHalfFloat(m.vertices[i].tan.Y);
                    d.writeHalfFloat(m.vertices[i].tan.Z);
                    d.writeHalfFloat(m.vertices[i].tan.W);
                }

                if (weight == 0)
                {
                    if (m.UVSize >= 18)
                    {
                        d.writeByte((int)m.vertices[i].col.X);
                        d.writeByte((int)m.vertices[i].col.Y);
                        d.writeByte((int)m.vertices[i].col.Z);
                        d.writeByte((int)m.vertices[i].col.W);
                    }

                    for (int j = 0; j < m.vertices[i].tx.Count; j++)
                    {
                        d.writeHalfFloat(m.vertices[i].tx[j].X);
                        d.writeHalfFloat(m.vertices[i].tx[j].Y);
                    }

                    // UV layers
                    //d.skip(4 * ((m.UVSize >> 4) - 1));
                }

                if (weight == 1)
                {
                    d.writeInt(v.node.Count > 0 ? v.node[0] : 0);
                    d.writeInt(v.node.Count > 1 ? v.node[1] : 0);
                    d.writeInt(v.node.Count > 2 ? v.node[2] : 0);
                    d.writeInt(v.node.Count > 3 ? v.node[3] : 0);
                    d.writeFloat(v.weight.Count > 0 ? v.weight[0] : 0);
                    d.writeFloat(v.weight.Count > 1 ? v.weight[1] : 0);
                    d.writeFloat(v.weight.Count > 2 ? v.weight[2] : 0);
                    d.writeFloat(v.weight.Count > 3 ? v.weight[3] : 0);
                }
                if (weight == 2)
                {
                    d.writeShort(v.node.Count > 0 ? v.node[0] : 0);
                    d.writeShort(v.node.Count > 1 ? v.node[1] : 0);
                    d.writeShort(v.node.Count > 2 ? v.node[2] : 0);
                    d.writeShort(v.node.Count > 3 ? v.node[3] : 0);
                    d.writeHalfFloat(v.weight.Count > 0 ? v.weight[0] : 0);
                    d.writeHalfFloat(v.weight.Count > 1 ? v.weight[1] : 0);
                    d.writeHalfFloat(v.weight.Count > 2 ? v.weight[2] : 0);
                    d.writeHalfFloat(v.weight.Count > 3 ? v.weight[3] : 0);
                }

                if (weight == 4)
                {
                    d.writeByte(v.node.Count > 0 ? v.node[0] : 0);
                    d.writeByte(v.node.Count > 1 ? v.node[1] : 0);
                    d.writeByte(v.node.Count > 2 ? v.node[2] : 0);
                    d.writeByte(v.node.Count > 3 ? v.node[3] : 0);
                    d.writeByte((int)(v.weight.Count > 0 ? Math.Round(v.weight[0] * 0xFF) : 0));
                    d.writeByte((int)(v.weight.Count > 1 ? Math.Round(v.weight[1] * 0xFF) : 0));
                    d.writeByte((int)(v.weight.Count > 2 ? Math.Round(v.weight[2] * 0xFF) : 0));
                    d.writeByte((int)(v.weight.Count > 3 ? Math.Round(v.weight[3] * 0xFF) : 0));
                }
            }
        }

        public static int[] writeMaterial(FileOutput d, List<Material> materials, FileOutput str)
        {
            int[] offs = new int[4];
            int c = 0;
            foreach (Material mat in materials)
            {
                offs[c++] = d.size();
                d.writeInt((int)mat.flags);
                d.writeInt(0); // padding
                d.writeShort(mat.srcFactor);
                d.writeShort(mat.textures.Count);
                d.writeShort(mat.dstFactor);
                d.writeByte(mat.ref1);
                d.writeByte(mat.ref0);
                d.writeByte(0); // unknown padding?
                d.writeByte(mat.drawPriority);
                d.writeByte(mat.cullMode);
                d.writeByte(mat.unknown1);
                d.writeInt(0); // padding
                d.writeInt(mat.unkownWater); 
                d.writeInt(mat.zBufferOffset); 

                foreach (Mat_Texture tex in mat.textures)
                {
                    d.writeInt(tex.hash);
                    d.writeInt(0);
                    d.writeShort(0);
                    d.writeShort(tex.MapMode);
                    d.writeByte(tex.WrapMode1);
                    d.writeByte(tex.WrapMode2);
                    d.writeByte(tex.minFilter);
                    d.writeByte(tex.magFilter);
                    d.writeByte(tex.mipDetail);
                    d.writeByte(tex.unknown);
                    d.writeInt(0); // padding
                    d.writeShort(0);
                }

                for (int i = 0; i < mat.entries.Count; i++)
                {
                    float[] data;
                    mat.entries.TryGetValue(mat.entries.ElementAt(i).Key, out data);
                    d.writeInt(i == mat.entries.Count - 1 ? 0 : 16 + 4 * data.Length);
                    d.writeInt(str.size());

                    str.writeString(mat.entries.ElementAt(i).Key);
                    str.writeByte(0);
                    str.align(16);

                    d.writeInt(data.Length);
                    d.writeInt(0);
                    foreach (float f in data)
                        d.writeFloat(f);
                }
            }
            return offs;
        }

        #endregion
        
        #region Functions
        public void MergePoly()
        {
            Dictionary<string, Mesh> nmesh = new Dictionary<string, Mesh>();
            foreach(Mesh m in mesh)
            {
                if (nmesh.ContainsKey(m.Text))
                {
                    // merge poly
                    nmesh[m.Text].polygons.AddRange(m.polygons);
                } else
                {
                    nmesh.Add(m.Text, m);
                }
            }
            // consolidate
            mesh.Clear();
            foreach (string n in nmesh.Keys)
            {
                mesh.Add(nmesh[n]);
            }
            PreRender();
        }
        #endregion

        #region ClassStructure

        public struct Vector4i
        {
            int x, y, z, w;

            public Vector4i(int x, int y, int z, int w)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
            }

            public static int Size = 4 * sizeof(int);
        }

        public struct dVertex
        {
            public Vector3 pos;
            public Vector3 nrm;
            public Vector2 tx0;
            public Vector4 col;
            public Vector4 node;
            public Vector4 weight;

            public static int Size = 4 * (3 + 3 + 2 + 4 + 4 + 4);
        }

        public class Vertex
        {
            public Vector3 pos = new Vector3(0, 0, 0), nrm = new Vector3(0, 0, 0);
            public Vector4 bitan = new Vector4(0, 0, 0, 1), tan = new Vector4(0, 0, 0, 1);
            public Vector4 col = new Vector4(127, 127, 127, 127);
            public List<Vector2> tx = new List<Vector2>();
            public List<int> node = new List<int>();
            public List<float> weight = new List<float>();

            public Vertex()
            {
            }

            public Vertex(float x, float y, float z)
            {
                pos = new Vector3(x, y, z);
            }

            public bool Equals(Vertex p)
            {
                return pos.Equals(p.pos) && nrm.Equals(p.nrm) && new HashSet<Vector2>(tx).SetEquals(p.tx) && col.Equals(p.col)
                    && new HashSet<int>(node).SetEquals(p.node) && new HashSet<float>(weight).SetEquals(p.weight);
            }
        }


        public class Mat_Texture
        {
            public int hash;
            public int MapMode = 0;
            public int WrapMode1 = 0;
            public int WrapMode2 = 0;
            public int minFilter = 0;
            public int magFilter = 0;
            public int mipDetail = 0;
            public int unknown = 0;
        }

        public enum SrcFactors 
        {
            Nothing = 0x0,
            SourceAlpha = 0x1,
            One = 0x2,
            InverseSourceAlpha = 0x3,
            SourceColor = 0x4,
            Zero = 0xA
        }

        public class Material
        {
            public Dictionary<string, float[]> entries = new Dictionary<string, float[]>();
            public Dictionary<string, float[]> anims = new Dictionary<string, float[]>();
            public List<Mat_Texture> textures = new List<Mat_Texture>();

            public uint flags;
            public int blendMode = 0;
            public int dstFactor = 0;
            public int srcFactor = 0;
            public int ref1 = 0;
            public int ref0 = 0;
            public int drawPriority = 0;
            public int cullMode = 0;
            public int displayTexId = -1;

            public int unknown1 = 0;
            public int unkownWater = 0;
            public int zBufferOffset = 0;

            public Material()
            {
            }
        }

        public class Polygon : TreeNode
        {
            public List<Vertex> vertices = new List<Vertex>();
            public List<int> faces = new List<int>();
            public int displayFaceSize = 0;

            // Material
            public List<Material> materials = new List<Material>();

            // for nud stuff
            public int vertSize = 0x46; // defaults to a basic bone weighted vertex format
            public int UVSize = 0x12;
            public int strip = 0x40;
            public int polflag = 0x04;

            // for drawing
            public bool isTransparent = false;
            public dVertex[] vertdata;
            public int[] display;

            public Polygon()
            {
                Checked = true;
            }

            public void AddVertex(Vertex v)
            {
                vertices.Add(v);
            }

            public void setDefaultMaterial()
            {
                Material mat = new Material();
                mat.flags = 0x9a011063;
                mat.cullMode = 4;
                mat.entries.Add("NU_colorSamplerUV", new float[]{1, 1, 0, 0});
                mat.entries.Add("NU_materialHash", new float[] {FileData.toFloat(0x68617368), 0, 0, 0});
                materials.Add(mat);
                
                mat.textures.Add(makeDefault());
                mat.textures.Add(makeDefault());
                mat.textures.Add(makeDefault());
            }

            public static Mat_Texture makeDefault()
            {
                Mat_Texture dif = new Mat_Texture();
                dif.WrapMode1 = 1;
                dif.WrapMode2 = 1;
                dif.minFilter = 3;
                dif.magFilter = 2;
                dif.mipDetail = 1;
                dif.mipDetail = 6;
                dif.hash = 0x10080000;
                return dif;
            }

            public List<int> getDisplayFace()
            {
                if ((strip >> 4) == 4)
                {
                    displayFaceSize = faces.Count;
                    return faces;
                }
                else
                {
                    List<int> f = new List<int>();

                    int startDirection = 1;
                    int p = 0;
                    int f1 = faces[p++];
                    int f2 = faces[p++];
                    int faceDirection = startDirection;
                    int f3;
                    do
                    {
                        f3 = faces[p++];
                        if (f3 == 0xFFFF)
                        {
                            f1 = faces[p++];
                            f2 = faces[p++];
                            faceDirection = startDirection;
                        }
                        else
                        {
                            faceDirection *= -1;
                            if ((f1 != f2) && (f2 != f3) && (f3 != f1))
                            {
                                if (faceDirection > 0)
                                {
                                    f.Add(f3);
                                    f.Add(f2);
                                    f.Add(f1);
                                }
                                else
                                {
                                    f.Add(f2);
                                    f.Add(f3);
                                    f.Add(f1);
                                }
                            }
                            f1 = f2;
                            f2 = f3;
                        }
                    } while (p < faces.Count);

                    displayFaceSize = f.Count;
                    return f;
                }
            }
        }

        // typically a mesh will just have 1 polygon
        // but you can just use the mesh class without polygons
        public class Mesh : TreeNode
        {
            public List<Polygon> polygons = new List<Polygon>();
            public int boneflag = 4; // 0 not rigged 4 rigged 8 singlebind
            public short singlebind = -1;
            
            public float[] bbox = new float[8];

            public Mesh()
            {
                Checked = true;
            }

            public void addVertex(Vertex v)
            {
                if (polygons.Count == 0)
                    polygons.Add(new Polygon());

                polygons[0].AddVertex(v);
            }
        }

        #endregion

        #region Converters

        /*public MBN toMBN()
        {
            MBN m = new Smash_Forge.MBN();

            m.setDefaultDescriptor();
            List<MBN.Vertex> vertBank = new List<MBN.Vertex>();

            foreach (Mesh mesh in mesh)
            {
                MBN.Mesh nmesh = new MBN.Mesh();

                int pi = 0;
                int fadd = vertBank.Count;
                nmesh.nodeList = new List<List<int>>();
                nmesh.faces = new List<List<int>>();
                foreach (Polygon p in mesh.polygons)
                {
                    List<int> nodeList = new List<int>();
                    // vertices
                    foreach(Vertex v in p.vertices)
                    {
                        MBN.Vertex mv = new MBN.Vertex();
                        mv.pos = v.pos;
                        mv.nrm = v.nrm;
                        mv.tx = v.tx;
                        mv.col = v.col;
                        int n1 = v.node[0];
                        int n2 = v.node.Count > 1 ? v.node[1] : 0;
                        if (!nodeList.Contains(n1)) nodeList.Add(n1);
                        if (!nodeList.Contains(n2)) nodeList.Add(n2);
                        mv.node.Add(nodeList.IndexOf(n1));
                        mv.node.Add(nodeList.IndexOf(n2));
                        mv.weight.Add(v.weight[0]);
                        mv.weight.Add(v.weight.Count > 1 ? v.weight[1] : 0);
                        vertBank.Add(mv);
                    }
                    // Node list 
                    nmesh.nodeList.Add(nodeList);
                    // polygons
                    List<int> fac = new List<int>();
                    nmesh.faces.Add(fac);
                    foreach (int i in p.faces)
                        fac.Add(i + fadd);
                    pi++;
                }

                m.mesh.Add(nmesh);
            }
            m.vertices = vertBank;

            //Console.WriteLine(m.vertices.Count + " " + m.descript.Count);

            return m;
        }

    */
        public void Optimize()
        {
            // to help with duplicates

            bool isSingleBound;
            int sbind;
            foreach (Mesh m in mesh)
            {
                isSingleBound = true;
                sbind = -1;
                foreach (Polygon p in m.polygons)
                {
                    List<Vertex> nVert = new List<Vertex>();
                    List<int> nFace = new List<int>();
                    //Console.WriteLine("Optimizing " + m.Text);
                    foreach (int f in p.faces)
                    {
                        int pos = -1; // nVert.IndexOf(p.vertices[f]);
                        int i = 0;
                        foreach (Vertex v in nVert)
                        {
                            if (v.node.Count > 0)
                            {
                                if (sbind == -1)
                                    sbind = p.vertices[f].node[0];
                                else
                                if (p.vertices[f].node[0] != sbind)
                                    isSingleBound = false;
                            }

                            if (v.Equals(p.vertices[f]))
                            {
                                pos = i;
                                break;
                            }
                            else
                                i++;
                        }

                        if (pos != -1)
                        {
                            nFace.Add(pos);
                        }
                        else
                        {
                            nVert.Add(p.vertices[f]);
                            nFace.Add(nVert.Count - 1);
                        }
                    }
                    p.vertices = nVert;
                    p.faces = nFace;
                    p.displayFaceSize = 0;
                }
                if (isSingleBound)
                {
                    m.boneflag = 0x08; // single bind flag
                    m.singlebind = (short)sbind; // single bind bone
                    foreach (Polygon p in m.polygons)
                    {
                        p.vertSize = p.vertSize & 0x0F;
                    }
                }
            }

            PreRender();
        }

        #endregion

        public List<int> GetTexIds()
        {
            List<int> texIds = new List<int>();
            foreach (var m in mesh)
                foreach (var poly in m.polygons)
                    foreach (var mat in poly.materials)
                        if(!texIds.Contains(mat.displayTexId))
                            texIds.Add(mat.displayTexId);
            return texIds;
        }
    }
}

