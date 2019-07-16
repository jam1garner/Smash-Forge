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
            public int sortBias = 0;
            public bool billboardY = false;
            public bool billboard = false;
            public bool useNsc = false;

            public bool sortByObjHierarchy = true;
            public float[] boundingSphere = new float[8];
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
                Vector3 cen1 = new Vector3(0,0,0), cen2 = new Vector3(0,0,0);
                double rad1 = 0, rad2 = 0;

                //Get first vert
                int vertCount = 0;
                Vector3 vert0 = new Vector3();
                foreach (Polygon p in Nodes)
                {
                    foreach (Vertex v in p.vertices)
                    {
                        vert0 = v.pos;
                        vertCount++;
                        break;
                    }
                    break;
                }

                if (vertCount == 0)
                    return;

                //Calculate average and min/max
                Vector3 min = new Vector3(vert0);
                Vector3 max = new Vector3(vert0);

                vertCount = 0;
                foreach (Polygon p in Nodes)
                {
                    foreach(Vertex v in p.vertices)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            min[i] = Math.Min(min[i], v.pos[i]);
                            max[i] = Math.Max(max[i], v.pos[i]);
                        }

                        cen1 += v.pos;
                        vertCount++;
                    }
                }

                cen1 /= vertCount;
                for (int i = 0; i < 3; i++)
                    cen2[i] = (min[i]+max[i])/2;

                //Calculate the radius of each
                double dist1, dist2;
                foreach (Polygon p in Nodes)
                {
                    foreach (Vertex v in p.vertices)
                    {
                        dist1 = ((Vector3)(v.pos - cen1)).Length;
                        if (dist1 > rad1)
                            rad1 = dist1;

                        dist2 = ((Vector3)(v.pos - cen2)).Length;
                        if (dist2 > rad2)
                            rad2 = dist2;
                    }
                }

                // Use the one with the lowest radius.
                Vector3 temp;
                double radius;
                if (rad1 < rad2)
                {
                    temp = cen1;
                    radius = rad1;
                }
                else
                {
                    temp = cen2;
                    radius = rad2;
                }

                // Set
                for (int i = 0; i < 3; i++)
                {
                    boundingSphere[i] = temp[i];
                    boundingSphere[i+4] = temp[i];
                }
                boundingSphere[3] = (float)radius;
                boundingSphere[7] = 0;
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

            private int CalculateSortBias()
            {
                if (!(Text.Contains("SORTBIAS")))
                    return 0;

                // Isolate the integer value from the mesh name.
                string sortBiasKeyWord = "SORTBIAS";
                string sortBiasText = GetSortBiasNumbers(sortBiasKeyWord);

                int sortBiasValue = 0;
                int.TryParse(sortBiasText, out sortBiasValue);

                return sortBiasValue;
            }

            private string GetSortBiasNumbers(string sortBiasKeyWord)
            {
                // HACK: Just ignore the 'm' prefix.
                string modifiedText = Text.Replace("m", "");

                string numbers = "";
                for (int i = modifiedText.IndexOf(sortBiasKeyWord) + sortBiasKeyWord.Length; i < Text.Length; i++)
                {
                    if (modifiedText[i] != '_')
                        numbers += modifiedText[i];
                    else
                        break;
                }

                return numbers;
            }

            public void SetMeshAttributesFromName()
            {
                sortBias = CalculateSortBias();
                billboard = Text.Contains("BILLBOARD");
                billboardY = Text.Contains("BILLBOARDYAXIS");
                useNsc = Text.Contains("NSC");
                sortByObjHierarchy = Text.Contains("HIR");
            }
        }
    }
}

