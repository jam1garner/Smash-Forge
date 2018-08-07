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

            AnimationCountTotal = TargetSwitchBFRES.SkeletalAnims.Count
                + TargetSwitchBFRES.BoneVisibilityAnims.Count
                + TargetSwitchBFRES.MaterialAnims.Count
                + TargetSwitchBFRES.ShapeAnims.Count;

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

                foreach (int node in model.Node_Array)
                {
                    Console.WriteLine(model.skeleton.bones[node].Text);
                }

                //MeshTime!!
                foreach (Shape shp in mdl.Shapes)
                {
                    Mesh poly = new Mesh();
                    poly.Text = shp.Name;
                    poly.MaterialIndex = shp.MaterialIndex;
                    poly.VertexSkinCount = shp.VertexSkinCount;
                    poly.boneIndx = shp.BoneIndex;
                    poly.fmdlIndx = ModelCur;

                    foreach (int bn in shp.SkinBoneIndices)
                    {
                        poly.BoneIndexList.Add(model.skeleton.bones[bn].Text, bn);
                    }

                    TModels.Nodes[ModelCur].Nodes.Add(poly);


                    ReadVertexBuffer(mdl, shp, poly, model);

                    poly.BoundingCount = shp.SubMeshBoundings.Count;

                    int CurLOD = 0;
                    foreach (var lod in shp.Meshes)
                    {
                        Mesh.LOD_Mesh lodmsh = new Mesh.LOD_Mesh();
                        lodmsh.index = CurLOD++;

                        uint FaceCount = lod.IndexCount;
                        uint[] indicesArray = lod.GetIndices().ToArray();


                        for (int face = 0; face < FaceCount; face++)
                            lodmsh.faces.Add((int)indicesArray[face] + (int)lod.FirstVertex);

                        poly.lodMeshes.Add(lodmsh);
                    }

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

                    MaterialData.ShaderAssign shaderassign = new MaterialData.ShaderAssign();


                    shaderassign.ShaderModel = mat.ShaderAssign.ShadingModelName;
                    shaderassign.ShaderArchive = mat.ShaderAssign.ShaderArchiveName;

                    int o = 0;
                    foreach (var op in mat.ShaderAssign.ShaderOptionDict)
                    {
                        shaderassign.options.Add(op.Key, mat.ShaderAssign.ShaderOptions[o]);
                        o++;
                    }
                    int sa = 0;
                    foreach (var smp in mat.ShaderAssign.SamplerAssignDict)
                    {
                 //       Console.WriteLine($"{smp.Key} ---> {mat.ShaderAssign.SamplerAssigns[sa]}");
                        shaderassign.samplers.Add(smp.Key, mat.ShaderAssign.SamplerAssigns[sa]);
                        sa++;
                    }
            
                    int va = 0;
                    foreach (var att in mat.ShaderAssign.AttribAssignDict)
                    {
                        shaderassign.attributes.Add(att.Key, mat.ShaderAssign.AttribAssigns[va]);
                        va++;
                    }

                    poly.material.shaderassign = shaderassign;

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
            model.skeleton.Text = "Skeleton";
            model.Nodes.Add(model.skeleton);

            foreach (TreeNode nod in model.skeleton.Nodes)
            {
                if (nod.Text == "model.sb")
                {
                    nod.Remove();
                }
            }
        }
        private void ReadVertexBuffer(Model mdl, Shape shp, Mesh poly, FMDL_Model model)
        {
            //Create a buffer instance which stores all the buffer data
            VertexBufferHelper helper = new VertexBufferHelper(mdl.VertexBuffers[shp.VertexBufferIndex], TargetSwitchBFRES.ByteOrder);

            //Set each array first from the lib if exist. Then add the data all in one loop
            Syroot.Maths.Vector4F[] vec4Positions = new Syroot.Maths.Vector4F[0];
            Syroot.Maths.Vector4F[] vec4Normals = new Syroot.Maths.Vector4F[0];
            Syroot.Maths.Vector4F[] vec4uv0 = new Syroot.Maths.Vector4F[0];
            Syroot.Maths.Vector4F[] vec4uv1 = new Syroot.Maths.Vector4F[0];
            Syroot.Maths.Vector4F[] vec4uv2 = new Syroot.Maths.Vector4F[0];
            Syroot.Maths.Vector4F[] vec4c0 = new Syroot.Maths.Vector4F[0];
            Syroot.Maths.Vector4F[] vec4t0 = new Syroot.Maths.Vector4F[0];
            Syroot.Maths.Vector4F[] vec4b0 = new Syroot.Maths.Vector4F[0];
            Syroot.Maths.Vector4F[] vec4w0 = new Syroot.Maths.Vector4F[0];
            Syroot.Maths.Vector4F[] vec4i0 = new Syroot.Maths.Vector4F[0];

            //For shape morphing
            Syroot.Maths.Vector4F[] vec4Positions1 = new Syroot.Maths.Vector4F[0];
            Syroot.Maths.Vector4F[] vec4Positions2 = new Syroot.Maths.Vector4F[0];

            foreach (VertexAttrib att in mdl.VertexBuffers[shp.VertexBufferIndex].Attributes)
            {
                Mesh.VertexAttribute attr = new Mesh.VertexAttribute();
                attr.Name = att.Name;
                attr.Format = att.Format;

                if (att.Name == "_p0")
                    vec4Positions = AttributeData(att, helper, "_p0");
                if (att.Name == "_n0")
                    vec4Normals = AttributeData(att, helper, "_n0");
                if (att.Name == "_u0")
                    vec4uv0 = AttributeData(att, helper, "_u0");
                if (att.Name == "_u1")
                    vec4uv1 = AttributeData(att, helper, "_u1");
                if (att.Name == "_u2")
                    vec4uv2 = AttributeData(att, helper, "_u2");
                if (att.Name == "_c0")
                    vec4c0 = AttributeData(att, helper, "_c0");
                if (att.Name == "_t0")
                    vec4t0 = AttributeData(att, helper, "_t0");
                if (att.Name == "_b0")
                    vec4b0 = AttributeData(att, helper, "_b0");
                if (att.Name == "_w0")
                    vec4w0 = AttributeData(att, helper, "_w0");
                if (att.Name == "_i0")
                    vec4i0 = AttributeData(att, helper, "_i0");

                if (att.Name == "_p1")
                    vec4Positions1 = AttributeData(att, helper, "_p1");
                if (att.Name == "_p2")
                    vec4Positions2 = AttributeData(att, helper, "_p2");

                poly.vertexAttributes.Add(attr);
            }
            for (int i = 0; i < vec4Positions.Length; i++)
            {
                Vertex v = new Vertex();
                if (vec4Positions.Length > 0)
                    v.pos = new Vector3(vec4Positions[i].X, vec4Positions[i].Y, vec4Positions[i].Z);
                if (vec4Positions1.Length > 0)
                    v.pos1 = new Vector3(vec4Positions1[i].X, vec4Positions1[i].Y, vec4Positions1[i].Z);
                if (vec4Positions2.Length > 0)
                    v.pos2 = new Vector3(vec4Positions2[i].X, vec4Positions2[i].Y, vec4Positions2[i].Z);
                if (vec4Normals.Length > 0)
                    v.nrm = new Vector3(vec4Normals[i].X, vec4Normals[i].Y, vec4Normals[i].Z);
                if (vec4uv0.Length > 0)
                    v.uv0 = new Vector2(vec4uv0[i].X, vec4uv0[i].Y);
                if (vec4uv1.Length > 0)
                    v.uv1 = new Vector2(vec4uv1[i].X, vec4uv1[i].Y);
                if (vec4uv2.Length > 0)
                    v.uv2 = new Vector2(vec4uv2[i].X, vec4uv2[i].Y);
                if (vec4w0.Length > 0)
                {
                        v.boneWeights.Add(vec4w0[i].X);
                        v.boneWeights.Add(vec4w0[i].Y);
                        v.boneWeights.Add(vec4w0[i].Z);
                        v.boneWeights.Add(vec4w0[i].W);
                }
                if (vec4i0.Length > 0)
                {
                        v.boneIds.Add((int)vec4i0[i].X);
                        v.boneIds.Add((int)vec4i0[i].Y);
                        v.boneIds.Add((int)vec4i0[i].Z);
                        v.boneIds.Add((int)vec4i0[i].W);
                }

                if (vec4t0.Length > 0)
                    v.tan = new Vector4(vec4t0[i].X, vec4t0[i].Y, vec4t0[i].Z, vec4t0[i].W);
                if (vec4b0.Length > 0)
                    v.bitan = new Vector4(vec4b0[i].X, vec4b0[i].Y, vec4b0[i].Z, vec4b0[i].W);
                if (vec4c0.Length > 0)
                    v.col = new Vector4(vec4c0[i].X, vec4c0[i].Y, vec4c0[i].Z, vec4c0[i].W);

                if (poly.VertexSkinCount == 1)
                {
                    Matrix4 sb = model.skeleton.bones[model.Node_Array[v.boneIds[0]]].transform;
                  //  Console.WriteLine(model.skeleton.bones[model.Node_Array[v.boneIds[0]]].Text);
                    v.pos = Vector3.TransformPosition(v.pos, sb);
                    v.nrm = Vector3.TransformNormal(v.nrm, sb);
                }
                if (poly.VertexSkinCount == 0)
                {
                    Matrix4 NoBindFix = model.skeleton.bones[poly.boneIndx].transform;
                    v.pos = Vector3.TransformPosition(v.pos, NoBindFix);
                    v.nrm = Vector3.TransformNormal(v.nrm, NoBindFix);
                }

                poly.vertices.Add(v);
            }
        }
        private static Syroot.Maths.Vector4F[] AttributeData(VertexAttrib att, VertexBufferHelper helper, string attName)
        {
            VertexBufferHelperAttrib attd = helper[attName];
            return attd.Data;
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
                texture.wrapModeW = (int)mat.Samplers[id].WrapModeW;
                texture.SamplerName = mat.SamplerDict.Keys.ElementAt(id);
        /*        try
                {
                    Console.WriteLine(poly.material.shaderassign.samplers[texture.SamplerName]);
                    texture.FragShaderSampler = poly.material.shaderassign.samplers[texture.SamplerName];
                }
                catch
                {

                }*/



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
                    if (AlbedoCount == 1)
                    {
                     //   poly.material.HasDiffuseLayer = true;
                    //    texture.hash = 19;
                     //   texture.Type = MatTexture.TextureType.DiffuseLayer2;

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
                else if (TextureName.Contains("b00") || TextureName.Contains("Moc") || TextureName.Contains("AO"))
                {
                    //AO and shadow can use the same sampler. Shader options will determine which channels to use 
                    texture.hash = 2;
                    poly.material.HasShadowMap = true;
                    texture.Type = MatTexture.TextureType.Shadow;
                }
                else if (TextureName.Contains("b01"))
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

                //Now determine the types by sampler from fragment shader
                //a0 and a1 will not be defined this way however because KSA using only those in no particular order
                if (texture.FragShaderSampler == "_t0")
                    poly.material.HasTransparencyMap = true;
                if (texture.FragShaderSampler == "_e0")
                    poly.material.HasEmissionMap = true;
                if (texture.FragShaderSampler == "_e1")
                    poly.material.HasEmissionMap = true;
                if (texture.FragShaderSampler == "_b0")
                    poly.material.HasShadowMap = true;
                if (texture.FragShaderSampler == "_b1")
                    poly.material.HasLightMap = true;
                if (texture.FragShaderSampler == "_n0")
                    poly.material.HasNormalMap = true;
                if (texture.FragShaderSampler == "_s0")
                    poly.material.HasSpecularMap = true;

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
                r.Type = (Syroot.NintenTools.Bfres.RenderInfoType)rnd.Type;

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
                    prm.Name = param.Name;

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
                        case ShaderParamType.UInt:
                            prm.Value_UInt = reader.ReadUInt32();
                            break;
                        case ShaderParamType.Bool:
                            prm.Value_Bool = reader.ReadBoolean();
                            break;
                        case ShaderParamType.Float4x4:
                            prm.Value_float4x4 = reader.ReadSingles(16);
                            break;
                    }
                    poly.material.matparam.Add(param.Name, prm);
                }
                reader.Close();
            }
        }


        #region HackyInject (No rebuild)

        private IDictionary<object, BlockEntry> _savedBlocks;
        public bool SaveShaderParam = true;

        public void InjectToFile(string FileName)
        {
            //Reparse and inject buffer data

            using (Syroot.BinaryData.BinaryDataReader reader = new Syroot.BinaryData.BinaryDataReader(new MemoryStream(BFRESFile)))
            using (Syroot.BinaryData.BinaryDataWriter writer = new Syroot.BinaryData.BinaryDataWriter(File.Open(FileName, FileMode.Create)))
            {
                writer.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
                reader.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;

                BufferInjection(writer, reader);
                BNTXInjectionSameSize(writer, reader);
            //    FSKAInjection(writer);
                FSKLInjection(writer);

                int mdl = 0;
                foreach (Model fmdl in TargetSwitchBFRES.Models)
                {
                    int s = 0;
                    foreach (Shape shp in fmdl.Shapes)
                    {
                        byte[] data = fmdl.Materials[shp.MaterialIndex].ShaderParamData;
                        byte[] NewParamData = WriteShaderParams(data, models[mdl].poly[s]);

                        if (SaveShaderParam == true)
                        {
                            writer.Seek(fmdl.Materials[shp.MaterialIndex].SourceParamOffset, SeekOrigin.Begin);
                            writer.Write(NewParamData);
                        }

                        s++;
                    }
                    mdl++;
                }

                writer.Flush();
                writer.Close();
            }
        }
        public void BufferInjection(Syroot.BinaryData.BinaryDataWriter writer, Syroot.BinaryData.BinaryDataReader reader)
        {
            writer.Write(BFRESFile);

            int mdl = 0;
            foreach (Model fmdl in TargetSwitchBFRES.Models)
            {
                int s = 0;
                foreach (Shape shp in fmdl.Shapes)
                {
                    Mesh msh = models[mdl].poly[s];


                    writer.Seek(shp.Meshes[0].DataOffset, SeekOrigin.Begin);

                    int[] Faces = msh.lodMeshes[msh.DisplayLODIndex].getDisplayFace().ToArray();

                    foreach (int f in Faces)
                    {
                        if (shp.Meshes[0].IndexFormat == IndexFormat.UInt16)
                            writer.Write((ushort)f);
                        if (shp.Meshes[0].IndexFormat == IndexFormat.UInt32)
                            writer.Write((uint)f);
                    }

                    foreach (VertexAttrib att in fmdl.VertexBuffers[shp.VertexBufferIndex].Attributes)
                    {
                        int test = (int)fmdl.VertexBuffers[shp.VertexBufferIndex].Buffers[att.BufferIndex].DataOffset + att.Offset + fmdl.VertexBuffers[shp.VertexBufferIndex].Buffers[att.BufferIndex].Stride;
                    }

                    for (int vt = 0; vt < fmdl.VertexBuffers[shp.VertexBufferIndex].VertexCount; vt++)
                    {
                        if (vt < msh.vertices.Count)
                        {
                       
                        }
                        else
                        {
                            Vertex vtx = new Vertex();

                            vtx.pos = new Vector3(0);
                            vtx.nrm = new Vector3(0);
                            vtx.col = new Vector4(1);
                            vtx.uv0 = new Vector2(0);
                            vtx.uv1 = new Vector2(0);
                            vtx.uv2 = new Vector2(0);
                            vtx.boneWeights.Add(1);
                            vtx.boneIds.Add(0);
                            vtx.tan = new Vector4(0);
                            vtx.bitan = new Vector4(0);

                            msh.vertices.Add(vtx);
                        }
                        Vertex v = msh.vertices[vt];

                        int at = 0;
                        foreach (VertexAttrib att in fmdl.VertexBuffers[shp.VertexBufferIndex].Attributes)
                        {
                            writer.Seek(fmdl.VertexBuffers[shp.VertexBufferIndex].Buffers[att.BufferIndex].DataOffset + att.Offset + fmdl.VertexBuffers[shp.VertexBufferIndex].Buffers[att.BufferIndex].Stride * vt, SeekOrigin.Begin);
                            if (att.Name == "_p0")
                            {
                                if (att.Format == AttribFormat.Format_32_32_32_Single)
                                {
                                    writer.Write(v.pos.X);
                                    writer.Write(v.pos.Y);
                                    writer.Write(v.pos.Z);
                                }
                                else if (att.Format == AttribFormat.Format_16_16_16_16_Single)
                                {
                                    Syroot.Maths.Vector4F value = new Syroot.Maths.Vector4F(v.pos.X, v.pos.Y, v.pos.Z, 0);

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
                                    int x = SingleToInt10(Syroot.Maths.Algebra.Clamp(v.nrm.X, -1, 1) * 511);
                                    int y = SingleToInt10(Syroot.Maths.Algebra.Clamp(v.nrm.Y, -1, 1) * 511);
                                    int z = SingleToInt10(Syroot.Maths.Algebra.Clamp(v.nrm.Z, -1, 1) * 511);
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
                                    writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.tan.X, -1, 1) * 127));
                                    writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.tan.Y, -1, 1) * 127));
                                    writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.tan.Z, -1, 1) * 127));
                                    writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.tan.W, -1, 1) * 127));
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
                                    writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.bitan.X, -1, 1) * 127));
                                    writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.bitan.Y, -1, 1) * 127));
                                    writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.bitan.Z, -1, 1) * 127));
                                    writer.Write((sbyte)(Syroot.Maths.Algebra.Clamp(v.bitan.W, -1, 1) * 127));
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
                                    writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.col.X, 0, 1) * 255));
                                    writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.col.Y, 0, 1) * 255));
                                    writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.col.Z, 0, 1) * 255));
                                    writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.col.W, 0, 1) * 255));
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
                                    writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv0.X, 0, 1) * 65535));
                                    writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv0.Y, 0, 1) * 65535));
                                }
                                else if (att.Format == AttribFormat.Format_32_32_Single)
                                {
                                    writer.Write(v.uv0.X);
                                    writer.Write(v.uv0.Y);
                                }
                                else if (att.Format == AttribFormat.Format_16_16_Single)
                                {
                                    Syroot.Maths.Vector2F value = new Syroot.Maths.Vector2F(v.uv0.X, v.uv0.Y);

                                    writer.Write((short)fromFloat(value.X));
                                    writer.Write((short)fromFloat(value.Y));
                                }
                                else if (att.Format == AttribFormat.Format_16_16_SNorm)
                                {
                                    writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv0.X, -1, 1) * 32767));
                                    writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv0.Y, -1, 1) * 32767));
                                }
                                else if (att.Format == AttribFormat.Format_8_8_UNorm)
                                {
                                    writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.uv0.X, 0, 1) * 255));
                                    writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.uv0.Y, 0, 1) * 255));
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
                                    writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv1.X, 0, 1) * 65535));
                                    writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv1.Y, 0, 1) * 65535));
                                }
                                else if (att.Format == AttribFormat.Format_32_32_Single)
                                {
                                    writer.Write(v.uv1.X);
                                    writer.Write(v.uv1.Y);
                                }
                                else if (att.Format == AttribFormat.Format_16_16_Single)
                                {
                                    Syroot.Maths.Vector2F value = new Syroot.Maths.Vector2F(v.uv0.X, v.uv0.Y);

                                    writer.Write((short)fromFloat(value.X));
                                    writer.Write((short)fromFloat(value.Y));
                                }
                                else if (att.Format == AttribFormat.Format_16_16_SNorm)
                                {
                                    writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv1.X, -1, 1) * 32767));
                                    writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv1.Y, -1, 1) * 32767));
                                }
                                else if (att.Format == AttribFormat.Format_8_8_UNorm)
                                {
                                    writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.uv1.X, 0, 1) * 255));
                                    writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.uv1.Y, 0, 1) * 255));
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
                                    writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv2.X, 0, 1) * 65535));
                                    writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv2.Y, 0, 1) * 65535));
                                }
                                else if (att.Format == AttribFormat.Format_32_32_Single)
                                {
                                    writer.Write(v.uv2.X);
                                    writer.Write(v.uv2.Y);
                                }
                                else if (att.Format == AttribFormat.Format_16_16_Single)
                                {
                                    Syroot.Maths.Vector2F value = new Syroot.Maths.Vector2F(v.uv0.X, v.uv0.Y);

                                    writer.Write((short)fromFloat(value.X));
                                    writer.Write((short)fromFloat(value.Y));
                                }
                                else if (att.Format == AttribFormat.Format_16_16_SNorm)
                                {
                                    writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv2.X, -1, 1) * 32767));
                                    writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv2.Y, -1, 1) * 32767));
                                }
                                else if (att.Format == AttribFormat.Format_8_8_UNorm)
                                {
                                    writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.uv2.X, 0, 1) * 255));
                                    writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.uv2.Y, 0, 1) * 255));
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
                                    writer.Write(v.boneIds.Count > 0 ? (byte)v.boneIds[0] : (byte)0);
                                    writer.Write(v.boneIds.Count > 1 ? (byte)v.boneIds[1] : (byte)0);
                                    writer.Write(v.boneIds.Count > 2 ? (byte)v.boneIds[2] : (byte)0);
                                    writer.Write(v.boneIds.Count > 3 ? (byte)v.boneIds[3] : (byte)0);
                                }
                                else if (att.Format == AttribFormat.Format_8_8_UInt)
                                {
                                    writer.Write(v.boneIds.Count > 0 ? (byte)v.boneIds[0] : (byte)0);
                                    writer.Write(v.boneIds.Count > 1 ? (byte)v.boneIds[1] : (byte)0);
                                }
                                else if (att.Format == AttribFormat.Format_8_UInt)
                                {
                                    writer.Write(v.boneIds.Count > 0 ? (byte)v.boneIds[0] : (byte)0);
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
                                    writer.Write(v.boneWeights.Count > 0 ? (byte)(Syroot.Maths.Algebra.Clamp(v.boneWeights[0], 0, 1) * 255) : (byte)0);
                                    writer.Write(v.boneWeights.Count > 1 ? (byte)(Syroot.Maths.Algebra.Clamp(v.boneWeights[1], 0, 1) * 255) : (byte)0);
                                    writer.Write(v.boneWeights.Count > 2 ? (byte)(Syroot.Maths.Algebra.Clamp(v.boneWeights[2], 0, 1) * 255) : (byte)0);
                                    writer.Write(v.boneWeights.Count > 3 ? (byte)(Syroot.Maths.Algebra.Clamp(v.boneWeights[3], 0, 1) * 255) : (byte)0);

                                }
                                else if (att.Format == AttribFormat.Format_8_8_UNorm)
                                {
                                    writer.Write(v.boneWeights.Count > 0 ? (byte)(Syroot.Maths.Algebra.Clamp(v.boneWeights[0], 0, 1) * 255) : (byte)0);
                                    writer.Write(v.boneWeights.Count > 1 ? (byte)(Syroot.Maths.Algebra.Clamp(v.boneWeights[1], 0, 1) * 255) : (byte)0);
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
        }
        public void FSKLInjection(Syroot.BinaryData.BinaryDataWriter writer)
        {
            int mdl = 0;
            foreach (Model fmdl in TargetSwitchBFRES.Models)
            {
                int CurBn = 0;
                foreach (Syroot.NintenTools.NSW.Bfres.Bone bn in fmdl.Skeleton.Bones)
                {
                    Bone bone = models[mdl].skeleton.bones[CurBn];

                    writer.Seek(bn.SRTPos, SeekOrigin.Begin);
                    writer.Write(bone.scale[0]);
                    writer.Write(bone.scale[1]);
                    writer.Write(bone.scale[2]);
                    writer.Write(bone.rotation[0]);
                    writer.Write(bone.rotation[1]);
                    writer.Write(bone.rotation[2]);
                    writer.Write(bone.rotation[3]);
                    writer.Write(bone.position[0]);
                    writer.Write(bone.position[1]);
                    writer.Write(bone.position[2]);
                    CurBn++;
                }
                mdl++;
            }
        }

        public void BNTXInjectionSameSize(Syroot.BinaryData.BinaryDataWriter writer, Syroot.BinaryData.BinaryDataReader reader)
        {

            foreach (ExternalFile ext in TargetSwitchBFRES.ExternalFiles)
            {
                reader.Seek(ext.ofsData, SeekOrigin.Begin);
                char[] emchar = reader.ReadChars(4);

                string em = new string(emchar);


                if (em == "BNTX")
                {
                    Console.WriteLine("Found BNTX");

                    long OriginalBNTXSize = ext.sizData;
                    long InjectedBNTXSize = BNTX.BNTXFile.Length;

                    if (OriginalBNTXSize == InjectedBNTXSize)
                    {
                        writer.Seek((int)ext.ofsData, SeekOrigin.Begin);
                        writer.Write(BNTX.BNTXFile);
                    }
                    else
                    {
                        MessageBox.Show("BNTX Is too big or small! Must be original Size!");
                    }
                }
            }
        }
        public byte[] WriteShaderParams(byte[] data, Mesh m)
        {
            //Write data to this byte array. 
            using (Syroot.BinaryData.BinaryDataWriter writer = new Syroot.BinaryData.BinaryDataWriter(new MemoryStream(data)))
            {
                writer.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
                foreach (var prm in m.material.matparam.Values)
                {
                    switch (prm.Type)
                    {
                        case Syroot.NintenTools.Bfres.ShaderParamType.Float:
                            writer.Write(prm.Value_float);
                            break;
                        case Syroot.NintenTools.Bfres.ShaderParamType.Float2:
                            writer.Write(prm.Value_float2.X);
                            writer.Write(prm.Value_float2.Y);
                            break;
                        case Syroot.NintenTools.Bfres.ShaderParamType.Float3:
                            writer.Write(prm.Value_float3.X);
                            writer.Write(prm.Value_float3.Y);
                            writer.Write(prm.Value_float3.Z);
                            break;
                        case Syroot.NintenTools.Bfres.ShaderParamType.Float4:
                            writer.Write(prm.Value_float4.X);
                            writer.Write(prm.Value_float4.Y);
                            writer.Write(prm.Value_float4.Z);
                            writer.Write(prm.Value_float4.W);
                            break;
                        case Syroot.NintenTools.Bfres.ShaderParamType.TexSrt:
                            writer.Write(prm.Value_TexSrt.Mode);
                            writer.Write(prm.Value_TexSrt.scale.X);
                            writer.Write(prm.Value_TexSrt.scale.Y);
                            writer.Write(prm.Value_TexSrt.rotate);
                            writer.Write(prm.Value_TexSrt.translate.X);
                            writer.Write(prm.Value_TexSrt.translate.Y);
                            break;
                        case Syroot.NintenTools.Bfres.ShaderParamType.Float4x4:
                            foreach (float f in prm.Value_float4x4)
                            {
                                writer.Write(f);
                            }
                            break;
                        case Syroot.NintenTools.Bfres.ShaderParamType.UInt:
                            writer.Write(prm.Value_UInt);
                            break;
                        default:
                            MessageBox.Show("Format not added to shader param saving " + prm.Type);
                            MessageBox.Show("Shader param will not save!");
                            SaveShaderParam = false;
                            break;

                    }
                }
                return data;
            }
        }

        private class BlockEntry
        {
            internal List<uint> Offsets;
            internal uint Alignment;
            internal Action Callback;

            internal BlockEntry(uint alignment, Action callback)
            {
                Alignment = alignment;
                Callback = callback;
            }
        }

        internal void SaveBlock(object data, uint alignment, Action callback)
        {
            if (data == null)
            {
                return;
            }
            else
            {
                _savedBlocks.Add(data, new BlockEntry(alignment, callback));
            }
        }

        public void WriteBlocks(Syroot.BinaryData.BinaryDataWriter writer)
        {
            foreach (KeyValuePair<object, BlockEntry> entry in _savedBlocks)
            {
                if (entry.Value.Alignment != 0) writer.Align((int)entry.Value.Alignment);

                // Write the data.
                entry.Value.Callback.Invoke();
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
            return (int)(((uint)value << 30) >> 30) & 0x3;
        }

        private static int SingleToInt10(float value)
        {
            if (value < -512 || value > 511)
            {
                throw new ArgumentException($"{value} cannot be converted to Int10 (exceeds range -512 to 511).",
                    nameof(value));
            }
            return (int)(((uint)value << 22) >> 22) & 0x3FF;
        }

        #endregion
    }
}