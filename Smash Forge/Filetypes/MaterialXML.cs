using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;
using System.Windows.Forms;
using System.Diagnostics;


namespace Smash_Forge
{
    class MaterialXML
    {
        public class PolyCountException : Exception
        {

        }

        public class ParamArrayLengthException : Exception
        {
            public string errorMessage = "";

            public ParamArrayLengthException(int polyID, string property)
            {
                errorMessage = String.Format("Polygon{0} does not contain 4 valid values for {1}.", polyID, property);
            }
        }

        public static void ExportMaterialAsXml(NUD n, string filename)
        {
            XmlDocument doc = new XmlDocument();

            XmlNode mainNode = doc.CreateElement("NUDMATERIAL");
            XmlAttribute polycount = doc.CreateAttribute("polycount");
            mainNode.Attributes.Append(polycount);
            doc.AppendChild(mainNode);

            int polyCount = 0;

            foreach (NUD.Mesh m in n.Nodes)
            {
                XmlNode meshnode = doc.CreateElement("mesh");
                XmlAttribute name = doc.CreateAttribute("name"); name.Value = m.Text; meshnode.Attributes.Append(name);
                mainNode.AppendChild(meshnode);
                foreach (NUD.Polygon p in m.Nodes)
                {
                    XmlNode polyNode = doc.CreateElement("polygon");
                    XmlAttribute pid = doc.CreateAttribute("id"); pid.Value = polyCount.ToString(); polyNode.Attributes.Append(pid);
                    meshnode.AppendChild(polyNode);

                    WriteMaterials(doc, p, polyNode);

                    polyCount++;
                }
            }
            polycount.Value = polyCount.ToString();

            doc.Save(filename);
        }

        private static void WriteMaterials(XmlDocument doc, NUD.Polygon p, XmlNode polynode)
        {
            foreach (NUD.Material mat in p.materials)
            {
                XmlNode matnode = doc.CreateElement("material");
                polynode.AppendChild(matnode);

                WriteMatAttributes(doc, mat, matnode);
                WriteTextureAttributes(doc, mat, matnode);
                WriteMatParams(doc, mat, matnode);
            }
        }

        private static void WriteMatAttributes(XmlDocument doc, NUD.Material mat, XmlNode matNode)
        {
            AddUintAttribute(doc, "flags", mat.Flags, matNode, true);
            AddIntAttribute(doc, "srcFactor", mat.srcFactor, matNode, false);
            AddIntAttribute(doc, "dstFactor", mat.dstFactor, matNode, false);
            AddIntAttribute(doc, "AlphaFunc", mat.AlphaFunc, matNode, false);
            AddIntAttribute(doc, "AlphaTest", mat.AlphaTest, matNode, false);
            AddIntAttribute(doc, "RefAlpha", mat.RefAlpha, matNode, false);
            AddIntAttribute(doc, "cullmode", mat.cullMode, matNode, true);
            AddIntAttribute(doc, "zbuffoff", mat.zBufferOffset, matNode, false);
        }

        private static void WriteTextureAttributes(XmlDocument doc, NUD.Material mat, XmlNode matnode)
        {
            foreach (NUD.MatTexture tex in mat.textures)
            {
                XmlNode texnode = doc.CreateElement("texture");

                AddIntAttribute(doc, "hash", tex.hash, texnode, true);
                AddIntAttribute(doc, "wrapmodeS", tex.WrapModeS, texnode, true);
                AddIntAttribute(doc, "wrapmodeT", tex.WrapModeT, texnode, true);
                AddIntAttribute(doc, "minfilter", tex.minFilter, texnode, true);
                AddIntAttribute(doc, "magfilter", tex.magFilter, texnode, true);
                AddIntAttribute(doc, "mipdetail", tex.mipDetail, texnode, true);

                matnode.AppendChild(texnode);
            }
        }

