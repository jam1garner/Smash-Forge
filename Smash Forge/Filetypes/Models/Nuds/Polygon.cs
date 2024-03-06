using OpenTK;
using SFGraphics.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SmashForge
{
    public partial class Nud
    {
        public class Polygon : TreeNode, IBoundableModel
        {
            public enum BoneTypes
            {
                NoBones =  0x00,
                Float = 0x10,
                HalfFloat = 0x20,
                Byte = 0x40
            }

            public enum VertexTypes
            {
                NoNormals = 0x0,
                NormalsFloat = 0x1,
                NormalsTanBiTanFloat = 0x3,
                NormalsHalfFloat = 0x6,
                NormalsTanBiTanHalfFloat = 0x7
            }

            public enum VertexColorTypes
            {
                None = 0,
                Byte = 2,
                HalfFloat = 4
            }

            // Smash uses both of these, but primarily Triangles. Other NU3G games only use Tristrips
            public enum PrimitiveTypes
            {
                TriangleStrip = 0x0,
                Triangles = 0x40
            }

            // Used to generate a unique color for viewport selection.
            private static List<int> previousDisplayIds = new List<int>();
            public int DisplayId { get; private set; } = 0;

            // The number of vertices is vertexIndices.Count because many vertices are shared.
            public List<Vertex> vertices = new List<Vertex>();
            public List<int> vertexIndices = new List<int>();
            public int displayFaceSize = 0;

            public NudRenderMesh renderMesh;

            public List<Material> materials = new List<Material>();


            // defaults to a basic bone weighted vertex format
            public int boneType = (int)BoneTypes.Byte;
            public int normalType = (int)VertexTypes.NormalsHalfFloat;
            // boneType is upper nybble, normalType is lower nybble
            public int vertSize {
                get { return (boneType & 0xF0) | (normalType & 0xF); }
                set { boneType = value & 0xF0; normalType = value & 0xF; }
            }

            public int uvCount = 1;
            public int colorType = (int)VertexColorTypes.Byte;
            // uvCount is upper nybble, normalType is lower nybble
            public int UVSize {
                get { return (uvCount << 4) | (colorType & 0xF); }
                set { uvCount = value >> 4; colorType = value & 0xF; }
            }

            public int strip = (int)PrimitiveTypes.Triangles;
            public int polflag {
                get { return boneType > 0 ? 4 : 0; }
                set {
                    if (value == 0 && boneType == 0) {}
                    else if (value == 4 && boneType != 0) {}
                    else throw new NotImplementedException("Poly flag not supported " + value);
                }
            }


            // for drawing
            public bool IsTransparent => (materials[0].SrcFactor > 0) || (materials[0].DstFactor > 0);
            public int[] display;
            public int[] selectedVerts;

            public Polygon()
            {
                Checked = true;
                Text = "Polygon";
                ImageKey = "polygon";
                SelectedImageKey = "polygon";
                GenerateDisplayId();
            }

            public void AddVertex(Vertex v)
            {
                vertices.Add(v);
            }

            public Vector4 BoundingSphere
            {
                get
                {
                    Mesh parent = (Mesh)Parent;
                    if (parent != null)
                        return new Vector4(parent.BoundingSphere);
                    else
                        return new Vector4(0, 0, 0, 100);
                }
            }

            private void GenerateDisplayId()
            {
                // Find last used ID. Next ID will be last ID + 1.
                // A color is generated from the integer as hexadecimal, but alpha is ignored.
                // Incrementing will affect RGB before it affects Alpha (ARGB color).
                int index = 0;
                if (previousDisplayIds.Count > 0)
                    index = previousDisplayIds.Last();
                index++;
                previousDisplayIds.Add(index);
                DisplayId = index;
            }

            public void AOSpecRefBlend()
            {
                // change aomingain to only affect specular and reflection. ignore 2nd material
                if (materials[0].HasProperty("NU_aoMinGain"))
                {
                    materials[0].GetPropertyValues("NU_aoMinGain")[0] = 15.0f;
                    materials[0].GetPropertyValues("NU_aoMinGain")[1] = 15.0f;
                    materials[0].GetPropertyValues("NU_aoMinGain")[2] = 15.0f;
                    materials[0].GetPropertyValues("NU_aoMinGain")[3] = 0.0f;
                }
            }

            public void GetDisplayVerticesAndIndices(out List<DisplayVertex> displayVerticesList, out List<int> vertexIndicesList)
            {
                displayVerticesList = CreateDisplayVertices();
                vertexIndicesList = new List<int>(display);
            }

            private List<DisplayVertex> CreateDisplayVertices()
            {
                // rearrange faces
                display = GetRenderingVertexIndices().ToArray();

                List<DisplayVertex> displayVertList = new List<DisplayVertex>();

                if (vertexIndices.Count < 3)
                    return displayVertList;
                foreach (Vertex v in vertices)
                {
                    DisplayVertex displayVert = new DisplayVertex()
                    {
                        pos = v.pos,
                        nrm = v.nrm,
                        tan = v.tan.Xyz,
                        bit = v.bitan.Xyz,
                        col = v.color / 127,
                        uv = v.uv.Count > 0 ? v.uv[0] : new Vector2(0, 0),
                        uv2 = v.uv.Count > 1 ? v.uv[1] : new Vector2(0, 0),
                        uv3 = v.uv.Count > 2 ? v.uv[2] : new Vector2(0, 0),
                        boneIds = new Vector4I 
                        (
                            v.boneIds.Count > 0 ? v.boneIds[0] : -1,
                            v.boneIds.Count > 1 ? v.boneIds[1] : -1,
                            v.boneIds.Count > 2 ? v.boneIds[2] : -1,
                            v.boneIds.Count > 3 ? v.boneIds[3] : -1
                        ),
                        weight = new Vector4(
                            v.boneWeights.Count > 0 ? v.boneWeights[0] : 0,
                            v.boneWeights.Count > 1 ? v.boneWeights[1] : 0,
                            v.boneWeights.Count > 2 ? v.boneWeights[2] : 0,
                            v.boneWeights.Count > 3 ? v.boneWeights[3] : 0),
                    };
                    displayVertList.Add(displayVert);
                }

                selectedVerts = new int[displayVertList.Count];
                return displayVertList;
            }

            public void CalculateTangentBitangent()
            {
                // Don't generate tangents and bitangents if the vertex format doesn't support them. 
                int vertType = vertSize & 0xF;
                if (!(vertType == 3 || vertType == 7))
                    return;

                List<int> vertexIndices = GetRenderingVertexIndices();

                Vector3[] tangents;
                Vector3[] bitangents;
                TriangleListUtils.CalculateTangentsBitangents(GetPositions(), GetNormals(), GetUv0(), vertexIndices, out tangents, out bitangents);

                ApplyTanBitanArray(tangents, bitangents);
            }

            public void SetVertexColor(Vector4 intColor)
            {
                // (127, 127, 127, 127) is white.
                foreach (Vertex v in vertices)
                {
                    v.color = intColor;
                }
            }


            private void ApplyTanBitanArray(Vector3[] tanArray, Vector3[] bitanArray)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    vertices[i].tan = new Vector4(tanArray[i], 1);
                    vertices[i].bitan = new Vector4(bitanArray[i], 1);
                }
            }

            public void SmoothNormals()
            {
                Vector3[] normals = new Vector3[vertices.Count];

                List<int> f = GetRenderingVertexIndices();

                for (int i = 0; i < displayFaceSize; i += 3)
                {
                    Vertex v1 = vertices[f[i]];
                    Vertex v2 = vertices[f[i+1]];
                    Vertex v3 = vertices[f[i+2]];
                    Vector3 nrm = VectorUtils.CalculateNormal(v1.pos, v2.pos, v3.pos);

                    normals[f[i + 0]] += nrm;
                    normals[f[i + 1]] += nrm;
                    normals[f[i + 2]] += nrm;
                }
                
                for (int i = 0; i < normals.Length; i++)
                    vertices[i].nrm = normals[i].Normalized();

                // Compare each vertex with all the remaining vertices. This might skip some.
                for (int i = 0; i < vertices.Count; i++)
                {
                    Vertex v = vertices[i];

                    for (int j = i + 1; j < vertices.Count; j++)
                    {
                        Vertex v2 = vertices[j];

                        if (v == v2)
                            continue;
                        float dis = (float)Math.Sqrt(Math.Pow(v.pos.X - v2.pos.X, 2) + Math.Pow(v.pos.Y - v2.pos.Y, 2) + Math.Pow(v.pos.Z - v2.pos.Z, 2));
                        if (dis <= 0f) // Extra smooth
                        {
                            Vector3 nn = ((v2.nrm + v.nrm) / 2).Normalized();
                            v.nrm = nn;
                            v2.nrm = nn;
                        }
                    }
                }
            }

            private List<Vector3> GetPositions()
            {
                var values = new List<Vector3>();
                foreach (var vertex in vertices)
                {
                    values.Add(vertex.pos);
                }
                return values;
            }

            private List<Vector3> GetNormals()
            {
                var values = new List<Vector3>();
                foreach (var vertex in vertices)
                {
                    values.Add(vertex.nrm);
                }
                return values;
            }


            private List<Vector2> GetUv0()
            {
                var values = new List<Vector2>();
                foreach (var vertex in vertices)
                {
                    values.Add(vertex.uv[0]);
                }
                return values;
            }

            public void CalculateNormals()
            {
                Vector3[] normals = new Vector3[vertices.Count];

                for (int i = 0; i < normals.Length; i++)
                    normals[i] = new Vector3(0, 0, 0);

                List<int> f = GetRenderingVertexIndices();

                for (int i = 0; i < displayFaceSize; i += 3)
                {
                    Vertex v1 = vertices[f[i]];
                    Vertex v2 = vertices[f[i + 1]];
                    Vertex v3 = vertices[f[i + 2]];
                    Vector3 nrm = VectorUtils.CalculateNormal(v1.pos, v2.pos, v3.pos);

                    normals[f[i + 0]] += nrm * (nrm.Length / 2);
                    normals[f[i + 1]] += nrm * (nrm.Length / 2);
                    normals[f[i + 2]] += nrm * (nrm.Length / 2);
                }

                for (int i = 0; i < normals.Length; i++)
                    vertices[i].nrm = normals[i].Normalized();
            }

            public void AddDefaultMaterial()
            {
                Material mat = Material.GetDefault();
                materials.Add(mat);
                mat.textures.Add(new MatTexture(0x10000000));
                mat.textures.Add(MatTexture.GetDefault());
            }

            public List<int> GetRenderingVertexIndices()
            {
                if (strip == (int)PrimitiveTypes.Triangles)
                {
                    displayFaceSize = vertexIndices.Count;
                    return vertexIndices;
                }
                else if (strip == (int)PrimitiveTypes.TriangleStrip)
                {
                    List<int> vertexIndices = new List<int>();

                    int startDirection = 1;
                    int p = 0;
                    int f1 = this.vertexIndices[p++];
                    int f2 = this.vertexIndices[p++];
                    int faceDirection = startDirection;
                    int f3;
                    do
                    {
                        f3 = this.vertexIndices[p++];
                        if (f3 == 0xFFFF)
                        {
                            f1 = this.vertexIndices[p++];
                            f2 = this.vertexIndices[p++];
                            faceDirection = startDirection;
                        }
                        else
                        {
                            faceDirection *= -1;
                            if ((f1 != f2) && (f2 != f3) && (f3 != f1))
                            {
                                if (faceDirection > 0)
                                {
                                    vertexIndices.Add(f3);
                                    vertexIndices.Add(f2);
                                    vertexIndices.Add(f1);
                                }
                                else
                                {
                                    vertexIndices.Add(f2);
                                    vertexIndices.Add(f3);
                                    vertexIndices.Add(f1);
                                }
                            }
                            f1 = f2;
                            f2 = f3;
                        }
                    } while (p < this.vertexIndices.Count);

                    displayFaceSize = vertexIndices.Count;
                    return vertexIndices;
                }
                else
                {
                    throw new NotImplementedException("Face type not supported: " + strip);
                }
            }
        }
    }
}

