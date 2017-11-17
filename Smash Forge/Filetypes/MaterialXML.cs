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
    class MaterialXMLPolycountException : Exception
    {

    }

    class MaterialXMLParamArrayLengthException : Exception
    {
    }

    class MaterialXML
    {
        public static void exportMaterialAsXML(NUD n, string filename)
        {
            XmlDocument doc = new XmlDocument();

            XmlNode mainNode = doc.CreateElement("NUDMATERIAL");
            XmlAttribute polycount = doc.CreateAttribute("polycount");
            mainNode.Attributes.Append(polycount);
            doc.AppendChild(mainNode);

            int polyCount = 0;

            foreach (NUD.Mesh m in n.mesh)
            {
                XmlNode meshnode = doc.CreateElement("mesh");
                XmlAttribute name = doc.CreateAttribute("name"); name.Value = m.Text; meshnode.Attributes.Append(name);
                mainNode.AppendChild(meshnode);
                foreach (NUD.Polygon p in m.Nodes)
                {
                    XmlNode polynode = doc.CreateElement("polygon");
                    XmlAttribute pid = doc.CreateAttribute("id"); pid.Value = polyCount.ToString(); polynode.Attributes.Append(pid);
                    meshnode.AppendChild(polynode);

                    foreach (NUD.Material mat in p.materials)
                    {
                        XmlNode matnode = doc.CreateElement("material");
                        polynode.AppendChild(matnode);
                        // attributes
                        { XmlAttribute flags = doc.CreateAttribute("flags"); flags.Value = mat.flags.ToString("x"); matnode.Attributes.Append(flags); }
                        { XmlAttribute a = doc.CreateAttribute("srcFactor"); a.Value = mat.srcFactor.ToString(); matnode.Attributes.Append(a); }
                        { XmlAttribute a = doc.CreateAttribute("dstFactor"); a.Value = mat.dstFactor.ToString(); matnode.Attributes.Append(a); }
                        { XmlAttribute a = doc.CreateAttribute("AlphaFunc"); a.Value = mat.AlphaFunc.ToString(); matnode.Attributes.Append(a); }
                        { XmlAttribute a = doc.CreateAttribute("AlphaTest"); a.Value = mat.AlphaTest.ToString(); matnode.Attributes.Append(a); }
                        { XmlAttribute a = doc.CreateAttribute("RefAlpha"); a.Value = mat.RefAlpha.ToString(); matnode.Attributes.Append(a); }
                        { XmlAttribute a = doc.CreateAttribute("cullmode"); a.Value = mat.cullMode.ToString("x"); matnode.Attributes.Append(a); }
                        { XmlAttribute a = doc.CreateAttribute("zbuffoff"); a.Value = mat.zBufferOffset.ToString(); matnode.Attributes.Append(a); }

                        // textures
                        foreach (NUD.Mat_Texture tex in mat.textures)
                        {
                            XmlNode texnode = doc.CreateElement("texture");
                            { XmlAttribute a = doc.CreateAttribute("hash"); a.Value = tex.hash.ToString("x"); texnode.Attributes.Append(a); }
                            { XmlAttribute a = doc.CreateAttribute("wrapmodeS"); a.Value = tex.WrapMode1.ToString("x"); texnode.Attributes.Append(a); }
                            { XmlAttribute a = doc.CreateAttribute("wrapmodeT"); a.Value = tex.WrapMode2.ToString("x"); texnode.Attributes.Append(a); }
                            { XmlAttribute a = doc.CreateAttribute("minfilter"); a.Value = tex.minFilter.ToString("x"); texnode.Attributes.Append(a); }
                            { XmlAttribute a = doc.CreateAttribute("magfilter"); a.Value = tex.magFilter.ToString("x"); texnode.Attributes.Append(a); }
                            { XmlAttribute a = doc.CreateAttribute("mipdetail"); a.Value = tex.mipDetail.ToString("x"); texnode.Attributes.Append(a); }
                            matnode.AppendChild(texnode);
                        }

                        // params
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

                    polyCount++;
                }
            }
            polycount.Value = polyCount.ToString();

            doc.Save(filename);
        }

        public static void importMaterialAsXML(NUD n, string filename)
        {
            int polyCount = 0;
            foreach (NUD.Mesh m in n.mesh)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    polyCount++;
                }
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            XmlNode main = doc.ChildNodes[0];
            Debug.WriteLine(main.Attributes.GetNamedItem("polycount").Value);


            List<NUD.Material> materials = new List<NUD.Material>();
            List<int> ids = new List<int>();

            // validate at every step
            foreach (XmlNode meshnode in main.ChildNodes)
            {
                if (meshnode.Name.Equals("mesh"))
                {
                    foreach (XmlNode polynode in meshnode.ChildNodes)
                    {
                        if (polynode.Name.Equals("polygon"))
                        {
                            ids.Add(polynode.ChildNodes.Count);

                            if (ids.Count > polyCount)
                            {
                                int countDif = ids.Count - polyCount;
                                MessageBox.Show(String.Format("Expected {0} polygons but found {1} in the XML file. The last {2} polygon(s) will be ignored.", 
                                    polyCount, ids.Count, countDif));
                            }

                            foreach (XmlNode matnode in polynode.ChildNodes)
                            {
                                if (matnode.Name.Equals("material"))
                                {
                                    // being creating the material
                                    NUD.Material mat = new NUD.Material();
                                    materials.Add(mat);
                                    foreach (XmlAttribute a in matnode.Attributes)
                                    {
                                        switch (a.Name)
                                        {
                                            case "flags": uint f = 0; if (uint.TryParse(a.Value, NumberStyles.HexNumber, null, out f)) { mat.flags = f; }; break;
                                            case "srcFactor": int.TryParse(a.Value, out mat.srcFactor); break;
                                            case "dstFactor": int.TryParse(a.Value, out mat.dstFactor); break;
                                            case "AlphaFunc": int.TryParse(a.Value, out mat.AlphaFunc); break;
                                            case "AlphaTest": int.TryParse(a.Value, out mat.AlphaTest); break;
                                            case "RefAlpha": int.TryParse(a.Value, out mat.RefAlpha); break;
                                            case "cullmode": int cm = 0; if (int.TryParse(a.Value, NumberStyles.HexNumber, null, out cm)) { mat.cullMode = cm; }; break;
                                            case "zbuffoff": int.TryParse(a.Value, out mat.zBufferOffset); break;
                                        }
                                    }

                                    foreach (XmlNode mnode in matnode.ChildNodes)
                                    {
                                        //textures
                                        if (mnode.Name.Equals("texture"))
                                        {
                                            NUD.Mat_Texture tex = new NUD.Mat_Texture();
                                            mat.textures.Add(tex);
                                            foreach (XmlAttribute a in mnode.Attributes)
                                            {
                                                switch (a.Name)
                                                {
                                                    case "hash": int f = 0; if (int.TryParse(a.Value, NumberStyles.HexNumber, null, out f)) { tex.hash = f; }; break;
                                                    case "wrapmodeS": int.TryParse(a.Value, out tex.WrapMode1); break;
                                                    case "wrapmodeT": int.TryParse(a.Value, out tex.WrapMode2); break;
                                                    case "minfilter": int.TryParse(a.Value, out tex.minFilter); break;
                                                    case "magfilter": int.TryParse(a.Value, out tex.magFilter); break;
                                                    case "mipdetail": int.TryParse(a.Value, out tex.mipDetail); break;
                                                }
                                            }
                                        }
                                        // parameters
                                        if (mnode.Name.Equals("param"))
                                        {
                                            string name = "";
                                            foreach (XmlAttribute a in mnode.Attributes)
                                            {
                                                switch (a.Name)
                                                {
                                                    case "name": name = a.Value; break;
                                                }
                                            }
                                            string[] values = mnode.InnerText.Split(' ');
                                            List<float> v = new List<float>();
                                            float f = 0;
                                            foreach (string s in values)
                                            {
                                                if (name == "NU_materialHash")
                                                {
                                                    int hash;
                                                    if (int.TryParse(s, NumberStyles.HexNumber, null, out hash))
                                                    {
                                                        f = BitConverter.ToSingle(BitConverter.GetBytes(hash), 0);
                                                        v.Add(f);
                                                    }
                                                }
                                                else
                                                    if (float.TryParse(s, out f)) v.Add(f);
                                                else
                                                    v.Add(0.0f);
                                            }

                                            // array should always have 4 values                                           
                                            // should do something more meaningful with this
                                            if (v.Count != 4)
                                                throw new MaterialXMLParamArrayLengthException();

                                            try
                                            {
                                                mat.entries.Add(name, v.ToArray());
                                            }
                                            catch (System.ArgumentException)
                                            {
                                                MessageBox.Show(String.Format("Polygon{0} contains more than 1 instance of {1}. \n"
                                                    + "Only the first instance of {1} will be added.", polynode.ChildNodes.Count.ToString(), name));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            int pid = 0;
            int mid = 0;
            foreach (NUD.Mesh m in n.mesh)
            {
                foreach (NUD.Polygon p in m.Nodes)
                {
                    p.materials.Clear();
                    for (int i = 0; i < ids[pid]; i++)
                    {
                        p.materials.Add(materials[mid++]);
                    }
                    pid++;
                }
            }
        }
    }
}
