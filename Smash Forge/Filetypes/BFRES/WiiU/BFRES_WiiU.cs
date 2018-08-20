using OpenTK;
using OpenTK.Graphics.OpenGL;
using Syroot.NintenTools.Bfres;
using Syroot.NintenTools.Bfres.GX2;
using Syroot.NintenTools.Bfres.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Syroot.NintenTools.Yaz0;

namespace Smash_Forge
{
    public partial class BFRES : TreeNode
    {
        public void Read(ResFile TargetWiiUBFRES)
        {
            Nodes.Add(TModels);
            Nodes.Add(TTextures);
            Nodes.Add(TShaderparam);
            Nodes.Add(TColoranim);
            Nodes.Add(TTextureSRT);
            Nodes.Add(TTexturePat);
            Nodes.Add(TBonevisabilty);
            Nodes.Add(TVisualAnim);
            Nodes.Add(TShapeAnim);
            Nodes.Add(TSceneAnim);
            Nodes.Add(TEmbedded);
            ImageKey = "bfres";
            SelectedImageKey = "bfres";

            FSKACount = TargetWiiUBFRES.SkeletalAnims.Count;
            FTXPCount = TargetWiiUBFRES.TexPatternAnims.Count;
            FSHUCount = TargetWiiUBFRES.ColorAnims.Count + TargetWiiUBFRES.TexSrtAnims.Count + TargetWiiUBFRES.ShaderParamAnims.Count;

            AnimationCountTotal = FSKACount + FTXPCount + FSHUCount;

            FTEXContainer = new FTEXContainer();
            foreach (Texture tex in TargetWiiUBFRES.Textures.Values)
            {
                string TextureName = tex.Name;
                FTEX texture = new FTEX();
                texture.ReadFTEX(tex);
         
                TTextures.Nodes.Add(texture);

                FTEXContainer.FTEXtextures.Add(texture.Text, texture);
            }
            Runtime.FTEXContainerList.Add(FTEXContainer);

            int ModelCur = 0;
            //FMDLs -Models-
            foreach (Model mdl in TargetWiiUBFRES.Models.Values)
            {
                FMDL_Model model = new FMDL_Model(); //This will store VBN data and stuff
                model.Text = mdl.Name;

                TModels.Nodes.Add(model);

                model.Node_Array = new int[mdl.Skeleton.MatrixToBoneList.Count];
                int nodes = 0;
                foreach (ushort node in mdl.Skeleton.MatrixToBoneList)
                {
                    model.Node_Array[nodes] = node;
                    nodes++;
                }

                foreach (Syroot.NintenTools.Bfres.Bone bn in mdl.Skeleton.Bones.Values)
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
                model.skeleton.reset();
                model.skeleton.update();

                //MeshTime!!
                int ShapeCur = 0;
                foreach (Shape shp in mdl.Shapes.Values)
                {
                    Mesh poly = new Mesh();
                    poly.Text = shp.Name;
                    poly.MaterialIndex = shp.MaterialIndex;
                    poly.VertexSkinCount = shp.VertexSkinCount;
                    poly.boneIndx = shp.BoneIndex;
                    poly.fmdlIndx = ModelCur;

                    foreach (int bn in shp.SkinBoneIndices)
                    {
                        if (!poly.BoneIndexList.ContainsKey(model.skeleton.bones[bn].Text))
                            poly.BoneIndexList.Add(model.skeleton.bones[bn].Text, bn);
                    }

                    TModels.Nodes[ModelCur].Nodes.Add(poly);

                    //Create a buffer instance which stores all the buffer data
                    VertexBufferHelper helper = new VertexBufferHelper(mdl.VertexBuffers[shp.VertexBufferIndex], TargetWiiUBFRES.ByteOrder);

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


                    foreach (VertexAttrib att in mdl.VertexBuffers[shp.VertexBufferIndex].Attributes.Values)
                    {
                        Mesh.VertexAttribute attr = new Mesh.VertexAttribute();
                        attr.Name = att.Name;
                       // attr.Format = att.Format;

                        if (att.Name == "_p0")
                            vec4Positions = WiiUAttributeData(att, helper, "_p0");
                        if (att.Name == "_n0")
                            vec4Normals = WiiUAttributeData(att, helper, "_n0");
                        if (att.Name == "_u0")
                            vec4uv0 = WiiUAttributeData(att, helper, "_u0");
                        if (att.Name == "_u1")
                            vec4uv1 = WiiUAttributeData(att, helper, "_u1");
                        if (att.Name == "_u2")
                            vec4uv2 = WiiUAttributeData(att, helper, "_u2");
                        if (att.Name == "_c0")
                            vec4c0 = WiiUAttributeData(att, helper, "_c0");
                        if (att.Name == "_t0")
                            vec4t0 = WiiUAttributeData(att, helper, "_t0");
                        if (att.Name == "_b0")
                            vec4b0 = WiiUAttributeData(att, helper, "_b0");
                        if (att.Name == "_w0")
                            vec4w0 = WiiUAttributeData(att, helper, "_w0");
                        if (att.Name == "_i0")
                            vec4i0 = WiiUAttributeData(att, helper, "_i0");

                        poly.vertexAttributes.Add(attr);
                    }
                    for (int i = 0; i < vec4Positions.Length; i++)
                    {
                        Vertex v = new Vertex();
                        if (vec4Positions.Length > 0)
                            v.pos = new Vector3(vec4Positions[i].X, vec4Positions[i].Y, vec4Positions[i].Z);
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

                    //shp.Meshes.Count - 1 //For going to the lowest poly LOD mesh


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

                    int SampIndex = 0;
                    foreach (var smp in mat.Samplers)
                    {
                        poly.material.Samplers.Add(smp.Key, SampIndex);
                        SampIndex++;
                    }

                    int AlbedoCount = 0;

                    string TextureName = "";

                    MaterialData.ShaderAssign shaderassign = new MaterialData.ShaderAssign();

                    if (mat.ShaderAssign != null) //Some special cases (env models) have none
                    {
                        shaderassign.ShaderModel = mat.ShaderAssign.ShadingModelName;
                        shaderassign.ShaderArchive = mat.ShaderAssign.ShaderArchiveName;


                        int o = 0;
                        foreach (var op in mat.ShaderAssign.ShaderOptions)
                        {
                            shaderassign.options.Add(op.Key, mat.ShaderAssign.ShaderOptions[o]);
                            o++;
                        }

                        int sa = 0;
                        foreach (var smp in mat.ShaderAssign.SamplerAssigns)
                        {
                            shaderassign.samplers.Add(smp.Key, mat.ShaderAssign.SamplerAssigns[sa]);
                            sa++;
                        }

                        int va = 0;
                        foreach (var att in mat.ShaderAssign.AttribAssigns)
                        {
                            shaderassign.attributes.Add(att.Key, mat.ShaderAssign.AttribAssigns[va]);
                            va++;
                        }
                    }

                    poly.material.shaderassign = shaderassign;

                    int id = 0;
                    foreach (TextureRef tex in mdl.Materials[shp.MaterialIndex].TextureRefs)
                    {
                        TextureName = tex.Name;

                        MatTexture texture = new MatTexture();

                        texture.wrapModeS = (int)mdl.Materials[shp.MaterialIndex].Samplers[id].TexSampler.ClampX;
                        texture.wrapModeT = (int)mdl.Materials[shp.MaterialIndex].Samplers[id].TexSampler.ClampY;


                        bool IsAlbedo = HackyTextureList.Any(TextureName.Contains);

                        if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_a0")
                        {
                            poly.material.HasDiffuseMap = true;
                            texture.hash = 0;
                            texture.Type = MatTexture.TextureType.Diffuse;
                        }
                        if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_a1")
                        {
                            poly.material.HasDiffuseLayer = true;
                            texture.hash = 19;
                            texture.Type = MatTexture.TextureType.DiffuseLayer2;
                        }
                        if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_n0")
                        {
                            texture.hash = 1;
                            poly.material.HasNormalMap = true;
                            texture.Type = MatTexture.TextureType.Normal;
                        }
                        if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_s0")
                        {
                            texture.hash = 4;
                            poly.material.HasSpecularMap = true;
                            texture.Type = MatTexture.TextureType.Specular;
                        }
                        if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_b0")
                        {
                            texture.hash = 2;
                            poly.material.HasShadowMap = true;
                            texture.Type = MatTexture.TextureType.Shadow;
                        }
                        if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_b1")
                        {
                            texture.hash = 3;
                            poly.material.HasLightMap = true;
                            texture.Type = MatTexture.TextureType.Light;
                        }
                        if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_e0")
                        {
                            texture.hash = 8;
                            poly.material.HasEmissionMap = true;
                            texture.Type = MatTexture.TextureType.Emission;
                        }
                 
                        texture.Name = TextureName;
                        poly.material.textures.Add(texture);
                        id++;
                    }

                    foreach (Sampler smp in mat.Samplers.Values)
                    {
                        SamplerInfo s = new SamplerInfo();
                        s.WrapModeU = (int)smp.TexSampler.ClampX;
                        s.WrapModeV = (int)smp.TexSampler.ClampY;
                        s.WrapModeW = (int)smp.TexSampler.ClampZ;
                        poly.material.samplerinfo.Add(s);
                    }

                    poly.material.Name = mdl.Materials[shp.MaterialIndex].Name;
                    if (mdl.Materials[shp.MaterialIndex].ShaderParamData != null) //Some special cases (env models) have none
                    {
                        using (Syroot.BinaryData.BinaryDataReader reader = new Syroot.BinaryData.BinaryDataReader(new MemoryStream(mdl.Materials[shp.MaterialIndex].ShaderParamData)))
                        {
                            reader.ByteOrder = Syroot.BinaryData.ByteOrder.BigEndian;
                            foreach (Syroot.NintenTools.Bfres.ShaderParam param in mdl.Materials[shp.MaterialIndex].ShaderParams.Values)
                            {
                                ShaderParam prm = new ShaderParam();

                                prm.Type = param.Type;
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
                                        texSRT.Mode = reader.ReadSingle(); //Scale mode, Maya, max ect
                                        texSRT.scale = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                                        texSRT.rotate = reader.ReadSingle();
                                        texSRT.translate = new Vector2(reader.ReadSingle(), reader.ReadSingle());

                                        prm.Value_TexSrt = texSRT; break;
                                }
                                poly.material.matparam.Add(param.Name, prm);
                            }
                            reader.Close();
                        }
                    }

                
                    model.poly.Add(poly);
                    ShapeCur++;
                }
                models.Add(model);
                ModelCur++;
            }
        }

