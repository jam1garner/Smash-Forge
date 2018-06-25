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


            foreach (Texture tex in TargetWiiUBFRES.Textures.Values)
            {
                string TextureName = tex.Name;
                FTEX texture = new FTEX();
                texture.ReadFTEX(tex);

                if (!FTEXtextures.ContainsKey(texture.Text))
                {
                    FTEXtextures.Add(TextureName, texture);
                }
                TTextures.Nodes.Add(texture);
            }

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
                    poly.matrFlag = shp.VertexSkinCount;
                    poly.fsklindx = shp.BoneIndex;

                    TModels.Nodes[ModelCur].Nodes.Add(poly);

                    //Create a buffer instance which stores all the buffer data
                    VertexBufferHelper helper = new VertexBufferHelper(mdl.VertexBuffers[shp.VertexBufferIndex], TargetWiiUBFRES.ByteOrder);

                    // VertexBufferHelperAttrib uv1 = helper["_u1"];


                    Vertex v = new Vertex();
                    foreach (VertexAttrib att in mdl.VertexBuffers[shp.VertexBufferIndex].Attributes.Values)
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
                                v.pos1.Add(new Vector3 { X = p.X, Y = p.Y, Z = p.Z });
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

                    //shp.Meshes.Count - 1 //For going to the lowest poly LOD mesh

                    int LODCount = 0;

                    uint FaceCount = FaceCount = shp.Meshes[LODCount].IndexCount;
                    uint[] indicesArray = shp.Meshes[LODCount].GetIndices().ToArray();

                    poly.BoundingCount = shp.SubMeshBoundings.Count;

                    for (int face = 0; face < FaceCount; face++)
                    {
                        poly.faces.Add((int)indicesArray[face] + (int)shp.Meshes[LODCount].FirstVertex);
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

                    int AlbedoCount = 0;

                    string TextureName = "";
                    int id = 0;

                    int o = 0;
                    foreach (var op in mdl.Materials[shp.MaterialIndex].ShaderAssign.ShaderOptions)
                    {
                        poly.material.shaderassign.Add(op.Key, op.Value);
                        o++;
                    }

                    int SampIndex = 0;
                    foreach (var smp in mdl.Materials[shp.MaterialIndex].Samplers)
                    {
                        poly.material.Samplers.Add(smp.Key, SampIndex);
                        SampIndex++;
                    }

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

                    poly.material.Name = mdl.Materials[shp.MaterialIndex].Name;

                    using (Syroot.BinaryData.BinaryDataReader reader = new Syroot.BinaryData.BinaryDataReader(new MemoryStream(mdl.Materials[shp.MaterialIndex].ShaderParamData)))
                    {
                        reader.ByteOrder = Syroot.BinaryData.ByteOrder.BigEndian;
                        foreach (Syroot.NintenTools.Bfres.ShaderParam param in mdl.Materials[shp.MaterialIndex].ShaderParams.Values)
                        {
                            ShaderParam prm = new ShaderParam();

                            prm.Type = param.Type;

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
                    model.poly.Add(poly);
                    ShapeCur++;
                }
                models.Add(model);
                ModelCur++;
            }
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
                        Vertex v = models[mdl].poly[s].vertices;

                        VertexBufferHelper helper = new VertexBufferHelper(fmdl.VertexBuffers[shp.VertexBufferIndex], TargetWiiUBFRES.ByteOrder);


                        foreach (VertexAttrib att in fmdl.VertexBuffers[shp.VertexBufferIndex].Attributes.Values)
                        {
                            int test = (int)fmdl.VertexBuffers[shp.VertexBufferIndex].Buffers[att.BufferIndex].BufferOffset + att.Offset + fmdl.VertexBuffers[shp.VertexBufferIndex].Buffers[att.BufferIndex].Stride;
                        //    Console.WriteLine(test + " " + att.Format + " " + (int)att.Format);
                        }

                        for (int vt = 0; vt < fmdl.VertexBuffers[shp.VertexBufferIndex].VertexCount; vt++)
                        {
                            int at = 0;
                            foreach (VertexAttrib att in fmdl.VertexBuffers[shp.VertexBufferIndex].Attributes.Values)
                            {
                                writer.Seek(fmdl.VertexBuffers[shp.VertexBufferIndex].Buffers[att.BufferIndex].BufferOffset + att.Offset + fmdl.VertexBuffers[shp.VertexBufferIndex].Buffers[att.BufferIndex].Stride * vt, SeekOrigin.Begin);
                    /*            if (att.Name == "_p0")
                                {
                                    if (att.Format == GX2AttribFormat.Format_32_32_32_Single)
                                    {
                                        writer.Write(v.pos[vt].X);
                                        writer.Write(v.pos[vt].Y);
                                        writer.Write(v.pos[vt].Z);
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_16_16_16_16_Single)
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
                                }*/
                                if (att.Name == "_n0")
                                {
                                    if (att.Format == GX2AttribFormat.Format_10_10_10_2_SNorm)
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
                                    if (att.Format == GX2AttribFormat.Format_8_8_8_8_SNorm)
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
                                    if (att.Format == GX2AttribFormat.Format_8_8_8_8_SNorm)
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
                                    if (att.Format == GX2AttribFormat.Format_8_8_8_8_UNorm)
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
                                    if (att.Format == GX2AttribFormat.Format_16_16_UNorm)
                                    {
                                        writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv0[vt].X, 0, 1) * 65535));
                                        writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv0[vt].Y, 0, 1) * 65535));
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_32_32_Single)
                                    {
                                        writer.Write(v.uv0[vt].X);
                                        writer.Write(v.uv0[vt].Y);
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_16_16_Single)
                                    {
                                        Syroot.Maths.Vector2F value = new Syroot.Maths.Vector2F(v.uv0[vt].X, v.uv0[vt].Y);

                                        writer.Write((short)fromFloat(value.X));
                                        writer.Write((short)fromFloat(value.Y));
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_16_16_SNorm)
                                    {
                                        writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv0[vt].X, -1, 1) * 32767));
                                        writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv0[vt].Y, -1, 1) * 32767));
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_8_8_UNorm)
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
                                    if (att.Format == GX2AttribFormat.Format_16_16_UNorm)
                                    {
                                        writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv1[vt].X, 0, 1) * 65535));
                                        writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv1[vt].Y, 0, 1) * 65535));
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_32_32_Single)
                                    {
                                        writer.Write(v.uv1[vt].X);
                                        writer.Write(v.uv1[vt].Y);
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_16_16_Single)
                                    {
                                        Syroot.Maths.Vector2F value = new Syroot.Maths.Vector2F(v.uv0[vt].X, v.uv0[vt].Y);

                                        writer.Write((short)fromFloat(value.X));
                                        writer.Write((short)fromFloat(value.Y));
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_16_16_SNorm)
                                    {
                                        writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv1[vt].X, -1, 1) * 32767));
                                        writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv1[vt].Y, -1, 1) * 32767));
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_8_8_UNorm)
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
                                    if (att.Format == GX2AttribFormat.Format_16_16_UNorm)
                                    {
                                        writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv2[vt].X, 0, 1) * 65535));
                                        writer.Write((ushort)(Syroot.Maths.Algebra.Clamp(v.uv2[vt].Y, 0, 1) * 65535));
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_32_32_Single)
                                    {
                                        writer.Write(v.uv2[vt].X);
                                        writer.Write(v.uv2[vt].Y);
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_16_16_Single)
                                    {
                                        Syroot.Maths.Vector2F value = new Syroot.Maths.Vector2F(v.uv0[vt].X, v.uv0[vt].Y);

                                        writer.Write((short)fromFloat(value.X));
                                        writer.Write((short)fromFloat(value.Y));
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_16_16_SNorm)
                                    {
                                        writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv2[vt].X, -1, 1) * 32767));
                                        writer.Write((short)(Syroot.Maths.Algebra.Clamp(v.uv2[vt].Y, -1, 1) * 32767));
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_8_8_UNorm)
                                    {
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.uv2[vt].X, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.uv2[vt].Y, 0, 1) * 255));
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unsupported  format for uv2 " + att.Format);
                                    }
                                }
                      /*          if (att.Name == "_i0")
                                {
                                    if (att.Format == GX2AttribFormat.Format_8_8_8_8_UInt)
                                    {
                                        writer.Write((byte)v.nodes[vt].X);
                                        writer.Write((byte)v.nodes[vt].Y);
                                        writer.Write((byte)v.nodes[vt].Z);
                                        writer.Write((byte)v.nodes[vt].W);
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_8_8_UInt)
                                    {
                                        writer.Write((byte)v.nodes[vt].X);
                                        writer.Write((byte)v.nodes[vt].Y);
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_8_UInt)
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
                                    if (att.Format == GX2AttribFormat.Format_8_8_8_8_UNorm)
                                    {
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.weights[vt].X, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.weights[vt].Y, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.weights[vt].Z, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.weights[vt].W, 0, 1) * 255));
                                    }
                                    else if (att.Format == GX2AttribFormat.Format_8_8_UNorm)
                                    {
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.weights[vt].X, 0, 1) * 255));
                                        writer.Write((byte)(Syroot.Maths.Algebra.Clamp(v.weights[vt].Y, 0, 1) * 255));
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unsupported format for weights " + att.Format);
                                    }
                                }*/
                            }
                            at++;
                        }

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
                                    VertexBufferHelperAttrib tangents = helper["_n0"]; // Access by name
                                    int t = 0;
                                    foreach (Vector3 n in models[mdl].poly[s].vertices.nrm)
                                    {
                                        tangents.Data[t] = new Syroot.Maths.Vector4F(n.X, n.Y, n.Z, 0);
                                        t++;
                                    }
                                }
                                break;
                            case "_t0":
                            {
                                    VertexBufferHelperAttrib tangents = helper["_t0"]; // Access by name
                                    int t = 0;
                                    foreach (Vector4 tan in models[mdl].poly[s].vertices.tans)
                                    {
                                        tangents.Data[t] = new Syroot.Maths.Vector4F(tan.X, tan.Y, tan.Z, tan.W);
                                        t++;
                                    }
                                }
                                break;
                            case "_b0":
                                {
                                    VertexBufferHelperAttrib bitangents = helper["_b0"]; // Access by name
                                    int b = 0;
                                    foreach (Vector4 bitan in models[mdl].poly[s].vertices.bitans)
                                    {
                                        bitangents.Data[b] = new Syroot.Maths.Vector4F(bitan.X, bitan.Y, bitan.Z, bitan.W);
                                        b++;
                                    }
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


            Model mdl = TargetWiiUBFRES.Models[CurModel];

            int CurShape = 0;

            foreach (Shape shp in mdl.Shapes.Values)
            {
                Mesh poly = b.models[CurModel].poly[CurShape];

                //Create a buffer instance which stores all the buffer data
                VertexBufferHelper helper = new VertexBufferHelper(mdl.VertexBuffers[shp.VertexBufferIndex], TargetWiiUBFRES.ByteOrder);

                // VertexBufferHelperAttrib uv1 = helper["_u1"];

                int TotalCount = poly.vertices.pos.Count;

                int LODCount = 0;

                uint FaceCount = FaceCount = shp.Meshes[LODCount].IndexCount;
                uint[] indicesArray = shp.Meshes[LODCount].GetIndices().ToArray();


                int TotalFaceCount = poly.faces.Count;

                poly.Faces = new int[0];
                poly.faces.Clear();

                for (int face = 0; face < FaceCount; face++)
                {
                    poly.faces.Add((int)indicesArray[face] + (int)shp.Meshes[LODCount].FirstVertex);
                }

                if (TotalFaceCount != poly.faces.Count)
                {
                    MessageBox.Show("Error F");
                }

                Vertex v = poly.vertices;
                v.pos.Clear();
                v.nrm.Clear();
                v.tans.Clear();
                v.uv0.Clear();
                v.uv1.Clear();
                v.bitans.Clear();
                v.weights.Clear();
                v.nodes.Clear();
                foreach (VertexAttrib att in mdl.VertexBuffers[shp.VertexBufferIndex].Attributes.Values)
                {
                    if (att.Name == "_p0")
                    {
                        VertexBufferHelperAttrib position = helper["_p0"];
                        Syroot.Maths.Vector4F[] vec4Positions = position.Data;

                        foreach (Syroot.Maths.Vector4F p in vec4Positions)
                        {
                            v.pos.Add(new Vector3 { X = p.X, Y = p.Y, Z = p.Z });
                        }
                        for (int i = 0; i < TotalCount - v.pos.Count; i++)
                        {
                            v.pos.Add(new Vector3 { X = 0, Y = 0, Z = 0 });
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
                        for (int i = 0; i < TotalCount - v.nrm.Count; i++)
                        {
                            v.nrm.Add(new Vector3 { X = 0, Y = 0, Z = 0 });
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
                        for (int i = 0; i < TotalCount - v.uv0.Count; i++)
                        {
                            v.uv0.Add(new Vector2 { X = 0, Y = 0 });
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
                        for (int i = 0; i < TotalCount - v.uv1.Count; i++)
                        {
                            v.uv1.Add(new Vector2 { X = 0, Y = 0 });
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
                        for (int i = 0; i < TotalCount - v.uv2.Count; i++)
                        {
                            v.uv2.Add(new Vector2 { X = 0, Y = 0 });

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
                        for (int i = 0; i < TotalCount - v.col.Count; i++)
                        {
                            v.col.Add(new Vector4 { X = 0, Y = 0, Z = 0, W = 0 });
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
                        for (int i = 0; i < TotalCount - v.tans.Count; i++)
                        {
                            v.tans.Add(new Vector4 { X = 0, Y = 0, Z = 0, W = 0 });
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
                        for (int i = 0; i < TotalCount - v.tans.Count; i++)
                        {
                            v.tans.Add(new Vector4 { X = 0, Y = 0, Z = 0, W = 0 });
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
                        for (int i = 0; i < TotalCount - v.weights.Count; i++)
                        {
                            v.weights.Add(new Vector4 { X = 0, Y = 0, Z = 0, W = 0 });
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
                        for (int i = 0; i < TotalCount - v.nodes.Count; i++)
                        {
                            v.nodes.Add(new Vector4 { X = 0, Y = 0, Z = 0, W = 0 });
                        }
                    }
                }
                CurShape++;

            }
            b.UpdateVertexData();
        }
    }
}