        private static void WriteMatParams(XmlDocument doc, NUD.Material mat, XmlNode matnode)
        {
            foreach (KeyValuePair<string, float[]> k in mat.entries)
            {
                XmlNode paramnode = doc.CreateElement("param");
                XmlAttribute a = doc.CreateAttribute("name"); a.Value = k.Key; paramnode.Attributes.Append(a);
                matnode.AppendChild(paramnode);

                if (k.Key == "NU_materialHash")
                {
                    // material hash should be in hex for easier reading
                    foreach (float f in k.Value)
                        paramnode.InnerText += BitConverter.ToUInt32(BitConverter.GetBytes(f), 0).ToString("x") + " ";
                }
                else
                {
                    int count = 0;
                    foreach (float f in k.Value)
                    {
                        // only need to print 4 values and avoids tons of 0's
                        if (count <= 4)
                            paramnode.InnerText += f.ToString() + " ";
                        count += 1;
                    }

                }

            }
        }

        private static void AddIntAttribute(XmlDocument doc, string name, int value, XmlNode node, bool useHex)
        {
            XmlAttribute a = doc.CreateAttribute(name);
            if (useHex)
                a.Value = value.ToString("x");
            else
                a.Value = value.ToString();

            node.Attributes.Append(a);
        }

        private static void AddUintAttribute(XmlDocument doc, string name, uint value, XmlNode node, bool useHex)
        {
            XmlAttribute a = doc.CreateAttribute(name);
            if (useHex)
                a.Value = value.ToString("x");
            else
                a.Value = value.ToString();

            node.Attributes.Append(a);
        }

        public static void ImportMaterialAsXml(NUD n, string filename)
        {
            // Creates a list of materials and then trys to apply the materials to the polygons. 
            int polyCount = CalculatePolygonCount(n);

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            List<NUD.Material> materialList = new List<NUD.Material>();
            List<int> matCountForPolyId = new List<int>();

            XmlNode main = doc.ChildNodes[0];

            foreach (XmlNode meshNode in main.ChildNodes)
            {
                if (meshNode.Name.Equals("mesh"))
                {
                    foreach (XmlNode polynode in meshNode.ChildNodes)
                    {
                        if (polynode.Name.Equals("polygon"))
                        {
                            matCountForPolyId.Add(polynode.ChildNodes.Count);

                            if (matCountForPolyId.Count > polyCount)
                            {
                                int countDif = matCountForPolyId.Count - polyCount;
                                MessageBox.Show(String.Format("Expected {0} polygons but found {1} in the XML file. " +
                                    "The last {2} polygon(s) will be ignored.",
                                    polyCount, matCountForPolyId.Count, countDif));
                            }

                            ReadMaterials(materialList, polynode);
                        }
                    }
                }
            }

            ApplyMaterials(n, materialList, matCountForPolyId);
        }

        private static int CalculatePolygonCount(NUD n)
        {
            int polyCount = 0;
            foreach (NUD.Mesh m in n.Nodes)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    polyCount++;
                }
            }

