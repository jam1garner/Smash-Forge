using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Syroot.NintenTools.Yaz0;
using Syroot.NintenTools.NSW.Bfres;
using Syroot.NintenTools.NSW.Bfres.GFX;
using Syroot.NintenTools.NSW.Bfres.Helpers;

namespace Smash_Forge
{
    public partial class BFRES : TreeNode
    {

        public void Read(ResFile TargetSwitchBFRES, FileData f)
        {

            Nodes.Add(TModels);
            Nodes.Add(TMaterialAnim);
            Nodes.Add(TVisualAnim);
            Nodes.Add(TShapeAnim);
            Nodes.Add(TSceneAnim);
            Nodes.Add(TEmbedded);
            ImageKey = "bfres";
            SelectedImageKey = "bfres";

            FSKACount = TargetSwitchBFRES.SkeletalAnims.Count;
            FVISCount = TargetSwitchBFRES.BoneVisibilityAnims.Count;
            FMAACount = TargetSwitchBFRES.MaterialAnims.Count;
            FSHACount = TargetSwitchBFRES.ShapeAnims.Count;

            Console.WriteLine("Name = " + TargetSwitchBFRES.Name);

            foreach (ExternalFile ext in TargetSwitchBFRES.ExternalFiles)
            {
                f = new FileData(ext.Data);

                if (ext.Data.Length > 4) //BOTW has some external files that are smaller than 4 which i need to read for magic
                {

                    int EmMagic = f.readInt();

                    if (EmMagic == 0x424E5458) //Textures
                    {
                        f.Endian = Endianness.Little;
                        f.skip(-4);


                        int temp = f.pos();
                        BNTX t = new BNTX();
                        t.ReadBNTX(f);
                        TEmbedded.Nodes.Add(t);
                    }
                }

            }


            int ModelCur = 0;
            //FMDLs -Models-
            foreach (Model mdl in TargetSwitchBFRES.Models)
            {
                FMDL_Model model = new FMDL_Model(); //This will store VBN data and stuff
                model.Text = mdl.Name;

                TModels.Nodes.Add(model);

                ReadSkeleton(model, mdl);

                model.skeleton.reset();
                model.skeleton.update();

                //MeshTime!!
                foreach (Shape shp in mdl.Shapes)
                {
                    Mesh poly = new Mesh();
                    poly.Text = shp.Name;
                    poly.MaterialIndex = shp.MaterialIndex;
                    poly.matrFlag = shp.VertexSkinCount;
                    poly.fsklindx = shp.BoneIndex;

                    if (model.Node_Array != null)
                        Console.WriteLine(model.skeleton.bones[poly.fsklindx].Text);

                    foreach (int bn in shp.SkinBoneIndices)
                    {
                        Console.WriteLine(model.skeleton.bones[bn].Text);
                        poly.BoneIndexList.Add(model.skeleton.bones[bn].Text, bn);
                    }

                    TModels.Nodes[ModelCur].Nodes.Add(poly);


                    ReadVertexBuffer(mdl, shp, poly);


                    //  int LODCount = shp.Meshes.Count - 1; //For going to the lowest poly LOD mesh
                    int LODCount = 0;

                    uint FaceCount = FaceCount = shp.Meshes[LODCount].IndexCount;
                    uint[] indicesArray = shp.Meshes[LODCount].GetIndices().ToArray();

                    poly.BoundingCount = shp.SubMeshBoundings.Count;

                    for (int face = 0; face < FaceCount; face++)
                        poly.faces.Add((int)indicesArray[face] + (int)shp.Meshes[LODCount].FirstVertex);

                    foreach (Bounding bnd in shp.SubMeshBoundings)
                    {

                        Mesh.BoundingBox box = new Mesh.BoundingBox();
                        box.Center = new Vector3(bnd.Center.X, bnd.Center.Y, bnd.Center.Z);
                        box.Extent = new Vector3(bnd.Extent.X, bnd.Extent.Y, bnd.Extent.Z);

                        poly.boundingBoxes.Add(box); //Each box is by LOD mesh. This will be in a seperate class later so only one will be added

                    }
                    foreach (float r in shp.RadiusArray)
                    {
                        poly.radius.Add(r);
                    }

                    // Read materials
                    Material mat = mdl.Materials[shp.MaterialIndex];

                    poly.material.Name = mat.Name;

                    int SampIndex = 0;
                    foreach (var smp in mat.SamplerDict)
                    {
                        poly.material.Samplers.Add(smp.Key, SampIndex);
                        SampIndex++;
                    }


                    int o = 0;
                    foreach (var op in mat.ShaderAssign.ShaderOptionDict)
                    {
                        poly.material.shaderassign.Add(op.Key, mat.ShaderAssign.ShaderOptions[o]);
                        o++;
                    }

                    ReadTextureRefs(mat, poly);
                    ReadShaderParams(mat, poly);
                    ReadRenderInfo(mat, poly);

                    foreach (Sampler smp in mdl.Materials[shp.MaterialIndex].Samplers)
                    {
                        SamplerInfo s = new SamplerInfo();
                        s.WrapModeU = (int)smp.WrapModeU;
                        s.WrapModeV = (int)smp.WrapModeV;
                        s.WrapModeW = (int)smp.WrapModeW;
                        poly.material.samplerinfo.Add(s);
                    }

                    model.poly.Add(poly);
                }
                models.Add(model);
                ModelCur++;
            }

        }
        private void ReadSkeleton(FMDL_Model model, Model mdl)
        {
            model.Node_Array = new int[mdl.Skeleton.MatrixToBoneList.Count];
            int nodes = 0;
            foreach (ushort node in mdl.Skeleton.MatrixToBoneList)
            {
                model.Node_Array[nodes] = node;
                nodes++;
            }

            foreach (Syroot.NintenTools.NSW.Bfres.Bone bn in mdl.Skeleton.Bones)
            {

                Bone bone = new Bone(model.skeleton);
                bone.Text = bn.Name;
                bone.boneId = bn.BillboardIndex;
                bone.parentIndex = bn.ParentIndex;
                bone.scale = new float[3];
                bone.rotation = new float[4];
                bone.position = new float[3];

                if (bn.FlagsRotation == BoneFlagsRotation.Quaternion)
                    bone.boneRotationType = 1;
                else
                    bone.boneRotationType = 0;
                bone.scale[0] = bn.Scale.X;
                bone.scale[1] = bn.Scale.Y;
                bone.scale[2] = bn.Scale.Z;
                bone.rotation[0] = bn.Rotation.X;
                bone.rotation[1] = bn.Rotation.Y;
                bone.rotation[2] = bn.Rotation.Z;
                bone.rotation[3] = bn.Rotation.W;
                bone.position[0] = bn.Position.X;
                bone.position[1] = bn.Position.Y;
                bone.position[2] = bn.Position.Z;

                model.skeleton.bones.Add(bone);

            }
        }
        private void ReadVertexBuffer(Model mdl, Shape shp, Mesh poly)
        {
            //Create a buffer instance which stores all the buffer data
            VertexBufferHelper helper = new VertexBufferHelper(mdl.VertexBuffers[shp.VertexBufferIndex], TargetSwitchBFRES.ByteOrder);

            Vertex v = new Vertex();
            foreach (VertexAttrib att in mdl.VertexBuffers[shp.VertexBufferIndex].Attributes)
            {
                if (att.Name == "_p0")
                {
                    VertexBufferHelperAttrib position = helper["_p0"];
                    Syroot.Maths.Vector4F[] vec4Positions = position.Data;

                    foreach (Syroot.Maths.Vector4F p in vec4Positions)
                    {
                        v.pos.Add(new Vector3 { X = p.X, Y = p.Y, Z = p.Z });
                    }
                }
                if (att.Name == "_n0")
                {
                    VertexBufferHelperAttrib normal = helper["_n0"];
                    Syroot.Maths.Vector4F[] vec4Normals = normal.Data;

                    foreach (Syroot.Maths.Vector4F n in vec4Normals)
                    {
                        v.nrm.Add(new Vector3 { X = n.X, Y = n.Y, Z = n.Z });
                    }
                }
                if (att.Name == "_u0")
                {
                    VertexBufferHelperAttrib uv0 = helper["_u0"];
                    Syroot.Maths.Vector4F[] vec4uv0 = uv0.Data;

                    foreach (Syroot.Maths.Vector4F u in vec4uv0)
                    {
                        v.uv0.Add(new Vector2 { X = u.X, Y = u.Y });
                    }
                }
                if (att.Name == "_u1")
                {

                    VertexBufferHelperAttrib uv1 = helper["_u1"];
                    Syroot.Maths.Vector4F[] vec4uv1 = uv1.Data;

                    foreach (Syroot.Maths.Vector4F u in vec4uv1)
                    {
                        v.uv1.Add(new Vector2 { X = u.X, Y = u.Y });
                    }
                }
                if (att.Name == "_u2")
                {
                    VertexBufferHelperAttrib uv2 = helper["_u2"];
                    Syroot.Maths.Vector4F[] vec4uv2 = uv2.Data;

                    foreach (Syroot.Maths.Vector4F u in vec4uv2)
                    {
                        v.uv2.Add(new Vector2 { X = u.X, Y = u.Y });
                    }
                }
                if (att.Name == "_c0")
                {
                    VertexBufferHelperAttrib c0 = helper["_c0"];
                    Syroot.Maths.Vector4F[] vec4c0 = c0.Data;

                    foreach (Syroot.Maths.Vector4F c in vec4c0)
                    {
                        v.col.Add(new Vector4 { X = c.X, Y = c.Y, Z = c.Z, W = c.W });
                    }
                }
                if (att.Name == "_t0")
                {
                    VertexBufferHelperAttrib t0 = helper["_t0"];
                    Syroot.Maths.Vector4F[] vec4t0 = t0.Data;

                    foreach (Syroot.Maths.Vector4F u in vec4t0)
                    {
                        v.tans.Add(new Vector4 { X = u.X, Y = u.Y, Z = u.Z, W = u.W });
                    }
                }
                if (att.Name == "_b0")
                {

                    VertexBufferHelperAttrib b0 = helper["_b0"];
                    Syroot.Maths.Vector4F[] vec4b0 = b0.Data;

                    foreach (Syroot.Maths.Vector4F u in vec4b0)
                    {
                        v.bitans.Add(new Vector4 { X = u.X, Y = u.Y, Z = u.Z, W = u.W });
                    }
                }
                if (att.Name == "_w0")
                {

                    VertexBufferHelperAttrib w0 = helper["_w0"];
                    Syroot.Maths.Vector4F[] vec4w0 = w0.Data;

                    foreach (Syroot.Maths.Vector4F w in vec4w0)
                    {
                        v.weights.Add(new Vector4 { X = w.X, Y = w.Y, Z = w.Z, W = w.W });
                    }
                }
                if (att.Name == "_i0")
                {
                    VertexBufferHelperAttrib i0 = helper["_i0"];
                    Syroot.Maths.Vector4F[] vec4i0 = i0.Data;

                    foreach (Syroot.Maths.Vector4F i in vec4i0)
                    {
                        v.nodes.Add(new Vector4 { X = i.X, Y = i.Y, Z = i.Z, W = i.W });
                    }
                }

                //Set these for morphing
                //This is a test. I may put this in it's own VBO since it's not offten used

                if (att.Name == "_p1")
                {
                    VertexBufferHelperAttrib p1 = helper["_p1"];
                    Syroot.Maths.Vector4F[] vec4p1 = p1.Data;

                    foreach (Syroot.Maths.Vector4F p in vec4p1)
                    {
                        v.pos1.Add(new Vector3 { X = p.X, Y = p.Y, Z = p.Z});
                    }
                }

                if (att.Name == "_p2")
                {
                    VertexBufferHelperAttrib p2 = helper["_p2"];
                    Syroot.Maths.Vector4F[] vec4p2 = p2.Data;

                    foreach (Syroot.Maths.Vector4F p in vec4p2)
                    {
                        v.pos2.Add(new Vector3 { X = p.X, Y = p.Y, Z = p.Z });
                    }
                }

            }
            poly.vertices = v;
        }
        private void ReadTextureRefs(Material mat, Mesh poly)
        {
            int AlbedoCount = 0;
            int id = 0;

            string TextureName = "";

            foreach (TextureRef tex in mat.TextureRefs)
            {
                TextureName = mat.TextureRefs[id].Name;

                MatTexture texture = new MatTexture();

                texture.wrapModeS = (int)mat.Samplers[id].WrapModeU;
                texture.wrapModeT = (int)mat.Samplers[id].WrapModeV;


                bool IsAlbedo = HackyTextureList.Any(TextureName.Contains);

                //This works decently for now. I tried samplers but Kirby Star Allies doesn't map with samplers properly? 
                if (IsAlbedo)
                {
                    if (AlbedoCount == 0)
                    {
                        poly.material.HasDiffuseMap = true;
                        AlbedoCount++;
                        texture.hash = 0;
                        texture.Type = MatTexture.TextureType.Diffuse;
                    }
                }


                else if (TextureName.Contains("Nrm") || TextureName.Contains("Norm") || TextureName.Contains("norm") || TextureName.Contains("nrm"))
                {
                    texture.hash = 1;
                    poly.material.HasNormalMap = true;
                    texture.Type = MatTexture.TextureType.Normal;
                }
                else if (TextureName.Contains("Emm"))
                {
                    texture.hash = 8;
                    poly.material.HasEmissionMap = true;
                    texture.Type = MatTexture.TextureType.Emission;
                }
                else if (TextureName.Contains("Spm"))
                {
                    texture.hash = 4;
                    poly.material.HasSpecularMap = true;
                    texture.Type = MatTexture.TextureType.Specular;
                }
                else if (TextureName.Contains("b00"))
                {
                    texture.hash = 2;
                    poly.material.HasShadowMap = true;
                    texture.Type = MatTexture.TextureType.Shadow;
                }
                else if (TextureName.Contains("b01") || TextureName.Contains("Moc"))
                {
                    texture.hash = 3;
                    poly.material.HasLightMap = true;
                }
                else if (TextureName.Contains("MRA")) //Metalness, Roughness, and Cavity Map in one
                {
                    texture.hash = 17;
                    poly.material.HasRoughnessMap = true;
                    texture.Type = MatTexture.TextureType.MRA;
                }
                else if (TextureName.Contains("mtl"))
                {
                    texture.hash = 16;
                    poly.material.HasMetalnessMap = true;
                    texture.Type = MatTexture.TextureType.Metalness;
                }
                else if (TextureName.Contains("rgh"))
                {
                    texture.Type = MatTexture.TextureType.Roughness;
                    texture.hash = 18;
                    poly.material.HasRoughnessMap = true;
                }
                texture.Name = TextureName;
                poly.material.textures.Add(texture);
                id++;
            }
        }
        private void ReadRenderInfo(Material mat, Mesh poly)
        {
            foreach (RenderInfo rnd in mat.RenderInfos)
            {
                RenderInfoData r = new RenderInfoData();

                r.Name = rnd.Name;

                switch (rnd.Type)
                {
                    case RenderInfoType.Int32:
                        foreach (int rn in rnd.GetValueInt32s())
                        {
                            r.Value_Int = rn;
                        }
                        break;
                    case RenderInfoType.Single:
                        foreach (float rn in rnd.GetValueSingles())
                        {
                            r.Value_Float = rn;
                        }
                        break;
                    case RenderInfoType.String:
                        foreach (string rn in rnd.GetValueStrings())
                        {
                            r.Value_String = rn;
                        }
                        break;
                }

                poly.material.renderinfo.Add(r);

                SetAlphaBlending(poly.material, r);
                SetRenderMode(poly.material, r, poly);
            }
        }

