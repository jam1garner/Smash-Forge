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

            TargetWiiUBFRES = new ResFile(path);

            FSKACount = TargetWiiUBFRES.SkeletalAnims.Count;

            textures.Clear();

            foreach (Texture tex in TargetWiiUBFRES.Textures.Values)
            {
                string TextureName = tex.Name;
                FTEX texture = new FTEX();
                texture.ReadFTEX(tex);
                textures.Add(TextureName, texture);
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
                            Console.WriteLine(att.Name);
                            VertexBufferHelperAttrib position = helper["_p0"];
                            Syroot.Maths.Vector4F[] vec4Positions = position.Data;

                            foreach (Syroot.Maths.Vector4F p in vec4Positions)
                            {
                                switch (position.Format)
                                {
                                    case GX2AttribFormat.Format_32_32_32_32_Single:
                                    case GX2AttribFormat.Format_32_32_32_Single:
                                    case GX2AttribFormat.Format_16_16_16_16_Single:
                                        v.pos.Add(new Vector3 { X = p.X, Y = p.Y, Z = p.Z });
                                        break;
                                    default:
                                        MessageBox.Show("Position has unsupported Format " + position.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_n0")
                        {
                            Console.WriteLine(att.Name);
                            VertexBufferHelperAttrib normal = helper["_n0"];
                            Syroot.Maths.Vector4F[] vec4Normals = normal.Data;

                            foreach (Syroot.Maths.Vector4F n in vec4Normals)
                            {
                                switch (normal.Format)
                                {
                                    case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                    case GX2AttribFormat.Format_32_32_32_32_Single:
                                    case GX2AttribFormat.Format_16_16_16_16_Single:
                                    case GX2AttribFormat.Format_8_8_8_8_SNorm:
                                    case GX2AttribFormat.Format_16_16_16_16_SNorm:
                                    case GX2AttribFormat.Format_32_32_Single:
                                    case GX2AttribFormat.Format_32_32_32_Single:
                                        v.nrm.Add(new Vector3 { X = n.X, Y = n.Y, Z = n.Z });
                                        break;
                                    default:
                                        MessageBox.Show("Normals has unsupported Format " + normal.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_u0")
                        {
                            Console.WriteLine(att.Name);
                            VertexBufferHelperAttrib uv0 = helper["_u0"];
                            Syroot.Maths.Vector4F[] vec4uv0 = uv0.Data;

                            foreach (Syroot.Maths.Vector4F u in vec4uv0)
                            {
                                switch (uv0.Format)
                                {
                                    case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                    case GX2AttribFormat.Format_32_32_32_32_UInt:
                                    case GX2AttribFormat.Format_16_16_UNorm:
                                    case GX2AttribFormat.Format_8_8_SNorm:
                                    case GX2AttribFormat.Format_8_8_UNorm:
                                    case GX2AttribFormat.Format_16_16_SNorm:
                                    case GX2AttribFormat.Format_16_16_Single:
                                    case GX2AttribFormat.Format_32_32_Single:
                                    case GX2AttribFormat.Format_32_Single:
                                        v.uv0.Add(new Vector2 { X = u.X, Y = u.Y });
                                        break;
                                    default:
                                        MessageBox.Show("UV 0 has unsupported Format " + uv0.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_u1")
                        {
                            Console.WriteLine(att.Name);
                            VertexBufferHelperAttrib uv1 = helper["_u1"];
                            Syroot.Maths.Vector4F[] vec4uv1 = uv1.Data;

                            foreach (Syroot.Maths.Vector4F u in vec4uv1)
                            {
                                switch (uv1.Format)
                                {
                                    case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                    case GX2AttribFormat.Format_32_32_32_32_UInt:
                                    case GX2AttribFormat.Format_16_16_UNorm:
                                    case GX2AttribFormat.Format_8_8_SNorm:
                                    case GX2AttribFormat.Format_8_8_UNorm:
                                    case GX2AttribFormat.Format_16_16_SNorm:
                                    case GX2AttribFormat.Format_16_16_Single:
                                    case GX2AttribFormat.Format_32_32_Single:
                                    case GX2AttribFormat.Format_32_Single:
                                        v.uv1.Add(new Vector2 { X = u.X, Y = u.Y });
                                        break;
                                    default:
                                        MessageBox.Show("UV 1 has unsupported Format " + uv1.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_u2")
                        {
                            Console.WriteLine(att.Name);
                            VertexBufferHelperAttrib uv2 = helper["_u2"];
                            Syroot.Maths.Vector4F[] vec4uv2 = uv2.Data;

                            foreach (Syroot.Maths.Vector4F u in vec4uv2)
                            {
                                switch (uv2.Format)
                                {
                                    case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                    case GX2AttribFormat.Format_32_32_32_32_UInt:
                                    case GX2AttribFormat.Format_16_16_UNorm:
                                    case GX2AttribFormat.Format_8_8_SNorm:
                                    case GX2AttribFormat.Format_8_8_UNorm:
                                    case GX2AttribFormat.Format_16_16_SNorm:
                                    case GX2AttribFormat.Format_16_16_Single:
                                    case GX2AttribFormat.Format_32_32_Single:
                                    case GX2AttribFormat.Format_32_Single:
                                        v.uv2.Add(new Vector2 { X = u.X, Y = u.Y });
                                        break;
                                    default:
                                        MessageBox.Show("UV 2 has unsupported Format " + uv2.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_c0")
                        {
                            Console.WriteLine(att.Name);
                            VertexBufferHelperAttrib c0 = helper["_c0"];
                            Syroot.Maths.Vector4F[] vec4c0 = c0.Data;

                            foreach (Syroot.Maths.Vector4F c in vec4c0)
                            {
                                switch (c0.Format)
                                {
                                    case GX2AttribFormat.Format_32_32_32_32_Single:
                                    case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                    case GX2AttribFormat.Format_32_32_32_32_UInt:
                                    case GX2AttribFormat.Format_16_16_16_16_Single:
                                    case GX2AttribFormat.Format_16_16_UNorm:
                                    case GX2AttribFormat.Format_8_8_SNorm:
                                    case GX2AttribFormat.Format_8_8_UNorm:
                                    case GX2AttribFormat.Format_16_16_SNorm:
                                    case GX2AttribFormat.Format_16_16_Single:
                                    case GX2AttribFormat.Format_32_32_Single:
                                    case GX2AttribFormat.Format_32_Single:
                                    case GX2AttribFormat.Format_8_8_8_8_SNorm:
                                    case GX2AttribFormat.Format_8_8_8_8_UNorm:
                                        v.col.Add(new Vector4 { X = c.X, Y = c.Y, Z = c.Z, W = c.W });
                                        break;
                                    default:
                                        MessageBox.Show("Color has unsupported Format " + c0.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_t0")
                        {
                            Console.WriteLine(att.Name);
                            VertexBufferHelperAttrib t0 = helper["_t0"];
                            Syroot.Maths.Vector4F[] vec4t0 = t0.Data;

                            foreach (Syroot.Maths.Vector4F u in vec4t0)
                            {
                                switch (t0.Format)
                                {
                                    case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                    case GX2AttribFormat.Format_32_32_32_32_UInt:
                                    case GX2AttribFormat.Format_16_16_UNorm:
                                    case GX2AttribFormat.Format_8_8_SNorm:
                                    case GX2AttribFormat.Format_8_8_UNorm:
                                    case GX2AttribFormat.Format_16_16_SNorm:
                                    case GX2AttribFormat.Format_16_16_Single:
                                    case GX2AttribFormat.Format_32_32_Single:
                                    case GX2AttribFormat.Format_32_Single:
                                    case GX2AttribFormat.Format_8_8_8_8_SNorm:
                                    case GX2AttribFormat.Format_8_8_8_8_UNorm:
                                        v.tans.Add(new Vector4 { X = u.X, Y = u.Y, Z = u.Z, W = u.W });
                                        break;
                                    default:
                                        MessageBox.Show("tangents have unsupported Format " + t0.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_b0")
                        {
                            Console.WriteLine(att.Name);
                            VertexBufferHelperAttrib b0 = helper["_b0"];
                            Syroot.Maths.Vector4F[] vec4b0 = b0.Data;

                            foreach (Syroot.Maths.Vector4F u in vec4b0)
                            {
                                switch (b0.Format)
                                {
                                    case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                    case GX2AttribFormat.Format_32_32_32_32_UInt:
                                    case GX2AttribFormat.Format_16_16_UNorm:
                                    case GX2AttribFormat.Format_8_8_SNorm:
                                    case GX2AttribFormat.Format_8_8_UNorm:
                                    case GX2AttribFormat.Format_16_16_SNorm:
                                    case GX2AttribFormat.Format_16_16_Single:
                                    case GX2AttribFormat.Format_32_32_Single:
                                    case GX2AttribFormat.Format_32_Single:
                                    case GX2AttribFormat.Format_8_8_8_8_SNorm:
                                    case GX2AttribFormat.Format_8_8_8_8_UNorm:
                                        v.bitans.Add(new Vector4 { X = u.X, Y = u.Y, Z = u.Z, W = u.W });
                                        break;
                                    default:
                                        MessageBox.Show("binorms has unsupported Format " + b0.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_w0")
                        {
                            Console.WriteLine(att.Name);
                            VertexBufferHelperAttrib w0 = helper["_w0"];
                            Syroot.Maths.Vector4F[] vec4w0 = w0.Data;

                            foreach (Syroot.Maths.Vector4F w in vec4w0)
                            {
                                switch (w0.Format)
                                {
                                    case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                    case GX2AttribFormat.Format_32_32_32_32_UInt:
                                    case GX2AttribFormat.Format_16_16_UNorm:
                                    case GX2AttribFormat.Format_8_8_SNorm:
                                    case GX2AttribFormat.Format_8_8_UNorm:
                                    case GX2AttribFormat.Format_16_16_SNorm:
                                    case GX2AttribFormat.Format_16_16_Single:
                                    case GX2AttribFormat.Format_32_32_Single:
                                    case GX2AttribFormat.Format_32_Single:
                                    case GX2AttribFormat.Format_8_8_8_8_SNorm:
                                    case GX2AttribFormat.Format_8_8_8_8_UNorm:
                                    case GX2AttribFormat.Format_32_32_32_UInt:
                                    case GX2AttribFormat.Format_32_32_32_32_Single:
                                    case GX2AttribFormat.Format_32_32_32_Single:
                                        v.weights.Add(new Vector4 { X = w.X, Y = w.Y, Z = w.Z, W = w.W });
                                        break;
                                    default:
                                        MessageBox.Show("weights has unsupported Format " + w0.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_i0")
                        {
                            Console.WriteLine(att.Name);
                            VertexBufferHelperAttrib i0 = helper["_i0"];
                            Syroot.Maths.Vector4F[] vec4i0 = i0.Data;

                            foreach (Syroot.Maths.Vector4F i in vec4i0)
                            {
                                switch (i0.Format)
                                {
                                    case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                    case GX2AttribFormat.Format_32_32_32_32_UInt:
                                    case GX2AttribFormat.Format_16_16_UNorm:
                                    case GX2AttribFormat.Format_8_8_SNorm:
                                    case GX2AttribFormat.Format_8_8_UNorm:
                                    case GX2AttribFormat.Format_16_16_SNorm:
                                    case GX2AttribFormat.Format_16_16_Single:
                                    case GX2AttribFormat.Format_32_32_Single:
                                    case GX2AttribFormat.Format_32_Single:
                                    case GX2AttribFormat.Format_8_8_8_8_UInt:
                                    case GX2AttribFormat.Format_8_8_8_8_UNorm:
                                    case GX2AttribFormat.Format_8_UInt:
                                    case GX2AttribFormat.Format_8_8_UInt:
                                    case GX2AttribFormat.Format_32_32_UInt:
                                    case GX2AttribFormat.Format_32_32_32_UInt:
                                    case GX2AttribFormat.Format_32_UInt:
                                    case GX2AttribFormat.Format_16_16_UInt:
                                        v.nodes.Add(new Vector4 { X = i.X, Y = i.Y, Z = i.Z, W = i.W });
                                        break;
                                    default:
                                        MessageBox.Show("indices has unsupported Format " + i0.Format);
                                        break;
                                }
                            }
                        }
                    }
                    poly.vertices = v;

                    //shp.Meshes.Count - 1 //For going to the lowest poly LOD mesh

                    int LODCount = 0;

                    uint FaceCount = FaceCount = shp.Meshes[LODCount].IndexCount;


                    uint[] indicesArray = shp.Meshes[LODCount].GetIndices().ToArray();

                    for (int face = 0; face < FaceCount; face++)
                    {
                        poly.faces.Add((int)indicesArray[face] + (int)shp.Meshes[LODCount].FirstVertex);
                    }

                    int AlbedoCount = 0;

                    string TextureName = "";
                    int id = 0;
                    foreach (TextureRef tex in mdl.Materials[shp.MaterialIndex].TextureRefs)
                    {
                        TextureName = tex.Name;

                        MatTexture texture = new MatTexture();

                        poly.Nodes.Add(new TreeNode { Text = TextureName });

                        if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_a0")
                        {
                            if (AlbedoCount == 0)
                            {
                                try
                                {
                                    poly.texHashs.Add(textures[TextureName].texture.display);
                                    AlbedoCount++;
                                }
                                catch
                                {
                                    poly.texHashs.Add(0);
                                }
                                poly.TextureMapTypes.Add("Diffuse");
                            }
                        }
                        if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_a1")
                        {
                            try
                            {
                                poly.texHashs.Add(textures[TextureName].texture.display);
                                AlbedoCount++;
                            }
                            catch
                            {
                                poly.texHashs.Add(0);
                            }
                            poly.TextureMapTypes.Add("Diffuse_Layer");
                        }
                        if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_n0")
                        {
                            try
                            {
                                poly.texHashs.Add(textures[TextureName].texture.display);
                            }
                            catch
                            {
                                poly.texHashs.Add(1);
                            }
                            poly.material.HasNormalMap = true;
                            poly.TextureMapTypes.Add("Normal");
                        }
                        if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_b0")
                        {
                            try
                            {
                                poly.texHashs.Add(textures[TextureName].texture.display);
                            }
                            catch
                            {
                                poly.texHashs.Add(2);
                            }
                            poly.TextureMapTypes.Add("Bake1");
                        }
                        if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_b1")
                        {
                            try
                            {
                                poly.texHashs.Add(textures[TextureName].texture.display);
                            }
                            catch
                            {
                                poly.texHashs.Add(3);
                            }
                            poly.TextureMapTypes.Add("Bake2");
                        }
                        id++;
                        texture.Name = TextureName;
                    }

                    poly.material.Name = mdl.Materials[shp.MaterialIndex].Name;

                    foreach (Sampler smp in mdl.Materials[shp.MaterialIndex].Samplers.Values)
                    {
                        SamplerInfo s = new SamplerInfo();
                        s.WrapModeU = (int)smp.TexSampler.ClampX;
                        s.WrapModeV = (int)smp.TexSampler.ClampY;
                        s.WrapModeW = (int)smp.TexSampler.ClampZ;
                        poly.material.samplerinfo.Add(s);
                    }

                    using (Syroot.BinaryData.BinaryDataReader reader = new Syroot.BinaryData.BinaryDataReader(new MemoryStream(mdl.Materials[shp.MaterialIndex].ShaderParamData)))
                    {
                        reader.ByteOrder = Syroot.BinaryData.ByteOrder.BigEndian;
                        foreach (Syroot.NintenTools.Bfres.ShaderParam param in mdl.Materials[shp.MaterialIndex].ShaderParams.Values)
                        {
                            ShaderParam prm = new ShaderParam();

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
    }
}