            return polyCount;
        }

        private static void ApplyMaterials(NUD n, List<NUD.Material> materialList, List<int> polyMatCount)
        {
            int matIndex = 0;
            int polyIndex = 0;

            foreach (NUD.Mesh m in n.Nodes)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    p.materials.Clear();
                    for (int i = 0; i < polyMatCount[polyIndex]; i++)
                    {
                        if (p.materials.Count < 2)
                        {
                            p.materials.Add(materialList[matIndex]);
                        }
                        matIndex += 1;
                    }
                
                    polyIndex += 1;
                }
            }
        }

        private static void ReadMaterials(List<NUD.Material> materialList, XmlNode polyNode)
        {
            foreach (XmlNode matnode in polyNode.ChildNodes)
            {
                if (matnode.Name.Equals("material"))
                {
                    NUD.Material mat = new NUD.Material();
                    materialList.Add(mat);

                    ReadAttributes(matnode, mat);

                    foreach (XmlNode mnode in matnode.ChildNodes)
                    {
                        if (mnode.Name.Equals("texture"))
                            ReadTextures(mat, mnode);
                        else if (mnode.Name.Equals("param"))
                            ReadMatParams(polyNode, mat, mnode);
                    }
                }
            }
        }

        private static void ReadAttributes(XmlNode matnode, NUD.Material mat)
        {
            foreach (XmlAttribute a in matnode.Attributes)
            {
                switch (a.Name)
                {
                    case "flags": uint f = 0; if (uint.TryParse(a.Value, NumberStyles.HexNumber, null, out f)) { mat.Flags = f; }; break;
                    case "srcFactor": int.TryParse(a.Value, out mat.srcFactor); break;
                    case "dstFactor": int.TryParse(a.Value, out mat.dstFactor); break;
                    case "AlphaFunc": int.TryParse(a.Value, out mat.AlphaFunc); break;
                    case "AlphaTest": int.TryParse(a.Value, out mat.AlphaTest); break;
                    case "RefAlpha": int.TryParse(a.Value, out mat.RefAlpha); break;
                    case "cullmode": int cm = 0; if (int.TryParse(a.Value, NumberStyles.HexNumber, null, out cm)) { mat.cullMode = cm; }; break;
                    case "zbuffoff": int.TryParse(a.Value, out mat.zBufferOffset); break;
                }
            }
        }

        private static void ReadTextures(NUD.Material mat, XmlNode matNode)
        {
            if (matNode.Name.Equals("texture"))
            {
                NUD.MatTexture tex = new NUD.MatTexture();
                mat.textures.Add(tex);

                foreach (XmlAttribute a in matNode.Attributes)
                {
                    switch (a.Name)
                    {
                        case "hash": int f = 0; if (int.TryParse(a.Value, NumberStyles.HexNumber, null, out f)) { tex.hash = f; }; break;
                        case "wrapmodeS": int.TryParse(a.Value, out tex.WrapModeS); break;
                        case "wrapmodeT": int.TryParse(a.Value, out tex.WrapModeT); break;
                        case "minfilter": int.TryParse(a.Value, out tex.minFilter); break;
                        case "magfilter": int.TryParse(a.Value, out tex.magFilter); break;
                        case "mipdetail": int.TryParse(a.Value, out tex.mipDetail); break;
                    }
                }
            }
        }

        private static void ReadMatParams(XmlNode polyNode, NUD.Material mat, XmlNode matNode)
        {
            if (!matNode.Name.Equals("param"))
                return;

            string name = ReadName(matNode);
            List<float> valueList = ReadParamValues(matNode, name);

            // Parameters should always have 4 values.                                           
            if (valueList.Count != 4)
                throw new ParamArrayLengthException(polyNode.ChildNodes.Count, name);

            // Using a dictionary prevents duplicate material parameters.
            try
            {
                mat.entries.Add(name, valueList.ToArray());
            }
            catch (System.ArgumentException)
            {
                MessageBox.Show(String.Format("Polygon{0} contains more than 1 instance of {1}. \n"
                    + "Only the first instance of {1} will be added.", polyNode.ChildNodes.Count.ToString(), name));
            }        
        }

        private static List<float> ReadParamValues(XmlNode matNode, string name)
        {
            string[] values = matNode.InnerText.Split(' ');
            List<float> valueList = new List<float>();
            foreach (string stringValue in values)
            {
                if (valueList.Count >= 4)
                    break;

                float f = 0;

                if (name == "NU_materialHash")
                {
                    int hash;
                    if (int.TryParse(stringValue, NumberStyles.HexNumber, null, out hash))
                    {
                        f = BitConverter.ToSingle(BitConverter.GetBytes(hash), 0);
                        valueList.Add(f);
                    }
                }
                else if (float.TryParse(stringValue, out f))
                    valueList.Add(f);
                else
                    valueList.Add(0.0f);            
            }

            return valueList;
        }

        private static string ReadName(XmlNode node)
        {
            foreach (XmlAttribute a in node.Attributes)
            {
                if (a.Name == "name")
                {
                    return a.Value;
                }
            }

            return "";
        }
    }
}