        private void SetRenderMode(MaterialData mat, RenderInfoData r, Mesh m)
        {
            switch (r.Name)
            {
                case "gsys_render_state_mode":
                    if (r.Value_String == "transparent")
                    {
                        m.isTransparent = true;
                    }
                    if (r.Value_String == "alpha")
                    {
                        m.isTransparent = true;
                    }
                    if (r.Value_String == "mask")
                    {
                        m.isTransparent = true;
                    }               
                    break;
                case "mode":
                    if (r.Value_Int == 1)
                    {
                        m.isTransparent = true;
                    }
                    if (r.Value_Int == 2)
                    {
                        m.isTransparent = true;
                    }
                    break;
            }
        }

            private void SetAlphaBlending(MaterialData mat, RenderInfoData r)
        {
            switch (r.Name)
            {
                case "gsys_color_blend_rgb_src_func":
                    if (r.Value_String == "src_alpha")
                    {
                        mat.srcFactor = 1;
                    }
                    break;
                case "gsys_color_blend_rgb_dst_func":
                    if (r.Value_String == "one_minus_src_alpha")
                    {
                        mat.dstFactor = 1;
                    }
                    break;
                case "gsys_color_blend_rgb_op":
                    if (r.Value_String == "add")
                    {

                    }
                    break;
                case "gsys_color_blend_alpha_op":
                    if (r.Value_String == "add")
                    {
                    }
                    break;
                case "gsys_color_blend_alpha_src_func":
                    if (r.Value_String == "one")
                    {

                    }
                    break;
                case "gsys_color_blend_alpha_dst_func":
                    if (r.Value_String == "zero")
                    {
                    }
                    break;
                case "gsys_alpha_test_func":
                    break;
                case "gsys_alpha_test_value":
                    break;
                case "sourceColorBlendFactor":
                    if (r.Value_String == "sourceAlpha")
                    {
                        mat.srcFactor = 1;
                    }
                    break;
                default:
                    break;
            }
        }
        private static void SetAlphaTesting(MaterialData mat, RenderInfoData r)
        {
            switch (r.Name)
            {
                case "gsys_alpha_test_enable":
                    if (r.Value_String == "true")
                    {

                    }
                    else
                    {

                    }
                    break;
                case "gsys_alpha_test_func":
                    if (r.Value_String == "lequal")
                    {

                    }
                    else if (r.Value_String == "gequal")
                    {

                    }
                    break;
                case "gsys_alpha_test_value":
                    break;
                default:
                    break;
            }
        }
        private static void SetDepthTesting(MaterialData mat, RenderInfoData r)
        {
            switch (r.Name)
            {
                case "gsys_depth_test_enable":
                    if (r.Value_String == "true")
                    {

                    }
                    else
                    {

                    }
                    break;
                case "gsys_depth_test_func":
                    if (r.Value_String == "lequal")
                    {

                    }
                    break;
                case "gsys_depth_test_write":
                    break;
                default:
                    break;
            }
        }