        private static Syroot.Maths.Vector4F[] WiiUAttributeData(VertexAttrib att, VertexBufferHelper helper, string attName)
        {
            VertexBufferHelperAttrib attd = helper[attName];
            return attd.Data;
        }

        #region HackyInject (No rebuild)

        public void InjectToWiiUBFRES(string FileName)
        {
            using (Syroot.BinaryData.BinaryDataWriter writer = new Syroot.BinaryData.BinaryDataWriter(File.Open(FileName, FileMode.Create)))
            {
                writer.ByteOrder = Syroot.BinaryData.ByteOrder.BigEndian;
                writer.Write(BFRESFile);


                int mdl = 0;
                foreach (Model fmdl in TargetWiiUBFRES.Models.Values)
                {
                    int s = 0;
                    foreach (Shape shp in fmdl.Shapes.Values)
                    {

                        s++;
                    }
                    mdl++;
                }
               
            }
        }

#endregion

        public void SaveFile(string FileName)
        {
            int mdl = 0;
            foreach (Model fmdl in TargetWiiUBFRES.Models.Values)
            {
                int s = 0;
                foreach (Shape shp in fmdl.Shapes.Values)
                {

                    VertexBufferHelper helper = new VertexBufferHelper(fmdl.VertexBuffers[shp.VertexBufferIndex], TargetWiiUBFRES.ByteOrder);

               
                    foreach (VertexAttrib att in fmdl.VertexBuffers[shp.VertexBufferIndex].Attributes.Values)
                    {
                        switch (att.Name)
                        {
                            case "_n0":
                                {
                                    VertexBufferHelperAttrib normals = helper["_n0"]; // Access by name                             
                                }
                                break;
                            case "_t0":
                            {
                                    VertexBufferHelperAttrib tangents = helper["_t0"]; // Access by name                                
                                }
                                break;
                            case "_b0":
                                {
                                    VertexBufferHelperAttrib bitangents = helper["_b0"]; // Access by name                                 
                                }
                                break;
                        }
                        fmdl.VertexBuffers[shp.VertexBufferIndex] = helper.ToVertexBuffer();
                    }
                    s++;
                }
                mdl++;
            }


            TargetWiiUBFRES.Save(FileName);
        }
        public static void WiiU2Switch(string FileName, int CurModel, BFRES b)
        {
            ResFile TargetWiiUBFRES = new ResFile(FileName);



            int CurMdl = 0;
            foreach (Model mdl in TargetWiiUBFRES.Models.Values)
            {
                int CurBn = 0;
                foreach (Syroot.NintenTools.Bfres.Bone bn in mdl.Skeleton.Bones.Values)
                {
                    Bone bone = b.models[CurMdl].skeleton.bones[CurBn];

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
                    CurBn++;
                }
                b.models[CurMdl].skeleton.reset();

                int CurShape = 0;
                foreach (Shape shp in mdl.Shapes.Values)
                {
                    Mesh poly = b.models[CurMdl].poly[CurShape];

                    //Create a buffer instance which stores all the buffer data
                    // VertexBufferHelperAttrib uv1 = helper["_u1"];

                    int TotalCount = poly.vertices.Count;

                    int LODCount = 0;

                    uint FaceCount = FaceCount = shp.Meshes[LODCount].IndexCount;
                    uint[] indicesArray = shp.Meshes[LODCount].GetIndices().ToArray();


                    int TotalFaceCount = poly.lodMeshes[poly.DisplayLODIndex].faces.Count;

                    poly.lodMeshes[poly.DisplayLODIndex].faces.Clear();

                    for (int face = 0; face < FaceCount; face++)
                    {
                        poly.lodMeshes[poly.DisplayLODIndex].faces.Add((int)indicesArray[face] + (int)shp.Meshes[LODCount].FirstVertex);
                    }

                    if (TotalFaceCount != poly.lodMeshes[poly.DisplayLODIndex].faces.Count)
                    {
                        MessageBox.Show("Error F");
                    }

                    poly.vertices.Clear();


                    //Create a buffer instance which stores all the buffer data
                    VertexBufferHelper helper = new VertexBufferHelper(mdl.VertexBuffers[shp.VertexBufferIndex], TargetWiiUBFRES.ByteOrder);

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


                    foreach (VertexAttrib att in mdl.VertexBuffers[shp.VertexBufferIndex].Attributes.Values)
                    {
                        Mesh.VertexAttribute attr = new Mesh.VertexAttribute();
                        attr.Name = att.Name;
                        // attr.Format = att.Format;

                        if (att.Name == "_p0")
                            vec4Positions = WiiUAttributeData(att, helper, "_p0");
                        if (att.Name == "_n0")
                            vec4Normals = WiiUAttributeData(att, helper, "_n0");
                        if (att.Name == "_u0")
                            vec4uv0 = WiiUAttributeData(att, helper, "_u0");
                        if (att.Name == "_u1")
                            vec4uv1 = WiiUAttributeData(att, helper, "_u1");
                        if (att.Name == "_u2")
                            vec4uv2 = WiiUAttributeData(att, helper, "_u2");
                        if (att.Name == "_c0")
                            vec4c0 = WiiUAttributeData(att, helper, "_c0");
                        if (att.Name == "_t0")
                            vec4t0 = WiiUAttributeData(att, helper, "_t0");
                        if (att.Name == "_b0")
                            vec4b0 = WiiUAttributeData(att, helper, "_b0");
                        if (att.Name == "_w0")
                            vec4w0 = WiiUAttributeData(att, helper, "_w0");
                        if (att.Name == "_i0")
                            vec4i0 = WiiUAttributeData(att, helper, "_i0");

                        poly.vertexAttributes.Add(attr);
                    }
                    for (int i = 0; i < vec4Positions.Length; i++)
                    {
                        Vertex v = new Vertex();
                        if (vec4Positions.Length > 0)
                            v.pos = new Vector3(vec4Positions[i].X, vec4Positions[i].Y, vec4Positions[i].Z);
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

                        poly.vertices.Add(v);

                    }

                    CurShape++;

                }
                CurMdl++;
            }
            b.UpdateRenderMeshes();
        }
    }
}
