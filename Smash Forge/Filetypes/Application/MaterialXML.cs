using System;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;
using System.Windows.Forms;


namespace SmashForge
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

        public static void ExportMaterialAsXml(Nud n, string filename)
        {
            XmlDocument doc = new XmlDocument();

            XmlNode mainNode = doc.CreateElement("NUDMATERIAL");
            XmlAttribute polycount = doc.CreateAttribute("polycount");
            mainNode.Attributes.Append(polycount);
            doc.AppendChild(mainNode);

            int polyCount = 0;

            foreach (Nud.Mesh m in n.Nodes)
            {
                XmlNode meshnode = doc.CreateElement("mesh");
                XmlAttribute name = doc.CreateAttribute("name"); name.Value = m.Text; meshnode.Attributes.Append(name);
                mainNode.AppendChild(meshnode);
                foreach (Nud.Polygon p in m.Nodes)
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

        private static void WriteMaterials(XmlDocument doc, Nud.Polygon p, XmlNode polynode)
        {
            foreach (Nud.Material mat in p.materials)
            {
                XmlNode matnode = doc.CreateElement("material");
                polynode.AppendChild(matnode);

                WriteMatAttributes(doc, mat, matnode);
                WriteTextureAttributes(doc, mat, matnode);
                WriteMatParams(doc, mat, matnode);
            }
        }

        private static void WriteMatAttributes(XmlDocument doc, Nud.Material mat, XmlNode matNode)
        {
            AddUintAttribute(doc, "flags", mat.Flags, matNode, true);
            AddIntAttribute(doc, "srcFactor", mat.SrcFactor, matNode, false);
            AddIntAttribute(doc, "dstFactor", mat.DstFactor, matNode, false);
            AddIntAttribute(doc, "AlphaFunc", mat.AlphaFunction, matNode, false);
            AddIntAttribute(doc, "AlphaTest", mat.AlphaTest, matNode, false);
            AddIntAttribute(doc, "RefAlpha", mat.RefAlpha, matNode, false);
            AddIntAttribute(doc, "cullmode", mat.CullMode, matNode, true);
            AddIntAttribute(doc, "zbuffoff", mat.ZBufferOffset, matNode, false);
        }

        private static void WriteTextureAttributes(XmlDocument doc, Nud.Material mat, XmlNode matnode)
        {
            foreach (Nud.MatTexture tex in mat.textures)
            {
                XmlNode texnode = doc.CreateElement("texture");

                AddIntAttribute(doc, "hash",      tex.hash,      texnode, true);
                AddIntAttribute(doc, "wrapmodeS", tex.wrapModeS, texnode, true);
                AddIntAttribute(doc, "wrapmodeT", tex.wrapModeT, texnode, true);
                AddIntAttribute(doc, "minfilter", tex.minFilter, texnode, true);
                AddIntAttribute(doc, "magfilter", tex.magFilter, texnode, true);
                AddIntAttribute(doc, "mipdetail", tex.mipDetail, texnode, true);

                matnode.AppendChild(texnode);
            }
        }

        private static void WriteMatParams(XmlDocument doc, Nud.Material mat, XmlNode matnode)
        {
            foreach (string materialProperty in mat.PropertyNames)
            {
                XmlNode paramnode = doc.CreateElement("param");
                XmlAttribute a = doc.CreateAttribute("name"); a.Value = materialProperty; paramnode.Attributes.Append(a);
                matnode.AppendChild(paramnode);

                if (materialProperty == "NU_materialHash")
                {
                    // Material hash should be in hex for easier reading.
                    foreach (float f in mat.GetPropertyValues(materialProperty))
                        paramnode.InnerText += BitConverter.ToUInt32(BitConverter.GetBytes(f), 0).ToString("x") + " ";
                }
                else
                {
                    int count = 0;
                    foreach (float f in mat.GetPropertyValues(materialProperty))
                    {
                        // Only print 4 values and avoids tons of trailing 0's.
                        if (count <= 4)
                            paramnode.InnerText += f.ToString() + " ";
                        count += 1;
                    }

                }

            }
        }

        private static void AddIntAttribute(XmlDocument doc, string name, int value, XmlNode node, bool useHex)
        {
            XmlAttribute attribute = doc.CreateAttribute(name);
            if (useHex)
                attribute.Value = value.ToString("x");
            else
                attribute.Value = value.ToString();

            node.Attributes.Append(attribute);
        }

        private static void AddUintAttribute(XmlDocument doc, string name, uint value, XmlNode node, bool useHex)
        {
            XmlAttribute attribute = doc.CreateAttribute(name);
            if (useHex)
                attribute.Value = value.ToString("x");
            else
                attribute.Value = value.ToString();

            node.Attributes.Append(attribute);
        }

        public static void ImportMaterialAsXml(Nud n, string filename)
        {
            // Creates a list of materials and then trys to apply the materials to the polygons. 
            int polyCount = CalculatePolygonCount(n);

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            List<Nud.Material> materialList = new List<Nud.Material>();
            List<int> matCountForPolyId = new List<int>();

            XmlNode main = doc.ChildNodes[0];

            foreach (XmlNode meshNode in main.ChildNodes)
            {
                if (!(meshNode.Name.Equals("mesh")))
                    continue;

                foreach (XmlNode polynode in meshNode.ChildNodes)
                {
                    if (!(polynode.Name.Equals("polygon")))
                        continue;
                  
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

            ApplyMaterials(n, materialList, matCountForPolyId);
        }

        private static int CalculatePolygonCount(Nud n)
        {
            int polyCount = 0;
            foreach (Nud.Mesh m in n.Nodes)
            {
                foreach (Nud.Polygon p in m.Nodes)
                {
                    polyCount++;
                }
            }

            return polyCount;
        }

        private static void ApplyMaterials(Nud n, List<Nud.Material> materialList, List<int> polyMatCount)
        {
            int matIndex = 0;
            int polyIndex = 0;

            foreach (Nud.Mesh m in n.Nodes)
            {
                foreach (Nud.Polygon p in m.Nodes)
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

        private static void ReadMaterials(List<Nud.Material> materialList, XmlNode polyNode)
        {
            foreach (XmlNode matnode in polyNode.ChildNodes)
            {
                if (matnode.Name.Equals("material"))
                {
                    Nud.Material mat = new Nud.Material();
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

        private static void ReadAttributes(XmlNode materialNode, Nud.Material material)
        {
            int value = 0;
            foreach (XmlAttribute attribute in materialNode.Attributes)
            {
                switch (attribute.Name)
                {
                    case "flags":
                        uint newFlags = 0;
                        if (uint.TryParse(attribute.Value, NumberStyles.HexNumber, null, out newFlags))
                            material.Flags = newFlags;
                        break;
                    case "srcFactor":
                        int.TryParse(attribute.Value, out value);
                        material.SrcFactor = value;
                        break;
                    case "dstFactor":
                        int.TryParse(attribute.Value, out value);
                        material.DstFactor = value;
                        break;
                    case "AlphaFunc":
                        int.TryParse(attribute.Value, out value);
                        material.AlphaFunction = value;
                        break;
                    case "AlphaTest":
                        int.TryParse(attribute.Value, out value);
                        material.AlphaTest = value;
                        break;
                    case "RefAlpha":
                        int.TryParse(attribute.Value, out value);
                        material.RefAlpha = value;
                        break;
                    case "cullmode":
                        int.TryParse(attribute.Value, NumberStyles.HexNumber, null, out value);
                        material.CullMode = value;
                        break;
                    case "zbuffoff":
                        int.TryParse(attribute.Value, out value);
                        material.ZBufferOffset = value;
                        break;
                }
            }
        }

        private static void ReadTextures(Nud.Material material, XmlNode textureNode)
        {
            if (!(textureNode.Name.Equals("texture")))
                return;
            
            Nud.MatTexture matTexture = new Nud.MatTexture();
            material.textures.Add(matTexture);

            foreach (XmlAttribute attribute in textureNode.Attributes)
            {
                switch (attribute.Name)
                {
                    case "hash":
                        int.TryParse(attribute.Value, NumberStyles.HexNumber, null, out matTexture.hash);
                        break;
                    case "wrapmodeS":
                        int.TryParse(attribute.Value, out matTexture.wrapModeS);
                        break;
                    case "wrapmodeT":
                        int.TryParse(attribute.Value, out matTexture.wrapModeT);
                        break;
                    case "minfilter":
                        int.TryParse(attribute.Value, out matTexture.minFilter);
                        break;
                    case "magfilter":
                        int.TryParse(attribute.Value, out matTexture.magFilter);
                        break;
                    case "mipdetail":
                        int.TryParse(attribute.Value, out matTexture.mipDetail);
                        break;
                }
            }
        }

        private static void ReadMatParams(XmlNode polyNode, Nud.Material material, XmlNode materialNode)
        {
            if (!materialNode.Name.Equals("param"))
                return;

            string name = GetNodeName(materialNode);
            List<float> valueList = ParamValuesFromMaterialPropertyText(materialNode, name);

            // Parameters should always have 4 values.                                           
            if (valueList.Count != 4)
                throw new ParamArrayLengthException(polyNode.ChildNodes.Count, name);

            // Prevents duplicate material parameters.
            if (!material.HasProperty(name))
            {
                material.UpdateProperty(name, valueList.ToArray());
            }
            else
            {
                MessageBox.Show(String.Format("Polygon{0} contains more than 1 instance of {1}. \n"
                    + "Only the first instance of {1} will be added.", polyNode.ChildNodes.Count.ToString(), name));
            }        
        }

        private static List<float> ParamValuesFromMaterialPropertyText(XmlNode materialPropertyNode, string propertyName)
        {
            // Only get the values and ignore any white space.
            string[] stringValues = materialPropertyNode.InnerText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            List<float> valueList = new List<float>();
            foreach (string stringValue in stringValues)
            {
                // Only read 4 values.
                if (valueList.Count >= 4)
                    break;

                float newValue = 0;
                if (propertyName == "NU_materialHash")
                {
                    int hash;
                    if (int.TryParse(stringValue, NumberStyles.HexNumber, null, out hash))
                    {
                        newValue = BitConverter.ToSingle(BitConverter.GetBytes(hash), 0);
                        valueList.Add(newValue);
                    }
                }
                else if (float.TryParse(stringValue, out newValue))
                    valueList.Add(newValue);
                else
                    valueList.Add(0.0f);            
            }

            return valueList;
        }

        private static string GetNodeName(XmlNode node)
        {
            foreach (XmlAttribute attribute in node.Attributes)
            {
                if (attribute.Name == "name")
                {
                    return attribute.Value;
                }
            }

            return "";
        }
    }
}