        private void ReadShaderParams(Material mat, Mesh poly)
        {
            using (Syroot.BinaryData.BinaryDataReader reader = new Syroot.BinaryData.BinaryDataReader(new MemoryStream(mat.ShaderParamData)))
            {

                reader.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
                foreach (Syroot.NintenTools.NSW.Bfres.ShaderParam param in mat.ShaderParams)
                {
                    ShaderParam prm = new ShaderParam();

                    prm.Type = (Syroot.NintenTools.Bfres.ShaderParamType)param.Type;

                    switch (param.Type)
                    {
                        case ShaderParamType.Float:
                            reader.Seek(param.DataOffset, SeekOrigin.Begin);
                            prm.Value_float = reader.ReadSingle();
                            break;
                        case ShaderParamType.Float2:
                            reader.Seek(param.DataOffset, SeekOrigin.Begin);
                            prm.Value_float2 = new Vector2(
                                reader.ReadSingle(),
                                reader.ReadSingle());
                            break;
                        case ShaderParamType.Float3:
                            reader.Seek(param.DataOffset, SeekOrigin.Begin);
                            prm.Value_float3 = new Vector3(
                                    reader.ReadSingle(),
                                    reader.ReadSingle(),
                                    reader.ReadSingle()); break;
                        case ShaderParamType.Float4:
                            reader.Seek(param.DataOffset, SeekOrigin.Begin);
                            prm.Value_float4 = new Vector4(
                                    reader.ReadSingle(),
                                    reader.ReadSingle(),
                                    reader.ReadSingle(),
                                    reader.ReadSingle()); break;
                        case ShaderParamType.TexSrt:
                            reader.Seek(param.DataOffset, SeekOrigin.Begin);
                            ShaderParam.TextureSRT texSRT = new ShaderParam.TextureSRT();
                            texSRT.Mode      = reader.ReadSingle(); //Scale mode, Maya, max ect
                            texSRT.scale     = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                            texSRT.rotate    = reader.ReadSingle();
                            texSRT.translate = new Vector2(reader.ReadSingle(), reader.ReadSingle());

                            prm.Value_TexSrt = texSRT;
                            break;
                            
                    }
                    poly.material.matparam.Add(param.Name, prm);
                }
                reader.Close();
            }
        }


