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

            Console.WriteLine("Name = " + TargetSwitchBFRES.Name);

            foreach (ExternalFile ext in TargetSwitchBFRES.ExternalFiles)
            {
                f = new FileData(ext.Data);

                f.Endian = Endianness.Little;

                string EmMagic = f.readString(f.pos(), 4);

                if (EmMagic.Equals("BNTX")) //Textures
                {
                    int temp = f.pos();
                    BNTX t = new BNTX();
                    t.ReadBNTX(f);
                    TEmbedded.Nodes.Add(t);

                }
            }

            int ModelCur = 0;
            //FMDLs -Models-
            foreach (Model mdl in TargetSwitchBFRES.Models)
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

                    TModels.Nodes[ModelCur].Nodes.Add(poly);



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
                                switch (position.Format)
                                {
                                    case AttribFormat.Format_32_32_32_32_Single:
                                    case AttribFormat.Format_32_32_32_Single:
                                    case AttribFormat.Format_16_16_16_16_Single:
                                        v.pos.Add(new Vector3 { X = p.X, Y = p.Y, Z = p.Z });
                                        break;
                                    default:
                                        MessageBox.Show("Something went wrong :(");
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_n0")
                        {
                            VertexBufferHelperAttrib normal = helper["_n0"];
                            Syroot.Maths.Vector4F[] vec4Normals = normal.Data;

                            foreach (Syroot.Maths.Vector4F n in vec4Normals)
                            {
                                switch (normal.Format)
                                {
                                    case AttribFormat.Format_10_10_10_2_SNorm:
                                    case AttribFormat.Format_32_32_32_32_Single:
                                    case AttribFormat.Format_16_16_16_16_Single:
                                    case AttribFormat.Format_8_8_8_8_SNorm:
                                    case AttribFormat.Format_16_16_16_16_SNorm:
                                    case AttribFormat.Format_32_32_Single:
                                    case AttribFormat.Format_32_32_32_Single:
                                        v.nrm.Add(new Vector3 { X = n.X, Y = n.Y, Z = n.Z });
                                        break;
                                    default:
                                        MessageBox.Show("Unsupported Format " + normal.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_u0")
                        {
                            VertexBufferHelperAttrib uv0 = helper["_u0"];
                            Syroot.Maths.Vector4F[] vec4uv0 = uv0.Data;

                            foreach (Syroot.Maths.Vector4F u in vec4uv0)
                            {
                                switch (uv0.Format)
                                {
                                    case AttribFormat.Format_10_10_10_2_SNorm:
                                    case AttribFormat.Format_32_32_32_32_UInt:
                                    case AttribFormat.Format_16_16_UNorm:
                                    case AttribFormat.Format_8_8_SNorm:
                                    case AttribFormat.Format_8_8_UNorm:
                                    case AttribFormat.Format_16_16_SNorm:
                                    case AttribFormat.Format_16_16_Single:
                                    case AttribFormat.Format_32_32_Single:
                                    case AttribFormat.Format_32_Single:
                                        v.uv0.Add(new Vector2 { X = u.X, Y = u.Y });
                                        break;
                                    default:
                                        MessageBox.Show("Unsupported Format " + uv0.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_u1")
                        {

                            VertexBufferHelperAttrib uv1 = helper["_u1"];
                            Syroot.Maths.Vector4F[] vec4uv1 = uv1.Data;

                            foreach (Syroot.Maths.Vector4F u in vec4uv1)
                            {
                                switch (uv1.Format)
                                {
                                    case AttribFormat.Format_10_10_10_2_SNorm:
                                    case AttribFormat.Format_32_32_32_32_UInt:
                                    case AttribFormat.Format_16_16_UNorm:
                                    case AttribFormat.Format_8_8_SNorm:
                                    case AttribFormat.Format_8_8_UNorm:
                                    case AttribFormat.Format_16_16_SNorm:
                                    case AttribFormat.Format_16_16_Single:
                                    case AttribFormat.Format_32_32_Single:
                                    case AttribFormat.Format_32_Single:
                                        v.uv1.Add(new Vector2 { X = u.X, Y = u.Y });
                                        break;
                                    default:
                                        MessageBox.Show("Unsupported Format for uv1 " + uv1.Format);
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
                                    case AttribFormat.Format_10_10_10_2_SNorm:
                                    case AttribFormat.Format_32_32_32_32_UInt:
                                    case AttribFormat.Format_16_16_UNorm:
                                    case AttribFormat.Format_8_8_SNorm:
                                    case AttribFormat.Format_8_8_UNorm:
                                    case AttribFormat.Format_16_16_SNorm:
                                    case AttribFormat.Format_16_16_Single:
                                    case AttribFormat.Format_32_32_Single:
                                    case AttribFormat.Format_32_Single:
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
                            VertexBufferHelperAttrib c0 = helper["_c0"];
                            Syroot.Maths.Vector4F[] vec4c0 = c0.Data;

                            foreach (Syroot.Maths.Vector4F c in vec4c0)
                            {
                                switch (c0.Format)
                                {
                                    case AttribFormat.Format_32_32_32_32_Single:
                                    case AttribFormat.Format_10_10_10_2_SNorm:
                                    case AttribFormat.Format_32_32_32_32_UInt:
                                    case AttribFormat.Format_16_16_16_16_Single:
                                    case AttribFormat.Format_16_16_UNorm:
                                    case AttribFormat.Format_8_8_SNorm:
                                    case AttribFormat.Format_8_8_UNorm:
                                    case AttribFormat.Format_16_16_SNorm:
                                    case AttribFormat.Format_16_16_Single:
                                    case AttribFormat.Format_32_32_Single:
                                    case AttribFormat.Format_32_Single:
                                    case AttribFormat.Format_8_8_8_8_SNorm:
                                    case AttribFormat.Format_8_8_8_8_UNorm:
                                        v.col.Add(new Vector4 { X = c.X, Y = c.Y, Z = c.Z, W = c.W });
                                        break;
                                    default:
                                        MessageBox.Show("Unsupported Format for c0 " + c0.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_t0")
                        {
                            VertexBufferHelperAttrib t0 = helper["_t0"];
                            Syroot.Maths.Vector4F[] vec4t0 = t0.Data;

                            foreach (Syroot.Maths.Vector4F u in vec4t0)
                            {
                                switch (t0.Format)
                                {
                                    case AttribFormat.Format_10_10_10_2_SNorm:
                                    case AttribFormat.Format_32_32_32_32_UInt:
                                    case AttribFormat.Format_16_16_UNorm:
                                    case AttribFormat.Format_8_8_SNorm:
                                    case AttribFormat.Format_8_8_UNorm:
                                    case AttribFormat.Format_16_16_SNorm:
                                    case AttribFormat.Format_16_16_Single:
                                    case AttribFormat.Format_32_32_Single:
                                    case AttribFormat.Format_32_Single:
                                    case AttribFormat.Format_8_8_8_8_SNorm:
                                    case AttribFormat.Format_8_8_8_8_UNorm:
                                        v.tans.Add(new Vector4 { X = u.X, Y = u.Y, Z = u.Z, W = u.W });
                                        break;
                                    default:
                                        MessageBox.Show("Unsupported Format for t0 " + t0.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_b0")
                        {

                            VertexBufferHelperAttrib b0 = helper["_b0"];
                            Syroot.Maths.Vector4F[] vec4b0 = b0.Data;

                            foreach (Syroot.Maths.Vector4F u in vec4b0)
                            {
                                switch (b0.Format)
                                {
                                    case AttribFormat.Format_10_10_10_2_SNorm:
                                    case AttribFormat.Format_32_32_32_32_UInt:
                                    case AttribFormat.Format_16_16_UNorm:
                                    case AttribFormat.Format_8_8_SNorm:
                                    case AttribFormat.Format_8_8_UNorm:
                                    case AttribFormat.Format_16_16_SNorm:
                                    case AttribFormat.Format_16_16_Single:
                                    case AttribFormat.Format_32_32_Single:
                                    case AttribFormat.Format_32_Single:
                                    case AttribFormat.Format_8_8_8_8_SNorm:
                                    case AttribFormat.Format_8_8_8_8_UNorm:
                                        v.bitans.Add(new Vector4 { X = u.X, Y = u.Y, Z = u.Z, W = u.W });
                                        break;
                                    default:
                                        MessageBox.Show("Unsupported Format " + b0.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_w0")
                        {

                            VertexBufferHelperAttrib w0 = helper["_w0"];
                            Syroot.Maths.Vector4F[] vec4w0 = w0.Data;

                            foreach (Syroot.Maths.Vector4F w in vec4w0)
                            {
                                switch (w0.Format)
                                {
                                    case AttribFormat.Format_10_10_10_2_SNorm:
                                    case AttribFormat.Format_32_32_32_32_UInt:
                                    case AttribFormat.Format_16_16_UNorm:
                                    case AttribFormat.Format_8_8_SNorm:
                                    case AttribFormat.Format_8_8_UNorm:
                                    case AttribFormat.Format_16_16_SNorm:
                                    case AttribFormat.Format_16_16_Single:
                                    case AttribFormat.Format_32_32_Single:
                                    case AttribFormat.Format_32_Single:
                                    case AttribFormat.Format_8_8_8_8_SNorm:
                                    case AttribFormat.Format_8_8_8_8_UNorm:
                                    case AttribFormat.Format_32_32_32_UInt:
                                    case AttribFormat.Format_32_32_32_32_Single:
                                    case AttribFormat.Format_32_32_32_Single:
                                        v.weights.Add(new Vector4 { X = w.X, Y = w.Y, Z = w.Z, W = w.W });
                                        break;
                                    default:
                                        MessageBox.Show("Unsupported Format " + w0.Format);
                                        break;
                                }
                            }
                        }
                        if (att.Name == "_i0")
                        {
                            VertexBufferHelperAttrib i0 = helper["_i0"];
                            Syroot.Maths.Vector4F[] vec4i0 = i0.Data;

                            foreach (Syroot.Maths.Vector4F i in vec4i0)
                            {
                                switch (i0.Format)
                                {
                                    case AttribFormat.Format_10_10_10_2_SNorm:
                                    case AttribFormat.Format_32_32_32_32_UInt:
                                    case AttribFormat.Format_16_16_UNorm:
                                    case AttribFormat.Format_8_8_SNorm:
                                    case AttribFormat.Format_8_8_UNorm:
                                    case AttribFormat.Format_16_16_SNorm:
                                    case AttribFormat.Format_16_16_Single:
                                    case AttribFormat.Format_32_32_Single:
                                    case AttribFormat.Format_32_Single:
                                    case AttribFormat.Format_8_8_8_8_UInt:
                                    case AttribFormat.Format_8_8_8_8_UNorm:
                                    case AttribFormat.Format_8_UInt:
                                    case AttribFormat.Format_8_8_UInt:
                                    case AttribFormat.Format_32_32_UInt:
                                    case AttribFormat.Format_32_32_32_UInt:
                                    case AttribFormat.Format_32_UInt:
                                        v.nodes.Add(new Vector4 { X = i.X, Y = i.Y, Z = i.Z, W = i.W });
                                        break;
                                    default:
                                        MessageBox.Show("Unsupported Format " + i0.Format);
                                        break;
                                }
                            }
                        }
                    }
                    poly.vertices = v;

                    //  int LODCount = shp.Meshes.Count - 1; //For going to the lowest poly LOD mesh

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
                        TextureName = mdl.Materials[shp.MaterialIndex].TextureRefs[id].Name;
                        //Need to have default texture load instead of random texture if texture doesn't exist within the tex index

                        //    model.mat[shp.MaterialIndex].TextureMaps.Add(TextureName);

                        MatTexture texture = new MatTexture();

                        bool IsAlbedo = HackyTextureList.Any(TextureName.Contains);

                        //This works decently for now. I tried samplers but Kirby Star Allies doesn't map with samplers properly? 
                        if (IsAlbedo)
                        {
                            if (AlbedoCount == 0)
                            {
                                try
                                {
                                    poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                    AlbedoCount++;
                                }
                                catch
                                {
                                    poly.texHashs.Add(0);
                                }
                                poly.TextureMapTypes.Add("Diffuse");
                            }
                        }
                        else if (TextureName.Contains("Nrm") || TextureName.Contains("Norm") || TextureName.Contains("norm") || TextureName.Contains("nrm"))
                        {
                            try
                            {
                                poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                            }
                            catch
                            {
                                poly.texHashs.Add(1);
                            }
                            poly.material.HasNormalMap = true;
                            poly.TextureMapTypes.Add("Normal");
                        }
                        else if (TextureName.Contains("Emm"))
                        {
                            try
                            {
                                poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                            }
                            catch
                            {
                                poly.texHashs.Add(2);
                            }
                            poly.TextureMapTypes.Add("EmissionMap");
                        }
                        else if (TextureName.Contains("Spm"))
                        {
                            try
                            {
                                poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                            }
                            catch
                            {
                                poly.texHashs.Add(2);
                            }
                            poly.TextureMapTypes.Add("SpecularMap");
                        }
                        else if (TextureName.Contains("b00"))
                        {
                            try
                            {
                                poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                            }
                            catch
                            {
                                poly.texHashs.Add(2);
                            }
                            poly.TextureMapTypes.Add("Bake1");
                        }
                        else if (TextureName.Contains("b01") || TextureName.Contains("Moc"))
                        {
                            try
                            {
                                poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                            }
                            catch
                            {
                                poly.texHashs.Add(3);
                            }
                            poly.TextureMapTypes.Add("Bake2");
                        }
                        else if (TextureName.Contains("MRA")) //Metalness, Roughness, and Cavity Map in one
                        {
                            try
                            {
                                poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                            }
                            catch
                            {
                                poly.texHashs.Add(7);
                            }
                            poly.TextureMapTypes.Add("MRA");
                        }
                        else if (TextureName.Contains("mtl"))
                        {
                            try
                            {
                                poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                            }
                            catch
                            {
                                poly.texHashs.Add(8);
                            }
                            poly.TextureMapTypes.Add("Metalness");
                        }
                        else if (TextureName.Contains("rgh"))
                        {
                            try
                            {
                                poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                            }
                            catch
                            {
                                poly.texHashs.Add(9);
                            }
                            poly.TextureMapTypes.Add("Roughness");
                        }
                        texture.Name = TextureName;
                        id++;
                    }

                    poly.material.Name = mdl.Materials[shp.MaterialIndex].Name;

                    foreach (Sampler smp in mdl.Materials[shp.MaterialIndex].Samplers)
                    {
                        SamplerInfo s = new SamplerInfo();
                        s.WrapModeU = (int)smp.WrapModeU;
                        s.WrapModeV = (int)smp.WrapModeV;
                        s.WrapModeW = (int)smp.WrapModeW;
                        poly.material.samplerinfo.Add(s);
                    }


                    foreach (RenderInfo rnd in mdl.Materials[shp.MaterialIndex].RenderInfos)
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
                    }

                    using (Syroot.BinaryData.BinaryDataReader reader = new Syroot.BinaryData.BinaryDataReader(new MemoryStream(mdl.Materials[shp.MaterialIndex].ShaderParamData)))
                    {

                        reader.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
                        foreach (Syroot.NintenTools.NSW.Bfres.ShaderParam param in mdl.Materials[shp.MaterialIndex].ShaderParams)
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
                }
                models.Add(model);
                ModelCur++;
            }

        }

        public void SaveFile(string FileName)
        {

            TargetWiiUBFRES.Save(FileName);
        }
    }
}