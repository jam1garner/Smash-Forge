using System;
using System.Linq;
using System.Collections.Generic;
using OpenTK;
using System.Windows.Forms;


namespace SmashForge
{
    public partial class Nud : IBoundableModel
    {
        // typically a mesh will just have 1 polygon
        // but you can just use the mesh class without polygons
        public class Mesh : TreeNode, IBoundableModel
        {
            public enum BoneFlags
            {
                NotRigged = 0,
                Rigged = 4,
                SingleBind = 8
            }

            // Used to generate a unique color for mesh viewport selection.
            private static readonly List<int> previousDisplayIds = new List<int>();
            public int DisplayId { get; private set; } = 0;

            public int boneflag = (int)BoneFlags.Rigged;
            public short singlebind = -1;
            public bool billboardY = false;
            public bool billboard = false;
            public bool useNsc = false;

            public bool sortByObjHierarchy = true;
            public float[] boundingSphere = new float[4];
            public float sortBias = 0;
            public float sortingDistance = 0;

            public Mesh()
            {
                Checked = true;
                ImageKey = "mesh";
                SelectedImageKey = "mesh";
                GenerateDisplayId();
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

            public Vector4 BoundingSphere
            {
                get { return new Vector4(boundingSphere[0], boundingSphere[1], boundingSphere[2], boundingSphere[3]); }
            }

            public void addVertex(Vertex v)
            {
                if (Nodes.Count == 0)
                    Nodes.Add(new Polygon());

                ((Polygon)Nodes[0]).AddVertex(v);
            }

            public void GenerateBoundingSphere()
            {
                bool initial = false;
                Vector3 min = new Vector3();
                Vector3 max = new Vector3();
                foreach (Polygon p in Nodes)
                {
                    foreach(Vertex v in p.vertices)
                    {
                        if (!initial)
                        {
                            min = new Vector3(v.pos);
                            max = new Vector3(v.pos);
                            initial = true;
                        }
                        else
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                min[i] = Math.Min(min[i], v.pos[i]);
                                max[i] = Math.Max(max[i], v.pos[i]);
                            }
                        }
                    }
                }

                Vector3 center = ((Vector3)(min + max)) / 2;
                double radius = 0.0;
                foreach (Polygon p in Nodes)
                {
                    foreach (Vertex v in p.vertices)
                    {
                        radius = Math.Max(radius, ((Vector3)(v.pos - center)).Length);
                    }
                }

                for (int i = 0; i < 3; i++)
                    boundingSphere[i] = center[i];
                boundingSphere[3] = (float)radius;
            }

            public float CalculateSortingDistance(Vector3 cameraPosition)
            {
                Vector3 meshCenter = new Vector3(boundingSphere[0], boundingSphere[1], boundingSphere[2]);
                // TODO: How does this work?
                if (useNsc && singlebind != -1)
                {
                    // Use the bone position as the bounding box center
                    ModelContainer modelContainer = (ModelContainer)Parent.Parent;
                    if (modelContainer.VBN != null)
                        meshCenter = modelContainer.VBN.bones[singlebind].pos;
                }

                Vector3 distanceVector = new Vector3(cameraPosition - meshCenter);
                return distanceVector.Length + boundingSphere[3] + sortBias;
            }

            public void SetMeshAttributesFromName()
            {
                billboard = Text.Contains("BILLBOARD");
                billboardY = Text.Contains("BILLBOARDYAXIS");
                useNsc = Text.Contains("NSC");
                sortByObjHierarchy = Text.Contains("HIR");
            }
        }
    }
}