        #region HackyInject (No rebuild)

        public void InjectToFile(string FileName)
        {
            //Reparse and inject buffer data

            using (Syroot.BinaryData.BinaryDataWriter writer = new Syroot.BinaryData.BinaryDataWriter(File.Open(FileName, FileMode.Create)))
            {
                writer.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;

                writer.Write(BFRESFile);

                int mdl = 0;
                foreach (Model fmdl in TargetSwitchBFRES.Models)
                {
                    int s = 0;
                    foreach (Shape shp in fmdl.Shapes)
                    {
                        Vertex v = models[mdl].poly[s].vertices;

                        writer.Seek(shp.Meshes[0].DataOffset);

                        int[] Faces = models[mdl].poly[s].getDisplayFace().ToArray();

                        writer.Write(Faces);

                        foreach (VertexAttrib att in fmdl.VertexBuffers[shp.VertexBufferIndex].Attributes)
                        {
                            int test = (int)fmdl.VertexBuffers[shp.VertexBufferIndex].Buffers[att.BufferIndex].DataOffset + att.Offset + fmdl.VertexBuffers[shp.VertexBufferIndex].Buffers[att.BufferIndex].Stride;
                        }

                        for (int vt = 0; vt < fmdl.VertexBuffers[shp.VertexBufferIndex].VertexCount; vt++)
                        {
                            int at = 0;
                            foreach (VertexAttrib att in fmdl.VertexBuffers[shp.VertexBufferIndex].Attributes)
                            {
                                writer.Seek(fmdl.VertexBuffers[shp.VertexBufferIndex].Buffers[att.BufferIndex].DataOffset + att.Offset + fmdl.VertexBuffers[shp.VertexBufferIndex].Buffers[att.BufferIndex].Stride * vt, SeekOrigin.Begin);
                                if (att.Name == "_p0")
                                {
                                    if (att.Format == AttribFormat.Format_32_32_32_Single)
                                    {
                                        writer.Write(v.pos[vt].X);
                                        writer.Write(v.pos[vt].Y);
                                        writer.Write(v.pos[vt].Z);
                                    }
                                    else if (att.Format == AttribFormat.Format_16_16_16_16_Single)
                                    {
                                        Syroot.Maths.Vector4F value = new Syroot.Maths.Vector4F(v.pos[vt].X, v.pos[vt].Y, v.pos[vt].Z, 0);

                                        writer.Write((short)fromFloat(value.X));
                                        writer.Write((short)fromFloat(value.Y));
                                        writer.Write((short)fromFloat(value.Z));
                                        writer.Write((short)fromFloat(value.W));
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unsupported  format for bitans " + att.Format);
                                    }
                                }
                                if (att.Name == "_n0")
                                {
                                    if (att.Format == AttribFormat.Format_10_10_10_2_SNorm)
                                    {
                                        int x = SingleToInt10(Syroot.Maths.Algebra.Clamp(v.nrm[vt].X, -1, 1) * 511);
                                        int y = SingleToInt10(Syroot.Maths.Algebra.Clamp(v.nrm[vt].Y, -1, 1) * 511);
                                        int z = SingleToInt10(Syroot.Maths.Algebra.Clamp(v.nrm[vt].Z, -1, 1) * 511);
                                        int w = SingleToInt2(Syroot.Maths.Algebra.Clamp(0, 0, 1));
                                        writer.Write(x | (y << 10) | (z << 20) | (w << 30));
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unsupported  format for normals " + att.Format);
                                    }
                                }
                                if (att.Name == "_t0")
                                {
                                    if (att.Format == AttribFormat.Format_8_8_8_8_SNorm)
                                    {
                                        writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.tans[vt].X, -1, 1) * 127));
                                        writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.tans[vt].Y, -1, 1) * 127));
                                        writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.tans[vt].Z, -1, 1) * 127));
                                        writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.tans[vt].W, -1, 1) * 127));
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unsupported  format for tangents " + att.Format);
                                    }
                                }
                                if (att.Name == "_b0")
                                {
                                    if (att.Format == AttribFormat.Format_8_8_8_8_SNorm)
                                    {
                                        writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.bitans[vt].X, -1, 1) * 127));
                                        writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.bitans[vt].Y, -1, 1) * 127));
                                        writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.bitans[vt].Z, -1, 1) * 127));
                                        writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.bitans[vt].W, -1, 1) * 127));
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unsupported  format for bitans " + att.Format);
                                    }
                                }
                                if (att.Name == "_c0")
                                {
                                    if (att.Format == AttribFormat.Format_8_8_8_8_UNorm)
                                    {
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.col[vt].X, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.col[vt].Y, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.col[vt].Z, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.col[vt].W, 0, 1) * 255));
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unsupported  format for bitans " + att.Format);
                                    }
                                }
                                if (att.Name == "_u0")
                                {
                                    if (att.Format == AttribFormat.Format_16_16_UNorm)
                                    {
                                        writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv0[vt].X, 0, 1) * 65535));
                                        writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv0[vt].Y, 0, 1) * 65535));
                                    }
                                    else if (att.Format == AttribFormat.Format_32_32_Single)
                                    {
                                        writer.Write(v.uv0[vt].X);
                                        writer.Write(v.uv0[vt].Y);
                                    }
                                    else if (att.Format == AttribFormat.Format_16_16_Single)
                                    {
                                        Syroot.Maths.Vector2F value = new Syroot.Maths.Vector2F(v.uv0[vt].X, v.uv0[vt].Y);

                                        writer.Write((short)fromFloat(value.X));
                                        writer.Write((short)fromFloat(value.Y));
                                    }
                                    else if (att.Format == AttribFormat.Format_16_16_SNorm)
                                    {
                                        writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv0[vt].X, -1, 1) * 32767));
                                        writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv0[vt].Y, -1, 1) * 32767));
                                    }
                                    else if (att.Format == AttribFormat.Format_8_8_UNorm)
                                    {
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.uv0[vt].X, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.uv0[vt].Y, 0, 1) * 255));
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unsupported  format for uv0 " + att.Format);
                                    }
                                }
                                if (att.Name == "_u1")
                                {
                                    if (att.Format == AttribFormat.Format_16_16_UNorm)
                                    {
                                        writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv1[vt].X, 0, 1) * 65535));
                                        writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv1[vt].Y, 0, 1) * 65535));
                                    }
                                    else if (att.Format == AttribFormat.Format_32_32_Single)
                                    {
                                        writer.Write(v.uv1[vt].X);
                                        writer.Write(v.uv1[vt].Y);
                                    }
                                    else if (att.Format == AttribFormat.Format_16_16_Single)
                                    {
                                        Syroot.Maths.Vector2F value = new Syroot.Maths.Vector2F(v.uv0[vt].X, v.uv0[vt].Y);

                                        writer.Write((short)fromFloat(value.X));
                                        writer.Write((short)fromFloat(value.Y));
                                    }
                                    else if (att.Format == AttribFormat.Format_16_16_SNorm)
                                    {
                                        writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv1[vt].X, -1, 1) * 32767));
                                        writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv1[vt].Y, -1, 1) * 32767));
                                    }
                                    else if (att.Format == AttribFormat.Format_8_8_UNorm)
                                    {
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.uv1[vt].X, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.uv1[vt].Y, 0, 1) * 255));
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unsupported  format for uv1 " + att.Format);
                                    }
                                }
                                if (att.Name == "_u2")
                                {
                                    if (att.Format == AttribFormat.Format_16_16_UNorm)
                                    {
                                        writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv2[vt].X, 0, 1) * 65535));
                                        writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv2[vt].Y, 0, 1) * 65535));
                                    }
                                    else if (att.Format == AttribFormat.Format_32_32_Single)
                                    {
                                        writer.Write(v.uv2[vt].X);
                                        writer.Write(v.uv2[vt].Y);
                                    }
                                    else if (att.Format == AttribFormat.Format_16_16_Single)
                                    {
                                        Syroot.Maths.Vector2F value = new Syroot.Maths.Vector2F(v.uv0[vt].X, v.uv0[vt].Y);

                                        writer.Write((short)fromFloat(value.X));
                                        writer.Write((short)fromFloat(value.Y));
                                    }
                                    else if (att.Format == AttribFormat.Format_16_16_SNorm)
                                    {
                                        writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv2[vt].X, -1, 1) * 32767));
                                        writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv2[vt].Y, -1, 1) * 32767));
                                    }
                                    else if (att.Format == AttribFormat.Format_8_8_UNorm)
                                    {
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.uv2[vt].X, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.uv2[vt].Y, 0, 1) * 255));
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unsupported  format for uv2 " + att.Format);
                                    }
                                }
                                if (att.Name == "_i0")
                                {
                                    if (att.Format == AttribFormat.Format_8_8_8_8_UInt)
                                    {
                                        writer.Write((byte)v.nodes[vt].X);
                                        writer.Write((byte)v.nodes[vt].Y);
                                        writer.Write((byte)v.nodes[vt].Z);
                                        writer.Write((byte)v.nodes[vt].W);
                                    }
                                    else if (att.Format == AttribFormat.Format_8_8_UInt)
                                    {
                                        writer.Write((byte)v.nodes[vt].X);
                                        writer.Write((byte)v.nodes[vt].Y);
                                    }
                                    else if (att.Format == AttribFormat.Format_8_UInt)
                                    {
                                        writer.Write((byte)v.nodes[vt].X);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unsupported format for indices " + att.Format);
                                    }
                                }
                                if (att.Name == "_w0")
                                {
                                    if (att.Format == AttribFormat.Format_8_8_8_8_UNorm)
                                    {
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.weights[vt].X, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.weights[vt].Y, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.weights[vt].Z, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.weights[vt].W, 0, 1) * 255));
                                    }
                                    else if (att.Format == AttribFormat.Format_8_8_UNorm)
                                    {
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.weights[vt].X, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.weights[vt].Y, 0, 1) * 255));
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unsupported format for weights " + att.Format);
                                    }
                                }
                            }
                            at++;
                        }

                        s++;
                    }
                    mdl++;
                }


                foreach (ExternalFile ext in TargetSwitchBFRES.ExternalFiles)
                {
                    using (Syroot.BinaryData.BinaryDataReader reader = new Syroot.BinaryData.BinaryDataReader(new MemoryStream(ext.Data)))
                    {
                        char[] emchar = reader.ReadChars(4);

                        string em = new string(emchar);

                        if (em == "BNTX")
                        {
                            Console.WriteLine("Found BNTX");

                            writer.Seek((int)ext.ofsData, SeekOrigin.Begin);

                            writer.Write(BNTX.BNTXFile);

                            //Go back and write new size
                            //    writer.Seek((int)pos + 8, SeekOrigin.Begin);
                            //    writer.Write(BNTX.BNTXFile.Length);

                        }
                        reader.Close();
                    }
                }
                writer.Flush();
                writer.Close();
            }
        }
   

        //Thanks stack exchange. This is all temp till i do rebuilding of course
        public byte[] truncate(float f)
        {
            byte[] output = new byte[16];
            byte[] temp = BitConverter.GetBytes(f);
            for (int x = 0; x < 16; x++)
            {
                output[x] = temp[x];
            }
            return output;
        }

        // ignores the higher 16 bits
        public static float toFloat(int hbits)
        {
            int mant = hbits & 0x03ff;            // 10 bits mantissa
            int exp = hbits & 0x7c00;            // 5 bits exponent
            if (exp == 0x7c00)                   // NaN/Inf
                exp = 0x3fc00;                    // -> NaN/Inf
            else if (exp != 0)                   // normalized value
            {
                exp += 0x1c000;                   // exp - 15 + 127
                if (mant == 0 && exp > 0x1c400)  // smooth transition
                    return BitConverter.ToSingle(BitConverter.GetBytes((hbits & 0x8000) << 16
                                                    | exp << 13 | 0x3ff), 0);
            }
            else if (mant != 0)                  // && exp==0 -> subnormal
            {
                exp = 0x1c400;                    // make it normal
                do
                {
                    mant <<= 1;                   // mantissa * 2
                    exp -= 0x400;                 // decrease exp by 1
                } while ((mant & 0x400) == 0); // while not normal
                mant &= 0x3ff;                    // discard subnormal bit
            }                                     // else +/-0 -> +/-0
            return BitConverter.ToSingle(BitConverter.GetBytes(          // combine all parts
                (hbits & 0x8000) << 16          // sign  << ( 31 - 15 )
                | (exp | mant) << 13), 0);         // value << ( 23 - 10 )
        }
        // returns all higher 16 bits as 0 for all results
        public static int fromFloat(float fval)
        {
            int fbits = BitConverter.ToInt32(BitConverter.GetBytes(fval), 0);
            int sign = fbits >> 16 & 0x8000;          // sign only
            int val = (fbits & 0x7fffffff) + 0x1000; // rounded value

            if (val >= 0x47800000)               // might be or become NaN/Inf
            {                                     // avoid Inf due to rounding
                if ((fbits & 0x7fffffff) >= 0x47800000)
                {                                 // is or must become NaN/Inf
                    if (val < 0x7f800000)        // was value but too large
                        return sign | 0x7c00;     // make it +/-Inf
                    return sign | 0x7c00 |        // remains +/-Inf or NaN
                        (fbits & 0x007fffff) >> 13; // keep NaN (and Inf) bits
                }
                return sign | 0x7bff;             // unrounded not quite Inf
            }
            if (val >= 0x38800000)               // remains normalized value
                return sign | val - 0x38000000 >> 13; // exp - 127 + 15
            if (val < 0x33000000)                // too small for subnormal
                return sign;                      // becomes +/-0
            val = (fbits & 0x7fffffff) >> 23;  // tmp exp for subnormal calc
            return sign | ((fbits & 0x7fffff | 0x800000) // add subnormal bit
                 + (0x800000 >> val - 102)     // round depending on cut off
              >> 126 - val);   // div by 2^(1-(exp-127+15)) and >> 13 | exp=0
        }

        private static int SingleToInt2(float value)
        {
            if (value < -1 || value > 1)
            {
                throw new ArgumentException($"{value} cannot be converted to Int2 (exceeds range -1 to 1).",
                    nameof(value));
            }
            return (int)(((uint)value << 30) >> 30) & 0b00000000_00000000_00000000_00000011;
        }

        private static int SingleToInt10(float value)
        {
            if (value < -512 || value > 511)
            {
                throw new ArgumentException($"{value} cannot be converted to Int10 (exceeds range -512 to 511).",
                    nameof(value));
            }
            return (int)(((uint)value << 22) >> 22) & 0b00000000_00000000_00000011_11111111;
        }

        #endregion
    }
}